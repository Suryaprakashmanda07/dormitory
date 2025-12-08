using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Saas_Dormitory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔒 requires valid JWT
    public class ProductsController : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public IActionResult GetProducts() =>
           Ok(new[] { new { Id = 1, Name = "Laptop" } });

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult AddProduct([FromBody] object product) =>
            Ok("Product created");
    }
}
