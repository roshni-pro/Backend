using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Seller;
using LinqKit;
using NLog;

namespace AngularJSAuthentication.API.ControllerV7.AppBanner
{
    [RoutePrefix("api/MurliReq")]
    public class MurliRequestController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);



        #region Save hindi text in   
        [Route("GenerateMurliaudio")]
        [HttpPost]
        public string GenerateMurliaudioFile(SellerMurliAudioDc murliAudioDc)
        {
            logger.Info("GenerateMurliaudioFile ");

            var fileName = "Seller_" + DateTime.Now.ToString("MMddyyyyHHmmss");

            logger.Info("GenerateMurliaudioFile file name: " + fileName);
            var url = GoogleTextToSpeach.ConvertHindiTextToAudio(murliAudioDc.hindiText, fileName);

            return url;

        }
        #endregion

        [Route("PostMurliReq")]
        [HttpPost]
        public ResMurliRequest AddMurliReq(MurliRequest obj)
        {
            ResMurliRequest res = new ResMurliRequest();

            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (obj != null && obj.Id > 0 && userid > 0)
                {
                    var MurliData = context.MurliRequests.FirstOrDefault(x => x.Id == obj.Id && x.IsDeleted == false);

                    if (MurliData != null && !MurliData.IsApproved && MurliData.Status != "Accept")
                    {
                        MurliData.Comment = obj.Comment;
                        MurliData.ModifiedBy = userid;
                        MurliData.ModifiedDate = DateTime.Now;
                        MurliData.StartDate = obj.StartDate;
                        MurliData.EndDate = obj.EndDate;
                        MurliData.MurliDescription = obj.MurliDescription;
                        MurliData.MurliNotificationTitle = obj.MurliNotificationTitle;
                        MurliData.MurliNotificationMsg = obj.MurliNotificationMsg;
                        MurliData.MurliFile = obj.MurliFile;
                        if (MurliData.Status == "Reject")
                        {
                            MurliData.Status = "Pending";
                        }
                        else
                        {
                            MurliData.Status = obj.Status;
                        }
                        context.Entry(MurliData).State = EntityState.Modified;
                        context.Commit();
                        res.Result = true;

                        res.msg = "Successfully Saved";
                    }
                    else
                    {
                        res.msg = "request cannot be changed, due to " + MurliData.Status;

                    }
                }
                else if (obj != null && obj.WarehouseIds.Any())
                {
                    List<MurliRequest> addMurliRequest = new List<MurliRequest>();
                    foreach (var wid in obj.WarehouseIds)
                    {
                        var MurliData = new MurliRequest
                        {
                            StartDate = obj.StartDate,
                            EndDate = obj.EndDate,
                            SubCatId = obj.SubCatId,
                            WarehouseId = wid,
                            Comment = obj.Comment,
                            MurliDescription = obj.MurliDescription,
                            MurliNotificationTitle = obj.MurliNotificationTitle,
                            MurliNotificationMsg = obj.MurliNotificationMsg,
                            MurliFile = obj.MurliFile,
                            ApprovedDate = obj.ApprovedDate,
                            IsApproved = obj.IsApproved,
                            ApprovedBy = userid,
                            IsActive = false,
                            IsDeleted = false,
                            CreatedBy = userid,
                            CreatedDate = DateTime.Now,
                            Status = "Pending"
                        };
                        addMurliRequest.Add(MurliData);
                    }
                    context.MurliRequests.AddRange(addMurliRequest);
                    context.Commit();
                    res.Result = true;

                    res.msg = "Successfully Added";
                }
                else
                {
                    res.msg = "Failed";
                    res.Result = false;
                }
            }
            return res;

        }


        [Route("GetMurliRequestList")]
        [HttpPost]
        public ResMurliRequest MurliRequestList(MurliFilterDc filter)
        {
            int Skiplist = (filter.skip - 1) * filter.take;
            ResMurliRequest res = new ResMurliRequest();

            List<MurliRequest> MurliReqList = new List<MurliRequest>();
            List<MurliRequestDc> List = new List<MurliRequestDc>();
            using (var context = new AuthContext())
            {

                string whereclause = "";

                if (filter.WarehouseIds != null && filter.WarehouseIds.Count > 0)
                {
                    var warehouseresult = string.Join(",", filter.WarehouseIds);
                    whereclause += " and b.WarehouseId in (" + warehouseresult + ")";
                }
                if (filter.startDate.HasValue && filter.endDate.HasValue)
                {
                    whereclause += " and (b.CreatedDate >= " + "'" + filter.startDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "'" + " And  b.CreatedDate <=" + "'" + filter.endDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "')";
                }
                if (filter.statusValue != null && filter.statusValue != "All")
                {
                    whereclause += " and b.status=" + "'" + filter.statusValue + "'";
                }
                string sqlquery = "select Id,b.StartDate,b.EndDate,b.MurliNotificationMsg,b.MurliDescription,b.MurliNotificationTitle,b.MurliFile,b.WarehouseId,b.Comment,b.ApprovedDate,b.IsApproved,b.ApprovedBy,b.CreatedDate,b.ModifiedDate,b.IsActive,b.IsDeleted,b.CreatedBy,b.ModifiedBy,b.SubCatId,b.MurliDescription,b.Status,b.ObjectId,w.WarehouseName as Warehouse,s.SubcategoryName as SubCatName from MurliRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause + " Order by b.Id desc offset " + Skiplist + " rows fetch next " + filter.take + " rows only";
                string sqlcount = "select count(*) from MurliRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause;

                int totalcount = context.Database.SqlQuery<int>(sqlcount).FirstOrDefault();
                List<MurliRequestDc> newdata = context.Database.SqlQuery<MurliRequestDc>(sqlquery).ToList();

                List = Mapper.Map(newdata).ToANew<List<MurliRequestDc>>();

                res.totalcount = totalcount;
                res.MurliRequestDcs = List;
                res.msg = "success";
                res.Result = true;
            }

            return res;
        }

        [Route("ExportMurliRequest")]
        [HttpPost]
        public ResMurliRequest ExportMurliRequest(MurliFilterDc filter)
        {
            ResMurliRequest res = new ResMurliRequest();

            List<MurliRequest> MurliReqList = new List<MurliRequest>();
            List<MurliRequestDc> List = new List<MurliRequestDc>();
            using (var context = new AuthContext())
            {

                string whereclause = "";

                if (filter.WarehouseIds != null && filter.WarehouseIds.Count > 0)
                {
                    var warehouseresult = string.Join(",", filter.WarehouseIds);
                    whereclause += " and b.WarehouseId in (" + warehouseresult + ")";
                }
                if (filter.startDate.HasValue && filter.endDate.HasValue)
                {
                    whereclause += " and (b.CreatedDate >= " + "'" + filter.startDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "'" + " And  b.CreatedDate <=" + "'" + filter.endDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "')";
                }
                string sqlquery = "select p.DisplayName as CreatedbyName,Isnull(ps.DisplayName,'') as ModifiedByName, Id,b.StartDate,b.EndDate,b.MurliNotificationMsg,b.MurliDescription,b.MurliNotificationTitle,b.MurliFile,b.WarehouseId,b.Comment,b.ApprovedDate,b.IsApproved,b.ApprovedBy,b.CreatedDate,b.ModifiedDate,b.IsActive,b.IsDeleted,b.CreatedBy,b.ModifiedBy,b.SubCatId,b.MurliDescription,b.Status,b.ObjectId,w.WarehouseName as Warehouse,s.SubcategoryName as SubCatName from MurliRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId join People as p on p.PeopleID=b.CreatedBy left join People as ps on ps.PeopleID=b.ModifiedBy where b.IsDeleted=0 " + whereclause;

                List<MurliRequestDc> newdata = context.Database.SqlQuery<MurliRequestDc>(sqlquery).ToList();

                List = Mapper.Map(newdata).ToANew<List<MurliRequestDc>>();

                res.totalcount = 0;
                res.MurliRequestDcs = List;
                res.msg = "success";
                res.Result = true;
            }

            return res;
        }
        [Route("ImageUpload")]
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult ImageUpload()
        {
            string LogoUrl = "";

            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                if (httpPostedFile != null)
                {
                    if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath("~/GrowBussiness")))
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/GrowBussiness"));

                    string extension = Path.GetExtension(httpPostedFile.FileName);
                    string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;
                    LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/GrowBussiness"), fileName);

                    httpPostedFile.SaveAs(LogoUrl);

                    AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(fileName, "~/GrowBussiness", LogoUrl);

                    LogoUrl = "/GrowBussiness/" + fileName;
                }

            }
            return Created<string>(LogoUrl, LogoUrl);
        }


        [Route("ApproveReq")]
        [HttpPost]
        public ResMurliRequest ApproveReq(MurliRequest obj)
        {
            ResMurliRequest res = new ResMurliRequest();
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                var MurliData = context.MurliRequests.FirstOrDefault(x => x.Id == obj.Id && x.IsDeleted == false);

                if (obj != null && obj.Id > 0 && userid > 0 && MurliData != null && !MurliData.IsApproved && MurliData.Status == "Accept")
                {
                    SellerRequestHelper helper = new SellerRequestHelper();
                    int objectid = helper.ApproveMurliRequest(MurliData);
                    if (objectid > 0)
                    {
                        MurliData.MurliDescription = obj.MurliDescription;
                        MurliData.MurliFile = obj.MurliFile;
                        MurliData.MurliNotificationTitle = obj.MurliNotificationTitle;
                        MurliData.MurliNotificationMsg = obj.MurliNotificationMsg;
                        MurliData.Status = "Approved";
                        MurliData.ApprovedDate = DateTime.Now;
                        MurliData.IsApproved = true;
                        MurliData.ApprovedBy = userid;
                        MurliData.ModifiedBy = userid;
                        MurliData.ModifiedDate = DateTime.Now;
                        MurliData.ObjectId = objectid;
                        context.Entry(MurliData).State = EntityState.Modified;
                        context.Commit();
                        res.msg = "Request Approved Successfully";
                        res.Result = true;
                        return res;
                    }
                    res.msg = "Request not Approved, Something went wrong";
                    res.Result = true;
                }
                else if (MurliData.IsApproved)
                {
                    res.msg = "Request already Approved";
                    res.Result = true;
                }
                else
                {
                    res.msg = "Failed";
                    res.Result = false;
                }
            }
            return res;

        }


        [Route("AcceptReq")]
        [HttpPost]
        public ResMurliRequest AcceptReq(MurliRequest obj)
        {
            ResMurliRequest res = new ResMurliRequest();
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                var MurliData = context.MurliRequests.FirstOrDefault(x => x.Id == obj.Id && x.IsDeleted == false);

                if (MurliData != null && !MurliData.IsApproved && MurliData.Status != "Cancel")
                {
                    if (obj.Status == "Accept")
                    {
                        MurliData.IsActive = true;
                    }
                    MurliData.Status = obj.Status;
                    MurliData.MurliDescription = obj.MurliDescription;
                    MurliData.MurliFile = obj.MurliFile;
                    MurliData.MurliNotificationTitle = obj.MurliNotificationTitle;
                    MurliData.MurliNotificationMsg = obj.MurliNotificationMsg;
                    MurliData.Comment = obj.Comment;
                    MurliData.ModifiedBy = userid;
                    MurliData.ModifiedDate = DateTime.Now;
                    context.Entry(MurliData).State = EntityState.Modified;
                    context.Commit();
                    res.msg = "Request " + MurliData.Status + " Successfully";
                    res.Result = true;
                    return res;

                }
                else if (MurliData != null)
                {
                    res.msg = "Request already " + MurliData.Status;
                    res.Result = true;
                }
                else
                {
                    res.msg = "Failed";
                    res.Result = false;
                }
            }
            return res;

        }



       
    }



}


public class MurliFilterDc
{
    public int skip { get; set; }
    public int take { get; set; }
    public List<int> WarehouseIds { get; set; }
    public DateTime? startDate { get; set; }
    public DateTime? endDate { get; set; }
    public string statusValue { get; set; }
}

public class SellerMurliAudioDc
{
    public string hindiText { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

}
