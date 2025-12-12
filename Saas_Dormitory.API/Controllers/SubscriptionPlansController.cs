using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Saas_Dormitory.DAL.Interface;
using Saas_Dormitory.DAL.Repositories;
using Saas_Dormitory.Models.ResponseDTO;

namespace Saas_Dormitory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionPlansController : ControllerBase
    {
        private readonly ISubscriptionPlansRepository _repository;

        public SubscriptionPlansController(ISubscriptionPlansRepository repository)
        {
            _repository = repository;
        }

        // ✅ CREATE
        [HttpPost("CreateSubscriptionPlan")]
        public async Task<IActionResult> CreateSubscriptionPlan([FromBody] SubscriptionPlanCreateDto model)
        {
            var result = await _repository.CreateSubscriptionPlanAsync(model);
            return Ok(result);
        }

        // ✅ GET BY ID
        [HttpGet("GetSubscriptionPlanById")]
        public async Task<IActionResult> GetById(int planId)
        {
            var result = await _repository.GetSubscriptionPlanByIdAsync(planId);

            if (!result.Valid)
                return NotFound(result);

            return Ok(result);
        }

        // ✅ GET ALL
        [HttpPost("GetAllSubscriptionPlans")]
        public async Task<IActionResult> GetAll([FromBody] PaginationRequestModel model)
        {
            var result = await _repository.GetAllSubscriptionPlansAsync(model);
            return Ok(result);
        }

        // ✅ UPDATE
        [HttpPut("UpdateSubscriptionPlan")]
        public async Task<IActionResult> Update([FromBody] SubscriptionPlanUpdateDto model)
        {
            var result = await _repository.UpdateSubscriptionPlanAsync(model);
            return Ok(result);
        }

        // ✅ ACTIVATE / DEACTIVATE
        [HttpPatch("UpdateSubscriptionPlanStatus")]
        public async Task<IActionResult> UpdateStatus(int planId)
        {
            var result = await _repository.UpdateSubscriptionPlanStatusAsync(planId);
            return Ok(result);
        }
        [HttpGet("LookUpGetSubscriptionPeriods")]
        public async Task<IActionResult> GetSubscriptionPeriods()
        {
            try
            {
                var response = await _repository.GetSubscriptionPeriodsAsync();

                if (response.Valid)
                    return Ok(response);

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                // Optional: log exception
                // _logger.LogError(ex, "Error in GetSubscriptionPeriods");

                return StatusCode(500, new
                {
                    Valid = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

    }

}
