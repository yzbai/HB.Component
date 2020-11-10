using HB.Framework.Database.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HB.Component.Identity.Entity
{
    /// <summary>
    /// ��ɫ
    /// </summary>
    public abstract class IdentityRole : DatabaseEntity
    {
        [UniqueGuidEntityProperty]
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();

        [EntityProperty("��ɫ��", Unique = true, NotNull = true)]
        public string Name { get; set; } = default!;

        [EntityProperty("DisplayName", Length = 500, NotNull = true)]
        public string DisplayName { get; set; } = default!;

        [EntityProperty("�Ƿ񼤻�")]
        public bool IsActivated { get; set; }

        [EntityProperty("˵��", Length = 1024)]
        public string? Comment { get; set; }
    }


}