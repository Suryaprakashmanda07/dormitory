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
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly DormitoryDbContext _db;

        public UserProfileRepository(DormitoryDbContext db)
        {
            _db = db;
        }

        public async Task<ResponseDTO> CreateProfileAsync(CreateUserprofileModel model)
        {
            ResponseDTO resp = new ResponseDTO();

            try
            {
                var profile = new Userprofile
                {
                    FullName = model.FullName,
                    Phone = model.Phone,
                    TenantId = model.TenantId,
                    CreatedBy = model.UserId,
                    CreatedDate = DateTime.UtcNow
                };

                _db.Userprofiles.Add(profile);
                await _db.SaveChangesAsync();

                resp.Valid = true;
                resp.Msg = "Profile created successfully";

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
