using HB.Framework.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Component.Identity.Abstractions
{
    internal interface IUserLoginControlBiz
    {
        Task SetAccessFailedCountAsync(string userGuid, long count, string lastUser);

        Task SetLockoutAsync(string userGuid, bool lockout, string lastUser,TimeSpan? lockoutTimeSpan = null);
    }
}
