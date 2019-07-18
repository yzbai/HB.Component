using HB.Component.Identity;
using HB.Component.Identity.Entity;
using HB.Framework.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Component.Authorization.Abstractions
{
    public enum SignInResultStatus
    {
        Success,
        LogoffOtherClientFailed,
        NewUserCreateFailed,
        NewUserCreateFailedMobileAlreadyTaken,
        NewUserCreateFailedEmailAlreadyTaken,
        NewUserCreateFailedUserNameAlreadyTaken,
        LockedOut,
        TwoFactorRequired,
        MobileNotConfirmed,
        EmailNotConfirmed,
        OverMaxFailedCount,
        NoSuchUser,
        PasswordWrong,
        AuthtokenCreatedFailed,
        ArgumentError,
        ExceptionThrown,
        NotFound,
        Failed,
        TooFrequent,
        InvalideAccessToken,
        InvalideUserGuid,
        NoTokenInStore,
        UserSecurityStampChanged,
        UpdateSignInTokenError,
        InvalideDeviceId,
    }

    public class SignInResult
    {
        public SignInResultStatus Status { get; set; }
        public User CurrentUser { get; set; }
        public bool NewUserCreated { get; set; }
        
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }

        public Exception Exception { get; set; }

        public bool IsSucceeded()
        {
            return Status == SignInResultStatus.Success;
        }
        public static SignInResult Throwed(Exception exception = null)
        {
            return new SignInResult { Status = SignInResultStatus.ExceptionThrown, Exception = exception };
        }

        public static SignInResult ArgumentError()
        {
            return new SignInResult { Status = SignInResultStatus.ArgumentError };
        }

        public static SignInResult NewUserCreateFailed()
        {
            return new SignInResult { Status = SignInResultStatus.NewUserCreateFailed };
        }

        public static SignInResult NewUserCreateFailedEmailAlreadyTaken()
        {
            return new SignInResult { Status = SignInResultStatus.NewUserCreateFailedEmailAlreadyTaken };
        }

        public static SignInResult NewUserCreateFailedMobileAlreadyTaken()
        {
            return new SignInResult { Status = SignInResultStatus.NewUserCreateFailedMobileAlreadyTaken };
        }

        public static SignInResult NewUserCreateFailedUserNameAlreadyTaken()
        {
            return new SignInResult { Status = SignInResultStatus.NewUserCreateFailedUserNameAlreadyTaken };
        }

        public static SignInResult NoSuchUser()
        {
            return new SignInResult { Status = SignInResultStatus.NoSuchUser };
        }

        public static SignInResult PasswordWrong()
        {
            return new SignInResult { Status = SignInResultStatus.PasswordWrong };
        }

        public static SignInResult AuthtokenCreatedFailed()
        {
            return new SignInResult { Status = SignInResultStatus.AuthtokenCreatedFailed };
        }

        public static SignInResult MobileNotConfirmed()
        {
            return new SignInResult { Status = SignInResultStatus.MobileNotConfirmed };
        }

        public static SignInResult EmailNotConfirmed()
        {
            return new SignInResult { Status = SignInResultStatus.EmailNotConfirmed };
        }

        public static SignInResult LockedOut()
        {
            return new SignInResult { Status = SignInResultStatus.LockedOut };
        }

        public static SignInResult OverMaxFailedCount()
        {
            return new SignInResult { Status = SignInResultStatus.OverMaxFailedCount };
        }

        public static SignInResult Succeeded()
        {
            return new SignInResult { Status = SignInResultStatus.Success };
        }

        public static SignInResult LogoffOtherClientFailed()
        {
            return new SignInResult { Status = SignInResultStatus.LogoffOtherClientFailed };
        }

        public static SignInResult TooFrequent()
        {
            return new SignInResult { Status = SignInResultStatus.TooFrequent };
        }

        public static SignInResult InvalideAccessToken()
        {
            return new SignInResult { Status = SignInResultStatus.InvalideAccessToken };
        }

        public static SignInResult InvalideUserGuid()
        {
            return new SignInResult { Status = SignInResultStatus.InvalideUserGuid };
        }

        public static SignInResult NoTokenInStore()
        {
            return new SignInResult { Status = SignInResultStatus.NoTokenInStore };
        }

        public static SignInResult UserSecurityStampChanged()
        {
            return new SignInResult { Status = SignInResultStatus.UserSecurityStampChanged };
        }

        public static SignInResult UpdateSignInTokenError()
        {
            return new SignInResult { Status = SignInResultStatus.UpdateSignInTokenError };
        }

        internal static SignInResult InvalideDeviceId()
        {
            return new SignInResult { Status = SignInResultStatus.InvalideDeviceId };
        }

        public SignInResult() { }
        public SignInResult(DatabaseResult dbResult)
        {
            Exception = dbResult?.Exception;

            switch (dbResult.ThrowIfNull(nameof(dbResult)).Status)
            {
                case DatabaseResultStatus.Failed:
                    Status = SignInResultStatus.Failed;
                    break;
                case DatabaseResultStatus.NotFound:
                    Status = SignInResultStatus.NotFound;
                    break;
                case DatabaseResultStatus.NotWriteable:
                    Status = SignInResultStatus.Failed;
                    break;
                case DatabaseResultStatus.Succeeded:
                    Status = SignInResultStatus.Success;
                    break;
                default:
                    Status = SignInResultStatus.Failed;
                    break;
            }
        }

        
    }

    public static class DatabaseResultExtensions
    {
        public static SignInResult ToSignInResult(this DatabaseResult dbResult)
        {
            return new SignInResult(dbResult);
        }
    }
}