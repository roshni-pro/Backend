using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Gullak;
using Common.Logging;
using Microsoft.Extensions.Logging;
using NLog;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/OrderReconcillation")]
    public class OrderReconcillationController : ApiController
    {
        public static Logger logger = NLog.LogManager.GetCurrentClassLogger();//Api for Upload Sheet on Warehouse to warehouse transfer
        string strJSON = null;
        string msgitemname, msg1;

        string txnId_col0, txnSubsubcategoryName_col1, txnPercentValue_col9, txnBuyerEditForecastQty_col11, txnInventoryDays_col17;
        string Warehouse_col1, ValueInAmt_col6;
        string Col0, col1, col2, col3, col4, col5, col6, col7, col8, col9, col10, col11, col12, col13, col14, col15, col16, col17, col18, col19, col20, col21;

        #region work by Neelesh
        [Route("UploadBankFile")]
        [HttpPost]
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
                    if (ext == ".xlsx" || ext == ".xls")
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
                        return ReadUploadBankFile(hssfwb, userid);
                    }
                    else
                    {
                        return Created("File extnsion required .xlsx", "File extnsion required .xlsx");
                    }
                }
            }

            return Created("Error", "Error");
        }

        public IHttpActionResult ReadUploadBankFile(XSSFWorkbook hssfwb, int userid)
        {

            string Msg = string.Empty;
            string sSheetName = hssfwb.GetSheetName(0);
            ISheet sheet = hssfwb.GetSheet(sSheetName);
            IRow rowData;
            ICell cellData = null;
            try
            {
                List<UploadBankFileDC> uploadBankFileDCs = new List<UploadBankFileDC>();
                int? TransactionDateCellIndex = null;
                int? transactionDescriptionCellIndex = null;
                int? ReferenceNumberCellIndex = null;
                int? ValueDateCellIndex = null;
                int? TransactionAmountCellIndex = null;
                int? typeCellIndex = null;

                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)
                {
                    if (iRowIdx == 0)
                    {
                        rowData = sheet.GetRow(iRowIdx);
                        if (rowData != null)
                        {
                            TransactionDateCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Transaction Date") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Transaction Date").ColumnIndex : (int?)null;
                            if (!TransactionDateCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Transaction Date does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            transactionDescriptionCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Transaction Description") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Transaction Description").ColumnIndex : (int?)null;
                            if (!transactionDescriptionCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Transaction Description does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            ReferenceNumberCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Reference No.") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Reference No.").ColumnIndex : (int?)null;
                            if (!ReferenceNumberCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Reference No. does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            ValueDateCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Value Date") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Value Date").ColumnIndex : (int?)null;
                            if (!ValueDateCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Value Date does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            TransactionAmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Transaction Amount") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Transaction Amount").ColumnIndex : (int?)null;
                            if (!TransactionAmountCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Transaction Amount does not exist..try again");
                                return Created(strJSON, strJSON);
                            }


                            typeCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Debit / Credit") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Debit / Credit").ColumnIndex : (int?)null;
                            if (!typeCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Debit / Credit does not exist..try again");
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
                            UploadBankFileDC uploadDC = new UploadBankFileDC();
                            try
                            {
                                //int requestTowarehouseId;
                                cellData = rowData.GetCell(TransactionDateCellIndex.Value);
                                Col0 = cellData == null ? "" : cellData.ToString();
                                uploadDC.TransactionDate = Convert.ToDateTime(Col0);
                                logger.Info("Transaction Date :" + uploadDC.TransactionDate);

                                cellData = rowData.GetCell(transactionDescriptionCellIndex.Value);
                                col1 = cellData == null ? "" : cellData.ToString();
                                uploadDC.TransactionDescription = Convert.ToString(col1);
                                logger.Info("TransactionDescription :" + uploadDC.TransactionDescription);

                                cellData = rowData.GetCell(ReferenceNumberCellIndex.Value);
                                col2 = cellData == null ? "" : cellData.ToString();
                                uploadDC.ReferenceNumber = Convert.ToString(col2);
                                logger.Info("ReferenceNumber :" + uploadDC.ReferenceNumber);

                                cellData = rowData.GetCell(ValueDateCellIndex.Value);
                                col3 = cellData == null ? "" : cellData.ToString();
                                uploadDC.ValueDate = Convert.ToDateTime(col3);
                                logger.Info("ValueDate :" + uploadDC.ValueDate);

                                cellData = rowData.GetCell(TransactionAmountCellIndex.Value);
                                col9 = cellData == null ? "" : cellData.ToString();
                                uploadDC.TransactionAmount = Convert.ToDouble(col9);
                                logger.Info("TransactionAmount :" + uploadDC.TransactionAmount);

                                //cellData = rowData.GetCell(warehouseCellIndex.Value);
                                //col8 = cellData == null ? "" : cellData.ToString();
                                //uploadDC.WarehouseName = Convert.ToString(col8);
                                //logger.Info("TransactionAmount :" + uploadDC.WarehouseName);

                                cellData = rowData.GetCell(typeCellIndex.Value);
                                col4 = cellData == null ? "" : cellData.ToString();
                                uploadDC.Type = Convert.ToString(col4);
                                logger.Info("Type :" + uploadDC.Type);


                                uploadBankFileDCs.Add(uploadDC);
                            }
                            catch (Exception ex)
                            {
                                msgitemname = ex.Message;
                                logger.Error("Error adding customer in collection " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                        }
                    }
                }
                if (uploadBankFileDCs != null || uploadBankFileDCs.Any())
                {

                    string ExcelSavePath = HttpContext.Current.Server.MapPath("~/UploadedFiles/OrderTwoOrderReconcillation");
                    if (!Directory.Exists(ExcelSavePath))
                        Directory.CreateDirectory(ExcelSavePath);

                    string fileUrl = "";
                    var BuyerForcastDt = ClassToDataTable.CreateDataTable(uploadBankFileDCs);
                    BuyerForcastDt.TableName = "OrderReconcillation Upload Data";
                    var fileName = "OrderReconcillation" + DateTime.Now.ToString("yyyyddMHHmmss") + ".xlsx";
                    string filePath = ExcelSavePath + "\\" + fileName;
                    ExcelGenerator.DataTable_To_Excel(new List<DataTable> { BuyerForcastDt }, filePath);
                    fileUrl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                            , HttpContext.Current.Request.Url.DnsSafeHost
                                                            , HttpContext.Current.Request.Url.Port
                                                            , string.Format("UploadedFiles/OrderTwoOrderReconcillation/{0}", fileName));


                    List<OrderReconcillationBankDetail> id = ReadUploadBankFiles(uploadBankFileDCs, userid);
                    string a = "";
                    if (id.Count() > 0)
                    {
                        a = id[0].TransactionDescription.ToString() != null ? "File Uploaded Successfully" : "Data Already Exists";
                    }
                    else
                    {
                        a = "Data Already Exists";
                    }

                    return Created(a, a);
                }

            }
            catch (Exception ex)
            {
                logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
            }
            return Created("Error", "Error"); ;

        }

        public List<OrderReconcillationBankDetail> ReadUploadBankFiles(List<UploadBankFileDC> trCollection, int userid)
        {
            using (var context = new AuthContext())
            {
                DateTime cdate = DateTime.Now;
                List<OrderReconcillationBankDetail> List = new List<OrderReconcillationBankDetail>();
                var bankdata = context.orderReconcillationBankDetails.Where(x => x.IsActive).ToList();
                var CurrencySettlementSourcedata = context.CurrencySettlementSource.Where(x => x.IsActive == true && x.TotalCashAmt > 0).ToList();
                foreach (var data in trCollection)
                {
                    int WarehouseId = 0;
                    string mop = "Other";
                    string transaction = data.TransactionDescription.ToUpper();
                    if (transaction.StartsWith("76026666TERMINAL 1 CARDS SETTL") || transaction.StartsWith("74035993TERMINAL 1 CARDS SETTL"))
                    {
                        mop = "HDFC_CCDC";
                    }
                    else if (transaction.StartsWith("UPI SETTLEMENT -IQ4835"))
                    {
                        mop = "HDFC_UPI";
                    }
                    else if (transaction.Contains("INFIBEAM"))
                    {
                        mop = "HDFC_NetBanking";
                    }
                    else if (transaction.Contains("SKETPL") || transaction.Contains("ECMS"))
                    {
                        mop = "RTGS/NEFT";
                    }
                    else if (transaction.StartsWith("UPI SETTLEMENT -YT"))
                    {
                        mop = "UPI";
                    }
                    else if (transaction.StartsWith("CHQ DEP - MICR 8 CLEARING") || transaction.StartsWith("FT - CR -") || transaction.StartsWith("FT - SHOP KIRANA E TRADING PRIVATE Cr –") || transaction.StartsWith("CHQ DEP - TRANSFER OW 1"))
                    {
                        mop = "Cheque";
                    }
                    else if (transaction.Contains("CASH DEP") || transaction.StartsWith("BRN-SI-D1494"))
                    {
                        mop = "Cash";
                    }
                    else if (transaction.Contains("CHQ DEP RET CHGS"))
                    {
                        mop = "Cheque Return Charges";
                    }
                    else if (transaction.StartsWith("ACH DR"))
                    {
                        mop = "DirectUdhaar_Bank";
                    }
                    else if (transaction.StartsWith("UPI SETTLEMENT -CJM162-"))
                    {
                        mop = "DirectUdhaar_UPI";
                    }

                    if (mop == "Cash")
                    {
                        if (transaction.StartsWith("BRN-SI-D1494"))
                        {
                            data.TransactionDate = data.TransactionDate.Date;
                            var waredata = CurrencySettlementSourcedata.Where(x => x.TotalCashAmt == Convert.ToDecimal(data.TransactionAmount) && x.SettlementSource == "AXIS BANK LIMITED" && x.IsActive == true && x.TotalCashAmt > 0
                            && (x.SettlementDate.Date == data.TransactionDate || x.SettlementDate.Date.AddDays(1) == data.TransactionDate || x.SettlementDate.Date.AddDays(2) == data.TransactionDate || x.SettlementDate.Date.AddDays(3) == data.TransactionDate)).FirstOrDefault();//context.CurrencySettlementSource.FirstOrDefault(x => x.TotalCashAmt == Convert.ToDecimal(data.TransactionAmount) && x.SettlementDate == data.TransactionDate);
                            if (waredata != null)
                            {
                                WarehouseId = waredata.Warehouseid;
                            }
                        }
                        else
                        {
                            data.TransactionDate = data.TransactionDate.Date;
                            var waredata = CurrencySettlementSourcedata.Where(x => x.TotalCashAmt == Convert.ToDecimal(data.TransactionAmount) && x.SettlementSource == "HDFC BANK LIMITED" && x.IsActive == true && x.TotalCashAmt > 0
                            && (x.SettlementDate.Date == data.TransactionDate || x.SettlementDate.Date.AddDays(1) == data.TransactionDate || x.SettlementDate.Date.AddDays(2) == data.TransactionDate || x.SettlementDate.Date.AddDays(3) == data.TransactionDate)).FirstOrDefault();//context.CurrencySettlementSource.FirstOrDefault(x => x.TotalCashAmt == Convert.ToDecimal(data.TransactionAmount) && x.SettlementDate == data.TransactionDate);
                            if (waredata != null)
                            {
                                WarehouseId = waredata.Warehouseid;
                            }
                        }

                    }
                    var isupload = bankdata.FirstOrDefault(x => x.TransactionDate == data.TransactionDate);
                    if (isupload == null)
                    {
                        OrderReconcillationBankDetail order = new OrderReconcillationBankDetail();
                        order.TransactionDate = data.TransactionDate;
                        order.CreatedBy = userid;
                        order.CreatedDate = cdate;
                        order.IsProcess = false;
                        order.IsActive = true;
                        order.IsDeleted = false;
                        order.TransactionDescription = data.TransactionDescription;
                        order.TransactionAmount = data.TransactionAmount;
                        order.RemainingAmount = data.TransactionAmount;
                        order.SettledAmount = 0;
                        order.Status = "Not Verified";
                        order.ModeofPayment = mop;
                        order.WarehouseId = WarehouseId;
                        order.RefrenceNumber = data.ReferenceNumber;
                        order.ValueDate = data.ValueDate;
                        order.Type = data.Type;
                        List.Add(order);
                    }
                    //}
                }
                if (List != null && List.Any())
                {
                    context.orderReconcillationBankDetails.AddRange(List);
                    context.Commit();
                }
                return List;
            }
        }

        [Route("UploadUpiFile")]
        [HttpPost]
        public IHttpActionResult UploadUpiFile()
        {
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                DateTime cdate = DateTime.Now;
                var formData1 = HttpContext.Current.Request.Form["compid"];
                // Access claims
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                // Get the uploaded image from the Files collection
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                string MSG = "";
                if (httpPostedFile != null)
                {
                    using (AuthContext context = new AuthContext())
                    {
                        string ext = Path.GetExtension(httpPostedFile.FileName);
                        if (ext == ".xlsx" || ext == ".xls")
                        {
                            var upidata = context.OrderReconcillationFileUploadDetails.Where(x => x.IsActive == true && x.IsDeleted == false && x.ModeOfPayment == "UPI").ToList();
                            string path = HttpContext.Current.Server.MapPath("~/UploadedFiles/OrderTwoOrderReconcillation");
                            string a1, b;

                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                            a1 = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + httpPostedFile.FileName;
                            b = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles/OrderTwoOrderReconcillation/"), a1);
                            httpPostedFile.SaveAs(b);

                            byte[] buffer = new byte[httpPostedFile.ContentLength];

                            using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
                            {
                                br.Read(buffer, 0, buffer.Length);
                            }
                            XSSFWorkbook hssfwb;
                            List<UploadUPIFileDC> uploaditemlist = new List<UploadUPIFileDC>();
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

                                int? TransactionDateCellIndex = null;
                                int? MOPCellIndex = null;
                                int? TransactionReferenceNumberCellIndex = null;
                                int? TransactionAmountCellIndex = null;
                                int? SettlementStatusCellIndex = null;
                                int? TransactionTypeCellIndex = null;
                                int? TransactionStatusCellIndex = null;


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
                                            TransactionDateCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Transaction Date") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Transaction Date").ColumnIndex : (int?)null;
                                            if (!TransactionDateCellIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Transaction Date does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }

                                            TransactionReferenceNumberCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Trans Ref No.") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Trans Ref No.").ColumnIndex : (int?)null;

                                            if (!TransactionReferenceNumberCellIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Trans Ref No. does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }

                                            TransactionAmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Transaction Amount") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Transaction Amount").ColumnIndex : (int?)null;

                                            if (!TransactionAmountCellIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Transaction Amount does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }

                                            SettlementStatusCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Settlement Status") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Settlement Status").ColumnIndex : (int?)null;

                                            if (!SettlementStatusCellIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Settlement Status does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }

                                            TransactionTypeCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "DR/CR") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "DR/CR").ColumnIndex : (int?)null;

                                            if (!TransactionTypeCellIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("DR/CR does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }
                                            TransactionStatusCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Transaction Status") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Transaction Status").ColumnIndex : (int?)null;

                                            if (!TransactionStatusCellIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Transaction Status does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }


                                        }

                                    }
                                    else
                                    {

                                        UploadUPIFileDC uploadDC = new UploadUPIFileDC();
                                        rowData = sheet.GetRow(iRowIdx);

                                        if (rowData != null)
                                        {
                                            var c = rowData.GetCell(0);
                                            cellData = rowData.GetCell(TransactionTypeCellIndex.Value);
                                            col9 = cellData == null ? "" : cellData.ToString();
                                            uploadDC.TransactionType = Convert.ToString(col9);
                                            logger.Info("Transaction Type :" + uploadDC.TransactionType);

                                            cellData = rowData.GetCell(TransactionStatusCellIndex.Value);
                                            col7 = cellData == null ? "" : cellData.ToString();
                                            uploadDC.TransactionType = Convert.ToString(col7);
                                            logger.Info("Transaction Status :" + uploadDC.TransactionStatus);


                                            //if (!string.IsNullOrEmpty(col9) && col9 == "CREDIT" && !string.IsNullOrEmpty(col7) && col7 == "SUCCESS")
                                            //{
                                            cellData = rowData.GetCell(TransactionDateCellIndex.Value);
                                            col12 = cellData == null ? "" : cellData.ToString();
                                            uploadDC.TransactionDate = Convert.ToDateTime(col12);
                                            logger.Info("Transaction Date :" + uploadDC.TransactionDate);

                                            cellData = rowData.GetCell(TransactionReferenceNumberCellIndex.Value);
                                            col5 = cellData == null ? "" : cellData.ToString();
                                            uploadDC.ReferenceNumber = Convert.ToString(col5);
                                            logger.Info("ReferenceNumber :" + uploadDC.ReferenceNumber);

                                            cellData = rowData.GetCell(TransactionAmountCellIndex.Value);

                                            logger.Info("Transaction Amount :" + uploadDC.TransactionAmount); cellData = rowData.GetCell(TransactionAmountCellIndex.Value);
                                            col11 = cellData == null ? "" : cellData.ToString();
                                            uploadDC.TransactionAmount = Convert.ToDouble(col11);

                                            logger.Info("Remaining Amount :" + uploadDC.TransactionAmount); cellData = rowData.GetCell(TransactionAmountCellIndex.Value);
                                            col11 = cellData == null ? "" : cellData.ToString();
                                            uploadDC.RemainingAmount = Convert.ToDouble(col11);

                                            //logger.Info("RemainingAmount Amount :" + uploadDC.TransactionAmount); cellData = rowData.GetCell(TransactionAmountCellIndex.Value);
                                            //col11 = cellData == null ? "" : cellData.ToString();
                                            //uploadDC.RemainingAmount = Convert.ToDouble(col11);

                                            uploadDC.MOP = "UPI";
                                            uploadDC.OrderId = null;
                                            uploadDC.IsProcess = false;
                                            uploadDC.Status = "Not Verified";
                                            uploadDC.WarehouseId = null;
                                            uploadDC.Charges = 0;
                                            uploadDC.TransactionDescription = null;
                                            uploadDC.SettledAmount = 0;

                                            var isdataexists = upidata.FirstOrDefault(x => x.RefNo == uploadDC.ReferenceNumber);
                                            if (isdataexists == null)
                                            {
                                                uploaditemlist.Add(uploadDC);
                                            }
                                            //}
                                            //else
                                            //{
                                            //    MSG = "TransactionStatus aur Transaction type not valid"; return Created(MSG, MSG);
                                            //}
                                        }
                                    }
                                }
                            }

                            if (uploaditemlist != null && uploaditemlist.Any())
                            {
                                if (uploaditemlist != null && uploaditemlist.Any())
                                {
                                    List<OrderReconcillationFileUploadDetail> orderReconcillationFileUploadDetailList = new List<OrderReconcillationFileUploadDetail>();
                                    foreach (var data in uploaditemlist)
                                    {
                                        OrderReconcillationFileUploadDetail orderReconcillationFileUploadDetail = new OrderReconcillationFileUploadDetail();
                                        orderReconcillationFileUploadDetail.TransactionDescription = null;
                                        orderReconcillationFileUploadDetail.TransactionDate = data.TransactionDate;
                                        orderReconcillationFileUploadDetail.ModeOfPayment = data.MOP;
                                        orderReconcillationFileUploadDetail.RefNo = data.ReferenceNumber;
                                        orderReconcillationFileUploadDetail.GullakReferenceNumber = null;
                                        orderReconcillationFileUploadDetail.TransactionAmount = data.TransactionAmount;
                                        orderReconcillationFileUploadDetail.RemainingAmount = data.TransactionAmount;
                                        orderReconcillationFileUploadDetail.SettledAmount = 0;
                                        orderReconcillationFileUploadDetail.OrderId = null;
                                        orderReconcillationFileUploadDetail.IsProcess = false;
                                        orderReconcillationFileUploadDetail.Status = "Not Verified";
                                        orderReconcillationFileUploadDetail.WarehouseId = null;
                                        orderReconcillationFileUploadDetail.Charges = 0;
                                        orderReconcillationFileUploadDetail.CreatedBy = userid;
                                        orderReconcillationFileUploadDetail.CreatedDate = cdate;
                                        orderReconcillationFileUploadDetail.IsActive = true;
                                        orderReconcillationFileUploadDetail.IsDeleted = false;
                                        orderReconcillationFileUploadDetailList.Add(orderReconcillationFileUploadDetail);
                                    }
                                    context.OrderReconcillationFileUploadDetails.AddRange(orderReconcillationFileUploadDetailList);

                                    if (context.Commit() > 0)
                                    {
                                        MSG = "File Saved Sucessfully"; return Created(MSG, MSG);
                                        ;
                                    }
                                    else { MSG = "Failed To Save"; return Created(MSG, MSG); }
                                }
                            }
                            else
                            {
                                MSG = "Data Already Exists"; return Created(MSG, MSG);
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
            else
            {
                return Created("Error", "Error");
            }
        }

        [Route("UploadHDFCFile")]
        [HttpPost]
        public IHttpActionResult UploadHDFCFile()
        {
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                DateTime cdate = DateTime.Now;
                var formData1 = HttpContext.Current.Request.Form["compid"];
                // Access claims
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                // Get the uploaded image from the Files collection
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                string MSG = "";
                using (AuthContext context = new AuthContext())
                {
                    if (httpPostedFile != null)
                    {
                        string ext = Path.GetExtension(httpPostedFile.FileName);
                        if (ext == ".xlsx" || ext == ".xls")
                        {
                            var hdfcdata = context.OrderReconcillationFileUploadDetails.Where(x => x.IsActive == true && x.IsDeleted == false && (x.ModeOfPayment == "HDFC_NetBanking" || x.ModeOfPayment == "HDFC_CCDC" || x.ModeOfPayment == "HDFC_UPI")).ToList();
                            string path = HttpContext.Current.Server.MapPath("~/UploadedFiles/OrderTwoOrderReconcillation");
                            string a1, b;

                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                            a1 = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + httpPostedFile.FileName;
                            b = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles/OrderTwoOrderReconcillation/"), a1);
                            httpPostedFile.SaveAs(b);

                            byte[] buffer = new byte[httpPostedFile.ContentLength];

                            using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
                            {
                                br.Read(buffer, 0, buffer.Length);
                            }
                            XSSFWorkbook hssfwb;
                            List<UploadUPIFileDC> uploaditemlist = new List<UploadUPIFileDC>();
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

                                int? TransactionDateCellIndex = null;
                                int? MOPCellIndex = null;
                                int? TransactionReferenceNumberCellIndex = null;
                                int? TransactionAmountCellIndex = null;
                                int? SettlementStatusCellIndex = null;
                                int? PaymentModeCellIndex = null;
                                int? TransactionStatusCellIndex = null;
                                int? OrderNoCellIndex = null;
                                int? GrossAmountCellIndex = null;
                                int? FeeFlatCellIndex = null;
                                int? TaxCellIndex = null;
                                int? ReferencenumbercellIndex = null;
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
                                            TransactionDateCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Order Datetime") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Order Datetime").ColumnIndex : (int?)null;
                                            if (!TransactionDateCellIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Order Datetime does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }
                                            PaymentModeCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Payment Mode") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Payment Mode").ColumnIndex : (int?)null;

                                            if (!PaymentModeCellIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("MOP does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }
                                            ReferencenumbercellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "CCAvenue Ref#") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "CCAvenue Ref#").ColumnIndex : (int?)null;

                                            if (!ReferencenumbercellIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Refrencenumber does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }



                                            OrderNoCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Order No") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Order No").ColumnIndex : (int?)null;

                                            if (!OrderNoCellIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Order No. cell does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }

                                            GrossAmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Gross Amount") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Gross Amount").ColumnIndex : (int?)null;

                                            if (!GrossAmountCellIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("GrossAmount cell does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }


                                            FeeFlatCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Fee Flat") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Fee Flat").ColumnIndex : (int?)null;
                                            TaxCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Tax") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Tax").ColumnIndex : (int?)null;

                                        }

                                    }
                                    else
                                    {

                                        UploadUPIFileDC uploadDC = new UploadUPIFileDC();
                                        rowData = sheet.GetRow(iRowIdx);

                                        if (rowData != null)
                                        {


                                            cellData = rowData.GetCell(TransactionDateCellIndex.Value);
                                            col12 = cellData == null ? "" : cellData.ToString();
                                            uploadDC.TransactionDate = Convert.ToDateTime(col12);
                                            logger.Info("Transaction Date :" + uploadDC.TransactionDate);

                                            uploadDC.ReferenceNumber = rowData.GetCell(ReferencenumbercellIndex.Value).ToString();

                                            cellData = rowData.GetCell(OrderNoCellIndex.Value);
                                            col11 = cellData == null ? "" : cellData.ToString();
                                            if (col11.Length == 7)
                                            {
                                                uploadDC.GullakReferenceNumber = "";
                                            }
                                            else
                                            {
                                                uploadDC.GullakReferenceNumber = col11;
                                            }
                                            logger.Info("ReferenceNumber :" + uploadDC.ReferenceNumber);


                                            cellData = rowData.GetCell(PaymentModeCellIndex.Value);
                                            logger.Info("Payment Mode :" + uploadDC.MOP);
                                            col13 = cellData == null ? "" : cellData.ToString();

                                            if (cellData.ToString() == "Credit Card" || cellData.ToString() == "Debit Card")
                                            {
                                                uploadDC.MOP = "HDFC_CCDC";
                                            }
                                            else if (col13 == "Unified Payments")
                                            {
                                                uploadDC.MOP = "HDFC_UPI";

                                            }
                                            else
                                            //if (col13 == "Net Banking")
                                            {
                                                uploadDC.MOP = "HDFC_NetBanking";

                                            }


                                            cellData = rowData.GetCell(PaymentModeCellIndex.Value);//transaction amount
                                            logger.Info("Order No:" + uploadDC.OrderId);
                                            if (uploadDC.MOP == "HDFC_CCDC" || uploadDC.MOP == "HDFC_UPI")
                                            {
                                                uploadDC.TransactionAmount = Convert.ToDouble(rowData.GetCell(GrossAmountCellIndex.Value).ToString());
                                            }
                                            else
                                            {
                                                //uploadDC.TransactionAmount = Convert.ToDouble(rowData.GetCell(GrossAmountCellIndex.Value).ToString()) - (Convert.ToDouble(rowData.GetCell(FeeFlatCellIndex.Value).ToString()) + Convert.ToDouble(rowData.GetCell(TaxCellIndex.Value).ToString()));
                                                uploadDC.TransactionAmount = Convert.ToDouble(rowData.GetCell(GrossAmountCellIndex.Value).ToString());
                                            }

                                            cellData = rowData.GetCell(OrderNoCellIndex.Value);  //order Id
                                            logger.Info("Order No:" + uploadDC.OrderId);
                                            if (cellData != null && cellData.ToString().Length == 7)
                                            {
                                                uploadDC.OrderId = Convert.ToInt32(rowData.GetCell(OrderNoCellIndex.Value).ToString());
                                            }
                                            else uploadDC.OrderId = null;

                                            uploadDC.RemainingAmount = uploadDC.TransactionAmount;

                                            uploadDC.IsProcess = false;
                                            uploadDC.Status = "Not Verified";
                                            uploadDC.SettledAmount = 0;
                                            uploadDC.WarehouseId = null;

                                            cellData = rowData.GetCell(PaymentModeCellIndex.Value);//Charges
                                            logger.Info("Order No:" + uploadDC.OrderId);
                                            if (cellData != null && uploadDC.MOP == "HDFC_NetBanking")
                                            {
                                                uploadDC.Charges = (Convert.ToDouble(rowData.GetCell(FeeFlatCellIndex.Value).ToString()) + Convert.ToDouble(rowData.GetCell(TaxCellIndex.Value).ToString()));
                                            }
                                            else { uploadDC.Charges = 0; }

                                            uploadDC.TransactionDescription = null;
                                            uploadDC.IsgullakVerified = false;

                                            if (uploadDC.ReferenceNumber.Length > 0)
                                            {
                                                var isdataexists = hdfcdata.FirstOrDefault(x => x.RefNo == uploadDC.ReferenceNumber);
                                                if (isdataexists == null)
                                                {
                                                    uploaditemlist.Add(uploadDC);
                                                }
                                            }
                                            else
                                            {
                                                var isdataexists = hdfcdata.FirstOrDefault(x => x.GullakReferenceNumber == uploadDC.GullakReferenceNumber);
                                                if (isdataexists == null)
                                                {
                                                    uploaditemlist.Add(uploadDC);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (uploaditemlist != null && uploaditemlist.Any())
                            {
                                if (uploaditemlist != null && uploaditemlist.Any())
                                {
                                    List<OrderReconcillationFileUploadDetail> orderReconcillationFileUploadDetailList = new List<OrderReconcillationFileUploadDetail>();

                                    foreach (var data in uploaditemlist)
                                    {
                                        OrderReconcillationFileUploadDetail orderReconcillationFileUploadDetail = new OrderReconcillationFileUploadDetail();
                                        orderReconcillationFileUploadDetail.TransactionDescription = null;
                                        orderReconcillationFileUploadDetail.TransactionDate = data.TransactionDate;
                                        orderReconcillationFileUploadDetail.ModeOfPayment = data.MOP;
                                        orderReconcillationFileUploadDetail.RefNo = data.ReferenceNumber;
                                        orderReconcillationFileUploadDetail.GullakReferenceNumber = data.GullakReferenceNumber;
                                        orderReconcillationFileUploadDetail.TransactionAmount = data.TransactionAmount;
                                        orderReconcillationFileUploadDetail.RemainingAmount = data.RemainingAmount;
                                        orderReconcillationFileUploadDetail.SettledAmount = 0;
                                        orderReconcillationFileUploadDetail.OrderId = data.OrderId;
                                        orderReconcillationFileUploadDetail.IsProcess = data.IsProcess;
                                        orderReconcillationFileUploadDetail.Status = data.Status;
                                        orderReconcillationFileUploadDetail.WarehouseId = null;
                                        orderReconcillationFileUploadDetail.Charges = data.Charges;
                                        orderReconcillationFileUploadDetail.CreatedBy = userid;
                                        orderReconcillationFileUploadDetail.CreatedDate = DateTime.Now;
                                        orderReconcillationFileUploadDetail.IsActive = true;
                                        orderReconcillationFileUploadDetail.IsDeleted = false;
                                        orderReconcillationFileUploadDetail.IsGullakVerified = data.IsgullakVerified;
                                        orderReconcillationFileUploadDetailList.Add(orderReconcillationFileUploadDetail);
                                    }
                                    context.OrderReconcillationFileUploadDetails.AddRange(orderReconcillationFileUploadDetailList);

                                    if (context.Commit() > 0)
                                    {
                                        MSG = "File Saved Sucessfully"; return Created(MSG, MSG);
                                    }
                                    else { MSG = "Failed To Save"; return Created(MSG, MSG); }
                                }
                            }
                            else
                            {
                                MSG = "Data Already Exists"; return Created(MSG, MSG);
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
            else
            {
                return Created("Error", "Error");
            }
        }

        [Route("UpdatingSettledOrder")] // --5min job
        [HttpGet]
        public bool UpdatingSettledOrder()
        {
            bool res = false;
            OrderReconcillationHelper orderReconcillationHelper = new OrderReconcillationHelper();
            res = orderReconcillationHelper.UpdatingSettledOrders();
            return res;
        }

        [Route("Notverifiedtoverifiedorder")]
        [HttpGet]
        public bool Notverifiedtoverifiedorder()
        {
            bool res = false;
            OrderReconcillationHelper orderReconcillationHelper = new OrderReconcillationHelper();
            res = orderReconcillationHelper.Notverifiedtoverifiedorders();
            return res;
        }

        [Route("MistoBankVerified")]
        [HttpGet]
        public bool MistoBankVerified()
        {
            bool res = false;
            OrderReconcillationHelper orderReconcillationHelper = new OrderReconcillationHelper();
            res = orderReconcillationHelper.MistoBankVerifieds();
            return res;
        }


        #endregion

        #region  Priyanka
        [Route("GetBankStatementFileDetails")]
        [HttpPost]
        [AllowAnonymous]
        public APIResponse GetBankStatementFileDetails(GetBankdetailPayload payload)
        {
            using (AuthContext context = new AuthContext())
            {
                if (payload.Keyword == "null" || payload.Keyword == "undefined") payload.Keyword = null;
                var status = new SqlParameter("@Status", payload.Status);
                var skip = new SqlParameter("@Skip", payload.Skip);
                var take = new SqlParameter("@Take", payload.Take);
                var fdate = new SqlParameter("@Fromdate", payload.Fromdate ?? (object)DBNull.Value);
                var tdate = new SqlParameter("@Todate", payload.Todate ?? (object)DBNull.Value);
                var keyword = new SqlParameter("@Keyword", payload.Keyword != null ? payload.Keyword : (object)DBNull.Value);
                var Type = new SqlParameter("@Type", payload.Type != null ? payload.Type : (object)DBNull.Value);
                var isexport = new SqlParameter("@IsExport", payload.isExport);
                var data = context.Database.SqlQuery<BankStatementFileDetailsListDC>("GetBankStatementFileDetails @Status,@Skip,@Take,@Fromdate,@Todate,@Keyword,@Type,@IsExport", status, skip, take, fdate,tdate, keyword,Type, isexport).ToList();
                if (data != null && data.Count > 0)
                    return new APIResponse { Status = true, Data = data };
                else
                    return new APIResponse { Status = false, Message = "Data Not Found!" };
            }

        }

        [HttpPut]
        [Route("UpdateBankResonId")]
        [AllowAnonymous]

        public APIResponse UpdateBankResonId(int id, string Reason, string comment)
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var BankData = context.orderReconcillationBankDetails.FirstOrDefault(x => x.Id == id);
                if (BankData != null)
                {
                    if (BankData.Status == "Not Verified")
                    {
                        BankData.Update_Reason = Reason;
                        BankData.IsProcess = true;
                        BankData.Status = "Verified";
                        BankData.Update_Comment = comment;
                        BankData.ModifiedBy = userid;
                        BankData.ModifiedDate = DateTime.Now;
                        context.Entry(BankData).State = System.Data.Entity.EntityState.Modified;
                    }
                    if (context.Commit() > 0)
                    {
                        return new APIResponse { Status = true, Message = "Update Successfully!" };
                    }
                    else
                    {
                        return new APIResponse { Status = false, Message = "Data Not Updated!!" };
                    }

                }
                return new APIResponse { Status = false, Message = "No Data Found!" };

            }
        }

        [HttpPost]
        [Route("GetOrderToOrderReconcilationHistories")]
        [AllowAnonymous]

        public List<ShowReconcillationHistoryDC> GetOrderToOrderReconcilationHistories(int orderid, string mop)
        {

            List<ShowReconcillationHistoryDC> historydata = new List<ShowReconcillationHistoryDC>();
            using (var context = new AuthContext())
            {
                int userid = 0;
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                var HistoryList = context.orderReconcillationHistories.Where(x => x.OrderId == orderid && x.ModeofPayment == mop).ToList();
                if (HistoryList.Count > 0 && HistoryList.Any())
                {
                    foreach (var data in HistoryList)
                    {
                        ShowReconcillationHistoryDC a = new ShowReconcillationHistoryDC();
                        a.Status = data.Status;
                        a.ModeOfPayment = data.ModeofPayment;
                        a.UpdatedBy = data.CreatedBy == 1 ? "System" : context.Peoples.FirstOrDefault(x => x.PeopleID == data.CreatedBy).DisplayName;
                        a.UpdatedDate = data.CreatedDate;
                        historydata.Add(a);
                    }
                }

            }
            return historydata;
        }

        [Route("GetOrderToOrderReconcillationList")]
        [HttpPost]
        [AllowAnonymous]
        public List<GetOrderToOrderReconcilationListDC> GetOrderToOrderReconcillationList(OrderReconcillationfilterDc reconcillationfilterDc)
        {
            var result = new List<GetOrderToOrderReconcilationListDC>();
            using (var context = new AuthContext())
            {
                if (reconcillationfilterDc != null)
                {

                    var Widlist = new DataTable();
                    Widlist.Columns.Add("IntValue");
                    foreach (var obj in reconcillationfilterDc.WarehouseIds)
                    {
                        var dr = Widlist.NewRow();
                        dr["IntValue"] = obj;
                        Widlist.Rows.Add(dr);
                    }
                    var param2 = new SqlParameter("@WarehouseIds", Widlist);
                    param2.SqlDbType = SqlDbType.Structured;
                    param2.TypeName = "dbo.IntValues";
                    var param3 = new SqlParameter("@FromDate", reconcillationfilterDc.FromDate == null ? DBNull.Value : (object)reconcillationfilterDc.FromDate);
                    var param4 = new SqlParameter("@ToDate", reconcillationfilterDc.ToDate == null ? DBNull.Value : (object)reconcillationfilterDc.ToDate);
                    var param5 = new SqlParameter("@ReconStatus", reconcillationfilterDc.ReconStatus == null ? DBNull.Value : (object)reconcillationfilterDc.ReconStatus);
                    var param6 = new SqlParameter("@MOP", reconcillationfilterDc.MOP == null ? DBNull.Value : (object)reconcillationfilterDc.MOP);
                    var param7 = new SqlParameter("@keyword", reconcillationfilterDc.keyword == null ? DBNull.Value : (object)reconcillationfilterDc.keyword);
                    var param8 = new SqlParameter("@skip", reconcillationfilterDc.skip);
                    var param9 = new SqlParameter("@take", reconcillationfilterDc.take);
                    result = context.Database.SqlQuery<GetOrderToOrderReconcilationListDC>("exec GetOrderToOrderReconciliationList @WarehouseIds, @FromDate,@ToDate,@ReconStatus,@MOP,@keyword,@skip,@take", param2, param3, param4, param5, param6, param7, param8, param9).ToList();

                }
            }

            return result;
        }

        [HttpPut]
        [Route("UpdateOrderReconcilationIdWise")]
        [AllowAnonymous]
        public APIResponse UpdateOrderReconcilationIdWise(int id, string comment, string status)
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var BankData = context.OrderReconcillationDetails.FirstOrDefault(x => x.Id == id);
                if (BankData != null)
                {
                    if (BankData.Status == "Not Verified" || BankData.Status == "Partially Verified")
                    {
                        OrderReconcillationHistory hist = new OrderReconcillationHistory();
                        if (status == "Verified")
                        {
                            BankData.IsProcess = true;
                        }
                        BankData.Status = status;
                        BankData.Comment = comment;
                        BankData.ModifiedBy = userid;
                        BankData.ModifiedDate = DateTime.Now;
                        hist.OrderId = BankData.OrderId;
                        hist.ModeofPayment = BankData.ModeofPayment;
                        hist.Status = BankData.Status;
                        hist.CreatedBy = userid;
                        hist.CreatedDate = DateTime.Now;
                        hist.IsActive = true;
                        hist.IsDeleted = false;
                        context.orderReconcillationHistories.Add(hist);
                        context.Entry(BankData).State = System.Data.Entity.EntityState.Modified;
                    }
                    if (context.Commit() > 0)
                    {
                        return new APIResponse { Status = true, Message = "Update Successfully!" };
                    }
                    else
                    {
                        return new APIResponse { Status = false, Message = "Data Not Updated!!" };
                    }

                }
                return new APIResponse { Status = false, Message = "No Data Found!" };

            }
        }


        [HttpPost]
        [Route("GetOrderToOrderReconciliationDashboard")]
        [AllowAnonymous]
        public OrderToOrderReconciliationDashboardShowListDC GetOrderToOrderReconciliationDashboard(DashboardfilterDC dashboardfilterDC)
        {
            var result = new List<OrderToOrderReconciliationDashboardListDC>();
            OrderToOrderReconciliationDashboardShowListDC res = new OrderToOrderReconciliationDashboardShowListDC();
            using (var context = new AuthContext())
            {
                if (dashboardfilterDC.IsToday)
                {

                    dashboardfilterDC.StartDate = DateTime.Today;
                    dashboardfilterDC.EndDate = DateTime.Now;

                }
                else if (dashboardfilterDC.IsMonth)
                {
                    DateTime now = DateTime.Now;
                    DateTime firstDay = new DateTime(now.Year, now.Month, 1);
                    dashboardfilterDC.StartDate = firstDay;
                    dashboardfilterDC.EndDate = DateTime.Now;
                }

                if (result != null)
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandTimeout = 900;
                    cmd.CommandText = "[GetOrderToOrderReconciliationDashboard]";

                    var Widlist = new DataTable();
                    Widlist.Columns.Add("IntValue");
                    foreach (var obj in dashboardfilterDC.warehouseId)
                    {
                        var dr = Widlist.NewRow();
                        dr["IntValue"] = obj;
                        Widlist.Rows.Add(dr);
                    }
                    var param1 = new SqlParameter("@warehouseIds", Widlist);
                    param1.SqlDbType = SqlDbType.Structured;
                    param1.TypeName = "dbo.IntValues";
                    cmd.Parameters.Add(param1);
                    cmd.Parameters.Add(new SqlParameter("@StartDate", dashboardfilterDC.StartDate));
                    cmd.Parameters.Add(new SqlParameter("@EndDate", dashboardfilterDC.EndDate));
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    var reader = cmd.ExecuteReader();
                    result = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<OrderToOrderReconciliationDashboardListDC>(reader).ToList();
                    if (result.Any() && result.Count > 0)
                    {
                        var cashresult = result.Where(x => x.ModeOfPayment == "Cash").ToList();
                        if (cashresult.Count > 0)
                        {
                            if (cashresult.Where(x => x.Status == "Not Verified").Count() > 0)
                            {
                                res.CashNotVerifiedAmount = cashresult.FirstOrDefault(x => x.Status == "Not Verified").Amount;
                                res.CashNotVerifiedOrder = cashresult.FirstOrDefault(x => x.Status == "Not Verified").Orders;
                            }
                            if (cashresult.Where(x => x.Status == "Verified").Count() > 0)
                            {
                                res.CashVerifiedAmount = cashresult.FirstOrDefault(x => x.Status == "Verified").Amount;
                                res.CashVerifiedOrder = cashresult.FirstOrDefault(x => x.Status == "Verified").Orders;
                            }
                            if (cashresult.Where(x => x.Status == "Partially Verified").Count() > 0)
                            {
                                res.CashPartiallyVerifiedAmount = cashresult.FirstOrDefault(x => x.Status == "Partially Verified").Amount;
                                res.CashPartiallyVerifiedOrder = cashresult.FirstOrDefault(x => x.Status == "Partially Verified").Orders;
                            }

                        }
                        var chequeresult = result.Where(x => x.ModeOfPayment == "Cheque").ToList();
                        if (chequeresult.Count > 0)
                        {
                            if (chequeresult.Where(x => x.Status == "Not Verified").Count() > 0)
                            {
                                res.ChequeNotVerifiedAmount = chequeresult.FirstOrDefault(x => x.Status == "Not Verified").Amount;
                                res.ChequeNotVerifiedOrder = chequeresult.FirstOrDefault(x => x.Status == "Not Verified").Orders;
                            }
                            if (chequeresult.Where(x => x.Status == "Verified").Count() > 0)
                            {
                                res.ChequeVerifiedAmount = chequeresult.FirstOrDefault(x => x.Status == "Verified").Amount;
                                res.ChequeVerifiedOrder = chequeresult.FirstOrDefault(x => x.Status == "Verified").Orders;
                            }
                            if (chequeresult.Where(x => x.Status == "Partially Verified").Count() > 0)
                            {
                                res.ChequePartiallyVerifiedAmount = chequeresult.FirstOrDefault(x => x.Status == "Partially Verified").Amount;
                                res.ChequePartiallyVerifiedOrder = chequeresult.FirstOrDefault(x => x.Status == "Partially Verified").Orders;
                            }

                        }
                        var directudharresult = result.Where(x => x.ModeOfPayment == "DirectUdhar").ToList();
                        if (directudharresult.Count > 0)
                        {
                            if (directudharresult.Where(x => x.Status == "Not Verified").Count() > 0)
                            {
                                res.DirectUdharNotVerifiedAmount = directudharresult.FirstOrDefault(x => x.Status == "Not Verified").Amount;
                                res.DirectUdharNotVerifiedOrder = directudharresult.FirstOrDefault(x => x.Status == "Not Verified").Orders;
                            }
                            if (directudharresult.Where(x => x.Status == "Verified").Count() > 0)
                            {
                                res.DirectUdharVerifiedAmount = directudharresult.FirstOrDefault(x => x.Status == "Verified").Amount;
                                res.DirectUdharVerifiedOrder = directudharresult.FirstOrDefault(x => x.Status == "Verified").Orders;
                            }
                            if (directudharresult.Where(x => x.Status == "Partially Verified").Count() > 0)
                            {
                                res.DirectUdharPartiallyVerifiedAmount = directudharresult.FirstOrDefault(x => x.Status == "Partially Verified").Amount;
                                res.DirectUdharPartiallyVerifiedOrder = directudharresult.FirstOrDefault(x => x.Status == "Partially Verified").Orders;
                            }
                        }
                        var Gullakresult = result.Where(x => x.ModeOfPayment == "Gullak").ToList();
                        if (Gullakresult.Count > 0)
                        {
                            if (Gullakresult.Where(x => x.Status == "Not Verified").Count() > 0)
                            {
                                res.GullakNotVerifiedAmount = Gullakresult.FirstOrDefault(x => x.Status == "Not Verified").Amount;
                                res.GullakNotVerifiedOrder = Gullakresult.FirstOrDefault(x => x.Status == "Not Verified").Orders;
                            }
                            if (Gullakresult.Where(x => x.Status == "Verified").Count() > 0)
                            {
                                res.GullakVerifiedAmount = Gullakresult.FirstOrDefault(x => x.Status == "Verified").Amount;
                                res.GullakVerifiedOrder = Gullakresult.FirstOrDefault(x => x.Status == "Verified").Orders;
                            }
                            if (Gullakresult.Where(x => x.Status == "Partially Verified").Count() > 0)
                            {
                                res.GullakPartiallyVerifiedAmount = Gullakresult.FirstOrDefault(x => x.Status == "Partially Verified").Amount;
                                res.GullakPartiallyVerifiedOrder = Gullakresult.FirstOrDefault(x => x.Status == "Partially Verified").Orders;
                            }

                        }
                        var rtgsresult = result.Where(x => x.ModeOfPayment == "RTGS/NEFT").ToList();
                        if (rtgsresult.Count > 0)
                        {
                            if (rtgsresult.Where(x => x.Status == "Not Verified").Count() > 0)
                            {
                                res.RTGS_NEFTNotVerifiedAmount = rtgsresult.FirstOrDefault(x => x.Status == "Not Verified").Amount;
                                res.RTGS_NEFTNotVerifiedOrder = rtgsresult.FirstOrDefault(x => x.Status == "Not Verified").Orders;
                            }
                            if (rtgsresult.Where(x => x.Status == "Verified").Count() > 0)
                            {
                                res.RTGS_NEFTVerifiedAmount = rtgsresult.FirstOrDefault(x => x.Status == "Verified").Amount;
                                res.RTGS_NEFTVerifiedOrder = rtgsresult.FirstOrDefault(x => x.Status == "Verified").Orders;
                            }
                            if (rtgsresult.Where(x => x.Status == "Partially Verified").Count() > 0)
                            {
                                res.RTGS_NEFTPartiallyVerifiedAmount = rtgsresult.FirstOrDefault(x => x.Status == "Partially Verified").Amount;
                                res.RTGS_NEFTPartiallyVerifiedOrder = rtgsresult.FirstOrDefault(x => x.Status == "Partially Verified").Orders;
                            }

                        }
                        var hdfcresult = result.Where(x => x.ModeOfPayment == "hdfc").ToList();
                        if (hdfcresult.Count > 0)
                        {
                            if (hdfcresult.Where(x => x.Status == "Not Verified").Count() > 0)
                            {
                                res.hdfcNotVerifiedAmount = hdfcresult.FirstOrDefault(x => x.Status == "Not Verified").Amount;
                                res.hdfcNotVerifiedOrder = hdfcresult.FirstOrDefault(x => x.Status == "Not Verified").Orders;
                            }
                            if (hdfcresult.Where(x => x.Status == "Verified").Count() > 0)
                            {
                                res.hdfcVerifiedAmount = hdfcresult.FirstOrDefault(x => x.Status == "Verified").Amount;
                                res.hdfcVerifiedOrder = hdfcresult.FirstOrDefault(x => x.Status == "Verified").Orders;
                            }
                            if (hdfcresult.Where(x => x.Status == "Partially Verified").Count() > 0)
                            {
                                res.hdfcPartiallyVerifiedAmount = hdfcresult.FirstOrDefault(x => x.Status == "Partially Verified").Amount;
                                res.hdfcPartiallyVerifiedOrder = hdfcresult.FirstOrDefault(x => x.Status == "Partially Verified").Orders;
                            }
                        }
                        var upiresult = result.Where(x => x.ModeOfPayment == "UPI").ToList();
                        if (upiresult.Count > 0)
                        {
                            if (upiresult.Where(x => x.Status == "Not Verified").Count() > 0)
                            {
                                res.UPINotVerifiedAmount = upiresult.FirstOrDefault(x => x.Status == "Not Verified").Amount;
                                res.UPINotVerifiedOrder = upiresult.FirstOrDefault(x => x.Status == "Not Verified").Orders;
                            }
                            if (upiresult.Where(x => x.Status == "Verified").Count() > 0)
                            {
                                res.UPIVerifiedAmount = upiresult.FirstOrDefault(x => x.Status == "Verified").Amount;
                                res.UPIVerifiedOrder = upiresult.FirstOrDefault(x => x.Status == "Verified").Orders;
                            }
                            if (upiresult.Where(x => x.Status == "Partially Verified").Count() > 0)
                            {
                                res.UPIPartiallyVerifiedAmount = upiresult.FirstOrDefault(x => x.Status == "Partially Verified").Amount;
                                res.UPIPartiallyVerifiedOrder = upiresult.FirstOrDefault(x => x.Status == "Partially Verified").Orders;
                            }
                        }
                        res.TotalOrders = result.FirstOrDefault().TotalOrders > 0 ? result.FirstOrDefault().TotalOrders : 0;
                        res.TotalAmount = result.FirstOrDefault().TotalAmount > 0 ? result.FirstOrDefault().TotalAmount : 0;
                        res.TotalPartialVerifiedOrder = result.FirstOrDefault().PartialVerifiedOrder > 0 ? result.FirstOrDefault().PartialVerifiedOrder : 0;
                        res.TotalPartialVerifiedAmount = result.FirstOrDefault().PartialVerifiedAmount > 0 ? result.FirstOrDefault().PartialVerifiedAmount : 0;
                        res.TotalVerifiedOrder = result.FirstOrDefault().VerifiedOrder > 0 ? result.FirstOrDefault().VerifiedOrder : 0;
                        res.TotalVerifiedAmount = result.FirstOrDefault().VerifiedAmount > 0 ? result.FirstOrDefault().VerifiedAmount : 0;
                        res.TotalNotVerifiedOrder = result.FirstOrDefault().NotVerifiedOrder > 0 ? result.FirstOrDefault().NotVerifiedOrder : 0;
                        res.TotalNotVerifiedAmount = result.FirstOrDefault().NotVerifiedAmount > 0 ? result.FirstOrDefault().NotVerifiedAmount : 0;
                        res.RefundAmount = result.FirstOrDefault().RefundAmount > 0 ? result.FirstOrDefault().RefundAmount : 0;
                        res.ExcessAmount = result.FirstOrDefault().ExcessAmount > 0 ? result.FirstOrDefault().ExcessAmount : 0;
                    }
                }
            }
            return res;
        }

        [HttpPost]
        [Route("ExportorderReconcilationDashboard")]
        public HttpResponseMessage ExportorderReconcilationDashboard(DashboardfilterDC dashboardfilterDC)
        {
            List<OrderToOrderReconciliationDashboardListDC> result = new List<OrderToOrderReconciliationDashboardListDC>();
            using (var context = new AuthContext())
            {
                if (dashboardfilterDC != null)
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandTimeout = 900;
                    cmd.CommandText = "[GetOrderToOrderReconciliationDashboard]";

                    var Widlist = new DataTable();
                    Widlist.Columns.Add("IntValue");
                    foreach (var obj in dashboardfilterDC.warehouseId)
                    {
                        var dr = Widlist.NewRow();
                        dr["IntValue"] = obj;
                        Widlist.Rows.Add(dr);
                    }
                    var param1 = new SqlParameter("@warehouseIds", Widlist);
                    param1.SqlDbType = SqlDbType.Structured;
                    param1.TypeName = "dbo.IntValues";
                    cmd.Parameters.Add(param1);
                    cmd.Parameters.Add(new SqlParameter("@StartDate", dashboardfilterDC.StartDate));
                    cmd.Parameters.Add(new SqlParameter("@EndDate", dashboardfilterDC.EndDate));
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    var reader = cmd.ExecuteReader();
                    result = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<OrderToOrderReconciliationDashboardListDC>(reader).ToList();

                }
            }
            if (result.Any() && result.Count > 0)
            {
                var response = new
                {
                    Status = true,
                    Message = "Data Exists",
                    data = result
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            else
            {
                var response = new
                {
                    Status = false,
                    Message = "Data Not Found"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }

        }

        [Route("ExportOrderToOrderReconcillationList")]
        [HttpPost]
        [AllowAnonymous]
        public List<ExportorderListData> ExportOrderToOrderReconcillationList(OrderReconcillationfilterDc reconcillationfilterDc)
        {
            var result = new List<ExportorderListData>();
            using (var context = new AuthContext())
            {
                if (reconcillationfilterDc != null)
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandTimeout = 900;
                    cmd.CommandText = "[ExportOrderToOrderReconciliationList]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    var Widlist = new DataTable();
                    Widlist.Columns.Add("IntValue");
                    foreach (var obj in reconcillationfilterDc.WarehouseIds)
                    {
                        var dr = Widlist.NewRow();
                        dr["IntValue"] = obj;
                        Widlist.Rows.Add(dr);
                    }
                    var param2 = new SqlParameter("@WarehouseIds", Widlist);
                    param2.SqlDbType = SqlDbType.Structured;
                    param2.TypeName = "dbo.IntValues";
                    var param3 = new SqlParameter("@FromDate", reconcillationfilterDc.FromDate ?? (object)DBNull.Value);
                    var param4 = new SqlParameter("@ToDate", reconcillationfilterDc.ToDate ?? (object)DBNull.Value);
                    var param5 = new SqlParameter("@ReconStatus", reconcillationfilterDc.ReconStatus == null ? DBNull.Value : (object)reconcillationfilterDc.ReconStatus);
                    var param6 = new SqlParameter("@MOP", reconcillationfilterDc.MOP == null ? DBNull.Value : (object)reconcillationfilterDc.MOP);
                    var param7 = new SqlParameter("@keyword", reconcillationfilterDc.keyword == null ? DBNull.Value : (object)reconcillationfilterDc.keyword);
                    cmd.Parameters.Add(param2);
                    cmd.Parameters.Add(param3);
                    cmd.Parameters.Add(param4);
                    cmd.Parameters.Add(param5);
                    cmd.Parameters.Add(param6);
                    cmd.Parameters.Add(param7);
                    var reader = cmd.ExecuteReader();
                    result = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<ExportorderListData>(reader).ToList();
                    //result = context.Database.SqlQuery<ExportorderListData>("exec ExportOrderToOrderReconciliationList @WarehouseIds, @FromDate,@ToDate,@ReconStatus,@MOP,@keyword", param2, param3, param4, param5, param6, param7).ToList();

                }
            }

            return result;
        }

        [Route("GetMisDetail")]
        [HttpPost]
        public APIResponse GetMisDetail(MisPayload payload)
        {
            using (AuthContext context = new AuthContext())
            {
                if (payload.Keyword == "null" || payload.Keyword == "undefined") payload.Keyword = null;
                var mop = new SqlParameter("@mop", payload.mop);
                var status = new SqlParameter("@Status", payload.Status);
                var skip = new SqlParameter("@Skip", payload.Skip);
                var take = new SqlParameter("@Take", payload.Take);
                var fdate = new SqlParameter("@Fromdate", payload.Fromdate ?? (object)DBNull.Value);
                var tdate = new SqlParameter("@Todate", payload.Todate ?? (object)DBNull.Value);
                var keyword = new SqlParameter("@Keyword", payload.Keyword != null ? payload.Keyword : (object)DBNull.Value);
                var isexport = new SqlParameter("@IsExport", payload.IsExport);
                var data = context.Database.SqlQuery<GetMisDetail>("GetMisFileDetails @mop,@Status,@Skip,@Take,@Fromdate,@Todate,@Keyword,@IsExport"
                    ,mop, status, skip, take, fdate, tdate, keyword, isexport).ToList();
                if (data != null && data.Count > 0)
                    return new APIResponse { Status = true, Data = data };
                else
                    return new APIResponse { Status = false, Message = "Data Not Found!" };
            }
        }

        [Route("GetMistobank")]
        [HttpGet]
        public List<GetMisToBankDetail> GetMistobank(DateTime FromDate ,DateTime Todate)
        {
            using (var db = new AuthContext())
            {
                List<GetMisToBankDetail> res = new List<GetMisToBankDetail>();
                var fdate = new SqlParameter("@FromDate", FromDate);
                var tdate = new SqlParameter("@ToDate", Todate);
                res = db.Database.SqlQuery<GetMisToBankDetail>("Sp_mistobankdetail @FromDate,@ToDate", fdate, tdate).ToList();
                return res;
            }
        }

        [Route("UpdatePaymentDate")]
        [HttpGet]
        [AllowAnonymous]
        public bool UpdatePaymentDate( List<UpdatePaymentDates> lists)
        {
            using (var db = new AuthContext())
            {
                bool res = false;
                //var data = db.Database.SqlQuery<GetunPaymentData>("exec Sp_getunupdatepayment").ToList();
                //var ids = data.Select(y => y.Id).ToList();
                //var orderdata = db.OrderReconcillationDetails.Where(x => ids.Contains(x.Id)).ToList();
                var ids = lists.Select(x => x.Id).Distinct().ToList();
                var data = lists.Where(x => ids.Contains(x.Id)).ToList();
                if(data.Count() >0 && data.Any())
                {
                    foreach(var d in data)
                    {
                        var id = new SqlParameter("@Id", d.Id);
                        var mop = new SqlParameter("@Mop", d.Moq);
                        res = db.Database.SqlQuery<bool>("exec Sp_confirmpaymentdate @Id,@Mop", id, mop).FirstOrDefault();
                        //if (result.HasValue && result!= null)
                        //{
                        //    var reconciledata = db.OrderReconcillationDetails.FirstOrDefault(x => x.Id == d.Id);
                        //    if(reconciledata != null)
                        //    {
                        //        reconciledata.PaymentReceiveDate = result;
                        //        db.Entry(reconciledata).State = System.Data.Entity.EntityState.Modified;
                        //    }
                        //}
                    }
                    //if(db.Commit() > 0)
                    //{
                    //    res = true;
                    //}
                    //else
                    //{
                    //    res = true;
                    //}
                }
                else
                {
                    res = false;
                }
                return res;
            }
        }


        [Route("TestMistoBankVerify")]
        [HttpGet]
        [AllowAnonymous]
        public bool TestMistoBankVerify()
        {
            bool res = false;
            DateTime cdate = DateTime.Now;
            int userid = 1;
            using (var context = new AuthContext())
            {
                List<MisDataDC> data = context.Database.SqlQuery<MisDataDC>("exec Sp_misreport").ToList();
                var datas = data.Count > 0 && data.Any() ? data.GroupBy(y => new { y.TransactionDate, y.TransactionAmount, y.ModeOfPayment, y.Status, y.IsProcess })
                    .Select(x => new
                    {
                        x.Key.TransactionAmount,
                        x.Key.TransactionDate,
                        x.Key.ModeOfPayment,
                        x.Key.IsProcess
                    }).ToList() : null;
                var idlist = data.Select(x => x.Id).ToList();
                List<OrderReconcillationFileUploadDetail> orderreconcilefileuploaddata = context.OrderReconcillationFileUploadDetails.Where(x => x.IsActive == true && idlist.Contains(x.Id)).ToList();

                if (datas.Any() && datas.Count > 0)
                {
                    foreach (var d in datas)
                    {
                        if (d.ModeOfPayment == "HDFC_CCDC" || d.ModeOfPayment == "HDFC_UPI" || d.ModeOfPayment == "HDFC_NetBanking" || d.ModeOfPayment == "UPI")
                        {
                            DateTime dates = d.TransactionDate.Date; ;
                            DateTime date1 = d.TransactionDate.AddDays(3).Date;
                            //DateTime date2 = DateTime.Now.AddDays(-18).Date;

                            var bankdata = context.orderReconcillationBankDetails.FirstOrDefault(x => x.IsProcess == false && x.Type == "C"  && x.ModeofPayment == d.ModeOfPayment && x.RemainingAmount >= d.TransactionAmount && (x.TransactionDate >= dates && x.TransactionDate <= date1));
                            //bankdatas.FirstOrDefault(x => x.ModeofPayment == d.ModeOfPayment && x.RemainingAmount >= d.TransactionAmount && (x.TransactionDate >= dates && x.TransactionDate <= date1));
                            if (bankdata != null)
                            {
                                bankdata.RemainingAmount -= d.TransactionAmount;
                                bankdata.SettledAmount += d.TransactionAmount;
                                bankdata.IsProcess = bankdata.RemainingAmount == 0 ? true : false;
                                bankdata.Status = bankdata.RemainingAmount == 0 ? "Verified" : "Not Verified";
                                //bankdata.ModifiedBy = userid;
                                //bankdata.ModifiedDate = cdate;
                                context.Entry(bankdata).State = System.Data.Entity.EntityState.Modified;
                                var data1 = data.Where(x => x.ModeOfPayment == d.ModeOfPayment && x.TransactionAmount == d.TransactionAmount && x.TransactionDate == d.TransactionDate).ToList();
                                foreach (var di in data1)
                                {
                                    //var up = context.OrderReconcillationDetails.Where(x => x.OrderReconcillationFileUploadDetailId == di.Id && x.IsProcess == true && x.OrderReconcillationFileUploadDetailId > 0).ToList();
                                    //if (up.Count > 0 && up.Any())
                                    //{
                                    //    foreach (var u in up)
                                    //    {
                                    //        OrderReconcillationHistory hist = new OrderReconcillationHistory();
                                    //        u.IsProcess = true;
                                    //        u.Status = "Verified";
                                    //        u.ModifiedBy = userid;
                                    //        u.ModifiedDate = cdate;
                                    //        u.orderReconcillationBankDetailId = bankdata.Id;
                                    //        hist.OrderId = u.OrderId;
                                    //        hist.ModeofPayment = u.ModeofPayment;
                                    //        hist.Status = u.Status;
                                    //        hist.CreatedDate = cdate;
                                    //        hist.CreatedBy = userid;
                                    //        hist.IsActive = true;
                                    //        hist.IsDeleted = false;
                                    //        context.orderReconcillationHistories.Add(hist);
                                    //        context.Entry(u).State = System.Data.Entity.EntityState.Modified;
                                    //    }
                                    //}
                                    var filedata = orderreconcilefileuploaddata.Where(x => x.Id == di.Id).FirstOrDefault();
                                    if (filedata != null)
                                    {
                                        //filedata.IsProcess = true;
                                        //filedata.Status = "Verified";
                                        //filedata.ModifiedBy = userid;
                                        //filedata.ModifiedDate = cdate;
                                        filedata.BankDetailId = bankdata.Id;
                                        context.Entry(filedata).State = System.Data.Entity.EntityState.Modified;
                                    }
                                }
                            }
                        }
                        context.Commit();
                        res = true;
                    }
                }
                
                return res;
            }
        }

        #endregion

        [HttpGet]
        [AllowAnonymous]
        [Route("checkfile")]
        public IHttpActionResult Checkfile()

        {
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                DateTime cdate = DateTime.Now;
                var formData1 = HttpContext.Current.Request.Form["compid"];
                // Access claims
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                // Get the uploaded image from the Files collection
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                string MSG = "";
                using (AuthContext context = new AuthContext())
                {
                    
                    if (httpPostedFile != null)
                    {
                        string ext = Path.GetExtension(httpPostedFile.FileName);
                        if (ext == ".xlsx" || ext == ".xls")
                        {
                            //var hdfcdata = context.OrderReconcillationFileUploadDetails.Where(x => x.IsActive == true && x.IsDeleted == false && (x.ModeOfPayment == "HDFC_NetBanking" || x.ModeOfPayment == "HDFC_CCDC" || x.ModeOfPayment == "HDFC_UPI")).ToList();
                            //string path = HttpContext.Current.Server.MapPath("~/UploadedFiles/OrderTwoOrderReconcillation");
                            //string a1, b;

                            //if (!Directory.Exists(path))
                            //{
                            //    Directory.CreateDirectory(path);
                            //}
                            //a1 = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + httpPostedFile.FileName;
                            //b = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles/OrderTwoOrderReconcillation/"), a1);
                            //httpPostedFile.SaveAs(b);

                            byte[] buffer = new byte[httpPostedFile.ContentLength];

                            using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
                            {
                                br.Read(buffer, 0, buffer.Length);
                            }
                            XSSFWorkbook hssfwb;
                            List<TestcheckfileDC> uploaditemlist = new List<TestcheckfileDC>();
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

                                int? TransactionDateCellIndex = null;
                                int? MOPCellIndex = null;
                                int? TransactionReferenceNumberCellIndex = null;
                                int? TransactionAmountCellIndex = null;
                                int? SettlementStatusCellIndex = null;
                                int? PaymentModeCellIndex = null;
                                int? TransactionStatusCellIndex = null;
                                int? OrderNoCellIndex = null;
                                int? GrossAmountCellIndex = null;
                                int? FeeFlatCellIndex = null;
                                int? TaxCellIndex = null;
                                int? ReferencenumbercellIndex = null;
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
                                            //TransactionDateCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Order Datetime") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Order Datetime").ColumnIndex : (int?)null;
                                            //if (!TransactionDateCellIndex.HasValue)
                                            //{
                                            //    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Order Datetime does not exist..try again");
                                            //    return Created(strJSON, strJSON);
                                            //}
                                            //PaymentModeCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Payment Mode") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Payment Mode").ColumnIndex : (int?)null;

                                            //if (!PaymentModeCellIndex.HasValue)
                                            //{
                                            //    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("MOP does not exist..try again");
                                            //    return Created(strJSON, strJSON);
                                            //}
                                            ReferencenumbercellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "CCAvenue Ref#") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "CCAvenue Ref#").ColumnIndex : (int?)null;

                                            if (!ReferencenumbercellIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Refrencenumber does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }



                                            OrderNoCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Order No") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Order No").ColumnIndex : (int?)null;

                                            if (!OrderNoCellIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Order No. cell does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }

                                            //    GrossAmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Gross Amount") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Gross Amount").ColumnIndex : (int?)null;

                                            //    if (!GrossAmountCellIndex.HasValue)
                                            //    {
                                            //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("GrossAmount cell does not exist..try again");
                                            //        return Created(strJSON, strJSON);
                                            //    }


                                            //    FeeFlatCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Fee Flat") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Fee Flat").ColumnIndex : (int?)null;
                                            //    TaxCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Tax") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Tax").ColumnIndex : (int?)null;

                                            //}

                                        }
                                        
                                    }
                                    else
                                    {


                                        rowData = sheet.GetRow(iRowIdx);

                                        if (rowData != null)
                                        {

                                            TestcheckfileDC uploadDC = new TestcheckfileDC();
                                            //cellData = rowData.GetCell(TransactionDateCellIndex.Value);
                                            //col12 = cellData == null ? "" : cellData.ToString();
                                            //uploadDC.TransactionDate = Convert.ToDateTime(col12);
                                            //logger.Info("Transaction Date :" + uploadDC.TransactionDate);

                                            uploadDC.ReferenceNumber = rowData.GetCell(ReferencenumbercellIndex.Value).ToString();

                                            //cellData = rowData.GetCell(OrderNoCellIndex.Value);
                                            //col11 = cellData == null ? "" : cellData.ToString();
                                            //if (col11.Length == 7)
                                            //{
                                            //    uploadDC.GullakReferenceNumber = "";
                                            //}
                                            //else
                                            //{
                                            //    uploadDC.GullakReferenceNumber = col11;
                                            //}
                                            //logger.Info("ReferenceNumber :" + uploadDC.ReferenceNumber);


                                            //cellData = rowData.GetCell(PaymentModeCellIndex.Value);
                                            //logger.Info("Payment Mode :" + uploadDC.MOP);
                                            //col13 = cellData == null ? "" : cellData.ToString();

                                            //if (cellData.ToString() == "Credit Card" || cellData.ToString() == "Debit Card")
                                            //{
                                            //    uploadDC.MOP = "HDFC_CCDC";
                                            //}
                                            //else if (col13 == "Unified Payments")
                                            //{
                                            //    uploadDC.MOP = "HDFC_UPI";

                                            //}
                                            //else
                                            ////if (col13 == "Net Banking")
                                            //{
                                            //    uploadDC.MOP = "HDFC_NetBanking";

                                            //}


                                            //cellData = rowData.GetCell(PaymentModeCellIndex.Value);//transaction amount
                                            //logger.Info("Order No:" + uploadDC.OrderId);
                                            //if (uploadDC.MOP == "HDFC_CCDC" || uploadDC.MOP == "HDFC_UPI")
                                            //{
                                            //    uploadDC.TransactionAmount = Convert.ToDouble(rowData.GetCell(GrossAmountCellIndex.Value).ToString());
                                            //}
                                            //else
                                            //{
                                            //    //uploadDC.TransactionAmount = Convert.ToDouble(rowData.GetCell(GrossAmountCellIndex.Value).ToString()) - (Convert.ToDouble(rowData.GetCell(FeeFlatCellIndex.Value).ToString()) + Convert.ToDouble(rowData.GetCell(TaxCellIndex.Value).ToString()));
                                            //    uploadDC.TransactionAmount = Convert.ToDouble(rowData.GetCell(GrossAmountCellIndex.Value).ToString());
                                            //}

                                            cellData = rowData.GetCell(OrderNoCellIndex.Value);  //order Id
                                            logger.Info("Order No:" + uploadDC.OrderId);
                                            if (cellData != null)//&& cellData.ToString().Length == 7)
                                            {
                                                uploadDC.OrderId = Convert.ToInt64(rowData.GetCell(OrderNoCellIndex.Value).ToString());
                                            }
                                            //else uploadDC.OrderId = null;

                                            //uploadDC.RemainingAmount = uploadDC.TransactionAmount;

                                            //uploadDC.IsProcess = false;
                                            //uploadDC.Status = "Not Verified";
                                            //uploadDC.SettledAmount = 0;
                                            //uploadDC.WarehouseId = null;

                                            //cellData = rowData.GetCell(PaymentModeCellIndex.Value);//Charges
                                            //logger.Info("Order No:" + uploadDC.OrderId);
                                            //if (cellData != null && uploadDC.MOP == "HDFC_NetBanking")
                                            //{
                                            //    uploadDC.Charges = (Convert.ToDouble(rowData.GetCell(FeeFlatCellIndex.Value).ToString()) + Convert.ToDouble(rowData.GetCell(TaxCellIndex.Value).ToString()));
                                            //}
                                            //else { uploadDC.Charges = 0; }

                                            //uploadDC.TransactionDescription = null;
                                            //uploadDC.IsgullakVerified = false;

                                            //if (uploadDC.ReferenceNumber.Length > 0)
                                            //{
                                            //    var isdataexists = hdfcdata.FirstOrDefault(x => x.RefNo == uploadDC.ReferenceNumber);
                                            //    if (isdataexists == null)
                                            //    {
                                            //        uploaditemlist.Add(uploadDC);
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //    var isdataexists = hdfcdata.FirstOrDefault(x => x.GullakReferenceNumber == uploadDC.GullakReferenceNumber);
                                            //    if (isdataexists == null)
                                            //    {
                                            //        uploaditemlist.Add(uploadDC);
                                            //    }
                                            //}
                                            uploaditemlist.Add(uploadDC);
                                        }
                                    }
                                }

                                if (uploaditemlist != null && uploaditemlist.Any())
                                {

                                    var data = context.OrderReconcillationFileUploadDetails.Where(x => x.GullakReferenceNumber.Length > 0 && x.RefNo.Length == 0 && x.IsActive == true ).ToList();
                                    foreach(var d in data)
                                    {
                                        var isexist = uploaditemlist.Where(x => x.OrderId == Convert.ToInt64(d.GullakReferenceNumber)).FirstOrDefault();
                                        if(isexist != null)
                                        {
                                            //d.OrderId = isexist.OrderId;
                                            d.RefNo = isexist.ReferenceNumber;
                                            context.Entry(d).State = System.Data.Entity.EntityState.Modified;
                                        }
                                    }

                                    if(context.Commit() > 0)
                                    {
                                        MSG = "yes chnage success"; return Created(MSG, MSG);
                                    }
                                    else
                                    {
                                        MSG = "some error"; return Created(MSG, MSG);
                                    }
                                }
                                else
                                {
                                    MSG = "Data not in excel"; return Created(MSG, MSG);
                                }
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
            else
            {
                return Created("Error", "Error");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Getorderidforverifiedbank")]
        public List<int> Getorderidforverifiedbank(long Id)
        {
            using (var db = new AuthContext())
            {
                List<int> result = new List<int>();
                var id = new SqlParameter("@Id", Id);
                result = db.Database.SqlQuery<int>("exec Sp_getorderids @Id", id).ToList();
                return result;
            }
        }
    }
}