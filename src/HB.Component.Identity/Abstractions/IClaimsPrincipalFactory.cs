using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using HB.Component.Identity.Entity;
using HB.Framework.Database;

namespace HB.Component.Identity.Abstractions
{
    internal interface IClaimsPrincipalFactory
    {
        Task<IEnumerable<Claim>> CreateClaimsAsync<TUserClaim, TRole, TRoleOfUser>(User user, TransactionContext transContext)
            where TUserClaim : UserClaim, new()
            where TRole : Role, new()
            where TRoleOfUser : RoleOfUser, new();
    }
}