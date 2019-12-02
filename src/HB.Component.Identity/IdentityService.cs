using HB.Component.Identity.Abstractions;
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
        //private readonly ILogger _logger;
        private readonly IDatabase _database;
        private readonly IUserBiz _userBiz;
        private readonly IClaimsPrincipalFactory _claimsFactory;

        public IdentityService(IDatabase database, IUserBiz userBiz, IClaimsPrincipalFactory claimsFactory/*, ILogger<IdentityService> logger*/)
        {
            _userBiz = userBiz;
            _database = database;
            //this._logger = logger;
            _claimsFactory = claimsFactory;
        }

        public async Task<User> CreateUserByMobileAsync(string userType, string mobile, string userName, string password, bool mobileConfirmed)
        {
            TransactionContext transactionContext = await _database.BeginTransactionAsync<User>().ConfigureAwait(false);
            try
            {
                User user = await _userBiz.CreateByMobileAsync(userType, mobile, userName, password, mobileConfirmed, transactionContext).ConfigureAwait(false);

                await _database.CommitAsync(transactionContext).ConfigureAwait(false);

                return user;
            }
            catch(Exception ex)
            {
                await _database.RollbackAsync(transactionContext).ConfigureAwait(false);

                //TODO: 思考，这里需要记录吗，还是在调用者那里记录
                //_logger.LogException(ex);

                throw ex;
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

        public async Task SetAccessFailedCountAsync(string userGuid, long count)
        {
            TransactionContext transactionContext = await _database.BeginTransactionAsync<User>().ConfigureAwait(false);
            try
            {
                await _userBiz.SetAccessFailedCountAsync(userGuid, count, transactionContext).ConfigureAwait(false);

                await _database.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                await _database.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw ex;
            }
        }

        public async Task SetLockoutAsync(string userGuid, bool lockout, TimeSpan? lockoutTimeSpan = null)
        {
            TransactionContext transactionContext = await _database.BeginTransactionAsync<User>().ConfigureAwait(false);

            try
            {
                await _userBiz.SetLockoutAsync(userGuid, lockout, transactionContext, lockoutTimeSpan).ConfigureAwait(false);

                await _database.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _database.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw ex;
            }
        }

        public Task<User> ValidateSecurityStampAsync(string userGuid, string securityStamp)
        {
            return _userBiz.ValidateSecurityStampAsync(userGuid, securityStamp);
        }

        public async Task<IList<Claim>> GetUserClaimAsync(User user)
        {
            TransactionContext transactionContext = await _database.BeginTransactionAsync<User>().ConfigureAwait(false);
            try
            {
                IList<Claim> claims = await _claimsFactory.CreateClaimsAsync(user, transactionContext).ConfigureAwait(false);

                await _database.CommitAsync(transactionContext).ConfigureAwait(false);

                return claims;
            }
            catch (Exception ex)
            {
                await _database.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw ex;
            }
        }
    }
}
