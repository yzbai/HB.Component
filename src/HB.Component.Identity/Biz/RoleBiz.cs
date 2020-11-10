using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entities;
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

        /// <summary>
        /// GetByUserGuidAsync
        /// </summary>
        /// <param name="userGuid"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public Task<IEnumerable<TRole>> GetByUserGuidAsync<TRole, TRoleOfUser>(string userGuid, TransactionContext? transContext = null) 
            where TRole : IdentityRole, new() 
            where TRoleOfUser : IdentityRoleOfUser, new()
        {
            FromExpression<TRole> from = _database.From<TRole>().RightJoin<TRoleOfUser>((r, ru) => r.Guid == ru.RoleGuid);
            WhereExpression<TRole> where = _database.Where<TRole>().And<IdentityRoleOfUser>(ru => ru.UserGuid == userGuid);

            return _database.RetrieveAsync(from, where, transContext);
        }
    }
}
