using AngularJSAuthentication.API.DataContract;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;


namespace AngularJSAuthentication.API.Controllers.Permission
{
    [RoutePrefix("api/RolePagePermission")]
    public class RolePagePermissionController : System.Web.Http.ApiController
    {
        // GET: RolePagePermission
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //[Authorize]
        [Route("GetAllRole")]
        [HttpGet]

        public dynamic GetAllRole()
        {


            using (AuthContext db = new AuthContext())
            {
                var query = "select Id,[Name] from AspNetRoles where IsTemp=0";
                var RolePagess = db.Database.SqlQuery<RolePages>(query).OrderBy(x => x.Name).ToList();
                return RolePagess;
            }

        }
        [Route("GetAllRoleforRequestAccess")]
        [HttpGet]
        [Authorize]
        public dynamic GetAllRoleforRequestAccess()
        {
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

            using (AuthContext db = new AuthContext())
            {
                var query = "select Id,[Name] as label from AspNetRoles where id not in(select roleid from RequestAccesses ra inner join RequestRoles rr on ra.ReqId =rr.id where rr.peopleId ='" + userid + "' and rr.status =1 and rr.UacApproved =1)";
                var RolePagess = db.Database.SqlQuery<RolePagesAcess>(query).ToList();
                return RolePagess;
            }

        }

        [Route("GetPageRequests")]
        [HttpGet]
        public dynamic GetPageRequests(int reqId)
        {

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



            using (AuthContext db = new AuthContext())
            {
                var getidFromReq = db.requestAccess.Where(x => x.ReqId == reqId).Select(x => new { x.Id }).FirstOrDefault();
                var query = "select ar.name as RoleName,pm.PageName,pr.validFrom,pr.validTill from AspNetRoles ar inner join RequestAccesses ra on ar.Id = ra.roleId " +
                             " inner join PageRequests pr on pr.ReqId = ra.Id inner join PageMasters pm on pm.Id = pr.pageId where ra.peopleId = '" + userid + "' and pr.ReqId ='" + getidFromReq.Id + "'";

                var PendingReq = db.Database.SqlQuery<PageRequestDetails>(query).ToList();
                return PendingReq;
            }


        }

        [Route("GetOwnRequests")]
        [HttpGet]
        public dynamic GetOwnRequests()
        {

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



            using (AuthContext db = new AuthContext())
            {
                var query = "select  rr.id as Id,p.DisplayName,rr.validfrom,rr.validtill,(case when rr.status = 1 then 'Approved By Head' when rr.rejectStatus =1 then 'Rejected By Head' else 'Pending' end) as Statuses,"
                           + " (case when rr.UacApproved = 1 then 'Approved By UAC' when rr.uacrejectStatus =1 then 'Rejected By UAC' else 'Pending By UAC' end) as UACStatus,rr.status as Status, rr.UacApproved as UacApproved," +
                           "   rr.rejectReason as RejectReason,rr.rejctReasonUAC from requestroles rr inner join People p on rr.peopleid = p.PeopleID where rr.peopleid='" + userid + "'";

                var PendingReq = db.Database.SqlQuery<PendingRequest>(query).ToList();
                return PendingReq;
            }


        }


