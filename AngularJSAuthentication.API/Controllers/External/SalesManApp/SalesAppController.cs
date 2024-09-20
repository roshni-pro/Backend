using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers.SalesApp;
using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.External;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model;
using LinqKit;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using static AngularJSAuthentication.API.Controllers.CustomersController;
using static AngularJSAuthentication.API.Controllers.DeliverychargeController;
using static AngularJSAuthentication.API.Controllers.SalesAppCounterController;
using MongoDB.Bson;
using AngularJSAuthentication.DataContracts.Transaction.Reports;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.API.Managers;
using AgileObjects.AgileMapper;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.DataContracts.ElasticSearch;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.DataContracts.PeopleNotification;
using System.Web;
using Dapper;
using System.Text;
using AngularJSAuthentication.DataContracts.CustomerReferralDc;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using AngularJSAuthentication.Model.CustomerShoppingCart;
using System.Data.Common;
using AngularJSAuthentication.Common.Helpers;
using Nito.AsyncEx;
using AngularJSAuthentication.API.Helper.Elastic;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/SalesApp")]
    public class SalesAppController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public int MemberShipHours = AppConstants.MemberShipHours;
        public double xPointValue = AppConstants.xPoint;
        public bool ElasticSearchEnable = AppConstants.ElasticSearchEnable;
        readonly string platformIdxName = $"skorderdata_{AppConstants.Environment}";

        #region:SalesAppCounter
        [Route("")]
        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage add(SalesAppCounterDc sale)
        {
            try
            {
                SalesAppCounter sales = new SalesAppCounter
                {
                    Date = indianTime,
                    Deleted = false,
                    lat = sale.lat,
                    Long = sale.Long,
                    SalesPersonId = sale.SalesPersonId
                };
                using (var db = new AuthContext())
                {
                    db.SalesAppCounterDB.Add(sales);
                    db.Commit();
                    SalesAppCounterDTO MUData = new SalesAppCounterDTO()
                    {
                        MUget = sales,
                        Status = true,
                        Message = " Added suscessfully."
                    };
                    //var query = @"select p.peopleId as SalesPersonId, p.Mobile, p.PeopleFirstName, p.PeopleLastName, p.Email, w.WarehouseName, w.WarehouseId from people p inner join Warehouses w on w.WarehouseId = p.WarehouseId where p.peopleId=#salesPersonID#";
                    //query = query.Replace("#salesPersonID#", sale.SalesPersonId.ToString());
                    //InitialPoint initialPoint = new InitialPoint()
                    //{
                    //    lat = sale.lat,
                    //    Long = sale.Long,
                    //    Mobile = sale.Mobile,
                    //    PeopleFirstName = sale.PeopleFirstName,
                    //    PeopleLastName = sale.PeopleLastName,
                    //    WarehouseId = sale.WarehouseId,
                    //    WarehouseName = sale.WarehouseName,
                    //    SalesPersonId = sale.SalesPersonId
                    //};
                    //var client = new SignalRMasterClient(DbConstants.URL + "signalr");
                    //// Send message to server.
                    //string message = JsonConvert.SerializeObject(initialPoint);
                    //client.SayHello(message, initialPoint.WarehouseId.ToString());
                    //client.Stop();
                    //string message = JsonConvert.SerializeObject(initialPoint);
                    //ChatFeed.SendChatMessage(message, initialPoint.WarehouseId.ToString());
                    return Request.CreateResponse(HttpStatusCode.OK, MUData);
                }
            }
            catch (Exception ex)
            {
                SalesAppCounterDTO MUData = new SalesAppCounterDTO()
                {
                    MUget = null,
                    Status = false,
                    Message = "Something Went Wrong."
                };
                logger.Error("Error in Add data salesperson " + ex.Message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, MUData);
            }
        }
        #endregion

        /// <summary>
        /// Delivery charge new API  for sales app
        /// </summary>
        /// <param name="WarehouseId"></param>
        /// <param name="PeopleId"></param>
        /// <returns></returns>
        [Route("DeliveryCharge")]
        [HttpGet]
        public async Task<DeliveryChageDC> GetWarehouseDeliveryCharge(int WarehouseId, int PeopleId)
        {
            DeliveryChageDC Commission = new DeliveryChageDC();
            if (WarehouseId > 0 && PeopleId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    var query = "Select delcharge.*,sum(agentcom.Amount) as CommissionAmt from DeliveryCharges delcharge inner join Customers cust on delcharge.WarehouseId = cust.Warehouseid inner join AgentCommissionforCities agentcom on agentcom.CustomerId = cust.CustomerId where delcharge.WarehouseId = " + WarehouseId + " and agentcom.PeopleId = " + PeopleId + " and delcharge.IsActive = 1 and delcharge.isDeleted = 0 group by delcharge.[id],delcharge.[CompanyId],delcharge.[min_Amount],delcharge.[max_Amount] ,delcharge.[del_Charge],delcharge.[WarehouseId],delcharge.[cluster_Id],delcharge.[warhouse_Name],delcharge.[cluster_Name],delcharge.[IsActive],delcharge.[isDeleted],delcharge.IsDistributor";
                    Commission = context.Database.SqlQuery<DeliveryChageDC>(query).FirstOrDefault();
                }
            }
            return Commission;
        }


        /// <summary>
        /// created by 19/01/2019
        /// get people profile
        /// </summary>
        /// <param name="PeopleId"></param>
        /// <returns></returns>
        [Route("Profile")]
        [HttpGet]
        public HttpResponseMessage GetProfile(int PeopleId)
        {
            Peopleresponse res;
            People person = new People();
            if (PeopleId > 0)
            {
                using (var db = new AuthContext())
                {
                    person = db.Peoples.Where(u => u.PeopleID == PeopleId).SingleOrDefault();
                    if (person != null)
                    {

                        if (person.IsLocation == null)
                        {
                            person.IsLocation = false;
                        }
                        if (person.IsRecording == null)
                        {
                            person.IsRecording = false;

                        }
                        if (person.LocationTimer == null)
                        {
                            person.LocationTimer = 0;
                        }
                        string query = "select distinct r.Name as Role from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where PeopleID='" + PeopleId + "'and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                        var role = db.Database.SqlQuery<string>(query).ToList();
                        var IsRole = role.Any(x => x.Contains("Hub sales lead"));
                        if (IsRole)
                        {
                            person.Role = "Hub sales lead";
                        }
                        else if (role.Any(x => x.Contains("Digital sales executive")))
                        {
                            person.Role = "Digital sales executive";
                        }
                        else if (role.Any(x => x.Contains("Telecaller")))
                        {
                            person.Role = "Telecaller";
                        }
                        else
                        {
                            person.Role = "";
                        }

                        var data = db.LocationResumeDetails.Where(z => z.PeopleId == PeopleId).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                        if (data != null)
                        {

                            person.Status = data.Status;
                        }

                        var list =

                        res = new Peopleresponse()
                        {
                            people = person,
                            Status = true,
                            message = "Success."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        res = new Peopleresponse()
                        {
                            people = person,
                            Status = false,
                            message = "People not exist."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
            }
            res = new Peopleresponse()
            {
                people = person,
                Status = false,
                message = "Something went wrong."
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }

        [Route("UpdateExectiveStartAddress")]
        [HttpGet]
        public HttpResponseMessage UpdateExectiveStartAddress(int peopleId, double lat, double lng)
        {
            if (peopleId > 0)
            {
                using (var db = new AuthContext())
                {
                    var person = db.Peoples.Where(u => u.PeopleID == peopleId).SingleOrDefault();
                    if (person != null)
                    {
                        person.StartLat = lat;
                        person.StartLng = lng;
                        person.UpdatedDate = DateTime.Now;
                        db.Entry(person).State = EntityState.Modified;
                        db.Commit();
                        var res = new Peopleresponse()
                        {
                            Status = true,
                            message = "Success."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        var res = new Peopleresponse()
                        {
                            Status = false,
                            message = "People not exist."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
            }
            else
            {
                var res = new Peopleresponse()
                {
                    Status = false,
                    message = "People not exist."
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }


        #region Global Search for sales app
        /// <summary>
        /// Created Raj by 25/02/2020
        /// Global customer Serach(Skcode/ShopName/Mobile)  
        /// </summary>
        /// <param name="WarehouseId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GlobalSearch")]
        [AllowAnonymous]
        public HttpResponseMessage GlobalsearchV1(int PeopleId, int WarehouseId, string Globalkey)
        {
            using (AuthContext db = new AuthContext())
            {

                GlobalcustomerDetail obj = new GlobalcustomerDetail();

                var customer = new List<SalespDTO>();
                if (!string.IsNullOrEmpty(Globalkey) && Globalkey.Length > 2)
                {

                    var Warehouseid = new SqlParameter
                    {
                        ParameterName = "WarehouseId",
                        Value = WarehouseId,

                    };
                    var ParamPeopleId = new SqlParameter
                    {
                        ParameterName = "PeopleId",
                        Value = PeopleId,

                    };
                    var GlobalKey = new SqlParameter
                    {
                        ParameterName = "Globalkey",
                        Value = Globalkey,

                    };
                    customer = db.Database.SqlQuery<SalespDTO>("CustomerGlobalSearch @WarehouseId,@PeopleId,@Globalkey", Warehouseid, ParamPeopleId, GlobalKey).ToList();
                }
                if (customer.Count() > 0)
                {
                    obj = new GlobalcustomerDetail()
                    {
                        customers = customer,
                        Status = true,
                        Message = "Customer Found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
                else
                {
                    obj = new GlobalcustomerDetail()
                    {
                        customers = customer,
                        Status = false,
                        Message = "No Customer found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }

            }
        }
        #endregion

        [HttpGet]
        [Route("Search")]
        public HttpResponseMessage Search(int PeopleId, string key)
        {
            using (AuthContext db = new AuthContext())
            {
                var customer = db.Customers.Where(c => (c.Skcode.Contains(key) || c.ShopName.Contains(key) || c.Mobile.Contains(key)) && c.Deleted == false).ToList();
                var warehouseIds = customer.Select(x => x.Warehouseid).Distinct().ToList();
                var warehouses = db.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId)).ToList();
                var customerids = customer.Select(x => x.CustomerId).ToList();
                var custDocs = db.CustomerDocs.Where(x => customerids.Contains(x.CustomerId) && x.IsActive).ToList();
                var gstDocttypeid = db.CustomerDocTypeMasters.FirstOrDefault(x => x.IsActive && x.DocType == "GST")?.Id;
                var clusterIds = customer.Select(x => x.ClusterId).ToList();
                var clusterExecutive = db.ClusterStoreExecutives.Where(x => clusterIds.Contains(x.ClusterId) && x.IsActive && !x.IsDeleted.Value).Select(x => new { x.ClusterId, x.ExecutiveId }).ToList();

                customer.ForEach(x =>
                {
                    x.ExecutiveId = clusterExecutive.Any(y => y.ClusterId == x.ClusterId && y.ExecutiveId == PeopleId) ? clusterExecutive.FirstOrDefault(y => y.ClusterId == x.ClusterId && y.ExecutiveId == PeopleId).ExecutiveId : 0;
                    x.WarehouseName = x.Warehouseid.HasValue ? warehouses.FirstOrDefault(z => z.WarehouseId == x.Warehouseid.Value)?.WarehouseName : "";
                    if (gstDocttypeid.HasValue && custDocs.Any(y => y.CustomerDocTypeMasterId != gstDocttypeid.Value))
                    {
                        x.CustomerDocTypeMasterId = custDocs.FirstOrDefault(y => y.CustomerDocTypeMasterId != gstDocttypeid.Value).CustomerDocTypeMasterId;
                    }
                    if (x.RefNo == "NA" || string.IsNullOrEmpty(x.RefNo))
                        x.RefNo = "";
                });

                return Request.CreateResponse(HttpStatusCode.OK, customer);
            }
        }

        [Route("GetCustomerDocType")]
        [HttpGet]
        public async Task<dynamic> GetCustomerDocType(int warehouseId, int PeopleId)
        {
            using (AuthContext db = new AuthContext())
            {
                var CustomerDocTypes = await db.CustomerDocTypeMasters.Where(x => x.IsActive).ToListAsync();
                return Request.CreateResponse(HttpStatusCode.OK, CustomerDocTypes);
            }

        }


        [Route("GetMinOrderAmount")]
        [HttpGet]
        public async Task<dynamic> GetRetailerMinOrderAmountSalesAPP(int warehouseId, int PeopleId, int customerId)
        {
            int minOrderValue = Convert.ToInt32(ConfigurationManager.AppSettings["MinOrderValue"]);
            int NoOfLineItemSales = 0;
            List<StoreMinOrder> AllStoreMinOrder = new List<StoreMinOrder>();


            List<long> StoreIds = new List<long>();
            using (var context = new AuthContext())
            {
                NoOfLineItemSales = context.CompanyDetailsDB.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x =>
                    x.NoOfLineItemSales
                ).FirstOrDefault();

                var warehouse = await context.Warehouses.FirstOrDefaultAsync(x => x.WarehouseId == warehouseId);

                MongoDbHelper<CustomerShoppingCart> mongoDbHelperCart = new MongoDbHelper<CustomerShoppingCart>();
                var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => x.CustomerId == customerId && x.WarehouseId == warehouseId && x.PeopleId == PeopleId && !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                if (PeopleId > 0)
                {
                    cartPredicate = cartPredicate.And(x => x.PeopleId == PeopleId);
                }
                else
                {
                    cartPredicate = cartPredicate.And(x => x.PeopleId == 0);
                }
                var customerShoppingCart = mongoDbHelperCart.Select(cartPredicate, x => x.OrderByDescending(y => y.ModifiedDate), null, null, collectionName: "CustomerShoppingCart").FirstOrDefault();



                var itemids = customerShoppingCart != null ? customerShoppingCart.ShoppingCartItems.Where(x => x.IsActive && x.qty > 0).Select(x => x.ItemId).Distinct().ToList() : new List<int>();
                if (itemids != null && itemids.Any())
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();

                    var ItemIdDt = new DataTable();
                    ItemIdDt.Columns.Add("IntValue");
                    foreach (var item in itemids)
                    {
                        var dr = ItemIdDt.NewRow();
                        dr["IntValue"] = item;
                        ItemIdDt.Rows.Add(dr);
                    }
                    var param = new SqlParameter("Item", ItemIdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetItemStoreId]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(param);
                    cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                    // Run the sproc
                    var reader1 = cmd.ExecuteReader();
                    StoreIds = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<long>(reader1).ToList();
                }

                if (warehouse != null && warehouse.Cityid > 0)
                {
                    MongoDbHelper<DataContracts.Mongo.RetailerMinOrder> mongoDbHelper = new MongoDbHelper<DataContracts.Mongo.RetailerMinOrder>();
                    var PredicateCart = PredicateBuilder.New<DataContracts.Mongo.RetailerMinOrder>(x => x.CityId == warehouse.Cityid && x.WarehouseId == warehouse.WarehouseId);
                    var retailerMinOrder = mongoDbHelper.Select(PredicateCart, null, null, null, collectionName: "RetailerMinOrder").FirstOrDefault();
                    if (retailerMinOrder != null)
                    {
                        minOrderValue = retailerMinOrder.MinOrderValue;
                    }
                }

                if (StoreIds != null && StoreIds.Any())
                {
                    MongoDbHelper<StoreMinOrder> mHelperStore = new MongoDbHelper<StoreMinOrder>();
                    var storeMinOrder = mHelperStore.Select(x => x.StoreId > 0 && (x.CityId == 0 || x.CityId == warehouse.Cityid) && x.WarehouseId == warehouseId && StoreIds.Contains(x.StoreId)).ToList();

                    if (storeMinOrder != null)
                    {
                        AllStoreMinOrder = storeMinOrder.GroupBy(x => new { x.CityId, x.StoreId }).Select(x => new StoreMinOrder { CityId = x.Key.CityId, StoreId = x.Key.StoreId, WarehouseId = x.FirstOrDefault().WarehouseId, MinOrderValue = x.FirstOrDefault().MinOrderValue, MinLineItem = x.FirstOrDefault().MinLineItem }).ToList();
                        var MinOrder = storeMinOrder.OrderBy(x => x.MinOrderValue).Select(x => new { MinOrderValue = x.MinOrderValue, MinLineItem = x.MinLineItem }).FirstOrDefault();
                        minOrderValue = 0;
                        NoOfLineItemSales = 0;
                        if (MinOrder != null)  // new change
                        {
                            minOrderValue = MinOrder.MinOrderValue;
                            NoOfLineItemSales = MinOrder.MinLineItem;
                        }
                    }
                }

                return new { minOrderValue = minOrderValue, MinLineItem = NoOfLineItemSales, StoreMinOrder = AllStoreMinOrder };
            }



        }



        // old api start
        //[Route("GetMinOrderAmount")]
        //[HttpGet]
        //public async Task<dynamic> GetRetailerMinOrderAmountSalesAPP(int warehouseId, int PeopleId)
        //{
        //    int minOrderValue = Convert.ToInt32(ConfigurationManager.AppSettings["MinOrderValue"]);
        //    int NoOfLineItemSales = 0;
        //    List<long> StoreIDs = new List<long>();
        //    List<StoreMinOrder> storeMinOrder = new List<StoreMinOrder>();
        //    using (var context = new AuthContext())
        //    {
        //        //NoOfLineItemSales = context.CompanyDetailsDB.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x =>
        //        //    x.NoOfLineItemSales
        //        //).FirstOrDefault();  // comment for takinf
        //        //var StoreIDs = (from k in context.ClusterStoreExecutives
        //        //                join j in context.StoreDB on k.StoreId equals j.Id
        //        //                where k.ExecutiveId == PeopleId && k.IsActive == true && k.IsDeleted == false && j.IsActive == true && j.IsDeleted == false
        //        //                select  k.StoreId).ToList();
        //        var PID = new SqlParameter("@PeopleID", PeopleId);
        //        StoreIDs = context.Database.SqlQuery<long>("EXEC GetUniverseWiseExecutive @PeopleID", PID).ToList();
        //        var warehouse = await context.Warehouses.FirstOrDefaultAsync(x => x.WarehouseId == warehouseId);
        //        if (warehouse != null && warehouse.Cityid > 0)
        //        {

        //            //context.ClusterStoreExecutives.Where(x => x.ExecutiveId == PeopleId && x.IsActive == true && x.IsDeleted == false).ToList();
        //            MongoDbHelper<DataContracts.Mongo.RetailerMinOrder> mongoDbHelper = new MongoDbHelper<DataContracts.Mongo.RetailerMinOrder>();
        //            var cartPredicate = PredicateBuilder.New<DataContracts.Mongo.RetailerMinOrder>(x => x.CityId == warehouse.Cityid && x.WarehouseId == warehouseId);
        //            var retailerMinOrder = mongoDbHelper.Select(cartPredicate, null, null, null, collectionName: "RetailerMinOrder").FirstOrDefault();
        //            if (retailerMinOrder != null)
        //            {
        //                minOrderValue = retailerMinOrder.MinOrderValue;
        //            }
        //            else
        //            {
        //                DataContracts.Mongo.RetailerMinOrder newRetailerMinOrder = new DataContracts.Mongo.RetailerMinOrder
        //                {
        //                    CityId = warehouse.Cityid,
        //                    WarehouseId = warehouse.WarehouseId,
        //                    MinOrderValue = minOrderValue,

        //                };
        //                var result = mongoDbHelper.Insert(newRetailerMinOrder);
        //            }
        //        }
        //        MongoDbHelper<StoreMinOrder> mHelperStore = new MongoDbHelper<StoreMinOrder>();
        //        //storeMinOrder = mHelperStore.Select(x => x.StoreId > 0 && x.WarehouseId == warehouseId && (x.CityId == 0 || x.CityId == warehouse.Cityid)).ToList();
        //        storeMinOrder = mHelperStore.Select(x => x.StoreId > 0 && x.WarehouseId == warehouseId && (x.CityId == 0 || x.CityId == warehouse.Cityid) && StoreIDs.Contains(x.StoreId)).ToList();
        //        //int minValue = 0;
        //        //minValue = storeMinOrder[0].MinOrderValue;
        //        //foreach (var x in storeMinOrder)
        //        //{
        //        //    if (minValue > x.MinOrderValue)
        //        //    {
        //        //        minValue = x.MinOrderValue;
        //        //    }
        //        //}
        //        storeMinOrder = storeMinOrder.GroupBy(x => new { x.CityId, x.StoreId }).Select(x => new StoreMinOrder { CityId = x.Key.CityId, WarehouseId = x.FirstOrDefault().WarehouseId, StoreId = x.Key.StoreId, MinOrderValue = x.FirstOrDefault().MinOrderValue, MinLineItem = x.FirstOrDefault().MinLineItem }).ToList(); //,MinLineItem=x.FirstOrDefault().MinLineItem
        //                                                                                                                                                                                                                                                                                                  //  storeMinOrder = storeMinOrder.GroupBy(x => new { x.CityId, x.StoreId ,x.WarehouseId}).Select(x => new StoreMinOrder { CityId = x.Key.CityId, StoreId = x.Key.StoreId,WarehouseId=x.Key.WarehouseId,  MinLineItem = x.FirstOrDefault().MinLineItem }).ToList();
        ////    }

        //    return new { minOrderValue = minOrderValue, StoreMinOrder = storeMinOrder };
        //    //NoOfLineItem = storeMinOrder.Select(x=>x.MinLineItem).FirstOrDefault()
        //}


        #region Beats
        /// <summary>
        /// Created By Raj 
        /// Date:25/02/2020
        /// </summary>
        /// <param name="id"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        [Route("customer/V3")]
        [AllowAnonymous]
        public HttpResponseMessage GetBeatDataV3(int id, string day, int skip, int take)
        {
            using (var db = new AuthContext())
            {
                GlobalcustomerDetail obj = new GlobalcustomerDetail();

                MongoDbHelper<ExecutiveBeats> mongoDbHelper = new MongoDbHelper<ExecutiveBeats>();
                var today = DateTime.Now.Date;
                var todayBeats = mongoDbHelper.Select(x => x.PeopleId == id && x.AssignmentDate == today);


                if (todayBeats != null && todayBeats.Any())
                {
                    if (!string.IsNullOrEmpty(day) && day != "undefined")
                    {
                        if (day != DateTime.Now.DayOfWeek.ToString())
                        {
                            var executiveBeats = db.Database.SqlQuery<SalespDTO>(string.Format("exec GetExcutiveBeatCustomerexceptToday {0},{1},{2}", id, skip, take)).ToList();
                            var OtherDayPlannedCustomers = executiveBeats.Select(i => new PlannedRoute
                            {
                                CustomerId = i.CustomerId,
                                CompanyId = i.CompanyId,
                                Active = i.Active,
                                CustomerVerify = i.CustomerVerify,
                                City = i.City,
                                WarehouseId = i.WarehouseId,
                                WarehouseName = i.WarehouseName,
                                lat = i.lat,
                                lg = i.lg,
                                ExecutiveId = i.ExecutiveId,
                                BeatNumber = i.BeatNumber,
                                Day = i.Day,
                                Skcode = i.Skcode,
                                Mobile = i.Mobile,
                                ShopName = i.ShopName,
                                BillingAddress = i.BillingAddress,
                                ShippingAddress = i.ShippingAddress,
                                Name = i.Name,
                                Emailid = i.Emailid,
                                RefNo = i.RefNo,
                                Password = i.Password,
                                UploadRegistration = i.UploadRegistration,
                                ResidenceAddressProof = i.ResidenceAddressProof,
                                DOB = i.DOB,
                                MaxOrderCount = i.MaxOrderCount,
                                IsKPP = i.IsKPP,
                                ClusterId = i.ClusterId,
                                ClusterName = i.ClusterName,
                                CustomerType = i.CustomerType
                            }).ToList();
                            todayBeats.ForEach(x =>
                            {
                                x.PlannedRoutes.AddRange(OtherDayPlannedCustomers);
                            });
                        }

                        todayBeats.ForEach(x =>
                        {
                            x.PlannedRoutes = x.PlannedRoutes.Where(s => !string.IsNullOrEmpty(s.Day) && s.Day == day).ToList();
                        });
                    }
                    else
                    {
                        var executiveBeats = db.Database.SqlQuery<SalespDTO>(string.Format("exec GetExcutiveBeatCustomerexceptToday {0},{1},{2}", id, skip, take)).ToList();
                        var OtherDayPlannedCustomers = executiveBeats.Select(i => new PlannedRoute
                        {
                            CustomerId = i.CustomerId,
                            CompanyId = i.CompanyId,
                            Active = i.Active,
                            CustomerVerify = i.CustomerVerify,
                            City = i.City,
                            WarehouseId = i.WarehouseId,
                            WarehouseName = i.WarehouseName,
                            lat = i.lat,
                            lg = i.lg,
                            ExecutiveId = i.ExecutiveId,
                            BeatNumber = i.BeatNumber,
                            Day = i.Day,
                            Skcode = i.Skcode,
                            Mobile = i.Mobile,
                            ShopName = i.ShopName,
                            BillingAddress = i.BillingAddress,
                            ShippingAddress = i.ShippingAddress,
                            Name = i.Name,
                            Emailid = i.Emailid,
                            RefNo = i.RefNo,
                            Password = i.Password,
                            UploadRegistration = i.UploadRegistration,
                            ResidenceAddressProof = i.ResidenceAddressProof,
                            DOB = i.DOB,
                            MaxOrderCount = i.MaxOrderCount,
                            IsKPP = i.IsKPP,
                            ClusterId = i.ClusterId,
                            ClusterName = i.ClusterName,
                            CustomerType = i.CustomerType
                        }).ToList();
                        todayBeats.ForEach(x =>
                        {
                            x.PlannedRoutes.AddRange(OtherDayPlannedCustomers);
                        });
                    }

                }


                if (todayBeats != null && todayBeats.Any())
                {
                    //var existingActualRoute = todayBeats != null
                    //                    ? todayBeats.Where(s => s.ActualRoutes != null && s.ActualRoutes.Any()).SelectMany(z => z.ActualRoutes)
                    //                    : null;

                    //if (existingActualRoute != null && existingActualRoute.Any())
                    //    todayBeats.ForEach(s => s.PlannedRoutes.RemoveAll(x => existingActualRoute.Select(z => z.CustomerId).Contains(x.CustomerId)));

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        customers = todayBeats,
                        Status = true,
                        Message = "Customer Found"
                    });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        customers = todayBeats,
                        Status = false,
                        Message = "No Customer found"
                    });


            }
        }

        [Route("GetSalesDashboardData")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<SalesDeshboardData> GetSalesDashboardData(int peopleId)
        {
            SalesDeshboardData salesDeshboardData = new SalesDeshboardData();
            string completeTargetColor = "#FF6161", IncompleteTargetColor = "#4FFF6C";
            using (var context = new AuthContext())
            {
                salesDeshboardData.ShowTarget = Convert.ToBoolean(ConfigurationManager.AppSettings["ShowSalesAppDesbaordTarget"]);
                MongoDbHelper<ExecutiveBeats> mongoDbHelper = new MongoDbHelper<ExecutiveBeats>();
                MongoDbHelper<ExecuteBeatTarget> mongoDbTargetHelper = new MongoDbHelper<ExecuteBeatTarget>();
                var today = DateTime.Now.Date;
                var people = (await context.Peoples.FirstOrDefaultAsync(x => x.PeopleID == peopleId));
                int cityId = people.Cityid ?? 0;
                int? clusterId = (await context.ClusterStoreExecutives.Where(x => x.ExecutiveId == peopleId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.ClusterId).FirstOrDefaultAsync());
                var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                var MonthBeat = await mongoDbHelper.SelectAsync(x => x.PeopleId == peopleId && x.AssignmentDate >= firstDayOfMonth && x.AssignmentDate <= today);
                var todayBeat = MonthBeat.FirstOrDefault(x => x.AssignmentDate == today);
                var MonthCustomers = await context.Customers.Where(x => x.ExecutiveId == peopleId && x.CreatedDate >= firstDayOfMonth).Select(x => new { x.CustomerId, x.CreatedDate }).ToListAsync();
                if (todayBeat != null && todayBeat.PlannedRoutes != null && todayBeat.PlannedRoutes.Any())
                {
                    MongoDbHelper<NextDayBeatPlan> mongoDbCustomBeatPlanHelper = new MongoDbHelper<NextDayBeatPlan>();
                    var CustomBeatPlans = (await mongoDbCustomBeatPlanHelper.SelectAsync(x => x.CreatedDate <= DateTime.Now && x.CreatedDate >= today && x.ExecutiveId == peopleId)).ToList();

                    var beatTargets = clusterId.HasValue ? mongoDbTargetHelper.Select(x => x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now && x.ClusterId == clusterId).ToList() : null;
                    var beatTarget = beatTargets.FirstOrDefault();

                    #region BeatCustomerOrder
                    var beatCustomerids = MonthBeat.SelectMany(y => y.PlannedRoutes.Select(x => x.CustomerId)).ToList();
                    if (MonthBeat.Select(x => x.ActualRoutes).Any())
                    {
                        if (beatCustomerids == null)
                            beatCustomerids = new List<int>();

                        beatCustomerids.AddRange(MonthBeat.Select(x => x.ActualRoutes).Where(x => x != null).SelectMany(x => x.Select(y => y.CustomerId)).ToList());
                    }
                    beatCustomerids = beatCustomerids.Distinct().ToList();

                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();
                    var customerIdDt = new DataTable();
                    customerIdDt.Columns.Add("IntValue");
                    foreach (var item in beatCustomerids)
                    {
                        var dr = customerIdDt.NewRow();
                        dr["IntValue"] = item;
                        customerIdDt.Rows.Add(dr);
                    }
                    var param = new SqlParameter("customerId", customerIdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.Parameters.Add(new SqlParameter("@ExectiveId", peopleId));
                    cmd.CommandText = "[dbo].[GetBeatCustomerOrder]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandTimeout = 600;
                    cmd.Parameters.Add(param);

                    // Run the sproc
                    var reader = cmd.ExecuteReader();
                    var beatCustomerOrders = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<BeatCustomerOrder>(reader).ToList();
                    #endregion


                    var BeatCustomers = todayBeat.PlannedRoutes.Select(x => new BeatCustomer
                    {
                        CustomerId = x.CustomerId,
                        Active = x.Active,
                        TravalStart = x.TravalStart,
                        BillingAddress = x.BillingAddress,
                        IsVisited = x.IsVisited,
                        BeatNumber = x.BeatNumber.HasValue ? x.BeatNumber.Value : todayBeat.PlannedRoutes.Count + 1,
                        lat = x.lat,
                        AreaName = x.AreaName,
                        lg = x.lg,
                        Mobile = x.Mobile,
                        Name = x.Name,
                        IsKPP = x.IsKPP,
                        ShippingAddress = x.ShippingAddress,
                        ShopName = x.ShopName,
                        Skcode = x.Skcode,
                        WarehouseId = x.WarehouseId,
                        WarehouseName = x.WarehouseName,
                        MaxOrderCount = x.MaxOrderCount,
                        WtAvgAmount = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId) ? beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId).FirstOrDefault().weightAvgAmount : 0,
                        WtAvgOrder = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId) ? beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId).FirstOrDefault().weightAvgOrder : 0,
                        AvgLineItem = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today) ? Convert.ToDecimal(beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today).Sum(z => z.lineItem) / beatCustomerOrders.Count(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today)) : 0,
                        TotalLineItem = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today) ? Convert.ToDecimal(beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today).Sum(z => z.lineItem)) : 0,
                        TotalOrder = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today) ? beatCustomerOrders.Count(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today) : 0,
                        TotalOrderAmount = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today) ? Convert.ToInt32(Math.Round(beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today).Sum(z => z.GrossAmount), 0)) : 0,
                        Comment = todayBeat.ActualRoutes != null && todayBeat.ActualRoutes.Any(y => y.CustomerId == x.CustomerId) ? todayBeat.ActualRoutes.FirstOrDefault(y => y.CustomerId == x.CustomerId).Comment : "",
                        Status = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today) ?
                                   "Ordered" : (!x.IsVisited ? "Not Visited" : (todayBeat.ActualRoutes != null && todayBeat.ActualRoutes.Any(y => y.CustomerId == x.CustomerId && !string.IsNullOrEmpty(y.ShopCloseImage))
                                   ? "Shop Closed - Skip" : ((CustomBeatPlans != null && CustomBeatPlans.Any(y => y.CustomerId == x.CustomerId)) ? "Reschedule" : (todayBeat.ActualRoutes != null && todayBeat.ActualRoutes.Any(y => y.CustomerId == x.CustomerId && !string.IsNullOrEmpty(y.Comment) && y.Comment.Contains("Not Visited")) ? "Not Visited" : "Visited"))))
                    }).ToList();


                    salesDeshboardData.MyBeat = new MyBeat
                    {
                        AreaName = todayBeat != null && todayBeat.PlannedRoutes != null ? todayBeat.PlannedRoutes.FirstOrDefault(x => !string.IsNullOrEmpty(x.ClusterName)).ClusterName + "-" + todayBeat.AssignmentDate.DayOfWeek.ToString() : "",
                        BeatCustomers = BeatCustomers,
                        TodayVisit = BeatCustomers != null ? BeatCustomers.Count : 0,
                        Visited = BeatCustomers != null && BeatCustomers.Any(x => x.IsVisited) ? BeatCustomers.Count(x => x.IsVisited) : 0,
                        AvgLineItem = BeatCustomers != null && BeatCustomers.Sum(x => x.TotalOrder) > 0 ? BeatCustomers.Sum(z => z.TotalLineItem) / BeatCustomers.Sum(x => x.TotalOrder) : 0,
                        BeatAmount = BeatCustomers != null ? BeatCustomers.Sum(z => z.TotalOrderAmount) : 0,
                        BeatOrder = BeatCustomers != null ? BeatCustomers.Sum(y => y.TotalOrder) : 0,
                        Conversion = BeatCustomers != null ? BeatCustomers.Where(y => y.TotalOrder > 0).Select(z => z.CustomerId).Distinct().Count() : 0,
                        AvgLineItemColor = "#ffffff",
                        BeatAmountColor = "#ffffff",
                        BeatOrderColor = "#ffffff",
                        ConversionColor = "#ffffff",
                        VisitedColor = "#ffffff",
                    };
                    if (beatTarget != null)
                    {
                        salesDeshboardData.MyBeat.VisitedColor = salesDeshboardData.MyBeat.Visited <= (salesDeshboardData.MyBeat.TodayVisit * beatTarget.VisitedPercent / 100) ? completeTargetColor : IncompleteTargetColor;
                        salesDeshboardData.MyBeat.ConversionColor = salesDeshboardData.MyBeat.Conversion <= (salesDeshboardData.MyBeat.TodayVisit * beatTarget.ConversionPercent / 100) ? completeTargetColor : IncompleteTargetColor;
                        salesDeshboardData.MyBeat.BeatOrderColor = salesDeshboardData.MyBeat.BeatOrder <= (salesDeshboardData.MyBeat.TodayVisit * beatTarget.OrderPercent / 100) ? completeTargetColor : IncompleteTargetColor;
                        salesDeshboardData.MyBeat.BeatAmountColor = salesDeshboardData.MyBeat.BeatAmount <= (salesDeshboardData.MyBeat.TodayVisit * beatTarget.OrderPercent / 100) * beatTarget.AvgOrderAmount ? completeTargetColor : IncompleteTargetColor;
                        salesDeshboardData.MyBeat.AvgLineItemColor = salesDeshboardData.MyBeat.AvgLineItem <= beatTarget.AvgLineItem ? completeTargetColor : IncompleteTargetColor;


                        salesDeshboardData.BeatTarget = new BeatTarget
                        {
                            AvgLineItem = beatTarget.AvgLineItem,
                            Conversion = Convert.ToInt32(salesDeshboardData.MyBeat.TodayVisit * beatTarget.ConversionPercent / 100),
                            CustomerCount = Convert.ToInt32(salesDeshboardData.MyBeat.TodayVisit * beatTarget.CustomerPercent / 100),
                            OrderCount = Convert.ToInt32((salesDeshboardData.MyBeat.TodayVisit * beatTarget.OrderPercent / 100)),
                            OrderAmount = Convert.ToInt32((salesDeshboardData.MyBeat.TodayVisit * beatTarget.OrderPercent / 100) * beatTarget.AvgOrderAmount),
                            Visited = Convert.ToInt32(salesDeshboardData.MyBeat.TodayVisit * beatTarget.VisitedPercent / 100),
                        };
                    }
                    else
                    {
                        salesDeshboardData.BeatTarget = new BeatTarget
                        {
                            AvgLineItem = 0,
                            Conversion = 0,
                            CustomerCount = 0,
                            OrderCount = 0,
                            OrderAmount = 0,
                            Visited = 0,
                        };
                    }


                    salesDeshboardData.SalesMetricsDaily = new BeatSale
                    {
                        CustomerCount = beatCustomerOrders != null && beatCustomerOrders.Any(x => x.CreatedDate.Date == today) ? beatCustomerOrders.Where(x => x.CreatedDate.Date == today).Select(x => x.CustomerId).Distinct().Count() : 0,
                        TotalOrders = beatCustomerOrders != null && beatCustomerOrders.Any(x => x.CreatedDate.Date == today) ? beatCustomerOrders.Where(x => x.CreatedDate.Date == today).Count() : 0,
                        AvgLineItem = beatCustomerOrders != null && beatCustomerOrders.Any(x => x.CreatedDate.Date == today) ? Convert.ToDecimal(beatCustomerOrders.Where(x => x.CreatedDate.Date == today).Sum(x => x.lineItem)) / beatCustomerOrders.Where(x => x.CreatedDate.Date == today).Count() : 0,
                        TotalAmount = beatCustomerOrders != null && beatCustomerOrders.Any(x => x.CreatedDate.Date == today) ? Convert.ToInt32(Math.Round(beatCustomerOrders.Where(x => x.CreatedDate.Date == today).Sum(x => x.GrossAmount), 0)) : 0,
                        AvgLineItemColor = "#ffffff",
                        TotalAmountColor = "#ffffff",
                        TotalOrdersColor = "#ffffff",
                        CustomerCountColor = "#ffffff",
                    };
                    if (beatTarget != null)
                    {
                        salesDeshboardData.SalesMetricsDaily.AvgLineItemColor = salesDeshboardData.SalesMetricsDaily.AvgLineItem < beatTarget.AvgLineItem ? completeTargetColor : IncompleteTargetColor;
                        salesDeshboardData.SalesMetricsDaily.TotalOrdersColor = salesDeshboardData.SalesMetricsDaily.TotalOrders < (salesDeshboardData.MyBeat.TodayVisit * beatTarget.OrderPercent / 100) ? completeTargetColor : IncompleteTargetColor;
                        salesDeshboardData.SalesMetricsDaily.CustomerCountColor = salesDeshboardData.SalesMetricsDaily.CustomerCount < (salesDeshboardData.MyBeat.TodayVisit * beatTarget.CustomerPercent / 100) ? completeTargetColor : IncompleteTargetColor;
                        salesDeshboardData.SalesMetricsDaily.TotalAmountColor = salesDeshboardData.SalesMetricsDaily.TotalOrders < (salesDeshboardData.MyBeat.TodayVisit * beatTarget.OrderPercent / 100) * beatTarget.AvgOrderAmount ? completeTargetColor : IncompleteTargetColor;

                        salesDeshboardData.SalesTarget = new SalesTarget
                        {
                            AvgLineItem = beatTarget.AvgLineItem,
                            CustomerCount = Convert.ToInt32(salesDeshboardData.MyBeat.TodayVisit * beatTarget.CustomerPercent / 100),
                            OrderCount = Convert.ToInt32((salesDeshboardData.MyBeat.TodayVisit * beatTarget.OrderPercent / 100)),
                            OrderAmount = Convert.ToInt32((salesDeshboardData.MyBeat.TodayVisit * beatTarget.OrderPercent / 100) * beatTarget.AvgOrderAmount),
                        };
                    }
                    else
                    {
                        salesDeshboardData.SalesTarget = new SalesTarget
                        {
                            AvgLineItem = 0,
                            CustomerCount = 0,
                            OrderCount = 0,
                            OrderAmount = 0,
                        };
                    }



                    var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
                    var weekBeats = MonthBeat.Where(x => x.AssignmentDate >= thisWeekStart && x.AssignmentDate <= today).SelectMany(p => p.PlannedRoutes.Select(x => new BeatCustomer
                    {
                        CustomerId = x.CustomerId,
                        Active = x.Active,
                        BillingAddress = x.BillingAddress,
                        IsVisited = x.IsVisited,
                        BeatNumber = x.BeatNumber.HasValue ? x.BeatNumber.Value : todayBeat.PlannedRoutes.Count + 1,
                        lat = x.lat,
                        lg = x.lg,
                        Mobile = x.Mobile,
                        Name = x.Name,
                        IsKPP = x.IsKPP,
                        ShippingAddress = x.ShippingAddress,
                        ShopName = x.ShopName,
                        Skcode = x.Skcode,
                        WarehouseId = x.WarehouseId,
                        WarehouseName = x.WarehouseName,
                        AreaName = x.AreaName,
                        MaxOrderCount = x.MaxOrderCount,
                        WtAvgAmount = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId) ? beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId).FirstOrDefault().weightAvgAmount : 0,
                        WtAvgOrder = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId) ? beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId).FirstOrDefault().weightAvgOrder : 0,
                        AvgLineItem = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? Convert.ToDecimal(beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today).Average(z => z.lineItem)) : 0,
                        TotalLineItem = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? Convert.ToDecimal(beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today).Sum(z => z.lineItem)) : 0,
                        TotalOrder = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? beatCustomerOrders.Count(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) : 0,
                        TotalOrderAmount = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? Convert.ToInt32(Math.Round(beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today).Sum(z => z.GrossAmount), 0)) : 0,
                        //Comment = p.ActualRoutes != null && todayBeat.ActualRoutes.Any(y => y.CustomerId == x.CustomerId) ? todayBeat.ActualRoutes.FirstOrDefault(y => y.CustomerId == x.CustomerId).Comment : "",
                        Status = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ?
                                    //"Ordered" : (!x.IsVisited ? "Not Visited" : (p.ActualRoutes != null && p.ActualRoutes.Any(y => y.CustomerId == x.CustomerId && !string.IsNullOrEmpty(y.ShopCloseImage))
                                    //? "Shop Closed - Skip" : "Visited"))
                                    "Ordered" : (!x.IsVisited ? "Not Visited" : (todayBeat.ActualRoutes != null && todayBeat.ActualRoutes.Any(y => y.CustomerId == x.CustomerId && !string.IsNullOrEmpty(y.ShopCloseImage))
                                   ? "Shop Closed - Skip" : ((CustomBeatPlans != null && CustomBeatPlans.Any(y => y.CustomerId == x.CustomerId)) ? "Reschedule" : (todayBeat.ActualRoutes != null && todayBeat.ActualRoutes.Any(y => y.CustomerId == x.CustomerId && !string.IsNullOrEmpty(y.Comment) && y.Comment.Contains("Not Visited")) ? "Not Visited" : "Visited"))))
                    }).ToList()).ToList();


                    salesDeshboardData.SaleMetricsWeekly = new BeatSaleWeekly
                    {
                        BeatCustomers = weekBeats,
                        PlannedVisit = weekBeats.Count(),
                        Visited = weekBeats.Where(x => x.IsVisited).Count(),
                        NotVisited = weekBeats.Where(x => !x.IsVisited).Count(),
                        CustomerCount = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? beatCustomerOrders.Where(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today).Select(x => x.CustomerId).Distinct().Count() : 0,
                        AvgLineItem = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? Convert.ToDecimal(beatCustomerOrders.Where(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today).Sum(z => z.lineItem)) / beatCustomerOrders.Where(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today).Count() : 0,
                        TotalAmount = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? Convert.ToInt32(Math.Round(beatCustomerOrders.Where(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today).Sum(z => z.GrossAmount), 0)) : 0,
                        TotalOrders = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? beatCustomerOrders.Count(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) : 0,
                        Conversion = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? beatCustomerOrders.Where(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today).Select(z => z.CustomerId).Distinct().Count() : 0,
                        AvgLineItemColor = "#ffffff",
                        ConversionColor = "#ffffff",
                        CustomerCountColor = "#ffffff",
                        NotVisitedColor = "#ffffff",
                        TotalAmountColor = "#ffffff",
                        TotalOrdersColor = "#ffffff",
                        VisitedColor = "#ffffff",
                    };

                    if (beatTarget != null)
                    {
                        salesDeshboardData.SaleMetricsWeekly.AvgLineItemColor = salesDeshboardData.SaleMetricsWeekly.AvgLineItem < beatTarget.AvgLineItem ? completeTargetColor : IncompleteTargetColor;
                        salesDeshboardData.SaleMetricsWeekly.TotalOrdersColor = salesDeshboardData.SaleMetricsWeekly.TotalOrders < (salesDeshboardData.SaleMetricsWeekly.PlannedVisit * beatTarget.OrderPercent / 100) ? completeTargetColor : IncompleteTargetColor;
                        salesDeshboardData.SaleMetricsWeekly.ConversionColor = salesDeshboardData.SaleMetricsWeekly.Conversion < (salesDeshboardData.SaleMetricsWeekly.PlannedVisit * beatTarget.ConversionPercent / 100) ? completeTargetColor : IncompleteTargetColor;
                        salesDeshboardData.SaleMetricsWeekly.TotalAmountColor = salesDeshboardData.SaleMetricsWeekly.TotalAmount < (salesDeshboardData.SaleMetricsWeekly.PlannedVisit * beatTarget.OrderPercent / 100) * beatTarget.AvgOrderAmount ? completeTargetColor : IncompleteTargetColor;
                        salesDeshboardData.SaleMetricsWeekly.VisitedColor = salesDeshboardData.SaleMetricsWeekly.Visited < (salesDeshboardData.SaleMetricsWeekly.PlannedVisit * beatTarget.VisitedPercent / 100) ? completeTargetColor : IncompleteTargetColor;

                        salesDeshboardData.SalesWeeklyTarget = new BeatTarget
                        {
                            AvgLineItem = beatTarget.AvgLineItem,
                            Conversion = Convert.ToInt32(salesDeshboardData.SaleMetricsWeekly.PlannedVisit * beatTarget.ConversionPercent / 100),
                            CustomerCount = Convert.ToInt32(salesDeshboardData.SaleMetricsWeekly.PlannedVisit * beatTarget.CustomerPercent / 100),
                            OrderCount = Convert.ToInt32((salesDeshboardData.SaleMetricsWeekly.PlannedVisit * beatTarget.OrderPercent / 100)),
                            OrderAmount = Convert.ToInt32((salesDeshboardData.SaleMetricsWeekly.PlannedVisit * beatTarget.OrderPercent / 100) * beatTarget.AvgOrderAmount),
                            Visited = Convert.ToInt32(salesDeshboardData.SaleMetricsWeekly.PlannedVisit * beatTarget.VisitedPercent / 100),
                        };
                    }
                    else
                    {
                        salesDeshboardData.SalesWeeklyTarget = new BeatTarget
                        {
                            AvgLineItem = 0,
                            Conversion = 0,
                            CustomerCount = 0,
                            OrderCount = 0,
                            OrderAmount = 0,
                            Visited = 0,
                        };
                    }

                    ClusterPareto clusterPareto = new ClusterPareto();

                    if (clusterId.HasValue)
                    {
                        var cmd1 = context.Database.Connection.CreateCommand();
                        cmd1.Parameters.Add(new SqlParameter("@warehouseid", people.WarehouseId));
                        cmd1.Parameters.Add(new SqlParameter("@clusterid", clusterId));
                        cmd1.CommandText = "[dbo].[GetCustomerItemPareto]";
                        cmd1.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd1.CommandTimeout = 600;


                        // Run the sproc
                        var reader1 = cmd1.ExecuteReader();
                        clusterPareto = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<ClusterPareto>(reader1).FirstOrDefault();
                    }

                    var monthBeats = MonthBeat.SelectMany(p => p.PlannedRoutes.Select(x => new BeatCustomer
                    {
                        CustomerId = x.CustomerId,
                        Active = x.Active,
                        BillingAddress = x.BillingAddress,
                        IsVisited = x.IsVisited,
                        BeatNumber = x.BeatNumber.HasValue ? x.BeatNumber.Value : todayBeat.PlannedRoutes.Count + 1,
                        lat = x.lat,
                        lg = x.lg,
                        Mobile = x.Mobile,
                        Name = x.Name,
                        IsKPP = x.IsKPP,
                        ShippingAddress = x.ShippingAddress,
                        ShopName = x.ShopName,
                        Skcode = x.Skcode,
                        WarehouseId = x.WarehouseId,
                        WarehouseName = x.WarehouseName,
                        AreaName = x.AreaName,
                        MaxOrderCount = x.MaxOrderCount,
                        AvgLineItem = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? Convert.ToDecimal(beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today).Average(z => z.lineItem)) : 0,
                        TotalOrder = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? beatCustomerOrders.Count(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) : 0,
                        TotalOrderAmount = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? Convert.ToInt32(Math.Round(beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today).Sum(z => z.GrossAmount), 0)) : 0,
                        //Comment = p.ActualRoutes != null && todayBeat.ActualRoutes.Any(y => y.CustomerId == x.CustomerId) ? todayBeat.ActualRoutes.FirstOrDefault(y => y.CustomerId == x.CustomerId).Comment : "",
                        Status = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ?
                                   "Ordered" : (!x.IsVisited ? "Not Visited" : (p.ActualRoutes != null && p.ActualRoutes.Any(y => y.CustomerId == x.CustomerId && !string.IsNullOrEmpty(y.ShopCloseImage))
                                   ? "Shop Closed - Skip" : "Visited"))
                    }).ToList()).ToList();

                    salesDeshboardData.SaleMetricsMonthly = new BeatSaleMonthly
                    {
                        //BeatCustomers= monthBeats,
                        CustomerCount = beatCustomerOrders != null && beatCustomerOrders.Any() ? beatCustomerOrders.Select(x => x.CustomerId).Distinct().Count() : 0,
                        TotalAmount = beatCustomerOrders != null && beatCustomerOrders.Any() ? Convert.ToInt32(Math.Round(beatCustomerOrders.Sum(z => z.GrossAmount), 0)) : 0,
                        TotalOrders = beatCustomerOrders != null && beatCustomerOrders.Any() ? beatCustomerOrders.Count() : 0,
                        AvgLineItem = beatCustomerOrders != null && beatCustomerOrders.Any() ? Convert.ToDecimal(beatCustomerOrders.Sum(z => z.lineItem)) / beatCustomerOrders.Count() : 0,
                        AvgLineItemColor = "#ffffff",
                        CustomerCountColor = "#ffffff",
                        TotalAmountColor = "#ffffff",
                        TotalOrdersColor = "#ffffff",
                        CustomerPareto = clusterPareto.CustomerPareto,
                        ProductPareto = clusterPareto.ItemPareto
                    };


                    if (beatTarget != null)
                    {
                        salesDeshboardData.SaleMetricsMonthly.AvgLineItemColor = salesDeshboardData.SaleMetricsMonthly.AvgLineItem < beatTarget.AvgLineItem ? completeTargetColor : IncompleteTargetColor;
                        salesDeshboardData.SaleMetricsMonthly.TotalOrdersColor = salesDeshboardData.SaleMetricsMonthly.TotalOrders < (monthBeats.Count() * beatTarget.OrderPercent / 100) ? completeTargetColor : IncompleteTargetColor;
                        salesDeshboardData.SaleMetricsMonthly.TotalAmountColor = salesDeshboardData.SaleMetricsMonthly.TotalAmount < (monthBeats.Count() * beatTarget.OrderPercent / 100) * beatTarget.AvgOrderAmount ? completeTargetColor : IncompleteTargetColor;
                        salesDeshboardData.SaleMetricsMonthly.CustomerCountColor = salesDeshboardData.SaleMetricsMonthly.CustomerCount < (monthBeats.Count() * beatTarget.CustomerPercent / 100) ? completeTargetColor : IncompleteTargetColor;

                        salesDeshboardData.SalesMonthlyTarget = new SalesMonthlyTarget
                        {
                            AvgLineItem = beatTarget.AvgLineItem,
                            CustomerCount = Convert.ToInt32(monthBeats.Count() * beatTarget.CustomerPercent / 100),
                            OrderCount = Convert.ToInt32(monthBeats.Count() * beatTarget.OrderPercent / 100),
                            OrderAmount = Convert.ToInt32(monthBeats.Count() * beatTarget.OrderPercent / 100) * beatTarget.AvgOrderAmount,
                            CustomerPareto = beatTarget.CustomerPareto,
                            ProductPareto = beatTarget.ProductPareto
                        };

                    }
                    else
                    {
                        salesDeshboardData.SalesMonthlyTarget = new SalesMonthlyTarget
                        {
                            AvgLineItem = 0,
                            CustomerCount = 0,
                            OrderCount = 0,
                            OrderAmount = 0,
                            CustomerPareto = 0,
                            ProductPareto = 0
                        };
                    }

                }
                else
                {
                    salesDeshboardData.MyBeat = new MyBeat
                    {
                        AvgLineItemColor = "#ffffff",
                        BeatAmountColor = "#ffffff",
                        BeatOrderColor = "#ffffff",
                        ConversionColor = "#ffffff",
                        VisitedColor = "#ffffff",
                    };
                    salesDeshboardData.SaleMetricsWeekly = new BeatSaleWeekly
                    {

                        AvgLineItemColor = "#ffffff",
                        ConversionColor = "#ffffff",
                        CustomerCountColor = "#ffffff",
                        NotVisitedColor = "#ffffff",
                        TotalAmountColor = "#ffffff",
                        TotalOrdersColor = "#ffffff",
                        VisitedColor = "#ffffff",
                    };
                    salesDeshboardData.SaleMetricsMonthly = new BeatSaleMonthly
                    {
                        AvgLineItemColor = "#ffffff",
                        CustomerCountColor = "#ffffff",
                        TotalAmountColor = "#ffffff",
                        TotalOrdersColor = "#ffffff",
                    };
                    salesDeshboardData.SalesMetricsDaily = new BeatSale
                    {
                        AvgLineItemColor = "#ffffff",
                        TotalAmountColor = "#ffffff",
                        TotalOrdersColor = "#ffffff",
                        CustomerCountColor = "#ffffff",
                    };
                }

                if (MonthCustomers != null && MonthCustomers.Any())
                {
                    var customerIds = MonthCustomers.Select(x => x.CustomerId).ToList();
                    var newCustSale = context.DbOrderMaster.Where(x => customerIds.Contains(x.CustomerId) && x.OrderTakenSalesPersonId == peopleId).Select(x => new { x.CustomerId, x.OrderId, x.GrossAmount }).ToList();
                    salesDeshboardData.CustomerAcquisitionMonthly = new BeatSale
                    {
                        CustomerCount = MonthCustomers.Count(),
                        TotalAmount = newCustSale != null && newCustSale.Any() ? Convert.ToInt32(Math.Round(newCustSale.Sum(x => x.GrossAmount), 0)) : 0,
                        TotalOrders = newCustSale != null && newCustSale.Any() ? newCustSale.Count() : 0,
                    };
                }
                else
                {
                    salesDeshboardData.CustomerAcquisitionMonthly = new BeatSale();
                }
                try
                {
                    #region  CancellationReportResDc
                    CancellationReportResDc result = new CancellationReportResDc();
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();
                    var cmdCancellation = context.Database.Connection.CreateCommand();
                    cmdCancellation.CommandText = "[Cancellation].[SalesManReport]";
                    cmdCancellation.CommandType = System.Data.CommandType.StoredProcedure;
                    cmdCancellation.Parameters.Add(new SqlParameter("@peopleId", peopleId));
                    var readerCancellation = cmdCancellation.ExecuteReader();
                    var CancellationReport = ((IObjectContextAdapter)context)
                     .ObjectContext
                     .Translate<CancellationReportDc>(readerCancellation).FirstOrDefault();
                    if (CancellationReport != null)
                    {
                        //on amount Cancellation
                        result.CancelAmount = Math.Round(CancellationReport.CurrentMonthCancelValue, 2);  //Current Month Cancellation amount   

                        // on count Cancellation
                        result.CancelCount = CancellationReport.CurrentMonthCancelCount; // Current month Cancellation count
                        result.CancelCountDiff = result.CancelCount - CancellationReport.LastMonthCancelCount;

                        double currentCancelCountPercent = Math.Round(CancellationReport.CurrentMonthCancelCount > 0 ? Convert.ToDouble(CancellationReport.CurrentMonthCancelCount) / CancellationReport.CurrentMonthTotalCount * 100 : 0, 2);
                        double lastCancelCountPercent = Math.Round(CancellationReport.LastMonthCancelCount > 0 ? Convert.ToDouble(CancellationReport.LastMonthCancelCount) / CancellationReport.LastMonthTotalCount * 100 : 0, 2);
                        result.CompareCountPercent = Math.Round(currentCancelCountPercent - lastCancelCountPercent, 2);

                        //Cancellation  Percent on value
                        result.CancellationPercant = Math.Round(CancellationReport.CurrentMonthCancelValue > 0 ? Convert.ToDouble(CancellationReport.CurrentMonthCancelValue) / CancellationReport.CurrentMonthTotalValue * 100 : 0, 2);
                        double lastCancellationPercant = CancellationReport.LastMonthCancelValue > 0 ? Convert.ToDouble(CancellationReport.LastMonthCancelValue) / CancellationReport.LastMonthTotalValue * 100 : 0;
                        result.CompareCancellationPercant = Math.Round(result.CancellationPercant - lastCancellationPercant, 2);
                        if (result.CancellationPercant >= 0 && result.CancellationPercant <= 5)
                        {
                            result.Backgroundcolor = "#FFFFFF";
                            result.WarningCount = 0;
                        }
                        else if (result.CancellationPercant > 5 && result.CancellationPercant < 10)
                        {
                            result.Backgroundcolor = "#FFFF00"; result.WarningCount = 0;
                        }
                        else
                        {
                            result.Backgroundcolor = "#FF0000"; //red
                            result.WarningCount = Convert.ToInt32(result.CancellationPercant / 10);
                        }

                    }
                    salesDeshboardData.CancellationReports = result;
                    #endregion
                }
                catch (Exception s) { }

            }
            return salesDeshboardData;
        }

        [Route("GetExecutiveRoute")]
        [AllowAnonymous]
        public HttpResponseMessage GetExecutiveRoute(int peopleId, DateTime date)
        {
            using (var db = new AuthContext())
            {
                GlobalcustomerDetail obj = new GlobalcustomerDetail();

                MongoDbHelper<ExecutiveBeats> mongoDbHelper = new MongoDbHelper<ExecutiveBeats>();
                var today = date.Date;
                var todayBeats = mongoDbHelper.Select(x => x.PeopleId == peopleId && x.AssignmentDate == today);

                if (todayBeats != null && todayBeats.Any())
                {

                    todayBeats.ForEach(s => s.PlannedRoutes = s.PlannedRoutes.Where(x => string.IsNullOrEmpty(x.Day) || (!string.IsNullOrEmpty(x.Day) && x.Day.Trim().ToLower() == date.DayOfWeek.ToString().ToLower())).ToList());

                    //todayBeats.ForEach(s => s.PlannedRoutes.ForEach(x => { 
                    //    x.IsVisited = s.ActualRoutes.Any(y => y.Day == x.Day);
                    //    if(s.ActualRoutes.Any(y => y.Day == x.Day))
                    //        x.VisitedOn = s.ActualRoutes.FirstOrDefault(y => y.Day == x.Day).CreatedDate;
                    //})) ;

                    //var existingActualRoute = todayBeats != null
                    //                    ? todayBeats.Where(s => s.ActualRoutes != null && s.ActualRoutes.Any()).SelectMany(z => z.ActualRoutes)
                    //                    : null;

                    //if (existingActualRoute != null && existingActualRoute.Any())
                    //    todayBeats.ForEach(s => s.PlannedRoutes.RemoveAll(x => existingActualRoute.Select(z => z.CustomerId).Contains(x.CustomerId)));

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        customers = todayBeats,
                        Status = true,
                        Message = "Customer Found"
                    });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        customers = todayBeats,
                        Status = false,
                        Message = "No Customer found"
                    });


            }
        }


        #region Beat Plan



        [Route("StartDay")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<bool> StartDay(DayStartParams param)
        {
            BeatsManager manager = new BeatsManager();
            return await manager.InsertBeatInMongo(param.PeopleId, param.lat, param.lng, param.DayStartAddress);
        }


        [Route("IsDayStarted/{peopleId}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> IsDayStarted(int PeopleId)
        {
            BeatsManager manager = new BeatsManager();
            return await manager.IsDayStarted(PeopleId);
        }

        [Route("BeatStart/{peopleId}/{customerId}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<DateTime?> BeatStart(int peopleId, int customerId)
        {
            BeatsManager manager = new BeatsManager();
            return await manager.BeatStart(peopleId, customerId);
        }


        #endregion

        [Route("InactiveCustOrderCount/{customerid}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<int> InactiveCustOrderCount(int customerid)
        {
            BeatsManager manager = new BeatsManager();
            return await manager.InactiveCustOrderCount(customerid);
        }


        [Route("PlanNextDayBeat")]
        [HttpGet]
        public async Task<bool> PlanNextDayBeat(int customerId, int peopleId, DateTime planDate, bool isAddOnBeat = false)
        {
            if (!isAddOnBeat)
            {
                MongoDbHelper<NextDayBeatPlan> mongoDbHelper = new MongoDbHelper<NextDayBeatPlan>();
                bool result = await mongoDbHelper.InsertAsync(new NextDayBeatPlan
                {
                    CustomerId = customerId,
                    ExecutiveId = peopleId,
                    PlanDate = planDate.Date,
                    CreatedDate = DateTime.Now
                });
                return result;
            }
            else
            {
                //using (var context = new AuthContext())
                //{
                //    string day = planDate.DayOfWeek.ToString();
                //    if (!context.CustomerExecutiveMappings.Any(x => x.ExecutiveId == peopleId && x.CustomerId == customerId && x.Day == day && x.IsActive))
                //    {
                //        int beat = context.CustomerExecutiveMappings.Count(x => x.ExecutiveId == peopleId && x.Day == day && x.IsActive);
                //        context.CustomerExecutiveMappings.Add(new Model.Store.CustomerExecutiveMapping
                //        {
                //            CustomerId = customerId,
                //            CreatedDate = DateTime.Now,
                //            CreatedBy = peopleId,
                //            Beat = beat,
                //            Day = day,
                //            ExecutiveId = peopleId,
                //            IsActive = true,
                //            IsDeleted = false
                //        });
                //        context.Commit();
                //    }
                //}
                return true;
            }
        }

        //[Route("CheckCustomeronBeat")]
        //[HttpGet]
        //public async Task<string> CheckCustomerOnBeat(int customerId, int peopleId)
        //{
        //    string day = string.Empty;
        //    using (var context = new AuthContext())
        //    {
        //        var custmapping = await context.CustomerExecutiveMappings.FirstOrDefaultAsync(x => x.ExecutiveId == peopleId && x.CustomerId == customerId && x.IsActive);
        //        if (custmapping != null)
        //            day = custmapping.Day;
        //    }
        //    return day;
        //}

        [Route("UpdateActualRoute")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<bool> UpdateActualRoute(List<DataContracts.External.MobileExecutiveDC.SalesAppRouteParam> param)
        {
            BeatsManager manager = new BeatsManager();
            return await manager.UpdateActualRoute(param);
        }
        [Route("UpdateActualRouteForSkip")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<bool> UpdateActualRouteForSkip(DataContracts.External.MobileExecutiveDC.SalesAppRouteParam param)
        {
            BeatsManager manager = new BeatsManager();
            return await manager.UpdateActualRouteForSkip(param);
        }


        [Route("CustomerAddressUpdateRequest")]
        [HttpPost]
        public async Task<HttpResponseMessage> CustomerAddressUpdateRequest(CustomerUpdateRequest customerUpdateRequest)
        {
            var Customer = new Customer();
            if (customerUpdateRequest.CustomerId > 0)
            {
                using (var context = new AuthContext())
                {
                    Customer = context.Customers.FirstOrDefault(x => x.CustomerId == customerUpdateRequest.CustomerId);
                }
            }
            MongoDbHelper<CustomerUpdateRequest> mongoDbHelper = new MongoDbHelper<CustomerUpdateRequest>();
            int count = mongoDbHelper.Count(x => x.CustomerId == customerUpdateRequest.CustomerId && x.RequestBy == customerUpdateRequest.RequestBy && x.Status == 0);
            if (count == 0)
            {
                customerUpdateRequest.CreatedDate = DateTime.Now;
                customerUpdateRequest.Status = 0;
                customerUpdateRequest.UpdatedDate = DateTime.Now;
                customerUpdateRequest.WarehouseId = Customer.Warehouseid ?? 0;
                customerUpdateRequest.SkCode = Customer.Skcode;
                customerUpdateRequest.MobileNo = Customer.Mobile;
                bool result = await mongoDbHelper.InsertAsync(customerUpdateRequest);
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = result,
                    Message = result ? "Updated request save successfully." : "Some issue occurred please try after some time."
                });

            }
            else
            {

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = false,
                    Message = "Already one updated request pending for this customer."
                });
            }
        }

        #endregion


        [Route("GetDefaultCustomerid")]
        [HttpGet]
        public SalesAppDefaultCustomersDC GetDefaultCustomerid(int WarehouseId)
        {
            using (var db = new AuthContext())
            {
                SalesAppDefaultCustomersDC res;
                var companydetails = db.CompanyDetailsDB.Where(x => x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (companydetails != null)
                {
                    if (WarehouseId > 0)
                    {
                        MongoDbHelper<SalesAppDefaultCustomers> SalesAppmongoDbHelper = new MongoDbHelper<SalesAppDefaultCustomers>();
                        var defaultCustomer = SalesAppmongoDbHelper.Select(x => x.WarehouseId == WarehouseId).FirstOrDefault();
                        if (defaultCustomer != null)
                        {
                            companydetails.DefaultSalesSCcustomerId = defaultCustomer.CustomerId;
                            res = new SalesAppDefaultCustomersDC
                            {
                                DefaultSalesSCcustomerId = companydetails.DefaultSalesSCcustomerId,
                                Status = true,
                                Message = "Success!!"
                            };
                            return res;
                        }
                        else
                        {
                            res = new SalesAppDefaultCustomersDC
                            {
                                DefaultSalesSCcustomerId = 0,
                                Status = false,
                                Message = "No Data Found!!"
                            };
                            return res;
                        }
                    }
                    else
                    {
                        res = new SalesAppDefaultCustomersDC
                        {
                            DefaultSalesSCcustomerId = 0,
                            Status = false,
                            Message = "No Data Found!!"
                        };
                        return res;
                    }
                }
                else
                {
                    res = new SalesAppDefaultCustomersDC
                    {
                        DefaultSalesSCcustomerId = 0,
                        Status = false,
                        Message = "No Data Found!!"
                    };
                    return res;
                }
            }

        }

        [Route("InsertMissExecutiveBeatJob")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> InsertMissExecutiveBeatJob()
        {
            List<int> executiveIds = new List<int>();
            using (var context = new AuthContext())
            {
                executiveIds = await context.ClusterStoreExecutives.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.ExecutiveId).Distinct().ToListAsync();
            }
            if (executiveIds.Any())
            {
                BeatsManager manager = new BeatsManager();
                await manager.InsertNotStartExecutiveBeatInMongo(executiveIds);
            }
            return true;
        }


        [Route("GetAddressUpdateRequest")]
        [HttpPost]
        public dynamic GetAddressUpdateRequestList(AddressUpdatepaginationDTO filterOrderDTO)
        {
            int Skiplist = (filterOrderDTO.Skip - 1) * filterOrderDTO.Take;
            AddressUpdateResDTO paggingData = new AddressUpdateResDTO();

            MongoDbHelper<CustomerUpdateRequest> mongoDbHelper = new MongoDbHelper<CustomerUpdateRequest>();
            var orderPredicate = PredicateBuilder.New<CustomerUpdateRequest>();
            if (filterOrderDTO.WarehouseId > 0)
            {
                if (filterOrderDTO.WarehouseId > 0)
                    orderPredicate.And(x => x.WarehouseId == filterOrderDTO.WarehouseId);

                if (!string.IsNullOrEmpty(filterOrderDTO.Keyword))
                    orderPredicate.And(x => x.SkCode.Contains(filterOrderDTO.Keyword) || x.MobileNo.Contains(filterOrderDTO.Keyword));

                if (filterOrderDTO.status >= 0)
                    orderPredicate.And(x => x.Status == filterOrderDTO.status);

                if (filterOrderDTO.FromDate.HasValue && filterOrderDTO.ToDate.HasValue)
                    orderPredicate.And(x => x.CreatedDate >= filterOrderDTO.FromDate.Value && x.CreatedDate <= filterOrderDTO.ToDate.Value);

                int dataCount = mongoDbHelper.Count(orderPredicate, collectionName: "CustomerUpdateRequest");

                var result = new List<CustomerUpdateRequest>();
                result = mongoDbHelper.Select(orderPredicate, x => x.OrderByDescending(z => z.CreatedDate), Skiplist, filterOrderDTO.Take, collectionName: "CustomerUpdateRequest").ToList();

                var warehouseIds = result.Select(x => x.WarehouseId).Distinct().ToList();
                using (var context = new AuthContext())
                {
                    var WarehouseName = context.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId)).ToList();
                    result.ForEach(x =>
                    {
                        x.WarehouseName = WarehouseName.Where(y => y.WarehouseId == x.WarehouseId).Select(y => y.WarehouseName).FirstOrDefault();
                        if (x.Status == 0)
                        {
                            x.status = "Pending";
                        }
                        if (x.Status == 1)
                        {
                            x.status = "Approved";
                        }
                        if (x.Status == 2)
                        {
                            x.status = "Reject";
                        }
                    });
                }
                paggingData.totalcount = 0;
                if (result != null && result.Any())
                {
                    paggingData.totalcount = dataCount;
                    paggingData.result = result;
                }
                return paggingData;
            }
            else
            {
                return null;
            }
        }

        [Route("UpdateAddressRequest")]
        [HttpGet]
        public async Task<bool> UpdateAddressRequest(string ObjId, int Status)
        {
            bool result = false;

            ObjectId id = ObjectId.Parse(ObjId);
            MongoDbHelper<CustomerUpdateRequest> mongoDbHelper = new MongoDbHelper<CustomerUpdateRequest>();
            ExpressionStarter<CustomerUpdateRequest> Predicate = PredicateBuilder.New<CustomerUpdateRequest>(x => (x.Id == id));
            CustomerUpdateRequest CustomerUpdateRequest = mongoDbHelper.Select(Predicate, null).FirstOrDefault();

            if (CustomerUpdateRequest != null && CustomerUpdateRequest.CustomerId > 0 && Status > 0)
            {
                if (Status == 1)
                {  //1 mean update in customer
                    using (var context = new AuthContext())
                    {
                        var cust = context.Customers.FirstOrDefault(c => c.CustomerId == CustomerUpdateRequest.CustomerId);
                        //cust.lat = CustomerUpdateRequest.UpdatedLat;
                        //cust.lg = CustomerUpdateRequest.UpdatedLng;
                        //cust.ShippingAddress = CustomerUpdateRequest.UpdateedAddress;
                        cust.UpdatedDate = indianTime;
                        context.Entry(cust).State = EntityState.Modified;
                        context.Commit();
                    }
                }
                CustomerUpdateRequest.Status = Status;
                await mongoDbHelper.ReplaceAsync(CustomerUpdateRequest.Id, CustomerUpdateRequest);
                result = true;
            }
            return result;
        }

        [Route("ExportAddressUpdateRequest")]
        [HttpPost]
        public dynamic ExportAddressUpdateRequest(AddressUpdatepaginationDTO filterOrderDTO)
        {
            //int Skiplist = (filterOrderDTO.Skip - 1) * filterOrderDTO.Take;
            AddressUpdateResDTO paggingData = new AddressUpdateResDTO();

            MongoDbHelper<ExportAddressUpdateRequest> mongoDbHelper = new MongoDbHelper<ExportAddressUpdateRequest>();
            var orderPredicate = PredicateBuilder.New<ExportAddressUpdateRequest>();

            if (filterOrderDTO.WarehouseId > 0)
                orderPredicate.And(x => x.WarehouseId == filterOrderDTO.WarehouseId);

            if (!string.IsNullOrEmpty(filterOrderDTO.Keyword))
                orderPredicate.And(x => x.SkCode.Contains(filterOrderDTO.Keyword) || x.MobileNo.Contains(filterOrderDTO.Keyword));

            if (filterOrderDTO.status > 0)
                orderPredicate.And(x => x.Status == filterOrderDTO.status);

            if (filterOrderDTO.FromDate.HasValue && filterOrderDTO.ToDate.HasValue)
                orderPredicate.And(x => x.CreatedDate >= filterOrderDTO.FromDate.Value && x.CreatedDate <= filterOrderDTO.ToDate.Value);

            //int dataCount = mongoDbHelper.Count(orderPredicate, collectionName: "CustomerUpdateRequest");
            var result = new List<ExportAddressUpdateRequest>();
            result = mongoDbHelper.Select(orderPredicate, x => x.OrderByDescending(z => z.CreatedDate), null, null, collectionName: "CustomerUpdateRequest").ToList();
            var warehouseIds = result.Select(x => x.WarehouseId).Distinct().ToList();
            using (var context = new AuthContext())
            {
                var WarehouseName = context.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId)).ToList();
                result.ForEach(x =>
                {
                    x.WarehouseName = WarehouseName.Where(y => y.WarehouseId == x.WarehouseId).Select(y => y.WarehouseName).FirstOrDefault();
                    if (x.Status == 0)
                    {
                        x.status = "Pending";
                    }
                    if (x.Status == 1)
                    {
                        x.status = "Approved";
                    }
                    if (x.Status == 2)
                    {
                        x.status = "Reject";
                    }
                });
            }

            paggingData.totalcount = 0;
            if (result != null && result.Any())
            {
                paggingData.result = result;
            }
            return paggingData;
        }

        /// <summary>
        /// Get Sales Dashboard Report on saral
        /// </summary>
        /// <param name="peopleId"></param>
        /// <returns></returns>
        [Route("SalesDashboardReport")]
        [HttpPost]
        public async Task<List<SalesDashboardReportDc>> GetSalesDashboardReport(SalesDashboardReportReqDc req)
        {
            List<string> WeekDays = new List<string>();
            var count = (req.EndDate.AddDays(1) - req.StartDate).TotalDays;
            if (count >= 7)
            {
                WeekDays.Add("Monday");
                WeekDays.Add("Tuesday");
                WeekDays.Add("Wednesday");
                WeekDays.Add("Thursday");
                WeekDays.Add("Friday");
                WeekDays.Add("Saturday");
                WeekDays.Add("Sunday");
            }
            else
            {
                for (var date = req.StartDate; date <= req.EndDate; date = date.AddDays(1))
                {
                    WeekDays.Add(date.DayOfWeek.ToString());
                }
                WeekDays.ToArray();
            }
            List<SalesDashboardReportDc> result = new List<SalesDashboardReportDc>();
            if (req != null && req.WarehouseId > 0 && req.PeopleIds.Any())
            {
                using (var context = new AuthContext())
                {
                    MongoDbHelper<ExecutiveBeats> mongoDbHelper = new MongoDbHelper<ExecutiveBeats>();
                    var MonthBeat = (await mongoDbHelper.SelectAsync(x => req.PeopleIds.Contains(x.PeopleId) && x.PlannedRoutes != null && x.PlannedRoutes.Count() > 0 && x.AssignmentDate >= req.StartDate && x.AssignmentDate <= req.EndDate)).ToList();

                    if (MonthBeat != null && MonthBeat.Any())
                    {
                        #region GetPeopleBeatCustomerOrder
                        var beatCustomerids = MonthBeat.Where(x => x.PlannedRoutes != null).SelectMany(x => x.PlannedRoutes.Select(y => y.CustomerId)).ToList();
                        var actualCustiomerids = MonthBeat.Where(x => x.ActualRoutes != null).SelectMany(x => x.ActualRoutes).Any() ? beatCustomerids : new List<int>();
                        if (actualCustiomerids != null && actualCustiomerids.Any())
                        {
                            beatCustomerids.AddRange(actualCustiomerids);
                        }
                        beatCustomerids = beatCustomerids.Distinct().ToList();

                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();
                        //CustomerIds
                        var customerIdDt = new DataTable();
                        customerIdDt.Columns.Add("IntValue");
                        foreach (var item in beatCustomerids)
                        {
                            var dr = customerIdDt.NewRow();
                            dr["IntValue"] = item;
                            customerIdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("customerId", customerIdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";

                        //PeopleIDs
                        var peopleIdDt = new DataTable();
                        peopleIdDt.Columns.Add("IntValue");
                        foreach (var item in req.PeopleIds)
                        {
                            var dr = peopleIdDt.NewRow();
                            dr["IntValue"] = item;
                            peopleIdDt.Rows.Add(dr);
                        }
                        var param1 = new SqlParameter("ExectiveIds", peopleIdDt);
                        param1.SqlDbType = SqlDbType.Structured;
                        param1.TypeName = "dbo.IntValues";

                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetPeopleBeatCustomerOrder]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandTimeout = 600;
                        cmd.Parameters.Add(param);
                        cmd.Parameters.Add(param1);


                        // Run the sproc
                        var reader = cmd.ExecuteReader();
                        var beatCustomerOrders = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<PeopleBeatCustomerOrder>(reader).ToList();
                        #endregion

                        #region GetPeopleBeatCustomers

                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();

                        var DaysIdDt = new DataTable();
                        DaysIdDt.Columns.Add("stringValues");
                        foreach (var item in WeekDays)
                        {
                            var dr = DaysIdDt.NewRow();
                            dr["stringValues"] = item;
                            DaysIdDt.Rows.Add(dr);
                        }
                        var param2 = new SqlParameter("Days", DaysIdDt);
                        param2.SqlDbType = SqlDbType.Structured;
                        param2.TypeName = "dbo.stringValues";

                        //PeopleIDs
                        var peopleIdDts = new DataTable();
                        peopleIdDts.Columns.Add("IntValue");
                        foreach (var item in req.PeopleIds)
                        {
                            var dr = peopleIdDts.NewRow();
                            dr["IntValue"] = item;
                            peopleIdDts.Rows.Add(dr);
                        }
                        var param3 = new SqlParameter("PeopleIds", peopleIdDts);
                        param3.SqlDbType = SqlDbType.Structured;
                        param3.TypeName = "dbo.IntValues";

                        var cmd1 = context.Database.Connection.CreateCommand();
                        cmd1.CommandText = "[dbo].[GetPeopleBeatCustomers]";
                        cmd1.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd1.CommandTimeout = 600;
                        cmd1.Parameters.Add(param2);
                        cmd1.Parameters.Add(param3);
                        // Run the sproc
                        var reader1 = cmd1.ExecuteReader();
                        var PeopleBeatCustomers = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<PeopleBeatCustomers>(reader1).ToList();
                        #endregion

                        result = MonthBeat.GroupBy(x => x.PeopleId).Select(x => new SalesDashboardReportDc
                        {
                            ExectiveId = (PeopleBeatCustomers != null && PeopleBeatCustomers.Count() > 0) ? PeopleBeatCustomers.Where(q => q.PeopleId == x.Key).Select(w => w.PeopleId).FirstOrDefault() : 0,
                            ExectiveName = (PeopleBeatCustomers != null && PeopleBeatCustomers.Count() > 0) ? PeopleBeatCustomers.FirstOrDefault(q => q.PeopleId == x.Key).ExectiveName : " ",
                            WarehouseName = (PeopleBeatCustomers != null && PeopleBeatCustomers.Count() > 0) ? PeopleBeatCustomers.FirstOrDefault(q => q.PeopleId == x.Key).WarehouseName : " ",
                            ClusterName = (PeopleBeatCustomers != null && PeopleBeatCustomers.Count() > 0) ? PeopleBeatCustomers.FirstOrDefault(q => q.PeopleId == x.Key).ClusterName : " ",
                            TotalCustomer = (PeopleBeatCustomers != null && PeopleBeatCustomers.Count() > 0) ? PeopleBeatCustomers.FirstOrDefault(q => q.PeopleId == x.Key).TotalCustomer : 0,
                            TotalBeat = (PeopleBeatCustomers != null && PeopleBeatCustomers.Count() > 0) ? PeopleBeatCustomers.FirstOrDefault(q => q.PeopleId == x.Key).TotalBeat : 0,
                            CustomerPlann = x.Any(u => u.PlannedRoutes != null && u.PlannedRoutes.Count() > 0) ? x.Where(u => u.PlannedRoutes != null && u.PlannedRoutes.Count() > 0).Sum(u => u.PlannedRoutes.Count()) : 0,
                            Visited = x.Any(u => u.PlannedRoutes != null && u.PlannedRoutes.Count() > 0) ? x.Where(u => u.PlannedRoutes != null && u.PlannedRoutes.Count() > 0).Sum(u => u.PlannedRoutes.Where(e => e.IsVisited == true).Count()) : 0,
                            Ordercount = beatCustomerOrders != null && beatCustomerOrders.Any(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key) ? Convert.ToInt32(beatCustomerOrders.Where(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key).Count()) : 0,
                            OrderAmount = beatCustomerOrders != null && beatCustomerOrders.Any(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key) ? Convert.ToInt32(Math.Round(beatCustomerOrders.Where(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key).Sum(z => z.GrossAmount), 0)) : 0,
                            AvgLine = beatCustomerOrders != null && beatCustomerOrders.Any(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key) ? Convert.ToDecimal(beatCustomerOrders.Where(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key).Sum(z => z.lineItem) / beatCustomerOrders.Count(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key)) : 0,
                            Conversion = beatCustomerOrders != null ? beatCustomerOrders.Where(y => y.OrderId > 0).Select(z => z.CustomerId).Distinct().Count() : 0,
                            VisitDetails = x.Any(u => u.PlannedRoutes != null && u.PlannedRoutes.Count() > 0) ? x.SelectMany(i => i.PlannedRoutes.Where(p => p.IsVisited).Select(y => new CustomerVisitDc
                            {
                                Address = y.ShippingAddress,
                                SKcode = y.Skcode,
                                Date = y.VisitedOn,
                                ShopName = y.ShopName
                            })).ToList() : null
                        }).ToList();
                    }
                }
            }
            return result;
        }


        #region SalesApp Company Apphome 

        [Route("GetAllSalesStore")]
        [HttpGet]
        public async Task<List<RetailerStore>> GetAllSalesStore(int PeopleId, int warehouseId, string lang)
        {
            List<RetailerStore> retailerStore = new List<RetailerStore>();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetAllStore]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@lang", lang));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                retailerStore = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<RetailerStore>(reader).ToList();

                #region Mappedstore
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                if (retailerStore != null && retailerStore.Any())
                {
                    List<int> Subcatids = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                    retailerStore = retailerStore.Where(x => Subcatids.Contains(x.SubCategoryId)).ToList();
                }
                #endregion
                #region block Barnd
                RetailerAppManager retailerAppManager = new RetailerAppManager();
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseId);
                if (retailerStore != null && retailerStore.Any() && blockBarnds != null && blockBarnds.Any())
                {
                    retailerStore = retailerStore.Where(x => !(blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId))).ToList();
                }
                #endregion
            }
            return retailerStore;
        }

        [Route("SalesSubCategoryOffer")]
        [HttpGet]
        public async Task<OfferdataDc> SalesSubCategoryOffer(int customerId, int PeopleId, int SubCategoryId)
        {
            List<OfferDc> FinalBillDiscount = new List<OfferDc>();
            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
            OfferdataDc res;
            using (AuthContext context = new AuthContext())
            {
                CustomersManager manager = new CustomersManager();

                List<BillDiscountOfferDc> billDiscountOfferDcs = manager.GetCustomerBillDiscount(customerId, "Retailer App");
                if (billDiscountOfferDcs.Any())
                {
                    foreach (var billDiscountOfferDc in billDiscountOfferDcs.Where(x => x.BillDiscountType == "subcategory" && x.OfferBillDiscountItems.Any(y => y.Id == SubCategoryId)))
                    {

                        var bdcheck = new OfferDc
                        {
                            OfferId = billDiscountOfferDc.OfferId,

                            OfferName = billDiscountOfferDc.OfferName,
                            OfferCode = billDiscountOfferDc.OfferCode,
                            OfferCategory = billDiscountOfferDc.OfferCategory,
                            OfferOn = billDiscountOfferDc.OfferOn,
                            start = billDiscountOfferDc.start,
                            end = billDiscountOfferDc.end,
                            DiscountPercentage = billDiscountOfferDc.DiscountPercentage,
                            BillAmount = billDiscountOfferDc.BillAmount,
                            LineItem = billDiscountOfferDc.LineItem,
                            Description = billDiscountOfferDc.Description,
                            BillDiscountOfferOn = billDiscountOfferDc.BillDiscountOfferOn,
                            BillDiscountWallet = billDiscountOfferDc.BillDiscountWallet,
                            IsMultiTimeUse = billDiscountOfferDc.IsMultiTimeUse,
                            IsUseOtherOffer = billDiscountOfferDc.IsUseOtherOffer,
                            IsScratchBDCode = billDiscountOfferDc.IsScratchBDCode,
                            BillDiscountType = billDiscountOfferDc.BillDiscountType,
                            OfferAppType = billDiscountOfferDc.OfferAppType,
                            ApplyOn = billDiscountOfferDc.ApplyOn,
                            WalletType = billDiscountOfferDc.WalletType,
                            OfferBillDiscountItems = billDiscountOfferDc.OfferBillDiscountItems.Select(y => new OfferBillDiscountItemDc
                            {
                                CategoryId = y.CategoryId,
                                Id = y.Id,
                                IsInclude = y.IsInclude,
                                SubCategoryId = y.SubCategoryId
                            }).ToList(),
                            OfferItems = billDiscountOfferDc.OfferItems.Select(y => new OfferItemdc
                            {
                                IsInclude = y.IsInclude,
                                itemId = y.itemId
                            }).ToList(),
                            RetailerBillDiscountFreeItemDcs = null
                        };

                        FinalBillDiscount.Add(bdcheck);
                    }
                }
                res = new OfferdataDc()
                {
                    offer = FinalBillDiscount,
                    Status = true,
                    Message = "Success"
                };
                return res;
            }

        }
        [Route("SalesHomePageGetSubSubCategories")]
        [HttpGet]
        public async Task<CatScatSscatDCs> SalesHomePageGetSubSubCategories(string lang, int subCategoryId, int PeopleId, int warehouseId)
        {
            using (var db = new AuthContext())
            {
                RetailerAppManager retailerAppManager = new RetailerAppManager();
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                List<Category> Cat = new List<Category>();
                List<SubCategory> Scat = new List<SubCategory>();
                List<SubsubCategory> SsCat = new List<SubsubCategory>();
                try
                {
                    var subCategoryQuery = "select SubCategoryId, 0 Categoryid,  '' CategoryName, [SubCategoryId],  (Case when '" + lang + "'='hi' and ( HindiName is not null or HindiName='') then HindiName else SubcategoryName end) SubcategoryName ,[LogoUrl],[itemcount],StoreBanner from SubCategories where IsActive=1 and Deleted=0 and SubCategoryId=" + subCategoryId;

                    var brandQuery = "Exec GetRetailerBrandBySubCategoryId " + warehouseId + "," + subCategoryId + "," + lang;
                    var Scatv = db.Database.SqlQuery<SubCategoryDCs>(subCategoryQuery).ToList();
                    var SsCatv = db.Database.SqlQuery<SubsubCategoryDcs>(brandQuery).ToList();

                    CatScatSscatDCs CatScatSscatcdc = new CatScatSscatDCs
                    {
                        subCategoryDC = Mapper.Map(Scatv).ToANew<List<SubCategoryDCs>>(),
                        subsubCategoryDc = Mapper.Map(SsCatv).ToANew<List<SubsubCategoryDcs>>(),
                    };


                    #region block Barnd
                    var custtype = 4;
                    var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseId);
                    if (blockBarnds != null && blockBarnds.Any())
                    {
                        CatScatSscatcdc.subsubCategoryDc = CatScatSscatcdc.subsubCategoryDc.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                        CatScatSscatcdc.subCategoryDC = CatScatSscatcdc.subCategoryDC.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId))).ToList();
                    }
                    #endregion


                    if (CatScatSscatcdc.subsubCategoryDc != null && CatScatSscatcdc.subsubCategoryDc.Any())
                    {

                        List<string> strCondition = StoreCategorySubCategoryBrands.Select(x => x.Categoryid + "-" + x.SubCategoryId + "-" + x.BrandId).Distinct().ToList();
                        List<string> companyStrCondition = StoreCategorySubCategoryBrands.Select(x => x.Categoryid + "-" + x.SubCategoryId).Distinct().ToList();


                        CatScatSscatcdc.subsubCategoryDc = CatScatSscatcdc.subsubCategoryDc.Where(x => !(strCondition.Contains(x.Categoryid + " " + x.SubCategoryId + " " + x.SubsubCategoryid))).ToList();
                        CatScatSscatcdc.subCategoryDC = CatScatSscatcdc.subCategoryDC.Where(x => !(companyStrCondition.Contains(x.Categoryid + " " + x.SubCategoryId))).ToList();

                    }

                    return CatScatSscatcdc;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in SalesHomePageGetSubSubCategories " + ex.Message);
                    return null;
                }
            }
        }
        [Route("SalesGetItembySubCatAndBrand")]
        [HttpGet]
        public async Task<ItemListDc> SalesGetItembySubCatAndBrand(string lang, int PeopleId, int warehouseId, int scatid, int sscatid)
        {
            using (var context = new AuthContext())
            {
                List<ItemListDc> brandItem = new List<ItemListDc>();
                ItemListDc item = new ItemListDc();
                List<ItemDataDC> newdata = new List<ItemDataDC>();

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetItemBySubCatAndBrand]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@sscatid", sscatid));
                cmd.Parameters.Add(new SqlParameter("@scatid", scatid));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                newdata = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<ItemDataDC>(reader).ToList();


                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseId);
                if (blockBarnds != null && blockBarnds.Any())
                {
                    newdata = newdata.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                }
                #endregion

                foreach (var it in newdata)
                {
                    it.dreamPoint = it.dreamPoint.HasValue ? it.dreamPoint : 0;
                    it.marginPoint = it.marginPoint.HasValue ? it.marginPoint : 0;
                    if (!it.IsOffer)
                    {
                        /// Dream Point Logic && Margin Point
                        int? MP, PP;
                        double xPoint = xPointValue * 10;
                        //salesman 0.2=(0.02 * 10=0.2)
                        if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                        {
                            PP = 0;
                        }
                        else
                        {
                            PP = it.promoPerItems;
                        }
                        if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                        {
                            MP = 0;
                        }
                        else
                        {
                            double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                            MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                        }
                        if (PP > 0 && MP > 0)
                        {
                            int? PP_MP = PP + MP;
                            it.dreamPoint = PP_MP;
                        }
                        else if (MP > 0)
                        {
                            it.dreamPoint = MP;
                        }
                        else if (PP > 0)
                        {
                            it.dreamPoint = PP;
                        }
                        else
                        {
                            it.dreamPoint = 0;
                        }
                        // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                        if (it.price > it.UnitPrice)
                        {
                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                        }
                        else
                        {
                            it.marginPoint = 0;
                        }
                    }

                    if (lang.Trim() == "hi")
                    {
                        if (!string.IsNullOrEmpty(it.HindiName))
                        {
                            if (it.IsSensitive == true && it.IsSensitiveMRP == true)
                            {
                                it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM;
                            }
                            else if (it.IsSensitive == true && it.IsSensitiveMRP == false)
                            {
                                it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name 
                            }

                            else if (it.IsSensitive == false && it.IsSensitiveMRP == false)
                            {
                                it.itemname = it.HindiName; //item display name
                            }
                            else if (it.IsSensitive == false && it.IsSensitiveMRP == true)
                            {
                                it.itemname = it.HindiName + " " + it.price + " MRP";//item display name 
                            }
                        }
                    }
                }

                item.ItemMasters = new List<ItemDataDC>();
                item.ItemMasters.AddRange(newdata);

                ItemListDc res = new ItemListDc();
                if (item.ItemMasters != null && item.ItemMasters.Any())
                {
                    res.Status = true;
                    res.Message = "Success";
                    res.ItemMasters = item.ItemMasters;
                    return res;
                }
                else
                {
                    res.Status = false;
                    res.Message = "Failed";
                    return res;
                }
            }
        }
        private List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> GetCatSubCatwithStores(int peopleid)
        {
            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> results = new List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand>();
            using (var context = new AuthContext())
            {
                var query = string.Format("exec IsSalesAppLead {0}", peopleid);
                var isSalesLead = context.Database.SqlQuery<int>(query).FirstOrDefault();
                List<long> storeids = new List<long>();
                if (isSalesLead > 0)
                    storeids = context.StoreDB.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.Id).ToList();
                else
                {
                    storeids = context.ClusterStoreExecutives.Where(x => x.ExecutiveId == peopleid && x.IsDeleted == false && x.IsActive).Select(x => x.StoreId).Distinct().ToList();
                    var universalStoreIds = context.StoreDB.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.IsUniversal).Select(x => x.Id).ToList();
                    if (universalStoreIds != null && universalStoreIds.Any())
                        storeids.AddRange(universalStoreIds);
                }
                RetailerAppManager retailerAppManager = new RetailerAppManager();
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = retailerAppManager.GetStoreWithDetail();

                results = StoreCategorySubCategoryBrands.Where(x => storeids.Contains(x.StoreId)).ToList();
            }
            return results;
        }
        #endregion



        [Route("UpdateProfileImage")]
        [HttpPost]
        public async Task<bool> UpdateProfileImage(UpdateSalesManProfileImageDc obj)
        {
            bool result = false;
            if (obj != null && obj.PeopleId > 0 && obj.ProfilePic != null)
            {
                using (var db = new AuthContext())
                {
                    var person = await db.Peoples.Where(u => u.PeopleID == obj.PeopleId).FirstOrDefaultAsync();
                    person.ProfilePic = obj.ProfilePic;
                    person.UpdatedDate = DateTime.Now;
                    db.Entry(person).State = EntityState.Modified;
                    result = db.Commit() > 0;
                }
            }
            return result;
        }

        [Route("GetallNotification")]

        [HttpGet]
        public PaggingDatas GetallNotification(int skip, int take, int PeopleId)
        {
            int ApproveTimeLeft = Convert.ToInt32(ConfigurationManager.AppSettings["ApproveNotifyTimeLeftInMinute"]);

            using (var context = new AuthContext())
            {
                DateTime dt1 = DateTime.Now;
                PaggingDatas data = new PaggingDatas();
                context.Database.CommandTimeout = 600;
                SalesAppManager manager = new SalesAppManager();
                skip = (take - 1) * skip;
                var PeopleSentNotificationDc = manager.GetPeopleSentNotificationDetail(skip, take, PeopleId);
                //    var query = "[Operation].[GetPeopleNotification] " + PeopleId.ToString() + "," + ((take - 1) * skip).ToString() + "," + take;
                //    var PeopleSentNotificationDc = context.Database.SqlQuery<PeopleSentNotificationDc>(query).ToList();
                PeopleSentNotificationDc.ForEach(x =>
                {
                    x.TimeLeft = x.TimeLeft.AddMinutes(ApproveTimeLeft); // from Create date

                    if (!string.IsNullOrEmpty(x.Shopimage) && !x.Shopimage.Contains("http"))
                    {
                        x.Shopimage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                              , HttpContext.Current.Request.Url.DnsSafeHost
                                                              , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                              , x.Shopimage);
                    }
                });
                data.notificationmaster = PeopleSentNotificationDc;
                data.total_count = PeopleSentNotificationDc != null && PeopleSentNotificationDc.Any() ? PeopleSentNotificationDc.FirstOrDefault().TotalCount : 0;
                return data;
            }
        }

        [Route("NotificationApprove")]
        [HttpGet]
        public async Task<bool> NotificationApprove(int Id, int PeopleId, bool IsNotificationApproved)
        {

            using (var context = new AuthContext())
            {
                ConfigureNotifyHelper helepr = new ConfigureNotifyHelper();
                bool IsUpdate = await helepr.IsNotificationApproved(Id, PeopleId, IsNotificationApproved, context);

                //Action CallList pending 
                return IsUpdate;
            }
        }


        [Route("NotApprovedNotification")]
        [HttpGet]
        public async Task<List<PeopleSentNotificationDc>> NotApprovedNotification(int PeopleId)
        {
            int ApproveTimeLeft = Convert.ToInt32(ConfigurationManager.AppSettings["ApproveNotifyTimeLeftInMinute"]);

            DateTime dt1 = DateTime.Now;
            using (var context = new AuthContext())
            {
                //var param = new SqlParameter("PeopleId", PeopleId);
                //var NotApprovedList = await context.Database.SqlQuery<PeopleSentNotificationDc>("exec Operation.NotApprovedNotification @PeopleId", param).ToListAsync();
                SalesAppManager manager = new SalesAppManager();
                var NotApprovedList = manager.NotApprovedNotificationManager(PeopleId);
                NotApprovedList.ForEach(x =>
                {
                    x.TimeLeft = x.TimeLeft.AddMinutes(ApproveTimeLeft); // from Create date
                    if (!string.IsNullOrEmpty(x.Shopimage) && !x.Shopimage.Contains("http"))
                    {
                        x.Shopimage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                              , HttpContext.Current.Request.Url.DnsSafeHost
                                                              , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                              , x.Shopimage);
                    }
                });
                return NotApprovedList;
            }
        }


        private string GenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)
        {
            using (AuthContext db = new AuthContext())
            {
                string sOTP = String.Empty;
                string sTempChars = String.Empty;
                Random rand = new Random();

                for (int i = 0; i < iOTPLength; i++)
                {
                    int p = rand.Next(0, saAllowedCharacters.Length);
                    sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
                    sOTP += sTempChars;
                }
                return sOTP;
            }
        }
        [Route("Genotp")]
        [HttpGet]
        [AllowAnonymous]

        public OTP Getotp(string MobileNumber, bool type, string mode = "")
        {
            string Apphash = "";
            bool TestUser = false;
            OTP b = new OTP();
            List<string> CustomerStatus = new List<string>();
            CustomerStatus.Add("Not Verified");
            CustomerStatus.Add("Pending For Submitted");
            CustomerStatus.Add("Pending For Activation");
            CustomerStatus.Add("Temporary Active");
            using (var context = new AuthContext())
            {
                if (!type)
                {
                    Customer cust = context.Customers.Where(c => c.Mobile.Trim().Equals(MobileNumber.Trim()) && !c.Deleted).FirstOrDefault();
                    if (cust != null)
                    {
                        TestUser = cust.CustomerCategoryId.HasValue && cust.CustomerCategoryId.Value == 0;
                        b = new OTP()
                        {
                            Status = false,
                            Message = "This mobile no already registered with " + cust.CustomerVerify + " Status.",
                            CustomerId = cust.CustomerId,
                            SkCode = cust.Skcode,
                            CanUpdateCustomer = (cust.CustomerVerify == "Full Verified" || cust.CustomerVerify == "Pending For Submitted" || cust.CustomerVerify == "Partial Verified") ? false : true
                        };
                        return b;
                    }
                }
            }
            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);
            // string OtpMessage = " is Your login Code. :). ShopKirana";
            string OtpMessage = ""; //"{#var1#} is Your login Code. {#var2#}. ShopKirana";
            var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Login_Code");
            OtpMessage = dltSMS == null ? "" : dltSMS.Template;
            OtpMessage = OtpMessage.Replace("{#var1#}", sRandomOTP);
            OtpMessage = OtpMessage.Replace("{#var2#}", ":)");


            if (string.IsNullOrEmpty(Apphash))
            {
                Apphash = ConfigurationManager.AppSettings["Apphash"];
            }

            //string OtpMessage = string.Format("<#> {0} : is Your Shopkirana Verification Code for complete process.{1}{2} Shopkirana", sRandomOTP, Environment.NewLine, Apphash);
            //string message = sRandomOTP + " :" + OtpMessage;
            // string message = OtpMessage;
            var status = dltSMS == null ? false : Common.Helpers.SendSMSHelper.SendSMS(MobileNumber, OtpMessage, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);
            //if (status)
            //{
            MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP> mongoDbHelper = new MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP>();
            Model.CustomerOTP.RetailerCustomerOTP CustomerOTP = new Model.CustomerOTP.RetailerCustomerOTP
            {
                CreatedDate = DateTime.Now,
                DeviceId = "",
                IsActive = true,
                Mobile = MobileNumber,
                Otp = sRandomOTP
            };
            mongoDbHelper.Insert(CustomerOTP);
            //}


            OTP a = new OTP()
            {
                OtpNo = TestUser || (!string.IsNullOrEmpty(mode) && mode == "debug") ? sRandomOTP : "",
                Status = true,
                Message = "Successfully sent OTP."
            };
            return a;
        }



        [Route("CheckOTP")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> CheckOTP(SalesCustomerRegistor otpCheckDc)
        {
            MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP> mongoDbHelper = new MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP>();
            var cartPredicate = PredicateBuilder.New<Model.CustomerOTP.RetailerCustomerOTP>(x => x.Mobile == otpCheckDc.MobileNumber);

            var CustomerOTPs = mongoDbHelper.Select(cartPredicate).ToList();
            if (CustomerOTPs != null && CustomerOTPs.Any(x => x.Otp == otpCheckDc.Otp))
            {
                foreach (var item in CustomerOTPs)
                {
                    await mongoDbHelper.DeleteAsync(item.Id);
                }

                using (var context = new AuthContext())
                {
                    People people = context.Peoples.Where(q => q.PeopleID == otpCheckDc.PeopleId).FirstOrDefault();
                    var cust = context.Customers.Where(x => x.Deleted == false && x.Mobile == otpCheckDc.MobileNumber).FirstOrDefault();
                    Cluster dd = null;
                    if (cust != null)
                    {
                        cust.Skcode = skcode();
                        cust.ShopName = otpCheckDc.ShopName;
                        cust.Shopimage = otpCheckDc.Shopimage;
                        cust.Mobile = otpCheckDc.MobileNumber;
                        cust.Active = false;
                        cust.Deleted = false;
                        cust.CreatedBy = people.DisplayName;
                        cust.CreatedDate = indianTime;
                        cust.lat = otpCheckDc.lat;
                        cust.lg = otpCheckDc.lg;
                        cust.Shoplat = otpCheckDc.lat;
                        cust.Shoplg = otpCheckDc.lg;
                        #region to assign cluster ID and determine if it is in cluster or not.

                        if (cust.lat != 0 && cust.lg != 0)
                        {
                            var query = new StringBuilder("select [dbo].[GetClusterFromLatLng]('").Append(cust.lat).Append("', '").Append(cust.lg).Append("')");
                            var clusterId = context.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
                            if (!clusterId.HasValue)
                            {
                                cust.InRegion = false;
                            }
                            else
                            {
                                var agent = context.ClusterAgent.FirstOrDefault(x => x.ClusterId == clusterId && x.active);

                                if (agent != null && agent.AgentId > 0)
                                    cust.AgentCode = Convert.ToString(agent.AgentId);


                                cust.ClusterId = clusterId;
                                dd = context.Clusters.Where(x => x.ClusterId == clusterId).FirstOrDefault();
                                cust.ClusterName = dd.ClusterName;
                                cust.InRegion = true;
                            }
                        }
                        #endregion

                        if (dd != null)
                        {
                            cust.Warehouseid = dd.WarehouseId;
                            cust.WarehouseName = dd.WarehouseName;
                            cust.ClusterId = dd.ClusterId;
                            cust.ClusterName = dd.ClusterName;
                            cust.Cityid = dd.CityId;
                            cust.City = dd.CityName;
                            cust.ShippingCity = dd.CityName;
                            cust.IsCityVerified = true;
                        }
                        context.Entry(cust).State = EntityState.Modified;
                        context.Commit();
                    }
                    else
                    {
                        cust = new Customer();
                        cust.Skcode = skcode();
                        cust.ShopName = otpCheckDc.ShopName;
                        cust.Shopimage = otpCheckDc.Shopimage;
                        cust.Mobile = otpCheckDc.MobileNumber;
                        cust.Active = false;
                        cust.Deleted = false;
                        cust.CreatedBy = people.DisplayName;
                        cust.CreatedDate = indianTime;
                        cust.UpdatedDate = indianTime;
                        cust.lat = otpCheckDc.lat;
                        cust.lg = otpCheckDc.lg;
                        cust.Shoplat = otpCheckDc.lat;
                        cust.Shoplg = otpCheckDc.lg;
                        cust.CompanyId = 1;
                        #region to assign cluster ID and determine if it is in cluster or not.

                        if (cust.lat != 0 && cust.lg != 0)
                        {
                            var query = new StringBuilder("select [dbo].[GetClusterFromLatLng]('").Append(cust.lat).Append("', '").Append(cust.lg).Append("')");
                            var clusterId = context.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
                            if (!clusterId.HasValue)
                            {
                                cust.InRegion = false;
                            }
                            else
                            {
                                var agent = context.ClusterAgent.FirstOrDefault(x => x.ClusterId == clusterId && x.active);

                                if (agent != null && agent.AgentId > 0)
                                    cust.AgentCode = Convert.ToString(agent.AgentId);


                                cust.ClusterId = clusterId;
                                dd = context.Clusters.Where(x => x.ClusterId == clusterId).FirstOrDefault();
                                cust.ClusterName = dd.ClusterName;
                                cust.InRegion = true;
                            }
                        }
                        #endregion

                        if (dd != null)
                        {
                            cust.Warehouseid = dd.WarehouseId;
                            cust.WarehouseName = dd.WarehouseName;
                            cust.ClusterId = dd.ClusterId;
                            cust.ClusterName = dd.ClusterName;

                            cust.Cityid = dd.CityId;
                            cust.City = dd.CityName;
                            cust.ShippingCity = dd.CityName;
                            cust.IsCityVerified = true;
                        }
                        context.Customers.Add(cust);
                    }
                    context.Commit();

                    var res = new
                    {
                        SkCode = cust.Skcode,
                        Status = true,
                        Message = "OTP Verify Successfully."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
            else
            {
                var res = new
                {
                    SkCode = "",
                    Status = false,
                    Message = "Please enter correct OTP."
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        #region SKCode genrate Function.
        /// <summary>
        /// Created by 19/12/2018 
        /// Get New Skcode function
        /// </summary>
        /// <returns></returns>
        public string skcode()
        {
            using (AuthContext db = new AuthContext())
            {
                var query = "select max(cast(replace(skcode,'SK','') as bigint)) from customers ";
                var intSkCode = db.Database.SqlQuery<long>(query).FirstOrDefault();
                var skcode = "SK" + (intSkCode + 1);
                bool flag = false;
                while (flag == false)
                {
                    var check = db.Customers.Any(s => s.Skcode.Trim().ToLower() == skcode.Trim().ToLower());

                    if (!check)
                    {
                        flag = true;
                        return skcode;
                    }
                    else
                    {
                        intSkCode += 1;
                        skcode = "SK" + intSkCode;
                    }
                }

                return skcode;
            }
        }
        #endregion
        [Route("GetPeopleReferralConfigurations")]
        [HttpGet]
        public List<GetCustReferralConfigDc> GetPeopleReferralConfigurations(int CityId)
        {
            List<GetCustReferralConfigDc> custReferralConfigList = new List<GetCustReferralConfigDc>();
            using (var db = new AuthContext())
            {
                custReferralConfigList = db.CustomerReferralConfigurationDb.Where(x => x.CityId == CityId && x.ReferralType == 2 && x.IsActive == true && x.IsDeleted == false)
                     .Select(x => new GetCustReferralConfigDc
                     {
                         OnOrder = x.OnOrder,
                         ReferralWalletPoint = x.ReferralWalletPoint,
                         CustomerWalletPoint = x.CustomerWalletPoint,
                         OnDeliverd = x.OnDeliverd
                     }).ToList();
                var statusids = custReferralConfigList.Select(x => x.OnDeliverd).Distinct().ToList();
                var customerReferralStatus = db.CustomerReferralStatusDb.Where(x => statusids.Contains((int)x.Id) && x.IsActive == true && x.IsDeleted == false).ToList();
                custReferralConfigList.ForEach(x =>
                {
                    x.OrderCount = x.OnOrder + " Order";
                    x.orderStatus = customerReferralStatus != null ? customerReferralStatus.FirstOrDefault(y => y.Id == x.OnDeliverd).OrderStatus : "NA";
                });
                return custReferralConfigList;
            }
        }
        [Route("GetPeopleReferralOrderList")]
        [HttpGet]
        public List<GetPeopleReferralOrderListDc> GetPeopleReferralOrderList(int PeopleId)
        {
            using (var context = new AuthContext())
            {
                var peopleId = new SqlParameter("@PeopleId", PeopleId);
                List<GetPeopleReferralOrderListDc> PeopleReferralList = context.Database.SqlQuery<GetPeopleReferralOrderListDc>("exec GetPeopleReferralOrderList @PeopleId", peopleId).ToList();
                return PeopleReferralList;
            }
        }


        #region New OTPScreen APIs by kapil

        [Route("GetAllSalesNotifications")]
        [HttpGet]
        public PaggingDatas GetAllSalesNotifications(int skip, int take, int PeopleId, int Requests)
        {
            int ApproveTimeLeft = Convert.ToInt32(ConfigurationManager.AppSettings["ApproveNotifyTimeLeftInMinute"]);

            using (var context = new AuthContext())
            {
                DateTime dt1 = DateTime.Now;
                PaggingDatas data = new PaggingDatas();
                context.Database.CommandTimeout = 600;
                SalesAppManager manager = new SalesAppManager();
                skip = (take - 1) * skip;
                var PeopleSentNotificationDc = manager.GetAllSalesNotificationsDetails(skip, take, PeopleId, Requests);

                OTPDetails result = new OTPDetails();
                var param = new SqlParameter("@PeopleId", PeopleId);
                result = context.Database.SqlQuery<OTPDetails>("Exec SalesOTPLimits  @PeopleId", param).FirstOrDefault();


                if (Requests == 1)
                {
                    PeopleSentNotificationDc.ForEach(x =>
                    {
                        x.TimeLeft = x.TimeLeft.AddMinutes(ApproveTimeLeft);

                        if (!string.IsNullOrEmpty(x.Shopimage) && !x.Shopimage.Contains("http"))
                        {
                            x.Shopimage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                  , HttpContext.Current.Request.Url.DnsSafeHost
                                                                  , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                  , x.Shopimage);
                        }
                    });
                }
                data.OTPDetail = result;
                data.notificationmaster = PeopleSentNotificationDc;
                data.total_count = PeopleSentNotificationDc != null && PeopleSentNotificationDc.Any() ? PeopleSentNotificationDc.FirstOrDefault().TotalCount : 0;
                return data;
            }
        }


        [Route("InsertMonthlySalesOTPUsed")]
        [HttpGet]
        public bool InsertMonthlySalesOTPUsed()
        {
            bool result = false;
            DateTime TodayDate = DateTime.Now.Date;
            DateTime SatrtDate = new DateTime(TodayDate.Year, TodayDate.Month, 1);
            DateTime LastDateOfMOnth = SatrtDate.AddMonths(1).AddDays(-1);

            if (TodayDate == LastDateOfMOnth)
            {
                using (var context = new AuthContext())
                {

                    List<SalesPersonOTPDetails> salesList = new List<SalesPersonOTPDetails>();

                    var SalesList = context.Database.SqlQuery<InsertMonthlySalesOTPUsedDC>("Exec InsertMonthlySalesOTPUsed").ToList();
                    if (SalesList != null && SalesList.Any())
                    {
                        foreach (var item in SalesList)
                        {
                            SalesPersonOTPDetails sales = new SalesPersonOTPDetails();

                            string sDate = TodayDate.AddDays(1).AddMonths(-4).ToString("yyyy-MM-dd");
                            string eDate = TodayDate.ToString("yyyy-MM-dd");
                            string Query = $" select  orderid from { platformIdxName} where executiveid = '{item.UserId}' and createddate >= '{sDate}' and createddate <= '{eDate}' and status in ('sattled', 'Delivered')  group by orderid"; //and isdigitalorder = '{item.IsDigital}' 
                            //ElasticSqlHelper<int> elasticSqlHelperData = new ElasticSqlHelper<int>();
                            //var Limit = AsyncContext.Run(async () => (await elasticSqlHelperData.GetListAsync(Query)).FirstOrDefault());
                            ElasticHelper elasticHelper = new ElasticHelper();
                            var OrderMasters = elasticHelper.GetList(Query);
                            int Limit = 0;
                            if (OrderMasters != null && OrderMasters.Count > 0)
                            {
                                Limit = ((OrderMasters.Count() / 3) * 20) / 100;
                            }
                            sales.SalesExecutiveId = item.UserId;
                            sales.OTPLimit = Limit > 0 ? Limit : 20;
                            sales.OTPUsed = item.UsedOTP;
                            sales.Fine = (sales.OTPLimit - item.UsedOTP) > 0 ? 0 : (sales.OTPLimit - item.UsedOTP) * (-50);
                            sales.Month = DateTime.Now.Month;
                            sales.Year = DateTime.Now.Year;
                            sales.CreatedDate = DateTime.Now.Date;
                            sales.CreatedBy = 0;
                            sales.ModifiedDate = null;
                            sales.ModifiedBy = 0;
                            sales.IsActive = true;
                            sales.IsDeleted = false;

                            salesList.Add(sales);
                        }
                        if (salesList.Count() > 0)
                        {
                            context.SalesPersonOTPDetails.AddRange(salesList);
                            context.Commit();
                            return result = true;
                        }
                        else
                        {
                            return result = false;
                        }
                    }
                }
            }
            return result;
        }


        #endregion
    }
}



