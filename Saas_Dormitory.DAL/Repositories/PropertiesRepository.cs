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
    public class PropertiesRepository : IPropertiesRepository
    {
        private readonly DormitoryDbContext _db;
        private readonly CurrentUserService _currentUser;

        public PropertiesRepository(DormitoryDbContext db, CurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        // ✅ CREATE
        public async Task<ResponseDTO> CreatePropertyAsync(PropertyCreateDto model)
        {
            var response = new ResponseDTO();
            try
            {
                var property = new Property
                {
                    TenantId = model.TenantId,
                    Name = model.Name,
                    Address = model.Address,
                    City = model.City,
                    State = model.State,
                    Country = model.Country,
                    CreatedDate = DateTime.UtcNow,
                    Isactive=true,
                    CreatedBy = _currentUser.GetUserId()
                };

                _db.Properties.Add(property);
                await _db.SaveChangesAsync();

                response.Id = property.PropertyId;
                response.Valid = true;
                response.Msg = "Property created successfully";
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
        public async Task<SingleItemResponseDTO<PropertyDetailsDto>> GetPropertyByIdAsync(int propertyId)
        {
            var response = new SingleItemResponseDTO<PropertyDetailsDto>();

            try
            {
                var property = await _db.Properties
                    .Where(x => x.PropertyId == propertyId)
                    .Select(p => new PropertyDetailsDto
                    {
                        PropertyId = p.PropertyId,
                        TenantId = p.TenantId,
                        Name = p.Name,
                        Address = p.Address,
                        City = p.City,
                        State = p.State,
                        Country = p.Country,
                        IsActive=p.Isactive,
                        CreatedDate = p.CreatedDate
                    })
                    .FirstOrDefaultAsync();

                if (property == null)
                {
                    response.Valid = false;
                    response.Msg = "Property not found";
                    return response;
                }

                response.Valid = true;
                response.Item = property;
                response.Msg = "Property fetched successfully";
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
        public async Task<ListResponseDTO<PropertyDetailsDto>> GetAllPropertiesAsync(PaginationRequestModel model)
        {
            var response = new ListResponseDTO<PropertyDetailsDto>();

            try
            {
                var query = _db.Properties
                    .Select(p => new PropertyDetailsDto
                    {
                        PropertyId = p.PropertyId,
                        TenantId = p.TenantId,
                        Name = p.Name,
                        Address = p.Address,
                        City = p.City,
                        State = p.State,
                        Country = p.Country,
                        IsActive=p.Isactive
                    })
                    .AsQueryable();

                // ✅ SEARCH
                if (!string.IsNullOrWhiteSpace(model.Search))
                {
                    var search = model.Search.ToLower();
                    query = query.Where(x =>
                        x.Name.ToLower().Contains(search) ||
                        x.City.ToLower().Contains(search) ||
                        x.State.ToLower().Contains(search) ||
                        x.Country.ToLower().Contains(search)
                    );
                }

                // ✅ SORTING
                query = model.SortColumn?.ToLower() switch
                {
                    "name" => model.SortDirection == "desc"
                        ? query.OrderByDescending(x => x.Name)
                        : query.OrderBy(x => x.Name),

                    "city" => model.SortDirection == "desc"
                        ? query.OrderByDescending(x => x.City)
                        : query.OrderBy(x => x.City),
                    "state" => model.SortDirection == "desc"
                   ? query.OrderByDescending(x => x.State)
                   : query.OrderBy(x => x.State),
                    "country" => model.SortDirection == "desc"
                   ? query.OrderByDescending(x => x.Country)
                   : query.OrderBy(x => x.Country),
                    _ => query.OrderBy(x => x.PropertyId)
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
                response.Msg = "Properties fetched successfully";

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
        public async Task<ResponseDTO> UpdatePropertyAsync(PropertyUpdateDto model)
        {
            var response = new ResponseDTO();
            try
            {
                var property = await _db.Properties.FindAsync(model.PropertyId);
                if (property == null)
                {
                    response.Valid = false;
                    response.Msg = "Property not found";
                    return response;
                }

                property.Name = model.Name;
                property.Address = model.Address;
                property.City = model.City;
                property.State = model.State;
                property.Country = model.Country;
                property.UpdatedDate = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                response.Valid = true;
                response.Msg = "Property updated successfully";
                return response;
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Msg = ex.Message;
                return response;
            }
        }

        // ✅ DELETE
        public async Task<ResponseDTO> UpdatePropertyStatusAsync(int propertyId)
        {
            var response = new ResponseDTO();

            try
            {
                var property = await _db.Properties.FindAsync(propertyId);

                if (property == null)
                {
                    response.Valid = false;
                    response.Msg = "Property not found";
                    return response;
                }

                // ✅ AUTO TOGGLE STATUS
                property.Isactive = !property.Isactive;
                property.UpdatedDate = DateTime.UtcNow;

                _db.Properties.Update(property);
                await _db.SaveChangesAsync();

                response.Valid = true;
                response.Msg = (bool)property.Isactive
                    ? "Property activated successfully"
                    : "Property deactivated successfully";

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
