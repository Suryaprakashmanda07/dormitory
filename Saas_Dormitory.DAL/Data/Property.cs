using System;
using System.Collections.Generic;

namespace Saas_Dormitory.DAL.Data;

public partial class Property
{
    public int PropertyId { get; set; }

    public int TenantId { get; set; }

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public bool? Isactive { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

    public virtual Tenant Tenant { get; set; } = null!;
}
