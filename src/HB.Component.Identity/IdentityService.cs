﻿using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entity;
using HB.Framework.Database;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    internal class IdentityService : IIdentityService
    {
        private readonly IUserBiz _userBiz;
        private readonly IClaimsPrincipalFactory claimsFactory;
        private readonly IDatabase database;
        private readonly ILogger logger;

        public IdentityService(IDatabase database, ILogger<IdentityService> logger, IUserBiz userBiz, IClaimsPrincipalFactory claimsFactory)
        {
            _userBiz = userBiz;
            this.database = database;
            this.logger = logger;
            this.claimsFactory = claimsFactory;
        }

        public async Task<IdentityResult> CreateUserByMobileAsync(string userType, string mobile, string userName, string password, bool mobileConfirmed)
        {
            TransactionContext transactionContext = await database.BeginTransactionAsync<User>().ConfigureAwait(false);
            try
            {
                IdentityResult result = await _userBiz.CreateByMobileAsync(userType, mobile, userName, password, mobileConfirmed, transactionContext).ConfigureAwait(false);

                if (!result.IsSucceeded())
                {
                    await database.RollbackAsync(transactionContext).ConfigureAwait(false);
                    return result;
                }

                await database.CommitAsync(transactionContext).ConfigureAwait(false);

                return result;
            }
            catch(Exception ex)
            {
                await database.RollbackAsync(transactionContext).ConfigureAwait(false);
                logger.LogCritical(ex, $"UserType :{userType}, Mobile:{mobile}");
                return IdentityResult.Throwed();
            }
        }

        public Task<User> GetUserByMobileAsync(string mobile)
        {
            return _userBiz.GetByMobileAsync(mobile);
        }

        public Task<User> GetUserByUserNameAsync(string userName)
        {
            return _userBiz.GetByUserNameAsync(userName);
        }

        public async Task<IdentityResult> SetAccessFailedCountAsync(string userGuid, long count)
        {
            TransactionContext transactionContext = await database.BeginTransactionAsync<User>().ConfigureAwait(false);
            try
            {
                IdentityResult result = await _userBiz.SetAccessFailedCountAsync(userGuid, count, transactionContext).ConfigureAwait(false);

                if (!result.IsSucceeded())
                {
                    await database.RollbackAsync(transactionContext).ConfigureAwait(false);
                    return result;
                }

                await database.CommitAsync(transactionContext).ConfigureAwait(false);

                return result;
            }
            catch(Exception ex)
            {
                await database.RollbackAsync(transactionContext).ConfigureAwait(false);
                logger.LogCritical(ex, $"UserGuid:{userGuid}, Count:{count}");
                return IdentityResult.Throwed();
            }
        }

        public async Task<IdentityResult> SetLockoutAsync(string userGuid, bool lockout, TimeSpan? lockoutTimeSpan = null)
        {
            TransactionContext transactionContext = await database.BeginTransactionAsync<User>().ConfigureAwait(false);

            try
            {
                IdentityResult result = await _userBiz.SetLockoutAsync(userGuid, lockout, transactionContext, lockoutTimeSpan).ConfigureAwait(false);

                if (!result.IsSucceeded())
                {
                    await database.RollbackAsync(transactionContext).ConfigureAwait(false);
                    return result;
                }

                await database.CommitAsync(transactionContext).ConfigureAwait(false);

                return result;
            }
            catch (Exception ex)
            {
                await database.RollbackAsync(transactionContext).ConfigureAwait(false);
                logger.LogCritical(ex, $"UserGuid:{userGuid}");
                return IdentityResult.Throwed();
            }
        }

        public Task<User> ValidateSecurityStampAsync(string userGuid, string securityStamp)
        {
            return _userBiz.ValidateSecurityStampAsync(userGuid, securityStamp);
        }

        public async Task<IList<Claim>> GetUserClaimAsync(User user)
        {
            TransactionContext transactionContext = await database.BeginTransactionAsync<User>().ConfigureAwait(false);
            try
            {
                IList<Claim> claims = await claimsFactory.CreateClaimsAsync(user, transactionContext).ConfigureAwait(false);

                await database.CommitAsync(transactionContext).ConfigureAwait(false);

                return claims;
            }
            catch (Exception ex)
            {
                await database.RollbackAsync(transactionContext).ConfigureAwait(false);
                logger.LogCritical(ex, $"User :{JsonUtil.ToJson(user)}");
                return new List<Claim>();
            }
        }
    }
}
