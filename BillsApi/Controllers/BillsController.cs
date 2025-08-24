using BillsApi.Dtos;
using BillsApi.Models;
using BillsApi.Repositories.UnitOfWork;
using BillsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BillsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly BillService _billService;

        public BillsController(IUnitOfWork unitOfWork, BillService billService)
        {
            _unitOfWork = unitOfWork;
            _billService = billService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bill>>> GetBills([FromQuery] string? filter)
        {
            var bills = await _unitOfWork.Bills.GetBillsWithConfigurationAsync(filter);
            return Ok(bills);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Bill>> GetBill(int id)
        {
            var bill = await _unitOfWork.Bills.GetBillWithConfigurationByIdAsync(id);

            if (bill == null)
            {
                return NotFound();
            }

            return Ok(bill);
        }

        [HttpPost]
        public async Task<ActionResult<Bill>> PostBill([FromBody] CreateBillDto createBillDto)
        {
            try
            {
                var newBill = await _billService.CreateBillAsync(createBillDto);
                await _unitOfWork.SaveAsync();
                return CreatedAtAction(nameof(GetBill), new { id = newBill.Id }, newBill);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutBill(int id, [FromBody] UpdateBillDto updateBillDto)
        {
            if (id != updateBillDto.Id)
            {
                return BadRequest("ID mismatch.");
            }

            try
            {
                var billExists = await _unitOfWork.Bills.BillExistsAsync(id);
                if (!billExists)
                {
                    return NotFound();
                }

                await _billService.UpdateBillAsync(updateBillDto);
                await _unitOfWork.SaveAsync();
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}