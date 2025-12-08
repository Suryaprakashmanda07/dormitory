using Microsoft.EntityFrameworkCore;
using Saas_Dormitory.DAL.Data;
using Saas_Dormitory.DAL.Interface;
using Saas_Dormitory.Models.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saas_Dormitory.DAL.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly DormitoryDbContext _db;

        public TenantRepository(DormitoryDbContext db)
        {
            _db = db;
        }

        public async Task<ResponseDTO> CreateTenantAsync(CreateTenantRequestModel model, string UserId)
        {
            ResponseDTO resp = new ResponseDTO();

            try
            {
                var tenant = new Tenant
                {
                    TenantName = model.TenantName,
                    Address = model.Address,
                    CreatedDate = DateTime.UtcNow
                };

                _db.Tenants.Add(tenant);

                // ✅ Save FIRST to generate TenantId
                await _db.SaveChangesAsync();

                var profile = new Userprofile
                {
                    FullName = model.FullName,
                    Phone = model.Phone,
                    TenantId = tenant.TenantId, // ✅ Now this is correct
                    //CreatedDate = DateTime.UtcNow,
                    UserId = UserId
                };

                _db.Userprofiles.Add(profile);
                await _db.SaveChangesAsync();

                resp.Valid = true;
                resp.Msg = "Tenant and profile created successfully";
                return resp;
            }
            catch (Exception ex)
            {
                resp.Valid = false;
                resp.Msg = ex.Message;
                return resp;
            }
        }

        public async Task<ResponseDTO> ActivateDeactivateTenantAsync(TenantStatusRequestModel model)
        {
            var response = new ResponseDTO();

            try
            {
                var tenant = await _db.Tenants.FindAsync(model.TenantId);

                if (tenant == null)
                {
                    response.Valid = false;
                    response.Msg = "Tenant not found";
                    return response;
                }

                tenant.IsActive = model.IsActive;
                tenant.UpdatedDate = DateTime.UtcNow;

                _db.Tenants.Update(tenant);
                await _db.SaveChangesAsync();

                response.Valid = true;
                response.Msg = model.IsActive ? "Tenant activated" : "Tenant deactivated";
                return response;
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Msg = ex.Message;
                return response;
            }
        }

        public async Task<SingleItemResponseDTO<TenantDetailsModel>> GetTenantByTenantIdAsync(int tenantId)
        {
            var response = new SingleItemResponseDTO<TenantDetailsModel>();

            try
            {
                var tenantDetails = await (
                    from t in _db.Tenants
                    join u in _db.Userprofiles on t.TenantId equals u.TenantId
                    join au in _db.AspNetUsers on u.UserId equals au.Id   // AspNetUsers
                    where t.TenantId == tenantId
                    select new TenantDetailsModel
                    {
                        TenantId = t.TenantId,
                        TenantName = t.TenantName,
                        Address = t.Address,
                        IsActive = (bool)t.IsActive,

                        FullName = u.FullName,
                        Phone = u.Phone,
                        Email = au.Email
                    }
                ).FirstOrDefaultAsync();

                if (tenantDetails == null)
                {
                    response.Valid = false;
                    response.Msg = "Tenant not found";
                    return response;
                }

                response.Item = tenantDetails;
                response.Valid = true;
                response.Msg = "Tenant details fetched successfully";

                return response;
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Msg = ex.Message;
                return response;
            }
        }


        public async Task<ListResponseDTO<TenantDetailsModel>> GetAllTenantsAsync(PaginationRequestModel model)
        {
            var response = new ListResponseDTO<TenantDetailsModel>();

            try
            {
                var query =
                    from t in _db.Tenants
                    join u in _db.Userprofiles on t.TenantId equals u.TenantId
                    join au in _db.AspNetUsers on u.UserId equals au.Id
                    select new TenantDetailsModel
                    {
                        TenantId = t.TenantId,
                        TenantName = t.TenantName,
                        Address = t.Address,
                        FullName = u.FullName,
                        Phone = u.Phone,
                        Email = au.Email
                    };

                // ✅ SEARCH
                if (!string.IsNullOrWhiteSpace(model.Search))
                {
                    var s = model.Search.ToLower();
                    query = query.Where(x =>
                        x.TenantName.ToLower().Contains(s) ||
                        x.Address.ToLower().Contains(s) ||
                        x.FullName.ToLower().Contains(s) ||
                        x.Phone.ToLower().Contains(s) ||
                        x.Email.ToLower().Contains(s)
                    );
                }

                // ✅ SORT
                query = model.SortColumn.ToLower() switch
                {
                    "tenantname" => model.SortDirection == "desc"
                        ? query.OrderByDescending(x => x.TenantName)
                        : query.OrderBy(x => x.TenantName),

                    "address" => model.SortDirection == "desc"
                        ? query.OrderByDescending(x => x.Address)
                        : query.OrderBy(x => x.Address),

                    "fullname" => model.SortDirection == "desc"
                        ? query.OrderByDescending(x => x.FullName)
                        : query.OrderBy(x => x.FullName),

                    "phone" => model.SortDirection == "desc"
                        ? query.OrderByDescending(x => x.Phone)
                        : query.OrderBy(x => x.Phone),

                    "email" => model.SortDirection == "desc"
                        ? query.OrderByDescending(x => x.Email)
                        : query.OrderBy(x => x.Email),

                    _ => query.OrderBy(x => x.TenantId)
                };

                // ✅ PAGINATION
                var pageNumber = model.PageNumber <= 0 ? 1 : model.PageNumber;
                var pageSize = model.PageSize <= 0 ? 10 : model.PageSize;

                var totalRecords = await query.CountAsync();

                var result = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // ✅ RESPONSE
                response.Items = result;
                //response.TotalRecords = totalRecords;
                //response.PageNumber = pageNumber;
                //response.PageSize = pageSize;
                response.Valid = true;
                response.Msg = "Tenant data fetched successfully";

                return response;
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Msg = ex.Message;
                return response;
            }
        }

        public async Task<ResponseDTO> UpdateTenantAsync(UpdateTenantRequestModel model)
        {
            var resp = new ResponseDTO();

            try
            {
                // 1. Get Tenant
                var tenant = await _db.Tenants.FindAsync(model.TenantId);

                if (tenant == null)
                {
                    resp.Valid = false;
                    resp.Msg = "Tenant not found";
                    return resp;
                }

                // 2. Update Tenant Fields
                tenant.TenantName = model.TenantName;
                tenant.Address = model.Address;
                tenant.UpdatedDate = DateTime.UtcNow;

                // 3. Get Related User Profile
                var profile = await _db.Userprofiles
                                       .FirstOrDefaultAsync(u => u.UserId == model.UserId);

                if (profile == null)
                {
                    resp.Valid = false;
                    resp.Msg = "User profile not found";
                    return resp;
                }

                // 4. Update User Profile Fields
                profile.FullName = model.FullName;
                profile.Phone = model.Phone;
                profile.UpdatedDate = DateTime.UtcNow;

                // 5. Save changes
                await _db.SaveChangesAsync();

                resp.Valid = true;
                resp.Msg = "Tenant updated successfully";
                return resp;
            }
            catch (Exception ex)
            {
                resp.Valid = false;
                resp.Msg = ex.Message;
                return resp;
            }
        }

    }

}
