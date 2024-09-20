using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
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
    [RoutePrefix("api/EpayLaterUpload")]
    public class EpayLaterUploadController : ApiController
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
        public IHttpActionResult UploadFile(string fileType)
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
                    //   XSSFWorkbook workbook1;                    using (MemoryStream memStream = new MemoryStream())
                    {
                        BinaryFormatter binForm = new BinaryFormatter();
                        memStream.Write(buffer, 0, buffer.Length);
                        memStream.Seek(0, SeekOrigin.Begin);
                        hssfwb = new XSSFWorkbook(memStream);
                    }

                    switch (fileType)
                    {
                        case "2":
                            return ReadEpaylaterUploadedFile(hssfwb, userid);

                    }
                    switch (fileType)
                    {
                        case "3":
                            return ReadHDfCUploadedFile(hssfwb, userid);

                    }
                    switch (fileType)
                    {
                        case "5":
                            return ReadMPosUploadedFile(hssfwb, userid);

                    }
                    switch (fileType)
                    {
                        case "6":
                            return ReadHDfCCreditUploadedFile(hssfwb, userid);

                    }
                    switch (fileType)
                    {
                        case "7":
                            return RazorpayQRUploadedFile(hssfwb, userid);

                    }
                    switch (fileType)
                    {
                        case "9":
                            return ChqBookUpload(hssfwb, userid);

                    }
                    switch (fileType)
                    {
                        case "11":
                            return ReadUPIUploadedFile(hssfwb, userid);

                    }
                    switch (fileType)
                    {
                        case "13":
                            return ReadICICIUploadedFile(hssfwb, userid);
                    }
                    httpPostedFile.SaveAs(FileUrl);

                }
            }

            return Created("Error", "Error");
        }

        #region Upload EpayLater
        public IHttpActionResult ReadEpaylaterUploadedFile(XSSFWorkbook hssfwb, int userid)
        {
            string sSheetName = hssfwb.GetSheetName(0);
            ISheet sheet = hssfwb.GetSheet(sSheetName);

            IRow rowData;
            ICell cellData = null;
            try
            {
                List<EpaylaterUpload> Epaylateruploader = new List<EpaylaterUpload>();
                int? txnIdCellIndex = null;
                int? marketplace_orderidCellIndex = null;
                int? orderAmountCellIndex = null;
                int? txnDateCellIndex = null;
                int? refNoCellIndex = null;
                int? settlementDateCellIndex = null;

                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                {
                    if (iRowIdx == 0)
                    {
                        rowData = sheet.GetRow(iRowIdx);

                        if (rowData != null)
                        {


                            txnIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "transaction_id") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "transaction_id").ColumnIndex : (int?)null;
                            if (!txnIdCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("transaction_id does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            marketplace_orderidCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "marketplace_orderid") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "marketplace_orderid").ColumnIndex : (int?)null;
                            if (!marketplace_orderidCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("marketplace_orderid does not exist..try again");
                                return Created(strJSON, strJSON); ;
                            }

                            orderAmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "order Amount") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "order Amount").ColumnIndex : (int?)null;
                            if (!orderAmountCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("order Amount does not exist..try again");
                                return Created(strJSON, strJSON); ;
                            }


                            txnDateCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "transaction_date") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "transaction_date").ColumnIndex : (int?)null;
                            if (!txnDateCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("transaction_date does not exist..try again");
                                return Created(strJSON, strJSON); ;
                            }

                            settlementDateCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "settlement_date") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "settlement_date").ColumnIndex : (int?)null;
                            if (!settlementDateCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("settlement_date does not exist..try again");
                                return Created(strJSON, strJSON); ;
                            }


                            refNoCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "UTR-NEFT -IMPS REF NO") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "UTR-NEFT -IMPS REF NO").ColumnIndex : (int?)null;
                            if (!refNoCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("UTR-NEFT -IMPS REF NO does not exist..try again");
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
                            EpaylaterUpload trnfrorder = new EpaylaterUpload();
                            try
                            {

                                //int requestTowarehouseId;
                                cellData = rowData.GetCell(txnIdCellIndex.Value);
                                col0 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.transaction_id = Convert.ToString(col0);
                                logger.Info("transaction_id :" + trnfrorder.transaction_id);

                                cellData = rowData.GetCell(marketplace_orderidCellIndex.Value);
                                col1 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.marketplace_orderid = Convert.ToString(col1);
                                logger.Info("marketplace_orderid :" + trnfrorder.marketplace_orderid);

                                cellData = rowData.GetCell(orderAmountCellIndex.Value);
                                col2 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Amount = Convert.ToDouble(col2);
                                logger.Info("order Amount :" + trnfrorder.Amount);

                                cellData = rowData.GetCell(txnDateCellIndex.Value);
                                col3 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.transaction_date = DateTimeHelper.ConvertToDateTime(col3).HasValue ? DateTimeHelper.ConvertToDateTime(col3).Value : DateTime.Now;//Convert.ToDateTime(col3);
                                logger.Info("transaction_date :" + trnfrorder.transaction_date);

                                cellData = rowData.GetCell(settlementDateCellIndex.Value);
                                col9 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.settlement_date = Convert.ToDateTime(col9);
                                logger.Info("settlement_date :" + trnfrorder.settlement_date);

                                cellData = rowData.GetCell(refNoCellIndex.Value);
                                col10 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.UTR_NEFT_IMPS_REFNO = Convert.ToString(col10);
                                logger.Info("UTR-NEFT -IMPS REF NO :" + trnfrorder.UTR_NEFT_IMPS_REFNO);


                                trnfrorder.userid = userid;
                                Epaylateruploader.Add(trnfrorder);

                            }
                            catch (Exception ex)
                            {
                                msgitemname = ex.Message;
                                logger.Error("Error adding customer in collection " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                        }
                    }
                }
                List<EpaylaterUpload> id = AddEpayLaterUpload(Epaylateruploader, userid);

                string a = id[0].UplaodId.ToString();
                return Created(a, a);

            }
            catch (Exception ex)
            {
                logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
            }

            return Created("Error", "Error"); ;
        }
        public List<EpaylaterUpload> AddEpayLaterUpload(List<EpaylaterUpload> trCollection, int userId)
        {
            logger.Info("start addbulk");
            List<EpaylaterUpload> pdList = new List<EpaylaterUpload>();
            using (var context = new AuthContext())
            {
                try
                {
                    UploadFileReconcile TOM = new UploadFileReconcile();
                    var peopledata = context.Peoples.Where(x => x.PeopleID == userId).FirstOrDefault();

                    TOM = new UploadFileReconcile();
                    TOM.FileType = "ePayLater";
                    TOM.UplaodBy = peopledata.DisplayName;
                    TOM.UploadDate = indianTime;
                    context.UploadFileReconcileDB.Add(TOM);
                    context.Commit();

                    var txnIds = trCollection.Select(x => x.transaction_id).ToList();
                    var payments = context.PaymentResponseRetailerAppDb.Where(x => txnIds.Contains(x.GatewayTransId) && x.status == "Success" && x.amount > 0).ToList();



                    for (var i = 0; i < trCollection.ToList().Count(); i++)
                    {
                        double amount = trCollection[i].Amount;
                        string transactionid = trCollection[i].transaction_id;
                        var payment = payments.Where(x => x.GatewayTransId == transactionid).FirstOrDefault();
                        var CheckUpload = context.EpaylaterUploadDB.Where(x => x.transaction_id == transactionid && x.IsReconcile == true).FirstOrDefault();
                        if (CheckUpload == null)
                        {
                            EpaylaterUpload pd = new EpaylaterUpload();
                            pd.UplaodId = TOM.UploadId;
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
                            pd.marketplace_orderid = trCollection[i].marketplace_orderid;
                            pd.Amount = trCollection[i].Amount;
                            pd.settlement_date = trCollection[i].settlement_date;
                            pd.UTR_NEFT_IMPS_REFNO = trCollection[i].UTR_NEFT_IMPS_REFNO;

                            pdList.Add(pd);
                        }

                        //context.EpaylaterUploadDB.Add(pd);


                    }

                    if (pdList != null && pdList.Any())
                    {
                        context.EpaylaterUploadDB.AddRange(pdList);

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
        #endregion

        #region Upload HDfC
        public IHttpActionResult ReadHDfCUploadedFile(XSSFWorkbook hssfwb, int userid)
        {
            string sSheetName = hssfwb.GetSheetName(0);
            ISheet sheet = hssfwb.GetSheet(sSheetName);

            IRow rowData;
            ICell cellData = null;
            try
            {
                List<HDFCUpload> HDFCUploader = new List<HDFCUpload>();
                int? txnIdCellIndex = null;
                int? MerchantIdCellIndex = null;
                int? CCAvenueCellIndex = null;
                int? txnDateCellIndex = null;
                int? AmountCellIndex = null;
                int? PayModeCellIndex = null;
                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                {
                    if (iRowIdx == 0)
                    {
                        rowData = sheet.GetRow(iRowIdx);

                        if (rowData != null)
                        {


                            txnIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Order #") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Order #").ColumnIndex : (int?)null;
                            if (!txnIdCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Order does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            MerchantIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Merchant ID") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Merchant ID").ColumnIndex : (int?)null;
                            if (!MerchantIdCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Merchant ID does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            CCAvenueCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "CCAvenue Ref. #") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "CCAvenue Ref. #").ColumnIndex : (int?)null;
                            if (!CCAvenueCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("CCAvenue Ref. # does not exist..try again");
                                return Created(strJSON, strJSON);
                            }


                            txnDateCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Date") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Date").ColumnIndex : (int?)null;
                            if (!txnDateCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Date does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Amount") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Amount").ColumnIndex : (int?)null;
                            if (!AmountCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Amount does not exist..try again");
                                return Created(strJSON, strJSON);
                            }


                            PayModeCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Pay Mode") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Pay Mode").ColumnIndex : (int?)null;
                            if (!PayModeCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize(" Pay Mode does not exist..try again");
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
                            HDFCUpload trnfrorder = new HDFCUpload();
                            try
                            {

                                //int requestTowarehouseId;
                                cellData = rowData.GetCell(txnIdCellIndex.Value);
                                col0 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.CCAvenueRef = Convert.ToString(col0);
                                logger.Info("Order # :" + trnfrorder.CCAvenueRef);

                                cellData = rowData.GetCell(MerchantIdCellIndex.Value);
                                col1 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.MerchantId = Convert.ToString(col1);
                                logger.Info("Merchant ID :" + trnfrorder.MerchantId);

                                cellData = rowData.GetCell(CCAvenueCellIndex.Value);
                                col2 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.transaction_id = Convert.ToString(col2);
                                logger.Info("CCAvenue Ref. # :" + trnfrorder.transaction_id);

                                cellData = rowData.GetCell(txnDateCellIndex.Value);
                                col3 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.transaction_date = DateTimeHelper.ConvertToDateTime(col3).HasValue ? DateTimeHelper.ConvertToDateTime(col3).Value : DateTime.Now;  // Convert.ToDateTime(col3);
                                logger.Info("Date :" + trnfrorder.transaction_date);

                                cellData = rowData.GetCell(AmountCellIndex.Value);
                                col6 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Amount = Convert.ToDouble(col6);
                                logger.Info("Amount :" + trnfrorder.Amount);

                                cellData = rowData.GetCell(PayModeCellIndex.Value);
                                col11 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.PayMode = Convert.ToString(col11);
                                logger.Info("Pay Mode :" + trnfrorder.PayMode);


                                trnfrorder.userid = userid;
                                HDFCUploader.Add(trnfrorder);

                            }
                            catch (Exception ex)
                            {
                                msgitemname = ex.Message;
                                logger.Error("Error adding customer in collection " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                        }
                    }
                }

                List<HDFCUpload> id = AddHDFCUpload(HDFCUploader, userid);

                string a = id[0].UploadId.ToString();
                return Created(a, a);

            }
            catch (Exception ex)
            {
                logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
            }

            return Created("Error", "Error");
        }



        public List<HDFCUpload> AddHDFCUpload(List<HDFCUpload> trCollection, int userId)
        {
            logger.Info("start addbulk");
            List<HDFCUpload> pdList = new List<HDFCUpload>();
            using (var context = new AuthContext())
            {

                try
                {
                    UploadFileReconcile TOM = new UploadFileReconcile();
                    var peopledata = context.Peoples.Where(x => x.PeopleID == userId).FirstOrDefault();

                    TOM = new UploadFileReconcile();
                    TOM.FileType = "HDFC";
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
                        var payment = payments.Where(x => x.GatewayTransId == transactionid).FirstOrDefault();
                        var CheckUpload = context.HDFCUploadDB.Where(x => x.transaction_id == transactionid && x.IsReconcile == true).FirstOrDefault();
                        if (CheckUpload == null)
                        {
                            HDFCUpload pd = new HDFCUpload();
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
                            pd.MerchantId = trCollection[i].MerchantId;
                            pd.Amount = trCollection[i].Amount;
                            pd.CCAvenueRef = trCollection[i].CCAvenueRef;
                            pd.PayMode = trCollection[i].PayMode;

                            pdList.Add(pd);
                        }

                        //context.EpaylaterUploadDB.Add(pd);

                    }

                    if (pdList != null && pdList.Any())
                    {
                        context.HDFCUploadDB.AddRange(pdList);

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


        #endregion

        #region Upload UPI
        public IHttpActionResult ReadUPIUploadedFile(XSSFWorkbook hssfwb, int userid)
        {
            string sSheetName = hssfwb.GetSheetName(0);
            ISheet sheet = hssfwb.GetSheet(sSheetName);

            IRow rowData;
            ICell cellData = null;
            try
            {
                List<UPIUpload> UPIUploader = new List<UPIUpload>();
                int? txnIdCellIndex = null;
                int? MerchantIdCellIndex = null;
                int? CCAvenueCellIndex = null;
                int? txnDateCellIndex = null;
                int? AmountCellIndex = null;
                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                {
                    if (iRowIdx == 0)
                    {
                        rowData = sheet.GetRow(iRowIdx);

                        if (rowData != null)
                        {
                            txnIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim().ToLower() == ("Order No").Trim().ToLower()) ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim().ToLower() == ("Order No").Trim().ToLower()).ColumnIndex : (int?)null;
                            if (!txnIdCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Order does not exist..try again");
                                return Created(strJSON, strJSON);
                            }
                            MerchantIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim().ToLower() == ("PG Merchant ID").Trim().ToLower()) ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim().ToLower() == ("PG Merchant ID").Trim().ToLower()).ColumnIndex : (int?)null;
                            if (!MerchantIdCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Merchant ID does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            txnDateCellIndex = rowData.Cells.Any(x => x.ToString().Trim().ToLower() == ("Transaction Date").Trim().ToLower()) ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim().ToLower() == ("Transaction Date").Trim().ToLower()).ColumnIndex : (int?)null;
                            if (!txnDateCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Transaction Date  does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            CCAvenueCellIndex = rowData.Cells.Any(x => x.ToString().Trim().ToLower() == ("Trans Ref No.").Trim().ToLower()) ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim().ToLower() == ("Trans Ref No.").Trim().ToLower()).ColumnIndex : (int?)null;
                            if (!CCAvenueCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Trans Ref No.. # does not exist..try again");
                                return Created(strJSON, strJSON);
                            }
                            AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim().ToLower() == ("Transaction Amount").Trim().ToLower()) ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim().ToLower() == ("Transaction Amount").Trim().ToLower()).ColumnIndex : (int?)null;
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

                        if (rowData != null)
                        {

                            cellData = rowData.GetCell(0);
                            rowData = sheet.GetRow(iRowIdx);
                            UPIUpload trnfrorder = new UPIUpload();
                            try
                            {

                                cellData = rowData.GetCell(txnIdCellIndex.Value);
                                col0 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.CCAvenueRef = Convert.ToString(col0);
                                logger.Info("Order No :" + trnfrorder.CCAvenueRef);

                                cellData = rowData.GetCell(MerchantIdCellIndex.Value);
                                col1 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.MerchantId = Convert.ToString(col1);
                                logger.Info("PG Merchant ID :" + trnfrorder.MerchantId);

                                cellData = rowData.GetCell(CCAvenueCellIndex.Value);
                                col2 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.transaction_id = Convert.ToString(col2);
                                logger.Info("Trans Ref No. :" + trnfrorder.transaction_id);

                                cellData = rowData.GetCell(txnDateCellIndex.Value);
                                col3 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.transaction_date = DateTimeHelper.ConvertToDateTime(col3).HasValue ? DateTimeHelper.ConvertToDateTime(col3).Value : DateTime.Now;  // Convert.ToDateTime(col3);
                                logger.Info("Transaction Date :" + trnfrorder.transaction_date);

                                cellData = rowData.GetCell(AmountCellIndex.Value);
                                col6 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Amount = Convert.ToDouble(col6);
                                logger.Info("Transaction Amount :" + trnfrorder.Amount);

                                trnfrorder.IsReconcile = false;
                                trnfrorder.userid = userid;

                                if (!UPIUploader.Any(x => x.transaction_id == trnfrorder.transaction_id))
                                {
                                    UPIUploader.Add(trnfrorder);

                                }

                            }
                            catch (Exception ex)
                            {
                                msgitemname = ex.Message;
                                logger.Error("Error adding customer in collection " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                        }
                    }
                }
                string a = "";
                if (UPIUploader != null && UPIUploader.Any() && userid > 0)
                {
                    List<UPIUpload> id = AddUPIUpload(UPIUploader, userid);
                    if(id.Count > 0)
                    {
                        a = id[0].UploadId.ToString();
                    }                    
                }

                return Created(a, a);

            }
            catch (Exception ex)
            {
                logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
            }

            return Created("Error", "Error");
        }

        public List<UPIUpload> AddUPIUpload(List<UPIUpload> trCollection, int userId)
        {
            logger.Info("start addbulk");
            List<UPIUpload> pdList = new List<UPIUpload>();
            using (var context = new AuthContext())
            {
                var peopledata = context.Peoples.Where(x => x.PeopleID == userId).FirstOrDefault();
                if (peopledata.Active)
                {
                    try
                    {
                        UploadFileReconcile TOM = new UploadFileReconcile();
                        TOM = new UploadFileReconcile();
                        TOM.FileType = "UPI";
                        TOM.UplaodBy = peopledata.DisplayName;
                        TOM.UploadDate = indianTime;
                        context.UploadFileReconcileDB.Add(TOM);
                        context.Commit();

                        var txnIds = trCollection.Select(x => x.transaction_id).Distinct().ToList();
                        var orderIds = trCollection.Select(x => x.CCAvenueRef).Distinct().ToList();
                        var payments = context.PaymentResponseRetailerAppDb.Where(x => orderIds.Contains(x.GatewayOrderId) && x.status == "Success" && x.amount > 0 && x.IsOnline).ToList();
                        for (var i = 0; i < trCollection.ToList().Count(); i++)
                        {
                            double amount = trCollection[i].Amount;
                            string transactionid = trCollection[i].transaction_id;
                            string CCAvenueRef = trCollection[i].CCAvenueRef;

                            var payment = payments.Where(x => x.GatewayOrderId == CCAvenueRef && x.PaymentFrom.Trim().ToLower() == "upi").ToList();
                            var upiTxnIdDetail = context.UPITransactions.Where(x => x.UPITxnID == transactionid && x.TxnNo == CCAvenueRef && x.IsActive && x.IsDeleted == false && x.Status == "SUCCESS").FirstOrDefault();
                            //if (upiTxnIdDetail != null)
                            //{
                                var CheckUpload = context.UPIUploadDB.Where(x => x.transaction_id == transactionid && x.IsReconcile == true && x.CCAvenueRef == CCAvenueRef).FirstOrDefault();
                                if (CheckUpload == null)
                                {
                                    UPIUpload pd = new UPIUpload();
                                    pd.UploadId = TOM.UploadId;
                                    if (payment != null && upiTxnIdDetail != null)
                                    {
                                        pd.IsReconcile = payment.Sum(x => x.amount) == trCollection[i].Amount;
                                    }
                                    else
                                    {
                                        pd.IsReconcile = false;
                                    }
                                    pd.transaction_id = trCollection[i].transaction_id;
                                    pd.transaction_date = trCollection[i].transaction_date;
                                    pd.MerchantId = trCollection[i].MerchantId;
                                    pd.Amount = trCollection[i].Amount;
                                    pd.CCAvenueRef = trCollection[i].CCAvenueRef;
                                    pd.PayMode = trCollection[i].PayMode;
                                    pdList.Add(pd);
                                }
                            //}
                        }

                        if (pdList != null && pdList.Any())
                        {
                            context.UPIUploadDB.AddRange(pdList);

                            if (pdList.All(x => x.IsReconcile))
                            {
                                TOM.IsReconcile = true;
                                context.Entry(TOM).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                        context.Commit();
                        if (pdList.Any(x => x.IsReconcile))
                        {
                            var reconciledTxns = pdList.Where(x => x.IsReconcile).Select(x => x.CCAvenueRef).Distinct().ToList();
                            payments.Where(x => reconciledTxns.Contains(x.GatewayOrderId)).ToList().ForEach(payment =>
                            {
                                payment.UploadId = pdList.FirstOrDefault(x => x.CCAvenueRef == payment.GatewayOrderId.ToString()).Id;
                                payment.IsSettled = true;
                                payment.UpdatedDate = indianTime;
                                payment.SettleDate = indianTime;
                                payment.SettledBy = peopledata.DisplayName;
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


            }
            return pdList;
        }

        #endregion

        #region Upload MPos
        public IHttpActionResult ReadMPosUploadedFile(XSSFWorkbook hssfwb, int userid)
        {
            string sSheetName = hssfwb.GetSheetName(0);
            ISheet sheet = hssfwb.GetSheet(sSheetName);

            IRow rowData;
            ICell cellData = null;
            try
            {
                List<MposUpload> MposUploader = new List<MposUpload>();
                int? txnIdCellIndex = null;
                int? DateCellIndex = null;
                int? UserCellIndex = null;
                int? userNameCellIndex = null;
                int? AmountCellIndex = null;
                int? CardtypeCellIndex = null;
                int? BrandTypeCellIndex = null;

                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                {
                    if (iRowIdx == 0)
                    {
                        rowData = sheet.GetRow(iRowIdx);

                        if (rowData != null)
                        {


                            txnIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "ID") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "ID").ColumnIndex : (int?)null;
                            if (!txnIdCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("transaction_id does not exist..try again");
                                return Created(strJSON, strJSON); ;
                            }

                            DateCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Date") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Date").ColumnIndex : (int?)null;
                            if (!DateCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Date does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            UserCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "User") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "User").ColumnIndex : (int?)null;
                            if (!UserCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("User does not exist..try again");
                                return Created(strJSON, strJSON);
                            }


                            userNameCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Username") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Username").ColumnIndex : (int?)null;
                            if (!userNameCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Username does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Amount") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Amount").ColumnIndex : (int?)null;
                            if (!AmountCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Amount does not exist..try again");
                                return Created(strJSON, strJSON);
                            }


                            CardtypeCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Card Type") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Card Type").ColumnIndex : (int?)null;
                            if (!CardtypeCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Card Type does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            BrandTypeCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Brand Type") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Brand Type").ColumnIndex : (int?)null;
                            if (!BrandTypeCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Brand Type does not exist..try again");
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
                            MposUpload trnfrorder = new MposUpload();
                            try
                            {

                                //int requestTowarehouseId;
                                cellData = rowData.GetCell(txnIdCellIndex.Value);
                                col0 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.transaction_id = Convert.ToString(col0);
                                logger.Info("ID :" + trnfrorder.transaction_id);

                                cellData = rowData.GetCell(DateCellIndex.Value);
                                col1 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.transaction_date = DateTimeHelper.ConvertToDateTime(col1).HasValue ? DateTimeHelper.ConvertToDateTime(col1).Value : DateTime.Now;//Convert.ToDateTime(col1);
                                logger.Info("Date :" + trnfrorder.transaction_date);

                                cellData = rowData.GetCell(UserCellIndex.Value);
                                col4 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.UserName = Convert.ToString(col4);
                                logger.Info("User :" + trnfrorder.UserName);

                                cellData = rowData.GetCell(userNameCellIndex.Value);
                                col5 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.User_Mobile = Convert.ToString(col5);
                                logger.Info("Username :" + trnfrorder.User_Mobile);

                                cellData = rowData.GetCell(AmountCellIndex.Value);
                                col8 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Amount = Convert.ToDouble(col8);
                                logger.Info("Amount :" + trnfrorder.Amount);

                                cellData = rowData.GetCell(CardtypeCellIndex.Value);
                                col14 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Cardtype = Convert.ToString(col14);
                                logger.Info("Card Type :" + trnfrorder.Cardtype);

                                cellData = rowData.GetCell(BrandTypeCellIndex.Value);
                                col15 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Brandtype = Convert.ToString(col15);
                                logger.Info("Brand Type :" + trnfrorder.Brandtype);


                                trnfrorder.userid = userid;
                                MposUploader.Add(trnfrorder);

                            }
                            catch (Exception ex)
                            {
                                msgitemname = ex.Message;
                                logger.Error("Error adding customer in collection " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                        }
                    }
                }
                List<MposUpload> id = AddMposUpload(MposUploader, userid);

                string a = id[0].UploadId.ToString();
                return Created(a, a);

            }
            catch (Exception ex)
            {
                logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
            }

            return Created("Error", "Error");
        }

        public List<MposUpload> AddMposUpload(List<MposUpload> trCollection, int userId)
        {
            logger.Info("start addbulk");
            List<MposUpload> pdList = new List<MposUpload>();
            using (var context = new AuthContext())
            {
                try
                {
                    UploadFileReconcile TOM = new UploadFileReconcile();
                    var peopledata = context.Peoples.Where(x => x.PeopleID == userId).FirstOrDefault();

                    TOM = new UploadFileReconcile();
                    TOM.FileType = "Mpos";
                    TOM.UplaodBy = peopledata.DisplayName;
                    TOM.UploadDate = indianTime;
                    context.UploadFileReconcileDB.Add(TOM);
                    context.Commit();

                    var txnIds = trCollection.Select(x => x.transaction_id).ToList();
                    var payments = context.PaymentResponseRetailerAppDb.Where(x => txnIds.Contains(x.GatewayTransId) && x.status == "Success" && x.amount > 0).ToList();



                    for (var i = 0; i < trCollection.ToList().Count(); i++)
                    {
                        double amount = trCollection[i].Amount;
                        string transactionid = trCollection[i].transaction_id;
                        var payment = payments.Where(x => x.GatewayTransId == transactionid).FirstOrDefault();
                        var CheckUpload = context.MposUploadDB.Where(x => x.transaction_id == transactionid && x.IsReconcile == true).FirstOrDefault();
                        if (CheckUpload == null)
                        {
                            MposUpload pd = new MposUpload();
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
                            pd.UserName = trCollection[i].UserName;
                            pd.User_Mobile = trCollection[i].User_Mobile;
                            pd.Amount = trCollection[i].Amount;
                            pd.Cardtype = trCollection[i].Cardtype;
                            pd.Brandtype = trCollection[i].Brandtype;

                            pdList.Add(pd);
                        }
                        //context.EpaylaterUploadDB.Add(pd);


                    }

                    if (pdList != null && pdList.Any())
                    {
                        context.MposUploadDB.AddRange(pdList);

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

        #endregion

        #region Upload HDFC-Credit
        public IHttpActionResult ReadHDfCCreditUploadedFile(XSSFWorkbook hssfwb, int userid)
        {
            string sSheetName = hssfwb.GetSheetName(0);
            ISheet sheet = hssfwb.GetSheet(sSheetName);

            IRow rowData;
            ICell cellData = null;
            try
            {
                List<HDFCCreditUpload> HDFCUploader = new List<HDFCCreditUpload>();
                int? txnIdCellIndex = null;
                int? MerchantIdCellIndex = null;
                int? CCAvenueCellIndex = null;
                int? txnDateCellIndex = null;
                int? AmountCellIndex = null;
                int? PayModeCellIndex = null;
                int? txnStatusCellIndex = null;
                // int? txnCurrencyCellIndex = null;
                int? FeesCellIndex = null;
                int? TaxCellIndex = null;
                int? AmountPayableCellIndex = null;
                int? GatewayCellIndex = null;
                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                {
                    if (iRowIdx == 0)
                    {
                        rowData = sheet.GetRow(iRowIdx);

                        if (rowData != null)
                        {


                            txnIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Order #") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Order #").ColumnIndex : (int?)null;
                            if (!txnIdCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Order does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            MerchantIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Merchant ID") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Merchant ID").ColumnIndex : (int?)null;
                            if (!MerchantIdCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Merchant ID does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            CCAvenueCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "CCAvenue Ref. #") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "CCAvenue Ref. #").ColumnIndex : (int?)null;
                            if (!CCAvenueCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("CCAvenue Ref. # does not exist..try again");
                                return Created(strJSON, strJSON);
                            }


                            txnDateCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Date") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Date").ColumnIndex : (int?)null;
                            if (!txnDateCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Date does not exist..try again");
                                return Created(strJSON, strJSON);
                            }
                            txnStatusCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Status") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Status").ColumnIndex : (int?)null;
                            if (!txnStatusCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Date does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Amount") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Amount").ColumnIndex : (int?)null;
                            if (!AmountCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Amount does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            FeesCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Fees") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Fees").ColumnIndex : (int?)null;
                            if (!FeesCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Amount does not exist..try again");
                                return Created(strJSON, strJSON);
                            }
                            TaxCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Tax") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Tax").ColumnIndex : (int?)null;
                            if (!TaxCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Amount does not exist..try again");
                                return Created(strJSON, strJSON);
                            }
                            AmountPayableCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Amount Payable") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Amount Payable").ColumnIndex : (int?)null;
                            if (!AmountPayableCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Amount does not exist..try again");
                                return Created(strJSON, strJSON);
                            }
                            GatewayCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Gateway") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Gateway").ColumnIndex : (int?)null;
                            if (!GatewayCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Amount does not exist..try again");
                                return Created(strJSON, strJSON);
                            }
                            PayModeCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Pay Mode") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Pay Mode").ColumnIndex : (int?)null;
                            if (!PayModeCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize(" Pay Mode does not exist..try again");
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
                            HDFCCreditUpload trnfrorder = new HDFCCreditUpload();
                            try
                            {

                                //int requestTowarehouseId;
                                cellData = rowData.GetCell(txnIdCellIndex.Value);
                                col0 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.OrderId = Convert.ToInt32(col0);
                                logger.Info("Order # :" + trnfrorder.OrderId);

                                cellData = rowData.GetCell(MerchantIdCellIndex.Value);
                                col1 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.MerchantID = Convert.ToInt32(col1);
                                logger.Info("Merchant ID :" + trnfrorder.MerchantID);

                                cellData = rowData.GetCell(CCAvenueCellIndex.Value);
                                col2 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.CCAvenueRef = Convert.ToString(col2);
                                logger.Info("CCAvenue Ref. # :" + trnfrorder.CCAvenueRef);

                                cellData = rowData.GetCell(txnDateCellIndex.Value);
                                col3 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Date = DateTimeHelper.ConvertToDateTime(col3).HasValue ? DateTimeHelper.ConvertToDateTime(col3).Value : DateTime.Now;  // Convert.ToDateTime(col3);
                                logger.Info("Date :" + trnfrorder.Date);

                                cellData = rowData.GetCell(txnStatusCellIndex.Value);
                                col4 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Status = Convert.ToString(col4);
                                logger.Info("Status :" + trnfrorder.Status);

                                //cellData = rowData.GetCell(txnCurrencyCellIndex.Value);
                                //col5 = cellData == null ? "" : cellData.ToString();
                                //trnfrorder.Currency = Convert.ToString(col5);
                                //logger.Info("Currency :" + trnfrorder.Currency);

                                cellData = rowData.GetCell(AmountCellIndex.Value);
                                col6 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Amount = Convert.ToDouble(col6);
                                logger.Info("Amount :" + trnfrorder.Amount);

                                cellData = rowData.GetCell(FeesCellIndex.Value);
                                col7 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Fees = Convert.ToDouble(col7);
                                logger.Info("Fees :" + trnfrorder.Fees);

                                cellData = rowData.GetCell(TaxCellIndex.Value);
                                col8 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Tax = Convert.ToDouble(col8);
                                logger.Info("Tax :" + trnfrorder.Tax);

                                cellData = rowData.GetCell(AmountPayableCellIndex.Value);
                                col9 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.AmountPayable = Convert.ToDouble(col9);
                                logger.Info("AmountPayable :" + trnfrorder.AmountPayable);

                                cellData = rowData.GetCell(GatewayCellIndex.Value);
                                col10 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Gateway = Convert.ToString(col10);
                                logger.Info("Gateway :" + trnfrorder.Gateway);

                                cellData = rowData.GetCell(PayModeCellIndex.Value);
                                col11 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.PayMode = Convert.ToString(col11);
                                logger.Info("PayMode :" + trnfrorder.PayMode);


                                trnfrorder.userid = userid;
                                HDFCUploader.Add(trnfrorder);

                            }
                            catch (Exception ex)
                            {
                                msgitemname = ex.Message;
                                logger.Error("Error adding customer in collection " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                        }
                    }
                }

                List<HDFCCreditUpload> id = AddHDFCCreditUpload(HDFCUploader, userid);

                string a = id[0].UploadId.ToString();
                return Created(a, a);

            }
            catch (Exception ex)
            {
                logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
            }

            return Created("Error", "Error");
        }
        public List<HDFCCreditUpload> AddHDFCCreditUpload(List<HDFCCreditUpload> trCollection, int userId)
        {
            logger.Info("start addbulk");
            List<HDFCCreditUpload> pdList = new List<HDFCCreditUpload>();
            using (var context = new AuthContext())
            {
                try
                {
                    UploadFileReconcile TOM = new UploadFileReconcile();
                    var peopledata = context.Peoples.Where(x => x.PeopleID == userId).FirstOrDefault();

                    TOM = new UploadFileReconcile();
                    TOM.FileType = "credit hdfc";
                    TOM.UplaodBy = peopledata.DisplayName;
                    TOM.UploadDate = indianTime;
                    context.UploadFileReconcileDB.Add(TOM);
                    context.Commit();

                    var txnIds = trCollection.Select(x => x.CCAvenueRef).ToList();
                    var payments = context.PaymentResponseRetailerAppDb.Where(x => txnIds.Contains(x.GatewayTransId) && x.status == "Success" && x.amount > 0).ToList();

                    //var uploadedTxnIds = 


                    for (var i = 0; i < trCollection.ToList().Count(); i++)
                    {
                        double amount = trCollection[i].Amount;
                        string transactionid = trCollection[i].CCAvenueRef;
                        var payment = payments.Where(x => x.GatewayTransId == transactionid).FirstOrDefault();
                        var CheckUpload = context.HDFCCreditUploadDB.Where(x => x.CCAvenueRef == transactionid && x.IsReconcile == true).FirstOrDefault();
                        if (CheckUpload == null)
                        {
                            HDFCCreditUpload hdfcCreditUpload = new HDFCCreditUpload();
                            hdfcCreditUpload.UploadId = TOM.UploadId;
                            if (payment != null)
                            {
                                hdfcCreditUpload.IsReconcile = payment.amount == trCollection[i].Amount;
                            }
                            else
                            {
                                hdfcCreditUpload.IsReconcile = false;
                            }
                            hdfcCreditUpload.CCAvenueRef = trCollection[i].CCAvenueRef;
                            hdfcCreditUpload.Date = trCollection[i].Date;
                            hdfcCreditUpload.MerchantID = trCollection[i].MerchantID;
                            hdfcCreditUpload.Amount = trCollection[i].Amount;
                            hdfcCreditUpload.Status = trCollection[i].Status;
                            // HDFCCredit.Currency = trCollection[i].Currency;
                            hdfcCreditUpload.Amount = trCollection[i].Amount;
                            hdfcCreditUpload.Fees = trCollection[i].Fees;
                            hdfcCreditUpload.Tax = trCollection[i].Tax;
                            hdfcCreditUpload.AmountPayable = trCollection[i].AmountPayable;
                            hdfcCreditUpload.Gateway = trCollection[i].Gateway;
                            hdfcCreditUpload.PayMode = trCollection[i].PayMode;

                            pdList.Add(hdfcCreditUpload);
                        }

                        //context.EpaylaterUploadDB.Add(pd);

                    }

                    if (pdList != null && pdList.Any())
                    {
                        context.HDFCCreditUploadDB.AddRange(pdList);

                        if (pdList.All(x => x.IsReconcile))
                        {
                            TOM.IsReconcile = true;
                            context.Entry(TOM).State = System.Data.Entity.EntityState.Modified;
                        }
                    }

                    context.Commit();

                    if (pdList.Any(x => x.IsReconcile))
                    {
                        var reconciledTxns = pdList.Where(x => x.IsReconcile).Select(x => x.CCAvenueRef).ToList();
                        payments.Where(x => reconciledTxns.Contains(x.GatewayTransId)).ToList().ForEach(payment =>
                        {
                            payment.UploadId = pdList.FirstOrDefault(x => x.CCAvenueRef == payment.GatewayTransId).Id;
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

        #endregion

        #region Upload RazorpayQR
        public IHttpActionResult RazorpayQRUploadedFile(XSSFWorkbook hssfwb, int userid)
        {
            string sSheetName = hssfwb.GetSheetName(0);
            ISheet sheet = hssfwb.GetSheet(sSheetName);

            IRow rowData;
            ICell cellData = null;
            try
            {
                List<RazorpayQRUpload> RazorpayQRUploader = new List<RazorpayQRUpload>();
                int? txnIdCellIndex = null;
                int? AmountCellIndex = null;
                int? txnStatusCellIndex = null;
                // int? orderIdCellIndex = null;
                int? methodCellIndex = null;
                int? descriptionCellIndex = null;
                int? vpaCellIndex = null;
                // int? GatewayCellIndex = null;
                int? contactCellIndex = null;
                int? txnDateCellIndex = null;
                int? notesCellIndex = null;
                int? FeeCellIndex = null;
                int? taxCellIndex = null;


                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                {
                    if (iRowIdx == 0)
                    {
                        rowData = sheet.GetRow(iRowIdx);

                        if (rowData != null)
                        {


                            txnIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "id") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "id").ColumnIndex : (int?)null;
                            if (!txnIdCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("id does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "amount") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "amount").ColumnIndex : (int?)null;
                            if (!AmountCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("amount does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            txnStatusCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "status") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "status").ColumnIndex : (int?)null;
                            if (!txnStatusCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("status does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            //orderIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "order_id") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "order_id").ColumnIndex : (int?)null;
                            //if (!orderIdCellIndex.HasValue)
                            //{
                            //    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("order_id does not exist..try again");
                            //    return Created(strJSON, strJSON);
                            //}

                            methodCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "method") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "method").ColumnIndex : (int?)null;
                            if (!methodCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("method does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            descriptionCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "description") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "description").ColumnIndex : (int?)null;
                            if (!descriptionCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("description does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            vpaCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "vpa") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "vpa").ColumnIndex : (int?)null;
                            if (!vpaCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("vpa does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            contactCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "contact") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "contact").ColumnIndex : (int?)null;
                            if (!contactCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("contact does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            notesCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "notes") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "notes").ColumnIndex : (int?)null;
                            if (!notesCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("notes does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            FeeCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "fee") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "fee").ColumnIndex : (int?)null;
                            if (!FeeCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("fee does not exist..try again");
                                return Created(strJSON, strJSON);
                            }
                            taxCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "tax") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "tax").ColumnIndex : (int?)null;
                            if (!taxCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("tax does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            txnDateCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "created_at") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "created_at").ColumnIndex : (int?)null;
                            if (!txnDateCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("created_at does not exist..try again");
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
                            RazorpayQRUpload trnfrorder = new RazorpayQRUpload();
                            try
                            {

                                //int requestTowarehouseId;
                                cellData = rowData.GetCell(txnIdCellIndex.Value);
                                col0 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.GatewayTransId = Convert.ToString(col0);
                                logger.Info("GatewayTransId :" + trnfrorder.GatewayTransId);

                                cellData = rowData.GetCell(AmountCellIndex.Value);
                                col1 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Amount = Convert.ToInt32(col1);
                                logger.Info("Amount :" + trnfrorder.Amount);

                                cellData = rowData.GetCell(txnStatusCellIndex.Value);
                                col4 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Status = Convert.ToString(col4);
                                logger.Info("Status :" + trnfrorder.Status);

                                //cellData = rowData.GetCell(orderIdCellIndex.Value);
                                //col5 = cellData == null ? "" : cellData.ToString();
                                //trnfrorder.OrderId = Convert.ToInt32(col5);
                                //logger.Info("OrderId :" + trnfrorder.OrderId);

                                cellData = rowData.GetCell(methodCellIndex.Value);
                                col7 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Method = Convert.ToString(col7);
                                logger.Info("Method :" + trnfrorder.Method);

                                cellData = rowData.GetCell(descriptionCellIndex.Value);
                                col12 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Description = Convert.ToString(col12);
                                logger.Info("Description :" + trnfrorder.Description);

                                cellData = rowData.GetCell(vpaCellIndex.Value);
                                col17 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.VPA = Convert.ToString(col17);
                                logger.Info("VPA :" + trnfrorder.VPA);


                                cellData = rowData.GetCell(contactCellIndex.Value);
                                col19 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.contact = Convert.ToString(col19);
                                logger.Info("contact :" + trnfrorder.contact);


                                cellData = rowData.GetCell(notesCellIndex.Value);
                                col20 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Notes = Convert.ToString(col20);
                                logger.Info("Notes :" + trnfrorder.Notes);


                                cellData = rowData.GetCell(FeeCellIndex.Value);
                                col21 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Fees = Convert.ToDouble(col21);
                                logger.Info("Fees :" + trnfrorder.Fees);


                                cellData = rowData.GetCell(taxCellIndex.Value);
                                col22 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Tax = Convert.ToDouble(col22);
                                logger.Info("Tax :" + trnfrorder.Tax);

                                cellData = rowData.GetCell(txnDateCellIndex.Value);
                                col25 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Date = DateTimeHelper.ConvertToDateTime(col25).HasValue ? DateTimeHelper.ConvertToDateTime(col25).Value : DateTime.Now;  // Convert.ToDateTime(col3);
                                logger.Info("Date :" + trnfrorder.Date);

                                trnfrorder.userid = userid;
                                RazorpayQRUploader.Add(trnfrorder);

                            }
                            catch (Exception ex)
                            {
                                msgitemname = ex.Message;
                                logger.Error("Error adding customer in collection " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                        }
                    }
                }

                List<RazorpayQRUpload> id = AddRazorpayQRUpload(RazorpayQRUploader, userid);

                string a = id[0].UploadId.ToString();
                return Created(a, a);

            }
            catch (Exception ex)
            {
                logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
            }

            return Created("Error", "Error");
        }
        public List<RazorpayQRUpload> AddRazorpayQRUpload(List<RazorpayQRUpload> trCollection, int userId)
        {
            logger.Info("start RazorpayQR");
            List<RazorpayQRUpload> pdList = new List<RazorpayQRUpload>();
            using (var context = new AuthContext())
            {
                try
                {
                    UploadFileReconcile TOM = new UploadFileReconcile();
                    var peopledata = context.Peoples.Where(x => x.PeopleID == userId).FirstOrDefault();

                    TOM = new UploadFileReconcile();
                    TOM.FileType = "Razorpay QR";
                    TOM.UplaodBy = peopledata.DisplayName;
                    TOM.UploadDate = indianTime;
                    context.UploadFileReconcileDB.Add(TOM);
                    context.Commit();

                    var txnIds = trCollection.Select(x => x.GatewayTransId).ToList();
                    var payments = context.PaymentResponseRetailerAppDb.Where(x => txnIds.Contains(x.GatewayTransId) && x.status == "Success" && x.amount > 0).ToList();

                    //var uploadedTxnIds = 


                    for (var i = 0; i < trCollection.ToList().Count(); i++)
                    {
                        double amount = trCollection[i].Amount;
                        string transactionid = trCollection[i].GatewayTransId;
                        var payment = payments.Where(x => x.GatewayTransId == transactionid).FirstOrDefault();
                        var CheckUpload = context.RazorpayQRUploadDB.Where(x => x.GatewayTransId == transactionid && x.IsReconcile == true).FirstOrDefault();
                        if (CheckUpload == null)
                        {
                            RazorpayQRUpload RazorpayQRUpload = new RazorpayQRUpload();
                            RazorpayQRUpload.UploadId = TOM.UploadId;
                            if (payment != null)
                            {
                                RazorpayQRUpload.IsReconcile = payment.amount == trCollection[i].Amount;
                            }
                            else
                            {
                                RazorpayQRUpload.IsReconcile = false;
                            }
                            RazorpayQRUpload.GatewayTransId = trCollection[i].GatewayTransId;

                            RazorpayQRUpload.Amount = trCollection[i].Amount;
                            RazorpayQRUpload.Status = trCollection[i].Status;
                            RazorpayQRUpload.Amount = trCollection[i].Amount;
                            //  RazorpayQRUpload.OrderId = trCollection[i].OrderId;
                            RazorpayQRUpload.Method = trCollection[i].Method;
                            RazorpayQRUpload.Description = trCollection[i].Description;
                            RazorpayQRUpload.VPA = trCollection[i].VPA;
                            RazorpayQRUpload.contact = trCollection[i].contact;
                            RazorpayQRUpload.Notes = trCollection[i].Notes;
                            RazorpayQRUpload.VPA = trCollection[i].VPA;
                            RazorpayQRUpload.Fees = trCollection[i].Fees;
                            RazorpayQRUpload.Tax = trCollection[i].Tax;
                            RazorpayQRUpload.Date = trCollection[i].Date;
                            pdList.Add(RazorpayQRUpload);
                        }

                        //context.EpaylaterUploadDB.Add(pd);

                    }

                    if (pdList != null && pdList.Any())
                    {
                        context.RazorpayQRUploadDB.AddRange(pdList);

                        if (pdList.All(x => x.IsReconcile))
                        {
                            TOM.IsReconcile = true;
                            context.Entry(TOM).State = System.Data.Entity.EntityState.Modified;
                        }
                    }

                    context.Commit();

                    if (pdList.Any(x => x.IsReconcile))
                    {
                        var reconciledTxns = pdList.Where(x => x.IsReconcile).Select(x => x.GatewayTransId).ToList();
                        payments.Where(x => reconciledTxns.Contains(x.GatewayTransId)).ToList().ForEach(payment =>
                        {
                            payment.UploadId = pdList.FirstOrDefault(x => x.GatewayTransId == payment.GatewayTransId).Id;
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

        #endregion

        #region Chqbook
        public IHttpActionResult ChqBookUpload(XSSFWorkbook hssfwb, int userid)
        {
            string sSheetName = hssfwb.GetSheetName(0);
            ISheet sheet = hssfwb.GetSheet(sSheetName);
            IRow rowData;
            ICell cellData = null;
            try
            {
                List<ChqBookUpload> ChequeUpload = new List<ChqBookUpload>();
                int? TRANSACTION_ID = null;
                int? Order_ref = null;
                int? InvoiceAmt = null;
                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                {
                    if (iRowIdx == 0)
                    {
                        rowData = sheet.GetRow(iRowIdx);

                        if (rowData != null)
                        {
                            TRANSACTION_ID = rowData.Cells.Any(x => x.ToString().Trim() == "TRANSACTION ID") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "TRANSACTION ID").ColumnIndex : (int?)null;
                            if (!TRANSACTION_ID.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("TRANSACTION ID does not exist..try again");
                                return Created(strJSON, strJSON);
                            }
                            Order_ref = rowData.Cells.Any(x => x.ToString().Trim() == "Order_ref") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Order_ref").ColumnIndex : (int?)null;
                            if (!Order_ref.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Order_ref does not exist..try again");
                                return Created(strJSON, strJSON);
                            }
                            InvoiceAmt = rowData.Cells.Any(x => x.ToString().Trim() == "Invoice Amt.") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Invoice Amt.").ColumnIndex : (int?)null;
                            if (!InvoiceAmt.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Invoice Amt. does not exist..try again");
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
                            ChqBookUpload trnfrorder = new ChqBookUpload();
                            try
                            {
                                cellData = rowData.GetCell(TRANSACTION_ID.Value);
                                col0 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.TRANSACTION_ID = Convert.ToString(col0);
                                logger.Info("TRANSACTION_ID :" + trnfrorder.TRANSACTION_ID);

                                cellData = rowData.GetCell(Order_ref.Value);
                                col1 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Orderid = Convert.ToInt32(col1);
                                logger.Info("Order_ref :" + trnfrorder.Orderid);

                                cellData = rowData.GetCell(InvoiceAmt.Value);
                                col7 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.InvoiceAmt = Convert.ToDouble(col7);
                                logger.Info("Invoice Amt  :" + trnfrorder.InvoiceAmt);
                                trnfrorder.userid = userid;
                                ChequeUpload.Add(trnfrorder);

                            }
                            catch (Exception ex)
                            {
                                msgitemname = ex.Message;
                                logger.Error("Error adding customer in collection " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                        }
                    }
                }

                List<ChqBookUpload> id = ChqBookUploadReconciliation(ChequeUpload, userid);
                string a = id[0].UploadId.ToString();
                return Created(a, a);
            }
            catch (Exception ex)
            {
                logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
            }
            return Created("Error", "Error");
        }
        public List<ChqBookUpload> ChqBookUploadReconciliation(List<ChqBookUpload> trCollection, int userId)
        {
            logger.Info("start ChequeUploadDetails");
            List<ChqBookUpload> pdList = new List<ChqBookUpload>();
            using (var context = new AuthContext())
            {
                var peopledata = context.Peoples.Where(x => x.PeopleID == userId).FirstOrDefault();
                try
                {
                    UploadFileReconcile TOM = new UploadFileReconcile();
                    TOM = new UploadFileReconcile();
                    TOM.FileType = "Cheque Upload";
                    TOM.UplaodBy = peopledata.DisplayName;
                    TOM.UploadDate = indianTime;
                    context.UploadFileReconcileDB.Add(TOM);
                    context.Commit();
                    var txnIds = trCollection.Select(x => x.TRANSACTION_ID).ToList();
                    var payments = context.PaymentResponseRetailerAppDb.Where(x => txnIds.Contains(x.GatewayTransId.Trim()) && x.status == "Success" && x.amount > 0).ToList();
                    for (var i = 0; i < trCollection.ToList().Count(); i++)
                    {
                        double amount = trCollection[i].InvoiceAmt;
                        string transactionid = trCollection[i].TRANSACTION_ID;
                        int orderId = trCollection[i].Orderid;

                        var payment = payments.Where(x => x.GatewayTransId == transactionid && x.OrderId == orderId).FirstOrDefault();
                        var ChequeUpload = context.ChqBookUploads.Where(x => x.TRANSACTION_ID == transactionid && x.IsReconcile == true).FirstOrDefault();
                        if (ChequeUpload == null && payment != null)
                        {
                            ChqBookUpload ChequeUploads = new ChqBookUpload();
                            ChequeUploads.UploadId = TOM.UploadId;
                            if (payment != null)
                            {
                                ChequeUploads.IsReconcile = payment.amount == trCollection[i].InvoiceAmt;
                            }
                            else
                            {
                                ChequeUploads.IsReconcile = false;
                            }
                            ChequeUploads.TRANSACTION_ID = trCollection[i].TRANSACTION_ID;
                            ChequeUploads.Orderid = trCollection[i].Orderid;
                            ChequeUploads.InvoiceAmt = trCollection[i].InvoiceAmt;
                            pdList.Add(ChequeUploads);
                        }
                    }

                    if (pdList != null && pdList.Any())
                    {
                        context.ChqBookUploads.AddRange(pdList);
                        //  if (pdList.All(x => x.TRANSACTION_ID))
                        {
                            TOM.IsReconcile = true;
                            context.Entry(TOM).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                    context.Commit();
                    if (pdList.Any(x => x.IsReconcile))
                    {
                        var reconciledTxns = pdList.Where(x => x.IsReconcile).Select(x => x.TRANSACTION_ID).ToList();
                        payments.Where(x => reconciledTxns.Contains(x.GatewayTransId)).ToList().ForEach(payment =>
                        {
                            payment.UploadId = pdList.FirstOrDefault(x => x.TRANSACTION_ID == payment.GatewayTransId).Id;
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
        #endregion

        #region Upload icici
        public IHttpActionResult ReadICICIUploadedFile(XSSFWorkbook hssfwb, int userid)
        {
            string sSheetName = hssfwb.GetSheetName(0);
            ISheet sheet = hssfwb.GetSheet(sSheetName);

            IRow rowData;
            ICell cellData = null;
            try
            {
                List<ICICIUpload> ICICIUploader = new List<ICICIUpload>();
                int? txnIdCellIndex = null;
                int? MerchantIdCellIndex = null;
                int? TransTypeCellIndex = null;
                int? txnDateCellIndex = null;
                int? AmountCellIndex = null;
                int? PayModeCellIndex = null;
                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                {
                    if (iRowIdx == 0)
                    {
                        rowData = sheet.GetRow(iRowIdx);

                        if (rowData != null)
                        {
                            txnIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Transaction ID") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Transaction ID").ColumnIndex : (int?)null;
                            if (!txnIdCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Transaction ID does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            MerchantIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Merchant RefNo") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Merchant RefNo").ColumnIndex : (int?)null;
                            if (!MerchantIdCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Merchant RefNo does not exist..try again");
                                return Created(strJSON, strJSON);
                            }


                            AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Amount") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Amount").ColumnIndex : (int?)null;
                            if (!AmountCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize(" Amount does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            txnDateCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Transmission Date Time") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Transmission Date Time").ColumnIndex : (int?)null;
                            if (!txnDateCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize(" Transmission Date Time does not exist..try again");
                                return Created(strJSON, strJSON);
                            }

                            PayModeCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Payment Mode") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Payment Mode").ColumnIndex : (int?)null;
                            if (!PayModeCellIndex.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize(" Payment Mode does not exist..try again");
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
                            ICICIUpload trnfrorder = new ICICIUpload();
                            try
                            {
                                cellData = rowData.GetCell(txnIdCellIndex.Value);
                                col1 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.TransactionID =col1.ToString();
                                logger.Info("TransactionID:" + trnfrorder.TransactionID);

                                cellData = rowData.GetCell(MerchantIdCellIndex.Value);
                                col2 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.MerchantRefNo = col2.ToString();
                                logger.Info("MerchantRefNo :" + trnfrorder.MerchantRefNo);

                                cellData = rowData.GetCell(AmountCellIndex.Value);
                                col3 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.Amount = Convert.ToDouble(col3);
                                logger.Info("Amount :" + trnfrorder.Amount);

                                cellData = rowData.GetCell(txnDateCellIndex.Value);
                                col4 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.TransmissionDateTime = DateTimeHelper.ConvertToDateTime(col4).HasValue ? DateTimeHelper.ConvertToDateTime(col4).Value : DateTime.Now;  // Convert.ToDateTime(col3);
                                logger.Info("TransmissionDateTime :" + trnfrorder.TransmissionDateTime);
                                
                                cellData = rowData.GetCell(PayModeCellIndex.Value);
                                col5 = cellData == null ? "" : cellData.ToString();
                                trnfrorder.PaymentMode = col5.ToString();
                                logger.Info("PaymentMode :" + trnfrorder.PaymentMode);
                                
                                //trnfrorder.UserID = userid;
                                ICICIUploader.Add(trnfrorder);

                            }
                            catch (Exception ex)
                            {
                                msgitemname = ex.Message;
                                logger.Error("Error adding customer in collection " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                        }
                    }
                }

                List<ICICIUpload> id = AddICICIUpload(ICICIUploader,userid);

                string a = id[0].UploadId.ToString();
                return Created(a, a);

            }
            catch (Exception ex)
            {
                logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
            }

            return Created("Error", "Error");
        }

        public List<ICICIUpload> AddICICIUpload(List<ICICIUpload> trCollection, int userId)
        {
            logger.Info("start addbulk");
            List<ICICIUpload> pdList = new List<ICICIUpload>();
            using (var context = new AuthContext())
            {

                try
                {
                    UploadFileReconcile TOM = new UploadFileReconcile();
                    var peopledata = context.Peoples.Where(x => x.PeopleID == userId).FirstOrDefault();

                    TOM = new UploadFileReconcile();
                    TOM.FileType = "ICICI";
                    TOM.UplaodBy = peopledata.DisplayName;
                    TOM.UploadDate = indianTime;
                    context.UploadFileReconcileDB.Add(TOM);
                    context.Commit();

                    var txnIds = trCollection.Select(x => x.TransactionID).ToList();
                    var payments = context.PaymentResponseRetailerAppDb.Where(x => txnIds.Contains(x.GatewayTransId) && x.status == "Success" && x.amount > 0).ToList();

                    for (var i = 0; i < trCollection.ToList().Count(); i++)
                    {
                        double amount = trCollection[i].Amount;
                        string transactionid = trCollection[i].TransactionID;
                        var payment = payments.Where(x => x.GatewayTransId == transactionid).FirstOrDefault();
                        var CheckUpload = context.HDFCUploadDB.Where(x => x.transaction_id == transactionid && x.IsReconcile == true).FirstOrDefault();
                        if (CheckUpload == null)
                        {
                            ICICIUpload pd = new ICICIUpload();
                            pd.UploadId = TOM.UploadId;
                            if (payment != null)
                            {
                                pd.IsReconcile = payment.amount == trCollection[i].Amount;
                            }
                            else
                            {
                                pd.IsReconcile = false;
                            }
                            pd.TransactionID = trCollection[i].TransactionID;
                            pd.TransmissionDateTime = trCollection[i].TransmissionDateTime;
                            pd.MerchantRefNo = trCollection[i].MerchantRefNo;
                            pd.Amount = trCollection[i].Amount;
                            pd.PaymentMode = trCollection[i].PaymentMode;
                            pdList.Add(pd);
                        }
                    }

                    if (pdList != null && pdList.Any())
                    {
                        context.ICICIUploadDB.AddRange(pdList);
                        if (pdList.All(x => x.IsReconcile))
                        {
                            TOM.IsReconcile = true;
                            context.Entry(TOM).State = System.Data.Entity.EntityState.Modified;
                        }
                    }

                    context.Commit();

                    if (pdList.Any(x => x.IsReconcile))
                    {
                        var reconciledTxns = pdList.Where(x => x.IsReconcile).Select(x => x.TransactionID).ToList();
                        payments.Where(x => reconciledTxns.Contains(x.GatewayTransId)).ToList().ForEach(payment =>
                        {
                            payment.UploadId = pdList.FirstOrDefault(x => x.TransactionID == payment.GatewayTransId).Id;
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
        #endregion
    }
}
