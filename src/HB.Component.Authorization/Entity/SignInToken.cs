using HB.Framework.Database.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using HB.Component.Identity.Entity;

namespace HB.Component.Authorization.Entity
{
    public class SignInToken : DatabaseEntity
    {
        [UniqueGuidEntityProperty]
        public string Guid { get; set; }

        [ForeignKey(typeof(User))]
        [GuidEntityProperty]
        public string UserGuid { get; set; }

        [Required]
        [EntityProperty]
        public string RefreshToken { get; set; }

        [EntityProperty]
        public DateTimeOffset? ExpireAt { get; set; }

        [EntityProperty]
        public long RefreshCount { get; set; } = 0;

        [EntityProperty]
        public bool Blacked { get; set; } = false;


        #region Device

        [Required]
        [EntityProperty]
        public string DeviceId { get; set; }

        [Required]
        [EntityProperty]
        public string DeviceType { get; set; }

        [EntityProperty]
        public string DeviceVersion { get; set; }

        [EntityProperty]
        public string DeviceAddress { get; set; }

        [EntityProperty]
        public string DeviceIp { get; set; }

        #endregion
    }
}