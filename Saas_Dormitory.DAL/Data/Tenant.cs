using System;
using System.Collections.Generic;

namespace Saas_Dormitory.DAL.Data;

public partial class Tenant
{
    public int TenantId { get; set; }

    public string TenantName { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Auditlog> Auditlogs { get; set; } = new List<Auditlog>();

    public virtual ICollection<Bed> Beds { get; set; } = new List<Bed>();

    public virtual ICollection<Guest> Guests { get; set; } = new List<Guest>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

    public virtual ICollection<Stay> Stays { get; set; } = new List<Stay>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public virtual ICollection<Userprofile> Userprofiles { get; set; } = new List<Userprofile>();
}
