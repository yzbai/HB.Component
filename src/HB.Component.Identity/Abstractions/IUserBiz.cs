using HB.Component.Identity.Entities;
using HB.Framework.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.Component.Identity.Abstractions
{
    internal interface IUserBiz
    {
        Task<TUser> CreateByMobileAsync<TUser>(string mobile, string? loginName, string? password, bool mobileConfirmed, string lastUser, TransactionContext transContext) where TUser : IdentityUser, new();

        Task<IEnumerable<TUser>> GetAsync<TUser>(IEnumerable<string> userGuids, TransactionContext? transContext = null) where TUser : IdentityUser, new();
        Task<TUser?> GetAsync<TUser>(string userGuid, TransactionContext? transContext = null) where TUser : IdentityUser, new();
        Task<TUser?> GetByEmailAsync<TUser>(string email, TransactionContext? transContext = null) where TUser : IdentityUser, new();
        Task<TUser?> GetByMobileAsync<TUser>(string mobile, TransactionContext? transContext = null) where TUser : IdentityUser, new();
        Task<TUser?> GetByLoginNameAsync<TUser>(string loginName, TransactionContext? transContext = null) where TUser : IdentityUser, new();

        Task SetPasswordByMobileAsync<TUser>(string mobile, string newPassword, string lastUser, TransactionContext transContext) where TUser : IdentityUser, new();

        Task SetLoginNameAsync<TUser>(string userGuid, string loginName, string lastUser, TransactionContext transContext) where TUser : IdentityUser, new();

        Task<TUser?> GetUserBySecurityStampAsync<TUser>(string userGuid, string? securityStamp, TransactionContext? transContext = null) where TUser : IdentityUser, new();
    }


}