        [Route("GetPendingRequestForUAC")]
        [HttpGet]
        public dynamic GetPendingRequestForUAC()
        {

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



            using (AuthContext db = new AuthContext())
            {

                //var GetRportingP = db.Peoples.Where(x => x.PeopleID == userid).Select(x => new { x.ReportPersonId }).SingleOrDefault();
                //var GetReportingManager = db.Peoples.Where(x => x.PeopleID == GetRportingP.ReportPersonId).Select(x => new { x.ReportPersonId }).SingleOrDefault();
                //var FinalId = db.Peoples.Where(x => x.PeopleID == GetReportingManager.ReportPersonId).Select(x => new { x.PeopleID }).SingleOrDefault();
                string roles = "";
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                    roles = identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value;

                List<string> roleslst = new List<string>();
                if (!string.IsNullOrEmpty(roles))
                    roleslst = roles.Split(',').ToList();
                //var checkUser = db.Peoples.Where(x => x.PeopleID == userid).Select(x => new { x.Permissions }).FirstOrDefault();
                if (roleslst.Any(x => x == "HQ Master login"))
                {
                    var query = "select rr.Id, ar.Name as Role,p.DisplayName,(case when rr.status = 1 then 'Approved By Head' when rr.rejectStatus =1 then 'Rejected By Head' else 'Pending' end)  as Statuses, "
                                                   + " (case when rr.UacApproved = 1 then 'Approved By UAC' when rr.uacrejectStatus =1 then 'Rejected By UAC' else 'Pending By UAC' end) as UACStatus, rr.rejectReason,rr.rejctReasonUAC,rr.validFrom, "
                                                   + "  rr.validTill,rr.CreatedDate,rr.UpdatedDate from RequestAccesses ra inner join RequestRoles rr on ra.ReqId = rr.Id inner join AspNetRoles ar on ra.roleId = ar.id "
                                                   + "  inner join People P on rr.peopleId = p.PeopleID";
                    var PendingReq = db.Database.SqlQuery<PendingForheadRequest>(query).ToList();
                    return PendingReq;

                }

                return null;

            }


        }

        [Route("GetPendingRequest")]
        [HttpGet]
        public dynamic GetPendingRequest()
        {

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

            using (AuthContext db = new AuthContext())
            {

                //var GetRportingP = db.Peoples.Where(x => x.PeopleID == userid).Select(x => new { x.ReportPersonId }).SingleOrDefault();
                //var GetReportingManager = db.Peoples.Where(x => x.PeopleID == GetRportingP.ReportPersonId).Select(x => new { x.ReportPersonId }).SingleOrDefault();
                //var FinalId = db.Peoples.Where(x => x.PeopleID == GetReportingManager.ReportPersonId).Select(x => new { x.PeopleID }).SingleOrDefault();
                string roles = "";
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                    roles = identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value;

                List<string> roleslst = new List<string>();
                if (!string.IsNullOrEmpty(roles))
                    roleslst = roles.Split(',').ToList();

                //var checkUser = db.Peoples.Where(x => x.PeopleID == userid).Select(x => new { x.Permissions }).FirstOrDefault();
                if (roleslst.Any(x => x == "HQ Master login"))
                {
                    var query = "select rr.Id, ar.Name as Role,p.DisplayName,(case when rr.status = 1 then 'Approved By Head' when rr.rejectStatus =1 then 'Rejected By Head' else 'Pending' end) as Statuses, "
                                                   + " (case when rr.UacApproved = 1 then 'Approved By UAC' when rr.uacrejectStatus =1 then 'Rejected By UAC' else 'Pending By UAC' end) as UACStatus, rr.rejectReason,rr.rejctReasonUAC,rr.validFrom, "
                                                   + "  rr.validTill,rr.CreatedDate,rr.UpdatedDate from RequestAccesses ra inner join RequestRoles rr on ra.ReqId = rr.Id inner join AspNetRoles ar on ra.roleId = ar.id "
                                                   + "  inner join People P on rr.peopleId = p.PeopleID";
                    var PendingReq = db.Database.SqlQuery<PendingForheadRequest>(query).ToList();
                    return PendingReq;

                }
                else
                {

                    var GetRportingP = db.requestRoles.Where(x => x.status == false || x.status == true).Select(x => new { x.peopleId }).ToList();



                    foreach (var item in GetRportingP)
                    {

                        var getManagerid = db.Peoples.Where(x => x.PeopleID == item.peopleId).Select(x => new { x.ReportPersonId }).SingleOrDefault();

                        if (getManagerid.ReportPersonId > 0)
                        {

                            var MainReportingId = db.Peoples.Where(x => x.PeopleID == getManagerid.ReportPersonId).Select(x => new { x.ReportPersonId }).SingleOrDefault();

                            if (MainReportingId.ReportPersonId == userid)
                            {
                                var query = "select rr.Id, ar.Name as Role,p.DisplayName,(case when rr.status = 1 then 'Approved By Head' when rr.rejectStatus =1 then 'Rejected By Head' else 'Pending' end) as Statuses, "
                                   + " (case when rr.UacApproved = 1 then 'Approved By UAC' when rr.uacrejectStatus =1 then 'Rejected By UAC' else 'Pending By UAC' end) as UACStatus, rr.rejectReason,rr.rejctReasonUAC,rr.validFrom, "
                                   + "  rr.validTill,rr.CreatedDate,rr.UpdatedDate from RequestAccesses ra inner join RequestRoles rr on ra.ReqId = rr.Id inner join AspNetRoles ar on ra.roleId = ar.id "
                                   + "  inner join People P on rr.peopleId = p.PeopleID where  rr.peopleId='" + item.peopleId + "'";
                                var PendingReq = db.Database.SqlQuery<PendingForheadRequest>(query).ToList();
                                return PendingReq;
                            }

                        }
                    }








                }
                return null;

            }


        }

