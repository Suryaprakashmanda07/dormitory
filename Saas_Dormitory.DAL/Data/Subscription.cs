using System;
using System.Collections.Generic;

namespace Saas_Dormitory.DAL.Data;

public partial class Subscription
{
    public int SubscriptionId { get; set; }

    public int TenantId { get; set; }

    public int? PlanId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public virtual Subscriptionplan? Plan { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
