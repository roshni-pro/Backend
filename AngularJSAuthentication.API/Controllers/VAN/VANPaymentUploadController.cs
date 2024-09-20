
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.VAN;
using Microsoft.Extensions.Logging;
using NLog;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace AngularJSAuthentication.API.Controllers.VAN
{
    [RoutePrefix("api/VANPaymantUpload")]
    public class VANPaymentUploadController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();//Api for Upload Sheet on Warehouse to warehouse transfer
        string msgitemname, msg1;
        //string msg1;
        string strJSON = null;
        string col0, col1, col2, col3, col4, col5, col6, col7, col8, col9, col10, col11, col12, col13, col14, col15,
            col16, col17, col18, col19, col20, col21, col22, col23, col24, col25;
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Authorize]
        [HttpPost]
        [Route("VANPaymantUploadFile")]
        public IHttpActionResult UploadFile()
        {            
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var formData1 = HttpContext.Current.Request.Form["compid"];
                logger.Info("start VANPaymantUpload Upload Exel File: ");
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
                    return ReadVANUploadedFile(hssfwb, userid);
                }
            }

            return Created("Error", "Error");
        }
        #region Upload VAN Payment
        public IHttpActionResult ReadVANUploadedFile(XSSFWorkbook hssfwb, int userid)
        {
            string sSheetName = hssfwb.GetSheetName(0);
            ISheet sheet = hssfwb.GetSheet(sSheetName);

            IRow rowData;
            ICell cellData = null;
            try
            {
                List<VANPaymentUploadDc> VANUploader = new List<VANPaymentUploadDc>();
                int? txnIdCellIndex = null;
                int? ClientCodeCellIndex = null;
                int? AmountCellIndex = null;

                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                {
                    if (iRowIdx == 0)
                    {
                        rowData = sheet.GetRow(iRowIdx);
                        if (rowData != null)
                        {
                            ClientCodeCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Client code") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Client code").ColumnIndex : (int?)null;
                            if (!ClientCodeCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Client code does not exist..try again");
                                return Created(strJSON, strJSON); ;
                            }

                            txnIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Reference No.") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Reference No.").ColumnIndex : (int?)null;
                            if (!txnIdCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Reference No. does not exist..try again");
                                return Created(strJSON, strJSON); ;
                            }

                            AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Amount") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Amount").ColumnIndex : (int?)null;
                            if (!AmountCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Amount does not exist..try again");
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
                            VANPaymentUploadDc trnfrorder = new VANPaymentUploadDc();
                            try
                            {

                                cellData = rowData.GetCell(ClientCodeCellIndex.Value);
                                col2 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Clientcode = Convert.ToString(col2);
                                logger.Info("Clientcode :" + trnfrorder.Clientcode);

                                cellData = rowData.GetCell(txnIdCellIndex.Value);
                                col4 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Reference_No = Convert.ToString(col4);
                                logger.Info("Reference_No :" + trnfrorder.Reference_No);

                                cellData = rowData.GetCell(AmountCellIndex.Value);
                                col5 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Amount = Convert.ToDouble(col5);
                                logger.Info("Amount :" + trnfrorder.Amount);

                                trnfrorder.userid = userid;
                                VANUploader.Add(trnfrorder);

                            }
                            catch (Exception ex)
                            {
                                msgitemname = ex.Message;
                                logger.Error("Error VAN Paymant Upload File " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                        }
                    }
                }
                string status = AddVANUploadPayment(VANUploader, userid);
                //string Msg = "No Record Found!!";
                //if (string.IsNullOrEmpty(status))
                //{
                //    Msg = "Record Updated Successfully!!";
                //}
                return Created(status, status);

            }
            catch (Exception ex)
            {
                logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
            }

            return Created("Error", "Error");
        }



        public string AddVANUploadPayment(List<VANPaymentUploadDc> trCollection, int userId)
        {
            logger.Info("start addbulk");
            StringBuilder str = new StringBuilder();
            using (var context = new AuthContext())
            {
                try
                {                    
                    string ExcelSavePath = HttpContext.Current.Server.MapPath("~/UploadedFiles/VANPaymentUpload");
                    if (!Directory.Exists(ExcelSavePath))
                        Directory.CreateDirectory(ExcelSavePath);

                    string fileUrl = "";
                    var gstDt = ClassToDataTable.CreateDataTable(trCollection);
                    gstDt.TableName = "VAN Upload Data";
                    var fileName = "VANUploadSheet" + DateTime.Now.ToString("yyyyddMHHmmss") + ".xlsx";
                    string filePath = ExcelSavePath + "\\"+ fileName;
                    ExcelGenerator.DataTable_To_Excel(new List<DataTable> { gstDt }, filePath);
                    fileUrl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                            , HttpContext.Current.Request.Url.DnsSafeHost
                                                            , HttpContext.Current.Request.Url.Port
                                                            , string.Format("UploadedFiles/VANPaymentUpload/{0}", fileName));
                    var txnIds = trCollection.Select(x => x.Reference_No).ToList();
                    var payments = context.VANResponses.Where(x => txnIds.Contains(x.UserReferenceNumber) && x.IsActive == true && x.IsDeleted == false).ToList();
                    if (payments != null && payments.Any())
                    {
                        var vANResponsesIds = payments.Select(x => x.Id).ToList();
                        var vANTransactionsList = context.VANTransactiones.Where(x => vANResponsesIds.Contains(x.ObjectId) && x.ObjectType == "VANResponse" && x.IsActive == true && x.IsDeleted == false && x.IsSettled == false).ToList();
                        var vANTransactionesIds = vANTransactionsList.Select(x => x.Id).ToList();
                        var vANTransactionsOrderList = context.VANTransactiones.Where(x => vANTransactionesIds.Contains(x.VANTransactionParentId) && x.ObjectType == "Order" && x.IsActive == true && x.IsDeleted == false && x.IsSettled == false).ToList();
                        var list = payments.Where(x => x.IsSettled == false).Distinct().ToList();
                        var Settledlist = payments.Where(x => x.IsSettled == true).Distinct().ToList();
                        var trnids = list.Select(x => x.UserReferenceNumber).Distinct().ToList();
                        var Settledtrnids = Settledlist.Select(x => x.UserReferenceNumber).Distinct().ToList();
                        var excelsheet = trCollection.Where(x => (trnids.Contains(x.Reference_No) || !Settledtrnids.Contains(x.Reference_No))).Distinct().ToList();
                        for (var i = 0; i < excelsheet.ToList().Count(); i++)
                        {
                            double amount = excelsheet[i].Amount;
                            string transactionid = excelsheet[i].Reference_No;
                            string Clientcode = excelsheet[i].Clientcode;
                            var payment = payments.Where(x => x.UserReferenceNumber == transactionid && x.BenefDetails2 == Clientcode && x.Amount == amount && x.IsSettled == false).FirstOrDefault();
                            if (payment != null)
                            {
                                payment.IsSettled = true;
                                payment.Settledby = userId;
                                payment.SettledDate = DateTime.Now;
                                payment.UploadFilePath = fileUrl;
                                payment.Status = Convert.ToInt32(PaymentStatus.Success);
                                payment.ReconciledAmount = amount;
                                context.Entry(payment).State = System.Data.Entity.EntityState.Modified;
                                //VANTransactiones VANResponse
                                var vANTransactions = vANTransactionsList.Where(x => x.ObjectId == payment.Id && x.ObjectType == "VANResponse" && x.IsActive == true && x.IsDeleted == false && x.IsSettled == false).FirstOrDefault();
                                if (vANTransactions != null)
                                {
                                    vANTransactions.IsSettled = true;
                                    vANTransactions.Settledby = userId;
                                    vANTransactions.SettledDate = DateTime.Now;
                                    context.Entry(vANTransactions).State = System.Data.Entity.EntityState.Modified;
                                }
                                //VANTransactiones
                                var vANTransactionsOrderLists = vANTransactionsOrderList.Where(x => x.VANTransactionParentId == vANTransactions.Id && x.IsActive == true && x.IsDeleted == false && x.IsSettled == false).ToList();
                                vANTransactionsOrderLists.ForEach(x =>
                                {
                                    x.IsSettled = true;
                                    x.Settledby = userId;
                                    x.SettledDate = DateTime.Now;
                                    context.Entry(x).State = System.Data.Entity.EntityState.Modified;
                                });
                            }
                            else
                            {
                                var PaymentDeviation = payments.Where(x => x.UserReferenceNumber == transactionid && x.BenefDetails2 == Clientcode && x.IsSettled == false).FirstOrDefault();
                                if (PaymentDeviation != null)
                                {
                                    PaymentDeviation.IsSettled = false;
                                    PaymentDeviation.UploadFilePath = fileUrl;
                                    PaymentDeviation.Status = Convert.ToInt32(PaymentStatus.Deviation);
                                    PaymentDeviation.ReconciledAmount = amount;
                                    context.Entry(PaymentDeviation).State = System.Data.Entity.EntityState.Modified;
                                }
                                else 
                                {
                                    str.AppendLine("There is mismatch in either ClientCode or Reference id : " + transactionid + " ,ClientSKcode : " + Clientcode);
                                }                                
                            }                            
                        }
                        if (str.Length == 0)
                        {
                            str.AppendLine("Successfully Uploaded!!");
                        }
                        context.Commit();
                    }
                    else
                    {
                        str.AppendLine("There is mismatch in either ClientCode or Reference id");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                }
            }
            return str.ToString();
        }
        #endregion
    }
}