        [Route("GetWarehousePeople")]
        [HttpGet]

        public dynamic GetWarehousePeople(int WarehouseId)
        {


            using (AuthContext db = new AuthContext())
            {

                var peoplename = "select PeopleId,DisplayName from People where Deleted=0 and Active=1 and DisplayName is not null and WarehouseId='" + WarehouseId + "'";
                var displayname = db.Database.SqlQuery<DisplayName>(peoplename).ToList();


                return displayname;
            }

        }
        [Route("GetPeopleRoles")]
        [HttpGet]

        public dynamic GetPeopleRoles(string Id)
        {


            using (AuthContext db = new AuthContext())
            {
                var query = "select [Name] from AspNetRoles where id='" + Id + "'";
                var rolenme = db.Database.SqlQuery<RolePage>(query).SingleOrDefault();

                var peoplename = "select PeopleId,DisplayName from People where Deleted=0 and Active=1 and DisplayName is not null and [Permissions]='" + rolenme.Name + "'";
                var displayname = db.Database.SqlQuery<DisplayName>(peoplename).ToList().OrderBy(x => x.displayName).ToList();


                //var peopledata =db.Peoples.Where(x=>x.Department==RoleName)
                return displayname;
            }

        }


        #region Get All people 
        /// <summary>
        /// Created Date 03/08/2019
        /// </summary>
        /// <returns></returns>
        [Route("GetPeopleall")]
        [HttpGet]
        public dynamic GetPeopleall()
        {
            using (AuthContext db = new AuthContext())
            {
                var peoplename = "select PeopleId,DisplayName from People where Deleted=0 and Active=1 and DisplayName is not null";
                var displayname = db.Database.SqlQuery<DisplayName>(peoplename).ToList().OrderBy(x => x.displayName).ToList();
                //var peopledata =db.Peoples.Where(x=>x.Department==RoleName)
                return displayname;
            }

        }
        #endregion 

