using Saas_Dormitory.Models.ResponseDTO;
using System.Threading.Tasks;

namespace Saas_Dormitory.DAL.Interface
{
    public interface IRoomRepository
    {
        Task<ResponseDTO> CreateRoomAsync(RoomCreateDto model);
        Task<ResponseDTO> UpdateRoomAsync(RoomUpdateDto model);
        Task<ResponseDTO> UpdateRoomStatusAsync(int roomId);
        Task<SingleItemResponseDTO<RoomDetailsDto>> GetRoomByIdAsync(int roomId);
        Task<ListResponseDTO<RoomDetailsDto>> GetAllRoomsAsync(PaginationRequestModel model);
    }
}

