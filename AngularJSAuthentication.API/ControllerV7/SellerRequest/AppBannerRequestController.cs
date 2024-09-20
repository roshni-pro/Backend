using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model.Seller;

namespace AngularJSAuthentication.API.ControllerV7.AppBanner
{
    [RoutePrefix("api/AppBannerReq")]
    public class AppBannerRequestController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("PostAppBannerReq")]
        [HttpPost]
        public ResAppBannerRequest AddAppBannerReq(AppBannerRequest obj)
        {
            ResAppBannerRequest res = new ResAppBannerRequest();
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (obj != null && obj.Id > 0 && userid > 0)
                {
                    var AppBannerData = context.AppBannerRequests.FirstOrDefault(x => x.Id == obj.Id && x.IsDeleted == false);
                    if (AppBannerData != null && !AppBannerData.IsApproved && AppBannerData.Status != "Accept")
                    {
                        AppBannerData.Comment = obj.Comment;
                        AppBannerData.ModifiedBy = userid;
                        AppBannerData.ModifiedDate = DateTime.Now;
                        AppBannerData.ImageUrl = obj.ImageUrl;
                        AppBannerData.StartDate = obj.StartDate;
                        AppBannerData.EndDate = obj.EndDate;
                        AppBannerData.Type = obj.Type;
                        AppBannerData.AppBannerDiscription = obj.AppBannerDiscription;
                        if (AppBannerData.Status == "Reject")
                        {
                            AppBannerData.Status = "Pending";
                        }
                        else
                        {
                            AppBannerData.Status = obj.Status;
                        }
                        context.Entry(AppBannerData).State = EntityState.Modified;
                        context.Commit();
                        res.Result = true;

                        res.msg = "Successfully Saved";
                    }
                    else
                    {
                        res.msg = "request cannot be changed, due to " + AppBannerData.Status;

                    }
                }
                else if (obj != null && obj.WarehouseIds.Any())
                {
                    List<AppBannerRequest> addAppBannerRequest = new List<AppBannerRequest>();

                    foreach (var wid in obj.WarehouseIds)
                    {
                        var AddAppBannerData = new AppBannerRequest
                        {
                            ImageUrl = obj.ImageUrl,
                            StartDate = obj.StartDate,
                            EndDate = obj.EndDate,
                            SubCatId = obj.SubCatId,
                            Comment = obj.Comment,
                            ApprovedDate = obj.ApprovedDate,
                            IsApproved = obj.IsApproved,
                            ApprovedBy = userid,
                            IsActive = false,
                            IsDeleted = false,
                            CreatedBy = userid,
                            CreatedDate = DateTime.Now,
                            WarehouseId = wid,
                            Type = obj.Type,
                            AppBannerDiscription = obj.AppBannerDiscription,
                            Status = "Pending",
                            SequenceNo = obj.SequenceNo

                        };
                        addAppBannerRequest.Add(AddAppBannerData);
                    }
                    context.AppBannerRequests.AddRange(addAppBannerRequest);
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

        [Route("GetAppBannerRequestList")]
        [HttpPost]
        public ResAppBannerRequest AppBannerRequestList(AppBannerFilterDc filter)
        {
            int Skiplist = (filter.skip - 1) * filter.take;
            ResAppBannerRequest res = new ResAppBannerRequest();

            List<AppBannerRequest> AppBannerReqList = new List<AppBannerRequest>();
            List<AppBannerRequestDc> List = new List<AppBannerRequestDc>();
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
                string sqlquery = "select b.ObjectId, b.Id,b.ImageUrl,b.StartDate,b.EndDate,b.WarehouseId,b.Comment,b.ApprovedDate,b.IsApproved,b.ApprovedBy,b.CreatedDate,b.IsActive,b.SubCatId, b.Type, b.AppBannerDiscription,b.SequenceNo, b.Status,b.CreatedBy,b.ModifiedBy,b.ModifiedDate,b.IsDeleted,b.AppBannerDiscription,w.WarehouseName as Warehouse,s.SubcategoryName as SubCatName from AppBannerRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause + " Order by b.Id desc offset " + Skiplist + " rows fetch next " + filter.take + " rows only";
                string sqlcount = "select count(*) from AppBannerRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause;

                int totalcount = context.Database.SqlQuery<int>(sqlcount).FirstOrDefault();
                List<AppBannerRequestDc> newdata = context.Database.SqlQuery<AppBannerRequestDc>(sqlquery).ToList();
                List = Mapper.Map(newdata).ToANew<List<AppBannerRequestDc>>();

                res.totalcount = totalcount;
                res.AppBannerRequestDcs = List;
                res.msg = "success";
                res.Result = true;

            }
            return res;
        }

        [Route("ExportAppBannerRequest")]
        [HttpPost]
        public ResAppBannerRequest ExportAppBannerRequest(AppBannerFilterDc filter)
        {

            ResAppBannerRequest res = new ResAppBannerRequest();

            List<AppBannerRequest> AppBannerReqList = new List<AppBannerRequest>();
            List<AppBannerRequestDc> List = new List<AppBannerRequestDc>();
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
                string sqlquery = "select p.DisplayName as CreatedbyName,Isnull(ps.DisplayName,'') as ModifiedByName, b.ObjectId,b.AppBannerDiscription, b.Id,b.ImageUrl,b.StartDate,b.EndDate,b.WarehouseId,b.Comment,b.ApprovedDate,b.IsApproved,b.ApprovedBy,b.CreatedDate,b.IsActive,b.SubCatId, b.Type, b.AppBannerDiscription, b.Status,b.CreatedBy,b.ModifiedBy,b.ModifiedDate,b.IsDeleted,w.WarehouseName as Warehouse,s.SubcategoryName as SubCatName from AppBannerRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId join People as p on p.PeopleID=b.CreatedBy left join People as ps on ps.PeopleID=b.ModifiedBy where b.IsDeleted=0 " + whereclause;

                List<AppBannerRequestDc> newdata = context.Database.SqlQuery<AppBannerRequestDc>(sqlquery).ToList();
                List = Mapper.Map(newdata).ToANew<List<AppBannerRequestDc>>();

                res.totalcount = 0;
                res.AppBannerRequestDcs = List;
                res.msg = "success";
                res.Result = true;
            }
            return res;
        }

        [Route("ApproveReq")]
        [HttpPost]
        public ResAppBannerRequest ApproveReq(AppBannerRequest obj)
        {
            ResAppBannerRequest res = new ResAppBannerRequest();
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                var AppBannerData = context.AppBannerRequests.FirstOrDefault(x => x.Id == obj.Id && x.IsDeleted == false);

                if (obj != null && obj.Id > 0 && userid > 0 && AppBannerData != null && !AppBannerData.IsApproved && obj.SequenceNo > 0 && AppBannerData.Status == "Accept")
                {
                    SellerRequestHelper helper = new SellerRequestHelper();
                    int objectid = helper.ApproveAppBannerRequests(AppBannerData, obj.SequenceNo);
                    if (objectid > 0)
                    {
                        AppBannerData.Status = "Approved";
                        AppBannerData.ApprovedDate = DateTime.Now;
                        AppBannerData.IsApproved = true;
                        AppBannerData.ApprovedBy = userid;
                        AppBannerData.ModifiedBy = userid;
                        AppBannerData.ModifiedDate = DateTime.Now;
                        AppBannerData.ObjectId = objectid;
                        AppBannerData.SequenceNo = obj.SequenceNo;
                        context.Entry(AppBannerData).State = EntityState.Modified;
                        context.Commit();
                        res.msg = "Request Approved Successfully";
                        res.Result = true;
                        return res;
                    }
                    res.msg = "Request not Approved , Something went wrong";
                    res.Result = false;
                }
                else if (AppBannerData.IsApproved)
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
        public ResAppBannerRequest AcceptReq(AppBannerRequest obj)
        {
            ResAppBannerRequest res = new ResAppBannerRequest();
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                var AppBannerData = context.AppBannerRequests.FirstOrDefault(x => x.Id == obj.Id && x.IsDeleted == false);

                if (AppBannerData != null && !AppBannerData.IsApproved && AppBannerData.Status != "Cancel")
                {
                    if (obj.Status == "Accept")
                    {
                        AppBannerData.IsActive = true;
                    }
                    AppBannerData.Status = obj.Status;
                    AppBannerData.Comment = obj.Comment;
                    AppBannerData.ModifiedBy = userid;
                    AppBannerData.ModifiedDate = DateTime.Now;
                    AppBannerData.SequenceNo = obj.SequenceNo;
                    context.Entry(AppBannerData).State = EntityState.Modified;
                    context.Commit();
                    res.msg = "Request " + AppBannerData.Status + " Successfully";
                    res.Result = true;
                    return res;
                }
                else if (AppBannerData != null)
                {
                    res.msg = "Request already " + AppBannerData.Status;
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

        [Route("AutoRejectAllRequest")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult AutoRejectAllRequest()
        {
            using (var context = new AuthContext())
            {
                var AppBannerReqList = context.AppBannerRequests.Where(x => x.StartDate <= DateTime.Now && x.Status == "Pending").ToList();
                if (AppBannerReqList.Count > 0)
                {
                    foreach (var item in AppBannerReqList)
                    {
                        item.Status = "Reject";
                        item.ModifiedDate = DateTime.Now;
                        context.Entry(item).State = EntityState.Modified;
                        context.Commit();
                    }
                }
                var FlashDealReqList = context.FlashDealRequests.Where(x => x.StartDate <= DateTime.Now && x.Status == "Pending").ToList();
                if (FlashDealReqList.Count > 0)
                {
                    foreach (var item in FlashDealReqList)
                    {
                        item.Status = "Reject";
                        item.ModifiedDate = DateTime.Now;
                        context.Entry(item).State = EntityState.Modified;
                        context.Commit();
                    }
                }
                var MurliReqList = context.MurliRequests.Where(x => x.StartDate <= DateTime.Now && x.Status == "Pending").ToList();
                if (MurliReqList.Count > 0)
                {
                    foreach (var item in MurliReqList)
                    {
                        item.Status = "Reject";
                        item.ModifiedDate = DateTime.Now;
                        context.Entry(item).State = EntityState.Modified;
                        context.Commit();
                    }
                }
                var NotificationReqList = context.NotificationRequests.Where(x => x.StartDate <= DateTime.Now && x.Status == "Pending").ToList();
                if (NotificationReqList.Count > 0)
                {
                    foreach (var item in NotificationReqList)
                    {
                        item.Status = "Reject";
                        item.ModifiedDate = DateTime.Now;
                        context.Entry(item).State = EntityState.Modified;
                        context.Commit();
                    }
                }
            }
            return Ok();
        }
    }
}

public class AppBannerFilterDc
{
    public int skip { get; set; }
    public int take { get; set; }
    public List<int> WarehouseIds { get; set; }
    public DateTime? startDate { get; set; }
    public DateTime? endDate { get; set; }
    public string statusValue { get; set; }
}