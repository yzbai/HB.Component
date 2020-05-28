using HB.Component.Identity.Entity;
using HB.Framework.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.Component.Identity.Abstractions
{
    internal interface IUserBiz
    {
        /// <exception cref="HB.Component.Identity.IdentityException"></exception>
        /// <exception cref="ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task<TUser> CreateByMobileAsync<TUser>(string mobile, string? userName, string? password, bool mobileConfirmed, TransactionContext transContext) where TUser : User, new();

        Task<IEnumerable<TUser>> GetAsync<TUser>(IEnumerable<string> userGuids, TransactionContext? transContext = null) where TUser : User, new();
        Task<TUser?> GetAsync<TUser>(string userGuid, TransactionContext? transContext = null) where TUser : User, new();
        Task<TUser?> GetByEmailAsync<TUser>(string email, TransactionContext? transContext = null) where TUser : User, new();
        Task<TUser?> GetByMobileAsync<TUser>(string mobile, TransactionContext? transContext = null) where TUser : User, new();
        Task<TUser?> GetByUserNameAsync<TUser>(string userName, TransactionContext? transContext = null) where TUser : User, new();

        /// <exception cref="HB.Component.Identity.IdentityException"></exception>
        /// <exception cref="ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task SetAccessFailedCountAsync<TUser>(string userGuid, long count, TransactionContext transContext) where TUser : User, new();

        /// <exception cref="HB.Component.Identity.IdentityException"></exception>
        /// <exception cref="ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task SetLockoutAsync<TUser>(string userGuid, bool lockout, TransactionContext transContext, TimeSpan? lockoutTimeSpan = null) where TUser : User, new();


        /// <exception cref="HB.Component.Identity.IdentityException"></exception>
        /// <exception cref="ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task SetPasswordByMobileAsync<TUser>(string mobile, string newPassword, TransactionContext transContext) where TUser : User, new();

        /// <exception cref="HB.Component.Identity.IdentityException"></exception>
        /// <exception cref="ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task SetUserNameAsync<TUser>(string userGuid, string userName, TransactionContext transContext) where TUser : User, new();


        Task<TUser?> ValidateSecurityStampAsync<TUser>(string userGuid, string? securityStamp, TransactionContext? transContext = null) where TUser : User, new();
    }


}