using AngularJSAuthentication.API.Helper.Notification;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Masters.UPIPayment;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.UPIPayment;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using NLog;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using static AngularJSAuthentication.API.Controllers.NotificationController;
using AngularJSAuthentication.API.Helper.Notification;





namespace AngularJSAuthentication.API.Controllers.UPIPayment
{
    [RoutePrefix("UPI")]
    public class UPIController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public int QRExpireTime = AppConstants.QRExpireTime;
        public static List<long> TxnInProcess = new List<long>();

        [HttpPost]
        [Route("callBackRes")]
        [AllowAnonymous]
        public async Task<bool> callBackRes(HttpRequestMessage request)
        {
            var content = request.Content;
            string resContent = content.ReadAsStringAsync().Result;
            TextFileLogHelper.TraceLog("UPI meRes Json Decrypt : " + resContent);

            string UPIMerchantKey = ConfigurationManager.AppSettings["UPIMerchantKey"];
            var SplitRes = resContent.Split('&');
            string decData = UPIKitHelper.Decrypt(SplitRes[0].Split('=')[1], UPIMerchantKey);
            TextFileLogHelper.TraceLog("UPI meRes After Decrypt : " + decData);
            MongoDbHelper<UPIcallBackResMongo> mongoDbHelper = new MongoDbHelper<UPIcallBackResMongo>();
            string[] Res = decData.Split('|');
            var Response = new callBackResDc();
            Response.UPITxnID = !string.IsNullOrEmpty(Res[0]) ? Convert.ToInt64(Res[0]) : 0;
            Response.MerchantTrnxReference = Res[1];
            Response.Amount = !string.IsNullOrEmpty(Res[2]) && Convert.ToDouble(Res[2]) > 0 ? Convert.ToDouble(Res[2]) : 0;
            Response.TransactionAuthDate = Res[3];
            Response.Status = Res[4];
            Response.StatusDescription = Res[5];
            Response.ResponseCode = Res[6];
            Response.ApprovalNumber = Res[7];
            Response.PayerVirtualAddress = Res[8];
            Response.CustomerReferenceNo = Res[9];
            Response.ReferenceID = Res[10];
            Response.AdditionalField1 = Res[11];
            Response.AdditionalField2 = Res[12];
            Response.AdditionalField3 = Res[13];
            Response.AdditionalField4 = Res[14];
            Response.AdditionalField5 = Res[15];
            Response.AdditionalField6 = Res[16];
            Response.AdditionalField7 = Res[17];
            Response.AdditionalField8 = Res[18];
            Response.AdditionalField9 = Res[19];
            Response.AdditionalField10 = Res[20];
            Response.pgMerchantId = SplitRes[1].Split('=')[1];
            mongoDbHelper.Insert(new UPIcallBackResMongo
            {
                UPITxnId = Convert.ToString(Response.UPITxnID),
                TxnNo = Response.MerchantTrnxReference,
                EncryptRes = resContent,
                DecryptRes = decData,
                CreatedDate = DateTime.Now
            });
            #region to stop duplicate req
            if (Response.UPITxnID > 0 && Response.Status == "SUCCESS")
            {
                if (TxnInProcess != null && TxnInProcess.Any(x => x == Response.UPITxnID))
                {
                    TextFileLogHelper.TraceLog("TxnInProcess Already in Queue: " + Response.UPITxnID);
                    return false;
                }
                else
                {
                    TxnInProcess.Add(Response.UPITxnID);
                }
            }
            #endregion
            InsertUpdatePayment(Response);

            TxnInProcess.RemoveAll(x => x == Response.UPITxnID);
            return true;
        }

