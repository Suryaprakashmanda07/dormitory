using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saas_Dormitory.Models.ResponseDTO
{
    public class LoginModel
    {
        public string email { get; set; } = "";
        public string Password { get; set; } = "";
    }
    public class AdminLoginModel
    {
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
    }
    public class TenantLoginModel
    {
        [Required]
        public string? Mobile { get; set; }
        [Required]
        public string? Otp { get; set; }
    }

}
