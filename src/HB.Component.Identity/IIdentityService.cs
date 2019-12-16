using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using HB.Component.Identity.Entity;

namespace HB.Component.Identity
{
    public interface IIdentityService
    {
        Task<TUser> ValidateSecurityStampAsync<TUser>(string userGuid, string securityStamp) where TUser : User, new();
        Task<TUser> GetUserByMobileAsync<TUser>(string mobile) where TUser : User, new();
        Task<TUser> GetUserByUserNameAsync<TUser>(string userName) where TUser : User, new();
        Task<TUser> CreateUserByMobileAsync<TUser>(string userType, string mobile, string userName, string password, bool mobileConfirmed) where TUser : User, new();
        Task SetLockoutAsync<TUser>(string userGuid, bool lockout, TimeSpan? lockoutTimeSpan = null) where TUser : User, new();
        Task SetAccessFailedCountAsync<TUser>(string userGuid, long count) where TUser : User, new();
        Task<IEnumerable<Claim>> GetUserClaimAsync<TUser, TUserClaim, TRole, TUserRole>(TUser user)
            where TUser : User, new()
            where TUserClaim : UserClaim, new()
            where TRole : Role, new()
            where TUserRole : UserRole, new();
    }
}
