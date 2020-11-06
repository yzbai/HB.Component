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
        Task<TUser> CreateByMobileAsync<TUser>(string mobile, string? loginName, string? password, bool mobileConfirmed, string lastUser, TransactionContext transContext) where TUser : IdenityUser, new();

        Task<IEnumerable<TUser>> GetAsync<TUser>(IEnumerable<string> userGuids, TransactionContext? transContext = null) where TUser : IdenityUser, new();
        Task<TUser?> GetAsync<TUser>(string userGuid, TransactionContext? transContext = null) where TUser : IdenityUser, new();
        Task<TUser?> GetByEmailAsync<TUser>(string email, TransactionContext? transContext = null) where TUser : IdenityUser, new();
        Task<TUser?> GetByMobileAsync<TUser>(string mobile, TransactionContext? transContext = null) where TUser : IdenityUser, new();
        Task<TUser?> GetByLoginNameAsync<TUser>(string loginName, TransactionContext? transContext = null) where TUser : IdenityUser, new();

        /// <exception cref="HB.Component.Identity.IdentityException"></exception>
        /// <exception cref="ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task SetAccessFailedCountAsync<TUser>(string userGuid, long count, string lastUser, TransactionContext transContext) where TUser : IdenityUser, new();

        /// <exception cref="HB.Component.Identity.IdentityException"></exception>
        /// <exception cref="ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task SetLockoutAsync<TUser>(string userGuid, bool lockout, string lastUser, TransactionContext transContext, TimeSpan? lockoutTimeSpan = null) where TUser : IdenityUser, new();


        /// <exception cref="HB.Component.Identity.IdentityException"></exception>
        /// <exception cref="ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task SetPasswordByMobileAsync<TUser>(string mobile, string newPassword, string lastUser, TransactionContext transContext) where TUser : IdenityUser, new();

        /// <exception cref="HB.Component.Identity.IdentityException"></exception>
        /// <exception cref="ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task SetLoginNameAsync<TUser>(string userGuid, string loginName, string lastUser, TransactionContext transContext) where TUser : IdenityUser, new();


        Task<TUser?> ValidateSecurityStampAsync<TUser>(string userGuid, string? securityStamp, TransactionContext? transContext = null) where TUser : IdenityUser, new();
    }


}