using HB.Component.Identity.Entity;
using System;

namespace HB.Component.Authorization
{
    public enum AuthorizationError
    {
        Failed = 0,
        Success = 1,
        LogoffOtherClientFailed = 2,
        NewUserCreateFailed = 3,
        NewUserCreateFailedMobileAlreadyTaken = 4,
        NewUserCreateFailedEmailAlreadyTaken = 5,
        NewUserCreateFailedUserNameAlreadyTaken = 6,
        LockedOut = 7,
        TwoFactorRequired = 8,
        MobileNotConfirmed = 9,
        EmailNotConfirmed = 10,
        OverMaxFailedCount = 11,
        NoSuchUser = 12,
        PasswordWrong = 13,
        AuthtokenCreatedFailed = 14,
        ArgumentError = 15,
        ExceptionThrown = 16,
        NotFound = 17,
        TooFrequent = 18,
        InvalideAccessToken = 19,
        InvalideUserGuid = 20,
        NoTokenInStore = 21,
        UserSecurityStampChanged = 22,
        UpdateSignInTokenError = 23,
        InvalideDeviceId = 24,
    }
}