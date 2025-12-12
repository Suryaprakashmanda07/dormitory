using Microsoft.EntityFrameworkCore;
using Saas_Dormitory.DAL.Data;
using Saas_Dormitory.DAL.Helpers;
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
        private readonly CurrentUserService _currentUser;
        public TenantRepository(DormitoryDbContext db, CurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<ResponseDTO> CreateTenantAsync(CreateTenantRequestModel model, string UserId)
        {
            ResponseDTO resp = new ResponseDTO();

            try
            {
                var tenant = new Tenant
                {
                    TenantName = model.TenantName,
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
                    CreatedDate = DateTime.UtcNow,
                    UserId = UserId,
                    CreatedBy= _currentUser.GetUserId()
                };

                _db.Userprofiles.Add(profile);
                await _db.SaveChangesAsync();
                resp.Id = tenant.TenantId;
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
                    response.Msg = "Admin not found";
                    return response;
                }

                tenant.IsActive = model.IsActive;
                tenant.UpdatedDate = DateTime.UtcNow;

                _db.Tenants.Update(tenant);
                await _db.SaveChangesAsync();

                response.Valid = true;
                response.Msg = model.IsActive
                                ? $"Admin '{tenant.TenantName}' activated successfully"
                                : $"Admin '{tenant.TenantName}' deactivated successfully";
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
            //Log.Error( "Error in CreateSubscriptionPlanAsync");
            try
            {
                var tenantDetails = await _db.Tenants
                    .Include(t => t.Userprofiles)
                        .ThenInclude(up => up.User)
                    .Where(t => t.TenantId == tenantId )
                    .Select(t => new TenantDetailsModel
                    {
                        TenantId = t.TenantId,
                        TenantName = t.TenantName,
                        FullName=t.Userprofiles.Select(a=>a.FullName).FirstOrDefault(),
                        IsActive = (bool)t.IsActive,
                        UserId = t.Userprofiles.Select(x => x.UserId).FirstOrDefault(),
                        Email = t.Userprofiles
                                 .Select(up => up.User.Email)
                                 .FirstOrDefault(),

                        Phone = t.Userprofiles
                                 .Select(up => up.User.PhoneNumber)
                                 .FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();


                response.Item = tenantDetails;
                response.Msg = tenantDetails == null ? "Tenant not found" : "Success";

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
                var query = _db.Tenants
                    .Where(t => t.Userprofiles.Any(up =>
                        up.User.Roles.Any(r => r.Name.ToLower() == "admin")
                    ))
                    .Select(t => new TenantDetailsModel
                    {
                        TenantId = t.TenantId,
                        TenantName = t.TenantName,
                        UserId = t.Userprofiles
                                    .Where(up => up.User.Roles.Any(r => r.Name.ToLower() == "admin"))
                                    .Select(x => x.UserId)
                                    .FirstOrDefault(),

                        FullName = t.Userprofiles
                                    .Where(up => up.User.Roles.Any(r => r.Name.ToLower() == "admin"))
                                    .Select(x => x.FullName)
                                    .FirstOrDefault(),

                        Phone = t.Userprofiles
                                    .Where(up => up.User.Roles.Any(r => r.Name.ToLower() == "admin"))
                                    .Select(x => x.Phone)
                                    .FirstOrDefault(),

                        Email = t.Userprofiles
                                    .Where(up => up.User.Roles.Any(r => r.Name.ToLower() == "admin"))
                                    .Select(x => x.User.Email)
                                    .FirstOrDefault()
                    })
                    .AsQueryable();

                // ✅ SEARCH
                if (!string.IsNullOrWhiteSpace(model.Search))
                {
                    var s = model.Search.ToLower();

                    query = query.Where(x =>
                        (x.TenantName ?? "").ToLower().Contains(s) ||
                        (x.FullName ?? "").ToLower().Contains(s) ||
                        (x.Phone ?? "").ToLower().Contains(s) ||
                        (x.Email ?? "").ToLower().Contains(s)
                    );
                }

                // ✅ SORT
                query = model.SortColumn?.ToLower() switch
                {
                    "tenantname" => model.SortDirection == "desc"
                        ? query.OrderByDescending(x => x.TenantName)
                        : query.OrderBy(x => x.TenantName),

                   
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
                response.iTotalRecords = totalRecords;
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
                tenant.UpdatedDate = DateTime.UtcNow;

                // 3. Get Related User Profile
                var profile = await _db.Userprofiles
                                       .FirstOrDefaultAsync(u => u.TenantId == model.TenantId && u.UserId==model.UserId);

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
