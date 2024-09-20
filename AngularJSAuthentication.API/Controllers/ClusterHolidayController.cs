using AngularJSAuthentication.DataContracts.ClusterHoliday;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
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
    [RoutePrefix("api/ClusterHoliday")]
    public class ClusterHolidayController : ApiController
    {
        string strJSON = null;


        [HttpPost]
        [Route("AddClusterHoliday")]
        public APIResponse AddClusterHolidayAsync(ClusterHolidayDc ClusterHolidayDc)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                APIResponse res = new APIResponse();
                using (AuthContext context = new AuthContext())
                {
                    if (ClusterHolidayDc != null)
                    {
                        var ExistData = context.ClusterHolidays.Where(x => x.ClusterId == ClusterHolidayDc.clusterId && x.WarehouseId == ClusterHolidayDc.warehouseId && x.IsActive == true && x.IsDeleted == false).ToList(); //&& x.Year == ClusterHolidayDc.year
                        if (ExistData != null && ExistData.Any())
                        {
                            foreach (var data in ExistData)
                            {
                                data.IsDeleted = true;
                                data.IsActive = false;
                                data.ModifiedBy = userid;
                                data.ModifiedDate = DateTime.Now;
                                context.Entry(data).State = EntityState.Modified;
                            }
                        }
                        foreach (var day in ClusterHolidayDc.holiday)
                        {
                            ClusterHoliday cluster = new ClusterHoliday();
                            cluster.ClusterId = ClusterHolidayDc.clusterId;
                            //cluster.Holiday = data.holiday;
                            cluster.WarehouseId = ClusterHolidayDc.warehouseId;
                            //cluster.Year = ClusterHolidayDc.year;
                            cluster.IsDeleted = false;
                            cluster.IsActive = true;
                            cluster.CreatedDate = DateTime.Now;
                            cluster.CreatedBy = userid;
                            cluster.Holiday = day;
                            context.ClusterHolidays.Add(cluster);
                        }

                        if (context.Commit() > 0)
                        {
                            res.Status = true;
                            res.Message = "Data Successfully Saved ";
                        }
                        else
                        {
                            res.Status = false;
                            res.Message = "Data Not Saved ";
                        }
                    }
                    return res;
                }
            }
            catch (Exception ex)
            {
                return new APIResponse { Status = false, Message = ex.Message };
            };
        }

        [HttpGet]
        [Route("getClusterHoliday")]
        public getClusterHolidayListDC GetClusterHoliday(long warehouseid, long clusterId)
        {
            using (AuthContext context = new AuthContext())
            {
                getClusterHolidayListDC res = new getClusterHolidayListDC();
                CustomerHolidayDc CustomerHolidayDcs = new CustomerHolidayDc();
                var Day = context.ClusterHolidays.Where(x => x.IsActive == true && x.IsDeleted == false && x.WarehouseId == warehouseid && x.ClusterId == clusterId).Select(x => x.Holiday).ToList();
                res.holiday = Day;
                List<DateTime> AllHoliday = new List<DateTime>();
                var datee = DateTime.Now.Year;
                DateTime start = new DateTime(datee, 1, 1);
                DateTime end = start.AddYears(1).AddSeconds(-1);
                var allDate = Enumerable.Range(0, 1 + end.Subtract(start).Days)
                                .Select(offset =>
                                 start.AddDays(offset))
                           .ToList();
                allDate.ForEach(x =>
                {
                    if (Day.Contains(x.ToString("dddd")))
                    {
                        AllHoliday.Add(x);
                    }
                });
                res.holiday = Day;
                res.HolidayDate = AllHoliday;
                return res;
            }
        }

        [HttpPost]
        [Route("GetCustomer")]
        public List<CustomerHolidayListDC> Getcustomer(int warehouseid, int clusterid, List<string> SkcodeList)
        {
            List<CustomerHolidayListDC> result = new List<CustomerHolidayListDC>();
            using (var context = new AuthContext())
            {
                if (clusterid > 0)
                {
                    var sList = new DataTable();
                    sList.Columns.Add("stringValue");
                    foreach (var item in SkcodeList)
                    {
                        var dr = sList.NewRow();
                        dr["stringValue"] = item;
                        sList.Rows.Add(dr);
                    }
                    var skcodeList = new SqlParameter("skcodeList", sList);
                    skcodeList.SqlDbType = SqlDbType.Structured;
                    skcodeList.TypeName = "dbo.stringValues";

                    var param1 = new SqlParameter("@warehouseid", warehouseid);
                    var param2 = new SqlParameter("@clusterid", clusterid);

                    result = context.Database.SqlQuery<CustomerHolidayListDC>("exec GetCustomerHolidays @warehouseid, @clusterid,@skcodeList", param1, param2, skcodeList).ToList();
                }
            }
            return result;
        }


        [HttpPost]
        [Route("GetCustomerWithST")]
        public CustomerHolidayListTotalDC Getcustomer(GetCustomerDC GetCustomerDC)
        {
            CustomerHolidayListTotalDC res = new CustomerHolidayListTotalDC();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                if (GetCustomerDC.clusterid > 0)
                {

                    var sList = new DataTable();
                    sList.Columns.Add("stringValue");
                    foreach (var item in GetCustomerDC.SkcodeList)
                    {
                        var dr = sList.NewRow();
                        dr["stringValue"] = item;
                        sList.Rows.Add(dr);
                    }
                    var skcodeList = new SqlParameter("skcodeList", sList);
                    skcodeList.SqlDbType = SqlDbType.Structured;
                    skcodeList.TypeName = "dbo.stringValues";

                    var take = new SqlParameter("@take", GetCustomerDC.take);

                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetCustomerHolidaysWithSkipTake]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(skcodeList);
                    cmd.Parameters.Add(new SqlParameter("@warehouseid", GetCustomerDC.warehouseid));
                    cmd.Parameters.Add(new SqlParameter("@clusterid", GetCustomerDC.clusterid));
                    cmd.Parameters.Add(new SqlParameter("@skip", GetCustomerDC.skip));
                    cmd.Parameters.Add(new SqlParameter("@take", GetCustomerDC.take));
                    // Run the sproc
                    var reader = cmd.ExecuteReader();
                    var Data = ((IObjectContextAdapter)context).ObjectContext.Translate<CustomerHolidayListDC>(reader).ToList();
                    reader.NextResult();
                    res.CustomerHolidayList = Data;
                    if (reader.Read())
                    {
                        res.TotalRecords = Convert.ToInt32(reader["TotalRecords"]);
                    }
                }
            }
            return res;
        }

        [HttpPost]
        [Route("UpdateCustomerHoliday")]
        public APIResponse UpdateCustomerHoliday(UpdateCustomerHolidayDC updateCustomerHolidayDC)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                APIResponse res = new APIResponse();
                using (AuthContext context = new AuthContext())
                {
                    var customeridd = context.Customers.Where(x => x.Skcode == updateCustomerHolidayDC.skCode && x.Active == true && x.Deleted == false).Select(x => new { x.CustomerId, x.fcmId }).FirstOrDefault();
                    if (customeridd != null && customeridd.CustomerId > 0)
                    {
                        var ExistingData = context.CustomerHolidays.Where(x => x.ClusterId == updateCustomerHolidayDC.clusterId && x.CustomerId == customeridd.CustomerId && x.IsActive == true && x.IsDeleted == false).ToList();
                        if (ExistingData != null && ExistingData.Any())
                        {
                            foreach (var data in ExistingData)
                            {
                                data.ModifiedDate = DateTime.Now;
                                data.ModifiedBy = userid;
                                data.IsActive = false;
                                data.IsDeleted = true;
                            }
                        }
                        string Days = "";

                        foreach (var day in updateCustomerHolidayDC.holiday)
                        {
                            CustomerHoliday customer = new CustomerHoliday();
                            customer.ClusterId = updateCustomerHolidayDC.clusterId;
                            //cluster.Holiday = data.holiday;
                            customer.WarehouseId = updateCustomerHolidayDC.warehouseid;
                            //customer.Year = updateCustomerHolidayDC.year;
                            customer.IsDeleted = false;
                            customer.IsActive = true;
                            customer.CreatedDate = DateTime.Now;
                            customer.CreatedBy = userid;
                            customer.Holiday = day;
                            customer.CustomerId = customeridd.CustomerId;
                            context.CustomerHolidays.Add(customer);
                            Days += string.IsNullOrEmpty(Days) ? day : ("," + day);
                        }

                        if (context.Commit() > 0)
                        {
                            if (!string.IsNullOrEmpty(customeridd.fcmId))
                            {
                                NotificationUpdatedController notificationUpdatedController = new NotificationUpdatedController();
                                notificationUpdatedController.sendHolidayNotification(customeridd.CustomerId, customeridd.fcmId, Days);
                            }
                            res.Status = true;
                            res.Message = "Data Successfully Saved ";
                        }
                        else
                        {
                            res.Status = false;
                            res.Message = "Data Not Saved ";
                        }

                    }
                }
                return res;
            }
            catch (Exception ex)
            {
                return new APIResponse { Status = false, Message = ex.Message };
            };
        }

        [HttpPost]
        [Route("HolidayListUploder")]
        public IHttpActionResult UploadFile(long warehouseid, long clusterid)
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
                        return ReadCustomerHolidayUploadedFile(hssfwb, userid, warehouseid, clusterid);
                    }
                    else
                    {
                        return Created("File extnsion required .xlsx", "File extnsion required .xlsx");
                    }
                }
            }

            return Created("Error", "Error");
        }

        public IHttpActionResult ReadCustomerHolidayUploadedFile(XSSFWorkbook hssfwb, int userid, long warehouseid, long clusterid)
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
                    int? txnIdCellIndex = null;
                    int? GroupCodeCellIndex = null;
                    int? ClientCodeCellIndex = null;
                    int? AmountCellIndex = null;
                    List<string> headerlst = new List<string>();

                    List<customerHolidayUploadDC> listdata = new List<customerHolidayUploadDC>();
                    List<customerHolidayUploadDC> trnfrorders = new List<customerHolidayUploadDC>();
                    for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                    {
                        if (iRowIdx == 0)
                        {
                            rowData = sheet.GetRow(iRowIdx);
                            if (rowData != null)
                            {
                                foreach (var item in rowData.Cells)
                                {
                                    headerlst.Add(item.ToString());
                                }
                                GroupCodeCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Skcode") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Skcode").ColumnIndex : (int?)null;
                                if (!GroupCodeCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Skcode does not exist..try again");
                                    return Created(strJSON, strJSON); ;
                                }//changes
                                ClientCodeCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Holiday") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Holiday").ColumnIndex : (int?)null;
                                if (!ClientCodeCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Holiday does not exist..try again");
                                    return Created(strJSON, strJSON); ;
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

                                customerHolidayUploadDC trnfrorder = new customerHolidayUploadDC();
                                string Holiday = string.Empty;
                                string Skcode = string.Empty;
                                try
                                {
                                    List<string> holiday11 = new List<string>();
                                    foreach (var cellDatas in rowData.Cells)
                                    {
                                        trnfrorder = new customerHolidayUploadDC();

                                        if (cellDatas.ColumnIndex == 0)
                                        {
                                            Skcode = cellDatas.ToString();
                                            trnfrorder.skcode = Skcode;
                                            trnfrorder.holiday = Holiday;
                                        }
                                        else if (cellDatas.ColumnIndex == 1)
                                        {

                                            Holiday = cellDatas.ToString();
                                            trnfrorder.skcode = Skcode;
                                            trnfrorder.holiday = Holiday;
                                        }

                                    }
                                    listdata.Add(trnfrorder);
                                }
                                catch (Exception ex)
                                {
                                    Msg = ex.Message;
                                    //logger.Error("Error VAN Paymant Upload File " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                                }
                            }
                        }
                    }
                    if (listdata != null && listdata.Any())
                    {
                        foreach (var dataa in listdata)
                        {
                            var skcodeList = dataa.skcode;
                            //var CustHolidays = listdata.Select(x => x.holiday).ToList();
                            var customerid = context.Customers.Where(x => skcodeList.Contains(x.Skcode) && x.Active == true && x.Deleted == false).Select(x => x.CustomerId).ToList();
                            var CustHolidays = dataa.holiday.Split(',').Distinct().ToList();
                            foreach (var idd in customerid)
                            {
                                var existdata = context.CustomerHolidays.Where(x => x.ClusterId == clusterid && x.WarehouseId == warehouseid && x.CustomerId == idd && x.IsActive == true && x.IsDeleted == false).ToList();//x.Year == DateTime.Now.Year &&
                                if (existdata != null && existdata.Any())
                                {
                                    foreach (var data in existdata)
                                    {
                                        data.IsDeleted = true;
                                        data.IsActive = false;
                                        data.ModifiedBy = userid;
                                        data.ModifiedDate = DateTime.Now;
                                        context.Entry(data).State = EntityState.Modified;
                                    }
                                }
                                foreach (string day in CustHolidays)
                                {
                                    CustomerHoliday customer = new CustomerHoliday();
                                    customer.ClusterId = clusterid;
                                    //cluster.Holiday = data.holiday;
                                    customer.WarehouseId = warehouseid;
                                    //customer.Year = DateTime.Now.Year;
                                    customer.IsDeleted = false;
                                    customer.IsActive = true;
                                    customer.CreatedDate = DateTime.Now;
                                    customer.CreatedBy = userid;
                                    customer.Holiday = day;
                                    customer.CustomerId = idd;
                                    context.CustomerHolidays.Add(customer);
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



        [HttpPost]
        [Route("PackageMaterialCostUploder")]
        public IHttpActionResult UploadFile()
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
                        return ReadPackageMaterialCostUploadedFile(hssfwb, userid);
                    }
                    else
                    {
                        return Created("File extnsion required .xlsx", "File extnsion required .xlsx");
                    }
                }
            }

            return Created("Error", "Error");
        }


        public IHttpActionResult ReadPackageMaterialCostUploadedFile(XSSFWorkbook hssfwb, int userid)
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

                    List<PackageMaterialCost> listdata = new List<PackageMaterialCost>();
                    for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                    {
                        if (iRowIdx == 0)
                        {
                            rowData = sheet.GetRow(iRowIdx);
                            if (rowData != null)
                            {
                                string Validatedheader = ValidateHeader(rowData);
                                if (Validatedheader != null)
                                {
                                    return Ok(Validatedheader);
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

                                PackageMaterialCost trnfrorder = new PackageMaterialCost();

                                try
                                {
                                    string col = null;
                                    col = string.Empty;
                                    cellData = rowData.GetCell(0);
                                    col = cellData == null ? "" : cellData.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.MRPId = Convert.ToInt32(col);
                                    }
                                    col = string.Empty;
                                    cellData = rowData.GetCell(1);
                                    var cellDataa = cellData.ToString();
                                    var wid = context.Warehouses.Where(x => x.WarehouseName == cellDataa).Select(x => x.WarehouseId).FirstOrDefault();
                                    col = wid.ToString() == null ? "" : wid.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.WarehouseId = Convert.ToInt32(col);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(2);
                                    col = cellData == null ? "" : cellData.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.Month = Convert.ToInt32(col);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(3);
                                    col = cellData == null ? "" : cellData.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.Year = Convert.ToInt32(col);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(4);
                                    col = cellData == null ? "" : cellData.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.PerPieceCost = Convert.ToDouble(col);
                                    }

                                    listdata.Add(trnfrorder);
                                }
                                catch (Exception ex)
                                {
                                    Msg = ex.Message;
                                    //logger.Error("Error VAN Paymant Upload File " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                                }
                            }
                        }
                    }
                    if (listdata != null && listdata.Any())
                    {
                        foreach (var dataa in listdata)
                        {
                            //var skcodeList = dataa.skcode;
                            //var MRPIDs = listdata.Select(x => x.MRPId).ToList();
                            //var customerid = context.Customers.Where(x => skcodeList.Contains(x.Skcode) && x.Active == true && x.Deleted == false).Select(x => x.CustomerId).ToList();
                            //var CustHolidays = dataa.holiday.Split(',').Distinct().ToList();

                            var existdata = context.PackageMaterialCosts.Where(x => x.MRPId == dataa.MRPId && x.WarehouseId == dataa.WarehouseId && x.Month == dataa.Month && x.Year == dataa.Year && x.IsActive == true && x.IsDeleted == false).ToList();

                            if (existdata != null && existdata.Any())
                            {
                                foreach (var data in existdata)
                                {
                                    data.PerPieceCost = dataa.PerPieceCost;
                                    data.IsDeleted = false;
                                    data.IsActive = true;
                                    data.ModifiedBy = userid;
                                    data.ModifiedDate = DateTime.Now;
                                    context.Entry(data).State = EntityState.Modified;
                                }
                            }
                            else
                            {
                                PackageMaterialCost PMC = new PackageMaterialCost();
                                PMC.MRPId = dataa.MRPId;
                                PMC.WarehouseId = dataa.WarehouseId;
                                PMC.Month = dataa.Month;
                                PMC.Year = dataa.Year;
                                PMC.PerPieceCost = dataa.PerPieceCost;
                                PMC.CreatedBy = userid;
                                PMC.CreatedDate = DateTime.Now;
                                PMC.IsActive = true;
                                PMC.IsDeleted = false;
                                context.PackageMaterialCosts.Add(PMC);
                            }
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

        private string ValidateHeader(IRow rowData)
        {
            string strJSON = null;
            string field = string.Empty;
            field = rowData.GetCell(0).ToString();
            if (field != "MRPID")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(1).ToString();
            if (field != "WarehouseName")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(2).ToString();
            if (field != "Month")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(3).ToString();
            if (field != "Year")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(4).ToString();
            if (field != "PerPieceCost")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            return strJSON;
        }


        [HttpPost]
        [Route("GetPackageMaterialCost")]
        public PkgMatCostListTC GetPackageMaterialCost(PkgMatCostDC obj)
        {
            using (AuthContext context = new AuthContext())
            {
                PkgMatCostListTC result = new PkgMatCostListTC();

                if (obj.warehouseid.Count > 0)
                {

                    var sList = new DataTable();
                    sList.Columns.Add("intValue");
                    foreach (var item in obj.warehouseid)
                    {
                        var dr = sList.NewRow();
                        dr["intValue"] = item;
                        sList.Rows.Add(dr);
                    }
                    var wList = new SqlParameter("warehouseid", sList);
                    wList.SqlDbType = SqlDbType.Structured;
                    wList.TypeName = "dbo.intValues";

                    var param1 = new SqlParameter("@month", obj.month);
                    var param2 = new SqlParameter("@year", obj.year);
                    var param3 = new SqlParameter("@skip", obj.skip);
                    var param4 = new SqlParameter("@take", obj.take);

                    result.PkgMatCostList = context.Database.SqlQuery<PkgMatCostList>("exec PkgMaterialCost @warehouseid, @month,@year, @skip,@take", wList, param1, param2, param3, param4).ToList();
                    if (result.PkgMatCostList.Any())
                    {
                        result.totalcount = result.PkgMatCostList.FirstOrDefault().totalcount;
                    }
                }
                else
                {
                    return null;
                }
                return result;
            }
        }


        [HttpPost]
        [Route("SkpKppCommissionUploder")]
        public IHttpActionResult UploadFilee()
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
                        return ReadSkpKppComUploadedFile(hssfwb, userid);
                    }
                    else
                    {
                        return Created("File extnsion required .xlsx", "File extnsion required .xlsx");
                    }
                }
            }

            return Created("Error", "Error");
        }

        public IHttpActionResult ReadSkpKppComUploadedFile(XSSFWorkbook hssfwb, int userid)
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

                    List<SKPKPPCommision> listdata = new List<SKPKPPCommision>();
                    for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                    {
                        if (iRowIdx == 0)
                        {
                            rowData = sheet.GetRow(iRowIdx);
                            if (rowData != null)
                            {
                                string Validatedheader = ValidateHeaders(rowData);
                                if (Validatedheader != null)
                                {
                                    return Ok(Validatedheader);
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

                                SKPKPPCommision trnfrorder = new SKPKPPCommision();

                                try
                                {
                                    string col = null;
                                    col = string.Empty;
                                    cellData = rowData.GetCell(0);
                                    var cellDataa = cellData.ToString();
                                    var sid = context.StoreDB.Where(x => x.Name == cellDataa).Select(x => x.Id).FirstOrDefault();
                                    col = sid == null ? "" : sid.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.StoreId = Convert.ToInt32(col);
                                    }
                                    col = string.Empty;
                                    cellData = rowData.GetCell(1);
                                    var cellDataaa = cellData.ToString();
                                    var wid = context.Warehouses.Where(x => x.WarehouseName == cellDataaa).Select(x => x.WarehouseId).FirstOrDefault();
                                    col = wid.ToString() == null ? "" : wid.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.WarehouseId = Convert.ToInt32(col);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(2);
                                    col = cellData == null ? "" : cellData.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.Type = col.ToUpper();
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(3);
                                    col = cellData == null ? "" : cellData.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.Month = Convert.ToInt32(col);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(4);
                                    col = cellData == null ? "" : cellData.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.Year = Convert.ToInt32(col);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(5);
                                    col = cellData == null ? "" : cellData.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.Commission = Convert.ToDouble(col);
                                    }

                                    listdata.Add(trnfrorder);
                                }
                                catch (Exception ex)
                                {
                                    Msg = ex.Message;
                                    //logger.Error("Error VAN Paymant Upload File " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                                }
                            }
                        }
                    }
                    if (listdata != null && listdata.Any())
                    {
                        foreach (var dataa in listdata)
                        {
                            //var skcodeList = dataa.skcode;
                            //var MRPIDs = listdata.Select(x => x.MRPId).ToList();
                            //var customerid = context.Customers.Where(x => skcodeList.Contains(x.Skcode) && x.Active == true && x.Deleted == false).Select(x => x.CustomerId).ToList();
                            //var CustHolidays = dataa.holiday.Split(',').Distinct().ToList();

                            var existdata = context.SKPKPPCommisions.Where(x => x.StoreId == dataa.StoreId && x.WarehouseId == dataa.WarehouseId && x.Type == dataa.Type && x.Month == dataa.Month && x.Year == dataa.Year && x.IsActive == true && x.IsDeleted == false).ToList();

                            if (existdata != null && existdata.Any())
                            {
                                foreach (var data in existdata)
                                {
                                    data.Commission = dataa.Commission;
                                    data.IsDeleted = false;
                                    data.IsActive = true;
                                    data.ModifiedBy = userid;
                                    data.ModifiedDate = DateTime.Now;
                                    context.Entry(data).State = EntityState.Modified;
                                }
                            }
                            else
                            {
                                SKPKPPCommision SKP = new SKPKPPCommision();
                                SKP.StoreId = dataa.StoreId;
                                SKP.WarehouseId = dataa.WarehouseId;
                                SKP.Month = dataa.Month;
                                SKP.Year = dataa.Year;
                                SKP.Type = dataa.Type;
                                SKP.Commission = dataa.Commission;
                                SKP.CreatedBy = userid;
                                SKP.CreatedDate = DateTime.Now;
                                SKP.IsActive = true;
                                SKP.IsDeleted = false;
                                context.SKPKPPCommisions.Add(SKP);
                            }
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

        private string ValidateHeaders(IRow rowData)
        {
            string strJSON = null;
            string field = string.Empty;
            field = rowData.GetCell(0).ToString();
            if (field != "StoreName")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(1).ToString();
            if (field != "WarehouseName")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(2).ToString();
            if (field != "Type")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(3).ToString();
            if (field != "Month")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(4).ToString();
            if (field != "Year")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(5).ToString();
            if (field != "Commission")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            return strJSON;
        }


        [HttpPost]
        [Route("GetGetSkpKppCommission")]
        public SkpKppListTC GetSkpKppCom(SkpKppComDC obj)
        {
            using (AuthContext context = new AuthContext())
            {
                SkpKppListTC result = new SkpKppListTC();
                //List<SkpKppList> SkpKppLists = new List<SkpKppList>();

                if (obj.warehouseid.Count > 0)
                {
                    var sList = new DataTable();
                    sList.Columns.Add("IntValue");
                    foreach (var item in obj.warehouseid)
                    {
                        var dr = sList.NewRow();
                        dr["IntValue"] = item;
                        sList.Rows.Add(dr);
                    }
                    var wList = new SqlParameter("warehouseid", sList);
                    wList.SqlDbType = SqlDbType.Structured;
                    wList.TypeName = "dbo.IntValues";

                    var stList = new DataTable();
                    stList.Columns.Add("IntValue");
                    foreach (var item in obj.storeid)
                    {
                        var dr = stList.NewRow();
                        dr["IntValue"] = item;
                        stList.Rows.Add(dr);
                    }
                    var storeList = new SqlParameter("storeid", stList);
                    storeList.SqlDbType = SqlDbType.Structured;
                    storeList.TypeName = "dbo.IntValues";

                    var param1 = new SqlParameter("@month", obj.month);
                    var param2 = new SqlParameter("@year", obj.year);
                    var param3 = new SqlParameter("@skip", obj.skip);
                    var param4 = new SqlParameter("@take", obj.take);

                    result.SkpKppList = context.Database.SqlQuery<SkpKppList>("exec SKPKPPCommisionList @warehouseid, @storeid, @month,@year,@skip,@take", wList, storeList, param1, param2, param3, param4).ToList();
                    if (result.SkpKppList.Any())
                    {
                        result.totalcount = result.SkpKppList.FirstOrDefault().totalcount;
                    }
                }
                else
                {
                    return null;
                }
                return result;
            }
        }

        
        [HttpGet]
        [Route("Getaccountdetailfetch")]
        public accountdetailfetch accountdetailfetchh()
        {
            accountdetailfetch res = new accountdetailfetch();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                string query = "select * from AccountTallyLadgers with(nolock)";
                var Data = context.Database.SqlQuery<TallyLadgers>(query).ToList();
                res.TallyLadgers = Data;

                string query2 = "select * from AccountDepartments with(nolock)";
                var Data2 = context.Database.SqlQuery<Departments>(query2).ToList();
                res.Department = Data2;

                string query3 = "select * from AccountExpenseMISHeads with(nolock)";
                var Data3 = context.Database.SqlQuery<ExpenseMISHeads>(query3).ToList();
                res.ExpenseMISHeads = Data3;

                string query4 = "select * from AccountFinancialHeads with(nolock)";
                var Data4 = context.Database.SqlQuery<FinancialHeads>(query4).ToList();
                res.FinancialHeads = Data4;

                string query5 = "select * from AccountCostMISHeads with(nolock)";
                var Data5 = context.Database.SqlQuery<CostMISHeads>(query5).ToList();
                res.CostMISHeads = Data5;

                string query6 = "select * from AccountVerticals with(nolock)";
                var Data6 = context.Database.SqlQuery<Verticals>(query6).ToList();
                res.Verticals = Data6;

                string query7 = "select * from AccountMISHeads with(nolock)";
                var Data7 = context.Database.SqlQuery<MISHead>(query7).ToList();
                res.MISHeads = Data7;

                //string query8 = "select * from AccountCM5Heads with(nolock)";
                //var Data8 = context.Database.SqlQuery<AccountCM5Heads>(query8).ToList();
                //res.AccountCM5Head = Data8;

            }
            return res;
        }

        [HttpPost]
        [Route("GetAccountMISData")]
        public List<dynamic> AccountMISData(AccountMISDC obj)
        {
            using (AuthContext context = new AuthContext())
            {
                List<AccountMISDataList> AccountMISDataLists = new List<AccountMISDataList>();

                if (obj.WarehouseId.Count > 0)
                {
                    var sList = new DataTable();
                    sList.Columns.Add("IntValue");
                    foreach (var item in obj.WarehouseId)
                    {
                        var dr = sList.NewRow();
                        dr["IntValue"] = item;
                        sList.Rows.Add(dr);
                    }
                    var wList = new SqlParameter("@WarehouseIds", sList);
                    wList.SqlDbType = SqlDbType.Structured;
                    wList.TypeName = "dbo.IntValues";

                    var param1 = new SqlParameter("@StartDate", obj.StartDate);
                    var param2 = new SqlParameter("@EndDate", obj.EndDate);

                    var abc = context.Database.SqlQuery<dynamic>("exec GetAccountMISData @StartDate, @EndDate, @WarehouseIds", param1, param2, wList).ToList();
                    return abc;
                }
                else
                {
                    return null;
                }
            }
        }


        [HttpPost]
        [Route("AccountMISUploder")]
        public IHttpActionResult UploadFilees()
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
                        return ReadAccountMISUplodFile(hssfwb, userid);
                    }
                    else
                    {
                        return Created("File extnsion required .xlsx", "File extnsion required .xlsx");
                    }
                }
            }

            return Created("Error", "Error");
        }

        public IHttpActionResult ReadAccountMISUplodFile(XSSFWorkbook hssfwb, int userid)
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
                   var AccountVerticals= context.AccountVerticals.ToList();
                   var AccountCanvasHeads= context.AccountCanvasHeads.ToList();
                   var AccountExpenseMISHeads =context.AccountExpenseMISHeads.ToList();
                   var AccountMISHeads = context.AccountMISHeads.ToList();
                   var AccountCostMISHeads = context.AccountCostMISHeads.ToList();
                   var AccountFinancialHeads = context.AccountFinancialHeads.ToList();

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
                                    return Ok(Validatedheader);
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
                                    var cellDataa = cellData.ToString();
                                    var sid = AccountTallyLadgers.Where(x => x.Name == cellDataa).Select(x => x.Id).FirstOrDefault();
                                    col = sid == null ? "" : sid.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.AccountTallyLadgerId = Convert.ToInt32(col);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(1);
                                    var cellDataaa = cellData.ToString();
                                    var wid = Warehouses.Where(x => x.WarehouseName == cellDataaa).Select(x => x.WarehouseId).FirstOrDefault();
                                    col = wid.ToString() == null ? "" : wid.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.WarehouseId = Convert.ToInt32(col);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(2);
                                    var cellData2 = cellData.ToString();
                                    var dept = AccountDepartments.Where(x => x.Name == cellData2).Select(x => x.Id).FirstOrDefault();
                                    col = dept == null ? "" : dept.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.AccountDepartmentId = Convert.ToInt32(col);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(3);
                                    var cellData3 = cellData.ToString();
                                    var vertical = AccountVerticals.Where(x => x.Name == cellData3).Select(x => x.Id).FirstOrDefault();
                                    col = vertical == null ? "" : vertical.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.AccountVerticalId = Convert.ToInt32(col);
                                    }

                                    //col = string.Empty;
                                    //cellData = rowData.GetCell(4);
                                    //var cellData4 = cellData.ToString();
                                    //var cm = context.AccountCM5Heads.Where(x => x.Name == cellData4).Select(x => x.Id).FirstOrDefault();
                                    //col = cm == null ? "" : cm.ToString();
                                    //if (col != "")
                                    //{
                                    //    trnfrorder.AccountCM5HeadId = Convert.ToInt32(col);
                                    //}

                                    col = string.Empty;
                                    cellData = rowData.GetCell(4);
                                    var cellData4 = cellData.ToString();
                                    var cm = AccountCanvasHeads.Where(x => x.Name == cellData4).Select(x => x.Id).FirstOrDefault();
                                    col = cm == null ? "" : cm.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.AccountCM5HeadId = Convert.ToInt32(col);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(5);
                                    var cellData5 = cellData.ToString();
                                    var canvashead = AccountCanvasHeads.Where(x => x.Name == cellData3).Select(x => x.Id).FirstOrDefault();
                                    col = canvashead == null ? "" : canvashead.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.AccountCanvasHeadId = Convert.ToInt32(col);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(6);
                                    var cellData6 = cellData.ToString();
                                    var ExMisHead = AccountExpenseMISHeads.Where(x => x.Name == cellData6).Select(x => x.Id).FirstOrDefault();
                                    col = ExMisHead == null ? "" : ExMisHead.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.AccountExpenseMISHeadId = Convert.ToInt32(col);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(7);
                                    var cellData7 = cellData.ToString();
                                    var MisHead = AccountMISHeads.Where(x => x.Name == cellData7).Select(x => x.Id).FirstOrDefault();
                                    col = MisHead == null ? "" : MisHead.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.AccountMISHeadId = Convert.ToInt32(col);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(8);
                                    var cellData8 = cellData.ToString();
                                    var CostMisHead = AccountCostMISHeads.Where(x => x.Name == cellData8).Select(x => x.Id).FirstOrDefault();
                                    col = CostMisHead == null ? "" : CostMisHead.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.AccountCostMISHeadId = Convert.ToInt32(col);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(9);
                                    var cellData9 = cellData.ToString();
                                    var FinHead = AccountFinancialHeads.Where(x => x.Name == cellData9).Select(x => x.Id).FirstOrDefault();
                                    col = FinHead == null ? "" : FinHead.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.AccountFinancialHeadId = Convert.ToInt32(col);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(10);
                                    var cellData10 = cellData.ToString();
                                    col = cellData10 == null && cellData10.Trim() !="-" ? "" : cellData10.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.Amount = Convert.ToDouble(col);
                                    }

                                    col = string.Empty;
                                    cellData = rowData.GetCell(11);
                                    var cellData11 = cellData.ToString();
                                    col = cellData11 == null ? "" : cellData11.ToString();
                                    if (col != "")
                                    {
                                        trnfrorder.MonthDate = Convert.ToDateTime(col);
                                    }

                                    listdata.Add(trnfrorder);
                                }
                                catch (Exception ex)
                                {
                                    Msg = ex.Message;
                                }
                            }
                        }

                        if (listdata != null && listdata.Any())
                        {
                            //foreach (var dataa in listdata)
                            //{
                            //    var existdata = context.AccountMISDataUploads.Where(x => EntityFunctions.TruncateTime(x.MonthDate) == dataa.MonthDate).ToList();

                            //    if (existdata != null && existdata.Any())
                            //    {
                            //        foreach (var data in existdata)
                            //        {
                            //            data.AccountDepartmentId = dataa.AccountDepartmentId,
                            //            data.AccountCanvasHeadId = dataa.AccountCanvasHeadId,
                            //            data.AccountExpenseMISHeadId = dataa.AccountExpenseMISHeadId,
                            //            data.AccountTallyLadgerId = dataa.AccountTallyLadgerId,
                            //            data.AccountFinancialHeadId = dataa.AccountFinancialHeadId,
                            //            data.AccountCostMISHeadId = dataa.AccountCostMISHeadId,
                            //            data.AccountVerticalId = dataa.AccountVerticalId,
                            //            data.AccountMISHeadId = dataa.AccountMISHeadId,
                            //            data.Amount = dataa.Amount,
                            //            data.MonthDate = dataa.MonthDate;
                            //            context.Entry(data).State = EntityState.Modified;
                            //        }
                            //    }
                            //    else
                            //    {
                            //        UploadEntity SKP = new UploadEntity();
                            //        data.AccountDepartmentId = dataa.AccountDepartmentId,
                            //        data.AccountCanvasHeadId = dataa.AccountCanvasHeadId,
                            //        data.AccountExpenseMISHeadId = dataa.AccountExpenseMISHeadId,
                            //        data.AccountTallyLadgerId = dataa.AccountTallyLadgerId,
                            //        data.AccountFinancialHeadId = dataa.AccountFinancialHeadId,
                            //        data.AccountCostMISHeadId = dataa.AccountCostMISHeadId,
                            //        data.AccountVerticalId = dataa.AccountVerticalId,
                            //        data.AccountMISHeadId = dataa.AccountMISHeadId,
                            //        data.Amount = dataa.Amount,
                            //        data.MonthDate = dataa.MonthDate,
                            //        context.AccountMISDataUploads.Add(SKP);
                            //    }
                            //}
                            //if (context.Commit() > 0)
                            //{
                            //    Msg = "record uploaded successfully!!";
                            //}
                            //else
                            //{
                            //    Msg = "record not uploaded !!";
                            //}
                        }
                        return Created(Msg, Msg);
                    }
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
                strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(1).ToString();
            if (field != "Warehouse")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(2).ToString();
            if (field != "Department")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(3).ToString();
            if (field != "Vertical")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(4).ToString();
            if (field != "CM5 head")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(5).ToString();
            if (field != "Canvas Head")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(6).ToString();
            if (field != "Expense MIS Head")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(7).ToString();
            if (field != "MIS Head")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(8).ToString();
            if (field != "Cost MIS Head")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(9).ToString();
            if (field != "Financial Head")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(10).ToString();
            if (field != "Amount")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            return strJSON;
        }


        public class AccountMISDC
        {
            public List<int> WarehouseId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        public class AccountMISDataList
        {
            public string CityName { get; set; }
            public string WarehouseName { get; set; }
            public string Particulars { get; set; }
        }
        public class accountdetailfetch
        {
            public List<TallyLadgers> TallyLadgers { get; set; }
            public List<CanvasHead> CanvasHead { get; set; }
            public List<CostMISHeads> CostMISHeads { get; set; }
            public List<ExpenseMISHeads> ExpenseMISHeads { get; set; }
            public List<FinancialHeads> FinancialHeads { get; set; }
            public List<MISHead> MISHeads { get; set; }
            public List<Verticals> Verticals { get; set; }
            public List<Departments> Department { get; set; }
            public List<AccountCM5Heads> AccountCM5Head { get; set; }
        }

        public class UploadEntity
        {
            public int AccountTallyLadgerId { get; set; }
            public int AccountCanvasHeadId { get; set; }
            public int AccountDepartmentId { get; set; }
            public int AccountExpenseMISHeadId { get; set; }
            public int AccountFinancialHeadId { get; set; }
            public int AccountCostMISHeadId { get; set; }
            public int AccountVerticalId { get; set; }
            public int AccountMISHeadId { get; set; }
            public int AccountCM5HeadId { get; set; }
            public double Amount { get; set; }
            public int WarehouseId{ get; set; }
            public DateTime MonthDate{ get; set; }
        }

        public class FinancialHeads
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }
        public class AccountCM5Heads
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }
        public class CostMISHeads
        {
            public long Id { get; set; }
            public string CostMISHeadName { get; set; }
        }
        public class MISHead
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }
        public class ExpenseMISHeads
        {
            public long Id { get; set; }
            public string ExpenseMISHead { get; set; }
        }
        public class Departments
        {
            public long Id { get; set; }
            public string DepartmentName { get; set; }
        }
        public class CanvasHead
        {
            public long Id { get; set; }
            public string CanvasHeadName { get; set; }
        }
        public class TallyLadgers
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }
        public class Verticals
        {
            public long Id { get; set; }
            public string Vertical { get; set; }
        }
        public class PkgMatCostDC
        {
            public List<int> warehouseid { get; set; }
            public int month { get; set; }
            public int year { get; set; }
            public int skip{ get; set; }
            public int take{ get; set; }
        }
        public class SkpKppComDC
        {
            public List<int> warehouseid { get; set; }
            public List<long> storeid { get; set; }
            public int month { get; set; }
            public int year { get; set; }
            public int skip{ get; set; }
            public int take{ get; set; }
        }
        public class PkgMatCostList
        {
            public string WarehouseName { get; set; }
            public int MRPId { get; set; }
            public int Month { get; set; }
            public int Year { get; set; }
            public double PerPieceCost { get; set; }
            public int totalcount { get; set; }
        }
        public class PkgMatCostListTC
        {
            public List<PkgMatCostList> PkgMatCostList { get; set; }
            public int totalcount { get; set; }
        }
        public class SkpKppList
        {
            public string WarehouseName { get; set; }
            public string StoreName { get; set; }
            public string Type { get; set; }
            public int Month { get; set; }
            public int Year { get; set; }
            public double Commission { get; set; }
            public int totalcount { get; set; }

        }
        public class SkpKppListTC
        {
            public List<SkpKppList> SkpKppList { get; set; }
            public int totalcount { get; set; }
        }
    }


}
