using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Saas_Dormitory.DAL;
using Saas_Dormitory.DAL.Interface;
using Saas_Dormitory.Models;
using Saas_Dormitory.Models.ResponseDTO;

namespace Saas_Dormitory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   // [Authorize]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantRepository _tenantRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TenantsController(
            ITenantRepository tenantRepo,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _tenantRepo = tenantRepo;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("TenantRegister")]
        public async Task<ActionResult<ResponseDTO>> Register(CreateTenantRequestModel request)
        {
            ResponseDTO resp = new ResponseDTO();

            try
            {
                // ✅ Check duplicate email
                var emailExists = await _userManager.FindByEmailAsync(request.Email);
                if (emailExists != null)
                {
                    resp.Valid = false;
                    resp.Msg = "Email already exists";
                    return Ok(resp);
                }

                // ✅ Check duplicate phone
                bool phoneExists = _userManager.Users.Any(u => u.PhoneNumber == request.Phone);
                if (phoneExists)
                {
                    resp.Valid = false;
                    resp.Msg = "Phone number already exists";
                    return Ok(resp);
                }

                // ✅ Create user
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    PhoneNumber = request.Phone,
                    EmailConfirmed = true
                };
               // user.TenantId=user.Id;
                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    resp.Valid = false;
                    resp.Msg = result.Errors.FirstOrDefault()?.Description;
                    return Ok(resp);
                }

                // ✅ Ensure Admin role exists
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // ✅ Assign role
                await _userManager.AddToRoleAsync(user, "Admin");

                var tenant = await _tenantRepo.CreateTenantAsync(request, user.Id);
                if (!tenant.Valid)
                {
                    await _userManager.DeleteAsync(user); // Rollback
                    resp.Valid = false;
                    resp.Msg = tenant.Msg;
                    return Ok(resp);
                }
                var createdUser = await _userManager.FindByIdAsync(user.Id);

                createdUser.TenantId = tenant.Id;

                var result12 = await _userManager.UpdateAsync(createdUser);

                if (!result12.Succeeded)
                {
                    var errors = string.Join(", ", result12.Errors.Select(e => e.Description));
                    throw new Exception("Update failed: " + errors);
                }
                // ✅ Final success response
                resp.Valid = true;
                resp.Msg = "Account created successfully.";
                return Ok(resp);
            }
            catch (Exception ex)
            {
                resp.Valid = false;
                resp.Msg = ex.Message;
                return StatusCode(500, resp);
            }
        }
        // ✅ 1. GET TENANT BY ID
        [HttpGet("GetTenantById")]
        public async Task<IActionResult> GetTenantById(int tenantId)
        {
            var result = await _tenantRepo.GetTenantByTenantIdAsync(tenantId);
            return Ok(result);
        }

        // ✅ 2. GET ALL TENANTS (Pagination + Search)
        [HttpPost("GetAllTenants")]
        public async Task<IActionResult> GetAllTenants( PaginationRequestModel model)
        {
            var result = await _tenantRepo.GetAllTenantsAsync(model);
            return Ok(result);
        }

        // ✅ 3. UPDATE TENANT
        [HttpPost("UpdateTenant")]
        public async Task<IActionResult> UpdateTenant([FromBody] UpdateTenantRequestModel model)
        {
            var result = await _tenantRepo.UpdateTenantAsync(model);
            return Ok(result);
        }

        // ✅ 4. ACTIVATE / DEACTIVATE TENANT
        [HttpPost("ActivateDeactivateTenant")]
        public async Task<IActionResult> ActivateDeactivateTenant(int tenantId)
        {
            var result = await _tenantRepo.ActivateDeactivateTenantAsync(tenantId);
            return Ok(result);
        }
    }
}
