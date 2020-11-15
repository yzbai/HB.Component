using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using HB.Component.Identity.Entities;
using HB.Framework.Database;

namespace HB.Component.Identity.Abstractions
{
    internal interface IClaimsPrincipalFactory
    {
        Task<IEnumerable<Claim>> CreateClaimsAsync<TUserClaim, TRole, TRoleOfUser>(IdentityUser user, TransactionContext transContext)
            where TUserClaim : IdentityUserClaim, new()
            where TRole : IdentityRole, new()
            where TRoleOfUser : IdentityRoleOfUser, new();
    }
}