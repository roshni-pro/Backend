using AngularJSAuthentication.API.DataContract;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.Model.Permission;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.Permission
{
    [RoutePrefix("api/PageMaster")]
    public class PageMasterController : ApiController
    {
        //private AuthContext authContext;
        int compid = 0, userid = 0;
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public PageMasterController()
        {
            using (AuthContext context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
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
            }
        }

        [Route("List")]
        [HttpPost]
        public IHttpActionResult GetList(PermissionPaginatorViewModel pager)
        {
            using (var authContext = new AuthContext())
            {
                SqlParameter containsParam = new SqlParameter("Contains", pager.Contains);
                SqlParameter firstParam = new SqlParameter("First", pager.First);
                SqlParameter lastParam = new SqlParameter("Last", pager.Last);
                SqlParameter columnNameParam = new SqlParameter("ColumnName", pager.ColumnName);
                SqlParameter isAscendingParam = new SqlParameter("IsAscending", pager.IsAscending);
                object[] parameters = new object[] { containsParam, firstParam, lastParam, columnNameParam, isAscendingParam };

                var list = authContext.Database.SqlQuery<PageMasterViewModel>("PageMasterPaginator @Contains,@First,@Last,@ColumnName,@IsAscending", parameters).ToList();
                return Ok(list);
            }
        }


        [HttpGet]
        [Route("GetPermissionData/{id}")]
        public dynamic GetPermissionData(int id)
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    // Access claims
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
                    var query = "select PM.PageName,PM.Id as PageMasterID,BM.Id as ButtonMasterID,BM.ButtonName,PB.Id as PageButtonID"
                                + " from PageButtons PB Inner JOIN PageMasters PM ON PM.Id = PB.PageMasterId AND PM.IsActive = 1 AND PB.IsActive = 1"
                                + " LEFT JOIN ButtonMasters BM ON BM.Id = PB.ButtonMasterId where PM.id='" + id + "' AND BM.IsActive = 1";
                    var getd = db.Database.SqlQuery<PeoplePageAccessPermissionDTO>(query).ToList();
                    return getd;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }


        [Route("GetAllPagesForDropDown")]
        [HttpGet]
        public IHttpActionResult GetAllPagesForDropDown()
        {
            List<PageMasterDc> PageMasters = new List<PageMasterDc>();
            using (AuthContext context = new AuthContext())
            {
                PageMasters = context.PageMaster.Where(x => !string.IsNullOrEmpty(x.RouteName) && x.IsActive && !x.IsDeleted).ToList().Select(x => new PageMasterDc
                {
                    Id = x.Id,
                    PageName = x.PageName
                }).OrderBy(x => x.PageName).ToList();
            }
            return Ok(PageMasters);
        }







        [Route("GetAllParentPagesForDropDown")]
        [HttpGet]
        public IHttpActionResult GetAllParentPagesForDropDown()
        {
            List<PageMasterDc> PageMasters = new List<PageMasterDc>();
            using (AuthContext context = new AuthContext())
            {
                PageMasters = context.PageMaster.Where(x => !x.ParentId.HasValue && x.IsActive && !x.IsDeleted).ToList().Select(x => new PageMasterDc
                {
                    Id = x.Id,
                    PageName = x.PageName
                }).OrderBy(x => x.PageName).ToList();
            }
            return Ok(PageMasters);
        }

        [Route("GetAllPageButton")]
        [HttpGet]
        public IHttpActionResult GetAllPageButton(long pageMasterId)
        {
            List<ButtonMasterDc> PageMasters = new List<ButtonMasterDc>();
            using (AuthContext context = new AuthContext())
            {
                context.Database.Log = s => Debug.WriteLine(s);
                PageMasters = context.ButtonMaster.Where(x => x.IsActive && !x.IsDeleted).Select(x =>
                    new ButtonMasterDc
                    {
                        Id = x.Id,
                        ButtonName = x.ButtonName,
                        IsChecked = x.PageButtons.Any(y => y.PageMasterId == pageMasterId && y.IsActive && !y.IsDeleted)
                    }).OrderBy(x => x.ButtonName).ToList();

                //PageMasters = (from c in context.ButtonMaster.Where(x => x.IsActive && !x.IsDeleted)
                //               join p in context.PageButton.Where(x => x.IsActive && !x.IsDeleted && x.Id == pageMasterId)
                //               on c.Id equals p.ButtonMasterId into ps
                //               from p in ps.DefaultIfEmpty()
                //               select new ButtonMasterDc
                //               {
                //                   Id = c.Id,
                //                   ButtonName = c.ButtonName,
                //                   IsChecked = p == null || p.Id==0 ? false : true,
                //               }).ToList();
            }
            return Ok(PageMasters);
        }

        [Route("SavePageButton")]
        [HttpPost]
        public HttpResponseMessage SavePageButton(List<PageButtonPermissionDTO> pageButtonPermissionlist)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    foreach (var item in pageButtonPermissionlist)
                    {

                        var IsUpdate = context.PageButton.Where(x => x.ButtonMasterId == item.ButtonMasterId && x.PageMasterId == item.PageMasterID).FirstOrDefault();
                        if (IsUpdate != null)
                        {
                            IsUpdate.IsActive = item.IsActive;
                            IsUpdate.ModifiedBy = userid;
                            IsUpdate.ModifiedDate = DateTime.Now;
                            context.Commit();
                        }
                        else if (item.IsActive)
                        {
                            var pagebutton = new PageButton();
                            pagebutton.ButtonMasterId = item.ButtonMasterId;
                            pagebutton.PageMasterId = item.PageMasterID;
                            pagebutton.IsActive = item.IsActive;
                            pagebutton.CreatedDate = DateTime.Now;
                            pagebutton.CreatedBy = userid;
                            pagebutton.IsDeleted = false;
                            context.PageButton.Add(pagebutton);
                            context.Commit();
                        }
                        //return Request.CreateResponse(HttpStatusCode.OK, true);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, true);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, false);
                }
            }
        }


        [Route("SavePeoplePageButtonPermission")]
        [HttpPost]
        public HttpResponseMessage SavePeoplePageButtonPermission(List<PeoplePageButtonPermissionDTO> peoplePageButtonPermissionlist)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    foreach (var item in peoplePageButtonPermissionlist)
                    {
                        var IsUpdate = context.PeoplePageAccessPermission.Where(x => x.PageButtonId == item.PageButtonId && x.PeopleId == item.PeopleId).FirstOrDefault();
                        if (IsUpdate != null)
                        {
                            IsUpdate.IsActive = item.IsActive;
                            IsUpdate.ModifiedBy = userid;
                            IsUpdate.ModifiedDate = DateTime.Now;
                            context.Commit();
                        }
                        else
                        {
                            var peoplepagebutton = new PeoplePageAccessPermission();
                            peoplepagebutton.PageButtonId = item.PageButtonId;
                            peoplepagebutton.PeopleId = item.PeopleId;
                            peoplepagebutton.IsActive = item.IsActive;
                            peoplepagebutton.CreatedDate = DateTime.Now;
                            peoplepagebutton.CreatedBy = userid;
                            peoplepagebutton.IsDeleted = false;
                            context.PeoplePageAccessPermission.Add(peoplepagebutton);
                            context.Commit();
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, true);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, false);
                }
            }
        }

        [Route("GetPageButtonAccess")]
        [HttpGet]
        public IHttpActionResult GetPageButtonAccess(long peopleId, long pageMasterId)
        {
            List<PeoplePageAccessPermissionDc> PeoplePageAccessPermissionDcs = new List<PeoplePageAccessPermissionDc>();
            using (AuthContext context = new AuthContext())
            {

                PeoplePageAccessPermissionDcs = context.PageButton.Where(x => x.IsActive && !x.IsDeleted && x.PageMasterId == pageMasterId).Select(p =>
                  new PeoplePageAccessPermissionDc
                  {
                      Id = p.ButtonMasterId,
                      ButtonName = p.ButtonMaster.ButtonName,
                      IsActive = p.PeoplePageAccessPermissions.Any(y => y.IsActive && !y.IsDeleted && y.PeopleId == peopleId),
                      IsDeleted = !p.PeoplePageAccessPermissions.Any(y => y.IsActive && !y.IsDeleted && y.PeopleId == peopleId),
                      PageButtonId = p.Id,
                      PeopleId = peopleId
                  }).ToList();
            }
            return Ok(PeoplePageAccessPermissionDcs);
        }


        [Route("GetAllParentPages")]
        [HttpGet]
        public IHttpActionResult GetAllParentPages()
        {
            List<PageMasterDc> PageMasters = new List<PageMasterDc>();
            using (AuthContext context = new AuthContext())
            {
                PageMasters = context.PageMaster.Where(x => !x.ParentId.HasValue && x.IsActive && !x.IsDeleted).ToList().Select(x => new PageMasterDc
                {
                    Id = x.Id,
                    PageName = x.PageName,
                    Sequence = x.Sequence,
                    ClassName = x.ClassName,
                    IconClassName = x.IconClassName,
                    RouteName = x.RouteName
                }).OrderBy(x => x.Sequence).ToList();
            }
            return Ok(PageMasters);
        }


        [Route("UpdatePageSequence")]
        [HttpPost]
        public IHttpActionResult UpdatePageSequence(List<PageMasterDc> pageMasterDcs, long? parentPageid)
        {
            bool result = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<PageMasterDc> NewPageMasterDcs = new List<PageMasterDc>();
            List<PageMaster> PageMasters = new List<PageMaster>();
            using (AuthContext context = new AuthContext())
            {
                if (!parentPageid.HasValue)
                {
                    PageMasters = context.PageMaster.Where(x => !x.ParentId.HasValue && x.IsActive && !x.IsDeleted).ToList();
                }
                else
                {
                    PageMasters = context.PageMaster.Where(x => x.ParentId.HasValue && x.ParentId.Value == parentPageid && x.IsActive && !x.IsDeleted).ToList();
                }
                foreach (var page in PageMasters)
                {
                    page.Sequence = pageMasterDcs.Any(x => x.Id == page.Id) ? pageMasterDcs.FirstOrDefault(x => x.Id == page.Id).Sequence : page.Sequence;
                    page.ModifiedBy = userid;
                    page.ModifiedDate = indianTime;
                    context.Entry(page).State = EntityState.Modified;
                }
                result = context.Commit() > 0;
            }

            return Ok(result);
        }

        [Route("GetAllChildPages")]
        [HttpGet]
        public IHttpActionResult GetAllChildPages(long pageMasterId)
        {
            List<PageMasterDc> PageMasters = new List<PageMasterDc>();
            using (AuthContext context = new AuthContext())
            {
                PageMasters = context.PageMaster.Where(x => x.ParentId.HasValue && x.ParentId.Value == pageMasterId && x.IsActive && !x.IsDeleted).ToList().Select(x => new PageMasterDc
                {
                    Id = x.Id,
                    PageName = x.PageName,
                    ParentId = x.ParentId,
                    Sequence = x.Sequence,
                    ClassName = x.ClassName,
                    IconClassName = x.IconClassName,
                    RouteName = x.RouteName,
                    IsNewPortalUrl = x.IsNewPortalUrl,
                    IsGroup2PortalUrl = x.IsGroup2PortalUrl
                }).OrderBy(x => x.Sequence).ToList();
            }
            return Ok(PageMasters);
        }

        [Route("AddPageMaster")]
        [HttpPost]
        public bool AddPageMaster(PageMasterDc PageMaster)
        {

            bool result = false;
            logger.Info("start Add Page: ");
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                using (AuthContext db = new AuthContext())
                {
                    int sequence = 0;
                    if (PageMaster.ParentId.HasValue)
                        sequence = db.PageMaster.Where(x => x.ParentId.Value == PageMaster.ParentId.Value).Count() + 1;
                    else
                        sequence = db.PageMaster.Where(x => !x.ParentId.HasValue).Count() + 1;

                    PageMaster dm = new PageMaster();

                    dm.PageName = PageMaster.PageName;
                    dm.RouteName = PageMaster.RouteName;
                    dm.ClassName = PageMaster.ClassName;
                    dm.IconClassName = PageMaster.IconClassName;
                    dm.ParentId = PageMaster.Id;
                    dm.Sequence = sequence;
                    dm.IsActive = true;
                    dm.IsDeleted = false;
                    dm.CreatedDate = DateTime.Now;
                    dm.CreatedBy = userid;
                    dm.IsNewPortalUrl = PageMaster.IsNewPortalUrl;
                    dm.IsGroup2PortalUrl = PageMaster.IsGroup2PortalUrl;
                    db.Entry(dm).State = EntityState.Added;
                    result = db.Commit() > 0;
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error("Error in Add Page " + ex.Message);
                return false;
            }

        }

        [Route("SavePageMaster")]
        [HttpPost]
        public bool SavePageMaster(PageMasterDc PageMaster)
        {

            bool result = false;
            logger.Info("start Add Page: ");
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                using (AuthContext db = new AuthContext())
                {
                    if (PageMaster.Id != 0)
                    {
                        var update = db.PageMaster.Where(x => x.Id == PageMaster.Id).FirstOrDefault();
                        if (update != null)
                        {
                            update.PageName = PageMaster.PageName;
                            update.RouteName = PageMaster.RouteName;
                            update.ClassName = PageMaster.ClassName;
                            update.IconClassName = PageMaster.IconClassName;
                            update.IsNewPortalUrl = PageMaster.IsNewPortalUrl;
                            update.IsGroup2PortalUrl = PageMaster.IsGroup2PortalUrl;
                            update.ModifiedDate = DateTime.Now;
                            update.ModifiedBy = userid;
                            db.Entry(update).State = EntityState.Modified;
                            result = db.Commit() > 0;
                        }
                    }
                    else
                    {
                        int sequence = 0;
                        if (PageMaster.ParentId.HasValue)
                            sequence = db.PageMaster.Where(x => x.ParentId.Value == PageMaster.ParentId.Value).Count() + 1;
                        else
                            sequence = db.PageMaster.Where(x => !x.ParentId.HasValue).Count() + 1;

                        PageMaster dm = new PageMaster();

                        dm.PageName = PageMaster.PageName;
                        dm.RouteName = PageMaster.RouteName;
                        dm.ClassName = PageMaster.ClassName;
                        dm.IconClassName = PageMaster.IconClassName;
                        dm.ParentId = PageMaster.ParentId;
                        dm.IsNewPortalUrl = PageMaster.IsNewPortalUrl;
                        dm.IsGroup2PortalUrl = PageMaster.IsGroup2PortalUrl;

                        dm.Sequence = sequence;
                        dm.IsActive = true;
                        dm.IsDeleted = false;
                        dm.CreatedDate = DateTime.Now;
                        dm.CreatedBy = userid;

                        db.PageMaster.Add(dm);
                        result = db.Commit() > 0;
                    }

                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error("Error in Update Page " + ex.Message);
                return false;
            }

        }
        [Route("Remove")]
        [HttpPost]
        public bool RemovePage(int id)
        {
            bool result = false;
            logger.Info("start Add Page: ");
            try
            {

                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                using (AuthContext db = new AuthContext())
                {
                    PageMaster PageDelete = db.PageMaster.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
                    PageDelete.IsDeleted = true;
                    PageDelete.IsActive = false;
                    PageDelete.ModifiedDate = indianTime;
                    PageDelete.ModifiedBy = userid;
                    db.Entry(PageDelete).State = EntityState.Modified;
                    result = db.Commit() > 0;
                }
                return result;
            }

            catch (Exception ex)
            {
                logger.Error("Error in Add Page " + ex.Message);
                return false;
            }

        }

    }
    public class PageButtonPermissionDTO
    {
        public long ButtonMasterId { get; set; }
        public long PageMasterID { get; set; }
        public bool IsActive { get; set; }
    }

    public class PeoplePageButtonPermissionDTO
    {
        public long PageButtonId { get; set; }
        public long PeopleId { get; set; }
        public bool IsActive { get; set; }
    }



    public class PeoplePageAccessPermissionDTO
    {
        public string PageName { get; set; }
        public long PageMasterID { get; set; }
        public long ButtonMasterID { get; set; }
        public string ButtonName { get; set; }
        public long PageButtonID { get; set; }
    }


}
