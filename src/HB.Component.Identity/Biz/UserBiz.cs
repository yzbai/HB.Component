using HB.Framework.Common;
using HB.Framework.Common.Validate;
using HB.Framework.Database;
using HB.Framework.Database.SQL;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entity;
using System.Linq;

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

        public async Task<User> ValidateSecurityStampAsync(string userGuid, string securityStamp, TransactionContext transContext = null)
        {
            ThrowIf.NullOrEmpty(userGuid, nameof(userGuid));
            ThrowIf.NullOrEmpty(securityStamp, nameof(securityStamp));

            return await _db.ScalarAsync<User>(u => u.Guid == userGuid && u.SecurityStamp == securityStamp, transContext).ConfigureAwait(false);
        }

        public Task<User> GetAsync(string userGuid, TransactionContext transContext = null)
        {
            ThrowIf.NullOrEmpty(userGuid, nameof(userGuid));

            return _db.ScalarAsync<User>(u => u.Guid == userGuid, transContext);
        }

        public Task<User> GetByMobileAsync(string mobile, TransactionContext transContext = null)
        {
            ThrowIf.NullOrNotMobile(mobile, nameof(mobile));

            return _db.ScalarAsync<User>(u => u.Mobile == mobile, transContext);
        }

        public Task<User> GetByUserNameAsync(string userName, TransactionContext transContext = null)
        {
            ThrowIf.NullOrNotUserName(userName, nameof(userName));

            return _db.ScalarAsync<User>(u => u.UserName == userName, transContext);
        }

        public Task<User> GetByEmailAsync(string email, TransactionContext transContext = null)
        {
            ThrowIf.NullOrNotEmail(email, nameof(email));

            return _db.ScalarAsync<User>(u => u.Email.Equals(email, GlobalSettings.ComparisonIgnoreCase), transContext);
        }

        public Task<IEnumerable<User>> GetAsync(IEnumerable<string> userGuids, TransactionContext transContext = null)
        {
            ThrowIf.AnyNull(userGuids, nameof(userGuids));

            return _db.RetrieveAsync<User>(u => SQLUtil.In(u.Guid, true, userGuids.ToArray()), transContext);
        }

        #endregion

        #region Update

        public async Task SetLockoutAsync(string userGuid, bool lockout, TransactionContext transContext, TimeSpan? lockoutTimeSpan = null)
        {
            transContext.ThrowIfNull(nameof(transContext));

            User user = await GetAsync(userGuid, transContext).ConfigureAwait(false);

            if (user == null)
            {
                throw new IdentityException(IdentityError.NotFound, $"userGuid:{userGuid}, lockout:{lockout}, lockoutTimeSpan:{lockoutTimeSpan?.TotalSeconds}" );
            }

            user.LockoutEnabled = lockout;

            if (lockout)
            {
                user.LockoutEndDate = DateTimeOffset.UtcNow + (lockoutTimeSpan == null ? lockoutTimeSpan.Value : TimeSpan.FromDays(1));
            }

            await _db.UpdateAsync(user, transContext).ConfigureAwait(false);
        }

        public async Task SetAccessFailedCountAsync(string userGuid, long count, TransactionContext transContext)
        {
            transContext.ThrowIfNull(nameof(transContext));

            User user = await GetAsync(userGuid, transContext).ConfigureAwait(false);

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

        public async Task SetUserNameAsync(string userGuid, string userName, TransactionContext transContext)
        {
            ThrowIf.Null(transContext, nameof(transContext));
            ThrowIf.NullOrEmpty(userGuid, nameof(userGuid));
            ThrowIf.NullOrEmpty(userName, nameof(userName));

            User user = await GetAsync(userGuid, transContext).ConfigureAwait(false);

            if (user == null)
            {
                throw new IdentityException(IdentityError.NotFound, $"userGuid:{userGuid}");
            }

            if (!user.UserName.Equals(userName, GlobalSettings.Comparison) && 0 != await _db.CountAsync<User>(u => u.UserName == userName, transContext).ConfigureAwait(false))
            {
                throw new IdentityException(IdentityError.AlreadyExists, $"userGuid:{userGuid}, userName:{userName}");
            }

            user.UserName = userName;

            await ChangeSecurityStampAsync(user).ConfigureAwait(false);

            await _db.UpdateAsync(user, transContext).ConfigureAwait(false);
        }

        public async Task SetPasswordByMobileAsync(string mobile, string newPassword, TransactionContext transContext)
        {
            ThrowIf.NullOrNotMobile(mobile, nameof(mobile));
            ThrowIf.NullOrNotPassword(mobile, nameof(newPassword));

            User user = await GetByMobileAsync(mobile, transContext).ConfigureAwait(false);

            if (user == null)
            {
                throw new IdentityException(IdentityError.NotFound, $"mobile:{mobile}");
            }

            user.PasswordHash = SecurityUtil.EncryptPwdWithSalt(newPassword, user.Guid);

            await ChangeSecurityStampAsync(user).ConfigureAwait(false);

            await _db.UpdateAsync(user, transContext).ConfigureAwait(false);
        }

        private async Task ChangeSecurityStampAsync(User user)
        {
            ThrowIf.Null(user, nameof(user));

            user.SecurityStamp = SecurityUtil.CreateUniqueToken();

            if (_identityOptions.Events != null)
            {
                IdentitySecurityStampChangeContext context = new IdentitySecurityStampChangeContext(user.Guid);
                await _identityOptions.Events.SecurityStampChanged(context).ConfigureAwait(false);
            }
        }

        #endregion

        #region Register

        private User InitNew(string userType, string mobile, string userName, string password)
        {
            User user = new User {
                UserType = userType,
                Mobile = mobile,
                Guid = SecurityUtil.CreateUniqueToken(),
                SecurityStamp = SecurityUtil.CreateUniqueToken(),
                IsActivated = true,
                AccessFailedCount = 0,
                UserName = userName,
                TwoFactorEnabled = false,
                //ImageUrl = string.Empty
            };

            user.PasswordHash = SecurityUtil.EncryptPwdWithSalt(password, user.Guid);

            return user;
        }

        public async Task<User> CreateByMobileAsync(string userType, string mobile, string userName, string password, bool mobileConfirmed, TransactionContext transContext)
        {
            ThrowIf.Null(transContext, nameof(transContext));
            ThrowIf.NullOrEmpty(userType, nameof(transContext));
            ThrowIf.NullOrNotMobile(mobile, nameof(mobile));
            ThrowIf.NullOrNotPassword(password, nameof(password));

            #region Existense Check

            User user = await GetByMobileAsync(mobile, transContext).ConfigureAwait(false);

            if (user != null)
            {
                throw new IdentityException(IdentityError.MobileAlreadyTaken, $"userType:{userType}, mobile:{mobile}, userName:{userName}");
            }

            if (!string.IsNullOrEmpty(userName))
            {
                User tmpUser = await GetByUserNameAsync(userName, transContext).ConfigureAwait(false);

                if (tmpUser != null)
                {
                    throw new IdentityException(IdentityError.UserNameAlreadyTaken, $"userType:{userType}, mobile:{mobile}, userName:{userName}");
                }
            }

            #endregion

            user = InitNew(userType, mobile, userName, password);
            user.MobileConfirmed = mobileConfirmed;

            await _db.AddAsync(user, transContext).ConfigureAwait(false);

            return user;
        }

        #endregion
    }
}
