using BillsApi.Models;
using BillsApi.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace BillsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillConfigurationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public BillConfigurationsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BillConfiguration>>> GetBillConfigurations()
        {
            var configurations = await _unitOfWork.BillConfigurations.GetAllAsync();

            return Ok(configurations);
        }
    }
}
