using HB.Component.Authorization.Abstractions;
using HB.Component.Authorization.Entity;
using HB.Framework.Database;
using HB.Framework.Database.SQL;
using Microsoft.Extensions.Logging;
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

        public async Task<SignInToken> CreateAsync(string userGuid, string deviceId, string deviceType, string deviceVersion, string deviceAddress, string ipAddress, TimeSpan expireTimeSpan, TransactionContext transContext = null)
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
                DeviceAddress = deviceAddress,
                DeviceIp = ipAddress,
                ExpireAt = DateTimeOffset.UtcNow + expireTimeSpan
            };

            await _db.AddAsync(token, transContext).ConfigureAwait(false);

            return token;
        }

        public async Task DeleteAppClientTokenByUserGuidAsync(string userGuid, TransactionContext transContext)
        {
            ThrowIf.NullOrEmpty(userGuid, nameof(userGuid));
            ThrowIf.Null(transContext, nameof(transContext));

            WhereExpression<SignInToken> where = _db.Where<SignInToken>()
                .Where(at => at.DeviceType != Enum.GetName(typeof(DeviceType), DeviceType.Web))
                .And(at => at.UserGuid == userGuid);

            IList<SignInToken> resultList = await _db.RetrieveAsync(where, transContext).ConfigureAwait(false);

            await _db.BatchDeleteAsync(resultList, transContext).ConfigureAwait(false);
        }

        public async Task DeleteByUserGuidAsync(string userGuid, TransactionContext transContext)
        {
            ThrowIf.NullOrEmpty(userGuid, nameof(userGuid));
            ThrowIf.Null(transContext, nameof(transContext));

            IList<SignInToken> resultList = await _db.RetrieveAsync<SignInToken>(at => at.UserGuid == userGuid, transContext).ConfigureAwait(false);

            await _db.BatchDeleteAsync(resultList, transContext).ConfigureAwait(false);
        }

        public async Task DeleteAsync(string signInTokenGuid, TransactionContext transContext)
        {
            ThrowIf.NullOrEmpty(signInTokenGuid, nameof(signInTokenGuid));
            ThrowIf.Null(transContext, nameof(transContext));

            IList<SignInToken> resultList = await _db.RetrieveAsync<SignInToken>(at => at.Guid == signInTokenGuid, transContext).ConfigureAwait(false);

            await _db.BatchDeleteAsync(resultList, transContext).ConfigureAwait(false);
        }

        public async Task<SignInToken> GetAsync(string signInTokenGuid, string refreshToken, string deviceId, string userGuid, TransactionContext transContext = null)
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

        public async Task UpdateAsync(SignInToken signInToken, TransactionContext transContext = null)
        {
            ThrowIf.Null(signInToken, nameof(signInToken));

            await _db.UpdateAsync(signInToken, transContext).ConfigureAwait(false);
        }
    }
}
