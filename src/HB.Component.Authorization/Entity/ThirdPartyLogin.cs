using HB.Framework.Database.Entity;
using HB.Component.Identity;
using System;
using HB.Component.Identity.Entity;

namespace HB.Component.Authorization.Entity
{
    public class ThirdPartyLogin : DatabaseEntity
    {
        [UniqueGuidEntityProperty]
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();

        [ForeignKey(typeof(IdenityUser))]
        [GuidEntityProperty(NotNull = true)]
        public string UserGuid { get; set; } = default!;

        [EntityProperty("登陆提供者", Length = 500, NotNull = true)]
        public string LoginProvider { get; set; } = default!;

        [EntityProperty("登陆key", Length = 500, NotNull = true)]
        public string ProviderKey { get; set; } = default!;

        [EntityProperty("提供者显示名称", Length = 500, NotNull = true)]
        public string ProviderDisplayName { get; set; } = default!;

        [EntityProperty("", NotNull = true)]
        public string SnsName { get; set; } = default!;

        [EntityProperty("", NotNull = true)]
        public string SnsId { get; set; } = default!;

        [EntityProperty("", NotNull = true)]
        public string AccessToken { get; set; } = default!;

        [EntityProperty("", Length = 1024)]
        public string? IconAddress { get; set; }
    }
}
