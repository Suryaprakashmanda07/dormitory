using Microsoft.EntityFrameworkCore;
using Saas_Dormitory.DAL.Data;
using Saas_Dormitory.DAL.Enums;
using Saas_Dormitory.DAL.Helpers;
using Saas_Dormitory.DAL.Interface;
using Saas_Dormitory.Models.ResponseDTO;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Saas_Dormitory.DAL.Repositories
{
    public class SubscriptionPlansRepository : ISubscriptionPlansRepository
    {
        private readonly DormitoryDbContext _db;
        private readonly CurrentUserService _currentUser;
        public SubscriptionPlansRepository(DormitoryDbContext db, CurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        // ✅ CREATE
        public async Task<ResponseDTO> CreateSubscriptionPlanAsync(SubscriptionPlanCreateDto model)
        {
            var response = new ResponseDTO();

            try
            {
                // ✅ Check for duplicate subscription plan name (case-insensitive)
                var existingPlan = await _db.Subscriptionplans
                    .FirstOrDefaultAsync(p => p.Name.ToLower() == model.Name.ToLower());

                if (existingPlan != null)
                {
                    response.Valid = false;
                    response.Msg = "Subscription plan with this name already exists";
                    return response;
                }

                var plan = new Subscriptionplan
                {
                    Name = model.Name,
                    Price = model.Price,
                    MaxProperties = model.MaxProperties,
                    MaxUsers = model.MaxUsers,
                     Period = model.Period,
                     Isactive=true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy=_currentUser.GetUserId(),
                };

                _db.Subscriptionplans.Add(plan);
                await _db.SaveChangesAsync();

                response.Id = plan.PlanId;
                response.Valid = true;
                response.Msg = "Subscription plan created successfully";

                return response;
            }
            catch (Exception ex)
            {
               // Log.Error(ex, "Error in CreateSubscriptionPlanAsync");
                response.Valid = false;
                response.Msg = ex.Message;
                return response;
            }
        }

        // ✅ GET BY ID
        public async Task<SingleItemResponseDTO<SubscriptionPlanDetailsModel>> GetSubscriptionPlanByIdAsync(int planId)
        {
            var response = new SingleItemResponseDTO<SubscriptionPlanDetailsModel>();

            try
            {
                var plan = await _db.Subscriptionplans
                    .Where(x => x.PlanId == planId)
                    .Select(p => new SubscriptionPlanDetailsModel
                    {
                        PlanId = p.PlanId,
                        Name = p.Name,
                        Price = p.Price,
                        MaxProperties = p.MaxProperties,
                        MaxUsers = p.MaxUsers,
                        Isactive = p.Isactive.GetValueOrDefault(false),
                        Period = (int)p.Period,
                        PeriodName = EnumHelper.GetDescription((SubscriptionPeriod)p.Period)
                    })
                    .FirstOrDefaultAsync();

                if (plan == null)
                {
                    response.Valid = false;
                    response.Msg = "Subscription plan not found";
                    return response;
                }

                response.Item = plan;
                response.Valid = true;
                response.Msg = "Subscription plan fetched successfully";

                return response;
            }
            catch (Exception ex)
            {
               // Log.Error(ex, "Error in GetSubscriptionPlanByIdAsync");
                response.Valid = false;
                response.Msg = ex.Message;
                return response;
            }
        }

        // ✅ GET ALL (with Search, Sort, Pagination)
        public async Task<ListResponseDTO<SubscriptionPlanDetailsModel>> GetAllSubscriptionPlansAsync(PaginationRequestModel model)
        {
            var response = new ListResponseDTO<SubscriptionPlanDetailsModel>();

            try
            {
                var query = _db.Subscriptionplans
                    .Select(p => new SubscriptionPlanDetailsModel
                    {
                        PlanId = p.PlanId,
                        Name = p.Name,
                        Price = p.Price,
                        MaxProperties = p.MaxProperties,
                        MaxUsers = p.MaxUsers,
                        Isactive=p.Isactive,
                        Period = (int)p.Period,
                        PeriodName = EnumHelper.GetDescription((SubscriptionPeriod)p.Period)
                    })
                    .AsQueryable();

                // ✅ SEARCH
                if (!string.IsNullOrWhiteSpace(model.Search))
                {
                    var search = model.Search.ToLower();

                    query = query.Where(x =>
                        x.Name.ToLower().Contains(search) ||
                        x.Price.ToString().Contains(search) ||
                        x.MaxProperties.ToString().Contains(search) ||
                        x.MaxUsers.ToString().Contains(search)
                    );
                }

                query = model.SortColumn?.ToLower() switch
                {
                    "name" => model.SortDirection == "desc"
                                ? query.OrderByDescending(x => x.Name)
                                : query.OrderBy(x => x.Name),

                    "price" => model.SortDirection == "desc"
                                ? query.OrderByDescending(x => x.Price)
                                : query.OrderBy(x => x.Price),

                    "maxproperties" => model.SortDirection == "desc"
                                ? query.OrderByDescending(x => x.MaxProperties)
                                : query.OrderBy(x => x.MaxProperties),

                    "maxusers" => model.SortDirection == "desc"
                                ? query.OrderByDescending(x => x.MaxUsers)
                                : query.OrderBy(x => x.MaxUsers),

                    // Period sorting (numerical)
                    "period" => model.SortDirection == "desc"
                                ? query.OrderByDescending(x => x.Period)
                                : query.OrderBy(x => x.Period),

                    _ => query.OrderBy(x => x.PlanId)
                };


                // ✅ PAGINATION
                var pageNumber = model.PageNumber <= 0 ? 1 : model.PageNumber;
                var pageSize = model.PageSize <= 0 ? 10 : model.PageSize;

                var totalRecords = await query.CountAsync();

                var result = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                response.Items = result;
                response.iTotalRecords = totalRecords;
                response.iTotalDisplayRecords = pageNumber;
                response.iTotalDisplayRecords = pageSize;

                response.Valid = true;
                response.Msg = "Subscription plans fetched successfully";

                return response;
            }
            catch (Exception ex)
            {
                //Log.Error(ex, "Error in GetAllSubscriptionPlansAsync");
                response.Valid = false;
                response.Msg = ex.Message;
                return response;
            }
        }

        // ✅ UPDATE
        public async Task<ResponseDTO> UpdateSubscriptionPlanAsync(SubscriptionPlanUpdateDto model)
        {
            var response = new ResponseDTO();

            try
            {
                var plan = await _db.Subscriptionplans.FindAsync(model.PlanId);

                if (plan == null)
                {
                    response.Valid = false;
                    response.Msg = "Subscription plan not found";
                    return response;
                }

                // ✅ Check for duplicate subscription plan name (case-insensitive, excluding current plan)
                var existingPlan = await _db.Subscriptionplans
                    .FirstOrDefaultAsync(p => p.Name.ToLower() == model.Name.ToLower() && p.PlanId != model.PlanId);

                if (existingPlan != null)
                {
                    response.Valid = false;
                    response.Msg = "Subscription plan with this name already exists";
                    return response;
                }

                plan.Name = model.Name;
                plan.Price = model.Price;
                plan.MaxProperties = model.MaxProperties;
                plan.MaxUsers = model.MaxUsers;
                plan.Period = model.Period;
                plan.UpdatedDate = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                response.Valid = true;
                response.Msg = "Subscription plan updated successfully";
                return response;
            }
            catch (Exception ex)
            {
                //Log.Error(ex, "Error in UpdateSubscriptionPlanAsync");
                response.Valid = false;
                response.Msg = ex.Message;
                return response;
            }
        }

        public async Task<ResponseDTO> UpdateSubscriptionPlanStatusAsync(int planId)
        {
            var response = new ResponseDTO();

            try
            {
                var plan = await _db.Subscriptionplans.FindAsync(planId);

                if (plan == null)
                {
                    response.Valid = false;
                    response.Msg = "Subscription plan not found";
                    return response;
                }

                // ✅ AUTO TOGGLE STATUS (handle nullable bool)
                plan.Isactive = !(plan.Isactive ?? false);
                plan.UpdatedDate = DateTime.UtcNow;

                _db.Subscriptionplans.Update(plan);
                await _db.SaveChangesAsync();

                response.Valid = true;
                response.Msg = (bool)plan.Isactive
                    ? $"Subscription plan '{plan.Name}' activated successfully"
                    : $"Subscription plan '{plan.Name}' deactivated successfully";

                return response;
            }
            catch (Exception ex)
            {
                //Log.Error(ex, "Error while updating SubscriptionPlan status. PlanId: {PlanId}", planId);
                response.Valid = false;
                response.Msg = ex.Message;
                return response;
            }
        }
        public async Task<ListResponseDTO<SubscriptionPeriodLookupModel>> GetSubscriptionPeriodsAsync()
        {
            var response = new ListResponseDTO<SubscriptionPeriodLookupModel>();

            try
            {
                var periods = Enum.GetValues(typeof(SubscriptionPeriod))
                                  .Cast<SubscriptionPeriod>()
                                  .Select(x => new SubscriptionPeriodLookupModel
                                  {
                                      Id = (int)x,
                                      PeriodName = EnumHelper.GetDescription(x)
                                  })
                                  .ToList();

                response.Valid = true;
                response.Items = periods;
                response.Msg = "Period list fetched successfully";
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Msg = $"Error occurred while fetching period list: {ex.Message}";
                // Optional: Log the exception
                // _logger.LogError(ex, "GetSubscriptionPeriodsAsync failed");
            }

            return response;
        }


    }
}
