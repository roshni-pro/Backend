using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.ElasticSearch;
using AngularJSAuthentication.DataContracts.External;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model.Salescommission;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.External.SalesManApp
{
    [RoutePrefix("api/SalesIncentive")]
    public class SaleIncentiveController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        readonly string platformIdxName = $"skorderdata_{AppConstants.Environment}";

        [Route("AddSaleIncentive")]
        [HttpPost]
        public responceDc AddSaleIncentive(DataContracts.Transaction.SaleIncentiveDC.SalesComissionTransactionDc saleComissionTransactionDc)
        {
            responceDc res = new responceDc();
            using (var db = new AuthContext())
            {
                int userid = GetUserId();
                if (saleComissionTransactionDc.CommissionCatMasterId == 1 || saleComissionTransactionDc.CommissionCatMasterId == 3)
                {
                    var name = saleComissionTransactionDc.EventName + '*' + saleComissionTransactionDc.Customers;
                    var salesComission = db.SalesComissionTransactions.Where(x => x.EventMasterId == saleComissionTransactionDc.EventMasterId
                    && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now && x.EventName == name
                    && x.WarehouseId == saleComissionTransactionDc.WarehouseId && x.IsActive == true && x.IsDeleted == false && x.StoreId == saleComissionTransactionDc.StoreId).FirstOrDefault();

                    if (salesComission != null)
                    {
                        res.Status = false;
                        res.Message = "Alredy Sales Comission Created!!";
                        return res;
                    };
                }
                var salesComissionTransactions = db.SalesComissionTransactions.Where(x => x.Id == saleComissionTransactionDc.Id && x.WarehouseId == saleComissionTransactionDc.WarehouseId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (salesComissionTransactions == null)
                {
                    var salesCommissionEventMasters = db.SalesCommissionEventMasters.FirstOrDefault(x => x.Id == saleComissionTransactionDc.EventMasterId && x.IsActive == true && x.IsDeleted == false);
                    if (salesCommissionEventMasters != null)
                    {
                        SalesComissionTransaction add = new SalesComissionTransaction();
                        add.WarehouseId = saleComissionTransactionDc.WarehouseId;
                        add.EventMasterId = saleComissionTransactionDc.EventMasterId;
                        add.BookedValue = saleComissionTransactionDc.BookedValue;
                        add.IncentiveType = saleComissionTransactionDc.IncentiveType;
                        add.IncentiveValue = saleComissionTransactionDc.IncentiveValue;
                        add.StartDate = saleComissionTransactionDc.StartDate;
                        add.EndDate = saleComissionTransactionDc.EndDate;
                        add.CreatedDate = DateTime.Now;
                        if (saleComissionTransactionDc.EventName != null)
                        {
                            add.EventName = saleComissionTransactionDc.EventName + '*' + saleComissionTransactionDc.Customers;
                        }
                        else
                        {
                            add.EventName = salesCommissionEventMasters.EventName;
                        }
                        add.IsActive = true;
                        add.IsDeleted = false;
                        add.CreatedBy = userid;
                        add.StoreId = saleComissionTransactionDc.StoreId;
                        add.SalesComTransDetails = new List<SalesComTransDetail>();
                        add.ExecutiveSalesCommissions = new List<ExecutiveSalesCommission>();

                        if (saleComissionTransactionDc.SubCategoryId.Count > 0)
                        {
                            DataTable dtSubCategoryIds = new DataTable();
                            dtSubCategoryIds.Columns.Add("IntValue");
                            saleComissionTransactionDc.SubCategoryId.ForEach(x =>
                            {
                                DataRow dr = dtSubCategoryIds.NewRow();
                                dr["IntValue"] = x;
                                dtSubCategoryIds.Rows.Add(dr);
                            });
                            var SubCategoryIds = new SqlParameter
                            {
                                TypeName = "dbo.intvalues",
                                ParameterName = "@SubCategoryId",
                                Value = dtSubCategoryIds
                            };
                            List<ItemlistDc> itemlist = db.Database.SqlQuery<ItemlistDc>("GetSubcategoryIdsbyMappingList @SubCategoryId", SubCategoryIds).ToList();

                            foreach (var item in itemlist)
                            {
                                if (itemlist.Any() && itemlist != null)
                                {
                                    SalesComTransDetail obj = new SalesComTransDetail()
                                    {
                                        SubCategoryMappingId = item.SubCategoryMappingId,
                                        BrandMappingId = item.BrandCategoryMappingId,
                                        ItemId = null,
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreatedDate = DateTime.Now,
                                        CreatedBy = userid
                                    };
                                    add.SalesComTransDetails.Add(obj);
                                }
                            }
                        }
                        if (saleComissionTransactionDc.ExecutiveIds.Count > 0)
                        {
                            foreach (var ExecutiveId in saleComissionTransactionDc.ExecutiveIds)
                            {
                                ExecutiveSalesCommission executiveAdd = new ExecutiveSalesCommission()
                                {
                                    ExecutiveId = ExecutiveId,
                                    IsActive = true,
                                    IsDeleted = false,
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = userid
                                };
                                add.ExecutiveSalesCommissions.Add(executiveAdd);
                            }
                        }
                        db.SalesComissionTransactions.Add(add);
                        if (db.Commit() > 0)
                        {
                            var update = db.SalesComissionTransactions.Where(x => x.Id == add.Id && x.WarehouseId == saleComissionTransactionDc.WarehouseId).FirstOrDefault();

                            if (salesCommissionEventMasters.EventName == "Business Commission")
                            {
                                update.Condition = salesCommissionEventMasters.EventJoin == null ? null : salesCommissionEventMasters.EventJoin.Replace("[Id]", update.Id.ToString()).Replace("[company]", saleComissionTransactionDc.EventName.ToString()).Replace("[customer]", saleComissionTransactionDc.Customers.ToString());
                            }
                            else
                            {
                                update.Condition = salesCommissionEventMasters.EventJoin == null ? null : salesCommissionEventMasters.EventJoin.Replace("[Id]", update.Id.ToString());
                            }
                            db.Entry(update).State = EntityState.Modified;
                            db.Commit();
                            res.Status = true;
                            res.Message = "Sales Comission Created Successfully!!";
                        }
                    }
                    else
                    {
                        res.Status = false;
                        res.Message = "Sales Commission Event not Created System!!";
                        return res;
                    }
                }
                else
                {
                    res.Status = false;
                    res.Message = "Alredy Added!!";
                    return res;
                }
                return res;
            }
        }
        [Route("GetSalesPersonCommissionList")]
        [HttpPost]
        public async Task<List<SalesPersonCommission>> GetSalesPersonCommissionList(GetListDc getListDc)
        {
            List<SalesPersonCommission> salesPersonCommission = new List<SalesPersonCommission>();
            using (var context = new AuthContext())
            {
                int mth = getListDc.month.Month;
                int year = getListDc.month.Year;
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                DataTable executiveId = new DataTable();
                executiveId.Columns.Add("IntValue");
                getListDc.ExecutiveIds.ForEach(x =>
                {
                    DataRow dr = executiveId.NewRow();
                    dr["IntValue"] = x;
                    executiveId.Rows.Add(dr);
                });

                var executiveIds = new SqlParameter
                {
                    TypeName = "dbo.intvalues",
                    ParameterName = "@executiveIds",
                    Value = executiveId
                };

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetSalesCommission]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", getListDc.warehouseId));
                cmd.Parameters.Add(new SqlParameter("@Month", mth));
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
                                       BookedValue = Convert.ToInt32(Math.Round(p.BookedValue, 0)),
                                       EventCatName = p.EventCatName,
                                       EventName = p.EventName,
                                       IncentiveType = p.IncentiveType,
                                       IncentiveValue = Math.Round(p.IncentiveValue, 0),
                                       ReqBookedValue = Convert.ToInt32(Math.Round(p.ReqBookedValue, 0)),
                                       EarnValue = Convert.ToInt32(Math.Round(p.EarnValue, 0)),
                                       EndDate = p.EndDate,
                                       StartDate = p.StartDate
                                   }
                                  ).ToList()
                               }
                        ).ToList()
                    }).ToList();
                }
            }

            return salesPersonCommission;
        }
        [Route("DeleteEventbyExecutive")]
        [HttpGet]
        public bool SalesComissionTransactionId(long SalesComissionTransactionId)
        {
            bool result = false;
            int userid = GetUserId();
            using (var db = new AuthContext())
            {
                var salesComissionTransactions = db.SalesComissionTransactions.Where(x => x.Id == SalesComissionTransactionId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (salesComissionTransactions != null)
                {
                    salesComissionTransactions.IsActive = false;
                    salesComissionTransactions.IsDeleted = true;
                    salesComissionTransactions.ModifiedBy = userid;
                    salesComissionTransactions.ModifiedDate = DateTime.Now;
                    db.Entry(salesComissionTransactions).State = EntityState.Modified;
                    //var executiveSalesCommissions = db.ExecutiveSalesCommission.Where(x => x.SalesComissionTransactionId == salesComissionTransactions.Id).FirstOrDefault();
                    //if (executiveSalesCommissions != null)
                    //{
                    //    executiveSalesCommissions.IsActive = false;
                    //    executiveSalesCommissions.IsDeleted = true;
                    //    executiveSalesCommissions.ModifiedBy = userid;
                    //    executiveSalesCommissions.ModifiedDate = DateTime.Now;
                    //    db.Entry(executiveSalesCommissions).State = EntityState.Modified;
                    //}
                    result = true;
                    db.Commit();
                }
                return result;
            }
        }
        [Route("StopEventbyExecutive")]
        [HttpGet]
        public bool StopEventbyExecutive(long SalesComissionTransactionId)
        {
            bool result = false;
            int userid = GetUserId();
            using (var db = new AuthContext())
            {
                var salesComissionTransactions = db.SalesComissionTransactions.Where(x => x.Id == SalesComissionTransactionId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (salesComissionTransactions != null)
                {
                    salesComissionTransactions.EndDate = DateTime.Now;
                    salesComissionTransactions.ModifiedBy = userid;
                    salesComissionTransactions.ModifiedDate = DateTime.Now;
                    db.Entry(salesComissionTransactions).State = EntityState.Modified;
                    db.Commit();
                    result = true;
                }
                return result;
            }
        }

        [Route("Export")]
        [HttpPost]
        public List<ExportSalesPersonCommission> Export(GetListDc getListDc)
        {
            List<ExportSalesPersonCommission> list = new List<ExportSalesPersonCommission>();

            using (var context = new AuthContext())
            {
                int mth = getListDc.month.Month;
                int year = getListDc.month.Year;
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                DataTable executiveId = new DataTable();
                executiveId.Columns.Add("IntValue");
                getListDc.ExecutiveIds.ForEach(x =>
                {
                    DataRow dr = executiveId.NewRow();
                    dr["IntValue"] = x;
                    executiveId.Rows.Add(dr);
                });

                var executiveIds = new SqlParameter
                {
                    TypeName = "dbo.intvalues",
                    ParameterName = "@executiveIds",
                    Value = executiveId
                };

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetSalesCommission]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", getListDc.warehouseId));
                cmd.Parameters.Add(new SqlParameter("@Month", mth));
                cmd.Parameters.Add(new SqlParameter("@Year", year));
                cmd.Parameters.Add(executiveIds);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 600;
                // Run the sproc
                var reader = cmd.ExecuteReader();
                var salesPersonCommissionData = ((IObjectContextAdapter)context)
                                    .ObjectContext
                                    .Translate<SalesPersonCommissionData>(reader).ToList();

                if (salesPersonCommissionData != null && salesPersonCommissionData.Any())
                {
                    var WarehouseName = context.Warehouses.FirstOrDefault(x => x.WarehouseId == getListDc.warehouseId).WarehouseName;
                    list = salesPersonCommissionData.Select(x => new ExportSalesPersonCommission
                    {
                        WarehouseName = WarehouseName,
                        ExecutiveName = x.Name,
                        CategoryName = x.CategoryName,
                        EventCatName = x.EventCatName,
                        EventName = x.EventName,
                        RequiredBookedValue = Math.Round(x.ReqBookedValue, 0),
                        IncentiveType = x.IncentiveType == 0 ? "Amount" : "Percantage",
                        IncentiveValue = x.IncentiveValue,
                        BookedValue = Math.Round(x.BookedValue, 0),
                        EarnValue = Math.Round(x.EarnValue, 0),
                        StartDate = x.StartDate,
                        EndDate = x.EndDate
                    }).OrderBy(x => x.ExecutiveName).ThenBy(x => x.CategoryName).ThenBy(x => x.EventName).ToList();
                }
            }
            return list;
        }
        [Route("GetExecutiveList/{warehouseId}/{storeId}")]
        [HttpGet]
        public List<storeExDc> GetExecutiveList(int warehouseId, int storeId)
        {
            List<storeExDc> result = null;
            using (var authContext = new AuthContext())
            {
                var query = @" Select distinct StoreExe.ExecutiveId,b.DisplayName as ExecutiveName from  ClusterStoreExecutives StoreExe  with(nolock) 
                             inner join People b with(nolock) on StoreExe.ExecutiveId = b.PeopleID
                             where IsActive = 1 and IsDeleted = 0 and b.WarehouseId = " + warehouseId + " and StoreId =" + storeId + "";
                result = authContext.Database.SqlQuery<storeExDc>(query).ToList();
                return result;
            }
        }
        [Route("GetStoreBySubCategoryList")]
        [HttpGet]
        public List<SubCategoryDC> GetStoreBySubCategoryList(int StoreId)
        {
            using (var db = new AuthContext())
            {
                var storeId = new SqlParameter("@StoreId", StoreId);
                var result = db.Database.SqlQuery<SubCategoryDC>("GetStoreBySubCategorys @StoreId", storeId).ToList();
                return result;
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

        [Route("GetItemClassificationIncentiveForExec")]
        [HttpGet]
        public async Task<List<DataContracts.External.MobileExecutiveDC.SalesIncentiveItemClassification>> GetItemClassificationIncentiveForExec(int peopleId, int warehouseId, int month, int year)
        {
            ItemMasterManager manager = new ItemMasterManager();
            return await manager.GetItemClassificationIncentiveForExec(peopleId, warehouseId, month, year);
        }


        [Route("GetSalesPersonKPIOld")]
        [HttpGet]
        public async Task<List<SalesPersonKpiResponse>> GetSalesPersonKPIOld(int peopleId, int warehouseId, int month, int year)
        {
            ItemMasterManager manager = new ItemMasterManager();
            var configs = await manager.GetSalesPersonKPIConfigs(peopleId, warehouseId, month, year);

            DateTime startDate = new DateTime(year, month, 1);
            string sDate = startDate.ToString("yyyy-MM-dd");
            string eDate = startDate.Date.AddMonths(1).ToString("yyyy-MM-dd");
            List<SalesPersonKpiResponse> salesPersonKpi = new List<SalesPersonKpiResponse>();

            foreach (var item in configs.GroupBy(s => new { s.KpiName, s.DisplayName, s.Type }))
            {
                SalesPersonKpiResponse kpi = new SalesPersonKpiResponse
                {
                    Month = month,
                    Year = year,
                    KpiName = item.Key.KpiName,
                    DisplayName = string.IsNullOrEmpty(item.Key.DisplayName) ? item.Key.KpiName : item.Key.DisplayName,
                    Type = item.Key.Type
                };

                ElasticSqlHelper<doubleVal> elasticSqlHelper = new ElasticSqlHelper<doubleVal>();
                ElasticSqlHelper<SalesPersonKpiElasticData> skcodeElasticHelper = new ElasticSqlHelper<SalesPersonKpiElasticData>();

                //var clusterIds = string.Join(",", item.Select(s => s.ClusterId).Distinct().ToList());


                var Targets = item.GroupBy(s => s.KPIId).Select(d => new { KPIID = d.Key, Target = d.FirstOrDefault().Target, IncentiveAmount = d.FirstOrDefault().IncentiveAmount });
                double incentiveAmount = 0;
                switch (item.Key.KpiName)
                {
                    case "MTD":

                        incentiveAmount = 0;
                        foreach (var store in item.GroupBy(d => d.StoreId))
                        {
                            //var clusterIds = string.Join(",", store.Select(s => s.ClusterId).Distinct().ToList());
                            var achievedVal = (await elasticSqlHelper.GetListAsync($"select sum(dispatchqty * price) as val from {platformIdxName} where createddate>='{sDate}' and status in ('Delivered','sattled') and createddate<'{eDate}'  and storeid={store.Key} and executiveid ={peopleId} ")).FirstOrDefault();

                            var Achievement = achievedVal.val;
                            var Target = Targets.Where(s => store.Select(d => d.KPIId).Contains(s.KPIID)).ToList();
                            incentiveAmount += Target.Sum(d => d.IncentiveAmount);

                            // var AchievementPercent = 100 * (Achievement / Target.Sum(d => d.Target));
                            // var incentivePercent = item.Where(s => Target.Select(a => a.KPIID).Contains(s.KPIId) && s.AchievePercent <= AchievementPercent).OrderByDescending(s => s.AchievePercent).FirstOrDefault();
                            // kpi.Earning += incentivePercent == null ? 0 : Math.Round((incentivePercent.IncentiveAmount * incentivePercent.IncentivePercent) / 100, 0);
                            kpi.Achievement += Math.Round(Achievement, 0);
                            kpi.Target += Target.Sum(d => d.Target);
                        }
                        break;


                    case "MAC":
                        incentiveAmount = 0;

                        foreach (var store in item.GroupBy(d => d.StoreId))
                        {
                            //var clusterIds = string.Join(",", store.Select(s => s.ClusterId).Distinct().ToList());

                            var achievedVal = (await elasticSqlHelper.GetListAsync($"select count(distinct custid) as val from {platformIdxName} where createddate>='{sDate}' and status in ('Delivered','sattled') and createddate<'{eDate}' and storeid={store.Key} and executiveid ={peopleId} ")).FirstOrDefault();

                            var Achievement = achievedVal.val;
                            var Target = Targets.Where(s => store.Select(d => d.KPIId).Contains(s.KPIID)).ToList();
                            incentiveAmount += Target.Sum(d => d.IncentiveAmount);

                            //var AchievementPercent = 100 * (Achievement / Target.Sum(d => d.Target));
                            //var incentivePercent = item.Where(s => Target.Select(a => a.KPIID).Contains(s.KPIId) && s.AchievePercent <= AchievementPercent).OrderByDescending(s => s.AchievePercent).FirstOrDefault();

                            //kpi.Earning += incentivePercent == null ? 0 : Math.Round((incentivePercent.IncentiveAmount * incentivePercent.IncentivePercent) / 100, 0);
                            kpi.Achievement += Math.Round(Achievement, 0);
                            kpi.Target += Target.Sum(d => d.Target);
                        }

                        break;

                    case "Success Stores":
                        incentiveAmount = 0;

                        var date = DateTime.Now;
                        var isMonthComplete = false;
                        if (date.Day < 3)
                        {
                            isMonthComplete = true;
                            date = DateTime.Now.AddMonths(-1);
                        }
                        var lastDayOfMonth = DateTime.DaysInMonth(date.Year, date.Month);

                        var mongoHelper = new MongoHelper<CustomersTargets.MonthlyCustomerTarget>();
                        string DocumentName = "MonthlyTargetData_" + date.Month.ToString() + date.Year.ToString();

                        foreach (var store in item.GroupBy(d => d.StoreId))
                        {
                            // var clusterIds = string.Join(",", store.Select(s => s.ClusterId).Distinct().ToList());

                            var query = $"select skcode, storeid, count(distinct itemnumber) linecount,  sum(dispatchqty * price) dispatchamount   from {platformIdxName} where createddate>='{sDate}' and createddate<'{eDate}' and status in ('Delivered','sattled') and storeid={store.Key} and executiveid ={peopleId} group by skcode,storeid";
                            var skCodeDataList = await skcodeElasticHelper.GetListAsync(query);

                            var allTarget = mongoHelper.Select(s => s.TargetOnStores != null
                                           && s.TargetOnStores.Count > 0
                                           && s.TargetOnStores.Any(d => d.StoreId == store.Key), collectionName: DocumentName).ToList();
                            List<CustStoreTargets> targetOnStores = new List<CustStoreTargets>();
                            foreach (var data in allTarget)
                            {
                                foreach (var storeTarget in data.TargetOnStores)
                                {
                                    targetOnStores.Add(new CustStoreTargets
                                    {
                                        skcode = data.Skcode,
                                        StoreId = storeTarget.StoreId,
                                        Target = storeTarget.Target,
                                        TargetLineItem = storeTarget.TargetLineItem
                                    });
                                }

                            }


                            //var targetOnStores = mongoHelper.GetWithProjection(s => s.TargetOnStores.Any(d => d.StoreId == store.Key),
                            //           s => s.TargetOnStores.Select(d => new CustStoreTargets
                            //           {
                            //               skcode = s.Skcode,
                            //               StoreId = d.StoreId,
                            //               Target = d.Target,
                            //               TargetLineItem = d.TargetLineItem
                            //           }).ToList(), DocumentName).SelectMany(d => d).ToList();

                            int Achievement = 0;

                            targetOnStores.ForEach(s =>
                            {
                                var skcodeData = skCodeDataList.FirstOrDefault(a => a.skcode == s.skcode && a.storeid == s.StoreId &&
                                    s.Target <= a.dispatchamount &&
                                    (!s.TargetLineItem.HasValue || (s.TargetLineItem.HasValue && s.TargetLineItem.Value <= a.linecount))
                                );

                                if (skcodeData != null)
                                    Achievement++;

                            });


                            var Target = Targets.Where(s => store.Select(d => d.KPIId).Contains(s.KPIID)).ToList();

                            incentiveAmount += Target.Sum(d => d.IncentiveAmount);

                            //var AchievementPercent = 100 * (Achievement / Target.Sum(d => d.Target));
                            //var incentivePercent = item.Where(s => Target.Select(a => a.KPIID).Contains(s.KPIId) && s.AchievePercent <= AchievementPercent).OrderByDescending(s => s.AchievePercent).FirstOrDefault();

                            //kpi.Earning += incentivePercent == null ? 0 : Math.Round((incentivePercent.IncentiveAmount * incentivePercent.IncentivePercent) / 100, 0);
                            kpi.Achievement += Achievement;
                            kpi.Target += Target.Sum(d => d.Target);
                        }

                        break;

                }

                kpi.AchievementPercent = Math.Round(100 * (kpi.Achievement / kpi.Target), 0);

                var incentives = item.GroupBy(d => new { d.AchievePercent, d.IncentivePercent });
                var incentivePercent = incentives.Where(s => s.Key.AchievePercent <= kpi.AchievementPercent).OrderByDescending(s => s.Key.AchievePercent)?.FirstOrDefault()?.Key?.IncentivePercent ?? 0;
                kpi.Earning += incentivePercent == 0 ? 0 : Math.Round((incentiveAmount * incentivePercent) / 100, 0);
                salesPersonKpi.Add(kpi);

            }
            return salesPersonKpi;
        }
        [Route("GetSalesPersonKPI")]
        [HttpGet]
        public async Task<List<SalesPersonKpiResponse>> GetSalesPersonKPI(int peopleId, int warehouseId, int month, int year)
        {
            ItemMasterManager manager = new ItemMasterManager();
            var configs = await manager.GetSalesPersonKPIConfigs(peopleId, warehouseId, month, year);

            DateTime startDate = new DateTime(year, month, 1);
            string sDate = startDate.ToString("yyyy-MM-dd");
            string eDate = startDate.Date.AddMonths(1).AddDays(1).ToString("yyyy-MM-dd");
            List<SalesPersonKpiResponse> salesPersonKpi = new List<SalesPersonKpiResponse>();
            bool Isdigitalexecutive = false;


            using (var authContext = new AuthContext())
            {
                string query = "select distinct r.Name as Role from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where PeopleID='" + peopleId + "'and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                var role = authContext.Database.SqlQuery<string>(query).ToList();
                Isdigitalexecutive = role.Any(x => x.Contains("Digital sales executive") || x.Contains("Telecaller"));
            }


            foreach (var item in configs.GroupBy(s => new { s.KpiName, s.DisplayName, s.Type, s.StoreId, s.StoreName }))
            {
                SalesPersonKpiResponse kpi = new SalesPersonKpiResponse
                {
                    Month = month,
                    Year = year,
                    KpiName = item.Key.KpiName,
                    DisplayName = string.IsNullOrEmpty(item.Key.DisplayName) ? item.Key.KpiName : item.Key.DisplayName,
                    Type = item.Key.Type
                };
                ElasticSqlHelper<SalesPersonKPIOrderData> elasticSqlHelperOrdeData = new ElasticSqlHelper<SalesPersonKPIOrderData>();
                ElasticSqlHelper<doubleVal> elasticSqlHelper = new ElasticSqlHelper<doubleVal>();
                ElasticSqlHelper<SalesPersonKpiElasticData> skcodeElasticHelper = new ElasticSqlHelper<SalesPersonKpiElasticData>();

                //var clusterIds = string.Join(",", item.Select(s => s.ClusterId).Distinct().ToList());


                var Targets = item.GroupBy(s => s.KPIId).Select(d => new { KPIID = d.Key, Target = d.FirstOrDefault().Target, IncentiveAmount = d.FirstOrDefault().IncentiveAmount });
                double incentiveAmount = 0;
                var queryOrderData = $"select skcode, storeid, itemnumber, dispatchqty , price,custid  from {platformIdxName} where createddate>= '{sDate}' and createddate<'{eDate}' and status in ('Delivered', 'sattled') and executiveid = { peopleId } and IIF(isdigitalorder is null, false, isdigitalorder) = false";

                if (Isdigitalexecutive)
                    queryOrderData = $"select skcode, storeid, itemnumber, dispatchqty , price,custid  from {platformIdxName} where createddate>= '{sDate}' and createddate<'{eDate}' and status in ('Delivered', 'sattled') and ordertakensalespersonid = { peopleId } and IIF(isdigitalorder is null, false, isdigitalorder) = true";

                var OrderData = (await elasticSqlHelperOrdeData.GetListAsync(queryOrderData)).ToList();
                switch (item.Key.KpiName)
                {
                    case "MTD":

                        incentiveAmount = 0;
                        foreach (var store in item.GroupBy(d => d.StoreId))
                        {
                            //var clusterIds = string.Join(",", store.Select(s => s.ClusterId).Distinct().ToList());
                            //Old Query: var achievedVal = (await elasticSqlHelper.GetListAsync($"select sum(dispatchqty * price) as val from {platformIdxName} where createddate>='{sDate}' and status in ('Delivered','sattled') and createddate<'{eDate}'  and storeid={store.Key} and executiveid ={peopleId} ")).FirstOrDefault();
                            var achievedVal = OrderData.Where(x => x.storeid == store.Key).Select(y => new { y.dispatchqty, y.price }).Sum(y => y.dispatchqty * y.price);
                            var Achievement = achievedVal;
                            var Target = Targets.Where(s => store.Select(d => d.KPIId).Contains(s.KPIID)).ToList();
                            incentiveAmount += Target.Sum(d => d.IncentiveAmount);

                            // var AchievementPercent = 100 * (Achievement / Target.Sum(d => d.Target));
                            // var incentivePercent = item.Where(s => Target.Select(a => a.KPIID).Contains(s.KPIId) && s.AchievePercent <= AchievementPercent).OrderByDescending(s => s.AchievePercent).FirstOrDefault();
                            // kpi.Earning += incentivePercent == null ? 0 : Math.Round((incentivePercent.IncentiveAmount * incentivePercent.IncentivePercent) / 100, 0);
                            kpi.Achievement += Math.Round(Achievement, 0);
                            kpi.Target += Target.Sum(d => d.Target);
                        }
                        break;


                    case "MAC":
                        incentiveAmount = 0;

                        foreach (var store in item.GroupBy(d => d.StoreId))
                        {
                            //var clusterIds = string.Join(",", store.Select(s => s.ClusterId).Distinct().ToList());

                            // var achievedVal = (await elasticSqlHelper.GetListAsync($"select count(distinct custid) as val from {platformIdxName} where createddate>='{sDate}' and status in ('Delivered','sattled') and createddate<'{eDate}' and storeid={store.Key} and executiveid ={peopleId} ")).FirstOrDefault();
                            var achievedVal = OrderData.Where(x => x.storeid == store.Key).Select(x => x.custid).Distinct().Count();
                            var Achievement = achievedVal;//achievedVal.val;
                            var Target = Targets.Where(s => store.Select(d => d.KPIId).Contains(s.KPIID)).ToList();
                            incentiveAmount += Target.Sum(d => d.IncentiveAmount);

                            //var AchievementPercent = 100 * (Achievement / Target.Sum(d => d.Target));
                            //var incentivePercent = item.Where(s => Target.Select(a => a.KPIID).Contains(s.KPIId) && s.AchievePercent <= AchievementPercent).OrderByDescending(s => s.AchievePercent).FirstOrDefault();

                            //kpi.Earning += incentivePercent == null ? 0 : Math.Round((incentivePercent.IncentiveAmount * incentivePercent.IncentivePercent) / 100, 0);
                            kpi.Achievement += Achievement;//Math.Round(Achievement, 0);
                            kpi.Target += Target.Sum(d => d.Target);
                        }

                        break;

                    case "Success Stores":
                        incentiveAmount = 0;

                        var date = DateTime.Now;
                        var isMonthComplete = false;
                        if (date.Day < 3)
                        {
                            isMonthComplete = true;
                            date = DateTime.Now.AddMonths(-1);
                        }
                        var lastDayOfMonth = DateTime.DaysInMonth(date.Year, date.Month);

                        var mongoHelper = new MongoHelper<CustomersTargets.MonthlyCustomerTarget>();
                        string DocumentName = "MonthlyTargetData_" + date.Month.ToString() + date.Year.ToString();
                        var query = $"select skcode, storeid, count(distinct itemnumber) linecount,  sum(dispatchqty * price) dispatchamount   from {platformIdxName} where createddate>='{sDate}' and createddate<'{eDate}' and status in ('Delivered','sattled') and executiveid ={peopleId} group by skcode,storeid";
                        var skCodeDataLists = await skcodeElasticHelper.GetListAsync(query);
                        foreach (var store in item.GroupBy(d => d.StoreId))
                        {
                            // var clusterIds = string.Join(",", store.Select(s => s.ClusterId).Distinct().ToList());

                            //var query = $"select skcode, storeid, count(distinct itemnumber) linecount,  sum(dispatchqty * price) dispatchamount   from {platformIdxName} where createddate>='{sDate}' and createddate<'{eDate}' and status in ('Delivered','sattled') and storeid={store.Key} and executiveid ={peopleId} group by skcode,storeid";
                            //var skCodeDataList = await skcodeElasticHelper.GetListAsync(query);

                            var skCodeDataList = skCodeDataLists.Where(x => x.storeid == store.Key).ToList();

                            var allTarget = mongoHelper.Select(s => s.TargetOnStores != null
                                           && s.TargetOnStores.Count > 0
                                           && s.TargetOnStores.Any(d => d.StoreId == store.Key), collectionName: DocumentName).ToList();
                            List<CustStoreTargets> targetOnStores = new List<CustStoreTargets>();
                            foreach (var data in allTarget)
                            {
                                foreach (var storeTarget in data.TargetOnStores)
                                {
                                    targetOnStores.Add(new CustStoreTargets
                                    {
                                        skcode = data.Skcode,
                                        StoreId = storeTarget.StoreId,
                                        Target = storeTarget.Target,
                                        TargetLineItem = storeTarget.TargetLineItem
                                    });
                                }

                            }

                            //var targetOnStores = mongoHelper.GetWithProjection(s => s.TargetOnStores.Any(d => d.StoreId == store.Key),
                            //           s => s.TargetOnStores.Select(d => new CustStoreTargets
                            //           {
                            //               skcode = s.Skcode,
                            //               StoreId = d.StoreId,
                            //               Target = d.Target,
                            //               TargetLineItem = d.TargetLineItem
                            //           }).ToList(), DocumentName).SelectMany(d => d).ToList();

                            int Achievement = 0;

                            targetOnStores.ForEach(s =>
                            {
                                var skcodeData = skCodeDataList.FirstOrDefault(a => a.skcode == s.skcode && a.storeid == s.StoreId &&
                                    s.Target <= a.dispatchamount &&
                                    (!s.TargetLineItem.HasValue || (s.TargetLineItem.HasValue && s.TargetLineItem.Value <= a.linecount))
                                );

                                if (skcodeData != null)
                                    Achievement++;

                            });


                            var Target = Targets.Where(s => store.Select(d => d.KPIId).Contains(s.KPIID)).ToList();

                            incentiveAmount += Target.Sum(d => d.IncentiveAmount);

                            //var AchievementPercent = 100 * (Achievement / Target.Sum(d => d.Target));
                            //var incentivePercent = item.Where(s => Target.Select(a => a.KPIID).Contains(s.KPIId) && s.AchievePercent <= AchievementPercent).OrderByDescending(s => s.AchievePercent).FirstOrDefault();

                            //kpi.Earning += incentivePercent == null ? 0 : Math.Round((incentivePercent.IncentiveAmount * incentivePercent.IncentivePercent) / 100, 0);
                            kpi.Achievement += Achievement;
                            kpi.Target += Target.Sum(d => d.Target);
                        }

                        break;

                }

                //kpi.AchievementPercent = Math.Round(100 * (kpi.Achievement / kpi.Target), 0);
                kpi.AchievementPercent = kpi.Target > 0 ? Math.Round(100 * (kpi.Achievement / kpi.Target), 0) : 0;
                var incentives = item.GroupBy(d => new { d.AchievePercent, d.IncentivePercent });
                var incentivePercent = incentives.Where(s => s.Key.AchievePercent <= kpi.AchievementPercent).OrderByDescending(s => s.Key.AchievePercent)?.FirstOrDefault()?.Key?.IncentivePercent ?? 0;
                kpi.Earning += incentivePercent == 0 ? 0 : Math.Round((incentiveAmount * incentivePercent) / 100, 0);
                salesPersonKpi.Add(kpi);

            }
            return salesPersonKpi;
        }

        //[Route("GetSalesPersonKPI")]
        //[HttpGet]
        //public async Task<List<SalesKpiResponse>> GetSalesPersonKPI(int peopleId, int warehouseId, int month, int year)
        //{
        //    ItemMasterManager manager = new ItemMasterManager();
        //    var configs = await manager.GetSalesPersonKPIConfigs(peopleId, warehouseId, month, year);

        //    DateTime startDate = new DateTime(year, month, 1);
        //    string sDate = startDate.ToString("yyyy-MM-dd");
        //    string eDate = startDate.Date.AddMonths(1).AddDays(1).ToString("yyyy-MM-dd");
        //    //List<SalesPersonKpiResponse> salesPersonKpi = new List<SalesPersonKpiResponse>();
        //    List<SalesKpiResponse> salesKpiResponses = new List<SalesKpiResponse>();


        //    bool Isdigitalexecutive = false;


        //    using (var authContext = new AuthContext())
        //    {
        //        string query = "select distinct r.Name as Role from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where PeopleID='" + peopleId + "'and ur.isActive=1 and p.Active=1 and p.Deleted=0";
        //        var role = authContext.Database.SqlQuery<string>(query).ToList();
        //        Isdigitalexecutive = role.Any(x => x.Contains("Digital sales executive") || x.Contains("Telecaller"));
        //    }

        //    foreach (var StoreData in configs.GroupBy(s => new { s.StoreId, s.StoreName }))
        //    {
        //        List<SalesPersonKpiResponse> salesPersonKpi = new List<SalesPersonKpiResponse>();
        //        SalesKpiResponse salesKpiResponse = new SalesKpiResponse();
        //        salesKpiResponse.StoreId = StoreData.Key.StoreId;
        //        salesKpiResponse.StoreName = StoreData.Key.StoreName;

        //        foreach (var item in StoreData.GroupBy(s => new { s.KpiName, s.DisplayName, s.Type}))
        //        {
        //            SalesPersonKpiResponse kpi = new SalesPersonKpiResponse
        //            {
        //                Month = month,
        //                Year = year,
        //                KpiName = item.Key.KpiName,
        //                DisplayName = string.IsNullOrEmpty(item.Key.DisplayName) ? item.Key.KpiName : item.Key.DisplayName,
        //                Type = item.Key.Type
        //            };
        //            ElasticSqlHelper<SalesPersonKPIOrderData> elasticSqlHelperOrdeData = new ElasticSqlHelper<SalesPersonKPIOrderData>();
        //            ElasticSqlHelper<doubleVal> elasticSqlHelper = new ElasticSqlHelper<doubleVal>();
        //            ElasticSqlHelper<SalesPersonKpiElasticData> skcodeElasticHelper = new ElasticSqlHelper<SalesPersonKpiElasticData>();

        //            //var clusterIds = string.Join(",", item.Select(s => s.ClusterId).Distinct().ToList());


        //            var Targets = item.GroupBy(s => s.KPIId).Select(d => new { KPIID = d.Key, Target = d.FirstOrDefault().Target, IncentiveAmount = d.FirstOrDefault().IncentiveAmount });
        //            double incentiveAmount = 0;
        //            var queryOrderData = $"select skcode, storeid, itemnumber, dispatchqty , price,custid  from {platformIdxName} where createddate>= '{sDate}' and createddate<'{eDate}' and status in ('Delivered', 'sattled') and executiveid = { peopleId } and IIF(isdigitalorder is null, false, isdigitalorder) = false";

        //            if (Isdigitalexecutive)
        //                queryOrderData = $"select skcode, storeid, itemnumber, dispatchqty , price,custid  from {platformIdxName} where createddate>= '{sDate}' and createddate<'{eDate}' and status in ('Delivered', 'sattled') and ordertakensalespersonid = { peopleId } and IIF(isdigitalorder is null, false, isdigitalorder) = true";

        //            var OrderData = (await elasticSqlHelperOrdeData.GetListAsync(queryOrderData)).ToList();
        //            switch (item.Key.KpiName)
        //            {
        //                case "MTD":

        //                    incentiveAmount = 0;
        //                    foreach (var store in item.GroupBy(d => d.StoreId))
        //                    {
        //                        //var clusterIds = string.Join(",", store.Select(s => s.ClusterId).Distinct().ToList());
        //                        //Old Query: var achievedVal = (await elasticSqlHelper.GetListAsync($"select sum(dispatchqty * price) as val from {platformIdxName} where createddate>='{sDate}' and status in ('Delivered','sattled') and createddate<'{eDate}'  and storeid={store.Key} and executiveid ={peopleId} ")).FirstOrDefault();
        //                        var achievedVal = OrderData.Where(x => x.storeid == store.Key).Select(y => new { y.dispatchqty, y.price }).Sum(y => y.dispatchqty * y.price);
        //                        var Achievement = achievedVal;
        //                        var Target = Targets.Where(s => store.Select(d => d.KPIId).Contains(s.KPIID)).ToList();
        //                        incentiveAmount += Target.Sum(d => d.IncentiveAmount);

        //                        // var AchievementPercent = 100 * (Achievement / Target.Sum(d => d.Target));
        //                        // var incentivePercent = item.Where(s => Target.Select(a => a.KPIID).Contains(s.KPIId) && s.AchievePercent <= AchievementPercent).OrderByDescending(s => s.AchievePercent).FirstOrDefault();
        //                        // kpi.Earning += incentivePercent == null ? 0 : Math.Round((incentivePercent.IncentiveAmount * incentivePercent.IncentivePercent) / 100, 0);
        //                        kpi.Achievement += Math.Round(Achievement, 0);
        //                        kpi.Target += Target.Sum(d => d.Target);
        //                    }
        //                    break;


        //                case "MAC":
        //                    incentiveAmount = 0;

        //                    foreach (var store in item.GroupBy(d => d.StoreId))
        //                    {
        //                        //var clusterIds = string.Join(",", store.Select(s => s.ClusterId).Distinct().ToList());

        //                        // var achievedVal = (await elasticSqlHelper.GetListAsync($"select count(distinct custid) as val from {platformIdxName} where createddate>='{sDate}' and status in ('Delivered','sattled') and createddate<'{eDate}' and storeid={store.Key} and executiveid ={peopleId} ")).FirstOrDefault();
        //                        var achievedVal = OrderData.Where(x => x.storeid == store.Key).Select(x => x.custid).Distinct().Count();
        //                        var Achievement = achievedVal;//achievedVal.val;
        //                        var Target = Targets.Where(s => store.Select(d => d.KPIId).Contains(s.KPIID)).ToList();
        //                        incentiveAmount += Target.Sum(d => d.IncentiveAmount);

        //                        //var AchievementPercent = 100 * (Achievement / Target.Sum(d => d.Target));
        //                        //var incentivePercent = item.Where(s => Target.Select(a => a.KPIID).Contains(s.KPIId) && s.AchievePercent <= AchievementPercent).OrderByDescending(s => s.AchievePercent).FirstOrDefault();

        //                        //kpi.Earning += incentivePercent == null ? 0 : Math.Round((incentivePercent.IncentiveAmount * incentivePercent.IncentivePercent) / 100, 0);
        //                        kpi.Achievement += Achievement;//Math.Round(Achievement, 0);
        //                        kpi.Target += Target.Sum(d => d.Target);
        //                    }

        //                    break;

        //                case "Success Stores":
        //                    incentiveAmount = 0;

        //                    var date = DateTime.Now;
        //                    var isMonthComplete = false;
        //                    if (date.Day < 3)
        //                    {
        //                        isMonthComplete = true;
        //                        date = DateTime.Now.AddMonths(-1);
        //                    }
        //                    var lastDayOfMonth = DateTime.DaysInMonth(date.Year, date.Month);

        //                    var mongoHelper = new MongoHelper<CustomersTargets.MonthlyCustomerTarget>();
        //                    string DocumentName = "MonthlyTargetData_" + date.Month.ToString() + date.Year.ToString();
        //                    var query = $"select skcode, storeid, count(distinct itemnumber) linecount,  sum(dispatchqty * price) dispatchamount   from {platformIdxName} where createddate>='{sDate}' and createddate<'{eDate}' and status in ('Delivered','sattled') and executiveid ={peopleId} group by skcode,storeid";
        //                    var skCodeDataLists = await skcodeElasticHelper.GetListAsync(query);
        //                    foreach (var store in item.GroupBy(d => d.StoreId))
        //                    {
        //                        // var clusterIds = string.Join(",", store.Select(s => s.ClusterId).Distinct().ToList());

        //                        //var query = $"select skcode, storeid, count(distinct itemnumber) linecount,  sum(dispatchqty * price) dispatchamount   from {platformIdxName} where createddate>='{sDate}' and createddate<'{eDate}' and status in ('Delivered','sattled') and storeid={store.Key} and executiveid ={peopleId} group by skcode,storeid";
        //                        //var skCodeDataList = await skcodeElasticHelper.GetListAsync(query);

        //                        var skCodeDataList = skCodeDataLists.Where(x => x.storeid == store.Key).ToList();

        //                        var allTarget = mongoHelper.Select(s => s.TargetOnStores != null
        //                                       && s.TargetOnStores.Count > 0
        //                                       && s.TargetOnStores.Any(d => d.StoreId == store.Key), collectionName: DocumentName).ToList();
        //                        List<CustStoreTargets> targetOnStores = new List<CustStoreTargets>();
        //                        foreach (var data in allTarget)
        //                        {
        //                            foreach (var storeTarget in data.TargetOnStores)
        //                            {
        //                                targetOnStores.Add(new CustStoreTargets
        //                                {
        //                                    skcode = data.Skcode,
        //                                    StoreId = storeTarget.StoreId,
        //                                    Target = storeTarget.Target,
        //                                    TargetLineItem = storeTarget.TargetLineItem
        //                                });
        //                            }

        //                        }

        //                        //var targetOnStores = mongoHelper.GetWithProjection(s => s.TargetOnStores.Any(d => d.StoreId == store.Key),
        //                        //           s => s.TargetOnStores.Select(d => new CustStoreTargets
        //                        //           {
        //                        //               skcode = s.Skcode,
        //                        //               StoreId = d.StoreId,
        //                        //               Target = d.Target,
        //                        //               TargetLineItem = d.TargetLineItem
        //                        //           }).ToList(), DocumentName).SelectMany(d => d).ToList();

        //                        int Achievement = 0;

        //                        targetOnStores.ForEach(s =>
        //                        {
        //                            var skcodeData = skCodeDataList.FirstOrDefault(a => a.skcode == s.skcode && a.storeid == s.StoreId &&
        //                                s.Target <= a.dispatchamount &&
        //                                (!s.TargetLineItem.HasValue || (s.TargetLineItem.HasValue && s.TargetLineItem.Value <= a.linecount))
        //                            );

        //                            if (skcodeData != null)
        //                                Achievement++;

        //                        });


        //                        var Target = Targets.Where(s => store.Select(d => d.KPIId).Contains(s.KPIID)).ToList();

        //                        incentiveAmount += Target.Sum(d => d.IncentiveAmount);

        //                        //var AchievementPercent = 100 * (Achievement / Target.Sum(d => d.Target));
        //                        //var incentivePercent = item.Where(s => Target.Select(a => a.KPIID).Contains(s.KPIId) && s.AchievePercent <= AchievementPercent).OrderByDescending(s => s.AchievePercent).FirstOrDefault();

        //                        //kpi.Earning += incentivePercent == null ? 0 : Math.Round((incentivePercent.IncentiveAmount * incentivePercent.IncentivePercent) / 100, 0);
        //                        kpi.Achievement += Achievement;
        //                        kpi.Target += Target.Sum(d => d.Target);
        //                    }

        //                    break;

        //            }

        //            //kpi.AchievementPercent = Math.Round(100 * (kpi.Achievement / kpi.Target), 0);
        //            kpi.AchievementPercent = kpi.Target > 0 ? Math.Round(100 * (kpi.Achievement / kpi.Target), 0) : 0;
        //            var incentives = item.GroupBy(d => new { d.AchievePercent, d.IncentivePercent });
        //            var incentivePercent = incentives.Where(s => s.Key.AchievePercent <= kpi.AchievementPercent).OrderByDescending(s => s.Key.AchievePercent)?.FirstOrDefault()?.Key?.IncentivePercent ?? 0;
        //            kpi.Earning += incentivePercent == 0 ? 0 : Math.Round((incentiveAmount * incentivePercent) / 100, 0);
        //            salesPersonKpi.Add(kpi);
        //        }
        //        salesKpiResponse.SalesPersonKpi = salesPersonKpi;
        //        salesKpiResponses.Add(salesKpiResponse);
        //    }
        //    return salesKpiResponses;
        //}


        [Route("ExportSuccessStoreTarget")]
        [HttpPost]
        public async Task<List<ExportSuccessStoreDC>> ExportSuccessStoreTargetAsync(ExportSuccessStoreFilter exportSuccessStoreFilter)
        {
            List<ExportSuccessStoreDC> exportSuccessStoreListDC = new List<ExportSuccessStoreDC>();

            using (var db = new AuthContext())
            {
                string StoreName = db.StoreDB.Where(x => x.Id == exportSuccessStoreFilter.StoreId).Select(x => x.Name).FirstOrDefault();
                ItemMasterManager manager = new ItemMasterManager();
                var configs = await manager.GetSalesPersonKPISuccessStore(exportSuccessStoreFilter.WarehouseIds, exportSuccessStoreFilter.ClusterId, exportSuccessStoreFilter.StoreId, exportSuccessStoreFilter.Month, exportSuccessStoreFilter.Year);

                DateTime startDate = new DateTime(exportSuccessStoreFilter.Year, exportSuccessStoreFilter.Month, 1);
                string sDate = startDate.ToString("yyyy-MM-dd");
                string eDate = startDate.Date.AddMonths(1).ToString("yyyy-MM-dd");

                //foreach (var item in configs.GroupBy(s => new { s.KpiName, s.DisplayName, s.Type }))
                foreach (var item in configs.GroupBy(s => new { s.ExecutiveId, s.ExecutiveName }))
                {
                    ExportSuccessStoreDC kpi = new ExportSuccessStoreDC
                    {
                        Month = exportSuccessStoreFilter.Month,
                        Year = exportSuccessStoreFilter.Year,
                        //KpiName = item.f.Key.KpiName,
                        //DisplayName = string.IsNullOrEmpty(item.DisplayName) ? item.KpiName : item.DisplayName,
                        //Type = item.Type,
                        ExecutiveId = item.Key.ExecutiveId,
                        ExecutiveName = item.Key.ExecutiveName
                    };

                    ElasticSqlHelper<doubleVal> elasticSqlHelper = new ElasticSqlHelper<doubleVal>();
                    ElasticSqlHelper<SalesPersonKpiElasticData> skcodeElasticHelper = new ElasticSqlHelper<SalesPersonKpiElasticData>();

                    var clusterIds = string.Join(",", item.Select(s => s.ClusterId).Distinct().ToList());


                    var Targets = item.GroupBy(s => s.KPIId).Select(d => new { KPIID = d.Key, Target = d.FirstOrDefault().Target, IncentiveAmount = d.FirstOrDefault().IncentiveAmount });
                    double incentiveAmount = 0;

                    incentiveAmount = 0;

                    var date = DateTime.Now;
                    var isMonthComplete = false;
                    if (date.Day < 3)
                    {
                        isMonthComplete = true;
                        date = DateTime.Now.AddMonths(-1);
                    }
                    var lastDayOfMonth = DateTime.DaysInMonth(date.Year, date.Month);

                    var mongoHelper = new MongoHelper<CustomersTargets.MonthlyCustomerTarget>();
                    string DocumentName = "MonthlyTargetData_" + date.Month.ToString() + date.Year.ToString();

                    foreach (var store in item.GroupBy(d => d.StoreId))
                    {
                        //var clusterIds = string.Join(",", store.Select(s => s.ClusterId).Distinct().ToList());

                        var query = $"select skcode, storeid, count(distinct itemnumber) linecount,  sum(dispatchqty * price) dispatchamount   from {platformIdxName} where createddate>='{sDate}' and createddate<'{eDate}' and status in ('Delivered','sattled') and storeid={store.Key} and executiveid ={item.Key.ExecutiveId} and IIF(isdigitalorder is null, false, isdigitalorder) = false group by skcode,storeid";
                        var skCodeDataList = await skcodeElasticHelper.GetListAsync(query);

                        var allTarget = mongoHelper.Select(s => s.TargetOnStores != null
                                           && s.TargetOnStores.Count > 0
                                           && s.TargetOnStores.Any(d => d.StoreId == store.Key), collectionName: DocumentName).ToList();
                        List<CustStoreTargets> targetOnStores = new List<CustStoreTargets>();
                        foreach (var data in allTarget)
                        {
                            foreach (var storeTarget in data.TargetOnStores)
                            {
                                targetOnStores.Add(new CustStoreTargets
                                {
                                    skcode = data.Skcode,
                                    StoreId = storeTarget.StoreId,
                                    Target = storeTarget.Target,
                                    TargetLineItem = storeTarget.TargetLineItem
                                });
                            }

                        }

                        //var targetOnStores = mongoHelper.GetWithProjection(s => s.TargetOnStores.Any(d => d.StoreId == store.Key),
                        //           s => s.TargetOnStores.Select(d => new CustStoreTargets
                        //           {
                        //               skcode = s.Skcode,
                        //               StoreId = d.StoreId,
                        //               Target = d.Target,
                        //               TargetLineItem = d.TargetLineItem
                        //           }).ToList(), DocumentName).SelectMany(d => d).ToList();

                        int Achievement = 0;

                        targetOnStores.ForEach(s =>
                        {
                            var skcodeData = skCodeDataList.FirstOrDefault(a => a.skcode == s.skcode && a.storeid == s.StoreId &&
                                s.Target <= a.dispatchamount &&
                                (!s.TargetLineItem.HasValue || (s.TargetLineItem.HasValue && s.TargetLineItem.Value <= a.linecount))
                            );

                            if (skcodeData != null)
                                Achievement++;

                        });



                        var Target = Targets.Where(s => store.Select(d => d.KPIId).Contains(s.KPIID)).ToList();

                        incentiveAmount += Target.Sum(d => d.IncentiveAmount);

                        kpi.Achievement += Achievement;
                        kpi.Target += Target.Sum(d => d.Target);
                    }

                    kpi.AchievementPercent = Math.Round(100 * (kpi.Achievement / kpi.Target), 0);

                    var incentives = item.GroupBy(d => new { d.AchievePercent, d.IncentivePercent });
                    var incentivePercent = incentives.Where(s => s.Key.AchievePercent <= kpi.AchievementPercent).OrderByDescending(s => s.Key.AchievePercent)?.FirstOrDefault()?.Key?.IncentivePercent ?? 0;
                    kpi.Earning += incentivePercent == 0 ? 0 : Math.Round((incentiveAmount * incentivePercent) / 100, 0);
                    kpi.Store = !string.IsNullOrEmpty(StoreName) ? StoreName : string.Empty;
                    kpi.KpiName = "Success Store";
                    exportSuccessStoreListDC.Add(kpi);
                }
            }

            return exportSuccessStoreListDC;
        }
        [Route("InsertChannelMarginDataJob")]
        [HttpGet]
        public bool InsertChannelMarginDataJob()
        {
            var Currentdate = DateTime.Now;
            var previousdate = DateTime.Now.AddMonths(-1);
            if (Currentdate.Day == 1)
            {
                using (var authContext = new AuthContext())
                {
                    DateTime Startdate = new DateTime(previousdate.Year, previousdate.Month, 1);
                    DateTime Enddate = new DateTime(Currentdate.Year, Currentdate.Month, 1);

                    var startdate = new SqlParameter("@startdate", Startdate);
                    var enddate = new SqlParameter("@enddate", Enddate);
                    authContext.Database.ExecuteSqlCommand("EXEC InsertChannelMarginData @startdate,@enddate", startdate, enddate);
                }
            }
            return true;
        }
    }
}
public class ItemlistDc
{
    // public int ItemId { get; set; }
    public int SubCategoryMappingId { get; set; }
    public int BrandCategoryMappingId { get; set; }
}
public class BrandlistsDc
{
    public string BrandName { get; set; }
    public int SubsubCategoryid { get; set; }
    public int SubCategoryMappingId { get; set; }
    public int BrandCategoryMappingId { get; set; }
}
public class GetListDc
{
    public List<int> ExecutiveIds { get; set; }
    public int warehouseId { get; set; }
    public DateTime month { get; set; }
}
public class responceDc
{
    public bool Status { get; set; }
    public string Message { get; set; }
}
public class storeExDc
{
    public int ExecutiveId { get; set; }
    public string ExecutiveName { get; set; }
}
public class SubCategoryDC
{
    public int SubCategoryId { get; set; }
    public string SubcategoryName { get; set; }
}

public class ExportSuccessStoreFilter
{
    public int Month { get; set; }
    public int Year { get; set; }
    public List<int> WarehouseIds { get; set; }
    public List<int> ClusterId { get; set; }
    public int StoreId { get; set; }
}

public class ExportSuccessStoreDC
{
    public int ExecutiveId { get; set; }
    public string ExecutiveName { get; set; }
    public double Target { get; set; }
    public string Store { get; set; }
    //public int Achivement { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }

    public string KpiName { get; set; }
    //public string DisplayName { get; set; }
    public string Type { get; set; }
    public double Achievement { get; set; }
    public double AchievementPercent { get; set; }
    public double Earning { get; set; }
    /*
   
   
   
    public double Target { get; set; }
    public double Achievement { get; set; }
    public double AchievementPercent { get; set; }
    public double Earning { get; set; }
    */
}