using System;
using System.ComponentModel.DataAnnotations;

namespace Saas_Dormitory.Models.ResponseDTO
{
    public class RoomCreateDto
    {
        [Required]
        public int TenantId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [Required]
        public string RoomNumber { get; set; } = string.Empty;

        public string? RoomType { get; set; }

        public int? Capacity { get; set; }

        public decimal? RentAmount { get; set; }
    }

    public class RoomUpdateDto
    {
        [Required]
        public int RoomId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [Required]
        public string RoomNumber { get; set; } = string.Empty;

        public string? RoomType { get; set; }

        public int? Capacity { get; set; }

        public decimal? RentAmount { get; set; }
    }

    public class RoomDetailsDto
    {
        public int RoomId { get; set; }
        public int TenantId { get; set; }
        public int PropertyId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string? RoomType { get; set; }
        public int? Capacity { get; set; }
        public decimal? RentAmount { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? PropertyName { get; set; }
    }
}

