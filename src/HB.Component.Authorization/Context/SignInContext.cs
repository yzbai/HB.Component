using HB.Framework.Common;
using System.ComponentModel.DataAnnotations;

namespace HB.Component.Authorization.Abstractions
{

    public class SignInContext : ValidatableObject
    {
        //public HttpContext HttpContext { get; set; }

        public SignInType SignInType { get; set; }


        [UserName]
        public string? UserName { get; set; }

        [Password]
        public string? Password { get; set; }

        [Mobile]
        public string? Mobile { get; set; }

        public bool RememberMe { get; set; }

        public string DeviceId { get; set; } = default!;
        public string DeviceType { get; set; } = default!;
        public string DeviceVersion { get; set; } = default!;
        public string DeviceAddress { get; set; } = default!;
        public string DeviceIp { get; set; } = default!;

        public string SignToWhere { get; set; } = default!;
    }
}
