using System;
using System.Collections.Generic;

namespace Saas_Dormitory.DAL.Data;

public partial class Bed
{
    public int BedId { get; set; }

    public int TenantId { get; set; }

    public int RoomId { get; set; }

    public string BedNumber { get; set; } = null!;

    public bool? IsOccupied { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public virtual Room Room { get; set; } = null!;

    public virtual ICollection<Stay> Stays { get; set; } = new List<Stay>();

    public virtual Tenant Tenant { get; set; } = null!;
}
