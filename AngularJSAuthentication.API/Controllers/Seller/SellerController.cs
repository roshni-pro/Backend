using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.ElasticSearch;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Masters.Seller;
using AngularJSAuthentication.DataContracts.Masters.Store;
using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using AngularJSAuthentication.Model;
using Elasticsearch.Net;
using MongoDB.Bson;
using MongoDB.Driver;
using Nest;
using Nito.AsyncEx;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using static AngularJSAuthentication.API.Controllers.WarehouseController;

namespace AngularJSAuthentication.API.Controllers.Seller
{
    [RoutePrefix("api/Seller")]
    public class SellerController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        readonly string platformIdxName = $"skorderdata_{AppConstants.Environment}";
        readonly string orderdatajobdate = $"skorderdataschedule_{AppConstants.Environment}";

        [Route("PeopleSubCat/{PeopleId}")]
        [HttpGet]
        public async Task<List<PeopleSubCatMappingDc>> PeopleSubCatMapping(int PeopleId)
        {
            List<PeopleSubCatMappingDc> result = new List<PeopleSubCatMappingDc>();
            if (PeopleId > 0)
            {
                using (var context = new AuthContext())
                {
                    string sqlquery = "Exec PeopleSubCatMapping " + PeopleId;
                    result = await context.Database.SqlQuery<PeopleSubCatMappingDc>(sqlquery).ToListAsync();
                    return result;
                }
            }
            return result;
        }

