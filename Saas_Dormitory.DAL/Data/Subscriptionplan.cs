using System;
using System.Collections.Generic;

namespace Saas_Dormitory.DAL.Data;

public partial class Subscriptionplan
{
    public int PlanId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public int? MaxProperties { get; set; }

    public int? MaxUsers { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public int? Period { get; set; }

    public bool? Isactive { get; set; }

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
