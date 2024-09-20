using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using AngularJSAuthentication.Model.Item;
using AngularJSAuthentication.Model.ProductPerfomanceDash;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using AngularJSAuthentication.DataContracts.ProductPerformanceDash;


namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/WarehouseQuadrant")]
    public class WarehouseQuadrantController : BaseAuthController
    {
        readonly string platformIdxName = $"skorderdata_{AppConstants.Environment}";

        #region  Warehouse Quadrant Margin Upload
        //[Route("GetWarehouseQuadrantByCustomerType")]
        //[HttpPost]
        //public List<SearchWarehouseQuadrantCustomerTypeDC> GetWarehouseQuadrantByCustomerType(WarehouseQuadrantCustomerTypeDC Obj)
        //{
        //    List<SearchWarehouseQuadrantCustomerTypeDC> WarehouseQuadrantCustomerTypeData = new List<SearchWarehouseQuadrantCustomerTypeDC>();

        //    if (Obj != null)
        //    {
        //        using (var context = new AuthContext())
        //        {
        //            var IdDt = new DataTable();
        //            IdDt = new DataTable();
        //            IdDt.Columns.Add("IntValue");

        //            foreach (var item in Obj.WarehouseIDs)
        //            {
        //                var dr = IdDt.NewRow();
        //                dr["IntValue"] = item;
        //                IdDt.Rows.Add(dr);
        //            }
        //            var param = new SqlParameter("@WarehouseIDs", IdDt);
        //            param.SqlDbType = SqlDbType.Structured;
        //            param.TypeName = "dbo.intValues";

        //            var param2 = new SqlParameter("@Quadrant", Obj.Quadrant != null ? (object)Obj.Quadrant : (DBNull.Value));
        //            var param3 = new SqlParameter("@CustomerType", Obj.CustomerType != null ? (object)Obj.CustomerType : (DBNull.Value));
        //            var param4 = new SqlParameter("@skip", Obj.skip);
        //            var param5 = new SqlParameter("@take", Obj.take);

        //            WarehouseQuadrantCustomerTypeData = context.Database.SqlQuery<SearchWarehouseQuadrantCustomerTypeDC>("Exec WarehouseQuadrantMarginSearch  @WarehouseIDs,@Quadrant,@CustomerType,@skip,@take", param, param2, param3, param4, param5).ToList();

        //        }
        //    }
        //    return WarehouseQuadrantCustomerTypeData;
        //}

        //[Route("WarehouseQuadrantMarginExport")]
        //[HttpPost]
        //public List<WarehouseQuadrantMarginExport> WarehouseQuadrantMarginExport(WarehouseQuadrantCustomerTypeDC Obj)
        //{
        //    List<WarehouseQuadrantMarginExport> WarehouseQuadrantMarginExport = new List<WarehouseQuadrantMarginExport>();

        //    if (Obj != null)
        //    {
        //        using (var context = new AuthContext())
        //        {
        //            var IdDt = new DataTable();
        //            IdDt = new DataTable();
        //            IdDt.Columns.Add("IntValue");

        //            foreach (var item in Obj.WarehouseIDs)
        //            {
        //                var dr = IdDt.NewRow();
        //                dr["IntValue"] = item;
        //                IdDt.Rows.Add(dr);
        //            }
        //            var param = new SqlParameter("@WarehouseIDs", IdDt);
        //            param.SqlDbType = SqlDbType.Structured;
        //            param.TypeName = "dbo.intValues";

        //            var param2 = new SqlParameter("@Quadrant", Obj.Quadrant != null ? (object)Obj.Quadrant : (DBNull.Value));
        //            var param3 = new SqlParameter("@CustomerType", Obj.CustomerType != null ? (object)Obj.CustomerType : (DBNull.Value));

        //            WarehouseQuadrantMarginExport = context.Database.SqlQuery<WarehouseQuadrantMarginExport>("Exec WarehouseQuadrantMarginExport  @WarehouseIDs,@Quadrant,@CustomerType", param, param2, param3).ToList();
        //        }
        //    }
        //    return WarehouseQuadrantMarginExport;
        //}


        //[Route("UpdateWarehouseQuadrantByCustomerType")]
        //[HttpPost]
        //public bool UpdateWarehouseQuadrantByCustomerType(WarehouseQuadrantCustomerTypeDC Obj)
        //{
        //    bool result = false;
        //    if (Obj != null)
        //    {
        //        using (var context = new AuthContext())
        //        {
        //            var IdDt = new DataTable();
        //            IdDt = new DataTable();
        //            IdDt.Columns.Add("IntValue");

        //            foreach (var item in Obj.WarehouseIDs)
        //            {
        //                var dr = IdDt.NewRow();
        //                dr["IntValue"] = item;
        //                IdDt.Rows.Add(dr);
        //            }
        //            var param = new SqlParameter("@WarehouseIDs", IdDt);
        //            param.SqlDbType = SqlDbType.Structured;
        //            param.TypeName = "dbo.intValues";

        //            var param2 = new SqlParameter("@Quadrant", Obj.Quadrant != null ? (object)Obj.Quadrant : (DBNull.Value));
        //            var param3 = new SqlParameter("@CustomerType", Obj.CustomerType != null ? (object)Obj.CustomerType : (DBNull.Value));
        //            var param4 = new SqlParameter("@Margin", Obj.Margin);

        //            var res = context.Database.ExecuteSqlCommand("Exec UpdateWarehouseQuadrantMargin  @WarehouseIDs,@Quadrant,@CustomerType,@Margin", param, param2, param3, param4);
        //            if (res > 0)
        //            {
        //                return result = true;
        //            }
        //            else
        //            {
        //                return result;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        return result;
        //    }
        //}

        //[Route("UpdateStoreQuadrantMargin")]
        //[HttpGet]
        //public string UpdateStoreQuadrantMargin(long id, float Margin)
        //{
        //    var identity = User.Identity as ClaimsIdentity;
        //    int userid = 0;
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //    string result = "";
        //    if (id > 0 && Margin > 0)
        //    {
        //        using (var context = new AuthContext())
        //        {
        //            WarehouseBasedQuadarant obj = new WarehouseBasedQuadarant();
        //            var Data = context.WarehouseBasedQuadarants.Where(x => x.Id == id).FirstOrDefault();
        //            if (Data != null)
        //            {
        //                Data.IsActive = false;
        //                Data.IsDeleted = true;
        //                context.Entry(Data).State = EntityState.Modified;

        //                obj.MinMarginPercent = Margin;
        //                obj.WarehouseId = Data.WarehouseId;
        //                obj.CreatedDate = DateTime.Now;
        //                obj.ModifiedDate = null;
        //                obj.IsActive = true;
        //                obj.IsDeleted = false;
        //                obj.CreatedBy = userid;
        //                obj.ModifiedBy = 0;
        //                obj.ClassificationMasterId = Data.ClassificationMasterId;
        //                obj.CustomerType = Data.CustomerType;
        //                context.WarehouseBasedQuadarants.Add(obj);

        //                if (context.Commit() > 0)
        //                {
        //                    result = "Updated Succesfully";
        //                }
        //                else
        //                {
        //                    result = "Something went wrong";
        //                }
        //            }
        //            else
        //            {
        //                result = "Data not found";
        //            }

        //        }
        //    }
        //    else
        //    {
        //        result = "Not Updated";
        //    }
        //    return result;
        //}

        #endregion



        #region Product Performance Sales Mix

        [Route("GetQuadrantItemforSearch")]
        [HttpPost]
        [AllowAnonymous]
        public List<QuadrantPerformanceItemListDC> GetQuadrantItemforSearch(QuadrantPerformancesItemDc performanceitemdc)
        {
            List<QuadrantPerformanceItemListDC> Result = new List<QuadrantPerformanceItemListDC>();
            List<WarehouseQuadrantCurentDispatch> Lists = new List<WarehouseQuadrantCurentDispatch>();
            double Result2 = 0; bool IsBuyerEdit = false, IsHubLeadEdit = false;

            var identity = User.Identity as ClaimsIdentity;
            List<string> roleNames = new List<string>();
            bool IsSaleslead = false;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            if (performanceitemdc != null)
            {
                using (AuthContext db = new AuthContext())
                {
                    if (roleNames.Any(x => x == "Hub sales lead" || x == "WH Sales lead")) IsSaleslead = true;
                    else if (roleNames.Any(x => x == "Buyer" || x == "Sourcing Associate" || x == "HQ Sourcing Associate" || x == "Sourcing Executive" || x == "HQ Sourcing Executive" || x == "Sourcing Senior Executive")) IsSaleslead = false;

                    var data = db.SalesBuyerForcastConfigs.Where(x => x.IsSalesForecast == IsSaleslead && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    var currentdate = DateTime.Now.Date;
                    if (data != null && ((currentdate.Day >= data.FromDay && currentdate.Day <= data.ToDay) || data.IsAnytime))
                    {
                        IsBuyerEdit = !IsSaleslead;
                        IsHubLeadEdit = IsSaleslead;
                    }



                    if (db.Database.Connection.State != ConnectionState.Open)
                        db.Database.Connection.Open();
                    DbCommand cmd = null;


                    var IdDt2 = new DataTable();
                    IdDt2.Columns.Add("IntValue");

                    foreach (var item in performanceitemdc.BrandIds)
                    {
                        var dr = IdDt2.NewRow();
                        dr["IntValue"] = item;
                        IdDt2.Rows.Add(dr);
                    }
                    var param2 = new SqlParameter("@BrandIds", IdDt2);
                    param2.SqlDbType = SqlDbType.Structured;
                    param2.TypeName = "dbo.intValues";


                    var param = new SqlParameter("@warehouseId", performanceitemdc.warehouseId);
                    var param3 = new SqlParameter("@StoreId", performanceitemdc.StoreId);
                    var param4 = new SqlParameter("@MonthDate", performanceitemdc.MonthDate);
                    var param5 = new SqlParameter("@Status", performanceitemdc.Status);

                    if (performanceitemdc.warehouseId > 0)
                    {
                        cmd = db.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetQuadrantItemforSearch]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(param);
                        cmd.Parameters.Add(param2);
                        cmd.Parameters.Add(param3);
                        cmd.Parameters.Add(param4);
                        cmd.Parameters.Add(param5);
                        using (var reader = cmd.ExecuteReader())
                        {
                            Result = ((IObjectContextAdapter)db).ObjectContext.Translate<QuadrantPerformanceItemListDC>(reader).ToList();

                        }

                    }
                    DateTime TodayDate = performanceitemdc.MonthDate;
                    DateTime startDate = new DateTime(TodayDate.Year, TodayDate.Month, TodayDate.Day);
                    DateTime EndDate = startDate.AddMonths(1).AddDays(-1);
                    string sDate = startDate.ToString("yyyy-MM-dd");
                    string eDate = EndDate.ToString("yyyy-MM-dd");
                    string warehouseid = performanceitemdc.warehouseId.ToString();
                    string Query = $" select itemnumber,sum(dispatchqty) as ordqty  from {platformIdxName} where createddate>='{sDate}' and createddate<='{eDate}' and storeid = {performanceitemdc.StoreId} and whid={warehouseid} group by itemnumber";

                    ElasticSqlHelper<ElasticGetQuadrantPerformanceCurentDispatch> elasticSqlHelperData = new ElasticSqlHelper<ElasticGetQuadrantPerformanceCurentDispatch>();
                    var orderdetails = AsyncContext.Run(async () => (await elasticSqlHelperData.GetListAsync(Query)).ToList());

                    foreach (var item in Result)
                    {

                        item.CurrentDispatch = Math.Round(Convert.ToDouble(orderdetails.Where(x => x.itemnumber == item.ItemNumber).Sum(x => (x.ordqty))) / item.CaseSize, 2);
                        item.AchievmentPercentage = Math.Round((item.CurrentDispatch / item.CommitedForeCastInCase) * 100, 2);

                        if (item.AchievmentPercentage > 100)
                        {
                            item.DeviationPercentage = Math.Round(item.AchievmentPercentage - 100, 2);

                        }
                        else
                        {
                            item.DeviationPercentage = Math.Round(100 - item.AchievmentPercentage, 2);
                        }

                        item.IsBuyerEdit = IsBuyerEdit;
                        item.IsHubLeadEdit = IsHubLeadEdit;

                    }

                }
            }
            return Result;
        }


        [Route("ExportAllQuadrantData")]
        [HttpPost]
        [AllowAnonymous]
        public string ExportAllQuadrantData(ExportItemRequestDc performanceitemdc)
        {
            string fileUrl = "";
            List<ExpoertAllQuadrantPerformanceItemListDC> Result = new List<ExpoertAllQuadrantPerformanceItemListDC>();

            if (performanceitemdc != null)
            {
                using (AuthContext db = new AuthContext())
                {
                    if (db.Database.Connection.State != ConnectionState.Open)
                        db.Database.Connection.Open();
                    DbCommand cmd = null;

                    var IdDt2 = new DataTable();
                    IdDt2.Columns.Add("IntValue");

                    foreach (var item in performanceitemdc.BrandIds)
                    {
                        var dr = IdDt2.NewRow();
                        dr["IntValue"] = item;
                        IdDt2.Rows.Add(dr);
                    }
                    var param2 = new SqlParameter("@BrandIds", IdDt2);
                    param2.SqlDbType = SqlDbType.Structured;
                    param2.TypeName = "dbo.intValues";

                    var param = new SqlParameter("@warehouseId", performanceitemdc.warehouseId);
                    var param3 = new SqlParameter("@StoreId", performanceitemdc.StoreId);
                    var param4 = new SqlParameter("@MonthDate", performanceitemdc.MonthDate);
                    var param5 = new SqlParameter("@Status", performanceitemdc.Status);

                    if (performanceitemdc.warehouseId > 0)
                    {
                        cmd = db.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[ExportAllQuadrantData]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(param);
                        cmd.Parameters.Add(param2);
                        cmd.Parameters.Add(param3);
                        cmd.Parameters.Add(param4);
                        cmd.Parameters.Add(param5);
                        using (var reader = cmd.ExecuteReader())
                        {
                            Result = ((IObjectContextAdapter)db).ObjectContext.Translate<ExpoertAllQuadrantPerformanceItemListDC>(reader).ToList();

                        }
                    }
                    DateTime TodayDate = performanceitemdc.MonthDate;
                    DateTime startDate = new DateTime(TodayDate.Year, TodayDate.Month, TodayDate.Day);
                    DateTime EndDate = startDate.AddMonths(1).AddDays(-1);
                    string sDate = startDate.ToString("yyyy-MM-dd");
                    string eDate = EndDate.ToString("yyyy-MM-dd");
                    string warehouseid = performanceitemdc.warehouseId.ToString();
                    string Query = $" select itemnumber,sum(dispatchqty) as ordqty  from {platformIdxName} where createddate>='{sDate}' and createddate<='{eDate}' and storeid = {performanceitemdc.StoreId} and whid={warehouseid} group by itemnumber";

                    ElasticSqlHelper<ElasticGetQuadrantPerformanceCurentDispatch> elasticSqlHelperData = new ElasticSqlHelper<ElasticGetQuadrantPerformanceCurentDispatch>();
                    var orderdetails = AsyncContext.Run(async () => (await elasticSqlHelperData.GetListAsync(Query)).ToList());

                    foreach (var item in Result)
                    {
                        item.MTD = Math.Round(Convert.ToDouble(orderdetails.Where(x => x.itemnumber == item.ItemNumber).Sum(x => (x.ordqty))) / item.MOQ, 2);
                        item.AchievmentPercentage = Math.Round((item.MTD / item.SalesForecast) * 100, 2);

                        if (item.AchievmentPercentage > 100)
                        {
                            item.DeviationPercentage = Math.Round(item.AchievmentPercentage - 100, 2);

                        }
                        else
                        {
                            item.DeviationPercentage = Math.Round(100 - item.AchievmentPercentage, 2);
                        }

                    }
                }
            }
            if (Result != null)
            {
                DataTable dt = ListtoDataTableConverter.ToDataTable(Result);
                string fileName = "ForecastData" + DateTime.Now.ToString("yyyyddMHHmmss") + ".xlsx";

                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                ExcelGenerator.DataTable_To_Excel(dt, "ForecastData", path);

                fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri(HttpContext.Current.Request.UrlReferrer.AbsoluteUri).Scheme
                                                                 , HttpContext.Current.Request.Url.DnsSafeHost
                                                                 , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                 , string.Format("ExcelGeneratePath/{0}", fileName));


            }

            return fileUrl;
        }




        [Route("ExportAllProductPerformanceData")]
        [HttpGet]
        [AllowAnonymous]
        public APIResponse ExportOverAllData()
        {
            APIResponse res = new APIResponse();
            string fileUrl = "";
            List<ExpoertAllQuadrantPerformanceItemListDC> Result = new List<ExpoertAllQuadrantPerformanceItemListDC>();


            DateTime TodayDate = DateTime.Now.Date;
            int Month = TodayDate.Month;
            int Year = TodayDate.Year;

            var identity = User.Identity as ClaimsIdentity;
            List<string> roleNames = new List<string>();

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            if (roleNames.Any(x => x == "Zonal sales lead" || x == "HQ Sales Lead" || x == "HQ Sourcing Lead" || x == "Buyer" || x == "Region sales lead" || x == "HQ Master login"))
            {
                using (AuthContext db = new AuthContext())
                {
                    if (db.Database.Connection.State != ConnectionState.Open)
                        db.Database.Connection.Open();
                    DbCommand cmd = null;

                    var param = new SqlParameter("@Month", Month);
                    var param2 = new SqlParameter("@Year", Year);

                    cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[ExportAllProductPerformanceData]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.Add(param);
                    cmd.Parameters.Add(param2);

                    using (var reader = cmd.ExecuteReader())
                    {
                        Result = ((IObjectContextAdapter)db).ObjectContext.Translate<ExpoertAllQuadrantPerformanceItemListDC>(reader).ToList();
                    }

                    //CFR_Result = db.Database.SqlQuery<CFR_ProductPerformanceDc>("exec CFR_ProductPerformance ").ToList();

                    DateTime startDate = new DateTime(TodayDate.Year, TodayDate.Month, 1);
                    DateTime EndDate = startDate.AddMonths(1).AddDays(-1);
                    string sDate = startDate.ToString("yyyy-MM-dd");
                    string eDate = EndDate.ToString("yyyy-MM-dd");

                    string Query = $" select itemnumber,sum(dispatchqty) as ordqty  from {platformIdxName} where createddate>='{sDate}' and createddate<='{eDate}' group by itemnumber";

                    ElasticSqlHelper<ElasticGetQuadrantPerformanceCurentDispatch> elasticSqlHelperData = new ElasticSqlHelper<ElasticGetQuadrantPerformanceCurentDispatch>();
                    var orderdetails = AsyncContext.Run(async () => (await elasticSqlHelperData.GetListAsync(Query)).ToList());

                    var DateDay = DateTime.Now.AddDays(-1);
                    foreach (var item in Result)
                    {
                        item.MTD = Math.Round(Convert.ToDouble(orderdetails.Where(x => x.itemnumber == item.ItemNumber).Sum(x => (x.ordqty))) / item.MOQ, 2);

                        item.AchievmentPercentage = Math.Round((item.MTD / item.SalesForecast) * 100, 2);
                        if (item.AchievmentPercentage > 100)
                        {
                            item.DeviationPercentage = Math.Round(item.AchievmentPercentage - 100, 2);

                        }
                        else
                        {
                            item.DeviationPercentage = Math.Round(100 - item.AchievmentPercentage, 2);
                        }

                        var Value = Math.Round(Convert.ToDouble(item.SalesForecast) / 30, 3) * DateDay.Day;
                        if (item.MTD < Value)
                        {
                            item.MTDStatus = "Laggard";
                        }
                        else if (item.MTD > Value && item.MTD < item.SalesForecast)
                        {
                            item.MTDStatus = "OnTrack";
                        }
                        else if (item.MTD >= item.SalesForecast)
                        {
                            item.MTDStatus = "Acchiver";
                        }


                    }
                    List<DataTable> dt = new List<DataTable>();
                    DataTable AllExportProductPerformance = new DataTable();
                    AllExportProductPerformance.TableName = "AllExportProductPerformance";
                    dt.Add(AllExportProductPerformance);

                    AllExportProductPerformance.Columns.Add("ID");
                    AllExportProductPerformance.Columns.Add("WarehouseName");
                    AllExportProductPerformance.Columns.Add("ItemNumber");
                    AllExportProductPerformance.Columns.Add("ItemName");
                    AllExportProductPerformance.Columns.Add("Status");
                    AllExportProductPerformance.Columns.Add("MOQ");
                    AllExportProductPerformance.Columns.Add("MRP");
                    AllExportProductPerformance.Columns.Add("ASP");
                    AllExportProductPerformance.Columns.Add("NewASP");
                    AllExportProductPerformance.Columns.Add("BuyerRemark");
                    AllExportProductPerformance.Columns.Add("Median");
                    AllExportProductPerformance.Columns.Add("SystemForecast");
                    AllExportProductPerformance.Columns.Add("MinValue");
                    AllExportProductPerformance.Columns.Add("MaxValue");
                    AllExportProductPerformance.Columns.Add("SalesForecast");
                    AllExportProductPerformance.Columns.Add("SalesRemark");
                    AllExportProductPerformance.Columns.Add("MTD");
                    AllExportProductPerformance.Columns.Add("AchievmentPercentage");
                    AllExportProductPerformance.Columns.Add("DeviationPercentage");
                    AllExportProductPerformance.Columns.Add("MultiBuyerComents");
                    AllExportProductPerformance.Columns.Add("MultiSalesComents");
                    AllExportProductPerformance.Columns.Add("CFRStatus");
                    AllExportProductPerformance.Columns.Add("MTDStatus");


                    DataTable CFR_ProductPerformance = new DataTable();
                    CFR_ProductPerformance.TableName = "CFR_ProductPerformance";
                    dt.Add(CFR_ProductPerformance);

                    CFR_ProductPerformance.Columns.Add("WarehouseName");
                    CFR_ProductPerformance.Columns.Add("Green");
                    CFR_ProductPerformance.Columns.Add("Red");
                    CFR_ProductPerformance.Columns.Add("SKU_Planned");
                    CFR_ProductPerformance.Columns.Add("CFRPercentage");
                    CFR_ProductPerformance.Columns.Add("Sales_Achivment");
                    CFR_ProductPerformance.Columns.Add("Laggard");
                    CFR_ProductPerformance.Columns.Add("Acchiver");
                    CFR_ProductPerformance.Columns.Add("OnTrack");

                    if (Result != null)
                    {
                        foreach (var item in Result)
                        {
                            var dr = AllExportProductPerformance.NewRow();
                            dr["ID"] = item.ID;
                            dr["WarehouseName"] = item.WarehouseName;
                            dr["ItemNumber"] = item.ItemNumber;
                            dr["ItemName"] = item.ItemName;
                            dr["Status"] = item.Status;
                            dr["MOQ"] = item.MOQ;
                            dr["MRP"] = item.MRP;
                            dr["ASP"] = item.ASP;
                            dr["NewASP"] = item.NewASP;
                            dr["BuyerRemark"] = item.BuyerRemark;
                            dr["Median"] = item.Median;
                            dr["SystemForecast"] = item.SystemForecast;
                            dr["MinValue"] = item.MinValue;
                            dr["MaxValue"] = item.MaxValue;
                            dr["SalesForecast"] = item.SalesForecast;
                            dr["SalesRemark"] = item.SalesRemark;
                            dr["MTD"] = item.MTD;
                            dr["AchievmentPercentage"] = item.AchievmentPercentage;
                            dr["DeviationPercentage"] = item.DeviationPercentage;
                            dr["MultiBuyerComents"] = item.MultiBuyerComents;
                            dr["MultiSalesComents"] = item.MultiSalesComents;
                            dr["CFRStatus"] = item.CFRStatus;
                            dr["MTDStatus"] = item.MTDStatus;
                            AllExportProductPerformance.Rows.Add(dr);
                        }
                    }

                    List<warehouselist> list = Result.GroupBy(x => new { x.WarehouseId, x.WarehouseName }).Select(x => new warehouselist { value = x.Key.WarehouseId, label = x.Key.WarehouseName }).ToList();

                    foreach (var item in list)
                    {
                        var Green = Result.Where(x => x.CFRStatus == "True" && x.WarehouseId == item.value).Count();
                        var Laggard = Result.Where(x => x.SalesForecast > 0 && x.MTDStatus == "Laggard" && x.WarehouseId == item.value).Count();
                        var Acchiver = Result.Where(x => x.SalesForecast > 0 && x.MTDStatus == "Acchiver" && x.WarehouseId == item.value).Count();
                        var OnTrack = Result.Where(x => x.SalesForecast > 0 && x.MTDStatus == "OnTrack" && x.WarehouseId == item.value).Count();
                        var SKU_Planned = Result.Where(x => x.SalesForecast > 0 && x.WarehouseId == item.value).Count();


                        var dr = CFR_ProductPerformance.NewRow();
                        dr["WarehouseName"] = item.label;
                        dr["Green"] = Green;
                        dr["Red"] = Result.Where(x => x.CFRStatus == "False" && x.WarehouseId == item.value).Count();
                        dr["SKU_Planned"] = SKU_Planned;
                        dr["CFRPercentage"] = Math.Round(Convert.ToDouble(Green) / SKU_Planned, 3) * 100;
                        dr["Sales_Achivment"] = SKU_Planned > 0 ? ((Acchiver + OnTrack) / SKU_Planned) * 100 : 0;
                        dr["Laggard"] = Laggard;
                        dr["Acchiver"] = Acchiver;
                        dr["OnTrack"] = OnTrack;
                        CFR_ProductPerformance.Rows.Add(dr);
                    }


                    if (AllExportProductPerformance.Rows.Count > 0)
                    {
                        string fileName = "AllForecastData" + DateTime.Now.ToString("yyyyddMHHmmss") + ".xlsx";
                        string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                        if (ExcelGenerator.DataTable_To_Excel(dt, path))
                        {
                            fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                             , HttpContext.Current.Request.Url.DnsSafeHost
                                                                             , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                            , string.Format("ExcelGeneratePath/{0}", fileName));

                        }
                    }

                    //if (Result != null)
                    //{
                    //    DataTable dt = ListtoDataTableConverter.ToDataTable(Result);
                    //    string fileName = "ForecastData" + DateTime.Now.ToString("yyyyddMHHmmss") + ".xlsx";

                    //    string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                    //    ExcelGenerator.DataTable_To_Excel(dt, "ForecastData", path);

                    //    fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                    //                                                     , HttpContext.Current.Request.Url.DnsSafeHost
                    //                                                     , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                    //                                                    , string.Format("ExcelGeneratePath/{0}", fileName));


                    //}

                    if (fileUrl != null && fileUrl != "")
                    {
                        res.Status = true;
                        res.Message = "Generated Successfully";
                        res.Data = fileUrl;
                    }

                    else
                    {
                        res.Status = false;
                        res.Message = "Not Generated";
                    }
                }
            }
            else
            {
                res.Status = false;
                res.Message = "You are not Authorized";
            }
            return res;
        }




        [Route("GetStoresBySalesLead")]
        [HttpGet]
        public List<WarehouseQuadrantStoreList> GetStoresBySalesLead(int warehouseid)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            List<WarehouseQuadrantStoreList> result = new List<WarehouseQuadrantStoreList>();
            if (warehouseid > 0 && userid > 0)
            {
                using (var context = new AuthContext())
                {
                    var param = new SqlParameter("@Warehouseid", warehouseid);
                    var param2 = new SqlParameter("@userid", userid);
                    result = context.Database.SqlQuery<WarehouseQuadrantStoreList>("Exec GetStoresBySalesLead  @Warehouseid,@userid", param, param2).ToList();
                }
            }
            return result;
        }

        [Route("GetWarehouseQuadrantBrandList")]
        [HttpGet]
        public List<BrandList> GetWarehouseQuadrantBrandList(int warehouseid, int storeid)
        {
            List<BrandList> result = new List<BrandList>();
            if (warehouseid > 0 && storeid > 0)
            {
                using (var db = new AuthContext())
                {
                    if (db.Database.Connection.State != ConnectionState.Open)
                        db.Database.Connection.Open();
                    DbCommand cmd = null;

                    var param = new SqlParameter("@warehouseid", warehouseid);
                    var param3 = new SqlParameter("@Storeid", storeid);

                    cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetWarehouseQuadrantBrandList]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(param);
                    cmd.Parameters.Add(param3);

                    using (var reader = cmd.ExecuteReader())
                    {
                        result = ((IObjectContextAdapter)db).ObjectContext.Translate<BrandList>(reader).ToList();
                    }

                }
            }
            return result;
        }




        [Route("UpdateSalesForcastValue")]
        [HttpPost]
        public async Task<APIResponse> SaveQuadrantDetails(QuadrantDetailPostDc obj)
        {
            APIResponse res = new APIResponse();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            double OldCaseSize = 0;

            foreach (Claim claim in identity.Claims)
            {
                if (claim.Type == "userid")
                {
                    userid = int.Parse(claim.Value);
                }
            }
            using (var db = new AuthContext())
            {
                if (obj != null && obj.Id > 0)
                {
                    var quadrant = db.QuadrantDetails.Where(x => x.Id == obj.Id).FirstOrDefault();

                    if (quadrant != null)
                    {
                        OldCaseSize = quadrant.CaseSize;
                        if (roleNames.Any(x => x == "Hub sales lead" || x == "WH Sales lead"))
                        {
                            if (obj.CommitedForeCastValue > 0)
                            {
                                quadrant.CommitedForeCastValue = obj.CommitedForeCastValue;
                                quadrant.CommitedForeCastInCase = obj.CommitedForeCastValue;
                                if ((obj.CommitedForeCastValue < obj.MinValue || obj.CommitedForeCastValue > obj.MaxValue) && quadrant.Status != "Approved")
                                {
                                    quadrant.Status = "Approved";//"Approval Pending";
                                }
                                else
                                {
                                    quadrant.Status = "Approved";
                                }
                            }

                            quadrant.ModifiedBy = userid;
                            quadrant.ModifiedDate = DateTime.Now;

                        }
                        if (roleNames.Any(x => x == "Buyer" || x == "Sourcing Executive" || x == "HQ Sourcing Executive" || x == "Sourcing Associates" || x == "HQ Sourcing Associates"))
                        {
                            if (obj.CaseSize > quadrant.CaseSize || obj.CaseSize < quadrant.CaseSize)
                            {
                                quadrant.CaseSize = obj.CaseSize;
                            }
                            quadrant.NewASP = obj.NewASP;
                            quadrant.ModifiedBy = userid;
                            quadrant.ModifiedDate = DateTime.Now;
                        }

                        QuadrantItemDetailHistory history = new QuadrantItemDetailHistory();
                        if (obj.CaseSize > OldCaseSize || obj.CaseSize < OldCaseSize)
                        {
                            history.CaseSize = obj.CaseSize;
                        }
                        history.Forcast = obj.CommitedForeCastValue;
                        history.ASP = obj.NewASP;
                        history.BuyerRemark = obj.BuyerRemark;
                        history.SalesRemark = obj.SalesRemark;
                        history.QuadrantDetailId = obj.Id;
                        history.CreatedDate = DateTime.Now;
                        history.CreatedBy = userid;
                        history.IsActive = true;
                        history.IsDeleted = false;
                        db.QuadrantItemDetailHistories.Add(history);

                        db.Entry(quadrant).State = EntityState.Modified;
                    }
                    else
                    {
                        res.Status = false;
                        res.Message = "Data not Found";
                    }

                    if (db.Commit() > 0)
                    {
                        res.Status = true;
                        res.Message = "Successfully Saved";
                    }
                    else
                    {
                        res.Status = false;
                        res.Message = "failed";
                    }
                }
                else
                {
                    res.Status = false;
                    res.Message = "Something Went Wrong";
                }
            }
            return res;
        }


        [HttpGet]
        [Route("GetOverAllMedian")]
        [AllowAnonymous]
        public DataTable GetOverAllMedian(string ItemNumber)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
            DataTable data = new DataTable();
            using (var connection = new SqlConnection(connectionString))
            {
                string query = "EXEC GetWarehouseAllMedian " + ItemNumber;
                using (var command = new SqlCommand(query, connection))
                {
                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    da.Fill(data);
                    da.Dispose();
                    connection.Close();
                    return data;
                }
            }
        }


        #region SalesAPI


        [Route("Getproductsalesitemsearch")]
        [HttpPost]
        [AllowAnonymous]
        public GetproductPerformanceDashboard Getproductsalesitemsearch(Getproductsalesitempayload getproductsalespayload)
        {
            List<GetQuadrantItemforSearchListDc> dbResult = new List<GetQuadrantItemforSearchListDc>();
            List<WarehouseQuadrantCurentDispatch> Lists = new List<WarehouseQuadrantCurentDispatch>();
            GetproductPerformanceDashboard result = new GetproductPerformanceDashboard { Items = new List<Getproductsalesappitemdashboard>() };

            var dashboardItems = new List<Getproductsalesappitemdashboard>();


            if (getproductsalespayload != null)
            {
                using (AuthContext db = new AuthContext())
                {
                    string storeids = "";
                    var storelist = db.WarehouseStoreMappings.Where(x => x.SalesLeadId == getproductsalespayload.peopleId && x.IsActive == true && x.IsDeleted == false).Select(y => y.StoreId).Distinct().ToList();
                    if (storelist.Count > 0)
                    {
                        storeids = String.Join(",", storelist);
                    }
                    else { storeids = "0"; }
                    if (db.Database.Connection.State != ConnectionState.Open)
                        db.Database.Connection.Open();
                    DbCommand cmd = null;

                    var IdDt2 = new DataTable();
                    IdDt2.Columns.Add("IntValue");

                    foreach (var item in storelist)
                    {
                        var dr = IdDt2.NewRow();
                        dr["IntValue"] = item;
                        IdDt2.Rows.Add(dr);
                    }
                    var param2 = new SqlParameter("@StoreIds", IdDt2);
                    param2.SqlDbType = SqlDbType.Structured;
                    param2.TypeName = "dbo.intValues";


                    var IdBrand = new DataTable();
                    IdBrand.Columns.Add("IntValue");

                    foreach (var item in getproductsalespayload.BrandIds)
                    {
                        var dr = IdBrand.NewRow();
                        dr["IntValue"] = item;
                        IdBrand.Rows.Add(dr);
                    }
                    var param5 = new SqlParameter("@brandids", IdBrand);
                    param5.SqlDbType = SqlDbType.Structured;
                    param5.TypeName = "dbo.intValues";


                    var param = new SqlParameter("@warehouseId", getproductsalespayload.warehouseId);
                    var param1 = new SqlParameter("@Month", getproductsalespayload.Month);
                    var param3 = new SqlParameter("@Year", getproductsalespayload.Year);
                    if (getproductsalespayload.warehouseId > 0)
                    {
                        cmd = db.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[Sp_getproductsalesitemsearch]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(param);
                        cmd.Parameters.Add(param1);
                        cmd.Parameters.Add(param2);
                        cmd.Parameters.Add(param3);
                        cmd.Parameters.Add(param5);
                        using (var reader = cmd.ExecuteReader())
                        {
                            dbResult = ((IObjectContextAdapter)db).ObjectContext.Translate<GetQuadrantItemforSearchListDc>(reader).ToList();
                        }
                    }
                    DateTime TodayDate = new DateTime(getproductsalespayload.Year, getproductsalespayload.Month, 1);
                    DateTime startDate = new DateTime(TodayDate.Year, TodayDate.Month, TodayDate.Day);
                    DateTime EndDate = startDate.AddMonths(1).AddDays(-1);
                    string sDate = startDate.ToString("yyyy-MM-dd");
                    string eDate = EndDate.ToString("yyyy-MM-dd");
                    string warehouseid = getproductsalespayload.warehouseId.ToString();
                    string Query = $" select itemnumber,sum(dispatchqty) as ordqty from {platformIdxName} where createddate>='{sDate}' and createddate<='{eDate}' and storeid in ({storeids}) and whid={warehouseid} group by itemnumber";

                    ElasticSqlHelper<ElasticGetQuadrantPerformanceCurentDispatch> elasticSqlHelperData = new ElasticSqlHelper<ElasticGetQuadrantPerformanceCurentDispatch>();
                    var orderdetails = AsyncContext.Run(async () => (await elasticSqlHelperData.GetListAsync(Query)).ToList());

                    var DateDay = DateTime.Now.AddDays(-1);
                    double totalacheivement = 0;
                    foreach (var item in dbResult)
                    {
                        item.MTD = Math.Round(Convert.ToDouble(orderdetails.Where(x => x.itemnumber == item.ItemNumber).Sum(x => (x.ordqty))) / item.MOQ, 2);
                        var Value = Math.Round(Convert.ToDouble(item.SalesForecast) / 30, 3) * DateDay.Day;
                        if (item.MTD < Value)
                        {
                            item.MTDStatus = "Laggard";
                        }
                        else if (item.MTD > Value && item.MTD < item.SalesForecast)
                        {
                            item.MTDStatus = "OnTrack";
                        }
                        else if (item.MTD >= item.SalesForecast)
                        {
                            item.MTDStatus = "Acchiver";
                        }
                        dashboardItems.Add(new Getproductsalesappitemdashboard
                        {
                            ASP = item.NewASP,
                            itemName = item.itemname,
                            ItemNumber = item.ItemNumber,
                            Salesforecast = item.CommitedForeCastValue,
                            Mtdsales = Math.Round(orderdetails.Where(s => s.itemnumber == item.ItemNumber).Select(s => s.ordqty).FirstOrDefault() / item.PurchaseMOQ, 2),
                            Acheivement = item.CommitedForeCastValue > 0 ? Math.Round((Math.Round(orderdetails.Where(s => s.itemnumber == item.ItemNumber).Select(s => s.ordqty).FirstOrDefault() / item.PurchaseMOQ, 2)
                                        / item.CommitedForeCastValue) * 100, 2) : 0,
                            deviation =
                            item.CommitedForeCastValue > 0 ?
                            Math.Round((Math.Round(orderdetails.Where(s => s.itemnumber == item.ItemNumber).Select(s => s.ordqty).FirstOrDefault() / item.PurchaseMOQ, 2)
                                        / item.CommitedForeCastValue) * 100, 2) > 100 ?
                                Math.Round((Math.Round(orderdetails.Where(s => s.itemnumber == item.ItemNumber).Select(s => s.ordqty).FirstOrDefault() / item.PurchaseMOQ, 2)
                                        / item.CommitedForeCastValue) * 100, 2) - 100 :
                                100 - Math.Round((Math.Round(orderdetails.Where(s => s.itemnumber == item.ItemNumber).Select(s => s.ordqty).FirstOrDefault() / item.PurchaseMOQ, 2)
                                        / item.CommitedForeCastValue) * 100, 2)
                                        : 100

                        });

                    }

                    var Acchiver = dbResult.Where(x => x.SalesForecast > 0 && x.MTDStatus == "Acchiver").Count();
                    var OnTrack = dbResult.Where(x => x.SalesForecast > 0 && x.MTDStatus == "OnTrack").Count();
                    var SKU_Planned = dbResult.Where(x => x.SalesForecast > 0).Count();
                    totalacheivement = SKU_Planned > 0 ? ((Acchiver + OnTrack) / SKU_Planned) * 100 : 0;
                    result.Items = dashboardItems;
                    result.TotalCount = result.Items.Count;
                    result.totalsalesforecast = result.Items.Sum(s => s.Salesforecast);
                    result.totalmtdsales = result.Items.Sum(s => s.Mtdsales);
                    result.TotalAcheivement = totalacheivement;//result.totalsalesforecast > 0 ? Math.Round(result.totalmtdsales / result.totalsalesforecast, 2) : 0;
                    result.totaldeviation = result.Items.Count > 0 ? Math.Round(result.Items.Select(s => s.deviation).Average(), 2) : 0;
                    result.Items = result.Items
                                .OrderBy(x => getproductsalespayload.Key == "saleforecaselh" ?
                                            x.Salesforecast : getproductsalespayload.Key == "mtdsaleslh"
                                            ? x.Mtdsales : getproductsalespayload.Key == "acheivementlh"
                                            ? x.Acheivement : getproductsalespayload.Key == "deviationlh"
                                            ? x.deviation : getproductsalespayload.Key == "saleforecasehl"
                                            ? -x.Salesforecast : getproductsalespayload.Key == "mtdsaleshl"
                                            ? -x.Mtdsales : getproductsalespayload.Key == "acheivementhl"
                                            ? -x.Acheivement : getproductsalespayload.Key == "deviationhl"
                                            ? -x.deviation : x.Acheivement)
                                .ToList();
                }
            }
            return result;
        }

        [Route("GetAllBrandid")]
        [HttpGet]
        public List<BrandList> GetAllBrandid(int peopleid, int warehouseid)
        {
            List<BrandList> result = new List<BrandList>();
            if (peopleid > 0 && warehouseid > 0)
            {
                using (var db = new AuthContext())
                {
                    if (db.Database.Connection.State != ConnectionState.Open)
                        db.Database.Connection.Open();
                    DbCommand cmd = null;

                    var storelist = db.WarehouseStoreMappings.Where(x => x.SalesLeadId == peopleid && x.IsActive == true && x.IsDeleted == false).Select(y => y.StoreId).Distinct().ToList();
                    var param = new SqlParameter("@warehouseid", warehouseid);
                    var IdStores = new DataTable();
                    IdStores.Columns.Add("IntValue");

                    foreach (var item in storelist)
                    {
                        var dr = IdStores.NewRow();
                        dr["IntValue"] = item;
                        IdStores.Rows.Add(dr);
                    }
                    var param3 = new SqlParameter("@Storeid", IdStores);
                    param3.SqlDbType = SqlDbType.Structured;
                    param3.TypeName = "dbo.intValues";

                    cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[Sp_GetWarehouseQuadrantBrandListforsalesapp]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(param);
                    cmd.Parameters.Add(param3);

                    using (var reader = cmd.ExecuteReader())
                    {
                        result = ((IObjectContextAdapter)db).ObjectContext.Translate<BrandList>(reader).ToList();
                    }

                }
            }
            return result;
        }


        #endregion


        [HttpPost]
        [Route("InsertSBForcastConfig")]
        [AllowAnonymous]
        public APIResponse InsertSBForcastConfig(SBForcastConfigDC ConfigDC)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var context = new AuthContext())
            {
                if (ConfigDC.Id > 0)
                {
                    var data = context.SalesBuyerForcastConfigs.Where(x => x.Id == ConfigDC.Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    data.FromDay = ConfigDC.FromDay;
                    data.ToDay = ConfigDC.ToDay;
                    data.IsAnytime = ConfigDC.IsAnytime;
                    data.ModifiedBy = userid;
                    data.ModifiedDate = DateTime.Now;
                    context.Entry(data).State = EntityState.Modified;
                }
                else
                {
                    var data = Mapper.Map(ConfigDC).ToANew<SalesBuyerForcastConfig>();
                    data.IsActive = true;
                    data.IsDeleted = false;
                    data.CreatedBy = userid;
                    data.CreatedDate = DateTime.Now;
                    context.SalesBuyerForcastConfigs.Add(data);
                }
                if (context.Commit() > 0)
                    return new APIResponse { Status = true, Message = "Successfully" };
                else
                    return new APIResponse { Status = false };
            }
        }



        [HttpGet]
        [Route("GetConfigDatabyWid")]
        [AllowAnonymous]
        public APIResponse GetConfigDatabyWid()
        {
            using (var context = new AuthContext())
            {
                var data = context.SalesBuyerForcastConfigs.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                if (data != null && data.Count > 0)
                    return new APIResponse { Status = true, Data = data };
                else
                    return new APIResponse { Status = false, Message = "Data Not Found" };
            }
        }



        [Route("UploadQuadrantFile")]
        [HttpPost]
        
        public IHttpActionResult UploadQuadrantFile()
        {
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var formData1 = HttpContext.Current.Request.Form["compid"];
                var identity = User.Identity as ClaimsIdentity;
                List<string> roleNames = new List<string>();
                List<QuadrantDetail> Quadrantdetails = new List<QuadrantDetail>();
                List<long> QuadrantdetailIds = new List<long>();
                int userid = 0; string Rolename = "";
                bool isbuyer = false;
                bool issaleslead = false;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                    roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

                if (roleNames.Any(x => x == "Hub sales lead" || x == "CM5 lead"))
                    issaleslead = true;
                else if (roleNames.Any(x => x == "Sourcing Associate" || x == "Buyer" || x == "HQ Sourcing Associate" || x == "Sourcing Executive" || x == "HQ Sourcing Executive"))
                    isbuyer = true;
                else if (roleNames.Any(x => x == "HQ Master login"))
                    isbuyer = true;
                   issaleslead = true;

                DateTime cdate = DateTime.Now;
                string Col0, col1, col2, col3, col4, col5;
                string MSG = "";
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                if (httpPostedFile != null)
                {
                    using (var db = new AuthContext())
                    {
                        // var data = db.QuadrantDetails.ToList();
                        List<QuadrantItemDetailHistory> quadrantItemDetailHistories = new List<QuadrantItemDetailHistory>();
                        string ext = Path.GetExtension(httpPostedFile.FileName);
                        if (ext == ".xlsx" || ext == ".xls")
                        {
                            byte[] buffer = new byte[httpPostedFile.ContentLength];
                            using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
                            {
                                br.Read(buffer, 0, buffer.Length);
                            }
                            XSSFWorkbook hssfwb;
                            using (MemoryStream memStream = new MemoryStream())
                            {
                                BinaryFormatter binForm = new BinaryFormatter();
                                memStream.Write(buffer, 0, buffer.Length);
                                memStream.Seek(0, SeekOrigin.Begin);
                                hssfwb = new XSSFWorkbook(memStream);
                                string sSheetName = hssfwb.GetSheetName(0);
                                ISheet sheet = hssfwb.GetSheet(sSheetName);
                                IRow rowData;
                                ICell cellData = null;
                                int? IdCellIndex = null;
                                int? ForecastCellIndex = null;
                                int? buyerRemarkCellIndex = null;
                                int? salesRemarkCellIndex = null;
                                int? ASPCellIndex = null;
                                int? CaseSizecellindex = null;
                                int Id, Forecast;
                                string buyerComment, salescomment ;
                                double ASP, casesize;
                                List<QuadrantItemDetailHistory> list = new List<QuadrantItemDetailHistory>();

                                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)
                                {
                                    if (iRowIdx == 0)
                                    {
                                        rowData = sheet.GetRow(iRowIdx);
                                        if (rowData != null)
                                        {
                                            string strJSON = null;
                                            string field = string.Empty;
                                            field = rowData.GetCell(0).ToString();
                                            IdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "ID") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "ID").ColumnIndex : (int?)null;
                                            if (!IdCellIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Id does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }
                                        }
                                    }
                                    if (iRowIdx > 0)
                                    {
                                        long ID = 0;
                                        rowData = sheet.GetRow(iRowIdx);
                                        if (rowData != null && rowData.Cells.Count > 0)
                                        {
                                            rowData = sheet.GetRow(iRowIdx);
                                            cellData = rowData.GetCell(IdCellIndex.Value);
                                            Col0 = cellData == null ? "" : cellData.ToString();
                                            ID = Convert.ToInt64(Col0);
                                            if (ID > 0)
                                            {
                                                QuadrantdetailIds.Add(ID);
                                            }
                                        }
                                    }
                                }
                                var Ids = db.QuadrantDetails.Where(x => QuadrantdetailIds.Contains(x.Id)).ToList();
                                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                                {
                                    if (iRowIdx == 0)
                                    {
                                        rowData = sheet.GetRow(iRowIdx);
                                        if (rowData != null)
                                        {
                                            string strJSON = null;
                                            string field = string.Empty;
                                            field = rowData.GetCell(0).ToString();
                                            IdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "ID") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "ID").ColumnIndex : (int?)null;
                                            if (!IdCellIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Id does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }


                                            ForecastCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "SalesForecast") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "SalesForecast").ColumnIndex : (int?)null;

                                            if (!ForecastCellIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Sales Forecast does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }

                                            buyerRemarkCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "BuyerRemark") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "BuyerRemark").ColumnIndex : (int?)null;

                                            if (!buyerRemarkCellIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("BuyerRemark does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }
                                            salesRemarkCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "SalesRemark") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "SalesRemark").ColumnIndex : (int?)null;

                                            if (!salesRemarkCellIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("SalesRemark does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }
                                            ASPCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "NewASP") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "NewASP").ColumnIndex : (int?)null;

                                            if (!ASPCellIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("New ASP does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }
                                            CaseSizecellindex = rowData.Cells.Any(x => x.ToString().Trim() == "MOQ") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "MOQ").ColumnIndex : (int?)null;

                                            if (!CaseSizecellindex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("MOQ does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        rowData = sheet.GetRow(iRowIdx);
                                        if (rowData != null && rowData.Cells.Count > 0)
                                        {
                                            rowData = sheet.GetRow(iRowIdx);
                                            cellData = rowData.GetCell(IdCellIndex.Value);
                                            Col0 = cellData == null ? "" : cellData.ToString();
                                            Id = Convert.ToInt32(Col0);

                                            cellData = rowData.GetCell(ForecastCellIndex.Value);
                                            col1 = cellData == null ? "" : cellData.ToString();
                                            Forecast = Convert.ToInt32(col1);

                                            cellData = rowData.GetCell(buyerRemarkCellIndex.Value);
                                            col2 = cellData == null ? "" : cellData.ToString();
                                            buyerComment = Convert.ToString(col2);

                                            cellData = rowData.GetCell(salesRemarkCellIndex.Value);
                                            col3 = cellData == null ? "" : cellData.ToString();
                                            salescomment = Convert.ToString(col3);

                                            cellData = rowData.GetCell(ASPCellIndex.Value);
                                            col4 = cellData == null ? "" : cellData.ToString();
                                            ASP = Convert.ToDouble(col4);


                                            cellData = rowData.GetCell(CaseSizecellindex.Value);
                                            col5 = cellData == null ? "" : cellData.ToString();
                                            casesize = Convert.ToDouble(col5);


                                            QuadrantItemDetailHistory his = new QuadrantItemDetailHistory();
                                            var datas = Ids.FirstOrDefault(x => x.Id == Id);
                                            if (datas != null)
                                            {
                                                DateTime sdate = datas.StartDate.Date;
                                                DateTime edate = datas.EndDate.Date;
                                                DateTime currentdate = DateTime.Now.Date;
                                               
                                                if (currentdate <= edate) //currentdate >= sdate &&
                                                {
                                                    if (datas.CommitedForeCastInCase != Forecast && issaleslead)
                                                    {
                                                        if (Forecast < datas.MinValueInCase || Forecast > datas.MaxValueInCase)
                                                        {
                                                            datas.Status = "Approved"; //"Approval Pending";
                                                        }
                                                        else
                                                        {
                                                            datas.Status = "Approved";
                                                        }

                                                        datas.CommitedForeCastValue = Forecast;
                                                        datas.CommitedForeCastInCase = Forecast;


                                                    }
                                                    if (isbuyer)
                                                    {
                                                        datas.NewASP = ASP;
                                                        if (datas.CaseSize != casesize)
                                                        {
                                                            datas.CaseSize = casesize;
                                                        }
                                                    }


                                                    Quadrantdetails.Add(datas);
                                                    db.Entry(datas).State = EntityState.Modified;

                                                    if (datas.CaseSize != casesize)
                                                    {
                                                        his.CaseSize = casesize;
                                                    }
                                                    his.QuadrantDetailId = datas.Id;
                                                    his.IsActive = true;
                                                    his.CreatedBy = userid;
                                                    his.CreatedDate = cdate;
                                                    his.IsDeleted = false;
                                                    his.BuyerRemark = isbuyer ? buyerComment : null;
                                                    his.SalesRemark = issaleslead ? salescomment : null;
                                                    his.Forcast = issaleslead ? Forecast : 0;
                                                    his.ASP = isbuyer ? ASP : 0;
                                                    list.Add(his);

                                                    #region Update Qty In System forecaste
                                                    var SystemForecastedetails = db.ItemForecastDetailDb.FirstOrDefault(x => x.ItemMultiMRPId == datas.ItemMultiMRPid && x.WarehouseId == datas.Warehouseid && sdate.Month == x.CreatedDate.Month && sdate.Year == x.CreatedDate.Year && x.IsActive == true && x.IsDeleted == false);
                                                    if (SystemForecastedetails != null )
                                                    {
                                                        if((datas.CommitedForeCastInCase * casesize) > SystemForecastedetails.SystemSuggestedQty)
                                                        {
                                                            SystemForecastedetails.SystemSuggestedQty = Convert.ToInt32(datas.CommitedForeCastInCase * casesize);
                                                            SystemForecastedetails.ModifiedDate = DateTime.Now;
                                                            SystemForecastedetails.ModifiedBy = 1;
                                                            db.Entry(SystemForecastedetails).State = EntityState.Modified;
                                                        }
                                                    }
                                                    #endregion
                                                }
                                            }
                                        }
                                    }
                                }
                                var QuadrantidList = string.Join(",", Quadrantdetails.Select(x => x.Id).Distinct().ToList());
                                var param = new SqlParameter("@QuadrantIds", QuadrantidList);
                                var quadrantDetailHistory = db.Database.SqlQuery<QuadrantDetailHistoryDC>("QuadrantDetailHistoryById @QuadrantIds", param).ToList();

                                foreach (var qd in list)
                                {
                                    var quadrant = quadrantDetailHistory.Where(x => x.QuadrantDetailId == qd.QuadrantDetailId).FirstOrDefault();
                                    if (quadrant == null || ((isbuyer && (quadrant.BuyerRemark != qd.BuyerRemark || quadrant.ASP != qd.ASP || quadrant.CaseSize != qd.CaseSize)) || (issaleslead && (quadrant.Forcast != qd.Forcast || quadrant.SalesRemark != qd.SalesRemark))))
                                    {
                                        db.QuadrantItemDetailHistories.Add(qd);
                                    }
                                }
                                if (db.Commit() > 0)
                                {
                                    MSG = "File Saved Sucessfully"; return Created(MSG, MSG);
                                }
                                else { MSG = "Failed To Save"; return Created(MSG, MSG); }
                            }
                        }
                        else
                        {
                            return Created("File extnsion required .xlsx", "File extnsion required .xlsx");
                        }
                    }
                }
                return Created("Error", "Error");
            }
            return Created("Error", "Error");
        }


        //[Route("UploadQuadrantFile")]
        //[HttpPost]
        //public IHttpActionResult UploadQuadrantFile()
        //{
        //    if (HttpContext.Current.Request.Files.AllKeys.Any())
        //    {
        //        var formData1 = HttpContext.Current.Request.Form["compid"];
        //        var identity = User.Identity as ClaimsIdentity;
        //        List<string> roleNames = new List<string>();
        //        List<QuadrantDetail> Quadrantdetails = new List<QuadrantDetail>();
        //        int userid = 0; string Rolename = "";
        //        bool isbuyer = false;
        //        bool issaleslead = false;

        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
        //            roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

        //        if (roleNames.Any(x => x == "Hub sales lead"))
        //            issaleslead = true;
        //        else if (roleNames.Any(x => x == "Sourcing Associate" || x == "Buyer" || x == "HQ Sourcing Associate" || x == "Sourcing Executive" || x == "HQ Sourcing Executive"))
        //            isbuyer = true;

        //        DateTime cdate = DateTime.Now;
        //        string Col0, col1, col2, col3, col4, col5;
        //        string MSG = "";
        //        var httpPostedFile = HttpContext.Current.Request.Files["file"];
        //        if (httpPostedFile != null)
        //        {
        //            using (var db = new AuthContext())
        //            {
        //                var data = db.QuadrantDetails.ToList();
        //                List<QuadrantItemDetailHistory> quadrantItemDetailHistories = new List<QuadrantItemDetailHistory>();
        //                string ext = Path.GetExtension(httpPostedFile.FileName);
        //                if (ext == ".xlsx" || ext == ".xls")
        //                {
        //                    byte[] buffer = new byte[httpPostedFile.ContentLength];
        //                    using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
        //                    {
        //                        br.Read(buffer, 0, buffer.Length);
        //                    }
        //                    XSSFWorkbook hssfwb;
        //                    using (MemoryStream memStream = new MemoryStream())
        //                    {
        //                        BinaryFormatter binForm = new BinaryFormatter();
        //                        memStream.Write(buffer, 0, buffer.Length);
        //                        memStream.Seek(0, SeekOrigin.Begin);
        //                        hssfwb = new XSSFWorkbook(memStream);
        //                        string sSheetName = hssfwb.GetSheetName(0);
        //                        ISheet sheet = hssfwb.GetSheet(sSheetName);
        //                        IRow rowData;
        //                        ICell cellData = null;
        //                        int? IdCellIndex = null;
        //                        int? ForecastCellIndex = null;
        //                        int? buyerRemarkCellIndex = null;
        //                        int? salesRemarkCellIndex = null;
        //                        int? ASPCellIndex = null;
        //                        int? CaseSizecellindex = null;
        //                        int Id, Forecast;
        //                        string buyerComment, salescomment;
        //                        double ASP, casesize;
        //                        List<QuadrantItemDetailHistory> list = new List<QuadrantItemDetailHistory>();
        //                        for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
        //                        {
        //                            if (iRowIdx == 0)
        //                            {
        //                                rowData = sheet.GetRow(iRowIdx);
        //                                if (rowData != null)
        //                                {
        //                                    string strJSON = null;
        //                                    string field = string.Empty;
        //                                    field = rowData.GetCell(0).ToString();
        //                                    IdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "ID") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "ID").ColumnIndex : (int?)null;
        //                                    if (!IdCellIndex.HasValue)
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Id does not exist..try again");
        //                                        return Created(strJSON, strJSON);
        //                                    }

        //                                    ForecastCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "SalesForecast") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "SalesForecast").ColumnIndex : (int?)null;

        //                                    if (!ForecastCellIndex.HasValue)
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Sales Forecast does not exist..try again");
        //                                        return Created(strJSON, strJSON);
        //                                    }

        //                                    buyerRemarkCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "BuyerRemark") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "BuyerRemark").ColumnIndex : (int?)null;

        //                                    if (!buyerRemarkCellIndex.HasValue)
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("BuyerRemark does not exist..try again");
        //                                        return Created(strJSON, strJSON);
        //                                    }
        //                                    salesRemarkCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "SalesRemark") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "SalesRemark").ColumnIndex : (int?)null;

        //                                    if (!salesRemarkCellIndex.HasValue)
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("SalesRemark does not exist..try again");
        //                                        return Created(strJSON, strJSON);
        //                                    }
        //                                    ASPCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "NewASP") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "NewASP").ColumnIndex : (int?)null;

        //                                    if (!ASPCellIndex.HasValue)
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("New ASP does not exist..try again");
        //                                        return Created(strJSON, strJSON);
        //                                    }
        //                                    CaseSizecellindex = rowData.Cells.Any(x => x.ToString().Trim() == "MOQ") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "MOQ").ColumnIndex : (int?)null;

        //                                    if (!CaseSizecellindex.HasValue)
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("MOQ does not exist..try again");
        //                                        return Created(strJSON, strJSON);
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                rowData = sheet.GetRow(iRowIdx);
        //                                if (rowData != null && rowData.Cells.Count > 0)
        //                                {
        //                                    rowData = sheet.GetRow(iRowIdx);
        //                                    cellData = rowData.GetCell(IdCellIndex.Value);
        //                                    Col0 = cellData == null ? "" : cellData.ToString();
        //                                    Id = Convert.ToInt32(Col0);

        //                                    cellData = rowData.GetCell(ForecastCellIndex.Value);
        //                                    col1 = cellData == null ? "" : cellData.ToString();
        //                                    Forecast = Convert.ToInt32(col1);

        //                                    cellData = rowData.GetCell(buyerRemarkCellIndex.Value);
        //                                    col2 = cellData == null ? "" : cellData.ToString();
        //                                    buyerComment = Convert.ToString(col2);

        //                                    cellData = rowData.GetCell(salesRemarkCellIndex.Value);
        //                                    col3 = cellData == null ? "" : cellData.ToString();
        //                                    salescomment = Convert.ToString(col3);

        //                                    cellData = rowData.GetCell(ASPCellIndex.Value);
        //                                    col4 = cellData == null ? "" : cellData.ToString();
        //                                    ASP = Convert.ToDouble(col4);


        //                                    cellData = rowData.GetCell(CaseSizecellindex.Value);
        //                                    col5 = cellData == null ? "" : cellData.ToString();
        //                                    casesize = Convert.ToDouble(col5);

        //                                    QuadrantItemDetailHistory his = new QuadrantItemDetailHistory();

        //                                    var datas = data.FirstOrDefault(x => x.Id == Id);
        //                                    if (datas != null)
        //                                    {
        //                                        DateTime sdate = datas.StartDate.Date;
        //                                        DateTime edate = datas.EndDate.Date;
        //                                        DateTime currentdate = DateTime.Now.Date;
        //                                        if (currentdate <= edate) //currentdate >= sdate &&
        //                                        {
        //                                            if (datas.CommitedForeCastInCase != Forecast && issaleslead)
        //                                            {
        //                                                if (Forecast < datas.MinValueInCase || Forecast > datas.MaxValueInCase)
        //                                                {
        //                                                    datas.Status = "Approval Pending";
        //                                                }
        //                                                else
        //                                                {
        //                                                    datas.Status = "Approved";
        //                                                }

        //                                                datas.CommitedForeCastValue = Forecast;
        //                                                datas.CommitedForeCastInCase = Forecast;


        //                                            }
        //                                            if (isbuyer)
        //                                            {
        //                                                datas.NewASP = ASP;
        //                                                if (datas.CaseSize != casesize)
        //                                                {
        //                                                    datas.CaseSize = casesize;
        //                                                }
        //                                            }


        //                                            Quadrantdetails.Add(datas);
        //                                            db.Entry(datas).State = EntityState.Modified;

        //                                            if (datas.CaseSize != casesize)
        //                                            {
        //                                                his.CaseSize = casesize;
        //                                            }
        //                                            his.QuadrantDetailId = datas.Id;
        //                                            his.CreatedBy = userid;
        //                                            his.CreatedDate = cdate;
        //                                            his.IsActive = true;
        //                                            his.IsDeleted = false;
        //                                            his.BuyerRemark = isbuyer ? buyerComment : null;
        //                                            his.SalesRemark = issaleslead ? salescomment : null;
        //                                            his.Forcast = issaleslead ? Forecast : 0;
        //                                            his.ASP = isbuyer ? ASP : 0;
        //                                            list.Add(his);
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        var QuadrantidList = string.Join(",", Quadrantdetails.Select(x => x.Id).Distinct().ToList());

        //                        var param = new SqlParameter("@QuadrantIds", QuadrantidList);
        //                        var quadrantDetailHistory = db.Database.SqlQuery<QuadrantDetailHistoryDC>("QuadrantDetailHistoryById @QuadrantIds", param).ToList();

        //                        foreach (var qd in list)
        //                        {
        //                            var quadrant = quadrantDetailHistory.Where(x => x.QuadrantDetailId == qd.QuadrantDetailId).FirstOrDefault();
        //                            if (quadrant == null || ((isbuyer && (quadrant.BuyerRemark != qd.BuyerRemark || quadrant.ASP != qd.ASP || quadrant.CaseSize != qd.CaseSize)) || (issaleslead && (quadrant.Forcast != qd.Forcast || quadrant.SalesRemark != qd.SalesRemark))))
        //                            {
        //                                db.QuadrantItemDetailHistories.Add(qd);
        //                            }
        //                        }

        //                        if (db.Commit() > 0)
        //                        {
        //                            MSG = "File Saved Sucessfully"; return Created(MSG, MSG);
        //                        }
        //                        else { MSG = "Failed To Save"; return Created(MSG, MSG); }


        //                    }

        //                }
        //                else
        //                {
        //                    return Created("File extnsion required .xlsx", "File extnsion required .xlsx");
        //                }
        //            }
        //        }
        //        return Created("Error", "Error");
        //    }
        //    return Created("Error", "Error");
        //}

        [Route("GetQuadrantDetails")]
        [HttpGet]
        [Authorize]
        public APIResponse QuadrantDetails(int warehouseId, string status, int skip, int take, string filter)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            APIResponse response = new APIResponse();
            if (userid > 0 && warehouseId > 0)
            {
                using (var context = new AuthContext())
                {
                    var param = new SqlParameter("Warehouseid", warehouseId);
                    var param2 = new SqlParameter("Warehouseid", warehouseId);

                    var ZonalLead = context.Database.SqlQuery<ZSMLeadDetailDc>("exec RegionalSalesForecastDetailList @Warehouseid", param).FirstOrDefault();

                    if (ZonalLead != null && ZonalLead.RegionManagerId == userid)
                    {
                        ZSMPerformanceList PerformanceList = new ZSMPerformanceList();
                        var Skip = new SqlParameter("skip", skip);
                        var Take = new SqlParameter("take", take);
                        var Filter = new SqlParameter("filter", filter == null ? DBNull.Value : (object)filter);
                        var param1 = new SqlParameter("status", status == null ? DBNull.Value : (object)status);

                        PerformanceList.ZsmPerformanceList = context.Database.SqlQuery<ZSMPerformanceListDc>("exec zsmApprovalist @Warehouseid, @status,@skip,@take,@filter", param2, param1, Skip, Take, Filter).ToList();

                        if (PerformanceList.ZsmPerformanceList != null && PerformanceList.ZsmPerformanceList.Any())
                        {
                            PerformanceList.TotalRecord = PerformanceList.ZsmPerformanceList.Select(x => x.totalcount).FirstOrDefault();
                            response.Data = PerformanceList;
                            response.Status = true;
                        }
                        else
                        {
                            response.Message = "Data not found";
                            response.Status = false;
                        }
                    }
                    else
                    {
                        response.Message = "You are not Authorised";
                        response.Status = false;
                    }
                }
            }
            else
            {
                response.Message = "User not authorised";
                response.Status = false;
            }
            return response;
        }

        [Route("IsApproveReject")]
        [HttpGet]
        [Authorize]
        public APIResponse QuadrantDetails(string status, string comment, int quadrantDetailId, int forcast)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            APIResponse response = new APIResponse();
            if (roleNames.Any(x => x == "Zonal sales lead"))
            {
                if (userid > 0 && !string.IsNullOrEmpty(status) && !string.IsNullOrEmpty(comment) && quadrantDetailId > 0)
                {
                    using (var context = new AuthContext())
                    {
                        var quadrantDetail = context.QuadrantDetails.Where(x => x.Id == quadrantDetailId).FirstOrDefault();
                        if (quadrantDetail != null)
                        {
                            quadrantDetail.ModifiedBy = userid;
                            quadrantDetail.ModifiedDate = DateTime.Now;
                            quadrantDetail.Status = status;
                            quadrantDetail.CommitedForeCastValue = forcast;
                            quadrantDetail.CommitedForeCastInCase = forcast;
                            context.Entry(quadrantDetail).State = EntityState.Modified;
                            QuadrantItemDetailHistory quadrant = new QuadrantItemDetailHistory();
                            quadrant.CreatedBy = userid;
                            quadrant.CreatedDate = DateTime.Now;
                            quadrant.SalesRemark = comment;
                            quadrant.Forcast = forcast;
                            quadrant.IsActive = true;
                            quadrant.IsDeleted = false;
                            quadrant.QuadrantDetailId = quadrantDetailId;
                            context.QuadrantItemDetailHistories.Add(quadrant);
                        }
                        if (context.Commit() > 0)
                        {
                            response.Message = "Updated Succesfully";
                            response.Status = true;
                        }
                        else
                        {
                            response.Message = "Something went wrong";
                            response.Status = false;
                        }
                    }
                }
                else
                {
                    response.Message = "user not authorized or parameters can't be null";
                    response.Status = false;
                }
            }
            else
            {
                response.Message = "user is not Zonal sales lead";
                response.Status = false;
            }
            return response;
        }

        [Route("UploadNewProductFile")]
        [HttpPost]
        public IHttpActionResult UploadNewProductFile()
        {
            APIResponse res = new APIResponse();
            bool EmptyField = false; string msg = " Is Empty";

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


            if (HttpContext.Current.Request.Files.Count > 0)
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];

                if (httpPostedFile != null)
                {
                    //Check uploaded file extenstion
                    var supportedTypes = new[] { "xls", "xlsx" };
                    string fileExtension = Path.GetExtension(httpPostedFile.FileName).Substring(1);
                    if (!supportedTypes.Contains(fileExtension))
                    {
                        res.Status = false;
                        res.Message = "Extension Is InValid - Only Upload EXCEL /.xls/.xlsx File.";
                        return Created(res.Message, res.Message);
                    }

                    DateTime cdate = DateTime.Now;

                    using (var db = new AuthContext())
                    {
                        var warehousedata = db.Warehouses.Where(p => p.Deleted == false && p.active == true && p.IsKPP == false).Select(x => new { x.WarehouseId, x.WarehouseName }).ToList();
                        var storedata = db.StoreDB.Where(p => p.IsDeleted == false && p.IsActive == true).Select(x => new { x.Id, x.Name }).ToList();
                        var QuadrantDate = db.QuadrantDetails.Where(x => x.IsActive == true && x.IsDeleted == false).OrderByDescending(x => x.Id).Select(x => new { x.StartDate, x.EndDate }).FirstOrDefault();

                        List<QuadrantItemDetailHistory> quadrantItemDetailHistories = new List<QuadrantItemDetailHistory>();
                        string ext = Path.GetExtension(httpPostedFile.FileName);

                        byte[] buffer = new byte[httpPostedFile.ContentLength];
                        using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
                        {
                            br.Read(buffer, 0, buffer.Length);
                        }
                        XSSFWorkbook hssfwb;
                        using (MemoryStream memStream = new MemoryStream())
                        {
                            BinaryFormatter binForm = new BinaryFormatter();
                            memStream.Write(buffer, 0, buffer.Length);
                            memStream.Seek(0, SeekOrigin.Begin);
                            hssfwb = new XSSFWorkbook(memStream);
                            string sSheetName = hssfwb.GetSheetName(0);
                            ISheet sheet = hssfwb.GetSheet(sSheetName);
                            IRow rowData;
                            ICell cellData = null;
                            List<QuadrantDetail> Excellist = new List<QuadrantDetail>();

                            int? ItemNumber = null;
                            int? ItemMultiMRPID = null;
                            int? MinQty = null;
                            int? MaxQty = null;
                            int? Warehouse = null;
                            int? Store = null;

                            for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                            {
                                if (iRowIdx == 0)
                                {
                                    rowData = sheet.GetRow(iRowIdx);

                                    if (rowData != null)
                                    {

                                        string strJSON = null;
                                        ItemNumber = rowData.Cells.Any(x => x.ToString().Trim() == "ItemNumber") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "ItemNumber").ColumnIndex : (int?)null;
                                        if (!ItemNumber.HasValue)
                                        {
                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("ItemNumber does not exist..try again");
                                            return Created(strJSON, strJSON);
                                        }

                                        ItemMultiMRPID = rowData.Cells.Any(x => x.ToString().Trim() == "ItemMultiMRPId") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "ItemMultiMRPId").ColumnIndex : (int?)null;
                                        if (!ItemMultiMRPID.HasValue)
                                        {
                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("ItemMultiMRPID does not exist..try again");
                                            return Created(strJSON, strJSON); ;
                                        }

                                        MinQty = rowData.Cells.Any(x => x.ToString().Trim() == "MinQty") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "MinQty").ColumnIndex : (int?)null;
                                        if (!MinQty.HasValue)
                                        {
                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("MinQty does not exist..try again");
                                            return Created(strJSON, strJSON);
                                        }
                                        MaxQty = rowData.Cells.Any(x => x.ToString().Trim() == "MaxQty") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "MaxQty").ColumnIndex : (int?)null;
                                        if (!MaxQty.HasValue)
                                        {
                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("MaxQty does not exist..try again");
                                            return Created(strJSON, strJSON);
                                        }
                                        Warehouse = rowData.Cells.Any(x => x.ToString().Trim() == "Warehouse") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Warehouse").ColumnIndex : (int?)null;
                                        if (!Warehouse.HasValue)
                                        {
                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Warehouse does not exist..try again");
                                            return Created(strJSON, strJSON);
                                        }
                                        Store = rowData.Cells.Any(x => x.ToString().Trim() == "Store") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Store").ColumnIndex : (int?)null;
                                        if (!Store.HasValue)
                                        {
                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Store does not exist..try again");
                                            return Created(strJSON, strJSON);
                                        }
                                    }
                                }
                                else
                                {
                                    rowData = sheet.GetRow(iRowIdx);
                                    DateTime datetoday = DateTime.Today;
                                    cellData = rowData.GetCell(0);
                                    rowData = sheet.GetRow(iRowIdx);
                                    if (rowData != null)
                                    {
                                        QuadrantDetail quadrantDetail = new QuadrantDetail();

                                        cellData = rowData.GetCell(0);
                                        if (cellData == null) { EmptyField = true; msg = "Item Number" + msg; break; }
                                        quadrantDetail.ItemNumber = Convert.ToString(cellData.ToString());

                                        cellData = rowData.GetCell(1);
                                        if (cellData == null) { EmptyField = true; msg = "ItemMultiMRPID" + msg; break; }
                                        quadrantDetail.ItemMultiMRPid = Convert.ToInt32(cellData.ToString());

                                        cellData = rowData.GetCell(2);
                                        if (cellData == null) { EmptyField = true; msg = "Min Qty" + msg; break; }
                                        quadrantDetail.MinValue = Convert.ToDouble(cellData.ToString());

                                        cellData = rowData.GetCell(3);
                                        if (cellData == null) { EmptyField = true; msg = "Max Qty" + msg; break; }
                                        quadrantDetail.MaxValue = Convert.ToDouble(cellData.ToString());

                                        cellData = rowData.GetCell(4);
                                        if (cellData == null) { EmptyField = true; msg = "Warehouse" + msg; break; }
                                        var warename = Convert.ToString(cellData.ToString());
                                        quadrantDetail.Warehouseid = warehousedata.Where(x => x.WarehouseName == warename).Select(x => x.WarehouseId).FirstOrDefault();

                                        cellData = rowData.GetCell(5);
                                        if (cellData == null) { EmptyField = true; msg = "Store" + msg; break; }
                                        var storename = Convert.ToString(cellData.ToString());
                                        quadrantDetail.Storeid = storedata.Where(x => x.Name == storename).Select(x => (int)x.Id).FirstOrDefault();

                                        //quadrantDetail.Comment = "New Product Introduce";
                                        quadrantDetail.Type = "NPI";
                                        quadrantDetail.Quadrant = "NPI";
                                        quadrantDetail.Status = "Approved";
                                        quadrantDetail.StartDate = QuadrantDate.StartDate;
                                        quadrantDetail.EndDate = QuadrantDate.EndDate;
                                        quadrantDetail.CreatedDate = DateTime.Now;
                                        quadrantDetail.CreatedBy = userid;
                                        quadrantDetail.IsActive = true;
                                        quadrantDetail.IsDeleted = false;
                                        Excellist.Add(quadrantDetail);

                                    }
                                }
                            }

                            if (EmptyField)
                            {
                                return Created(msg, msg);
                            }
                            var itemno = Excellist.Select(x => x.ItemNumber).Distinct().ToList();
                            var multimrpid = Excellist.Select(x => x.ItemMultiMRPid).Distinct().ToList();
                            var warehouseid = Excellist.Select(x => x.Warehouseid).Distinct().ToList();
                            var store = Excellist.Select(x => x.Storeid).Distinct().ToList();

                            var MultiMRPData = db.ItemMultiMRPDB.Where(x => itemno.Contains(x.ItemNumber) && multimrpid.Contains(x.ItemMultiMRPId)).Select(x => new { x.ItemNumber, x.ItemMultiMRPId }).Distinct().ToList();

                            List<QuadrantDetail> QuadrantDetaillist = new List<QuadrantDetail>();
                            var currentdate = DateTime.Now.Date;
                            var FirstDate = new DateTime(currentdate.Year, currentdate.Month, 01);
                            var LastDate = FirstDate.AddMonths(1);

                            QuadrantDetaillist = db.QuadrantDetails.Where(x => itemno.Contains(x.ItemNumber) && multimrpid.Contains(x.ItemMultiMRPid)
                                                  && warehouseid.Contains(x.Warehouseid) && store.Contains(x.Storeid)
                                                  && x.CreatedDate >= FirstDate && x.CreatedDate < LastDate && x.Quadrant == "NPI"
                                                && x.IsActive == true && x.IsDeleted == false).ToList();

                            foreach (var QD in Excellist)
                            {
                                var MultiMRPexist = MultiMRPData.Any(x => x.ItemNumber.Trim() == QD.ItemNumber.Trim() && x.ItemMultiMRPId == QD.ItemMultiMRPid);
                                if (MultiMRPexist)
                                {
                                    var exists = QuadrantDetaillist.Where(x => QD.ItemNumber == x.ItemNumber && QD.ItemMultiMRPid == x.ItemMultiMRPid && QD.Warehouseid == x.Warehouseid && QD.Storeid == x.Storeid).FirstOrDefault();
                                    if (exists != null)
                                    {
                                        exists.MinValue = QD.MinValue;
                                        exists.MaxValue = QD.MaxValue;
                                        exists.ModifiedBy = userid;
                                        exists.ModifiedDate = DateTime.Now;
                                        db.Entry(exists).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        db.QuadrantDetails.Add(QD);
                                    }
                                }
                                else
                                {
                                    string Error = "Item Number " + QD.ItemNumber + " or " + QD.ItemMultiMRPid + " MultiMRP Not Exists";
                                    return Created(Error, Error);
                                }
                            }

                            string MSG = "";
                            if (db.Commit() > 0)
                            {
                                MSG = "File Saved Sucessfully"; return Created(MSG, MSG);
                            }
                            else { MSG = "Failed To Save"; return Created(MSG, MSG); }
                        }
                    }
                }
                else
                {
                    res.Message = "something went wrong";
                    res.Status = false;
                    return Created(res.Message, res.Message);
                }
            }
            return Created(res.Message, res.Message);
        }

        [Route("GetQuadrantitemhistory")]
        [HttpGet]
        public List<GetQuadrantitemHistory> GetQuadrantitemhistory(long QuadrantDetailId)
        {
            List<GetQuadrantitemHistory> result = new List<GetQuadrantitemHistory>();
            using (var db = new AuthContext())
            {
                if (QuadrantDetailId > 0)
                {
                    var data = db.QuadrantItemDetailHistories.Where(x => x.QuadrantDetailId == QuadrantDetailId && x.IsActive == true && x.IsDeleted == false).OrderByDescending(y => y.Id).ToList();
                    if (data.Count > 0 && data.Any())
                    {
                        foreach (var a in data)
                        {
                            GetQuadrantitemHistory b = new GetQuadrantitemHistory();
                            b.CreatedDate = a.CreatedDate;
                            b.BuyerRemark = a.BuyerRemark;
                            b.SalesRemark = a.SalesRemark;
                            b.Forast = a.Forcast;
                            b.ASP = a.ASP;
                            b.CaseSize = a.CaseSize;
                            b.CreatedBy = db.Peoples.FirstOrDefault(x => x.PeopleID == a.CreatedBy).DisplayName;
                            result.Add(b);
                        }
                    }
                }
                return result;
            }
        }

        [Route("GetsalesForecastQuadrant")]
        [HttpGet]
        public List<GetSalesQuadrant> GetsalesForecastQuadrant(int PeopleId, int WarehouseId)
        {
            List<GetSalesQuadrant> response = new List<GetSalesQuadrant>();
            using (var db = new AuthContext())
            {
                var storelist = db.WarehouseStoreMappings.Where(x => x.SalesLeadId == PeopleId && x.IsActive == true && x.IsDeleted == false).Select(y => y.StoreId).Distinct().ToList();

                if (storelist.Count > 0 && storelist.Any())
                {
                    if (db.Database.Connection.State != ConnectionState.Open)
                        db.Database.Connection.Open();
                    DbCommand cmd = null;


                    var IdDt2 = new DataTable();
                    IdDt2.Columns.Add("IntValue");

                    foreach (var item in storelist)
                    {
                        var dr = IdDt2.NewRow();
                        dr["IntValue"] = item;
                        IdDt2.Rows.Add(dr);
                    }
                    var param2 = new SqlParameter("@StoreId", IdDt2);
                    param2.SqlDbType = SqlDbType.Structured;
                    param2.TypeName = "dbo.intValues";
                    var param = new SqlParameter("@warehouseid", WarehouseId);
                    cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[Sp_GetSalesQuadrant]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(param);
                    cmd.Parameters.Add(param2);
                    using (var reader = cmd.ExecuteReader())
                    {
                        response = ((IObjectContextAdapter)db).ObjectContext.Translate<GetSalesQuadrant>(reader).ToList();
                        GetSalesQuadrant a = new GetSalesQuadrant();
                        a.Quadrant = "NPI";
                        a.Id = 0;
                        response.Add(a);
                    }
                }

            }
            return response;
        }


        [Route("DownloadConifgSampleFile")]
        [HttpGet]
        public List<SampleFileDC> DownloadConifgSampleFile()
        {
            using (var context = new AuthContext())
            {
                var data = context.Database.SqlQuery<SampleFileDC>("NPISampleFile").ToList();
                if (data != null && data.Count > 0)
                    return data;
                else
                    return new List<SampleFileDC>();
            }
        }


        [Route("GetWarehouses")]
        [HttpGet]
        public List<warehouselist> GetWarehouses()
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                List<string> roleNames = new List<string>();
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                    roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

                List<warehouselist> list = new List<warehouselist>();
                if (userid > 0)
                {
                    if (roleNames.Any(x => x == "Hub Sales Lead"))
                    {
                        var user = new SqlParameter("@Userid", userid);
                        list = context.Database.SqlQuery<warehouselist>(" Exec GetWarehouses @Userid", user).ToList();
                    }
                    else
                    {
                        var userIds = new SqlParameter("@userid", userid);
                        list = context.Database.SqlQuery<warehouselist>("WarehouseGetCommon @userid", userIds).ToList();
                    }
                }


                return list;
            }
        }



        #endregion




    }
}