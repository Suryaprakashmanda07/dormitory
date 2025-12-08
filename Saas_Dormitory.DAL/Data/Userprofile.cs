using System;
using System.Collections.Generic;

namespace Saas_Dormitory.DAL.Data;

public partial class Userprofile
{
    public int ProfileId { get; set; }

    public string UserId { get; set; } = null!;

    public int TenantId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Role { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
