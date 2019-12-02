using HB.Component.Authorization.Entity;
using HB.Framework.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Component.Authorization.Abstractions
{
    internal interface ISignInTokenBiz
    {
        Task<SignInToken> CreateAsync(string userGuid, string deviceId, string deviceType, string deviceVersion, string deviceAddress, string ipAddress, TimeSpan expireTimeSpan, TransactionContext transContext = null);
        Task DeleteAppClientTokenByUserGuidAsync(string userGuid, TransactionContext transContext);
        Task DeleteAsync(string signInTokenGuid, TransactionContext transContext);
        Task DeleteByUserGuidAsync(string userGuid, TransactionContext transContext);
        Task<SignInToken> GetAsync(string signInTokenGuid, string refreshToken, string deviceId, string userGuid, TransactionContext transContext = null);
        Task UpdateAsync(SignInToken signInToken, TransactionContext transContext = null);
    }
}
