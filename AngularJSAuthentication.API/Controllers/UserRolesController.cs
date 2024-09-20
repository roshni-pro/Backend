using AngularJSAuthentication.API;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.Model.Store;
using AngularJSAuthentication.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.Controllers
{

    [RoutePrefix("api/usersroles")]
    public class UserRolesController : BaseApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private AuthRepository _repo = null;
        public UserRolesController()
        {
            _repo = new AuthRepository();
        }

        [Route("{id:guid}", Name = "GetRoleById")]
        public async Task<IHttpActionResult> GetRole(string Id)
        {
            var role = await this.AppRoleManager.FindByIdAsync(Id);
            if (role != null)
            {
                return Ok(TheModelFactory.Create(role));
            }
            return NotFound();
        }
        [Route("GetAllRoles")]
        public IHttpActionResult GetAllRoles()
        {
            var roles = this.AppRoleManager.Roles;
            return Ok(roles);
        }


        //[Authorize]
        [Route("GetAllUserRoles/{peopleId}")]
        public IHttpActionResult Get(int peopleId)
        {
            try
            {


                //logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                using (var context = new AuthContext())
                {
                    string sqlquery = "Exec GetUserAllRoles " + peopleId + "";
                    var UserRoleVM = context.Database.SqlQuery<RoleViewModel>(sqlquery).ToList();
                    return Ok(UserRoleVM);
                }

            }
            catch (Exception ex)
            {
                logger.Error("Error in Role " + ex.Message);
                logger.Info("End  Role: ");
                return null;
            }
        }

        [Route("UpdateRoles")]
        [HttpPost]
        public bool UpdateRoles(RoleComparatorViewModel vm)
        {
            using (var db = new AuthContext())
            {
                try
                {

                    //logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    if (vm.NewRoles != null)
                    {
                        List<RoleViewModel> newRoles = vm.NewRoles;
                        if (vm.OldRoles != null && vm.OldRoles.Any())
                        {
                            //var newRoles = vm.NewRoles.RemoveAll(x => !vm.OldRoles.Select(y => y.RoleID).Contains(x.RoleID));
                            newRoles = vm.NewRoles.Where(p => !vm.OldRoles.Any(x => x.RoleID == p.RoleID)).ToList();
                        }

                        //var manager = new ApplicationUserManager<ApplicationUser>(new UserStore<ApplicationUser>(new AuthContext()));

                        string connectionString = ConfigurationManager.ConnectionStrings["authcontext"].ToString();
                        if (newRoles != null && newRoles.Any())
                        {
                            //manager.AddToRoles(vm.NewRoles.First().SearchedUserId, vm.NewRoles.Select(x => x.RoleName).ToArray());


                            using (SqlConnection con = new SqlConnection(connectionString))
                            {
                                con.Open();

                                var roleIdDt = new DataTable();
                                roleIdDt.Columns.Add("stringValue");
                                foreach (var item in newRoles)
                                {
                                    var dr = roleIdDt.NewRow();
                                    dr["stringValue"] = item.RoleID;
                                    roleIdDt.Rows.Add(dr);
                                }

                                var param = new SqlParameter("RoleIds", roleIdDt);
                                param.SqlDbType = SqlDbType.Structured;
                                param.TypeName = "dbo.stringValues";

                                SqlCommand command = new SqlCommand("UpdateUserRoleNew", con);
                                command.CommandType = CommandType.StoredProcedure;
                                command.Parameters.Add(new SqlParameter("@UserId", vm.NewRoles.First().SearchedUserId));
                                command.Parameters.Add(new SqlParameter("@CreatedBy", 1));
                                command.Parameters.Add(new SqlParameter("@IsDeleted", false));
                                command.Parameters.Add(param);
                                int rowsAffected = command.ExecuteNonQuery();

                                con.Close();
                            }

                        }

                        vm.OldRoles = vm.OldRoles.Where(p => !vm.NewRoles.Any(x => x.RoleID == p.RoleID)).ToList();
                        //var oldRoles = vm.OldRoles.RemoveAll(x => !vm.NewRoles.Select(y => y.RoleID).Contains(x.RoleID));
                        if (vm.OldRoles != null && vm.OldRoles.Any())
                        {

                            using (SqlConnection con = new SqlConnection(connectionString))
                            {

                                con.Open();
                                var roleIdDt = new DataTable();
                                roleIdDt.Columns.Add("stringValue");
                                foreach (var item in vm.OldRoles)
                                {
                                    var dr = roleIdDt.NewRow();
                                    dr["stringValue"] = item.RoleID;
                                    roleIdDt.Rows.Add(dr);
                                }

                                var param = new SqlParameter("RoleIds", roleIdDt);
                                param.SqlDbType = SqlDbType.Structured;
                                param.TypeName = "dbo.stringValues";

                                SqlCommand command = new SqlCommand("UpdateUserRoleNew", con);
                                command.CommandType = CommandType.StoredProcedure;
                                command.Parameters.Add(new SqlParameter("@UserId", vm.OldRoles.First().SearchedUserId));
                                command.Parameters.Add(new SqlParameter("@CreatedBy", 1));
                                command.Parameters.Add(new SqlParameter("@IsDeleted", true));
                                command.Parameters.Add(param);
                                int rowsAffected = command.ExecuteNonQuery();
                                con.Close();

                            }
                            //   manager.RemoveFromRoles(vm.OldRoles.First().SearchedUserId, vm.OldRoles.Select(x => x.RoleName).ToArray());
                        }
                        var Userid = vm.OldRoles.Select(x => x.UserId).FirstOrDefault();
                        if(Userid == null)
                        {
                            Userid = vm.NewRoles.Select(x => x.UserId).FirstOrDefault();
                        }
                        
                        string query = "select p.PeopleID from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where  ur.UserId='" + Userid + "' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                        var PeopleId = db.Database.SqlQuery<int>(query).FirstOrDefault();

                        //var ExistingSalesExecutiveRole = vm.OldRoles.FirstOrDefault(x=>x.RoleName == "Sales Executive");
                        //var NewRole = vm.NewRoles.FirstOrDefault(x => x.RoleName == ExistingSalesExecutiveRole.RoleName) != null ? vm.NewRoles.FirstOrDefault(x => x.RoleName == ExistingSalesExecutiveRole.RoleName).RoleName : null;

                        var ExistingSalesExecutiveRole = vm.NewRoles.FirstOrDefault(x => x.RoleName == "Sales Executive" || x.RoleName == "Telecaller");


                        if (ExistingSalesExecutiveRole == null)
                        {
                            var updatecustomerexecutive = db.ClusterStoreExecutives.Where(x => x.ExecutiveId == PeopleId && x.IsDeleted == false && x.IsActive == true).ToList();
                            if (updatecustomerexecutive != null && updatecustomerexecutive.Count()>0)
                            {
                                foreach (var up in updatecustomerexecutive)
                                {
                                    up.IsActive = false;
                                    up.IsDeleted = true;
                                    up.ModifiedDate = DateTime.Now;
                                    up.ExecutiveId = PeopleId;
                                    db.Entry(up).State = EntityState.Modified;
                                }
                                var updatecustomerexecutivee = db.ClusterStoreExecutives.Where(x => x.ExecutiveId == PeopleId).ToList();
                                foreach (var upp in updatecustomerexecutivee)
                                {
                                    var data = db.ClusterStoreExecutiveHistories.Where(x => x.ClusterStoreExecutiveId == upp.Id).FirstOrDefault();
                                    if (data != null)
                                    {
                                        data.ClusterId = upp.ClusterId;
                                        data.StoreId = upp.StoreId;
                                        data.ExecutiveId = upp.ExecutiveId;
                                        data.EndDate = DateTime.Now;
                                        db.Entry(data).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        ClusterStoreExecutiveHistory clusterStoreExecutiveHistory = new ClusterStoreExecutiveHistory();
                                        clusterStoreExecutiveHistory.ClusterId = upp.ClusterId;
                                        clusterStoreExecutiveHistory.StoreId = upp.StoreId;
                                        clusterStoreExecutiveHistory.ExecutiveId = upp.ExecutiveId;
                                        clusterStoreExecutiveHistory.StartDate = DateTime.Now;
                                        clusterStoreExecutiveHistory.EndDate = DateTime.Now;
                                        clusterStoreExecutiveHistory.ClusterStoreExecutiveId = upp.Id;
                                        db.ClusterStoreExecutiveHistories.Add(clusterStoreExecutiveHistory);
                                    }
                                }
                                db.Commit();
                            }
                        }
                        
                    }
                    return true;

                }
                catch (Exception ex)
                {
                    logger.Error("Error in Role " + ex.Message);
                    logger.Info("End  Role: ");
                    return false;
                }
            }
              
        }




        [Route("create")]
        public async Task<IHttpActionResult> Create(CreateRoleBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = new IdentityRole { Name = model.Name };

            var result = await this.AppRoleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            Uri locationHeader = new Uri(Url.Link("GetRoleById", new { id = role.Id }));

            return Created(locationHeader, TheModelFactory.Create(role));

        }

        [Route("{id:guid}")]
        public async Task<IHttpActionResult> DeleteRole(string Id)
        {

            var role = await this.AppRoleManager.FindByIdAsync(Id);

            if (role != null)
            {
                IdentityResult result = await this.AppRoleManager.DeleteAsync(role);

                if (!result.Succeeded)
                {
                    return GetErrorResult(result);
                }
                return Ok();
            }
            return NotFound();
        }

        [Route("ManageUsersInRole")]
        public async Task<IHttpActionResult> ManageUsersInRole(UsersInRoleModel model)
        {
            var role = await this.AppRoleManager.FindByIdAsync(model.Id);

            if (role == null)
            {
                ModelState.AddModelError("", "Role does not exist");
                return BadRequest(ModelState);
            }

            foreach (string user in model.EnrolledUsers)
            {
                var appUser = await this.AppUserManager.FindByIdAsync(user);

                if (appUser == null)
                {
                    ModelState.AddModelError("", String.Format("User: {0} does not exists", user));
                    continue;
                }

                if (!this.AppUserManager.IsInRole(user, role.Name))
                {
                    IdentityResult result = await this.AppUserManager.AddToRoleAsync(user, role.Name);

                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", String.Format("User: {0} could not be added to role", user));
                    }

                }
            }

            foreach (string user in model.RemovedUsers)
            {
                var appUser = await this.AppUserManager.FindByIdAsync(user);

                if (appUser == null)
                {
                    ModelState.AddModelError("", String.Format("User: {0} does not exists", user));
                    continue;
                }

                IdentityResult result = await this.AppUserManager.RemoveFromRoleAsync(user, role.Name);

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", String.Format("User: {0} could not be removed from role", user));
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok();
        }


        [Route("GetAllPagePermission")]
        [HttpGet]
        public async Task<IHttpActionResult> GetPermission()
        {
            List<API.DataContract.PeoplePageDc> PeoplePageDc = new List<API.DataContract.PeoplePageDc>();
            var identity = User.Identity as ClaimsIdentity;
            string Roleids = ""; int userid = 0;
            List<string> lstRoleids = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Roleids"))
                Roleids = identity.Claims.FirstOrDefault(x => x.Type == "Roleids").Value.ToString();

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (!string.IsNullOrEmpty(Roleids))
                lstRoleids = Roleids.Split(',').ToList();
            PeoplePageDc = new PagePermissionManager().GetPeoplePage(userid, lstRoleids);
            return Ok(PeoplePageDc);
        }
    }
}
