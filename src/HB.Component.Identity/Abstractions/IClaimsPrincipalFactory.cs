using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using HB.Component.Identity.Entity;
using HB.Framework.Database;

namespace HB.Component.Identity.Abstractions
{
    internal interface IClaimsPrincipalFactory
    {
        Task<IList<Claim>> CreateClaimsAsync(User user, TransactionContext transContext);
    }
}