using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saas_Dormitory.DAL
{
    public class ApplicationUser : IdentityUser
    {
        public Guid TenantId { get; set; }
    }
}
