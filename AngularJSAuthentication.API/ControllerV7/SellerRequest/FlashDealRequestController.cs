using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model.Seller;
using LinqKit;

namespace AngularJSAuthentication.API.ControllerV7.AppBanner
{
    [RoutePrefix("api/FlashDealReq")]
    public class FlashDealRequestController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("PostFlashDealReq")]
        [HttpPost]
        public ResFlashDealRequest AddFlashDealReq(FlashDealRequest obj)
        {
            ResFlashDealRequest res = new ResFlashDealRequest();

            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (obj != null && obj.Id > 0 && userid > 0)
                {
                    var FlashDealData = context.FlashDealRequests.Where(x => x.Id == obj.Id && x.IsDeleted == false).Include(x => x.FlashDealRequestItems).FirstOrDefault();

                    if (FlashDealData != null && !FlashDealData.IsApproved && FlashDealData.Status != "Accept")
                    {
                        FlashDealData.Comment = obj.Comment;
                        FlashDealData.ModifiedBy = userid;
                        FlashDealData.ModifiedDate = DateTime.Now;
                        FlashDealData.ImageUrl = obj.ImageUrl;
                        FlashDealData.SequenceNo = obj.SequenceNo;
                        if (FlashDealData.Status == "Reject")
                        {
                            FlashDealData.Status = "Pending";
                        }
                        else
                        {
                            FlashDealData.Status = obj.Status;
                        }

                        foreach (var m in obj.FlashDealRequestItems)
                        {
                            //var item = context.itemMasters.Where(x => x.ItemId == m.ItemId).FirstOrDefault();
                            var FlashDealItems = FlashDealData.FlashDealRequestItems.Where(x => x.Id == m.Id).FirstOrDefault();
                            if (FlashDealItems != null)
                            {
                                FlashDealItems.Moq = m.Moq;
                                FlashDealItems.ItemMultiMrpId = m.ItemMultiMrpId;
                                //FlashDealItems.ItemName = item.itemname;
                                FlashDealItems.AvailableQty = m.AvailableQty;
                                FlashDealItems.MaxQty = m.MaxQty;
                                FlashDealItems.FlashDealPrice = m.FlashDealPrice;
                                FlashDealItems.StartDate = m.StartDate;
                                FlashDealItems.EndDate = m.EndDate;
                                FlashDealData.StartDate = m.StartDate;
                                FlashDealData.EndDate = m.EndDate;
                                context.Entry(FlashDealItems).State = EntityState.Modified;
                            }
                        }
                        context.Entry(FlashDealData).State = EntityState.Modified;
                        context.Commit();
                        res.Result = true;

                        res.msg = "Successfully Saved";
                    }
                    else
                    {
                        res.msg = "request cannot be changed, due to " + FlashDealData.Status;
                    }

                }
                else if (obj != null && obj.WarehouseIds.Any())
                {
                    List<FlashDealRequest> AddFlashDealRequestList = new List<FlashDealRequest>();
                    var ItemIds = obj.FlashDealRequestItems.Select(x => x.ItemId).ToList();
                    var ItemMasterList = context.itemMasters.Where(x => ItemIds.Contains(x.ItemId)).ToList();
                    foreach (var wid in obj.WarehouseIds)
                    {
                        if (obj.FlashDealRequestItems != null)
                        {
                            foreach (var m in obj.FlashDealRequestItems)
                            {
                                FlashDealRequest FlashDealData = new FlashDealRequest();

                                FlashDealData.ImageUrl = obj.ImageUrl;
                                FlashDealData.StartDate = m.StartDate;
                                FlashDealData.EndDate = m.EndDate;
                                FlashDealData.SubCatId = obj.SubCatId;
                                FlashDealData.WarehouseId = wid;
                                FlashDealData.Comment = obj.Comment;
                                FlashDealData.ApprovedDate = null;
                                FlashDealData.IsApproved = false;
                                FlashDealData.ApprovedBy = null;
                                FlashDealData.IsActive = false;
                                FlashDealData.IsDeleted = false;
                                FlashDealData.CreatedBy = userid;
                                FlashDealData.CreatedDate = DateTime.Now;
                                FlashDealData.Status = "Pending";
                                FlashDealData.SequenceNo = obj.SequenceNo;
                                FlashDealData.FlashDealRequestItems = new List<FlashDealRequestItem>();
                                var item = ItemMasterList.Where(x => x.ItemId == m.ItemId).FirstOrDefault();
                                FlashDealRequestItem flashdeal = new FlashDealRequestItem()
                                {
                                    Moq = m.Moq,
                                    ItemMultiMrpId = m.ItemMultiMrpId,
                                    ItemName = item.itemname,
                                    AvailableQty = m.AvailableQty,
                                    MaxQty = m.MaxQty,
                                    FlashDealPrice = m.FlashDealPrice,
                                    StartDate = m.StartDate,
                                    EndDate = m.EndDate
                                };
                                FlashDealData.FlashDealRequestItems.Add(flashdeal);
                                AddFlashDealRequestList.Add(FlashDealData);
                            }
                        }
                    }
                    if (AddFlashDealRequestList != null && AddFlashDealRequestList.Any())
                    {
                        context.FlashDealRequests.AddRange(AddFlashDealRequestList);
                        context.Commit();

                    }
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

        [Route("GetFlashDealRequestList")]
        [HttpPost]
        public ResFlashDealRequest FlashDealRequestList(FlashDealFilterDc filter)
        {
            int Skiplist = (filter.skip - 1) * filter.take;
            ResFlashDealRequest res = new ResFlashDealRequest();

            List<FlashDealRequest> FlashDealReqList = new List<FlashDealRequest>();
            List<FlashDealRequestDc> List = new List<FlashDealRequestDc>();
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

                string sqlquery = "select b.Id,b.ImageUrl,b.ItemName,b.StartDate,b.SequenceNo,b.EndDate,b.WarehouseId,b.Moq,b.AvailableQty,b.MaxQty,b.FlashDealPrice,b.Comment,b.ApprovedDate,b.IsApproved,b.ApprovedBy,b.CreatedDate,b.ModifiedDate,b.IsActive,b.IsDeleted,b.CreatedBy,b.ModifiedBy,b.SubCatId,b.ItemMultiMrpId,b.ItemName,b.Status,b.ObjectId,w.WarehouseName as Warehouse,s.SubcategoryName as SubCatName from FlashDealRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId  where b.IsDeleted=0 " + whereclause + " Order by b.Id desc offset " + Skiplist + " rows fetch next " + filter.take + " rows only";
                string sqlcount = "select count(*) from FlashDealRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause;

                int totalcount = context.Database.SqlQuery<int>(sqlcount).FirstOrDefault();
                List<FlashDealRequestDc> newdata = context.Database.SqlQuery<FlashDealRequestDc>(sqlquery).ToList();
                List = Mapper.Map(newdata).ToANew<List<FlashDealRequestDc>>();

                res.totalcount = totalcount;
                res.FlashDealRequestDcs = List;
                res.msg = "success";
                res.Result = true;
            }

            return res;
        }

        [Route("FlashDealRequestItemsById")]
        [HttpGet]
        public ResFlashDealItems FlashDealRequestItems(int Id)
        {
            ResFlashDealItems res = new ResFlashDealItems();
            List<FlashDealItemsDc> List = new List<FlashDealItemsDc>();
            using (var context = new AuthContext())
            {
                var FlashDealData = context.FlashDealRequestItems.Where(x => x.FlashDealRequestMasterId == Id).ToList();
                List = Mapper.Map(FlashDealData).ToANew<List<FlashDealItemsDc>>();
                var MultiMrpId = List.Select(x => x.ItemMultiMrpId).Distinct().ToList();
                var MRP = context.ItemMultiMRPDB.Where(x => MultiMrpId.Contains(x.ItemMultiMRPId)).ToList();
                List.ForEach(x =>
                {
                    x.MRP = MRP.Where(y => y.ItemMultiMRPId == x.ItemMultiMrpId).Select(y => y.MRP).FirstOrDefault();
                });
                res.FlashDealItemsDcs = List;

            }
            return res;
        }
        [Route("ExportFlashDealRequest")]
        [HttpPost]
        public ResFlashDealRequest ExportFlashDealRequest(FlashDealFilterDc filter)
        {
            ResFlashDealRequest res = new ResFlashDealRequest();

            List<FlashDealRequest> FlashDealReqList = new List<FlashDealRequest>();
            List<FlashDealRequestDc> List = new List<FlashDealRequestDc>();
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
                string sqlquery = "select p.DisplayName as CreatedbyName,Isnull(ps.DisplayName,'') as ModifiedByName,b.Id,i.StartDate,i.EndDate,b.SequenceNo,b.WarehouseId,i.Moq,i.AvailableQty,i.MaxQty,i.FlashDealPrice,b.Comment,b.ApprovedDate,b.IsApproved,b.ApprovedBy,b.CreatedDate,b.ModifiedDate,b.IsActive,b.IsDeleted,b.CreatedBy,b.ModifiedBy,b.SubCatId,i.ItemMultiMrpId,i.ItemName,b.Status,b.ObjectId,w.WarehouseName as Warehouse,s.SubcategoryName as SubCatName from FlashDealRequests as b  join FlashDealRequestItems as i on i.FlashDealRequestMasterId=b.Id Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId join People as p on p.PeopleID=b.CreatedBy left join People as ps on ps.PeopleID=b.ModifiedBy where b.IsDeleted=0 " + whereclause;

                List<FlashDealRequestDc> newdata = context.Database.SqlQuery<FlashDealRequestDc>(sqlquery).ToList();
                List = Mapper.Map(newdata).ToANew<List<FlashDealRequestDc>>();
                var MultiMrpId = List.Select(x => x.ItemMultiMrpId).Distinct().ToList();
                var MRP = context.itemMasters.Where(x => MultiMrpId.Contains(x.ItemMultiMRPId)).ToList();
                List.ForEach(x =>
                {
                    x.MRP = MRP.Where(y => y.ItemMultiMRPId == x.ItemMultiMrpId).Select(y => y.MRP).FirstOrDefault();
                });

                res.totalcount = 0;
                res.FlashDealRequestDcs = List;
                res.msg = "success";
                res.Result = true;
            }

            return res;
        }

        [Route("ApproveReq")]
        [HttpPost]
        public ResFlashDealRequest ApproveReq(FlashDealRequest obj)
        {
            ResFlashDealRequest res = new ResFlashDealRequest();

            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                var FlashDealData = context.FlashDealRequests.Where(x => x.Id == obj.Id && x.IsDeleted == false).Include(x => x.FlashDealRequestItems).FirstOrDefault();

                if (obj != null && obj.Id > 0 && userid > 0 && FlashDealData != null && !FlashDealData.IsApproved && FlashDealData.Status == "Accept")
                {
                    #region Check if item already in flash during 
                    if (FlashDealData.FlashDealRequestItems.Any())
                    {
                        foreach (var it in FlashDealData.FlashDealRequestItems)
                        {
                            var item = context.itemMasters.Where(x => x.WarehouseId == FlashDealData.WarehouseId && x.ItemMultiMRPId == it.ItemMultiMrpId && x.MinOrderQty == it.Moq && x.Deleted == false).FirstOrDefault();

                            if (item == null)
                            {
                                res.msg = "Request not Approved due to item MRP Not Found for Item: " + it.ItemName;
                                res.Result = true;
                                return res;
                            }
                            bool IsExistresult = context.AppHomeSectionItemsDB.Any(x => x.ItemId == item.ItemId && x.IsFlashDeal == true && x.Active == true
                                                        && x.Deleted == false && (x.OfferEndTime.HasValue && x.OfferEndTime.Value >= it.StartDate) && (x.OfferStartTime.HasValue && x.OfferStartTime.Value <= it.StartDate));

                            if (IsExistresult)
                            {
                                res.msg = "Request not Approved due to item already in flashdeal" + item.itemname;
                                res.Result = true;
                                return res;
                            }
                        }
                    }
                    else
                    {
                        var item = context.itemMasters.Where(x => x.WarehouseId == FlashDealData.WarehouseId && x.ItemMultiMRPId == FlashDealData.ItemMultiMrpId && x.MinOrderQty == FlashDealData.Moq && x.Deleted == false).FirstOrDefault();

                        if (item == null)
                        {
                            res.msg = "Request not Approved due to item MRP changed for Item: " + FlashDealData.ItemName;
                            res.Result = true;
                            return res;
                        }
                        bool IsExistresult = context.AppHomeSectionItemsDB.Any(x => x.ItemId == item.ItemId && x.IsFlashDeal == true && x.Active == true
                                                    && x.Deleted == false && (x.OfferEndTime.HasValue && x.OfferEndTime.Value >= FlashDealData.StartDate) && (x.OfferStartTime.HasValue && x.OfferStartTime.Value <= FlashDealData.StartDate));

                        if (IsExistresult)
                        {
                            res.msg = "Request not Approved due to item already in flashdeal" + item.itemname;
                            res.Result = true;
                            return res;
                        }
                    }
                    #endregion
                    SellerRequestHelper helper = new SellerRequestHelper();
                    int objectid = helper.ApproveFlashDealRequest(FlashDealData, obj.SequenceNo);
                    if (objectid > 0)
                    {
                        FlashDealData.SequenceNo = obj.SequenceNo;
                        FlashDealData.Status = "Approved";
                        FlashDealData.ApprovedDate = DateTime.Now;
                        FlashDealData.IsApproved = true;
                        FlashDealData.ApprovedBy = userid;
                        FlashDealData.ModifiedBy = userid;
                        FlashDealData.ModifiedDate = DateTime.Now;
                        FlashDealData.ObjectId = objectid;
                        context.Entry(FlashDealData).State = EntityState.Modified;
                        context.Commit();
                        res.msg = "Request Approved Successfully";
                        res.Result = true;
                        return res;
                    }

                    res.msg = "Request not Approved, Something went wrong";
                    res.Result = true;
                }

                else if (FlashDealData.IsApproved)
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
        public ResFlashDealRequest AcceptReq(FlashDealRequest obj)
        {
            ResFlashDealRequest res = new ResFlashDealRequest();
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                var FlashDealData = context.FlashDealRequests.Where(x => x.Id == obj.Id && x.IsDeleted == false).Include(x => x.FlashDealRequestItems).FirstOrDefault();
                if (userid > 0 && FlashDealData != null && !FlashDealData.IsApproved && FlashDealData.Status != "Cancel")
                {
                    if (obj.Status == "Accept")
                    {
                        FlashDealData.IsActive = true;
                        if (FlashDealData.FlashDealRequestItems != null && FlashDealData.FlashDealRequestItems.Any())
                        {
                            foreach (var it in FlashDealData.FlashDealRequestItems)
                            {
                                var item = context.itemMasters.Where(x => x.WarehouseId == FlashDealData.WarehouseId && x.ItemMultiMRPId == it.ItemMultiMrpId && x.MinOrderQty == it.Moq && x.Deleted == false).FirstOrDefault();
                                if (item == null)
                                {
                                    res.msg = "Request not Accpeted due to item MRP Not Found for Item: " + it.ItemName;
                                    res.Result = true;
                                    return res;
                                }
                            }
                        }
                    }
                    FlashDealData.Status = obj.Status;
                    FlashDealData.Comment = obj.Comment;
                    FlashDealData.SequenceNo = obj.SequenceNo;
                    FlashDealData.ModifiedBy = userid;
                    FlashDealData.ModifiedDate = DateTime.Now;

                    context.Entry(FlashDealData).State = EntityState.Modified;
                    context.Commit();
                    res.msg = "Request " + FlashDealData.Status + " Successfully";
                    res.Result = true;
                    return res;
                }
                else if (FlashDealData != null)
                {
                    res.msg = "Request already " + FlashDealData.Status;
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



