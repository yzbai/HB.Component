using HB.Component.Authorization.Entity;
using HB.Framework.Database;
using System;
using System.Threading.Tasks;

namespace HB.Component.Authorization.Abstractions
{
    internal interface ISignInTokenBiz
    {
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task<SignInToken> CreateAsync(string userGuid, string deviceId, string deviceType, string deviceVersion, string deviceAddress, string ipAddress, TimeSpan expireTimeSpan, TransactionContext? transContext = null);


        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        Task DeleteAppClientTokenByUserGuidAsync(string userGuid, TransactionContext transContext);


        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task DeleteAsync(string signInTokenGuid, TransactionContext transContext);

        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task DeleteByUserGuidAsync(string userGuid, TransactionContext transContext);


        Task<SignInToken?> GetAsync(string? signInTokenGuid, string? refreshToken, string deviceId, string? userGuid, TransactionContext? transContext = null);


        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task UpdateAsync(SignInToken signInToken, TransactionContext? transContext = null);
    }
}
