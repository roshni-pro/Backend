using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts;
using AngularJSAuthentication.DataContracts.ServiceRequestParam;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.Model.Base.Audit;
using AngularJSAuthentication.Model.RequestParams;
using LinqKit;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllersReports
{
    [RoutePrefix("api/History")]
    public class EntityHistoryController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Get Entity History to show on UI
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        [Route("")]
        [AcceptVerbs("GET")]
        public AuditBaseToShow Get(string entityName, string entityId)
        {
            var result = new AuditBaseToShow();
            try
            {
                MongoDbHelper<Audit> mongoDbHelper = new MongoDbHelper<Audit>();
                var predicate = PredicateBuilder.New<Audit>(x => x.PkValue == entityId);

                var auditList = mongoDbHelper.Select(predicate, x => x.OrderByDescending(z => z.AuditDate), null, null, false, null, entityName + "_Audit").ToList();

                if (auditList != null && auditList.Any())
                {
                    var historyList = auditList.SelectMany(z => z.AuditFields.Where(y => y.FieldName != "CreatedDate" && y.FieldName != "UpdatedDate" && y.FieldName != "UpdateBy" && y.FieldName != "UpdatedBy" && y.FieldName != "CreatedBy")
                    .Select(x => new AuditHistory
                    {
                        AuditAction = z.AuditAction,
                        AuditDate = z.AuditDate,
                        AuditEntity = z.AuditEntity,
                        AuditId = z.AuditId,
                        FieldName = x.FieldName,
                        NewValue = x.NewValue,
                        OldValue = x.OldValue,
                        UserName = z.UserName
                    })).ToList();


                    historyList = historyList.OrderByDescending(x => x.AuditDate).ToList();
                    result.AuditEntity = historyList.FirstOrDefault().AuditEntity;
                    result.FieldNames = historyList.GroupBy(x => x.FieldName).Select(x => x.Key).OrderBy(x => x).ToList();
                    result.AuditHistory = historyList.OrderByDescending(x => x.AuditDate).GroupBy(x => new { x.AuditId, x.AuditAction, x.AuditDate, x.AuditEntity, x.UserName }).Select(x => new AuditHistoryToShow
                    {
                        AuditAction = x.Key.AuditAction,
                        AuditDate = x.Key.AuditDate,
                        UserName =  x.Key.UserName == "DeliveryApp" ? "SarthiApp" : x.Key.UserName,
                        AuditFields = x.OrderBy(z => z.FieldName).Select(z => new AuditFieldsToShow
                        {
                            FieldName = z.FieldName,
                            NewValue = z.NewValue,
                            OldValue = z.OldValue
                        }).ToList()
                    }).ToList();
                    foreach (var history in result.AuditHistory)
                    {
                        var FieldList = result.FieldNames.Where(x => !history.AuditFields.Where(y => y.FieldName == x).Any()).ToList();
                        foreach (var field in FieldList)
                        {
                            AuditFieldsToShow auditFieldsToShow = new AuditFieldsToShow();
                            auditFieldsToShow.FieldName = field;
                            auditFieldsToShow.NewValue  = "";
                            auditFieldsToShow.OldValue = "";
                            history.AuditFields.Add(auditFieldsToShow);
                        }
                    }
                    result.AuditHistory.ForEach(x=>
                    {
                        x.AuditFields = x.AuditFields.OrderBy(y => y.FieldName).ToList();
                    });
                }


                return result;

            }
            catch (Exception ex)
            {
                logger.Error(new StringBuilder("Error while getting Audit Info : ").Append(ex.ToString()).ToString());
            }

            return null;
        }


        [AllowAnonymous]
        [Route("GetUserHistory/{entityName}/{peopleiId}/{startDate}/{endDate}")]
        [AcceptVerbs("GET")]
        public async Task<string> GetUserHistory(string entityName, int peopleiId, DateTime startDate, DateTime endDate)
        {
            var result = new List<AuditHistoryToExport>();
            endDate = endDate.Date.AddDays(1);
            string fileUrl = "";

            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/EmployeeAuditLogs/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);


            try
            {

                string userName = "";

                using (var context = new AuthContext())
                {
                    context.Database.CommandTimeout = 600;
                    //var query = "select c.WarehouseId, sum(c.CurrentInventory * item.purchasePrice) as Inventory from CurrentStocks c with (nolock) inner join GMWarehouseProgresses gw with (nolock) on c.WarehouseId=gw.WarehouseID and gw.IsLaunched=1 and c.WarehouseId!=67  cross apply(select max(i.netpurchaseprice) purchasePrice from  itemmasters i  with (nolock) where c.WarehouseId = i.WarehouseId and c.ItemNumber = i.Number and c.ItemMultiMRPId = i.ItemMultiMRPId and i.Deleted=0 group by i.WarehouseId, i.ItemMultiMRPId ) item group by c.WarehouseId";
                    //hubInventory = context.Database.SqlQuery<HubAvgInventory>(query).ToList();
                    var query = $"select a.UserName from people p with(nolock) inner join AspNetUsers a with(nolock) on a.Email =p.Email and p.PeopleID={peopleiId}";
                    userName = context.Database.SqlQuery<string>(query).FirstOrDefault();
                }

                MongoDbHelper<Audit> mongoDbHelper = new MongoDbHelper<Audit>();
                var predicate = PredicateBuilder.New<Audit>(x => x.UserName == userName
                                                             //&& x.AuditFields.Any(d => d.NewValue!= d.OldValue)
                                                             && x.AuditDate >= startDate && x.AuditDate < endDate

                );

                var auditList = mongoDbHelper.Select(predicate, x => x.OrderByDescending(z => z.AuditDate), null, null, false, null, entityName + "_Audit").ToList();

                if (auditList != null && auditList.Any(s => s.AuditFields.Any(d => d.OldValue != d.NewValue)))
                {

                    foreach (var item in auditList.GroupBy(s => new { s.PkValue, s.AuditDate, s.AuditAction, s.UserName }))
                    {
                        result.AddRange
                        (
                            item.SelectMany(d => d.AuditFields.Where(f => (string.IsNullOrEmpty(f.OldValue) && !string.IsNullOrEmpty(f.NewValue)) || (!string.IsNullOrEmpty(f.OldValue) && !string.IsNullOrEmpty(f.NewValue) && f.OldValue.Trim() != f.NewValue.Trim())).Select(a => new AuditHistoryToExport
                            {
                                AuditAction = item.Key.AuditAction,
                                AuditDate = item.Key.AuditDate,
                                FieldName = a.FieldName,
                                NewValue = a.NewValue,
                                OldValue = a.OldValue,
                                PkValue = item.Key.PkValue,
                                UserName = item.Key.UserName
                            })).ToList()
                        );

                    }

                    var dataTables = new List<DataTable>();
                    DataTable dt = ClassToDataTable.CreateDataTable(result);
                    dt.TableName = $"{peopleiId}_AuditLog";
                    dataTables.Add(dt);

                    string baseFileName = $"{peopleiId}_AuditLog_{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}.xlsx";
                    string filePath = ExcelSavePath + baseFileName;
                    ExcelGenerator.DataTable_To_Excel(dataTables, filePath);

                    fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                  , HttpContext.Current.Request.Url.DnsSafeHost
                                                                  , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                  , "ExcelGeneratePath/EmployeeAuditLogs/" + baseFileName);

                }


                return fileUrl;

            }
            catch (Exception ex)
            {
                logger.Error(new StringBuilder("Error while getting Audit Info : ").Append(ex.ToString()).ToString());
            }

            return null;
        }


        [Route("GetTodayTrace")]
        [AcceptVerbs("GET")]
        public List<TraceLog> GetTodayTraceLog()
        {
            var result = new AuditBaseToShow();
            var tomorrow = DateTime.Now.Date.AddDays(1);
            var yesterday = DateTime.Now.Date.AddDays(-1);
            MongoDbHelper<TraceLog> mongoDbHelper = new MongoDbHelper<TraceLog>();
            return mongoDbHelper.Select(x => x.CreatedDate > yesterday && x.CreatedDate < tomorrow).OrderByDescending(x => x.CreatedDate).ThenBy(x => x.CoRelationId).ToList();
        }

        [Route("GetTraceHistory")]
        [AcceptVerbs("Post")]
        public MongoLog GetTraceLog(GetTraceLogRequest param)
        {
            var date = DateTime.Parse(param.dateStr).Date;
            var result = new AuditBaseToShow();
            MongoDbHelper<TraceLog> mongoDbHelper = new MongoDbHelper<TraceLog>();

            var tomorrow = param.StartTime.HasValue && param.EndTime.HasValue ? date.Date.Add(param.EndTime.Value.TimeOfDay) : date.Date.AddDays(1).AddMilliseconds(-1);
            var yesterday = param.StartTime.HasValue && param.EndTime.HasValue ? date.Date.Add(param.StartTime.Value.TimeOfDay) : date.Date.AddDays(-1).AddMilliseconds(1);

            var traceLogPredicate = PredicateBuilder.New<TraceLog>(x => x.CreatedDate < tomorrow && x.CreatedDate > yesterday);

            if (!string.IsNullOrEmpty(param.apiName))
            {
                traceLogPredicate.And(x => x.RequestInfo.ToLower().Contains(param.apiName.ToLower()));
            }

            if (!string.IsNullOrEmpty(param.userName))
            {
                traceLogPredicate.And(x => x.UserName.ToLower().Contains(param.userName.ToLower()));
            }

            if (!string.IsNullOrEmpty(param.CoRelationId))
            {
                traceLogPredicate.And(x => x.CoRelationId == param.CoRelationId);
            }

            if (!string.IsNullOrEmpty(param.MessageSearch))
                traceLogPredicate.And(x => x.RequestInfo.ToLower().Contains(param.MessageSearch.ToLower()) || x.Message.ToLower().Contains(param.MessageSearch.ToLower()));

            MongoLog mongoLog = new MongoLog();
            mongoLog.TraceLogList = mongoDbHelper.Select(traceLogPredicate, x => x.OrderByDescending(z => z.CreatedDate).ThenBy(z => z.CoRelationId), param.Skip, param.Take, true, date.ToString(@"MMddyyyy")).ToList();
            mongoLog.Count = mongoDbHelper.Count(traceLogPredicate, true, date.ToString(@"MMddyyyy"));
            return mongoLog;
        }

        [Route("ExportEntityExcelByDate")]
        [AcceptVerbs("GET")]
        public string GetExcel(string entityName, DateTime NowDate)
        {
            var fileUrl = "";
            var result = new AuditBaseToShow();
            try
            {
                using (AuthContext context = new AuthContext())
                {

                    MongoDbHelper<Audit> mongoDbHelper = new MongoDbHelper<Audit>();
                    var predicate = PredicateBuilder.New<Audit>(x => x.AuditDate >= NowDate);

                    var auditList = mongoDbHelper.Select(predicate, null, null, null, false, null, entityName + "_Audit").ToList();

                    if (auditList != null && auditList.Any())
                    {
                        var historyList = auditList.SelectMany(z => z.AuditFields.Where(y => y.FieldName != "CreatedDate" && y.FieldName != "UpdatedDate" && y.FieldName != "UpdateBy" && y.FieldName != "UpdatedBy" && y.FieldName != "CreatedBy" && y.FieldName != "UserName")
                        .Select(x => new AuditHistory
                        {
                            AuditAction = z.AuditAction,
                            AuditDate = z.AuditDate,
                            AuditEntity = z.AuditEntity,
                            AuditId = z.AuditId,
                            FieldName = x.FieldName,
                            NewValue = x.NewValue,
                            OldValue = x.OldValue,
                            UserName = z.UserName
                        })).ToList();


                        historyList = historyList.OrderByDescending(x => x.AuditDate).ToList();
                        result.AuditEntity = historyList.FirstOrDefault().AuditEntity;
                        result.FieldNames = historyList.GroupBy(x => x.FieldName).Select(x => x.Key).OrderBy(x => x).ToList();
                        result.AuditHistory = historyList.OrderByDescending(x => x.AuditDate).GroupBy(x => new { x.AuditId, x.AuditAction, x.AuditDate, x.AuditEntity, x.UserName }).Select(x => new AuditHistoryToShow
                        {
                            AuditAction = x.Key.AuditAction,
                            AuditDate = x.Key.AuditDate,
                            UserName = x.Key.UserName,
                            AuditFields = x.OrderBy(z => z.FieldName).Select(z => new AuditFieldsToShow
                            {
                                FieldName = z.FieldName,
                                NewValue = z.NewValue,
                                OldValue = z.OldValue
                            }).ToList()
                        }).ToList();
                        List<DataTable> lstDataTable = new List<DataTable>();
                        DataTable dtNew = new DataTable(entityName + "_NewValue");
                        DataTable dtOld = new DataTable(entityName + "_OldValue");
                        dtNew.Columns.Add("UserName");
                        dtNew.Columns.Add("AuditDate");
                        dtOld.Columns.Add("UserName");
                        dtOld.Columns.Add("AuditDate");
                        foreach (var item in result.FieldNames)
                        {
                            dtNew.Columns.Add(item);
                            dtOld.Columns.Add(item);
                        }

                        foreach (var item in result.AuditHistory)
                        {
                            DataRow drnew = dtNew.NewRow();
                            DataRow drold = dtOld.NewRow();
                            drnew["UserName"] = item.UserName;
                            drnew["AuditDate"] = item.AuditDate;
                            drold["UserName"] = item.UserName;
                            drold["AuditDate"] = item.AuditDate;
                            for (int i = 1; i < dtNew.Columns.Count; i++)
                            {
                                if (item.AuditFields.Any(x => x.FieldName == dtNew.Columns[i].ColumnName))
                                {
                                    drnew[dtNew.Columns[i].ColumnName] = item.AuditFields.FirstOrDefault(x => x.FieldName == dtNew.Columns[i].ColumnName).NewValue;
                                    drold[dtNew.Columns[i].ColumnName] = item.AuditFields.FirstOrDefault(x => x.FieldName == dtNew.Columns[i].ColumnName).OldValue;
                                }
                            }
                            dtNew.Rows.Add(drnew);
                            dtOld.Rows.Add(drold);
                        }

                        lstDataTable.Add(dtOld);
                        lstDataTable.Add(dtNew);

                        string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
                        if (!Directory.Exists(ExcelSavePath))
                            Directory.CreateDirectory(ExcelSavePath);


                        var fileName = entityName + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + ".xlsx";

                        string filePath = ExcelSavePath + fileName;

                        ExcelGenerator.DataTable_To_Excel(lstDataTable, filePath);
                        fileUrl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                                , HttpContext.Current.Request.Url.DnsSafeHost
                                                                , HttpContext.Current.Request.Url.Port
                                                                , string.Format("ExcelGeneratePath/{0}", fileName));
                    }



                    return fileUrl;

                }
            }
            catch (Exception ex)
            {
                logger.Error(new StringBuilder("Error while getting Audit Info : ").Append(ex.ToString()).ToString());
            }

            return null;
        }


        [Route("GetCustomerHistoryExcel")]
        [AcceptVerbs("GET")]
        public string GetCustomerHistoryExcel()
        {
            var fileUrl = "";
            var result = new AuditBaseToShow();

            List<CustomerGstHistory> custBatch = new List<CustomerGstHistory>();
            List<CustomerGstHistory> gstHistoryList = new List<CustomerGstHistory>();
            int rowCount = 100;

            //using (var context = new AuthContext())
            //{
            //    rowCount = context.Database.SqlQuery<int>("select  count(*) from customers where DATEDIFF(MONTH, UpdatedDate ,getdate())<=6 ").FirstOrDefault();
            //}

            MongoDbHelper<Audit> mongoDbHelper = new MongoDbHelper<Audit>();
            int batchSize = 100;
            for (int i = 0; i < rowCount; i += batchSize)
            {
                using (AuthContext context = new AuthContext())
                {
                    custBatch = context.Database.SqlQuery<CustomerGstHistory>("select  CustomerId , SkCode,refno NewGstNo from customers where DATEDIFF(MONTH, UpdatedDate ,getdate())<=6 order by CustomerId offset " + i + " rows fetch next " + (i + batchSize) + " rows only").ToList();
                }

                var getCustIds = custBatch.Select(x => x.CustomerId.ToString());

                var auditList = mongoDbHelper.Select(x => getCustIds.Contains(x.PkValue), null, null, null, false, null, "Customer_Audit").ToList();

                custBatch.ForEach(x =>
                {
                    var history = auditList.Where(z => z.PkValue == x.CustomerId.ToString()).ToList();
                    if (history != null && history.Any())
                    {
                        var gstHistory = history.OrderByDescending(z => z.AuditDate)
                                            .SelectMany(z => z.AuditFields.Where(y => y.FieldName == "RefNo" && y.OldValue != x.NewGstNo).Select(a => new
                                            {
                                                UpdatedDate = z.AuditDate,
                                                OldGstNo = a.OldValue,
                                                NewGstNo = a.NewValue,
                                                UpdatedBy = z.UserName
                                            })).ToList();

                        var lastUpdatedGst = gstHistory.Where(a => x.NewGstNo != a.OldGstNo).OrderByDescending(z => z.UpdatedDate).FirstOrDefault();
                        if (lastUpdatedGst != null)
                        {
                            x.OldGstNo = lastUpdatedGst.OldGstNo;
                            //x.NewGstNo = lastUpdatedGst.NewGstNo;
                            x.UpdatedDate = lastUpdatedGst.UpdatedDate.ToShortDateString();
                            x.UpdatedBy = lastUpdatedGst.UpdatedBy;
                        }
                    }
                });

                gstHistoryList.AddRange(custBatch.Where(z => !string.IsNullOrEmpty(z.UpdatedBy) && z.OldGstNo != "NA" && z.NewGstNo != null).ToList());
            }


            if (gstHistoryList != null && gstHistoryList.Any())
            {
                var gstDt = ClassToDataTable.CreateDataTable(gstHistoryList);
                gstDt.TableName = "Customer GST History";
                string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
                if (!Directory.Exists(ExcelSavePath))
                    Directory.CreateDirectory(ExcelSavePath);


                var fileName = "CustomerGstHistory_" + DateTime.Now.ToString("yyyyddMHHmmss") + ".xlsx";

                string filePath = ExcelSavePath + fileName;

                ExcelGenerator.DataTable_To_Excel(new List<DataTable> { gstDt }, filePath);
                fileUrl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                        , HttpContext.Current.Request.Url.DnsSafeHost
                                                        , HttpContext.Current.Request.Url.Port
                                                        , string.Format("ExcelGeneratePath/{0}", fileName));
            }

            return fileUrl;


        }


        [Route("InsertPageVisits")]
        [AcceptVerbs("Post")]
        public async Task<bool> InsertPageVisits(PageVisits param)
        {
            using (var db = new AuthContext())
            {
                var date = DateTime.Now;
                MongoDbHelper<PageVisits> mongoDbHelper = new MongoDbHelper<PageVisits>();
                param.VisitedOn = date;
                var person = db.Peoples.Where(x => (x.PeopleFirstName == param.UserName || x.UserName == param.UserName || x.DisplayName == param.UserName || x.Email == param.UserName) && x.Active && !x.Deleted).OrderByDescending(x => x.PeopleID).FirstOrDefault();
                if (person != null)
                {
                    await mongoDbHelper.InsertAsync(param);

                    return true;
                }
                else
                {
                    return false;
                }

            }

        }

        [Route("RolePageVisits")]
        [AcceptVerbs("Post")]
        public async Task<string> RolePageVisits(PageVisits param)
        {
            using (var db = new AuthContext())
            {
                var date = DateTime.Now;

                MongoDbHelper<PageVisits> mongoDbHelper = new MongoDbHelper<PageVisits>();
                param.VisitedOn = date;
                var person = db.Peoples.Where(x => (x.PeopleFirstName == param.UserName || x.UserName == param.UserName || x.DisplayName == param.UserName || x.Email == param.UserName) && x.Active && !x.Deleted).OrderByDescending(x => x.PeopleID).FirstOrDefault();
                if (param.Route[0] == '/') param.Route = param.Route.Remove(0, 1);
                var id = db.PageMaster.Where(x => x.RouteName == param.Route && x.IsActive == true && x.IsDeleted == false).Select(y => y.Id).FirstOrDefault();
                var query2 = $@"select top 1 pm.PageName from People p 
                    inner join AspNetUsers anu on p.Email = anu.Email 
                    inner join AspNetUserRoles anur on anu.Id = anur.UserId 
                    inner join AspNetRoles anr on anur.RoleId = anr.Id 
                    inner join RolePagePermissions rp on rp.RoleId = anr.Id 
                    inner join PageMasters pm on pm.Id = rp.PageMasterId 
                    where anur.isActive = 1 and p.Active = 1 and p.Deleted = 0 and p.PeopleID = {person.PeopleID} 
                    and rp.IsActive = 1 and rp.IsDeleted = 0and pm.IsActive = 1 and pm.IsDeleted = 0 and pm.Id= {id}";

                var xx = db.Database.SqlQuery<string>(query2).FirstOrDefault();

                if (xx != null || param.Route == "layout")
                {
                    return xx;
                }
                else
                {
                    return xx;
                }

            }

        }

        [Route("CheckPagePermission")]
        [AcceptVerbs("Get")]
        public async Task<List<string>> CheckPagePermission(string PageName)
        {
            using (var db = new AuthContext())
            {
                if (!string.IsNullOrEmpty(PageName))
                {
                    string query = @"select distinct Name from AspNetRoles a where Id in (select RoleId from RolePagePermissions where PageMasterId 
                                 in(select top 1 Id from PageMasters where PageName=" + "'" + PageName + "'" + " and IsActive=1 and IsDeleted=0) and IsActive=1 and IsDeleted=0) ";
                    var RoleList = await db.Database.SqlQuery<string>(query).ToListAsync();
                    return RoleList;
                }
                else
                {
                    return null;
                }
            }

        }

        [Route("AngularPageList")]
        [AcceptVerbs("Get")]
        public async Task<PageListDC> AngularPageList(int userid)
        {
            using (var db = new AuthContext())
            {
                PageListDC pageListDC = new PageListDC();
                    if (db.Database.Connection.State != ConnectionState.Open)
                        db.Database.Connection.Open();
                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandTimeout = 900;
                    cmd.CommandText = "[dbo].[GetPageNameList]";
                    cmd.Parameters.Add(new SqlParameter("@PeopleID", userid));
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    var reader = cmd.ExecuteReader();
                    pageListDC.AllPageListDC = ((IObjectContextAdapter)db)
                    .ObjectContext
                    .Translate<AllPageListDC>(reader).ToList();

                    reader.NextResult();
                    if (reader.HasRows)
                    {
                        pageListDC.AssignedPageListDc = ((IObjectContextAdapter)db)
                                        .ObjectContext
                                        .Translate<AssignedPageListDc>(reader).ToList();
                    }
                    db.Database.Connection.Close();
                    return pageListDC;
            }

        }

        [AllowAnonymous]
        [Route("GetUserHistoryAuditPurpose")]
        [AcceptVerbs("GET")]
        public async Task<string> GetUserHistoryAuditPurpose()
        {
            var result = new List<AuditHistoryToExport>();
            var NewList = new List<AuditHistoryToExport>();
            string fileUrl = "";
            string entityName = "People";
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/EmployeeAuditLogs/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);


            try
            {
                MongoDbHelper<Audit> mongoDbHelper = new MongoDbHelper<Audit>();
                List<string> Peopleids = new List<string> {
"1483",
"1788",
"1829",
"2011",
"2021",
"2065",
"2080",
"2142",
"2174",
"2279",
"2494",
"2547",
"2783",
"2853",
"2880",
"3000",
"3009",
"3078",
"3082",
"3206",
"3291",
"3342",
"3360",
"3393",
"3453",
"3466",
"3475",
"3495",
"3522",
"3591",
"3692",
"3760",
"3771",
"3805",
"3836",
"3910",
"3918",
"3920",
"3925",
"3931",
"3932",
"3934",
"3936",
"3945",
"3962",
"3963",
"3964",
"4007",
"4094",
"4097",
"4098",
"4114",
"4115",
"4116",
"4126",
"4152",
"4154",
"4161",
"4178",
"4181",
"4214",
"4235",
"4261",
"4262",
"4274",
"4277",
"4310",
"4319",
"4327",
"4332",
"4336",
"4340",
"4402",
"4407",
"4408",
"4416",
"4435",
"4544",
"4631",
"4667",
"4701",
"4731",
"4751",
"4757",
"4787",
"4795",
"4802",
"4804",
"4809",
"4836",
"4891",
"4896",
"4959",
"4965",
"4966",
"4983",
"4997",
"5009",
"5019",
"5021",
"5029",
"5033",
"5048",
"5049",
"5082",
"5101",
"5113",
"5133",
"5136",
"5145",
"5157",
"5160",
"5174",
"5179",
"5200",
"5202",
"5206",
"5238",
"5256",
"5268",
"5351",
"5357",
"5358",
"5364",
"5401",
"5405",
"5413",
"5414",
"5438",
"5454",
"5462",
"5471",
"5479",
"5485",
"5486",
"5491",
"5500",
"5504",
"5507",
"5516",
"5521",
"5523",
"5524",
"5525",
"5531",
"5536",
"5590",
"5606",
"5607",
"5612",
"5617",
"5618",
"5619",
"5620",
"5632",
"5644",
"5665",
"5669",
"5672",
"5675",
"5676",
"5679",
"5680",
"5682",
"5689",
"5703",
"5784",
"5869",
"5886"
};
                // List<dynamic> auditList = new List<dynamic>();
                Peopleids.ForEach(PeopleId =>
                {
                    var predicate = PredicateBuilder.New<Audit>(x => x.PkValue == PeopleId
                    //&& x.AuditFields.Any(d => d.NewValue!= d.OldValue)
                    );

                    var auditList = mongoDbHelper.Select(predicate, x => x.OrderByDescending(z => z.AuditDate), null, null, false, null, entityName + "_Audit").ToList();

                    if (auditList != null && auditList.Any(s => s.AuditFields.Any(d => d.OldValue != d.NewValue)))
                    {
                        foreach (var item in auditList.GroupBy(s => new { s.PkValue, s.AuditDate, s.AuditAction, s.UserName }))
                        {
                            if (!NewList.Any(x => x.FieldName == "Active" && x.PkValue == item.Key.PkValue))
                            {
                                result.AddRange
                            (
                                item.SelectMany(d => d.AuditFields.Where(f => f.FieldName == "Active" && ((string.IsNullOrEmpty(f.OldValue) && !string.IsNullOrEmpty(f.NewValue)) || (!string.IsNullOrEmpty(f.OldValue) && !string.IsNullOrEmpty(f.NewValue) && f.OldValue.Trim() != f.NewValue.Trim()))).Select(a => new AuditHistoryToExport
                                {
                                    AuditAction = item.Key.AuditAction,
                                    AuditDate = item.Key.AuditDate,
                                    FieldName = a.FieldName,
                                    NewValue = a.NewValue,
                                    OldValue = a.OldValue,
                                    PkValue = item.Key.PkValue,
                                    UserName = item.Key.UserName
                                })).ToList()
                            );

                                foreach (var filter in result.GroupBy(s => new { s.PkValue }))
                                {
                                    if (!NewList.Any(x => x.FieldName == "Active" && x.PkValue == filter.Key.PkValue))
                                        NewList.Add(filter.Where(x => x.PkValue == filter.Key.PkValue).OrderByDescending(x => x.AuditDate).FirstOrDefault());
                                }
                            }
                        }

                    }
                });
                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(NewList);
                dt.TableName = $"People_AuditLog";
                dataTables.Add(dt);

                string baseFileName = $"People_AuditLog_{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}.xlsx";
                string filePath = ExcelSavePath + baseFileName;
                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);

                fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                              , HttpContext.Current.Request.Url.DnsSafeHost
                                                              , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                              , "ExcelGeneratePath/EmployeeAuditLogs/" + baseFileName);

                return fileUrl;

            }
            catch (Exception ex)
            {
                logger.Error(new StringBuilder("Error while getting Audit Info : ").Append(ex.ToString()).ToString());
            }

            return null;
        }

    }

    class CustomerGstHistory
    {
        public int CustomerId { get; set; }
        public string SKCode { get; set; }
        public string OldGstNo { get; set; }
        public string NewGstNo { get; set; }
        public string UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }

    }
}