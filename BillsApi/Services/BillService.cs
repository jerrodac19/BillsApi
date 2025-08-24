namespace BillsApi.Services
{
    using BillsApi.Dtos;
    using BillsApi.Models;
    using BillsApi.Repositories;
    using BillsApi.Repositories.UnitOfWork;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class BillService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly GoogleTaskService _googleTaskService;

        public BillService(IUnitOfWork unitOfWork, GoogleTaskService googleTaskService)
        {
            _unitOfWork = unitOfWork;
            _googleTaskService = googleTaskService;
        }

        public async Task<Bill> CreateBillAsync(CreateBillDto createBillDto)
        {
            // Retrieve the BillConfiguration using the repository from the Unit of Work
            var billConfiguration = await _unitOfWork.BillConfigurations.GetByIdAsync(createBillDto.ConfigurationId);
            if (billConfiguration == null)
            {
                throw new ArgumentException("Invalid BillConfigurationId provided.");
            }

            var currentTime = DateTime.UtcNow;

            // Map the DTO to a new Bill entity
            var bill = new Bill
            {
                Amount = (createBillDto.Amount > 0) ? createBillDto.Amount : billConfiguration.DefaultAmount,
                DueDate = createBillDto.DueDate ?? DateOnly.Parse($"{currentTime.Year}-{currentTime.Month}-{billConfiguration.MonthlyDueDate}"),
                Payed = createBillDto.Payed,
                PayEarly = createBillDto.PayEarly > 0 ? createBillDto.PayEarly : billConfiguration.DefaultPayEarly,
                Active = createBillDto.Active,
                ConfigurationId = createBillDto.ConfigurationId,
                Valid = createBillDto.Valid,
                CalendarEventId = createBillDto.CalendarEventId,
                ReminderId = createBillDto.ReminderId,
                TaskId = createBillDto.TaskId,
                Title = !string.IsNullOrEmpty(createBillDto.Title) ? createBillDto.Title : billConfiguration.DefaultTitle,
                Updated = currentTime
            };

            // If DueDate is in the past, move it to the next month
            if (((DateOnly)bill.DueDate).ToDateTime(new TimeOnly(0, 0, 0)) < currentTime)
            {
                bill.DueDate = ((DateOnly)bill.DueDate).AddMonths(1);
            }

            // Create a Google Task for the new bill if a TaskId doesn't exist
            if (string.IsNullOrEmpty(bill.TaskId))
            {
                var createdGoogleTask = await _googleTaskService.CreateTaskAsync(
                    newTitle: GetFormattedTaskTitle(bill),
                    dueDate: ((DateOnly)bill.DueDate).ToDateTime(new TimeOnly(0, 0, 0))
                );
                if (createdGoogleTask != null)
                {
                    bill.TaskId = createdGoogleTask.Id;
                }
            }

            // Add the new bill to the repository
            await _unitOfWork.Bills.AddAsync(bill);

            return bill;
        }

        public async Task UpdateBillAsync(UpdateBillDto updateBillDto)
        {
            // Retrieve the bill using the repository from the Unit of Work
            var bill = await _unitOfWork.Bills.GetByIdAsync(updateBillDto.Id);
            if (bill == null)
            {
                throw new KeyNotFoundException("Bill not found.");
            }

            // Update the bill properties from the DTO
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
            bill.Updated = DateTime.UtcNow;

            // Update the Google Task if any relevant bill properties changed
            if (updateGoogleTask)
            {
                var billStatus = (bill.Active == false || bill.Payed == true) ? "completed" : "needsAction";
                if (!string.IsNullOrEmpty(bill.TaskId) && bill.DueDate != null)
                {
                    try
                    {
                        await _googleTaskService.UpdateTaskAsync(bill.TaskId, new GoogleTaskUpdateDto { Title = GetFormattedTaskTitle(bill), Due = ((DateOnly)bill.DueDate).ToDateTime(new TimeOnly(0, 0, 0)), Status = billStatus });
                    }
                    catch { }
                }
            }

            // Update the bill in the repository
            await _unitOfWork.Bills.UpdateAsync(bill);
        }

        private string GetFormattedTaskTitle(Bill bill)
        {
            return $"Pay {bill.Title} Bill - ${bill.Amount:F2} Due";
        }
    }
}
