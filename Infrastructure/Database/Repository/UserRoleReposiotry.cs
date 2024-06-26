﻿using Domain.ApplicationUserAggregate;
using Infrastructure.Database.Database;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Extensions;
using Domain.LookupsAggregate;
using Domain.Base;
using Utilities.Exceptions;

namespace Infrastructure.Database.Repository
{
    public class UserRoleReposiotry : RepositoryBase, IUserRoleRepository
    {
        public UserRoleReposiotry(SpatiumDbContent spatiumDbContent) : base(spatiumDbContent)
        {

        }
        #region GetRoleDetailes
        public async Task<UserRole> GetRoleByIdAsync(string roleId, int blogId)
        {
            return await SpatiumDbContent.Roles.Include(ru => ru.ApplicationUsers)
                            .Where(r => r.Id == roleId && (r.BlogId == blogId || r.BlogId==null))
                            .FirstOrDefaultAsync();

        }

        public async Task<List<UserRole>> GetDefaultRoles(int blogId)
        {
            return await SpatiumDbContent.Roles.Include(x => x.ApplicationUsers.Where(y => y.BlogId == blogId)).Where(r => r.RoleOwnerId == null).ToListAsync();
        }

        public async Task<IReadOnlyList<ApplicationUser>> GetUsersByBlogIdAndRolePriority(int blogId, int priorityOfCurrent)
        {
            return await SpatiumDbContent.Users.Where(u => u.BlogId == blogId && u.Role.Priority >= priorityOfCurrent).ToListAsync();
        }

        public async Task<List<UserRole>> SearchInRole(string CoulmnName, string Value)
        {
            var query = SpatiumDbContent.Roles.AsQueryable();
            var result = query.ApplySearch(CoulmnName, Value);
            return await result.ToListAsync();
        }

        public async Task<IReadOnlyList<UserRole>> GetRolesAsync(GetEntitiyParams entitiyParams, int blogId)
        {
            var query = SpatiumDbContent.Roles.Include(x => x.ApplicationUsers.Where(y => y.BlogId == blogId)).AsQueryable();

            if (!string.IsNullOrEmpty(entitiyParams.FilterColumn))
            {
                if (!string.IsNullOrEmpty(entitiyParams.FilterValue) && entitiyParams.StartDate == null && entitiyParams.EndDate == null)
                {
                    query = query.ApplyFilter(entitiyParams.FilterColumn, entitiyParams.FilterValue);
                }
                if (entitiyParams.StartDate != null && entitiyParams.EndDate != null && entitiyParams.FilterColumn.ToLower() == "createdat")
                {
                    query = query.Where(p => p.CreatedAt >= entitiyParams.StartDate && p.CreatedAt == entitiyParams.EndDate || p.CreatedAt < entitiyParams.EndDate);
                }
            }

            if (!string.IsNullOrEmpty(entitiyParams.SortColumn))
            {
                query = query.ApplySort(entitiyParams.SortColumn, entitiyParams.IsDescending);
            }

            if (!string.IsNullOrEmpty(entitiyParams.SearchColumn) && !string.IsNullOrEmpty(entitiyParams.SearchValue))
            {
                query = query.ApplySearch(entitiyParams.SearchColumn, entitiyParams.SearchValue);
            }

            var paginatedQuery = query.Skip((entitiyParams.Page - 1) * entitiyParams.PageSize).Take(entitiyParams.PageSize);
            return await paginatedQuery.ToListAsync();
        }
        public async Task<List<UserModule>> GetModuleWithPermissions()
        {
            return await SpatiumDbContent.UserModules.Include(m => m.UserPermissions).ToListAsync();

        }
        public Task<List<int>> GetRolePermissionIds(string roleId)
        {
            return SpatiumDbContent.RolePermission.Where(r => r.UserRoleId == roleId).Select(r => r.UserPermissionId).ToListAsync();
        }

        public  async Task<IEnumerable<UserRole>> GetRoles(int blogId)
        {  
            return await SpatiumDbContent.Roles.Where(r => r.BlogId == blogId).ToListAsync();
        }
        #endregion

        //public async Task CreatAsync(UserRole role)
        //{
        //    await SpatiumDbContent.Roles.AddAsync(role);
        //}
        //public async Task UpdateAsync(string roleId, UserRole role)
        //{
        //    var found = await SpatiumDbContent.Roles.FirstOrDefaultAsync(r => r.Id.Equals(roleId));
        //    if (found != null)
        //    {
        //        SpatiumDbContent.Roles.Update(found);
        //    }
        //}

