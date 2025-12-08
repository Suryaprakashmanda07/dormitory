using System;
using System.Collections.Generic;

namespace Saas_Dormitory.DAL.Data;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int TenantId { get; set; }

    public int InvoiceId { get; set; }

    public decimal Amount { get; set; }

    public string? PaymentMethod { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? ReferenceNo { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;

    public virtual Tenant Tenant { get; set; } = null!;
}
