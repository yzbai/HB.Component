﻿using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entity;
using HB.Framework.Database;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

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

        public async Task<IList<Claim>> CreateClaimsAsync(User user, TransactionContext transContext)
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

            IList<UserClaim> userClaims = await _userClaimBiz.GetAsync(user.Guid, transContext).ConfigureAwait(false);

            foreach (UserClaim item in userClaims)
            {
                if (item.AddToJwt)
                {
                    claims.Add(new Claim(item.ClaimType, item.ClaimValue));
                }
            }

            IList<Role> roles = await _roleBiz.GetByUserGuidAsync(user.Guid, transContext).ConfigureAwait(false);

            foreach (string roleName in roles.Select(r=>r.Name))
            {
                claims.Add(new Claim(ClaimExtensionTypes.Role, roleName));
            }

            return claims;
        }
    }
}
