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
        Task<User> ValidateSecurityStampAsync(string userGuid, string securityStamp);
        Task<User> GetUserByMobileAsync(string mobile);
        Task<User> GetUserByUserNameAsync(string userName);
        Task<User> CreateUserByMobileAsync(string userType, string mobile, string userName, string password, bool mobileConfirmed);
        Task SetLockoutAsync(string userGuid, bool lockout, TimeSpan? lockoutTimeSpan = null);
        Task SetAccessFailedCountAsync(string userGuid, long count);
        Task<IEnumerable<Claim>> GetUserClaimAsync(User user);
    }
}
