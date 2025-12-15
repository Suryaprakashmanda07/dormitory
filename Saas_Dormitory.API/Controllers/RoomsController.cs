using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saas_Dormitory.DAL.Interface;
using Saas_Dormitory.Models.ResponseDTO;
using System.Threading.Tasks;

namespace Saas_Dormitory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // Optional
    public class RoomsController : ControllerBase
    {
        private readonly IRoomRepository _roomRepo;

        public RoomsController(IRoomRepository roomRepo)
        {
            _roomRepo = roomRepo;
        }

        // ✅ CREATE
        [HttpPost("CreateRoom")]
        public async Task<IActionResult> CreateRoom([FromBody] RoomCreateDto model)
        {
            var result = await _roomRepo.CreateRoomAsync(model);
            return Ok(result);
        }

        // ✅ UPDATE
        [HttpPost("UpdateRoom")]
        public async Task<IActionResult> UpdateRoom([FromBody] RoomUpdateDto model)
        {
            var result = await _roomRepo.UpdateRoomAsync(model);
            return Ok(result);
        }

        // ✅ STATUS TOGGLE
        [HttpPost("UpdateRoomStatus")]
        public async Task<IActionResult> UpdateRoomStatus(int roomId)
        {
            var result = await _roomRepo.UpdateRoomStatusAsync(roomId);
            return Ok(result);
        }

        // ✅ GET BY ID
        [HttpGet("GetRoomById")]
        public async Task<IActionResult> GetRoomById(int roomId)
        {
            var result = await _roomRepo.GetRoomByIdAsync(roomId);
            return Ok(result);
        }

        // ✅ GET ALL (SEARCH, SORT, PAGINATION)
        [HttpPost("GetAllRooms")]
        public async Task<IActionResult> GetAllRooms([FromBody] PaginationRequestModel model)
        {
            var result = await _roomRepo.GetAllRoomsAsync(model);
            return Ok(result);
        }
    }
}

