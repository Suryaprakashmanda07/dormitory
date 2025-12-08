using System;
using System.Collections.Generic;

namespace Saas_Dormitory.DAL.Data;

public partial class Stay
{
    public int StayId { get; set; }

    public int TenantId { get; set; }

    public int GuestId { get; set; }

    public int? BedId { get; set; }

    public DateTime CheckIn { get; set; }

    public DateTime? CheckOut { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public virtual Bed? Bed { get; set; }

    public virtual Guest Guest { get; set; } = null!;

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual Tenant Tenant { get; set; } = null!;
}
