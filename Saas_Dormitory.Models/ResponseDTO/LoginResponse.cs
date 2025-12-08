using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saas_Dormitory.Models.ResponseDTO
{
    public class LoginResponse
    {
        public string? Token { get; set; }
        public DateTime Expiration { get; set; }
        public List<string>? Roles { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? RoleName { get; set; }
        public string? Username { get; set; }
    }
}
