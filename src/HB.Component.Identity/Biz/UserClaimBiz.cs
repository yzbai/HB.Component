using HB.Framework.Database;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entity;
using System;

namespace HB.Component.Identity
{
    internal class UserClaimBiz : IUserClaimBiz
    {
        private readonly IDatabase _db;

        public UserClaimBiz(IDatabase database)
        {
            _db = database;
        }

        public Task<IEnumerable<UserClaim>> GetAsync(string userGuid, TransactionContext transContext = null)
        {
            ThrowIf.NullOrEmpty(userGuid, nameof(userGuid));

            return _db.RetrieveAsync<UserClaim>(uc => uc.UserGuid == userGuid, transContext);
        }
    }
}