        [Route("GetAllPagesForDropDown")]
        [HttpGet]
        public IHttpActionResult GetAllPageButton(string Id)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            List<RolePagePermissionDc> rolePagePermissionDc = new List<RolePagePermissionDc>();
            List<RolePagePermissionDc> AllrolePagePermissionDc = new List<RolePagePermissionDc>();
            using (AuthContext context = new AuthContext())
            {
                //context.Database.Log = s => Debug.WriteLine(s);
                rolePagePermissionDc = context.PageMaster.Where(x => x.IsActive && !x.IsDeleted).Select(x =>
                 new RolePagePermissionDc
                 {
                     RolePageId = x.RolePagePermissions.Any(y => !y.IsDeleted && y.RoleId == Id) ? x.RolePagePermissions.FirstOrDefault(y => !y.IsDeleted && y.RoleId == Id).RolePageId : 0,
                     PageMasterId = x.Id,
                     PageName = x.PageName,
                     ParentPageId = x.ParentId,
                     Sequence = x.Sequence,
                     IsChecked = x.RolePagePermissions.Any(y => !y.IsDeleted && y.RoleId == Id) ? x.RolePagePermissions.FirstOrDefault(y => !y.IsDeleted && y.RoleId == Id).IsActive : false,
                 }).ToList();


                AllrolePagePermissionDc = rolePagePermissionDc.Where(x => !x.ParentPageId.HasValue).ToList();
                foreach (var item in AllrolePagePermissionDc)
                {
                    item.ChildRolePagePermissionDcs = rolePagePermissionDc.Where(x => x.ParentPageId.HasValue && x.ParentPageId.Value == item.PageMasterId).ToList();
                }
            }
            return Ok(AllrolePagePermissionDc);
        }


