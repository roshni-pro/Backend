using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using AngularJSAuthentication.Model.Base;
using LinqKit;
using MongoDB.Bson;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/inventory")]
    public class InventoryReportController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("GetInventoryReport")]
        [HttpGet]
        public dynamic GetInventoryReport(int warehouseId, DateTime from, DateTime to)
        {

            using (var con = new AuthContext())
            {
                var param = new SqlParameter("param", warehouseId);
                var param1 = new SqlParameter("param1", from);
                var param2 = new SqlParameter("param2", to);

                var ordHistories = con.Database.SqlQuery<InventoryData>("exec DateWiseInOutStock @param1,@param2,@param", param1, param2, param).ToList();

                return Ok(ordHistories);
            }
        }


        [Route("GetFlashDealReport")]
        [HttpGet]
        public dynamic GetFlashDealReport(int warehouseId, DateTime from, DateTime to)
        {

            using (var con = new AuthContext())
            {
                try
                {
                   
                    var param = new SqlParameter("warehouseId", warehouseId);
                    var param1 = new SqlParameter("startDate", from);
                    var param2 = new SqlParameter("endDate", to);
                     var FlashDealReport = new FlashDealReport();
                    FlashDealReport.FlashDeal = con.Database.SqlQuery<FlashDealData>("exec DateWHWiseFlashDeal @startDate,@endDate,@warehouseId", param1, param2, param).OrderBy(x => x.OrderId).ToList();
                    FlashDealReport.TotalOrders = FlashDealReport.FlashDeal.Select(grp => grp.OrderId).Distinct().Count();
                    FlashDealReport.TotalAmount = FlashDealReport.FlashDeal.Select(grp => grp.TotalAmt).Distinct().Sum();

                    return Ok(FlashDealReport);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        [Route("GetFlashDealReportCity")]
        [HttpGet]
        public dynamic GetFlashDealReportCity(int cityId, DateTime from, DateTime to)
        {

            using (var con = new AuthContext())
            {
                try
                {
                    var param = new SqlParameter("cityId", cityId);
                    var param1 = new SqlParameter("startDate", from);
                    var param2 = new SqlParameter("endDate", to);

                    //var FlashDeal = con.Database.SqlQuery<FlashDealData>("exec DateCityWiseFlashDeal @startDate,@endDate,@cityId", param1, param2, param).OrderBy(x => x.OrderId).ToList();

                    var FlashDealReport = new FlashDealReport();
                    FlashDealReport.FlashDeal = con.Database.SqlQuery<FlashDealData>("exec DateCityWiseFlashDeal @startDate,@endDate,@cityId", param1, param2, param).OrderBy(x => x.OrderId).ToList();
                    FlashDealReport.TotalOrders = FlashDealReport.FlashDeal.Select(grp => grp.OrderId).Distinct().Count();
                    FlashDealReport.TotalAmount = FlashDealReport.FlashDeal.Select(grp => grp.TotalAmt).Sum();

                    return Ok(FlashDealReport);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        [Route("GetFlashDealReportSKCode")]
        [HttpGet]
        public dynamic GetFlashDealReportSKCode(string skCode)
        {

            using (var con = new AuthContext())
            {
                try
                {
                    var param = new SqlParameter("skCode", skCode);
                    //var param1 = new SqlParameter("startDate", from);
                    //var param2 = new SqlParameter("endDate", to);

                    //var FlashDeal = con.Database.SqlQuery<FlashDealData>("exec SKCodeWiseFlashDeal @skCode", param).OrderBy(x => x.OrderId).ToList();

                    var FlashDealReport = new FlashDealReport();
                    FlashDealReport.FlashDeal = con.Database.SqlQuery<FlashDealData>("exec SKCodeWiseFlashDeal @skCode", param).OrderBy(x => x.OrderId).ToList();
                    FlashDealReport.TotalOrders = FlashDealReport.FlashDeal.Select(grp => grp.OrderId).Distinct().Count();
                    FlashDealReport.TotalAmount = FlashDealReport.FlashDeal.Select(grp => grp.TotalAmt).Distinct().Sum();

                    return Ok(FlashDealReport);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        [Route("GetZone")]
        [HttpGet]
        public dynamic GetZone()
        {

            using (var con = new AuthContext())
            {
                try
                {
                    var Zone = con.Database.SqlQuery<Zonedto>("exec GetZone").ToList();

                    return Ok(Zone);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        [Route("GetRegion")]
        [HttpGet]
        public dynamic GetRegion(int zoneid)
        {

            using (var con = new AuthContext())
            {
                try
                {
                    var param = new SqlParameter("zoneid", zoneid);

                    var Region = con.Database.SqlQuery<Regiondto>("exec GetRegion @zoneid", param).ToList();

                    return Ok(Region);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        [Route("GetRegionAll")]
        [HttpGet]
        public dynamic GetRegion()
        {
            using (var con = new AuthContext())
            {
                var allRegion = con.Regions.Where(x => x.IsActive == true && x.Deleted == false).Select(x => new Regiondto { RegionId = x.RegionId, RegionName = x.RegionName }).ToList();
                return Ok(allRegion);
            }
        }

        [Route("GetCity")]
        [HttpGet]
        public dynamic GetCity(int regionId)
        {
            using (var con = new AuthContext())
            {
                try
                {
                    var param = new SqlParameter("regionid", regionId);

                    var Warehouse = con.Database.SqlQuery<Citydto>("exec GetCity @regionid", param).ToList();

                    return Ok(Warehouse);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }


        [Route("GetWarehouse")]
        [HttpGet]
        public dynamic GetWarehouse(int regionId)
        {

            using (var con = new AuthContext())
            {
                try
                {
                    var param = new SqlParameter("regionid", regionId);

                    var Warehouse = con.Database.SqlQuery<Warehousedto>("exec GetWarehouse @regionid", param).ToList();

                    return Ok(Warehouse);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        // Added by Anoop on 15/2/2021

        //[Route("PostWarehouseData")]
        //[HttpPost]
        //public IEnumerable<Warehousedto> PostWarehouseData(int regionId)
        //{

        //    using (var con = new AuthContext())
        //    {
        //        try
        //        {
        //            var param = new SqlParameter("regionid", regionId);

        //            var Warehouse = con.Database.SqlQuery<Warehousedto>("exec GetWarehouse @regionid", param).ToList();

        //            return Warehouse;
        //        }
        //        catch (Exception ex)
        //        {
        //            throw;
        //        }
        //    }
        //}


        [Route("GetWarehouseCityWise")]
        [HttpGet]
        public dynamic GetWarehouseCityWise(int cityId)
        {

            using (var con = new AuthContext())
            {
                try
                {
                    var param = new SqlParameter("cityid", cityId);

                    var Warehouse = con.Database.SqlQuery<Warehousedto>("exec GetWarehousecitywise @cityid", param).ToList();

                    return Ok(Warehouse);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        [Route("GetCluster")]
        [HttpGet]
        public dynamic GetCluster(int warehouseid)
        {

            using (var con = new AuthContext())
            {
                try
                {
                    var param = new SqlParameter("warehouseid", warehouseid);

                    var Cluster = con.Database.SqlQuery<Clusterdto>("exec GetCluster @warehouseid", param).ToList();

                    return Ok(Cluster);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        [Route("GetCheckerList")]
        [HttpGet]
        public dynamic GetCheckerList1()
        {
            var identity = User.Identity as ClaimsIdentity;
            string Rolename = "";

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                Rolename = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value);

            List<string> roles = new List<string>();
            if (!string.IsNullOrEmpty(Rolename))
            {
                roles = Rolename.Split(',').ToList();
            }

            using (var con = new AuthContext())
            {
                try
                {
                    var CheckerList = con.Database.SqlQuery<Checker>("exec GetMasterCheckerList").ToList();
                    CheckerList = roles.Contains("HQ Master login") ? CheckerList : CheckerList.Where(x => roles.Contains(x.RoleName)).ToList();
                    return Ok(CheckerList);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        [Route("GetCheckerData")]
        [HttpGet]
        public dynamic GetCheckerData(string entityname, string status)
        {

            using (var con = new AuthContext())
            {
                try
                {
                    MongoDbHelper<MakerChecker> mongoDbHelper = new MongoDbHelper<MakerChecker>();

                    var orderPredicate = PredicateBuilder.New<MakerChecker>();
                    orderPredicate = orderPredicate.And(x => x.EntityName == entityname && x.Status == status);
                    List<MakerChecker> makercheckers = mongoDbHelper.Select(orderPredicate, collectionName: "MakerChecker").OrderByDescending(x => x.MakerDate).ToList();
                    var Makers = new List<MakerCheckerDTO>();

                    foreach (var item in makercheckers)
                    {
                        MakerCheckerDTO Maker = new MakerCheckerDTO();
                        Maker.Id = item.Id;
                        Maker.MongoId = item.Id.ToString();
                        Maker.EntityName = item.EntityName;
                        Maker.EntityId = item.EntityId;
                        Maker.Operation = item.Operation;
                        Maker.NewJson = item.NewJson;
                        Maker.OldJson = item.OldJson;
                        Maker.Status = item.Status;
                        Maker.CheckerComment = item.CheckerComment;
                        Maker.MakerBy = item.MakerBy;
                        Maker.CheckerBy = item.CheckerBy;
                        Maker.MakerDate = item.MakerDate;
                        Maker.CheckerDate = item.CheckerDate;
                        Makers.Add(Maker);
                    }

                    return Ok(Makers);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        [Route("MakerOperation")]
        [HttpPost]
        public async Task<bool> MakerOperation(MakerCheckerDTO makerdata)
        {
            try
            {


                bool result = false;
                var identity = User.Identity as ClaimsIdentity;
                string username = "";

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    username = (identity.Claims.FirstOrDefault(x => x.Type == "username").Value);

                MongoDbHelper<MakerChecker> mongoDbHelper = new MongoDbHelper<MakerChecker>();
                ObjectId Id = new ObjectId(makerdata.MongoId);
                var makerCheckerSearchs =(await mongoDbHelper.SelectAsync(x => x.Id == Id && x.EntityName == makerdata.EntityName)).FirstOrDefault();
                if (makerCheckerSearchs != null)
                {
                    if (makerdata.Status == "Approve")
                    {
                        string path = HttpRuntime.AppDomainAppPath;
                        string pathToDomain = Path.Combine(path, @"bin");

                        pathToDomain += "\\" + "AngularJSAuthentication.Model.dll";
                        Assembly domainAssembly = Assembly.LoadFrom(pathToDomain);


                        using (var context = new AuthContext())
                        {
                           
                            //var makerCheckerMasters = GetSetMakerCheckerMastersFromCache().ToList();

                            var makerCheckerMasters = context.GetMakerCheckerMasters();
                            var master = makerCheckerMasters.FirstOrDefault(x => x.EntityName == makerdata.EntityName);
                            Type entityType = domainAssembly.GetType(master.ClassName);

                            var instance = Activator.CreateInstance(entityType);

                            var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(makerdata.NewJson);

                            PropertyInfo[] dbProperties = entityType.GetProperties();                            
                            foreach (var item in values)
                            {                                
                                var property = dbProperties.FirstOrDefault(x => x.Name == item.Key);
                                var isValid = IsValid(property, instance, item.Value);                               
                            }

                            if (makerdata.Operation == "Added")
                            {
                                if (master.EntityName.ToUpper().Equals("SUPPLIER"))
                                {
                                    bool ISExists = IsSupplierExists(makerdata.SupplierCode);

                                    if (ISExists)
                                    {
                                        throw new Exception("This supplier is already exists!!!");
                                    }
                                }
                                if (master.EntityName.ToUpper().Equals("DEPOMASTER"))
                                {
                                    bool ISExists = IsExistsDepo(makerdata.DepoCodes);

                                    if (ISExists)
                                    {
                                        throw new Exception("This Depo is already exists!!!");
                                    }
                                }                                

                                    context.Entry(instance).State = EntityState.Added;
                            }
                            else
                            {
                                context.Entry(instance).State = EntityState.Modified;
                            }

                            await context.CommitAsync(isApproveFromChecker: true);

                            if (master.EntityName.ToUpper().Equals("SUBCATEGORY") && makerdata.Operation == "Added")
                            {
                                if (values.Any(x => x.Key == "SubcategoryName"))
                                {
                                    string subcatroryname = values.FirstOrDefault(x => x.Key == "SubcategoryName").Value.ToString();
                                    var strcatIds = values.FirstOrDefault(x => x.Key == "CategoryIds").Value.ToString();
                                    //var strcatIds= values.FirstOrDefault(x => x.Key == "CategoryName").Value.ToString();
                                    if (!string.IsNullOrEmpty(strcatIds))
                                    {
                                        var catIds= strcatIds.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                                        if (catIds != null && catIds.Any())
                                        {
                                            var subcat = await context.SubCategorys.FirstOrDefaultAsync(x => x.SubcategoryName == subcatroryname && x.IsActive && !x.Deleted);
                                            if (subcat != null)
                                            {
                                                foreach (var Categoryid in catIds)
                                                {
                                                    var mapping = await context.SubcategoryCategoryMappingDb.AnyAsync(x => x.Categoryid == Categoryid && x.SubCategoryId == subcat.SubCategoryId && x.IsActive && !x.Deleted);

                                                    if (!mapping)
                                                    {
                                                        //string a = categoryId.ToString();
                                                        AngularJSAuthentication.Model.SubcategoryCategoryMapping submapp = new AngularJSAuthentication.Model.SubcategoryCategoryMapping();
                                                        submapp.Categoryid = Categoryid;
                                                        submapp.SubCategoryId = subcat.SubCategoryId;
                                                        submapp.CreatedDate = indianTime;
                                                        submapp.UpdatedDate = indianTime;
                                                        submapp.IsActive = true;
                                                        submapp.Deleted = false;
                                                        context.SubcategoryCategoryMappingDb.Add(submapp);
                                                        await context.CommitAsync();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            

                        }

                        makerCheckerSearchs.Status = makerdata.Status;
                        makerCheckerSearchs.CheckerComment = makerdata.CheckerComment;
                        makerCheckerSearchs.CheckerBy = username;
                        makerCheckerSearchs.CheckerDate = indianTime;
                        result = mongoDbHelper.Replace(makerCheckerSearchs.Id, makerCheckerSearchs);

                    }

                    if (makerdata.Status == "Reject")
                    {
                        makerCheckerSearchs.Status = makerdata.Status;
                        makerCheckerSearchs.CheckerComment = makerdata.CheckerComment;
                        makerCheckerSearchs.CheckerBy = username;
                        makerCheckerSearchs.CheckerDate = indianTime;
                        result = mongoDbHelper.Replace(makerCheckerSearchs.Id, makerCheckerSearchs);
                    }
                }

                return result;
            }
            catch (Exception ex)
            
            {
                throw new Exception(ex.Message.ToString());
            }



        }

        public virtual bool IsValid(PropertyInfo dbProperty, object dbTypeInstance, object excelColumnValue)
        {
            try
            {
                var propertyType = dbProperty.PropertyType;

                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    propertyType = (propertyType.GetGenericArguments()[0]);
                }
                switch (Convert.ToString(propertyType.Name).ToLower())
                {
                    case "int16":
                        dbProperty.SetValue(dbTypeInstance, Convert.ToInt16(excelColumnValue), null);
                        return true;
                    case "int32":
                        dbProperty.SetValue(dbTypeInstance, Convert.ToInt32(excelColumnValue), null);
                        return true;
                    case "int64":
                        dbProperty.SetValue(dbTypeInstance, Convert.ToInt64(excelColumnValue), null);
                        return true;
                    case "uint16":
                        dbProperty.SetValue(dbTypeInstance, Convert.ToUInt16(excelColumnValue), null);
                        return true;
                    case "uint32":
                        dbProperty.SetValue(dbTypeInstance, Convert.ToUInt32(excelColumnValue), null);
                        return true;
                    case "uint64":
                        dbProperty.SetValue(dbTypeInstance, Convert.ToUInt64(excelColumnValue), null);
                        return true;
                    case "string":
                        dbProperty.SetValue(dbTypeInstance, Convert.ToString(excelColumnValue), null);
                        return true;
                    case "double":
                        dbProperty.SetValue(dbTypeInstance, Convert.ToDouble(excelColumnValue), null);
                        return true;
                    case "decimal":
                        dbProperty.SetValue(dbTypeInstance, Convert.ToDecimal(excelColumnValue), null);
                        return true;
                    case "datetime":
                        dbProperty.SetValue(dbTypeInstance, excelColumnValue != null ? Convert.ToDateTime(excelColumnValue) : excelColumnValue, null);
                        return true;
                    case "timespan":
                        DateTime dt = Convert.ToDateTime(excelColumnValue);
                        TimeSpan timespanVal = dt.TimeOfDay;
                        //TimeSpan.TryParse(Convert.ToString(excelColumnValue), out timespanVal);
                        dbProperty.SetValue(dbTypeInstance, timespanVal, null);
                        return true;
                    case "boolean":
                        var boolVal = false;
                        bool.TryParse(Convert.ToString(excelColumnValue), out boolVal);
                        dbProperty.SetValue(dbTypeInstance, boolVal, null);
                        return true;
                    case "byte":
                        dbProperty.SetValue(dbTypeInstance, Convert.ToByte(excelColumnValue), null);
                        return true;
                    case "single":
                        dbProperty.SetValue(dbTypeInstance, Convert.ToSingle(excelColumnValue), null);
                        return true;
                    case "char":
                        dbProperty.SetValue(dbTypeInstance, Convert.ToChar(excelColumnValue), null);
                        return true;
                    case "sbyte":
                        dbProperty.SetValue(dbTypeInstance, Convert.ToSByte(excelColumnValue), null);
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpGet]
        [Route("KPPDashboard")]
        [AllowAnonymous]
        public List<KPPDashboard> GetKPPDashboard()//List<int> warehouseids)
        {

            using (AuthContext db = new AuthContext())
            {
                var Field = new List<string>();
                Field.Add("New KPP (in Number)");
                Field.Add("Active KPP (in Number)");
                Field.Add("Sale");
                Field.Add("Kisan Kirana Sale");
                Field.Add("ShopKirana Sale");
                Field.Add("% of Kisan Kirana sale vs Shopkirana");
                Field.Add("Sale/Active Customer (in Number)");
                Field.Add("Order till date (in Number)");
                Field.Add("Order Delivered within 48Hr (in Number)");
                Field.Add("Order Sattled (in Number)");
                Field.Add("KPP > 2 order (in Number)");
                Field.Add("KPP > 2 order (in %)");

                var todaydate = indianTime;
                var yesterdaydate = todaydate.AddDays(-1);
                var lastmonthdate = todaydate.AddMonths(-1);

                DateTime now = DateTime.Now;
                var LMSD = new DateTime(now.Year, now.AddMonths(-1).Month, 1);
                var PMED = now.AddMonths(1).AddDays(-1);

                #region CalculationVariable
                //double TSValueTT = 0;
                double TSValueLM = 0;
                double TSValueTM = 0;
                double TSValueY = 0;
                double TSValueTD = 0;

                //double DOValueTT = 0;
                //double DOValueLM = 0;
                //double DOValueTM = 0;
                //double DOValueY = 0;
                //double DOValueTD = 0;

                //double TONOTT = 0;
                double TONOLM = 0;
                double TONOTM = 0;
                double TONOY = 0;
                double TONOTD = 0;

                //double DGNOTT = 0;
                double DGNOLM = 0;
                double DGNOTM = 0;
                double DGNOY = 0;
                double DGNOTD = 0;

                double SLM = 0;
                double STM = 0;
                double SY = 0;
                double ST = 0;

                double ACLM = 0;
                double ACTM = 0;
                double ACY = 0;
                double ACT = 0;

                #endregion

                var kppcustomers = db.Customers.Where(x => x.IsKPP == true && x.Deleted == false).Select(x => new { CreatedDate = x.CreatedDate, Active = x.Active }).ToList();

                MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();

                var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.Status != "Inactive" && x.Status != "Dummy Order Cancelled" && !x.Deleted);
                var orderMasters = new List<MongoOrderMaster>();

                List<int> ids = db.Customers.Where(x => x.IsKPP == true && x.Active == true && x.Deleted == false).Select(x => x.CustomerId).ToList();
                orderPredicate.And(x => ids.Contains(x.CustomerId));
                //orderPredicate.And(x => x.CreatedDate >= LMSD && x.CreatedDate <= PMED);

                orderMasters = mongoDbHelper.Select(orderPredicate, collectionName: "OrderMaster").ToList();

                var sale = orderMasters.Select(x => new { CreatedDate = x.CreatedDate, TotalAmount = x.TotalAmount, CustomerId = x.CustomerId }).ToList();

                List<KPPDashboard> listdsd = new List<KPPDashboard>();
                //if (warehouseids.Count > 0)
                //{

                foreach (var item in Field)
                {
                    if (item == "New KPP (in Number)")
                    {
                        KPPDashboard dsd = new KPPDashboard();
                        dsd.FieldName = item;

                        //var total = allcustomers.Count();
                        var lastmonth = kppcustomers.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                        var thismonth = kppcustomers.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                        var Yesterday = kppcustomers.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                        var today = kppcustomers.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                        //dsd.Total = total;
                        dsd.LastMonth = lastmonth;
                        dsd.ThisMonth = thismonth;
                        dsd.Yesterday = Yesterday;
                        dsd.Today = today;
                        listdsd.Add(dsd);
                    }
                    else if (item == "Active KPP (in Number)")
                    {
                        KPPDashboard dsd = new KPPDashboard();
                        dsd.FieldName = item;

                        //var total = customers.Count();
                        var lastmonth = kppcustomers.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year && x.Active == true).Count();
                        var thismonth = kppcustomers.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.Active == true).Count();
                        var Yesterday = kppcustomers.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day && x.Active == true).Count();
                        var today = kppcustomers.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day && x.Active == true).Count();

                        ACLM = lastmonth;
                        ACTM = thismonth;
                        ACY = Yesterday;
                        ACT = today;

                        //dsd.Total = total;
                        dsd.LastMonth = lastmonth;
                        dsd.ThisMonth = thismonth;
                        dsd.Yesterday = Yesterday;
                        dsd.Today = today;
                        listdsd.Add(dsd);
                    }
                    else if (item == "Sale")
                    {
                        KPPDashboard dsd = new KPPDashboard();
                        dsd.FieldName = item;

                        var lastmonth = sale.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Sum(x => x.TotalAmount);
                        var thismonth = sale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Sum(x => x.TotalAmount);
                        var Yesterday = sale.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Sum(x => x.TotalAmount);
                        var today = sale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Sum(x => x.TotalAmount);

                        SLM = lastmonth;
                        STM = thismonth;
                        SY = Yesterday;
                        ST = today;

                        //dsd.Total = total;
                        dsd.LastMonth = lastmonth;
                        dsd.ThisMonth = thismonth;
                        dsd.Yesterday = Yesterday;
                        dsd.Today = today;
                        listdsd.Add(dsd);
                    }
                    else if (item == "Kisan Kirana Sale")
                    {
                        KPPDashboard dsd = new KPPDashboard();
                        dsd.FieldName = item;

                        var kksale = db.DbOrderDetails.Where(x => x.Deleted == false && ids.Contains(x.OrderId) && (x.SubsubcategoryName == "Kisan Kirana" || x.SubsubcategoryName == "Kisan Kirana Jumbo")).Select(x => new { TotalAmt = x.TotalAmt, CreatedDate = x.CreatedDate }).ToList();

                        var lastmonth = kksale.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Sum(x => x.TotalAmt);
                        var thismonth = kksale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Sum(x => x.TotalAmt);
                        var Yesterday = kksale.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Sum(x => x.TotalAmt);
                        var today = kksale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Sum(x => x.TotalAmt);

                        //TONOTT = total;
                        TONOLM = lastmonth;
                        TONOTM = thismonth;
                        TONOY = Yesterday;
                        TONOTD = today;

                        //dsd.Total = total;
                        dsd.LastMonth = lastmonth;
                        dsd.ThisMonth = thismonth;
                        dsd.Yesterday = Yesterday;
                        dsd.Today = today;
                        listdsd.Add(dsd);
                    }
                    else if (item == "ShopKirana Sale")
                    {
                        KPPDashboard dsd = new KPPDashboard();
                        dsd.FieldName = item;

                        var sksale = db.DbOrderDetails.Where(x => x.Deleted == false && ids.Contains(x.OrderId) && x.SubsubcategoryName != "Kisan Kirana" && x.SubsubcategoryName != "Kisan Kirana Jumbo").Select(x => new { TotalAmt = x.TotalAmt, CreatedDate = x.CreatedDate }).ToList();

                        var lastmonth = sksale.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Sum(x => x.TotalAmt);
                        var thismonth = sksale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Sum(x => x.TotalAmt);
                        var Yesterday = sksale.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Sum(x => x.TotalAmt);
                        var today = sksale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Sum(x => x.TotalAmt);

                        //DGNOTT = total;
                        DGNOLM = lastmonth;
                        DGNOTM = thismonth;
                        DGNOY = Yesterday;
                        DGNOTD = today;

                        //dsd.Total = total;
                        dsd.LastMonth = lastmonth;
                        dsd.ThisMonth = thismonth;
                        dsd.Yesterday = Yesterday;
                        dsd.Today = today;
                        listdsd.Add(dsd);
                    }
                    else if (item == "% of Kisan Kirana sale vs Shopkirana")
                    {
                        KPPDashboard dsd = new KPPDashboard();
                        dsd.FieldName = item;
                        double lastmonth = 0; try { lastmonth = TONOLM * 100 / DGNOLM; } catch (Exception) { lastmonth = 0; }
                        double thismonth = 0; try { thismonth = TONOTM * 100 / DGNOTM; } catch (Exception) { thismonth = 0; }
                        double Yesterday = 0; try { Yesterday = TONOLM * 100 / DGNOLM; } catch (Exception) { Yesterday = 0; }
                        double today = 0; try { today = TONOLM * 100 / DGNOLM; } catch (Exception) { today = 0; }

                        //TSValueTT = total ?? 0;
                        TSValueLM = lastmonth;
                        TSValueTM = thismonth;
                        TSValueY = Yesterday;
                        TSValueTD = today;

                        //dsd.Total = total ?? 0;
                        dsd.LastMonth = double.IsNaN(lastmonth) ? 0 : lastmonth;
                        dsd.ThisMonth = double.IsNaN(thismonth) ? 0 : thismonth;
                        dsd.Yesterday = double.IsNaN(Yesterday) ? 0 : Yesterday;
                        dsd.Today = double.IsNaN(today) ? 0 : today;
                        listdsd.Add(dsd);
                    }
                    else if (item == "Sale/Active Customer (in Number)")
                    {
                        KPPDashboard dsd = new KPPDashboard();
                        dsd.FieldName = item;
                        //var total = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self").Select(o => (double?)o.TotalAmount).Sum();
                        var lastmonth = SLM / ACLM;
                        var thismonth = STM / ACTM;
                        var Yesterday = SY / ACY;
                        var today = ST / ACT;


                        //dsd.Total = total ?? 0;
                        dsd.LastMonth = double.IsNaN(lastmonth) ? 0 : lastmonth;
                        dsd.ThisMonth = double.IsNaN(thismonth) ? 0 : thismonth;
                        dsd.Yesterday = double.IsNaN(Yesterday) ? 0 : Yesterday;
                        dsd.Today = double.IsNaN(today) ? 0 : today;
                        listdsd.Add(dsd);
                    }
                    else if (item == "Order till date (in Number)")
                    {
                        KPPDashboard dsd = new KPPDashboard();
                        dsd.FieldName = item;

                        //double total; try { total = (DOValueTT / TSValueTT) * 100; } catch (Exception) { total = 0; }
                        var lastmonth = sale.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                        var thismonth = sale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                        var Yesterday = sale.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                        var today = sale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                        //dsd.Total = total;
                        dsd.LastMonth = double.IsNaN(lastmonth) ? 0 : lastmonth;
                        dsd.ThisMonth = double.IsNaN(thismonth) ? 0 : thismonth;
                        dsd.Yesterday = double.IsNaN(Yesterday) ? 0 : Yesterday;
                        dsd.Today = double.IsNaN(today) ? 0 : today;
                        listdsd.Add(dsd);
                    }
                    else if (item == "Order Delivered within 48Hr (in Number)")
                    {
                        KPPDashboard dsd = new KPPDashboard();
                        dsd.FieldName = item;

                        string qry = "select count(*) from OrderMasters om inner join OrderMasterHistories omh on om.OrderId = omh.orderid where om.CustomerId = 35991 and omh.Status = 'Delivered' and DATEDIFF(hour, omh.CreatedDate, getdate()) <= 48";
                        //double total = 0; try { total = (DGNOTT / TONOTT) * 100; } catch (Exception) { total = 0; }
                        double lastmonth = 0;
                        double thismonth = 0;
                        double Yesterday = 0;
                        double today = 0;

                        //dsd.Total = total;
                        dsd.LastMonth = double.IsNaN(lastmonth) ? 0 : lastmonth;
                        dsd.ThisMonth = double.IsNaN(thismonth) ? 0 : thismonth;
                        dsd.Yesterday = double.IsNaN(Yesterday) ? 0 : Yesterday;
                        dsd.Today = double.IsNaN(today) ? 0 : today;
                        listdsd.Add(dsd);
                    }
                    else if (item == "Order Sattled (in Number)")
                    {
                        KPPDashboard dsd = new KPPDashboard();
                        dsd.FieldName = item;

                        var os = db.DbOrderMaster.Where(x => x.Status == "sattled").Select(x => new { CreatedDate = x.CreatedDate, Status = x.Status }).ToList();
                        //var total = LineItemsOnline.Count() / DGNOTT;
                        var lastmonth = os.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                        var thismonth = os.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                        var Yesterday = os.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                        var today = os.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                        //dsd.Total = total;
                        dsd.LastMonth = double.IsNaN(lastmonth) ? 0 : lastmonth;
                        dsd.ThisMonth = double.IsNaN(thismonth) ? 0 : thismonth;
                        dsd.Yesterday = double.IsNaN(Yesterday) ? 0 : Yesterday;
                        dsd.Today = double.IsNaN(today) ? 0 : today;

                        listdsd.Add(dsd);
                    }
                    else if (item == "KPP > 2 order (in Number)")
                    {
                        KPPDashboard dsd = new KPPDashboard();
                        dsd.FieldName = item;

                        //                    var item1 = sale.GroupBy(
                        //p => p.CustomerId,
                        //(key, g) => new { CustomerId = key, o = g.Count() });

                        double lastmonth = 0;
                        double thismonth = 0;
                        double Yesterday = 0;
                        double today = 0;

                        //var total = 0; try { total = LineItemsOffline.Count() / OfflineOrder.Count(); } catch (Exception) { total = 0; };
                        //var lastmonth = sale.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                        //var thismonth = sale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                        //var Yesterday = sale.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                        //var today = sale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();


                        //dsd.Total = total;
                        dsd.LastMonth = double.IsNaN(lastmonth) ? 0 : lastmonth;
                        dsd.ThisMonth = double.IsNaN(thismonth) ? 0 : thismonth;
                        dsd.Yesterday = double.IsNaN(Yesterday) ? 0 : Yesterday;
                        dsd.Today = double.IsNaN(today) ? 0 : today;

                        listdsd.Add(dsd);
                    }
                    else if (item == "KPP > 2 order (in %)")
                    {
                        KPPDashboard dsd = new KPPDashboard();
                        dsd.FieldName = item;

                        //var total = 0; try { total = LineItemsOffline.Count() / OfflineOrder.Count(); } catch (Exception) { total = 0; };
                        double lastmonth = 0;
                        double thismonth = 0;
                        double Yesterday = 0;
                        double today = 0;

                        //dsd.Total = total;
                        dsd.LastMonth = double.IsNaN(lastmonth) ? 0 : lastmonth;
                        dsd.ThisMonth = double.IsNaN(thismonth) ? 0 : thismonth;
                        dsd.Yesterday = double.IsNaN(Yesterday) ? 0 : Yesterday;
                        dsd.Today = double.IsNaN(today) ? 0 : today;

                        listdsd.Add(dsd);
                    }
                }
                return listdsd;
            }

        }

        [Route("GetKPPReportExport")]
        [HttpGet]
        public dynamic GetKPPReportExport()
        {

            DateTime now = DateTime.Now;
            var LMSD = new DateTime(now.Year, now.AddMonths(-1).Month, 1);
            var PMED = now.AddMonths(1).AddDays(-1);

            using (var con = new AuthContext())
            {
                try
                {
                    string query = "select * from view_KPPReport";
                    var exportdata = con.Database.SqlQuery<KPPReport>(query).ToList();

                    //MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();

                    //var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.Status != "Inactive" && x.Status != "Dummy Order Cancelled" && !x.Deleted);
                    //var orderMasters = new List<MongoOrderMaster>();

                    List<int> ids = con.Customers.Where(x => x.IsKPP == true && x.Active == true && x.Deleted == false).Select(x => x.CustomerId).ToList();
                    //orderPredicate.And(x => ids.Contains(x.CustomerId));
                    //orderPredicate.And(x => x.CreatedDate >= LMSD && x.CreatedDate <= PMED);


                    //orderMasters = mongoDbHelper.Select(orderPredicate, collectionName: "OrderMaster").ToList();

                    //var data = orderMasters.Select(x => new { TotalAmount = x.TotalAmount, CreatedDate = x.CreatedDate, Skcode = x.Skcode }).ToList();
                    //var KKdetails = orderMasters.Where(x => x.orderDetails.Select(y => y.SubsubcategoryName == "Kisan Kirana" || y.SubsubcategoryName == "Kisan Kirana Jumbo").Any()).ToList();
                    //var sKdetails = orderMasters.Where(x => x.orderDetails.Select(y => y.SubsubcategoryName != "Kisan Kirana" && y.SubsubcategoryName != "Kisan Kirana Jumbo").Any()).ToList();


                    var data = con.DbOrderMaster.Where(x => ids.Contains(x.CustomerId)).Select(x => new { TotalAmount = x.TotalAmount, CreatedDate = x.CreatedDate, CustomerId = x.CustomerId }).ToList();
                    var KKdetails = con.DbOrderDetails.Where(x => ids.Contains(x.CustomerId) && (x.SubsubcategoryName == "Kisan Kirana" || x.SubsubcategoryName == "Kisan Kirana Jumbo")).Select(x => new { TotalAmt = x.TotalAmt, CreatedDate = x.CreatedDate, CustomerId = x.CustomerId }).ToList();
                    var sKdetails = con.DbOrderDetails.Where(x => ids.Contains(x.CustomerId) && x.SubsubcategoryName != "Kisan Kirana" && x.SubsubcategoryName != "Kisan Kirana Jumbo").Select(x => new { TotalAmt = x.TotalAmt, CreatedDate = x.CreatedDate, CustomerId = x.CustomerId }).ToList();

                    foreach (var item in exportdata)
                    {
                        item.TLMTD = Convert.ToDecimal(data.Where(x => x.CustomerId == item.customerid && x.CreatedDate.Month == DateTime.Now.AddMonths(-1).Month).Sum(x => x.TotalAmount));
                        item.TMTD = Convert.ToDecimal(data.Where(x => x.CustomerId == item.customerid && x.CreatedDate.Month == DateTime.Now.Month).Sum(x => x.TotalAmount));
                        item.TFTD = Convert.ToDecimal(data.Where(x => x.CustomerId == item.customerid && x.CreatedDate.Month == DateTime.Now.Month && x.CreatedDate.Year == DateTime.Now.Year && x.CreatedDate.Day == DateTime.Now.Day).Sum(x => x.TotalAmount));

                        item.SKLMTD = Convert.ToDecimal(sKdetails.Where(x => x.CustomerId == item.customerid && x.CreatedDate.Month == DateTime.Now.AddMonths(-1).Month).Sum(x => x.TotalAmt));
                        item.SKMTD = Convert.ToDecimal(sKdetails.Where(x => x.CustomerId == item.customerid && x.CreatedDate.Month == DateTime.Now.Month).Sum(x => x.TotalAmt));
                        item.SKFTD = Convert.ToDecimal(sKdetails.Where(x => x.CustomerId == item.customerid && x.CreatedDate.Month == DateTime.Now.Month && x.CreatedDate.Year == DateTime.Now.Year && x.CreatedDate.Day == DateTime.Now.Day).Sum(x => x.TotalAmt));

                        item.KKLMTD = Convert.ToDecimal(KKdetails.Where(x => x.CustomerId == item.customerid && x.CreatedDate.Month == DateTime.Now.AddMonths(-1).Month).Sum(x => x.TotalAmt));
                        item.KKMTD = Convert.ToDecimal(KKdetails.Where(x => x.CustomerId == item.customerid && x.CreatedDate.Month == DateTime.Now.Month).Sum(x => x.TotalAmt));
                        item.KKFTD = Convert.ToDecimal(KKdetails.Where(x => x.CustomerId == item.customerid && x.CreatedDate.Month == DateTime.Now.Month && x.CreatedDate.Year == DateTime.Now.Year && x.CreatedDate.Day == DateTime.Now.Day).Sum(x => x.TotalAmt));

                    }

                    return Ok(exportdata);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        private bool IsSupplierExists(string SupplierCode)
        {
            bool IsExist = false;
            using (AuthContext Context = new AuthContext())
            {
                int Count = Context.Suppliers.Where(x => x.SUPPLIERCODES.ToUpper() == SupplierCode.ToUpper() && x.Deleted == false).Count();
                if (Count > 0)
                {
                    IsExist = true;
                }

                return IsExist;
            }
        }

        private List<MakerCheckerMaster> GetSetMakerCheckerMastersFromCache()
        {
            List<MakerCheckerMaster> result = new List<MakerCheckerMaster>();
            using (AuthContext context = new AuthContext())
            {
                result = context.Database.SqlQuery<MakerCheckerMaster>(new StringBuilder("select * from MakerCheckerMasters where IsActive=1").ToString()).ToList();
                return result;
            }
        }

        private bool IsExistsDepo(string DepoCodes)
        {
            using (AuthContext context = new AuthContext())
            {
                bool IsExists = false;

                int count = context.DepoMasters.Where(x => x.DepoCodes == DepoCodes && x.Deleted == false).Count();
                if (count > 0)
                {
                    IsExists = true;
                }
                return IsExists;

            }
        }
    }

    public class KPPReport
    {
        public string Region { get; set; }
        public string CityName { get; set; }
        public string WarehouseName { get; set; }
        public string SkCode { get; set; }
        public string Status { get; set; }
        public int customerid { get; set; }
        public string KPPName { get; set; }
        public string Town { get; set; }
        public string ContactNo { get; set; }
        public decimal KKLMTD { get; set; }
        public decimal KKMTD { get; set; }
        public decimal KKFTD { get; set; }
        public decimal SKLMTD { get; set; }
        public decimal SKMTD { get; set; }
        public decimal SKFTD { get; set; }
        public decimal TLMTD { get; set; }
        public decimal TMTD { get; set; }
        public decimal TFTD { get; set; }
    }

    public class KPPDashboard
    {
        public string FieldName { get; set; }
        //public double Total { get; set; }
        public double LastMonth { get; set; }
        public double ThisMonth { get; set; }
        public double Yesterday { get; set; }
        public double Today { get; set; }
    }


    public class MakerCheckerDTO
    {
        public ObjectId Id { get; set; }
        public string MongoId { get; set; }
        public string EntityName { get; set; }
        public string EntityId { get; set; }
        public string Operation { get; set; }
        public string OldJson { get; set; }
        public string NewJson { get; set; }
        public string Status { get; set; }
        public string CheckerComment { get; set; }
        public string MakerBy { get; set; }
        public string CheckerBy { get; set; }
        public DateTime MakerDate { get; set; }
        public DateTime? CheckerDate { get; set; }
        public string SupplierCode { get; set; }

        public string DepoCodes { get; set; }
    }
    public class MakerCRUD
    {
        public dynamic MakerData { get; set; }
        public dynamic OldData { get; set; }
        public string Operation { get; set; }
    }



    public class Checker
    {
        public int Id { get; set; }
        public string EntityName { get; set; }
        public string RoleName { get; set; }
    }

    public class InventoryData
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public double Tax { get; set; }
        public int OpeningStock { get; set; }
        public double OpeningStockAmount { get; set; }
        public int InWardTotal { get; set; }
        public double InwardTotalAmountWithoutTax { get; set; }
        public int OutwardQuantity { get; set; }
        public double OutwardWithoutTaxAmount { get; set; }
        public int ClosingQty { get; set; }
        public double AdjClosingAmount { get; set; }
        public int DifferenceQty { get; set; }
        public double pilferageAmount { get; set; }
        public double GrossMargin { get; set; }
    }

    public class FlashDealData
    {
        public string SKCode { get; set; }
        public int OrderId { get; set; }
        public string OfferCode { get; set; }
        public string ShopName { get; set; }
        public string RetailerName { get; set; }
        public string Mobile { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string HSNCode { get; set; }
        public string Warehouse { get; set; }
        public string Date { get; set; }
        public string OrderBy { get; set; }
        public string Executive { get; set; }
        public double MRP { get; set; }
        public double UnitPrice { get; set; }
        public int MOQPrice { get; set; }
        public int Quantity { get; set; }
        public double TotalAmt { get; set; }
        public string Status { get; set; }
    }

    public class Zonedto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; }
    }

    public class Regiondto
    {
        public int RegionId { get; set; }
        public string RegionName { get; set; }
    }

    public class Citydto
    {
        public int CityId { get; set; }
        public string CityName { get; set; }
    }

    public class Warehousedto
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int RegionId { get; set; }
        public int? CityId { get; set; }


    }

    public class Clusterdto
    {
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
    }
    public class PurchaseReportData
    {
        public int PurchaseOrderId { get; set; }
        public string WarehouseName { get; set; }
        public string Status { get; set; }
        public string BuyerName { get; set; }
        public string SupplierName { get; set; }
        public string Brand { get; set; }
        public int ItemId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public string ItemName { get; set; }
        public double MRP { get; set; }
        public double TotalTaxPercentage { get; set; }
        public DateTime? invoicedate { get; set; }
        public DateTime? GRDate1 { get; set; }
        public int? gr1Qty { get; set; }
        public DateTime? GRDate2 { get; set; }
        public int? gr2Qty { get; set; }
        public DateTime? GRDate3 { get; set; }
        public int? gr3Qty { get; set; }
        public DateTime? GRDate4 { get; set; }
        public int? gr4Qty { get; set; }
        public DateTime? GRDate5 { get; set; }
        public int? gr5Qty { get; set; }
        public double? IR1Qty { get; set; }
        public double? IR2Qty { get; set; }
        public double? IR3Qty { get; set; }
        public double? IR4Qty { get; set; }
        public double? IR5Qty { get; set; }
        public int? GRTotalQty { get; set; }
        public double? IRTotalQty { get; set; }
        public double GR_IRDifference { get; set; }
        public double? GRAmount { get; set; }
        public double? IRCalculatedAmount { get; set; }
        public double gstAmnt { get; set; }


    }

    //public class PurchaseReportDataDc
    //{
    //    public int PurchaseOrderId { get; set; }
    //    public string InvoiceNumbers { get; set; }
    //    public int ItemMultiMrpId { get; set; }
    //    public string WarehouseName { get; set; }
    //    public string BuyerName { get; set; }
    //    public string SupplierName { get; set; }

    //    public string GstinNumber { get; set; }

    //    public string SupplierCode { get; set; }
    //    public decimal TotalTdsAmount { get; set; }
    //    public double TdsPercentage { get; set; }

    //    public string Status { get; set; }
    //    public string ItemName { get; set; }
    //    public double PORate { get; set; }
    //    public double MRP { get; set; }

    //    public decimal PreValue { get; set; }
    //    public decimal SGSTTaxPercentage { get; set; }
    //    public decimal SGSTTaxAmmount { get; set; }
    //    public decimal CGSTTaxPercentage { get; set; }
    //    public decimal CGSTTaxAmmount { get; set; }
    //    public decimal IGSTTaxPercentage { get; set; }
    //    public decimal IGSTTaxAmmount { get; set; }

    //    public double IRWeightAvgPrice { get; set; }
    //    public DateTime? invoicedate { get; set; }
    //    public DateTime? GrDate1 { get; set; }
    //    public DateTime? GrDate2 { get; set; }
    //    public DateTime? GrDate3 { get; set; }
    //    public DateTime? GrDate4 { get; set; }
    //    public DateTime? GrDate5 { get; set; }
    //    public DateTime? IrDate1 { get; set; }
    //    public DateTime? IrDate2 { get; set; }
    //    public DateTime? IrDate3 { get; set; }
    //    public DateTime? IrDate4 { get; set; }
    //    public DateTime? IrDate5 { get; set; }
    //    public int? gr1Qty { get; set; }
    //    public int? gr2Qty { get; set; }
    //    public int? gr3Qty { get; set; }
    //    public int? gr4Qty { get; set; }
    //    public int? gr5Qty { get; set; }
    //    public int? ir1Qty { get; set; }
    //    public int? ir2Qty { get; set; }
    //    public int? ir3Qty { get; set; }
    //    public int? ir4Qty { get; set; }
    //    public int? ir5Qty { get; set; }
    //    public int? GrTotalQty { get; set; }
    //    public int? IRTotalQty { get; set; }
    //    public int? GrIrDiff { get; set; }
    //    public decimal IrAmount { get; set; }
    //    public decimal GRAmount { get; set; }
    //    public double TaxAmount { get; set; }
    //    public double Discount { get; set; }
    //    public double OtherAmount { get; set; }
    //    public double gstAmnt { get; set; }
    //    public double Expense { get; set; }
    //    public double Freight { get; set; }
    //    public double RoundOf { get; set; }
    //    public DateTime? PoCreateDate { get; set; }
    //}



    public class PurchaseReportDataDc
    {
        public int PurchaseOrderId { get; set; }
        public string InvoiceNumbers
        { get; set; }
        public int ItemMultiMRPId
        { get; set; }//ItemMultiMRPId
        public string CityName { get; set; }//sudhir--> 25-07-2023
        public string WarehouseName { get; set; }
        public string BuyerName
        { get; set; }
        public string SupplierName
        { get; set; }

        public string Status
        { get; set; }
        public string StoreName { get; set; }//sudhir--> 25-07-2023
        public int ItemId { get; set; }//sudhir--> 25-07-2023
        public string CategoryName { get; set; }//sudhir--> 25-07-2023
        public string SubcategoryName { get; set; }//sudhir--> 25-07-2023
        public string BrandName { get; set; }//sudhir--> 25-07-2023
        public string ItemName { get; set; }
        public double PORate { get; set; }
        public double MRP { get; set; }

        public string SUPPLIERCODES { get; set; }
        public string bussinessType { get; set; }
        public string GstinNumber { get; set; }
        public decimal IR1PreValue { get; set; }
        public decimal IR2PreValue { get; set; }
        public decimal IR3PreValue { get; set; }
        public decimal IR4PreValue { get; set; }
        public decimal IR5PreValue { get; set; }

        public double SGSTTaxPercentage { get; set; }
        public double SGSTTaxAmmount { get; set; }
        public double CGSTTaxPercentage { get; set; }
        public double CGSTTaxAmmount { get; set; }
        public double IGSTTaxPercentage { get; set; }
        public double IGSTTaxAmmount { get; set; }

        public double CessTaxAmount { get; set; }
        public double CessPercentage { get; set; }

        public double IRWeightAvgPrice { get; set; }
        public string invoicedate { get; set; }
        public string GrDate1 { get; set; }
        public string GrDate2 { get; set; }
        public string GrDate3 { get; set; }
        public string GrDate4 { get; set; }
        public string GrDate5 { get; set; }
        public string IrDate1 { get; set; }
        public string IrDate2 { get; set; }
        public string IrDate3 { get; set; }
        public string IrDate4 { get; set; }
        public string IrDate5 { get; set; }
        public int? gr1Qty { get; set; }
        public int? gr2Qty { get; set; }
        public int? gr3Qty { get; set; }
        public int? gr4Qty { get; set; }
        public int? gr5Qty { get; set; }
        public int? ir1Qty { get; set; }
        public int? ir2Qty { get; set; }
        public int? ir3Qty { get; set; }
        public int? ir4Qty { get; set; }
        public int? ir5Qty { get; set; }
        public int? GrTotalQty { get; set; }
        public int? IRTotalQty { get; set; }
        public int? GrIrDiff { get; set; }
        public decimal IrAmount { get; set; }
        public decimal GRAmount { get; set; }
        public double TaxAmount1 { get; set; }
        public double TaxAmount2 { get; set; }

        public double TaxAmount3 { get; set; }

        public double TaxAmount4 { get; set; }

        public double TaxAmount5 { get; set; }


        public double Discount { get; set; }
        public double OtherAmount { get; set; }
      
        public double Expense { get; set; }
        public double Freight { get; set; }
        public double POFreightCharges { get; set; }//weight On Qty
        public double RoundOf { get; set; }
        public decimal TotalIRPrevalue { get; set; }

        

        public double TotalTdsAmount { get; set; }
        public double TdsPercentage { get; set; }
      
       public decimal IrAmount_Other_Expense { get; set; }
        public double TaxPercentage { get; set; }
        public double TotalTaxAmount { get; set; }
        public string IRStatus { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string DepoGST { get; set; }
        public string Quadrant { get; set; }
        public double? PTR { get; set; }
        public string OtherAmountRemark { get; set; }
        public DateTime? IRApprovedDate { get; set; }
    }

    public class FlashDealReport
    {
        public List<FlashDealData> FlashDeal { get; set; }
        public double TotalOrders { get; set; }
        public double TotalAmount { get; set; }

    }
}
