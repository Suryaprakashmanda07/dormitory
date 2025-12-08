using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saas_Dormitory.Models.ResponseDTO
{
    public class CreateUserprofileModel
    {
        [Required]
        public string UserId { get; set; } = null!;
        [Required]
        public int TenantId { get; set; }
        [Required]
        public string FullName { get; set; } = null!;
        [Required]
        public string? Phone { get; set; }

    }
}
