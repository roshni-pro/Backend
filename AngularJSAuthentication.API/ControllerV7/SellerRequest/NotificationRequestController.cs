using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model.Seller;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using LinqKit;

namespace AngularJSAuthentication.API.ControllerV7.AppBanner
{
    [RoutePrefix("api/NotificationReq")]
    public class NotificationRequestController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("PostNotificationReq")]
        [HttpPost]
        public ResNotificationRequest AddNotificationReq(NotificationRequest obj)
        {
            ResNotificationRequest res = new ResNotificationRequest();

            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (obj != null && obj.Id > 0 && userid > 0)
                {
                    var NotificationData = context.NotificationRequests.FirstOrDefault(x => x.Id == obj.Id && x.IsDeleted == false);
                    if (NotificationData != null && !NotificationData.IsApproved && NotificationData.Status != "Accept")
                    {
                        NotificationData.Comment = obj.Comment;
                        NotificationData.ModifiedBy = userid;
                        NotificationData.ModifiedDate = DateTime.Now;
                        NotificationData.ImageUrl = obj.ImageUrl;
                        NotificationData.StartDate = obj.StartDate.Date;
                        NotificationData.EndDate = obj.StartDate.Date.AddDays(1).AddSeconds(-1);
                        NotificationData.NotificationTitle = obj.NotificationTitle;
                        NotificationData.NotificationImage = obj.ImageUrl;
                        NotificationData.NotificationDescription = obj.NotificationDescription;
                        if (NotificationData.Status == "Reject")
                        {
                            NotificationData.Status = "Pending";
                        }
                        else
                        {
                            NotificationData.Status = obj.Status;
                        }
                        context.Entry(NotificationData).State = EntityState.Modified;
                        context.Commit();
                        res.Result = true;

                        res.msg = "Successfully Saved";
                    }
                    else {
                        res.msg = "request cannot be changed, due to " + NotificationData.Status;

                    }
                }
                else if (obj != null && obj.WarehouseIds.Any())
                {
                    List<NotificationRequest> addNotificationRequest = new List<NotificationRequest>();

                    foreach (var wid in obj.WarehouseIds)
                    {
                        var NotificationData = new NotificationRequest
                        {
                            ImageUrl = obj.ImageUrl,
                            StartDate = obj.StartDate.Date,
                            EndDate = obj.StartDate.Date.AddDays(1).AddSeconds(-1),
                            SubCatId = obj.SubCatId,
                            WarehouseId = wid,
                            Comment = obj.Comment,
                            NotificationDescription = obj.NotificationDescription,
                            NotificationTitle = obj.NotificationTitle,
                            NotificationImage = obj.ImageUrl,
                            ApprovedDate = obj.ApprovedDate,
                            IsApproved = obj.IsApproved,
                            ApprovedBy = userid,
                            IsActive = false,
                            IsDeleted = false,
                            CreatedBy = userid,
                            CreatedDate = DateTime.Now,
                            Status = "Pending"
                        };
                        addNotificationRequest.Add(NotificationData);
                    }
                    context.NotificationRequests.AddRange(addNotificationRequest);
                    context.Commit();
                    res.msg = "Successfully Added";
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

        [Route("GetNotificationRequestList")]
        [HttpPost]
        public ResNotificationRequest NotificationRequestList(NotificationFilterDc filter)
        {
            int Skiplist = (filter.skip - 1) * filter.take;

            ResNotificationRequest res = new ResNotificationRequest();

            List<NotificationRequest> NotificationReqList = new List<NotificationRequest>();
            List<NotificationRequestDc> List = new List<NotificationRequestDc>();
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
                string sqlquery = "select b.Id,b.ImageUrl,b.StartDate,b.EndDate,b.NotificationTitle,b.NotificationDescription,b.NotificationImage,b.WarehouseId,b.Comment,b.ApprovedDate,b.IsApproved,b.ApprovedBy,b.CreatedDate,b.ModifiedDate,b.IsActive,b.IsDeleted,b.CreatedBy,b.ModifiedBy,b.SubCatId,b.NotificationDescription,b.Status,b.ObjectId,w.WarehouseName as Warehouse,s.SubcategoryName as SubCatName from NotificationRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause + " Order by b.Id desc offset " + Skiplist + " rows fetch next " + filter.take + " rows only";
                string sqlcount = "select count(*) from NotificationRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause;

                int totalcount = context.Database.SqlQuery<int>(sqlcount).FirstOrDefault();
                List<NotificationRequestDc> newdata = context.Database.SqlQuery<NotificationRequestDc>(sqlquery).ToList();
                List = Mapper.Map(newdata).ToANew<List<NotificationRequestDc>>();

                res.totalcount = totalcount;
                res.NotificationRequestDcs = List;
                res.msg = "success";
                res.Result = true;
            }
            return res;
        }

        [Route("ExportNotificationRequest")]
        [HttpPost]
        public ResNotificationRequest ExportNotificationRequest(NotificationFilterDc filter)
        {
            ResNotificationRequest res = new ResNotificationRequest();

            List<NotificationRequest> NotificationReqList = new List<NotificationRequest>();
            List<NotificationRequestDc> List = new List<NotificationRequestDc>();
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
                string sqlquery = "select p.DisplayName as CreatedbyName,Isnull(ps.DisplayName,'') as ModifiedByName, b.Id,b.ImageUrl,b.StartDate,b.EndDate,b.NotificationTitle,b.NotificationDescription,b.NotificationImage,b.WarehouseId,b.Comment,b.ApprovedDate,b.IsApproved,b.ApprovedBy,b.CreatedDate,b.ModifiedDate,b.IsActive,b.IsDeleted,b.CreatedBy,b.ModifiedBy,b.SubCatId,b.NotificationDescription,b.Status,b.ObjectId,w.WarehouseName as Warehouse,s.SubcategoryName as SubCatName from NotificationRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId join People as p on p.PeopleID=b.CreatedBy left join People as ps on ps.PeopleID=b.ModifiedBy where b.IsDeleted=0 " + whereclause;

                List<NotificationRequestDc> newdata = context.Database.SqlQuery<NotificationRequestDc>(sqlquery).ToList();
                List = Mapper.Map(newdata).ToANew<List<NotificationRequestDc>>();

                res.totalcount = 0;
                res.NotificationRequestDcs = List;
                res.msg = "success";
                res.Result = true;
            }
            return res;
        }

        [Route("ApproveReq")]
        [HttpPost]
        public ResNotificationRequest ApproveReq(NotificationRequest obj)
        {
            ResNotificationRequest res = new ResNotificationRequest();

            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                var NotificationData = context.NotificationRequests.FirstOrDefault(x => x.Id == obj.Id && x.IsDeleted == false);

                if (obj != null && obj.Id > 0 && userid > 0 && NotificationData != null && !NotificationData.IsApproved && NotificationData.Status == "Accept")
                {
                    if (NotificationData.ImageUrl != null) 
                    {
                        string strPathAndQuery = HttpContext.Current.Request.Url.PathAndQuery;
                        string absolutepath = HttpContext.Current.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                        absolutepath = absolutepath.EndsWith("/") ? absolutepath.Substring(0, absolutepath.Length - 1) : absolutepath;

                        string LogoUrl = absolutepath + "" + NotificationData.ImageUrl;
                        string filename = System.IO.Path.GetFileName(LogoUrl);

                        Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);
                        Cloudinary cloudinary = new Cloudinary(account);
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(LogoUrl),
                            PublicId = "Notifications/" + filename,
                            Overwrite = true,
                            Invalidate = true,
                            Backup = false
                        };
                        var uploadResult = cloudinary.Upload(uploadParams);
                        if (uploadResult.SecureUri != null && !string.IsNullOrEmpty(uploadResult.SecureUri.ToString()))
                        {
                            NotificationData.NotificationImage = uploadResult.SecureUri.ToString();

                        }
                        else
                        {
                            NotificationData.NotificationImage = LogoUrl;

                        }
                    }
                    SellerRequestHelper helper = new SellerRequestHelper();
                    int objectid = helper.ApproveNotificationRequest(NotificationData);
                    if (objectid > 0)
                    {
                        NotificationData.Status = "Approved";
                        NotificationData.ApprovedDate = DateTime.Now;
                        NotificationData.IsApproved = true;
                        NotificationData.ApprovedBy = userid;
                        NotificationData.ModifiedBy = userid;
                        NotificationData.ModifiedDate = DateTime.Now;
                        NotificationData.ObjectId = objectid;
                        context.Entry(NotificationData).State = EntityState.Modified;
                        context.Commit();
                        res.msg = "Request Approved Successfully";
                        res.Result = true;
                        return res;
                    }
                    res.msg = "Something went wrong";
                    res.Result = true;
                }
                else if (NotificationData.IsApproved)
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
        public ResNotificationRequest AcceptReq(NotificationRequest obj)
        {
            ResNotificationRequest res = new ResNotificationRequest();

            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                var NotificationData = context.NotificationRequests.FirstOrDefault(x => x.Id == obj.Id && x.IsDeleted == false);

                if (NotificationData != null && !NotificationData.IsApproved && NotificationData.Status != "Cancel")
                {
                    if (obj.Status == "Accept")
                    {
                        NotificationData.IsActive = true;
                    }
                    NotificationData.Status = obj.Status;
                    NotificationData.Comment = obj.Comment;

                    NotificationData.ModifiedBy = userid;
                    NotificationData.ModifiedDate = DateTime.Now;
                    context.Entry(NotificationData).State = EntityState.Modified;
                    context.Commit();
                    res.msg = "Request " + NotificationData.Status + " Successfully";
                    res.Result = true;
                    return res;

                }
                else if (NotificationData != null)
                {
                    res.msg = "Request already " + NotificationData.Status;
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

public class NotificationFilterDc
{
    public int skip { get; set; }
    public int take { get; set; }
    public List<int> WarehouseIds { get; set; }
    public DateTime? startDate { get; set; }
    public DateTime? endDate { get; set; }
    public string statusValue { get; set; }
}