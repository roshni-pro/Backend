using AngularJSAuthentication.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using NLog;
using AngularJSAuthentication.DataContracts.Shared;
using System.Threading.Tasks;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Model;
using System.Collections.Concurrent;
using System.Data.Entity;
using AngularJSAuthentication.API.Controllers;
using SqlBulkTools;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using System.Web.Hosting;
using MongoDB.Driver;
using MongoDB.Bson;
using Nito.AsyncEx;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;
using AngularJSAuthentication.DataContracts.ServiceRequestParam;
using AngularJSAuthentication.DataContracts;
using LinqKit;
using AgileObjects.AgileMapper;
using System.Security.Claims;
using AngularJSAuthentication.Common.Constants;
using static AngularJSAuthentication.API.Controllers.WarehouseController;
using AngularJSAuthentication.DataContracts.BatchCode;
using AngularJSAuthentication.BatchManager;

namespace AngularJSAuthentication.API.Managers
{
    public class ReportManager
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public bool GetCatSubCatLiveItem()
        {
            var table = new DataTable();
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
            using (var context = new AuthContext())
            {
                context.Database.CommandTimeout = 600;
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "GetCatSubCatLiveItem";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection.Open();
                table.Load(cmd.ExecuteReader());
            }


            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);


            if (table.Rows.Count > 0)
            {
                var datatables = table.AsEnumerable()
                                          .GroupBy(r => new { Col1 = r["WarehouseName"] })
                                          .Select(g => g.OrderBy(x => x["CategoryName"]).ThenBy(x => x["SubcategoryName"]).CopyToDataTable()).ToList();
                datatables.ForEach(x => { x.TableName = x.Rows[0][0].ToString(); });
                string filePath = ExcelSavePath + "LowInventoryAlert_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";

                if (ExcelGenerator.DataTable_To_Excel(datatables, filePath))
                {

                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='InventroyLow'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = "Alert : Low Inventory Report on " + DateTime.Now.ToString("dd MMM yyyy");
                    string message = "Please find attach Warehouse base low inventory. Please take action immediately on attach warehouse";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                    else
                        logger.Error("Low Inventory Report To and From empty");

                }
            }


