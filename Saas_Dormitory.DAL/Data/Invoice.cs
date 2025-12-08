using System;
using System.Collections.Generic;

namespace Saas_Dormitory.DAL.Data;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public int TenantId { get; set; }

    public int StayId { get; set; }

    public DateOnly InvoiceMonth { get; set; }

    public decimal RentAmount { get; set; }

    public DateOnly DueDate { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Stay Stay { get; set; } = null!;

    public virtual Tenant Tenant { get; set; } = null!;
}
