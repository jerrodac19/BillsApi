using BillsApi.Models;
using BillsApi.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace BillsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncomeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public IncomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Income>>> GetIncome([FromQuery] int groupId = 1)
        {
            var incomes = await _unitOfWork.Incomes.GetIncomesByGroupIdAsync(groupId);

            if (incomes == null || !incomes.Any())
            {
                return NotFound();
            }

            return Ok(incomes);
        }
    }
}