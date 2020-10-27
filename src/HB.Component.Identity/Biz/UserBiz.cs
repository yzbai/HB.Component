using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entity;
using HB.Framework.Common;
using HB.Framework.Database;
using HB.Framework.Database.SQL;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    /// <summary>
    /// 重要改变（比如Password）后，一定要清空对应UserId的Authtoken
    /// </summary>
    internal class UserBiz : IUserBiz
    {
        private readonly IDatabase _db;
        //private readonly ILogger _logger;
        private readonly IdentityOptions _identityOptions;

        public UserBiz(IOptions<IdentityOptions> identityOptions, IDatabase database/*, ILogger<UserBiz> logger*/)
        {
            _identityOptions = identityOptions.Value;
            _db = database;
        }

        #region Retrieve

        public async Task<TUser?> ValidateSecurityStampAsync<TUser>(string userGuid, string? securityStamp, TransactionContext? transContext = null) where TUser : User, new()
        {
            if (securityStamp.IsNullOrEmpty())
            {
                return null;
            }
            return await _db.ScalarAsync<TUser>(u => u.Guid == userGuid && u.SecurityStamp == securityStamp, transContext).ConfigureAwait(false);
        }

        public Task<TUser?> GetAsync<TUser>(string userGuid, TransactionContext? transContext = null) where TUser : User, new()
        {
            return _db.ScalarAsync<TUser>(u => u.Guid == userGuid, transContext);
        }

        public Task<TUser?> GetByMobileAsync<TUser>(string mobile, TransactionContext? transContext = null) where TUser : User, new()
        {
            return _db.ScalarAsync<TUser>(u => u.Mobile == mobile, transContext);
        }

        public Task<TUser?> GetByLoginNameAsync<TUser>(string loginName, TransactionContext? transContext = null) where TUser : User, new()
        {
            return _db.ScalarAsync<TUser>(u => u.LoginName == loginName, transContext);
        }

        public Task<TUser?> GetByEmailAsync<TUser>(string email, TransactionContext? transContext = null) where TUser : User, new()
        {
            return _db.ScalarAsync<TUser>(u => u.Email == email, transContext);
        }

        public Task<IEnumerable<TUser>> GetAsync<TUser>(IEnumerable<string> userGuids, TransactionContext? transContext = null) where TUser : User, new()
        {
            return _db.RetrieveAsync<TUser>(u => SQLUtil.In(u.Guid, true, userGuids.ToArray()), transContext);
        }

        #endregion

        #region Update

        /// <summary>
        /// SetLockoutAsync
        /// </summary>
        /// <param name="userGuid"></param>
        /// <param name="lockout"></param>
        /// <param name="transContext"></param>
        /// <param name="lockoutTimeSpan"></param>
        /// <returns></returns>
        /// <exception cref="HB.Component.Identity.IdentityException"></exception>
        /// <exception cref="ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task SetLockoutAsync<TUser>(string userGuid, bool lockout, TransactionContext transContext, TimeSpan? lockoutTimeSpan = null) where TUser : User, new()
        {
            TUser? user = await GetAsync<TUser>(userGuid, transContext).ConfigureAwait(false);

            if (user == null)
            {
                throw new IdentityException(IdentityError.NotFound, $"userGuid:{userGuid}, lockout:{lockout}, lockoutTimeSpan:{lockoutTimeSpan?.TotalSeconds}");
            }

            user.LockoutEnabled = lockout;

            if (lockout)
            {
                user.LockoutEndDate = DateTimeOffset.UtcNow + (lockoutTimeSpan ?? TimeSpan.FromDays(1));
            }

            await _db.UpdateAsync(user, transContext).ConfigureAwait(false);
        }

        /// <summary>
        /// SetAccessFailedCountAsync
        /// </summary>
        /// <param name="userGuid"></param>
        /// <param name="count"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="HB.Component.Identity.IdentityException"></exception>
        /// <exception cref="ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task SetAccessFailedCountAsync<TUser>(string userGuid, long count, TransactionContext transContext) where TUser : User, new()
        {
            TUser? user = await GetAsync<TUser>(userGuid, transContext).ConfigureAwait(false);

            if (user == null)
            {
                throw new IdentityException(IdentityError.NotFound, $"userGuid:{userGuid}, count:{count}");
            }

            if (count != 0)
            {
                user.AccessFailedLastTime = DateTime.UtcNow;
            }

            user.AccessFailedCount = count;

            await _db.UpdateAsync(user, transContext).ConfigureAwait(false);
        }

        /// <summary>
        /// SetLoginNameAsync
        /// </summary>
        /// <param name="userGuid"></param>
        /// <param name="loginName"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="HB.Component.Identity.IdentityException"></exception>
        /// <exception cref="ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task SetLoginNameAsync<TUser>(string userGuid, string loginName, TransactionContext transContext) where TUser : User, new()
        {
            ThrowIf.NullOrNotLoginName(loginName, nameof(loginName));

            TUser? user = await GetAsync<TUser>(userGuid, transContext).ConfigureAwait(false);

            if (user == null)
            {
                throw new IdentityException(IdentityError.NotFound, $"userGuid:{userGuid}");
            }

            if (!loginName.Equals(user.LoginName, GlobalSettings.Comparison) && 0 != await _db.CountAsync<TUser>(u => u.LoginName == loginName, transContext).ConfigureAwait(false))
            {
                throw new IdentityException(IdentityError.AlreadyExists, $"userGuid:{userGuid}, loginName:{loginName}");
            }

            user.LoginName = loginName;

            await ChangeSecurityStampAsync(user).ConfigureAwait(false);

            await _db.UpdateAsync(user, transContext).ConfigureAwait(false);
        }

        /// <summary>
        /// SetPasswordByMobileAsync
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="newPassword"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="HB.Component.Identity.IdentityException"></exception>
        /// <exception cref="ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task SetPasswordByMobileAsync<TUser>(string mobile, string newPassword, TransactionContext transContext) where TUser : User, new()
        {
            ThrowIf.NullOrNotMobile(mobile, nameof(mobile));
            ThrowIf.NotPassword(mobile, nameof(newPassword), false);

            TUser? user = await GetByMobileAsync<TUser>(mobile, transContext).ConfigureAwait(false);

            if (user == null)
            {
                throw new IdentityException(IdentityError.NotFound, $"mobile:{mobile}");
            }

            user.PasswordHash = SecurityUtil.EncryptPwdWithSalt(newPassword, user.Guid);

            await ChangeSecurityStampAsync(user).ConfigureAwait(false);

            await _db.UpdateAsync(user, transContext).ConfigureAwait(false);
        }

        private Task ChangeSecurityStampAsync(User user)
        {
            user.SecurityStamp = SecurityUtil.CreateUniqueToken();

            if (_identityOptions.Events != null)
            {
                IdentitySecurityStampChangeContext context = new IdentitySecurityStampChangeContext(user.Guid);
                return _identityOptions.Events.SecurityStampChangedAsync(context);
            }

            return Task.CompletedTask;
        }

        #endregion

        #region Register

        /// <summary>
        /// InitNew
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="loginName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private static TUser InitNew<TUser>(string mobile, string? loginName, string? password) where TUser : User, new()
        {
            TUser user = new TUser
            {
                //UserType = userType,
                Mobile = mobile,
                Guid = SecurityUtil.CreateUniqueToken(),
                SecurityStamp = SecurityUtil.CreateUniqueToken(),
                IsActivated = true,
                AccessFailedCount = 0,
                LoginName = loginName,
                TwoFactorEnabled = false,
                //ImageUrl = string.Empty,
            };

            if (password != null)
            {
                user.PasswordHash = SecurityUtil.EncryptPwdWithSalt(password, user.Guid);
            }

            return user;
        }

        /// <summary>
        /// CreateByMobileAsync
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="loginName"></param>
        /// <param name="password"></param>
        /// <param name="mobileConfirmed"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="HB.Component.Identity.IdentityException"></exception>
        /// <exception cref="ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task<TUser> CreateByMobileAsync<TUser>(string mobile, string? loginName, string? password, bool mobileConfirmed, TransactionContext transContext) where TUser : User, new()
        {
            ThrowIf.NullOrNotMobile(mobile, nameof(mobile));
            ThrowIf.NotPassword(password, nameof(password), true);

            #region Existense Check

            TUser? user = await GetByMobileAsync<TUser>(mobile, transContext).ConfigureAwait(false);

            if (user != null)
            {
                throw new IdentityException(IdentityError.MobileAlreadyTaken, $"userType:{typeof(TUser)}, mobile:{mobile}");
            }

            if (!string.IsNullOrEmpty(loginName))
            {
                TUser? tmpUser = await GetByLoginNameAsync<TUser>(loginName, transContext).ConfigureAwait(false);

                if (tmpUser != null)
                {
                    throw new IdentityException(IdentityError.LoginNameAlreadyTaken, $"userType:{typeof(TUser)}, mobile:{mobile}, loginName:{loginName}");
                }
            }

            #endregion

            user = InitNew<TUser>(mobile, loginName, password);

            user.MobileConfirmed = mobileConfirmed;

            await _db.AddAsync(user, transContext).ConfigureAwait(false);

            return user;
        }

        #endregion
    }
}
