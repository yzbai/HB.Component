using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entity;
using HB.Framework.Database;
using System;
using System.Collections.Generic;
using System.Data;
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

        /// <summary>
        /// CreateUserByMobileAsync
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="loginName"></param>
        /// <param name="password"></param>
        /// <param name="mobileConfirmed"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public async Task<TUser> CreateUserByMobileAsync<TUser>(string mobile, string? loginName, string? password, bool mobileConfirmed) where TUser : User, new()
        {
            TransactionContext transactionContext = await _database.BeginTransactionAsync<TUser>(IsolationLevel.ReadCommitted).ConfigureAwait(false);
            try
            {
                TUser user = await _userBiz.CreateByMobileAsync<TUser>(mobile, loginName, password, mobileConfirmed, transactionContext).ConfigureAwait(false);

                await _database.CommitAsync(transactionContext).ConfigureAwait(false);

                return user;
            }
            catch
            {
                await _database.RollbackAsync(transactionContext).ConfigureAwait(false);

                //TODO: 思考，这里需要记录吗，还是在调用者那里记录
                //_logger.LogException(ex);

                throw;
            }
        }

        public Task<TUser?> GetUserByMobileAsync<TUser>(string mobile) where TUser : User, new()
        {
            return _userBiz.GetByMobileAsync<TUser>(mobile);
        }

        public Task<TUser?> GetUserByLoginNameAsync<TUser>(string loginName) where TUser : User, new()
        {
            return _userBiz.GetByLoginNameAsync<TUser>(loginName);
        }

        /// <summary>
        /// SetAccessFailedCountAsync
        /// </summary>
        /// <param name="userGuid"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public async Task SetAccessFailedCountAsync<TUser>(string userGuid, long count) where TUser : User, new()
        {
            TransactionContext transactionContext = await _database.BeginTransactionAsync<TUser>(IsolationLevel.ReadCommitted).ConfigureAwait(false);
            try
            {
                await _userBiz.SetAccessFailedCountAsync<TUser>(userGuid, count, transactionContext).ConfigureAwait(false);

                await _database.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await _database.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// SetLockoutAsync
        /// </summary>
        /// <param name="userGuid"></param>
        /// <param name="lockout"></param>
        /// <param name="lockoutTimeSpan"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public async Task SetLockoutAsync<TUser>(string userGuid, bool lockout, TimeSpan? lockoutTimeSpan = null) where TUser : User, new()
        {
            TransactionContext transactionContext = await _database.BeginTransactionAsync<TUser>(IsolationLevel.ReadCommitted).ConfigureAwait(false);

            try
            {
                await _userBiz.SetLockoutAsync<TUser>(userGuid, lockout, transactionContext, lockoutTimeSpan).ConfigureAwait(false);

                await _database.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await _database.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        public Task<TUser?> ValidateSecurityStampAsync<TUser>(string userGuid, string? securityStamp) where TUser : User, new()
        {
            return _userBiz.ValidateSecurityStampAsync<TUser>(userGuid, securityStamp);
        }

        /// <summary>
        /// GetUserClaimAsync
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public async Task<IEnumerable<Claim>> GetUserClaimAsync<TUserClaim, TRole, TRoleOfUser>(User user)
            where TUserClaim : UserClaim, new()
            where TRole : Role, new()
            where TRoleOfUser : RoleOfUser, new()
        {
            TransactionContext transactionContext = await _database.BeginTransactionAsync<TUserClaim>(IsolationLevel.ReadCommitted).ConfigureAwait(false);
            try
            {
                IEnumerable<Claim> claims = await _claimsFactory.CreateClaimsAsync<TUserClaim, TRole, TRoleOfUser>(user, transactionContext).ConfigureAwait(false);

                await _database.CommitAsync(transactionContext).ConfigureAwait(false);

                return claims;
            }
            catch
            {
                await _database.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }
    }
}
