using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saas_Dormitory.Models.ResponseDTO
{
    public class UserToken
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public bool IsRevoked { get; set; } = false;
    }
}
