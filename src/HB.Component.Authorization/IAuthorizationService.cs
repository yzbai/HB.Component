using HB.Component.Authorization.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace HB.Component.Authorization
{
    public interface IAuthorizationService
    {
        Task<string> RefreshAccessTokenAsync(RefreshContext context);
        Task<SignInResult> SignInAsync(SignInContext context);
        Task SignOutAsync(string signInTokenGuid);
        JsonWebKeySet GetJsonWebKeySet();
    }
}