using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entity;
using HB.Framework.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    internal class ClaimsPrincipalFactory : IClaimsPrincipalFactory
    {
        private readonly IRoleBiz _roleBiz;
        private readonly IUserClaimBiz _userClaimBiz;

        public ClaimsPrincipalFactory(IUserClaimBiz userClaims, IRoleBiz roleBiz)
        {
            _userClaimBiz = userClaims;
            _roleBiz = roleBiz;
        }

        /// <summary>
        /// 在Claims中放入UserGuid, SecurityStamp, UserClaim表中声明加入JWT的, 所有roleName 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        public Task<IEnumerable<Claim>> CreateClaimsAsync<TUserClaim, TRole, TUserRole>(User user, TransactionContext transContext) 
            where TUserClaim : UserClaim, new()
            where TRole : Role, new()
            where TUserRole : UserRole, new()
        {
            ThrowIf.Null(transContext, nameof(transContext));

            if (user == null) { return null; }

            IList<Claim> claims = new List<Claim>
            {
                new Claim(ClaimExtensionTypes.UserGuid, user.Guid),
                new Claim(ClaimExtensionTypes.SecurityStamp, user.SecurityStamp),
                //new Claim(ClaimExtensionTypes.UserId, user.Id.ToString(GlobalSettings.Culture)),
                //new Claim(ClaimExtensionTypes.UserName, user.UserName??""),
                //new Claim(ClaimExtensionTypes.MobilePhone, user.Mobile??""),
                //new Claim(ClaimExtensionTypes.IsMobileConfirmed, user.MobileConfirmed.ToString(GlobalSettings.Culture))
            };

            //并行

            return TaskUtil.Concurrence(_userClaimBiz.GetAsync<TUserClaim>(user.Guid, transContext), _roleBiz.GetByUserGuidAsync<TRole, TUserRole>(user.Guid, transContext),
                userClaims =>
                {
                    List<Claim> rts = new List<Claim>();
                    userClaims.ForEach((item) =>
                    {
                        if (item.AddToJwt)
                        {
                            rts.Add(new Claim(item.ClaimType, item.ClaimValue));
                        }
                    });
                    return rts;
                },
                roles =>
                {
                    List<Claim> rts = new List<Claim>();

                    roles.Select(r => r.Name).ForEach(roleName => rts.Add(new Claim(ClaimExtensionTypes.Role, roleName)));

                    return rts;
                });
        }
    }
}
