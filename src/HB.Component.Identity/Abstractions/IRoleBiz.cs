using HB.Component.Identity.Entity;
using HB.Framework.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace HB.Component.Identity.Abstractions
{
    internal interface IRoleBiz
    {

        //ErrCode AddRole(Role role, string lastUser);
        //ErrCode AddUserToRole(int userId, string roleName, string lastUser);
        //ErrCode DeleteRole(Role role, string lastUser);
        //ErrCode DeleteRoleByRoleName(string RoleName, string lastUser);
        //ErrCode DeleteUserFromRole(int userId, string roleName, string lastUser);
        //IEnumerable<Role> GetRole();
        //Role GetRoleById(int roleId);
        //Role GetRoleByName(string roleName);
        //IList<Role> GetRoleByNames(string[] roleNames);
        //IList<Role> GetRoleByUserId(int userId);
        //bool IsUserInRole(int userId, string roleName);
        //ErrCode SeTRoleOfUsers(int userId, string[] roleNames, string lastUser);
        //ErrCode UpdateRole(Role role, string lastUser);

        /// <exception cref="DatabaseException"></exception>
        Task<IEnumerable<TRole>> GetByUserGuidAsync<TRole, TRoleOfUser>(string userGuid, TransactionContext? transContext = null) where TRole : IdentityRole, new() where TRoleOfUser : IdentityRoleOfUser, new();

    }
}