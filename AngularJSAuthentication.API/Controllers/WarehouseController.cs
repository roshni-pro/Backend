using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helper.BackendOrderProcess;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Warehouse")]
    public class WarehouseController : BaseAuthController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Authorize]
        [Route("")]
        [AllowAnonymous]
        public IEnumerable<Warehouse> Get()
        {
            using (AuthContext context = new AuthContext())
            {
                List<Warehouse> ass = new List<Warehouse>();

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (Warehouse_id > 0)
                {
                    ass = context.AllWarehouseWid(compid, Warehouse_id).OrderBy(a => a.CityName).ToList();
                    return ass;
                }
                else
                {
                    // Changed FROM AllWarehouse TO AllWarehousewithoutTestId by ANOOP on 22/2/2021
                    ass = context.AllWarehousewithoutTestId(compid).OrderBy(a => a.CityName).ToList();
                    return ass;
                }
            }
        }

        [Authorize]
        [Route("WarehouseWithKpp")]
        [AllowAnonymous]
        public IEnumerable<Warehouse> GetWarehouseIsKpp()
        {
            using (AuthContext context = new AuthContext())
            {
                List<Warehouse> ass = new List<Warehouse>();

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (Warehouse_id > 0)
                {
                    ass = context.AllWarehouseWidwithKPP(compid, Warehouse_id).OrderBy(a => a.CityName).ToList();
                    return ass;
                }
                else
                {
                    ass = context.AllWarehouseWithKPP(compid).OrderBy(a => a.CityName).ToList();
                    return ass;
                }
            }
        }

        [Authorize]
        [Route("GetWarehouseWOKPP")]
        public IEnumerable<Warehouse> GetWarehouseWOKPP()
        {
            using (AuthContext context = new AuthContext())
            {
                List<Warehouse> ass = new List<Warehouse>();
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (Warehouse_id > 0)
                {
                    ass = context.AllWarehouseWid(compid, Warehouse_id).ToList();
                    return ass;
                }
                else
                {
                    ass = context.AllWarehouse(compid).ToList();
                    return ass;
                }
            }
        }
        [AllowAnonymous]
        [Route("GetAllWarehouse")]
        public IEnumerable<Warehouse> GetAllWarehouse()
        {
            using (AuthContext context = new AuthContext())
            {
                List<Warehouse> ass = new List<Warehouse>();

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                ass = context.AllWarehouse(compid).OrderBy(X => X.CityName).ToList();
                return ass;
            }
        }

        #region Get city Warehouse Based
        /// <summary>
        /// get data warehouse id based
        /// Createdate 07/03/2019
        /// </summary>
        /// <param name="cityid"></param>
        /// <returns></returns>
        [Route("GetWarehouseCity")]
        [HttpGet]
        public IEnumerable<Warehouse> GetWarehousecity(int cityid)
        {
            using (AuthContext db = new AuthContext())
            {
                List<Warehouse> ass = new List<Warehouse>();

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                List<Warehouse> whdata = db.Warehouses.Where(x => x.Cityid == cityid && x.Deleted == false && x.active == true && x.IsKPP == false).ToList();
                return whdata;
            }
        }

        [Route("GetWarehouseByCity")]
        [HttpGet]
        public List<WareHouseDc> GetWarehouseByCity(int cityid)
        {
            using (AuthContext db = new AuthContext())
            {
                string Sql = "select a.WarehouseId,a.WarehouseName,a.Cityid,a.CityName from Warehouses a with(nolock) inner join GMWarehouseProgresses b  with(nolock) on a.WarehouseId=b.WarehouseID where cityid=" + cityid + " and active=1 and Deleted=0 and IsKPP=0 and b.IsLaunched=1";
                List<WareHouseDc> whdata = db.Database.SqlQuery<WareHouseDc>(Sql).ToList();

                return whdata;
            }
        }

        //GetActiveWarehouseDelivery
        [Route("GetActiveWarehouseDelivery")] ////warehouse accor to user login
        [HttpGet]
        public List<WareHouseDc> GetActiveWarehouseDelivery(int cityid)
        {
            using (AuthContext db = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                var userids = new SqlParameter("@userid", userid);
                var cityids = new SqlParameter("@cityid", cityid);


                //string Sql = "select a.WarehouseId,a.WarehouseName,a.Cityid,a.CityName from Warehouses a with(nolock) inner join GMWarehouseProgresses b  with(nolock) on a.WarehouseId=b.WarehouseID where cityid=" + cityid + " and active=1 and Deleted=0 and IsKPP=0 and b.IsLaunched=1";
                List<WareHouseDc> whdata = db.Database.SqlQuery<WareHouseDc>("GetActiveWarehouseDelivery @userid, @cityid", userids, cityids).ToList();

                return whdata;
            }
        }


        [Route("GetActiveWarehouseCity")]
        [HttpGet]
        [AllowAnonymous]
        public List<SpecificWarehousesDTO> GetActiveWarehouseCity()
        {
            using (AuthContext db = new AuthContext())
            {
                //string Sql = "select distinct a.Cityid,a.CityName from Warehouses a with(nolock) inner join GMWarehouseProgresses b  with(nolock) on a.WarehouseId=b.WarehouseID where active=1 and Deleted=0 and IsKPP=0 and b.IsLaunched=1";
                //List<SpecificWarehousesDTO> cityData = db.Database.SqlQuery<SpecificWarehousesDTO>(Sql).ToList();


                List<SpecificWarehousesDTO> cityData = db.Database.SqlQuery<SpecificWarehousesDTO>("exec  GetActiveWarehouseCity").ToList();
                return cityData;
            }
        }

        [Route("GetActiveWarehouseCityDelivery")]  //city accor to user login
        [HttpGet]
        [AllowAnonymous]
        public List<SpecificWarehousesDTO> GetActiveWarehouseCityNew()
        {
            using (AuthContext db = new AuthContext())
            {
                //string Sql = "select distinct a.Cityid,a.CityName from Warehouses a with(nolock) inner join GMWarehouseProgresses b  with(nolock) on a.WarehouseId=b.WarehouseID where active=1 and Deleted=0 and IsKPP=0 and b.IsLaunched=1";
                //List<SpecificWarehousesDTO> cityData = db.Database.SqlQuery<SpecificWarehousesDTO>(Sql).ToList();

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                var userids = new SqlParameter("@userid", userid);

                List<SpecificWarehousesDTO> cityData = db.Database.SqlQuery<SpecificWarehousesDTO>("exec  GetActiveWarehouseCityNew @userid", userids).ToList();
                return cityData;
            }
        }


        /// <summary>
        /// get data warehouse id based
        /// Createdate 07/03/2019
        /// </summary>
        /// <param name="cityid"></param>
        /// <returns></returns>
        [Route("GetWarehouseCityOnOrder")]
        [HttpGet]
        public IEnumerable<Warehouse> GetWarehousecityOnOrder(int cityid)
        {
            using (AuthContext db = new AuthContext())
            {
                List<Warehouse> ass = new List<Warehouse>();

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                List<Warehouse> whdata = db.Warehouses.Where(x => x.Cityid == cityid && x.Deleted == false && x.active == true).ToList();
                return whdata;
            }
        }

        // Developed on 13/05/2020
        [Route("GetWarehouseCitiesOnOrder")]
        [HttpPost]
        public IEnumerable<Warehouse> GetWarehousecitiesOnOrder(List<int> cities)
        {
            using (AuthContext db = new AuthContext())
            {
                List<Warehouse> ass = new List<Warehouse>();

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                List<Warehouse> whdata = db.Warehouses.Where(x => cities.Contains(x.Cityid) && x.Deleted == false && x.active == true).ToList();
                return whdata;
            }
        }

        #endregion

        //B to C APP
        [Route("")]
        public List<Warehouse> post(string type)
        {
            using (AuthContext context = new AuthContext())
            {
                List<Warehouse> ass = new List<Warehouse>();

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                int CompanyId = compid;
                if (type == "ids")
                {
                    try
                    {
                        ass = context.AllWHouseforapp(CompanyId).ToList();
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }

                }
                return ass;
            }
        }

        [Route("")]
        public Warehouse Get(int id)
        {
            using (AuthContext context = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 1, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                int CompanyId = compid;

                Warehouse ass = new Warehouse();
                ass = context.getwarehousebyid(id, CompanyId);
                return ass;
            }
        }
        [Route("GetAllWarehousekpp")]
        public IEnumerable<Warehouse> GetAllWarehousekpp()
        {
            using (AuthContext context = new AuthContext())
            {
                List<Warehouse> ass = new List<Warehouse>();

                ass = context.Warehouses.Where(x => x.Deleted == false && x.active == true).ToList();
                return ass;
            }
        }
        [AllowAnonymous]

        [Route("WarehouseGEtData")]
        public IEnumerable<Warehouse> GEtwarehousessss()
        {
            using (var context = new AuthContext())
            {
                List<Warehouse> warehouses = new List<Warehouse>();

                warehouses = context.Warehouses.Where(x => x.Deleted == false && x.active == true).ToList();
                return warehouses;

            }
        }




        [Route("GetWarehouse")]
        public IEnumerable<Warehouse> GetWarehouse(int CityId)
        {
            using (AuthContext context = new AuthContext())
            {
                List<Warehouse> ass = new List<Warehouse>();
                ass = context.Warehouses.Where(x => x.Cityid == CityId && x.Deleted == false && x.active == true).ToList();
                return ass;
            }
        }

        [Authorize]
        [Route("")]
        public IEnumerable<State> Get(string recordtype)
        {
            if (recordtype == "states")
            {
                using (AuthContext context = new AuthContext())
                {
                    logger.Info("start Warehouse: ");
                    List<Warehouse> ass = new List<Warehouse>();
                    List<State> st = new List<State>();

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    ass = context.AllWarehouse(compid).ToList();

                    var distinctrecords = (from r in ass
                                           orderby r.Stateid
                                           select new { r.Stateid, r.StateName }).Distinct();
                    foreach (var item in distinctrecords)
                    {
                        State s = new State();
                        s.Stateid = item.Stateid;
                        s.StateName = item.StateName;
                        st.Add(s);
                    }
                    return st;
                }
            }
            return null;
        }

        [Authorize]
        [Route("")]
        public IEnumerable<string> Get(string recordtype, string state)
        {
            if (recordtype == "city")
            {
                logger.Info("start Warehouse: ");
                using (AuthContext context = new AuthContext())
                {
                    List<Warehouse> ass = new List<Warehouse>();

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    ass = context.AllWarehouse(compid).ToList();

                    var distinctrecords = (from r in ass
                                           orderby r.Cityid
                                           where r.StateName.Equals(state)
                                           select r.CityName).Distinct();

                    return distinctrecords;

                }
            }
            return null;
        }

        [ResponseType(typeof(Warehouse))]
        [Route("")]
        [AcceptVerbs("POST")]
        public Warehouse add(Warehouse item)
        {
            //using (AuthContext context = new AuthContext())
            {
                using (AuthContext db = new AuthContext())
                {
                    try
                    {
                        var identity = User.Identity as ClaimsIdentity;
                        int compid = 0, userid = 0;

                        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                            compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                        item.CompanyId = compid;
                        if (item == null)
                        {
                            throw new ArgumentNullException("item");
                        }

                        Company com = db.Companies.Where(x => x.Deleted == false && x.Id == compid).FirstOrDefault();
                        item.CompanyName = com.CompanyName;
                       
                        if (item.Createactive == true)
                        {
                            //context.AddWarehouse(item);

                            //wrong logic
                            List<Warehouse> warehouses = db.Warehouses.Where(c => c.WarehouseId.Equals(item.WarehouseId) && c.Deleted == false && c.CompanyId == item.CompanyId).ToList();
                            City city = db.Cities.Where(x => x.Cityid == item.Cityid && x.Deleted == false).Select(x => x).FirstOrDefault();
                            State St = db.States.Where(x => x.Stateid == item.Stateid && x.Deleted == false).Select(x => x).FirstOrDefault();
                            TaxGroup Tg = db.DbTaxGroup.Where(x => x.GruopID == item.GruopID && x.Deleted == false && x.CompanyId == item.CompanyId).Select(x => x).SingleOrDefault();
                            Warehouse objWarehouse = new Warehouse();
                            if (warehouses.Count == 0)
                            {
                                item.GruopID = item.GruopID;
                                if (Tg != null)
                                {
                                    item.TGrpName = Tg.TGrpName;
                                }
                                else
                                {
                                    item.TGrpName = "Tax";
                                }

                                var warehouseCount = db.Warehouses.Where(x => x.Cityid == item.Cityid && x.Stateid == item.Stateid).Count();

                                var wareHouseName = St.AliasName + "-" + city.aliasName + "-" + (warehouseCount + 1);

                                item.WarehouseName = wareHouseName;

                                item.CreatedBy = item.CreatedBy;
                                item.CreatedDate = DateTime.Now;
                                item.UpdatedDate = DateTime.Now;
                                item.CityName = city.CityName;
                                item.StateName = St.StateName;
                                item.Deleted = false;
                                item.IsStore = item.IsStore;
                                db.Warehouses.Add(item);
                                int id = db.Commit();
                                if (item.IsStore == true)
                                {
                                    BackendOrderProcessHelper backendOrderProcessHelper = new BackendOrderProcessHelper();
                                    backendOrderProcessHelper.CreateDboy_SalesExecutive_Cluster(item.WarehouseId,item.WarehouseName, item.latitude, item.longitude, item.Cityid, city.CityName, userid, item.CreatedBy);
                                }
                                if (id != 0)
                                {

                                    string sqlquery = "Insert into itemmasters ( itemname ,active,itemBaseName	,itemcode,WarehouseId,WarehouseName,BaseCategoryid,BaseCategoryName,Categoryid,CategoryName,CompanyId,"
                                                    + " CreatedDate,Deleted ,Description ,Discount,DisplaySellingPrice,free,GeneralPrice,GruopID ,CessGrpID,HindiName,HSNCode,IsBulkItem,IsDailyEssential,"
                                                    + " IsHighestDPItem ,IsOffer,IsPramotionalItem,LogoUrl,Margin,marginPoint,MinOrderQty,NetPurchasePrice,Number,PramotionalDiscount ,price ,"
                                                    + " ShowTypes, DefaultBaseMargin, promoPerItems,promoPoint ,PurchaseMinOrderQty,PurchasePrice,PurchaseSku,PurchaseUnitName ,SellingSku,SellingUnitName, SizePerUnit, "
                                                    + " SubCategoryId ,SubcategoryName,SubsubCategoryid,SubsubcategoryName,SubSubCode,TGrpName,CessGrpName ,UnitPrice,VATTax ,UpdatedDate,"
                                                    + " MRP,UnitofQuantity,UOM ,ItemMultiMRPId,cityid,SupplierId,TotalTaxPercentage,inTally,IsSensitive,IsSensitiveMRP)			"
                                                    + " select  itemname ,0,itemBaseName	,itemcode," + item.WarehouseId + ",'" + item.WarehouseName + "',BaseCategoryid,BaseCategoryName,Categoryid,CategoryName,CompanyId,"
                                                    + " getdate(),0 ,Description ,Discount,DisplaySellingPrice,free,GeneralPrice,GruopID ,CessGrpID,HindiName,HSNCode,IsBulkItem,IsDailyEssential,"
                                                    + " IsHighestDPItem ,IsOffer,IsPramotionalItem,LogoUrl,Margin,marginPoint,MinOrderQty,NetPurchasePrice,Number,PramotionalDiscount ,price ,"
                                                    + " ShowTypes, DefaultBaseMargin, promoPerItems,promoPoint ,PurchaseMinOrderQty,PurchasePrice,PurchaseSku,PurchaseUnitName ,SellingSku,SellingUnitName, SizePerUnit, 	"
                                                    + " SubCategoryId ,SubcategoryName,SubsubCategoryid,SubsubcategoryName,SubSubCode,TGrpName,CessGrpName ,UnitPrice,VATTax ,UpdatedDate, "
                                                    + " MRP,UnitofQuantity,UOM ,ItemMultiMRPId," + item.Cityid + ",0,TotalTaxPercentage,0,IsSensitive,IsSensitiveMRP from ItemMasterCentrals where Deleted=0";
                                    int i = db.Database.ExecuteSqlCommand(sqlquery);
                                    if (i > 0)
                                    {
                                        //sqlquery = " MERGE CurrentStockS AS TARGET"
                                        //                + " USING (select * from  itemmasters where warehouseid=" + item.WarehouseId + ") AS SOURCE "
                                        //                + " ON (TARGET.ItemNumber = SOURCE.Number AND TARGET.WarehouseId = " + item.WarehouseId + " AND TARGET.CompanyId = SOURCE.CompanyId AND TARGET.ItemMultiMRPId = SOURCE.ItemMultiMRPId)  "
                                        //                + " WHEN MATCHED THEN"
                                        //                + " UPDATE SET TARGET.ItemNumber = SOURCE.Number,"
                                        //                + " TARGET.Barcode = SOURCE.Barcode,"
                                        //                + " TARGET.WarehouseId = SOURCE.WarehouseId,"
                                        //                + " TARGET.WarehouseName = SOURCE.WarehouseName,"
                                        //                + " TARGET.CompanyId = SOURCE.CompanyId,"
                                        //                + " TARGET.Deleted = 0,"
                                        //                + " TARGET.UpdatedDate = GETDATE(), "
                                        //                + " TARGET.MRP = SOURCE.price,"
                                        //                + " TARGET.UnitofQuantity = SOURCE.UnitofQuantity,"
                                        //                + " TARGET.UOM = SOURCE.UOM,"
                                        //                + " TARGET.ItemMultiMRPId = SOURCE.ItemMultiMRPId,"
                                        //                + " TARGET.itemname = SOURCE.itemname,"
                                        //                + " TARGET.itemBaseName = SOURCE.itemBaseName"                                                        
                                        //                + " WHEN NOT MATCHED Then"
                                        //                + " insert ( ItemId, ItemNumber ,Barcode ,WarehouseId ,WarehouseName,CompanyId ,CurrentInventory,CreationDate,UpdatedDate,MRP ,UnitofQuantity,UOM,ItemMultiMRPId ,itemname,itemBaseName,Deleted,IsEmptyStock  )"
                                        //                + " values(SOURCE.ItemId,SOURCE.Number,SOURCE.Barcode,SOURCE.WarehouseId,SOURCE.WarehouseName,SOURCE.CompanyId,0,getdate(),getdate(),SOURCE.price,SOURCE.UnitofQuantity,SOURCE.UOM,SOURCE.ItemMultiMRPId,SOURCE.itemname,SOURCE.itemBaseName,0,0) ;";
                                        sqlquery = "Insert Into CurrentStockS (ItemNumber  ,WarehouseId ,WarehouseName,CompanyId ,CurrentInventory,CreationDate,UpdatedDate,MRP ,UnitofQuantity,UOM,ItemMultiMRPId ,itemname,itemBaseName,Deleted,IsEmptyStock  )"
                                                 + " Select SOURCE.Number,SOURCE.WarehouseId,SOURCE.WarehouseName,SOURCE.CompanyId,0,getdate(),getdate(),SOURCE.price,SOURCE.UnitofQuantity,SOURCE.UOM,SOURCE.ItemMultiMRPId,max(SOURCE.itemname),SOURCE.itemBaseName,0,0 from  itemmasters SOURCE where warehouseid=" + item.WarehouseId + " group by SOURCE.Number,SOURCE.WarehouseId,SOURCE.WarehouseName,SOURCE.CompanyId,SOURCE.price,SOURCE.UnitofQuantity,SOURCE.UOM,SOURCE.ItemMultiMRPId,SOURCE.itemBaseName";
                                        i = db.Database.ExecuteSqlCommand(sqlquery);
                                    }
                                }
                            }
                        }
                        else if (item.active == true)
                        {

                            //wrong logic
                            List<Warehouse> warehouses = db.Warehouses.Where(c => c.WarehouseId.Equals(item.WarehouseId) && c.Deleted == false && c.CompanyId == item.CompanyId).ToList();
                            City city = db.Cities.Where(x => x.Cityid == item.Cityid && x.Deleted == false).Select(x => x).FirstOrDefault();
                            State St = db.States.Where(x => x.Stateid == item.Stateid && x.Deleted == false).Select(x => x).FirstOrDefault();
                            TaxGroup Tg = db.DbTaxGroup.Where(x => x.GruopID == item.GruopID && x.Deleted == false && x.CompanyId == item.CompanyId).Select(x => x).SingleOrDefault();
                            Warehouse objWarehouse = new Warehouse();

                            if (warehouses.Count == 0)
                            {
                                item.GruopID = item.GruopID;
                                if (Tg != null)
                                {
                                    item.TGrpName = Tg.TGrpName;
                                }
                                else
                                {
                                    item.TGrpName = "Tax";
                                }

                                var warehouseCount = db.Warehouses.Where(x => x.Cityid == item.Cityid && x.Stateid == item.Stateid).Count();

                                var wareHouseName = St.AliasName + "-" + city.aliasName + "-" + (warehouseCount + 1);

                                item.WarehouseName = wareHouseName;

                                item.CreatedBy = item.CreatedBy;
                                item.CreatedDate = DateTime.Now;
                                item.UpdatedDate = DateTime.Now;
                                item.CityName = city.CityName;
                                item.StateName = St.StateName;
                                item.Deleted = false;
                                item.IsStore = item.IsStore;
                                db.Warehouses.Add(item);
                                int id = db.Commit();
                            }
                        }
                        else
                        {
                            return null;

                        }

                        return item;
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
            }
        }

        [ResponseType(typeof(Warehouse))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public Warehouse Put(Warehouse item)
        {
            using (AuthContext db = new AuthContext())
            {
                using (AuthContext context = new AuthContext())
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    item.CompanyId = compid;

                    Company com = db.Companies.Where(x => x.Deleted == false && x.Id == compid).FirstOrDefault();

                    item.CompanyName = com.CompanyName;
                    if (item.active == true)
                    {
                        return context.PutWarehouse(item);
                    }
                    else
                    {
                        return context.PutWarehouse(item);
                    }
                }
            }
        }

        [ResponseType(typeof(Warehouse))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            using (AuthContext context = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
                int CompanyId = compid;

                context.DeleteWarehouse(id, CompanyId);
            }
        }

        //Angular 7
        [ResponseType(typeof(Warehouse))]
        [Route("DeleteV7")]
        [AcceptVerbs("Delete")]
        public Boolean RemoveV7(int id)
        {
            using (AuthContext context = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                int CompanyId = compid;
                return context.DeleteWarehouse(id, CompanyId);
            }
        }

        [Route("SalesManAllWarehouse")]
        public HttpResponseMessage GetAllWarehouseSalesMan(string Mobile, int PeopleId)
        {
            using (AuthContext db = new AuthContext())
            {
                WarehouseDetail obj = new WarehouseDetail();
                List<Warehouse> data = new List<Warehouse>();

                People people = db.Peoples.Where(e => e.PeopleID == PeopleId && e.Mobile == Mobile && e.Active == true).FirstOrDefault();
                if (people != null)
                {
                    var param = new SqlParameter("peopleId", PeopleId);

                    data = db.Database.SqlQuery<Warehouse>("exec SalesManAllWarehouse @peopleId", param).ToList();

                    if (data.Count() > 0)
                    {
                        obj = new WarehouseDetail()
                        {
                            Warehouses = data,
                            Status = true,
                            Message = "Warehouse Found"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }
                    else
                    {
                        if (data.Count() > 0)
                        {
                            obj = new WarehouseDetail()
                            {
                                Warehouses = data,
                                Status = true,
                                Message = "Warehouse Not Found"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                    }
                }

                obj = new WarehouseDetail()
                {
                    Warehouses = data,
                    Status = false,
                    Message = "people not found"
                };
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
        }

        [Route("GetKppList")]
        [HttpGet]
        public IEnumerable<Warehouse> getKppList()
        {
            using (AuthContext db = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                List<Warehouse> whdata = db.Warehouses.Where(p => p.CompanyId == compid && p.Deleted == false && p.IsKPP == true && p.active == true).OrderBy(x => x.WarehouseName).ToList();
                return whdata;
            }
        }

        [Route("GetHub")]
        [HttpGet]
        public IEnumerable<Warehouse> getHub()
        {
            logger.Info("start WarehouseKppList: ");
            using (AuthContext db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                List<Warehouse> whdata = db.Warehouses.Where(p => p.CompanyId == compid && p.Deleted == false && p.active == true && (p.IsKPP == false || p.IsKppShowAsWH == true)).OrderBy(x => x.Cityid).ToList();
                return whdata;

            }
        }

        [Route("GetWarehouseCity1")]
        [HttpGet]
        public IEnumerable<Warehouse> GetWarehousecity1(int cityid)
        {
            using (AuthContext db = new AuthContext())
            {
                List<Warehouse> ass = new List<Warehouse>();

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                List<Warehouse> whdata = db.Warehouses.Where(x => x.Cityid == cityid && x.Deleted == false && x.IsKPP == false && x.active == true).ToList();
                return whdata;

            }
        }

        [Route("GetKppCity")]
        [HttpGet]
        public IEnumerable<Warehouse> GetKppCity(int cityid)
        {
            logger.Info("start Warehouse: ");
            using (AuthContext db = new AuthContext())
            {
                List<Warehouse> ass = new List<Warehouse>();

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                List<Warehouse> whdata = db.Warehouses.Where(x => x.Cityid == cityid && x.Deleted == false && x.active == false).ToList();
                return whdata;

            }
        }

        #region get Agent and Dbay Devicehistory          

        [Route("GetActiveAgentsForWarehouse")]
        [HttpGet]
        public dynamic AgentnDboyDevicehistory()
        {

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0, Warehouse_id = 0;
            // Access claims
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            int CompanyId = compid;
            using (AuthContext db = new AuthContext())
            {

                var data = db.Peoples.Where(x => x.Active == true).Select(x => new { x.DisplayName, x.PeopleID }).ToList();
                return data;
            }

        }
        #endregion

        #region get Specific Warehouses from WarehousePermissionDB for a user     
        /// <summary>
        /// 
        /// tejas 12-10-2019
        /// </summary>
        /// <returns></returns>

        [Route("getSpecificWarehouses")]
        [HttpGet]
        public dynamic getSpecificWarehouses()
        {

            try
            {

                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                string RoleNames = string.Empty;
                List<string> Roles = new List<string>();

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                    RoleNames = identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value;

                if (!string.IsNullOrEmpty(RoleNames))
                    Roles = RoleNames.Split(',').ToList();
                using (AuthContext db = new AuthContext())
                {
                    if (Roles.Any() && (Roles.Contains("HQ Master login") || Roles.Contains("Item Master Creator")))
                    {
                        var whouse = from w in db.Warehouses.Where(x => x.active == true && x.Deleted == false && (x.IsKPP == false || x.IsKppShowAsWH))
                                     select new getSpecificWarehousesDTO
                                     {
                                         WarehouseId = w.WarehouseId,
                                         WarehouseName = w.WarehouseName,
                                         CityName = w.CityName,
                                         Cityid = w.Cityid,
                                         IsDeliveryOptimizationEnabled = w.IsDeliveryOptimizationEnabled,
                                         Storetype = w.StoreType
                                     };

                        var list = whouse.OrderBy(a => a.CityName).ToList();
                        return list;
                    }
                    else
                    {
                        var whouse = from w in db.Warehouses.Where(x => x.active == true && x.Deleted == false && (x.IsKPP == false || x.IsKppShowAsWH))
                                     join p in db.WarehousePermissionDB.Where(p => p.PeopleID == userid && p.IsActive == true)
                                     on w.WarehouseId equals p.WarehouseId
                                     select new getSpecificWarehousesDTO
                                     {
                                         WarehouseId = w.WarehouseId,
                                         WarehouseName = w.WarehouseName,
                                         CityName = w.CityName,
                                         Cityid = w.Cityid,
                                         IsDeliveryOptimizationEnabled = w.IsDeliveryOptimizationEnabled,
                                         Storetype = w.StoreType
                                     };
                        var list = whouse.OrderBy(a => a.CityName).ToList();
                        return list;
                    }
                }
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
        #endregion

        #region get Specific Warehouses from WarehousePermissionDB for a user     
        /// <summary>
        /// 
        /// sudhir 11-12-2019
        /// </summary>
        /// <returns></returns>

        [Route("getSpecificWarehousesid")]
        [HttpGet]
        public dynamic getSpecificWarehousesid(int regionId)
        {

            try
            {

                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                string RoleNames = string.Empty;
                List<string> Roles = new List<string>();

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                    RoleNames = identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value;

                if (!string.IsNullOrEmpty(RoleNames))
                    Roles = RoleNames.Split(',').ToList();
                using (AuthContext db = new AuthContext())
                {
                    if (Roles.Any() && (Roles.Contains("HQ Master login") || Roles.Contains("Item Master Creator")))
                    {
                        var whouse = from w in db.Warehouses.Where(x => x.active == true && x.Deleted == false && x.IsKPP == false && x.RegionId == regionId)
                                     select new getSpecificWarehousesDTO
                                     {
                                         WarehouseId = w.WarehouseId,
                                         WarehouseName = w.WarehouseName,
                                         CityName = w.CityName,
                                     };

                        var list = whouse.OrderBy(a => a.CityName).ToList();
                        return list;
                    }
                    else
                    {
                        var whouse = from w in db.Warehouses.Where(x => x.active == true && x.Deleted == false && x.IsKPP == false && x.RegionId == regionId)
                                     join p in db.WarehousePermissionDB.Where(p => p.PeopleID == userid && p.IsActive == true)
                                     on w.WarehouseId equals p.WarehouseId
                                     select new getSpecificWarehousesDTO
                                     {
                                         WarehouseId = w.WarehouseId,
                                         WarehouseName = w.WarehouseName,
                                         CityName = w.CityName,
                                     };
                        var list = whouse.OrderBy(a => a.CityName).ToList();
                        return list;
                    }
                }
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
        #endregion
        [Route("ChangeWarehouseID")]
        [HttpGet]
        public IHttpActionResult SetWarehouseId(int wid)
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                identity.RemoveClaim(identity.FindFirst("Warehouseid"));
            identity.AddClaim(new Claim("Warehouseid", wid.ToString()));

            return Ok(identity);
        }

        [Route("GetMultipleWarehouses")]
        [HttpGet]
        public IHttpActionResult GetMultipleWarehouses()
        {
            string Warehouseids = string.Empty;
            var identity = User.Identity as ClaimsIdentity;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseids"))
                Warehouseids = identity.Claims.FirstOrDefault(x => x.Type == "Warehouseids").Value;

            return Ok(Warehouseids);
        }

        [Route("getSpecificWarehousesidForRegion")]
        [HttpPost]
        public dynamic getSpecificWarehousesidForRegion(GetRegionIds region)
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var whouse = from w in db.Warehouses.Where(x => x.active == true && x.Deleted == false && x.IsKPP == false && region.regionIds.Contains(x.RegionId))
                                 select new getSpecificWarehousesDTO
                                 {
                                     WarehouseId = w.WarehouseId,
                                     WarehouseName = w.WarehouseName,
                                     CityName = w.CityName,
                                 };

                    var list = whouse.OrderBy(a => a.CityName).ToList();
                    return list;

                }
                catch (Exception ex)
                {
                    return ex;
                }
            }
        }

        #region get Specific Cities from Warehouseid's for a user     
        /// <summary>
        /// 
        /// Vinayak 14-01-2020
        /// </summary>
        /// <returns></returns>

        [Route("getSpecificCitiesforuser")]
        [HttpGet]
        public dynamic getSpecificCitiesforuser()
        
        
        {


            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            string RoleNames = string.Empty;
            List<string> Roles = new List<string>();

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                RoleNames = identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value;

            if (!string.IsNullOrEmpty(RoleNames))
                Roles = RoleNames.Split(',').ToList();
            using (AuthContext db = new AuthContext())
            {
                if (Roles.Any() && (Roles.Contains("HQ Master login") || Roles.Contains("Item Master Creator")))
                {
                    var cities = "select distinct(w.Cityid),c.CityName from Warehouses w inner join Cities c " +
                        "on c.Cityid=w.Cityid where w.active=1 and w.IsKPP=0 and w.Deleted=0";

                    var query = db.Database.SqlQuery<SpecificWarehousesDTO>(cities).ToList();

                    return query;
                }
                else
                {
                    var cities = "select distinct(w.Cityid),c.CityName from Warehouses w inner join Cities c " +
                               "on c.Cityid=w.Cityid where w.active=1 and w.IsKPP=0 and w.Deleted=0";

                    var query = db.Database.SqlQuery<SpecificWarehousesDTO>(cities).ToList();

                    return query;
                }
            }

        }
        [Route("WhForWarkingCapital")]
        [HttpGet]
        [AllowAnonymous]
        public List<WCWarehouse> WhForWarkingCapital()
        {
            using (AuthContext db = new AuthContext())
            {
                var WCWH = "select w.WarehouseId,w.WarehouseName, w.RegionId from Warehouses " +
            "w inner join GMWarehouseProgresses b on w.WarehouseId = b.WarehouseID and b.IsLaunched=1 " +
                "and w.active=1 and w.Deleted=0 and w.IsKPP=0 and w.CityName not like '%test%' order by (w.WarehouseId)";
                var query = db.Database.SqlQuery<WCWarehouse>(WCWH).ToList();
                return query;
            }
        }



        [Route("WhForRedispatch")]
        [HttpGet]
        public List<WCWarehouse> WhForRedispatch(int peopleid)
        {
            using (AuthContext db = new AuthContext())
            {
                //var People = db.Peoples.Where(x => x.PeopleID == peopleid).SingleOrDefault();
                var WCWH = "select w.WarehouseId,w.WarehouseName, w.RegionId from Warehouses " +
            "w inner join GMWarehouseProgresses b on w.WarehouseId = b.WarehouseID and b.IsLaunched=1 " +
            "inner join WarehousePermissions c on w.WarehouseId = c.WarehouseID and c.IsActive = 1 and c.PeopleID= " + peopleid +
                "and w.active=1 and w.Deleted=0 and w.IsKPP=0 and w.CityName not like '%test%'";
                var query = db.Database.SqlQuery<WCWarehouse>(WCWH).ToList();
                return query;
            }
        }



        #endregion

        [AllowAnonymous]
        [HttpGet]
        [Route("GetDboyWarehouseBased")]
        public dynamic GetDboyWarehouseBased(int WarehouseId)
        {
            using (AuthContext db = new AuthContext())
            {
                var dboy = db.Peoples.Where(a => a.WarehouseId == WarehouseId && a.Type == "Delivery Boy" && a.Active == true).ToList();
                return dboy;

            }
        }

        [Authorize]
        [Route("OnAssign")]
        public List<WarehousePermissionInventory> GetPermissionAssign()
        {
            using (AuthContext context = new AuthContext())
            {
                List<WarehousePermissionInventory> ass = new List<WarehousePermissionInventory>();

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
                string RoleName = null;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                    RoleName = identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value.ToString();
                if (Warehouse_id > 0)
                {

                    ass = (from wh in context.Warehouses
                           where wh.active == true && wh.Deleted == false && wh.WarehouseId == Warehouse_id
                           select new WarehousePermissionInventory
                           {
                               WarehouseId = wh.WarehouseId,
                               WarehouseName = wh.WarehouseName
                           }).ToList();

                    return ass;
                }
                else if (RoleName.Contains("HQ Master login"))
                {
                    ass = (from wh in context.Warehouses
                           where wh.active == true && wh.Deleted == false
                           select new WarehousePermissionInventory
                           {
                               WarehouseId = wh.WarehouseId,
                               WarehouseName = wh.WarehouseName
                           }).ToList();

                    return ass;

                }
                else
                {
                    ass = (from whper in context.WarehousePermissionDB
                           join wh in context.Warehouses
                           on whper.WarehouseId equals wh.WarehouseId
                           where wh.active == true && wh.Deleted == false && whper.PeopleID == userid  // changes 10 Oct wh.IsKPP==false
                           select new WarehousePermissionInventory
                           {
                               WarehouseId = wh.WarehouseId,
                               WarehouseName = wh.WarehouseName
                           }).ToList();
                    //if (ass.Count == 0) {
                    //    ass = (from wh in context.Warehouses
                    //           where wh.active == true && wh.Deleted == false 
                    //           select new WarehousePermissionInventory
                    //           {
                    //               WarehouseId = wh.WarehouseId,
                    //               WarehouseName = wh.WarehouseName
                    //           }).ToList();

                    //}

                    return ass;
                }
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetWareHouseBrand")]
        public HttpResponseMessage GetWareHouseBrand()
        {
            using (AuthContext context = new AuthContext())
            {
                List<WareHouseDc> ObjWareHouseDc = GetwarehouseBrandMethod();

                //var Warehouses = (from a in context.Warehouses

                //                  .Where(x => x.Deleted == false && x.active == true && x.IsKPP == false)
                //                  select new WareHouseDc { WareHouseId = a.WarehouseId, WareHouseName = a.WarehouseName }).ToList().Distinct();

                //var Brands = (from B in context.BrandBuyerDB
                //                  //join W in context.Warehouses on B.WarehosueId equals W.WarehouseId
                //              join c in context.SubCategorys on B.BrandId equals c.SubCategoryId

                //              select new BrandBuyerDc
                //              {
                //                  BuyerId=B.BuyerId,
                //                  BrandId = B.BrandId,
                //                  SubcategoryName = c.SubcategoryName,
                //                  AllocationPercent = B.AllocationPercent
                //              }).GroupBy(n => new { n.BrandId, n.SubcategoryName, n.AllocationPercent })
                //                           .Select(g => g.FirstOrDefault())
                //                           .ToList();

                WareHouseBrand objWareHouseBrand = new WareHouseBrand
                {
                    WarehouseWareHouseDc = ObjWareHouseDc.ToList(),
                    BrandBuyer = null,

                };

                return Request.CreateResponse(HttpStatusCode.OK, objWareHouseBrand);
            }
        }


        [Authorize]
        [HttpPost]
        [Route("UpdateBrandBuyer")]
        public HttpResponseMessage UpdatebrandBuyer(List<UpdateBrandBuyer> ObjBrandBuyer)
        {
            try
            {
                bool Result = false;
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (AuthContext Context = new AuthContext())
                    {
                        // using (TransactionScope Scope = new TransactionScope())
                        //{
                        List<BrandBuyerHistory> ObjListBrandBuyerHistory = new List<BrandBuyerHistory>();
                        List<BrandBuyer> ObjListBrandBuyer = new List<BrandBuyer>();
                        foreach (var data in ObjBrandBuyer)
                        {
                            BrandBuyer ObjnewBrandBuyer = Context.BrandBuyerDB.Where(x => (x.BrandId == data.BrandId && x.WarehosueId == data.WareHouseId && x.BuyerId == data.BuyerId)).FirstOrDefault();
                            if (ObjnewBrandBuyer != null)
                            {
                                BrandBuyerHistory objBrandBuyerHistory = new BrandBuyerHistory();
                                objBrandBuyerHistory.Id = ObjnewBrandBuyer.Id;
                                objBrandBuyerHistory.WarehosueId = ObjnewBrandBuyer.WarehosueId;
                                objBrandBuyerHistory.BuyerId = ObjnewBrandBuyer.BuyerId;
                                objBrandBuyerHistory.BrandId = ObjnewBrandBuyer.BrandId;
                                objBrandBuyerHistory.CreatedDate = ObjnewBrandBuyer.CreatedDate;
                                objBrandBuyerHistory.UpdateDate = ObjnewBrandBuyer.UpdateDate;
                                objBrandBuyerHistory.Active = ObjnewBrandBuyer.Active;
                                objBrandBuyerHistory.Deleted = ObjnewBrandBuyer.Deleted;
                                objBrandBuyerHistory.AllocationPercent = ObjnewBrandBuyer.AllocationPercent;
                                objBrandBuyerHistory.UpdatedBy = ObjnewBrandBuyer.UpdatedBy;
                                objBrandBuyerHistory.BackUpdate = DateTime.Now;
                                ObjListBrandBuyerHistory.Add(objBrandBuyerHistory);
                                ObjnewBrandBuyer.AllocationPercent = data.AllocatePercent;
                                ObjnewBrandBuyer.UpdateDate = DateTime.Now;
                                ObjnewBrandBuyer.UpdatedBy = GetUserId();
                                //ObjListBrandBuyer.Add(ObjnewBrandBuyer);
                                Context.BrandBuyerDB.Attach(ObjnewBrandBuyer);
                                Context.Entry(ObjnewBrandBuyer).State = EntityState.Modified;
                            }
                            else
                            {
                                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Something went wrong please try again later");
                            }
                        }

                        Context.BrandBuyerHistory.AddRange(ObjListBrandBuyerHistory);
                        Context.SaveChanges();
                        scope.Complete();
                        Result = true;

                        //}
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, Result);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Something went wrong please try again later!!");
            }
        }
        [Authorize]
        [HttpGet]
        [Route("GetWareHouseBrandCapacity")]
        public HttpResponseMessage GetWareHouseBrandCapacity(int WareHouseId)
        {
            try
            {
                WareHouseBrandCapacityDC objWareHouseBrandCapacity = WareHouseBrandCapacity(WareHouseId);
                List<BrandCapacity> objBrandcapacity = GetBrandcapacity(WareHouseId, objWareHouseBrandCapacity.TotalWareHousecapacity);
                objWareHouseBrandCapacity = new WareHouseBrandCapacityDC
                {
                    WarehouseName = objWareHouseBrandCapacity.WarehouseName,
                    TotalWareHousecapacity = objWareHouseBrandCapacity.TotalWareHousecapacity,
                    TotalWareHouseConsumecapacity = objBrandcapacity.Sum(x => x.Capacity),
                    BrandCapacity = objBrandcapacity
                };

                return Request.CreateResponse(HttpStatusCode.OK, objWareHouseBrandCapacity);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }

        }

        private WareHouseBrandCapacityDC WareHouseBrandCapacity(int WareHouseId)
        {
            try
            {
                using (AuthContext Context = new AuthContext())
                {

                    //WareHouseBrandCapacityDC ObjWareHouseBrandCapacityDC = Context.Database.SqlQuery<WareHouseBrandCapacityDC>("GetWareHouseCapacity @WareHouseId", WareHouseIdParam).FirstOrDefault();

                    Warehouse objWarehouses = Context.Warehouses.Where(x => x.WarehouseId == WareHouseId).FirstOrDefault();

                    WareHouseBrandCapacityDC ObjWareHouseBrandCapacityDC = new WareHouseBrandCapacityDC
                    {
                        TotalWareHousecapacity = objWarehouses.CapacityinAmount,
                        WarehouseName = objWarehouses.WarehouseName
                    };
                    return ObjWareHouseBrandCapacityDC;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }



        public List<BrandCapacity> GetBrandcapacity(int WareHouseId, double WareHouseCapacity)
        {
            try
            {

                using (AuthContext Context = new AuthContext())
                {

                    var WareHouseIdParam = new SqlParameter
                    {
                        ParameterName = "WareHouseId",
                        Value = WareHouseId
                    };
                    List<BrandCapacity> ObjBrandCapacity = Context.Database.SqlQuery<BrandCapacity>("GetWareHouseCapacity @WareHouseId", WareHouseIdParam).ToList();

                    //ObjBrandCapacity = CalculateCapacity(ObjBrandCapacity, WareHouseCapacity);

                    return ObjBrandCapacity;

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        private List<BrandCapacity> CalculateCapacity(List<BrandCapacity> ObjBrandCapacity, double WareHouseCapacity)
        {
            ObjBrandCapacity.ForEach(x =>
            {

                x.Capacity = (x.AllocationPercent.HasValue) ? (WareHouseCapacity * x.AllocationPercent / 100) : 0;
            });
            return ObjBrandCapacity;
        }
        [Authorize]
        [HttpGet]
        [Route("BrandBuyerAlllocation")]
        public HttpResponseMessage BrandBuyerAlllocation(int WarehouseId)
        {
            try
            {

                using (AuthContext Context = new AuthContext())
                {

                    var WareHouseIdParam = new SqlParameter
                    {
                        ParameterName = "WareHouseId",
                        Value = WarehouseId
                    };
                    List<BrandBuyerDc> ObjBrandBuyerDc = Context.Database.SqlQuery<BrandBuyerDc>("GetBrandAllocation @WareHouseId", WareHouseIdParam).ToList();


                    return Request.CreateResponse(HttpStatusCode.OK, ObjBrandBuyerDc);

                }
                //List<BrandBuyerDc> Brands = (from B in Context.BrandBuyerDB
                //                                 //join W in context.Warehouses on B.WarehosueId equals W.WarehouseId
                //                             join c in Context.SubCategorys on B.BrandId equals c.SubCategoryId
                //                             join p in Context.Peoples on B.BuyerId equals p.PeopleID
                //                             where B.WarehosueId == WarehouseId
                //                             select new BrandBuyerDc
                //                             {
                //                                 BuyerId = B.BuyerId,
                //                                 BrandId = B.BrandId,
                //                                 SubcategoryName = c.SubcategoryName,
                //                                 AllocationPercent = B.AllocationPercent,
                //                                 BuyerName = p.DisplayName
                //                             }).ToList();

                // return Request.CreateResponse(HttpStatusCode.OK, Brands);

            }
            catch (Exception EX)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, EX.GetBaseException().Message.ToString());
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetWareHouseByuerWiseCapacity")]
        public HttpResponseMessage GetWareHouseByuerWiseCapacity(int WareHouseId)
        {
            try
            {
                WareHouseBrandCapacityDC objWareHouseBrandCapacityDC = WareHouseBrandCapacity(WareHouseId);
                List<BrandCapacity> objBrandcapacity = GetBrandcapacity(WareHouseId, objWareHouseBrandCapacityDC.TotalWareHousecapacity);
                objWareHouseBrandCapacityDC = new WareHouseBrandCapacityDC
                {
                    WarehouseName = objWareHouseBrandCapacityDC.WarehouseName,
                    TotalWareHousecapacity = objWareHouseBrandCapacityDC.TotalWareHousecapacity,
                    TotalWareHouseConsumecapacity = objBrandcapacity.Sum(x => x.ConsumeCapacity),

                };

                List<BuyerwiseCapacity> ObjGetBuyerwiseCapacity = GetWarehousecapacity(WareHouseId);

                WarehouseBuyerCapacity objWarehouseBuyerCapacity = new WarehouseBuyerCapacity
                {
                    WarehouseName = objWareHouseBrandCapacityDC.WarehouseName,
                    WarehouseCapacity = objWareHouseBrandCapacityDC.TotalWareHousecapacity,
                    WarehouseConsumeCapacity = objWareHouseBrandCapacityDC.TotalWareHouseConsumecapacity,
                    BuyerwiseCapacity = ObjGetBuyerwiseCapacity

                };
                return Request.CreateResponse(HttpStatusCode.OK, objWarehouseBuyerCapacity);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }
        }
        private List<BuyerwiseCapacity> GetWarehousecapacity(int Warehouseid)
        {

            try
            {

                using (AuthContext Context = new AuthContext())
                {

                    var WareHouseIdParam = new SqlParameter
                    {
                        ParameterName = "WareHouseId",
                        Value = Warehouseid
                    };
                    List<BuyerwiseCapacity> ObjBuyerwiseCapacity = Context.Database.SqlQuery<BuyerwiseCapacity>("GetWareHouseBuyerWiseCapacity @WareHouseId", WareHouseIdParam).ToList();
                    return ObjBuyerwiseCapacity;




                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Authorize]
        [HttpGet]
        [Route("GetBrandAcBuyer")]
        public HttpResponseMessage GetBrandAcBuyer(int BuyerId, int WareHouseId)
        {
            try
            {
                List<BrandCapacity> objListBrandCapacity = GetBrandAcBuyerMethod(BuyerId, WareHouseId);
                return Request.CreateResponse(HttpStatusCode.OK, objListBrandCapacity);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString()); ;

            }
        }
        private List<BrandCapacity> GetBrandAcBuyerMethod(int BuyerId, int WareHouseId)
        {
            try
            {
                using (AuthContext Context = new AuthContext())
                {

                    var WareHouseIdParam = new SqlParameter
                    {
                        ParameterName = "WareHouseId",
                        Value = WareHouseId
                    };

                    var BuyerIdParam = new SqlParameter
                    {
                        ParameterName = "BuyerId",
                        Value = BuyerId
                    };
                    List<BrandCapacity> ObjBrandCapacity = Context.Database.SqlQuery<BrandCapacity>("GetBrandAcBuyer @WareHouseId,@BuyerId", WareHouseIdParam, BuyerIdParam).ToList();
                    return ObjBrandCapacity;




                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<WareHouseDc> GetwarehouseBrandMethod()
        {
            try
            {
                using (AuthContext Context = new AuthContext())
                {


                    List<WareHouseDc> ObjWareHouseDc = Context.Database.SqlQuery<WareHouseDc>("GetWareHouseBrandConsume ").ToList();
                    return ObjWareHouseDc;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private int GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }

        public class WCWarehouse //Working Capital
        {
            public string WarehouseName { get; set; }
            public int WarehouseId { get; set; }
            public int RegionId { get; set; }

        }
        public class WarehousePermissionInventory
        {
            public string WarehouseName { get; set; }
            public int WarehouseId { get; set; }


        }


        public class GetRegionIds
        {
            public List<int> regionIds { get; set; }
        }
        public class WarehouseDetail
        {
            public List<Warehouse> Warehouses { get; set; }

            public bool Status { get; set; }
            public string Message { get; set; }
        }
        public class getSpecificWarehousesDTO
        {
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public string CityName { get; set; }
            public int Cityid { get; set; }
            public bool IsDeliveryOptimizationEnabled { get; set; }
            public int Storetype { get; set; }
        }
        public class SpecificWarehousesDTO
        {
            public string CityName { get; set; }
            public int Cityid { get; set; }

        }
        public class SpecificStateDTO
        {
            public int Stateid { get; set; }
            public string StateName { get; set; }

        }
        public class SpecificWarehouseDTO
        {
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; }

        }

        public class WareHouseBrand
        {
            public List<WareHouseDc> WarehouseWareHouseDc { get; set; }
            public List<BrandBuyerDc> BrandBuyer { get; set; }
        }

        public class WareHouseDc
        {
            public int WareHouseId { get; set; }
            public string WareHouseName { get; set; }

            public double? ConsumedPercentage { get; set; }
            public int? Cityid { get; set; }
            public string CityName { get; set; }
        }

        public class BrandBuyerDc
        {
            public int? BrandId { get; set; }
            public string SubcategoryName { get; set; }
            public double AllocationPercent { get; set; }
            public string BuyerName { get; set; }
            public int? BuyerId { get; set; }
        }

        public class UpdateBrandBuyer
        {
            public int WareHouseId { get; set; }
            public int BrandId { get; set; }
            public int BuyerId { get; set; }
            public double AllocatePercent { get; set; }
        }

        public class WareHouseBrandCapacityDC
        {

            public double TotalWareHousecapacity { get; set; }
            public double? TotalWareHouseConsumecapacity { get; set; }
            public string WarehouseName { get; set; }

            public List<BrandCapacity> BrandCapacity { get; set; }
        }




        public class BrandCapacity
        {
            public int WarehouseId { get; set; }
            public double? Capacity { get; set; }

            public double ConsumeCapacity { get; set; }
            public int SubsubCategoryid { get; set; }
            public string BuyerName { get; set; }
            public string SubsubcategoryName { get; set; }
            public int? BuyerId { get; set; }

            public double? AllocationPercent { get; set; }
        }

        public class BrandBuyerAlllocationDC
        {
            public double AllocationPercent { get; set; }
            public string BrandName { get; set; }
            public int BrandId { get; set; }
        }

        public class WarehouseBuyerCapacity
        {
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; }

            public double WarehouseCapacity { get; set; }

            public double? WarehouseConsumeCapacity { get; set; }

            public List<BuyerwiseCapacity> BuyerwiseCapacity { get; set; }


        }


        public class BuyerwiseCapacity
        {
            public int? BuyerId { get; set; }
            public string BuyerName { get; set; }

            public double Capacity { get; set; }

            public double ConsumeCapacity { get; set; }
        }

        public class BackendOrderResponse
        {
            public string message { get; set; }
            public string status { get; set; }
        }
    }
}

