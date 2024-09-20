using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.Model.Account;
using NLog;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/AccountMIS")]
    public class AccountMISController : BaseAuthController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);


        [Route("GetAccountMISData")]
        [HttpPost]
        public HttpResponseMessage GetAccountMISData(AccountMISRequest accountMISRequest)
        {
            DataTable AccountMisTable = new DataTable();
            using (var context = new AuthContext())
            {
                using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                {
                    var WhIdDt = new DataTable();
                    WhIdDt.Columns.Add("IntValue");
                    foreach (var item in accountMISRequest.WarehouseIds)
                    {
                        var dr = WhIdDt.NewRow();
                        dr["IntValue"] = item;
                        WhIdDt.Rows.Add(dr);
                    }
                    var param = new SqlParameter("WarehouseIds", WhIdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    SqlCommand cmd = new SqlCommand("[dbo].[GetAccountMISData]", connection);
                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    //var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetAccountMISData]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@StartDate", accountMISRequest.StartDate));
                    cmd.Parameters.Add(new SqlParameter("@EndDate", accountMISRequest.EndDate));
                    cmd.Parameters.Add(param);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(AccountMisTable);
                    da.Dispose();
                }
            }
            var result = Newtonsoft.Json.JsonConvert.SerializeObject(AccountMisTable);
            result = result.Replace(@"\", "");
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }


        [Route("ExportAllAccountMISData")]
        [HttpPost]
        public APIResponse ExportAllAccountMISData(AccountMISRequest accountMISRequest)
        {
            APIResponse aPIResponse = new APIResponse();
            string fileUrl = string.Empty;
            using (var context = new AuthContext())
            {
                using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                {
                    var WhIdDt = new DataTable();
                    WhIdDt.Columns.Add("IntValue");
                    foreach (var item in accountMISRequest.WarehouseIds)
                    {
                        var dr = WhIdDt.NewRow();
                        dr["IntValue"] = item;
                        WhIdDt.Rows.Add(dr);
                    }
                    var param = new SqlParameter("WarehouseIds", WhIdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    SqlCommand cmd = new SqlCommand("[dbo].[ExportAccountMISData]", connection);
                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    //var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[ExportAccountMISData]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@StartDate", accountMISRequest.StartDate));
                    cmd.Parameters.Add(new SqlParameter("@EndDate", accountMISRequest.EndDate));
                    cmd.Parameters.Add(param);
                    DataTable AccountMisTable = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(AccountMisTable);
                    if (AccountMisTable.Rows.Count < 1)
                    {
                        aPIResponse.Status = false;
                        aPIResponse.Message = "No Data Found";
                        return aPIResponse;
                    }
                    da.Dispose();
                    var fileName = DateTime.Now.ToString("yyyyddMHHmmss") + "_MIS.csv";
                    string ExcelSavePath = HttpContext.Current.Server.MapPath("~/AccountMISReport/");
                    if (!Directory.Exists(ExcelSavePath))
                        Directory.CreateDirectory(ExcelSavePath);
                    string path = Path.Combine(ExcelSavePath, fileName);
                    AccountMisTable.WriteToCsvFile(path);



                    /* fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri(HttpContext.Current.Request.UrlReferrer.AbsoluteUri).Scheme
                                                                     , HttpContext.Current.Request.Url.DnsSafeHost
                                                                     , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                     , string.Format("ExcelGeneratePath/{0}", zipfilename));*/
                    fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                     , HttpContext.Current.Request.Url.DnsSafeHost
                                                                     , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                     , string.Format("AccountMISReport/{0}", fileName));

                    if (!string.IsNullOrEmpty(fileUrl))
                    {
                        aPIResponse.Status = true;
                        aPIResponse.Message = fileUrl;
                    }
                    else
                    {
                        aPIResponse.Status = false;
                        aPIResponse.Message = "No Data foundd!";
                    }
                }
            }
            return aPIResponse;
        }

        [HttpPost]
        [Route("AccountMISTemplateUploder")]
        public IHttpActionResult UploadFilees(DateTime MonthDate)
        {
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var formData1 = HttpContext.Current.Request.Form["compid"];
                // Access claims
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                // Get the uploaded image from the Files collection
                var httpPostedFile = HttpContext.Current.Request.Files["file"];

                if (httpPostedFile != null)
                {
                    string ext = Path.GetExtension(httpPostedFile.FileName);
                    if (ext == ".xlsx")
                    {
                        byte[] buffer = new byte[httpPostedFile.ContentLength];

                        using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
                        {
                            br.Read(buffer, 0, buffer.Length);
                        }
                        XSSFWorkbook hssfwb;
                        //   XSSFWorkbook workbook1;
                        using (MemoryStream memStream = new MemoryStream())
                        {
                            BinaryFormatter binForm = new BinaryFormatter();
                            memStream.Write(buffer, 0, buffer.Length);
                            memStream.Seek(0, SeekOrigin.Begin);
                            hssfwb = new XSSFWorkbook(memStream);
                        }
                        return ReadAccountMISUplodFile(hssfwb, userid, MonthDate);
                    }
                    else
                    {
                        return Created("File extnsion required .xlsx", "File extnsion required .xlsx");
                    }
                }
            }

            return Created("Error", "Error");
        }
        public IHttpActionResult ReadAccountMISUplodFile(XSSFWorkbook hssfwb, int userid, DateTime MonthDate)
        {
            using (var context = new AuthContext())
            {
                string Msg = string.Empty;
                string sSheetName = hssfwb.GetSheetName(0);
                ISheet sheet = hssfwb.GetSheet(sSheetName);
                IRow rowData;
                ICell cellData = null;
                try
                {
                    List<string> headerlst = new List<string>();
                    var AccountTallyLadgers = context.AccountTallyLadgers.ToList();
                    var Warehouses = context.Warehouses.Select(x => new { x.WarehouseName, x.WarehouseId }).ToList();
                    var AccountDepartments = context.AccountDepartments.ToList();
                    var AccountVerticals = context.AccountVerticals.ToList();
                    var AccountCanvasHeads = context.AccountCanvasHeads.ToList();
                    var AccountExpenseMISHeads = context.AccountExpenseMISHeads.ToList();
                    var AccountMISHeads = context.AccountMISHeads.ToList();
                    var AccountCostMISHeads = context.AccountCostMISHeads.ToList();
                    var AccountFinancialHeads = context.AccountFinancialHeads.ToList();
                    var AccountCM5Heads = context.AccountCM5Heads.ToList();


                    List<UploadEntity> listdata = new List<UploadEntity>();
                    for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                    {
                        if (iRowIdx == 0)
                        {
                            rowData = sheet.GetRow(iRowIdx);
                            //rowData = sheet.getcol
                            if (rowData != null)
                            {
                                string Validatedheader = ValidateHeaderss(rowData);
                                if (Validatedheader != null)
                                {
                                    return Created(Validatedheader, Validatedheader);
                                }
                            }
                        }

                        else
                        {
                            rowData = sheet.GetRow(iRowIdx);
                            cellData = rowData.GetCell(0);
                            rowData = sheet.GetRow(iRowIdx);
                            if (rowData != null)
                            {

                                UploadEntity trnfrorder = new UploadEntity();

                                try
                                {
                                    string col = null;
                                    col = string.Empty;
                                    cellData = rowData.GetCell(0);
                                    var cellDataa = cellData == null ? "" : cellData.ToString().Trim();
                                    var sid = AccountTallyLadgers.Where(x => x.Name == cellDataa).Select(x => x.Id).FirstOrDefault();
                                    if (sid == 0)
                                    {
                                        trnfrorder.AccountTallyLadgerId = null;
                                    }
                                    else
                                    {
                                        trnfrorder.AccountTallyLadgerId = Convert.ToInt32(sid);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(1);
                                    var cellDataaa = cellData == null ? "" : cellData.ToString().Trim();
                                    var wid = Warehouses.Where(x => x.WarehouseName == cellDataaa).Select(x => x.WarehouseId).FirstOrDefault();
                                    if (wid != 0)
                                    {
                                        trnfrorder.WarehouseId = Convert.ToInt32(wid);
                                    }


                                    col = string.Empty;
                                    cellData = rowData.GetCell(2);
                                    var cellData2 = cellData == null ? "" : cellData.ToString().Trim();
                                    var dept = AccountDepartments.Where(x => x.Name == cellData2).Select(x => x.Id).FirstOrDefault();
                                    if (dept == 0)
                                    {
                                        trnfrorder.AccountDepartmentId = null;
                                    }
                                    else
                                    {
                                        trnfrorder.AccountDepartmentId = Convert.ToInt32(dept);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(3);
                                    var cellData3 = cellData == null ? "" : cellData.ToString().Trim();
                                    var vertical = AccountVerticals.Where(x => x.Name == cellData3).Select(x => x.Id).FirstOrDefault();
                                    if (vertical == 0)
                                    {
                                        trnfrorder.AccountVerticalId = null;
                                    }
                                    else
                                    {
                                        trnfrorder.AccountVerticalId = Convert.ToInt32(vertical);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(4);
                                    var cellData4 = cellData == null ? "" : cellData.ToString().Trim();
                                    var cm = AccountCM5Heads.Where(x => x.Name == cellData4).Select(x => x.Id).FirstOrDefault();
                                    if (cm == 0)
                                    {
                                        trnfrorder.AccountCM5HeadId = null;
                                    }
                                    else
                                    {
                                        trnfrorder.AccountCM5HeadId = Convert.ToInt32(cm);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(5);
                                    var cellData5 = cellData == null ? "" : cellData.ToString().Trim();
                                    var canvashead = AccountCanvasHeads.Where(x => x.Name == cellData5).Select(x => x.Id).FirstOrDefault();
                                    if (canvashead == 0)
                                    {
                                        trnfrorder.AccountCanvasHeadId = null;
                                    }
                                    else
                                    {
                                        trnfrorder.AccountCanvasHeadId = Convert.ToInt32(canvashead);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(6);
                                    var cellData6 = cellData == null ? "" : cellData.ToString().Trim();
                                    var ExMisHead = AccountExpenseMISHeads.Where(x => x.Name == cellData6).Select(x => x.Id).FirstOrDefault();
                                    if (ExMisHead == 0)
                                    {
                                        trnfrorder.AccountExpenseMISHeadId = null;
                                    }
                                    else
                                    {
                                        trnfrorder.AccountExpenseMISHeadId = Convert.ToInt32(ExMisHead);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(7);
                                    var cellData7 = cellData == null ? "" : cellData.ToString().Trim();
                                    var MisHead = AccountMISHeads.Where(x => x.Name == cellData7).Select(x => x.Id).FirstOrDefault();
                                    if (MisHead == 0)
                                    {
                                        trnfrorder.AccountMISHeadId = null;
                                    }
                                    else
                                    {
                                        trnfrorder.AccountMISHeadId = Convert.ToInt32(MisHead);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(8);
                                    var cellData8 = cellData == null ? "" : cellData.ToString().Trim();
                                    var CostMisHead = AccountCostMISHeads.Where(x => x.Name == cellData8).Select(x => x.Id).FirstOrDefault();
                                    if (CostMisHead == 0)
                                    {
                                        trnfrorder.AccountCostMISHeadId = null;
                                    }
                                    else
                                    {
                                        trnfrorder.AccountCostMISHeadId = Convert.ToInt32(CostMisHead);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(9);
                                    var cellData9 = cellData == null ? "" : cellData.ToString().Trim();
                                    var FinHead = AccountFinancialHeads.Where(x => x.Name == cellData9).Select(x => x.Id).FirstOrDefault();
                                    if (FinHead == 0)
                                    {
                                        trnfrorder.AccountFinancialHeadId = null;
                                    }
                                    else
                                    {
                                        trnfrorder.AccountFinancialHeadId = Convert.ToInt32(FinHead);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(10);
                                    var cellData10 = cellData == null ? "" : cellData.ToString().Trim();
                                    col = cellData10 == "" && cellData10.Trim() != "-" ? null : cellData10.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.Amount = Convert.ToDouble(col);
                                    }

                                    listdata.Add(trnfrorder);
                                }
                                catch (Exception ex)
                                {
                                    Msg = ex.Message;
                                }
                            }
                        }
                    }

                    if (listdata != null && listdata.Any())
                    {
                        var existdata = context.AccountMISDataUploads.Where(x => EntityFunctions.TruncateTime(x.MonthDate) == MonthDate).ToList();
                        context.AccountMISDataUploads.RemoveRange(existdata);

                        foreach (var item in listdata.Where(x => x.Amount > 0))
                        {

                            AccountMISDataUpload data = new AccountMISDataUpload();
                            data.AccountDepartmentId = item.AccountDepartmentId;
                            data.AccountCanvasHeadId = item.AccountCanvasHeadId;
                            data.AccountExpenseMISHeadId = item.AccountExpenseMISHeadId;
                            data.AccountTallyLadgerId = item.AccountTallyLadgerId;
                            data.AccountFinancialHeadId = item.AccountFinancialHeadId;
                            data.AccountCostMISHeadId = item.AccountCostMISHeadId;
                            data.AccountVerticalId = item.AccountVerticalId;
                            data.AccountMISHeadId = item.AccountMISHeadId;
                            data.AccountCM5HeadId = item.AccountCM5HeadId;
                            data.WarehouseId = item.WarehouseId;
                            data.Amount = item.Amount;
                            data.MonthDate = MonthDate;
                            context.AccountMISDataUploads.Add(data);
                        }
                        if (context.Commit() > 0)
                        {
                            Msg = "record uploaded successfully!!";
                        }
                        else
                        {
                            Msg = "record not uploaded !!";
                        }
                    }
                    return Created(Msg, Msg);

                }

                catch (Exception ex)
                {
                    Msg = ex.Message;
                }
                return Created("Error", "Error");
            }
        }
        private string ValidateHeaderss(IRow rowData)
        {
            string strJSON = null;
            string field = string.Empty;
            field = rowData.GetCell(0).ToString();
            if (field != "Tally Ledger")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + "Tally Ledger" + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(1).ToString();
            if (field != "Warehouse")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + "Warehouse" + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(2).ToString();
            if (field != "Department")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + "Department" + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(3).ToString();
            if (field != "Vertical")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + "Vertical" + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(4).ToString();
            if (field != "CM5 head")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + "CM5 head" + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(5).ToString();
            if (field != "Canvas Head")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + "Canvas Head" + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(6).ToString();
            if (field != "Expense MIS Head")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + "Expense MIS Head" + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(7).ToString();
            if (field != "MIS Head")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + "MIS Head" + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(8).ToString();
            if (field != "Cost MIS Head")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + "Cost MIS Head" + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(9).ToString();
            if (field != "Financial Head")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + "Financial Head" + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(10).ToString();
            if (field != "Amount")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + "Amount" + " does not exist..try again");
                return strJSON;
            }

            return strJSON;
        }


        [Route("AccountMisTemplateExport")]
        [HttpPost]
        public APIResponse AccountMisTemplateExport(TemplateExportDc obj)
        {
            APIResponse aPIResponse = new APIResponse();
            string fileUrl = string.Empty;
            using (var context = new AuthContext())
            {
                using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("[dbo].[AccountMisTemplateExport]", connection);
                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    //var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[AccountMisTemplateExport]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@MonthDate", obj.MonthDate));
                    DataTable AccountMisTable = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(AccountMisTable);
                    if (AccountMisTable.Rows.Count < 1)
                    {
                        aPIResponse.Status = false;
                        aPIResponse.Message = "No Data Found";
                        return aPIResponse;
                    }
                    da.Dispose();
                    var fileName = DateTime.Now.ToString("yyyyddMHHmmss") + "_MIS.csv";
                    string ExcelSavePath = HttpContext.Current.Server.MapPath("~/AccountMisTemplateExport/");
                    if (!Directory.Exists(ExcelSavePath))
                        Directory.CreateDirectory(ExcelSavePath);
                    string path = Path.Combine(ExcelSavePath, fileName);
                    AccountMisTable.WriteToCsvFile(path);

                    fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                     , HttpContext.Current.Request.Url.DnsSafeHost
                                                                     , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                     , string.Format("AccountMisTemplateExport/{0}", fileName));

                    if (!string.IsNullOrEmpty(fileUrl))
                    {
                        aPIResponse.Status = true;
                        aPIResponse.Message = fileUrl;
                    }
                    else
                    {
                        aPIResponse.Status = false;
                        aPIResponse.Message = "No Data foundd!";
                    }
                }
            }
            return aPIResponse;
        }

        [HttpPost]
        [Route("GetMultipleCities")]
        public List<PrimeNgDropDown<int?>> WarehouseGetByCityListCommon(List<int> Stateid)
        {
            using (var db = new AuthContext())
            {
                var StateidDts = new DataTable();
                StateidDts.Columns.Add("IntValue");

                if (Stateid != null && Stateid.Any())
                {
                    foreach (var item in Stateid)
                    {
                        var dr = StateidDts.NewRow();
                        dr["IntValue"] = item;
                        StateidDts.Rows.Add(dr);
                    }
                }
                var Daysparam = new SqlParameter("stateid", StateidDts);
                Daysparam.SqlDbType = SqlDbType.Structured;
                Daysparam.TypeName = "dbo.IntValues";

                int userid = GetLoginUserId();
                var userIds = new SqlParameter("@userId", userid);
                var result = db.Database.SqlQuery<PrimeNgDropDown<int?>>("exec GetMultipleCities @userId,@stateid", userIds, Daysparam).ToList();
                return result;
            }
        }


        [HttpGet]
        [Route("InsertAccountMISData")]
        public APIResponse InsertAccountMISData(DateTime MOnthdate)
        {


            APIResponse aPIResponse = new APIResponse();
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (MOnthdate != null)
                {
                    if (userid > 0)
                    {
                        var IsProcess = context.AccountMISInserts.Any(x => !x.IsProcess);
                        if (!IsProcess)
                        {
                            AccountMISInsert ins = new AccountMISInsert();
                            ins.UserId = userid;
                            ins.MonthDate = MOnthdate;
                            ins.IsProcess = false;
                            context.AccountMISInserts.Add(ins);
                            if (context.Commit() > 0)
                            {
                                aPIResponse.Status = true;
                                aPIResponse.Message = "Request accepted please wait some time!!";
                            }
                            else
                            {
                                aPIResponse.Status = false;
                                aPIResponse.Message = "Some issue during save record please try after some time!!";
                            }

                        }
                        else
                        {
                            aPIResponse.Status = false;
                            aPIResponse.Message = "Already Pending request!!";
                        }
                    }
                }
            }
            return aPIResponse;
        }

        [Route("InsertAccountMISDataSchedular")]
        [HttpPost]
        [AllowAnonymous]
        public bool InsertAccountMISDataSchedular()
        {
            using (AuthContext context = new AuthContext())
            {

                var Data = context.AccountMISInserts.FirstOrDefault(x => x.IsProcess == false);
                if (Data != null)
                {
                    Data.StartDate = DateTime.Now;
                    var param1 = new SqlParameter("@MonthDate", Data.MonthDate);
                    int result = context.Database.ExecuteSqlCommand("InsertAccountMISData @MonthDate", param1);
                    Data.EndDate = DateTime.Now;
                    Data.IsProcess = true;
                    context.Entry(Data).State = EntityState.Modified;
                    context.Commit();
                }
            }
            return true;
        }



        public class TemplateExportDc
        {
            public DateTime MonthDate { get; set; }
        }
        public class UploadEntity
        {
            public int? AccountTallyLadgerId { get; set; }
            public int? AccountCanvasHeadId { get; set; }
            public int? AccountDepartmentId { get; set; }
            public int? AccountExpenseMISHeadId { get; set; }
            public int? AccountFinancialHeadId { get; set; }
            public int? AccountCostMISHeadId { get; set; }
            public int? AccountVerticalId { get; set; }
            public int? AccountMISHeadId { get; set; }
            public int? AccountCM5HeadId { get; set; }
            public double Amount { get; set; }
            public int WarehouseId { get; set; }
        }


        public class cityList
        {
            public int CityId { get; set; }
            public string CityName { get; set; }
        }
    }
}