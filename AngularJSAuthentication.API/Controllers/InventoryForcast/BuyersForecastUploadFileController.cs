using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.ForCast;
using AngularJSAuthentication.Model.Forecasting;
using NLog;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace AngularJSAuthentication.API.Controllers.InventoryForcast
{

    [RoutePrefix("api/BuyerForecastExcelFile")]

    public class BuyersForecastUploadFileController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();//Api for Upload Sheet on Warehouse to warehouse transfer
        string strJSON = null;
        string col0, col1, col2, col3, col4, col5, col6, col7, col8, col9, col10, col11, col12, col13, col14, col15,
            col16, col17, col18, col19, col20, col21, col22, col23, col24, col25;
        string txnId_col0, txnSubsubcategoryName_col1, txnPercentValue_col9, txnBuyerEditForecastQty_col11, txnInventoryDays_col17;
        string Warehouse_col1, ValueInAmt_col6;

        [Route("GetDownloadeSampleFile")]
        [HttpGet]
        public DataTable GetDownloadeSampleFiles()
        {
            using (var myContext = new AuthContext())
            {
                //var data = myContext.Database.SqlQuery<dynamic>("EXEC sp_DownloadeSampleFile").FirstOrDefault();
                //return data;

                string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                DataTable DT = new DataTable();
                using (var connection = new SqlConnection(connectionString))
                {
                     using (var command = new SqlCommand("EXEC sp_DownloadeSampleFile", connection))
                    //using (var command = new SqlCommand("EXEC sp_DownloadeSampleFileEnhancement", connection)) 
                    
                    {

                        if (connection.State != ConnectionState.Open)
                            connection.Open();

                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(DT);
                        da.Dispose();
                        connection.Close();
                        return DT;
                    }
                }

            }
        }
        [Route("GetDownloadeBrandSummaryFile")]
        [HttpGet]
        public List<DownloadeBrandSummaryFileDC> GetDownloadeBrandSummaryFIle(string UploadbyID)
        {
            using (var context = new AuthContext())
            {
                List<DownloadeBrandSummaryFileDC> BuyerData = new List<DownloadeBrandSummaryFileDC>();
                var Keytype = new SqlParameter("@UploadId", UploadbyID);
                BuyerData = context.Database.SqlQuery<DownloadeBrandSummaryFileDC>("EXEC sp_GetDownloadeBrandSummary @UploadId", Keytype).ToList();
                if (BuyerData != null && BuyerData.Any())
                {
                    return BuyerData;
                }
                return BuyerData;
            }
        }
        [HttpPost]
        [Route("BuyersBrandSummaryUploadFile")]
        public IHttpActionResult BrandSummaryUploadFile()
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
                    return ReadBrandSummaryUploadFile(hssfwb, userid);
                }
            }

            return Created("Error", "Error");
        }


        public IHttpActionResult ReadBrandSummaryUploadFile(XSSFWorkbook hssfwb, int userid)
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
                    List<DownloadeBrandSummaryFileDC> BuyersForecastUploader = new List<DownloadeBrandSummaryFileDC>();
                    int? IdCellIndex = null;
                    int? WarehouseCellIndex = null;
                    int? DepartmentCellIndex = null;
                    int? CategoryCellIndex = null;
                    int? SubCategoryCellIndex = null;
                    int? BrandNameCellIndex = null;
                    int? ValueInAmtCellIndex = null;

                    List<string> headerlst = new List<string>();
                    //List<BuyerForecastUploderDetail> Addlist = new List<BuyerForecastUploderDetail>();
                    List<DownloadeBrandSummaryFileDC> Exceltrnfrorders = new List<DownloadeBrandSummaryFileDC>();
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
                                IdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Id") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Id").ColumnIndex : (int?)null;
                                if (!IdCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Id does not exist..try again");
                                    return Created(strJSON, strJSON); ;
                                }

                                WarehouseCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "WarehouseName") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "WarehouseName").ColumnIndex : (int?)null;
                                if (!WarehouseCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("WarehouseName does not exist..try again");
                                    return Created(strJSON, strJSON); ;
                                }

                                DepartmentCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Department") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Department").ColumnIndex : (int?)null;
                                if (!DepartmentCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Department does not exist..try again");
                                    return Created(strJSON, strJSON); ;
                                }

                                CategoryCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Category") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Category").ColumnIndex : (int?)null;
                                if (!CategoryCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Category does not exist..try again");
                                    return Created(strJSON, strJSON);
                                }

                                SubCategoryCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "SubCategory") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "SubCategory").ColumnIndex : (int?)null;
                                if (!SubCategoryCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("SubCategory does not exist..try again");
                                    return Created(strJSON, strJSON);
                                }

                                BrandNameCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "BrandName") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "BrandName").ColumnIndex : (int?)null;
                                if (!BrandNameCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("BrandName does not exist..try again");
                                    return Created(strJSON, strJSON);
                                }

                                ValueInAmtCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "ValueInAmt") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "ValueInAmt").ColumnIndex : (int?)null;
                                if (!ValueInAmtCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("ValueInAmt does not exist..try again");
                                    return Created(strJSON, strJSON);
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

                                DownloadeBrandSummaryFileDC trnfrorder = new DownloadeBrandSummaryFileDC();

                                try
                                {
                                    cellData = rowData.GetCell(IdCellIndex.Value);
                                    txnId_col0 = cellData == null ? "" : cellData.ToString();
                                    trnfrorder.Id = Convert.ToInt32(txnId_col0);
                                    logger.Info("ID :" + trnfrorder.Id);


                                    cellData = rowData.GetCell(WarehouseCellIndex.Value);
                                    Warehouse_col1 = cellData == null ? "" : cellData.ToString();
                                    trnfrorder.WarehouseName = Warehouse_col1;
                                    logger.Info("ID :" + trnfrorder.WarehouseName);


                                    cellData = rowData.GetCell(ValueInAmtCellIndex.Value);
                                    ValueInAmt_col6 = cellData == null ? "" : cellData.ToString();
                                    trnfrorder.ValueInAmt = Convert.ToDecimal(ValueInAmt_col6);
                                    logger.Info("PercentValue :" + trnfrorder.ValueInAmt);


                                    trnfrorder.userid = userid;
                                    Exceltrnfrorders.Add(trnfrorder);

                                }
                                catch (Exception ex)
                                {
                                    //msgitemname = ex.Message;
                                    //logger.Error("Error VAN Paymant Upload File " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                                }
                            }
                        }
                    }
                    ItemForeCastUpdateResponse itemForeCastUpdateResponse = new ItemForeCastUpdateResponse();
                    if (Exceltrnfrorders != null && Exceltrnfrorders.Any())
                    {
                        var ValidationExcel = Exceltrnfrorders.GroupBy(x => x.WarehouseName).Select(x => new BrandSummaryValueInAmtDc
                        {
                            WarehouseName = x.Key,
                            ValueInAmt = x.Sum(y => y.ValueInAmt)
                        }).ToList();
                        var Ids = Exceltrnfrorders.Select(x => x.Id).Distinct().ToList();
                        var BrandForecastDetail = context.BrandForecastDetailDb.Where(x => Ids.Contains(x.Id)).ToList();
                        var HOPData = context.Database.SqlQuery<HOPGroupHubPlan>("EXEC GetHOPGroupHubPlan").ToList();
                        if (BrandForecastDetail != null && BrandForecastDetail.Any())
                        {
                            foreach (var warehouse in ValidationExcel)
                            {
                                var Filterwarehouse = HOPData.Where(x => x.WarehouseName == warehouse.WarehouseName).FirstOrDefault();
                                if (Filterwarehouse.WarehouseId > 0)
                                {
                                    foreach (var item in BrandForecastDetail.Where(x => x.Warehouseid == Filterwarehouse.WarehouseId))
                                    {
                                        //if (Convert.ToDecimal(Filterwarehouse.PlannedValue) < warehouse.ValueInAmt)
                                        //{
                                            var ExcelValue = Exceltrnfrorders.Where(x => x.Id == item.Id).FirstOrDefault();
                                            double CalPercentValue = 0;
                                            CalPercentValue = ((Convert.ToDouble(ExcelValue.ValueInAmt) / Filterwarehouse.PlannedValue) * 100);
                                            item.PercentValue = CalPercentValue;//float
                                            item.ValueInAmt = ExcelValue.ValueInAmt;
                                            item.ModifiedBy = userid;
                                            item.ModifiedDate = DateTime.Now;
                                        
                                        //}
                                        //else
                                        //{
                                        //    Msg = "Excel Total ValueInAmt [" + warehouse.ValueInAmt + "] of Warehouse [" + Filterwarehouse.WarehouseName + "] is higher than HOP ValueInAmt [" + Filterwarehouse.PlannedValue + "] ";
                                        //    itemForeCastUpdateResponse.Status = true;
                                        //    itemForeCastUpdateResponse.msg = Msg;
                                        //    return Created(Msg, Msg);
                                        //}
                                    }
                                }
                            }
                        }
                        if (context.Commit() > 0)
                        {
                            Msg = "Brand Summary data updated successfully.";
                            itemForeCastUpdateResponse.Status = true;
                            itemForeCastUpdateResponse.msg = Msg;
                        }
                        else
                        {
                            Msg = "Update Request Brand Summary Record not Found.";
                            itemForeCastUpdateResponse.Status = false;
                            itemForeCastUpdateResponse.msg = Msg;
                        }
                    }
                    else
                    {
                        Msg = "No Record Found!!";
                    }
                    return Created(Msg, Msg);

                }
                catch (Exception ex)
                {

                    //logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                }
                return Created("Error", "Error");
            }
        }



        [HttpPost]
        [Route("BuyersForecastUploadFile")]
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
                        return ReadBuyersForecastUploadedFile(hssfwb, userid);
                    }
                    else
                    {
                        return Created("File extnsion required .xlsx", "File extnsion required .xlsx");
                    }
                }
            }

            return Created("Error", "Error");
        }

       

        public IHttpActionResult ReadBuyersForecastUploadedFile(XSSFWorkbook hssfwb, int userid)
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
                    List<BuyersForecastUploadedFileDc> BuyersForecastUploader = new List<BuyersForecastUploadedFileDc>();
                    int? txnIdCellIndex = null;
                    int? GroupCodeCellIndex = null;
                    int? ClientCodeCellIndex = null;
                    int? AmountCellIndex = null;
                    List<string> headerlst = new List<string>();
                    List<BuyerForecastUploderDetail> Addlist = new List<BuyerForecastUploderDetail>();
                    List<BuyersForecastUploadedFileDc> trnfrorders = new List<BuyersForecastUploadedFileDc>();
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
                                GroupCodeCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Group_Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Group_Name").ColumnIndex : (int?)null;
                                if (!GroupCodeCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Group_Name does not exist..try again");
                                    return Created(strJSON, strJSON); ;
                                }//changes
                                ClientCodeCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Department_Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Department_Name").ColumnIndex : (int?)null;
                                if (!ClientCodeCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Dept does not exist..try again");
                                    return Created(strJSON, strJSON); ;
                                }

                                txnIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Category_Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Category_Name").ColumnIndex : (int?)null;
                                if (!txnIdCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Category does not exist..try again");
                                    return Created(strJSON, strJSON); ;
                                }

                                AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Brand_Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Brand_Name").ColumnIndex : (int?)null;
                                if (!AmountCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Brand does not exist..try again");
                                    return Created(strJSON, strJSON);
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

                                BuyersForecastUploadedFileDc trnfrorder = new BuyersForecastUploadedFileDc();
                                string dept = string.Empty, Category = string.Empty, brand = string.Empty; string group = string.Empty;
                                try
                                {
                                    foreach (var cellDatas in rowData.Cells)
                                    {
                                        trnfrorder = new BuyersForecastUploadedFileDc();
                                        if (cellDatas.ColumnIndex < 4)
                                        {
                                            if (cellDatas.ColumnIndex == 0)
                                                group = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 1)
                                                dept = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 2)
                                                Category = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 3)
                                                brand = cellDatas.ToString();
                                        }
                                        else
                                        {
                                            trnfrorder.Group = group;
                                            trnfrorder.Dept = dept;
                                            trnfrorder.Category = Category;
                                            trnfrorder.Brand = brand;
                                            trnfrorder.WarehouseName = headerlst[cellDatas.ColumnIndex];
                                            trnfrorder.PercentValue = Convert.ToDouble(cellDatas.ToString());
                                            trnfrorders.Add(trnfrorder);
                                        }
                                    }
                                    BuyersForecastUploader.Add(trnfrorder);


                                }
                                catch (Exception ex)
                                {
                                    //msgitemname = ex.Message;
                                    //logger.Error("Error VAN Paymant Upload File " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                                }
                            }
                        }
                    }

                    if (trnfrorders != null && trnfrorders.Any())
                    {
                        var validgroup = trnfrorders.GroupBy(x => x.Group).Select(x => new BuyersGroupWisePerValDc
                        {
                            GroupName = x.Key

                        }).ToList();
                        if (validgroup.Count == 1)
                        {

                            var validexcel = trnfrorders.GroupBy(x => x.WarehouseName).Select(x => new BuyersBrandWisePerValDc
                            {
                                WarehouseName = x.Key,
                                TotalPercentValue = Math.Round(x.Sum(y => y.PercentValue))//x.Sum(y => y.PercentValue)
                            }).ToList();
                            foreach (var i in validexcel)
                            {
                                if (i.TotalPercentValue < 100)
                                {
                                    Msg = i.WarehouseName + " PercentValue is not 100% ";
                                    return Created(Msg, Msg);
                                }
                                if (i.TotalPercentValue > 100)
                                {
                                    Msg = i.WarehouseName + " PercentValue should  not greater than 100% ";
                                    return Created(Msg, Msg);
                                }
                            }

                            var BuyerForecastUploderDbs = context.BuyerForecastUploderDb.Where(x =>   x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                            if (BuyerForecastUploderDbs != null)
                            {
                                BuyerForecastUploderDbs.IsActive = false;
                                BuyerForecastUploderDbs.IsDeleted = true;
                                context.Entry(BuyerForecastUploderDbs).State = EntityState.Modified;
                            }
                            var BuyerForecastUploderDetailDbs = context.BuyerForecastUploderDetailDb.Where(x =>  x.IsActive == true && x.IsDeleted == false).ToList();
                            if (BuyerForecastUploderDetailDbs != null && BuyerForecastUploderDetailDbs.Any())
                            {
                                BuyerForecastUploderDetailDbs.ForEach(x =>
                                {
                                    x.IsActive = false;
                                    x.IsDeleted = true;
                                    context.Entry(x).State = EntityState.Modified;
                                });
                            }

                            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/UploadedFiles/BuyerforecastUpload");
                            if (!Directory.Exists(ExcelSavePath))
                                Directory.CreateDirectory(ExcelSavePath);

                            string fileUrl = "";
                            var BuyerForcastDt = ClassToDataTable.CreateDataTable(trnfrorders);
                            BuyerForcastDt.TableName = "Buyerforecast Upload Data";
                            var fileName = "BuyerforecastSheet" + DateTime.Now.ToString("yyyyddMHHmmss") + ".xlsx";
                            string filePath = ExcelSavePath + "\\" + fileName;
                            ExcelGenerator.DataTable_To_Excel(new List<DataTable> { BuyerForcastDt }, filePath);
                            fileUrl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                                    , HttpContext.Current.Request.Url.DnsSafeHost
                                                                    , HttpContext.Current.Request.Url.Port
                                                                    , string.Format("UploadedFiles/BuyerforecastUpload/{0}", fileName));


                            var buyerForecastUploder = context.BuyerForecastUploderDb.Add(new BuyerForecastUploder
                            {
                                FilePath = fileUrl,
                                Status = 1,     //1 In Process
                                CreatedDate = DateTime.Now,
                                ModifiedDate = null,
                                IsActive = true,
                                IsDeleted = false,
                                CreatedBy = userid,
                                ModifiedBy = 0
                            });
                            var WarehouseList = trnfrorders.Select(x => x.WarehouseName).Distinct().ToList();
                            var WarehouseNameList = context.Warehouses.Where(x => WarehouseList.Contains(x.WarehouseName)).Select(x => x.WarehouseName).ToList();
                            var Grouplist = trnfrorders.Select(x => x.Group).Distinct().ToList();
                            var Groupnamelist = context.StoreDB.Where(x => x.Name.Contains(x.Name)).Select(x => x.Name).ToList();
                            string query = "EXEC GetSubcategoryCategoryMappings ";
                            var ValidList = context.Database.SqlQuery<GetSubcategoryCategoryMappingsDC>(query).ToList();
                            int status = 2;
                            trnfrorders.ForEach(x =>
                            {
                                string ErrorMsgs = "";
                                BuyerForecastUploderDetail buyerForecastUploderDetail = new BuyerForecastUploderDetail();
                                buyerForecastUploderDetail.UplodeId = buyerForecastUploder.Id;
                                buyerForecastUploderDetail.GroupName = x.Group;
                                buyerForecastUploderDetail.BrandName = x.Brand;
                                buyerForecastUploderDetail.WarehouseName = x.WarehouseName;
                                buyerForecastUploderDetail.CateName = x.Category;
                                buyerForecastUploderDetail.BaseCateName = x.Dept;
                                buyerForecastUploderDetail.PercentValue = x.PercentValue;


                                if (!ValidList.Any(y => y.CategoryName == x.Category))
                                {
                                    ErrorMsgs = x.Category + " Category Not Found";
                                    status = 3;
                                }
                                if (!ValidList.Any(y => y.BaseCategoryName == x.Dept))
                                {
                                    ErrorMsgs = ErrorMsgs + "~" + x.Dept + " BaseCategoryName Not Found";
                                    status = 3;
                                }
                                if (!ValidList.Any(y => y.SubsubcategoryName == x.Brand))
                                {
                                    ErrorMsgs = ErrorMsgs + "~" + x.Brand + " Brand Not Found";
                                    status = 3;
                                }
                                if (!WarehouseNameList.Any(y => y == x.WarehouseName))
                                {
                                    ErrorMsgs = ErrorMsgs + "~" + x.WarehouseName + " WarehouseName Not Found";
                                    status = 3;
                                }
                                if (!Groupnamelist.Any(Z => Z == x.Group))
                                {
                                    ErrorMsgs = ErrorMsgs + "~" + x.Group + " GroupName Not Found";
                                    status = 3;
                                }
                                buyerForecastUploderDetail.ErrorMsg = ErrorMsgs;
                                buyerForecastUploderDetail.CreatedDate = buyerForecastUploder.CreatedDate;
                                buyerForecastUploderDetail.IsActive = true;
                                buyerForecastUploderDetail.IsDeleted = false;
                                buyerForecastUploderDetail.CreatedBy = userid;
                                Addlist.Add(buyerForecastUploderDetail);
                            });
                            context.BuyerForecastUploderDetailDb.AddRange(Addlist);
                            context.Commit();
                            //var buyerForecastUploderDetailDbss = context.BuyerForecastUploderDetailDb.Where(x => x.UplodeId == buyerForecastUploder.Id && x.ErrorMsg != "").FirstOrDefault();
                            if (buyerForecastUploder.Id > 0)
                            {
                                var BuyerForecastUploderDbss = context.BuyerForecastUploderDb.Where(x => x.Id == buyerForecastUploder.Id).FirstOrDefault();
                                BuyerForecastUploderDbss.Status = status;

                                context.Entry(BuyerForecastUploderDbss).State = EntityState.Modified;
                                context.Commit();
                                if (status == 2)
                                {
                                    var JObData = context.Database.ExecuteSqlCommand("EXEC InsertBrandForecastValues");
                                }
                            }

                            Msg = "Record Insert Successfully!!";
                        }
                        else
                        {
                            Msg = "Multiple Group names are inserting  ";
                        }
                    }

                    else
                    {
                        Msg = "No Record Found!!";
                    }
                    return Created(Msg, Msg);

                }
                catch (Exception ex)
                {

                    //logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                }
                return Created("Error", "Error");
            }
        }

        [HttpPost]
        [Route("BuyersEditUploadFile")]
        public IHttpActionResult BuyersEditUploadFile()
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

                        return ReadBuyersEditUploadedFile(hssfwb, userid);
                    }
                    else
                    {
                        return Created("File extnsion required .xlsx", "File extnsion required .xlsx");
                    }
                }
            }

            return Created("Error", "Error");
        }


        public IHttpActionResult ReadBuyersEditUploadedFile(XSSFWorkbook hssfwb, int userid)
        {
            using (var context = new AuthContext())
            {
                string Msg = string.Empty;
                string sSheetName = hssfwb.GetSheetName(0);
                ISheet sheet = hssfwb.GetSheet(sSheetName);
                IRow rowData;
                ICell cellData = null;
                ItemForeCastUpdateResponse itemForeCastUpdateResponse = new ItemForeCastUpdateResponse();
                try
                {
                    List<BuyersForecastUploadedFileDc> BuyersForecastUploader = new List<BuyersForecastUploadedFileDc>();
                    int? txnIdCellIndex = null;
                    int? txnSubsubcategoryNameCellIndex = null;
                    int? PercentValueCellIndex = null;
                    int? BuyerEditForecastCellIndex = null;
                    int? InventoryDaysCellIndex = null;
                    List<string> headerlst = new List<string>();
                    List<BuyerForecastUploderDetail> Addlist = new List<BuyerForecastUploderDetail>();
                    List<BuyersEditUploadedFileDc> Exceltrnfrorders = new List<BuyersEditUploadedFileDc>();
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
                                txnIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Id") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Id").ColumnIndex : (int?)null;
                                if (!txnIdCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Id does not exist..try again");
                                    return Created(strJSON, strJSON); ;
                                }

                                txnSubsubcategoryNameCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "SubsubcategoryName") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "SubsubcategoryName").ColumnIndex : (int?)null;
                                if (!txnSubsubcategoryNameCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("SubsubcategoryName does not exist..try again");
                                    return Created(strJSON, strJSON); ;
                                }

                                PercentValueCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "PercentValue") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "PercentValue").ColumnIndex : (int?)null;
                                if (!PercentValueCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("PercentValue does not exist..try again");
                                    return Created(strJSON, strJSON); ;
                                }

                                BuyerEditForecastCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "BuyerEditForecast") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "BuyerEditForecast").ColumnIndex : (int?)null;
                                if (!BuyerEditForecastCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("BuyerEditForecast does not exist..try again");
                                    return Created(strJSON, strJSON);
                                }

                                InventoryDaysCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "InventoryDays") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "InventoryDays").ColumnIndex : (int?)null;
                                if (!InventoryDaysCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("InventoryDays does not exist..try again");
                                    return Created(strJSON, strJSON);
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

                                BuyersEditUploadedFileDc trnfrorder = new BuyersEditUploadedFileDc();

                                try
                                {
                                    cellData = rowData.GetCell(txnIdCellIndex.Value);
                                    txnId_col0 = cellData == null ? "" : cellData.ToString();
                                    trnfrorder.ID = Convert.ToInt32(txnId_col0);
                                    logger.Info("ID :" + trnfrorder.ID);


                                    cellData = rowData.GetCell(txnSubsubcategoryNameCellIndex.Value);
                                    txnSubsubcategoryName_col1 = cellData == null ? "" : cellData.ToString();
                                    trnfrorder.SubsubcategoryName = Convert.ToString(txnSubsubcategoryName_col1);
                                    logger.Info("BrandName :" + trnfrorder.SubsubcategoryName);

                                    cellData = rowData.GetCell(PercentValueCellIndex.Value);
                                    txnPercentValue_col9 = cellData == null ? "" : cellData.ToString();
                                    trnfrorder.PercentValue = Convert.ToDouble(txnPercentValue_col9);
                                    logger.Info("PercentValue :" + trnfrorder.PercentValue);

                                    cellData = rowData.GetCell(BuyerEditForecastCellIndex.Value);
                                    txnBuyerEditForecastQty_col11 = cellData == null ? "" : cellData.ToString();
                                    trnfrorder.BuyerEditForecastQty = Convert.ToInt32(txnBuyerEditForecastQty_col11);
                                    logger.Info("Amount :" + trnfrorder.BuyerEditForecastQty);

                                    cellData = rowData.GetCell(InventoryDaysCellIndex.Value);
                                    txnInventoryDays_col17 = cellData == null ? "" : cellData.ToString();
                                    trnfrorder.InventoryDays = Convert.ToInt32(txnInventoryDays_col17);
                                    logger.Info("InventoryDays :" + trnfrorder.InventoryDays);

                                    trnfrorder.userid = userid;
                                    Exceltrnfrorders.Add(trnfrorder);
                                }
                                catch (Exception ex)
                                {
                                    //msgitemname = ex.Message;
                                    //logger.Error("Error VAN Paymant Upload File " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                                    Msg = "Excel File: " + ex.Message;
                                    itemForeCastUpdateResponse.Status = true;
                                    itemForeCastUpdateResponse.msg = Msg;
                                    return Created(Msg, Msg);
                                }
                            }
                        }
                    }

                    if (Exceltrnfrorders != null && Exceltrnfrorders.Any())
                    {
                        //var result = new BuyersEditBrandWisePerValMainList();
                        //result.BuyersEditBrandWisePerValDc = Exceltrnfrorders.GroupBy(x => x.SubsubcategoryName).Select(x => new BuyersEditBrandWisePerValDc
                        //{
                        //    BrandName = x.Key,
                        //    TotalPercentValue = x.Sum(y => y.PercentValue)
                        //}).ToList();

                        var Ids = Exceltrnfrorders.Select(x => x.ID).Distinct().ToList();

                        var ExcelitemForecastDetail = context.ItemForecastDetailDb.Where(x => Ids.Contains(x.Id)).ToList();
                        var brandforecastIds = ExcelitemForecastDetail.Select(x => x.BrandForecastId).Distinct().ToList();

                        var brandforecastlst = context.BrandForecastDetailDb.Where(x => brandforecastIds.Contains(x.Id)).Select(x => new { x.Id, x.SubSubCatID, x.PercentValue, x.ValueInAmt }).ToList();
                        if (ExcelitemForecastDetail != null && ExcelitemForecastDetail.Any())
                        {

                            foreach (var item in ExcelitemForecastDetail.GroupBy(x => x.BrandForecastId))
                            {
                                var itemforecastids = item.Select(x => x.Id).ToList();
                                var excelitemforecastsvalues = Exceltrnfrorders.Where(x => itemforecastids.Contains(x.ID)).Sum(x => x.PercentValue);
                                var barnvalue = brandforecastlst.FirstOrDefault(x => x.Id == item.Key)?.PercentValue;
                                var ValueInAmt = brandforecastlst.FirstOrDefault(x => x.Id == item.Key)?.ValueInAmt;
                                if (barnvalue < excelitemforecastsvalues)
                                {
                                    itemForeCastUpdateResponse.Status = false;
                                    itemForeCastUpdateResponse.msg = "Total brand item percent value greater than brand percent value.";
                                    return Created(itemForeCastUpdateResponse.msg, itemForeCastUpdateResponse);
                                }
                                foreach (var item1 in item)
                                {
                                    var ExcelValue = Exceltrnfrorders.Where(x => x.ID == item1.Id).FirstOrDefault();
                                    if ((DateTime.Now.Day >= 1) && (DateTime.Now.Day <= 5))
                                    {
                                        item1.PercentValue = ExcelValue.PercentValue;
                                        item1.BuyerEdit = ExcelValue.BuyerEditForecastQty;
                                        item1.GrowthInAmount = Convert.ToDouble(ValueInAmt) * ExcelValue.PercentValue / 100;
                                    }
                                    item1.InventoryDays = ExcelValue.InventoryDays;
                                    item1.ModifiedBy = userid;
                                    item1.ModifiedDate = DateTime.Now;
                                    context.Entry(item1).State = EntityState.Modified;
                                }
                            }
                            if (context.Commit() > 0)
                            {
                                Msg = "Data updated successfully.";
                                itemForeCastUpdateResponse.Status = true;
                                itemForeCastUpdateResponse.msg = Msg;
                            }
                        }
                        else
                        {
                            Msg = "Update Request Record not Found.";
                            itemForeCastUpdateResponse.Status = false;
                            itemForeCastUpdateResponse.msg = Msg;
                        }
                        //context.Entry(BuyerForecastUploderDbss).State = EntityState.Modified;
                        //context.Commit();
                        //Msg = "Record Insert Successfully!!";
                    }
                    else
                    {
                        Msg = "No Record Found!!";
                    }
                    return Created(Msg, Msg);

                }
                catch (Exception ex)
                {

                    //logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                }
                return Created("Error", "Error");
            }
        }

        [HttpPost]
        [Route("ForecastInventoryDaysUploadFile")]
        public IHttpActionResult UploadInventoryDaysFile()
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
                    return ReadForecastInventoryDaysUploadedFile(hssfwb, userid);
                }
            }

            return Created("Error", "Error");
        }
        public IHttpActionResult ReadForecastInventoryDaysUploadedFile(XSSFWorkbook hssfwb, int userid)
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
                    List<ForecastInventoryDaysUploadedFileDc> BuyersForecastUploader = new List<ForecastInventoryDaysUploadedFileDc>();
                    int? txnIdCellIndex = null;
                    int? ClientCodeCellIndex = null;
                    int? AmountCellIndex = null;
                    List<string> headerlst = new List<string>();
                    List<ForecastInventoryDay> Addlist = new List<ForecastInventoryDay>();
                    List<ForecastInventoryDaysUploadedFileDc> trnfrorders = new List<ForecastInventoryDaysUploadedFileDc>();
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


                                txnIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Category_Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Category_Name").ColumnIndex : (int?)null;
                                if (!txnIdCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Category does not exist..try again");
                                    return Created(strJSON, strJSON); ;
                                }

                                AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Brand_Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Brand_Name").ColumnIndex : (int?)null;
                                if (!AmountCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Brand does not exist..try again");
                                    return Created(strJSON, strJSON);
                                }
                                AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "SubCateName") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "SubCateName").ColumnIndex : (int?)null;
                                if (!AmountCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("SubCateName does not exist..try again");
                                    return Created(strJSON, strJSON);
                                }
                                AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "WarehouseName") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "WarehouseName").ColumnIndex : (int?)null;
                                if (!AmountCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("WarehouseName does not exist..try again");
                                    return Created(strJSON, strJSON);
                                }
                                //SubCateName
                            }
                        }
                        else
                        {
                            rowData = sheet.GetRow(iRowIdx);
                            cellData = rowData.GetCell(0);
                            rowData = sheet.GetRow(iRowIdx);
                            if (rowData != null)
                            {

                                ForecastInventoryDaysUploadedFileDc trnfrorder = new ForecastInventoryDaysUploadedFileDc();
                                string WarehouseNm = string.Empty, Category = string.Empty, brand = string.Empty, SubCateName = string.Empty, inventorydays = string.Empty;
                                try
                                {
                                    foreach (var cellDatas in rowData.Cells)
                                    {
                                        trnfrorder = new ForecastInventoryDaysUploadedFileDc();
                                        if (cellDatas.ColumnIndex < 4)
                                        {
                                            if (cellDatas.ColumnIndex == 0)
                                                brand = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 1)
                                                WarehouseNm = cellDatas.ToString();

                                            else if (cellDatas.ColumnIndex == 2)
                                                Category = cellDatas.ToString();

                                            else if (cellDatas.ColumnIndex == 3)
                                                SubCateName = cellDatas.ToString();
                                        }
                                        else
                                        {
                                            //trnfrorder. = dept;
                                            trnfrorder.SubCateName = SubCateName;
                                            trnfrorder.CateName = Category;
                                            trnfrorder.BrandName = brand;
                                            trnfrorder.WarehouseName = WarehouseNm;// headerlst[cellDatas.ColumnIndex];
                                            trnfrorder.InventoryDays = Convert.ToInt32(cellDatas.ToString()); //int.Parse(inventorydays);
                                            trnfrorders.Add(trnfrorder);
                                        }
                                    }
                                    BuyersForecastUploader.Add(trnfrorder);
                                }
                                catch (Exception ex)
                                {
                                    //msgitemname = ex.Message;
                                    //logger.Error("Error VAN Paymant Upload File " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                                }
                            }
                        }
                    }

                    if (trnfrorders != null && trnfrorders.Any())
                    {
                        var ForecastInventoryUploderDetailDbs = context.ForecastInventoryDayDb.Where(x => x.CreatedDate.Month == DateTime.Now.Month && x.CreatedDate.Year == DateTime.Now.Year && x.IsActive == true && x.IsDeleted == false).ToList();
                        if (ForecastInventoryUploderDetailDbs != null && ForecastInventoryUploderDetailDbs.Any())
                        {
                            ForecastInventoryUploderDetailDbs.ForEach(x =>
                            {
                                x.IsActive = false;
                                x.IsDeleted = true;
                                context.Entry(x).State = EntityState.Modified;
                            });
                        }

                        var WarehouseList = trnfrorders.Select(x => x.WarehouseName).Distinct().ToList();
                        var WarehouseNameList = context.Warehouses.Where(x => WarehouseList.Contains(x.WarehouseName) && x.active == true && x.Deleted == false).Select(x => new { x.WarehouseName, x.WarehouseId }).ToList();

                        //var CategoryList = trnfrorders.Select(x => x.CateName).Distinct().ToList();
                        //var CategoryNameList = context.Categorys.Where(x => CategoryList.Contains(x.CategoryName) && x.IsActive == true && x.Deleted == false).Select(x => new { x.CategoryName, x.Categoryid }).ToList();
                        //var SubCatList = trnfrorders.Select(x => x.SubCateName).Distinct().ToList();
                        //var SubCatNameList = context.SubCategorys.Where(x => SubCatList.Contains(x.SubcategoryName) && x.IsActive == true && x.Deleted == false).Select(x => new { x.SubCategoryId, x.SubcategoryName }).ToList();
                        //var BrandList = trnfrorders.Select(x => x.BrandName).Distinct().ToList();
                        //var BrandNameList = context.SubsubCategorys.Where(x => BrandList.Contains(x.SubsubcategoryName) && x.IsActive == true && x.Deleted == false).Select(x => new { x.SubsubCategoryid, x.SubsubcategoryName }).ToList();

                        string query = "EXEC GetSubcategoryCategoryMappings ";
                        var ValidList = context.Database.SqlQuery<GetSubcategoryCategoryMappingsDC>(query).ToList();
                        foreach (var x in trnfrorders)
                        {
                            var WarehouseID = WarehouseNameList.FirstOrDefault(y => y.WarehouseName == x.WarehouseName)?.WarehouseId;
                            var Categoryid = ValidList.FirstOrDefault(z => z.CategoryName == x.CateName)?.Categoryid;
                            var SubCategoryId = ValidList.FirstOrDefault(w => w.BrandName == x.SubCateName)?.SubCategoryId;
                            var SubSubCategoryId = ValidList.FirstOrDefault(v => v.SubsubcategoryName == x.BrandName)?.SubsubcategoryId ;
                            if (WarehouseID == null || Categoryid == null || SubCategoryId == null || SubSubCategoryId == null)
                            {
                                if (Categoryid == null)
                                {
                                    Msg = x.CateName + " Category Not Found";
                                }
                                if (SubCategoryId == null)
                                {
                                    Msg = Msg + "~" + x.SubCateName + " SubCategory Not Found";
                                }
                                if (SubSubCategoryId == null)
                                {
                                    Msg = Msg + "~" + x.BrandName + " BrandName Not Found";
                                }
                                if (WarehouseID == null)
                                {
                                    Msg = Msg + "~" + x.WarehouseName + " WarehouseName Not Found";
                                }
                                return Created(Msg, Msg);
                            }
                            else
                            {

                                ForecastInventoryDay InventoryDayUploderDetail = new ForecastInventoryDay();
                                InventoryDayUploderDetail.SubSubCatID = Convert.ToInt32(SubSubCategoryId);
                                InventoryDayUploderDetail.WarehouseId = Convert.ToInt32(WarehouseID);
                                InventoryDayUploderDetail.CatID = Convert.ToInt32(Categoryid);
                                InventoryDayUploderDetail.SubCatID = Convert.ToInt32(SubCategoryId);
                                InventoryDayUploderDetail.InventoryDays = x.InventoryDays;
                                InventoryDayUploderDetail.CreatedDate = DateTime.Now;
                                InventoryDayUploderDetail.IsActive = true;
                                InventoryDayUploderDetail.IsDeleted = false;
                                InventoryDayUploderDetail.CreatedBy = userid;
                                InventoryDayUploderDetail.ModifiedBy = 0;
                                InventoryDayUploderDetail.ModifiedDate = null;
                                Addlist.Add(InventoryDayUploderDetail);
                            }
                        };
                        context.ForecastInventoryDayDb.AddRange((IEnumerable<ForecastInventoryDay>)Addlist);
                        context.Commit();
                        var show = context.Database.ExecuteSqlCommand("EXEC ItemForecastInventoryDays");
                        Msg = "Record Insert Successfully!!";
                    }
                    else
                    {
                        Msg = "No Record Found!!";
                    }
                    return Created(Msg, Msg);
                }
                catch (Exception ex)
                {

                    //logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                }
                return Created("Error", "Error");
            }
        }


        [HttpPost]
        [Route("ForecastInventoryDaysRestrictionUploadFile")]
        public IHttpActionResult UploadRestrictionInventoryDaysFile()
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
                    return RestrictionReadForecastInventoryDaysUploadedFile(hssfwb, userid);
                }
            }

            return Created("Error", "Error");
        }
        public IHttpActionResult RestrictionReadForecastInventoryDaysUploadedFile(XSSFWorkbook hssfwb, int userid)
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
                    List<ForecastInventoryDaysUploadedFileDc> BuyersForecastUploader = new List<ForecastInventoryDaysUploadedFileDc>();
                    int? txnIdCellIndex = null;
                    int? ClientCodeCellIndex = null;
                    int? AmountCellIndex = null;
                    List<string> headerlst = new List<string>();
                    List<ForecastInventoryDay> Addlist = new List<ForecastInventoryDay>();
                    List<ForecastInventoryDaysUploadedFileDc> trnfrorders = new List<ForecastInventoryDaysUploadedFileDc>();
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
                                AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Id") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Id").ColumnIndex : (int?)null;
                                if (!AmountCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Id does not exist..try again");
                                    return Created(strJSON, strJSON);
                                }
                                AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "StoreName") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "StoreName").ColumnIndex : (int?)null;
                                if (!AmountCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("StoreName does not exist..try again");
                                    return Created(strJSON, strJSON);
                                }
                                txnIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "WarehouseName") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "WarehouseName").ColumnIndex : (int?)null;
                                if (!txnIdCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("WarehouseName does not exist..try again");
                                    return Created(strJSON, strJSON); ;
                                }

                                AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "CateName") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "CateName").ColumnIndex : (int?)null;
                                if (!AmountCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Brand does not exist..try again");
                                    return Created(strJSON, strJSON);
                                }

                                AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "SubCateName") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "SubCateName").ColumnIndex : (int?)null;
                                if (!AmountCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("SubCateName does not exist..try again");
                                    return Created(strJSON, strJSON);
                                }

                                AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "BrandName") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "BrandName").ColumnIndex : (int?)null;
                                if (!AmountCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Brand does not exist..try again");
                                    return Created(strJSON, strJSON);
                                }
                               
                                AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "InventoryDays") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "InventoryDays").ColumnIndex : (int?)null;
                                if (!AmountCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("InventoryDays does not exist..try again");
                                    return Created(strJSON, strJSON);
                                }
                                AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "CalculateInventoryDays") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "CalculateInventoryDays").ColumnIndex : (int?)null;
                                if (!AmountCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("CalculateInventoryDays does not exist..try again");
                                    return Created(strJSON, strJSON);
                                }
                                AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "SafetyDays") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "SafetyDays").ColumnIndex : (int?)null;
                                if (!AmountCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("SafetyDays does not exist..try again");
                                    return Created(strJSON, strJSON);
                                }
                                //SubCateName
                            }
                        }
                        else
                        {
                            rowData = sheet.GetRow(iRowIdx);
                            cellData = rowData.GetCell(0);
                            rowData = sheet.GetRow(iRowIdx);
                            if (rowData != null)
                            {
                                ForecastInventoryDaysUploadedFileDc trnfrorder = new ForecastInventoryDaysUploadedFileDc();
                                string WarehouseNm = string.Empty, SubCateName=string.Empty, brand = string.Empty, calculateInventoryDays = string.Empty, StoreName = string.Empty, safetydays = string.Empty, inventorydays = string.Empty,   CatName = string.Empty, Id = string.Empty;
                                try
                                {
                                    foreach (var cellDatas in rowData.Cells)
                                    {
                                        trnfrorder = new ForecastInventoryDaysUploadedFileDc();
                                        if (cellDatas.ColumnIndex < 9)
                                        {
                                            if (cellDatas.ColumnIndex == 0)
                                                Id = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 1)
                                                StoreName = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 2)
                                                WarehouseNm = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 3)
                                                CatName = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 4)
                                                SubCateName = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 5)
                                                brand = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 6)
                                                calculateInventoryDays = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 7)
                                                inventorydays = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 8)
                                                safetydays = cellDatas.ToString();

                                        }
                                        if (cellDatas.ColumnIndex == 8)
                                        {
                                            trnfrorder.Id = Convert.ToInt32(Id);
                                            trnfrorder.StoreName = StoreName;
                                            trnfrorder.WarehouseName = WarehouseNm;
                                            trnfrorder.CateName = CatName;
                                            trnfrorder.SubCateName = SubCateName;
                                            trnfrorder.BrandName = brand;
                                            trnfrorder.CalculateInventoryDays = Convert.ToDouble(calculateInventoryDays);
                                            trnfrorder.InventoryDays = Convert.ToInt32(inventorydays); //int.Parse(inventorydays);
                                            trnfrorder.SafetyDays = Convert.ToInt32(safetydays);
                                          //  trnfrorder.CalculateInventoryDays = Convert.ToDouble(cellDatas.ToString());
                                            trnfrorders.Add(trnfrorder);
                                        }
                                    }
                                    BuyersForecastUploader.Add(trnfrorder);
                                }
                                catch (Exception ex)
                                {
                                    //msgitemname = ex.Message;
                                    //logger.Error("Error VAN Paymant Upload File " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                                }
                            }
                        }
                    }

                    if (trnfrorders != null && trnfrorders.Any())
                    {
                        
                        var validmsg = "Inventory Days ";
                        var InventryDaysWrong = false;
                        var InventryDaysWrongt = false;
                        var WarehouseList = trnfrorders.Select(x => x.WarehouseName).Distinct().ToList();
                        var WarehouseNameList = context.Warehouses.Where(x => WarehouseList.Contains(x.WarehouseName) && x.active == true && x.Deleted == false).Select(x => new { x.WarehouseName, x.WarehouseId }).ToList();
                        string query = "EXEC GetBrandMappingsInventoryRestriction ";
                        var ValidList = context.Database.SqlQuery<GetSubcategoryCategoryMappingsDC>(query).ToList();

                        foreach (var x in trnfrorders)
                        {
                            var WarehouseID = WarehouseNameList.FirstOrDefault(y => y.WarehouseName == x.WarehouseName)?.WarehouseId;
                            var SubCategoryId = ValidList.FirstOrDefault(w => w.SubCategoryName == x.SubCateName)?.SubCategoryId;
                            var SubSubCategoryId = ValidList.FirstOrDefault(v => v.BrandName == x.BrandName)?.BrandId;
                            var storeid = ValidList.FirstOrDefault(s => s.StoreName == x.StoreName)?.StoreId;
                            var cid = ValidList.FirstOrDefault(f => f.CategoryName == x.CateName)?.Categoryid; // new 06 april
                            if (WarehouseID == null ||  SubSubCategoryId == null || storeid==null) //Categoryid == null || SubCategoryId == null ||
                            {
                                
                                if (SubSubCategoryId == null)
                                {
                                    Msg = Msg + "~" + x.BrandName + " BrandName Not Found";
                                }
                                if (WarehouseID == null)
                                {
                                    Msg = Msg + "~" + x.WarehouseName + " WarehouseName Not Found";
                                }
                                if (storeid == null)
                                {
                                    Msg = Msg + "~" + x.StoreName + " StoreName Not Found";
                                }
                                if(cid==null)
                                {
                                    Msg = Msg + "~" + x.CateName + " CategoryName Not Found";

                                }
                                return Created(Msg, Msg);
                            }
                         
                            else
                            {
                               
                               
                                        if (x.InventoryDays <= 60 && x.InventoryDays>0 ) //&& Excelitem.InventoryDays != x.InventoryDays)
                                        {
                                        InventryDaysWrong = true;
                                        }
                                        else
                                        {
                                             if(x.InventoryDays==0 )
                                                {
                                            InventryDaysWrongt = true;
                                                    validmsg = validmsg + " value should be greater than 0 of this Brand " +x.BrandName + ",";
                                                }
                                             if(x.InventoryDays > 60)
                                                {
                                            InventryDaysWrongt = true;
                                                    validmsg = validmsg + " value should be less than or equal 60 of this Brand " + x.BrandName + ",";
                                                }
                                    if (x.InventoryDays == null)
                                    {
                                        InventryDaysWrongt = true;
                                        validmsg = validmsg + " value should not be null of this Brand " + x.BrandName + ",";
                                    }
                                    
                                }
                                   
                             
                                                            
                            }
                        };
                        if (InventryDaysWrongt == true )
                        {
                            Msg = validmsg;
                        }
                        if (InventryDaysWrong == true && InventryDaysWrongt == false)
                        {


                            foreach (var x in trnfrorders)
                            {
                                var WarehouseID = WarehouseNameList.FirstOrDefault(y => y.WarehouseName == x.WarehouseName)?.WarehouseId;
                                var SubCategoryId = ValidList.FirstOrDefault(w => w.SubCategoryName == x.SubCateName)?.SubCategoryId;
                                var SubSubCategoryId = ValidList.FirstOrDefault(v => v.BrandName == x.BrandName)?.BrandId;
                                var storeid = ValidList.FirstOrDefault(s => s.StoreName == x.StoreName)?.StoreId;
                                var cid = ValidList.FirstOrDefault(f => f.CategoryName == x.CateName)?.Categoryid;
                                var InventoryDays = new SqlParameter("@InventoryDays",x.InventoryDays);
                                var SafetyDays = new SqlParameter("@SafetyDays", x.SafetyDays);
                                var Warehouseid = new SqlParameter("@Warehouseid", WarehouseID);
                                var SubCatID = new SqlParameter("@SubCatID", SubCategoryId);
                                var SubSubCatID = new SqlParameter("@SubSubCatID", SubSubCategoryId);
                                var useridd = new SqlParameter("@userid", userid);
                                var storeidd = new SqlParameter("@storeid", storeid);
                                var Id = new SqlParameter("@Id", x.Id);
                                var BrandName = new SqlParameter("@BrandName", x.BrandName);
                                var Catid = new SqlParameter("@Cid", cid);
                                var res = context.Database.SqlQuery<string>("EXEC sp_UpdateInventoryDaysRestriction @InventoryDays,@SafetyDays,@Warehouseid,@SubCatID,@SubSubCatID,@userid,@storeid,@Id,@BrandName,@Cid", InventoryDays, SafetyDays, Warehouseid, SubCatID, SubSubCatID, useridd, storeidd, Id, BrandName, Catid).FirstOrDefault();
                                Msg = res;
                            }
                        }


                     
                    }
                    else
                    {
                        Msg = "No Record Found!!";
                    }
                    return Created(Msg, Msg);
                }
                catch (Exception ex)
                {

                    //logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                }
                return Created("Error", "Error");
            }
        }

        [HttpPost]
        [Route("BulkAddNewArticleUploadFile")]
        public IHttpActionResult AddNewArticleUploadFile()
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
                    return NewArticleUploadedFile(hssfwb, userid);
                }
            }

            return Created("Error", "Error");
        }

        public IHttpActionResult NewArticleUploadedFile(XSSFWorkbook hssfwb, int userid)
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
                    List<AddNewArticleUploadedFileDc> BuyersForecastUploader = new List<AddNewArticleUploadedFileDc>();
                    int? txnIdCellIndex = null;
                    int? ClientCodeCellIndex = null;
                    int? AmountCellIndex = null;
                    List<string> headerlst = new List<string>();
                    List<ForecastInventoryDay> Addlist = new List<ForecastInventoryDay>();
                    List<AddNewArticleUploadedFileDc> trnfrorders = new List<AddNewArticleUploadedFileDc>();
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
                                AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "ItemMultiMRPId") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "ItemMultiMRPId").ColumnIndex : (int?)null;
                                if (!AmountCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("ItemMultiMRPId does not exist..try again");
                                    return Created(strJSON, strJSON);
                                }
                                AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "WarehouseName") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "WarehouseName").ColumnIndex : (int?)null;
                                if (!AmountCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("WarehouseName does not exist..try again");
                                    return Created(strJSON, strJSON);
                                }
                               

                                AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "MaxSellingPrice") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "MaxSellingPrice").ColumnIndex : (int?)null;
                                if (!AmountCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("MaxSellingPrice does not exist..try again");
                                    return Created(strJSON, strJSON);
                                }

                                AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "BuyerEdit") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "BuyerEdit").ColumnIndex : (int?)null;
                                if (!AmountCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("BuyerEdit does not exist..try again");
                                    return Created(strJSON, strJSON);
                                }

                                AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "InventoryDays") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "InventoryDays").ColumnIndex : (int?)null;
                                if (!AmountCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("InventoryDays does not exist..try again");
                                    return Created(strJSON, strJSON);
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
                                AddNewArticleUploadedFileDc trnfrorder = new AddNewArticleUploadedFileDc();
                                string WarehouseNm = string.Empty, BuyerEdit = string.Empty, brand = string.Empty, calculateInventoryDays = string.Empty, StoreName = string.Empty, InventoryDays = string.Empty, MaxSellingPrice = string.Empty, ItemMultiMRPId = string.Empty;
                                try
                                {
                                    foreach (var cellDatas in rowData.Cells)
                                    {
                                        trnfrorder = new AddNewArticleUploadedFileDc();
                                        if (cellDatas.ColumnIndex < 5)
                                        {
                                            if (cellDatas.ColumnIndex == 0)
                                                ItemMultiMRPId = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 1)
                                                WarehouseNm = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 2)
                                                MaxSellingPrice = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 3)
                                                BuyerEdit = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 4)
                                                InventoryDays = cellDatas.ToString();
                                        }
                                        if (cellDatas.ColumnIndex == 4)
                                        {
                                            trnfrorder.ItemMultiMRPId = Convert.ToInt32(ItemMultiMRPId);
                                            trnfrorder.WarehouseName = WarehouseNm;
                                            trnfrorder.MaxSellingPrice = Convert.ToDouble(MaxSellingPrice);
                                            trnfrorder.BuyerEdit = Convert.ToInt32(BuyerEdit);
                                            trnfrorder.InventoryDays = Convert.ToInt32(InventoryDays);
                                            trnfrorders.Add(trnfrorder);
                                        }
                                    }
                                   
                                }
                                catch (Exception ex)
                                {
                                    //msgitemname = ex.Message;
                                    //logger.Error("Error VAN Paymant Upload File " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                                }
                            }
                        }
                    }

                    if (trnfrorders != null && trnfrorders.Any())
                    {
                        var WarehouseList = trnfrorders.Select(x => x.WarehouseName).Distinct().ToList();
                        var WarehouseNameList = context.Warehouses.Where(x => WarehouseList.Contains(x.WarehouseName) && x.active == true && x.Deleted == false).Select(x => new { x.WarehouseName, x.WarehouseId }).ToList();
                        foreach (var x in trnfrorders)
                        {
                            var WarehouseID = WarehouseNameList.FirstOrDefault(y => y.WarehouseName == x.WarehouseName)?.WarehouseId;

                            if (WarehouseID == null) 
                            {
                                if (WarehouseID == null)
                                {
                                    Msg = Msg + "~" + x.WarehouseName + " WarehouseName Not Found";
                                }

                                return Created(Msg, Msg);
                            }

                            else
                            {
                                var chk = context.ItemForecastDetailDb.Where(z => z.ItemMultiMRPId == x.ItemMultiMRPId && z.WarehouseId == WarehouseID && z.IsActive == true && z.CreatedDate.Month == DateTime.Now.Month && z.CreatedDate.Year == DateTime.Now.Year).Select(z => z.ItemMultiMRPId).FirstOrDefault();
                                if (chk == 0)
                                {
                                        var Multimrpid = new SqlParameter("@ItemMultiMRPId", x.ItemMultiMRPId);
                                        var ware = new SqlParameter("@warehouseId", WarehouseID);
                                        var APP = new SqlParameter("@MaxSellingPrice", x.MaxSellingPrice);
                                        var ssQty = new SqlParameter("@SystemSuggestedQty", x.BuyerEdit);
                                        var Idays = new SqlParameter("@InventoryDays", x.InventoryDays);
                                        var res = context.Database.SqlQuery<string>("EXEC sp_AddNewItemInBuyerEditForecast @ItemMultiMRPId,@warehouseId,@MaxSellingPrice,@SystemSuggestedQty,@InventoryDays", Multimrpid, ware, APP, ssQty, Idays).FirstOrDefault();
                                        Msg = res;  
                                }
                                else
                                {
                                    Msg = "Already Exists ITemMultiMRPId " + chk+", "+Msg;
                                  
                                }
                            }
                        };
                    }

                    else
                    {
                        Msg = "No Record Found!!";
                    }
                    return Created(Msg, Msg);
                }
                catch (Exception ex)
                {

                    //logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                }
                return Created("Error", "Error");
            }
        }


    }
}
