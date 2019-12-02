using HB.Component.Authorization.Abstractions;
using HB.Component.Authorization.Entity;
using HB.Component.Identity;
using HB.Component.Identity.Entity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HB.Component.Authorization
{
    internal class JwtBuilder : IJwtBuilder
    {
        private readonly SignInOptions _signInOptions;
        private readonly AuthorizationOptions _options;
        private readonly SigningCredentials _signingCredentials;

        private readonly IIdentityService _identityService;

        public JwtBuilder(IOptions<AuthorizationOptions> options, ICredentialBiz credentialBiz, IIdentityService identityService)
        {
            _options = options.Value;
            _signInOptions = _options.SignInOptions;
            _signingCredentials = credentialBiz.GetSigningCredentials();
            _identityService = identityService;
        }

        public async Task<string> BuildJwtAsync(User user, SignInToken signInToken, string audience)
        {
            DateTime utcNow = DateTime.UtcNow;

            IList<Claim> claims = await _identityService.GetUserClaimAsync(user).ConfigureAwait(false);

            claims.Add(new Claim(ClaimExtensionTypes.SignInTokenGuid, signInToken.Guid));

            //这个JWT只能在当前DeviceId上使用
            claims.Add(new Claim(ClaimExtensionTypes.DeviceId, signInToken.DeviceId));

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            JwtSecurityToken token = handler.CreateJwtSecurityToken(
                _options.OpenIdConnectConfiguration.Issuer,
                _options.NeedAudienceToBeChecked ? audience : null,
                new ClaimsIdentity(claims),
                utcNow,
                utcNow + _signInOptions.AccessTokenExpireTimeSpan,
                utcNow,
                _signingCredentials
                );

            return handler.WriteToken(token);
        }
    }
}