        [Route("GenerateOrderAmtQRCode")]
        [HttpPost]
        public async Task<QRPaymentResponseDc> GenerateOrderAmtQRCode(GenerateOrderAmtQRCodeDc GenerateOrderAmtQRCode)
        {
            QRPaymentResponseDc result = new QRPaymentResponseDc();
            if (GenerateOrderAmtQRCode != null && GenerateOrderAmtQRCode.orderIds.Any())
            {
                using (var context = new AuthContext())
                {

                    var oId = new SqlParameter("@OrderId", GenerateOrderAmtQRCode.orderIds.FirstOrDefault());
                    bool IsQREnabled = context.Database.SqlQuery<bool>("exec IsQREnabled @OrderId", oId).FirstOrDefault();
                    if (!IsQREnabled)
                    {
                        result.Status = false;
                        result.msg = "QR not generated, due to QR is Disabled at warehouse.";
                        return result;
                    }

                    var existsUPIs = context.UPITransactions.Where(x => GenerateOrderAmtQRCode.orderIds.Contains(x.OrderId) && x.IsActive && !x.IsSuccess).ToList();
                    if (existsUPIs != null && existsUPIs.Any())  //SUCCESS, FAILED, PENDING, EXPIRED, REJECTED & SPAM
                    {
                        var TxnNo = existsUPIs.FirstOrDefault().TxnNo;

                        if (existsUPIs.Any(x => x.UPITxnID != null && DateTime.Now > x.CreatedDate))
                        {
                            var txnstatuscheck = await TransactionStatusEnquiryAPI(existsUPIs.FirstOrDefault().TxnNo);
                            result.Status = txnstatuscheck.Status;
                            result.msg = txnstatuscheck.Message;
                            return result;

                        }
                        else if (context.UPITransactions.Count(c => c.TxnNo == TxnNo) == GenerateOrderAmtQRCode.orderIds.Count())
                        {
                            result.Status = true;
                            string QRCodeurl = string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Scheme
                                                                        , HttpContext.Current.Request.Url.DnsSafeHost
                                                                        , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                         , existsUPIs.FirstOrDefault().QrCodeUrl);
                            result.QRCodeurl = QRCodeurl.Replace("::", ":");


                            result.Amount = existsUPIs.Sum(x => x.Amount);
                            result.OrderIds = new List<int>();
                            result.OrderIds.AddRange(existsUPIs.Where(x => x.Amount > 0).Select(x => x.OrderId).Distinct().ToList());
                            return result;
                        }
                    }

                    var people = context.Peoples.FirstOrDefault(x => x.PeopleID == GenerateOrderAmtQRCode.peopleId);
                    if (people != null && people.Active && !people.Deleted)
                    {
                        var orderPayments = context.PaymentResponseRetailerAppDb.Where(x => GenerateOrderAmtQRCode.orderIds.Contains(x.OrderId) && x.IsOnline == false && x.status == "Success" && x.IsRefund == false).GroupBy(x => x.OrderId).Select(x => new { OrderId = x.Key, Amount = x.Sum(y => y.amount) }).ToList();
                        double Paymentamount = orderPayments.Any() ? orderPayments.Sum(x => x.Amount) : 0;
                        if (Paymentamount > 0)
                        {
                            string UPIUrl = ConfigurationManager.AppSettings["HDFCUPIUrl"];
                            if (!string.IsNullOrEmpty(UPIUrl))
                            {
                                if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/OrderQRCode")))
                                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/OrderQRCode"));
                                string fileName = string.Join("_", GenerateOrderAmtQRCode.orderIds).ToString() + '_' + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".jpeg";
                                string LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/OrderQRCode"), fileName);

                                string TxnId = "SKOD" + GetUnquieTxnId(DateTime.Now.ToString("ddMMyyyyHHmmss"));  //string.Join("_", orderIds).ToString();

                                UPIUrl = UPIUrl.Replace("[TrancId]", TxnId.ToString()).Replace("[Amount]", string.Format("{0:0.00}", Paymentamount));

                                //--Upi QR Expire
                                DateTime QRExpiredDt = QRExpireTime != null && QRExpireTime > 0 ? DateTime.Now.AddMinutes(QRExpireTime) : DateTime.Now.AddMinutes(30);
                                string gmtQRExpire = QRExpiredDt.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK");//("ddd, dd MMM yyy HH':'mm':'ss 'GMT'");
                                UPIUrl = UPIUrl.Replace("[QRexpire]", gmtQRExpire);
                                if (ConfigurationManager.AppSettings["Environment"] == "Production")
                                {
                                    Random generatorNo = new Random();
                                    string rno = generatorNo.Next(0, 999999).ToString("D12");
                                    UPIUrl = UPIUrl.Replace("[RandomNo]", ("." + rno.ToString()));

                                }
                                else
                                {
                                    UPIUrl = UPIUrl.Replace("[RandomNo]", "");
                                }
                                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                                QRCodeData qrCodeData = qrGenerator.CreateQrCode(UPIUrl, QRCodeGenerator.ECCLevel.Q);
                                QRCode qrCode = new QRCode(qrCodeData);
                                Bitmap qrCodeImage = qrCode.GetGraphic(20, Color.Black, Color.White, true);
                                qrCodeImage.Save(LogoUrl, System.Drawing.Imaging.ImageFormat.Jpeg);

                                foreach (var item in existsUPIs)
                                {
                                    item.IsActive = false;
                                    item.ModifiedBy = GenerateOrderAmtQRCode.peopleId;
                                    item.ModifiedDate = DateTime.Now;
                                    context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                }

                                double Amount = Paymentamount;
                                foreach (var order in orderPayments)
                                {
                                    double upiamount = ((order.Amount - Amount) > 0) ? Amount : order.Amount;
                                    if (Amount > 0 && upiamount > 0)
                                    {
                                        context.UPITransactions.Add(new UPITransaction
                                        {
                                            Amount = upiamount,
                                            CreatedBy = GenerateOrderAmtQRCode.peopleId,
                                            CreatedDate = DateTime.Now,
                                            IsActive = true,
                                            IsDeleted = false,
                                            IsScan = false,
                                            OrderId = order.OrderId,
                                            QrCodeUrl = "/OrderQRCode/" + fileName,
                                            TxnNo = TxnId,
                                            UPIUrl = UPIUrl,
                                            IsSuccess = false,
                                            Status = "QrGenerated",
                                            QRExpireDate = QRExpiredDt
                                        });
                                        Amount -= upiamount;
                                    }
                                }

                                context.Commit();
                                result.Status = true;

                                string QRCodeurl = string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Scheme
                                                                            , HttpContext.Current.Request.Url.DnsSafeHost
                                                                            , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                         , "/OrderQRCode/" + fileName);


                                result.QRCodeurl = QRCodeurl.Replace("::", ":");
                                result.Amount = Paymentamount;
                                result.OrderIds = new List<int>();
                                result.OrderIds.AddRange(orderPayments.Where(x => x.Amount > 0).Select(x => x.OrderId).Distinct().ToList());
                            }
                            else
                            {
                                result.Status = false;
                                result.msg = "QR not generated due to UPI Url not found.";
                            }
                        }
                        else
                        {
                            result.Status = false;
                            result.msg = "QR not generated due to already paid order amount.";
                        }
                    }
                    else
                    {
                        result.Status = false;
                        result.msg = "You are not authorized to generate QR Code.";
                    }
                }
            }
            else
            {
                result.Status = false;
                result.msg = "QR not generated due to OrderId not passed.";
            }
            return result;

        }


        //[Route("GenerateBackEndAmtQRCode")]
        //[HttpPost]
        //public async Task<BackEndQRGenerateDc> GenerateBackEndAmtQRCode(GenerateBackEndAmtQRCode GenerateOrderAmtQRCode)
        //{
        //    BackEndQRGenerateDc result = new BackEndQRGenerateDc();

        //    var identity = User.Identity as ClaimsIdentity;
        //    int userid = 0;
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //    if (GenerateOrderAmtQRCode != null && GenerateOrderAmtQRCode.OrderId > 0 && GenerateOrderAmtQRCode.amount > 0)
        //    {
        //        using (var context = new AuthContext())
        //        {
        //            var oId = new SqlParameter("@OrderId", GenerateOrderAmtQRCode.OrderId);
        //            bool IsQREnabled = context.Database.SqlQuery<bool>("exec IsQREnabled @OrderId", oId).FirstOrDefault();
        //            if (!IsQREnabled)
        //            {
        //                result.Status = false;
        //                result.msg = "QR not generated, due to QR is Disabled at warehouse.";
        //                return result;
        //            }
        //            var existsUPIs = context.UPITransactions.Where(x => x.OrderId == GenerateOrderAmtQRCode.OrderId && x.IsActive && x.IsDeleted == false).ToList();

        //            var people = context.Peoples.FirstOrDefault(x => x.PeopleID ==userid);
        //            if (people != null && people.Active && !people.Deleted)
        //            {
        //                //var orderPayments = context.PaymentResponseRetailerAppDb.Where(x => x.OrderId == GenerateOrderAmtQRCode.OrderId && x.IsOnline == false && x.status == "Success" && x.IsRefund == false).FirstOrDefault();
        //                if (GenerateOrderAmtQRCode.amount >0)
        //                {
        //                    string UPIUrl = ConfigurationManager.AppSettings["HDFCUPIUrl"];
        //                    if (!string.IsNullOrEmpty(UPIUrl))
        //                    {
        //                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/OrderQRCode")))
        //                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/OrderQRCode"));
        //                        string fileName = string.Join("_", GenerateOrderAmtQRCode.OrderId).ToString() + '_' + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".jpeg";
        //                        string LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/OrderQRCode"), fileName);

        //                        string TxnId = "SKBO" + GetUnquieTxnId(DateTime.Now.ToString("ddMMyyyyHHmmss"));

        //                        UPIUrl = UPIUrl.Replace("[TrancId]", TxnId.ToString()).Replace("[Amount]", string.Format("{0:0.00}", GenerateOrderAmtQRCode.amount));

        //                        DateTime QRExpiredDt = QRExpireTime != null && QRExpireTime > 0 ? DateTime.Now.AddMinutes(QRExpireTime) : DateTime.Now.AddMinutes(30);
        //                        string gmtQRExpire = QRExpiredDt.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK");
        //                        UPIUrl = UPIUrl.Replace("[QRexpire]", gmtQRExpire);
        //                        if (ConfigurationManager.AppSettings["Environment"] == "Production")
        //                        {
        //                            Random generatorNo = new Random();
        //                            string rno = generatorNo.Next(0, 999999).ToString("D12");
        //                            UPIUrl = UPIUrl.Replace("[RandomNo]", ("." + rno.ToString()));
        //                        }
        //                        else
        //                        {
        //                            UPIUrl = UPIUrl.Replace("[RandomNo]", "");
        //                        }
        //                        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        //                        QRCodeData qrCodeData = qrGenerator.CreateQrCode(UPIUrl, QRCodeGenerator.ECCLevel.Q);
        //                        QRCode qrCode = new QRCode(qrCodeData);
        //                        Bitmap qrCodeImage = qrCode.GetGraphic(20, Color.Black, Color.White, true);
        //                        qrCodeImage.Save(LogoUrl, System.Drawing.Imaging.ImageFormat.Jpeg);


        //                        context.UPITransactions.Add(new UPITransaction
        //                        {
        //                            Amount = GenerateOrderAmtQRCode.amount,
        //                            CreatedBy = userid,
        //                            CreatedDate = DateTime.Now,
        //                            IsActive = true,
        //                            IsDeleted = false,
        //                            IsScan = false,
        //                            OrderId = GenerateOrderAmtQRCode.OrderId,
        //                            QrCodeUrl = "/OrderQRCode/" + fileName,
        //                            TxnNo = TxnId,
        //                            UPIUrl = UPIUrl,
        //                            IsSuccess = false,
        //                            Status = "QrGenerated",
        //                            QRExpireDate = QRExpiredDt
        //                        });
        //                        context.Commit();
        //                        result.Status = true;
        //                        string QRCodeurl = string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Scheme
        //                                                                    , HttpContext.Current.Request.Url.DnsSafeHost
        //                                                                    , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
        //                                                                 , "/OrderQRCode/" + fileName);

        //                        result.QRCodeurl = QRCodeurl.Replace("::", ":");
        //                        result.Amount = GenerateOrderAmtQRCode.amount;
        //                        result.OrderId = GenerateOrderAmtQRCode.OrderId;
        //                        result.msg = "Qr Code Generated Successfully!!";
        //                    }
        //                    else
        //                    {
        //                        result.Status = false;
        //                        result.msg = "QR not generated due to UPI Url not found.";
        //                    }
        //                }
        //                else
        //                {  
        //                    result.Status = false;
        //                    result.msg = "QR not generated due to already paid order amount.";
        //                }
        //            }
        //            else
        //            { 
        //                result.Status = false;
        //                result.msg = "You are not authorized to generate QR Code.";
        //            }
        //        }
        //    }
        //    else
        //    {
        //        result.Status = false;
        //        result.msg = "QR not generated due to OrderId not passed.";
        //    }
        //    return result;

        //}





        [Route("GenerateSalesAppOrderAmtQRCode")]
        [HttpPost]
        public async Task<SalesAppQRGenerateDc> GenerateSalesAppOrderAmtQRCode(GenerateSalesAppOrderAmtQRCodeDc GenerateSalesAppOrderAmtQRCode)
        {
            var result = new SalesAppQRGenerateDc();
            if (GenerateSalesAppOrderAmtQRCode != null && GenerateSalesAppOrderAmtQRCode.OrderId > 0 && GenerateSalesAppOrderAmtQRCode.amount > 0)
            {
                using (var context = new AuthContext())
                {
                    var oId = new SqlParameter("@OrderId", GenerateSalesAppOrderAmtQRCode.OrderId);
                    bool IsQREnabled = context.Database.SqlQuery<bool>("exec IsQREnabled @OrderId", oId).FirstOrDefault();
                    if (!IsQREnabled)
                    {
                        result.Status = false;
                        result.msg = "QR not generated, due to QR is Disabled at warehouse.";
                        return result;
                    }
                    var paylaterorder = context.PayLaterCollectionDb.Where(x => x.OrderId == GenerateSalesAppOrderAmtQRCode.OrderId && x.IsActive == true).FirstOrDefault();
                    if (paylaterorder != null)
                    {
                        #region Delete upi transaction for this order
                        var existupidata = context.UPITransactions.Where(x => x.OrderId == paylaterorder.OrderId && x.IsActive == true && x.IsDeleted == false && x.UPITxnID == null).ToList();
                        if (existupidata != null && existupidata.Any())
                        {
                            foreach (var a in existupidata)
                            {
                                a.IsActive = false;
                                a.IsDeleted = true;
                                context.Entry(a).State = EntityState.Modified;
                            }
                            context.Commit();
                        }
                        #endregion
                        double orderamount = 0;
                        var orderid = new SqlParameter("@Orderid", paylaterorder.OrderId);
                        orderamount = context.Database.SqlQuery<double>("Sp_PayLaterRemainingAmount @Orderid", orderid).FirstOrDefault();
                        if (orderamount > 0)
                        {
                            GenerateSalesAppOrderAmtQRCode.amount = orderamount;
                        }
                        else
                        {
                            result.msg = "Remaining Amount is 0.";
                            result.Status = false;
                            return result;
                        }
                    }

                    var existsUPIs = context.UPITransactions.Where(x => x.OrderId == GenerateSalesAppOrderAmtQRCode.OrderId && x.IsActive && x.IsDeleted == false).ToList();
                    if (existsUPIs != null && existsUPIs.Any())
                    {
                        if (existsUPIs.Any(x => x.IsActive && x.UPITxnID != null && DateTime.Now > x.CreatedDate))
                        {
                            result.Status = existsUPIs.Any(x => x.IsActive && x.UPITxnID != null && x.IsSuccess) ? true : false;
                            result.msg = existsUPIs.Any(x => x.IsActive && x.UPITxnID != null && x.IsSuccess) ? "Transaction success" : "Transaction failed";
                            result.UPITxnID = existsUPIs.FirstOrDefault(x => x.IsActive && x.UPITxnID != null && x.IsSuccess).UPITxnID;
                            return result;

                        }
                        else
                        {
                            result.Status = true;
                            string QRCodeurl = string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Scheme
                                                                        , HttpContext.Current.Request.Url.DnsSafeHost
                                                                        , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                         , existsUPIs.FirstOrDefault().QrCodeUrl);
                            result.QRCodeurl = QRCodeurl.Replace("::", ":");
                            result.Amount = existsUPIs.Sum(x => x.Amount);
                            result.OrderId = GenerateSalesAppOrderAmtQRCode.OrderId;
                            return result;
                        }
                    }

                    var people = context.Peoples.FirstOrDefault(x => x.PeopleID == GenerateSalesAppOrderAmtQRCode.peopleId);
                    if (people != null && people.Active && !people.Deleted)
                    {
                        var orderPayments = context.PaymentResponseRetailerAppDb.Where(x => x.OrderId == GenerateSalesAppOrderAmtQRCode.OrderId && x.IsOnline == false && x.status == "Success" && x.IsRefund == false).FirstOrDefault();
                        if (orderPayments == null)
                        {
                            string UPIUrl = ConfigurationManager.AppSettings["HDFCUPIUrl"];
                            if (!string.IsNullOrEmpty(UPIUrl))
                            {
                                if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/OrderQRCode")))
                                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/OrderQRCode"));
                                string fileName = string.Join("_", GenerateSalesAppOrderAmtQRCode.OrderId).ToString() + '_' + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".jpeg";
                                string LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/OrderQRCode"), fileName);

                                string TxnId = "SKSA" + GetUnquieTxnId(DateTime.Now.ToString("ddMMyyyyHHmmss"));

                                UPIUrl = UPIUrl.Replace("[TrancId]", TxnId.ToString()).Replace("[Amount]", string.Format("{0:0.00}", GenerateSalesAppOrderAmtQRCode.amount));

                                DateTime QRExpiredDt = QRExpireTime != null && QRExpireTime > 0 ? DateTime.Now.AddMinutes(QRExpireTime) : DateTime.Now.AddMinutes(30);
                                string gmtQRExpire = QRExpiredDt.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK");
                                UPIUrl = UPIUrl.Replace("[QRexpire]", gmtQRExpire);
                                if (ConfigurationManager.AppSettings["Environment"] == "Production")
                                {
                                    Random generatorNo = new Random();
                                    string rno = generatorNo.Next(0, 999999).ToString("D12");
                                    UPIUrl = UPIUrl.Replace("[RandomNo]", ("." + rno.ToString()));
                                }
                                else
                                {
                                    UPIUrl = UPIUrl.Replace("[RandomNo]", "");
                                }
                                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                                QRCodeData qrCodeData = qrGenerator.CreateQrCode(UPIUrl, QRCodeGenerator.ECCLevel.Q);
                                QRCode qrCode = new QRCode(qrCodeData);
                                Bitmap qrCodeImage = qrCode.GetGraphic(20, Color.Black, Color.White, true);
                                qrCodeImage.Save(LogoUrl, System.Drawing.Imaging.ImageFormat.Jpeg);

                                foreach (var item in existsUPIs.Where(x => x.IsSuccess == false))
                                {
                                    item.IsActive = false;
                                    item.ModifiedBy = GenerateSalesAppOrderAmtQRCode.peopleId;
                                    item.ModifiedDate = DateTime.Now;
                                    context.Entry(item).State = EntityState.Modified;
                                }
                                context.UPITransactions.Add(new UPITransaction
                                {
                                    Amount = GenerateSalesAppOrderAmtQRCode.amount,
                                    CreatedBy = GenerateSalesAppOrderAmtQRCode.peopleId,
                                    CreatedDate = DateTime.Now,
                                    IsActive = true,
                                    IsDeleted = false,
                                    IsScan = false,
                                    OrderId = GenerateSalesAppOrderAmtQRCode.OrderId,
                                    QrCodeUrl = "/OrderQRCode/" + fileName,
                                    TxnNo = TxnId,
                                    UPIUrl = UPIUrl,
                                    IsSuccess = false,
                                    Status = "QrGenerated",
                                    QRExpireDate = QRExpiredDt
                                });
                                context.Commit();
                                result.Status = true;

                                string QRCodeurl = string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Scheme
                                                                            , HttpContext.Current.Request.Url.DnsSafeHost
                                                                            , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                         , "/OrderQRCode/" + fileName);

                                result.QRCodeurl = QRCodeurl.Replace("::", ":");
                                result.Amount = GenerateSalesAppOrderAmtQRCode.amount;
                                result.OrderId = GenerateSalesAppOrderAmtQRCode.OrderId;
                            }
                            else
                            {
                                result.Status = false;
                                result.msg = "QR not generated due to UPI Url not found.";
                            }
                        }
                        else
                        {
                            result.Status = false;
                            result.msg = "QR not generated due to already paid order amount.";
                        }
                    }
                    else
                    {
                        result.Status = false;
                        result.msg = "You are not authorized to generate QR Code.";
                    }
                }
            }
            else
            {
                result.Status = false;
                result.msg = "QR not generated due to OrderId not passed.";
            }
            return result;
        }


        #region Direct Udhar
        //[Route("InitiateDUPayInetentReq")]
        //[HttpPost]
        //public async Task<UPIPaymentResponseDc> InitiateDUPayInetentReq(InitiateDUPayInetentReqDc InitiateDUPayInetentReq)
        //{
        //    string UPIUrl = ConfigurationManager.AppSettings["HDFCUPIUrl"];
        //    string UPIIntentUrl = ConfigurationManager.AppSettings["UPIInitiatePayInetentReqUrl"];
        //    string pgMerchantId = ConfigurationManager.AppSettings["pgMerchantId"];
        //    string UPIMerchantKey = ConfigurationManager.AppSettings["UPIMerchantKey"];
        //    UPIPaymentResponseDc result = new UPIPaymentResponseDc();
        //    if (InitiateDUPayInetentReq != null && InitiateDUPayInetentReq.PaymentReqId != null && InitiateDUPayInetentReq.UserId > 0 && InitiateDUPayInetentReq.amount > 0 && !string.IsNullOrEmpty(UPIUrl))
        //    {
        //        using (var context = new AuthContext())
        //        {
        //            var cust = context.Customers.FirstOrDefault(x => x.CustomerId == InitiateDUPayInetentReq.UserId);
        //            if (cust != null && cust.Active && !cust.Deleted)
        //            {
        //                string TxnId = "SKUD" + GetUnquieTxnId(DateTime.Now.ToString("ddMMyyyyHHmmss"));

        //                var existsUPIs = context.UPITransactions.Where(x => x.PaymentReqId == InitiateDUPayInetentReq.PaymentReqId && x.IsActive && !x.IsSuccess).ToList();
        //                foreach (var item in existsUPIs)
        //                {
        //                    item.IsActive = false;
        //                    item.ModifiedBy = cust.CustomerId;
        //                    item.ModifiedDate = DateTime.Now;
        //                    context.Entry(item).State = EntityState.Modified;
        //                }
        //                //PG MERCHANTID | Order no ( unique) | MCC | Payment Type |Transaction Type|Transaction Description (mention your description)|Payee Virtual Address|Payee Name|Amount|||||||||NA|NA

        //                string quaryparam = pgMerchantId + "|" + TxnId + "|6012|P2M|PAY|Pay|shopkirana@hdfcbank|SHOP KIRANA E TRADING PRIVATE LIMITED|" + InitiateDUPayInetentReq.amount + "|||||||||NA|NA";

        //                TextFileLogHelper.TraceLog("UPI Payment Initiate Start quaryparam : " + quaryparam);

        //                string quaryparamEncrypt = UPIKitHelper.Encrypt(quaryparam, UPIMerchantKey);
        //                TextFileLogHelper.TraceLog("UPI Payment Initiate quaryparamEncrypt  : " + quaryparamEncrypt);
        //                PostInetentReqDc PostInetentReq = new PostInetentReqDc();
        //                PostInetentReq.pgMerchantId = pgMerchantId;
        //                PostInetentReq.requestMsg = quaryparamEncrypt;
        //                var PostJson = JsonConvert.SerializeObject(PostInetentReq);

        //                var PayInetentReqResDc = new PayInetentReqResDc();
        //                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        //                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        //                var EndPoint = UPIIntentUrl;
        //                var httpClientHandler = new HttpClientHandler();
        //                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
        //                {
        //                    return true;
        //                };
        //                using (var httpClient = new HttpClient(httpClientHandler))
        //                {

        //                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), EndPoint))
        //                    {
        //                        request.Headers.TryAddWithoutValidation("Accept", "*/*");
        //                        request.Content = new StringContent(PostJson);
        //                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        //                        var response = await httpClient.SendAsync(request);
        //                        if (HttpStatusCode.OK == response.StatusCode)
        //                        {
        //                            string responseBody = response.Content.ReadAsStringAsync().Result;
        //                            TextFileLogHelper.TraceLog("UPI Payment Initiate Start Decrypt : " + responseBody);
        //                            string decData = UPIKitHelper.Decrypt(responseBody, UPIMerchantKey);
        //                            TextFileLogHelper.TraceLog("UPI Payment Initiate end Decrypt: " + decData);
        //                            string[] Res = decData.Split('|');
        //                            PayInetentReqResDc.MerchantTrnxReference = Res[0];
        //                            PayInetentReqResDc.Status = Res[1];
        //                            PayInetentReqResDc.TransactionDescription = Res[2];

        //                        }
        //                        else if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound)
        //                        {
        //                            string jsonString = (await response.Content.ReadAsStringAsync());
        //                            result.msg = jsonString;
        //                            result.Status = false;
        //                            return result;
        //                        }
        //                        else
        //                        {
        //                            result.Status = false;
        //                            result.msg = "Payment not initiated, something went wrong";
        //                            return result;

        //                        }

        //                    }
        //                }
        //                UPIUrl = UPIUrl.Replace("[TrancId]", TxnId.ToString()).Replace("[Amount]", string.Format("{0:0.00}", InitiateDUPayInetentReq.amount));
        //                context.UPITransactions.Add(new Model.UPIPayment.UPITransaction
        //                {
        //                    Amount = InitiateDUPayInetentReq.amount,
        //                    CreatedBy = InitiateDUPayInetentReq.UserId,
        //                    CreatedDate = DateTime.Now,
        //                    IsActive = true,
        //                    IsDeleted = false,
        //                    IsScan = false,
        //                    OrderId = 0,
        //                    PaymentReqId = InitiateDUPayInetentReq.PaymentReqId,
        //                    QrCodeUrl = null,
        //                    TxnNo = TxnId,
        //                    UPIUrl = UPIUrl,
        //                    IsSuccess = false,
        //                    Status = "Initiated"//PayInetentReqResDc.Status
        //                });
        //                context.Commit();
        //                result.Status = true;
        //                result.IntentString = UPIUrl;
        //                result.msg = PayInetentReqResDc.TransactionDescription;
        //            }
        //            else
        //            {
        //                result.Status = false;
        //                result.msg = "Payment not initiated, something went wrong";
        //            }
        //        }
        //    }
        //    else
        //    {
        //        result.Status = false;
        //        result.msg = "Payment not initiated, something went wrong";
        //    }
        //    return result;
        //}
        #endregion


        [Route("CheckTransactionStatus")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ResponseMsg> CheckTransactionStatus(int OrderId)
        {
            var result = new ResponseMsg();
            if (OrderId > 0)
            {
                using (var context = new AuthContext())
                {
                    var UPITransaction = await context.UPITransactions.FirstOrDefaultAsync(x => x.OrderId == OrderId && x.IsActive);
                    if (UPITransaction != null)
                    {
                        if (UPITransaction.UPITxnID == null)
                        {
                            result.Status = false;
                            result.Message = "awaiting payment";
                        }
                        else
                        {
                            result = await TransactionStatusEnquiryAPI(UPITransaction.TxnNo);
                        }
                    }
                    else
                    {
                        result.Status = false;
                        result.Message = "Record not found";
                    }
                }
            }
            return result;
        }


        [Route("CheckTransactionStatusBackendOrder")]
        [HttpGet]
        [AllowAnonymous]

        public async Task<ResponseMsg> CheckTransactionStatusBackendOrder(int OrderId)
        {
            var result = new ResponseMsg();

            if (OrderId > 0)
            {
                using (var context = new AuthContext())
                {
                    // Retrieve all UPI transactions for the given OrderId, ordered by timestamp
                    var upiTransactions = await context.UPITransactions
                        .Where(x => x.OrderId == OrderId && x.IsActive).OrderBy(x => x.CreatedDate).ToListAsync();

                    if (upiTransactions.Any())
                    {
                        var latestTransaction = upiTransactions.LastOrDefault(); // Get the latest transaction based on timestamp

                        if (latestTransaction.UPITxnID == null)
                        {
                            result.Status = false;
                            result.Message = "Latest transaction is awaiting payment";
                        }
                        else
                        {
                            result = await TransactionStatusEnquiryBackendAPI(latestTransaction.TxnNo);
                        }
                    }
                    else
                    {
                        result.Status = false;
                        result.Message = "No transactions found for the specified OrderId";
                    }
                }
            }

            return result;
        }



        [Route("TransactionDetail")]
        [HttpGet]
        public async Task<UPITransactionDetailDc> TransactionDetail(string UPITxnID)
        {
            var result = new UPITransactionDetailDc();
            if (UPITxnID != null)
            {
                using (var context = new AuthContext())
                {
                    var upitxnid = new SqlParameter("@UPITxnID", UPITxnID);
                    result = context.Database.SqlQuery<UPITransactionDetailDc>("exec UPITransactionDetail @UPITxnID", upitxnid).FirstOrDefault();
                }
            }
            return result;
        }

        private string GetUnquieTxnId(string source)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                string hash = HashHelper.GetMd5Hash(md5Hash, source);
                return UPIHashString(hash);
            }
        }
        private static string UPIHashString(string text)
        {
            const string chars = "0234589ABCDEFGHOPQRTUWXYZ";
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            char[] hash2 = new char[16];
            // Note that here we are wasting bits of hash! 
            // But it isn't really important, because hash.Length == 32
            for (int i = 0; i < hash2.Length; i++)
            {
                hash2[i] = chars[hash[i] % chars.Length];
            }
            return new string(hash2);
        }
        public bool SendNotificationUPIPaymentSales(string FcmId, string FCMKey, string status, string Description, string notify_type, string UPITxnID)
        {
            try
            {
                //string Key = FCMKey;
                //FCMRequest objNotification = new FCMRequest();
                //objNotification.to = FcmId;
                //objNotification.MessageId = "";
                //objNotification.data = new FCMData
                //{
                //    title = status, //Success ,Failed
                //    body = Description, //Discription
                //    icon = "",
                //    typeId = 0,
                //    notificationCategory = "",
                //    notificationType = "",
                //    notificationId = 0,
                //    notify_type = notify_type,
                //    OrderId = 0,
                //    UPITxnID = UPITxnID,
                //    url = ""
                //};
                var data = new FCMData
                {
                    title = status, //Success ,Failed
                    body = Description, //Discription
                    icon = "",
                    typeId = 0,
                    notificationCategory = "",
                    notificationType = "",
                    notificationId = 0,
                    notify_type = notify_type,
                    OrderId = 0,
                    UPITxnID = UPITxnID,
                    url = ""
                };
                var firebaseService = new FirebaseNotificationServiceHelper(FCMKey);
                //var fcmid = "fZGIeP5dTKGd-2JBZttcDj:APA91bGINtqXqKuCcEHd4qXZMN-VtX5KC2g98KkytmGdpc28_-duDu8Ry1P6Kk_Xb9RgRG0iDWrp8DkwE1EPCOPG0OLz2uRjo-HBg-Ysg-mDaMErlYLjJGZE7ScXKIkjmWZ0xNO6UyxN";
                var result = firebaseService.SendNotificationForApprovalAsync(FcmId, data);
                if (result != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                //tRequest.Method = "post";
                //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
                //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                //tRequest.ContentLength = byteArray.Length;
                //tRequest.ContentType = "application/json";
                //using (Stream dataStream = tRequest.GetRequestStream())
                //{
                //    dataStream.Write(byteArray, 0, byteArray.Length);
                //    using (WebResponse tResponse = tRequest.GetResponse())
                //    {
                //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                //        {
                //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                //            {
                //                String responseFromFirebaseServer = tReader.ReadToEnd();
                //                FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                //                if (response.success == 1 && response.results != null && response.results.Any() && !string.IsNullOrEmpty(response.results.FirstOrDefault().message_id))
                //                {
                //                    return true;
                //                }
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ds)
            {
                logger.Error("Error during  SendNotificationUPIPayment" + notify_type + " : " + ds.ToString());
            }
            return false;
        }

        private async Task<ResponseMsg> InsertUpdatePayment(callBackResDc Response)
        {
            var result = new ResponseMsg();
            string jsonInRes = JsonConvert.SerializeObject(Response);
            TextFileLogHelper.TraceLog("UPI callBackRes in jsonInRes : " + jsonInRes);

            using (AuthContext context = new AuthContext())
            {
                var existsUPIs = context.UPITransactions.Where(x => x.TxnNo == Response.MerchantTrnxReference && x.IsActive && !x.IsSuccess).ToList();
                foreach (var item in existsUPIs)
                {
                    item.ResponseMsg = jsonInRes;
                    item.IsSuccess = Response.Status == "SUCCESS";
                    item.Status = Response.Status;
                    item.ModifiedBy = 1;
                    item.IsActive = item.IsSuccess;
                    item.ModifiedDate = DateTime.Now;
                    item.UPITxnID = Convert.ToString(Response.UPITxnID);
                    context.Entry(item).State = EntityState.Modified;
                }
                if (existsUPIs != null && existsUPIs.Any() && Response.Status == "SUCCESS")
                {
                    TextFileLogHelper.TraceLog("UPI callBackRes Update #TxnNo: " + existsUPIs.FirstOrDefault().TxnNo);
                    var oid = existsUPIs.FirstOrDefault().OrderId;
                    var orderIds = existsUPIs.Select(x => x.OrderId).Distinct().ToList();
                    var orderPayments = context.PaymentResponseRetailerAppDb.Where(x => orderIds.Contains(x.OrderId) && x.PaymentFrom == "Cash" && x.IsOnline == false && x.status == "Success" && x.IsRefund == false).ToList();
                    var Ordermaster = context.DbOrderMaster.Where(x => x.OrderId == oid).FirstOrDefault();

                    foreach (var item in existsUPIs)
                    {
                        var paylaterorderdata = context.PayLaterCollectionDb.Where(x => x.OrderId == item.OrderId).FirstOrDefault();
                        if (paylaterorderdata == null)
                        {
                            foreach (var pitem in orderPayments.Where(x => x.OrderId == item.OrderId))
                            {
                                pitem.amount = (item.Amount == pitem.amount) ? pitem.amount : (pitem.amount - item.Amount);
                                pitem.status = (item.Amount == pitem.amount) ? "Falied" : pitem.status;
                                context.Entry(pitem).State = EntityState.Modified;
                            }
                            string Gtxn = Convert.ToString(Response.UPITxnID);
                            if (context.PaymentResponseRetailerAppDb.FirstOrDefault(z => z.OrderId == item.OrderId && z.GatewayTransId == Gtxn && z.PaymentFrom == "UPI" && z.IsOnline == true && z.status == "Success") == null)
                            {
                                var PaymentResponseRetailerAppDb = new GenricEcommers.Models.PaymentResponseRetailerApp
                                {
                                    GatewayOrderId = Response.MerchantTrnxReference,
                                    GatewayTransId = Convert.ToString(Response.UPITxnID),
                                    amount = item.Amount,
                                    CreatedDate = DateTime.Now,
                                    currencyCode = "INR",
                                    OrderId = item.OrderId,
                                    PaymentFrom = "UPI",
                                    status = "Success",
                                    UpdatedDate = DateTime.Now,
                                    IsRefund = false,
                                    IsOnline = true,
                                    GatewayResponse = jsonInRes,
                                    GatewayRequest = existsUPIs.FirstOrDefault().UPIUrl,
                                    PaymentThrough = "UPI"
                                };
                                context.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                                if (context.OnlinePaymentDtlsForLedgerDB.FirstOrDefault(z => z.OrderId == item.OrderId && z.TransactionId == PaymentResponseRetailerAppDb.GatewayTransId) == null)
                                {
                                    OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
                                    Opdl.OrderId = item.OrderId;
                                    Opdl.IsPaymentSuccess = true;
                                    Opdl.IsLedgerAffected = "Yes";
                                    Opdl.PaymentDate = DateTime.Now;
                                    Opdl.TransactionId = PaymentResponseRetailerAppDb.GatewayTransId;
                                    Opdl.IsActive = true;
                                    Opdl.CustomerId = Ordermaster.CustomerId;
                                    context.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
                                }
                                if (existsUPIs.Count() == 1 && Ordermaster.Status == "Payment Pending")
                                {
                                    Ordermaster.Status = "Pending";
                                    Ordermaster.paymentThrough = "UPI";
                                    Ordermaster.paymentMode = "Online";
                                    Ordermaster.UpdatedDate = DateTime.Now;
                                    context.Entry(Ordermaster).State = EntityState.Modified;
                                }
                            }
                        }
                        else
                        {
                            #region paylaterhistory
                            var paylaterhistory = new AngularJSAuthentication.Model.CashManagement.PayLaterCollectionHistory
                            {
                                Amount = item.Amount,
                                PaymentMode = "UPI",
                                RefNo = Convert.ToString(Response.UPITxnID),
                                PayLaterCollectionId = paylaterorderdata.Id,
                                CreatedDate = DateTime.Now,
                                IsActive = true,
                                IsDeleted = false,
                                CreatedBy = existsUPIs.FirstOrDefault().CreatedBy,
                                CurrencyHubStockId = 0,
                                Comment = "Sales App Pay",
                                PaymentStatus = 1
                            };
                            context.PayLaterCollectionHistoryDb.Add(paylaterhistory);
                            context.Commit();
                            #endregion

                            #region paylaterstatusupdate
                            paylaterorderdata.Status = Convert.ToInt32(AngularJSAuthentication.API.DataContract.PayCollectionEnum.Paid);
                            paylaterorderdata.ModifiedBy = existsUPIs.FirstOrDefault().CreatedBy;
                            paylaterorderdata.ModifiedDate = DateTime.Now;
                            context.Entry(paylaterorderdata).State = EntityState.Modified;
                            #endregion

                            #region ladger entry
                            long historyid = paylaterhistory.Id;
                            string gatewayid = "SkPayLater" + historyid;
                            if (context.OnlinePaymentDtlsForLedgerDB.FirstOrDefault(z => z.OrderId == item.OrderId && z.TransactionId == gatewayid) == null)
                            {
                                OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
                                Opdl.OrderId = item.OrderId;
                                Opdl.IsPaymentSuccess = true;
                                Opdl.IsLedgerAffected = "Yes";
                                Opdl.PaymentDate = DateTime.Now;
                                Opdl.TransactionId = gatewayid;
                                Opdl.IsActive = true;
                                Opdl.CustomerId = Ordermaster.CustomerId;
                                context.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
                            }
                            #endregion

                            #region ordermaster settle 
                            CashCollectionNewController ctrl = new CashCollectionNewController();
                            bool res = ctrl.OrderSettle(context, item.OrderId);
                            #endregion
                        }

                        //foreach (var pitem in orderPayments.Where(x => x.OrderId == item.OrderId))
                        //{
                        //    pitem.amount = (item.Amount == pitem.amount) ? pitem.amount : (pitem.amount - item.Amount);
                        //    pitem.status = (item.Amount == pitem.amount) ? "Falied" : pitem.status;
                        //    context.Entry(pitem).State = EntityState.Modified;
                        //}
                        //string Gtxn = Convert.ToString(Response.UPITxnID);
                        //if (context.PaymentResponseRetailerAppDb.FirstOrDefault(z => z.OrderId == item.OrderId && z.GatewayTransId == Gtxn && z.PaymentFrom == "UPI" && z.IsOnline == true && z.status == "Success") == null)
                        //{
                        //    var PaymentResponseRetailerAppDb = new GenricEcommers.Models.PaymentResponseRetailerApp
                        //    {
                        //        GatewayOrderId = Response.MerchantTrnxReference,
                        //        GatewayTransId = Convert.ToString(Response.UPITxnID),
                        //        amount = item.Amount,
                        //        CreatedDate = DateTime.Now,
                        //        currencyCode = "INR",
                        //        OrderId = item.OrderId,
                        //        PaymentFrom = "UPI",
                        //        status = "Success",
                        //        UpdatedDate = DateTime.Now,
                        //        IsRefund = false,
                        //        IsOnline = true,
                        //        GatewayResponse = jsonInRes,
                        //        GatewayRequest = existsUPIs.FirstOrDefault().UPIUrl,
                        //        PaymentThrough = "UPI"
                        //    };
                        //    context.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                        //    if (context.OnlinePaymentDtlsForLedgerDB.FirstOrDefault(z => z.OrderId == item.OrderId && z.TransactionId == PaymentResponseRetailerAppDb.GatewayTransId) == null)
                        //    {
                        //        OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
                        //        Opdl.OrderId = item.OrderId;
                        //        Opdl.IsPaymentSuccess = true;
                        //        Opdl.IsLedgerAffected = "Yes";
                        //        Opdl.PaymentDate = DateTime.Now;
                        //        Opdl.TransactionId = PaymentResponseRetailerAppDb.GatewayTransId;
                        //        Opdl.IsActive = true;
                        //        Opdl.CustomerId = Ordermaster.CustomerId;
                        //        context.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
                        //    }
                        //    if (existsUPIs.Count() == 1 && Ordermaster.Status == "Payment Pending")
                        //    {
                        //        Ordermaster.Status = "Pending";
                        //        Ordermaster.paymentThrough = "UPI";
                        //        Ordermaster.paymentMode = "Online";
                        //        Ordermaster.UpdatedDate = DateTime.Now;
                        //        context.Entry(Ordermaster).State = EntityState.Modified;
                        //    }
                        //}
                    }
                    context.Commit();
                    TextFileLogHelper.TraceLog("UPI callBackRes Commit end #TxnNo: " + existsUPIs.FirstOrDefault().TxnNo);
                }
                if (existsUPIs != null && existsUPIs.Any() && Response.Status != "SUCCESS")
                { context.Commit(); }
                result.Status = true;
                result.Message = Response.StatusDescription;
                if (existsUPIs != null && existsUPIs.Any())
                {
                    var userid = existsUPIs.FirstOrDefault().CreatedBy;
                    var people = context.Peoples.FirstOrDefault(x => x.PeopleID == userid);
                    //delivery app notification
                    string status = Response.Status;
                    string Description = Response.StatusDescription;
                    var orderId = new SqlParameter("@OrderId", existsUPIs.FirstOrDefault().OrderId);
                    bool IsTripOrder = context.Database.SqlQuery<bool>("exec IsTripOrder @OrderId", orderId).FirstOrDefault();
                    //OldDeliveryFcmApiKey
                    string FCMKey = string.Empty;
                    bool IsSent = false;
                    if (IsTripOrder == false)
                    {
                        FCMKey = ConfigurationManager.AppSettings["SalesFcmApiKey"];
                        IsSent = SendNotificationUPIPaymentSales(people.FcmId, FCMKey, status, Description, "QRCodePayment", Convert.ToString(Response.UPITxnID));
                    }
                    else
                    {
                        FCMKey = ConfigurationManager.AppSettings["DeliveryFcmApiKey"];
                        IsSent = SendNotificationUPIPaymentSales(people.FcmId, FCMKey, status, Description, "QRCodePayment", Convert.ToString(Response.UPITxnID));
                    }
                    if (!IsSent)
                    {
                        logger.Error("Error during SendNotificationForQRPayment : "
                           + people.FcmId + " " + FCMKey + " " + status + " " + Description);
                    }
                }
                return result;
            }
        }


        private async Task<ResponseMsg> TransactionStatusEnquiryAPI(string TxnNo)
        {
            var result = new ResponseMsg();
            string UPIUrl = ConfigurationManager.AppSettings["UPITransactionStatusUrl"];
            string pgMerchantId = ConfigurationManager.AppSettings["pgMerchantId"];
            string UPIMerchantKey = ConfigurationManager.AppSettings["UPIMerchantKey"];
            if (UPIUrl != null && pgMerchantId != null && UPIMerchantKey != null)
            {
                string quaryparam = pgMerchantId + "|" + TxnNo + "|||||||||||NA|NA";
                string quaryparamEncrypt = UPIKitHelper.Encrypt(quaryparam, UPIMerchantKey);
                TextFileLogHelper.TraceLog("UPI Payment  TransactionStatusEnquiryAPI  : " + quaryparamEncrypt);
                PostInetentReqDc PostInetentReq = new PostInetentReqDc();
                PostInetentReq.pgMerchantId = pgMerchantId;
                PostInetentReq.requestMsg = quaryparamEncrypt;
                var PostJson = JsonConvert.SerializeObject(PostInetentReq);
                TextFileLogHelper.TraceLog("UPI Payment  TransactionStatusEnquiryAPI  Post Json : " + PostJson);
                MongoDbHelper<UPIcallBackResMongo> mongoDbHelper = new MongoDbHelper<UPIcallBackResMongo>();

                var PayInetentReqResDc = new PayInetentReqResDc();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                var EndPoint = UPIUrl;
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    return true;
                };
                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), EndPoint))
                    {
                        request.Headers.TryAddWithoutValidation("Accept", "*/*");
                        request.Content = new StringContent(PostJson);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        var response = await httpClient.SendAsync(request);
                        if (HttpStatusCode.OK == response.StatusCode)
                        {
                            string responseBody = response.Content.ReadAsStringAsync().Result;
                            TextFileLogHelper.TraceLog("UPI Payment response TransactionStatusEnquiryAPI Start Decrypt : " + responseBody);
                            var SplitRes = responseBody.Split('&');
                            string decData = UPIKitHelper.Decrypt(responseBody, UPIMerchantKey);
                            TextFileLogHelper.TraceLog("UPI Payment response TransactionStatusEnquiryAPI end Decrypt: " + decData);
                            string[] Res = decData.Split('|');

                            var Response = new callBackResDc();
                            Response.UPITxnID = !string.IsNullOrEmpty(Res[0]) ? Convert.ToInt64(Res[0]) : 0;
                            Response.MerchantTrnxReference = Res[1];
                            Response.Amount = !string.IsNullOrEmpty(Res[2]) && Convert.ToDouble(Res[2]) > 0 ? Convert.ToDouble(Res[2]) : 0;
                            Response.TransactionAuthDate = Res[3];
                            Response.Status = Res[4];
                            Response.StatusDescription = Res[5];
                            Response.ResponseCode = Res[6];
                            Response.ApprovalNumber = Res[7];
                            Response.PayerVirtualAddress = Res[8];
                            Response.CustomerReferenceNo = Res[9];
                            Response.ReferenceID = Res[10];
                            Response.AdditionalField1 = Res[11];
                            Response.AdditionalField2 = Res[12];
                            Response.AdditionalField3 = Res[13];
                            Response.AdditionalField4 = Res[14];
                            Response.AdditionalField5 = Res[15];
                            Response.AdditionalField6 = Res[16];
                            Response.AdditionalField7 = Res[17];
                            Response.AdditionalField8 = Res[18];
                            Response.AdditionalField9 = Res[19];
                            Response.AdditionalField10 = Res[20];
                            Response.pgMerchantId = pgMerchantId;
                            if (Response.UPITxnID > 0)
                            {
                                result = await InsertUpdatePayment(Response);
                            }
                            else if (Response.UPITxnID == 0)
                            {
                                result.Status = false;
                                result.Message = "transaction is pending";
                            }
                            else
                            {
                                result.Status = false;
                                result.Message = Response.StatusDescription;
                            }

                            mongoDbHelper.Insert(new UPIcallBackResMongo
                            {
                                UPITxnId = Convert.ToString(Response.UPITxnID),
                                TxnNo = Response.MerchantTrnxReference,
                                EncryptRes = responseBody,
                                DecryptRes = decData,
                                CreatedDate = DateTime.Now
                            });

                        }
                        else if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound)
                        {
                            string jsonString = (await response.Content.ReadAsStringAsync());
                            result.Message = jsonString;
                            result.Status = false;
                            return result;
                        }
                        else
                        {
                            result.Status = false;
                            result.Message = "something went wrong ,please try after sometime";
                            return result;

                        }
                    }
                }
            }
            else
            {
                result.Status = false;
                result.Message = "something went wrong ,please try after sometime";
                return result;
            }
            return result;
        }
        private async Task<ResponseMsg> TransactionStatusEnquiryBackendAPI(string TxnNo)
        {
            var result = new ResponseMsg();
            string UPIUrl = ConfigurationManager.AppSettings["UPITransactionStatusUrl"];
            string pgMerchantId = ConfigurationManager.AppSettings["pgMerchantId"];
            string UPIMerchantKey = ConfigurationManager.AppSettings["UPIMerchantKey"];
            if (UPIUrl != null && pgMerchantId != null && UPIMerchantKey != null)
            {
                string quaryparam = pgMerchantId + "|" + TxnNo + "|||||||||||NA|NA";
                string quaryparamEncrypt = UPIKitHelper.Encrypt(quaryparam, UPIMerchantKey);
                TextFileLogHelper.TraceLog("UPI Payment  TransactionStatusEnquiryAPI  : " + quaryparamEncrypt);
                PostInetentReqDc PostInetentReq = new PostInetentReqDc();
                PostInetentReq.pgMerchantId = pgMerchantId;
                PostInetentReq.requestMsg = quaryparamEncrypt;
                var PostJson = JsonConvert.SerializeObject(PostInetentReq);
                TextFileLogHelper.TraceLog("UPI Payment  TransactionStatusEnquiryAPI  Post Json : " + PostJson);
                MongoDbHelper<UPIcallBackResMongo> mongoDbHelper = new MongoDbHelper<UPIcallBackResMongo>();

                var PayInetentReqResDc = new PayInetentReqResDc();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                var EndPoint = UPIUrl;
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    return true;
                };
                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), EndPoint))
                    {
                        request.Headers.TryAddWithoutValidation("Accept", "*/*");
                        request.Content = new StringContent(PostJson);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        var response = await httpClient.SendAsync(request);
                        if (HttpStatusCode.OK == response.StatusCode)
                        {
                            string responseBody = response.Content.ReadAsStringAsync().Result;
                            TextFileLogHelper.TraceLog("UPI Payment response TransactionStatusEnquiryAPI Start Decrypt : " + responseBody);
                            var SplitRes = responseBody.Split('&');
                            string decData = UPIKitHelper.Decrypt(responseBody, UPIMerchantKey);
                            TextFileLogHelper.TraceLog("UPI Payment response TransactionStatusEnquiryAPI end Decrypt: " + decData);
                            string[] Res = decData.Split('|');

                            var Response = new callBackResDc();
                            Response.UPITxnID = !string.IsNullOrEmpty(Res[0]) ? Convert.ToInt64(Res[0]) : 0;
                            Response.MerchantTrnxReference = Res[1];
                            Response.Amount = !string.IsNullOrEmpty(Res[2]) && Convert.ToDouble(Res[2]) > 0 ? Convert.ToDouble(Res[2]) : 0;
                            Response.TransactionAuthDate = Res[3];
                            Response.Status = Res[4];
                            Response.StatusDescription = Res[5];
                            Response.ResponseCode = Res[6];
                            Response.ApprovalNumber = Res[7];
                            Response.PayerVirtualAddress = Res[8];
                            Response.CustomerReferenceNo = Res[9];
                            Response.ReferenceID = Res[10];
                            Response.AdditionalField1 = Res[11];
                            Response.AdditionalField2 = Res[12];
                            Response.AdditionalField3 = Res[13];
                            Response.AdditionalField4 = Res[14];
                            Response.AdditionalField5 = Res[15];
                            Response.AdditionalField6 = Res[16];
                            Response.AdditionalField7 = Res[17];
                            Response.AdditionalField8 = Res[18];
                            Response.AdditionalField9 = Res[19];
                            Response.AdditionalField10 = Res[20];
                            Response.pgMerchantId = pgMerchantId;
                            if (Response.UPITxnID > 0)
                            {
                                result = await InsertUpdatePayment(Response);
                            }
                            else if (Response.UPITxnID == 0)
                            {
                                result.Status = false;
                                result.Message = "transaction is pending";
                            }
                            else
                            {
                                result.Status = false;
                                result.Message = Response.StatusDescription;
                            }

                            mongoDbHelper.Insert(new UPIcallBackResMongo
                            {
                                UPITxnId = Convert.ToString(Response.UPITxnID),
                                TxnNo = Response.MerchantTrnxReference,
                                EncryptRes = responseBody,
                                DecryptRes = decData,
                                CreatedDate = DateTime.Now
                            });

                        }
                        else if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound)
                        {
                            string jsonString = (await response.Content.ReadAsStringAsync());
                            result.Message = jsonString;
                            result.Status = false;
                            return result;
                        }
                        else
                        {
                            result.Status = false;
                            result.Message = "something went wrong ,please try after sometime";
                            return result;

                        }
                    }
                }
            }
            else
            {
                result.Status = false;
                result.Message = "something went wrong ,please try after sometime";
                return result;
            }
            return result;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("RemoveUPITxnIdInStatic")]
        public bool RemoveUPITxnIdInStatic(long UPITxnId)
        {
            if (UPITxnId > 0 && TxnInProcess.Any(x => x == UPITxnId))
            {
                TxnInProcess.RemoveAll(x => x == UPITxnId);
                return true;
            }
            return false;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("CheckUPIResponse")]
        public async Task<UpiStatusResponse> CheckUPIResponse(int OrderId, double amount)
        {
            using (AuthContext db = new AuthContext())
            {
                var consumerorder = db.ConsumerRazorPayOrders.Where(x => x.OrderId == OrderId && x.IsSuccess == false).ToList().OrderByDescending(x=>x.Id).FirstOrDefault();
                if (consumerorder != null)
                {
                    var companydetails = db.ConsumerCompanyDetailDB.FirstOrDefault();
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)192 |
                            (SecurityProtocolType)768 | (SecurityProtocolType)3072;
                    Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient(companydetails.RazorpayApiKeyId, companydetails.RazorpayApiKeySecret);
                    string qrcodeid = consumerorder.RazorPayQRId;
                    Dictionary<string, object> paramRequest = new Dictionary<string, object>();
                    paramRequest.Add("count", "1");
                    TextFileLogHelper.TraceLog("consumer qr code request response manual request "+ qrcodeid);
                    List<Razorpay.Api.Payment> qRCodepayment = client.QrCode.FetchAllPayments(qrcodeid, paramRequest);
                    if(qRCodepayment != null && qRCodepayment.Any())
                    {
                        string paymentResponse = JsonConvert.SerializeObject(qRCodepayment);
                        TextFileLogHelper.TraceLog("consumer qr code request response response" + paymentResponse);
                        string res = qRCodepayment[0].Attributes;
                        RazorPayResponse razorPayResponce = JsonConvert.DeserializeObject<RazorPayResponse>(res);
                        if (razorPayResponce.status == "captured")
                        {
                            consumerorder.PaymentResponse = paymentResponse;
                            consumerorder.RazorPayId = razorPayResponce.id;
                            consumerorder.payments_amount_received = razorPayResponce.amount;
                            consumerorder.IsSuccess = true;
                            consumerorder.Status = "SUCCESS";
                            db.Entry(consumerorder).State = EntityState.Modified;
                            db.Commit();
                        }
                        
                    }
                }

                var Order = new SqlParameter("@OrderId", OrderId);
                var amt = new SqlParameter("@Amount", amount);
                var data = db.Database.SqlQuery<UpiStatusResponse>("EXEC sp_CheckStatusUPITxn @OrderId,@Amount", Order, amt).FirstOrDefault();

                #region Send qr to Warehouse Mobile
                try
                {
                    if (data.IsSuccess)
                    {
                        var whid = db.DbOrderMaster.AsNoTracking().Where(x => x.OrderId == OrderId).Select(s => s.WarehouseId).FirstOrDefault();
                        var whmobile = db.WarehouseQrDevices.FirstOrDefault(s => s.WarehouseId == whid && s.IsActive);
                        if (whmobile != null)
                        {
                            string jsonPath = ConfigurationManager.AppSettings["WhQrFcmJsonPath"];
                            var firebaseService = new FirebaseNotificationServiceHelper(jsonPath);
                            var notification = new AngularJSAuthentication.Model.Notification
                            {
                                title = "qrcode_success",
                                Message = $"Rs. {Convert.ToString(amount)}",
                                priority = "high",
                                CompanyId = OrderId
                            };

                            var notResult = AsyncContext.Run(() => firebaseService.SendNotificationAsync(whmobile.FcmId, notification, "qrcode_success"));


                        }
                    }

                }
                catch (Exception ex)
                {
                    TextFileLogHelper.LogError(ex.ToString());
                }

                #endregion

                return data;
            }

        }

        [Route("GenerateBackEndAmtQRCode")]
        [HttpPost]
        public async Task<BackEndQRGenerateDc> GenerateBackEndAmtQRCode(GenerateBackEndAmtQRCode GenerateOrderAmtQRCode)
        {
            BackEndQRGenerateDc result = new BackEndQRGenerateDc();

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var context = new AuthContext())
            {
                var ordermaster = context.DbOrderMaster.Where(x => x.OrderId == GenerateOrderAmtQRCode.OrderId).FirstOrDefault();
                var warehousedata = context.Warehouses.Where(x => x.WarehouseId == ordermaster.WarehouseId).FirstOrDefault();
                if (warehousedata.StoreType == 0)
                {
                    if (GenerateOrderAmtQRCode != null && GenerateOrderAmtQRCode.OrderId > 0 && GenerateOrderAmtQRCode.amount > 0)
                    {

                        var oId = new SqlParameter("@OrderId", GenerateOrderAmtQRCode.OrderId);
                        bool IsQREnabled = context.Database.SqlQuery<bool>("exec IsQREnabled @OrderId", oId).FirstOrDefault();
                        if (!IsQREnabled)
                        {
                            result.Status = false;
                            result.msg = "QR not generated, due to QR is Disabled at warehouse.";
                            return result;
                        }
                        var existsUPIs = context.UPITransactions.Where(x => x.OrderId == GenerateOrderAmtQRCode.OrderId && x.IsActive && x.IsDeleted == false).ToList();

                        var people = context.Peoples.FirstOrDefault(x => x.PeopleID == userid);
                        if (people != null && people.Active && !people.Deleted)
                        {
                            //var orderPayments = context.PaymentResponseRetailerAppDb.Where(x => x.OrderId == GenerateOrderAmtQRCode.OrderId && x.IsOnline == false && x.status == "Success" && x.IsRefund == false).FirstOrDefault();
                            if (GenerateOrderAmtQRCode.amount > 0)
                            {
                                string UPIUrl = ConfigurationManager.AppSettings["HDFCUPIUrl"];
                                if (!string.IsNullOrEmpty(UPIUrl))
                                {
                                    if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/OrderQRCode")))
                                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/OrderQRCode"));
                                    string fileName = string.Join("_", GenerateOrderAmtQRCode.OrderId).ToString() + '_' + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".jpeg";
                                    string LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/OrderQRCode"), fileName);

                                    string TxnId = "SKBO" + GetUnquieTxnId(DateTime.Now.ToString("ddMMyyyyHHmmss"));

                                    UPIUrl = UPIUrl.Replace("[TrancId]", TxnId.ToString()).Replace("[Amount]", string.Format("{0:0.00}", GenerateOrderAmtQRCode.amount));

                                    DateTime QRExpiredDt = QRExpireTime != null && QRExpireTime > 0 ? DateTime.Now.AddMinutes(QRExpireTime) : DateTime.Now.AddMinutes(30);
                                    string gmtQRExpire = QRExpiredDt.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK");
                                    UPIUrl = UPIUrl.Replace("[QRexpire]", gmtQRExpire);
                                    if (ConfigurationManager.AppSettings["Environment"] == "Production")
                                    {
                                        Random generatorNo = new Random();
                                        string rno = generatorNo.Next(0, 999999).ToString("D12");
                                        UPIUrl = UPIUrl.Replace("[RandomNo]", ("." + rno.ToString()));
                                    }
                                    else
                                    {
                                        UPIUrl = UPIUrl.Replace("[RandomNo]", "");
                                    }
                                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(UPIUrl, QRCodeGenerator.ECCLevel.Q);
                                    QRCode qrCode = new QRCode(qrCodeData);
                                    Bitmap qrCodeImage = qrCode.GetGraphic(20, Color.Black, Color.White, true);
                                    qrCodeImage.Save(LogoUrl, System.Drawing.Imaging.ImageFormat.Jpeg);


                                    context.UPITransactions.Add(new UPITransaction
                                    {
                                        Amount = GenerateOrderAmtQRCode.amount,
                                        CreatedBy = userid,
                                        CreatedDate = DateTime.Now,
                                        IsActive = true,
                                        IsDeleted = false,
                                        IsScan = false,
                                        OrderId = GenerateOrderAmtQRCode.OrderId,
                                        QrCodeUrl = "/OrderQRCode/" + fileName,
                                        TxnNo = TxnId,
                                        UPIUrl = UPIUrl,
                                        IsSuccess = false,
                                        Status = "QrGenerated",
                                        QRExpireDate = QRExpiredDt
                                    });
                                    context.Commit();
                                    result.Status = true;
                                    string QRCodeurl = string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Scheme
                                                                                , HttpContext.Current.Request.Url.DnsSafeHost
                                                                                , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                             , "/OrderQRCode/" + fileName);

                                    result.QRCodeurl = QRCodeurl.Replace("::", ":");
                                    result.Amount = GenerateOrderAmtQRCode.amount;
                                    result.OrderId = GenerateOrderAmtQRCode.OrderId;
                                    result.msg = "Qr Code Generated Successfully!!";
                                }
                                else
                                {
                                    result.Status = false;
                                    result.msg = "QR not generated due to UPI Url not found.";
                                }
                            }
                            else
                            {
                                result.Status = false;
                                result.msg = "QR not generated due to already paid order amount.";
                            }
                        }
                        else
                        {
                            result.Status = false;
                            result.msg = "You are not authorized to generate QR Code.";
                        }

                    }
                    else
                    {
                        result.Status = false;
                        result.msg = "QR not generated due to OrderId not passed.";
                    }
                }
                else if (warehousedata.StoreType == 1)
                {
                    if (GenerateOrderAmtQRCode != null && GenerateOrderAmtQRCode.OrderId > 0 && GenerateOrderAmtQRCode.amount > 0)
                    {
                        string RazorPayCustomerId = null;
                        string RazorPayId = null;
                        var companydetails = context.ConsumerCompanyDetailDB.FirstOrDefault();
                        DateTime currentTime = DateTime.UtcNow;
                        DateTime closeTime = currentTime.AddMinutes(companydetails.RazorPayQREXpiretime);
                        long closeByTimestamp = ((DateTimeOffset)closeTime).ToUnixTimeSeconds();
                        long maxUnixTimestamp = 2147483647;
                        if (closeByTimestamp > maxUnixTimestamp)
                        {
                            closeByTimestamp = maxUnixTimestamp;
                        }
                        DateTime closedqrtime = DateTimeOffset.FromUnixTimeSeconds(closeByTimestamp).DateTime;
                        ServicePointManager.SecurityProtocol = (SecurityProtocolType)192 |
                                (SecurityProtocolType)768 | (SecurityProtocolType)3072;
                        Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient(companydetails.RazorpayApiKeyId, companydetails.RazorpayApiKeySecret);
                        var qrdata = context.ConsumerRazorPayOrders.Where(x => x.OrderId == GenerateOrderAmtQRCode.OrderId && x.OrderAmount == GenerateOrderAmtQRCode.amount).ToList();
                        if (qrdata != null && qrdata.Any())
                        {
                            if (qrdata.Any(x => x.IsSuccess == true))
                            {
                                result.Status = false;
                                result.msg = "Please Check Status";
                            }
                            if (qrdata.Any(x => x.Status == "QrGenerated"))
                            {
                                var qrgeneratedata = qrdata.Where(x => x.Status == "QrGenerated").ToList();
                                foreach (var qr in qrgeneratedata)
                                {
                                    string autocloseid = null;
                                    string qrcodeid = qr.RazorPayQRId;
                                    Razorpay.Api.QrCode qrcode = client.QrCode.Fetch(qrcodeid).Close();
                                    autocloseid = qrcode["id"].ToString();
                                    if (!string.IsNullOrEmpty(autocloseid))
                                    {
                                        string response = JsonConvert.SerializeObject(qrcode);
                                        string qrcodestatus = qrcode["status"].ToString();
                                        qr.IsSuccess = false;
                                        qr.Status = "FAILED";
                                        qr.QRCode_Status = qrcodestatus;
                                        qr.PaymentResponse = response;
                                        context.Entry(qr).State = EntityState.Modified;
                                        context.Commit();
                                    }
                                    else
                                    {
                                        result.Status = false;
                                        result.msg = "Some error Occured in Close QR Code " + qrcodeid;
                                    }
                                }
                            }
                        }
                        string customername = ordermaster.CustomerName ?? "";
                        string mobile = ordermaster.Customerphonenum;
                        string warehousename = ordermaster.WarehouseName;
                        mobile = String.Concat("+91", mobile);
                        if (customername.Length < 3)
                        {
                            customername = String.Concat(customername, ordermaster.Customerphonenum);
                        }
                        if (customername.Length > 50)
                        {
                            customername = customername.Substring(0, 50);
                        }
                        Dictionary<string, object> options = new Dictionary<string, object>();
                        options.Add("name", customername);
                        options.Add("contact", mobile);
                        options.Add("fail_existing", "0");
                        Razorpay.Api.Customer customer = client.Customer.Create(options);
                        RazorPayCustomerId = customer["id"].ToString();
                        if (!string.IsNullOrEmpty(RazorPayCustomerId))
                        {
                            Dictionary<string, object> qrRequest = new Dictionary<string, object>();
                            qrRequest.Add("type", "upi_qr");
                            qrRequest.Add("name", "Consumer_Store " + warehousename);
                            qrRequest.Add("usage", "single_use");
                            qrRequest.Add("fixed_amount", true);
                            qrRequest.Add("payment_amount", GenerateOrderAmtQRCode.amount * 100);
                            qrRequest.Add("description", "OrderId - " + GenerateOrderAmtQRCode.OrderId);
                            qrRequest.Add("customer_id", RazorPayCustomerId);
                            qrRequest.Add("close_by", closeByTimestamp);
                            string qrrequestjson = JsonConvert.SerializeObject(qrRequest);
                            Razorpay.Api.QrCode qrcode = client.QrCode.Create(qrRequest);
                            string qrresponsejson = JsonConvert.SerializeObject(qrcode);
                            RazorPayId = qrcode["id"].ToString();
                            if (!string.IsNullOrEmpty(RazorPayId))
                            {
                                string qrcodeurl = qrcode["image_url"].ToString(); ;
                                decimal paymentamount = Convert.ToDecimal(qrcode["payment_amount"].ToString());
                                decimal paymentreceivedamount = Convert.ToDecimal(qrcode["payments_amount_received"].ToString());
                                string qrcodestatus = qrcode["status"].ToString(); ;
                                Model.RazorPay.ConsumerRazorPayOrder consumerRazorPayOrder = new Model.RazorPay.ConsumerRazorPayOrder();
                                consumerRazorPayOrder.RazorPayCustomerId = RazorPayCustomerId;
                                consumerRazorPayOrder.RazorPayQRId = RazorPayId;
                                consumerRazorPayOrder.OrderId = GenerateOrderAmtQRCode.OrderId;
                                consumerRazorPayOrder.OrderAmount = GenerateOrderAmtQRCode.amount;
                                consumerRazorPayOrder.QRCodeURL = qrcodeurl;
                                consumerRazorPayOrder.payment_amount = paymentamount;
                                consumerRazorPayOrder.payments_amount_received = paymentreceivedamount;
                                consumerRazorPayOrder.IsSuccess = false;
                                consumerRazorPayOrder.Status = "QrGenerated"; //QrGenerated , FAILED , SUCCESS
                                consumerRazorPayOrder.QRCode_Status = qrcodestatus;
                                consumerRazorPayOrder.QRRequest = qrrequestjson;
                                consumerRazorPayOrder.QRResponse = qrresponsejson;
                                consumerRazorPayOrder.CreatedDate = DateTime.Now;
                                consumerRazorPayOrder.QRCloseTime = closedqrtime;
                                consumerRazorPayOrder.IsActive = true;
                                consumerRazorPayOrder.IsDeleted = false;
                                consumerRazorPayOrder.CreatedBy = userid;
                                context.ConsumerRazorPayOrders.Add(consumerRazorPayOrder);
                                if (context.Commit() > 0)
                                {
                                    result.Status = true;
                                    result.QRCodeurl = qrcodeurl;
                                    result.Amount = GenerateOrderAmtQRCode.amount;
                                    result.OrderId = GenerateOrderAmtQRCode.OrderId;
                                    result.msg = "Qr Code Generated Successfully!!";

                                    #region Send qr to Warehouse Mobile

                                    try
                                    {
                                        var whmobile = context.WarehouseQrDevices.FirstOrDefault(s => s.WarehouseId == warehousedata.WarehouseId && s.IsActive);
                                        if (whmobile != null)
                                        {

                                            TextFileLogHelper.TraceLog($"QR Gen- Orderid: {GenerateOrderAmtQRCode.OrderId}, FcmId: {whmobile.FcmId}");


                                            string jsonPath = ConfigurationManager.AppSettings["WhQrFcmJsonPath"];
                                            var firebaseService = new FirebaseNotificationServiceHelper(jsonPath);
                                            var notification = new Notification
                                            {
                                                title = "qrcode",
                                                Message = qrcodeurl,
                                                priority = "high",
                                                CityName = $"Rs. {Convert.ToString(GenerateOrderAmtQRCode.amount)}",
                                                CompanyId = GenerateOrderAmtQRCode.OrderId
                                            };


                                            var notResult = await firebaseService.SendNotificationAsync(whmobile.FcmId, notification, "qrcode");
                                            TextFileLogHelper.TraceLog($"QR Gen- Orderid: {GenerateOrderAmtQRCode.OrderId}, Result: {notResult.ToString()}");


                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        TextFileLogHelper.LogError(ex.ToString());
                                    }

                                    #endregion
                                }
                                else
                                {
                                    result.Status = false;
                                    result.msg = "Something Went Wrong";
                                }
                            }
                            else
                            {
                                result.Status = false;
                                result.msg = "RazorPayId is not Created!!";
                            }
                        }
                        else
                        {
                            result.Status = false;
                            result.msg = "RazorPayCustomerId is not Created!!";
                        }



                    }
                    else
                    {
                        result.Status = false;
                        result.msg = "OrderId or Amount is Not Passed!!";
                    }

                }
                else
                {
                    result.Status = false;
                    result.msg = "QR Code Not Configured!!";
                }

            }
            return result;

        }


        



    }
}
