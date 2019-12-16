using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entity;
using HB.Framework.Database;
using HB.Framework.Database.SQL;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    internal class RoleBiz : IRoleBiz
    {
        private readonly IDatabase _database;

        public RoleBiz(IDatabase database)
        {
            _database = database;
        }

        public Task<IEnumerable<TRole>> GetByUserGuidAsync<TRole, TUserRole>(string userGuid, TransactionContext transContext = null) 
            where TRole : Role, new() 
            where TUserRole : UserRole, new()
        {
            ThrowIf.NullOrEmpty(userGuid, nameof(userGuid));

            FromExpression<TRole> from = _database.From<TRole>().RightJoin<TUserRole>((r, ru) => r.Guid == ru.RoleGuid);
            WhereExpression<TRole> where = _database.Where<TRole>().And<UserRole>(ru => ru.UserGuid == userGuid);

            return _database.RetrieveAsync(from, where, transContext);
        }
    }
}
