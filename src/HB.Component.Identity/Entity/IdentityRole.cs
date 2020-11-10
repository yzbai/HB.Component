using HB.Framework.Database.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HB.Component.Identity.Entity
{
    /// <summary>
    /// 角色
    /// </summary>
    public abstract class IdentityRole : DatabaseEntity
    {
        [UniqueGuidEntityProperty]
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();

        [EntityProperty("角色名", Unique = true, NotNull = true)]
        public string Name { get; set; } = default!;

        [EntityProperty("DisplayName", Length = 500, NotNull = true)]
        public string DisplayName { get; set; } = default!;

        [EntityProperty("是否激活")]
        public bool IsActivated { get; set; }

        [EntityProperty("说明", Length = 1024)]
        public string? Comment { get; set; }
    }


}