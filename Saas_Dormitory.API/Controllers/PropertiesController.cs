using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saas_Dormitory.DAL.Interface;
using Saas_Dormitory.Models.ResponseDTO;
using System.Threading.Tasks;

namespace Saas_Dormitory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   // [Authorize] // Optional
    public class PropertiesController : ControllerBase
    {
        private readonly IPropertiesRepository _propertiesRepo;

        public PropertiesController(IPropertiesRepository propertiesRepo)
        {
            _propertiesRepo = propertiesRepo;
        }

        // ✅ CREATE
        [HttpPost("CreateProperty")]
        public async Task<IActionResult> CreateProperty([FromBody] PropertyCreateDto model)
        {
            var result = await _propertiesRepo.CreatePropertyAsync(model);
            return Ok(result);
        }

        // ✅ UPDATE
        [HttpPost("UpdateProperty")]
        public async Task<IActionResult> UpdateProperty([FromBody] PropertyUpdateDto model)
        {
            var result = await _propertiesRepo.UpdatePropertyAsync(model);
            return Ok(result);
        }

        //// ✅ DELETE
        [HttpDelete("UpdatePropertyStatus")]
        public async Task<IActionResult> UpdatePropertyStatus(int propertyId)
        {
            var result = await _propertiesRepo.UpdatePropertyStatusAsync(propertyId);
            return Ok(result);
        }

        // ✅ GET BY ID
        [HttpGet("GetPropertyById")]
        public async Task<IActionResult> GetPropertyById(int propertyId)
        {
            var result = await _propertiesRepo.GetPropertyByIdAsync(propertyId);
            return Ok(result);
        }

        // ✅ GET ALL (SEARCH, SORT, PAGINATION)
        [HttpPost("GetAllProperties")]
        public async Task<IActionResult> GetAllProperties([FromBody] PaginationRequestModel model)
        {
            var result = await _propertiesRepo.GetAllPropertiesAsync(model);
            return Ok(result);
        }
    }
}
