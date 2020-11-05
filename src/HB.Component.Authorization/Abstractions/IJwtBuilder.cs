using System.Security;
using System.Threading.Tasks;
using HB.Component.Authorization.Entity;
using HB.Component.Identity;
using HB.Component.Identity.Entity;

namespace HB.Component.Authorization.Abstractions
{
    internal interface IJwtBuilder
    {
        Task<string> BuildJwtAsync<TUserClaim, TRole, TRoleOfUser>(IdenityUser user, SignInToken signInToken, string? audience)
            where TUserClaim : IdentityUserClaim, new()
            where TRole : IdentityRole, new()
            where TRoleOfUser : IdentityRoleOfUser, new();
        //string BuildJwt(User user, SignInToken signInToken, string audience);
    }
}
