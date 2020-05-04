using HB.Framework.Database.Entity;
using System;
using System.Diagnostics.CodeAnalysis;

namespace HB.Component.Identity.Entity
{
    /// <summary>
    /// 用户-角色 关系 实体
    /// </summary>
    public abstract class RoleOfUser : DatabaseEntity
    {
        [UniqueGuidEntityProperty]
        public string Guid { get; set; } = default!;

        [ForeignKey(typeof(User))]
        [GuidEntityProperty(NotNull = true)]
        [DisallowNull, NotNull]
        public string UserGuid { get; set; } = default!;


        [ForeignKey(typeof(Role))]
        [GuidEntityProperty(NotNull = true)]
        public string RoleGuid { get; set; } = default!;
    }
}
