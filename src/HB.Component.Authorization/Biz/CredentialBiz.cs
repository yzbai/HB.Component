using HB.Component.Authorization.Abstractions;
using HB.Component.Authorization.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace HB.Component.Authorization
{
    internal class CredentialBiz : ICredentialBiz
    {
        private readonly AuthorizationOptions _options;
        private readonly SigningCredentials _signingCredentials;
        private readonly JsonWebKeySet _jsonWebKeySet;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        /// <exception cref="FileNotFoundException">证书文件不存在</exception>
        /// <exception cref="ArgumentException">Json无法解析</exception>
        public CredentialBiz(IOptions<AuthorizationOptions> options)
        {
            _options = options.Value;

            X509Certificate2? cert = CertificateUtil.GetBySubject(_options.CertificateSubject);

            if (cert == null)
            {
                throw new FileNotFoundException(_options.CertificateSubject);
            }

            X509SecurityKey securityKey = new X509SecurityKey(cert);

            _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256Signature);

            RSA publicKey = (RSA)securityKey.PublicKey;
            RSAParameters parameters = publicKey.ExportParameters(false);

            IList<JsonWebKey> jsonWebKeys = new List<JsonWebKey> {
                new JsonWebKey {
                    Kty = "RSA",
                    Use = "sig",
                    Kid = securityKey.KeyId,
                    E = Base64UrlEncoder.Encode(parameters.Exponent),
                    N = Base64UrlEncoder.Encode(parameters.Modulus)
                }
            };

            string jsonWebKeySetString = SerializeUtil.ToJson(new { Keys = jsonWebKeys });

            _jsonWebKeySet = new JsonWebKeySet(jsonWebKeySetString);
        }

        /// <summary>
        /// 公钥
        /// </summary>
        /// <returns></returns>
        public JsonWebKeySet GetJsonWebKeySet()
        {
            return _jsonWebKeySet;
        }

        /// <summary>
        /// GetIssuerSigningKeys
        /// </summary>
        /// <returns></returns>
        /// <exception cref="HB.Component.Authorization.AuthorizationException"></exception>
        public IEnumerable<SecurityKey> GetIssuerSigningKeys()
        {
            if (_jsonWebKeySet == null)
            {
                throw new AuthorizationException(Resources.JsonWebKeySetIsNullErrorMessage);
            }

            return _jsonWebKeySet.GetSigningKeys();
        }

        /// <summary>
        /// 私钥
        /// </summary>
        /// <returns></returns>
        public SigningCredentials GetSigningCredentials()
        {
            return _signingCredentials;
        }
    }
}
