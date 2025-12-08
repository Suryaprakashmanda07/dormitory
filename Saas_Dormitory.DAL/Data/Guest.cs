using System;
using System.Collections.Generic;

namespace Saas_Dormitory.DAL.Data;

public partial class Guest
{
    public int GuestId { get; set; }

    public int TenantId { get; set; }

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? IdProof { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public virtual ICollection<Stay> Stays { get; set; } = new List<Stay>();

    public virtual Tenant Tenant { get; set; } = null!;
}
