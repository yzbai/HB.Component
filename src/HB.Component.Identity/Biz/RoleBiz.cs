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

        public Task<IEnumerable<Role>> GetByUserGuidAsync(string userGuid, TransactionContext transContext = null)
        {
            ThrowIf.NullOrEmpty(userGuid, nameof(userGuid));

            FromExpression<Role> from = _database.From<Role>().RightJoin<UserRole>((r, ru) => r.Guid == ru.RoleGuid);
            WhereExpression<Role> where = _database.Where<Role>().And<UserRole>(ru => ru.UserGuid == userGuid);

            return _database.RetrieveAsync<Role>(from, where, transContext);
        }
    }
}
