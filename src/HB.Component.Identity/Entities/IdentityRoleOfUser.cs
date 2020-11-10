using HB.Framework.Database.Entities;
using System;
using System.Diagnostics.CodeAnalysis;

namespace HB.Component.Identity.Entities
{
    /// <summary>
    /// 用户-角色 关系 实体
    /// </summary>
    public abstract class IdentityRoleOfUser : DatabaseEntity
    {
        [ForeignKey(typeof(IdenityUser))]
        [GuidEntityProperty(NotNull = true)]
        [DisallowNull, NotNull]
        public string UserGuid { get; set; } = default!;


        [ForeignKey(typeof(IdentityRole))]
        [GuidEntityProperty(NotNull = true)]
        public string RoleGuid { get; set; } = default!;
    }
}
