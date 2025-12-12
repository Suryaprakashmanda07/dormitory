using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Saas_Dormitory.API.Helpers;
using Saas_Dormitory.DAL;
using Saas_Dormitory.Models;   
using Saas_Dormitory.Models.ResponseDTO;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
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
        private readonly IWebHostEnvironment _env;
        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
            _env = env;
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
                        //Username = userProfile?.FullName

                    };
                }
                else
                {
                    response.Valid = false;
                    response.Msg = "Invalid Login credentials";
                }
            }
            catch (Exception ex)
            {
               // Log.Error(ex, "Exception in Login: {Message}", ex.Message);
                response.Valid = false;
                response.Msg = "controller: AuthController, method: Login, error: " + ex.Message;
            }

            return Ok(response);
        }

       // [Authorize]
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
        [HttpPost("ChangePasswordByAdmin")]
        public async Task<IActionResult> ChangeAdminPassword(ResetPasswordAdminModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
                return BadRequest(new { message = "User not found" });

            // ✅ Allow ONLY Admin
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin)
                return BadRequest(new { message = "Only Admin password can be changed" });

            // ✅ Generate reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // ✅ Reset password
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Password updated successfully" });
        }

        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return Ok(new { message = "If user exists, reset link sent." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);

            // Create Reset Link
            string baseUrl = _config["AppSettings:FrontendBaseUrl"];

            string resetLink = $"{baseUrl}/reset-password?email={user.Email}&token={encodedToken}";

            // Load template
            string templatePath = Path.Combine(_env.WebRootPath, "EmailTemplates", "ResetPasswordTemplate.html");
            string template = LoadEmailTemplate(templatePath);

            // Replace placeholders
            string emailBody = template
                .Replace("{{UserName}}", user.UserName)
                .Replace("{{ResetLink}}", resetLink);

            // Send email
            SendEmail _SendEmail = new SendEmail();
            await _SendEmail.SendHtmlEmail(user.Email, "Password Reset", emailBody);

            return Ok(new { message = "Reset link sent to email." });
        }
        private string LoadEmailTemplate(string filePath)
        {
            return System.IO.File.ReadAllText(filePath);
        }


        private MailMessage CreateMailMessage(string from, string to, string subject, string body)
        {
            var message = new MailMessage(from, to);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true; // IMPORTANT
            return message;
        }
        [AllowAnonymous]
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return Ok(new {vaild=false, message = "Invalid request" });

            // Token must be decoded (important)
            string decodedToken = Uri.UnescapeDataString(model.Token);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (!result.Succeeded)
                return Ok(new
                {
                    vaild = false,
                    message = "Password reset failed",
                    errors = result.Errors
                });

            return Ok(new { vaild = true, message = "Password reset successful" });
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
               // Log.Error(ex, "Admin Login Error");
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
                //Log.Error(ex, "Tenant Login Error");
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
