using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Saas_Dormitory.DAL.Helpers
{
    public class CurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserId()
        {
            return _httpContextAccessor.HttpContext?.User?
                   .FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public string GetUserRole()
        {
            return _httpContextAccessor.HttpContext?.User?
                   .FindFirst(ClaimTypes.Role)?.Value;
        }
    }

}