        [Route("SaveRolePageData")]
        [HttpPost]
        public IHttpActionResult SaveRolePageData(List<RolePagePermissionDc> rolePagePermissionDc, string roleId)
        {
            bool result = false;
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext context = new AuthContext())
            {

                List<RolePagePermissionDc> newrolePagePermissionDc = rolePagePermissionDc.Select(x => x).ToList();
                newrolePagePermissionDc.AddRange(rolePagePermissionDc.SelectMany(x => x.ChildRolePagePermissionDcs).ToList());
                List<long> existPageids = new List<long>();
                var RolePagePermission = context.RolePagePermission.Where(x => x.RoleId == roleId).ToList();

                foreach (var item in RolePagePermission)
                {
                    existPageids.Add(item.PageMasterId);
                    if (newrolePagePermissionDc != null && newrolePagePermissionDc.Any(x => x.PageMasterId == item.PageMasterId && x.IsChecked))
                    {
                        item.IsActive = true;
                        item.IsDeleted = false;
                        item.ModifiedDate = indianTime;
                        item.ModifiedBy = userid;
                    }
                    else
                    {
                        item.IsActive = false;
                        item.IsDeleted = false;
                        item.ModifiedDate = indianTime;
                        item.ModifiedBy = userid;
                    }
                    //context.ClusterAgent.Attach(item);
                    context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                }

                foreach (var item in newrolePagePermissionDc.Where(x => !existPageids.Any(y => y == x.PageMasterId)))
                {
                    Model.Permission.RolePagePermission rolespage = new Model.Permission.RolePagePermission
                    {
                        CreatedBy = userid,
                        CreatedDate = indianTime,
                        IsActive = item.IsChecked,
                        IsDeleted = false,
                        PageMasterId = item.PageMasterId,
                        RoleId = roleId
                    };
                    context.RolePagePermission.Add(rolespage);
                }

                result = context.Commit() > 0;
            }
            return Ok(result);
        }








        [Route("GetAllPeoplePage")]
        [HttpGet]
        public IHttpActionResult GetAllPeoplePage(int peopleId)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            string Roleids = "", Email = "";
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            List<string> lstRoleids = new List<string>();


            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<OverrideRolePagePermissionDc> rolePagePermissionDc = new List<OverrideRolePagePermissionDc>();
            List<OverrideRolePagePermissionDc> AllrolePagePermissionDc = new List<OverrideRolePagePermissionDc>();
            using (AuthContext context = new AuthContext())
            {
                Email = context.Peoples.FirstOrDefault(x => x.PeopleID == peopleId).Email;
                var query = "select STRING_AGG(a.RoleId,',') from AspNetUserRoles a inner join  AspNetUsers b on a.UserId=b.Id and b.Email='" + Email + "'  group by a.UserId";
                Roleids = context.Database.SqlQuery<string>(query).FirstOrDefault();

                if (!string.IsNullOrEmpty(Roleids))
                    lstRoleids = Roleids.Split(',').ToList();
                context.Database.Log = s => Debug.WriteLine(s);

                var OverrideRolePagePermissions = context.OverrideRolePagePermission.Where(x => !x.IsDeleted && x.PeopleId == peopleId).ToList();

                rolePagePermissionDc = context.PageMaster.Where(x => x.IsActive && !x.IsDeleted).Select(x =>
                 new OverrideRolePagePermissionDc
                 {
                     //PeopePageId = x.OverrideRolePagePermissions.Any(y => !y.IsDeleted && y.PeopleId == peopleId) ? x.OverrideRolePagePermissions.FirstOrDefault(y => !y.IsDeleted && y.PeopleId == peopleId).PeopePageId : 0,
                     PageMasterId = x.Id,
                     PageName = x.PageName,
                     ParentPageId = x.ParentId,
                     Sequence = x.Sequence,
                     IsChecked = x.RolePagePermissions.Any(y => !y.IsDeleted && lstRoleids.Contains(y.RoleId)) ? x.RolePagePermissions.FirstOrDefault(y => !y.IsDeleted && lstRoleids.Contains(y.RoleId)).IsActive : false,
                     //IsChecked = x.OverrideRolePagePermissions.Any(y => !y.IsDeleted && y.PeopleId == peopleId) ? x.OverrideRolePagePermissions.FirstOrDefault(y => !y.IsDeleted && y.PeopleId == peopleId).IsActive : (x.RolePagePermissions.Any(y => !y.IsDeleted && lstRoleids.Contains(y.RoleId)) ? x.RolePagePermissions.FirstOrDefault(y => !y.IsDeleted && lstRoleids.Contains(y.RoleId)).IsActive : false),
                 }).ToList();

                rolePagePermissionDc.ForEach(x =>
                {
                    x.IsChecked = OverrideRolePagePermissions.Any(y => y.PageMasterId == x.PageMasterId) ? OverrideRolePagePermissions.FirstOrDefault(y => y.PageMasterId == x.PageMasterId).IsActive : x.IsChecked;
                });

                AllrolePagePermissionDc = rolePagePermissionDc.Where(x => !x.ParentPageId.HasValue).ToList();
                foreach (var item in AllrolePagePermissionDc)
                {
                    item.OverrideRolePagePermissionDcs = rolePagePermissionDc.Where(x => x.ParentPageId.HasValue && x.ParentPageId.Value == item.PageMasterId).ToList();
                }
            }
            return Ok(AllrolePagePermissionDc);
        }


        [Route("SavePeoplePageData")]
        [HttpPost]
        public IHttpActionResult SavePeoplePageData(List<OverrideRolePagePermissionDc> overrideRolePagePermissionDcDc, int peopleId)
        {
            bool result = false;
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext context = new AuthContext())
            {

                List<OverrideRolePagePermissionDc> newrolePagePermissionDc = overrideRolePagePermissionDcDc.Select(x => x).ToList();
                newrolePagePermissionDc.AddRange(overrideRolePagePermissionDcDc.SelectMany(x => x.OverrideRolePagePermissionDcs).ToList());
                List<long> existPageids = new List<long>();
                var OverrideRolePagePermission = context.OverrideRolePagePermission.Where(x => x.PeopleId == peopleId).ToList();

                foreach (var item in OverrideRolePagePermission)
                {
                    existPageids.Add(item.PageMasterId);
                    if (newrolePagePermissionDc != null && newrolePagePermissionDc.Any(x => x.PageMasterId == item.PageMasterId && x.IsChecked))
                    {
                        item.IsActive = true;
                        item.IsDeleted = false;
                        item.ModifiedDate = indianTime;
                        item.ModifiedBy = userid;
                    }
                    else
                    {
                        item.IsActive = false;
                        item.IsDeleted = false;
                        item.ModifiedDate = indianTime;
                        item.ModifiedBy = userid;
                    }
                    //context.ClusterAgent.Attach(item);
                    context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                }

                foreach (var item in newrolePagePermissionDc.Where(x => !existPageids.Any(y => y == x.PageMasterId)))
                {
                    Model.Permission.OverrideRolePagePermission rolespage = new Model.Permission.OverrideRolePagePermission
                    {
                        CreatedBy = userid,
                        CreatedDate = indianTime,
                        IsActive = item.IsChecked,
                        IsDeleted = false,
                        PageMasterId = item.PageMasterId,
                        PeopleId = peopleId
                    };
                    context.OverrideRolePagePermission.Add(rolespage);
                }

                result = context.Commit() > 0;
            }
            return Ok(result);
        }


        [Route("UpdateStatus")]
        [HttpPut]
        public dynamic UpdateByHead(RequestRole objreq)
        {

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


            using (var context = new AuthContext())
            {


                RequestRole request = context.requestRoles.Where(x => x.Id == objreq.Id).FirstOrDefault();
                var GetUserName = context.Peoples.Where(x => x.PeopleID == userid).Select(x => new { x.DisplayName }).SingleOrDefault();

                if (request != null)
                {

                    request.UpdatedDate = indianTime;
                    request.UpdateBy = GetUserName.DisplayName;
                    request.status = true;
                    request.approverId = userid;
                    request.rejectReason = "";
                    context.requestRoles.Attach(request);
                    context.Entry(request).State = EntityState.Modified;
                    context.Commit();

                    var check = context.requestRoles.Where(x => x.Id == objreq.Id && x.UacApproved == true && x.status == true).FirstOrDefault();

                    if (check != null)
                    {
                        var getRoleId = "select ra.roleId from RequestAccesses ra inner join RequestRoles rr on ra.ReqId =rr.Id where  rr.id='" + objreq.Id + "'";
                        var rId = context.Database.SqlQuery<GetRoleId>(getRoleId).SingleOrDefault();


                        var query = "update AspNetUserRoles set IsActive=1,ModifiedDate='" + DateTime.Now.Date.ToString("yyyy-MM-dd") + "',ModifiedBy='" + GetUserName.DisplayName + "' where RoleId='" + rId.roleid + "'";
                        context.Database.ExecuteSqlCommand(query);


                    }


                    return objreq;

                }
                else
                {
                    return objreq;
                }





            }

        }

        [Route("RejectByHead")]
        [HttpPut]
        public dynamic RejectByHead(RequestRole objreq)
        {
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
            using (var context = new AuthContext())
            {


                RequestRole request = context.requestRoles.Where(x => x.Id == objreq.Id).FirstOrDefault();
                var GetUserName = context.Peoples.Where(x => x.PeopleID == userid).Select(x => new { x.DisplayName }).SingleOrDefault();

                if (request != null)
                {

                    request.UpdatedDate = indianTime;
                    request.UpdateBy = GetUserName.DisplayName;
                    request.status = false;
                    request.rejectStatus = true;
                    request.approverId = userid;
                    request.rejectReason = objreq.rejectReason;
                    context.requestRoles.Attach(request);
                    context.Entry(request).State = EntityState.Modified;
                    context.Commit();


                    return objreq;

                }
                else
                {
                    return objreq;
                }





            }

        }

        [Route("RejectByUAC")]
        [HttpPut]
        public dynamic RejectByUAC(RequestRole objreq)
        {
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
            using (var context = new AuthContext())
            {


                RequestRole request = context.requestRoles.Where(x => x.Id == objreq.Id).FirstOrDefault();
                var GetUserName = context.Peoples.Where(x => x.PeopleID == userid).Select(x => new { x.DisplayName }).SingleOrDefault();

                if (request != null)
                {

                    request.approvedDate = indianTime;
                    request.UpdateBy = GetUserName.DisplayName;
                    request.UacApproved = false;
                    request.uacrejectStatus = true;
                    request.UacId = userid;
                    request.rejctReasonUAC = objreq.rejctReasonUAC;
                    context.requestRoles.Attach(request);
                    context.Entry(request).State = EntityState.Modified;
                    context.Commit();


                    return objreq;

                }
                else
                {
                    return objreq;
                }





            }

        }





        [Route("UpdateStatusbyUAC")]
        [HttpPut]
        public dynamic UpdateStatusbyUAC(RequestRole objreq)
        {
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



            using (var context = new AuthContext())
            {


                RequestRole request = context.requestRoles.Where(x => x.Id == objreq.Id).FirstOrDefault();
                var GetUserName = context.Peoples.Where(x => x.PeopleID == userid).Select(x => new { x.DisplayName }).SingleOrDefault();

                if (request != null)
                {

                    request.approvedDate = indianTime;
                    request.UpdateBy = GetUserName.DisplayName;
                    request.UacApproved = true;
                    request.UacId = userid;
                    request.rejctReasonUAC = "";
                    context.requestRoles.Attach(request);
                    context.Entry(request).State = EntityState.Modified;
                    context.Commit();


                    var check = context.requestRoles.Where(x => x.Id == objreq.Id && x.UacApproved == true && x.status == true).FirstOrDefault();

                    if (check != null)
                    {
                        var getRoleId = "select ra.roleId from RequestAccesses ra inner join RequestRoles rr on ra.ReqId =rr.Id where  rr.id='" + objreq.Id + "'";
                        var rId = context.Database.SqlQuery<GetRoleId>(getRoleId).SingleOrDefault();


                        var query = "update AspNetUserRoles set IsActive=1,ModifiedDate='" + DateTime.Now.Date.ToString("yyyy-MM-dd") + "',ModifiedBy='" + GetUserName.DisplayName + "' where RoleId='" + rId.roleid + "'";
                        context.Database.ExecuteSqlCommand(query);


                    }


                    return objreq;
                }
                else
                {
                    return objreq;
                }

            }



        }

        #region DTO Class
        public class RolePage
        {
            public string Name { get; set; }
        }

        public class RolePages
        {
            public string Id { get; set; }
            public string Name { get; set; }

        }
        public class RolePagesAcess
        {
            public string Id { get; set; }
            public string label { get; set; }
        }

        public class PendingRequest
        {
            public int Id { get; set; }
            public string DisplayName { get; set; }
            public DateTime validfrom { get; set; }
            public DateTime validtill { get; set; }
            public string Statuses { get; set; }
            public string UACStatus { get; set; }
            public bool Status { get; set; }
            public bool UacApproved { get; set; }
            public string pageName { get; set; }

            public string RejectReason { get; set; }
            public string rejctReasonUAC { get; set; }

        }
        public class PendingForheadRequest
        {
            public int Id { get; set; }
            public string Role { get; set; }
            public string DisplayName { get; set; }
            public DateTime validfrom { get; set; }
            public DateTime validtill { get; set; }
            public string Statuses { get; set; }
            public string UACStatus { get; set; }
            public string rejectReason { get; set; }
            public string rejctReasonUAC { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? UpdatedDate { get; set; }
            //public bool Status { get; set; }
            //public bool UacApproved { get; set; }
            //public string pageName { get; set; }


        }
        public class PageRequestDetails
        {
            public int Id { get; set; }
            public string RoleName { get; set; }
            public string PageName { get; set; }
            public DateTime validfrom { get; set; }
            public DateTime validtill { get; set; }


        }
        public class DisplayName
        {
            public int PeopleId { get; set; }
            public string displayName { get; set; }
        }
        public class GetId
        {

            public string id { get; set; }
        }
        public class GetRoleId
        {
            public string roleid { get; set; }

        }

        #endregion
    }
}