            return true;


        }


        public async Task<bool> GenerateItemClassificationInActiveReport()
        {
            using (var authContext = new AuthContext())
            {
                var ItemClassificationInActiveReportList = await authContext.Database.SqlQuery<ItemClassificationInActiveReportDc>("exec GetItemClassificationInActiveReport").ToListAsync();
                if (ItemClassificationInActiveReportList.Any())
                {

                    foreach (var item in ItemClassificationInActiveReportList)
                    {

                        if (item.ItemActive == 0) { item.Comment = "Inactive"; } else { item.Comment = "Active But Not Consider"; }

                    }
                    string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

                    if (!Directory.Exists(ExcelSavePath))
                        Directory.CreateDirectory(ExcelSavePath);

                    var dataTables = new List<DataTable>();
                    DataTable dt = ClassToDataTable.CreateDataTable(ItemClassificationInActiveReportList);
                    dt.TableName = "Daily_ItemClassInActiveReport";
                    dataTables.Add(dt);

                    string filePath = ExcelSavePath + "Daily_ItemClassInActiveReport_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                    if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                    {
                        string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                        string To = "", From = "", Bcc = "";
                        DataTable emaildatatable = new DataTable();
                        using (var connection = new SqlConnection(connectionString))
                        {
                            using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='Daily_ItemClassInActiveReport'", connection))
                            {

                                if (connection.State != ConnectionState.Open)
                                    connection.Open();

                                SqlDataAdapter da = new SqlDataAdapter(command);
                                da.Fill(emaildatatable);
                                da.Dispose();
                                connection.Close();
                            }
                        }
                        if (emaildatatable.Rows.Count > 0)
                        {
                            To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                            From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                            Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                        }
                        string subject = DateTime.Now.ToString("dd MMM yyyy") + " Daily Item Classification InActive Report";
                        string message = "Please find attach Daily Item Classification InActive Report";
                        if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                            EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                        else
                            logger.Error("Daily Item Classification InActive Report");
                    }
                }
            }
            return true;
        }

        public async Task<bool> GenerateCFRReport()
        {
            using (var authContext = new AuthContext())
            {
                var ItemClassificationCFRReportList = await authContext.Database.SqlQuery<CFRReportDc>("exec Generate_CfrArticleReport").ToListAsync(); // old spGenerate_CFRReport
                if (ItemClassificationCFRReportList.Any())
                {

                    foreach (var item in ItemClassificationCFRReportList)
                    {

                        if (item.active == 0)
                        {
                            item.activeItem = "Inactive";
                        }
                        else if (item.active == 1)
                        {
                            item.activeItem = "Active";
                        }
                        else
                        {
                            item.activeItem = "Not Considered Active";
                        }

                    }

                    string ExcelSavePath = HostingEnvironment.MapPath("~/ExcelGeneratePath/");

                    if (!Directory.Exists(ExcelSavePath))
                        Directory.CreateDirectory(ExcelSavePath);

                    var dataTables = new List<DataTable>();
                    DataTable dt = ClassToDataTable.CreateDataTable(ItemClassificationCFRReportList);
                    dt.TableName = "Daily_CFR_Report";
                    dataTables.Add(dt);

                    string filePath = ExcelSavePath + "Daily_CFR_Report_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                    if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                    {
                        string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                        string To = "", From = "", Bcc = "";
                        DataTable emaildatatable = new DataTable();
                        using (var connection = new SqlConnection(connectionString))
                        {
                            using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='Daily_CFR_Report'", connection))
                            {

                                if (connection.State != ConnectionState.Open)
                                    connection.Open();

                                SqlDataAdapter da = new SqlDataAdapter(command);
                                da.Fill(emaildatatable);
                                da.Dispose();
                                connection.Close();
                            }
                        }
                        if (emaildatatable.Rows.Count > 0)
                        {
                            To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                            From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                            Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                        }
                        string subject = DateTime.Now.ToString("dd MMM yyyy") + " Daily CFR Report";
                        string message = "Please find attach Daily CFR Report";
                        if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                            EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                        else
                            logger.Error("Daily CFR Report");
                    }
                }
            }
            return true;
        }


        public async Task<bool> GenerateinActivePeopleReport()
        {
            using (var authContext = new AuthContext())
            {
                var inserPeopleList = await EmployeevisitList();
                authContext.Database.CommandTimeout = 6000;
                var InActivePeopleReportList = await authContext.Database.SqlQuery<InActivePeopleReportDC>("exec Generate_InActiveReport").ToListAsync(); // old spGenerate_CFRReport
                if (InActivePeopleReportList.Any())
                {
                    string ExcelSavePath = HostingEnvironment.MapPath("~/ExcelGeneratePath/");

                    if (!Directory.Exists(ExcelSavePath))
                        Directory.CreateDirectory(ExcelSavePath);

                    var dataTables = new List<DataTable>();
                    DataTable dt = ClassToDataTable.CreateDataTable(InActivePeopleReportList);
                    dt.TableName = "Daily_InActivePeople_Report_";
                    dataTables.Add(dt);

                    string filePath = ExcelSavePath + "Daily_InActivePeople_Report_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                    if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                    {
                        string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                        string To = "", From = "", Bcc = "";
                        DataTable emaildatatable = new DataTable();
                        using (var connection = new SqlConnection(connectionString))
                        {
                            using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='Daily_InActivePeople_Report_'", connection))
                            {

                                if (connection.State != ConnectionState.Open)
                                    connection.Open();

                                SqlDataAdapter da = new SqlDataAdapter(command);
                                da.Fill(emaildatatable);
                                da.Dispose();
                                connection.Close();
                            }
                        }
                        if (emaildatatable.Rows.Count > 0)
                        {
                            To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                            From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                            Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                        }
                        string subject = DateTime.Now.ToString("dd MMM yyyy") + " Daily InActive People Report";
                        string message = "Please find attach Daily InActive People Report";
                        if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                            EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                        else
                            logger.Error("Daily InActive People Report");
                    }
                }
            }
            return true;
        }

        public async Task<bool> EmpvisitList()
        {
            bool res = false;
            List<AppVisitDC> appVisitDCList = new List<AppVisitDC>();
            MongoDbHelper<AppVisits> mongoAppVisitDbHelper = new MongoDbHelper<AppVisits>();
            List<AppVisits> appVisitList = new List<AppVisits>();
            var searchPredicateDetail = PredicateBuilder.New<AppVisits>(x => x.UserName != null);
            appVisitList = mongoAppVisitDbHelper.Select(searchPredicateDetail).OrderByDescending(x => x.Id).ToList();
            appVisitDCList = Mapper.Map(appVisitList).ToANew<List<AppVisitDC>>();

            List<PeopleDC> peopledata = new List<PeopleDC>();
            InactivePeople inactivePeopleData = new InactivePeople();
            using (var myContext = new AuthContext())
            {
                peopledata = myContext.Database.SqlQuery<PeopleDC>("GetPeopleData").ToList();
                var backendVisitData = myContext.Database.SqlQuery<ERPPageVisistData>("PageVisitData").ToList();
                var InActiveCustomerInHrs = myContext.CompanyDetailsDB.Where(x => x.IsActive && x.InActiveCustomerInHrs > 0).OrderByDescending(x => x.Id).FirstOrDefault();
                if (backendVisitData.Count > 0)
                {
                    //var bckData = backendVisitData.Select(x => x.UserName).ToList();
                    //var pName = peopledata.Where(x => bckData.Contains(x.UserName)).ToList();
                    foreach (var er in backendVisitData)
                    {
                        er.RemainingTimeinHrs = ((er.VisitedOn - DateTime.Now.AddHours(-InActiveCustomerInHrs.InActiveCustomerInHrs)).Hours);
                        if (er.RemainingTimeinHrs < 0)//168
                        {
                            var pd = peopledata.Where(x => x.UserName == er.UserName).FirstOrDefault();
                            if (pd != null)
                            {
                                var inactivePeopleDetail = myContext.InactivePeopleDB.Where(x => x.PeopleID == pd.PeopleID && !x.Active && !x.Deleted).OrderByDescending(x => x.Id).FirstOrDefault();
                                if (inactivePeopleDetail == null && (Convert.ToDateTime(er.VisitedOn) < Convert.ToDateTime(DateTime.Now.AddHours(-InActiveCustomerInHrs.InActiveCustomerInHrs))))//DateTime.Today.AddDays(-7)
                                {
                                    inactivePeopleData.Active = false;
                                    inactivePeopleData.PeopleFirstName = pd.PeopleFirstName;
                                    inactivePeopleData.PeopleLastName = pd.PeopleLastName;
                                    inactivePeopleData.UserName = pd.UserName;
                                    inactivePeopleData.PeopleID = pd.PeopleID;
                                    inactivePeopleData.Mobile = pd.Mobile;
                                    inactivePeopleData.Empcode = pd.Empcode;
                                    inactivePeopleData.Department = pd.Department;
                                    inactivePeopleData.Desgination = pd.Desgination;
                                    inactivePeopleData.DisplayName = pd.DisplayName;
                                    inactivePeopleData.city = pd.city;
                                    inactivePeopleData.CreatedBy = GetLoginUserId();
                                    inactivePeopleData.CreationDate = DateTime.Now;
                                    inactivePeopleData.LastVisitedDate = er.VisitedOn;
                                    myContext.InactivePeopleDB.Add(inactivePeopleData);

                                    var people = myContext.Peoples.Where(x => x.PeopleID == pd.PeopleID).FirstOrDefault();
                                    people.Active = false;
                                    people.UpdatedDate = DateTime.Now;
                                    myContext.Entry(people).State = EntityState.Modified;
                                    myContext.Commit();
                                }
                            }


                        }
                    }
                }
                if (appVisitDCList.Count > 0)
                {
                    foreach (var appData in appVisitDCList)
                    {
                        appData.RemainingTimeinHrs = ((appData.VisitedOn - DateTime.Now.AddHours(-InActiveCustomerInHrs.InActiveCustomerInHrs)).Hours);
                        if (appData.RemainingTimeinHrs < 0)//168
                        {
                            var pd = peopledata.Where(x => x.UserName == appData.UserName).FirstOrDefault();
                            if (pd != null)
                            {
                                var inactivePeopleDetail = myContext.InactivePeopleDB.Where(x => x.PeopleID == pd.PeopleID && !x.Active && !x.Deleted).OrderByDescending(x => x.Id).FirstOrDefault();
                                if (inactivePeopleDetail == null && (Convert.ToDateTime(appData.VisitedOn) < Convert.ToDateTime(DateTime.Now.AddHours(-InActiveCustomerInHrs.InActiveCustomerInHrs))))//DateTime.Today.AddDays(-7)
                                {
                                    inactivePeopleData.Active = false;
                                    inactivePeopleData.PeopleFirstName = pd.PeopleFirstName;
                                    inactivePeopleData.PeopleLastName = pd.PeopleLastName;
                                    inactivePeopleData.UserName = pd.UserName;
                                    inactivePeopleData.PeopleID = pd.PeopleID;
                                    inactivePeopleData.Mobile = pd.Mobile;
                                    inactivePeopleData.Empcode = pd.Empcode;
                                    inactivePeopleData.Department = pd.Department;
                                    inactivePeopleData.Desgination = pd.Desgination;
                                    inactivePeopleData.DisplayName = pd.DisplayName;
                                    inactivePeopleData.city = pd.city;
                                    inactivePeopleData.CreatedBy = GetLoginUserId();
                                    inactivePeopleData.CreationDate = DateTime.Now;
                                    inactivePeopleData.LastVisitedDate = appData.VisitedOn;
                                    myContext.InactivePeopleDB.Add(inactivePeopleData);

                                    var people = myContext.Peoples.Where(x => x.PeopleID == pd.PeopleID).FirstOrDefault();
                                    people.Active = false;
                                    people.UpdatedDate = DateTime.Now;
                                    myContext.Entry(people).State = EntityState.Modified;
                                    myContext.Commit();
                                }
                            }
                        }
                    }

                }

            }
            return res;
        }

        public async Task<bool> EmployeevisitList()
        {
            bool res = false;
            using (var context = new AuthContext())
            {
                var InActiveCustomerInHrs = context.CompanyDetailsDB.Where(x => x.IsActive && x.InActiveCustomerInHrs > 0).OrderByDescending(x => x.Id).FirstOrDefault();
                //BackendVisitData
                var empVisitData = new List<EmpVisitDC>();
                var inactivePeople = new List<InactivePeople>();
                var peopleData = new List<People>();
                context.Database.CommandTimeout = 1200;
                empVisitData = context.Database.SqlQuery<EmpVisitDC>("GetEmpVisitData").ToList();
                if (empVisitData != null && empVisitData.Any())
                {
                    empVisitData.ForEach(x =>
                    {
                        int RemainingTimeinHrs = ((x.VisitedOn - DateTime.Now.AddHours(-InActiveCustomerInHrs.InActiveCustomerInHrs)).Hours);
                        if (RemainingTimeinHrs < 0)//168
                        {
                            var inactivePeopleDetail = context.InactivePeopleDB.Where(p => x.PeopleID == x.PeopleID && !p.Active && !p.Deleted).OrderByDescending(p => p.Id).FirstOrDefault();
                            if (inactivePeopleDetail == null)
                            {
                                inactivePeople.Add(new InactivePeople
                                {
                                    Active = false,
                                    PeopleFirstName = x.PeopleFirstName,
                                    PeopleLastName = x.PeopleLastName,
                                    UserName = x.UserName,
                                    PeopleID = x.PeopleID,
                                    Mobile = x.Mobile,
                                    Empcode = x.Empcode,
                                    Department = x.Department,
                                    Desgination = x.Desgination,
                                    DisplayName = x.DisplayName,
                                    city = x.city,
                                    CreatedBy = GetLoginUserId(),
                                    CreationDate = DateTime.Now,
                                    LastVisitedDate = x.VisitedOn
                                });
                                var people = context.Peoples.Where(p => p.PeopleID == x.PeopleID).FirstOrDefault();
                                people.Active = false;
                                people.UpdatedDate = DateTime.Today;
                                context.Entry(people).State = EntityState.Modified;
                            }
                        }
                    });
                }

                //AppVisitData
                List<AppVisitDC> appVisitDCList = new List<AppVisitDC>();
                MongoDbHelper<AppVisits> mongoAppVisitDbHelper = new MongoDbHelper<AppVisits>();
                List<AppVisits> appVisitList = new List<AppVisits>();
                var peopledata = new List<PeopleDC>();
                var searchPredicateDetail = PredicateBuilder.New<AppVisits>(x => x.UserName != null);
                appVisitList = mongoAppVisitDbHelper.Select(searchPredicateDetail).OrderByDescending(x => x.Id).ToList();
                appVisitDCList = Mapper.Map(appVisitList).ToANew<List<AppVisitDC>>();

                if (appVisitDCList.Count > 0)
                {
                    context.Database.CommandTimeout = 1200;
                    peopledata = context.Database.SqlQuery<PeopleDC>("GetPeopleData").ToList();
                    var query = from applist in appVisitDCList
                                join pData in peopleData on applist.UserName equals pData.UserName
                                where ((applist.VisitedOn - DateTime.Now.AddHours(-InActiveCustomerInHrs.InActiveCustomerInHrs)).Hours) < 0
                                select new EmpVisitDC
                                {
                                    VisitedOn = applist.VisitedOn,
                                    UserName = applist.UserName,
                                    city = pData.city,
                                    Department = pData.Department,
                                    Desgination = pData.Desgination,
                                    DisplayName = pData.DisplayName,
                                    Empcode = pData.Empcode,
                                    Mobile = pData.Mobile,
                                    PeopleFirstName = pData.PeopleFirstName,
                                    PeopleID = pData.PeopleID,
                                    PeopleLastName = pData.PeopleLastName
                                };
                    var result = query.ToList();
                    if (result != null && result.Any())
                    {
                        result.ForEach(x =>
                        {
                            var inactivePeopleDetail = context.InactivePeopleDB.Where(p => x.PeopleID == x.PeopleID && !p.Active && !p.Deleted).OrderByDescending(p => p.Id).FirstOrDefault();
                            if (inactivePeopleDetail == null)
                            {
                                inactivePeople.Add(new InactivePeople
                                {
                                    Active = false,
                                    PeopleFirstName = x.PeopleFirstName,
                                    PeopleLastName = x.PeopleLastName,
                                    UserName = x.UserName,
                                    PeopleID = x.PeopleID,
                                    Mobile = x.Mobile,
                                    Empcode = x.Empcode,
                                    Department = x.Department,
                                    Desgination = x.Desgination,
                                    DisplayName = x.DisplayName,
                                    city = x.city,
                                    CreatedBy = GetLoginUserId(),
                                    CreationDate = DateTime.Now,
                                    LastVisitedDate = x.VisitedOn
                                });
                                var people = context.Peoples.Where(p => p.PeopleID == x.PeopleID).FirstOrDefault();
                                people.Active = false;
                                people.UpdatedDate = DateTime.Today;
                                context.Entry(people).State = EntityState.Modified;
                            }
                        });

                    }
                }


                if (inactivePeople != null && inactivePeople.Any())
                {
                    context.InactivePeopleDB.AddRange(inactivePeople);
                    res = context.Commit() > 0;
                }

            }
            return res;
        }
        public async Task<bool> PeoplevisitList()
        {
            bool res = false;
            //List<PageVisitDC> pageVisitDCList = new List<PageVisitDC>();
            //MongoDbHelper<PageVisits> mongoDbHelper = new MongoDbHelper<PageVisits>();
            //List<PageVisits> pageVisitList = new List<PageVisits>();
            //var searchPredicate = PredicateBuilder.New<PageVisits>(x => x.UserName != null);
            //pageVisitList = mongoDbHelper.Select(searchPredicate).OrderByDescending(x => x.Id).ToList();
            //pageVisitDCList = Mapper.Map(pageVisitList).ToANew<List<PageVisitDC>>();

            List<AppVisitDC> appVisitDCList = new List<AppVisitDC>();
            MongoDbHelper<AppVisits> mongoAppVisitDbHelper = new MongoDbHelper<AppVisits>();
            List<AppVisits> appVisitList = new List<AppVisits>();
            var searchPredicateDetail = PredicateBuilder.New<AppVisits>(x => x.UserName != null);
            appVisitList = mongoAppVisitDbHelper.Select(searchPredicateDetail).OrderByDescending(x => x.Id).ToList();
            appVisitDCList = Mapper.Map(appVisitList).ToANew<List<AppVisitDC>>();


            List<PeopleDC> peopledata = new List<PeopleDC>();
            InactivePeople inactivePeopleData = new InactivePeople();
            EmailRecipients emailRecipients = new EmailRecipients();
            using (var myContext = new AuthContext())
            {
                myContext.Database.CommandTimeout = 1200;
                peopledata = myContext.Database.SqlQuery<PeopleDC>("GetPeopleData").ToList();
                var backendVisitData = myContext.Database.SqlQuery<ERPPageVisistData>("PageVisitData").ToList();
                var InActiveCustomerInHrs = myContext.CompanyDetailsDB.Where(x => x.IsActive && x.InActiveCustomerInHrs > 0).OrderByDescending(x => x.Id).FirstOrDefault();
                if (backendVisitData.Count > 0)
                {
                    foreach (var data in peopledata)
                    {
                        foreach (var pagedVisitata in backendVisitData)
                        {
                            //foreach (var pagedVisit in pageVisitDCList)
                            //{
                            if (data.UserName == pagedVisitata.UserName)
                            {
                                pagedVisitata.RemainingTimeinHrs = ((pagedVisitata.VisitedOn - DateTime.Now.AddHours(-InActiveCustomerInHrs.InActiveCustomerInHrs)).Hours);

                                if (pagedVisitata.RemainingTimeinHrs > InActiveCustomerInHrs.InActiveCustomerInHrs || pagedVisitata.RemainingTimeinHrs < 0)//168
                                {
                                    //var VisitedDate = pagedVisitata.VisitedOn.ToString("dd/MM/yyyy");
                                    //var CurrentDate = DateTime.Today.AddHours(-InActiveCustomerInHrs.InActiveCustomerInHrs).ToString("dd/MM/yyyy");
                                    //var we = Convert.ToDateTime(pagedVisitata.VisitedOn);
                                    //var abc = Convert.ToDateTime(DateTime.Now.AddHours(-InActiveCustomerInHrs.InActiveCustomerInHrs));
                                    var people = myContext.Peoples.Where(x => x.PeopleID == data.PeopleID).FirstOrDefault();
                                    var inactivePeopleDetail = myContext.InactivePeopleDB.Where(x => x.PeopleID == data.PeopleID && !x.Active && !x.Deleted).OrderByDescending(x => x.Id).FirstOrDefault();
                                    if (inactivePeopleDetail == null && (Convert.ToDateTime(pagedVisitata.VisitedOn) < Convert.ToDateTime(DateTime.Now.AddHours(-InActiveCustomerInHrs.InActiveCustomerInHrs))))//DateTime.Today.AddDays(-7)
                                    {
                                        inactivePeopleData.Active = false;
                                        inactivePeopleData.PeopleFirstName = data.PeopleFirstName;
                                        inactivePeopleData.PeopleLastName = data.PeopleLastName;
                                        inactivePeopleData.UserName = data.UserName;
                                        inactivePeopleData.PeopleID = data.PeopleID;
                                        inactivePeopleData.Mobile = data.Mobile;
                                        inactivePeopleData.Empcode = data.Empcode;
                                        inactivePeopleData.Department = data.Department;
                                        inactivePeopleData.Desgination = data.Desgination;
                                        inactivePeopleData.DisplayName = data.DisplayName;
                                        inactivePeopleData.city = data.city;
                                        inactivePeopleData.CreatedBy = GetLoginUserId();
                                        inactivePeopleData.CreationDate = DateTime.Now;
                                        inactivePeopleData.LastVisitedDate = pagedVisitata.VisitedOn;
                                        myContext.InactivePeopleDB.Add(inactivePeopleData);

                                        people.Active = false;
                                        people.UpdatedDate = DateTime.Now;
                                        myContext.Entry(people).State = EntityState.Modified;
                                        myContext.Commit();
                                    }
                                }
                            }
                            //}
                        }
                    }
                }
                if (appVisitDCList.Count > 0)
                {
                    foreach (var data in peopledata)
                    {
                        foreach (var appVisitata in appVisitDCList)
                        {
                            if (data.Mobile == appVisitata.UserName)
                            {

                                appVisitata.RemainingTimeinHrs = ((appVisitata.VisitedOn - DateTime.Now.AddHours(-InActiveCustomerInHrs.InActiveCustomerInHrs)).Hours);
                                if (appVisitata.RemainingTimeinHrs > InActiveCustomerInHrs.InActiveCustomerInHrs || appVisitata.RemainingTimeinHrs < 0)//168
                                {
                                    var people = myContext.Peoples.Where(x => x.PeopleID == data.PeopleID).FirstOrDefault();
                                    var inactivePeopleDetail = myContext.InactivePeopleDB.Where(x => x.PeopleID == data.PeopleID && !x.Active && !x.Deleted).OrderByDescending(x => x.Id).FirstOrDefault();
                                    if (inactivePeopleDetail == null && (Convert.ToDateTime(appVisitata.VisitedOn) < Convert.ToDateTime(DateTime.Now.AddHours(-InActiveCustomerInHrs.InActiveCustomerInHrs))))
                                    {
                                        inactivePeopleData.Active = false;
                                        inactivePeopleData.PeopleFirstName = data.PeopleFirstName;
                                        inactivePeopleData.PeopleLastName = data.PeopleLastName;
                                        inactivePeopleData.UserName = data.UserName;
                                        inactivePeopleData.PeopleID = data.PeopleID;
                                        inactivePeopleData.Mobile = data.Mobile;
                                        inactivePeopleData.Empcode = data.Empcode;
                                        inactivePeopleData.Department = data.Department;
                                        inactivePeopleData.Desgination = data.Desgination;
                                        inactivePeopleData.DisplayName = data.DisplayName;
                                        inactivePeopleData.city = data.city;
                                        inactivePeopleData.CreatedBy = GetLoginUserId();
                                        inactivePeopleData.CreationDate = DateTime.Now;
                                        inactivePeopleData.LastVisitedDate = appVisitata.VisitedOn;
                                        myContext.InactivePeopleDB.Add(inactivePeopleData);

                                        people.Active = false;
                                        people.UpdatedDate = DateTime.Now;
                                        myContext.Entry(people).State = EntityState.Modified;
                                        myContext.Commit();
                                    }

                                }
                            }
                        }
                    }
                }

            }
            return res;
        }

        public int GetLoginUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }

        //public async Task<bool> GenerateinActivePeopleReport()
        //{
        //    using (var authContext = new AuthContext())
        //    {
        //        var InActivePeopleReportList = await authContext.Database.SqlQuery<InActivePeopleReportDC>("exec Generate_InActiveReport").ToListAsync(); // old spGenerate_CFRReport
        //        if (InActivePeopleReportList.Any())
        //        {
        //            string ExcelSavePath = HostingEnvironment.MapPath("~/ExcelGeneratePath/");

        //            if (!Directory.Exists(ExcelSavePath))
        //                Directory.CreateDirectory(ExcelSavePath);

        //            var dataTables = new List<DataTable>();
        //            DataTable dt = ClassToDataTable.CreateDataTable(InActivePeopleReportList);
        //            dt.TableName = "Daily_InActivePeople_Report_";
        //            dataTables.Add(dt);

        //            string filePath = ExcelSavePath + "Daily_InActivePeople_Report_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
        //            if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
        //            {
        //                string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
        //                string To = "", From = "", Bcc = "";
        //                DataTable emaildatatable = new DataTable();
        //                using (var connection = new SqlConnection(connectionString))
        //                {
        //                    using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='Daily_InActivePeople_Report_'", connection))
        //                    {

        //                        if (connection.State != ConnectionState.Open)
        //                            connection.Open();

        //                        SqlDataAdapter da = new SqlDataAdapter(command);
        //                        da.Fill(emaildatatable);
        //                        da.Dispose();
        //                        connection.Close();
        //                    }
        //                }
        //                if (emaildatatable.Rows.Count > 0)
        //                {
        //                    To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
        //                    From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
        //                    Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
        //                }
        //                string subject = DateTime.Now.ToString("dd MMM yyyy") + " Daily InActive People Report";
        //                string message = "Please find attach Daily InActive People Report";
        //                if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
        //                    EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
        //                else
        //                    logger.Error("Daily InActive People Report");
        //            }
        //        }
        //    }
        //    return true;
        //}



        public async Task<bool> CreateCustomerRetentionData()
        {
            using (var authContext = new AuthContext())
            {
                authContext.Database.CommandTimeout = 3000;
                var result = await authContext.Database.SqlQuery<bool>("exec GetCustomerRetentionData").FirstOrDefaultAsync();
            }
            return true;
        }

        public async Task<bool> RemoveGamePoint()
        {
            MongoDbHelper<Controllers.CustomerPlayedGame> mongoDbHelper = new MongoDbHelper<Controllers.CustomerPlayedGame>();
            DateTime startDate = DateTime.Now.Date.AddDays(-1);
            DateTime endDate = startDate.AddDays(1).AddMinutes(-1);
            var CustomerPlayedGames = mongoDbHelper.Select(x => !x.IsExpired).ToList();
            if (CustomerPlayedGames != null && CustomerPlayedGames.Any())
            {
                DataTable customerDt = new DataTable();
                customerDt.Columns.Add("CustomerId");
                customerDt.Columns.Add("MinDate", typeof(DateTime));
                customerDt.Columns.Add("Points");
                List<Controllers.CustomerPlayedGame> RemoveExpirtPlayGames = new List<Controllers.CustomerPlayedGame>();
                foreach (var item in CustomerPlayedGames.GroupBy(x => x.CustomerId))
                {

                    if (item.Max(x => x.CreatedDate).AddHours(8) <= DateTime.Now)
                    {
                        DataRow dr = customerDt.NewRow();
                        dr[0] = item.Key;
                        dr[1] = item.Max(x => x.CreatedDate);
                        dr[2] = item.Sum(x => x.Point);
                        customerDt.Rows.Add(dr);
                        RemoveExpirtPlayGames.AddRange(item.ToList());
                    }
                }

                if (customerDt.Rows.Count > 0)
                {
                    using (var authContext = new AuthContext())
                    {
                        var param = new SqlParameter("CustomerGameTypes", customerDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.CustomerGameType";
                        authContext.Database.CommandTimeout = 300;
                        var result = await authContext.Database.ExecuteSqlCommandAsync("exec [RemoveCustomerGamePoint] @CustomerGameTypes", param);
                    }
                }

                if (RemoveExpirtPlayGames.Count > 0)
                {
                    foreach (var item in RemoveExpirtPlayGames)
                    {
                        item.IsExpired = true;
                        item.ModifiedDate = DateTime.Now;
                        mongoDbHelper.Replace(item.Id, item);
                    }
                }
            }
            return true;
        }

        public async Task<bool> AssignUpdateCustomerCompanyTarget()
        {
            TargetModuleController targetModuleController = new TargetModuleController();
            return await targetModuleController.AssignUpdateCustomerCompanyTarget();
        }

        public async Task<bool> InActiveSubCatTarget()
        {
            using (var con = new AuthContext())
            {
                DateTime endDate = DateTime.Now.Date.AddDays(-6).AddMilliseconds(-1);
                var subCatTargets = con.subCatTargets.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.EndDate < endDate)
                    .Include(x => x.subCatTargetBrands)
                    .ToList();
                var subCatTargetBrandIds = subCatTargets.SelectMany(x => x.subCatTargetBrands.Select(y => y.Id)).ToList();
                foreach (var item in subCatTargets)
                {
                    item.IsActive = false;
                    item.ModifiedBy = 1;
                    item.ModifiedDate = DateTime.Now;
                }
                if (con.Commit() > 0)
                {
                    var IdDt = new DataTable();
                    IdDt.Columns.Add("IntValue");
                    foreach (var item in subCatTargetBrandIds)
                    {
                        var dr = IdDt.NewRow();
                        dr["IntValue"] = item;
                        IdDt.Rows.Add(dr);
                    }
                    var param = new SqlParameter("@subCatTargetBrandIds", IdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    var result = con.Database.ExecuteSqlCommand("exec [ExpiredCustomerSubCatTarget] @subCatTargetBrandIds", param);
                }
            }
            return true;
        }

        public async Task<bool> AddPrimeCustomerWalletPoint()
        {
            using (var con = new AuthContext())
            {
                var customerMemberShipBuckets = con.CustomerMemberShipBuckets.Where(x => x.IsActive && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(DateTime.Now)).ToList();
                if (customerMemberShipBuckets != null && customerMemberShipBuckets.Any())
                {
                    var customerIds = customerMemberShipBuckets.Select(x => x.customerId).Distinct().ToList();
                    var Walletcustomers = con.WalletDb.Where(x => customerIds.Contains(x.CustomerId)).ToList();
                    foreach (var customerBucket in customerMemberShipBuckets)
                    {
                        var amount = customerBucket.WalletPoint;

                        var Walletcustomer = Walletcustomers.FirstOrDefault(x => x.CustomerId == customerBucket.customerId);
                        CustomerWalletHistory od = new CustomerWalletHistory();
                        customerBucket.IsActive = false;
                        if (Walletcustomer != null)
                        {
                            var previousWaletPoint = Walletcustomer.TotalAmount;
                            Walletcustomer.TotalAmount += amount;
                            Walletcustomer.UpdatedDate = DateTime.Now;

                            od.CustomerId = Walletcustomer.CustomerId;
                            od.WarehouseId = Walletcustomer.WarehouseId;
                            od.CompanyId = Walletcustomer.CompanyId;
                            od.Through = "Fayda";
                            od.comment = amount + " point added into " + previousWaletPoint + " .Total Points : " + Walletcustomer.TotalAmount;
                            od.TotalWalletAmount = Walletcustomer.TotalAmount;
                            od.NewAddedWAmount = amount;
                            od.UpdatedDate = DateTime.Now;
                            od.TransactionDate = DateTime.Now;
                            od.CreatedDate = DateTime.Now;
                            con.CustomerWalletHistoryDb.Add(od);
                            con.Entry(Walletcustomer).State = EntityState.Modified;
                        }
                        con.Entry(customerBucket).State = EntityState.Modified;

                    }
                    con.Commit();

                }
            }
            return true;
        }

        public bool SendLastDayOrderOtpAccessEmail()
        {

            DateTime now = DateTime.Now.Date;
            var StartDate = new DateTime(now.Year, now.Month, 1);
            // var FirstDayOfNextMonth = StartDate.AddMonths(1).Date;
            //DateTime LastDayOfMonth = FirstDayOfNextMonth.AddDays(-1).Date;
            if (now.Day == 1)
            {
                StartDate = StartDate.AddMonths(-1).Date;
            }

            MongoDbHelper<OrderMasterrController.OrderDeliveryOTPHistory> mongoDbHelper = new MongoDbHelper<OrderMasterrController.OrderDeliveryOTPHistory>();
            var LastDayOTPDeliverdetails = mongoDbHelper.Select(x => x.CreatedDate >= StartDate && x.CreatedDate < now).ToList();

            if (LastDayOTPDeliverdetails != null && LastDayOTPDeliverdetails.Any())
            {
                string ExcelSavePath = HostingEnvironment.MapPath("~/ExcelGeneratePath/");
                var OrderOtp = LastDayOTPDeliverdetails.GroupBy(x => new { x.CreatedByName, x.CreatedDate, x.OrderId, x.OrderStatus, x.OTP }).Select(x => new { AccessBy = x.Key.CreatedByName, Date = x.Key.CreatedDate, OrderNo = x.Key.OrderId, Status = x.Key.OrderStatus, OTP = x.Key.OTP }).ToList();
                if (!Directory.Exists(ExcelSavePath))
                    Directory.CreateDirectory(ExcelSavePath);


                List<FinalOrdeOtpAccess> finalOrderOtp = new List<FinalOrdeOtpAccess>();
                if (OrderOtp.Any())
                {
                    var orderIds = OrderOtp.Select(x => x.OrderNo);

                    using (var con = new AuthContext())
                    {
                        var orderdata = con.DbOrderMaster.Where(x => orderIds.Contains(x.OrderId)).Select(x => new { x.OrderId, x.WarehouseName, x.Status, x.CustomerType, x.Skcode }).ToList();
                        foreach (var item in OrderOtp)
                        {
                            var Dept = con.OrderDeliveryOTP.Where(x => x.OTP == item.OTP && x.OrderId == item.OrderNo && x.Status == item.Status && x.IsUsed == true).OrderByDescending(x => x.Id).Select(y => y.UserType).FirstOrDefault();
                            if (orderdata.Any(x => x.OrderId == item.OrderNo))//&& x.Status != "Delivery Redispatch" && x.Status != "Delivery Canceled"
                            {
                                var order = orderdata.FirstOrDefault(x => x.OrderId == item.OrderNo);
                                finalOrderOtp.Add(new FinalOrdeOtpAccess
                                {
                                    WarehouseName = order.WarehouseName,
                                    AccessBy = item.AccessBy,
                                    Department = Dept != null ? Dept : null,
                                    Date = item.Date,
                                    OrderNo = item.OrderNo,
                                    SKCode = order.Skcode,
                                    CustomerType = order.CustomerType,
                                    OTPStatus = item.Status
                                });
                            }

                        }
                    }
                }


                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(finalOrderOtp);
                dt.TableName = "Daily_Order_OTPAccess";
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "Daily_Order_OTPAccess" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='Daily_Order_OTPAccess'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " Order Delivery OTP access Report";
                    string message = "Please find attach Daily Order Delivery OTP access Report";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                    else
                        logger.Error("To From email emply on Daily Order Delivery OTP access Report");
                }

            }
            return true;
        }

        public bool GenerateCustomerStoreSales()
        {
            using (var con = new AuthContext())
            {
                con.Database.CommandTimeout = 600;
                con.Database.ExecuteSqlCommand("Exec GenerateCustomerStoreSales");
            }
            return true;
        }

        public bool GenerateSellerMonthlyCharges()
        {
            using (var con = new AuthContext())
            {
                con.Database.CommandTimeout = 600;
                con.Database.ExecuteSqlCommand("Exec GenerateSellerMonthlyCharges");
            }
            return true;
        }

        public bool invoiceNoNotgnerated()
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);
            using (var context = new AuthContext())
            {
                context.Database.CommandTimeout = 300;
                var response = context.Database.SqlQuery<int?>("exec  InvoiceNoNotGenerated").ToList();
                if (response != null && response.Any())
                {
                    var dataTables = new List<DataTable>();
                    DataTable dt = ClassToDataTable.CreateDataTable(response);
                    dt.TableName = "InvoiceNoNotGenerated";
                    dataTables.Add(dt);
                    string filePath = ExcelSavePath + "InvoiceNoNotGenerated" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                    if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                    {
                        string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                        string To = "", From = "", Bcc = "";
                        DataTable emaildatatable = new DataTable();
                        using (var connection = new SqlConnection(connectionString))
                        {
                            using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='InvoiceNoNotGenerated'", connection))
                            {

                                if (connection.State != ConnectionState.Open)
                                    connection.Open();

                                SqlDataAdapter da = new SqlDataAdapter(command);
                                da.Fill(emaildatatable);
                                da.Dispose();
                                connection.Close();
                            }
                        }
                        if (emaildatatable.Rows.Count > 0)
                        {
                            To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                            From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                            Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                        }
                        string subject = DateTime.Now.ToString("dd MMM yyyy") + "Report";
                        string message = "Invoice number not generated " + string.Join(",", response);
                        if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                            EmailHelper.SendMail(From, To, Bcc, subject, message, "");
                        else
                            logger.Error("Data Report To and From empty");
                    }
                }
            }
            return true;
        }

        public async Task<bool> InsertRetailerTraceLog(string tracelogname)
        {
            MongoDbHelper<TraceLog> mongoDbHelper = new MongoDbHelper<TraceLog>();
            var TraceLog = mongoDbHelper.mongoDatabase.GetCollection<BsonDocument>(tracelogname);
            var RetailerTraceLog = mongoDbHelper.mongoDatabase.GetCollection<BsonDocument>("RetailerTraceLog");

            var retailerTraceLogs = new List<RetailerTraceLog>();
            RetailerData retailerData = new RetailerData();

            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("LogType", "Request");
            filter = filter & builder.Eq("Referrer", "unknown");
            filter = filter & builder.Regex("RequestInfo", new BsonRegularExpression("RetailerAppVersion")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerCategory")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerItemSearch")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerHomePageGetCategories")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerGetallNotification")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerGetItembycatesscatid")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("GetCustomerCart")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("AddCustomerFavorite")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("GetFavoriteItem")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("GetCustomerWalletPoints")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("GetStoreHome")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerGetItemBySection")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RatailerFlashDealoffer")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerStoreFlashDealoffer")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerGetAllBrand")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerHomePageGetSubSubCategories")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerGetOfferItem")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerGetStoreOfferItem")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerGetItembySubCatAndBrand")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerGetAllItemByBrand")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerCommonDiscountOffer")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerSubCategoryOffer")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerMultiMoqItem")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerWallet")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("GetStoreCategories")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("OrderMastersAPI/V6"));


            //var builder1 = Builders<BsonDocument>.Projection;
            //var projection = builder1.Include(new FilterDefinition<BsonDocument>()).Include(u => u.UserName).Include(u => u.RequestInfo).Include(u=>u.Message);
            var projection = Builders<BsonDocument>.Projection.Include("UserName").Include("RequestInfo").Include("Message").Include("CreatedDate");
            FindOptions<BsonDocument> options = new FindOptions<BsonDocument>
            {
                BatchSize = 2000,
                Projection = projection,
                NoCursorTimeout = false
            };
            using (IAsyncCursor<BsonDocument> cursor = AsyncContext.Run(() => TraceLog.FindAsync(filter, options)))
            {
                while (cursor.MoveNext())
                {
                    IEnumerable<BsonDocument> batch = cursor.Current;
                    var result = new ConcurrentBag<RetailerTraceLog>();
                    ParallelLoopResult loopResult = Parallel.ForEach(batch, (document) =>
                    {
                        var tracedata = BsonSerializer.Deserialize<TraceLog>(document);
                        if (tracedata != null && tracedata.UserName.IndexOf('_') > -1)
                        {
                            var id = Convert.ToInt32(tracedata.UserName.Substring(tracedata.UserName.IndexOf('_') + 1));
                            if (id > 0)
                            {
                                var myObj = formateRetailerTrace(tracedata, retailerData);
                                result.Add(myObj);
                            }
                        }
                    });

                    if (loopResult.IsCompleted)
                        retailerTraceLogs.AddRange(result.ToList());
                }
            }

            if (retailerTraceLogs.Any())
            {
                var customerIds = retailerTraceLogs.Select(x => x.CustomerId).Distinct().ToList();
                using (var context = new AuthContext())
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();
                    var customerIdDt = new DataTable();
                    customerIdDt.Columns.Add("IntValue");
                    foreach (var item in customerIds)
                    {
                        var dr = customerIdDt.NewRow();
                        dr["IntValue"] = item;
                        customerIdDt.Rows.Add(dr);
                    }

                    var cmd = context.Database.Connection.CreateCommand();
                    var param = new SqlParameter("customerIds", customerIdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    cmd.CommandText = "[dbo].[GetRetailerCustomerLogData]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(param);
                    // Run the sproc
                    var reader = cmd.ExecuteReader();
                    var Retailercustomers = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<Retailercustomer>(reader).ToList();
                    reader.NextResult();
                    List<RetailerObject> categories = new List<RetailerObject>();
                    List<RetailerObject> subcategories = new List<RetailerObject>();
                    List<RetailerObject> subSubcategories = new List<RetailerObject>();
                    while (reader.Read())
                    {
                        categories = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<RetailerObject>(reader).ToList();
                    }
                    reader.NextResult();
                    while (reader.Read())
                    {
                        subcategories = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<RetailerObject>(reader).ToList();
                    }

                    reader.NextResult();
                    while (reader.Read())
                    {
                        subSubcategories = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<RetailerObject>(reader).ToList();
                    }

                    retailerData.Retailercustomers = Retailercustomers;
                    retailerData.Categories = categories;
                    retailerData.SubCategories = subcategories;
                    retailerData.SubSubCategories = subSubcategories;
                }
                //List<WriteModel<BsonDocument>> bulkOps = new List<WriteModel<BsonDocument>>();
                foreach (var item in retailerTraceLogs)
                {
                    if (retailerData != null)
                    {
                        if (retailerData.Retailercustomers != null && retailerData.Retailercustomers.Any(x => x.CustomerId == item.CustomerId))
                        {
                            var retailer = retailerData.Retailercustomers.FirstOrDefault(x => x.CustomerId == item.CustomerId);
                            item.Address = retailer.ShippingAddress;
                            item.CityId = retailer.Cityid.HasValue ? retailer.Cityid.Value : 0;
                            item.CityName = retailer.CityName;
                            item.ClusterId = retailer.ClusterId.HasValue ? retailer.ClusterId.Value : 0;
                            item.ClusterName = retailer.ClusterName;
                            item.ShopName = retailer.ShopName;
                            item.SkCode = retailer.Skcode;
                            item.WarehouseId = retailer.Warehouseid.HasValue ? retailer.Warehouseid.Value : 0;
                            item.WarehouseName = retailer.WarehouseName;
                            item.Mobile = retailer.Mobile;
                        }
                        if (retailerData.Categories != null && item.CategoryId > 0 && retailerData.Categories.Any(x => x.Id == item.CategoryId))
                        {
                            var cat = retailerData.Categories.FirstOrDefault(x => x.Id == item.CategoryId);
                            item.CategoryName = cat.Name;
                        }
                        if (retailerData.SubCategories != null && item.SubCategoryId > 0 && retailerData.SubCategories.Any(x => x.Id == item.SubCategoryId))
                        {
                            var subcat = retailerData.SubCategories.FirstOrDefault(x => x.Id == item.SubCategoryId);
                            item.SubCategoryName = subcat.Name;
                        }
                        if (retailerData.SubSubCategories != null && item.SubSubCategoryId > 0 && retailerData.SubSubCategories.Any(x => x.Id == item.SubSubCategoryId))
                        {
                            var brand = retailerData.SubSubCategories.FirstOrDefault(x => x.Id == item.SubSubCategoryId);
                            item.SubSubCategoryName = brand.Name;
                        }
                    }
                    // bulkOps.Add(new InsertOneModel<BsonDocument>(item.ToBsonDocument()));
                }
                //await RetailerTraceLog.BulkWriteAsync(bulkOps, new BulkWriteOptions() { BypassDocumentValidation = true, IsOrdered = false });
                var TargetCustomers = new BulkOperations();
                TargetCustomers.Setup<RetailerTraceLog>(x => x.ForCollection(retailerTraceLogs))
                    .WithTable("RetailerTraceLog")
                    .WithBulkCopyBatchSize(4000)
                    .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                    .WithSqlCommandTimeout(720) // Default is 600 seconds
                    .AddAllColumns()
                    .BulkInsert();
                TargetCustomers.CommitTransaction("AuthContext");
            }
            return true;
        }

        public async Task<bool> InsertTodayRetailerTraceLog(string tracelogname)
        {
            var startdate = new DateTime();
            using (var context = new AuthContext())
            {
                startdate = context.Database.SqlQuery<DateTime>("select max(createdDate) from RetailerTraceLog").FirstOrDefault();
            }
            var today = DateTime.Now;
            // startdate = new DateTime(startdate.Year, startdate.Month, startdate.Day, startdate.Hour, startdate.Minute, startdate.Second, startdate.Millisecond, DateTimeKind.Utc);
            var endDate = new DateTime(today.Year, today.Month, today.Day, today.Hour, today.Minute, 0);
            MongoDbHelper<TraceLog> mongoDbHelper = new MongoDbHelper<TraceLog>();
            var TraceLog = mongoDbHelper.mongoDatabase.GetCollection<BsonDocument>(tracelogname);
            var RetailerTraceLog = mongoDbHelper.mongoDatabase.GetCollection<BsonDocument>("RetailerTraceLog");

            var retailerTraceLogs = new List<RetailerTraceLog>();
            RetailerData retailerData = new RetailerData();

            var builder = Builders<BsonDocument>.Filter;

            //var filter = builder.Eq("_id", ObjectId.Parse("625f91cd65c4c23dec3d53f8"));
            //filter = filter & builder.Gte("CreatedDate", startdate.ToUniversalTime());
            //              //  & builder.Lte("CreatedDate", endDate);
            //var data = (await TraceLog.FindAsync(filter)).ToList();

            // var data = mongoDbHelper.Select(x => x.Id == new ObjectId("625f91cd65c4c23dec3d53f8")).ToList();
            // return true;
            var filter = builder.Eq("LogType", "Request");
            filter = filter & builder.Eq("Referrer", "unknown");
            filter = filter & builder.Gt("CreatedDate", startdate.ToUniversalTime());
            //  & builder.Lte("CreatedDate", endDate.ToUniversalTime());
            filter = filter & builder.Regex("RequestInfo", new BsonRegularExpression("RetailerAppVersion")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerCategory")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerItemSearch")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerHomePageGetCategories")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerGetallNotification")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerGetItembycatesscatid")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("GetCustomerCart")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("AddCustomerFavorite")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("GetFavoriteItem")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("GetCustomerWalletPoints")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("GetStoreHome")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerGetItemBySection")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RatailerFlashDealoffer")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerStoreFlashDealoffer")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerGetAllBrand")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerHomePageGetSubSubCategories")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerGetOfferItem")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerGetStoreOfferItem")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerGetItembySubCatAndBrand")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerGetAllItemByBrand")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerCommonDiscountOffer")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerSubCategoryOffer")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerMultiMoqItem")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("RetailerWallet")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("GetStoreCategories")) |
                  builder.Regex("RequestInfo", new BsonRegularExpression("OrderMastersAPI/V6"));


            //var builder1 = Builders<BsonDocument>.Projection;
            //var projection = builder1.Include(new FilterDefinition<BsonDocument>()).Include(u => u.UserName).Include(u => u.RequestInfo).Include(u=>u.Message);
            var projection = Builders<BsonDocument>.Projection.Include("UserName").Include("RequestInfo").Include("Message").Include("CreatedDate");
            FindOptions<BsonDocument> options = new FindOptions<BsonDocument>
            {
                BatchSize = 2000,
                Projection = projection,
                NoCursorTimeout = false
            };
            using (IAsyncCursor<BsonDocument> cursor = AsyncContext.Run(() => TraceLog.FindAsync(filter, options)))
            {
                while (cursor.MoveNext())
                {
                    IEnumerable<BsonDocument> batch = cursor.Current;
                    var result = new ConcurrentBag<RetailerTraceLog>();
                    ParallelLoopResult loopResult = Parallel.ForEach(batch, (document) =>
                    {
                        var tracedata = BsonSerializer.Deserialize<TraceLog>(document);
                        if (tracedata != null && tracedata.UserName.IndexOf('_') > -1)
                        {
                            Int32 id;
                            var result1 = Int32.TryParse(tracedata.UserName.Substring(tracedata.UserName.IndexOf('_') + 1), out id);
                            //var id = Convert.ToInt32(tracedata.UserName.Substring(tracedata.UserName.IndexOf('_') + 1));
                            if (result1 && id > 0)
                            {
                                var myObj = formateRetailerTrace(tracedata, retailerData);
                                result.Add(myObj);
                            }
                        }
                    });

                    if (loopResult.IsCompleted)
                        retailerTraceLogs.AddRange(result.ToList());
                }
            }

            if (retailerTraceLogs.Any())
            {
                var customerIds = retailerTraceLogs.Select(x => x.CustomerId).Distinct().ToList();
                using (var context = new AuthContext())
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();
                    var customerIdDt = new DataTable();
                    customerIdDt.Columns.Add("IntValue");
                    foreach (var item in customerIds)
                    {
                        var dr = customerIdDt.NewRow();
                        dr["IntValue"] = item;
                        customerIdDt.Rows.Add(dr);
                    }

                    var cmd = context.Database.Connection.CreateCommand();
                    var param = new SqlParameter("customerIds", customerIdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    cmd.CommandText = "[dbo].[GetRetailerCustomerLogData]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(param);
                    // Run the sproc
                    var reader = cmd.ExecuteReader();
                    var Retailercustomers = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<Retailercustomer>(reader).ToList();
                    reader.NextResult();
                    List<RetailerObject> categories = new List<RetailerObject>();
                    List<RetailerObject> subcategories = new List<RetailerObject>();
                    List<RetailerObject> subSubcategories = new List<RetailerObject>();
                    while (reader.Read())
                    {
                        categories = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<RetailerObject>(reader).ToList();
                    }
                    reader.NextResult();
                    while (reader.Read())
                    {
                        subcategories = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<RetailerObject>(reader).ToList();
                    }

                    reader.NextResult();
                    while (reader.Read())
                    {
                        subSubcategories = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<RetailerObject>(reader).ToList();
                    }

                    retailerData.Retailercustomers = Retailercustomers;
                    retailerData.Categories = categories;
                    retailerData.SubCategories = subcategories;
                    retailerData.SubSubCategories = subSubcategories;
                }
                //List<WriteModel<BsonDocument>> bulkOps = new List<WriteModel<BsonDocument>>();
                foreach (var item in retailerTraceLogs)
                {
                    if (retailerData != null)
                    {
                        if (retailerData.Retailercustomers != null && retailerData.Retailercustomers.Any(x => x.CustomerId == item.CustomerId))
                        {
                            var retailer = retailerData.Retailercustomers.FirstOrDefault(x => x.CustomerId == item.CustomerId);
                            item.Address = retailer.ShippingAddress;
                            item.CityId = retailer.Cityid.HasValue ? retailer.Cityid.Value : 0;
                            item.CityName = retailer.CityName;
                            item.ClusterId = retailer.ClusterId.HasValue ? retailer.ClusterId.Value : 0;
                            item.ClusterName = retailer.ClusterName;
                            item.ShopName = retailer.ShopName;
                            item.SkCode = retailer.Skcode;
                            item.WarehouseId = retailer.Warehouseid.HasValue ? retailer.Warehouseid.Value : 0;
                            item.WarehouseName = retailer.WarehouseName;
                            item.Mobile = retailer.Mobile;
                        }
                        if (retailerData.Categories != null && item.CategoryId > 0 && retailerData.Categories.Any(x => x.Id == item.CategoryId))
                        {
                            var cat = retailerData.Categories.FirstOrDefault(x => x.Id == item.CategoryId);
                            item.CategoryName = cat.Name;
                        }
                        if (retailerData.SubCategories != null && item.SubCategoryId > 0 && retailerData.SubCategories.Any(x => x.Id == item.SubCategoryId))
                        {
                            var subcat = retailerData.SubCategories.FirstOrDefault(x => x.Id == item.SubCategoryId);
                            item.SubCategoryName = subcat.Name;
                        }
                        if (retailerData.SubSubCategories != null && item.SubSubCategoryId > 0 && retailerData.SubSubCategories.Any(x => x.Id == item.SubSubCategoryId))
                        {
                            var brand = retailerData.SubSubCategories.FirstOrDefault(x => x.Id == item.SubSubCategoryId);
                            item.SubSubCategoryName = brand.Name;
                        }
                    }
                    // bulkOps.Add(new InsertOneModel<BsonDocument>(item.ToBsonDocument()));
                }
                //await RetailerTraceLog.BulkWriteAsync(bulkOps, new BulkWriteOptions() { BypassDocumentValidation = true, IsOrdered = false });
                var TargetCustomers = new BulkOperations();
                TargetCustomers.Setup<RetailerTraceLog>(x => x.ForCollection(retailerTraceLogs))
                    .WithTable("RetailerTraceLog")
                    .WithBulkCopyBatchSize(4000)
                    .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                    .WithSqlCommandTimeout(720) // Default is 600 seconds
                    .AddAllColumns()
                    .BulkInsert();
                TargetCustomers.CommitTransaction("AuthContext");
            }
            return true;
        }

        private RetailerTraceLog formateRetailerTrace(TraceLog traceLog, RetailerData retailerData)
        {
            RetailerTraceLog retailerTraceLog = new RetailerTraceLog();
            Uri uri = new Uri(traceLog.RequestInfo);
            List<string> QueryString = !string.IsNullOrEmpty(uri.Query) ? uri.Query.Replace("?", "").Split('&').ToList() : null;
            List<KeyValuePair<string, string>> paramitems = new List<KeyValuePair<string, string>>();

            if (QueryString != null)
            {
                foreach (var item in QueryString)
                {
                    if (item.Split('=').Length == 2)
                        paramitems.Add(new KeyValuePair<string, string>(item.Split('=')[0], item.Split('=')[1]));
                }
            }
            if (traceLog.UserName.IndexOf('_') > -1)
                retailerTraceLog.CustomerId = Convert.ToInt32(traceLog.UserName.Substring(traceLog.UserName.IndexOf('_') + 1));


            if (traceLog.RequestInfo.IndexOf("RetailerAppVersion") > -1)
            {
                retailerTraceLog.PageType = "Splashscreen";
            }
            else if (traceLog.RequestInfo.IndexOf("RetailerCategory") > -1)
            {
                retailerTraceLog.PageType = "CategoryPage";
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "warehouseid"))
                    retailerTraceLog.WarehouseId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "warehouseid").Value);
            }
            else if (traceLog.RequestInfo.IndexOf("GetStoreCategories") > -1)
            {
                retailerTraceLog.PageType = "StoreCategoryHomePage";
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "wid"))
                    retailerTraceLog.WarehouseId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "wid").Value);
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "subCategoryId"))
                    retailerTraceLog.SubCategoryId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "subCategoryId").Value);
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "baseCategoryId"))
                    retailerTraceLog.BaseCategoryId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "baseCategoryId").Value);
            }

            else if (traceLog.RequestInfo.IndexOf("RetailerItemSearch") > -1)
            {
                retailerTraceLog.PageType = "Searchbutton";
                SearchItem searchItem = null;
                try
                {
                    searchItem = JsonConvert.DeserializeObject<SearchItem>(traceLog.Message);
                }
                catch (Exception ex)
                {
                }
                if (searchItem != null)
                    retailerTraceLog.Desc = searchItem.itemkeyword;
            }
            else if (traceLog.RequestInfo.IndexOf("RetailerHomePageGetCategories") > -1)
            {
                retailerTraceLog.PageType = "CategoryHomePage";
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "wid"))
                    retailerTraceLog.WarehouseId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "wid").Value);
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "itemid"))
                    retailerTraceLog.BaseCategoryId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "itemid").Value);
            }
            else if (traceLog.RequestInfo.IndexOf("RetailerGetallNotification") > -1)
            {
                retailerTraceLog.PageType = "NotificationPage";
            }
            else if (traceLog.RequestInfo.IndexOf("RetailerGetItembycatesscatid") > -1)
            {
                retailerTraceLog.PageType = "SubSubCategoryPage";
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "catid"))
                    retailerTraceLog.CategoryId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "catid").Value);
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "scatid"))
                    retailerTraceLog.SubCategoryId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "scatid").Value);
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "sscatid"))
                    retailerTraceLog.SubSubCategoryId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "sscatid").Value);
            }
            else if (traceLog.RequestInfo.IndexOf("GetCustomerCart") > -1)
            {
                retailerTraceLog.PageType = "ShoppingCartPage";
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "warehouseid"))
                    retailerTraceLog.WarehouseId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "warehouseid").Value);
            }
            else if (traceLog.RequestInfo.IndexOf("GetPublishedSection") > -1)
            {
                retailerTraceLog.PageType = "HomePage";
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "wid"))
                    retailerTraceLog.WarehouseId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "wid").Value);
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "lang"))
                    retailerTraceLog.Desc = paramitems.FirstOrDefault(x => x.Key.ToLower() == "lang").Value;
            }
            else if (traceLog.RequestInfo.IndexOf("RetailerGetItembySubCatAndBrand") > -1)
            {
                retailerTraceLog.PageType = "BrandPage";
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "scatid"))
                    retailerTraceLog.SubCategoryId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "scatid").Value);
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "sscatid"))
                    retailerTraceLog.SubSubCategoryId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "sscatid").Value);
            }
            else if (traceLog.RequestInfo.IndexOf("AddCustomerFavorite") > -1)
            {
                retailerTraceLog.PageType = "AddFavorite";
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "itemid"))
                    retailerTraceLog.ItemId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "itemid").Value);
            }
            else if (traceLog.RequestInfo.IndexOf("GetFavoriteItem") > -1)
            {
                retailerTraceLog.PageType = "ShowFavorite";
            }
            else if (traceLog.RequestInfo.IndexOf("GetCustomerWalletPoints") > -1)
            {
                retailerTraceLog.PageType = "CheckWalletPoint";
            }
            else if (traceLog.RequestInfo.IndexOf("GetStoreHome") > -1)
            {
                retailerTraceLog.PageType = "StoreHome";
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "subcategoryid"))
                    retailerTraceLog.SubCategoryId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "subcategoryid").Value);
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "wid"))
                    retailerTraceLog.WarehouseId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "wid").Value);

            }
            else if (traceLog.RequestInfo.IndexOf("RetailerGetItemBySection") > -1)
            {
                retailerTraceLog.PageType = "PublishSection";
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "sectionid"))
                    retailerTraceLog.objectId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "sectionid").Value);
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "warehouseid"))
                    retailerTraceLog.WarehouseId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "warehouseid").Value);

            }
            else if (traceLog.RequestInfo.IndexOf("RatailerFlashDealoffer") > -1)
            {
                retailerTraceLog.PageType = "FlashDealLoadMore";
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "sectionid"))
                    retailerTraceLog.objectId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "sectionid").Value);
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "warehouseid"))
                    retailerTraceLog.WarehouseId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "warehouseid").Value);

            }
            else if (traceLog.RequestInfo.IndexOf("RetailerStoreFlashDealoffer") > -1)
            {
                retailerTraceLog.PageType = "StoreFlashDealLoadMore";
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "sectionid"))
                    retailerTraceLog.objectId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "sectionid").Value);
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "warehouseid"))
                    retailerTraceLog.WarehouseId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "warehouseid").Value);
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "subcategoryid"))
                    retailerTraceLog.SubCategoryId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "subcategoryid").Value);

            }
            else if (traceLog.RequestInfo.IndexOf("RetailerGetAllBrand") > -1)
            {
                retailerTraceLog.PageType = "AllBrand";
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "brandname"))
                    retailerTraceLog.Desc = paramitems.FirstOrDefault(x => x.Key.ToLower() == "brandname").Value;
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "warehouseid"))
                    retailerTraceLog.WarehouseId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "warehouseid").Value);
            }
            else if (traceLog.RequestInfo.IndexOf("RetailerHomePageGetSubSubCategories") > -1)
            {
                retailerTraceLog.PageType = "StoreHomeDefault";
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "wid"))
                    retailerTraceLog.WarehouseId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "wid").Value);
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "subcategoryid"))
                    retailerTraceLog.SubCategoryId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "subcategoryid").Value);

            }
            else if (traceLog.RequestInfo.IndexOf("RetailerGetOfferItem") > -1)
            {
                retailerTraceLog.PageType = "Freebies";
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "warehouseid"))
                    retailerTraceLog.WarehouseId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "warehouseid").Value);

            }
            else if (traceLog.RequestInfo.IndexOf("RetailerGetStoreOfferItem") > -1)
            {
                retailerTraceLog.PageType = "StoreFreebies";
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "warehouseid"))
                    retailerTraceLog.WarehouseId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "warehouseid").Value);
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "subcategoryid"))
                    retailerTraceLog.SubCategoryId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "subcategoryid").Value);

            }

            else if (traceLog.RequestInfo.IndexOf("RetailerGetAllItemByBrand") > -1)
            {
                retailerTraceLog.PageType = "BrandItem";
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "warehouseid"))
                    retailerTraceLog.WarehouseId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "warehouseid").Value);
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "brandname"))
                    retailerTraceLog.SubSubCategoryName = paramitems.FirstOrDefault(x => x.Key.ToLower() == "brandname").Value;
            }
            else if (traceLog.RequestInfo.IndexOf("RetailerCommonDiscountOffer") > -1)
            {
                retailerTraceLog.PageType = "OfferPage";
            }
            else if (traceLog.RequestInfo.IndexOf("RetailerSubCategoryOffer") > -1)
            {
                retailerTraceLog.PageType = "CompanyOfferPage";
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "subcategoryid"))
                    retailerTraceLog.SubCategoryId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "subcategoryid").Value);
            }
            else if (traceLog.RequestInfo.IndexOf("RetailerMultiMoqItem") > -1)
            {
                retailerTraceLog.PageType = "TradeOfferMOQPage";
                if (paramitems.Count > 0 && paramitems.Any(x => x.Key.ToLower() == "warehouseid"))
                    retailerTraceLog.WarehouseId = Convert.ToInt32(paramitems.FirstOrDefault(x => x.Key.ToLower() == "warehouseid").Value);
            }
            else if (traceLog.RequestInfo.IndexOf("RetailerWallet") > -1)
            {
                retailerTraceLog.PageType = "WalletPage";
            }
            else if (traceLog.RequestInfo.IndexOf("OrderMastersAPI/V6") > -1)
            {
                ShoppingCart shoppingCart = null;
                retailerTraceLog.PageType = "PlaceOrder";
                try
                {
                    shoppingCart = JsonConvert.DeserializeObject<ShoppingCart>(traceLog.Message);
                }
                catch (Exception ex)
                {
                }
                if (shoppingCart != null)
                {
                    if (shoppingCart.APPType == "Retailer")
                        retailerTraceLog.PageType = "RetailerPlaceOrder";
                    else if (shoppingCart.APPType == "SalesApp")
                        retailerTraceLog.PageType = "SalesMainPlaceOrder";
                    retailerTraceLog.CustomerId = shoppingCart.CustomerId.HasValue ? shoppingCart.CustomerId.Value : 0;
                    retailerTraceLog.Desc = shoppingCart.paymentThrough;
                }
            }
            retailerTraceLog.UserName = traceLog.UserName;
            retailerTraceLog.RequestInfo = traceLog.RequestInfo;
            retailerTraceLog.CreatedDate = traceLog.CreatedDate.ToLocalTime();
            //retailerTraceLog.Desc = traceLog.Id.ToString();
            return retailerTraceLog;
        }


        public async Task<bool> GenerateItemSalesInventoryData()
        {
            using (var authContext = new AuthContext())
            {
                authContext.Database.CommandTimeout = 3000;
                var ItemSalesInventoryData = await authContext.Database.SqlQuery<ItemSalesInventoryData>("exec GetItemSalesInventory").ToListAsync(); // old spGenerate_CFRReport
                if (ItemSalesInventoryData.Any())
                {
                    string ExcelSavePath = HostingEnvironment.MapPath("~/ExcelGeneratePath/");

                    if (!Directory.Exists(ExcelSavePath))
                        Directory.CreateDirectory(ExcelSavePath);

                    var dataTables = new List<DataTable>();
                    DataTable dt = ClassToDataTable.CreateDataTable(ItemSalesInventoryData);
                    dt.TableName = "Daily_ItemSalesInventory_Report";
                    dataTables.Add(dt);

                    string filePath = ExcelSavePath + "Daily_ItemSI_Report_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                    if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                    {
                        string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                        string To = "", From = "", Bcc = "";
                        DataTable emaildatatable = new DataTable();
                        using (var connection = new SqlConnection(connectionString))
                        {
                            using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='Daily_ItemSalesInventory_Report'", connection))
                            {

                                if (connection.State != ConnectionState.Open)
                                    connection.Open();

                                SqlDataAdapter da = new SqlDataAdapter(command);
                                da.Fill(emaildatatable);
                                da.Dispose();
                                connection.Close();
                            }
                        }
                        if (emaildatatable.Rows.Count > 0)
                        {
                            To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                            From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                            Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                        }
                        string subject = DateTime.Now.ToString("dd MMM yyyy") + " Daily Item Sale Inventory Report";
                        string message = "Please find attach Daily Item Sale Inventory Report";
                        if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                            EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                        else
                            logger.Error("Daily Item Sale Inventory Report");
                    }
                }
            }
            return true;
        }
        public async Task<bool> GetGullakInPayments()
        {
            using (var authContext = new AuthContext())
            {
                authContext.Database.CommandTimeout = 3000;
                var getGullakInPayments = await authContext.Database.SqlQuery<GetGullakInPaymentsDc>("exec GetGullakInPayments").ToListAsync(); // old spGenerate_CFRReport
                if (getGullakInPayments.Any())
                {
                    string ExcelSavePath = HostingEnvironment.MapPath("~/ExcelGeneratePath/");

                    if (!Directory.Exists(ExcelSavePath))
                        Directory.CreateDirectory(ExcelSavePath);

                    var dataTables = new List<DataTable>();
                    DataTable dt = ClassToDataTable.CreateDataTable(getGullakInPayments);
                    dt.TableName = "Daily_Gullak_Payments_Report";
                    dataTables.Add(dt);

                    string filePath = ExcelSavePath + "Daily_Gullak_Payments_Report_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                    if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                    {
                        string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                        string To = "", From = "", Bcc = "";
                        DataTable emaildatatable = new DataTable();
                        using (var connection = new SqlConnection(connectionString))
                        {
                            using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='Gullak_Payments_Report'", connection))
                            {

                                if (connection.State != ConnectionState.Open)
                                    connection.Open();

                                SqlDataAdapter da = new SqlDataAdapter(command);
                                da.Fill(emaildatatable);
                                da.Dispose();
                                connection.Close();
                            }
                        }
                        if (emaildatatable.Rows.Count > 0)
                        {
                            To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                            From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                            Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                        }
                        string subject = DateTime.Now.ToString("dd MMM yyyy") + " Daily Gullak Payments Report";
                        string message = "Please find attach Daily Gullak Payments Report";
                        if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                            EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                        else
                            logger.Error("Daily Gullak Payments Report");
                    }
                }
            }
            return true;
        }


        public bool GetAccountMonthEndData()
        {
            TextFileLogHelper.LogError("Error AccountMonthEndData Start");
            var result = false;
            var firstDayOfMonth = DateTime.Now.Day;
            TextFileLogHelper.LogError("Error AccountMonthEndData firstDayOfMonth" + firstDayOfMonth + "");
            if (firstDayOfMonth == 1)
            {
                var Currentmonth = DateTime.Now.AddMonths(-1);
                var month = Currentmonth.Month;
                var year = Currentmonth.Year;
                //string ExcelSavePath = HttpContext.Current.Server.MapPath("~/MonthEndData/" + DateTime.Now.ToString("MMM-yyyy"));
                string ExcelSavePath = @"D:\ProdShopkirana\InternalEr15\MonthEndData\" + DateTime.Now.ToString("MMM-yyyy");
                TextFileLogHelper.LogError("Error AccountMonthEndData ExcelSavePath -----   " + ExcelSavePath + "");
                if (!Directory.Exists(ExcelSavePath))
                    Directory.CreateDirectory(ExcelSavePath);
                //file delete code..
                string[] files = Directory.GetFiles(ExcelSavePath);
                TextFileLogHelper.LogError("Error AccountMonthEndData GetFiles start");
                foreach (string file in files)
                {
                    File.Delete(file);
                }
                TextFileLogHelper.LogError("Error AccountMonthEndData GetFiles end");
                //end
                DateTime start = DateTime.Parse("2019-04-01 00:00:00");
                DateTime end = DateTime.Today.AddDays(1);
                var previousEndDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");
                List<WCWarehouse> Wlist = new List<WCWarehouse>();
                SqlParameter Warehouseids = new SqlParameter();
                List<KeyValuePair<string, string>> AccountData = new List<KeyValuePair<string, string>>();

                AccountData.Add(new KeyValuePair<string, string>("SalesRegister", "MonthEnd.GetSalesRegister"));
                AccountData.Add(new KeyValuePair<string, string>("SalesReturnRegister", "MonthEnd.GetReturnSalesRegister"));
                AccountData.Add(new KeyValuePair<string, string>("SalesRegisterExcludingSettledAndPOC", "MonthEnd.GetSalesRegisterExcludingSettled_POC"));
                AccountData.Add(new KeyValuePair<string, string>("Customerledgersummary", "MonthEnd.GetCustomerledgersummary"));
                AccountData.Add(new KeyValuePair<string, string>("SupplierledgerSummary", "[dbo].GetSupplierSummary"));
                AccountData.Add(new KeyValuePair<string, string>("GRIRDifference", "[dbo].GrButNoIR"));
                AccountData.Add(new KeyValuePair<string, string>("PendingFromBuyerSide", "[dbo].SPBuyerData"));
                AccountData.Add(new KeyValuePair<string, string>("CMSDump", "MonthEnd.GetCMSDump"));
                AccountData.Add(new KeyValuePair<string, string>("DamageStock", "MonthEnd.GetDamageStock"));
                AccountData.Add(new KeyValuePair<string, string>("FreeStock", "MonthEnd.GetFreeStock"));
                AccountData.Add(new KeyValuePair<string, string>("CurrentStock", "MonthEnd.GetCurrentStock"));
                AccountData.Add(new KeyValuePair<string, string>("InTransitInventory", "MonthEnd.GetInTransitInventory"));
                AccountData.Add(new KeyValuePair<string, string>("UnutilisedWalletDiscountPoints", "MonthEnd.GetUnutilisedWalletPoint"));
                AccountData.Add(new KeyValuePair<string, string>("InventoryAging", "MonthEnd.GetInventoryAging"));
                AccountData.Add(new KeyValuePair<string, string>("TDSReportAdvancePayment", "MonthEnd.GetTDSAdvancePayment"));
                AccountData.Add(new KeyValuePair<string, string>("TDSReportBillToBill", "MonthEnd.GetTDSBillToBill"));
                AccountData.Add(new KeyValuePair<string, string>("TCSReport", "MonthEnd.GetTCSOrderReport"));
                AccountData.Add(new KeyValuePair<string, string>("FreebiesData", "MonthEnd.GetFreebiesData"));
                AccountData.Add(new KeyValuePair<string, string>("OrderBillDiscountData", "MonthEnd.GetOrderWiseOfferData"));
                AccountData.Add(new KeyValuePair<string, string>("WarehouseInTransitStock", "MonthEnd.GetWarehouseInTransitStock"));
                AccountData.Add(new KeyValuePair<string, string>("InternalTransfers", "MonthEnd.InternalTransfer"));
                AccountData.Add(new KeyValuePair<string, string>("OrderDeliveryCharges", "MonthEnd.OrderDeliveryCharges"));
                AccountData.Add(new KeyValuePair<string, string>("DirectUdharReport", "MonthEnd.DirectUdharReport"));
                //AccountData.Add(new KeyValuePair<string, string>("CurrentNetStock", "MonthEnd.CurrentNetStock"));
                AccountData.Add(new KeyValuePair<string, string>("ClearanceStockData", "MonthEnd.ClearanceStockData"));
                AccountData.Add(new KeyValuePair<string, string>("NonSaleableStock", "MonthEnd.NonSaleableStock"));
                AccountData.Add(new KeyValuePair<string, string>("RedispatchedWalletPointData", "MonthEnd.RedispatchedWalletPointData"));
                AccountData.Add(new KeyValuePair<string, string>("PurchaseRegistorData", "[dbo].GetPurchaseRegistorData"));
                AccountData.Add(new KeyValuePair<string, string>("NonRevenueStock", "MonthEnd.NonRevenueStocks"));
                AccountData.Add(new KeyValuePair<string, string>("BatchcodeWiseCurrentStock", "MonthEnd.GetBatchcodeWiseCurrentStock"));


                TextFileLogHelper.LogError("Error AccountMonthEndData AccountData");
                TextFileLogHelper.LogError("Error AccountMonthEndData ParallelLoop Start");
                ParallelLoopResult parellelResult = Parallel.ForEach(AccountData, (x) =>
                {
                    try
                    {
                        var dataTables = new List<DataTable>();
                        using (var context = new AuthContext())
                        {
                            var Months = new SqlParameter("month", month);
                            var Year = new SqlParameter("year", year);
                            var StartDate = new SqlParameter("StartDate", start);
                            var EndDate = new SqlParameter("EndDate", end);
                            var monthEndDate = new SqlParameter("monthEndDate", previousEndDate);
                            var StartDatePR = new SqlParameter("startDate", Currentmonth);
                            var EndDatePR = new SqlParameter("endDate", end);
                            if (x.Key == "PurchaseRegistorData")
                            {
                                WarehouseController warehouseController = new WarehouseController();
                                TextFileLogHelper.LogError("Error AccountMonthEndData warehouse start");
                                Wlist = warehouseController.WhForWarkingCapital();
                                TextFileLogHelper.LogError("Error AccountMonthEndData warehouse end");
                                if (Wlist.Any() && Wlist != null)
                                {
                                    var WareIdDtw = new DataTable();
                                    WareIdDtw.Columns.Add("Intvalue");
                                    foreach (var item in Wlist.Select(y => y.WarehouseId))
                                    {
                                        var dr = WareIdDtw.NewRow();
                                        dr["IntValue"] = item;
                                        WareIdDtw.Rows.Add(dr);
                                    }

                                    Warehouseids = new SqlParameter
                                    {
                                        ParameterName = "warehouse",
                                        SqlDbType = SqlDbType.Structured,
                                        TypeName = "dbo.IntValues",
                                        Value = WareIdDtw
                                    };
                                }
                            }
                            using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                            {
                                using (var cmd2 = new SqlCommand())
                                {
                                    if (connection.State != ConnectionState.Open)
                                        connection.Open();

                                    cmd2.Connection = connection;
                                    cmd2.CommandText = x.Value;
                                    if (x.Key == "SalesRegister"
                                     || x.Key == "SalesReturnRegister"
                                     || x.Key == "CMSDump"
                                     || x.Key == "TDSReportAdvancePayment"
                                     || x.Key == "TDSReportBillToBill"
                                     || x.Key == "TCSReport"
                                     || x.Key == "FreebiesData"
                                     || x.Key == "OrderBillDiscountData"
                                     || x.Key == "InventoryAging"
                                     || x.Key == "InternalTransfers"
                                     || x.Key == "OrderDeliveryCharges"
                                     || x.Key == "DirectUdharReport"
                                     || x.Key == "ClearanceStockData"
                                     || x.Key == "NonSaleableStock"
                                     || x.Key == "RedispatchedWalletPointData"
                                     || x.Key == "DamageStock"
                                     || x.Key == "NonRevenueStock"
                                     )
                                    {
                                        cmd2.Parameters.Add(Months);
                                        cmd2.Parameters.Add(Year);
                                    }
                                    else if (x.Key == "SalesRegisterExcludingSettledAndPOC" || x.Key == "GRIRDifference")
                                    {
                                        cmd2.Parameters.Add(StartDate);
                                        cmd2.Parameters.Add(EndDate);
                                    }
                                    else if (x.Key == "InTransitInventory")
                                    {
                                        cmd2.Parameters.Add(EndDate);
                                    }
                                    else if (x.Key == "WarehouseInTransitStock")
                                    {
                                        cmd2.Parameters.Add(monthEndDate);
                                    }
                                    else if (x.Key == "PurchaseRegistorData")
                                    {
                                        // @startDate,@endDate,@warehouse
                                        cmd2.Parameters.Add(StartDatePR);
                                        cmd2.Parameters.Add(EndDatePR);
                                        cmd2.Parameters.Add(Warehouseids);
                                    }
                                    cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                                    cmd2.CommandTimeout = 1200;
                                    DataTable dt = new DataTable();
                                    using (SqlDataAdapter da = new SqlDataAdapter(cmd2))
                                    {
                                        da.Fill(dt);
                                    }
                                    dt.TableName = x.Key;
                                    dataTables.Add(dt);
                                    string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                                    ExcelGenerator.DataTable_To_Excel(dataTables, filePath);

                                }
                            }
                        }
                        #region old code --> 01-09-2023
                        //if (x.Key == "SalesRegister")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        var Months = new SqlParameter("month", month);
                        //        var Year = new SqlParameter("year", year);

                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            //var cmd2 = context.Database.Connection.CreateCommand();
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.Parameters.Add(Months);
                        //                cmd2.Parameters.Add(Year);
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                //List<GetSalesRegisterDc> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<GetSalesRegisterDc>(reader2).ToList();

                        //                // DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "SalesReturnRegister")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            //var cmd2 = context.Database.Connection.CreateCommand();
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                var Months = new SqlParameter("month", month);
                        //                var Year = new SqlParameter("year", year);
                        //                //*****************************************************************
                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();

                        //                cmd2.CommandText = x.Value;
                        //                cmd2.Parameters.Add(Months);
                        //                cmd2.Parameters.Add(Year);
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<GetSalesRegisterDc> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<GetSalesRegisterDc>(reader2).ToList();
                        //                ////**********************************************************************
                        //                //// List<GetSalesRegisterDc> reportdata = context.Database.SqlQuery<GetSalesRegisterDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();

                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "SalesRegisterExcludingSettledAndPOC")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            //var cmd2 = context.Database.Connection.CreateCommand();
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                var StartDate = new SqlParameter("StartDate", start);
                        //                var EndDate = new SqlParameter("EndDate", end);
                        //                //*************************************************************
                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.Parameters.Add(StartDate);
                        //                cmd2.Parameters.Add(EndDate);
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<GetSalesRegisterDc> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<GetSalesRegisterDc>(reader2).ToList();
                        //                //****************************************************************
                        //                //List<GetSalesRegisterDc> reportdata = context.Database.SqlQuery<GetSalesRegisterDc>("Exec " + x.Value + " @StartDate,@EndDate", StartDate, EndDate).ToList();

                        //                // DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "Customerledgersummary")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            //var cmd2 = context.Database.Connection.CreateCommand();
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                //**************************************************************
                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                //cmd2.Parameters.Add(Months);
                        //                //cmd2.Parameters.Add(Year);
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<CustomerledgersummaryDc> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<CustomerledgersummaryDc>(reader2).ToList();
                        //                //*****************************************************************
                        //                //List<CustomerledgersummaryDc> reportdata = context.Database.SqlQuery<CustomerledgersummaryDc>("Exec " + x.Value).ToList();
                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "SupplierledgerSummary")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            //var cmd2 = context.Database.Connection.CreateCommand();
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = "[dbo]." + x.Value;
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<SupplierSummaryDTO> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<SupplierSummaryDTO>(reader2).ToList();
                        //                ////***************************************************
                        //                //// List<SupplierSummaryDTO> reportdata = context.Database.SqlQuery<SupplierSummaryDTO>("Exec " + x.Value).ToList();
                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "GRIRDifference")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            //var cmd2 = context.Database.Connection.CreateCommand();
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                var StartDate = new SqlParameter("StartDate", start);
                        //                var EndDate = new SqlParameter("EndDate", end);
                        //                //**********************************************************
                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = "[dbo]." + x.Value;
                        //                cmd2.Parameters.Add(StartDate);
                        //                cmd2.Parameters.Add(EndDate);
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<GrButNoIrReportDc> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<GrButNoIrReportDc>(reader2).ToList();
                        //                ////**********************************************************
                        //                ////  List<GrButNoIrReportDc> reportdata = context.Database.SqlQuery<GrButNoIrReportDc>("Exec " + x.Value + " @StartDate,@EndDate", StartDate, EndDate).ToList();
                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "PendingFromBuyerSide")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            //var cmd2 = context.Database.Connection.CreateCommand();
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                //{
                        //                //    //**************************************************
                        //                //    if (context.Database.Connection.State != ConnectionState.Open)
                        //                //        context.Database.Connection.Open();
                        //                //    var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = "[dbo]." + x.Value;
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<IRMasters> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<IRMasters>(reader2).ToList();
                        //                ////************************************************
                        //                //// List<IRMasters> reportdata = context.Database.SqlQuery<IRMasters>("Exec " + x.Value).ToList();
                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "CMSDump")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                var Months = new SqlParameter("month", month);
                        //                var Year = new SqlParameter("year", year);
                        //                //*******************************************************
                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.Parameters.Add(Months);
                        //                cmd2.Parameters.Add(Year);
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<CMSDumpDc> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<CMSDumpDc>(reader2).ToList();

                        //                ////******************************************************
                        //                //// List<CMSDumpDc> reportdata = context.Database.SqlQuery<CMSDumpDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();
                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "DamageStock")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                //***************************************************
                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<MonthDamageStockDc> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<MonthDamageStockDc>(reader2).ToList();
                        //                ////****************************************************
                        //                ////List<MonthDamageStockDc> reportdata = context.Database.SqlQuery<MonthDamageStockDc>("Exec " + x.Value).ToList();
                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "FreeStock")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;

                        //                //***********************************************
                        //                //        if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                var reader2 = cmd2.ExecuteReader();
                        //                //List<FreeStockDc> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<FreeStockDc>(reader2).ToList();
                        //                ////***********************************************
                        //                //// List<FreeStockDc> reportdata = context.Database.SqlQuery<FreeStockDc>("Exec " + x.Value).ToList();
                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "CurrentStock")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                ////*****************************************************************
                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<CurrentStockDc> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<CurrentStockDc>(reader2).ToList();
                        //                ////******************************************************************
                        //                ////List<CurrentStockDc> reportdata = context.Database.SqlQuery<CurrentStockDc>("Exec " + x.Value).ToList();
                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "InTransitInventory")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                var EndDate = new SqlParameter("EndDate", end);
                        //                //*****************************************************************
                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.Parameters.Add(EndDate);
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<InTransitInventoryDc> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<InTransitInventoryDc>(reader2).ToList();
                        //                ////******************************************************************
                        //                //// List<InTransitInventoryDc> reportdata = context.Database.SqlQuery<InTransitInventoryDc>("Exec " + x.Value + " @EndDate", EndDate).ToList();
                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "UnutilisedWalletDiscountPoints")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                //*****************************************************************
                        //                //    if (context.Database.Connection.State != ConnectionState.Open)
                        //                //context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<UnutilisedWalletPointDc> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<UnutilisedWalletPointDc>(reader2).ToList();
                        //                ////******************************************************************
                        //                ////List<UnutilisedWalletPointDc> reportdata = context.Database.SqlQuery<UnutilisedWalletPointDc>("Exec " + x.Value).ToList();
                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "TDSReportAdvancePayment")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;

                        //                var Months = new SqlParameter("month", month);
                        //                var Year = new SqlParameter("year", year);
                        //                //*****************************************************************
                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.Parameters.Add(Months);
                        //                cmd2.Parameters.Add(Year);
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<TDSAdvancePaymentDc> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<TDSAdvancePaymentDc>(reader2).ToList();
                        //                ////******************************************************************
                        //                ////List<TDSAdvancePaymentDc> reportdata = context.Database.SqlQuery<TDSAdvancePaymentDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();
                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "TDSReportBillToBill")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                var Months = new SqlParameter("month", month);
                        //                var Year = new SqlParameter("year", year);
                        //                //*****************************************************************
                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.Parameters.Add(Months);
                        //                cmd2.Parameters.Add(Year);
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<TDSReportBillToBillDc> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<TDSReportBillToBillDc>(reader2).ToList();
                        //                ////******************************************************************
                        //                //// List<TDSReportBillToBillDc> reportdata = context.Database.SqlQuery<TDSReportBillToBillDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();
                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "TCSReport")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                var Months = new SqlParameter("month", month);
                        //                var Year = new SqlParameter("year", year);
                        //                //*****************************************************************
                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.Parameters.Add(Months);
                        //                cmd2.Parameters.Add(Year);
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<GetSalesRegisterDc> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<GetSalesRegisterDc>(reader2).ToList();
                        //                ////******************************************************************
                        //                ////  List<GetSalesRegisterDc> reportdata = context.Database.SqlQuery<GetSalesRegisterDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();
                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "FreebiesData")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                var Months = new SqlParameter("month", month);
                        //                var Year = new SqlParameter("year", year);
                        //                //*****************************************************************
                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.Parameters.Add(Months);
                        //                cmd2.Parameters.Add(Year);
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<FreebiesDataDc> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<FreebiesDataDc>(reader2).ToList();
                        //                ////******************************************************************
                        //                //// List<FreebiesDataDc> reportdata = context.Database.SqlQuery<FreebiesDataDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();
                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "OrderBillDiscountData")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                var Months = new SqlParameter("month", month);
                        //                var Year = new SqlParameter("year", year);
                        //                //*****************************************************************
                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.Parameters.Add(Months);
                        //                cmd2.Parameters.Add(Year);
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<OrderBillDiscountDataDc> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<OrderBillDiscountDataDc>(reader2).ToList();
                        //                ////******************************************************************
                        //                //// List<OrderBillDiscountDataDc> reportdata = context.Database.SqlQuery<OrderBillDiscountDataDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();
                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "InventoryAging")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                var Months = new SqlParameter("month", month);
                        //                var Year = new SqlParameter("year", year);
                        //                //*****************************************************************
                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.Parameters.Add(Months);
                        //                cmd2.Parameters.Add(Year);
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<InventoryAgingDc> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<InventoryAgingDc>(reader2).ToList();
                        //                ////******************************************************************
                        //                ////List<InventoryAgingDc> reportdata = context.Database.SqlQuery<InventoryAgingDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();
                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "WarehouseInTransitStock")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                var monthEndDate = new SqlParameter("monthEndDate", previousEndDate);
                        //                //*****************************************************************
                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.Parameters.Add(monthEndDate);
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<WarehouseInTransitStockDc> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<WarehouseInTransitStockDc>(reader2).ToList();
                        //                ////******************************************************************
                        //                ////List<WarehouseInTransitStockDc> reportdata = context.Database.SqlQuery<WarehouseInTransitStockDc>("Exec " + x.Value + " @monthEndDate", monthEndDate).ToList();
                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "InternalTransfers")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                var Months = new SqlParameter("month", month);
                        //                var Year = new SqlParameter("year", year);
                        //                //*****************************************************************
                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.Parameters.Add(Months);
                        //                cmd2.Parameters.Add(Year);
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<InternalTransferDc> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<InternalTransferDc>(reader2).ToList();
                        //                ////******************************************************************
                        //                ////List<InternalTransferDc> reportdata = context.Database.SqlQuery<InternalTransferDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();

                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);

                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "OrderDeliveryCharges")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                var Months = new SqlParameter("month", month);
                        //                var Year = new SqlParameter("year", year);
                        //                //*****************************************************************
                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                // var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.Parameters.Add(Months);
                        //                cmd2.Parameters.Add(Year);
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<OrderDeliveryChargesDc> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<OrderDeliveryChargesDc>(reader2).ToList();
                        //                //******************************************************************
                        //                // List<OrderDeliveryChargesDc> reportdata = context.Database.SqlQuery<OrderDeliveryChargesDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();

                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "DirectUdharReport")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                var Months = new SqlParameter("month", month);
                        //                var Year = new SqlParameter("year", year);

                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.Parameters.Add(Months);
                        //                cmd2.Parameters.Add(Year);
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<DirectUdharDC> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<DirectUdharDC>(reader2).ToList();
                        //                ////    List<GetSalesRegisterDc> reportdata = context.Database.SqlQuery<GetSalesRegisterDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();

                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "ClearanceStockData")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                var Months = new SqlParameter("month", month);
                        //                var Year = new SqlParameter("year", year);

                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.Parameters.Add(Months);
                        //                cmd2.Parameters.Add(Year);
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                var reader2 = cmd2.ExecuteReader();
                        //                //List<ClearanceStockDataDC> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<ClearanceStockDataDC>(reader2).ToList();
                        //                ////    List<GetSalesRegisterDc> reportdata = context.Database.SqlQuery<GetSalesRegisterDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();

                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "NonSaleableStock")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;
                        //                var Months = new SqlParameter("month", month);
                        //                var Year = new SqlParameter("year", year);

                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.Parameters.Add(Months);
                        //                cmd2.Parameters.Add(Year);
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                // var reader2 = cmd2.ExecuteReader();
                        //                //List<NonSaleableStockDC> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<NonSaleableStockDC>(reader2).ToList();
                        //                //    List<GetSalesRegisterDc> reportdata = context.Database.SqlQuery<GetSalesRegisterDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();

                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        //if (x.Key == "RedispatchedWalletPointData")
                        //{
                        //    using (var context = new AuthContext())
                        //    {
                        //        using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                        //        {
                        //            using (var cmd2 = new SqlCommand())
                        //            {
                        //                if (connection.State != ConnectionState.Open)
                        //                    connection.Open();

                        //                cmd2.Connection = connection;

                        //                var Months = new SqlParameter("month", month);
                        //                var Year = new SqlParameter("year", year);

                        //                //if (context.Database.Connection.State != ConnectionState.Open)
                        //                //    context.Database.Connection.Open();
                        //                //var cmd2 = context.Database.Connection.CreateCommand();
                        //                cmd2.CommandText = x.Value;
                        //                cmd2.Parameters.Add(Months);
                        //                cmd2.Parameters.Add(Year);
                        //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd2.CommandTimeout = 1200;
                        //                // Run the sproc
                        //                //var reader2 = cmd2.ExecuteReader();
                        //                //List<RedispatchedWalletPointDataDC> reportdata = ((IObjectContextAdapter)context)
                        //                //.ObjectContext
                        //                //.Translate<RedispatchedWalletPointDataDC>(reader2).ToList();
                        //                //    List<GetSalesRegisterDc> reportdata = context.Database.SqlQuery<GetSalesRegisterDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();

                        //                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                        //                SqlDataAdapter da = new SqlDataAdapter(cmd2);
                        //                DataTable dt = new DataTable();
                        //                da.Fill(dt);
                        //                dt.TableName = x.Key;
                        //                dataTables.Add(dt);
                        //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                        //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                        //            }
                        //        }
                        //    }
                        //}
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        TextFileLogHelper.LogError("Error During File Create for " + x.Key + ":" + ex.ToString());
                        EmailHelper.SendMail(AppConstants.MasterEmail, "s.patil@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " --Month End Account Report Error", ex.ToString(), "");
                    }
                });
                TextFileLogHelper.LogError("Error AccountMonthEndData DataTable_To_Excel end");
                if (parellelResult.IsCompleted)
                    result = true;
            }
            return result;
        }




        public async Task<bool> MonthEndDeliveredData(int month, int year)
        {
            DataTable dt = new DataTable();
            AuthContext context = new AuthContext();
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/MonthEndData/" + DateTime.Now.ToString("MMM-yyyy"));

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);
            //file delete code..
            //string[] files = Directory.GetFiles(ExcelSavePath);
            //foreach (string file in files)
            //{
            //    File.Delete(file);
            //}
            using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
            {
                using (var cmd2 = new SqlCommand("[MonthEnd].[MonthEndDeliveredData]", connection))
                {
                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    cmd2.Connection = connection;

                    cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd2.CommandTimeout = 1200;
                    cmd2.Parameters.Add("@month", SqlDbType.Int).Value = month;
                    cmd2.Parameters.Add("@year", SqlDbType.Int).Value = year;
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd2))
                    {
                        da.Fill(dt);
                    }

                    string filePath = ExcelSavePath + "\\" + "MonthEndDeliveredData" + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";

                    if (ExcelGenerator.DataTable_To_Excel(dt, "MonthEndDeliveredData", filePath))
                    {
                        string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                        string To = "", From = "", Bcc = "";
                        DataTable emaildatatable = new DataTable();

                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='MonthEndDeliveredData'", connection))
                        {
                            if (connection.State != ConnectionState.Open)
                                connection.Open();
                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }

                        if (emaildatatable.Rows.Count > 0)
                        {
                            To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                            From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                            Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                        }
                        string subject = "MonthEndDelivery Report";
                        string message = "MonthEndDeliveredData is Attached Below";
                        if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                            EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                        else
                            logger.Error("Data Report To and From empty");
                    }

                }
            }
            return true;
        }

        public bool AutoBatchSubscribe()
        {
            FileWriter fileWriter = new FileWriter();
            //IsChangeEventRunning = true;
            MongoDbHelper<BatchCodeSubjectMongoQueue> mongoDbHelper = new MongoDbHelper<BatchCodeSubjectMongoQueue>();
            //var subjectList = mongoDbHelper.GetAllUnProcessed();
            var collection = mongoDbHelper.mongoDatabase.GetCollection<BatchCodeSubjectMongoQueue>(typeof(BatchCodeSubjectMongoQueue).Name);
            var subjectList = collection.Find(x => x.IsProcess == false && x.TransactionDate < DateTime.Now && x.HashCode != null && x.HashCode != "").ToList();

            fileWriter.WriteToFile($"Total Subjects are: {subjectList.Count().ToString()}");
            if (subjectList != null && subjectList.Any())
            {
                foreach (var item in subjectList)
                {
                    BatchCodeSubject subject = new BatchCodeSubject
                    {
                        WarehouseId = item.WarehouseId,
                        TransactionType = item.TransactionType,
                        TransactionDate = item.TransactionDate,
                        Quantity = item.Quantity,
                        ObjectId = item.ObjectId,
                        ObjectDetailId = item.ObjectDetailId,
                        ItemMultiMrpId = item.ItemMultiMrpId,
                        HashCode = item.HashCode
                    };
                    Subscriber subscriber = new Subscriber();

                    bool result = AsyncContext.Run(() => subscriber.OnSubscribe(subject));
                    //if (result)
                    //{
                    //    mongoDbHelper.UpdateByHashCode(subject.HashCode, true);
                    //}
                    //else
                    //{
                    //    mongoDbHelper.UpdateWhenSubscriberErrorOccurs(subject.HashCode, true, "Action returns false");

                    //}
                    //BatchCodeHelper batchCodeHelper = new BatchCodeHelper();
                    //batchCodeHelper.DeleteSubjectFromMongo(item);

                }
                //System.Threading.Interlocked.Exchange(ref IsChangeEventRunning, 0);

            }
            else
            {
                //System.Threading.Interlocked.Exchange(ref IsChangeEventRunning, 0);
            }
            return true;
        }

        public bool ConsumerCurrentNetStockAutoLive()
        {
            using (AuthContext context = new AuthContext())
            {
                context.Database.CommandTimeout = 300;
                var consumerCurrentNetStockAutoLive = context.Database.SqlQuery<ActivateConsumerStockDc>("exec [ConsumerCurrentNetStockAutoLive]").ToList();
                if (consumerCurrentNetStockAutoLive != null && consumerCurrentNetStockAutoLive.Any())
                {
                    var activateItemForConsumerStores = new List<ActivateItemForConsumerStoreDc>();
                    foreach (var x in consumerCurrentNetStockAutoLive)
                    {
                        activateItemForConsumerStores.Add(new ActivateItemForConsumerStoreDc
                        {
                            ItemMultiMrpId = x.ItemMultiMrpId,
                            Qty = x.CurrentNetInventory,
                            WarehouseId = x.WarehouseId
                        });
                    }
                    ActivateItemForConsumerStore(activateItemForConsumerStores, context);

                }
            }
            return true;
        }
        private bool ActivateItemForConsumerStore(List<ActivateItemForConsumerStoreDc> ActivateItemForConsumerStore, AuthContext context)
        {
            List<ItemMaster> NewAdditems = new List<ItemMaster>();

            //var loopResult = Parallel.ForEach(ActivateItemForConsumerStore.Select(x => x.WarehouseId).Distinct(), (WarehouseId) =>
            //{

            foreach (var WarehouseId in ActivateItemForConsumerStore.Select(x => x.WarehouseId).Distinct().ToList())
            {

                List<int> ItemMultiMrpIds = ActivateItemForConsumerStore.Where(x => x.WarehouseId == WarehouseId).Select(x => x.ItemMultiMrpId).ToList();

                var ItemMultiMRPlist = context.ItemMultiMRPDB.Where(x => ItemMultiMrpIds.Contains(x.ItemMultiMRPId)).ToList();

                List<string> numbers = ItemMultiMRPlist.Select(x => x.ItemNumber).Distinct().ToList();

                var itemmasterlist = context.itemMasters.Where(x => numbers.Contains(x.Number) && x.WarehouseId == WarehouseId && x.IsDisContinued == false && x.Deleted == false).ToList();

                if (itemmasterlist != null && itemmasterlist.Any(x => ItemMultiMrpIds.Contains(x.ItemMultiMRPId)))
                {

                    foreach (var itemmaster in itemmasterlist.Where(x => ItemMultiMrpIds.Contains(x.ItemMultiMRPId) && !x.active).ToList())
                    {
                        itemmaster.UnitPrice = ItemMultiMRPlist.FirstOrDefault(x => x.ItemMultiMRPId == itemmaster.ItemMultiMRPId).MRP;
                        itemmaster.active = true;
                        itemmaster.ModifiedBy = 1;
                        itemmaster.UpdatedDate = DateTime.Now;
                        itemmaster.Description = "ActivateItemForConsumerStore";
                        context.Entry(itemmaster).State = EntityState.Modified;
                    }
                }

                var ExistItemMultiMrpId = itemmasterlist.Select(x => x.ItemMultiMRPId).ToList();
                var NotExistItemMultiMrpId = ItemMultiMrpIds.Where(x => !ExistItemMultiMrpId.Contains(x)).Select(x => x).ToList();

                foreach (var ItemMultiMrpId in NotExistItemMultiMrpId.Distinct())
                {
                    var mrpobj = ItemMultiMRPlist.FirstOrDefault(x => x.ItemMultiMRPId == ItemMultiMrpId);
                    var itemmaster = itemmasterlist.Where(x => x.WarehouseId == WarehouseId && x.Number == mrpobj.ItemNumber).FirstOrDefault();
                    if (itemmaster != null && mrpobj != null)
                    {
                        ItemMaster item = new ItemMaster();
                        item.SupplierId = itemmaster.SupplierId;
                        item.SupplierName = itemmaster.SupplierName;
                        item.SUPPLIERCODES = itemmaster.SUPPLIERCODES;
                        item.DepoId = itemmaster.DepoId;
                        item.DepoName = itemmaster.DepoName;
                        item.BuyerName = itemmaster.BuyerName;
                        item.BuyerId = itemmaster.BuyerId;
                        item.GruopID = itemmaster.GruopID;
                        item.TGrpName = itemmaster.TGrpName;
                        item.DistributionPrice = mrpobj.MRP;
                        item.TotalTaxPercentage = itemmaster.TotalTaxPercentage;
                        item.CessGrpID = itemmaster.CessGrpID;
                        item.CessGrpName = itemmaster.CessGrpName;
                        item.TotalCessPercentage = itemmaster.TotalCessPercentage;
                        item.CatLogoUrl = itemmaster.CatLogoUrl;
                        item.WarehouseId = itemmaster.WarehouseId;
                        item.WarehouseName = itemmaster.WarehouseName;
                        item.BaseCategoryid = itemmaster.BaseCategoryid;
                        item.LogoUrl = itemmaster.LogoUrl;
                        item.UpdatedDate = DateTime.Now;
                        item.CreatedDate = DateTime.Now;
                        item.CategoryName = itemmaster.CategoryName;
                        item.Categoryid = itemmaster.Categoryid;
                        item.SubcategoryName = itemmaster.SubcategoryName;
                        item.SubCategoryId = itemmaster.SubCategoryId;
                        item.SubsubcategoryName = itemmaster.SubsubcategoryName;
                        item.SubsubCategoryid = itemmaster.SubsubCategoryid;
                        item.SubSubCode = itemmaster.SubSubCode;
                        item.itemcode = itemmaster.itemcode;
                        item.marginPoint = itemmaster.marginPoint;
                        item.Number = itemmaster.Number;
                        item.PramotionalDiscount = itemmaster.PramotionalDiscount;
                        item.MinOrderQty = 1;
                        item.NetPurchasePrice = mrpobj.MRP;
                        item.GeneralPrice = mrpobj.MRP;
                        item.promoPerItems = itemmaster.promoPerItems;
                        item.promoPoint = itemmaster.promoPoint;
                        item.PurchaseMinOrderQty = itemmaster.PurchaseMinOrderQty;
                        item.PurchaseSku = itemmaster.PurchaseSku;
                        item.PurchaseUnitName = itemmaster.PurchaseUnitName;
                        item.SellingSku = itemmaster.SellingSku;
                        item.SellingUnitName = itemmaster.SellingUnitName;
                        item.SizePerUnit = itemmaster.SizePerUnit;
                        item.VATTax = itemmaster.VATTax;
                        item.HSNCode = itemmaster.HSNCode;
                        item.HindiName = itemmaster.HindiName;
                        item.CompanyId = itemmaster.CompanyId;
                        item.Reason = itemmaster.Reason;
                        item.DefaultBaseMargin = itemmaster.DefaultBaseMargin;
                        item.Deleted = false;
                        item.active = true;
                        item.itemBaseName = itemmaster.itemBaseName;
                        item.Cityid = itemmaster.Cityid;
                        item.CityName = itemmaster.CityName;
                        item.UOM = mrpobj.UOM;
                        item.UnitofQuantity = mrpobj.UnitofQuantity;
                        item.PurchasePrice = mrpobj.MRP;
                        item.UnitPrice = mrpobj.MRP;
                        item.ItemMultiMRPId = mrpobj.ItemMultiMRPId;
                        item.MRP = mrpobj.MRP;
                        item.price = mrpobj.MRP;
                        if (itemmaster.IsSensitive == true && itemmaster.IsSensitiveMRP == true)
                        {
                            item.itemname = itemmaster.itemBaseName + " " + mrpobj.MRP + " MRP " + mrpobj.UnitofQuantity + " " + mrpobj.UOM;
                        }
                        else if (itemmaster.IsSensitive == true && itemmaster.IsSensitiveMRP == false)
                        {
                            item.itemname = item.itemBaseName + " " + mrpobj.UnitofQuantity + " " + mrpobj.UOM; //item display name 
                        }
                        else if (itemmaster.IsSensitive == false && itemmaster.IsSensitiveMRP == false)
                        {
                            item.itemname = item.itemBaseName; //item display name
                        }
                        else if (itemmaster.IsSensitive == false && itemmaster.IsSensitiveMRP == true)
                        {
                            item.itemname = item.itemBaseName + " " + mrpobj.MRP + " MRP";//item display name 
                        }
                        item.SellingUnitName = item.itemname + " " + item.MinOrderQty + "Unit";//item selling unit name
                        item.PurchaseUnitName = item.itemname + " " + item.PurchaseMinOrderQty + "Unit";//
                        item.IsSensitive = itemmaster.IsSensitive;
                        item.IsSensitiveMRP = itemmaster.IsSensitiveMRP;
                        item.ShelfLife = itemmaster.ShelfLife;
                        item.IsReplaceable = itemmaster.IsReplaceable;
                        item.BomId = itemmaster.BomId;
                        item.Type = itemmaster.Type;
                        item.CreatedBy = 1;
                        itemmaster.Description = "ActivateItemForConsumerStore";
                        item.SellerStorePrice = item.MRP;
                        item.IsSellerStoreItem = itemmaster.IsSellerStoreItem;
                        NewAdditems.Add(item);
                    }
                }
                //});
                //if (loopResult.IsCompleted)
                //{
                if (NewAdditems != null && NewAdditems.Any())
                {
                    context.itemMasters.AddRange(NewAdditems);
                }
                context.Commit();
                //}

            }
            return true;
        }


        //OnSubscribeErrorSendMail
        public bool OnSubscribeErrorSendMail()
        {
            bool IsEmailSent = false;
            DateTime TOdayDate = DateTime.Now;
            DateTime date24HoursFromNow = TOdayDate.AddHours(-24);
            List<BatchCodeSubjectMongoQueue> OrderInvoicedata = new List<BatchCodeSubjectMongoQueue>();
            MongoDbHelper<BatchCodeSubjectMongoQueue> mongoDbHelper = new MongoDbHelper<BatchCodeSubjectMongoQueue>();//x.SubscriberError.Contains("OnSubscribeError") && //&& x.TransactionDate >= date24HoursFromNow && x.TransactionDate <= TOdayDate
            var BatchCodePredicate = PredicateBuilder.New<BatchCodeSubjectMongoQueue>(x => x.SubscriberError.Contains("OnSubscribe") && x.IsSubscriberErrorOccurs == true && x.QueueName == "OrderInvoice" && (x.TransactionDate) >= (date24HoursFromNow) && (x.TransactionDate) <= (TOdayDate) && x.IsProcess == true);
            OrderInvoicedata = (mongoDbHelper.Select(BatchCodePredicate)).ToList();
            List<long> OrderIds = OrderInvoicedata.Select(x => x.ObjectId).ToList();
            AuthContext context = new AuthContext();
            var dispatchData = context.OrderDispatchedMasters.Where(x => OrderIds.Contains(x.OrderId) && string.IsNullOrEmpty(x.invoice_no)).Select(x => new { x.OrderId, x.Status }).ToList();
            List<OnSubscribeErrorSendMailDc> OnSubscribeErrorSendMailDc = Mapper.Map(dispatchData).ToANew<List<OnSubscribeErrorSendMailDc>>();
            if (OrderInvoicedata.Any() && OrderInvoicedata.Count() > 0)
            {
                string ExcelSavePath = HostingEnvironment.MapPath("~/ExcelGeneratePath/");

                if (!Directory.Exists(ExcelSavePath))
                    Directory.CreateDirectory(ExcelSavePath);

                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(OnSubscribeErrorSendMailDc);
                dt.TableName = "Daily_OrderSubscribeError_Report";
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "Daily_OrderSubscribeError_Report_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='OnSubscribeErrorInInvoice'", connection))
                        {
                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + "Daily InvoiceNo Failed Report";
                    string message = "Please find below orders whose Invoice is not generated!";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        IsEmailSent = EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                    else
                        logger.Error("Daily InvoiceNo Failed Report");

                }
            }
            return IsEmailSent;
        }

        public bool ConsumerCurrentZeroStockAutoInactive()
        {
            using (AuthContext context = new AuthContext())
            {
                context.Database.CommandTimeout = 300;
                var consumerCurrentZeroStockAutoLive = context.Database.SqlQuery<int>("exec [ConsumerCurrentZeroStockAutoLive]").ToList();
                if (consumerCurrentZeroStockAutoLive != null && consumerCurrentZeroStockAutoLive.Any())
                {
                    List<int> itemids = consumerCurrentZeroStockAutoLive.Select(x => x).Distinct().ToList();
                    List<ItemMaster> itemMasters = context.itemMasters.Where(x => itemids.Contains(x.ItemId)).ToList();
                    if(itemMasters != null && itemMasters.Any())
                    {
                        foreach (var item in itemMasters)
                        {
                            item.active = false;
                            item.Description = "InActivateItemForConsumerStore";
                            item.UpdatedDate = DateTime.Now;
                            item.ModifiedBy = 1;
                            context.Entry(item).State = EntityState.Modified;
                        }
                        context.Commit();
                    }
                }
            }
            return true;
        }
    }
    public class OnSubscribeErrorSendMailDc
    {
        public long OrderId { get; set; }
        public string Status { get; set; }
    }
    public class Retailercustomer
    {
        public int CustomerId { get; set; }
        public int? Warehouseid { get; set; }
        public string WarehouseName { get; set; }
        public string CityName { get; set; }
        public int? Cityid { get; set; }
        public int? ClusterId { get; set; }
        public string ClusterName { get; set; }
        public string Mobile { get; set; }
        public string ShopName { get; set; }
        public string Skcode { get; set; }
        public string ShippingAddress { get; set; }
    }

    public class RetailerObject
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }

    public class RetailerData
    {
        public List<Retailercustomer> Retailercustomers { get; set; }
        public List<RetailerObject> Categories { get; set; }
        public List<RetailerObject> SubCategories { get; set; }
        public List<RetailerObject> SubSubCategories { get; set; }
    }

    public class FinalOrdeOtpAccess
    {
        public string WarehouseName { get; set; }
        public string AccessBy { get; set; }
        public DateTime Date { get; set; }
        public int OrderNo { get; set; }
        public string SKCode { get; set; }
        public string CustomerType { get; set; }
        public string OTPStatus { get; set; }
        public string Department { get; set; }
    }

    public class ActivateItemForConsumerStoreDc
    {
        public int ItemMultiMrpId { get; set; }
        public int WarehouseId { get; set; }
        public int Qty { get; set; }

    }
    public class ActivateConsumerStockDc
    {
        public string WarehouseName { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public int CurrentInventory { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int CurrentNetInventory { get; set; }
        public int WarehouseId { get; set; }

    }
}