        [Route("UpdatePeopleSubCatMapping")]
        [HttpPost]
        public async Task<string> UpdatePeopleSubCatMapping(List<PeopleSubCatMappingDc> PeopleSubList)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            string result = "";
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (userid > 0 && PeopleSubList != null && PeopleSubList.Count > 0 && PeopleSubList.Any())
            {
                List<PeopleSubcatMapping> Addlist = new List<PeopleSubcatMapping>();
                List<PeopleSubcatMapping> Updatelist = new List<PeopleSubcatMapping>();
                ///code here
                using (var db = new AuthContext())
                {
                    var peopleid = PeopleSubList.FirstOrDefault().PeopleId;
                    var people = db.Peoples.FirstOrDefault(x => x.PeopleID == peopleid && x.Deleted == false);
                    if (people != null)
                    {
                        var oldSubCatList = db.PeopleSubcatMappings.Where(x => x.PeopleId == peopleid && x.IsDeleted == false).ToList();
                        var DeleteSubcatids = oldSubCatList.Where(p => PeopleSubList.All(p2 => p2.SubCategoryId != p.SubCatId)).Select(x => x.SubCatId);
                        var DeletedList = oldSubCatList.Where(x => DeleteSubcatids.Contains(x.SubCatId)).ToList();
                        if (DeletedList != null && DeletedList.Any())
                        {
                            Updatelist.AddRange(DeletedList);
                        }

                        var AddSubcatids = PeopleSubList.Where(p => oldSubCatList.All(p2 => p2.SubCatId != p.SubCategoryId)).Select(x => x.SubCategoryId);
                        var AddList = PeopleSubList.Where(x => AddSubcatids.Contains(x.SubCategoryId)).ToList();
                        foreach (var i in AddList)
                        {
                            PeopleSubcatMapping obj = new PeopleSubcatMapping();
                            obj.SubCatId = i.SubCategoryId;
                            obj.PeopleId = i.PeopleId;
                            obj.IsActive = true;
                            obj.IsDeleted = false;
                            obj.CreatedDate = DateTime.Now;
                            obj.CreatedBy = userid;
                            Addlist.Add(obj);
                        }
                        foreach (var item in Updatelist)
                        {
                            item.IsDeleted = true;
                            item.ModifiedBy = userid;
                            item.ModifiedDate = indianTime;
                            db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        }
                        db.PeopleSubcatMappings.AddRange(Addlist);

                        if (people.IsSellerPortallogin == false || people.IsSellerPortallogin == null)
                        {
                            people.IsSellerPortallogin = true;
                            people.UpdatedDate = indianTime;
                            db.Entry(people).State = System.Data.Entity.EntityState.Modified;
                        }
                        if (db.Commit() > 0) { result = "Updated Successfully"; } else { result = "Something went wrong"; }
                    }
                    else
                    {
                        result = "User not found";
                    }
                }
            }
            else { result = "Mapping list is null"; }
            return result;
        }

        [Route("Mapping")]
        [HttpGet]
        public async Task<List<PeopleSubCatMappingDc>> GetMappingList()
        {
            var Ids = GetReqParam();

            List<PeopleSubCatMappingDc> result = new List<PeopleSubCatMappingDc>();
            if (Ids.PeopleId > 0)
            {
                using (var context = new AuthContext())
                {
                    PeopleManager manager = new PeopleManager();
                    result = await manager.GetPeopleSubCatMapping(Ids.PeopleId);
                }
            }
            return result;
        }

        [Route("GetCatelogueItemWithCFR/{CityId}/{WarehouseId}")]
        [HttpGet]
        public async Task<GetCatelogueItemWithCFRDc> GetCatelogueItemWithCFR(int CityId, int WarehouseId)
        {
            var Ids = GetReqParam();
            GetCatelogueItemWithCFRDc result = new GetCatelogueItemWithCFRDc();
            if (Ids.PeopleId > 0 && Ids.SubCatId > 0 && CityId > 0)
            {
                using (var context = new AuthContext())
                {

                    List<Object> parameters = new List<object>();
                    string sqlquery = "exec Seller.GetCatelogueItemWithCFR";

                    parameters.Add(new SqlParameter("@SubCategoryId", Ids.SubCatId));
                    parameters.Add(new SqlParameter("@CityId", CityId));
                    parameters.Add(new SqlParameter("@WarehouseId", WarehouseId));
                    sqlquery = sqlquery + " @SubCategoryId" + ", @CityId" + ", @WarehouseId";

                    result = await context.Database.SqlQuery<GetCatelogueItemWithCFRDc>(sqlquery, parameters.ToArray()).FirstOrDefaultAsync();
                }
            }
            return result;
        }

        [Route("GetSellerSales")]
        [HttpPost]
        public async Task<GetSellerSalesDc> GetSellerSales(SearchReqDc SearchReq)
        {
            var Ids = GetReqParam();
            GetSellerSalesDc result = new GetSellerSalesDc();
            if (Ids.PeopleId > 0 && Ids.SubCatId > 0 && SearchReq != null && SearchReq.CityIds.Any())
            {
                using (var context = new AuthContext())
                {
                    var SubCategoryId = new SqlParameter("@SubCategoryId", Ids.SubCatId);
                    var cityidlist = new DataTable();
                    cityidlist.Columns.Add("IntValue");
                    foreach (var item in SearchReq.CityIds)
                    {
                        var dr = cityidlist.NewRow();
                        dr["IntValue"] = item;
                        cityidlist.Rows.Add(dr);
                    }
                    var CityIds = new SqlParameter("CityIds", cityidlist);
                    CityIds.SqlDbType = SqlDbType.Structured;
                    CityIds.TypeName = "dbo.IntValues";

                    result = await context.Database.SqlQuery<GetSellerSalesDc>("exec Seller.GetSellerSales @SubCategoryId, @CityIds ", SubCategoryId, CityIds).FirstOrDefaultAsync();
                }
            }
            return result;
        }

        [Route("DashboardPoStatusCount")]
        [HttpPost]
        public async Task<DashboardPoStatusCount> DashboardPoStatusCount(SearchReqDc SearchReq)
        {
            var Ids = GetReqParam();

            DashboardPoStatusCount result = new DashboardPoStatusCount();
            if (Ids.PeopleId > 0 && Ids.SubCatId > 0)
            {
                using (var context = new AuthContext())
                {

                    var SubCategoryId = new SqlParameter("@SubCategoryId", Ids.SubCatId);
                    var startDate = new SqlParameter("@startDate", SearchReq.FromDate);
                    var enddate = new SqlParameter("@enddate", SearchReq.ToDate);
                    var cityidlist = new DataTable();
                    cityidlist.Columns.Add("IntValue");
                    foreach (var item in SearchReq.CityIds)
                    {
                        var dr = cityidlist.NewRow();
                        dr["IntValue"] = item;
                        cityidlist.Rows.Add(dr);
                    }
                    var CityIds = new SqlParameter("CityIds", cityidlist);
                    CityIds.SqlDbType = SqlDbType.Structured;
                    CityIds.TypeName = "dbo.IntValues";

                    List<DashboardPoStatusCount> res = await context.Database.SqlQuery<DashboardPoStatusCount>("exec Seller.DashboardPoStatusCount @SubCategoryId, @CityIds,@startDate, @enddate ", SubCategoryId, CityIds, startDate, enddate).ToListAsync();

                    result.PendingPOCount = res != null && res.Any() ? res.Where(x => x.PendingPOCount > 0)?.Sum(x => x.PendingPOCount) ?? 0 : 0;
                    result.PendingPOAmount = res != null && res.Any() ? res.Where(x => x.PendingPOAmount > 0)?.Sum(x => x.PendingPOAmount) ?? 0 : 0;
                    result.ClosedPOCount = res != null && res.Any() ? res.Where(x => x.ClosedPOCount > 0)?.Sum(x => x.ClosedPOCount) ?? 0 : 0;
                    result.ClosedPOAmount = res != null && res.Any() ? res.Where(x => x.ClosedPOAmount > 0)?.Sum(x => x.ClosedPOAmount) ?? 0 : 0;
                    result.PartialPOCount = res != null && res.Any() ? res.Where(x => x.PartialPOCount > 0)?.Sum(x => x.PartialPOCount) ?? 0 : 0;
                    result.PartialPOAmount = res != null && res.Any() ? res.Where(x => x.PartialPOAmount > 0)?.Sum(x => x.PartialPOAmount) ?? 0 : 0;
                    result.CancelPOCount = res != null && res.Any() ? res.Where(x => x.CancelPOCount > 0)?.Sum(x => x.CancelPOCount) ?? 0 : 0;
                    result.CancelPOAmount = res != null && res.Any() ? res.Where(x => x.CancelPOAmount > 0)?.Sum(x => x.CancelPOAmount) ?? 0 : 0;
                }
            }
            return result;
        }

        [Route("DashboardOrderStatusData")]
        [HttpPost]
        public async Task<DashboardOrderStatusDataCount> DashboardOrderStatusData(SearchReqDc SearchReq)
        {
            var Ids = GetReqParam();

            DashboardOrderStatusDataCount result = new DashboardOrderStatusDataCount();
            if (Ids.PeopleId > 0 && Ids.SubCatId > 0)
            {
                using (var context = new AuthContext())
                {
                    var SubCategoryId = new SqlParameter("@SubCategoryId", Ids.SubCatId);
                    var startDate = new SqlParameter("@startDate", SearchReq.FromDate);
                    var enddate = new SqlParameter("@enddate", SearchReq.ToDate);
                    var cityidlist = new DataTable();
                    cityidlist.Columns.Add("IntValue");
                    foreach (var item in SearchReq.CityIds)
                    {
                        var dr = cityidlist.NewRow();
                        dr["IntValue"] = item;
                        cityidlist.Rows.Add(dr);
                    }
                    var CityIds = new SqlParameter("CityIds", cityidlist);
                    CityIds.SqlDbType = SqlDbType.Structured;
                    CityIds.TypeName = "dbo.IntValues";
                    var res = await context.Database.SqlQuery<DashboardOrderStatusDataDc>("exec Seller.DashboardOrderStatusData @SubCategoryId, @CityIds,@startDate,@enddate ", SubCategoryId, CityIds, startDate, enddate).ToListAsync();
                    if (res != null && res.Any())
                    {
                        result.PendingOrdercount = res != null && res.Any() ? res.Where(x => x.Status == "Pending" || x.Status == "ReadyToPick" || x.Status == "InTransit")?.Sum(x => x.Ordercount) ?? 0 : 0;
                        result.ReadytoDispatchOrdercount = res != null && res.Any() ? res.Where(x => x.Status == "Ready to Dispatch")?.Sum(x => x.Ordercount) ?? 0 : 0;
                        result.IssuedOrdercount = res != null && res.Any() ? res.Where(x => x.Status == "Issued")?.Sum(x => x.Ordercount) ?? 0 : 0;
                        result.ShippedOrdercount = res != null && res.Any() ? res.Where(x => x.Status == "Shipped")?.Sum(x => x.Ordercount) ?? 0 : 0;
                        result.DeliveredOrdercount = res != null && res.Any() ? res.Where(x => x.Status == "Delivered" || x.Status == "sattled")?.Sum(x => x.Ordercount) ?? 0 : 0;
                        result.DeliveryRedispatchOrdercount = res != null && res.Any() ? res.Where(x => x.Status == "Delivery Redispatch")?.Sum(x => x.Ordercount) ?? 0 : 0;
                        result.DeliveryCanceledOrdercount = res != null && res.Any() ? res.Where(x => x.Status == "Delivery Canceled" || x.Status == "Post Order Canceled")?.Sum(x => x.Ordercount) ?? 0 : 0;
                        result.PreCanceledOrdercount = res != null && res.Any() ? res.Where(x => x.Status == "Order Canceled")?.Sum(x => x.Ordercount) ?? 0 : 0;

                        result.PendingOrderAmount = res != null && res.Any() ? res.Where(x => x.Status == "Pending" || x.Status == "ReadyToPick" || x.Status == "InTransit")?.Sum(x => x.Sales) ?? 0 : 0;
                        result.ReadytoDispatchOrderAmount = res != null && res.Any() ? res.Where(x => x.Status == "Ready to Dispatch")?.Sum(x => x.Sales) ?? 0 : 0;
                        result.IssuedOrderAmount = res != null && res.Any() ? res.Where(x => x.Status == "Issued")?.Sum(x => x.Sales) ?? 0 : 0;
                        result.ShippedOrderAmount = res != null && res.Any() ? res.Where(x => x.Status == "Shipped")?.Sum(x => x.Sales) ?? 0 : 0;
                        result.DeliveredOrderAmount = res != null && res.Any() ? res.Where(x => x.Status == "Delivered" || x.Status == "sattled")?.Sum(x => x.Sales) ?? 0 : 0;
                        result.DeliveryRedispatchOrderAmount = res != null && res.Any() ? res.Where(x => x.Status == "Delivery Redispatch")?.Sum(x => x.Sales) ?? 0 : 0;
                        result.DeliveryCanceledOrderAmount = res != null && res.Any() ? res.Where(x => x.Status == "Delivery Canceled" || x.Status == "Post Order Canceled")?.Sum(x => x.Sales) ?? 0 : 0;
                        result.PreCanceledOrderAmount = res != null && res.Any() ? res.Where(x => x.Status == "Order Canceled")?.Sum(x => x.Sales) ?? 0 : 0;

                    }
                }
            }
            return result;
        }

        [Route("DashboardOrderFillRate")]
        [HttpPost]
        public async Task<FillRateDc> DashboardOrderFillRate(SearchReqDc SearchReq)
        {
            var Ids = GetReqParam();

            FillRateDc result = new FillRateDc();
            if (Ids.PeopleId > 0 && Ids.SubCatId > 0)
            {

                using (var context = new AuthContext())
                {

                    var SubCategoryId = new SqlParameter("@SubCategoryId", Ids.SubCatId);
                    var startDate = new SqlParameter("@startDate", SearchReq.FromDate);
                    var enddate = new SqlParameter("@enddate", SearchReq.ToDate);
                    var cityidlist = new DataTable();
                    cityidlist.Columns.Add("IntValue");
                    foreach (var item in SearchReq.CityIds)
                    {
                        var dr = cityidlist.NewRow();
                        dr["IntValue"] = item;
                        cityidlist.Rows.Add(dr);
                    }
                    var CityIds = new SqlParameter("CityIds", cityidlist);
                    CityIds.SqlDbType = SqlDbType.Structured;
                    CityIds.TypeName = "dbo.IntValues";
                    result = await context.Database.SqlQuery<FillRateDc>("exec Seller.DashboardOrderFillRate @SubCategoryId, @CityIds,@startDate,@enddate ", SubCategoryId, CityIds, startDate, enddate).FirstOrDefaultAsync();
                }

            }
            return result;
        }


        [Route("POFillRate")]
        [HttpPost]
        public async Task<FillRateDc> POFillRate(SearchReqDc SearchReq)
        {
            var Ids = GetReqParam();

            FillRateDc result = new FillRateDc();
            if (Ids.PeopleId > 0 && Ids.SubCatId > 0)
            {

                using (var context = new AuthContext())
                {

                    var SubCategoryId = new SqlParameter("@SubCategoryId", Ids.SubCatId);
                    var startDate = new SqlParameter("@startDate", SearchReq.FromDate);
                    var enddate = new SqlParameter("@enddate", SearchReq.ToDate);
                    var cityidlist = new DataTable();
                    cityidlist.Columns.Add("IntValue");
                    foreach (var item in SearchReq.CityIds)
                    {
                        var dr = cityidlist.NewRow();
                        dr["IntValue"] = item;
                        cityidlist.Rows.Add(dr);
                    }
                    var CityIds = new SqlParameter("CityIds", cityidlist);
                    CityIds.SqlDbType = SqlDbType.Structured;
                    CityIds.TypeName = "dbo.IntValues";
                    result = await context.Database.SqlQuery<FillRateDc>("exec Seller.POFillRate @SubCategoryId, @CityIds,@startDate,@enddate ", SubCategoryId, CityIds, startDate, enddate).FirstOrDefaultAsync();

                }

            }
            return result;
        }


        [Route("DashboardOrderAvgTAT")]
        [HttpPost]
        public async Task<DashboardOrderAvgTATDc> DashboardOrderAvgTAT(SearchReqDc SearchReq)
        {
            var Ids = GetReqParam();

            DashboardOrderAvgTATDc result = new DashboardOrderAvgTATDc();
            if (Ids.PeopleId > 0 && Ids.SubCatId > 0)
            {
                using (var context = new AuthContext())
                {

                    var SubCategoryId = new SqlParameter("@SubCategoryId", Ids.SubCatId);
                    var startDate = new SqlParameter("@startDate", SearchReq.FromDate);
                    var enddate = new SqlParameter("@enddate", SearchReq.ToDate);
                    var cityidlist = new DataTable();
                    cityidlist.Columns.Add("IntValue");
                    foreach (var item in SearchReq.CityIds)
                    {
                        var dr = cityidlist.NewRow();
                        dr["IntValue"] = item;
                        cityidlist.Rows.Add(dr);
                    }
                    var CityIds = new SqlParameter("CityIds", cityidlist);
                    CityIds.SqlDbType = SqlDbType.Structured;
                    CityIds.TypeName = "dbo.IntValues";
                    result = await context.Database.SqlQuery<DashboardOrderAvgTATDc>("exec Seller.DashboardOrderAvgTAT @SubCategoryId, @CityIds,@startDate,@enddate ", SubCategoryId, CityIds, startDate, enddate).FirstOrDefaultAsync();

                }
            }
            return result;
        }

        [Route("DashboardCurrentVsNetCurrent")]
        [HttpPost]
        public async Task<DashboardCurrentVsNetCurrentDc> DashboardCurrentVsNetCurrent(SearchReqDc SearchReq)
        {
            var Ids = GetReqParam();

            DashboardCurrentVsNetCurrentDc result = new DashboardCurrentVsNetCurrentDc();
            if (Ids.PeopleId > 0 && Ids.SubCatId > 0)
            {
                using (var context = new AuthContext())
                {

                    var SubCategoryId = new SqlParameter("@SubCategoryId", Ids.SubCatId);
                    var startDate = new SqlParameter("@startDate", SearchReq.FromDate);
                    var enddate = new SqlParameter("@enddate", SearchReq.ToDate);
                    var cityidlist = new DataTable();
                    cityidlist.Columns.Add("IntValue");
                    foreach (var item in SearchReq.CityIds)
                    {
                        var dr = cityidlist.NewRow();
                        dr["IntValue"] = item;
                        cityidlist.Rows.Add(dr);
                    }
                    var CityIds = new SqlParameter("CityIds", cityidlist);
                    CityIds.SqlDbType = SqlDbType.Structured;
                    CityIds.TypeName = "dbo.IntValues";
                    result = await context.Database.SqlQuery<DashboardCurrentVsNetCurrentDc>("exec Seller.DashboardCurrentVsNetCurrent @SubCategoryId, @CityIds,@startDate,@enddate ", SubCategoryId, CityIds, startDate, enddate).FirstOrDefaultAsync();

                }
            }
            return result;
        }


        [Route("POAvgTAT")]
        [HttpPost]
        public async Task<POAvgTATDc> POAvgTAT(SearchReqDc SearchReq)
        {
            var Ids = GetReqParam();

            POAvgTATDc result = new POAvgTATDc();
            if (Ids.PeopleId > 0 && Ids.SubCatId > 0)
            {
                using (var context = new AuthContext())
                {


                    var SubCategoryId = new SqlParameter("@SubCategoryId", Ids.SubCatId);
                    var startDate = new SqlParameter("@startDate", SearchReq.FromDate);
                    var enddate = new SqlParameter("@enddate", SearchReq.ToDate);
                    var cityidlist = new DataTable();
                    cityidlist.Columns.Add("IntValue");
                    foreach (var item in SearchReq.CityIds)
                    {
                        var dr = cityidlist.NewRow();
                        dr["IntValue"] = item;
                        cityidlist.Rows.Add(dr);
                    }
                    var CityIds = new SqlParameter("CityIds", cityidlist);
                    CityIds.SqlDbType = SqlDbType.Structured;
                    CityIds.TypeName = "dbo.IntValues";
                    result = await context.Database.SqlQuery<POAvgTATDc>("exec Seller.POAvgTAT @SubCategoryId, @CityIds,@startDate,@enddate ", SubCategoryId, CityIds, startDate, enddate).FirstOrDefaultAsync();

                }
            }
            return result;
        }

        [Route("POGRIRCount")]
        [HttpPost]
        public async Task<POGRIRDC> GetPOGRIRCount(SearchReqDc SearchReq)
        {
            var Ids = GetReqParam();

            POGRIRDC result = new POGRIRDC();
            if (Ids.PeopleId > 0 && Ids.SubCatId > 0)
            {
                using (var context = new AuthContext())
                {

                    var SubCategoryId = new SqlParameter("@SubCategoryId", Ids.SubCatId);
                    var startDate = new SqlParameter("@startDate", SearchReq.FromDate);
                    var enddate = new SqlParameter("@enddate", SearchReq.ToDate);
                    var cityidlist = new DataTable();
                    cityidlist.Columns.Add("IntValue");
                    foreach (var item in SearchReq.CityIds)
                    {
                        var dr = cityidlist.NewRow();
                        dr["IntValue"] = item;
                        cityidlist.Rows.Add(dr);
                    }
                    var CityIds = new SqlParameter("CityIds", cityidlist);
                    CityIds.SqlDbType = SqlDbType.Structured;
                    CityIds.TypeName = "dbo.IntValues";
                    result = await context.Database.SqlQuery<POGRIRDC>("exec Seller.POGRIRCount @SubCategoryId, @CityIds,@startDate,@enddate ", SubCategoryId, CityIds, startDate, enddate).FirstOrDefaultAsync();

                }
            }
            return result;
        }



        [Route("Brand")] //SubCategory
        [HttpGet]
        public async Task<List<SubCatMappingBrandDc>> GetSubcatBrandList()
        {
            var Ids = GetReqParam();

            List<SubCatMappingBrandDc> result = new List<SubCatMappingBrandDc>();
            if (Ids.PeopleId > 0 && Ids.SubCatId > 0)
            {
                using (var context = new AuthContext())
                {
                    PeopleManager manager = new PeopleManager();
                    result = await manager.GetSubcatBrandList(Ids.PeopleId, Ids.SubCatId);
                }
            }
            return result;
        }

        //[Route("Pareto/{type}/{itemnumber}")]
        //[HttpPost]
        //public async Task<List<ParetoIndex>> GetPareto(SearchReqDc SearchReq, int? type, string itemnumber)
        //{
        //    var Ids = GetReqParam();

        //    List<ParetoIndex> result = new List<ParetoIndex>();
        //    if (Ids.PeopleId > 0 && Ids.SubCatId > 0)
        //    {
        //        using (var context = new AuthContext())
        //        {
        //            List<Object> parameters = new List<object>();
        //            string sqlquery = "exec Seller.ParetoIndex";
        //            parameters.Add(new SqlParameter("@SubCategoryId", Ids.SubCatId));
        //            parameters.Add(new SqlParameter("@CityId", SearchReq.CityId));
        //            parameters.Add(new SqlParameter("@startDate", SearchReq.FromDate));
        //            parameters.Add(new SqlParameter("@enddate", SearchReq.ToDate));
        //            parameters.Add(new SqlParameter("@type", type));
        //            parameters.Add(new SqlParameter("@itemnumber", itemnumber));

        //            sqlquery = sqlquery + " @SubCategoryId" + ", @CityId" + ", @startDate" + ", @enddate" + ", @type" + ", @itemnumber";
        //            result = await context.Database.SqlQuery<ParetoIndex>(sqlquery, parameters.ToArray()).ToListAsync();
        //        }
        //    }
        //    return result;
        //}
        public SellerReqDc GetReqParam()
        {
            SellerReqDc SellerReq = new SellerReqDc
            {
                PeopleId = Convert.ToInt32(Request.Headers.GetValues("PeopleId").First()),
                SubCatId = Convert.ToInt32(Request.Headers.GetValues("SubCatId").First())
            };
            return SellerReq;
        }


        //Order export

        [Route("OrderDetailExport/{type}")]
        [HttpPost]
        public async Task<List<OrderDetailExportDc>> GetOrderDetailExport(SearchReqDc SearchReq, int type)
        {
            var Ids = GetReqParam();

            List<OrderDetailExportDc> result = new List<OrderDetailExportDc>();
            if (Ids.PeopleId > 0 && Ids.SubCatId > 0)
            {
                using (var context = new AuthContext())
                {
                    var SubCategoryId = new SqlParameter("@SubCategoryId", Ids.SubCatId);
                    var startDate = new SqlParameter("@startDate", SearchReq.FromDate);
                    var enddate = new SqlParameter("@enddate", SearchReq.ToDate);
                    var types = new SqlParameter("@type", type);

                    var cityidlist = new DataTable();
                    cityidlist.Columns.Add("IntValue");
                    foreach (var item in SearchReq.CityIds)
                    {
                        var dr = cityidlist.NewRow();
                        dr["IntValue"] = item;
                        cityidlist.Rows.Add(dr);
                    }
                    var CityIds = new SqlParameter("CityIds", cityidlist);
                    CityIds.SqlDbType = SqlDbType.Structured;
                    CityIds.TypeName = "dbo.IntValues";
                    result = await context.Database.SqlQuery<OrderDetailExportDc>("exec Seller.OrderDetailExport @SubCategoryId, @CityIds,@startDate,@enddate,@type", SubCategoryId, CityIds, startDate, enddate, types).ToListAsync();


                }

            }
            return result;
        }


        //[Route("LiveCfr")]
        //[HttpGet]
        //public async Task<List<SellerCFRDc>> LiveCfr()
        //{
        //    var Ids = GetReqParam();
        //    var LiveCfrList = new List<SellerCFRDc>();
        //    using (var authContext = new AuthContext())
        //    {
        //        LiveCfrList = await authContext.Database.SqlQuery<SellerCFRDc>("exec Generate_CfrArticleReport" , Ids.SubCatId).ToListAsync();
        //        if (LiveCfrList.Any())
        //        {
        //            foreach (var item in LiveCfrList)
        //            {
        //                item.LimitValue = Math.Round(item.LimitValue, 2);
        //                if (item.active == 0) { item.activeItem = "Inactive"; } else if (item.active == 1) { item.activeItem = "Active"; } else { item.activeItem = "Not Considered Active"; }
        //            }
        //        }
        //    }
        //    return LiveCfrList;
        //}


        [Route("GetWarehouseByCityids/{cityids}")]
        [HttpPost]
        public async Task<List<WareHouseDc>> GetWarehouseByCityids(PostCityidsDc PostCityidsDc)
        {
            List<WareHouseDc> result = new List<WareHouseDc>();
            if (PostCityidsDc.cityids != null && PostCityidsDc.cityids.Any())
            {
                using (AuthContext context = new AuthContext())
                {
                    var cityidlist = new DataTable();
                    cityidlist.Columns.Add("IntValue");
                    foreach (var item in PostCityidsDc.cityids)
                    {
                        var dr = cityidlist.NewRow();
                        dr["IntValue"] = item;
                        cityidlist.Rows.Add(dr);
                    }
                    var CityIds = new SqlParameter("CityIds", cityidlist);
                    CityIds.SqlDbType = SqlDbType.Structured;
                    CityIds.TypeName = "dbo.IntValues";
                    result = await context.Database.SqlQuery<WareHouseDc>("exec [Seller].[GetWarehouseByCityids] @CityIds", CityIds).ToListAsync();
                }
            }

            return result;
        }



        [Route("CatelogueItemExport/{CityId}/{WarehouseId}")]
        [HttpGet]
        public async Task<List<CatelogueItemExportDc>> GetCatelogueItemExport(int CityId, int WarehouseId)
        {
            var Ids = GetReqParam();
            List<CatelogueItemExportDc> result = new List<CatelogueItemExportDc>();
            if (Ids.PeopleId > 0 && Ids.SubCatId > 0 && CityId > 0)
            {
                using (var context = new AuthContext())
                {

                    List<Object> parameters = new List<object>();
                    string sqlquery = "exec Seller.CatelogueItemExport";

                    parameters.Add(new SqlParameter("@SubCategoryId", Ids.SubCatId));
                    parameters.Add(new SqlParameter("@CityId", CityId));
                    parameters.Add(new SqlParameter("@WarehouseId", WarehouseId));
                    sqlquery = sqlquery + " @SubCategoryId" + ", @CityId" + ", @WarehouseId";

                    result = await context.Database.SqlQuery<CatelogueItemExportDc>(sqlquery, parameters.ToArray()).ToListAsync();
                }
            }
            return result;
        }


        [Route("CatalogCFRExport/{CityId}/{WarehouseId}")]
        [HttpGet]
        public async Task<List<CatelogueItemExportDc>> CatalogCFRExport(int CityId, int WarehouseId)
        {
            var Ids = GetReqParam();
            List<CatelogueItemExportDc> result = new List<CatelogueItemExportDc>();
            if (Ids.PeopleId > 0 && Ids.SubCatId > 0 && CityId > 0)
            {
                using (var context = new AuthContext())
                {

                    List<Object> parameters = new List<object>();
                    string sqlquery = "exec Seller.CatalogCFRExport";
                    parameters.Add(new SqlParameter("@SubCategoryId", Ids.SubCatId));
                    parameters.Add(new SqlParameter("@CityId", CityId));
                    parameters.Add(new SqlParameter("@WarehouseId", WarehouseId));
                    sqlquery = sqlquery + " @SubCategoryId" + ", @CityId" + ", @WarehouseId";

                    result = await context.Database.SqlQuery<CatelogueItemExportDc>(sqlquery, parameters.ToArray()).ToListAsync();
                }
            }
            return result;
        }

        [Route("SalesExport")]
        [HttpPost]
        public async Task<List<SalesExportDc>> GetSalesExport(SearchReqDc SearchReq)
        {
            var Ids = GetReqParam();
            List<SalesExportDc> result = new List<SalesExportDc>();
            if (Ids.PeopleId > 0 && Ids.SubCatId > 0 && SearchReq != null && SearchReq.CityIds.Any())
            {
                using (var context = new AuthContext())
                {
                    var SubCategoryId = new SqlParameter("@SubCategoryId", Ids.SubCatId);
                    var cityidlist = new DataTable();
                    cityidlist.Columns.Add("IntValue");
                    foreach (var item in SearchReq.CityIds)
                    {
                        var dr = cityidlist.NewRow();
                        dr["IntValue"] = item;
                        cityidlist.Rows.Add(dr);
                    }
                    var CityIds = new SqlParameter("CityIds", cityidlist);
                    CityIds.SqlDbType = SqlDbType.Structured;
                    CityIds.TypeName = "dbo.IntValues";

                    result = await context.Database.SqlQuery<SalesExportDc>("exec Seller.GetSalesExport @SubCategoryId, @CityIds ", SubCategoryId, CityIds).ToListAsync();
                }
            }
            return result;
        }

        [Route("POFillRateExport")]
        [HttpPost]
        public async Task<List<POFillRateExportDc>> GetPOFillRateExport(SearchReqDc SearchReq)
        {
            var Ids = GetReqParam();

            List<POFillRateExportDc> result = new List<POFillRateExportDc>();
            if (Ids.PeopleId > 0 && Ids.SubCatId > 0)
            {

                using (var context = new AuthContext())
                {

                    var SubCategoryId = new SqlParameter("@SubCategoryId", Ids.SubCatId);
                    var startDate = new SqlParameter("@startDate", SearchReq.FromDate);
                    var enddate = new SqlParameter("@enddate", SearchReq.ToDate);
                    var cityidlist = new DataTable();
                    cityidlist.Columns.Add("IntValue");
                    foreach (var item in SearchReq.CityIds)
                    {
                        var dr = cityidlist.NewRow();
                        dr["IntValue"] = item;
                        cityidlist.Rows.Add(dr);
                    }
                    var CityIds = new SqlParameter("CityIds", cityidlist);
                    CityIds.SqlDbType = SqlDbType.Structured;
                    CityIds.TypeName = "dbo.IntValues";
                    result = await context.Database.SqlQuery<POFillRateExportDc>("exec Seller.POFillRateExport @SubCategoryId, @CityIds,@startDate,@enddate ", SubCategoryId, CityIds, startDate, enddate).ToListAsync();

                }

            }
            return result;
        }

        [Route("OrderFillRateExport")]
        [HttpPost]
        public async Task<List<OrderFillRateExportDc>> OrderFillRateExport(SearchReqDc SearchReq)
        {
            var Ids = GetReqParam();

            List<OrderFillRateExportDc> result = new List<OrderFillRateExportDc>();
            if (Ids.PeopleId > 0 && Ids.SubCatId > 0)
            {

                using (var context = new AuthContext())
                {

                    var SubCategoryId = new SqlParameter("@SubCategoryId", Ids.SubCatId);
                    var startDate = new SqlParameter("@startDate", SearchReq.FromDate);
                    var enddate = new SqlParameter("@enddate", SearchReq.ToDate);
                    var cityidlist = new DataTable();
                    cityidlist.Columns.Add("IntValue");
                    foreach (var item in SearchReq.CityIds)
                    {
                        var dr = cityidlist.NewRow();
                        dr["IntValue"] = item;
                        cityidlist.Rows.Add(dr);
                    }
                    var CityIds = new SqlParameter("CityIds", cityidlist);
                    CityIds.SqlDbType = SqlDbType.Structured;
                    CityIds.TypeName = "dbo.IntValues";
                    result = await context.Database.SqlQuery<OrderFillRateExportDc>("exec Seller.OrderFillRateExport @SubCategoryId, @CityIds,@startDate,@enddate ", SubCategoryId, CityIds, startDate, enddate).ToListAsync();
                }

            }
            return result;
        }

        //[Route("GetCompanyDashboard")]
        //[HttpPost]
        //public async Task<SellerDashboardData> GetCompanyDashboard(SellerDashboardRequest sellerDashboardRequest)
        //{
        //    SellerDashboardData sellerDashboardData = new SellerDashboardData { };
        //    CompanySale CurrentCompanySale = new CompanySale();
        //    CompanySale PreviouseCompanySale = new CompanySale();
        //    CompanyInventory CompanyInventory = new CompanyInventory();


        //    var taskList = new List<Task>();
        //    var task1 = Task.Factory.StartNew(() =>
        //    {
        //        using (var authContext = new AuthContext())
        //        {
        //            if (authContext.Database.Connection.State != ConnectionState.Open)
        //                authContext.Database.Connection.Open();
        //            authContext.Database.CommandTimeout = 600;

        //            var brandIdDt = new DataTable();
        //            brandIdDt.Columns.Add("IntValue");
        //            foreach (var item in sellerDashboardRequest.BrandIds)
        //            {
        //                var dr = brandIdDt.NewRow();
        //                dr["IntValue"] = item;
        //                brandIdDt.Rows.Add(dr);
        //            }

        //            var param = new SqlParameter("brandIds", brandIdDt);
        //            param.SqlDbType = SqlDbType.Structured;
        //            param.TypeName = "dbo.IntValues";
        //            var cmd = authContext.Database.Connection.CreateCommand();
        //            cmd.CommandText = "[Buyer].[GetCompanySales]";
        //            cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //            cmd.Parameters.Add(new SqlParameter("@companyId", sellerDashboardRequest.CompanyId));
        //            cmd.Parameters.Add(param);
        //            cmd.Parameters.Add(new SqlParameter("@cityId", sellerDashboardRequest.CityId));
        //            cmd.Parameters.Add(new SqlParameter("@startDate", sellerDashboardRequest.StartDate));
        //            cmd.Parameters.Add(new SqlParameter("@endDate", sellerDashboardRequest.EndDate));

        //            // Run the sproc
        //            var reader = cmd.ExecuteReader();

        //            CurrentCompanySale = ((IObjectContextAdapter)authContext)
        //            .ObjectContext
        //            .Translate<CompanySale>(reader).FirstOrDefault();
        //        }

        //    });
        //    taskList.Add(task1);
        //    var task2 = Task.Factory.StartNew(() =>
        //    {
        //        using (var authContext = new AuthContext())
        //        {
        //            if (authContext.Database.Connection.State != ConnectionState.Open)
        //                authContext.Database.Connection.Open();
        //            authContext.Database.CommandTimeout = 850;
        //            int diff = sellerDashboardRequest.EndDate.Subtract(sellerDashboardRequest.StartDate).Days;
        //            DateTime pre_startDate = new DateTime();
        //            DateTime pre_endDate = sellerDashboardRequest.StartDate.AddSeconds(-1);
        //            if (sellerDashboardRequest.DateRangeType == "T")
        //                pre_startDate = sellerDashboardRequest.StartDate.AddDays(-1);
        //            else if (sellerDashboardRequest.DateRangeType == "W")
        //                pre_startDate = sellerDashboardRequest.StartDate.AddDays(-6);
        //            else if (sellerDashboardRequest.DateRangeType == "M")
        //                pre_startDate = sellerDashboardRequest.StartDate.AddMonths(-1);
        //            else if (sellerDashboardRequest.DateRangeType == "C")
        //                pre_startDate = sellerDashboardRequest.StartDate.AddDays(-1 * diff);
        //            var brandIdDt = new DataTable();
        //            brandIdDt.Columns.Add("IntValue");
        //            foreach (var item in sellerDashboardRequest.BrandIds)
        //            {
        //                var dr = brandIdDt.NewRow();
        //                dr["IntValue"] = item;
        //                brandIdDt.Rows.Add(dr);
        //            }

        //            var param = new SqlParameter("brandIds", brandIdDt);
        //            param.SqlDbType = SqlDbType.Structured;
        //            param.TypeName = "dbo.IntValues";
        //            var cmd = authContext.Database.Connection.CreateCommand();
        //            cmd.CommandText = "[Buyer].[GetCompanySales]";
        //            cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //            cmd.Parameters.Add(new SqlParameter("@companyId", sellerDashboardRequest.CompanyId));
        //            cmd.Parameters.Add(param);
        //            cmd.Parameters.Add(new SqlParameter("@cityId", sellerDashboardRequest.CityId));
        //            cmd.Parameters.Add(new SqlParameter("@startDate", pre_startDate));
        //            cmd.Parameters.Add(new SqlParameter("@endDate", pre_endDate));

        //            // Run the sproc
        //            var reader = cmd.ExecuteReader();

        //            PreviouseCompanySale = ((IObjectContextAdapter)authContext)
        //            .ObjectContext
        //            .Translate<CompanySale>(reader).FirstOrDefault();

        //        }
        //    });
        //    taskList.Add(task2);

        //    var task3 = Task.Factory.StartNew(() =>
        //    {
        //        using (var authContext = new AuthContext())
        //        {
        //            if (authContext.Database.Connection.State != ConnectionState.Open)
        //                authContext.Database.Connection.Open();
        //            authContext.Database.CommandTimeout = 600;
        //            var brandIdDt = new DataTable();
        //            brandIdDt.Columns.Add("IntValue");
        //            foreach (var item in sellerDashboardRequest.BrandIds)
        //            {
        //                var dr = brandIdDt.NewRow();
        //                dr["IntValue"] = item;
        //                brandIdDt.Rows.Add(dr);
        //            }

        //            var param = new SqlParameter("brandIds", brandIdDt);
        //            param.SqlDbType = SqlDbType.Structured;
        //            param.TypeName = "dbo.IntValues";
        //            var cmd = authContext.Database.Connection.CreateCommand();
        //            cmd.CommandText = "[Buyer].[GetCompanyCurrentStock]";
        //            cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //            cmd.Parameters.Add(new SqlParameter("@CompanyId", sellerDashboardRequest.CompanyId));
        //            cmd.Parameters.Add(param);
        //            cmd.Parameters.Add(new SqlParameter("@cityId", sellerDashboardRequest.CityId));


        //            // Run the sproc
        //            var reader = cmd.ExecuteReader();

        //            CompanyInventory = ((IObjectContextAdapter)authContext)
        //            .ObjectContext
        //            .Translate<CompanyInventory>(reader).FirstOrDefault();
        //        }

        //    });
        //    taskList.Add(task3);

        //    Task.WaitAll(taskList.ToArray());

        //    sellerDashboardData.CurrentCompanySale = CurrentCompanySale;
        //    sellerDashboardData.PreviouseCompanySale = PreviouseCompanySale;
        //    sellerDashboardData.CompanyInventory = CompanyInventory;
        //    if (sellerDashboardData.CurrentCompanySale != null && sellerDashboardData.PreviouseCompanySale != null)
        //    {
        //        sellerDashboardData.CurrentCompanySale.TotalSalePercant = sellerDashboardData.PreviouseCompanySale.TotalSale > 0 ?
        //             ((sellerDashboardData.CurrentCompanySale.TotalSale - sellerDashboardData.PreviouseCompanySale.TotalSale)
        //            / (double)sellerDashboardData.PreviouseCompanySale.TotalSale) * 100 : (double?)null;

        //        sellerDashboardData.CurrentCompanySale.TotalDispatchAmtPercant = sellerDashboardData.PreviouseCompanySale.TotalDispatchAmt > 0 ?
        //            ((sellerDashboardData.CurrentCompanySale.TotalDispatchAmt - sellerDashboardData.PreviouseCompanySale.TotalDispatchAmt)
        //            / (double)sellerDashboardData.PreviouseCompanySale.TotalDispatchAmt) * 100 : (double?)null;

        //        sellerDashboardData.CurrentCompanySale.TotalPOCAmtPercant = sellerDashboardData.PreviouseCompanySale.TotalPOCAmt > 0 ?
        //           ((sellerDashboardData.CurrentCompanySale.TotalPOCAmt - sellerDashboardData.PreviouseCompanySale.TotalPOCAmt)
        //           / (double)sellerDashboardData.PreviouseCompanySale.TotalPOCAmt) * 100 : (double?)null;

        //        sellerDashboardData.CurrentCompanySale.SkuSoldPercant = sellerDashboardData.PreviouseCompanySale.SkuSold > 0 ?
        //          ((sellerDashboardData.CurrentCompanySale.SkuSold - sellerDashboardData.PreviouseCompanySale.SkuSold)
        //          / (double)sellerDashboardData.PreviouseCompanySale.SkuSold) * 100 : (double?)null;

        //        sellerDashboardData.CurrentCompanySale.LiveCFRSKUPercant = sellerDashboardData.PreviouseCompanySale.LiveCFRSKU > 0 ?
        //          ((sellerDashboardData.CurrentCompanySale.LiveCFRSKU - sellerDashboardData.PreviouseCompanySale.LiveCFRSKU)
        //          / (double)sellerDashboardData.PreviouseCompanySale.LiveCFRSKU) * 100 : (double?)null;

        //        sellerDashboardData.CurrentCompanySale.AverageOrderValuePercant = sellerDashboardData.PreviouseCompanySale.AverageOrderValue > 0 ?
        //          ((sellerDashboardData.CurrentCompanySale.AverageOrderValue - sellerDashboardData.PreviouseCompanySale.AverageOrderValue)
        //          / (double)sellerDashboardData.PreviouseCompanySale.AverageOrderValue) * 100 : (double?)null;

        //        sellerDashboardData.CurrentCompanySale.AvgLineItemPercant = sellerDashboardData.PreviouseCompanySale.AvgLineItem > 0 ?
        //          ((sellerDashboardData.CurrentCompanySale.AvgLineItem - sellerDashboardData.PreviouseCompanySale.AvgLineItem)
        //          / (double)sellerDashboardData.PreviouseCompanySale.AvgLineItem) * 100 : (double?)null;

        //        sellerDashboardData.CurrentCompanySale.BilledCustomerPercant = sellerDashboardData.PreviouseCompanySale.BilledCustomer > 0 ?
        //          ((sellerDashboardData.CurrentCompanySale.BilledCustomer - sellerDashboardData.PreviouseCompanySale.BilledCustomer)
        //          / (double)sellerDashboardData.PreviouseCompanySale.BilledCustomer) * 100 : (double?)null;

        //        sellerDashboardData.CurrentCompanySale.CustomerReachPercant = sellerDashboardData.PreviouseCompanySale.CustomerReach > 0 ?
        //         ((sellerDashboardData.CurrentCompanySale.CustomerReach - sellerDashboardData.PreviouseCompanySale.CustomerReach)
        //         / (double)sellerDashboardData.PreviouseCompanySale.CustomerReach) * 100 : (double?)null;

        //    }


        //    return sellerDashboardData;

        //}


        [Route("GetCompanyDashboard")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<SellerDashboardData> GetCompanyDashboard(SellerDashboardRequest sellerDashboardRequest)
        {
            SellerDashboardData sellerDashboardData = new SellerDashboardData { };
            CompanySale CurrentCompanySale = new CompanySale();
            CompanySale PreviouseCompanySale = new CompanySale();
            CompanyInventory CompanyInventory = new CompanyInventory();

            var taskList = new List<Task>();


            var task1 = Task.Factory.StartNew(() =>
            {
                CurrentCompanySale = AsyncContext.Run(async () => await GetCompanySaleAsync(sellerDashboardRequest.StartDate, sellerDashboardRequest.EndDate, sellerDashboardRequest.CompanyId, sellerDashboardRequest.CityId, sellerDashboardRequest.BrandIds));
            });
            taskList.Add(task1);


            var task2 = Task.Factory.StartNew(() =>
            {
                int diff = sellerDashboardRequest.EndDate.Subtract(sellerDashboardRequest.StartDate).Days;
                DateTime pre_startDate = new DateTime();
                DateTime pre_endDate = sellerDashboardRequest.StartDate.AddSeconds(-1);
                if (sellerDashboardRequest.DateRangeType == "T")
                    pre_startDate = sellerDashboardRequest.StartDate.AddDays(-1);
                else if (sellerDashboardRequest.DateRangeType == "W")
                    pre_startDate = sellerDashboardRequest.StartDate.AddDays(-6);
                else if (sellerDashboardRequest.DateRangeType == "M")
                    pre_startDate = sellerDashboardRequest.StartDate.AddMonths(-1);
                else if (sellerDashboardRequest.DateRangeType == "C")
                    pre_startDate = sellerDashboardRequest.StartDate.AddDays(-1 * diff);

                PreviouseCompanySale = AsyncContext.Run(async () => await GetCompanySaleAsync(pre_startDate, pre_endDate, sellerDashboardRequest.CompanyId, sellerDashboardRequest.CityId, sellerDashboardRequest.BrandIds));

            });

            taskList.Add(task2);

            //var task3 = Task.Factory.StartNew(() =>
            //{
            //    using (var authContext = new AuthContext())
            //    {
            //        if (authContext.Database.Connection.State != ConnectionState.Open)
            //            authContext.Database.Connection.Open();
            //        authContext.Database.CommandTimeout = 600;
            //        var brandIdDt = new DataTable();
            //        brandIdDt.Columns.Add("IntValue");
            //        foreach (var item in sellerDashboardRequest.BrandIds)
            //        {
            //            var dr = brandIdDt.NewRow();
            //            dr["IntValue"] = item;
            //            brandIdDt.Rows.Add(dr);
            //        }

            //        var param = new SqlParameter("brandIds", brandIdDt);
            //        param.SqlDbType = SqlDbType.Structured;
            //        param.TypeName = "dbo.IntValues";
            //        var cmd = authContext.Database.Connection.CreateCommand();
            //        cmd.CommandText = "[Buyer].[GetCompanyCurrentStock]";
            //        cmd.CommandType = CommandType.StoredProcedure;
            //        cmd.Parameters.Add(new SqlParameter("@CompanyId", sellerDashboardRequest.CompanyId));
            //        cmd.Parameters.Add(param);
            //        cmd.Parameters.Add(new SqlParameter("@cityId", sellerDashboardRequest.CityId));


            //        // Run the sproc
            //        var reader = cmd.ExecuteReader();

            //        CompanyInventory = ((IObjectContextAdapter)authContext)
            //        .ObjectContext
            //        .Translate<CompanyInventory>(reader).FirstOrDefault();
            //    }

            //});
            //taskList.Add(task3);

            Task.WaitAll(taskList.ToArray());

            sellerDashboardData.CurrentCompanySale = CurrentCompanySale;
            sellerDashboardData.PreviouseCompanySale = PreviouseCompanySale;
            sellerDashboardData.CompanyInventory = CompanyInventory;
            if (sellerDashboardData.CurrentCompanySale != null && sellerDashboardData.PreviouseCompanySale != null)
            {
                sellerDashboardData.CurrentCompanySale.TotalSalePercant = sellerDashboardData.PreviouseCompanySale.TotalSale > 0 ?
                     ((sellerDashboardData.CurrentCompanySale.TotalSale - sellerDashboardData.PreviouseCompanySale.TotalSale)
                    / (double)sellerDashboardData.PreviouseCompanySale.TotalSale) * 100 : (double?)null;

                sellerDashboardData.CurrentCompanySale.TotalDispatchAmtPercant = sellerDashboardData.PreviouseCompanySale.TotalDispatchAmt > 0 ?
                    ((sellerDashboardData.CurrentCompanySale.TotalDispatchAmt - sellerDashboardData.PreviouseCompanySale.TotalDispatchAmt)
                    / (double)sellerDashboardData.PreviouseCompanySale.TotalDispatchAmt) * 100 : (double?)null;

                sellerDashboardData.CurrentCompanySale.TotalPOCAmtPercant = sellerDashboardData.PreviouseCompanySale.TotalPOCAmt > 0 ?
                   ((sellerDashboardData.CurrentCompanySale.TotalPOCAmt - sellerDashboardData.PreviouseCompanySale.TotalPOCAmt)
                   / (double)sellerDashboardData.PreviouseCompanySale.TotalPOCAmt) * 100 : (double?)null;

                sellerDashboardData.CurrentCompanySale.SkuSoldPercant = sellerDashboardData.PreviouseCompanySale.SkuSold > 0 ?
                  ((sellerDashboardData.CurrentCompanySale.SkuSold - sellerDashboardData.PreviouseCompanySale.SkuSold)
                  / (double)sellerDashboardData.PreviouseCompanySale.SkuSold) * 100 : (double?)null;

                sellerDashboardData.CurrentCompanySale.LiveCFRSKUPercant = sellerDashboardData.PreviouseCompanySale.LiveCFRSKU > 0 ?
                  ((sellerDashboardData.CurrentCompanySale.LiveCFRSKU - sellerDashboardData.PreviouseCompanySale.LiveCFRSKU)
                  / (double)sellerDashboardData.PreviouseCompanySale.LiveCFRSKU) * 100 : (double?)null;

                sellerDashboardData.CurrentCompanySale.AverageOrderValuePercant = sellerDashboardData.PreviouseCompanySale.AverageOrderValue > 0 ?
                  ((sellerDashboardData.CurrentCompanySale.AverageOrderValue - sellerDashboardData.PreviouseCompanySale.AverageOrderValue)
                  / (double)sellerDashboardData.PreviouseCompanySale.AverageOrderValue) * 100 : (double?)null;

                sellerDashboardData.CurrentCompanySale.AvgLineItemPercant = sellerDashboardData.PreviouseCompanySale.AvgLineItem > 0 ?
                  ((sellerDashboardData.CurrentCompanySale.AvgLineItem - sellerDashboardData.PreviouseCompanySale.AvgLineItem)
                  / (double)sellerDashboardData.PreviouseCompanySale.AvgLineItem) * 100 : (double?)null;

                sellerDashboardData.CurrentCompanySale.BilledCustomerPercant = sellerDashboardData.PreviouseCompanySale.BilledCustomer > 0 ?
                  ((sellerDashboardData.CurrentCompanySale.BilledCustomer - sellerDashboardData.PreviouseCompanySale.BilledCustomer)
                  / (double)sellerDashboardData.PreviouseCompanySale.BilledCustomer) * 100 : (double?)null;

                sellerDashboardData.CurrentCompanySale.CustomerReachPercant = sellerDashboardData.PreviouseCompanySale.CustomerReach > 0 ?
                 ((sellerDashboardData.CurrentCompanySale.CustomerReach - sellerDashboardData.PreviouseCompanySale.CustomerReach)
                 / (double)sellerDashboardData.PreviouseCompanySale.CustomerReach) * 100 : (double?)null;

            }


            return sellerDashboardData;

        }


        [Route("AllCompanyDashboard")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<List<AllCompanySale>> AllCompanyDashboard(AllCompanyPayload sellerDashboardRequest)
        {

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<AllCompanySale> AllCompanySales = new List<AllCompanySale>();
            List<int> CompanyIds = new List<int>();

            using (var db = new AuthContext())
            {
                if (sellerDashboardRequest != null && userid > 0)
                {
                    CompanyIds = db.PeopleSubcatMappings.Where(x => x.PeopleId == userid && x.IsActive == true && x.IsDeleted == false).Select(y => y.SubCatId).Distinct().ToList();

                    if(CompanyIds != null && CompanyIds.Count() > 0)
                    {
                        AllCompanySales = AsyncContext.Run(async () => await AllCompanySaleAsync(db, sellerDashboardRequest.StartDate, sellerDashboardRequest.EndDate, CompanyIds, sellerDashboardRequest.CityId));

                        if(AllCompanySales != null && AllCompanySales.Count()> 0)
                        {
                            return AllCompanySales;

                        }
                    }

                }
            }
            return AllCompanySales;
        }


        [Route("GetCompanyInventoryForDashboard")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<CompanyInventory> GetCompanyInventoryForDashboard(SellerDashboardRequest sellerDashboardRequest)
        {
            CompanyInventory CompanyInventory = new CompanyInventory();
            using (var authContext = new AuthContext())
            {
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();
                authContext.Database.CommandTimeout = 600;
                var brandIdDt = new DataTable();
                brandIdDt.Columns.Add("IntValue");
                foreach (var item in sellerDashboardRequest.BrandIds)
                {
                    var dr = brandIdDt.NewRow();
                    dr["IntValue"] = item;
                    brandIdDt.Rows.Add(dr);
                }

                var param = new SqlParameter("brandIds", brandIdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                var cmd = authContext.Database.Connection.CreateCommand();
                cmd.CommandText = "[Buyer].[GetCompanyCurrentStock]";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@CompanyId", sellerDashboardRequest.CompanyId));
                cmd.Parameters.Add(param);
                cmd.Parameters.Add(new SqlParameter("@cityId", sellerDashboardRequest.CityId));

                // Run the sproc
                var reader = cmd.ExecuteReader();

                CompanyInventory = ((IObjectContextAdapter)authContext)
                .ObjectContext
                .Translate<CompanyInventory>(reader).FirstOrDefault();
            }

            return CompanyInventory;
        }

        private async Task<CompanySale> GetCompanySaleAsync(DateTime startDate, DateTime endDate, int companyid, int? cityId = 0, List<int> brandIds = null)
        {
            CompanySale CurrentCompanySale = new CompanySale();
            StoreViewCfr storeViewCfr = new StoreViewCfr();

            var taskList = new List<Task>();
            var ordCountAndAmt = new ordcountamount();
            var dispatch = new doubleVal();
            var poc = new doubleVal();

            string sDate = startDate.Date.ToString("yyyy-MM-dd");
            string eDate = endDate.Date.AddDays(1).ToString("yyyy-MM-dd");

            StringBuilder whereCond = new StringBuilder($" and compid={companyid}");

            if (cityId.HasValue && cityId.Value > 0)
                whereCond.Append($" and cityid={cityId.Value}");

            if (brandIds != null && brandIds.Any())
                whereCond.Append($" and brandid in ({string.Join(",", brandIds.Select(s => s))})");


            var task1 = Task.Factory.StartNew(async () =>
            {
                ElasticSqlHelper<doubleVal> elasticSqlHelper = new ElasticSqlHelper<doubleVal>();

                dispatch = (await elasticSqlHelper.GetListAsync($"select sum(dispatchqty * price) as val from {platformIdxName} where dispatchdate>='{sDate}' and dispatchdate<'{eDate}' {whereCond.ToString()}")).FirstOrDefault();

            });
            taskList.Add(task1);

            var task2 = Task.Factory.StartNew(() =>
            {
                ElasticSqlHelper<doubleVal> elasticSqlHelper = new ElasticSqlHelper<doubleVal>();

                poc = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync($"select sum(dispatchqty * price) as val from {platformIdxName} where status='Post Order Canceled' and updateddate>='{sDate}' and updateddate<'{eDate}' {whereCond.ToString()}")).FirstOrDefault());

            });
            taskList.Add(task2);


            var task3 = Task.Factory.StartNew(() =>
            {
                ElasticSqlHelper<ordcountamount> elasticSqlHelper = new ElasticSqlHelper<ordcountamount>();

                ordCountAndAmt = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync($"select sum(ordqty * price) as ordamount, count(distinct orderid) ordcount, count(orderdetailid) linecount, count(distinct custid) billedcust, count(distinct itemnumber) skusold from {platformIdxName} where  createddate>='{sDate}' and createddate<'{eDate}' {whereCond.ToString()}")).FirstOrDefault());

            });
            taskList.Add(task3);

            var task4 = Task.Factory.StartNew(() =>
            {
                using (var authContext = new AuthContext())
                {
                    if (authContext.Database.Connection.State != ConnectionState.Open)
                        authContext.Database.Connection.Open();
                    authContext.Database.CommandTimeout = 850;

                    var brandIdDt = new DataTable();
                    brandIdDt.Columns.Add("IntValue");
                    foreach (var item in brandIds)
                    {
                        var dr = brandIdDt.NewRow();
                        dr["IntValue"] = item;
                        brandIdDt.Rows.Add(dr);
                    }

                    var param = new SqlParameter("brandIds", brandIdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    var cmd = authContext.Database.Connection.CreateCommand();
                    cmd.CommandText = "Buyer.GetStoreViewCfrCount";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@startDate", startDate));
                    cmd.Parameters.Add(new SqlParameter("@endDate", endDate));
                    cmd.Parameters.Add(new SqlParameter("@cityId", cityId));
                    cmd.Parameters.Add(new SqlParameter("@companyId", companyid));
                    cmd.Parameters.Add(param);

                    // Run the sproc
                    var reader = cmd.ExecuteReader();

                    storeViewCfr = ((IObjectContextAdapter)authContext)
                    .ObjectContext
                    .Translate<StoreViewCfr>(reader).FirstOrDefault();

                }

            });
            taskList.Add(task4);


            Task.WaitAll(taskList.ToArray());

            CurrentCompanySale.TotalDispatchAmt = (int)Math.Round(dispatch.val, 0);
            CurrentCompanySale.TotalPOCAmt = (int)Math.Round(poc.val, 0);

            CurrentCompanySale.TotalSale = (int)Math.Round(ordCountAndAmt.ordamount, 0);
            CurrentCompanySale.AverageOrderValue = ordCountAndAmt.ordcount > 0 ? CurrentCompanySale.TotalSale / ordCountAndAmt.ordcount : 0;
            CurrentCompanySale.AvgLineItem = ordCountAndAmt.ordcount > 0 ? ordCountAndAmt.linecount / (double)ordCountAndAmt.ordcount : 0;
            CurrentCompanySale.SkuSold = ordCountAndAmt.skusold;
            CurrentCompanySale.BilledCustomer = ordCountAndAmt.billedcust;
            CurrentCompanySale.CustomerReach = storeViewCfr.StoreView;
            CurrentCompanySale.LiveCFRSKU = storeViewCfr.LiveCfrSKU;


            return CurrentCompanySale;
        }

        private async Task<List<AllCompanySale>> AllCompanySaleAsync(AuthContext db, DateTime startDate, DateTime endDate, List<int> companyids, int? cityId = 0)
        {
            List<AllCompanySale> CurrentCompanySales = new List<AllCompanySale>();
            var taskList = new List<Task>();

            string sDate = startDate.Date.ToString("yyyy-MM-dd");
            string eDate = endDate.Date.AddDays(1).ToString("yyyy-MM-dd");

            StringBuilder whereCond = new StringBuilder();//($" and compid={companyid}");

            if (cityId.HasValue && cityId.Value > 0)
                whereCond.Append($" and cityid={cityId.Value}");

            if (companyids != null && companyids.Count() > 0)
            {
                var CompanyNames = db.ItemMasterCentralDB.Where(x => companyids.Contains(x.SubCategoryId)).Select(x=>new {x.SubCategoryId, x.SubcategoryName}).Distinct().ToList();

                ElasticSqlHelper<AllCompanyValue> elasticDispatchSqlHelper = new ElasticSqlHelper<AllCompanyValue>();
                ElasticSqlHelper<AllCompanyordcountamounts> elasticOrdCountAndAmtSqlHelper = new ElasticSqlHelper<AllCompanyordcountamounts>();

                whereCond.Append($" and compid in ({string.Join(",", companyids.Select(s => s))})");
                var dispatch = (await elasticDispatchSqlHelper.GetListAsync($"select sum(dispatchqty * price) as val , compid from {platformIdxName} where dispatchdate>='{sDate}' and dispatchdate<'{eDate}' {whereCond.ToString()} group by compid")).ToList();
                var ordCountAndAmt = AsyncContext.Run(async () => (await elasticOrdCountAndAmtSqlHelper.GetListAsync($"select sum(ordqty * price) as ordamount, count(distinct orderid) ordcount, count(distinct custid) billedcust ,compid from {platformIdxName} where  createddate>='{sDate}' and createddate<'{eDate}' {whereCond.ToString()} group by compid")).ToList());

                foreach (var companyid in companyids)
                {
                    
                    if (dispatch != null && ordCountAndAmt != null)
                    {
                        AllCompanySale CurrentCompanySale = new AllCompanySale();
                        CurrentCompanySale.CompanyId = companyid;
                        CurrentCompanySale.CompanyName = CompanyNames!= null ? CompanyNames.FirstOrDefault(x => x.SubCategoryId == companyid).SubcategoryName : "";
                        CurrentCompanySale.OrderCount = ordCountAndAmt.FirstOrDefault(x => x.compid == companyid) != null ? ordCountAndAmt.FirstOrDefault(x => x.compid == companyid).ordcount : 0;
                        CurrentCompanySale.TotalDispatchAmt = dispatch.FirstOrDefault(x => x.compid == companyid) != null ? (int)Math.Round(dispatch.FirstOrDefault(x => x.compid == companyid).val,0) : 0;// (int)Math.Round(dispatch.val, 0);
                        CurrentCompanySale.TotalSale = ordCountAndAmt.FirstOrDefault(x => x.compid == companyid) != null ? (int)Math.Round(ordCountAndAmt.FirstOrDefault(x => x.compid == companyid).ordamount, 0) : 0;
                        CurrentCompanySale.AverageOrderValue = ordCountAndAmt.FirstOrDefault(x => x.compid == companyid) != null ? CurrentCompanySale.TotalSale / ordCountAndAmt.FirstOrDefault(x => x.compid == companyid).ordcount : 0;
                        CurrentCompanySale.BilledCustomer = ordCountAndAmt.FirstOrDefault(x => x.compid == companyid) != null ? ordCountAndAmt.FirstOrDefault(x => x.compid == companyid).billedcust : 0;

                        CurrentCompanySales.Add(CurrentCompanySale);
                    }
                }
            }
            return CurrentCompanySales;


        }



        [Route("GetCompanyDashboardGraphData")]
        [HttpPost]
        public async Task<List<CompanyGraphData>> GetCompanyDashboardGraphData(SellerDashboardGraphRequest sellerDashboardRequest)
        {
            List<CompanyGraphData> CompanyGraphDatas = new List<CompanyGraphData>();

            if (sellerDashboardRequest.Type.ToLower() == "customerreach" || sellerDashboardRequest.Type.ToLower() == "skuactive")
            {

                using (var authContext = new AuthContext())
                {
                    authContext.Database.CommandTimeout = 600;
                    if (authContext.Database.Connection.State != ConnectionState.Open)
                        authContext.Database.Connection.Open();

                    var brandIdDt = new DataTable();
                    brandIdDt.Columns.Add("IntValue");
                    foreach (var item in sellerDashboardRequest.BrandIds)
                    {
                        var dr = brandIdDt.NewRow();
                        dr["IntValue"] = item;
                        brandIdDt.Rows.Add(dr);
                    }

                    var param = new SqlParameter("brandIds", brandIdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    var cmd = authContext.Database.Connection.CreateCommand();
                    cmd.CommandText = "[Buyer].[CompanyDashboardReachActiveSKU]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@companyId", sellerDashboardRequest.CompanyId));
                    cmd.Parameters.Add(param);
                    cmd.Parameters.Add(new SqlParameter("@cityId", sellerDashboardRequest.CityId));
                    cmd.Parameters.Add(new SqlParameter("@startDate", sellerDashboardRequest.StartDate));
                    cmd.Parameters.Add(new SqlParameter("@endDate", sellerDashboardRequest.EndDate));
                    cmd.Parameters.Add(new SqlParameter("@type", sellerDashboardRequest.Type));
                    // Run the sproc
                    var reader = cmd.ExecuteReader();

                    CompanyGraphDatas = ((IObjectContextAdapter)authContext)
                    .ObjectContext
                    .Translate<CompanyGraphData>(reader).ToList();
                }
            }
            else
            {

                string sDate = sellerDashboardRequest.StartDate.Date.ToString("yyyy-MM-dd");
                string eDate = sellerDashboardRequest.EndDate.Date.AddDays(1).ToString("yyyy-MM-dd");

                StringBuilder whereCond = new StringBuilder($" and compid={sellerDashboardRequest.CompanyId}");

                if (sellerDashboardRequest.CityId > 0)
                    whereCond.Append($" and cityid={sellerDashboardRequest.CityId}");

                if (sellerDashboardRequest.BrandIds != null && sellerDashboardRequest.BrandIds.Any())
                    whereCond.Append($" and brandid in ({string.Join(",", sellerDashboardRequest.BrandIds.Select(s => s))})");

                string sqlQuery = $"select histogram(createddate,INTERVAL [[interval]] ) Xaxis, round(sum(ordqty * price),2) Yaxis from {platformIdxName} where createddate>='{sDate}' and createddate < '{eDate}' {whereCond.ToString()}  group by Xaxis";

                if (sellerDashboardRequest.Type.ToLower() == "dispatch")
                    sqlQuery = $"select histogram(dispatchdate,INTERVAL [[interval]] ) Xaxis, round(sum(dispatchqty * price),2) Yaxis from {platformIdxName} where dispatchdate>='{sDate}' and dispatchdate <'{eDate}' {whereCond.ToString()}  group by Xaxis";

                if (sellerDashboardRequest.Type.ToLower() == "aov")
                    sqlQuery = $"select histogram(createddate,INTERVAL [[interval]] ) Xaxis, round(sum(ordqty * price) / count(distinct orderid),2) Yaxis from {platformIdxName} where createddate>='{sDate}' and createddate <'{eDate}' {whereCond.ToString()}  group by Xaxis";

                if (sellerDashboardRequest.Type.ToLower() == "billcustomer")
                    sqlQuery = $"select histogram(createddate,INTERVAL [[interval]] ) Xaxis, count(distinct custid) Yaxis from {platformIdxName} where createddate>='{sDate}' and createddate <'{eDate}' {whereCond.ToString()}  group by Xaxis";

                if (sellerDashboardRequest.Type.ToLower() == "poc")
                    sqlQuery = $"select histogram(updateddate,INTERVAL [[interval]] ) Xaxis, round(sum(dispatchqty * price),2) Yaxis from {platformIdxName} where updateddate>='{sDate}' and status='Post Order Canceled' and updateddate <'{eDate}' {whereCond.ToString()}  group by Xaxis";

                if (sellerDashboardRequest.Type.ToLower() == "ali")
                    sqlQuery = $"select histogram(createddate,INTERVAL [[interval]] ) Xaxis, round(count(orderdetailid) / count(distinct orderid),2) Yaxis from {platformIdxName} where createddate>='{sDate}' and createddate <'{eDate}' {whereCond.ToString()}  group by Xaxis";

                if (sellerDashboardRequest.Type.ToLower() == "skusold")
                    sqlQuery = $"select histogram(createddate,INTERVAL [[interval]] ) Xaxis, count(distinct itemnumber) Yaxis from {platformIdxName} where createddate>='{sDate}' and createddate <'{eDate}' {whereCond.ToString()}  group by Xaxis";



                int dateDiffInDays = sellerDashboardRequest.EndDate.Date.Subtract(sellerDashboardRequest.StartDate.Date).Days;

                if (dateDiffInDays == 0)
                    sqlQuery = sqlQuery.Replace("[[interval]]", "1 HOUR");
                else
                {
                    int daysInMonth = DateTime.DaysInMonth(sellerDashboardRequest.StartDate.Year, sellerDashboardRequest.StartDate.Month);
                    if (dateDiffInDays < daysInMonth)
                        sqlQuery = sqlQuery.Replace("[[interval]]", "1 DAY");
                    else
                        sqlQuery = sqlQuery.Replace("[[interval]]", "1 MONTH");
                }

                ElasticSqlHelper<CompanyGraphData> elasticSqlHelper = new ElasticSqlHelper<CompanyGraphData>();

                CompanyGraphDatas = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync(sqlQuery)).ToList());


            }


            return CompanyGraphDatas;
        }

        [Route("GetCompanyCatelog")]
        [HttpPost]
        public async Task<CompanyCatalog> GetCompanyCatelog(CompanyCatalogRequest companyCatalogRequest)
        {
            CompanyCatalog companyCatalog = new CompanyCatalog();
            using (var authContext = new AuthContext())
            {
                authContext.Database.CommandTimeout = 600;
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();

                var cmd = authContext.Database.Connection.CreateCommand();
                cmd.CommandText = "[Buyer].[GetCompanyCatalog]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@companyId", companyCatalogRequest.CompanyId));
                cmd.Parameters.Add(new SqlParameter("@cityId", companyCatalogRequest.CityId));

                // Run the sproc
                var reader = cmd.ExecuteReader();

                companyCatalog = ((IObjectContextAdapter)authContext)
                                .ObjectContext
                                .Translate<CompanyCatalog>(reader).FirstOrDefault();
            }

            return companyCatalog;
        }

        [Route("GetCompanyCatelogBrand")]
        [HttpPost]
        public async Task<List<CompanyCatalogBrand>> GetCompanyCatelogBrand(CompanyCatalogBrandRequest companyCatalogBrenaRequest)
        {
            List<CompanyCatalogBrand> companyCatalogBrands = new List<CompanyCatalogBrand>();
            using (var authContext = new AuthContext())
            {
                authContext.Database.CommandTimeout = 600;
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();

                var cmd = authContext.Database.Connection.CreateCommand();
                cmd.CommandText = "[Buyer].[GetCompanyCatalogBrand]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@companyId", companyCatalogBrenaRequest.CompanyId));
                cmd.Parameters.Add(new SqlParameter("@cityId", companyCatalogBrenaRequest.CityId));
                cmd.Parameters.Add(new SqlParameter("@type", companyCatalogBrenaRequest.Type));
                // Run the sproc
                var reader = cmd.ExecuteReader();

                companyCatalogBrands = ((IObjectContextAdapter)authContext)
                                        .ObjectContext
                                        .Translate<CompanyCatalogBrand>(reader).ToList();
            }

            return companyCatalogBrands;
        }


        [Route("GetCompanyCatelogBrandItem")]
        [HttpPost]
        public async Task<List<CompanyCatalogItem>> GetCompanyCatelogBrandItem(CompanyCatalogItemRequest companyCatalogItemRequest)
        {
            List<CompanyCatalogItem> companyCatalogItems = new List<CompanyCatalogItem>();
            using (var authContext = new AuthContext())
            {
                authContext.Database.CommandTimeout = 600;
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();

                var cmd = authContext.Database.Connection.CreateCommand();
                cmd.CommandText = "[Buyer].[GetCompanyCatalogBrandItem]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@companyId", companyCatalogItemRequest.CompanyId));
                cmd.Parameters.Add(new SqlParameter("@cityId", companyCatalogItemRequest.CityId));
                cmd.Parameters.Add(new SqlParameter("@brandId", companyCatalogItemRequest.BrandId));
                cmd.Parameters.Add(new SqlParameter("@type", companyCatalogItemRequest.Type));
                cmd.Parameters.Add(new SqlParameter("@CategoryName", companyCatalogItemRequest.CategoryName));

                // Run the sproc
                var reader = cmd.ExecuteReader();

                companyCatalogItems = ((IObjectContextAdapter)authContext)
                                        .ObjectContext
                                        .Translate<CompanyCatalogItem>(reader).ToList();
            }

            return companyCatalogItems;
        }


        [Route("GetCompanySale")]
        [HttpPost]
        public async Task<CompanyPageSaleData> GetCompanySale(CompanySaleRequest companySaleRequest)
        {
            CompanyPageSaleData CurrentCompanySale = new CompanyPageSaleData();
            CompanyPageSaleData PreviouseCompanySale = new CompanyPageSaleData();

            var taskList = new List<Task>();
            var task1 = Task.Factory.StartNew(() =>
            {
                CurrentCompanySale = AsyncContext.Run(() => GetCompanySaleFromElasticAsync(companySaleRequest.StartDate, companySaleRequest.EndDate, companySaleRequest.CompanyId, companySaleRequest.CityId));

            });
            taskList.Add(task1);
            var task2 = Task.Factory.StartNew(() =>
            {
                int diff = companySaleRequest.EndDate.Subtract(companySaleRequest.StartDate).Days;
                DateTime pre_startDate = new DateTime();
                DateTime pre_endDate = companySaleRequest.StartDate.AddSeconds(-1);
                if (companySaleRequest.DateRangeType == "T")
                    pre_startDate = companySaleRequest.StartDate.AddDays(-1);
                else if (companySaleRequest.DateRangeType == "W")
                    pre_startDate = companySaleRequest.StartDate.AddDays(-6);
                else if (companySaleRequest.DateRangeType == "M")
                    pre_startDate = companySaleRequest.StartDate.AddMonths(-1);
                else if (companySaleRequest.DateRangeType == "C")
                    pre_startDate = companySaleRequest.StartDate.AddDays(-1 * diff);

                PreviouseCompanySale = AsyncContext.Run(() => GetCompanySaleFromElasticAsync(pre_startDate, pre_endDate, companySaleRequest.CompanyId, companySaleRequest.CityId));


            });
            taskList.Add(task2);


            Task.WaitAll(taskList.ToArray());


            if (CurrentCompanySale != null && PreviouseCompanySale != null)
            {
                CurrentCompanySale.TotalSalePercant = PreviouseCompanySale.TotalSale > 0 ?
                     ((CurrentCompanySale.TotalSale - PreviouseCompanySale.TotalSale)
                    / (double)PreviouseCompanySale.TotalSale) * 100 : (double?)null;

                CurrentCompanySale.TotalDispatchAmtPercant = PreviouseCompanySale.TotalDispatchAmt > 0 ?
                    ((CurrentCompanySale.TotalDispatchAmt - PreviouseCompanySale.TotalDispatchAmt)
                    / (double)PreviouseCompanySale.TotalDispatchAmt) * 100 : (double?)null;

                CurrentCompanySale.TotalPOCAmtPercant = PreviouseCompanySale.TotalPOCAmt > 0 ?
                   ((CurrentCompanySale.TotalPOCAmt - PreviouseCompanySale.TotalPOCAmt)
                   / (double)PreviouseCompanySale.TotalPOCAmt) * 100 : (double?)null;

                CurrentCompanySale.AverageOrderValuePercant = PreviouseCompanySale.AverageOrderValue > 0 ?
                  ((CurrentCompanySale.AverageOrderValue - PreviouseCompanySale.AverageOrderValue)
                  / (double)PreviouseCompanySale.AverageOrderValue) * 100 : (double?)null;

                CurrentCompanySale.AvgLineItemPercant = PreviouseCompanySale.AvgLineItem > 0 ?
                  ((CurrentCompanySale.AvgLineItem - PreviouseCompanySale.AvgLineItem)
                  / (double)PreviouseCompanySale.AvgLineItem) * 100 : (double?)null;


            }
            return CurrentCompanySale;

        }


        private async Task<CompanyPageSaleData> GetCompanySaleFromElasticAsync(DateTime startDate, DateTime endDate, int companyid, int? cityId = 0)
        {
            CompanyPageSaleData CurrentCompanySale = new CompanyPageSaleData();

            var taskList = new List<Task>();
            var ordCountAndAmt = new ordcountamount();
            var dispatch = new doubleVal();
            var poc = new doubleVal();

            string sDate = startDate.Date.ToString("yyyy-MM-dd");
            string eDate = endDate.Date.AddDays(1).ToString("yyyy-MM-dd");

            StringBuilder whereCond = new StringBuilder($" and compid={companyid}");

            if (cityId.HasValue && cityId.Value > 0)
                whereCond.Append($" and cityid={cityId.Value}");


            var task1 = Task.Factory.StartNew(async () =>
            {
                ElasticSqlHelper<doubleVal> elasticSqlHelper = new ElasticSqlHelper<doubleVal>();

                dispatch = (await elasticSqlHelper.GetListAsync($"select sum(dispatchqty * price) as val from {platformIdxName} where dispatchdate>='{sDate}' and dispatchdate<'{eDate}' {whereCond.ToString()}")).FirstOrDefault();

            });
            taskList.Add(task1);

            var task2 = Task.Factory.StartNew(() =>
            {
                ElasticSqlHelper<doubleVal> elasticSqlHelper = new ElasticSqlHelper<doubleVal>();

                poc = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync($"select sum(dispatchqty * price) as val from {platformIdxName} where status='Post Order Canceled' and updateddate>='{sDate}' and updateddate<'{eDate}' {whereCond.ToString()}")).FirstOrDefault());

            });
            taskList.Add(task2);


            var task3 = Task.Factory.StartNew(() =>
            {
                ElasticSqlHelper<ordcountamount> elasticSqlHelper = new ElasticSqlHelper<ordcountamount>();

                ordCountAndAmt = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync($"select sum(ordqty * price) as ordamount, count(distinct orderid) ordcount, count(orderdetailid) linecount, count(distinct custid) billedcust, count(distinct itemnumber) skusold from {platformIdxName} where  createddate>='{sDate}' and createddate<'{eDate}' {whereCond.ToString()}")).FirstOrDefault());

            });
            taskList.Add(task3);


            Task.WaitAll(taskList.ToArray());

            CurrentCompanySale.TotalDispatchAmt = (int)Math.Round(dispatch.val, 0);
            CurrentCompanySale.TotalPOCAmt = (int)Math.Round(poc.val, 0);

            CurrentCompanySale.TotalSale = (int)Math.Round(ordCountAndAmt.ordamount, 0);
            CurrentCompanySale.AverageOrderValue = ordCountAndAmt.ordcount > 0 ? CurrentCompanySale.TotalSale / ordCountAndAmt.ordcount : 0;
            CurrentCompanySale.AvgLineItem = ordCountAndAmt.ordcount > 0 ? ordCountAndAmt.linecount / (double)ordCountAndAmt.ordcount : 0;


            return CurrentCompanySale;
        }

        [Route("GetCompanySaleGraph")]
        [HttpPost]
        public async Task<CompanySaleGraph> GetCompanySaleGraph(CompanySaleRequest companySaleRequest)
        {
            List<CompanySaleData> companySaleDatas = new List<CompanySaleData>();
            CompanySaleGraph companySaleGraph = new CompanySaleGraph();

            string sDate = companySaleRequest.StartDate.Date.ToString("yyyy-MM-dd");
            string eDate = companySaleRequest.EndDate.Date.AddDays(1).ToString("yyyy-MM-dd");

            StringBuilder whereCond = new StringBuilder($" and compid={companySaleRequest.CompanyId}");

            if (companySaleRequest.CityId > 0)
                whereCond.Append($" and cityid={companySaleRequest.CityId}");


            string sqlQuery = $"select itemname, brandid,brandname as BrandName, round(sum(ordqty*price),2) as Amount, sum(ordqty) Qty  from {platformIdxName} where createddate>='{sDate}' and createddate < '{eDate}' {whereCond.ToString()}  group by itemname,brandid,brandname";


            if (companySaleRequest.DateRangeType.ToLower() == "dispatch")
                sqlQuery = $"select itemname, brandid BrandId,brandname as BrandName, round(sum(dispatchqty*price),2) as Amount, sum(ordqty) Qty  from {platformIdxName} where dispatchdate>='{sDate}' and dispatchdate < '{eDate}' {whereCond.ToString()}  group by itemname,brandid,brandname";

            if (companySaleRequest.DateRangeType.ToLower() == "aov")
                sqlQuery = $"select itemname, brandid BrandId,brandname as BrandName, round(sum(ordqty*price) / count(distinct orderid),2) as Amount, round(sum(ordqty) / count(distinct orderid),2) Qty  from {platformIdxName} where createddate>='{sDate}' and createddate < '{eDate}' {whereCond.ToString()}  group by itemname,brandid,brandname";

            if (companySaleRequest.DateRangeType.ToLower() == "poc")
                sqlQuery = $"select itemname, brandid BrandId,brandname as BrandName, round(sum(dispatchqty*price),2) as Amount, sum(ordqty) Qty  from {platformIdxName} where status='Post Order Canceled' and updateddate>='{sDate}' and updateddate < '{eDate}' {whereCond.ToString()}  group by itemname,brandid,brandname";

            if (companySaleRequest.DateRangeType.ToLower() == "ali")
                sqlQuery = $"select itemname, brandid BrandId,brandname as BrandName, round(count(orderdetailid) / count(distinct orderid),2) as Qty, 0 as Amount  from {platformIdxName} where createddate>='{sDate}' and createddate < '{eDate}' {whereCond.ToString()}  group by itemname,brandid,brandname";


            ElasticSqlHelper<CompanySaleData> elasticSqlHelper = new ElasticSqlHelper<CompanySaleData>();

            companySaleDatas = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync(sqlQuery)).ToList());




            //using (var authContext = new AuthContext())
            //{
            //    authContext.Database.CommandTimeout = 600;
            //    if (authContext.Database.Connection.State != ConnectionState.Open)
            //        authContext.Database.Connection.Open();

            //    var cmd = authContext.Database.Connection.CreateCommand();
            //    cmd.CommandText = "[Buyer].[GetCompanySalesGraph]";
            //    cmd.CommandType = System.Data.CommandType.StoredProcedure;
            //    cmd.Parameters.Add(new SqlParameter("@companyId", companySaleRequest.CompanyId));
            //    cmd.Parameters.Add(new SqlParameter("@cityId", companySaleRequest.CityId));
            //    cmd.Parameters.Add(new SqlParameter("@startDate", companySaleRequest.StartDate));
            //    cmd.Parameters.Add(new SqlParameter("@endDate", companySaleRequest.EndDate));
            //    cmd.Parameters.Add(new SqlParameter("@type", companySaleRequest.DateRangeType));

            //    // Run the sproc
            //    var reader = cmd.ExecuteReader();

            //    companySaleDatas = ((IObjectContextAdapter)authContext)
            //                        .ObjectContext
            //                        .Translate<CompanySaleData>(reader).ToList();
            //}

            if (companySaleDatas != null)
            {
                companySaleGraph.BrandAmount = new List<GraphAmountData>();
                companySaleGraph.BrandQty = new List<GraphQtyData>();
                companySaleGraph.ItemAmount = new List<GraphAmountData>();
                companySaleGraph.ItemQty = new List<GraphQtyData>();
                companySaleGraph.BrandAmount = companySaleDatas.GroupBy(x => new { x.BrandName, x.BrandId }).Select(x => new GraphAmountData
                {
                    Id = x.Key.BrandId,
                    Name = x.Key.BrandName,
                    Amount = x.Sum(y => y.Amount)
                }).ToList();
                companySaleGraph.BrandQty = companySaleDatas.GroupBy(x => new { x.BrandName, x.BrandId }).Select(x => new GraphQtyData
                {
                    Id = x.Key.BrandId,
                    Name = x.Key.BrandName,
                    Qty = x.Sum(y => y.Qty)
                }).ToList();
            }
            return companySaleGraph;

        }


        [Route("GetCompanySalesItemGraph")]
        [HttpPost]
        public async Task<CompanySalesItemGraph> GetCompanySalesItemGraph(CompanySaleItemRequest companySaleRequest)
        {
            List<CompanySaleData> companySaleDatas = new List<CompanySaleData>();
            CompanySalesItemGraph companySaleGraph = new CompanySalesItemGraph();


            string sDate = companySaleRequest.StartDate.Date.ToString("yyyy-MM-dd");
            string eDate = companySaleRequest.EndDate.Date.AddDays(1).ToString("yyyy-MM-dd");

            StringBuilder whereCond = new StringBuilder($" and brandid={companySaleRequest.BrandId}");

            if (companySaleRequest.CityId > 0)
                whereCond.Append($" and cityid={companySaleRequest.CityId}");


            string sqlQuery = $"select itemname, brandid,brandname as BrandName, round(sum(ordqty*price),2) as Amount, sum(ordqty) Qty  from {platformIdxName} where createddate>='{sDate}' and createddate < '{eDate}' {whereCond.ToString()}  group by itemname,brandid,brandname";


            if (companySaleRequest.DateRangeType.ToLower() == "dispatch")
                sqlQuery = $"select itemname, brandid BrandId,brandname as BrandName, round(sum(dispatchqty*price),2) as Amount, sum(ordqty) Qty  from {platformIdxName} where dispatchdate>='{sDate}' and dispatchdate < '{eDate}' {whereCond.ToString()}  group by itemname,brandid,brandname";

            if (companySaleRequest.DateRangeType.ToLower() == "aov")
                sqlQuery = $"select itemname, brandid BrandId,brandname as BrandName, round(sum(ordqty*price) / count(distinct orderid),2) as Amount, round(sum(ordqty) / count(distinct orderid),2) Qty  from {platformIdxName} where createddate>='{sDate}' and createddate < '{eDate}' {whereCond.ToString()}  group by itemname,brandid,brandname";

            if (companySaleRequest.DateRangeType.ToLower() == "poc")
                sqlQuery = $"select itemname, brandid BrandId,brandname as BrandName, round(sum(dispatchqty*price),2) as Amount, sum(ordqty) Qty  from {platformIdxName} where status='Post Order Canceled' and updateddate>='{sDate}' and updateddate < '{eDate}' {whereCond.ToString()}  group by itemname,brandid,brandname";

            if (companySaleRequest.DateRangeType.ToLower() == "ali")
                sqlQuery = $"select itemname, brandid BrandId,brandname as BrandName, round(count(orderdetailid) / count(distinct orderid),2) as Qty, 0 as Amount  from {platformIdxName} where createddate>='{sDate}' and createddate < '{eDate}' {whereCond.ToString()}  group by itemname,brandid,brandname";


            ElasticSqlHelper<CompanySaleData> elasticSqlHelper = new ElasticSqlHelper<CompanySaleData>();

            companySaleDatas = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync(sqlQuery)).ToList());


            if (companySaleDatas != null)
            {
                companySaleGraph.ItemAmount = new List<GraphAmountData>();
                companySaleGraph.ItemQty = new List<GraphQtyData>();
                companySaleGraph.ItemAmount = companySaleDatas.GroupBy(x => x.itemname).Select(x => new GraphAmountData
                {
                    Id = 0,
                    Name = x.Key,
                    Amount = x.Sum(y => y.Amount)
                }).ToList();
                companySaleGraph.ItemQty = companySaleDatas.GroupBy(x => x.itemname).Select(x => new GraphQtyData
                {
                    Id = 0,
                    Name = x.Key,
                    Qty = x.Sum(y => y.Qty)
                }).ToList();
            }
            return companySaleGraph;

        }

        [Route("GetCompanyHeatMapData")]
        [HttpPost]
        public async Task<List<HeatMapData>> GetCompanyHeatMapData(HeatMapRequest heatMapRequest)
        {
            List<HeatMapData> heatMapDatas = new List<HeatMapData>();

            string sDate = heatMapRequest.startDate.Date.ToString("yyyy-MM-dd");
            string eDate = heatMapRequest.endDate.Date.AddDays(1).ToString("yyyy-MM-dd");

            StringBuilder whereCond = new StringBuilder($" and compid={heatMapRequest.Id}");

            if (heatMapRequest.CityId > 0)
                whereCond.Append($" and cityid={heatMapRequest.CityId}");


            string sqlQuery = $"select lat,lng as lg, sum(ordqty * price) as TotalSale , sum(ordqty) as TotalQty  from {platformIdxName} where lat!=0 and lng != 0 and createddate>='{sDate}' and createddate < '{eDate}' {whereCond.ToString()}  group by lat,lng";



            ElasticSqlHelper<HeatMapData> elasticSqlHelper = new ElasticSqlHelper<HeatMapData>();

            heatMapDatas = (await elasticSqlHelper.GetListAsync(sqlQuery)).ToList();



            return heatMapDatas;


        }

        [Route("GetBrandHeatMapData")]
        [HttpPost]
        public async Task<List<HeatMapData>> GetBrandHeatMapData(HeatMapRequest heatMapRequest)
        {
            List<HeatMapData> heatMapDatas = new List<HeatMapData>();

            string sDate = heatMapRequest.startDate.Date.ToString("yyyy-MM-dd");
            string eDate = heatMapRequest.endDate.Date.AddDays(1).ToString("yyyy-MM-dd");

            StringBuilder whereCond = new StringBuilder($" and brandid={heatMapRequest.Id}");

            if (heatMapRequest.CityId > 0)
                whereCond.Append($" and cityid={heatMapRequest.CityId}");


            string sqlQuery = $"select lat,lng as lg, sum(ordqty * price) as TotalSale , sum(ordqty) as TotalQty  from {platformIdxName} where lat!=0 and lng != 0 and createddate>='{sDate}' and createddate < '{eDate}' {whereCond.ToString()}  group by lat,lng";



            ElasticSqlHelper<HeatMapData> elasticSqlHelper = new ElasticSqlHelper<HeatMapData>();

            heatMapDatas = (await elasticSqlHelper.GetListAsync(sqlQuery)).ToList();


            //using (var authContext = new AuthContext())
            //{
            //    authContext.Database.CommandTimeout = 600;
            //    if (authContext.Database.Connection.State != ConnectionState.Open)
            //        authContext.Database.Connection.Open();

            //    var cmd = authContext.Database.Connection.CreateCommand();
            //    cmd.CommandText = "[Buyer].[GetBrandHeatMapData]";
            //    cmd.CommandType = System.Data.CommandType.StoredProcedure;
            //    cmd.Parameters.Add(new SqlParameter("@brandId", heatMapRequest.Id));
            //    cmd.Parameters.Add(new SqlParameter("@cityId", heatMapRequest.CityId));
            //    cmd.Parameters.Add(new SqlParameter("@startDate", heatMapRequest.startDate));
            //    cmd.Parameters.Add(new SqlParameter("@endDate", heatMapRequest.endDate));

            //    // Run the sproc
            //    var reader = cmd.ExecuteReader();

            //    heatMapDatas = ((IObjectContextAdapter)authContext)
            //                        .ObjectContext
            //                        .Translate<HeatMapData>(reader).ToList();
            //}


            return heatMapDatas;


        }

        [Route("GetCompanyInventory")]
        [HttpPost]
        public async Task<CompanyInventory> GetCompanyInventory(CompanyInventoryRequest companyInventoryRequest)
        {
            SellerDashboardData sellerDashboardData = new SellerDashboardData { };
            CompanySale CurrentCompanySale = new CompanySale();
            CompanySale PreviouseCompanySale = new CompanySale();
            CompanyInventory CompanyInventory = new CompanyInventory();


            using (var authContext = new AuthContext())
            {
                authContext.Database.CommandTimeout = 600;
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();

                var brandIdDt = new DataTable();
                brandIdDt.Columns.Add("IntValue");

                var param = new SqlParameter("brandIds", brandIdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                var cmd = authContext.Database.Connection.CreateCommand();
                cmd.CommandText = "[Buyer].[GetCompanyCurrentStock]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@CompanyId", companyInventoryRequest.CompanyId));
                cmd.Parameters.Add(param);
                cmd.Parameters.Add(new SqlParameter("@cityId", companyInventoryRequest.CityId));


                // Run the sproc
                var reader = cmd.ExecuteReader();

                CompanyInventory = ((IObjectContextAdapter)authContext)
                .ObjectContext
                .Translate<CompanyInventory>(reader).FirstOrDefault();
            }

            return CompanyInventory;

        }


        [Route("GetCompanyInventoryGraph")]
        [HttpPost]
        public async Task<CompanyInventoryGraph> GetCompanyInventoryGraph(CompanyInventoryGraphRequest companySaleRequest)
        {
            List<CompanySaleData> companySaleDatas = new List<CompanySaleData>();
            List<CompanyInventoryAgingData> companyInventoryAgingData = new List<CompanyInventoryAgingData>();
            CompanyInventoryGraph companyInventoryGraph = new CompanyInventoryGraph();
            using (var authContext = new AuthContext())
            {
                authContext.Database.CommandTimeout = 600;
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();

                var cmd = authContext.Database.Connection.CreateCommand();
                cmd.CommandText = "[Buyer].[GetCompanyInventoryGrapth]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@companyId", companySaleRequest.CompanyId));
                cmd.Parameters.Add(new SqlParameter("@cityId", companySaleRequest.CityId));
                cmd.Parameters.Add(new SqlParameter("@type", companySaleRequest.type));

                // Run the sproc
                var reader = cmd.ExecuteReader();
                companySaleDatas = ((IObjectContextAdapter)authContext)
                                    .ObjectContext
                                    .Translate<CompanySaleData>(reader).ToList();

                reader.NextResult();
                if (reader.Read())
                {
                    companyInventoryAgingData = ((IObjectContextAdapter)authContext)
                                    .ObjectContext
                                    .Translate<CompanyInventoryAgingData>(reader).ToList();
                }
            }

            if (companySaleDatas != null)
            {
                companyInventoryGraph.BrandInventoryAmount = new List<GraphAmountData>();
                companyInventoryGraph.BrandInventoryQty = new List<GraphQtyData>();
                companyInventoryGraph.ItemInventoryAmount = new List<GraphAmountData>();
                companyInventoryGraph.ItemInventoryQty = new List<GraphQtyData>();
                companyInventoryGraph.ItemAvgAging = new List<GraphQtyData>();
                companyInventoryGraph.BrandAvgAging = new List<GraphQtyData>();
                companyInventoryGraph.BrandInventoryAmount = companySaleDatas.GroupBy(x => x.BrandName).Select(x => new GraphAmountData
                {
                    Name = x.Key,
                    Amount = x.Sum(y => y.Amount)
                }).ToList();
                companyInventoryGraph.BrandInventoryQty = companySaleDatas.GroupBy(x => x.BrandName).Select(x => new GraphQtyData
                {
                    Name = x.Key,
                    Qty = x.Sum(y => y.Qty)
                }).ToList();

                companyInventoryGraph.ItemInventoryAmount = companySaleDatas.GroupBy(x => x.itemname).Select(x => new GraphAmountData
                {
                    Name = x.Key,
                    Amount = x.Sum(y => y.Amount)
                }).ToList();
                companyInventoryGraph.ItemInventoryQty = companySaleDatas.GroupBy(x => x.itemname).Select(x => new GraphQtyData
                {
                    Name = x.Key,
                    Qty = x.Sum(y => y.Qty)
                }).ToList();

                companyInventoryGraph.BrandAvgAging = companyInventoryAgingData.GroupBy(x => x.BrandName).Select(x => new GraphQtyData
                {
                    Name = x.Key,
                    Qty = x.Sum(y => y.Qty)
                }).ToList();

                companyInventoryGraph.ItemAvgAging = companyInventoryAgingData.GroupBy(x => x.itemname).Select(x => new GraphQtyData
                {
                    Name = x.Key,
                    Qty = x.Sum(y => y.Qty)
                }).ToList();

            }
            return companyInventoryGraph;


        }


        [Route("GetCompanyOrder")]
        [HttpPost]
        public async Task<List<CompanyOrder>> GetCompanyOrder(SellerRequest sellerRequest)
        {
            List<CompanyOrder> CompanyOrders = new List<CompanyOrder>();

            string sDate = sellerRequest.StartDate.Date.ToString("yyyy-MM-dd");
            string eDate = sellerRequest.EndDate.Date.AddDays(1).ToString("yyyy-MM-dd");

            StringBuilder whereCond = new StringBuilder($" compid={sellerRequest.CompanyId}");

            if (sellerRequest.CityId > 0)
                whereCond.Append($" and cityid={sellerRequest.CityId}");

            if (sellerRequest.BrandIds != null && sellerRequest.BrandIds.Any())
                whereCond.Append($" and brandid in ({string.Join(",", sellerRequest.BrandIds.Select(s => s))})");

            //string sqlQuery = $"select status,count(distinct orderid) OrderCount from {platformIdxName} where ((createddate>='{sDate}' and createddate < '{eDate}') or (updateddate>='{sDate}' and updateddate < '{eDate}')) {whereCond.ToString()}  group by status";

            whereCond.Append($" and ((createddate>='{sDate}' and createddate < '{eDate}') or (updateddate>='{sDate}' and updateddate < '{eDate}'))");

            List<CompanyOrderList> CompanyOrdersList = new List<CompanyOrderList>();

            string sqlQuery = $"select orderid,max(createddate) createddate, max(updateddate) updateddate,max(delivereddate) delivereddate,cityname, sum(ordqty*price) orderamount, sum(dispatchqty * price) dispatchamount, status from {platformIdxName} where {whereCond.ToString()}  group by orderid,cityname,status";


            ElasticSqlHelper<CompanyOrderListExtendedElastic> elasticSqlHelper = new ElasticSqlHelper<CompanyOrderListExtendedElastic>();

            var statusData = (await elasticSqlHelper.GetListAsync(sqlQuery)).ToList();


            CompanyOrders.Add
                (
                         new CompanyOrder
                         {
                             Status = "Pending",
                             OrderCount = statusData.Where(s => s.createddate >= sellerRequest.StartDate && (s.createddate <= sellerRequest.EndDate && s.status == "Pending" || s.status == "ReadyToPick")).Select(d => d.orderid).Distinct().Count()
                         }
                );

            CompanyOrders.Add
               (
                        new CompanyOrder
                        {
                            Status = "Issued",
                            OrderCount = statusData.Where(s => s.updateddate >= sellerRequest.StartDate && s.updateddate <= sellerRequest.EndDate && (s.status == "Issued" || s.status == "Ready to Dispatch")).Select(d => d.orderid).Distinct().Count()
                        }
               );

            CompanyOrders.Add
               (
                        new CompanyOrder
                        {
                            Status = "Shipped",
                            OrderCount = statusData.Where(s => s.updateddate >= sellerRequest.StartDate && s.updateddate <= sellerRequest.EndDate && s.status == "Shipped").Select(d => d.orderid).Distinct().Count()
                        }
               );

            CompanyOrders.Add
               (
                        new CompanyOrder
                        {
                            Status = "Redispatch",
                            OrderCount = statusData.Where(s => s.updateddate >= sellerRequest.StartDate && s.updateddate <= sellerRequest.EndDate && s.status == "Delivery Redispatch").Select(d => d.orderid).Distinct().Count()
                        }
               );

            CompanyOrders.Add
               (
                        new CompanyOrder
                        {
                            Status = "Cancelled",
                            OrderCount = statusData.Where(s => s.updateddate >= sellerRequest.StartDate && s.updateddate <= sellerRequest.EndDate && (s.status == "Post Order Canceled" || s.status == "Delivery Canceled" || s.status == "Order Canceled")).Select(d => d.orderid).Distinct().Count()
                        }
               );

            CompanyOrders.Add
               (
                        new CompanyOrder
                        {
                            Status = "Delivered",
                            OrderCount = statusData.Where(s => s.delivereddate.HasValue && s.delivereddate.Value >= sellerRequest.StartDate && s.delivereddate.Value <= sellerRequest.EndDate && (s.status == "Delivered" || s.status == "sattled")).Select(d => d.orderid).Distinct().Count()
                        }
               );

            return CompanyOrders;

        }


        [Route("GetCompanyOrderList")]
        [HttpPost]
        public async Task<List<CompanyOrderList>> GetCompanyOrderList(SellerDashboardRequest sellerDashboardRequest)
        {
            List<CompanyOrderList> CompanyOrders = new List<CompanyOrderList>();

            string sDate = sellerDashboardRequest.StartDate.Date.ToString("yyyy-MM-dd");
            string eDate = sellerDashboardRequest.EndDate.Date.AddDays(1).ToString("yyyy-MM-dd");

            StringBuilder whereCond = new StringBuilder($" compid={sellerDashboardRequest.CompanyId}");

            if (sellerDashboardRequest.CityId > 0)
                whereCond.Append($" and cityid={sellerDashboardRequest.CityId}");

            if (sellerDashboardRequest.BrandIds != null && sellerDashboardRequest.BrandIds.Any())
                whereCond.Append($" and brandid in ({string.Join(",", sellerDashboardRequest.BrandIds.Select(s => s))})");

            if (sellerDashboardRequest.DateRangeType == "Pending")
                whereCond.Append($" and createddate>='{sDate}' and createddate < '{eDate}' and (status='Pending' or status='ReadyToPick')");
            else if (sellerDashboardRequest.DateRangeType == "Issued")
                whereCond.Append($" and updateddate>='{sDate}' and updateddate < '{eDate}' and (status='Issued' or status='Ready to Dispatch')");
            else if (sellerDashboardRequest.DateRangeType == "Shipped")
                whereCond.Append($" and updateddate>='{sDate}' and updateddate < '{eDate}' and (status='Shipped')");
            else if (sellerDashboardRequest.DateRangeType == "Redispatch")
                whereCond.Append($" and updateddate>='{sDate}' and updateddate < '{eDate}' and (status='Delivery Redispatch')");
            else if (sellerDashboardRequest.DateRangeType == "Cancelled")
                whereCond.Append($" and updateddate>='{sDate}' and updateddate < '{eDate}' and (status='Post Order Canceled' or status='Delivery Canceled' or status = 'Order Canceled')");
            else if (sellerDashboardRequest.DateRangeType == "Delivered")
                whereCond.Append($" and updateddate>='{sDate}' and updateddate < '{eDate}' and (status='Delivered' or status='sattled')");


            string sqlQuery = $"select orderid,createddate,cityname, sum(ordqty*price) orderamount, sum(dispatchqty * price) dispatchamount, status from {platformIdxName} where {whereCond.ToString()}  group by orderid,createddate,cityname,status";


            ElasticSqlHelper<CompanyOrderListElastic> elasticSqlHelper = new ElasticSqlHelper<CompanyOrderListElastic>();

            var elasticData = (await elasticSqlHelper.GetListAsync(sqlQuery)).ToList();

            CompanyOrders = elasticData.Select(s => new CompanyOrderList
            {
                BillAmount = sellerDashboardRequest.DateRangeType == "Pending" ? (int)s.orderamount : (int)s.dispatchamount,
                City = s.cityname,
                Createddate = s.createddate,
                OrderId = s.orderid,
                OrderStatus = sellerDashboardRequest.DateRangeType
            }).ToList();


            return CompanyOrders;
        }


        [Route("GetCompanyOrderDetail")]
        [HttpGet]
        public async Task<List<CompanyOrderDetail>> GetCompanyOrderDetail(int OrderId)
        {
            var Ids = GetReqParam();
            List<CompanyOrderDetail> CompanyOrders = new List<CompanyOrderDetail>();

            string sqlQuery = $"select itemname,mrp,price sellingprice, ordqty,dispatchqty from {platformIdxName} where orderid={OrderId} and compid={Ids.SubCatId}";


            ElasticSqlHelper<CompanyOrderDetailElastic> elasticSqlHelper = new ElasticSqlHelper<CompanyOrderDetailElastic>();

            var elasticData = (await elasticSqlHelper.GetListAsync(sqlQuery)).ToList();

            CompanyOrders = elasticData.Select(s => new CompanyOrderDetail
            {
                ItemName = s.itemname,
                MRP = s.mrp,
                SellingPrice = s.sellingprice,
                Qty = s.dispatchqty.HasValue ? s.dispatchqty.Value : s.ordqty
            }).ToList();

            return CompanyOrders;
        }

        [Route("ExportCompanyOrderList")]
        [HttpPost]
        public async Task<List<ExportCompanyOrderList>> ExportCompanyOrderList(SellerDashboardRequest sellerDashboardRequest)
        {
            List<ExportCompanyOrderList> ExportCompanyOrders = new List<ExportCompanyOrderList>();

            var node = new Uri(ConfigurationManager.AppSettings["ElasticSearchBaseUrl"]);
            var pool = new SingleNodeConnectionPool(node);
            String userName = ConfigurationManager.AppSettings["elasticUserName"];
            String password = ConfigurationManager.AppSettings["elasticPassword"];

            var settings = new ConnectionSettings(pool)
               .DefaultMappingFor<skorderdata>(m => m
                   .IndexName(platformIdxName)

               ).BasicAuthentication(userName, password);

            var client = new ElasticClient(settings);

            var filters = new List<Func<QueryContainerDescriptor<skorderdata>, QueryContainer>>();

            filters.Add(fq => fq.Terms(t => t.Field("compid").Terms(new[] { sellerDashboardRequest.CompanyId })));

            if (sellerDashboardRequest.CityId > 0)
                filters.Add(fq => fq.Terms(t => t.Field("cityid").Terms(new[] { sellerDashboardRequest.CityId })));

            if (sellerDashboardRequest.BrandIds != null && sellerDashboardRequest.BrandIds.Any())
                filters.Add(fq => fq.Terms(t => t.Field("brandid").Terms(sellerDashboardRequest.BrandIds.ToArray())));



            filters.Add(fq => fq.DateRange(t => t.Field("createddate").GreaterThanOrEquals(sellerDashboardRequest.StartDate)));
            filters.Add(fq => fq.DateRange(t => t.Field("createddate").LessThanOrEquals(sellerDashboardRequest.EndDate)));


            var countResponse =
               client.Count<skorderdata>(x => x.Query(q => q
               .Bool(bq => bq.Filter(filters)))
               ).Count;

            int batchSize = 1000;
            List<int> batches = new List<int>();

            for (int i = 0; i < countResponse; i += batchSize)
            {
                batches.Add(i);
            }

            ConcurrentBag<skorderdata> rawDatasBag = new ConcurrentBag<skorderdata>();

            var platformResult = Parallel.ForEach(batches, item =>
            {
                var datas =
                 client.Search<skorderdata>(x => x
                 .Query(q => q
                 .Bool(bq => bq.Filter(filters))).Skip(item).Take(batchSize)
                 ).Documents
                 .ToList()
                 ;

                foreach (var data in datas)
                {
                    rawDatasBag.Add(data);
                }
            });

            if (platformResult.IsCompleted)
            {
                ExportCompanyOrders = rawDatasBag.GroupBy(s => new { s.orderid, s.orderdetailid, s.status, s.cityname, s.skcode, s.shopname, s.itemname, s.orderby })
                   .OrderBy(d => d.Key.orderid).Select(s => new ExportCompanyOrderList
                   {
                       City = s.Key.cityname,
                       CreatedDate = s.Min(d => d.createddate),
                       Deliverydate = s.Min(d => d.deliverydate),
                       DispatchedAmount = s.Any(d => d.dispatchdate.HasValue) ? s.Where(d => d.dispatchqty.HasValue).Sum(d => d.dispatchqty.Value * d.price) : 0,
                       DispatchQuantity = s.Any(d => d.dispatchdate.HasValue) ? s.Where(d => d.dispatchqty.HasValue).Sum(d => d.dispatchqty.Value) : 0,
                       ItemName = s.Key.itemname,
                       OrderBy = s.Key.orderby,
                       OrderedAmount = s.Sum(d => d.ordqty * d.price),
                       OrderedQuantity = s.Sum(d => d.ordqty),
                       OrderId = s.Key.orderid,
                       OrderStatus = s.Key.status == "ReadyToPick" || s.Key.status == "Pending" ? "Pending"
                                     : s.Key.status == "Ready to Dispatch" || s.Key.status == "Issued" ? "Issued"
                                     : s.Key.status == "Delivery Redispatch" ? "Redispatch"
                                     : s.Key.status == "Post Order Canceled" || s.Key.status == "Delivery Canceled" || s.Key.status == "Order Canceled" ? "Cancelled"
                                     : s.Key.status == "sattled" || s.Key.status == "Delivered" ? "Delivered"
                                     : s.Key.status,
                       PaymentType = s.Min(d => d.paymenttype),
                       Remarks = s.Min(d => d.orderremark),
                       ShopName = s.Key.shopname,
                       Skcode = s.Key.skcode,

                   }).ToList();
            }

            return ExportCompanyOrders;
        }

        [Route("ExportCompanySalesList")]
        [HttpPost]
        public async Task<List<ExportCompanyOrderList>> ExportCompanySalesList(SellerDashboardRequest sellerDashboardRequest)
        {
            List<ExportCompanyOrderList> ExportCompanyOrders = new List<ExportCompanyOrderList>();

            var node = new Uri(ConfigurationManager.AppSettings["ElasticSearchBaseUrl"]);
            var pool = new SingleNodeConnectionPool(node);
            String userName = ConfigurationManager.AppSettings["elasticUserName"];
            String password = ConfigurationManager.AppSettings["elasticPassword"];

            var settings = new ConnectionSettings(pool)
               .DefaultMappingFor<skorderdata>(m => m
                   .IndexName(platformIdxName)

               ).BasicAuthentication(userName, password);

            var client = new ElasticClient(settings);

            var filters = new List<Func<QueryContainerDescriptor<skorderdata>, QueryContainer>>();

            filters.Add(fq => fq.Terms(t => t.Field("compid").Terms(new[] { sellerDashboardRequest.CompanyId })));

            if (sellerDashboardRequest.CityId > 0)
                filters.Add(fq => fq.Terms(t => t.Field("cityid").Terms(new[] { sellerDashboardRequest.CityId })));

            filters.Add(fq => fq.DateRange(t => t.Field("createddate").GreaterThanOrEquals(sellerDashboardRequest.StartDate)));
            filters.Add(fq => fq.DateRange(t => t.Field("createddate").LessThanOrEquals(sellerDashboardRequest.EndDate)));


            var countResponse =
               client.Count<skorderdata>(x => x.Query(q => q
               .Bool(bq => bq.Filter(filters)))
               ).Count;

            int batchSize = 1000;
            List<int> batches = new List<int>();

            for (int i = 0; i < countResponse; i += batchSize)
            {
                batches.Add(i);
            }

            ConcurrentBag<skorderdata> rawDatasBag = new ConcurrentBag<skorderdata>();

            var platformResult = Parallel.ForEach(batches, item =>
            {
                var datas =
                 client.Search<skorderdata>(x => x
                 .Query(q => q
                 .Bool(bq => bq.Filter(filters))).Skip(item).Take(batchSize)
                 ).Documents
                 .ToList()
                 ;

                foreach (var data in datas)
                {
                    rawDatasBag.Add(data);
                }
            });

            if (platformResult.IsCompleted)
            {
                ExportCompanyOrders = rawDatasBag.GroupBy(s => new { s.orderid, s.orderdetailid, s.status, s.cityname, s.skcode, s.shopname, s.itemname, s.orderby })
                     .OrderBy(d => d.Key.orderid).Select(s => new ExportCompanyOrderList
                     {
                         City = s.Key.cityname,
                         CreatedDate = s.Min(d => d.createddate),
                         Deliverydate = s.Min(d => d.deliverydate),
                         DispatchedAmount = s.Any(d => d.dispatchdate.HasValue) ? s.Where(d => d.dispatchqty.HasValue).Sum(d => d.dispatchqty.Value * d.price) : 0,
                         DispatchQuantity = s.Any(d => d.dispatchdate.HasValue) ? s.Where(d => d.dispatchqty.HasValue).Sum(d => d.dispatchqty.Value) : 0,
                         ItemName = s.Key.itemname,
                         OrderBy = s.Key.orderby,
                         OrderedAmount = s.Sum(d => d.ordqty * d.price),
                         OrderedQuantity = s.Sum(d => d.ordqty),
                         OrderId = s.Key.orderid,
                         OrderStatus = s.Key.status == "ReadyToPick" || s.Key.status == "Pending" ? "Pending"
                                    : s.Key.status == "Ready to Dispatch" || s.Key.status == "Issued" ? "Issued"
                                    : s.Key.status == "Delivery Redispatch" ? "Redispatch"
                                    : s.Key.status == "Post Order Canceled" || s.Key.status == "Delivery Canceled" || s.Key.status == "Order Canceled" ? "Cancelled"
                                    : s.Key.status == "sattled" || s.Key.status == "Delivered" ? "Delivered"
                                    : s.Key.status,
                         PaymentType = s.Min(d => d.paymenttype),
                         Remarks = s.Min(d => d.orderremark),
                         ShopName = s.Key.shopname,
                         Skcode = s.Key.skcode
                     }).ToList();
            }

            return ExportCompanyOrders;

        }



        [Route("InsertOrderDataInElastic")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> InsertOrderDataInElastic()
        {


            var query = $"select max(rundate) rundate from {orderdatajobdate}";

            ElasticSqlHelper<orderdataschedule> elasticHelper = new ElasticSqlHelper<orderdataschedule>();
            var maxTDateresult = (await elasticHelper.GetListAsync(query)).FirstOrDefault();


            List<skorderdata> custRawDatas = GetRawDataFromDB(maxTDateresult?.rundate);

            if (custRawDatas.Any())
                await ProcessToInsertInElastic(custRawDatas, maxTDateresult?.rundate);



            return true;
        }

        [Route("InsertCurrentMonthOrderDataInElastic")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> InsertCurrentMonthOrderDataInElastic()
        {
            var todayDate = DateTime.Now;
            var maxTDateresult = new DateTime(todayDate.Year, todayDate.Month, 1);

            await DropOrderDataFromElastic(maxTDateresult);

            List<skorderdata> custRawDatas = GetRawDataFromDB(maxTDateresult);

            if (custRawDatas.Any())
                await ProcessToInsertInElastic(custRawDatas, maxTDateresult, true);


            return true;
        }

        private List<skorderdata> GetRawDataFromDB(DateTime? maxDateFromElastic = null)
        {

            #region Prepare Final List to insert in Elastic
            List<skorderdata> allcustRawData = new List<skorderdata>();

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString))
            {
                string sqlquery = "Select  * from OrderDataForSellerDashboard with(nolock)";
                var dataTable = new DataTable();

                if (maxDateFromElastic.HasValue)
                {
                    string datetime = maxDateFromElastic.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    sqlquery = $"exec Buyer.GetOrderDataToInsertInElastic '{datetime}'";
                }

                using (var command = new SqlCommand(sqlquery.ToString(), connection))
                {
                    command.CommandTimeout = 1000;

                    if (connection.State != ConnectionState.Open)
                        connection.Open();


                    SqlDataAdapter da = new SqlDataAdapter(command);
                    da.Fill(dataTable);
                    da.Dispose();
                }


                connection.Close();

                if (dataTable.Rows.Count > 0)
                {
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        try
                        {
                            allcustRawData.Add(new skorderdata
                            {
                                orderid = dataTable.Rows[i].Field<int>("orderid"),
                                orderdetailid = dataTable.Rows[i].Field<int>("orderdetailid"),
                                brandid = dataTable.Rows[i].Field<int>("brandid"),
                                catid = dataTable.Rows[i].Field<int>("catid"),
                                catname = dataTable.Rows[i].Field<string>("catname"),
                                brandname = dataTable.Rows[i].Field<string>("brandname"),
                                cityid = dataTable.Rows[i].Field<int?>("cityid"),
                                compid = dataTable.Rows[i].Field<int>("compid"),
                                cityname = dataTable.Rows[i].Field<string>("CityName"),
                                compname = dataTable.Rows[i].Field<string>("compname"),
                                createddate = dataTable.Rows[i].Field<DateTime>("createddate"),
                                custid = dataTable.Rows[i].Field<int>("custid"),
                                delivereddate = dataTable.Rows[i].Field<DateTime?>("delivereddate"),
                                deliverydate = dataTable.Rows[i].Field<DateTime>("deliverydate"),
                                dispatchdate = dataTable.Rows[i].Field<DateTime?>("dispatchdate"),
                                dispatchqty = dataTable.Rows[i].Field<int?>("dispatchqty"),
                                itemmultimrpid = dataTable.Rows[i].Field<int>("itemmultimrpid"),
                                itemname = dataTable.Rows[i].Field<string>("itemname"),
                                itemnumber = dataTable.Rows[i].Field<string>("itemNumber"),
                                lat = dataTable.Rows[i].Field<double>("lat"),
                                lng = dataTable.Rows[i].Field<double>("lng"),
                                mrp = dataTable.Rows[i].Field<double>("mrp"),
                                orderby = dataTable.Rows[i].Field<string>("orderby"),
                                orderremark = dataTable.Rows[i].Field<string>("orderremark"),
                                ordqty = dataTable.Rows[i].Field<int>("ordqty"),
                                paymenttype = dataTable.Rows[i].Field<string>("paymenttype"),
                                price = dataTable.Rows[i].Field<double>("price"),
                                shopname = dataTable.Rows[i].Field<string>("shopname"),
                                skcode = dataTable.Rows[i].Field<string>("skcode"),
                                status = dataTable.Rows[i].Field<string>("status"),
                                updateddate = dataTable.Rows[i].Field<DateTime>("updateddate"),
                                whid = dataTable.Rows[i].Field<int>("whid"),
                                whname = dataTable.Rows[i].Field<string>("whname"),
                                billdiscountamount = dataTable.Rows[i].Field<double?>("billdiscountamount"),
                                deliverycharge = dataTable.Rows[i].Field<double?>("deliverycharge"),
                                discountamount = dataTable.Rows[i].Field<double>("discountamount"),
                                grossamount = dataTable.Rows[i].Field<double>("grossamount"),
                                taxamount = dataTable.Rows[i].Field<double>("taxamount"),
                                tcsamount = dataTable.Rows[i].Field<double>("tcsamount"),
                                walletamount = dataTable.Rows[i].Field<double?>("walletamount"),
                                clusterid = dataTable.Rows[i].Field<int>("ClusterId"),
                                clustername = dataTable.Rows[i].Field<string>("ClusterName"),
                                ordertakensalespersonid = dataTable.Rows[i].Field<int?>("OrderTakenSalesPersonId"),
                                storeid = dataTable.Rows[i].Field<long>("StoreId"),
                                executiveid = dataTable.Rows[i].Field<int>("ExecutiveId"),
                                isdigitalorder = dataTable.Rows[i].Field<bool?>("IsDigitalOrder"),
                                customertype = dataTable.Rows[i].Field<string>("CustomerType"),
                                customerclass = dataTable.Rows[i].Field<string>("CustomerClass"),
                                isfreeitem = dataTable.Rows[i].Field<bool>("IsFreeItem"),
                                ordertype = dataTable.Rows[i].Field<int>("OrderType"),
                                channelmasterid = dataTable.Rows[i].Field<long>("ChannelMasterId")
                            });
                        }
                        catch (Exception ex)
                        {
                            TextFileLogHelper.LogError($"Error for Order: {dataTable.Rows[i].Field<int>("orderid")} :: {Environment.NewLine} {ex.ToString()}");
                        }

                    }

                }

            }

            #endregion


            return allcustRawData;

        }

        async Task ProcessToInsertInElastic(List<skorderdata> custRawDatas, DateTime? maxDateFromElastic = null, bool IsMonthDataSync = false)
        {
            ElasticSqlHelper<skorderdata> crmRawDataHelper = new ElasticSqlHelper<skorderdata>();

            if (maxDateFromElastic.HasValue)
            {

                var node = new Uri(ConfigurationManager.AppSettings["ElasticSearchBaseUrl"]);
                var pool = new SingleNodeConnectionPool(node);
                String userName = ConfigurationManager.AppSettings["elasticUserName"];
                String password = ConfigurationManager.AppSettings["elasticPassword"];

                var settings = new ConnectionSettings(pool)
                 .DefaultMappingFor<skorderdata>(m => m
                     .IndexName(platformIdxName)

                 ).BasicAuthentication(userName, password);

                ElasticClient elasticClient = new ElasticClient(settings);


                ParallelLoopResult result = Parallel.ForEach(custRawDatas, item =>
                {
                    //string q = $"select * from {platformIdxName} where orderid='{item.orderid}' and orderdetailid={item.orderdetailid}";
                    //var rawdata = AsyncContext.Run(() => crmRawDataHelper.GetListAsync(q)).ToList();

                    //if (rawdata != null && rawdata.Any())
                    //{
                    try
                    {
                        var response = elasticClient.DeleteByQuery<skorderdata>(x => x.Query(eq =>
                                            eq.Bool(bq =>
                                            bq.Filter(
                                                fq => fq.Terms(t => t.Field(f => f.orderid).Terms(item.orderid)),
                                                fq => fq.Range(t => t.Field(f => f.orderdetailid).GreaterThanOrEquals(item.orderdetailid))
                                           ))).Index(platformIdxName)
                                        );
                    }
                    catch (Exception ex)
                    {
                        TextFileLogHelper.LogError($"InsertOrderDataInElastic -- orderid:{item.orderid} , orderdetailid: {item.orderdetailid} {Environment.NewLine} --> {ex.ToString()}");
                    }
                    //}
                });
            }


            ElasticNestHelper<skorderdata> elasticInsertHelper = new ElasticNestHelper<skorderdata>();
            await elasticInsertHelper.InsertDataToIndex(platformIdxName, custRawDatas);

            if (!IsMonthDataSync)
            {
                var maxcdate = custRawDatas.Max(s => s.createddate);
                var maxudate = custRawDatas.Max(s => s.updateddate);
                var maxDate = new List<orderdataschedule> { new orderdataschedule { rundate = maxcdate > maxudate ? maxcdate : maxudate } };

                ElasticNestHelper<orderdataschedule> scheduleInsertHelper = new ElasticNestHelper<orderdataschedule>();
                await scheduleInsertHelper.InsertDataToIndex(orderdatajobdate, maxDate);
            }

        }

        async Task DropOrderDataFromElastic(DateTime fromDate)
        {
            var node = new Uri(ConfigurationManager.AppSettings["ElasticSearchBaseUrl"]);
            var pool = new SingleNodeConnectionPool(node);
            String userName = ConfigurationManager.AppSettings["elasticUserName"];
            String password = ConfigurationManager.AppSettings["elasticPassword"];

            var settings = new ConnectionSettings(pool)
             .DefaultMappingFor<skorderdata>(m => m
                 .IndexName(platformIdxName)

             ).BasicAuthentication(userName, password);

            ElasticClient elasticClient = new ElasticClient(settings);
            var response = elasticClient.DeleteByQuery<skorderdata>(x => x.Query(eq =>
                                      eq.Bool(sq =>
                                          sq.Should(pq =>
                                                    pq.Bool(bq =>
                                                            bq.Must(
                                                                  fq => fq.DateRange(t => t.Field(f => f.createddate).GreaterThan(fromDate))
                                                            )
                                                    ),
                                                    dq => dq.Bool(bq =>
                                                             bq.Must(
                                                                   fq => fq.DateRange(t => t.Field(f => f.updateddate).GreaterThan(fromDate))
                                                             )
                                                          )
                                                    )
                                          )).Index(platformIdxName)
                                        );
            long total = response.Total;
        }

        [Route("GetLastUpdatedDate")]
        [HttpGet]
        public async Task<orderdataschedule> GetLastUpdatedDate()
        {
            var query = $"select max(rundate) rundate from {orderdatajobdate}";
            ElasticSqlHelper<orderdataschedule> elasticHelper = new ElasticSqlHelper<orderdataschedule>();
            var maxTDateresult = (await elasticHelper.GetListAsync(query)).FirstOrDefault();
            return maxTDateresult;
        }

        #region Insert Missing Order in Elastic

        [HttpGet]
        [Route("InsertMissingOrder")]
        [AllowAnonymous]
        public async Task InsertMissingOrderInElastic()
        {
            DateTime now = DateTime.Now;
            var startDate = new DateTime(now.Year, now.Month, 1);
            var endDate = startDate.AddMonths(1);
            using (var Context = new AuthContext())
            {
                #region ElasticOrderDatasdf
                string sDate = startDate.Date.ToString("yyyy-MM-dd");
                string eDate = endDate.Date.ToString("yyyy-MM-dd");
                ElasticSqlHelper<GetElsaticOrderIdDC> elasticSqlGroupMetricsHelper = new ElasticSqlHelper<GetElsaticOrderIdDC>();
                var ElasticOrderIds = (await elasticSqlGroupMetricsHelper.GetListAsync($"SELECT orderid from  {platformIdxName} " + $"where  createddate>='" + sDate + "' and createddate<'" + eDate + "' group by orderid")).ToList();

                #endregion

                var DbOrderIds = Context.Database.SqlQuery<GetElsaticOrderIdDC>("GetOrderIdForElasticJob").ToList();
                var sqlOrdrrIds = DbOrderIds.Select(x => x.orderid).ToList();

                var MissingOrderIds = sqlOrdrrIds.Where(x => !ElasticOrderIds.Select(y => y.orderid).Distinct().ToList().Contains(x)).ToList();
                if (MissingOrderIds != null && MissingOrderIds.Count > 0)
                {
                    var orderid = new DataTable();
                    orderid.Columns.Add("intValue");
                    foreach (var id in MissingOrderIds)
                    {
                        var dr = orderid.NewRow();
                        dr["intValue"] = id;
                        orderid.Rows.Add(dr);
                    }
                    var ids = new SqlParameter("@OrderIds", orderid);
                    ids.SqlDbType = SqlDbType.Structured;
                    ids.TypeName = "dbo.intValues";

                    var MissingDataList = Context.Database.SqlQuery<skorderdata>("GetOrderDataToInsertInElasticJob @OrderIds", ids).ToList();
                    ElasticNestHelper<skorderdata> elasticInsertHelper = new ElasticNestHelper<skorderdata>();
                    await elasticInsertHelper.InsertDataToIndex(platformIdxName, MissingDataList);
                }
            }
        }


        [HttpGet]
        [Route("UpdateOrderStatusInElastic")]
        [AllowAnonymous]
        public async Task UpdateOrderStatusInElastic()
        {
            DateTime now = DateTime.Now;
            var startDate = new DateTime(now.Year, now.Month, 1);
            var endDate = startDate.AddMonths(1);
            using (var Context = new AuthContext())
            {
                #region ElasticOrderDatasdf
                string sDate = startDate.Date.ToString("yyyy-MM-dd");
                string eDate = endDate.Date.ToString("yyyy-MM-dd");
                ElasticSearchOrderHelper obj = new ElasticSearchOrderHelper();
                //ElasticSqlHelper<GetElsaticOrderIdDC> elasticSqlGroupMetricsHelper = new ElasticSqlHelper<GetElsaticOrderIdDC>();
                //var ElasticOrderIds = (await elasticSqlGroupMetricsHelper.GetListAsync($"SELECT orderid,status from  {platformIdxName} " + $"where  createddate>='" + sDate + "' and createddate<'" + eDate + "' group by orderid,status")).ToList();
                string query = $@"{{""query"": {{ ""range"": {{ ""createddate"": {{ ""gte"": ""{sDate}"",""lt"": ""{eDate}""}}
        }}}},""_source"": [""orderid"",""status""],""from"": #skip,""size"": #take}}";
                var ElasticOrderIds = ElasticSearchOrderHelper.GetAllOrderSearchQuery(query, platformIdxName, 0, 1000);
                #endregion

                var OrderStatus = Context.Database.SqlQuery<GetElsaticOrderIdDC>("GetOrderIdForElasticJob").ToList();

                List<int> orderidList = new List<int>();
                ElasticOrderIds.ForEach(x =>
                {
                    var id = OrderStatus.FirstOrDefault(y => y.orderid == x.orderid && y.status != x.status) != null ? OrderStatus.FirstOrDefault(y => y.orderid == x.orderid && y.status != x.status).orderid : 0;
                    if (id > 0)
                    {
                        orderidList.Add(id);
                    }
                });

                if (orderidList != null && orderidList.Count > 0)
                {
                    var orderid = new DataTable();
                    orderid.Columns.Add("intValue");
                    foreach (var id in orderidList)
                    {
                        var dr = orderid.NewRow();
                        dr["intValue"] = id;
                        orderid.Rows.Add(dr);
                    }
                    var ids = new SqlParameter("@OrderIds", orderid);
                    ids.SqlDbType = SqlDbType.Structured;
                    ids.TypeName = "dbo.intValues";

                    var MissingDataList = Context.Database.SqlQuery<skorderdata>("GetOrderDataToInsertInElasticJob @OrderIds", ids).ToList();
                    ElasticNestHelper<skorderdata> elasticInsertHelper = new ElasticNestHelper<skorderdata>();
                    var descriptor = new Nest.BulkDescriptor(platformIdxName);
                    foreach (var i in MissingDataList.Select(x => x.orderid).Distinct())
                    {
                        foreach (var item in ElasticOrderIds.Where(x => x.orderid == i))
                        {
                            var id = item._id;
                            descriptor.Delete<ElasticOrderIds>(op => op
                                .Document(new ElasticOrderIds { id = id })
                            );
                        }
                    }
                    await elasticInsertHelper.BulkDeleteByObject(platformIdxName, descriptor);
                    await elasticInsertHelper.InsertDataToIndex(platformIdxName, MissingDataList);
                }
            }
        }
        #endregion

        [Route("GetSellerExecutiveListOfStoretwo/{warehouseId}/{SubCatId}")]
        [HttpGet]
        public List<ClusterExecutive> GetExecutiveListOfStoretwo(int warehouseId,int SubCatId)
        {
            List<ClusterExecutive> result = null;
            using (var context = new AuthContext())
            {
                var idParam = new SqlParameter("WarehouseId", SqlDbType.Int);
                idParam.Value = warehouseId;

                var SCId = new SqlParameter("SubCatId", SqlDbType.Int);
                SCId.Value = SubCatId;

                result = context.Database.SqlQuery<ClusterExecutive>("exec GetSellerExecutiveListOfStoretwo @WarehouseId,@SubCatId", idParam, SCId).ToList();
            }
            return result;
        }
    }

    public class ElasticOrderIds
    {
        public string id { get; set; }
    }
}




