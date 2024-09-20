
using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using MongoDB.Bson;

namespace AngularJSAuthentication.API.Controllers.CustomizedOrder
{
    [RoutePrefix("api/CustomizedPrepaidOrder")]
    public class CustomizedPrepaidOrdersController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);


        [Route("GetCustomizedPrepaidOrder")]
        [AllowAnonymous]
        [HttpGet]
        public List<CustomizedPrepaidOrdersDC> GetCustDetails()
        {
            using (AuthContext context = new AuthContext())
            {
                List<CustomizedPrepaidOrdersDC> CustomizedPrepaidOrderss = new List<CustomizedPrepaidOrdersDC>();
                var CustPrepaidOrder = context.CustomizedPrepaidOrders.ToList();
                foreach (var item in CustPrepaidOrder)
                {
                    var warehousename = context.Warehouses.Where(x => x.WarehouseId == item.warehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                    var CustomizedPrepaidOrder = new CustomizedPrepaidOrdersDC();
                    CustomizedPrepaidOrder.id = item.Id;
                    CustomizedPrepaidOrder.warehouseid = item.warehouseId;
                    CustomizedPrepaidOrder.Warehousename = warehousename;
                    CustomizedPrepaidOrder.OrderAmount = item.OrderAmount;
                    CustomizedPrepaidOrder.OnlineParcentage = item.OnlineParcentage;
                    CustomizedPrepaidOrder.CashParcentage = item.CashParcentage;
                    CustomizedPrepaidOrder.IsDelete = item.IsDeleted;
                    CustomizedPrepaidOrder.IsActive = item.IsActive;
                    CustomizedPrepaidOrderss.Add(CustomizedPrepaidOrder);
                }
                return CustomizedPrepaidOrderss;
            }

        }

        [Route("AddCustomizedPrepaidOrders")]
        [HttpPost]
        public CustomizedPrepaidOrderDc AddCustomizedPrepaidOrders(CustomizedPrepaidOrderDc CustomizedPrepaidOrderDc)
        {
            List<CustomizedPrepaidOrders> CustomizedPrepaidOrder = new List<CustomizedPrepaidOrders>();

            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                CustomizedPrepaidOrders Cust = context.CustomizedPrepaidOrders.Where(x => x.warehouseId == CustomizedPrepaidOrderDc.warehouseId && x.Id == CustomizedPrepaidOrderDc.Id).FirstOrDefault();

                if (Cust != null)
                {
                    Cust.OrderId = CustomizedPrepaidOrderDc.OrderId;
                    Cust.OrderAmount = CustomizedPrepaidOrderDc.OrderAmount;
                    Cust.OnlineParcentage = CustomizedPrepaidOrderDc.OnlineParcentage;
                    Cust.CashParcentage = CustomizedPrepaidOrderDc.CashParcentage;
                    Cust.CreatedBy = userid;
                    Cust.CreatedDate = indianTime;
                    Cust.IsActive = CustomizedPrepaidOrderDc.IsActive;
                    Cust.IsDeleted = CustomizedPrepaidOrderDc.IsDeleted.HasValue ? CustomizedPrepaidOrderDc.IsDeleted.Value : false;
                    Cust.warehouseId = CustomizedPrepaidOrderDc.warehouseId;
                    context.Entry(Cust).State = EntityState.Modified;

                }
                else
                {
                    CustomizedPrepaidOrders custorder = new CustomizedPrepaidOrders();
                    custorder.OrderId = CustomizedPrepaidOrderDc.OrderId;
                    custorder.OrderAmount = CustomizedPrepaidOrderDc.OrderAmount;
                    custorder.OnlineParcentage = CustomizedPrepaidOrderDc.OnlineParcentage;
                    custorder.CashParcentage = CustomizedPrepaidOrderDc.CashParcentage;
                    custorder.CreatedBy = userid;
                    custorder.CreatedDate = indianTime;
                    custorder.IsActive = true;
                    custorder.IsDeleted = false;
                    custorder.warehouseId = CustomizedPrepaidOrderDc.warehouseId;
                    context.CustomizedPrepaidOrders.Add(custorder);
                }
                context.Commit();
                return CustomizedPrepaidOrderDc;
            }
        }

        [Route("ADDCustomizedOrder")]
        [AllowAnonymous]
        [HttpPost]
        public CustomizedPrepaidOrders ADDCustomizedOrder(CustomizedPrepaidOrderDc customizedPrepaidOrderDc)
        {
            using (AuthContext context = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var CustPrepaidOrder = context.CustomizedPrepaidOrders.Where(x => x.warehouseId == customizedPrepaidOrderDc.warehouseId).FirstOrDefault();
                if (CustPrepaidOrder == null)
                {
                    CustomizedPrepaidOrders custorder = new CustomizedPrepaidOrders();
                    custorder.OrderId = customizedPrepaidOrderDc.OrderId;
                    custorder.OrderAmount = customizedPrepaidOrderDc.OrderAmount;
                    custorder.OnlineParcentage = customizedPrepaidOrderDc.OnlineParcentage;
                    custorder.CashParcentage = customizedPrepaidOrderDc.CashParcentage;
                    custorder.CreatedBy = userid;
                    custorder.CreatedDate = indianTime;
                    custorder.IsActive = true;
                    custorder.IsDeleted = false;
                    custorder.warehouseId = customizedPrepaidOrderDc.warehouseId;
                    context.CustomizedPrepaidOrders.Add(custorder);
                }
                else
                {
                    return null;
                }
                context.Commit();
                return CustPrepaidOrder;
            }
        }


        [Route("GetCustomizedOrderDelete")]
        [AllowAnonymous]
        [HttpPost]
        public CustomizedPrepaidOrders GetCustomizedOrderDelete(CustomizedPrepaidOrderActiveDc CustomizedPrepaidOrderActiveDc)
        {
            using (AuthContext context = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var CustPrepaidOrder = context.CustomizedPrepaidOrders.Where(x => x.Id == CustomizedPrepaidOrderActiveDc.Id && x.IsDeleted == false && x.IsActive == true).FirstOrDefault();

                CustPrepaidOrder.IsActive = false;
                CustPrepaidOrder.IsDeleted = CustomizedPrepaidOrderActiveDc.IsDeleted;
                CustPrepaidOrder.ModifiedDate = indianTime;
                CustPrepaidOrder.ModifiedBy = userid;
                context.Entry(CustPrepaidOrder).State = EntityState.Modified;
                context.Commit();
                return CustPrepaidOrder;

            }

        }

        [Route("GetCompanyWheelConfigurations")]
        [HttpGet]
        public List<CompanyWheelConfiguration> GetCompanyWheelConfigurations()
        {
            using (AuthContext context = new AuthContext())
            {
                var getwheel = context.CompanyWheelConfiguration.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                return getwheel;
            }
        }
        [Route("updateCompanyWheelConfigurations")]
        [HttpPost]
        public bool updateCompanyWheelConfigurations(CompanyWheelConfiguration CompanyWheelConfigurations)
        {
            using (AuthContext context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var updatewheel = context.CompanyWheelConfiguration.Where(x => x.IsActive == true && x.IsDeleted == false && x.IsStore == CompanyWheelConfigurations.IsStore).FirstOrDefault();
                if (updatewheel != null)
                {
                    updatewheel.OrderAmount = CompanyWheelConfigurations.OrderAmount;
                    updatewheel.LineItemCount = CompanyWheelConfigurations.LineItemCount;
                    updatewheel.ModifiedBy = userid;
                    updatewheel.ModifiedDate = indianTime;
                    context.Entry(updatewheel).State = EntityState.Modified;
                }
                else
                {
                    return false;
                }
                context.Commit();
                return true;
            }
        }

        [Route("updateWheelPointWeightPercentConfigs")]
        [HttpPost]
        public bool updateWheelPointWeightPercentConfigs(WheelPointWeightPercentConfig WheelPointWeightPercentConfigs)
        {
            using (AuthContext context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var updateWheelPoint = context.WheelPointWeightPercentConfig.Where(x => x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (updateWheelPoint != null)
                {
                    updateWheelPoint.WheelPoint = WheelPointWeightPercentConfigs.WheelPoint;
                    updateWheelPoint.WheelWeightPercent = WheelPointWeightPercentConfigs.WheelWeightPercent;
                    updateWheelPoint.ModifiedBy = userid;
                    updateWheelPoint.ModifiedDate = indianTime;
                    context.Entry(updateWheelPoint).State = EntityState.Modified;
                }
                else
                {
                    return false;
                }
                context.Commit();
                return true;
            }
        }
        [Route("AddStoreMinOrder")]
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult AddStoreMinOrder(StoreMinOrder obj)
        {
            bool flag = false;
            MongoDbHelper<StoreMinOrder> mHelperStore = new MongoDbHelper<StoreMinOrder>();
            {
                var storeMinOrder = mHelperStore.Select(x => x.StoreId == obj.StoreId && x.WarehouseId == obj.WarehouseId && x.CityId == obj.CityId).FirstOrDefault();

                if (storeMinOrder != null)
                {
                    storeMinOrder.StoreId = obj.StoreId;
                    storeMinOrder.WarehouseId = obj.WarehouseId;
                    storeMinOrder.CityId = obj.CityId;
                    storeMinOrder.MinOrderValue = obj.MinOrderValue;
                    storeMinOrder.MinLineItem = obj.MinLineItem;
                    flag = mHelperStore.Replace(storeMinOrder.Id, storeMinOrder);
                }
                else
                {
                    StoreMinOrder data = new StoreMinOrder
                    {
                        StoreId = obj.StoreId,
                        CityId = obj.CityId,
                        WarehouseId = obj.WarehouseId,
                        MinOrderValue = obj.MinOrderValue,
                        MinLineItem = obj.MinLineItem
                    };
                    flag = mHelperStore.Insert(data);
                }

                if (flag)
                {
                    return Ok(true);
                }
                return Ok();
            }

        }
        [HttpGet]
        [Route("GetStoreMinOrderList")]
        [AllowAnonymous]
        public dynamic StoreMinOrderList()
        {
            List<StoreMinOrder> StoreMinOrderlist = new List<StoreMinOrder>();
            MongoDbHelper<StoreMinOrder> mHelperStore = new MongoDbHelper<StoreMinOrder>();

            var searchPredicate = PredicateBuilder.New<StoreMinOrder>(x => x.StoreId != 0);
            StoreMinOrderlist = mHelperStore.Select(searchPredicate, x => x.OrderByDescending(y => y.StoreId), null, null).ToList();

            List<StoreMinOrderDc> MinOrderlist = new List<StoreMinOrderDc>();
            MinOrderlist = Mapper.Map(StoreMinOrderlist).ToANew<List<StoreMinOrderDc>>();
            using (var context = new AuthContext())
            {
                var cities = context.Cities.Where(x => x.active);
                var warehoues = context.Warehouses.Where(x => !x.IsKPP);
                var stories = context.StoreDB.Where(x => x.IsActive);
                foreach (var item in MinOrderlist)
                {
                    item.StoreName = stories.FirstOrDefault(x => x.Id == item.StoreId).Name;
                    item.CityName = item.CityId == 0 ? "NA" : cities.FirstOrDefault(x => x.Cityid == item.CityId).CityName;
                    item.WarehouseName = item.WarehouseId == 0 ? "NA" : warehoues.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).WarehouseName;
                }
            }
            var list = MinOrderlist.OrderBy(x => x.CityName).ToList();
            return list;
        }
        [HttpPost]
        [Route("UpdateStoreMinOrderList")]
        [AllowAnonymous]

        public string UpdateMinorderList(UpdateStoreMinOrder updateobj)
        {
            string result = "";


            MongoDbHelper<StoreMinOrder> mHelperStore = new MongoDbHelper<StoreMinOrder>();
            {
                var storeMinOrder = mHelperStore.Select(x => x.Id == ObjectId.Parse(updateobj.Id)).FirstOrDefault();
                if (storeMinOrder == null)
                {
                    result = "Data not found";
                }
                else
                {
                    storeMinOrder.MinOrderValue = updateobj.MinOrderValue;
                    storeMinOrder.MinLineItem = updateobj.MinLineItem;
                    mHelperStore.Replace(storeMinOrder.Id, storeMinOrder);
                    result = "Data Updated Successfully";
                }
            }

            return result;
        }




        [Route("GetAllStore")]
        [HttpGet]
        public dynamic GetAllStores()
        {
            using (var context = new AuthContext())
            {
                var results = context.StoreDB.Where(x => x.IsDeleted == false && x.IsActive == true).ToList();
                return results;
            }
        }
    }

}

public class CustomizedPrepaidOrdersDC
{
    public long id { get; set; }
    public int warehouseid { get; set; }
    public string Warehousename { get; set; }
    public decimal OrderAmount { get; set; }
    public decimal OnlineParcentage { get; set; }
    public decimal CashParcentage { get; set; }
    public bool? IsDelete { get; set; }
    public bool IsActive { get; set; }

}

public class CustomizedPrepaidOrderDc
{
    public long Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public int CreatedBy { get; set; }
    public int? ModifiedBy { get; set; }
    public int warehouseId { get; set; }
    public int? OrderId { get; set; }
    public decimal OrderAmount { get; set; }
    public decimal OnlineParcentage { get; set; }
    public decimal CashParcentage { get; set; }

}
public class CustomizedPrepaidOrderActiveDc
{
    public long Id { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public int? ModifiedBy { get; set; }


}
