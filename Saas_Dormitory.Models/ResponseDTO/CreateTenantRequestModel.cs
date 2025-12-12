using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saas_Dormitory.Models.ResponseDTO
{
    public class CreateTenantRequestModel
    {
        [Required]
        public string? TenantName { get; set; }
        [Required]
        public string? FullName { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
        [Required]
        public string? Phone { get; set; }
    }

    public class UpdateTenantRequestModel
    {
        public string UserId { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }        
        public string FullName { get; set; }
        public string Phone { get; set; }
    }


    public class TenantStatusRequestModel
    {
        public int TenantId { get; set; }
        public bool IsActive { get; set; }
    }
    public class TenantDetailsModel
    {
        public string UserId { get; set; }
        public string? TenantName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Phone { get; set; }
        public int TenantId { get; set; }
        public bool IsActive { get; set; } = false;

    }
}
