using System;
using System.Collections.Generic;

namespace Saas_Dormitory.DAL.Data;

public partial class Room
{
    public int RoomId { get; set; }

    public int TenantId { get; set; }

    public int PropertyId { get; set; }

    public string RoomNumber { get; set; } = null!;

    public string? RoomType { get; set; }

    public int? Capacity { get; set; }

    public decimal? RentAmount { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Bed> Beds { get; set; } = new List<Bed>();

    public virtual Property Property { get; set; } = null!;

    public virtual Tenant Tenant { get; set; } = null!;
}
