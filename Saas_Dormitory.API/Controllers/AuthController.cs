using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Saas_Dormitory.DAL;
using Saas_Dormitory.Models;   
using Saas_Dormitory.Models.ResponseDTO;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Saas_Dormitory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }

        // 🔐 REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 🆕 Create ApplicationUser (not IdentityUser)
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // 🔑 Assign Role (optional)
            if (!string.IsNullOrEmpty(model.Role))
            {
                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(model.Role));
                }

                await _userManager.AddToRoleAsync(user, model.Role);
            }

            return Ok(new { message = "User registered successfully" });
        }
        [AllowAnonymous]

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var response = new SingleItemResponseDTO<LoginResponse>();

            try
            {
                // var user = await _userManager.FindByNameAsync(model.Username);

                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == model.email.ToLower());

                if (user is null)
                {
                    response.Msg = "User not exists";
                    response.Valid = false;
                    return Ok(response);
                }


                //if (!user..IsActive)
                //{
                //    response.Message = "Account is deactivated , contact admin";
                //    return Ok(response);
                //}

                bool isValidCredential = await _userManager.CheckPasswordAsync(user, model.Password);
                if (isValidCredential)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);

                    var claims = new List<Claim>
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Name, user.UserName ?? user.Id.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.Id)
                    };

                    // Add all roles as separate claims
                    claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));


                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var expireMinutes = Convert.ToInt32(_config["Jwt:ExpireMinutes"]);
                    var token = new JwtSecurityToken(
                        issuer: _config["Jwt:Issuer"],
                        audience: _config["Jwt:Audience"],
                        claims: claims,
                        expires: DateTime.UtcNow.AddYears(expireMinutes),
                        signingCredentials: creds
                    );
                    response.Item = new LoginResponse
                    {
                        Token = new JwtSecurityTokenHandler().WriteToken(token),
                        Expiration = token.ValidTo,
                        Roles = userRoles.ToList(),
                        RoleName = userRoles.FirstOrDefault(),
                        UserId = user.Id,
                        Email = user.Email,
                    };

                    response.Valid = true;
                    response.Msg = "Login successful";


                }
                else
                {
                    response.Valid = false;
                    response.Msg = "Invalid Login credentials";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception in Login: {Message}", ex.Message);
                response.Valid = false;
                response.Msg = "controller: AuthController, method: Login, error: " + ex.Message;
            }

            return Ok(response);
        }

        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
                return BadRequest(new { message = "User not found" });

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Password changed successfully" });
        }
        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return Ok(new { message = "If user exists, reset link sent." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // TODO: send by email OR return for testing
            return Ok(new
            {
                message = "Reset token generated",
                resetToken = token,
                email = user.Email
            });
        }
        [AllowAnonymous]
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return BadRequest(new { message = "Invalid request" });

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Password reset successful" });
        }
        [AllowAnonymous]
        [HttpPost("AdminLogin")]
        public async Task<IActionResult> AdminLogin([FromBody] AdminLoginModel model)
        {
            var response = new SingleItemResponseDTO<LoginResponse>();

            try
            {
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                if (user == null)
                {
                    response.Msg = "Invalid email or password";
                    response.Valid = false;
                    return Ok(response);
                }

                var isValid = await _userManager.CheckPasswordAsync(user, model.Password);
                if (!isValid)
                {
                    response.Msg = "Invalid email or password";
                    response.Valid = false;
                    return Ok(response);
                }

                return Ok(await GenerateJwtToken(user, "Admin"));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Admin Login Error");
                response.Valid = false;
                response.Msg = ex.Message;
                return Ok(response);
            }
        }


        [AllowAnonymous]
        [HttpPost("tenant-login")]
        public async Task<IActionResult> TenantLogin([FromBody] TenantLoginModel model)
        {
            var response = new SingleItemResponseDTO<LoginResponse>();

            try
            {
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == model.Mobile);

                if (user == null)
                {
                    response.Msg = "Mobile number not registered";
                    response.Valid = false;
                    return Ok(response);
                }

                // ✅ Validate OTP (Replace with DB or service)
                //if (!ValidateOtp(model.Mobile, model.Otp))
                //{
                //    response.Msg = "Invalid OTP";
                //    response.Valid = false;
                //    return Ok(response);
                //}

                return Ok(await GenerateJwtToken(user, "Tenant"));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Tenant Login Error");
                response.Valid = false;
                response.Msg = ex.Message;
                return Ok(response);
            }
        }
        private async Task<SingleItemResponseDTO<LoginResponse>> GenerateJwtToken(ApplicationUser user, string loginType)
        {
            var response = new SingleItemResponseDTO<LoginResponse>();

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim("loginType", loginType),
        new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? user.PhoneNumber)
    };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            int expireMinutes = Convert.ToInt32(_config["Jwt:ExpireMinutes"]);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds
            );

            response.Item = new LoginResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
                UserId = user.Id,
                Email = user.Email,
                RoleName = roles.FirstOrDefault(),
                Roles = roles.ToList()
            };

            response.Valid = true;
            response.Msg = "Login successful";

            return response;
        }

    }
}
