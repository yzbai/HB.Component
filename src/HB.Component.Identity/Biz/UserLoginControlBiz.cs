using HB.Component.Identity.Abstractions;
using HB.Framework.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Component.Identity.Biz
{
    internal class UserLoginControlBiz : IUserLoginControlBiz
    {
        public 

        public async Task SetLockoutAsync(string userGuid, bool lockout, string lastUser, TimeSpan? lockoutTimeSpan = null)  
        {
            TUser? user = await GetAsync<TUser>(userGuid, transContext).ConfigureAwait(false);

            if (user == null)
            {
                throw new IdentityException(ErrorCode.IdentityNotFound, $"userGuid:{userGuid}, lockout:{lockout}, lockoutTimeSpan:{lockoutTimeSpan?.TotalSeconds}");
            }

            user.LockoutEnabled = lockout;

            if (lockout)
            {
                user.LockoutEndDate = DateTimeOffset.UtcNow + (lockoutTimeSpan ?? TimeSpan.FromDays(1));
            }

            await _db.UpdateAsync(user, lastUser, transContext).ConfigureAwait(false);
        }

        public async Task SetAccessFailedCountAsync(string userGuid, long count, string lastUser)
        {
            TUser? user = await GetAsync<TUser>(userGuid, transContext).ConfigureAwait(false);

            if (user == null)
            {
                throw new IdentityException(ErrorCode.IdentityNotFound, $"userGuid:{userGuid}, count:{count}");
            }

            if (count != 0)
            {
                user.AccessFailedLastTime = DateTime.UtcNow;
            }

            user.AccessFailedCount = count;

            await _db.UpdateAsync(user, lastUser, transContext).ConfigureAwait(false);
        }
    }
}
