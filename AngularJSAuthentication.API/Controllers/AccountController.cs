using AngularJSAuthentication.API.DataContract;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.API.Results;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.Transaction.RequestAccess;
using AngularJSAuthentication.Infrastructure;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Permission;
using AspNetIdentity.WebApi.Infrastructure;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Account")]
    public class AccountController : BaseApiController
    {
        private AuthRepository _repo = null;

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        public AccountController()
        {
            _repo = new AuthRepository();
        }
        public Logger logger = LogManager.GetCurrentClassLogger();
        // POST api/Account/Register





        [Route("rolessave")]
        [HttpPost]
        [Authorize]
        public async Task<int> rolesave(RoleRequestandUserRole objModel)

        {

            try
            {
                var rslt = 0;
                using (AuthContext context = new AuthContext())
                {


                    bool result = false;
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }


                    var querytogetPeopleId = context.Peoples.Where(x => x.PeopleID == userid).Select(x => x.Email).SingleOrDefault();
                    var peoplId = context.Peoples.Where(x => x.PeopleID == userid).Select(x => x.PeopleID).SingleOrDefault();
                    var username = context.Peoples.Where(x => x.PeopleID == peoplId).Select(x => x.DisplayName).SingleOrDefault();
                    var GetUIdquery = "select Id as Unid from AspNetUsers where Email = '" + querytogetPeopleId + "'";
                    var GetUnId = context.Database.SqlQuery<GetUniqueIdUser>(GetUIdquery).SingleOrDefault();

                    bool a = true;

                    var role = new ApplicationRole();
                    var roleId = new IdentityRole();
                    var roleM = new ApplicationUserRole();
                    var incnumber = "select count(id) + 2 as MaxId from AspNetRoles";
                    var getMx = context.Database.SqlQuery<GetMax>(incnumber).SingleOrDefault();


                    if (objModel.RoleRequest.First().PageId > 0 && a == true)
                    {
                        role = new ApplicationRole()
                        {
                            IsTemp = true,
                            CreatedDate = DateTime.Now.Date,
                            CreatedBy = username,
                            Name = "Temp Role-" + getMx.MaxId,
                            Id = roleId.Id

                        };


                        IdentityResult results = await this.AppRoleManager.CreateAsync(role);

                        rslt = await this.AppRoleManager.AddToRoleAsync(GetUnId.Unid, role.Id, userid, false, objModel.RoleRequest.First().validFrom, objModel.RoleRequest.First().validTo, username, false);
                        a = false;
                    }


                    if (String.IsNullOrEmpty(role.Name))
                    {
                        foreach (var item in objModel.SaveUsersRole)
                        {

                            rslt = await this.AppRoleManager.AddToRoleAsync(GetUnId.Unid, item.RoleId, userid, false, item.ValidFrom, item.ValidTo, username, false);

                        }
                    }


                    ApplicationUserRole objrole = new ApplicationUserRole();
                    RequestAccess objReqAccess = new RequestAccess();
                    RolePagePermission rolepage = new RolePagePermission();
                    RequestRole objrqRole = new RequestRole();
                    PageRequest objReqPage = new PageRequest();
                    objrqRole.peopleId = userid;
                    objrqRole.validFrom = objModel.RoleRequest.First().validFrom;
                    objrqRole.validTill = objModel.RoleRequest.First().validTo;
                    objrqRole.CreatedBy = username;
                    objrqRole.CreatedDate = DateTime.Now.Date;
                    context.requestRoles.Add(objrqRole);
                    context.Commit();
                    int id = objrqRole.Id;
                    int newId = 0;

                    if (objModel.RoleRequest.First().PageId > 0)
                    {

                        objReqAccess.peopleId = userid;
                        if (!String.IsNullOrEmpty(role.Name))
                        {
                            objReqAccess.roleId = roleId.Id;
                        }
                        else
                        {
                            objReqAccess.roleId = objModel.RoleRequest.First().RoleId;
                        }

                        objReqAccess.validFrom = objModel.RoleRequest.First().validFrom;
                        objReqAccess.validTill = objModel.RoleRequest.First().validTo;
                        objReqAccess.CreatedBy = username;
                        objReqAccess.CreatedDate = DateTime.Now.Date;
                        objReqAccess.ReqId = id;
                        context.requestAccess.Add(objReqAccess);
                        context.Commit();
                        newId = objReqAccess.Id;


                    }
                    else
                    {
                        foreach (var objj in objModel.RoleRequest)
                        {
                            objReqAccess.peopleId = userid;
                            if (!String.IsNullOrEmpty(role.Name))
                            {
                                objReqAccess.roleId = roleId.Id;
                            }
                            else
                            {
                                objReqAccess.roleId = objj.RoleId;
                            }

                            objReqAccess.validFrom = objj.validFrom;
                            objReqAccess.validTill = objj.validTo;
                            objReqAccess.CreatedBy = username;
                            objReqAccess.CreatedDate = DateTime.Now.Date;
                            objReqAccess.ReqId = id;
                            context.requestAccess.Add(objReqAccess);
                            context.Commit();
                            newId = objReqAccess.Id;
                        }



                    }


                    foreach (var obj in objModel.RoleRequest)
                    {




                        if (obj.PageId > 0)
                        {

                            objReqPage.peopleId = userid;
                            objReqPage.validFrom = obj.validFrom;
                            objReqPage.validTill = obj.validTo;
                            objReqPage.CreatedBy = username;
                            objReqPage.CreatedDate = DateTime.Now.Date;
                            objReqPage.pageId = obj.PageId;
                            objReqPage.ReqId = newId;
                            context.pageRequests.Add(objReqPage);
                            context.Commit();


                            var checkParentId = context.PageMaster.Where(x => x.Id == obj.PageId).Select(x => new { x.ParentId }).SingleOrDefault();
                            if (checkParentId.ParentId > 0)
                            {
                                var roole = "";
                                if (!String.IsNullOrEmpty(role.Name))
                                {
                                    rolepage.RoleId = roleId.Id;
                                    roole = roleId.Id;
                                }
                                else
                                {
                                    rolepage.RoleId = obj.RoleId;
                                    roole = obj.RoleId;
                                }

                                var checkExists = context.RolePagePermission.Where(x => x.RoleId == roole && x.PageMasterId == checkParentId.ParentId).SingleOrDefault();
                                if (checkExists == null)
                                {
                                    rolepage.PageMasterId = (long)checkParentId.ParentId;
                                    rolepage.CreatedDate = DateTime.Now.Date;
                                    rolepage.IsActive = true;
                                    rolepage.CreatedBy = userid;
                                    rolepage.IsDeleted = false;
                                    context.RolePagePermission.Add(rolepage);
                                    context.Commit();

                                }

                            }




                            if (!String.IsNullOrEmpty(role.Name))
                            {
                                rolepage.RoleId = roleId.Id;
                            }
                            else
                            {
                                rolepage.RoleId = obj.RoleId;
                            }
                            rolepage.PageMasterId = (long)obj.PageId;
                            rolepage.CreatedDate = DateTime.Now.Date;
                            rolepage.IsActive = true;
                            rolepage.CreatedBy = userid;
                            rolepage.IsDeleted = false;
                            context.RolePagePermission.Add(rolepage);
                            context.Commit();

                        }


                    }

                }
                return rslt;
            }


            catch (Exception ex)
            {

                return 0;
            }


        }








        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(UserModel userModel)
        {
            logger.Info("Get async: ");
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    string Username = null;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "username")
                        {
                            Username = (claim.Value);
                        }
                    }

                    byte Levels = 4;
                    userModel.UserName = userModel.PeopleFirstName + userModel.Mobile.Substring(userModel.Mobile.Length - 3);
                    var user = new ApplicationUser()
                    {
                        UserName = userModel.UserName,
                        Email = userModel.Email + "@shopKirana.com",
                        FirstName = userModel.PeopleFirstName,
                        LastName = userModel.PeopleLastName,
                        Level = Levels,
                        JoinDate = DateTime.Now.Date,
                        EmailConfirmed = true
                    };
                    //var role = new ApplicationRole()
                    //{
                    //    IsTemp = false,
                    //    CreatedDate = DateTime.Now.Date,
                    //    CreatedBy = userModel.UserName,
                    //    Name=userModel.UserName



                    //};
                    var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new AuthContext()));
                    var ExistsUser = manager.FindByName(userModel.UserName);
                    if (ExistsUser == null)
                    {
                        userModel.Email = userModel.Email + "@shopKirana.com";
                        int index1 = userModel.Email.IndexOf("@") + 1;
                        int index2 = userModel.Email.LastIndexOf(".");
                        int length = index2 - index1;
                        string company = userModel.Email.Substring(index1, length);

                        bool existingcompany = true;
                        existingcompany = context.CompanyExists(company);
                        Company c = null;
                        if (!existingcompany)
                        {
                            c = new Company();
                            c.CompanyName = userModel.CompanyName;
                            c.Address = userModel.Address;
                            c.CompanyPhone = userModel.CompanyPhone;
                            c.CompanyZip = userModel.CompanyZip;
                            c.EmployeesCount = userModel.Employees;
                            c.Name = company;
                            c = context.AddCompany(c);

                        }
                        //updated by sumit....
                        if (c == null)
                        {
                            c = context.Companies.Where(x => x.Name == company).FirstOrDefault();
                        }
                        userModel.DepartmentId = c.Id.ToString();
                        //   IdentityResult result = await _repo.RegisterUser(userModel);

                        IdentityResult result = await this.AppUserManager.CreateAsync(user, userModel.Password);





                        if (result.Succeeded)
                        {
                            var adminUser = manager.FindByName(userModel.UserName);//var adminUser = manager.FindByName(displayname);
                            manager.AddToRole(adminUser.Id, userModel.RoleName);



                            if (!result.Succeeded)
                            {
                                return GetErrorResult(result);
                            }

                            if (result.Errors.Count() > 0)
                            {
                                IHttpActionResult errorResult11 = GetErrorResult(result);
                                return errorResult11;
                            }
                            //People p = null;
                            //try
                            //{
                            // bool found = context.Peoples.Any(a => a.Email.Equals(userModel.Email) && a.CompanyId == c.Id && a.UserName == userModel.UserName);
                            // p = context.GetPeoplebyCompanyId(c.Id).Where(a => a.Email.Equals(userModel.Email)).SingleOrDefault();
                            //}
                            //catch (Exception ex) { }
                            //if (!found)
                            //{
                            People p = new People();
                            p.UserName = userModel.UserName;
                            p.Email = userModel.Email;
                            p.CompanyId = c.Id;
                            p.Password = userModel.Password;
                            p.SUPPLIERCODES = userModel.SUPPLIERCODES;
                            p.PeopleFirstName = userModel.PeopleFirstName;
                            p.PeopleLastName = userModel.PeopleLastName;
                            p.WarehouseId = userModel.WarehouseId;
                            p.Stateid = userModel.Stateid;
                            p.Cityid = userModel.Cityid;
                            p.Mobile = userModel.Mobile;
                            p.Department = userModel.Department;
                            p.Active = true;
                            p.Type = userModel.Department;
                            p.EmailConfirmed = true;
                            //p.Permissions = userModel.RoleName;
                            p.Skcode = userModel.Skcode;
                            p.Salesexecutivetype = userModel.Salesexecutivetype;
                            // NEw Employee Feilds...
                            p.Empcode = userModel.Empcode;
                            p.Desgination = userModel.Desgination;
                            p.Status = userModel.Status;
                            p.DOB = userModel.DOB;
                            p.DataOfJoin = userModel.DataOfJoin;
                            p.DataOfMarriage = userModel.DataOfMarriage;
                            p.EndDate = userModel.EndDate;
                            p.Unit = userModel.Unit;
                            //p.Salary = userModel.Salary;
                            p.Reporting = userModel.Reporting;
                            p.Account_Number = userModel.Account_Number;
                            p.IfscCode = userModel.IfscCode;
                            p.ReportPersonId = userModel.ReportPersonId;
                            //Created by Name
                            p.CreatedBy = Username;
                            p.id = userModel.id;
                            IHttpActionResult errorResult1 = GetErrorResult(result);
                            if (errorResult1 != null)
                            {
                                return errorResult1;
                            }
                            else
                            {
                                context.AddPeople(p);
                            }
                            // }
                            // Util.NotifyUsersForConfirmingRegistration(userModel.Email, userModel.Password);
                        }
                        else
                        {
                            logger.Error("User Not create due to password incorrect on Register Method");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting async " + ex.Message);
                }
            }
            logger.Info("end async ");
            return Ok();
        }

        // GET api/Account/ExternalLogin
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            string redirectUri = string.Empty;

            if (error != null)
            {
                return BadRequest(Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            var redirectUriValidationResult = ValidateClientAndRedirectUri(this.Request, ref redirectUri);

            if (!string.IsNullOrWhiteSpace(redirectUriValidationResult))
            {
                return BadRequest(redirectUriValidationResult);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            IdentityUser user = await _repo.FindAsync(new UserLoginInfo(externalLogin.LoginProvider, externalLogin.ProviderKey));

            bool hasRegistered = user != null;

            redirectUri = string.Format("{0}#external_access_token={1}&provider={2}&haslocalaccount={3}&external_user_name={4}",
                                            redirectUri,
                                            externalLogin.ExternalAccessToken,
                                            externalLogin.LoginProvider,
                                            hasRegistered.ToString(),
                                            externalLogin.UserName);

            return Redirect(redirectUri);

        }

        // POST api/Account/RegisterExternal
        [AllowAnonymous]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var verifiedAccessToken = await VerifyExternalAccessToken(model.Provider, model.ExternalAccessToken);
            if (verifiedAccessToken == null)
            {
                return BadRequest("Invalid Provider or External Access Token");
            }

            IdentityUser user = await _repo.FindAsync(new UserLoginInfo(model.Provider, verifiedAccessToken.user_id));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                return BadRequest("External user is already registered");
            }

            user = new IdentityUser() { UserName = model.UserName };

            IdentityResult result = await _repo.CreateAsync(user);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            var info = new ExternalLoginInfo()
            {
                DefaultUserName = model.UserName,
                Login = new UserLoginInfo(model.Provider, verifiedAccessToken.user_id)
            };

            result = await _repo.AddLoginAsync(user.Id, info.Login);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            //generate access token response
            var accessTokenResponse = GenerateLocalAccessTokenResponse(model.UserName);

            return Ok(accessTokenResponse);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ObtainLocalAccessToken")]
        public async Task<IHttpActionResult> ObtainLocalAccessToken(string provider, string externalAccessToken)
        {

            if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(externalAccessToken))
            {
                return BadRequest("Provider or external access token is not sent");
            }

            var verifiedAccessToken = await VerifyExternalAccessToken(provider, externalAccessToken);
            if (verifiedAccessToken == null)
            {
                return BadRequest("Invalid Provider or External Access Token");
            }

            IdentityUser user = await _repo.FindAsync(new UserLoginInfo(provider, verifiedAccessToken.user_id));

            bool hasRegistered = user != null;

            if (!hasRegistered)
            {
                return BadRequest("External user is not registered");
            }

            //generate access token response
            var accessTokenResponse = GenerateLocalAccessTokenResponse(user.UserName);

            return Ok(accessTokenResponse);

        }


        [Route("ChangePassword")]
        public async Task<IHttpActionResult> BackendUserUpdatePassword(UserModel userModel)
        {
            using (var db = new AuthContext())
            {
                string userName = userModel.UserName;
                var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new AuthContext()));

                var user = manager.FindByName(userName);
                if (user.PasswordHash != null)
                {
                    manager.RemovePassword(user.Id);
                }
                try
                {
                    manager.AddPassword(user.Id, userModel.Password);
                    People p = db.getPersonIdfromEmail(user.Email);
                    int UserId = p.PeopleID;
                    try
                    {

                        if (UserId > 0)
                        {
                            People people = db.Peoples.Where(x => x.PeopleID == UserId).FirstOrDefault();

                            people.Password = userModel.Password;
                            people.UpdatedDate = DateTime.Now;
                            db.Entry(people).State = EntityState.Modified;
                            db.Commit();
                        }
                    }
                    catch (Exception ss) { }


                }
                catch (Exception es)
                {
                }
            }
            return Ok();
        }

       

        [AllowAnonymous]
        [Route("RegisterV7")]
        public async Task<IHttpActionResult> RegisterV7(UserModel userModel)
        {
            logger.Info("Get async: ");
            string userID = null;
            PeopleOutputVM peopleOutputVm = new PeopleOutputVM();
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    string Username = null;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "username")
                        {
                            Username = (claim.Value);
                        }
                    }

                    byte Levels = 4;
                    //userModel.UserName = userModel.PeopleFirstName + userModel.Mobile.Substring(userModel.Mobile.Length - 3);
                    userModel.UserName = userModel.Email + "@shopKirana.com";
                    var user = new ApplicationUser()
                    {
                        UserName = userModel.UserName,
                        Email = userModel.Email + "@shopKirana.com",
                        FirstName = userModel.PeopleFirstName,
                        LastName = userModel.PeopleLastName,
                        Level = Levels,
                        JoinDate = DateTime.Now.Date,
                        EmailConfirmed = true
                    };
                    int mobilecount = 0;
                    if (!string.IsNullOrEmpty(userModel.Mobile))
                    {
                        string query = "select count(PeopleID) from People where Mobile='" + userModel.Mobile + "' and Active = 1 and Deleted=0";
                        mobilecount = context.Database.SqlQuery<int>(query).FirstOrDefault();
                    }
                        

                    int empCodecount = 0;
                    if (!string.IsNullOrEmpty(userModel.Empcode))
                    {
                        string EmpCodequery = "select count(PeopleID) from People where Empcode='" + userModel.Empcode + "' and Active = 1 and Deleted=0";
                        empCodecount = context.Database.SqlQuery<int>(EmpCodequery).FirstOrDefault();
                    }
                    

                    if (mobilecount == 0)
                    {
                        if (empCodecount==0)
                        {

                            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new AuthContext()));
                            var ExistsUser = manager.FindByName(userModel.UserName);
                            if (ExistsUser == null)
                            {
                                userModel.Email = userModel.Email + "@shopKirana.com";
                                int index1 = userModel.Email.IndexOf("@") + 1;
                                int index2 = userModel.Email.LastIndexOf(".");
                                int length = index2 - index1;
                                string company = userModel.Email.Substring(index1, length);

                                bool existingcompany = true;
                                existingcompany = context.CompanyExists(company);
                                Company c = null;
                                if (!existingcompany)
                                {
                                    c = new Company();
                                    c.CompanyName = userModel.CompanyName;
                                    c.Address = userModel.Address;
                                    c.CompanyPhone = userModel.CompanyPhone;
                                    c.CompanyZip = userModel.CompanyZip;
                                    c.EmployeesCount = userModel.Employees;
                                    c.Name = company;
                                    c = context.AddCompany(c);

                                }
                                //updated by sumit....
                                if (c == null)
                                {
                                    c = context.Companies.Where(x => x.Name == company).FirstOrDefault();
                                }
                                userModel.DepartmentId = c.Id.ToString();
                                //   IdentityResult result = await _repo.RegisterUser(userModel);
                                try
                                {
                                    IdentityResult result = await this.AppUserManager.CreateAsync(user, userModel.Password);
                                    if (result.Succeeded)
                                    {
                                        var adminUser = manager.FindByName(userModel.UserName);//var adminUser = manager.FindByName(displayname);
                                        userID = adminUser.Id;
                                        //People p = null;
                                        //try
                                        //{
                                        // bool found = context.Peoples.Any(a => a.Email.Equals(userModel.Email) && a.CompanyId == c.Id && a.UserName == userModel.UserName);
                                        // p = context.GetPeoplebyCompanyId(c.Id).Where(a => a.Email.Equals(userModel.Email)).SingleOrDefault();
                                        //}
                                        //catch (Exception ex) { }
                                        //if (!found)
                                        //{
                                        People p = new People();
                                        p.UserName = userModel.UserName;
                                        p.Email = userModel.Email;
                                        p.CompanyId = c.Id;
                                        p.Password = userModel.Password;
                                        p.SUPPLIERCODES = userModel.SUPPLIERCODES;
                                        p.PeopleFirstName = userModel.PeopleFirstName;
                                        p.PeopleLastName = userModel.PeopleLastName;
                                        p.WarehouseId = userModel.WarehouseId;
                                        p.Stateid = userModel.Stateid;
                                        p.Cityid = userModel.Cityid;
                                        p.city = userModel.Cityid > 0 ? context.Cities.FirstOrDefault(z => z.Cityid == userModel.Cityid).CityName : null;
                                        p.Mobile = userModel.Mobile;
                                        p.Department = userModel.Department;
                                        p.Active = userModel.Active;
                                        p.Type = userModel.Department;
                                        p.EmailConfirmed = true;
                                        //p.Permissions = userModel.RoleName;
                                        p.Skcode = userModel.Skcode;
                                        p.Salesexecutivetype = userModel.Salesexecutivetype;
                                        // NEw Employee Feilds...
                                        p.Empcode = userModel.Empcode;
                                        p.Desgination = userModel.Desgination;
                                        p.Status = userModel.Status;
                                        p.DOB = userModel.DOB;
                                        p.DataOfJoin = userModel.DataOfJoin;
                                        p.DataOfMarriage = userModel.DataOfMarriage;
                                        p.EndDate = userModel.EndDate;
                                        p.Unit = userModel.Unit;
                                        //p.Salary = userModel.Salary;
                                        p.Reporting = userModel.Reporting;
                                        p.Account_Number = userModel.Account_Number;
                                        p.IfscCode = userModel.IfscCode;
                                        p.ReportPersonId = userModel.ReportPersonId;
                                        p.Empcode = userModel.Empcode;
                                        //Created by Name
                                        p.CreatedBy = Username;
                                        p.id = userModel.id;

                                        IHttpActionResult errorResult1 = GetErrorResult(result);
                                        if (errorResult1 != null)
                                        {
                                            return errorResult1;
                                        }
                                        else
                                        {
                                            var newPeople = context.AddPeople(p);


                                            //PSalary ps = new PSalary();
                                            //ps.PeopleID = newPeople.PeopleID;
                                            //ps.Salary = userModel.Salary;
                                            //ps.B_Salary = userModel.B_Salary;
                                            //ps.Hra_Salary = userModel.Hra_Salary;
                                            //ps.CA_Salary = userModel.CA_Salary;
                                            //ps.DA_Salary = userModel.DA_Salary;
                                            //ps.Lta_Salary = userModel.Lta_Salary;
                                            //ps.PF_Salary = userModel.PF_Salary;
                                            //ps.ESI_Salary = userModel.ESI_Salary;
                                            //ps.M_Incentive = userModel.M_Incentive;
                                            //ps.Y_Incentive = userModel.Y_Incentive;
                                            //context.PeoplesSalaryDB.Add(ps);
                                            //context.SaveChanges();
                                            peopleOutputVm.Succeeded = result.Succeeded;
                                            peopleOutputVm.UserID = userID;
                                            peopleOutputVm.PeopleId = newPeople.PeopleID;
                                            return Ok(peopleOutputVm);
                                        }
                                        // }
                                        // Util.NotifyUsersForConfirmingRegistration(userModel.Email, userModel.Password);

                                    }

                                    else
                                    {
                                        logger.Error("User Not create due to password incorrect on Register Method");
                                        peopleOutputVm.ErrorMessage = result.Errors.FirstOrDefault();
                                        peopleOutputVm.Succeeded = result.Succeeded;
                                        peopleOutputVm.UserID = userID;
                                        return Ok(peopleOutputVm);
                                    }


                                }
                                catch (Exception e)
                                {
                                    peopleOutputVm.ErrorMessage = userModel.UserName + ' ' + "Already Exists";
                                    peopleOutputVm.Succeeded = false;
                                    return Ok(peopleOutputVm);
                                }
                            }
                            else
                            {
                                peopleOutputVm.ErrorMessage = userModel.UserName + ' ' + "Already Exists";
                                peopleOutputVm.Succeeded = false;
                                return Ok(peopleOutputVm);
                            }
                        }
                        else
                        {
                            peopleOutputVm.ErrorMessage = "Employee Code Already Exists";
                            peopleOutputVm.Succeeded = false;
                            return Ok(peopleOutputVm);
                        }

                    }
                    else
                    {

                        peopleOutputVm.ErrorMessage = "Mobile Number Already Exists";
                        peopleOutputVm.Succeeded = false;
                        return Ok(peopleOutputVm);

                    }
                }

                catch (Exception ex)
                {

                    logger.Error("Error in getting async " + ex.Message);
                    return InternalServerError();
                }

            }

            logger.Info("end async ");
            //return Ok(peopleOutputVm);

        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repo.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private string ValidateClientAndRedirectUri(HttpRequestMessage request, ref string redirectUriOutput)
        {

            Uri redirectUri;

            var redirectUriString = GetQueryString(Request, "redirect_uri");

            if (string.IsNullOrWhiteSpace(redirectUriString))
            {
                return "redirect_uri is required";
            }

            bool validUri = Uri.TryCreate(redirectUriString, UriKind.Absolute, out redirectUri);

            if (!validUri)
            {
                return "redirect_uri is invalid";
            }

            var clientId = GetQueryString(Request, "client_id");

            if (string.IsNullOrWhiteSpace(clientId))
            {
                return "client_Id is required";
            }

            var client = _repo.FindClient(clientId);

            if (client == null)
            {
                return string.Format("Client_id '{0}' is not registered in the system.", clientId);
            }

            if (!string.Equals(client.AllowedOrigin, redirectUri.GetLeftPart(UriPartial.Authority), StringComparison.OrdinalIgnoreCase))
            {
                return string.Format("The given URL is not allowed by Client_id '{0}' configuration.", clientId);
            }

            redirectUriOutput = redirectUri.AbsoluteUri;

            return string.Empty;

        }

        private string GetQueryString(HttpRequestMessage request, string key)
        {
            var queryStrings = request.GetQueryNameValuePairs();

            if (queryStrings == null) return null;

            var match = queryStrings.FirstOrDefault(keyValue => string.Compare(keyValue.Key, key, true) == 0);

            if (string.IsNullOrEmpty(match.Value)) return null;

            return match.Value;
        }

        private async Task<ParsedExternalAccessToken> VerifyExternalAccessToken(string provider, string accessToken)
        {
            ParsedExternalAccessToken parsedToken = null;

            var verifyTokenEndPoint = "";

            if (provider == "Facebook")
            {
                //You can get it from here: https://developers.facebook.com/tools/accesstoken/
                //More about debug_tokn here: http://stackoverflow.com/questions/16641083/how-does-one-get-the-app-access-token-for-debug-token-inspection-on-facebook
                var appToken = "xxxxxx";
                verifyTokenEndPoint = string.Format("https://graph.facebook.com/debug_token?input_token={0}&access_token={1}", accessToken, appToken);
            }
            else if (provider == "Google")
            {
                verifyTokenEndPoint = string.Format("https://www.googleapis.com/oauth2/v1/tokeninfo?access_token={0}", accessToken);
            }
            else
            {
                return null;
            }

            var client = new HttpClient();
            var uri = new Uri(verifyTokenEndPoint);
            var response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                dynamic jObj = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(content);

                parsedToken = new ParsedExternalAccessToken();

                if (provider == "Facebook")
                {
                    parsedToken.user_id = jObj["data"]["user_id"];
                    parsedToken.app_id = jObj["data"]["app_id"];

                    if (!string.Equals(Startup.facebookAuthOptions.AppId, parsedToken.app_id, StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                }
                else if (provider == "Google")
                {
                    parsedToken.user_id = jObj["user_id"];
                    parsedToken.app_id = jObj["audience"];

                    if (!string.Equals(Startup.googleAuthOptions.ClientId, parsedToken.app_id, StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }

                }

            }

            return parsedToken;
        }

        private JObject GenerateLocalAccessTokenResponse(string userName)
        {

            var tokenExpiration = TimeSpan.FromDays(1);

            ClaimsIdentity identity = new ClaimsIdentity(OAuthDefaults.AuthenticationType);

            identity.AddClaim(new Claim(ClaimTypes.Name, userName));
            identity.AddClaim(new Claim("role", "user"));

            var props = new AuthenticationProperties()
            {
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.Add(tokenExpiration),
            };

            var ticket = new AuthenticationTicket(identity, props);

            var accessToken = Startup.OAuthBearerOptions.AccessTokenFormat.Protect(ticket);

            JObject tokenResponse = new JObject(
                                        new JProperty("userName", userName),
                                          new JProperty("CompanyId", userName),
                                        new JProperty("access_token", accessToken),
                                        new JProperty("token_type", "bearer"),
                                        new JProperty("expires_in", tokenExpiration.TotalSeconds.ToString()),
                                        new JProperty(".issued", ticket.Properties.IssuedUtc.ToString()),
                                        new JProperty(".expires", ticket.Properties.ExpiresUtc.ToString())
        );

            return tokenResponse;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }
            public string ExternalAccessToken { get; set; }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer) || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name),
                    ExternalAccessToken = identity.FindFirstValue("ExternalAccessToken"),
                };
            }
        }


        [AllowAnonymous]
        [Route("GenerateWarehouseUser")]
        [HttpGet]
        public async Task<bool> GenerateWarehouseUser(int warehouseId, string role)
        {
            using (var context = new AuthContext())
            {
                string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.WarehouseId=" + warehouseId + " and p.Department='" + role + "' and ur.isActive=1 and p.Active=1 and p.Deleted=0";

                //string query = "select p.PeopleID from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.WarehouseId=" + warehouseId + " and Department='" + role + "' and active=1 and Deleted=0";
                //List<int> peopleIds = context.Database.SqlQuery<int>(query).ToList();
                //if (peopleIds == null || !peopleIds.Any())
                //    peopleIds = new List<int>();
                byte Levels = 4;
                var peoples = context.Database.SqlQuery<People>(query).ToList();
                // var peoples = context.Peoples.Where(x => x.WarehouseId == warehouseId && x.Department == role && !x.Deleted && x.Active && !peopleIds.Contains(x.PeopleID)).ToList();
                if (peoples != null && peoples.Any())
                {
                    foreach (var people in peoples)
                    {
                        var user = new ApplicationUser()
                        {
                            UserName = !string.IsNullOrEmpty(people.UserName) ? people.UserName : people.DisplayName.Replace(" ", ""),
                            Email = people.Email.Contains("@") ? people.Email : people.Email + "@shopKirana.com",
                            FirstName = people.PeopleFirstName,
                            LastName = people.PeopleLastName,
                            Level = Levels,
                            JoinDate = DateTime.Now.Date,
                            EmailConfirmed = true
                        };


                        var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new AuthContext()));
                        var ExistsUser = manager.FindByName(user.UserName);
                        if (ExistsUser == null)
                        {
                            var regex = @"(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{6,}";

                            Match match = Regex.Match(people.Password, regex, RegexOptions.IgnoreCase);
                            if (!match.Success)
                                people.Password = "Sk@123";

                            IdentityResult result = await this.AppUserManager.CreateAsync(user, people.Password);

                            if (result.Succeeded)
                            {
                                var adminUser = manager.FindByName(user.UserName);//var adminUser = manager.FindByName(displayname);
                                query = "Select count() from AspNetUserRoles where UserId='" + adminUser.Id + "'";
                                int isexistcount = context.Database.SqlQuery<int>(query).FirstOrDefault();
                                if (isexistcount == 0)
                                {
                                    query = "Insert Into AspNetUserRoles (userid,roleid,isprimary,CreatedDate,Createdby,isactive) Select '" + adminUser.Id + "',id,1,getdate(),1,1 from AspNetRoles where Name='" + role + "'";
                                    int i = context.Database.ExecuteSqlCommand(query);
                                }
                                //manager.AddToRole(adminUser.Id, role);
                            }
                            if (!match.Success)
                            {
                                query = "Update people set Password='" + people.Password + "' where Peopleid=" + people.PeopleID;
                                int i = context.Database.ExecuteSqlCommand(query);
                            }
                        }

                    }


                }

                return true;
            }
        }

        #endregion

        [Route("GetUserRole")]
        [HttpGet]
        [Authorize]
        public async Task<string> GetUserRole()
        {
            string rolesNames = string.Empty;
            using (AuthContext context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                string userid = "";

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "identityuserid"))
                    userid = identity.Claims.FirstOrDefault(x => x.Type == "identityuserid").Value;
                string sqlquery = "Exec GetUserRoles '" + userid + "'";
                var UserRoleDc = context.Database.SqlQuery<DataContracts.Masters.UserRoleDc>(sqlquery).ToList();

                var roleslsts = UserRoleDc != null && UserRoleDc.Any() ? UserRoleDc.Select(x => x.RoleName).ToList() : new List<string>();
                //rolesNames.Add(p.Permissions);
                rolesNames = string.Join(",", roleslsts.Distinct().ToList());
            }
            return rolesNames;
        }

        [HttpGet]
        [Route("CreateTradeWarehousePeople")]
        public async Task<bool> CreateTradeWarehousePeople(int customerId)
        {
            bool result = true;
            using (var context = new AuthContext())
            {
                var customers = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                if (customers != null && !string.IsNullOrEmpty(customers.Mobile) && customers.Cityid.HasValue)
                {
                    City city = context.Cities.Where(x => x.Cityid == customers.Cityid && x.Deleted == false).Select(x => x).FirstOrDefault();
                    State St = context.States.Where(x => x.Stateid == city.Stateid && x.Deleted == false).Select(x => x).FirstOrDefault();
                    Warehouse warehouse = context.Warehouses.FirstOrDefault(c => c.Phone == customers.Mobile && c.Deleted == false);

                    if (warehouse == null)
                    {
                        warehouse = new Warehouse();
                        TaxGroup Tg = context.DbTaxGroup.Where(x => x.GruopID == 4 && x.Deleted == false).Select(x => x).SingleOrDefault();
                        warehouse.GruopID = 4;
                        if (Tg != null)
                        {
                            warehouse.TGrpName = Tg.TGrpName;
                        }
                        else
                        {
                            warehouse.TGrpName = "Tax";
                        }

                        warehouse.WarehouseName = customers.ShopName;
                        warehouse.latitude = customers.lat;
                        warehouse.active = true;
                        warehouse.Address = customers.BillingAddress;
                        warehouse.Cityid = city.Cityid;
                        warehouse.CompanyId = 1;
                        warehouse.longitude = customers.lg;
                        warehouse.CompanyName = customers.ShopName;
                        warehouse.Createactive = false;
                        warehouse.Email = customers.Emailid;
                        warehouse.IsKPP = true;
                        warehouse.IsKppShowAsWH = true;
                        warehouse.Phone = customers.Mobile;
                        warehouse.CreatedBy = "Admin";
                        warehouse.CreatedDate = DateTime.Now;
                        warehouse.UpdatedDate = DateTime.Now;
                        warehouse.CityName = city?.CityName;
                        warehouse.StateName = St.StateName;
                        warehouse.Deleted = false;
                        context.Warehouses.Add(warehouse);
                        int id = context.Commit();
                        customers.KPPWarehouseId = warehouse.WarehouseId;
                        context.Entry(customers).State = EntityState.Modified;
                        context.Commit();
                    }
                    else
                    {
                        warehouse.Address = customers.BillingAddress;
                        warehouse.longitude = customers.lg;
                        warehouse.latitude = customers.lat;
                        warehouse.Email = customers.Emailid;
                        warehouse.Phone = customers.Mobile;
                        context.Entry(warehouse).State = EntityState.Modified;
                        customers.KPPWarehouseId = warehouse.WarehouseId;
                        context.Entry(customers).State = EntityState.Modified;
                        context.Commit();
                    }

                    var people = context.Peoples.FirstOrDefault(x => x.Mobile == customers.Mobile);
                    if (people == null)
                    {
                        people = new People();
                        byte Levels = 4;
                        var user = new ApplicationUser()
                        {
                            UserName = customers.Skcode + "@shopKirana.com",
                            Email = customers.Skcode + "@shopKirana.com",
                            FirstName = customers.Name,
                            LastName = customers.ShopName,
                            Level = Levels,
                            JoinDate = DateTime.Now.Date,
                            EmailConfirmed = true
                        };
                        IdentityResult userresult = await this.AppUserManager.CreateAsync(user, "Sk@12345");
                        string username = customers.Skcode + "@shopKirana.com";
                        string Email = customers.Skcode + "@shopKirana.com";
                        if (!userresult.Succeeded)
                        {
                            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new AuthContext()));
                            var adminUser = manager.FindByName(people.UserName);
                            if (adminUser.PasswordHash != null)
                            {
                                manager.RemovePassword(adminUser.Id);
                            }
                            manager.AddPassword(adminUser.Id, "Sk@12345");
                            username = adminUser.UserName;
                            Email = adminUser.Email;
                        }
                        people.DisplayName = customers.Name;
                        people.state = St?.StateName;
                        people.city = city?.CityName;
                        people.Cityid = city?.Cityid;
                        people.Stateid = St?.Stateid;
                        people.CreatedBy = "Admin";
                        people.CreatedDate = DateTime.Now;
                        people.UpdatedDate = DateTime.Now;
                        people.Active = true;
                        people.tempdel = false;
                        people.CompanyId = 1;
                        people.WarehouseId = warehouse.WarehouseId;
                        people.PeopleFirstName = customers.Name;
                        people.PeopleLastName = customers.ShopName;
                        people.Skcode = customers.Skcode;
                        people.Email = Email;
                        people.UserName = username;
                        people.Mobile = customers.Mobile;
                        people.Password = "Sk@12345";
                        context.Peoples.Add(people);
                        if (context.Commit() > 0)
                        {
                            context.WarehousePermissionDB.Add(new WarehousePermission
                            {
                                CreatedBy = 1,
                                CreatedDate = DateTime.Now,
                                IsActive = true,
                                IsDeleted = false,
                                PeopleID = people.PeopleID,
                                WarehouseId = warehouse.WarehouseId
                            });
                            context.Commit();
                        }


                        //}
                    }
                    else
                    {
                        var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new AuthContext()));
                        var adminUser = manager.FindByName(people.UserName);
                        if (adminUser.PasswordHash != null)
                        {
                            manager.RemovePassword(adminUser.Id);
                        }
                        var passresult = manager.AddPassword(adminUser.Id, "Sk@12345");
                        if (passresult.Succeeded)
                        {
                            people.Email = adminUser.Email;
                            people.UserName = adminUser.UserName;
                            people.Password = "Sk@12345";
                            context.Entry(people).State = EntityState.Modified;
                            context.Commit();
                        }

                        var peopleId = new SqlParameter
                        {
                            ParameterName = "peopleId",
                            Value = people.PeopleID
                        };
                        if (!context.WarehousePermissionDB.Any(x => x.WarehouseId == warehouse.WarehouseId && x.PeopleID == people.PeopleID && x.IsActive))
                        {
                            context.WarehousePermissionDB.Add(new WarehousePermission
                            {
                                CreatedBy = 1,
                                CreatedDate = DateTime.Now,
                                IsActive = true,
                                IsDeleted = false,
                                PeopleID = people.PeopleID,
                                WarehouseId = warehouse.WarehouseId
                            });
                        }
                        context.Commit();
                    }

                    #region assign role
                    if (!string.IsNullOrEmpty(people.UserName))
                    {
                        var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new AuthContext()));
                        var adminUser = manager.FindByName(people.UserName);

                        var peopleId = new SqlParameter
                        {
                            ParameterName = "peopleId",
                            Value = people.PeopleID
                        };
                        var userRoles = context.Database.SqlQuery<string>("CheckTradeUserRole @peopleId", peopleId).ToList();
                        if (userRoles != null && userRoles.Any())
                        {
                            var roleIdDt = new DataTable();
                            roleIdDt.Columns.Add("stringValue");
                            foreach (var item in userRoles)
                            {
                                var dr = roleIdDt.NewRow();
                                dr["stringValue"] = item;
                                roleIdDt.Rows.Add(dr);
                            }
                            var RoleIds = new SqlParameter("RoleIds", roleIdDt);
                            RoleIds.SqlDbType = SqlDbType.Structured;
                            RoleIds.TypeName = "dbo.stringValues";

                            var UserId = new SqlParameter
                            {
                                ParameterName = "UserId",
                                Value = adminUser.Id
                            };
                            var IsDeleted = new SqlParameter
                            {
                                ParameterName = "IsDeleted",
                                Value = false
                            };

                            var CreatedBy = new SqlParameter
                            {
                                ParameterName = "CreatedBy",
                                Value = 1
                            };

                            int i = context.Database.ExecuteSqlCommand("UpdateUserRoleNew @UserId,@RoleIds,@CreatedBy,@IsDeleted ", UserId, RoleIds, CreatedBy, IsDeleted);
                        }
                    }
                    #endregion
                }
                else
                {
                    result = false;
                }

            }

            return result;
        }


        [HttpPost]
        [Route("CreateDeliveryBoy")]
        public async Task<bool> CreateDeliveryBoy(UserModel userModel)
        {
            bool result = true;
            using (var context = new AuthContext())
            {
                userModel.UserName = userModel.Mobile + "@shopKirana.com";
                var people = context.Peoples.FirstOrDefault(x => x.Mobile == userModel.Mobile);

                if (people == null)
                {
                    people = new People();
                    people.UserName = userModel.UserName;
                    people.Email = userModel.UserName;
                    people.CompanyId = 1;
                    people.Password = userModel.Password;
                    people.SUPPLIERCODES = userModel.SUPPLIERCODES;
                    people.PeopleFirstName = userModel.PeopleFirstName;
                    people.PeopleLastName = userModel.PeopleLastName;
                    people.DisplayName = string.Format("{0} {1}", userModel.PeopleFirstName, userModel.PeopleLastName);
                    people.WarehouseId = userModel.WarehouseId;
                    people.Stateid = userModel.Stateid;
                    people.Cityid = userModel.Cityid;
                    people.Mobile = userModel.Mobile;
                    people.Department = userModel.Department;
                    people.Active = userModel.Active;
                    people.Type = userModel.Department;
                    people.EmailConfirmed = true;
                    people.Skcode = userModel.Skcode;
                    people.Salesexecutivetype = userModel.Salesexecutivetype;
                    people.Empcode = userModel.Empcode;
                    people.Desgination = userModel.Desgination;
                    people.Status = userModel.Status;
                    people.DOB = userModel.DOB;
                    people.DataOfJoin = userModel.DataOfJoin;
                    people.DataOfMarriage = userModel.DataOfMarriage;
                    people.EndDate = userModel.EndDate;
                    people.Unit = userModel.Unit;
                    people.Reporting = userModel.Reporting;
                    people.Account_Number = userModel.Account_Number;
                    people.IfscCode = userModel.IfscCode;
                    people.ReportPersonId = userModel.ReportPersonId;
                    people.Empcode = userModel.Empcode;
                    people.CreatedBy = userModel.Email;
                    people.CreatedDate = DateTime.Now;
                    people.UpdatedDate = DateTime.Now;

                    people.id = userModel.id;

                    context.Peoples.Add(people);
                    context.SaveChanges();
                }



                var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new AuthContext()));
                var ExistsUser = manager.FindByName(userModel.UserName);


                if (ExistsUser == null)
                {
                    byte Levels = 4;
                    ExistsUser = new ApplicationUser()
                    {
                        UserName = userModel.UserName,
                        Email = userModel.UserName,
                        FirstName = userModel.PeopleFirstName,
                        LastName = userModel.PeopleLastName,
                        Level = Levels,
                        JoinDate = DateTime.Now.Date,
                        EmailConfirmed = true
                    };
                    IdentityResult userresult = await this.AppUserManager.CreateAsync(ExistsUser, "Sk@12345");
                }
                else
                {
                    if (ExistsUser.PasswordHash != null)
                    {
                        manager.RemovePassword(ExistsUser.Id);
                    }
                    var chnagepasswordresult = manager.AddPassword(ExistsUser.Id, "Sk@12345");

                }



                people.UserName = ExistsUser.UserName;
                people.Email = ExistsUser.Email;
                people.Password = "Sk@12345";
                people.WarehouseId = userModel.WarehouseId;

                context.Entry(people).State = EntityState.Modified;
                context.SaveChanges();


                #region assign role


                if (ExistsUser != null)
                {
                    var mobile = new SqlParameter
                    {
                        ParameterName = "mobile",
                        Value = userModel.Mobile
                    };
                    var rolename = new SqlParameter
                    {
                        ParameterName = "rolename",
                        Value = "Delivery Boy"
                    };
                    int i = context.Database.ExecuteSqlCommand("AssignRoleToPeople @mobile,@rolename", mobile, rolename);
                    //string roleid = context.Database.SqlQuery<string>("select id from AspNetRoles where name = 'Delivery Boy").FirstOrDefault();
                    //var roleIdDt = new DataTable();
                    //roleIdDt.Columns.Add("stringValue");
                    //var dr = roleIdDt.NewRow();
                    //dr["stringValue"] = roleid;
                    //roleIdDt.Rows.Add(dr);

                    //var RoleIds = new SqlParameter("RoleIds", roleIdDt);
                    //RoleIds.SqlDbType = SqlDbType.Structured;
                    //RoleIds.TypeName = "dbo.stringValues";

                    //var UserId = new SqlParameter
                    //{
                    //    ParameterName = "UserId",
                    //    Value = ExistsUser.Id
                    //};
                    //var IsDeleted = new SqlParameter
                    //{
                    //    ParameterName = "IsDeleted",
                    //    Value = false
                    //};

                    //var CreatedBy = new SqlParameter
                    //{
                    //    ParameterName = "CreatedBy",
                    //    Value = 1
                    //};

                    //int i = context.Database.ExecuteSqlCommand("UpdateUserRoleNew @UserId,@RoleIds,@CreatedBy,@IsDeleted ", UserId, RoleIds, CreatedBy, IsDeleted);

                }

                #endregion


            }

            return result;
        }





        public async Task<bool> CreateSellerUser(UserModel userModel)
        {
            bool result = false;
            using (var context = new AuthContext())
            {
                userModel.UserName = userModel.Mobile;
                var people = context.Peoples.FirstOrDefault(x => x.Mobile == userModel.Mobile);
                if (people == null)
                {

                    people = new People();
                    people.UserName = userModel.UserName;
                    people.Email = userModel.Email;
                    people.CompanyId = 1;
                    people.Password = userModel.Password;
                    people.PeopleFirstName = userModel.PeopleFirstName;
                    people.PeopleLastName = userModel.PeopleLastName;
                    people.DisplayName = string.Format("{0} {1}", userModel.PeopleFirstName, userModel.PeopleLastName);
                    people.Stateid = userModel.Stateid;
                    people.Cityid = userModel.Cityid;
                    people.Mobile = userModel.Mobile;
                    people.Active = userModel.Active;
                    people.Type = userModel.Department;
                    people.EmailConfirmed = true;
                    people.Status = userModel.Status;
                    people.DOB = userModel.DOB;
                    people.DataOfJoin = userModel.DataOfJoin;
                    people.DataOfMarriage = userModel.DataOfMarriage;
                    people.EndDate = userModel.EndDate;
                    people.Unit = userModel.Unit;
                    people.Reporting = userModel.Reporting;
                    people.Account_Number = userModel.Account_Number;
                    people.IfscCode = userModel.IfscCode;
                    people.ReportPersonId = userModel.ReportPersonId;
                    people.Empcode = userModel.Empcode;
                    people.CreatedBy = userModel.Email;
                    people.CreatedDate = DateTime.Now;
                    people.UpdatedDate = DateTime.Now;
                    context.Peoples.Add(people);
                    context.Commit();
                }
                var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new AuthContext()));
                var ExistsUser = manager.FindByName(userModel.UserName);
                if (ExistsUser == null)
                {
                    byte Levels = 4;
                    ExistsUser = new ApplicationUser()
                    {
                        UserName = userModel.UserName,
                        Email = userModel.Email,
                        FirstName = userModel.PeopleFirstName,
                        LastName = userModel.PeopleLastName,
                        Level = Levels,
                        JoinDate = DateTime.Now.Date,
                        EmailConfirmed = true,
                        usertype = 1 // 1 for Seller
                    };
                    IdentityResult userresult = await this.AppUserManager.CreateAsync(ExistsUser, userModel.Password);
                }
                else
                {
                    if (ExistsUser.PasswordHash != null)
                    {
                        manager.RemovePassword(ExistsUser.Id);
                    }
                    var chnagepasswordresult = manager.AddPassword(ExistsUser.Id, userModel.Password);
                }
                people.UserName = ExistsUser.UserName;
                people.Email = ExistsUser.Email;
                people.Password = userModel.Password;
                people.WarehouseId = userModel.WarehouseId;
                context.Entry(people).State = EntityState.Modified;
                result = context.Commit() > 0;
                #region assign role
                if (ExistsUser != null)
                {
                    var mobile = new SqlParameter
                    {
                        ParameterName = "mobile",
                        Value = userModel.Mobile
                    };
                    var rolename = new SqlParameter
                    {
                        ParameterName = "rolename",
                        Value = "Seller"
                    };
                    int i = context.Database.ExecuteSqlCommand("AssignRoleToPeople @mobile,@rolename", mobile, rolename);
                }
                #endregion
            }
            return result;
        }



        public async Task<bool> CreatePeopleForDBoy(People userModel, ApplicationUser user)
        {
            bool result = true;
            using (var context = new AuthContext())
            {
                var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new AuthContext()));
                var ExistsUser = manager.FindByName(userModel.UserName);
                if (ExistsUser == null)
                {
                    IdentityResult userresult = await this.AppUserManager.CreateAsync(ExistsUser, "Sk@12345");
                }
                else
                {
                    if (ExistsUser.PasswordHash != null)
                    {
                        manager.RemovePassword(ExistsUser.Id);
                    }
                    var chnagepasswordresult = manager.AddPassword(ExistsUser.Id, "Sk@12345");

                }
                #region assign role
                if (ExistsUser != null)
                {
                    var mobile = new SqlParameter
                    {
                        ParameterName = "mobile",
                        Value = userModel.Mobile
                    };
                    var rolename = new SqlParameter
                    {
                        ParameterName = "rolename",
                        Value = "Delivery Boy"
                    };
                    int i = context.Database.ExecuteSqlCommand("AssignRoleToPeople @mobile,@rolename", mobile, rolename);
                }
                #endregion
            }

            return result;
        }

        [Route("PostDBoyMaster")]
        [HttpPost]
        public async Task<IHttpActionResult> RegisterDboyV7(DboyMaster obj)
        {

            logger.Info("Get async: ");
            string userID = null;
            PeopleOutputVM peopleOutputVm = new PeopleOutputVM();
            using (var context = new AuthContext())
            {

                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    string Username = null;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "username")
                        {
                            Username = (claim.Value);
                        }
                    }
                    if (obj.Id == 0)
                    {

                        var IsDuplicateBlock = context.DboyMasters.FirstOrDefault(x => x.IsDeleted == false && x.IsBlocked == true && (x.MobileNo == obj.MobileNo || x.AadharNo == obj.AadharNo));
                        if (IsDuplicateBlock != null)
                        {
                            peopleOutputVm.ErrorMessage = "your number/aadhar is blocked";
                            //res.Result = false;
                            return Ok(peopleOutputVm);
                        }
                        var IsDuplicate = context.DboyMasters.FirstOrDefault(x => x.IsDeleted == false && x.IsBlocked == false && (x.MobileNo == obj.MobileNo || x.AadharNo == obj.AadharNo));
                        if (IsDuplicate != null)
                        {
                            peopleOutputVm.ErrorMessage = "Dboy Already Registerd";
                            //res.Result = false;
                            return Ok(peopleOutputVm);
                        }
                    }

                    UserModel userModel = new UserModel();
                    userModel.UserName = obj.Name;
                    userModel.Email = obj.Name;
                    userModel.Password = "Sk@12345";
                    userModel.PeopleFirstName = obj.Name;
                    userModel.PeopleLastName = obj.Name;
                    userModel.WarehouseId = obj.WarehouseId;
                    userModel.Cityid = obj.CityId;
                    userModel.Department = "Delivery Boy";
                    userModel.Active = true;
                    userModel.Desgination = "Delivery Boy";
                    userModel.DOB = DateTime.Now;
                    userModel.DataOfJoin = DateTime.Now;
                    userModel.DataOfMarriage = DateTime.Now;
                    userModel.EndDate = DateTime.Now;
                    userModel.Account_Number = 000;
                    userModel.IfscCode = "";
                    userModel.ReportPersonId = null;
                    userModel.CA_Salary = 0;
                    userModel.CompanyZip = "";
                    userModel.CompanyName = "";
                    userModel.CompanyPhone = "";
                    userModel.Employees = 0;
                    userModel.PF_Salary = 0;
                    userModel.Hra_Salary = 0;
                    userModel.Mobile = obj.MobileNo;
                    byte Levels = 4;
                    //userModel.UserName = userModel.PeopleFirstName + userModel.Mobile.Substring(userModel.Mobile.Length - 3);
                    userModel.UserName = userModel.PeopleFirstName + "@shopKirana.com";
                    userModel.Email = userModel.PeopleFirstName;
                    string query = "select count(PeopleID) from People where Mobile='" + userModel.Mobile + "'";
                    int mobilecount = context.Database.SqlQuery<int>(query).FirstOrDefault();
                    if (mobilecount == 0)
                    {
                        var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new AuthContext()));
                        var ExistsUser = manager.FindByName(userModel.UserName);
                        if (ExistsUser == null)
                        {
                            userModel.Email = userModel.Email + "@shopKirana.com";
                            int index1 = userModel.Email.IndexOf("@") + 1;
                            int index2 = userModel.Email.LastIndexOf(".");
                            int length = index2 - index1;
                            string company = userModel.Email.Substring(index1, length);

                            bool existingcompany = true;
                            existingcompany = context.CompanyExists(company);
                            Company c = null;
                            if (!existingcompany)
                            {
                                c = new Company();
                                c.CompanyName = userModel.CompanyName;
                                c.Address = userModel.Address;
                                c.CompanyPhone = userModel.CompanyPhone;
                                c.CompanyZip = userModel.CompanyZip;
                                c.EmployeesCount = userModel.Employees;
                                c.Name = company;
                                c = context.AddCompany(c);

                            }
                            if (c == null)
                            {
                                c = context.Companies.Where(x => x.Name == company).FirstOrDefault();
                            }
                            userModel.DepartmentId = c.Id.ToString();
                            ExistsUser = new ApplicationUser()
                            {
                                UserName = userModel.UserName,
                                Email = userModel.PeopleFirstName + "@shopKirana.com",
                                FirstName = userModel.PeopleFirstName,
                                LastName = userModel.PeopleLastName,
                                Level = Levels,
                                JoinDate = DateTime.Now.Date,
                                EmailConfirmed = true,
                            };
                            try
                            {
                                IdentityResult result = await this.AppUserManager.CreateAsync(ExistsUser, userModel.Password);

                                if (result.Succeeded)
                                {
                                    var adminUser = manager.FindByName(userModel.UserName);//var adminUser = manager.FindByName(displayname);
                                    userID = adminUser.Id;

                                    People p = new People();
                                    p.UserName = userModel.UserName;
                                    p.Email = userModel.Email;
                                    p.CompanyId = c.Id;
                                    p.Password = userModel.Password;
                                    p.SUPPLIERCODES = userModel.SUPPLIERCODES;
                                    p.PeopleFirstName = userModel.PeopleFirstName;
                                    p.PeopleLastName = userModel.PeopleLastName;
                                    p.WarehouseId = userModel.WarehouseId;
                                    p.Stateid = userModel.Stateid;
                                    p.Cityid = userModel.Cityid;
                                    p.Mobile = userModel.Mobile;
                                    p.Department = userModel.Department;
                                    p.Active = userModel.Active;
                                    p.Type = userModel.Department;
                                    p.EmailConfirmed = true;
                                    //p.Permissions = userModel.RoleName;
                                    p.Skcode = userModel.Skcode;
                                    p.Salesexecutivetype = userModel.Salesexecutivetype;
                                    // NEw Employee Feilds...
                                    p.Empcode = userModel.Empcode;
                                    p.Desgination = userModel.Desgination;
                                    p.Status = userModel.Status;
                                    p.DOB = userModel.DOB;
                                    p.DataOfJoin = userModel.DataOfJoin;
                                    p.DataOfMarriage = userModel.DataOfMarriage;
                                    p.EndDate = userModel.EndDate;
                                    p.Unit = userModel.Unit;
                                    //p.Salary = userModel.Salary;
                                    p.Reporting = userModel.Reporting;
                                    p.Account_Number = userModel.Account_Number;
                                    p.IfscCode = userModel.IfscCode;
                                    p.ReportPersonId = userModel.ReportPersonId;
                                    p.Empcode = userModel.Empcode;
                                    //Created by Name
                                    p.CreatedBy = Username;
                                    p.id = new List<int>();
                                    p.id.Add(obj.WarehouseId);

                                    IHttpActionResult errorResult1 = GetErrorResult(result);
                                    if (errorResult1 != null)
                                    {
                                        return errorResult1;
                                    }
                                    else
                                    {
                                        var newPeople = context.AddPeople(p);
                                        peopleOutputVm.Succeeded = result.Succeeded;
                                        peopleOutputVm.UserID = userID;
                                        peopleOutputVm.PeopleId = newPeople.PeopleID;
                                        if (newPeople.PeopleID > 0)
                                        {
                                            if (ExistsUser != null)
                                            {
                                                var mobile = new SqlParameter
                                                {
                                                    ParameterName = "mobile",
                                                    Value = userModel.Mobile
                                                };
                                                var rolename = new SqlParameter
                                                {
                                                    ParameterName = "rolename",
                                                    Value = "Delivery Boy"
                                                };
                                                int i = context.Database.ExecuteSqlCommand("AssignRoleToPeople @mobile,@rolename", mobile, rolename);
                                            }
                                            //add Dboy master
                                            DboyMaster PostDboyData = new DboyMaster()
                                            {
                                                Name = obj.Name,
                                                Address = obj.Address,
                                                MobileNo = obj.MobileNo,
                                                AadharNo = obj.AadharNo,
                                                AadharCopy = obj.AadharCopy,
                                                AadharCopyBack = obj.AadharCopyBack,
                                                Photo = obj.Photo,
                                                Type = obj.Type,
                                                AgencyName = obj.AgencyName,
                                                AgentOrTransport = obj.AgentOrTransport,
                                                ValidFrom = obj.ValidFrom,
                                                ValidTill = obj.ValidTill,
                                                IsActive = obj.IsActive,
                                                IsDeleted = false,
                                                IsBlocked = obj.IsBlocked,
                                                CreatedBy = userid,
                                                CreatedDate = DateTime.Now,
                                                CityId = obj.CityId,
                                                AgentId = obj.AgentId,
                                                WarehouseId = obj.WarehouseId,
                                                PeopleId = peopleOutputVm.PeopleId,
                                                DboyCost = obj.DboyCost
                                                //TripTypeEnum = obj.TripTypeEnum
                                            };
                                            context.DboyMasters.Add(PostDboyData);
                                            if (context.Commit() > 0)
                                            {
                                                return Ok(peopleOutputVm);
                                            }
                                        }
                                        else
                                        {

                                        }
                                    }
                                }
                                else
                                {
                                    logger.Error("User Not create due to password incorrect on Register Method");
                                    peopleOutputVm.ErrorMessage = result.Errors.FirstOrDefault();
                                    peopleOutputVm.Succeeded = result.Succeeded;
                                    peopleOutputVm.UserID = userID;
                                    return Ok(peopleOutputVm);
                                }
                            }
                            catch (Exception e)
                            {
                                peopleOutputVm.ErrorMessage = userModel.UserName + ' ' + "Already Exists";
                                peopleOutputVm.Succeeded = false;
                                return Ok(peopleOutputVm);
                            }
                        }
                        else
                        {
                            peopleOutputVm.ErrorMessage = userModel.UserName + ' ' + "Already Exists";
                            peopleOutputVm.Succeeded = false;
                            return Ok(peopleOutputVm);
                        }
                    }
                    else
                    {
                        DboyMaster PostDboyData = new DboyMaster()
                        {
                            Name = obj.Name,
                            Address = obj.Address,
                            MobileNo = obj.MobileNo,
                            AadharNo = obj.AadharNo,
                            AadharCopy = obj.AadharCopy,
                            AadharCopyBack = obj.AadharCopyBack,
                            Photo = obj.Photo,
                            Type = obj.Type,
                            AgencyName = obj.AgencyName,
                            AgentOrTransport = obj.AgentOrTransport,
                            ValidFrom = obj.ValidFrom,
                            ValidTill = obj.ValidTill,
                            IsActive = obj.IsActive,
                            IsDeleted = false,
                            IsBlocked = obj.IsBlocked,
                            CreatedBy = userid,
                            CreatedDate = DateTime.Now,
                            CityId = obj.CityId,
                            AgentId = obj.AgentId,
                            WarehouseId = obj.WarehouseId,
                            DboyCost = obj.DboyCost,
                            PeopleId = context.Peoples.FirstOrDefault(x => x.Mobile == obj.MobileNo && x.Deleted == false && x.Active == true).PeopleID
                        };
                        context.DboyMasters.Add(PostDboyData);
                        if (context.Commit() > 0)
                        {
                            peopleOutputVm.ErrorMessage = "Dboy Saved Successfully";
                            peopleOutputVm.Succeeded = true;
                            return Ok(peopleOutputVm);

                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting async " + ex.Message);
                    return InternalServerError();
                }
            }
            logger.Info("end async ");
            return Ok(peopleOutputVm);
        }
        public async Task<int?> CreatePeopleOfDboy(DboyMaster DboyData, AuthContext context)
        {
            #region Create Dboy in People

            string query = "select PeopleID from People where Mobile='" + DboyData.MobileNo + "'";
            int? PeopleID = context.Database.SqlQuery<int?>(query).FirstOrDefault();
            if (PeopleID == null)
            {
                byte Levels = 4;
                People p = new People();
                p.UserName = DboyData.MobileNo;
                p.Password = "Sk@123";
                var user = new ApplicationUser()
                {
                    UserName = p.UserName,
                    Email = DboyData.MobileNo + "@" + DboyData.MobileNo + ".com",
                    FirstName = DboyData.Name,
                    LastName = "",
                    Level = Levels,
                    JoinDate = DateTime.Now.Date,
                    EmailConfirmed = true,
                };

                AccountController s = new AccountController();
                var UserId = s.CreatePeopleForDBoy(p, user);

                var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == DboyData.WarehouseId);
                p.Email = user.Email;
                p.PeopleFirstName = DboyData.Name;
                p.WarehouseId = warehouse.WarehouseId;
                p.Stateid = warehouse.Stateid;
                p.Cityid = warehouse.Cityid;
                p.Mobile = p.UserName;
                p.Active = true;
                p.EmailConfirmed = true;
                p.Empcode = "";
                p.Desgination = "Delivery Boy";
                p.DOB = DateTime.Now;
                p.DataOfJoin = DateTime.Now;
                p.CreatedDate = DateTime.Now;
                p.UpdatedDate = DateTime.Now;
                p.DisplayName = DboyData.Name;
                context.Peoples.Add(p);
                context.Commit();
                PeopleID = p.PeopleID;
            }
            #endregion
            return PeopleID;
        }


        //public async Task<IHttpActionResult> Create_Dboy_ForBackendOrder(CreateDboyBackendOrderDc obj, AuthContext context)
        //{
        //    PeopleOutputVM peopleOutputVm = new PeopleOutputVM();
        //    //using (var context = new AuthContext())
        //    {
        //        try
        //        {
        //            var IsDuplicate = context.Peoples.FirstOrDefault(x => x.Deleted == false && x.Mobile == obj.MobileNo && x.WarehouseId == obj.Warehouseid);
        //            if (IsDuplicate != null)
        //            {
        //                peopleOutputVm.ErrorMessage = "Dboy Already Registerd";
        //                //res.Result = false;
        //                return Ok(peopleOutputVm);
        //            }

        //            UserModel userModel = new UserModel();
        //            userModel.UserName = obj.Name;
        //            userModel.Email = obj.Name;
        //            userModel.Password = "Sk@12345";
        //            userModel.PeopleFirstName = obj.Name;
        //            userModel.PeopleLastName = obj.Name;
        //            userModel.WarehouseId = obj.Warehouseid;
        //            userModel.Cityid = obj.CityId;
        //            userModel.Department = "Delivery Boy";
        //            userModel.Active = true;
        //            userModel.Desgination = "Delivery Boy";
        //            userModel.DOB = DateTime.Now;
        //            userModel.DataOfJoin = DateTime.Now;
        //            userModel.DataOfMarriage = DateTime.Now;
        //            userModel.EndDate = DateTime.Now;
        //            userModel.Account_Number = 000;
        //            userModel.IfscCode = "";
        //            userModel.ReportPersonId = null;
        //            userModel.CA_Salary = 0;
        //            userModel.CompanyZip = "";
        //            userModel.CompanyName = "";
        //            userModel.CompanyPhone = "";
        //            userModel.Employees = 0;
        //            userModel.PF_Salary = 0;
        //            userModel.Hra_Salary = 0;
        //            userModel.Mobile = obj.MobileNo;
        //            byte Levels = 4;
        //            //userModel.UserName = userModel.PeopleFirstName + userModel.Mobile.Substring(userModel.Mobile.Length - 3);
        //            userModel.UserName = userModel.PeopleFirstName + "@shopKirana.com";
        //            userModel.Email = userModel.PeopleFirstName;
        //            string query = "select count(PeopleID) from People where Mobile='" + userModel.Mobile + "'";
        //            int mobilecount = context.Database.SqlQuery<int>(query).FirstOrDefault();
        //            if (mobilecount == 0)
        //            {
        //                var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new AuthContext()));
        //                var ExistsUser = manager.FindByName(userModel.UserName);
        //                if (ExistsUser == null)
        //                {
        //                    userModel.Email = userModel.Email + "@shopKirana.com";
        //                    int index1 = userModel.Email.IndexOf("@") + 1;
        //                    int index2 = userModel.Email.LastIndexOf(".");
        //                    int length = index2 - index1;
        //                    string company = userModel.Email.Substring(index1, length);

        //                    bool existingcompany = true;
        //                    existingcompany = context.CompanyExists(company);
        //                    Company c = null;
        //                    if (!existingcompany)
        //                    {
        //                        c = new Company();
        //                        c.CompanyName = userModel.CompanyName;
        //                        c.Address = userModel.Address;
        //                        c.CompanyPhone = userModel.CompanyPhone;
        //                        c.CompanyZip = userModel.CompanyZip;
        //                        c.EmployeesCount = userModel.Employees;
        //                        c.Name = company;
        //                        c = context.AddCompany(c);

        //                    }
        //                    if (c == null)
        //                    {
        //                        c = context.Companies.Where(x => x.Name == company).FirstOrDefault();
        //                    }
        //                    userModel.DepartmentId = c.Id.ToString();
        //                    ExistsUser = new ApplicationUser()
        //                    {
        //                        UserName = userModel.UserName,
        //                        Email = userModel.PeopleFirstName + "@shopKirana.com",
        //                        FirstName = userModel.PeopleFirstName,
        //                        LastName = userModel.PeopleLastName,
        //                        Level = Levels,
        //                        JoinDate = DateTime.Now.Date,
        //                        EmailConfirmed = true,
        //                        usertype = 0
        //                    };
        //                    try
        //                    {
        //                        //var managers = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new AuthContext()));

        //                        IdentityResult result = await manager.CreateAsync(ExistsUser, userModel.Password);

        //                        if (result.Succeeded)
        //                        {
        //                            People p = new People();
        //                            p.UserName = userModel.UserName;
        //                            p.Email = userModel.Email;
        //                            p.CompanyId = c.Id;
        //                            p.Password = userModel.Password;
        //                            p.SUPPLIERCODES = userModel.SUPPLIERCODES;
        //                            p.PeopleFirstName = userModel.PeopleFirstName;
        //                            p.PeopleLastName = userModel.PeopleLastName;
        //                            p.WarehouseId = userModel.WarehouseId;
        //                            p.Stateid = userModel.Stateid;
        //                            p.Cityid = userModel.Cityid;
        //                            p.Mobile = userModel.Mobile;
        //                            p.Department = userModel.Department;
        //                            p.Active = userModel.Active;
        //                            p.Type = userModel.Department;
        //                            p.EmailConfirmed = true;
        //                            p.DisplayName = string.Format("{0} {1}", userModel.PeopleFirstName, userModel.PeopleLastName);
        //                            //p.Permissions = userModel.RoleName;
        //                            p.Skcode = userModel.Skcode;
        //                            p.Salesexecutivetype = userModel.Salesexecutivetype;
        //                            // NEw Employee Feilds...
        //                            p.Empcode = userModel.Empcode;
        //                            p.Desgination = userModel.Desgination;
        //                            p.Status = userModel.Status;
        //                            p.DOB = userModel.DOB;
        //                            p.DataOfJoin = userModel.DataOfJoin;
        //                            p.DataOfMarriage = userModel.DataOfMarriage;
        //                            p.EndDate = userModel.EndDate;
        //                            p.Unit = userModel.Unit;
        //                            //p.Salary = userModel.Salary;
        //                            p.Reporting = userModel.Reporting;
        //                            p.Account_Number = userModel.Account_Number;
        //                            p.IfscCode = userModel.IfscCode;
        //                            p.ReportPersonId = userModel.ReportPersonId;
        //                            p.Empcode = userModel.Empcode;
        //                            //Created by Name
        //                            p.CreatedBy = obj.UserName;
        //                            p.id = new List<int>();
        //                            p.id.Add(obj.Warehouseid);
        //                            p.CreatedDate = DateTime.Now;
        //                            p.UpdatedDate = DateTime.Now;
        //                            using (var cntxt = new AuthContext())
        //                            {
        //                                cntxt.Peoples.Add(p);
        //                                int id = cntxt.Commit();

        //                                IHttpActionResult errorResult1 = GetErrorResult(result);
        //                                if (errorResult1 != null)
        //                                {
        //                                    return errorResult1;
        //                                }
        //                                else
        //                                {
        //                                    // var newPeople = BackendAddPeople(p, context);                                                                              
        //                                    int peopleId = p.PeopleID;
        //                                    // for people provide warehouse permission
        //                                    if (p.id.Count > 0)
        //                                    {
        //                                        var wp = new WarehousePermission();
        //                                        wp.WarehouseId = p.WarehouseId;
        //                                        wp.PeopleID = peopleId;
        //                                        wp.IsActive = true;
        //                                        wp.IsDeleted = false;
        //                                        wp.CreatedDate = DateTime.Now;
        //                                        wp.CreatedBy = p.PeopleID;
        //                                        cntxt.WarehousePermissionDB.Add(wp);
        //                                        cntxt.Commit();
        //                                    }

        //                                    peopleOutputVm.Succeeded = result.Succeeded;
        //                                    peopleOutputVm.UserID = obj.userid;
        //                                    peopleOutputVm.PeopleId = p.PeopleID;
        //                                    if (p.PeopleID > 0)
        //                                    {
        //                                        if (ExistsUser != null)
        //                                        {
        //                                            var mobile = new SqlParameter
        //                                            {
        //                                                ParameterName = "mobile",
        //                                                Value = userModel.Mobile
        //                                            };
        //                                            var rolename = new SqlParameter
        //                                            {
        //                                                ParameterName = "rolename",
        //                                                Value = "Delivery Boy"
        //                                            };
        //                                            int i = cntxt.Database.ExecuteSqlCommand("AssignRoleToPeople @mobile,@rolename", mobile, rolename);
        //                                        }
        //                                        if (cntxt.Commit() > 0)
        //                                        {
        //                                            return Ok(peopleOutputVm);
        //                                        }
        //                                    }
        //                                    else
        //                                    {

        //                                    }
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            logger.Error("User Not create due to password incorrect on Register Method");
        //                            peopleOutputVm.ErrorMessage = result.Errors.FirstOrDefault();
        //                            peopleOutputVm.Succeeded = result.Succeeded;
        //                            peopleOutputVm.UserID = obj.userid;
        //                            return Ok(peopleOutputVm);
        //                        }
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        peopleOutputVm.ErrorMessage = userModel.UserName + ' ' + "Already Exists";
        //                        peopleOutputVm.Succeeded = false;
        //                        return Ok(peopleOutputVm);
        //                    }
        //                }
        //                else
        //                {
        //                    peopleOutputVm.ErrorMessage = userModel.UserName + ' ' + "Already Exists";
        //                    peopleOutputVm.Succeeded = false;
        //                    return Ok(peopleOutputVm);
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error in getting async " + ex.Message);
        //            return InternalServerError();
        //        }
        //    }
        //    return Ok(peopleOutputVm);
        //}
        public async Task<IHttpActionResult> Create_Executive_ForBackendOrder(CreateDboyBackendOrderDc obj, AuthContext context)
        {
            PeopleOutputVM peopleOutputVm = new PeopleOutputVM();
            //using (var context = new AuthContext())
            {
                try
                {
                    var IsDuplicate = context.Peoples.FirstOrDefault(x => x.Deleted == false && x.Mobile == obj.MobileNo && x.WarehouseId == obj.Warehouseid);
                    if (IsDuplicate != null)
                    {
                        peopleOutputVm.ErrorMessage = "Executive Already Registerd";
                        //res.Result = false;
                        return Ok(peopleOutputVm);
                    }

                    UserModel userModel = new UserModel();
                    userModel.UserName = obj.Name;
                    userModel.Email = obj.Name;
                    userModel.Password = "Sk@12345";
                    userModel.PeopleFirstName = obj.Name;
                    userModel.PeopleLastName = obj.Name;
                    userModel.WarehouseId = obj.Warehouseid;
                    userModel.Cityid = obj.CityId;
                    userModel.Department = "Sales";
                    userModel.Active = true;
                    userModel.Desgination = "Sales Executive";
                    userModel.DOB = DateTime.Now;
                    userModel.DataOfJoin = DateTime.Now;
                    userModel.DataOfMarriage = DateTime.Now;
                    userModel.EndDate = DateTime.Now;
                    userModel.Account_Number = 000;
                    userModel.IfscCode = "";
                    userModel.ReportPersonId = null;
                    userModel.CA_Salary = 0;
                    userModel.CompanyZip = "";
                    userModel.CompanyName = "";
                    userModel.CompanyPhone = "";
                    userModel.Employees = 0;
                    userModel.PF_Salary = 0;
                    userModel.Hra_Salary = 0;
                    userModel.Mobile = obj.MobileNo;
                    byte Levels = 4;
                    //userModel.UserName = userModel.PeopleFirstName + userModel.Mobile.Substring(userModel.Mobile.Length - 3);
                    userModel.UserName = userModel.PeopleFirstName + "@shopKirana.com";
                    userModel.Email = userModel.PeopleFirstName;
                    string query = "select count(PeopleID) from People where Mobile='" + userModel.Mobile + "'";
                    int mobilecount = context.Database.SqlQuery<int>(query).FirstOrDefault();
                    if (mobilecount == 0)
                    {
                        var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new AuthContext()));
                        var ExistsUser = manager.FindByName(userModel.UserName);
                        if (ExistsUser == null)
                        {
                            userModel.Email = userModel.Email + "@shopKirana.com";
                            int index1 = userModel.Email.IndexOf("@") + 1;
                            int index2 = userModel.Email.LastIndexOf(".");
                            int length = index2 - index1;
                            string company = userModel.Email.Substring(index1, length);

                            bool existingcompany = true;
                            existingcompany = context.CompanyExists(company);
                            Company c = null;
                            if (!existingcompany)
                            {
                                c = new Company();
                                c.CompanyName = userModel.CompanyName;
                                c.Address = userModel.Address;
                                c.CompanyPhone = userModel.CompanyPhone;
                                c.CompanyZip = userModel.CompanyZip;
                                c.EmployeesCount = userModel.Employees;
                                c.Name = company;
                                c = context.AddCompany(c);

                            }
                            if (c == null)
                            {
                                c = context.Companies.Where(x => x.Name == company).FirstOrDefault();
                            }
                            userModel.DepartmentId = c.Id.ToString();
                            ExistsUser = new ApplicationUser()
                            {
                                UserName = userModel.UserName,
                                Email = userModel.PeopleFirstName + "@shopKirana.com",
                                FirstName = userModel.PeopleFirstName,
                                LastName = userModel.PeopleLastName,
                                Level = Levels,
                                JoinDate = DateTime.Now.Date,
                                EmailConfirmed = true,
                            };
                            try
                            {
                                IdentityResult result = await manager.CreateAsync(ExistsUser, userModel.Password);

                                if (result.Succeeded)
                                {
                                    var adminUser = manager.FindByName(userModel.UserName);


                                    People p = new People();
                                    p.UserName = userModel.UserName;
                                    p.Email = userModel.Email;
                                    p.CompanyId = c.Id;
                                    p.Password = userModel.Password;
                                    p.SUPPLIERCODES = userModel.SUPPLIERCODES;
                                    p.PeopleFirstName = userModel.PeopleFirstName;
                                    p.PeopleLastName = userModel.PeopleLastName;
                                    p.WarehouseId = userModel.WarehouseId;
                                    p.Stateid = userModel.Stateid;
                                    p.Cityid = userModel.Cityid;
                                    p.Mobile = userModel.Mobile;
                                    p.Department = userModel.Department;
                                    p.Active = userModel.Active;
                                    p.Type = userModel.Department;
                                    p.EmailConfirmed = true;
                                    p.DisplayName = string.Format("{0} {1}", userModel.PeopleFirstName, userModel.PeopleLastName);
                                    //p.Permissions = userModel.RoleName;
                                    p.Skcode = userModel.Skcode;
                                    p.Salesexecutivetype = userModel.Salesexecutivetype;
                                    // NEw Employee Feilds...
                                    p.Empcode = userModel.Empcode;
                                    p.Desgination = userModel.Desgination;
                                    p.Status = userModel.Status;
                                    p.DOB = userModel.DOB;
                                    p.DataOfJoin = userModel.DataOfJoin;
                                    p.DataOfMarriage = userModel.DataOfMarriage;
                                    p.EndDate = userModel.EndDate;
                                    p.Unit = userModel.Unit;
                                    //p.Salary = userModel.Salary;
                                    p.Reporting = userModel.Reporting;
                                    p.Account_Number = userModel.Account_Number;
                                    p.IfscCode = userModel.IfscCode;
                                    p.ReportPersonId = userModel.ReportPersonId;
                                    p.Empcode = userModel.Empcode;
                                    //Created by Name
                                    p.CreatedBy = obj.UserName;
                                    p.id = new List<int>();
                                    p.id.Add(obj.Warehouseid);
                                    p.CreatedDate = DateTime.Now;
                                    p.UpdatedDate = DateTime.Now;
                                    using (var cntxt = new AuthContext())
                                    {
                                        cntxt.Peoples.Add(p);
                                        int id = cntxt.Commit();
                                        IHttpActionResult errorResult1 = GetErrorResult(result);
                                        if (errorResult1 != null)
                                        {
                                            return errorResult1;
                                        }
                                        else
                                        {
                                            int peopleId = p.PeopleID;
                                            // for people provide warehouse permission
                                            if (p.id.Count > 0)
                                            {
                                                var wp = new WarehousePermission();
                                                wp.WarehouseId = p.WarehouseId;
                                                wp.PeopleID = peopleId;
                                                wp.IsActive = true;
                                                wp.IsDeleted = false;
                                                wp.CreatedDate = DateTime.Now;
                                                wp.CreatedBy = p.PeopleID;
                                                cntxt.WarehousePermissionDB.Add(wp);
                                                cntxt.Commit();
                                            }
                                            //var newPeople = BackendAddPeople(p, context);
                                            peopleOutputVm.Succeeded = result.Succeeded;
                                            peopleOutputVm.UserID = obj.userid;
                                            peopleOutputVm.PeopleId = peopleId;
                                            if (peopleId > 0)
                                            {
                                                if (ExistsUser != null)
                                                {
                                                    var mobile = new SqlParameter
                                                    {
                                                        ParameterName = "mobile",
                                                        Value = userModel.Mobile
                                                    };
                                                    var rolename = new SqlParameter
                                                    {
                                                        ParameterName = "rolename",
                                                        Value = "Sales Executive"
                                                    };
                                                    int i = context.Database.ExecuteSqlCommand("AssignRoleToPeople @mobile,@rolename", mobile, rolename);
                                                }
                                                if (context.Commit() > 0)
                                                {
                                                    return Ok(peopleOutputVm);
                                                }
                                            }
                                            else
                                            {

                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    logger.Error("User Not create due to password incorrect on Register Method");
                                    peopleOutputVm.ErrorMessage = result.Errors.FirstOrDefault();
                                    peopleOutputVm.Succeeded = result.Succeeded;
                                    peopleOutputVm.UserID = obj.userid;
                                    return Ok(peopleOutputVm);
                                }
                            }
                            catch (Exception e)
                            {
                                peopleOutputVm.ErrorMessage = userModel.UserName + ' ' + "Already Exists";
                                peopleOutputVm.Succeeded = false;
                                return Ok(peopleOutputVm);
                            }
                        }
                        else
                        {
                            peopleOutputVm.ErrorMessage = userModel.UserName + ' ' + "Already Exists";
                            peopleOutputVm.Succeeded = false;
                            return Ok(peopleOutputVm);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting async " + ex.Message);
                    return InternalServerError();
                }
            }
            return Ok(peopleOutputVm);
        }
    }
    public class RoleRequestandUserRole
    {

        public List<RolRequestDC> RoleRequest { get; set; }
        public List<RequestAccessDataViewModel> SaveUsersRole { get; set; }

    }
    public class GetMax
    {

        public int MaxId { get; set; }

    }

    public class PeopleOutputVM
    {
        public string UserID { get; set; }
        public string ErrorMessage { get; set; }
        public Boolean Succeeded { get; set; }
        public int PeopleId { get; set; }
    }

    public class CreateDboyBackendOrderDc
    {
        public int Warehouseid { get; set; }
        public int CityId { get; set; }
        public string userid { get; set; }
        public string UserName { get; set; }
        public string MobileNo { get; set; }
        public string Name { get; set; }
        public string ErrorMessage { get; set; }
        public Boolean Succeeded { get; set; }
        public int PeopleId { get; set; }
    }
}
