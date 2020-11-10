using HB.Component.Identity.Entities;
using HB.Framework.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace HB.Component.Identity.Abstractions
{
    internal interface IUserClaimBiz
    {
        Task<IEnumerable<TUserClaim>> GetAsync<TUserClaim>(string userGuid, TransactionContext? transContext = null) where TUserClaim : IdentityUserClaim, new();
    }
}