        #region Delete Role 
        public async Task<List<ApplicationUser>> GetUserInRoleAsync(string roleId)
        {
            return await SpatiumDbContent.Users.Where(ur => ur.RoleId == roleId).ToListAsync();
        }

        public async Task<List<RolePermission>> GetRolePermissionsAsync(string roleId)
        {
            return await SpatiumDbContent.RolePermission.Where(rp => rp.UserRoleId == roleId).ToListAsync();

        }
        #endregion
        //public async Task DeleteRoleAsync(string roleId)
        //{
        //    //deleted related user
        //    //var user = await GetUserInRoleAsync(roleId);
        //    //SpatiumDbContent.Users.Remove(user);

        //    //deleted related roles in rolepermissions
        //    var rolePermissions = await SpatiumDbContent.RolePermissions.Where(rp => rp.UserRoleId == roleId).ToListAsync();
        //    SpatiumDbContent.RolePermissions.RemoveRange(rolePermissions);


        //    //deleted the role from the table of role 
        //    var role = await SpatiumDbContent.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
        //    SpatiumDbContent.Roles.Remove(role);
        //}


        //public async Task<ApplicationUser> GetUserInRoleAsync(string roleId)
        //{
        //    return await SpatiumDbContent.Users.FirstOrDefaultAsync(ur => ur.RoleId == roleId);
        //}

        //public async Task<bool> IsRoleAssignedToUserAsync(string roleId)
        //{
        //    return await SpatiumDbContent.UserRoles.AnyAsync(ur => ur.RoleId == roleId);
        //}

        //public async Task<bool> DoesRoleHavePermissionsAsync(string roleId)
        //{
        //    return await SpatiumDbContent.RolePermissions.AnyAsync(rp => rp.UserRoleId == roleId);
        //}

        public async Task<UserRole> GetRoleByIdForUpdate(string roleId)
        {
            return await SpatiumDbContent.Roles.Include(x => x.RolePermission).IgnoreQueryFilters().Where(x => x.Id == roleId && x.RoleOwnerId != null).FirstOrDefaultAsync();
        }

        public async Task<List<ApplicationUser>> GetUsersInRoleAsync(string roleId)
        {
            return await SpatiumDbContent.Users.Where(ur => ur.RoleId == roleId).ToListAsync();
        }


        public async Task<UserRole> GetAssignRoleById(int blogId, string roleId)
        {
            return await SpatiumDbContent.Roles.FirstOrDefaultAsync(x => x.Priority>1 && (x.BlogId == blogId ||x.Blog==null) && x.Id == roleId);
        }

        public async Task<List<RoleIcon>> GetRoleIconsAsync()
        {
            return await SpatiumDbContent.RoleIcons.ToListAsync();
        }

        #region Try Delete User
        public async Task TryDeleteCustomRoleAsync(int blogId, string superAdmidId)
        {
            var customRoles = await SpatiumDbContent.Roles.Where(r => r.BlogId == blogId).ToListAsync();
            foreach (var item in customRoles)
            {
                foreach (var rolepermission in item.RolePermission)
                {
                    testDeleteRolePermission(rolepermission);
                }
            }
            foreach (var item in customRoles)
            {
                foreach (var user in item.ApplicationUsers)
                {
                    testDeleteUser(user);
                }
            }
            foreach (var item in customRoles)
                testDeleteCustomRole(item);

        }
        public void testDeleteRolePermission(RolePermission rolePermission)
        {
            SpatiumDbContent.RolePermission.Remove(rolePermission);
        }
        public void testDeleteUser(ApplicationUser user)
        {
            SpatiumDbContent.Users.Remove(user);
        }
        public void testDeleteCustomRole(UserRole userRole)
        {
            SpatiumDbContent.Roles.Remove(userRole);
        }
        public async Task FinalDeleteUser(int blogId, string superAdmidId)
        {
          var user= await SpatiumDbContent.Users.Where(U=>U.BlogId==blogId  && U.Id == superAdmidId).FirstOrDefaultAsync();
            if (user !=null)
            {
                SpatiumDbContent.Users.RemoveRange(user.ApplicationUsers);
                SpatiumDbContent.Users.Remove(user);

               await SpatiumDbContent.SaveChangesAsync();
            }
            else
            {
                throw new SpatiumException("User Not found");
            }

        }

       
       

        #endregion

        #region ActivityLog
        public async Task<IEnumerable<ActivityLog>> GetActivityLog(string UserId)
        {
            return await SpatiumDbContent.ActivityLogs.Include(a => a.LogIcon).Where(a => a.UserId == UserId).ToListAsync();
        }

        #endregion
    }
}
