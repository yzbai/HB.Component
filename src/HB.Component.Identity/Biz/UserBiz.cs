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

        public Task<TUser> ValidateSecurityStampAsync<TUser>(string userGuid, string securityStamp, TransactionContext transContext = null) where TUser : User, new()
        {
            ThrowIf.NullOrEmpty(userGuid, nameof(userGuid));
            ThrowIf.NullOrEmpty(securityStamp, nameof(securityStamp));

            return _db.ScalarAsync<TUser>(u => u.Guid == userGuid && u.SecurityStamp == securityStamp, transContext);
        }

        public Task<TUser> GetAsync<TUser>(string userGuid, TransactionContext transContext = null) where TUser : User, new()
        {
            ThrowIf.NullOrEmpty(userGuid, nameof(userGuid));

            return _db.ScalarAsync<TUser>(u => u.Guid == userGuid, transContext);
        }

        public Task<TUser> GetByMobileAsync<TUser>(string mobile, TransactionContext transContext = null) where TUser : User, new()
        {
            ThrowIf.NullOrNotMobile(mobile, nameof(mobile));

            return _db.ScalarAsync<TUser>(u => u.Mobile == mobile, transContext);
        }

        public Task<TUser> GetByUserNameAsync<TUser>(string userName, TransactionContext transContext = null) where TUser : User, new()
        {
            ThrowIf.NullOrNotUserName(userName, nameof(userName));

            return _db.ScalarAsync<TUser>(u => u.UserName == userName, transContext);
        }

        public Task<TUser> GetByEmailAsync<TUser>(string email, TransactionContext transContext = null) where TUser : User, new()
        {
            ThrowIf.NullOrNotEmail(email, nameof(email));

            return _db.ScalarAsync<TUser>(u => u.Email.Equals(email, GlobalSettings.ComparisonIgnoreCase), transContext);
        }

        public Task<IEnumerable<TUser>> GetAsync<TUser>(IEnumerable<string> userGuids, TransactionContext transContext = null) where TUser : User, new()
        {
            ThrowIf.AnyNull(userGuids, nameof(userGuids));

            return _db.RetrieveAsync<TUser>(u => SQLUtil.In(u.Guid, true, userGuids.ToArray()), transContext);
        }

        #endregion

        #region Update

        public async Task SetLockoutAsync<TUser>(string userGuid, bool lockout, TransactionContext transContext, TimeSpan? lockoutTimeSpan = null) where TUser : User, new()
        {
            transContext.ThrowIfNull(nameof(transContext));

            TUser user = await GetAsync<TUser>(userGuid, transContext).ConfigureAwait(false);

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

        public async Task SetAccessFailedCountAsync<TUser>(string userGuid, long count, TransactionContext transContext) where TUser : User, new()
        {
            transContext.ThrowIfNull(nameof(transContext));

            TUser user = await GetAsync<TUser>(userGuid, transContext).ConfigureAwait(false);

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

        public async Task SetUserNameAsync<TUser>(string userGuid, string userName, TransactionContext transContext) where TUser : User, new()
        {
            ThrowIf.Null(transContext, nameof(transContext));
            ThrowIf.NullOrEmpty(userGuid, nameof(userGuid));
            ThrowIf.NullOrEmpty(userName, nameof(userName));

            TUser user = await GetAsync<TUser>(userGuid, transContext).ConfigureAwait(false);

            if (user == null)
            {
                throw new IdentityException(IdentityError.NotFound, $"userGuid:{userGuid}");
            }

            if (!user.UserName.Equals(userName, GlobalSettings.Comparison) && 0 != await _db.CountAsync<TUser>(u => u.UserName == userName, transContext).ConfigureAwait(false))
            {
                throw new IdentityException(IdentityError.AlreadyExists, $"userGuid:{userGuid}, userName:{userName}");
            }

            user.UserName = userName;

            await ChangeSecurityStampAsync(user).ConfigureAwait(false);

            await _db.UpdateAsync(user, transContext).ConfigureAwait(false);
        }

        public async Task SetPasswordByMobileAsync<TUser>(string mobile, string newPassword, TransactionContext transContext) where TUser : User, new()
        {
            ThrowIf.NullOrNotMobile(mobile, nameof(mobile));
            ThrowIf.NullOrNotPassword(mobile, nameof(newPassword));

            TUser user = await GetByMobileAsync<TUser>(mobile, transContext).ConfigureAwait(false);

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
            ThrowIf.Null(user, nameof(user));

            user.SecurityStamp = SecurityUtil.CreateUniqueToken();

            if (_identityOptions.Events != null)
            {
                IdentitySecurityStampChangeContext context = new IdentitySecurityStampChangeContext(user.Guid);
                return _identityOptions.Events.SecurityStampChanged(context);
            }

            return Task.CompletedTask;
        }

        #endregion

        #region Register

        private TUser InitNew<TUser>(string mobile, string userName, string password) where TUser : User, new()
        {
            TUser user = new TUser {
                //UserType = userType,
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

        public async Task<TUser> CreateByMobileAsync<TUser>(string mobile, string userName, string password, bool mobileConfirmed, TransactionContext transContext) where TUser : User, new()
        {
            ThrowIf.Null(transContext, nameof(transContext));
            ThrowIf.NullOrNotMobile(mobile, nameof(mobile));
            ThrowIf.NullOrNotPassword(password, nameof(password));

            #region Existense Check

            TUser user = await GetByMobileAsync<TUser>(mobile, transContext).ConfigureAwait(false);

            if (user != null)
            {
                throw new IdentityException(IdentityError.MobileAlreadyTaken, $"userType:{typeof(TUser)}, mobile:{mobile}, userName:{userName}");
            }

            if (!string.IsNullOrEmpty(userName))
            {
                TUser tmpUser = await GetByUserNameAsync<TUser>(userName, transContext).ConfigureAwait(false);

                if (tmpUser != null)
                {
                    throw new IdentityException(IdentityError.UserNameAlreadyTaken, $"userType:{typeof(TUser)}, mobile:{mobile}, userName:{userName}");
                }
            }

            #endregion

            user = InitNew<TUser>(mobile, userName, password);
            user.MobileConfirmed = mobileConfirmed;

            await _db.AddAsync(user, transContext).ConfigureAwait(false);

            return user;
        }

        #endregion
    }
}
