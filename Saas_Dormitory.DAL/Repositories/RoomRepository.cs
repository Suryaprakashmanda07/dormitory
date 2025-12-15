using Microsoft.EntityFrameworkCore;
using Saas_Dormitory.DAL.Data;
using Saas_Dormitory.DAL.Helpers;
using Saas_Dormitory.DAL.Interface;
using Saas_Dormitory.Models.ResponseDTO;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Saas_Dormitory.DAL.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly DormitoryDbContext _db;
        private readonly CurrentUserService _currentUser;

        public RoomRepository(DormitoryDbContext db, CurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        // ✅ CREATE
        public async Task<ResponseDTO> CreateRoomAsync(RoomCreateDto model)
        {
            var response = new ResponseDTO();
            try
            {
                // Check if room number already exists for this property
                var existingRoom = await _db.Rooms
                    .FirstOrDefaultAsync(r => r.RoomNumber == model.RoomNumber && r.PropertyId == model.PropertyId);

                if (existingRoom != null)
                {
                    response.Valid = false;
                    response.Msg = "Room number already exists for this property";
                    return response;
                }

                var room = new Room
                {
                    TenantId = model.TenantId,
                    PropertyId = model.PropertyId,
                    RoomNumber = model.RoomNumber,
                    RoomType = model.RoomType,
                    Capacity = model.Capacity,
                    RentAmount = model.RentAmount,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true,
                    CreatedBy = _currentUser.GetUserId()
                };

                _db.Rooms.Add(room);
                await _db.SaveChangesAsync();

                response.Id = room.RoomId;
                response.Valid = true;
                response.Msg = "Room created successfully";
                return response;
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Msg = ex.Message;
                return response;
            }
        }

        // ✅ GET BY ID
        public async Task<SingleItemResponseDTO<RoomDetailsDto>> GetRoomByIdAsync(int roomId)
        {
            var response = new SingleItemResponseDTO<RoomDetailsDto>();

            try
            {
                var room = await _db.Rooms
                    .Include(r => r.Property)
                    .Where(x => x.RoomId == roomId)
                    .Select(r => new RoomDetailsDto
                    {
                        RoomId = r.RoomId,
                        TenantId = r.TenantId,
                        PropertyId = r.PropertyId,
                        RoomNumber = r.RoomNumber,
                        RoomType = r.RoomType,
                        Capacity = r.Capacity,
                        RentAmount = r.RentAmount,
                        IsActive = r.IsActive,
                        CreatedDate = r.CreatedDate,
                        UpdatedDate = r.UpdatedDate,
                        PropertyName = r.Property.Name
                    })
                    .FirstOrDefaultAsync();

                if (room == null)
                {
                    response.Valid = false;
                    response.Msg = "Room not found";
                    return response;
                }

                response.Valid = true;
                response.Item = room;
                response.Msg = "Room fetched successfully";
                return response;
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Msg = ex.Message;
                return response;
            }
        }

        // ✅ GET ALL WITH SEARCH, SORT & PAGINATION
        public async Task<ListResponseDTO<RoomDetailsDto>> GetAllRoomsAsync(PaginationRequestModel model)
        {
            var response = new ListResponseDTO<RoomDetailsDto>();

            try
            {
                var query = _db.Rooms
                    .Include(r => r.Property)
                    .Select(r => new RoomDetailsDto
                    {
                        RoomId = r.RoomId,
                        TenantId = r.TenantId,
                        PropertyId = r.PropertyId,
                        RoomNumber = r.RoomNumber,
                        RoomType = r.RoomType,
                        Capacity = r.Capacity,
                        RentAmount = r.RentAmount,
                        IsActive = r.IsActive,
                        CreatedDate = r.CreatedDate,
                        UpdatedDate = r.UpdatedDate,
                        PropertyName = r.Property.Name
                    })
                    .AsQueryable();

                // ✅ SEARCH
                if (!string.IsNullOrWhiteSpace(model.Search))
                {
                    var search = model.Search.ToLower();
                    query = query.Where(x =>
                        x.RoomNumber.ToLower().Contains(search) ||
                        (x.RoomType != null && x.RoomType.ToLower().Contains(search)) ||
                        (x.PropertyName != null && x.PropertyName.ToLower().Contains(search))
                    );
                }

                // ✅ SORTING
                query = model.SortColumn?.ToLower() switch
                {
                    "roomnumber" => model.SortDirection == "desc"
                        ? query.OrderByDescending(x => x.RoomNumber)
                        : query.OrderBy(x => x.RoomNumber),

                    "roomtype" => model.SortDirection == "desc"
                        ? query.OrderByDescending(x => x.RoomType)
                        : query.OrderBy(x => x.RoomType),

                    "propertyname" => model.SortDirection == "desc"
                        ? query.OrderByDescending(x => x.PropertyName)
                        : query.OrderBy(x => x.PropertyName),

                    "rentamount" => model.SortDirection == "desc"
                        ? query.OrderByDescending(x => x.RentAmount)
                        : query.OrderBy(x => x.RentAmount),

                    "capacity" => model.SortDirection == "desc"
                        ? query.OrderByDescending(x => x.Capacity)
                        : query.OrderBy(x => x.Capacity),

                    _ => query.OrderBy(x => x.RoomId)
                };

                // ✅ PAGINATION
                int pageSize = model.PageSize <= 0 ? 10 : model.PageSize;
                int pageNumber = model.PageNumber <= 0 ? 1 : model.PageNumber;

                var totalRecords = await query.CountAsync();
                var result = await query.Skip((pageNumber - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToListAsync();

                response.Items = result;
                response.iTotalRecords = totalRecords;
                response.iTotalDisplayRecords = result.Count;
                response.Valid = true;
                response.Msg = "Rooms fetched successfully";

                return response;
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Msg = ex.Message;
                return response;
            }
        }

        // ✅ UPDATE
        public async Task<ResponseDTO> UpdateRoomAsync(RoomUpdateDto model)
        {
            var response = new ResponseDTO();
            try
            {
                var room = await _db.Rooms.FindAsync(model.RoomId);
                if (room == null)
                {
                    response.Valid = false;
                    response.Msg = "Room not found";
                    return response;
                }

                // Check if room number already exists for this property (excluding current room)
                var existingRoom = await _db.Rooms
                    .FirstOrDefaultAsync(r => r.RoomNumber == model.RoomNumber 
                        && r.PropertyId == model.PropertyId 
                        && r.RoomId != model.RoomId);

                if (existingRoom != null)
                {
                    response.Valid = false;
                    response.Msg = "Room number already exists for this property";
                    return response;
                }

                room.PropertyId = model.PropertyId;
                room.RoomNumber = model.RoomNumber;
                room.RoomType = model.RoomType;
                room.Capacity = model.Capacity;
                room.RentAmount = model.RentAmount;
                room.UpdatedDate = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                response.Valid = true;
                response.Msg = "Room updated successfully";
                return response;
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Msg = ex.Message;
                return response;
            }
        }

        // ✅ STATUS TOGGLE
        public async Task<ResponseDTO> UpdateRoomStatusAsync(int roomId)
        {
            var response = new ResponseDTO();

            try
            {
                var room = await _db.Rooms.FindAsync(roomId);

                if (room == null)
                {
                    response.Valid = false;
                    response.Msg = "Room not found";
                    return response;
                }

                // ✅ AUTO TOGGLE STATUS
                room.IsActive = !room.IsActive;
                room.UpdatedDate = DateTime.UtcNow;

                _db.Rooms.Update(room);
                await _db.SaveChangesAsync();

                response.Valid = true;
                response.Msg = (bool)room.IsActive
                    ? "Room activated successfully"
                    : "Room deactivated successfully";

                return response;
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Msg = ex.Message;
                return response;
            }
        }
    }
}

