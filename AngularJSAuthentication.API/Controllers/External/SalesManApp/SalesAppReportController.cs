using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AngularJSAuthentication.Model;
using System.Data.Entity;
using System.Security.Claims;
using LinqKit;
using MongoDB.Driver;
using MongoDB.Bson;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using AngularJSAuthentication.API.Managers;
using System.Data.SqlClient;
using System.Threading.Tasks;
using AngularJSAuthentication.Model.Seller;
using System.Web;
using System.IO;
using System.Data;
using System.Data.Entity.Infrastructure;
using AngularJSAuthentication.DataContracts.Mongo;
using System.Drawing;
using AngularJSAuthentication.DataContracts.External;
using Newtonsoft.Json;
using AngularJSAuthentication.DataContracts.Masters.Store;
using AngularJSAuthentication.Model.Salescommission;

namespace AngularJSAuthentication.API.Controllers.External.SalesManApp
{
    [RoutePrefix("api/SalesAppReport")]
    public class SalesAppReportController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("BrandWise")]
        [HttpGet]
        public dynamic Getdata(string day, int PeopleId)
        {

            using (var db = new AuthContext())
            {
                var people = db.Peoples.Where(x => x.PeopleID == PeopleId && x.Deleted == false).SingleOrDefault();

                if (day != null && PeopleId > 0)
                {
                    List<Target> item = new List<Target>();
                    //item = db.TargetDb.Where(x => x.WarehouseId == people.WarehouseId).ToList();
                    var date = indianTime;
                    var sDate = indianTime.Date;
                    if (day == "1Month")
                    {
                        sDate = indianTime.AddMonths(-1).Date;
                    }
                    else if (day == "3Month")
                    {
                        sDate = indianTime.AddMonths(-3).Date;
                    }
                    var list = (from i in db.DbOrderDetails
                                where i.CreatedDate > sDate && i.CreatedDate <= date && i.WarehouseId == people.WarehouseId && i.ExecutiveId == PeopleId
                                join k in db.itemMasters on i.ItemId equals k.ItemId
                                join l in db.SubsubCategorys on k.SubsubCategoryid equals l.SubsubCategoryid
                                select new SaleDC
                                {
                                    Sale = i.TotalAmt,
                                    SubsubcategoryName = l.SubsubcategoryName,
                                }).ToList();


                    var result = list.GroupBy(d => d.SubsubcategoryName)
                        .Select(
                            g => new
                            {
                                Sale = g.Sum(s => s.Sale),
                                BrandName = g.First().SubsubcategoryName,
                            });
                    return result;
                }

                else
                {
                    return null;
                }
            }

        }


        [Route("SalesManAppNew")]
        [HttpGet]
        public HttpResponseMessage MobileAppV1(DateTime? datefrom, DateTime? dateto, int id)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var orderQuery = string.Format("select distinct sum(totalamt) over(partition by  o.executiveid ) sale ," +
                                                    "sum(totalamt) over(partition by  o.executiveid, o.storeid) Storesale," +
                        " max(isnull(s.name, 'Other'))  over(partition by  o.storeid) StoreName," +
                        " dense_rank() over(partition by  o.executiveid order by od.orderid) + dense_rank() over(partition by  o.executiveid order by od.orderid desc) - 1 OrderCount, " +
                        " dense_rank() over(partition by  o.executiveid order by od.CustomerId) + dense_rank() over(partition by  o.executiveid order by od.CustomerId desc) - 1 OrderCustomerCount, " +
                        "  dense_rank() over(partition by  o.executiveid, o.storeid  order by od.orderid) + dense_rank() over(partition by  o.executiveid, o.storeid  order by od.orderid desc) - 1 StoreOrderCount" +
                        " from OrderDetails o with(nolock)" +
                       " inner join OrderMasters od  with(nolock) on o.OrderId = od.OrderId and o.ExecutiveId = {0}" +
                       " left join stores s with(nolock) on o.StoreId = s.Id", id);

                    int ActiveCustomercount = 0, TotalCustomercount = 0;

                    RetailerAppManager retailerAppManager = new RetailerAppManager();
                    List<DataContracts.Masters.Store.ClusterStoreExecutiveDc> clusterStoreExecutiveDcs = retailerAppManager.GetStoreClusterExecutiveDetail();
                    var clusterIds = clusterStoreExecutiveDcs.Where(x => x.ExecutiveId == id).Select(x => x.ClusterId).Distinct().ToList();
                    var predicate = PredicateBuilder.New<Customer>();
                    predicate = predicate.And(x => x.ClusterId.HasValue && clusterIds.Contains(x.ClusterId.Value) && x.Deleted == false);

                    //var builder = Builders<BsonDocument>.Filter;
                    //var filter = builder.Eq("orderDetails.ExecutiveId", id) & builder.Eq("active", true) & builder.Eq("Deleted", false);
                    if (datefrom != null && dateto != null)
                    {
                        //filter = filter & builder.Gte("CreatedDate", datefrom.Value) & builder.Lte("CreatedDate", dateto.Value);
                        predicate = predicate.And(x => x.CreatedDate >= datefrom && x.CreatedDate <= dateto);
                        orderQuery += string.Format(" where od.CreatedDate between '{0}' and '{1}'", datefrom.Value, dateto.Value);
                    }
                    //MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
                    TotalCustomercount = db.Customers.Count(predicate);
                    predicate = predicate.And(x => x.Active);
                    ActiveCustomercount = db.Customers.Count(predicate);
                    ////IMongoDatabase mogodb = mongoDbHelper.dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
                    //var collection = mongoDbHelper.mongoDatabase.GetCollection<BsonDocument>("OrderMaster");
                    //var aggTotal = collection.Aggregate().Match(filter)
                    //.Group(new BsonDocument
                    //                {
                    //                    {
                    //                      "_id", "$orderDetails.ExecutiveId"
                    //                    },
                    //                    {
                    //                        "total", new BsonDocument
                    //                                     {
                    //                                         {
                    //                                             "$sum", "$orderDetails.TotalAmt"
                    //                                         }
                    //                                     }
                    //                    },
                    //                    {

                    //                         "count", new BsonDocument
                    //                                     {
                    //                                       {
                    //                                           "$sum", 1
                    //                                       }
                    //                                    }
                    //                    }
                    //                }).Project(new BsonDocument
                    //                {
                    //                    {"_id", 0},
                    //                    {"total", 1},
                    //                    {"count", 2},
                    //                });


                    //var doc = aggTotal.FirstOrDefault();

                    var orderData = db.Database.SqlQuery<orderDataDC>(orderQuery).ToList();

                    var res = new
                    {
                        Customercountdata = TotalCustomercount,
                        ActiveCustomer = ActiveCustomercount,
                        OrderCountdata = orderData != null && orderData.Any() ? orderData.FirstOrDefault().OrderCount : 0,
                        TotalOrderAmount = orderData != null && orderData.Any() ? orderData.FirstOrDefault().sale : 0,
                        OrderCustomerCount = orderData != null && orderData.Any() ? orderData.FirstOrDefault().OrderCustomerCount : 0,
                        StoreDetail = orderData != null && orderData.Any() ? orderData.Select(x => new StoreSalesDc { StoreName = x.StoreName, Storesale = x.Storesale, StoreOrderCount = x.StoreOrderCount }).ToList() : new List<StoreSalesDc>()
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                catch (Exception ex)
                {
                    var storedetail = new { StoreName = "", Storesale = 0, StoreOrderCount = 0 };
                    return Request.CreateResponse(HttpStatusCode.OK,

                        new
                        {
                            Customercountdata = 0,
                            ActiveCustomer = 0,
                            OrderCountdata = 0,
                            TotalOrderAmount = 0,
                            OrderCustomerCount = 0,
                            StoreDetail = new List<StoreSalesDc>()
                        });

                }
            }
        }


        [Route("OrderSummary")]
        [HttpGet]
        public HttpResponseMessage OrderSummary(int PeopleId)
        {
            var result = new OrderSummaryDc();
            using (var context = new AuthContext())
            {
                if (PeopleId > 0)
                {
                    var param = new SqlParameter("@PeopleId", PeopleId);
                    result = context.Database.SqlQuery<OrderSummaryDc>("exec [SalesAppOrderSummary] @PeopleId", param).FirstOrDefault();
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }


        [Route("ExecuteSalesTarget")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<SalesTargetResponse> ExecuteSalesTarget(int peopleId, int CustomerId, int skip, int take, string itemName = null)
        {
            SalesTargetResponse response = new SalesTargetResponse();
            List<SalesTargetCustomerItem> result = new List<SalesTargetCustomerItem>();
            using (var context = new AuthContext())
            {
                DateTime startDate, endDate;
                DateTime now = indianTime;
                itemName = itemName == null ? "" : itemName;
                //if (Month == 0)
                //{
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //    endDate = startDate.AddMonths(1).AddDays(-1).AddHours(now.Hour).AddMinutes(now.Minute);
                //}
                //else if (Month == 1)
                //{
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //    endDate = startDate.AddMonths(1).AddDays(-1).AddHours(now.Hour).AddMinutes(now.Minute);
                //    now = now.AddMonths(-1);
                //    startDate = new DateTime(now.Year, now.Month, 1);

                //}
                //else
                //{
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //    endDate = startDate.AddMonths(1).AddDays(-1).AddHours(now.Hour).AddMinutes(now.Minute);
                //    now = now.AddMonths(-2);
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //}

                var subcatid = 0;
                //DateTime date = indianTime.AddMonths(Month);
                List<Object> parameters = new List<object>();
                string sqlquery = "exec Seller.SalesTargetByCustomerId";
                parameters.Add(new SqlParameter("@CustomerId", CustomerId));
                parameters.Add(new SqlParameter("@PeopleId", peopleId));
                parameters.Add(new SqlParameter("@SubCategoryId", subcatid));
                parameters.Add(new SqlParameter("@itemName", itemName));
                //parameters.Add(new SqlParameter("@StartDate", startDate));
                //parameters.Add(new SqlParameter("@EndDate", endDate));
                parameters.Add(new SqlParameter("@skip", skip));
                parameters.Add(new SqlParameter("@take", take));
                sqlquery = sqlquery + " @CustomerId, @PeopleId,@SubCategoryId,@itemName, @skip,@take";
                result = await context.Database.SqlQuery<SalesTargetCustomerItem>(sqlquery, parameters.ToArray()).ToListAsync();

                if (skip == 0)
                {
                    response.AchivePercent = await context.Database.SqlQuery<double>("exec GetAchiveSalesTargetByPeopleId " + peopleId).FirstOrDefaultAsync();
                }
                response.SalesTargetCustomerItems = result;

            }
            return response;
        }

        [Route("CustomerSalesTargetbyBrand")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<SalesTargetCustomerItem>> CustomerSalesTargetbyBrand(int peopleId, int CustomerId, int subCategoryId)
        {
            List<SalesTargetCustomerItem> result = new List<SalesTargetCustomerItem>();
            using (var context = new AuthContext())
            {
                DateTime startDate, endDate;
                DateTime now = indianTime;
                string itemName = "";
                //if (Month == 0)
                //{
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //    endDate = startDate.AddMonths(1).AddDays(-1).AddHours(now.Hour).AddMinutes(now.Minute);
                //}
                //else if (Month == 1)
                //{
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //    endDate = startDate.AddMonths(1).AddDays(-1).AddHours(now.Hour).AddMinutes(now.Minute);
                //    now = now.AddMonths(-1);
                //    startDate = new DateTime(now.Year, now.Month, 1);

                //}
                //else
                //{
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //    endDate = startDate.AddMonths(1).AddDays(-1).AddHours(now.Hour).AddMinutes(now.Minute);
                //    now = now.AddMonths(-2);
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //}

                if (CustomerId > 0)
                {
                    int skip = 0; int take = 100;
                    //  DateTime date = indianTime.AddMonths(Month);
                    List<Object> parameters = new List<object>();
                    string sqlquery = "exec Seller.SalesTargetByCustomerId";
                    parameters.Add(new SqlParameter("@CustomerId", CustomerId));
                    parameters.Add(new SqlParameter("@PeopleId", peopleId));
                    parameters.Add(new SqlParameter("@SubCategoryId", subCategoryId));
                    parameters.Add(new SqlParameter("@itemName", itemName));
                    //parameters.Add(new SqlParameter("@StartDate", startDate));
                    //parameters.Add(new SqlParameter("@EndDate", endDate));
                    parameters.Add(new SqlParameter("@skip", skip));
                    parameters.Add(new SqlParameter("@take", take));
                    sqlquery = sqlquery + " @CustomerId, @PeopleId, @SubCategoryId,@itemName, @skip, @take";
                    result = await context.Database.SqlQuery<SalesTargetCustomerItem>(sqlquery, parameters.ToArray()).ToListAsync();
                }
            }
            return result;
        }

        [Route("BrandWiseCustomerSalesTarget")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<CompanySalesTargetCustomer>> BrandWiseCustomerSalesTarget(int peopleId, int CustomerId, int skip, int take)
        {
            List<CompanySalesTargetCustomer> result = new List<CompanySalesTargetCustomer>();
            using (var context = new AuthContext())
            {
                DateTime startDate, endDate;
                DateTime now = indianTime;

                //if (Month == 0)
                //{
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //    endDate = startDate.AddMonths(1).AddDays(-1).AddHours(now.Hour).AddMinutes(now.Minute);
                //}
                //else if (Month == 1)
                //{
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //    endDate = startDate.AddMonths(1).AddDays(-1).AddHours(now.Hour).AddMinutes(now.Minute);
                //    now = now.AddMonths(-1);
                //    startDate = new DateTime(now.Year, now.Month, 1);

                //}
                //else
                //{
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //    endDate = startDate.AddMonths(1).AddDays(-1).AddHours(now.Hour).AddMinutes(now.Minute);
                //    now = now.AddMonths(-2);
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //}

                if (CustomerId > 0)
                {
                    //DateTime date = indianTime.AddMonths(Month);
                    List<Object> parameters = new List<object>();
                    string sqlquery = "exec Seller.SalesTargetByCustomerBrand";
                    parameters.Add(new SqlParameter("@CustomerId", CustomerId));
                    parameters.Add(new SqlParameter("@PeopleId", peopleId));
                    //parameters.Add(new SqlParameter("@StartDate", startDate));
                    //parameters.Add(new SqlParameter("@EndDate", endDate));
                    parameters.Add(new SqlParameter("@skip", skip));
                    parameters.Add(new SqlParameter("@take", take));
                    sqlquery = sqlquery + " @CustomerId, @PeopleId,  @skip,@take";
                    result = await context.Database.SqlQuery<CompanySalesTargetCustomer>(sqlquery, parameters.ToArray()).ToListAsync();
                }
            }
            return result;
        }


        #region Sales Target page

        [Route("UniqueItemByNumber")]
        [HttpGet]
        public async Task<SalesTargetItemDc> SearchUniqueItemByNumber(string ItemNumber)
        {
            SalesTargetItemDc result = new SalesTargetItemDc();
            using (var context = new AuthContext())
            {
                result = await context.ItemMasterCentralDB.Where(y => y.Number == ItemNumber && y.Deleted == false).Select(x => new SalesTargetItemDc
                {
                    ItemName = x.itemBaseName,
                    ItemNumber = x.Number
                }).FirstOrDefaultAsync();
            }
            return result;
        }

        [Route("InsertUpdateSalesTarget")]
        [HttpPost]
        public async Task<string> InsertUpdateSalesTargets(PostSalesTargetItemDc PostSalesTargetItem)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            string result = "";
            using (var context = new AuthContext())
            {
                if (PostSalesTargetItem != null && PostSalesTargetItem.Id > 0)
                {
                    var item = context.SalesTargets.Where(x => x.Id == PostSalesTargetItem.Id).FirstOrDefault();
                    if (item != null && PostSalesTargetItem.BaseQty >= 0)
                    {
                        item.BaseQty = PostSalesTargetItem.BaseQty;
                        context.Entry(item).State = EntityState.Modified;
                        context.Commit();
                        result = "Record updated successfully";

                    }
                    else { result = "No Record exits"; }
                }
                else
                {
                    bool isexist = context.SalesTargets.Any(x => x.ItemNumber == PostSalesTargetItem.ItemNumber && x.ItemMultiMrpId == PostSalesTargetItem.ItemMultiMrpId && x.StoreId == PostSalesTargetItem.StoreId);
                    if (!isexist)
                    {
                        SalesTarget item = new SalesTarget();
                        item.IsActive = true;
                        item.IsDeleted = false;
                        item.BaseQty = PostSalesTargetItem.BaseQty;
                        item.ItemMultiMrpId = PostSalesTargetItem.ItemMultiMrpId;
                        item.ItemNumber = PostSalesTargetItem.ItemNumber;
                        item.StoreId = PostSalesTargetItem.StoreId;
                        item.CreatedBy = userid;
                        item.CreatedDate = indianTime;
                        item.ModifiedBy = userid;
                        item.ModifiedDate = indianTime;
                        context.SalesTargets.Add(item);
                        context.Commit();
                        result = "Record added successfully";
                    }
                    else { result = "Record already exits"; }

                }

            }
            return result;
        }


        [Route("SalesTargetListByStoreId")]
        [HttpGet]
        public async Task<List<SalesTargetListItemDc>> SalesTargetListByStoreId(long StoreId)
        {
            List<SalesTargetListItemDc> result = new List<SalesTargetListItemDc>();
            using (var context = new AuthContext())
            {
                if (StoreId > 0)
                {
                    //int skip = 0; int take = 100;
                    List<Object> parameters = new List<object>();
                    string sqlquery = "exec Seller.GetSalesTargetByStoreId";
                    parameters.Add(new SqlParameter("@StoreId", StoreId));

                    //parameters.Add(new SqlParameter("@skip", skip));
                    //parameters.Add(new SqlParameter("@take", take));
                    sqlquery = sqlquery + " @StoreId";
                    result = await context.Database.SqlQuery<SalesTargetListItemDc>(sqlquery, parameters.ToArray()).ToListAsync();
                }
            }
            return result;
        }

        [Route("SaleTargetReport")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<SalesTargetCustomerItem>> SaleTargetReportBySKcode(int peopleId)
        {
            List<SalesTargetCustomerItem> result = new List<SalesTargetCustomerItem>();
            using (var context = new AuthContext())
            {

                List<Object> parameters = new List<object>();
                string sqlquery = "exec Seller.SaleTargetReportBySKcode";
                parameters.Add(new SqlParameter("@PeopleId", peopleId));
                sqlquery = sqlquery + "  @PeopleId";
                result = await context.Database.SqlQuery<SalesTargetCustomerItem>(sqlquery, parameters.ToArray()).ToListAsync();
            }
            return result;
        }


        [Route("SaleCustomerRetentionTarget")]
        [HttpGet]
        public async Task<string> SaleCustomerRetentionTarget(int peopleId)
        {
            string expiredHtml = string.Empty;
            List<ExecutiveRetailer> executiveRetailers = new List<ExecutiveRetailer>();
            List<ExecutiveBrandPurchaseRetailer> executiveBrandPurchaseRetailers = new List<ExecutiveBrandPurchaseRetailer>();
            using (var context = new AuthContext())
            {

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetSalesPersonData]";
                cmd.Parameters.Add(new SqlParameter("@peopleId", peopleId));
                cmd.CommandType = CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                executiveRetailers = ((IObjectContextAdapter)context)
                                    .ObjectContext
                                    .Translate<ExecutiveRetailer>(reader).ToList();
                reader.NextResult();
                while (reader.Read())
                {
                    executiveBrandPurchaseRetailers = ((IObjectContextAdapter)context)
                                                       .ObjectContext
                                                       .Translate<ExecutiveBrandPurchaseRetailer>(reader).ToList();
                }

            }

            if (executiveRetailers != null && executiveRetailers.Any())
            {
                int minCustomer = executiveRetailers.Select(x => new
                {
                    Date = new DateTime(x.year, x.month, 1),
                    x.CustomerCount
                }).OrderBy(x => x.Date).FirstOrDefault().CustomerCount;

                int maxCustomer = executiveRetailers.Select(x => new
                {
                    Date = new DateTime(x.year, x.month, 1),
                    x.CustomerCount
                }).OrderByDescending(x => x.Date).FirstOrDefault().CustomerCount;


                maxCustomer = maxCustomer > minCustomer ? minCustomer : maxCustomer;
                int parcent = maxCustomer * 100 / minCustomer;

                MongoDbHelper<ExecutiveCompanyTarget> mongoDbHelper = new MongoDbHelper<ExecutiveCompanyTarget>();
                List<ExecutiveCompanyTarget> ExecutiveCompanyTargets = mongoDbHelper.Select(x => x.SubCategoryId > 0).ToList();
                string pathToHTMLFile = HttpContext.Current.Server.MapPath("~/Templates") + "/SalesAPPBrand.html";
                string content = File.ReadAllText(pathToHTMLFile);
                if (!string.IsNullOrEmpty(content))
                {
                    string html = "<div class='row'><div class='col-xs-3 nopadding' ><b>[name]</b></div><div class='col-xs-9 nopadding'><div class='progress'><div class='progress-bar progress-bar-striped active' role='progressbar' aria-valuenow='[currentvalue]' aria-valuemin='0' aria-valuemax='100' style='background-color: #[color];width:[currentvalue]%'> [currentvalue]%</div></div></div></div>";
                    string retailer = html.Replace("[name]", "Total Retailer").Replace("[currentvalue]", parcent.ToString()).Replace("[color]", "5cb85c");
                    string Brandretailer = "";
                    Random rnd = new Random();
                    foreach (var item in ExecutiveCompanyTargets)
                    {
                        var color = string.IsNullOrEmpty(item.Color) ? String.Format("{0:X6}", rnd.Next(0x1000000)) : item.Color;
                        int customercount = 0;
                        if (executiveBrandPurchaseRetailers != null && executiveBrandPurchaseRetailers.Any(x => x.SubcategoryName == item.SubCategoryName))
                        {
                            var data = executiveBrandPurchaseRetailers.FirstOrDefault(x => x.SubcategoryName == item.SubCategoryName);
                            customercount = data.CustomerCount > item.CustomerCount ? item.CustomerCount : data.CustomerCount;
                        }
                        parcent = customercount * 100 / item.CustomerCount;
                        Brandretailer += html.Replace("[name]", item.SubCategoryName).Replace("[currentvalue]", parcent.ToString()).Replace("[color]", color);
                    }
                    expiredHtml = content.Replace("[ExecutiveRetailer]", retailer).Replace("[ExecutiveBrandRetailer]", Brandretailer);
                }
            }
            return expiredHtml;
        }


        #endregion

        [Route("GetSalesPersonCommission")]
        [HttpGet]
        public async Task<SalesPersonCommission> GetSalesPersonCommission(int peopleId, int warehouseId, int month,int year=2021)
        {
            SalesPersonCommission salesPersonCommission = new SalesPersonCommission();
            using (var context = new AuthContext())
            {

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                DataTable peopleidDt = new DataTable();
                peopleidDt.Columns.Add("IntValue");
                DataRow dr = peopleidDt.NewRow();
                dr[0] = peopleId;
                peopleidDt.Rows.Add(dr);

                var executiveIds = new SqlParameter("executiveIds", peopleidDt);
                executiveIds.SqlDbType = SqlDbType.Structured;
                executiveIds.TypeName = "dbo.IntValues";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetSalesCommission]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@Month", month));
                cmd.Parameters.Add(new SqlParameter("@Year", year));
                cmd.Parameters.Add(executiveIds);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 600;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var SalesPersonCommissionData = ((IObjectContextAdapter)context)
                                    .ObjectContext
                                    .Translate<SalesPersonCommissionData>(reader).ToList();

                if (SalesPersonCommissionData != null && SalesPersonCommissionData.Any())
                {
                    salesPersonCommission = SalesPersonCommissionData.GroupBy(x => x.Name).Select(x => new SalesPersonCommission
                    {
                        Name = x.Key,
                        CategoryCommissions = x.GroupBy(y => new { y.CategoryName, y.ShowColumnWithValueField }).Select(z =>
                               new CategoryCommission
                               {
                                   CategoryName = z.Key.CategoryName,
                                   ShowColumnWithValueField = JsonConvert.DeserializeObject<Dictionary<string, string>>(z.Key.ShowColumnWithValueField),
                                   EventCommissions = z.Select(p => new EventCommission
                                   {
                                       Id = p.Id,
                                       BookedValue = Convert.ToInt32(Math.Round(p.BookedValue,0)),
                                       EventCatName = p.EventCatName,
                                       EventName = p.EventName,
                                       IncentiveType = p.IncentiveType,
                                       IncentiveValue = p.IncentiveValue,
                                       ReqBookedValue = Convert.ToInt32(Math.Round(p.ReqBookedValue,0)),
                                       EarnValue = Convert.ToInt32(Math.Round(p.EarnValue,0)),
                                       EndDate = p.EndDate,
                                       StartDate = p.StartDate
                                   }
                                  ).ToList()
                               }
                        ).ToList()
                    }).FirstOrDefault();
                }
                else
                {
                    string Name= context.Peoples.FirstOrDefault(x => x.PeopleID == peopleId).DisplayName;
                    salesPersonCommission = new SalesPersonCommission
                    {
                        Name = Name
                    };
                }
            }

            return salesPersonCommission;
        }
        [Route("GetBrandsWiseItemList/{Warehouseid}")]
        [HttpPost]
        public IEnumerable<ItemMasterSalesDc> GetBrandsWiseItemList([FromUri] int Warehouseid, [FromBody] List<int> BrandId)
        {
            using (var db = new AuthContext())
            {
              //  List<ItemMaster> ass = new List<ItemMaster>();
                List<ItemMasterSalesDc> result = new List<ItemMasterSalesDc>();
                using (var context = new AuthContext())
                {
                    try
                    {
                        if (Warehouseid > 0)
                        {
                             List<ItemMasterSalesDc> ass = context.itemMasters.Where(t => BrandId.Contains(t.SubsubCategoryid) && t.WarehouseId == Warehouseid && t.Deleted == false && t.active==true).Select(t=>new ItemMasterSalesDc { ItemId=t.ItemId,SellingSku= t.SellingSku,itemname=t.itemname}).ToList();                            
                            foreach (var item in ass.GroupBy(x => x.SellingSku))
                            {                                      
                                result.Add(item.ToList().FirstOrDefault());
                            }
                            result.Insert(0, new ItemMasterSalesDc
                            {
                                itemname = "All Item",
                                ItemId = 0,
                                SellingSku=""
                            });
                            var results = result.OrderBy(x =>x.ItemId);
                            return results;
                        }

                        else
                        {
                            return null;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error in Item Master " + ex.Message);
                        return null;
                    }
                }
            }
        }

        [Route("GetsalesCommissionCatMaster")]
        [HttpGet]
        public List<SalesCommissionCatMaster> GetsalesCommissionCatMaster()
        {
            using (var db = new AuthContext())
            {
                List<SalesCommissionCatMaster> list = db.SalesCommissionCatMasters.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                return list;
            }
        }
        [Route("GetSalesCommissionEventMasterList")]
        [HttpGet]
        public List<SalesCommissionEventMaster> GetSalesCommissionEventMasterList(long CommissionCatMasterId)
        {
            using (var db = new AuthContext())
            {
                List<SalesCommissionEventMaster> list = db.SalesCommissionEventMasters.Where(x => x.CommissionCatMasterId == CommissionCatMasterId && x.IsActive == true && x.IsDeleted == false).ToList();
                return list;
            }
        }
        [Route("GetExecutiveList/{warehouseId}")]
        [HttpGet]
        public List<ClusterExecutive> GetExecutiveList(int warehouseId)
        {
            List<ClusterExecutive> result = null;
            using (var authContext = new AuthContext())
            {
                var idParam = new SqlParameter("WarehouseId", SqlDbType.Int);
                idParam.Value = warehouseId;
                result = authContext.Database.SqlQuery<ClusterExecutive>("exec Store_GetDistinctClusterExecutiveByWarehouseId @WarehouseId", idParam).ToList();
                return result;
            }
        }
    }

    //public class StoreSalesDc
    //{
    //    public double Storesale { get; set; }
    //    public string StoreName { get; set; }
    //    public long StoreOrderCount { get; set; }
    //}
    //internal class orderDataDC
    //{
    //    public double sale { get; set; }
    //    public long OrderCount { get; set; }
    //    public long OrderCustomerCount { get; set; }
    //    public double Storesale { get; set; }
    //    public string StoreName { get; set; }
    //    public long StoreOrderCount { get; set; }
    //}

    //internal class SaleDC
    //{
    //    public double Sale { get; set; }
    //    public string SubsubcategoryName { get; set; }

    //}
    //internal class SubCatgeoryorderDataDC
    //{
    //    public double Sale { get; set; }
    //    public string BrandName { get; set; }

    //}


    //internal class OrderSummaryDc
    //{
    //    public int PendingCount { get; set; }
    //    public double PendingAmount { get; set; }
    //    public int InProcessCount { get; set; }
    //    public double InProcessAmount { get; set; }
    //    public int CanceledCount { get; set; }
    //    public double CanceledAmount { get; set; }
    //    public int DeliveredCount { get; set; }
    //    public double DeliveredAmount { get; set; }
    //}

    //public class SalesTargetResponse
    //{
    //    public double AchivePercent { get; set; }
    //    public List<SalesTargetCustomerItem> SalesTargetCustomerItems { get; set; }
    //}

    //public class SalesTargetCustomerItem
    //{
    //    public int Achieveqty { get; set; }
    //    public int TargetQty { get; set; }

    //    public double Percent
    //    {
    //        get
    //        {

    //            return TargetQty > 0 ? Achieveqty * 100 / TargetQty : 0;
    //        }
    //    }

    //    public string ItemName { get; set; }
    //    public double price { get; set; }
    //    public int SubCategoryId { get; set; }
    //    public string SubcategoryName { get; set; }
    //}


    //public class CompanySalesTargetCustomer
    //{
    //    public string CompanyName { get; set; }
    //    public int CompanyId { get; set; }
    //    public int TargetQty { get; set; }
    //    public int Achieveqty { get; set; }
    //}

    //public class SalesTargetItemDc
    //{
    //    public string ItemName { get; set; }
    //    public string ItemNumber { get; set; }
    //}
    //public class PostSalesTargetItemDc
    //{
    //    public string ItemNumber { get; set; }
    //    public int ItemMultiMrpId { get; set; }
    //    public long StoreId { get; set; }
    //    public int BaseQty { get; set; }
    //    public int? Id { get; set; } //use for Update case

    //}

    //public class SalesTargetListItemDc
    //{
    //    public double MRP { get; set; }
    //    public string ItemName { get; set; }
    //    public string ItemNumber { get; set; }
    //    public int ItemMultiMrpId { get; set; }
    //    public long StoreId { get; set; }
    //    public int BaseQty { get; set; }
    //    public long Id { get; set; }

    //}

    //public class ExecutiveRetailer
    //{
    //    public int month { get; set; }
    //    public int year { get; set; }
    //    public int CustomerCount { get; set; }
    //    public int RemainingCustomerCount { get; set; }
    //}

    //public class ExecutiveBrandPurchaseRetailer
    //{
    //    public string SubcategoryName { get; set; }
    //    public int CustomerCount { get; set; }
    //    public int TargetCustomerCount { get; set; }
    //}
    //public class ItemMasterSalesDc 
    //{
    //    public int ItemId { get; set; }
    //    public string itemname { get; set; }
    //    public string SellingSku { get; set; }
    //}

}
