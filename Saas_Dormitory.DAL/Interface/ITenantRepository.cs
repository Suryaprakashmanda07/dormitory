using Saas_Dormitory.Models.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saas_Dormitory.DAL.Interface
{
    public interface ITenantRepository
    {
        Task<ResponseDTO> CreateTenantAsync(CreateTenantRequestModel tenant, string UserId);

        Task<SingleItemResponseDTO<TenantDetailsModel>> GetTenantByTenantIdAsync(int tenantId);
        Task<ListResponseDTO<TenantDetailsModel>> GetAllTenantsAsync(PaginationRequestModel model);

        Task<ResponseDTO> UpdateTenantAsync(UpdateTenantRequestModel model);

        Task<ResponseDTO> ActivateDeactivateTenantAsync(int tenantId);
    }

}
