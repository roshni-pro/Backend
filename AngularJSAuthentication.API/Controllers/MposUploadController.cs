using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model;
using NLog;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/MposUpload")]
    public class MposUploadController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();//Api for Upload Sheet on Warehouse to warehouse transfer
        string msgitemname, msg1;
        //string msg1;
        string strJSON = null;
        string col0, col1, col2, col3, col4, col5, col6, col7, col8, col9, col10, col11, col12, col13, col14, col15, col16, col17, col39;
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Authorize]
        [HttpPost]
        public IHttpActionResult HDFCNetUploadFile(string fileType)
        {
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {

                var formData1 = HttpContext.Current.Request.Form["compid"];
                logger.Info("start Transfer Order Upload Exel File: ");

                // Access claims
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;


                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                // Get the uploaded image from the Files collection
                System.Web.HttpPostedFile httpPostedFile = HttpContext.Current.Request.Files["file"];

                if (httpPostedFile != null)
                {
                    var FileUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles"), httpPostedFile.FileName);
                    // Validate the uploaded image(optional)
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

                    switch (fileType)
                    {
                        case "3":
                            return ReadHDFCNetUploadedFile(hssfwb, userid);
                    }

                    httpPostedFile.SaveAs(FileUrl);

                }
            }

            return Created("Error", "Error");
        }
        #region Upload UPI HDfC
        public IHttpActionResult ReadHDFCNetUploadedFile(XSSFWorkbook hssfwb, int userid)
        {
            string sSheetName = hssfwb.GetSheetName(0);
            ISheet sheet = hssfwb.GetSheet(sSheetName);

            IRow rowData;
            ICell cellData = null;
            try
            {
                List<HDFCNetBankingUpload> HDFCNetUploader = new List<HDFCNetBankingUpload>();
                int? OrderidCellIndex = null;
                int? transactiondateIndex = null;
                int? AmountCellIndex = null;
                int? txnIdCellIndex = null;
                int? CardtypeCellIndex = null;
                int? PayModeCellIndex = null;
                int? settleDateCellIndex = null;
                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                {
                    if (iRowIdx == 0)
                    {
                        rowData = sheet.GetRow(iRowIdx);

                        if (rowData != null)
                        {
                            txnIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "CCAvenue Ref#") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "CCAvenue Ref#").ColumnIndex : (int?)null;
                            if (!txnIdCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Date does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            OrderidCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Order No") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Order No").ColumnIndex : (int?)null;
                            if (!OrderidCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("OrderId does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            transactiondateIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Order Datetime") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Order Datetime").ColumnIndex : (int?)null;
                            if (!transactiondateIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Order Datetime does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Order Amount") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Order Amount").ColumnIndex : (int?)null;
                            if (!AmountCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Order Amount does not exist..try again");
                                return Created(strJSON, strJSON);
                            }




                            PayModeCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Payment Mode") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Payment Mode").ColumnIndex : (int?)null;
                            if (!PayModeCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize(" Payment Mode does not exist..try again");
                                return Created(strJSON, strJSON);
                            }
                            CardtypeCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Card Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Card Name").ColumnIndex : (int?)null;
                            if (!CardtypeCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Card Name does not exist..try again");
                                return Created(strJSON, strJSON);
                            }
                            settleDateCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Order Stlmt Date") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Order Stlmt Date").ColumnIndex : (int?)null;
                            if (!settleDateCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Order Stlmt Date does not exist..try again");
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
                            HDFCNetBankingUpload trnfrorder = new HDFCNetBankingUpload();
                            try
                            {

                                //int requestTowarehouseId;
                                cellData = rowData.GetCell(OrderidCellIndex.Value);
                                col2 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.OrderId = Convert.ToString(col2);
                                logger.Info("Order No :" + trnfrorder.OrderId);

                                cellData = rowData.GetCell(transactiondateIndex.Value);
                                col3 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.transaction_date = DateTimeHelper.ConvertToDateTime(col3).HasValue ? DateTimeHelper.ConvertToDateTime(col3).Value : DateTime.Now; ;
                                logger.Info("Order Datetime :" + trnfrorder.transaction_date);

                                cellData = rowData.GetCell(AmountCellIndex.Value);
                                col12 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Amount = Convert.ToDouble(col12);
                                logger.Info("Order Amount :" + trnfrorder.Amount);

                                cellData = rowData.GetCell(txnIdCellIndex.Value);
                                col0 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.transaction_id = Convert.ToString(col0);  // Convert.ToDateTime(col3);
                                logger.Info("CCAvenue Ref# :" + trnfrorder.transaction_id);

                                cellData = rowData.GetCell(PayModeCellIndex.Value);
                                col6 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.PaymentType = Convert.ToString(col6);
                                logger.Info("Payment Mode :" + trnfrorder.PaymentType);

                                cellData = rowData.GetCell(CardtypeCellIndex.Value);
                                col8 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.CardName = Convert.ToString(col8);
                                logger.Info("Card Name :" + trnfrorder.CardName);

                                cellData = rowData.GetCell(settleDateCellIndex.Value);
                                col39 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.settlementDate = DateTimeHelper.ConvertToDateTime(col39).HasValue ? DateTimeHelper.ConvertToDateTime(col39).Value : DateTime.Now;  // Convert.ToDateTime(col3);
                                logger.Info("Order Stlmt Date :" + trnfrorder.settlementDate);


                                trnfrorder.userid = userid;
                                HDFCNetUploader.Add(trnfrorder);

                            }
                            catch (Exception ex)
                            {
                                msgitemname = ex.Message;
                                logger.Error("Error adding customer in collection " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                        }
                    }
                }

                List<HDFCNetBankingUpload> id = AddHDFCUpload(HDFCNetUploader, userid);
                string a = string.Empty;
                if (id.Any() && id.Count() > 0)
                {
                    a = id[0].UploadId.ToString();
                }
                return Created(a, a);

            }
            catch (Exception ex)
            {
                logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
            }

            return Created("Error", "Error");
        }



        public List<HDFCNetBankingUpload> AddHDFCUpload(List<HDFCNetBankingUpload> trCollection, int userId)
        {
            logger.Info("start addbulk");
            List<HDFCNetBankingUpload> pdList = new List<HDFCNetBankingUpload>();
            using (var context = new AuthContext())
            {

                try
                {
                    UploadFileReconcile TOM = new UploadFileReconcile();
                    var peopledata = context.Peoples.Where(x => x.PeopleID == userId).FirstOrDefault();

                    TOM = new UploadFileReconcile();
                    TOM.FileType = "HDFC_NetBanking";
                    TOM.UplaodBy = peopledata.DisplayName;
                    TOM.UploadDate = indianTime;
                    context.UploadFileReconcileDB.Add(TOM);
                    context.Commit();

                    var txnIds = trCollection.Select(x => x.transaction_id).ToList();
                    var payments = context.PaymentResponseRetailerAppDb.Where(x => txnIds.Contains(x.GatewayTransId) && x.status == "Success" && x.amount > 0).ToList();

                    //var uploadedTxnIds = 


                    for (var i = 0; i < trCollection.ToList().Count(); i++)
                    {
                        double amount = trCollection[i].Amount;
                        string transactionid = trCollection[i].transaction_id;
                        var payment = payments.Where(x => x.GatewayTransId == transactionid && x.amount > 0).FirstOrDefault();
                        var CheckUpload = context.HDFCNetBankingUploadDB.Where(x => x.transaction_id == transactionid && x.IsReconcile == true).FirstOrDefault();
                        if (CheckUpload == null)
                        {
                            HDFCNetBankingUpload pd = new HDFCNetBankingUpload();
                            pd.UploadId = TOM.UploadId;
                            if (payment != null)
                            {
                                pd.IsReconcile = payment.amount == trCollection[i].Amount;
                            }
                            else
                            {
                                pd.IsReconcile = false;
                            }
                            pd.transaction_id = trCollection[i].transaction_id;
                            pd.transaction_date = trCollection[i].transaction_date;
                            pd.OrderId = trCollection[i].OrderId;
                            pd.Amount = trCollection[i].Amount;
                            pd.PaymentType = trCollection[i].PaymentType;
                            pd.CardName = trCollection[i].CardName;
                            pd.settlementDate = trCollection[i].settlementDate;

                            pdList.Add(pd);
                        }

                        //context.EpaylaterUploadDB.Add(pd);


                    }

                    if (pdList != null && pdList.Any())
                    {
                        context.HDFCNetBankingUploadDB.AddRange(pdList);

                        if (pdList.All(x => x.IsReconcile))
                        {
                            TOM.IsReconcile = true;
                            context.Entry(TOM).State = System.Data.Entity.EntityState.Modified;
                        }
                    }

                    context.Commit();

                    if (pdList.Any(x => x.IsReconcile))
                    {
                        var reconciledTxns = pdList.Where(x => x.IsReconcile).Select(x => x.transaction_id).ToList();
                        payments.Where(x => reconciledTxns.Contains(x.GatewayTransId)).ToList().ForEach(payment =>
                        {
                            payment.UploadId = pdList.FirstOrDefault(x => x.transaction_id == payment.GatewayTransId).Id;
                            payment.IsSettled = true;
                            payment.UpdatedDate = indianTime;
                            payment.SettleDate = indianTime;
                            context.Entry(payment).State = System.Data.Entity.EntityState.Modified;
                        });

                        context.Commit();
                    }

                }

                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                }
            }
            return pdList;
        }
    }
}
#endregion
