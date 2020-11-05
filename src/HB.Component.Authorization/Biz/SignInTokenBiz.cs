using HB.Component.Authorization.Abstractions;
using HB.Component.Authorization.Entity;
using HB.Framework.Database;
using HB.Framework.Database.SQL;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.Component.Authorization
{
    internal class SignInTokenBiz : ISignInTokenBiz
    {
        private readonly IDatabase _db;

        public SignInTokenBiz(IDatabase database)
        {
            _db = database;
        }

        /// <summary>
        /// CreateAsync
        /// </summary>
        /// <param name="userGuid"></param>
        /// <param name="deviceId"></param>
        /// <param name="deviceType"></param>
        /// <param name="deviceVersion"></param>
        /// <param name="deviceAddress"></param>
        /// <param name="ipAddress"></param>
        /// <param name="expireTimeSpan"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task<SignInToken> CreateAsync(string userGuid, string deviceId, string deviceType, string deviceVersion, /*string deviceAddress,*/ string ipAddress, TimeSpan expireTimeSpan, TransactionContext? transContext = null)
        {
            SignInToken token = new SignInToken
            {
                Guid = SecurityUtil.CreateUniqueToken(),
                UserGuid = userGuid,
                RefreshToken = SecurityUtil.CreateUniqueToken(),
                RefreshCount = 0,
                Blacked = false,
                DeviceId = deviceId,
                DeviceType = deviceType,
                DeviceVersion = deviceVersion,
                //DeviceAddress = deviceAddress,
                DeviceIp = ipAddress,
                ExpireAt = DateTimeOffset.UtcNow + expireTimeSpan
            };

            await _db.AddAsync(token, transContext).ConfigureAwait(false);

            return token;
        }

        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        public async Task DeleteAppClientTokenByUserGuidAsync(string userGuid, TransactionContext transContext)
        {
            ThrowIf.Empty(userGuid, nameof(userGuid));
            ThrowIf.Null(transContext, nameof(transContext));

            WhereExpression<SignInToken> where = _db.Where<SignInToken>()
                .Where(at => at.DeviceType != Enum.GetName(typeof(DeviceType), DeviceType.Web))
                .And(at => at.UserGuid == userGuid);

            IEnumerable<SignInToken> resultList = await _db.RetrieveAsync(where, transContext).ConfigureAwait(false);

            await _db.BatchDeleteAsync(resultList, transContext).ConfigureAwait(false);
        }

        /// <summary>
        /// DeleteByUserGuidAsync
        /// </summary>
        /// <param name="userGuid"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task DeleteByUserGuidAsync(string userGuid, TransactionContext transContext)
        {
            ThrowIf.NullOrEmpty(userGuid, nameof(userGuid));
            ThrowIf.Null(transContext, nameof(transContext));

            IEnumerable<SignInToken> resultList = await _db.RetrieveAsync<SignInToken>(at => at.UserGuid == userGuid, transContext).ConfigureAwait(false);

            await _db.BatchDeleteAsync(resultList, transContext).ConfigureAwait(false);
        }

        /// <summary>
        /// DeleteAsync
        /// </summary>
        /// <param name="signInTokenGuid"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task DeleteAsync(string signInTokenGuid, TransactionContext transContext)
        {
            ThrowIf.NullOrEmpty(signInTokenGuid, nameof(signInTokenGuid));
            ThrowIf.Null(transContext, nameof(transContext));

            IEnumerable<SignInToken> resultList = await _db.RetrieveAsync<SignInToken>(at => at.Guid == signInTokenGuid, transContext).ConfigureAwait(false);

            await _db.BatchDeleteAsync(resultList, transContext).ConfigureAwait(false);
        }

        public async Task<SignInToken?> GetAsync(string? signInTokenGuid, string? refreshToken, string deviceId, string? userGuid, TransactionContext? transContext = null)
        {
            if (signInTokenGuid.IsNullOrEmpty() || refreshToken.IsNullOrEmpty() || userGuid.IsNullOrEmpty())
            {
                return null;
            }

            return await _db.ScalarAsync<SignInToken>(s =>
                s.UserGuid == userGuid &&
                s.Guid == signInTokenGuid &&
                s.RefreshToken == refreshToken &&
                s.DeviceId == deviceId, transContext).ConfigureAwait(false);
        }

        /// <summary>
        /// UpdateAsync
        /// </summary>
        /// <param name="signInToken"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public Task UpdateAsync(SignInToken signInToken, TransactionContext? transContext = null)
        {
            return _db.UpdateAsync(signInToken, transContext);
        }
    }
}
