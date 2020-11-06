using HB.Framework.Database.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using HB.Component.Identity.Entity;

namespace HB.Component.Authorization.Entity
{
    public class SignInToken : DatabaseEntity
    {
        [Required]
        [UniqueGuidEntityProperty]
        public string Guid { get; set; } = default!;

        [ForeignKey(typeof(IdenityUser))]
        [GuidEntityProperty(NotNull = true)]
        public string UserGuid { get; set; } = default!;

        [Required]
        [EntityProperty(NotNull = true)]
        public string RefreshToken { get; set; } = default!;

        [EntityProperty]
        public DateTimeOffset? ExpireAt { get; set; }

        [EntityProperty]
        public long RefreshCount { get; set; } = 0;

        [EntityProperty]
        public bool Blacked { get; set; } = false;


        #region Device

        [Required]
        [EntityProperty(NotNull = true)]
        public string DeviceId { get; set; } = default!;

        [Required]
        [EntityProperty(NotNull = true, Converter = typeof(DeviceInfosDatabaseTypeConverter))]
        public DeviceInfos DeviceInfos { get; set; } = default!;

        [EntityProperty(NotNull = true)]
        public string DeviceVersion { get; set; } = default!;

        [EntityProperty(NotNull = false)]
        public string? DeviceAddress { get; set; }

        [EntityProperty(NotNull = true)]
        public string DeviceIp { get; set; } = default!;

        #endregion
    }

    public class DeviceInfosDatabaseTypeConverter : DatabaseTypeConverter
    {
        protected override object? StringDbValueToTypeValue(string stringValue)
        {
            return SerializeUtil.FromJson<DeviceInfos>(stringValue);
        }

        protected override string TypeValueToStringDbValue(object typeValue)
        {
            return SerializeUtil.ToJson(typeValue);
        }
    }
}