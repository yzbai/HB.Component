using HB.Framework.Common;
using System.ComponentModel.DataAnnotations;

namespace HB.Component.Authorization.Abstractions
{
    public class RefreshContext : ValidatableObject
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }

        [Required]
        public string DeviceId { get; set; }
        public string DeviceType { get; set; }
        public string DeviceVersion { get; set; }
        public string DeviceAddress { get; set; }
    }
}
