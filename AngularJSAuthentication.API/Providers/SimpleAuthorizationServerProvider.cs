
using AngularJSAuthentication.API.DataContract;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Infrastructure;
using AngularJSAuthentication.Model;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AngularJSAuthentication.API.Providers
{
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {

        public Logger logger = LogManager.GetCurrentClassLogger();
        int User_Id;
        string ac_Type;
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            if (context.Parameters.Any(f => f.Key == "warehouseid"))
            {
                string warehouseId = context.Parameters.Where(f => f.Key == "warehouseid").Select(f => f.Value).SingleOrDefault()[0];
                context.OwinContext.Set<string>("warehouseid", warehouseId);
            }
            if (context.Parameters.Any(f => f.Key == "usertype"))
            {
                string userType = context.Parameters.Where(f => f.Key == "usertype").Select(f => f.Value).SingleOrDefault()[0];
                context.OwinContext.Set<string>("usertype", userType);
            }
            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var allowedOrigin = "*";
            //context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });
            string warehouseid = context.OwinContext.Get<string>("warehouseid");
            string userType = context.OwinContext.Get<string>("usertype");
            try
            {
                using (AuthContext con = new AuthContext())
                {

                    var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();
                    ApplicationUser user = await userManager.FindAsync(context.UserName, context.Password);
                    if (user == null)
                    {
                        context.SetError("invalid_grant", "The user name or password is incorrect.");
                        return;
                    }
                    if (!user.EmailConfirmed)
                    {
                        context.SetError("invalid_grant", "User did not confirm email.");
                        return;
                    }
                    if (user != null && userType != "1" && user.usertype == 1)
                    {
                        context.SetError("invalid_grant", "You are not authorize to acees this application.");
                        return;
                    }
                    People p = con.getPersonIdfromEmail(user.Email);
                    // sk seller (Group2)
                    if (userType == "1" && (user.usertype == 1 || p.IsSellerPortallogin == true) && p != null)
                    {
                        ClaimsIdentity identity = await user.GenerateUserIdentityAsync(userManager, "JWT");
                        identity.AddClaims(ExtendedClaimsProvider.GetClaims(user));
                        identity.AddClaims(RolesFromClaims.CreateRolesBasedOnClaims(identity));
                        identity.AddClaim(new Claim("userid", p.PeopleID.ToString()));

                        string sqlquery = "Exec GetUserRoles '" + user.Id + "'";
                        var UserRoleDc = con.Database.SqlQuery<DataContracts.Masters.UserRoleDc>(sqlquery).ToList();

                        var rolesIds = UserRoleDc != null && UserRoleDc.Any() ? UserRoleDc.Select(x => x.RoleId).ToList() : new List<string>();
                        var rolesNames = UserRoleDc != null && UserRoleDc.Any() ? UserRoleDc.Select(x => x.RoleName).ToList() : new List<string>();
                        rolesNames = rolesNames.Distinct().ToList();
                        var IsAdmin = UserRoleDc != null && UserRoleDc.Any() ? UserRoleDc.Any(x => x.RoleName == "HQ Master login") : false;
                        var pagePermissions = new List<PeoplePageDc>();
                        var pageManager = new PagePermissionManager();
                        pagePermissions = await pageManager.GetPeoplePagePermissionAsync(p.PeopleID, rolesIds);

#if !DEBUG
                    Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
                    _cacheProvider.Remove(Caching.CacheKeyHelper.PeoplePageKey(p.PeopleID));
                    _cacheProvider.Set(Caching.CacheKeyHelper.PeoplePageKey(p.PeopleID), pagePermissions);
                     if (pagePermissions != null && pagePermissions.Any())
                        identity.AddClaims(pagePermissions.Where(x => !x.ParentId.HasValue)
                            .Select(x => new Claim(!string.IsNullOrEmpty(x.RouteName) ? x.RouteName.Replace(" ", "") : x.PageName.Replace(" ", ""), "True")
                            ));
#endif
                        if (rolesNames != null && rolesNames.Any())
                            identity.AddClaim(new Claim(ClaimTypes.Role, rolesNames.FirstOrDefault()));
                        identity.AddClaim(new Claim("Roleids", string.Join(",", rolesIds)));
                        identity.AddClaim(new Claim("RoleNames", string.Join(",", rolesNames)));

                        var props = new AuthenticationProperties(new Dictionary<string, string> { });
                        props = new AuthenticationProperties(new Dictionary<string, string>
                        {
                            { "as:client_id", (context.ClientId == null) ? string.Empty : context.ClientId },
                            { "userName", context.UserName },
                            { "UserType", userType.ToString()},
                            { "userid", p.PeopleID.ToString() },
                            { "rolenames",  string.Join(",", rolesNames) },
                            { "pagePermissions", JsonConvert.SerializeObject(pagePermissions)}

                        });
                        var ticket = new AuthenticationTicket(identity, props);
                        context.Validated(ticket);
                    }
                    else if (user.usertype != 1)
                    {
                        if (string.IsNullOrEmpty(user.ApkName))
                        {
                            int UserId = p.PeopleID;
                            if (!p.Active)
                            {
                                context.SetError("invalid_grant", "Please check your registered email address to validate email address.");
                                return;
                            }
                            if (UserId == 0)
                            {
                                context.SetError("invalid_grant", "The user name or password is incorrect.");
                                return;
                            }

                            ClaimsIdentity identity = await user.GenerateUserIdentityAsync(userManager, "JWT");
                            identity.AddClaims(ExtendedClaimsProvider.GetClaims(user));
                            identity.AddClaims(RolesFromClaims.CreateRolesBasedOnClaims(identity));

                            // UserAccessPermission uap = con.getRoleDetail(p.Permissions); // Get Role Access Detail
                            //var rolesIds = user.Roles.Select(x => x.RoleId).ToList();

                            string sqlquery = "Exec GetUserRoles '" + user.Id + "'";
                            var UserRoleDc = con.Database.SqlQuery<DataContracts.Masters.UserRoleDc>(sqlquery).ToList();

                            var rolesIds = UserRoleDc != null && UserRoleDc.Any() ? UserRoleDc.Select(x => x.RoleId).ToList() : new List<string>();
                            var rolesNames = UserRoleDc != null && UserRoleDc.Any() ? UserRoleDc.Select(x => x.RoleName).ToList() : new List<string>();
                            //rolesNames.Add(p.Permissions);
                            rolesNames = rolesNames.Distinct().ToList();
                            var IsAdmin = UserRoleDc != null && UserRoleDc.Any() ? UserRoleDc.Any(x => x.RoleName == "HQ Master login") : false;

                            //
                            var pagePermissions = new List<PeoplePageDc>();
                            var pageManager = new PagePermissionManager();
                            pagePermissions = await pageManager.GetPeoplePagePermissionAsync(UserId, rolesIds);

#if !DEBUG
                    Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
                    _cacheProvider.Remove(Caching.CacheKeyHelper.PeoplePageKey(UserId));
                    _cacheProvider.Set(Caching.CacheKeyHelper.PeoplePageKey(UserId), pagePermissions);
                     if (pagePermissions != null && pagePermissions.Any())
                        identity.AddClaims(pagePermissions.Where(x => !x.ParentId.HasValue)
                            .Select(x => new Claim(!string.IsNullOrEmpty(x.RouteName) ? x.RouteName.Replace(" ", "") : x.PageName.Replace(" ", ""), "True")
                            ));

#endif


                            var peopleManager = new PeopleManager();
                            List<int> peopleWarehouseid = peopleManager.GetPeopleWarehouse(p.PeopleID, IsAdmin);

                            //var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                            //identity.AddClaims(ExtendedClaimsProvider.GetClaims(user));
                            identity.AddClaims(RolesFromClaims.CreateRolesBasedOnClaims(identity));
                            identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
                            if (rolesNames != null && rolesNames.Any())
                                identity.AddClaim(new Claim(ClaimTypes.Role, rolesNames.FirstOrDefault()));
                            identity.AddClaim(new Claim("Warehouseid", !string.IsNullOrEmpty(warehouseid) ? warehouseid : p.WarehouseId.ToString()));
                            identity.AddClaim(new Claim("Warehouseids", peopleWarehouseid != null ? string.Join(",", peopleWarehouseid) : ""));
                            identity.AddClaim(new Claim("firsttime", "true"));
                            identity.AddClaim(new Claim("compid", p.CompanyId.ToString()));
                            identity.AddClaim(new Claim("email", user.Email));
                            identity.AddClaim(new Claim("Email", user.Email.ToString()));
                            identity.AddClaim(new Claim("Level", user.Level.ToString()));
                            identity.AddClaim(new Claim("userid", UserId.ToString()));


                            identity.AddClaim(new Claim("DisplayName", p.DisplayName));
                            identity.AddClaim(new Claim("username", (p.PeopleFirstName + " " + p.PeopleLastName).ToString()));
                            identity.AddClaim(new Claim("userid", UserId.ToString()));
                            identity.AddClaim(new Claim("Roleids", string.Join(",", rolesIds)));
                            identity.AddClaim(new Claim("RoleNames", string.Join(",", rolesNames)));
                            identity.AddClaim(new Claim("identityuserid", user.Id));
                            User_Id = UserId;
                            var props = new AuthenticationProperties(new Dictionary<string, string> { });



                            if (rolesNames.Any(x => x == "HQ Master login"))
                            {
                                props = new AuthenticationProperties(new Dictionary<string, string>
                        {
                            { "as:client_id", (context.ClientId == null) ? string.Empty : context.ClientId },
                            { "userName", context.UserName },
                            { "Warehouseid",!string.IsNullOrEmpty(warehouseid) ? warehouseid : p.WarehouseId.ToString() },
                            { "compid", p.CompanyId.ToString() },
                            { "Level", user.Level.ToString() },
                           // { "role" ,p.Permissions },
                            { "email" ,user.Email },
                            { "userid", UserId.ToString() },
                            { "Skcode",  p.Skcode },
                            { "rolenames",  string.Join(",", rolesNames) },

                    { "Admin", "True" },
                    { "AppAdmin", "True" },
                    { "CaseManagement", "True"},
                    { "Delivery", "True" },
                    { "PurchaseOrder", "True" },
                    { "TaxMaster", "True" },
                    { "Customer",           "True"},
                    { "Supplier",           "True"},
                    { "Warehouse",          "True"},
                    { "CurrentStock",       "True"},
                    { "OrderMaster",        "True"},
                    { "DamageStock",        "True"},
                     { "CashManagment",     "True"},
                    { "ItemMaster",         "True"},
                    { "Reports",            "True"},
                    { "StatisticalRep",     "True"},
                    { "Offer",              "True"},
                     { "Account",           "True"},
                    { "Sales",              "True"},
                    { "AppPromotion",       "True"},
                    { "ItemCategory",       "True"},
                    { "CRM",                "True"},
                    { "Request",            "True"},
                    { "UnitEconomics",      "True"},
                    { "Promopoint",         "True"},
                    { "News",               "True" },
                    { "Warehouseids", peopleWarehouseid != null ? string.Join(",", peopleWarehouseid) : ""},
                    { "pagePermissions", JsonConvert.SerializeObject(pagePermissions)}
               });
                            }
                            else
                            {
                                props = new AuthenticationProperties(new Dictionary<string, string>
                        {
                            { "as:client_id", (context.ClientId == null) ? string.Empty : context.ClientId },
                            { "userName", context.UserName },
                            { "Warehouseid",!string.IsNullOrEmpty(warehouseid) ? warehouseid : p.WarehouseId.ToString() },
                            { "compid", user.PhoneNumber },
                           // { "role" ,p.Permissions },
                            { "Level", user.Level.ToString() },
                            { "email" ,user.Email },
                            { "userid", UserId.ToString() },
                            { "rolenames",  string.Join(",", rolesNames) },

                    { "Admin", "True" },
                    { "AppAdmin", "True" },
                    { "CaseManagement", "True"},
                    { "Delivery", "True" },
                    { "PurchaseOrder", "True" },
                    { "TaxMaster", "True" },
                    { "Customer",           "True"},
                    { "Supplier",           "True"},
                    { "Warehouse",          "True"},
                    { "CurrentStock",       "True"},
                    { "OrderMaster",        "True"},
                    { "DamageStock",        "True"},
                     { "CashManagment",     "True"},
                    { "ItemMaster",         "True"},
                    { "Reports",            "True"},
                    { "StatisticalRep",     "True"},
                    { "Offer",              "True"},
                     { "Account",           "True"},
                    { "Sales",              "True"},
                    { "AppPromotion",       "True"},
                    { "ItemCategory",       "True"},
                    { "CRM",                "True"},
                    { "Request",            "True"},
                    { "UnitEconomics",      "True"},
                    { "Promopoint",         "True"},
                    { "News",               "True" },
                    { "Warehouseids", peopleWarehouseid != null ? string.Join(",", peopleWarehouseid) : ""},
                    { "pagePermissions", JsonConvert.SerializeObject(pagePermissions)}
                        });
                            }

                            //if (pagePermissions != null && pagePermissions.Any())
                            //{
                            //    pagePermissions.ToList().ForEach(x =>
                            //    {
                            //        props.Dictionary.Add(!string.IsNullOrEmpty(x.RouteName)? x.RouteName.Replace(" ",""): x.PageName.Replace(" ", ""), "True");
                            //    });

                            //}
                            var ticket = new AuthenticationTicket(identity, props);
                            context.Validated(ticket);
                        }
                        else  // Authorize APK 
                        {
                            ClaimsIdentity identity = await user.GenerateUserIdentityAsync(userManager, "JWT");
                            identity.AddClaims(ExtendedClaimsProvider.GetClaims(user));
                            identity.AddClaims(RolesFromClaims.CreateRolesBasedOnClaims(identity));
                            identity.AddClaim(new Claim("AppName", user.ApkName));

                            var props = new AuthenticationProperties(new Dictionary<string, string> { });

                            props = new AuthenticationProperties(new Dictionary<string, string>
                        {
                            { "as:client_id", (context.ClientId == null) ? string.Empty : context.ClientId },
                            { "userName", context.UserName },
                            { "AppName", user.ApkName}
                        });


                            var ticket = new AuthenticationTicket(identity, props);
                            context.Validated(ticket);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Unable to validate user {0}", context.UserName);
                logger.Error(ex.InnerException != null ? ex.InnerException.ToString() : ex.ToString());
            }
        }

        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            var originalClient = context.Ticket.Properties.Dictionary["as:client_id"];
            var currentClient = context.ClientId;
            if (originalClient != currentClient)
            {
                context.SetError("invalid_clientId", "Refresh token is issued to a different clientId.");
                return Task.FromResult<object>(null);
            }
            // Change auth ticket for refresh token requests
            var newIdentity = new ClaimsIdentity(context.Ticket.Identity);

            var newClaim = newIdentity.Claims.Where(c => c.Type == "newClaim").FirstOrDefault();
            if (newClaim != null)
            {
                newIdentity.RemoveClaim(newClaim);
            }
            newIdentity.AddClaim(new Claim("newClaim", "newValue"));

            var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
            context.Validated(newTicket);

            return Task.FromResult<object>(null);
        }
        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }
            //User traking Code strated....
            //UserTraking us = new UserTraking();
            //us.PeopleId = User_Id + "";
            //us.Type = ac_Type;
            //us.LoginTime = DateTime.Now;
            //us.Remark = "login page ,";
            //dc.UserTrakings.Add(us);
            //dc.Commit();
            //END User traking Code....
            return Task.FromResult<object>(null);
        }
    }
}