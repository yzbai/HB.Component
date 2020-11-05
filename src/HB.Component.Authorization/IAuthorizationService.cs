using HB.Component.Authorization.Abstractions;
using HB.Component.Identity.Entity;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace HB.Component.Authorization
{
    public interface IAuthorizationService
    {
        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="HB.Component.Authorization.AuthorizationException"></exception>
        Task<string> RefreshAccessTokenAsync<TUser, TUserClaim, TRole, TRoleOfUser>(RefreshContext context)
            where TUser : IdenityUser, new()
            where TUserClaim : IdentityUserClaim, new()
            where TRole : IdentityRole, new()
            where TRoleOfUser : IdentityRoleOfUser, new();

        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="HB.Component.Authorization.AuthorizationException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task<SignInResult> SignInAsync<TUser, TUserClaim, TRole, TRoleOfUser>(SignInContext context)
            where TUser : IdenityUser, new()
            where TUserClaim : IdentityUserClaim, new()
            where TRole : IdentityRole, new()
            where TRoleOfUser : IdentityRoleOfUser, new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signInTokenGuid"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.Database.DatabaseException"></exception>
        Task SignOutAsync(string signInTokenGuid);

        /// <exception cref="FileNotFoundException">证书文件不存在</exception>
        /// <exception cref="ArgumentException">Json无法解析</exception>
        JsonWebKeySet GetJsonWebKeySet();

    }
}