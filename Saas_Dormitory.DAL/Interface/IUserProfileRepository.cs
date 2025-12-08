using Saas_Dormitory.Models.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saas_Dormitory.DAL.Interface
{
    public interface IUserProfileRepository
    {
        Task<ResponseDTO> CreateProfileAsync(CreateUserprofileModel profile);
    }

}
