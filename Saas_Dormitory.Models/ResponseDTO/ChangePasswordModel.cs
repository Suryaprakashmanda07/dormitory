using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saas_Dormitory.Models.ResponseDTO
{
    public class ChangePasswordModel
    {
        [Required]
        public string? UserId { get; set; }
        [Required]
        public string? OldPassword { get; set; }
        [Required]
        public string? NewPassword { get; set; }
    }

}
