using NLog;
using System;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/HDFCUpload")]
    public class HDFCUploadController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();//Api for Upload Sheet on Warehouse to warehouse transfer
        string msgitemname, msg1;
        //string msg1;
        string strJSON = null;
        string col0, col1, col2, col3, col4, col5, col6, col7, col8, col9, col10, col11, col12, col13, col14, col15;
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        //[Authorize]
        //[HttpPost]
        //public string HDFCUPIUploadFile(string fileType)
        //{
        //    if (HttpContext.Current.Request.Files.AllKeys.Any())
        //    {

        //        var formData1 = HttpContext.Current.Request.Form["compid"];
        //        logger.Info("start Transfer Order Upload Exel File: ");

        //        // Access claims
        //        var identity = User.Identity as ClaimsIdentity;
        //        int userid = 0;


        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //        // Get the uploaded image from the Files collection
        //        System.Web.HttpPostedFile httpPostedFile = HttpContext.Current.Request.Files["file"];

        //        if (httpPostedFile != null)
        //        {
        //            var FileUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles"), httpPostedFile.FileName);
        //            // Validate the uploaded image(optional)
        //            byte[] buffer = new byte[httpPostedFile.ContentLength];

        //            using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
        //            {
        //                br.Read(buffer, 0, buffer.Length);
        //            }
        //            XSSFWorkbook hssfwb;
        //            //   XSSFWorkbook workbook1;
        //            using (MemoryStream memStream = new MemoryStream())
        //            {
        //                BinaryFormatter binForm = new BinaryFormatter();
        //                memStream.Write(buffer, 0, buffer.Length);
        //                memStream.Seek(0, SeekOrigin.Begin);
        //                hssfwb = new XSSFWorkbook(memStream);
        //            }

        //            switch (fileType)
        //            {
        //                case "3":
        //                return ReadHDGCUPIUploadedFile(hssfwb, userid);
        //            }

        //            httpPostedFile.SaveAs(FileUrl);

        //        }
        //    }

        //    return "Error";
        //}
        #region Upload UPI HDfC
        //public string ReadHDGCUPIUploadedFile(XSSFWorkbook hssfwb, int userid)
        //{
        //    string sSheetName = hssfwb.GetSheetName(0);
        //    ISheet sheet = hssfwb.GetSheet(sSheetName);

        //    IRow rowData;
        //    ICell cellData = null;
        //    try
        //    {
        //        List<HDFCUPIUpload> HDFCUPIUploader = new List<HDFCUPIUpload>();
        //        int? txnIdCellIndex = null;
        //        int? MerchantIdCellIndex = null;
        //        int? SettleDateCellIndex = null;
        //        int? txnDateCellIndex = null;
        //        int? AmountCellIndex = null;
        //        int? PayModeCellIndex = null;
        //        for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
        //        {
        //            if (iRowIdx == 0)
        //            {
        //                rowData = sheet.GetRow(iRowIdx);

        //                if (rowData != null)
        //                {


        //                    txnIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Order ID") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Order ID").ColumnIndex : (int?)null;
        //                    if (!txnIdCellIndex.HasValue)
        //                    {
        //                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Transaction does not exist..try again");
        //                        return strJSON;
        //                    }

        //                    MerchantIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Txn ref no. (RRN)") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Txn ref no. (RRN)").ColumnIndex : (int?)null;
        //                    if (!MerchantIdCellIndex.HasValue)
        //                    {
        //                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Txn ref does not exist..try again");
        //                        return strJSON;
        //                    }

        //                    txnDateCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Transaction Req Date") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Transaction Req Date").ColumnIndex : (int?)null;
        //                    if (!txnDateCellIndex.HasValue)
        //                    {
        //                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Date does not exist..try again");
        //                        return strJSON;
        //                    }


        //                    SettleDateCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Settlement Date") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Settlement Date").ColumnIndex : (int?)null;
        //                    if (!SettleDateCellIndex.HasValue)
        //                    {
        //                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Date does not exist..try again");
        //                        return strJSON;
        //                    }

        //                    PayModeCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Currency") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Currency").ColumnIndex : (int?)null;
        //                    if (!PayModeCellIndex.HasValue)
        //                    {
        //                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize(" Currency does not exist..try again");
        //                        return strJSON;
        //                    }
        //                    AmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Transaction Amount") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Transaction Amount").ColumnIndex : (int?)null;
        //                    if (!AmountCellIndex.HasValue)
        //                    {
        //                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Amount does not exist..try again");
        //                        return strJSON;
        //                    }




        //                }
        //            }
        //            else
        //            {

        //                rowData = sheet.GetRow(iRowIdx);

        //                cellData = rowData.GetCell(0);
        //                rowData = sheet.GetRow(iRowIdx);
        //                if (rowData != null)
        //                {
        //                    HDFCUPIUpload trnfrorder = new HDFCUPIUpload();
        //                    try
        //                    {

        //                        //int requestTowarehouseId;
        //                        cellData = rowData.GetCell(txnIdCellIndex.Value);
        //                        col7 = cellData == null ? "" : cellData.ToString();
        //                        trnfrorder.transaction_id = Convert.ToString(col7);
        //                        logger.Info("Order ID :" + trnfrorder.transaction_id);

        //                        cellData = rowData.GetCell(MerchantIdCellIndex.Value);
        //                        col8 = cellData == null ? "" : cellData.ToString();
        //                        trnfrorder.Txnrefno = Convert.ToString(col8);
        //                        logger.Info("Txn ref no. (RRN) :" + trnfrorder.Txnrefno);

        //                        cellData = rowData.GetCell(txnDateCellIndex.Value);
        //                        col9 = cellData == null ? "" : cellData.ToString();                             
        //                        trnfrorder.transaction_date = DateTimeHelper.ConvertToDateTime(col9).HasValue ? DateTimeHelper.ConvertToDateTime(col9).Value : DateTime.Now; ;
        //                        logger.Info("Transaction Req Date # :" + trnfrorder.transaction_date);

        //                        cellData = rowData.GetCell(SettleDateCellIndex.Value);
        //                        col10 = cellData == null ? "" : cellData.ToString();
        //                        trnfrorder.settlementDate = DateTimeHelper.ConvertToDateTime(col10).HasValue ? DateTimeHelper.ConvertToDateTime(col3).Value : DateTime.Now;  // Convert.ToDateTime(col3);
        //                        logger.Info("Settlement Date :" + trnfrorder.settlementDate);

        //                        cellData = rowData.GetCell(PayModeCellIndex.Value);
        //                        col11 = cellData == null ? "" : cellData.ToString();
        //                        trnfrorder.Currency = Convert.ToString(col11);
        //                        logger.Info("Currency :" + trnfrorder.Currency);

        //                        cellData = rowData.GetCell(AmountCellIndex.Value); 
        //                         col12 = cellData == null ? "" : cellData.ToString();
        //                        trnfrorder.Amount = Convert.ToDouble(col12);
        //                        logger.Info("Transaction Amount :" + trnfrorder.Amount);


        //                        trnfrorder.userid = userid;
        //                        HDFCUPIUploader.Add(trnfrorder);

        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        msgitemname = ex.Message;
        //                        logger.Error("Error adding customer in collection " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
        //                    }
        //                }
        //            }
        //        }

        //        List<HDFCUPIUpload> id = AddHDFCUpload(HDFCUPIUploader, userid);

        //        string a = id[0].UploadId.ToString();
        //        return a;

        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
        //    }

        //    return "Error";
        //}



        //public List<HDFCUPIUpload> AddHDFCUpload(List<HDFCUPIUpload> trCollection, int userId)
        //{
        //    logger.Info("start addbulk");
        //    List<HDFCUPIUpload> pdList = new List<HDFCUPIUpload>();
        //    using (var context = new AuthContext())
        //    {

        //        try
        //        {
        //            UploadFileReconcile TOM = new UploadFileReconcile();
        //            var peopledata = context.Peoples.Where(x => x.PeopleID == userId).FirstOrDefault();

        //            TOM = new UploadFileReconcile();
        //            TOM.FileType = "HDFC_UPI";
        //            TOM.UplaodBy = peopledata.DisplayName;
        //            TOM.UploadDate = indianTime;
        //            context.UploadFileReconcileDB.Add(TOM);
        //            context.SaveChanges();

        //            var txnIds = trCollection.Select(x => x.transaction_id).ToList();
        //            var payments = context.PaymentResponseRetailerAppDb.Where(x => txnIds.Contains(x.GatewayTransId)).ToList();

        //            //var uploadedTxnIds = 


        //            for (var i = 0; i < trCollection.ToList().Count(); i++)
        //            {
        //                double amount = trCollection[i].Amount;
        //                string transactionid = trCollection[i].transaction_id;
        //                var payment = payments.Where(x => x.GatewayTransId == transactionid).FirstOrDefault();

        //                HDFCUPIUpload pd = new HDFCUPIUpload();
        //                pd.UploadId = TOM.UploadId;
        //                if (payment != null)
        //                {
        //                    pd.IsReconcile = payment.amount == trCollection[i].Amount;
        //                }
        //                else
        //                {
        //                    pd.IsReconcile = false;
        //                }
        //                pd.transaction_id = trCollection[i].transaction_id;
        //                pd.transaction_date = trCollection[i].transaction_date;
        //                pd.Txnrefno = trCollection[i].Txnrefno;
        //                pd.Amount = trCollection[i].Amount;
        //                pd.Currency = trCollection[i].Currency;
        //                pd.settlementDate = trCollection[i].settlementDate;

        //                pdList.Add(pd);

        //                //context.EpaylaterUploadDB.Add(pd);


        //            }

        //            if (pdList != null && pdList.Any())
        //            {
        //                context.HDFCUPIUploadDB.AddRange(pdList);

        //                if (pdList.All(x => x.IsReconcile))
        //                {
        //                    TOM.IsReconcile = true;
        //                    context.Entry(TOM).State = System.Data.Entity.EntityState.Modified;
        //                }
        //            }

        //            context.SaveChanges();

        //            if (pdList.Any(x => x.IsReconcile))
        //            {
        //                var reconciledTxns = pdList.Where(x => x.IsReconcile).Select(x => x.transaction_id).ToList();
        //                payments.Where(x => reconciledTxns.Contains(x.GatewayTransId)).ToList().ForEach(payment =>
        //                {
        //                    payment.UploadId = pdList.FirstOrDefault(x => x.transaction_id == payment.GatewayTransId).Id;
        //                    payment.IsSettled = true;
        //                    payment.UpdatedDate = indianTime;
        //                    payment.SettleDate = indianTime;
        //                    context.Entry(payment).State = System.Data.Entity.EntityState.Modified;
        //                });

        //                context.SaveChanges();
        //            }

        //        }

        //        catch (Exception ex)
        //        {
        //            logger.Error(ex.ToString());
        //        }
        //    }
        //    return pdList;
        //}
    }
}
#endregion