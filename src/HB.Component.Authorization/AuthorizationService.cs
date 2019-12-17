using HB.Component.Authorization.Abstractions;
using HB.Component.Authorization.Entity;
using HB.Component.Identity;
using HB.Component.Identity.Entity;
using HB.Framework.Database;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HB.Component.Authorization
{
    internal class AuthorizationService : IAuthorizationService
    {
        private readonly IDatabase _database;
        private readonly AuthorizationOptions _options;
        private readonly SignInOptions _signInOptions;
        private readonly IIdentityService _identityService;
        private readonly ISignInTokenBiz _signInTokenBiz;
        private readonly IJwtBuilder _jwtBuilder;
        private readonly ICredentialBiz _credentialBiz;
        private readonly DistributedCacheFrequencyChecker _frequencyChecker;

        //private readonly ILogger logger;

        public AuthorizationService(IDatabase database, IOptions<AuthorizationOptions> options, IDistributedCache distributedCache,
            ISignInTokenBiz signInTokenBiz, IIdentityService identityManager, IJwtBuilder jwtBuilder, ICredentialBiz credentialManager/*, ILogger<AuthorizationService> logger*/)
        {
            _database = database;
            _options = options.Value;
            _signInOptions = _options.SignInOptions;

            //this.logger = logger;
            _frequencyChecker = new DistributedCacheFrequencyChecker(distributedCache);

            _signInTokenBiz = signInTokenBiz;
            _identityService = identityManager;
            _jwtBuilder = jwtBuilder;
            _credentialBiz = credentialManager;

        }

        public JsonWebKeySet GetJsonWebKeySet()
        {
            return _credentialBiz.GetJsonWebKeySet();
        }

        public async Task SignOutAsync(string signInTokenGuid)
        {
            TransactionContext transactionContext = await _database.BeginTransactionAsync<SignInToken>().ConfigureAwait(false);
            try
            {
                await _signInTokenBiz.DeleteAsync(signInTokenGuid, transactionContext).ConfigureAwait(false);

                await _database.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await _database.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        public async Task<SignInResult> SignInAsync<TUser, TUserClaim, TRole, TRoleOfUser>(SignInContext context)
            where TUser : User, new()
            where TUserClaim : UserClaim, new()
            where TRole : Role, new()
            where TRoleOfUser : RoleOfUser, new()
        {
            ThrowIf.NullOrNotValid(context, nameof(context));

            TransactionContext transactionContext = await _database.BeginTransactionAsync<SignInToken>().ConfigureAwait(false);

            try
            {
                //查询用户
                TUser user = context.SignInType switch
                {
                    SignInType.ByUserNameAndPassword => await _identityService.GetUserByUserNameAsync<TUser>(context.UserName).ConfigureAwait(false),
                    SignInType.BySms => await _identityService.GetUserByMobileAsync<TUser>(context.Mobile).ConfigureAwait(false),
                    SignInType.ByMobileAndPassword => await _identityService.GetUserByMobileAsync<TUser>(context.Mobile).ConfigureAwait(false),
                    _ => null
                };

                //不存在，则新建用户
                bool newUserCreated = false;

                if (user == null && context.SignInType == SignInType.BySms)
                {
                    user = await _identityService.CreateUserByMobileAsync<TUser>(context.Mobile, context.UserName, context.Password, true).ConfigureAwait(false);

                    newUserCreated = true;
                }

                if (user == null)
                {
                    throw new AuthorizationException(AuthorizationError.NoSuchUser, $"SignInContext:{SerializeUtil.ToJson(context)}");
                }

                //密码检查
                if (context.SignInType == SignInType.ByMobileAndPassword || context.SignInType == SignInType.ByUserNameAndPassword)
                {
                    if (!PassowrdCheck(user, context.Password))
                    {
                        await OnPasswordCheckFailedAsync(user).ConfigureAwait(false);

                        throw new AuthorizationException(AuthorizationError.PasswordWrong, $"SignInContext:{SerializeUtil.ToJson(context)}");
                    }
                }

                //其他检查
                await PreSignInCheckAsync(user).ConfigureAwait(false);

                //注销其他客户端
                DeviceType clientType = DeviceTypeChecker.Check(context.DeviceType);

                if (clientType != DeviceType.Web && _signInOptions.AllowOnlyOneAppClient)
                {
                    await _signInTokenBiz.DeleteAppClientTokenByUserGuidAsync(user.Guid, transactionContext).ConfigureAwait(false);
                }

                //创建Token
                SignInToken userToken = await _signInTokenBiz.CreateAsync(
                    user.Guid,
                    context.DeviceId,
                    clientType.ToString(),
                    context.DeviceVersion,
                    context.DeviceAddress,
                    context.DeviceIp,
                    context.RememberMe ? _signInOptions.RefreshTokenLongExpireTimeSpan : _signInOptions.RefreshTokenShortExpireTimeSpan,
                    transactionContext).ConfigureAwait(false);

                await _database.CommitAsync(transactionContext).ConfigureAwait(false);

                //构造 Jwt
                SignInResult result = new SignInResult
                {
                    AccessToken = await _jwtBuilder.BuildJwtAsync<TUserClaim, TRole, TRoleOfUser>(user, userToken, context.SignToWhere).ConfigureAwait(false),
                    RefreshToken = userToken.RefreshToken,
                    NewUserCreated = newUserCreated,
                    CurrentUser = user
                };

                return result;

            }
            catch
            {
                await _database.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        //TODO: 做好详细的历史纪录，各个阶段都要打log。一有风吹草动，就立马删除SignInToken
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns>新的AccessToken</returns>
        public async Task<string> RefreshAccessTokenAsync<TUser, TUserClaim, TRole, TRoleOfUser>(RefreshContext context)
            where TUser : User, new()
            where TUserClaim : UserClaim, new()
            where TRole : Role, new()
            where TRoleOfUser : RoleOfUser, new()
        {
            ThrowIf.NullOrNotValid(context, nameof(context));

            //频率检查

            //解决并发涌入

            if (!(await _frequencyChecker.CheckAsync(nameof(RefreshAccessTokenAsync), context.DeviceId, _options.RefreshIntervalTimeSpan).ConfigureAwait(false)))
            {
                throw new AuthorizationException(AuthorizationError.TooFrequent, $"Context:{SerializeUtil.ToJson(context)}");
            }

            //AccessToken, Claims 验证

            ClaimsPrincipal claimsPrincipal = null;
            try
            {
                claimsPrincipal = ValidateTokenWithoutLifeCheck(context);
            }
            catch(Exception ex)
            {
                throw new AuthorizationException(AuthorizationError.InvalideAccessToken, $"Context: {SerializeUtil.ToJson(context)}", ex);
            }

            //TODO: 这里缺DeviceId验证

            if (claimsPrincipal == null)
            {
                //TODO: Black concern SigninToken by RefreshToken
                throw new AuthorizationException(AuthorizationError.InvalideAccessToken, $"Context: {SerializeUtil.ToJson(context)}");
            }

            if (claimsPrincipal.GetDeviceId() != context.DeviceId)
            {
                throw new AuthorizationException(AuthorizationError.InvalideDeviceId, $"Context: {SerializeUtil.ToJson(context)}");
            }

            string userGuid = claimsPrincipal.GetUserGuid();

            if (string.IsNullOrEmpty(userGuid))
            {
                throw new AuthorizationException(AuthorizationError.InvalideUserGuid, $"Context: {SerializeUtil.ToJson(context)}");
            }


            //SignInToken 验证
            TUser user;
            SignInToken signInToken;
            TransactionContext transactionContext = await _database.BeginTransactionAsync<SignInToken>().ConfigureAwait(false);

            try
            {
                signInToken = await _signInTokenBiz.GetAsync(
                    claimsPrincipal.GetSignInTokenGuid(),
                    context.RefreshToken,
                    context.DeviceId,
                    userGuid,
                    transactionContext
                    ).ConfigureAwait(false);

                if (signInToken == null || signInToken.Blacked)
                {
                    //await _database.RollbackAsync(transactionContext).ConfigureAwait(false);

                    throw new AuthorizationException(AuthorizationError.NoTokenInStore, $"Refresh token error. signInToken not saved in db. Context : {SerializeUtil.ToJson(context)}");
                }

                // User 信息变动验证

                user = await _identityService.ValidateSecurityStampAsync<TUser>(userGuid, claimsPrincipal.GetUserSecurityStamp()).ConfigureAwait(false);

                if (user == null)
                {
                    await _database.RollbackAsync(transactionContext).ConfigureAwait(false);

                    await BlackSignInTokenAsync(signInToken).ConfigureAwait(false);

                    throw new AuthorizationException(AuthorizationError.UserSecurityStampChanged, $"Refresh token error. User SecurityStamp Changed. Context : {SerializeUtil.ToJson(context)}");
                }

                // 更新SignInToken
                signInToken.RefreshCount++;

                await _signInTokenBiz.UpdateAsync(signInToken, transactionContext).ConfigureAwait(false);

                await _database.CommitAsync(transactionContext).ConfigureAwait(false);

            }
            catch
            {
                await _database.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }

            // 发布新的AccessToken

            return await _jwtBuilder.BuildJwtAsync<TUserClaim, TRole, TRoleOfUser>(user, signInToken, claimsPrincipal.GetAudience()).ConfigureAwait(false);
        }

        private Task PreSignInCheckAsync<TUser>(TUser user) where TUser : User, new()
        {
            ThrowIf.Null(user, nameof(user));

            //2, 手机验证
            if (_signInOptions.RequireMobileConfirmed && !user.MobileConfirmed)
            {
                throw new AuthorizationException(AuthorizationError.MobileNotConfirmed, $"user:{SerializeUtil.ToJson(user)}");
            }

            //3, 邮件验证
            if (_signInOptions.RequireEmailConfirmed && !user.EmailConfirmed)
            {
                throw new AuthorizationException(AuthorizationError.EmailNotConfirmed, $"user:{SerializeUtil.ToJson(user)}");
            }

            //4, Lockout 检查
            if (_signInOptions.RequiredLockoutCheck && user.LockoutEnabled && user.LockoutEndDate > DateTimeOffset.UtcNow)
            {
                throw new AuthorizationException(AuthorizationError.LockedOut, $"user:{SerializeUtil.ToJson(user)}");
            }

            //5, 一天内,最大失败数检测
            if (_signInOptions.RequiredMaxFailedCountCheck)
            {
                if (DateTimeOffset.UtcNow - user.AccessFailedLastTime < TimeSpan.FromDays(_signInOptions.AccessFailedRecoveryDays))
                {
                    if (user.AccessFailedCount > _signInOptions.MaxFailedCount)
                    {
                        throw new AuthorizationException(AuthorizationError.OverMaxFailedCount, $"user:{SerializeUtil.ToJson(user)}");
                    }
                }
            }
            Task setLockTask = _signInOptions.RequiredLockoutCheck ? _identityService.SetLockoutAsync<TUser>(user.Guid, false) : Task.CompletedTask;
            Task setAccessFailedCountTask = _signInOptions.RequiredMaxFailedCountCheck ? _identityService.SetAccessFailedCountAsync<TUser>(user.Guid, 0) : Task.CompletedTask;

            if (_signInOptions.RequireTwoFactorCheck && user.TwoFactorEnabled)
            {
                //TODO: 后续加上twofactor验证. 即登录后,再验证手机或者邮箱
            }

            return Task.WhenAll(setLockTask, setAccessFailedCountTask);
        }

        private static bool PassowrdCheck(User user, string password)
        {
            string passwordHash = SecurityUtil.EncryptPwdWithSalt(password, user.Guid);
            return passwordHash.Equals(user.PasswordHash, GlobalSettings.Comparison);
        }

        private Task OnPasswordCheckFailedAsync<TUser>(TUser user) where TUser : User, new()
        {
            Task setAccessFailedCountTask = Task.CompletedTask;

            if (_signInOptions.RequiredMaxFailedCountCheck)
            {
                setAccessFailedCountTask = _identityService.SetAccessFailedCountAsync<TUser>(user.Guid, user.AccessFailedCount + 1);
            }

            Task setLockoutTask = Task.CompletedTask;

            if (_signInOptions.RequiredLockoutCheck)
            {
                if (user.AccessFailedCount + 1 > _signInOptions.LockoutAfterAccessFailedCount)
                {
                    setLockoutTask = _identityService.SetLockoutAsync<TUser>(user.Guid, true, _signInOptions.LockoutTimeSpan);
                }
            }

            return Task.WhenAll(setAccessFailedCountTask, setLockoutTask);
        }

        private async Task BlackSignInTokenAsync(SignInToken signInToken)
        {
            //TODO: 详细记录Black SiginInToken 的历史纪录
            TransactionContext transactionContext = await _database.BeginTransactionAsync<SignInToken>().ConfigureAwait(false);
            try
            {
                await _signInTokenBiz.DeleteAsync(signInToken.Guid, transactionContext).ConfigureAwait(false);

                await _database.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await _database.RollbackAsync(transactionContext).ConfigureAwait(false);

                throw;
            }
        }

        private ClaimsPrincipal ValidateTokenWithoutLifeCheck(RefreshContext context)
        {
            TokenValidationParameters parameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidIssuer = _options.OpenIdConnectConfiguration.Issuer,
                IssuerSigningKeys = _credentialBiz.GetIssuerSigningKeys()
            };

            return new JwtSecurityTokenHandler().ValidateToken(context.AccessToken, parameters, out SecurityToken validatedToken);
        }
    }
}
