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

        public async Task<TUser> CreateUserByMobileAsync<TUser>(string mobile, string userName, string password, bool mobileConfirmed) where TUser : User, new()
        {
            TransactionContext transactionContext = await _database.BeginTransactionAsync<TUser>().ConfigureAwait(false);
            try
            {
                TUser user = await _userBiz.CreateByMobileAsync<TUser>(mobile, userName, password, mobileConfirmed, transactionContext).ConfigureAwait(false);

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

        public Task<TUser> GetUserByMobileAsync<TUser>(string mobile) where TUser : User, new()
        {
            return _userBiz.GetByMobileAsync<TUser>(mobile);
        }

        public Task<TUser> GetUserByUserNameAsync<TUser>(string userName) where TUser : User, new()
        {
            return _userBiz.GetByUserNameAsync<TUser>(userName);
        }

        public async Task SetAccessFailedCountAsync<TUser>(string userGuid, long count) where TUser : User, new()
        {
            TransactionContext transactionContext = await _database.BeginTransactionAsync<TUser>().ConfigureAwait(false);
            try
            {
                await _userBiz.SetAccessFailedCountAsync<TUser>(userGuid, count, transactionContext).ConfigureAwait(false);

                await _database.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                await _database.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw ex;
            }
        }

        public async Task SetLockoutAsync<TUser>(string userGuid, bool lockout, TimeSpan? lockoutTimeSpan = null) where TUser : User, new()
        {
            TransactionContext transactionContext = await _database.BeginTransactionAsync<TUser>().ConfigureAwait(false);

            try
            {
                await _userBiz.SetLockoutAsync<TUser>(userGuid, lockout, transactionContext, lockoutTimeSpan).ConfigureAwait(false);

                await _database.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _database.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw ex;
            }
        }

        public Task<TUser> ValidateSecurityStampAsync<TUser>(string userGuid, string securityStamp) where TUser : User, new()
        {
            return _userBiz.ValidateSecurityStampAsync<TUser>(userGuid, securityStamp);
        }

        public async Task<IEnumerable<Claim>> GetUserClaimAsync<TUserClaim, TRole, TRoleOfUser>(User user)
            where TUserClaim : UserClaim, new()
            where TRole : Role, new()
            where TRoleOfUser: RoleOfUser, new()
        {
            TransactionContext transactionContext = await _database.BeginTransactionAsync<TUserClaim>().ConfigureAwait(false);
            try
            {
                IEnumerable<Claim> claims = await _claimsFactory.CreateClaimsAsync<TUserClaim, TRole, TRoleOfUser>(user, transactionContext).ConfigureAwait(false);

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
