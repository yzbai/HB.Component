using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace HB.Component.Authorization.Abstractions
{
    internal interface ICredentialBiz
    {
        SigningCredentials GetSigningCredentials();

        /// <exception cref="HB.Component.Authorization.AuthorizationException"></exception>
        IEnumerable<SecurityKey> GetIssuerSigningKeys();

        JsonWebKeySet GetJsonWebKeySet();

    }
}
