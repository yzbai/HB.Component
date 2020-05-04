using HB.Framework.Database.Entity;
using System;

namespace HB.Component.Identity.Entity
{
    public abstract class UserClaim : DatabaseEntity
    {
        [UniqueGuidEntityProperty]
        public string Guid { get; set; } = default!;

        [ForeignKey(typeof(User))]
        [GuidEntityProperty(NotNull = true)]
        public string UserGuid { get; set; } = default!;

        [EntityProperty("ClaimType", Length = 65530, NotNull = true)]
        public string ClaimType { get; set; } = default!;

        [EntityProperty("ClaimValue", Length = 65530, NotNull = true)]
        public string ClaimValue { get; set; } = default!;

        [EntityProperty]
        public bool AddToJwt { get; set; } = false;
    }
}
