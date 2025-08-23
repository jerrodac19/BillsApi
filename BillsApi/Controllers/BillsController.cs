using BillsApi.Dtos;
using BillsApi.Models;
using BillsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;

namespace BillsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillsController : ControllerBase
    {
        private readonly BillsApiContext _context;
        private readonly GoogleTaskService _googleTaskService;

        public BillsController(BillsApiContext context, GoogleTaskService googleTaskService)
        {
            _context = context;
            _googleTaskService = googleTaskService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bill>>> GetBills([FromQuery] string? filter)
        {
            var query = _context.Bills.Include(b => b.Configuration).AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(b => b.Title != null && b.Title.Contains(filter) && b.Active == true);
            }
            else
            {
                query = query.Where(b => b.Active == true);
            }

            return await query.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Bill>> GetBill(int id)
        {
            var bill = await _context.Bills.Include(b => b.Configuration).FirstOrDefaultAsync(b => b.Id == id);

            if (bill == null)
            {
                return NotFound();
            }

            return Ok(bill);
        }

        [HttpPost]
        public async Task<ActionResult<Bill>> PostBill([FromBody] CreateBillDto createBillDto)
        {
            // Make sure the incoming BillConfigurationId exists
            var billConfiguration = await _context.BillConfigurations.FindAsync(createBillDto.ConfigurationId);

            if (billConfiguration == null)
            {
                return BadRequest("Invalid BillConfigurationId provided.");
            }

            var currentTime = DateTime.UtcNow;

            // Map the DTO to a new Bill entity
            var bill = new Bill
            {
                Amount = (createBillDto.Amount > 0) ? createBillDto.Amount : billConfiguration.DefaultAmount,
                DueDate = createBillDto.DueDate != null ? createBillDto.DueDate : DateOnly.Parse($"{currentTime.Year}-{currentTime.Month}-{billConfiguration.MonthlyDueDate}"),
                Payed = createBillDto.Payed,
                PayEarly = createBillDto.PayEarly != 0 ? createBillDto.PayEarly : billConfiguration.DefaultPayEarly,
                Active = createBillDto.Active,
                ConfigurationId = createBillDto.ConfigurationId,
                Valid = createBillDto.Valid,
                CalendarEventId = createBillDto.CalendarEventId,
                ReminderId = createBillDto.ReminderId,
                TaskId = createBillDto.TaskId,
                Title = !string.IsNullOrEmpty(createBillDto.Title) ? createBillDto.Title : billConfiguration.DefaultTitle,
                Updated = currentTime // Set the updated date on the server
            };

            if (((DateOnly)bill.DueDate).ToDateTime(new TimeOnly(0, 0, 0)) < currentTime)
            {
                bill.DueDate = ((DateOnly)bill.DueDate).AddMonths(1);
            }

            if (string.IsNullOrEmpty(bill.TaskId))
            {
                var createdGoogleTask = await _googleTaskService.CreateTaskAsync(
                    newTitle: GetFormattedTaskTitle(bill),
                    //notes: "This task was created automatically by the Bills API.",
                    dueDate: ((DateOnly)bill.DueDate).ToDateTime(new TimeOnly(0,0,0))
                );
                if (createdGoogleTask != null)
                {
                    bill.TaskId = createdGoogleTask.Id;
                }
            }

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            // Return a 201 Created status code with the newly created bill
            return CreatedAtAction("GetBills", new { id = bill.Id }, bill);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutBill(int id, UpdateBillDto updateBillDto)
        {
            // A standard check to ensure the ID in the URL matches the ID in the body
            if (id != updateBillDto.Id)
            {
                return BadRequest();
            }

            var bill = await _context.Bills.FindAsync(id);
            if (bill == null)
            {
                return NotFound();
            }

            // Map the properties from the DTO to the entity
            bool updateGoogleTask = false;
            if (updateBillDto.Amount > 0)
            {
                bill.Amount = updateBillDto.Amount;
                updateGoogleTask = true;
            }
            if (updateBillDto.DueDate != null)
            {
                bill.DueDate = updateBillDto.DueDate;
                updateGoogleTask = true;
            }
            if (updateBillDto.Payed != null)
            {
                updateGoogleTask = true;
                bill.Payed = updateBillDto.Payed;
            }
            if (updateBillDto.PayEarly > 0)
            {
                bill.PayEarly = updateBillDto.PayEarly;
            }
            if (updateBillDto.Active != null)
            {
                updateGoogleTask = true;
                bill.Active = updateBillDto.Active;
            }
            if (updateBillDto.ConfigurationId != 0)
            {
                bill.ConfigurationId = updateBillDto.ConfigurationId;
            }
            if (updateBillDto.Valid != null)
            {
                bill.Valid = (bool)updateBillDto.Valid;
            }
            if (!string.IsNullOrEmpty(updateBillDto.CalendarEventId))
            {
                bill.CalendarEventId = updateBillDto.CalendarEventId;
            }
            if (!string.IsNullOrEmpty(updateBillDto.ReminderId))
            {
                bill.ReminderId = updateBillDto.ReminderId;
            }
            if (!string.IsNullOrEmpty(updateBillDto.TaskId))
            {
                bill.TaskId = updateBillDto.TaskId;
            }
            if (!string.IsNullOrEmpty(updateBillDto.Title))
            {
                bill.Title = updateBillDto.Title;
                updateGoogleTask = true;
            }
            bill.Updated = DateTime.UtcNow; // Set the updated date on the server

            if (updateGoogleTask)
            {
                var billStatus = (bill.Active == false || bill.Payed == true) ? "completed" : "needsAction";
                if (!string.IsNullOrEmpty(bill.TaskId) && bill.DueDate != null)
                {
                    try
                    {
                        await _googleTaskService.UpdateTaskAsync(bill.TaskId, new GoogleTaskUpdateDto { Title = GetFormattedTaskTitle(bill), Due = ((DateOnly)bill.DueDate).ToDateTime(new TimeOnly(0, 0, 0)), Status = billStatus });
                    }
                    catch{}
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await BillExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // A successful update typically returns a 204 No Content status
            return NoContent();
        }

        // Helper method to check if a bill exists
        private async Task<bool> BillExists(int id)
        {
            return await _context.Bills.AnyAsync(e => e.Id == id);
        }

        private string GetFormattedTaskTitle(Bill bill)
        {
            return $"Pay {bill.Title} Bill - ${bill.Amount:F2} Due";
        }
    }
}
