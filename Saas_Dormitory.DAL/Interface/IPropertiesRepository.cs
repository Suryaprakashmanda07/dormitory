using Saas_Dormitory.Models.ResponseDTO;
using System.Threading.Tasks;

namespace Saas_Dormitory.DAL.Interface
{
    public interface IPropertiesRepository
    {
        Task<ResponseDTO> CreatePropertyAsync(PropertyCreateDto model);
        Task<ResponseDTO> UpdatePropertyAsync(PropertyUpdateDto model);
        Task<ResponseDTO> UpdatePropertyStatusAsync(int propertyId);
        Task<SingleItemResponseDTO<PropertyDetailsDto>> GetPropertyByIdAsync(int propertyId);
        Task<ListResponseDTO<PropertyDetailsDto>> GetAllPropertiesAsync(PaginationRequestModel model);
    }
}
