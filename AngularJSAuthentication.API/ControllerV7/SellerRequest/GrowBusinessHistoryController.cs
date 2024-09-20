using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model.Seller;
using LinqKit;

namespace AngularJSAuthentication.API.ControllerV7.AppBanner
{
    [RoutePrefix("api/GrowBusinessHistory")]
    public class GrowBusinessHistoryController : ApiController
    {
        [Route("GetBussinessRequestList")]
        [HttpPost]
        public ResGrowBusinessHistory GetBussinessRequestList(BussinessHistoryFilterDc filter)
        {

            int SubCatId = Convert.ToInt32(Request.Headers.GetValues("SubCatId").First());


            int Skiplist = filter.skip;

            ResGrowBusinessHistory res = new ResGrowBusinessHistory();

            int TotalCount = 0;
            List<AppBannerRequest> AppBannerReqList = new List<AppBannerRequest>();
            List<BrandStoreRequest> BrandStoreReqList = new List<BrandStoreRequest>();
            List<FlashDealRequest> FlashDealReqList = new List<FlashDealRequest>();
            List<MurliRequest> MurliReqList = new List<MurliRequest>();
            List<NotificationRequest> NotificationReqList = new List<NotificationRequest>();

            List<AppBannerRequestDc> AppBannerList = new List<AppBannerRequestDc>();
            List<NotificationRequestDc> NotificationList = new List<NotificationRequestDc>();
            List<FlashDealRequestDc> FlashDealList = new List<FlashDealRequestDc>();
            List<BrandStoreRequestDc> BrandStoreList = new List<BrandStoreRequestDc>();
            List<MurliRequestDc> MurliList = new List<MurliRequestDc>();

            List<GrowBusinessHistoryDc> BussinessList = new List<GrowBusinessHistoryDc>();

            using (var context = new AuthContext())
            {
                if (filter.RequestType == 1) //AppBanner
                {
                    string whereclause = "";

                    if (SubCatId > 0)
                    {
                        whereclause += " and b.SubCatId =" + SubCatId;
                    }
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
                    string sqlquery = "select b.ObjectId, 'AppBanner' as ReqTypeName, b.Id, b.SubCatId,b.ImageUrl,b.StartDate,b.EndDate,b.WarehouseId,b.Comment,b.ApprovedDate,b.IsApproved,b.ApprovedBy,b.CreatedDate,b.IsActive,b.SubCatId, b.Type, b.AppBannerDiscription,b.SequenceNo, b.Status,b.CreatedBy,b.ModifiedBy,b.ModifiedDate,b.IsDeleted,b.AppBannerDiscription,w.WarehouseName,s.SubcategoryName as SubCatName from AppBannerRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause + " Order by b.Id desc offset " + Skiplist + " rows fetch next " + filter.take + " rows only";
                    string sqlcount = "select count(*) from AppBannerRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause;

                    int totalcount = context.Database.SqlQuery<int>(sqlcount).FirstOrDefault();
                    List<GrowBusinessHistoryDc> newdata = context.Database.SqlQuery<GrowBusinessHistoryDc>(sqlquery).ToList();
                    BussinessList = Mapper.Map(newdata).ToANew<List<GrowBusinessHistoryDc>>();
                    #region view and click count 
                    List<int?> ObjectIds = BussinessList.Where(x => x.IsApproved == true).Select(x => x.ObjectId).ToList();
                    if (ObjectIds != null && ObjectIds.Any())
                    {

                        List<ActionCountDc> _result = GetActionCount(ObjectIds, filter.RequestType);
                        if (_result != null && _result.Any())
                        {
                            foreach (var item in BussinessList.Where(x => x.IsApproved == true && ObjectIds.Contains(x.ObjectId)))
                            {
                                item.TotalViews = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalViews;
                                item.TotalSent = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalSent;
                                item.TotalReceived = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalReceived;

                            }
                        }

                    }
                    #endregion

                    TotalCount = totalcount;
                }
                if (filter.RequestType == 2) //Notification
                {
                    string whereclause = "";

                    if (SubCatId > 0)
                    {
                        whereclause += " and b.SubCatId =" + SubCatId;
                    }

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
                    string sqlquery = "select b.Id,b.ImageUrl,'Notification' as ReqTypeName, b.SubCatId, b.StartDate,b.EndDate,b.NotificationTitle,b.NotificationDescription,b.NotificationImage,b.WarehouseId,b.Comment,b.ApprovedDate,b.IsApproved,b.ApprovedBy,b.CreatedDate,b.ModifiedDate,b.IsActive,b.IsDeleted,b.CreatedBy,b.ModifiedBy,b.SubCatId,b.NotificationDescription,b.Status,b.ObjectId,w.WarehouseName,s.SubcategoryName as SubCatName from NotificationRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause + " Order by b.Id desc offset " + Skiplist + " rows fetch next " + filter.take + " rows only";
                    string sqlcount = "select count(*) from NotificationRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause;
                    int totalcount = context.Database.SqlQuery<int>(sqlcount).FirstOrDefault();
                    List<GrowBusinessHistoryDc> newdata = context.Database.SqlQuery<GrowBusinessHistoryDc>(sqlquery).ToList();
                    BussinessList = Mapper.Map(newdata).ToANew<List<GrowBusinessHistoryDc>>();


                    #region view and click count 
                    List<int?> ObjectIds = BussinessList.Where(x => x.IsApproved == true).Select(x => x.ObjectId).ToList();
                    if (ObjectIds != null && ObjectIds.Any())
                    {

                        List<ActionCountDc> _result = GetActionCount(ObjectIds, filter.RequestType);
                        if (_result != null && _result.Any())
                        {
                            foreach (var item in BussinessList.Where(x => x.IsApproved == true && ObjectIds.Contains(x.ObjectId)))
                            {
                                item.TotalViews = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalViews;
                                item.TotalSent = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalSent;
                                item.TotalReceived = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalReceived;
                            }
                        }

                    }
                    #endregion


                    TotalCount = totalcount;
                }
                if (filter.RequestType == 3) //Flash deal
                {
                    string whereclause = "";

                    if (SubCatId > 0)
                    {
                        whereclause += " and b.SubCatId =" + SubCatId;
                    }
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
                    string sqlquery = "select b.Id,b.ImageUrl,'FlashDeal' as ReqTypeName,b.SubCatId, b.ItemName,b.StartDate,b.SequenceNo,b.EndDate,b.WarehouseId,b.Moq,b.AvailableQty,b.MaxQty,b.FlashDealPrice,b.Comment,b.ApprovedDate,b.IsApproved,b.ApprovedBy,b.CreatedDate,b.ModifiedDate,b.IsActive,b.IsDeleted,b.CreatedBy,b.ModifiedBy,b.SubCatId,b.ItemMultiMrpId,b.ItemName,b.Status,b.ObjectId,w.WarehouseName,s.SubcategoryName as SubCatName from FlashDealRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause + " Order by b.Id desc offset " + Skiplist + " rows fetch next " + filter.take + " rows only";
                    string sqlcount = "select count(*) from FlashDealRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause;

                    int totalcount = context.Database.SqlQuery<int>(sqlcount).FirstOrDefault();
                    List<GrowBusinessHistoryDc> newdata = context.Database.SqlQuery<GrowBusinessHistoryDc>(sqlquery).ToList();
                    BussinessList = Mapper.Map(newdata).ToANew<List<GrowBusinessHistoryDc>>();
                    #region view and click count 
                    List<int?> ObjectIds = BussinessList.Where(x => x.IsApproved == true).Select(x => x.ObjectId).ToList();
                    if (ObjectIds != null && ObjectIds.Any())
                    {

                        List<ActionCountDc> _result = GetActionCount(ObjectIds, filter.RequestType);
                        if (_result != null && _result.Any())
                        {
                            foreach (var item in BussinessList.Where(x => x.IsApproved == true && ObjectIds.Contains(x.ObjectId)))
                            {
                                item.TotalViews = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalViews;
                                item.TotalSent = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalSent;
                                item.TotalReceived = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalReceived;
                            }
                        }

                    }
                    #endregion
                    TotalCount = totalcount;
                }
                if (filter.RequestType == 4) //Brand Store
                {
                    string whereclause = "";

                    if (SubCatId > 0)
                    {
                        whereclause += " and b.SubCatId =" + SubCatId;
                    }
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
                    string sqlquery = "select b.Id,b.ImageUrl,'BrandStore' as ReqTypeName,b.SubCatId,b.CategoryId,b.StartDate,b.EndDate,b.WarehouseId,b.Comment,b.ApprovedDate,b.IsApproved,b.ApprovedBy,b.CreatedDate,b.ModifiedDate,b.IsActive,b.IsDeleted,b.CreatedBy,b.ModifiedBy,b.SubCatId,b.Status,b.ObjectId,w.WarehouseName,s.SubcategoryName as SubCatName,c.CategoryName from BrandStoreRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join Categories as c on c.Categoryid=b.CategoryId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause + " Order by b.Id desc offset " + Skiplist + " rows fetch next " + filter.take + " rows only";
                    string sqlcount = "select count(*) from BrandStoreRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join Categories as c on c.Categoryid=b.CategoryId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause;

                    int totalcount = context.Database.SqlQuery<int>(sqlcount).FirstOrDefault();
                    List<GrowBusinessHistoryDc> newdata = context.Database.SqlQuery<GrowBusinessHistoryDc>(sqlquery).ToList();
                    BussinessList = Mapper.Map(newdata).ToANew<List<GrowBusinessHistoryDc>>();
                    #region view and click count 
                    List<int?> ObjectIds = BussinessList.Where(x => x.IsApproved == true).Select(x => x.ObjectId).ToList();
                    if (ObjectIds != null && ObjectIds.Any())
                    {

                        List<ActionCountDc> _result = GetActionCount(ObjectIds, filter.RequestType);
                        if (_result != null && _result.Any())
                        {
                            foreach (var item in BussinessList.Where(x => x.IsApproved == true && ObjectIds.Contains(x.ObjectId)))
                            {
                                item.TotalViews = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalViews;
                                item.TotalSent = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalSent;
                                item.TotalReceived = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalReceived;
                            }
                        }

                    }
                    #endregion
                    TotalCount = totalcount;
                }
                if (filter.RequestType == 5) // Murli
                {
                    string whereclause = "";

                    if (SubCatId > 0)
                    {
                        whereclause += " and b.SubCatId =" + SubCatId;
                    }
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
                    string sqlquery = "select Id,b.StartDate,'Murli' as ReqTypeName, b.SubCatId, b.EndDate,b.MurliNotificationMsg,b.MurliDescription,b.MurliNotificationTitle,b.MurliFile,b.WarehouseId,b.Comment,b.ApprovedDate,b.IsApproved,b.ApprovedBy,b.CreatedDate,b.ModifiedDate,b.IsActive,b.IsDeleted,b.CreatedBy,b.ModifiedBy,b.SubCatId,b.MurliDescription,b.Status,b.ObjectId,w.WarehouseName,s.SubcategoryName as SubCatName from MurliRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause + " Order by b.Id desc offset " + Skiplist + " rows fetch next " + filter.take + " rows only";
                    string sqlcount = "select count(*) from MurliRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause;
                    int totalcount = context.Database.SqlQuery<int>(sqlcount).FirstOrDefault();
                    List<GrowBusinessHistoryDc> newdata = context.Database.SqlQuery<GrowBusinessHistoryDc>(sqlquery).ToList();
                    BussinessList = Mapper.Map(newdata).ToANew<List<GrowBusinessHistoryDc>>();

                    #region view and click count 
                    List<int?> ObjectIds = BussinessList.Where(x => x.IsApproved == true).Select(x => x.ObjectId).ToList();
                    if (ObjectIds != null && ObjectIds.Any())
                    {

                        List<ActionCountDc> _result = GetActionCount(ObjectIds, filter.RequestType);
                        if (_result != null && _result.Any())
                        {
                            foreach (var item in BussinessList.Where(x => x.IsApproved == true && ObjectIds.Contains(x.ObjectId)))
                            {
                                item.TotalViews = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalViews;
                                item.TotalSent = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalSent;
                                item.TotalReceived = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalReceived;
                            }
                        }

                    }
                    #endregion
                    TotalCount = totalcount;
                }
                res.Result = true;
                res.totalcount = TotalCount;
                res.GrowBusinessHistoryDcs = BussinessList;
            }
            return res;
        }

