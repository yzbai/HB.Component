using HB.Component.Identity.Entity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    public interface IIdentityService
    {
        Task<TUser?> ValidateSecurityStampAsync<TUser>(string userGuid, string? securityStamp) where TUser : User, new();
        Task<TUser?> GetUserByMobileAsync<TUser>(string mobile) where TUser : User, new();
        Task<TUser?> GetUserByUserNameAsync<TUser>(string userName) where TUser : User, new();

        /// <exception cref="DatabaseException"></exception>
        Task<TUser> CreateUserByMobileAsync<TUser>(string mobile, string? userName, string? password, bool mobileConfirmed) where TUser : User, new();

        /// <exception cref="DatabaseException"></exception>
        Task SetLockoutAsync<TUser>(string userGuid, bool lockout, TimeSpan? lockoutTimeSpan = null) where TUser : User, new();

        /// <exception cref="DatabaseException"></exception>
        Task SetAccessFailedCountAsync<TUser>(string userGuid, long count) where TUser : User, new();

        /// <exception cref="DatabaseException"></exception>
        Task<IEnumerable<Claim>> GetUserClaimAsync<TUserClaim, TRole, TRoleOfUser>(User user)
            where TUserClaim : UserClaim, new()
            where TRole : Role, new()
            where TRoleOfUser : RoleOfUser, new();
    }
}
