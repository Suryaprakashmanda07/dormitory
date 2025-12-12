using Saas_Dormitory.Models.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saas_Dormitory.DAL.Interface
{
    public interface ISubscriptionPlansRepository
    {
        Task<ResponseDTO> CreateSubscriptionPlanAsync(SubscriptionPlanCreateDto model);

        Task<SingleItemResponseDTO<SubscriptionPlanDetailsModel>> GetSubscriptionPlanByIdAsync(int planId);

        Task<ListResponseDTO<SubscriptionPlanDetailsModel>> GetAllSubscriptionPlansAsync(PaginationRequestModel model);

        Task<ResponseDTO> UpdateSubscriptionPlanAsync(SubscriptionPlanUpdateDto model);

        Task<ResponseDTO> UpdateSubscriptionPlanStatusAsync(int planId);
        Task<ListResponseDTO<SubscriptionPeriodLookupModel>> GetSubscriptionPeriodsAsync();

    }

}
