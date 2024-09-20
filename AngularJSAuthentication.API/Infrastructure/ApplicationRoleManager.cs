using AngularJSAuthentication.API;
using AspNetIdentity.WebApi.Infrastructure;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AngularJSAuthentication.Infrastructure
{
    public class ApplicationRoleManager : RoleManager<IdentityRole>
    {
        public ApplicationRoleManager(IRoleStore<IdentityRole, string> roleStore)
            : base(roleStore)
        {
        }



        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            var appRoleManager = new ApplicationRoleManager(new RoleStore<IdentityRole>(context.Get<AuthContext>()));

            return appRoleManager;
        }
        public async Task<int> AddToRoleAsync(string user, String roleName, int reqid, bool IsPrimary, DateTime? validfrom,
            DateTime? validTill, string createdby, bool isactive)
        {
            ApplicationRole role = null;
            using (AuthContext context = new AuthContext())
            {

                try
                {
                    role = context.Set<ApplicationRole>().First();
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                context.Set<ApplicationUserRole>().Add(new ApplicationUserRole
                {
                    ReqAccId = reqid,
                    RoleId = roleName,
                    UserId = user,
                    IsPrimary = IsPrimary,
                    validFrom = validfrom,
                    validTill = validTill,
                    CreatedDate = DateTime.Now.Date,
                    CreatedBy = createdby,
                    isActive = isactive



                });

                return await context.CommitAsync();
            }
        }

    }
}