        [Route("GetBussinessDetail")]
        [HttpGet]
        public GrowBusinessHistoryDc GetBussinessDetail(int RequestType, int Id)
        {
            ResGrowBusinessHistory res = new ResGrowBusinessHistory();

            AppBannerRequest AppBannerReqList = new AppBannerRequest();
            BrandStoreRequest BrandStoreReqList = new BrandStoreRequest();
            FlashDealRequest FlashDealReqList = new FlashDealRequest();
            MurliRequest MurliReqList = new MurliRequest();
            NotificationRequest NotificationReqList = new NotificationRequest();

            List<AppBannerRequestDc> AppBannerList = new List<AppBannerRequestDc>();
            List<NotificationRequestDc> NotificationList = new List<NotificationRequestDc>();
            List<FlashDealRequestDc> FlashDealList = new List<FlashDealRequestDc>();
            List<BrandStoreRequestDc> BrandStoreList = new List<BrandStoreRequestDc>();
            List<MurliRequestDc> MurliList = new List<MurliRequestDc>();

            List<FlashDealItemsDc> List = new List<FlashDealItemsDc>();

            GrowBusinessHistoryDc BussinessList = new GrowBusinessHistoryDc();

            using (var context = new AuthContext())
            {
                if (RequestType == 1) //AppBanner
                {
                    AppBannerReqList = context.AppBannerRequests.Where(x => x.IsDeleted == false && x.Id == Id).OrderByDescending(x => x.Id).FirstOrDefault();
                    BussinessList = Mapper.Map(AppBannerReqList).ToANew<GrowBusinessHistoryDc>();
                    BussinessList.WarehouseName = context.Warehouses.Where(x => x.WarehouseId == AppBannerReqList.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                }
                if (RequestType == 2) //Notification
                {

                    NotificationReqList = context.NotificationRequests.Where(x => x.IsDeleted == false && x.Id == Id).OrderByDescending(x => x.Id).FirstOrDefault();
                    BussinessList = Mapper.Map(NotificationReqList).ToANew<GrowBusinessHistoryDc>();
                    BussinessList.WarehouseName = context.Warehouses.Where(x => x.WarehouseId == NotificationReqList.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();

                }
                if (RequestType == 3) //Flash deal
                {
                    FlashDealReqList = context.FlashDealRequests.Where(x => x.IsDeleted == false && x.Id == Id).OrderByDescending(x => x.Id).FirstOrDefault();
                    BussinessList = Mapper.Map(FlashDealReqList).ToANew<GrowBusinessHistoryDc>();
                    BussinessList.WarehouseName = context.Warehouses.Where(x => x.WarehouseId == FlashDealReqList.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();

                    var FlashDealData = context.FlashDealRequestItems.Where(x => x.FlashDealRequestMasterId == Id).ToList();
                    List = Mapper.Map(FlashDealData).ToANew<List<FlashDealItemsDc>>();

                    var MultiMrpId = List.Select(x => x.ItemMultiMrpId).Distinct().ToList();
                    var MRP = context.ItemMultiMRPDB.Where(x => MultiMrpId.Contains(x.ItemMultiMRPId)).ToList();
                    var itemIds = context.itemMasters.Where(x => MultiMrpId.Contains(x.ItemMultiMRPId)).ToList();
                    //double MRP = context.itemMasters.Where(y => y.SubCategoryId == BussinessList.SubCatId && y.WarehouseId == BussinessList.WarehouseId && y.Deleted == false&& y.ItemMultiMRPId==);
                    List.ForEach(x =>
                    {
                        x.MRP = MRP.Where(y => y.ItemMultiMRPId == x.ItemMultiMrpId).Select(y => y.MRP).FirstOrDefault();
                        x.ItemId = itemIds.Where(y => y.ItemMultiMRPId == x.ItemMultiMrpId).Select(y => y.ItemId).FirstOrDefault();
                        if (BussinessList.Status == "Approved" || BussinessList.Status == "Accept")
                        {
                            x.IsRemove = true;
                        }
                        else
                        {
                            x.IsRemove = false;
                        }
                    });
                    BussinessList.FlashDealItemsDcs = List;

                }
                if (RequestType == 4) //Brand Store
                {

                    BrandStoreReqList = context.BrandStoreRequests.Where(x => x.IsDeleted == false && x.Id == Id).OrderByDescending(x => x.Id).FirstOrDefault();
                    BussinessList = Mapper.Map(BrandStoreReqList).ToANew<GrowBusinessHistoryDc>();
                    BussinessList.WarehouseName = context.Warehouses.Where(x => x.WarehouseId == BrandStoreReqList.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                }
                if (RequestType == 5) // Murli
                {

                    MurliReqList = context.MurliRequests.Where(x => x.IsDeleted == false && x.Id == Id).OrderByDescending(x => x.Id).FirstOrDefault();
                    BussinessList = Mapper.Map(MurliReqList).ToANew<GrowBusinessHistoryDc>();
                    BussinessList.WarehouseName = context.Warehouses.Where(x => x.WarehouseId == MurliReqList.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                }
                return BussinessList;
            }
        }

        [Route("ExportBussinessRequest")]
        [HttpPost]
        public ResGrowBusinessHistoryExportDc ExportBussinessRequest(BussExportFilterDc filter)
        {
            int SubCatId = Convert.ToInt32(Request.Headers.GetValues("SubCatId").First());

            ResGrowBusinessHistoryExportDc res = new ResGrowBusinessHistoryExportDc();

            List<AppBannerRequest> AppBannerReqList = new List<AppBannerRequest>();
            List<BrandStoreRequest> BrandStoreReqList = new List<BrandStoreRequest>();
            List<FlashDealRequest> FlashDealReqList = new List<FlashDealRequest>();
            List<MurliRequest> MurliReqList = new List<MurliRequest>();
            List<NotificationRequest> NotificationReqList = new List<NotificationRequest>();

            List<AppBannerRequestDc> AppBannerList = new List<AppBannerRequestDc>();
            List<NotificationRequestDc> NotificationList = new List<NotificationRequestDc>();
            List<FlashDealRequestDc> FlashDealList = new List<FlashDealRequestDc>();
            List<BrandStoreRequestDc> BrandStoreList = new List<BrandStoreRequestDc>();
            List<MurliRequestDc> MurliList = new List<MurliRequestDc>();


            List<FlashDealHistoryExportDc> flashdealexport = new List<FlashDealHistoryExportDc>();
            List<AppBannerHistoryExportDc> appbannerexport = new List<AppBannerHistoryExportDc>();
            List<NotificationHistoryExportDc> notificationexport = new List<NotificationHistoryExportDc>();
            List<MurliHistoryExportDc> murliexport = new List<MurliHistoryExportDc>();
            List<BrandStoreRHistoryExportDc> brandexport = new List<BrandStoreRHistoryExportDc>();

            using (var context = new AuthContext())
            {
                if (filter.RequestType == 1) //AppBanner
                {
                    string whereclause = "";

                    if (SubCatId > 0)
                    {
                        whereclause += " and b.SubCatId =" + SubCatId;
                    }
                    if (filter.WarehouseIds != null && filter.WarehouseIds.Count > 0)
                    {
                        var warehouseresult = string.Join(",", filter.WarehouseIds);
                        whereclause += " and b.WarehouseId in (" + warehouseresult + ")";
                    }
                    if (filter.startDate.HasValue && filter.endDate.HasValue)
                    {
                        whereclause += " and (b.CreatedDate >= " + "'" + filter.startDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "'" + " And  b.CreatedDate <=" + "'" + filter.endDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "')";
                    }
                    string sqlquery = "select p.DisplayName as CreatedbyName,Isnull(ps.DisplayName,'') as ModifiedByName, b.ObjectId, b.Id,b.ImageUrl,b.StartDate,b.EndDate,b.WarehouseId,b.Comment,b.ApprovedDate,b.IsApproved,b.ApprovedBy,b.CreatedDate,b.IsActive,b.SubCatId, b.Type, b.AppBannerDiscription,b.SequenceNo, b.Status,b.CreatedBy,b.ModifiedBy,b.ModifiedDate,b.IsDeleted,b.AppBannerDiscription,w.WarehouseName,s.SubcategoryName as SubCatName from AppBannerRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId join People as p on p.PeopleID=b.CreatedBy left join People as ps on ps.PeopleID=b.ModifiedBy where b.IsDeleted=0 " + whereclause;
                    string sqlcount = "select count(*) from AppBannerRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause;

                    int totalcount = context.Database.SqlQuery<int>(sqlcount).FirstOrDefault();
                    List<GrowBusinessHistoryDc> newdata = context.Database.SqlQuery<GrowBusinessHistoryDc>(sqlquery).ToList();
                    appbannerexport = Mapper.Map(newdata).ToANew<List<AppBannerHistoryExportDc>>();
                    #region view and click count 
                    List<int?> ObjectIds = appbannerexport.Where(x => x.IsApproved == true).Select(x => x.ObjectId).ToList();
                    if (ObjectIds != null && ObjectIds.Any())
                    {

                        List<ActionCountDc> _result = GetActionCount(ObjectIds, filter.RequestType);
                        if (_result != null && _result.Any())
                        {
                            foreach (var item in appbannerexport.Where(x => x.IsApproved == true && ObjectIds.Contains(x.ObjectId)))
                            {
                                item.TotalViews = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalViews;
                                item.TotalSent = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalSent;
                                item.TotalReceived = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalReceived;

                            }
                        }

                    }
                    #endregion
                    res.AppBannerHistoryExportDcs = appbannerexport;
                }
                if (filter.RequestType == 2) //Notification
                {
                    string whereclause = "";

                    if (SubCatId > 0)
                    {
                        whereclause += " and b.SubCatId =" + SubCatId;
                    }
                    if (filter.WarehouseIds != null && filter.WarehouseIds.Count > 0)
                    {
                        var warehouseresult = string.Join(",", filter.WarehouseIds);
                        whereclause += " and b.WarehouseId in (" + warehouseresult + ")";
                    }
                    if (filter.startDate.HasValue && filter.endDate.HasValue)
                    {
                        whereclause += " and (b.CreatedDate >= " + "'" + filter.startDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "'" + " And  b.CreatedDate <=" + "'" + filter.endDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "')";
                    }
                    string sqlquery = "select p.DisplayName as CreatedbyName,Isnull(ps.DisplayName,'') as ModifiedByName, b.Id,b.ImageUrl,b.StartDate,b.EndDate,b.NotificationTitle,b.NotificationDescription,b.NotificationImage,b.WarehouseId,b.Comment,b.ApprovedDate,b.IsApproved,b.ApprovedBy,b.CreatedDate,b.ModifiedDate,b.IsActive,b.IsDeleted,b.CreatedBy,b.ModifiedBy,b.SubCatId,b.NotificationDescription,b.Status,b.ObjectId,w.WarehouseName,s.SubcategoryName as SubCatName from NotificationRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId join People as p on p.PeopleID=b.CreatedBy left join People as ps on ps.PeopleID=b.ModifiedBy where b.IsDeleted=0 " + whereclause;
                    string sqlcount = "select count(*) from NotificationRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause;
                    int totalcount = context.Database.SqlQuery<int>(sqlcount).FirstOrDefault();
                    List<GrowBusinessHistoryDc> newdata = context.Database.SqlQuery<GrowBusinessHistoryDc>(sqlquery).ToList();
                    notificationexport = Mapper.Map(newdata).ToANew<List<NotificationHistoryExportDc>>();
                    #region view and click count 
                    List<int?> ObjectIds = notificationexport.Where(x => x.IsApproved == true).Select(x => x.ObjectId).ToList();
                    if (ObjectIds != null && ObjectIds.Any())
                    {

                        List<ActionCountDc> _result = GetActionCount(ObjectIds, filter.RequestType);
                        if (_result != null && _result.Any())
                        {
                            foreach (var item in notificationexport.Where(x => x.IsApproved == true && ObjectIds.Contains(x.ObjectId)))
                            {
                                item.TotalViews = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalViews;
                                item.TotalSent = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalSent;
                                item.TotalReceived = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalReceived;

                            }
                        }

                    }
                    #endregion
                    res.NotificationHistoryExportDcs = notificationexport;
                }
                if (filter.RequestType == 3) //Flash deal
                {
                    string whereclause = "";

                    if (SubCatId > 0)
                    {
                        whereclause += " and b.SubCatId =" + SubCatId;
                    }
                    if (filter.WarehouseIds != null && filter.WarehouseIds.Count > 0)
                    {
                        var warehouseresult = string.Join(",", filter.WarehouseIds);
                        whereclause += " and b.WarehouseId in (" + warehouseresult + ")";
                    }
                    if (filter.startDate.HasValue && filter.endDate.HasValue)
                    {
                        whereclause += " and (b.CreatedDate >= " + "'" + filter.startDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "'" + " And  b.CreatedDate <=" + "'" + filter.endDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "')";
                    }
                    string sqlquery = "select p.DisplayName as CreatedbyName,Isnull(ps.DisplayName,'') as ModifiedByName, b.Id,b.ImageUrl,i.StartDate,i.EndDate,b.WarehouseId,i.Moq,i.AvailableQty,i.MaxQty,i.FlashDealPrice,b.Comment,b.ApprovedDate,b.IsApproved,b.ApprovedBy,b.CreatedDate,b.ModifiedDate,b.IsActive,b.IsDeleted,b.CreatedBy,b.ModifiedBy,b.SubCatId,i.ItemMultiMrpId,i.ItemName,b.Status,b.ObjectId,w.WarehouseName,s.SubcategoryName as SubCatName from FlashDealRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId join People as p on p.PeopleID=b.CreatedBy left join People as ps on ps.PeopleID=b.ModifiedBy join FlashDealRequestItems as i on i.FlashDealRequestMasterId=b.Id where b.IsDeleted=0 " + whereclause;
                    string sqlcount = "select count(*) from FlashDealRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause;

                    int totalcount = context.Database.SqlQuery<int>(sqlcount).FirstOrDefault();
                    List<GrowBusinessHistoryDc> newdata = context.Database.SqlQuery<GrowBusinessHistoryDc>(sqlquery).ToList();
                    flashdealexport = Mapper.Map(newdata).ToANew<List<FlashDealHistoryExportDc>>();

                    var MultiMrpId = flashdealexport.Select(x => x.ItemMultiMrpId).Distinct().ToList();
                    var MRP = context.itemMasters.Where(x => MultiMrpId.Contains(x.ItemMultiMRPId)).ToList();
                    flashdealexport.ForEach(x =>
                    {
                        x.MRP = MRP.Where(y => y.ItemMultiMRPId == x.ItemMultiMrpId).Select(y => y.MRP).FirstOrDefault();
                    });
                    //#region view and click count 
                    //List<int?> ObjectIds = flashdealexport.Where(x => x.IsApproved == true).Select(x => x.ObjectId).ToList();
                    //if (ObjectIds != null && ObjectIds.Any())
                    //{

                    //    List<ActionCountDc> _result = GetActionCount(ObjectIds, filter.RequestType);
                    //    if (_result != null && _result.Any())
                    //    {
                    //        foreach (var item in flashdealexport.Where(x => x.IsApproved == true && ObjectIds.Contains(x.ObjectId)))
                    //        {
                    //            item.TotalViews = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalViews;
                    //            item.TotalSent = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalSent;
                    //            item.TotalReceived = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalReceived;

                    //        }
                    //    }

                    //}
                    //#endregion
                    res.FlashDealHistoryExportDcs = flashdealexport;
                }
                if (filter.RequestType == 4) //Brand Store
                {
                    string whereclause = "";

                    if (SubCatId > 0)
                    {
                        whereclause += " and b.SubCatId =" + SubCatId;
                    }
                    if (filter.WarehouseIds != null && filter.WarehouseIds.Count > 0)
                    {
                        var warehouseresult = string.Join(",", filter.WarehouseIds);
                        whereclause += " and b.WarehouseId in (" + warehouseresult + ")";
                    }
                    if (filter.startDate.HasValue && filter.endDate.HasValue)
                    {
                        whereclause += " and (b.CreatedDate >= " + "'" + filter.startDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "'" + " And  b.CreatedDate <=" + "'" + filter.endDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "')";
                    }
                    string sqlquery = "select p.DisplayName as CreatedbyName,Isnull(ps.DisplayName,'') as ModifiedByName, b.Id,b.ImageUrl,b.CategoryId,b.StartDate,b.EndDate,b.WarehouseId,b.Comment,b.ApprovedDate,b.IsApproved,b.ApprovedBy,b.CreatedDate,b.ModifiedDate,b.IsActive,b.IsDeleted,b.CreatedBy,b.ModifiedBy,b.SubCatId,b.Status,b.ObjectId,w.WarehouseName,s.SubcategoryName as SubCatName,c.CategoryName from BrandStoreRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join Categories as c on c.Categoryid=b.CategoryId join SubCategories as s on s.SubCategoryId = b.subcatId join People as p on p.PeopleID=b.CreatedBy left join People as ps on ps.PeopleID=b.ModifiedBy where b.IsDeleted=0 " + whereclause;
                    string sqlcount = "select count(*) from BrandStoreRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join Categories as c on c.Categoryid=b.CategoryId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause;

                    int totalcount = context.Database.SqlQuery<int>(sqlcount).FirstOrDefault();
                    List<GrowBusinessHistoryDc> newdata = context.Database.SqlQuery<GrowBusinessHistoryDc>(sqlquery).ToList();
                    brandexport = Mapper.Map(newdata).ToANew<List<BrandStoreRHistoryExportDc>>();
                    #region view and click count 
                    List<int?> ObjectIds = brandexport.Where(x => x.IsApproved == true).Select(x => x.ObjectId).ToList();
                    if (ObjectIds != null && ObjectIds.Any())
                    {

                        List<ActionCountDc> _result = GetActionCount(ObjectIds, filter.RequestType);
                        if (_result != null && _result.Any())
                        {
                            foreach (var item in brandexport.Where(x => x.IsApproved == true && ObjectIds.Contains(x.ObjectId)))
                            {
                                item.TotalViews = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalViews;
                                item.TotalSent = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalSent;
                                item.TotalReceived = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalReceived;
                            }
                        }
                    }
                    #endregion
                    res.BrandStoreRHistoryExportDcs = brandexport;
                }
                if (filter.RequestType == 5) // Murli
                {
                    string whereclause = "";

                    if (SubCatId > 0)
                    {
                        whereclause += " and b.SubCatId =" + SubCatId;
                    }
                    if (filter.WarehouseIds != null && filter.WarehouseIds.Count > 0)
                    {
                        var warehouseresult = string.Join(",", filter.WarehouseIds);
                        whereclause += " and b.WarehouseId in (" + warehouseresult + ")";
                    }
                    if (filter.startDate.HasValue && filter.endDate.HasValue)
                    {
                        whereclause += " and (b.CreatedDate >= " + "'" + filter.startDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "'" + " And  b.CreatedDate <=" + "'" + filter.endDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "')";
                    }
                    string sqlquery = "select p.DisplayName as CreatedbyName,Isnull(ps.DisplayName,'') as ModifiedByName, Id,b.StartDate,b.EndDate,b.MurliNotificationMsg,b.MurliDescription,b.MurliNotificationTitle,b.MurliFile,b.WarehouseId,b.Comment,b.ApprovedDate,b.IsApproved,b.ApprovedBy,b.CreatedDate,b.ModifiedDate,b.IsActive,b.IsDeleted,b.CreatedBy,b.ModifiedBy,b.SubCatId,b.MurliDescription,b.Status,b.ObjectId,w.WarehouseName,s.SubcategoryName as SubCatName from MurliRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId join People as p on p.PeopleID=b.CreatedBy left join People as ps on ps.PeopleID=b.ModifiedBy where b.IsDeleted=0 " + whereclause;
                    string sqlcount = "select count(*) from MurliRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause;
                    int totalcount = context.Database.SqlQuery<int>(sqlcount).FirstOrDefault();
                    List<GrowBusinessHistoryDc> newdata = context.Database.SqlQuery<GrowBusinessHistoryDc>(sqlquery).ToList();
                    murliexport = Mapper.Map(newdata).ToANew<List<MurliHistoryExportDc>>();
                    #region view and click count 
                    List<int?> ObjectIds = murliexport.Where(x => x.IsApproved == true).Select(x => x.ObjectId).ToList();
                    if (ObjectIds != null && ObjectIds.Any())
                    {

                        List<ActionCountDc> _result = GetActionCount(ObjectIds, filter.RequestType);
                        if (_result != null && _result.Any())
                        {
                            foreach (var item in murliexport.Where(x => x.IsApproved == true && ObjectIds.Contains(x.ObjectId)))
                            {
                                item.TotalViews = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalViews;
                                item.TotalSent = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalSent;
                                item.TotalReceived = _result.FirstOrDefault(x => x.ObjectId == item.ObjectId).TotalReceived;
                            }
                        }
                    }
                    #endregion
                    res.MurliHistoryExportDcs = murliexport;
                }
                res.Result = true;
                res.msg = "success";
            }
            return res;
        }

        [Route("CancleRequest")]
        [HttpGet]
        public bool CancleRequest(int RequestType, int Id)
        {
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                if (RequestType == 1) //AppBanner
                {
                    var list = db.AppBannerRequests.Where(x => x.Id == Id && (x.Status == "Pending" || x.Status == "Reject")).FirstOrDefault();
                    if (list != null)
                    {
                        list.Status = "Cancel";
                        list.ModifiedBy = userid;
                        list.ModifiedDate = DateTime.Now;
                        db.Entry(list).State = EntityState.Modified;
                    }
                }
                if (RequestType == 2) //Notification
                {
                    var list = db.NotificationRequests.Where(x => x.Id == Id && (x.Status == "Pending" || x.Status == "Reject")).FirstOrDefault();
                    if (list != null)
                    {
                        list.Status = "Cancel";
                        list.ModifiedBy = userid;
                        list.ModifiedDate = DateTime.Now;
                        db.Entry(list).State = EntityState.Modified;
                    }
                }
                if (RequestType == 3) //Flash deal
                {
                    var list = db.FlashDealRequests.Where(x => x.Id == Id && (x.Status == "Pending" || x.Status == "Reject")).FirstOrDefault();
                    if (list != null)
                    {
                        list.Status = "Cancel";
                        list.ModifiedBy = userid;
                        list.ModifiedDate = DateTime.Now;
                        db.Entry(list).State = EntityState.Modified;
                    }
                }
                if (RequestType == 4) //Brand Store
                {
                    var list = db.BrandStoreRequests.Where(x => x.Id == Id && (x.Status == "Pending" || x.Status == "Reject")).FirstOrDefault();
                    if (list != null)
                    {
                        list.Status = "Cancel";
                        list.ModifiedBy = userid;
                        list.ModifiedDate = DateTime.Now;
                        db.Entry(list).State = EntityState.Modified;
                    }
                }
                if (RequestType == 5) // Murli
                {
                    var list = db.MurliRequests.Where(x => x.Id == Id && (x.Status == "Pending" || x.Status == "Reject")).FirstOrDefault();
                    if (list != null)
                    {
                        list.Status = "Cancel";
                        list.ModifiedBy = userid;
                        list.ModifiedDate = DateTime.Now;
                        db.Entry(list).State = EntityState.Modified;
                    }
                }
                if (db.Commit() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        [Route("GetSectionpreview")]
        [HttpGet]
        public List<AppHomeSectionsDc> GetSectionpreview(int requestId, string RequestType)
        {
            List<AppHomeSectionsDc> result = new List<AppHomeSectionsDc>();
            if (requestId > 0 && RequestType != null)
            {
                SellerRequestHelper helper = new SellerRequestHelper();
                result = helper.GetSectionpreview(requestId, RequestType);
            }
            return result;
        }

        public List<ActionCountDc> GetActionCount(List<int?> ObjIds, int RequestType)
        {
            List<ActionCountDc> _result = new List<ActionCountDc>();
            using (var context = new AuthContext())
            {
                if (RequestType > 0 && ObjIds != null && ObjIds.Any())
                {
                    var ObjectIds = new DataTable();
                    ObjectIds.Columns.Add("IntValue");
                    foreach (var item in ObjIds)
                    {
                        var dr = ObjectIds.NewRow();
                        dr["IntValue"] = item;
                        ObjectIds.Rows.Add(dr);
                    }
                    var param = new SqlParameter("param", ObjectIds);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";

                    var param2 = new SqlParameter("param2", RequestType);

                    _result = context.Database.SqlQuery<ActionCountDc>("exec [GetActionCount] @param, @param2", param, param2).ToList();
                }
            }
            return _result;

        }
        [Route("DeleteItemRow")]
        [HttpGet]
        public IHttpActionResult DeleteItemRow(int Id)
        {
            using (var context = new AuthContext())
            {
                FlashDealRequestItem itemRow = context.FlashDealRequestItems.Find(Id);
                if (itemRow != null)
                {
                    context.FlashDealRequestItems.Remove(itemRow);
                    context.Commit();
                }

            }
            return Ok();
        }
    }
}


public class BussinessHistoryFilterDc
{
    public int RequestType { get; set; }
    public int skip { get; set; }
    public int take { get; set; }
    public List<int> WarehouseIds { get; set; }
    public DateTime? startDate { get; set; }
    public DateTime? endDate { get; set; }
    public string statusValue { get; set; }
}

public class BussExportFilterDc
{
    public int RequestType { get; set; }
    public int skip { get; set; }
    public int take { get; set; }
    public List<int> WarehouseIds { get; set; }
    public DateTime? startDate { get; set; }
    public DateTime? endDate { get; set; }
}


public class ActionCountDc
{
    public int? TotalViews { get; set; }
    public int? TotalReceived { get; set; }
    public int? TotalSent { get; set; }
    public int? ObjectId { get; set; }

}


