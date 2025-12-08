using System;
using System.Collections.Generic;

namespace Saas_Dormitory.DAL.Data;

public partial class Auditlog
{
    public int LogId { get; set; }

    public int TenantId { get; set; }

    public string? UserId { get; set; }

    public string? Action { get; set; }

    public DateTime? Timestamp { get; set; }

    public string? CreatedBy { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual AspNetUser? User { get; set; }
}
