using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.PurchaseOrder;
using OpenHtmlToPdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers.PurchaseOrder
{
    [RoutePrefix("api/PRApproval")]
    public class PRApprovalController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);


        //[Route("Get")]
        //[HttpGet]
        //public List<PurchaseOrderMaster> Get()
        //{
        //    using (var db = new AuthContext())
        //    {
        //        try
        //        {
        //            //ass = db.PRApprovalDB.Where(a => a.Warehouseid == wid).ToList();
        //            var ass = db.DPurchaseOrderMaster.Where(x => x.PRStatus == 3 && x.IsPR == true).OrderByDescending(p => p.PurchaseOrderId).ToList();

        //            return ass;
        //        }
        //        catch (Exception ex)
        //        {

        //            return null;
        //        }
        //    }
        //}

        [Authorize]
        [Route("GetPendingData")]
        public List<PurchaseOrderMaster> GetPendingData(int warehouseid)
        {
            using (var db = new AuthContext())
            {

                var PRWH = db.DPurchaseOrderMaster.Where(x => x.PRStatus == 3 && x.IsPR == true && x.WarehouseId == warehouseid).OrderByDescending(p => p.PurchaseOrderId).ToList();
                return PRWH;

            }

        }
        [Route("GetApproved")]
        [HttpGet]
        public List<PurchaseOrderMaster> GetAppoved()
        {

            using (var db = new AuthContext())
            {
                try
                {
                    var ass = db.DPurchaseOrderMaster.Where(x => x.PRStatus == 5 && x.IsPR == true).OrderByDescending(p => p.PurchaseOrderId).ToList();
                    return ass;
                }
                catch (Exception ex)
                {

                    return null;
                }
            }
        }
        [Authorize]
        [Route("GetApprovedData")]
        public List<PurchaseOrderMaster> GetApprovedData(int warehouseid)
        {
            using (var db = new AuthContext())
            {

                var PRWH = db.DPurchaseOrderMaster.Where(x => x.PRStatus == 5 && x.IsPR == true && x.WarehouseId == warehouseid).OrderByDescending(p => p.PurchaseOrderId).ToList();

                return PRWH;

            }
        }

        //[Route("GetReject")]
        //[HttpGet]
        //public List<PurchaseOrderMaster> GetReject()
        //{

        //    using (var db = new AuthContext())
        //    {

        //        //ass = db.PRApprovalDB.Where(a => a.Warehouseid == wid).ToList();
        //        var ass = db.DPurchaseOrderMaster.Where(x => x.PRStatus == 6 && x.IsPR == true).OrderByDescending(p => p.PurchaseOrderId).ToList();

        //        return ass;


        //    }
        //}
        [Authorize]
        [Route("GetRejectedData")]
        public List<PurchaseOrderMaster> GetApproved(int warehouseid)
        {
            using (var db = new AuthContext())
            {

                var PRWH = db.DPurchaseOrderMaster.Where(x => x.PRStatus == 6 && x.IsPR == true && x.WarehouseId == warehouseid).OrderByDescending(p => p.PurchaseOrderId).ToList();

                return PRWH;

            }
        }

        [Route("Approved")]
        [HttpGet]
        public POResult prappoved(int PurchaseOrderId)
        {
            POResult pOResult = new POResult();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<PRApproval> pr = new List<PRApproval>();
            using (var db = new AuthContext())
            {

                var pRPaymentAppoved = db.PRPaymentAppoved.Where(x => x.PRId == PurchaseOrderId && x.IsActive == true).FirstOrDefault();

                if (pRPaymentAppoved.ApprovedBY == userid)
                {
                    var people = db.Peoples.Where(x => x.PeopleID == pRPaymentAppoved.ApprovedBY).Select(x => new { x.Mobile, x.Email }).FirstOrDefault();
                    if (people != null)
                    {
                        string[] saallowedcharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                        
                        var otp = GenerateRandomOTP(4, saallowedcharacters);
                        //ShopKirana PR id: {#var#} are waiting for your approval.	
                        //  var message = "PR#:" + PurchaseOrderId + " Approval OTP :"+ otp + ". ShopKirana";
                        var message = ""; //"PR#:{#var1#} Approval OTP :{#var2#}. Shopkirana";
                        var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "PR_Approval");
                        message = dltSMS == null ? "" : dltSMS.Template;                       
                        message = message.Replace("{#var1#}", PurchaseOrderId.ToString());
                        message = message.Replace("{#var2#}", otp.ToString());

                        //string messagesend = message + " :" + otp;
                        var isotpsend = false;
                        var Emailotpsend = false;
                        if (!string.IsNullOrEmpty(people.Mobile) && dltSMS!=null)
                        {
                            isotpsend = Common.Helpers.SendSMSHelper.SendSMS(people.Mobile, message, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), dltSMS.DLTId);

                        }
                        if (!string.IsNullOrEmpty(people.Email))
                        {
                            string fromemail = ConfigurationManager.AppSettings["FromEmailAddress"];
                            Emailotpsend = Common.Helpers.EmailHelper.SendMail(fromemail, people.Email, "", message, message, "");
                        }
                        if (isotpsend || Emailotpsend)
                        {

                            pRPaymentAppoved.OTP = otp;
                            db.Entry(pRPaymentAppoved).State = EntityState.Modified;
                            if (db.Commit() > 0)
                            {
                                pOResult.Status = true;
                                pOResult.Message = "OTP Successful Send on your mobile no.";
                            }
                        }
                        else
                        {
                            pOResult.Status = false;
                            pOResult.Message = "Something Went Wrong...";
                        }
                    }
                    else
                    {
                        pOResult.Status = false;
                        pOResult.Message = "Approve mobile not exist.";
                    }
                }
                else
                {
                    pOResult.Status = false;
                    pOResult.Message = "Your are not authorize to approvel PR";
                }

                return pOResult;

            }
        }

        [Route("Validateotp")]
        [HttpGet]
        public POResult validateotp(string otp, int purchaseorderid)
        {
           
            POResult pOResult = new POResult();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var db = new AuthContext())
            {

                var pRPaymentAppoved = db.PRPaymentAppoved.Where(x => x.PRId == purchaseorderid && x.IsActive == true).FirstOrDefault();
                if (pRPaymentAppoved.ApprovedBY == userid)
                {

                    var POMaster = db.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == purchaseorderid).FirstOrDefault();
                    var POItemDetails = db.DPurchaseOrderDeatil.Where(x => x.PurchaseOrderId == purchaseorderid).ToList();


                    #region checkAjustmentPOPaymentSettle
                    var ispaid = db.Database.SqlQuery<int>("exec CheckAdjustmentClosedPOPaid " + purchaseorderid).FirstOrDefault();
                    #endregion

                    if (otp == pRPaymentAppoved.OTP)
                    {
                        pRPaymentAppoved.IsPaymentDone = (ispaid > 0) ? true : false;
                        pRPaymentAppoved.IsApproved = true;
                        db.Entry(pRPaymentAppoved).State = EntityState.Modified;
                        POMaster.Status = "Approved";
                        POMaster.PRType = 0;
                        POMaster.PRStatus = 5;
                        POMaster.Advance_Amt = POMaster.ETotalAmount;
                        db.Entry(POMaster).State = EntityState.Modified;

                        UpdateItemWeight(POItemDetails, db);
                        if (db.Commit() > 0)
                        {
                            var PRCheck = db.PurchaseRequestMasterDB.Where(x => x.PurchaseOrderId == purchaseorderid).FirstOrDefault();
                            if (PRCheck == null)
                            {
                                PurchaseRequestMaster pm = new PurchaseRequestMaster();
                                pm.SupplierId = POMaster.SupplierId;
                                pm.SupplierName = POMaster.SupplierName;
                                pm.CreationDate = DateTime.Now;
                                pm.WarehouseId = POMaster.WarehouseId;
                                pm.CompanyId = POMaster.CompanyId;
                                pm.WarehouseName = POMaster.WarehouseName;
                                pm.PoType = "Manual";
                                pm.ETotalAmount = POMaster.ETotalAmount;
                                pm.BuyerId = POMaster.BuyerId;
                                pm.BuyerName = POMaster.BuyerName;
                                pm.Active = true;
                                pm.IsCashPurchase = POMaster.IsCashPurchase;
                                pm.CashPurchaseName = POMaster.CashPurchaseName;
                                pm.Advance_Amt = POMaster.ETotalAmount;
                                pm.DepoId = POMaster.DepoId;
                                pm.DepoName = POMaster.DepoName;
                                pm.CreatedBy = POMaster.CreatedBy;
                                pm.PurchaseOrderId = POMaster.PurchaseOrderId;
                                pm.PoInvoiceNo = POMaster.PoInvoiceNo;
                                pm.PurchaseOrderRequestDetail = new List<PurchaseOrderRequestDetail>();

                                foreach (var data in POItemDetails)
                                {
                                    PurchaseOrderRequestDetail pd = new PurchaseOrderRequestDetail();
                                    pd.ItemId = data.ItemId;
                                    pd.ItemNumber = data.ItemNumber;
                                    pd.itemBaseName = data.itemBaseName;
                                    pd.ItemMultiMRPId = data.ItemMultiMRPId;
                                    pd.HSNCode = data.HSNCode;
                                    pd.MRP = data.MRP;
                                    pd.SellingSku = data.SellingSku;
                                    pd.ItemName = data.ItemName;
                                    pd.PurchaseQty = data.PurchaseQty;
                                    pd.CreationDate = DateTime.Now;
                                    pd.Status = "ordered";
                                    pd.MOQ = data.MOQ;
                                    pd.Price = data.Price;
                                    pd.WarehouseId = data.WarehouseId;
                                    pd.CompanyId = data.CompanyId;
                                    pd.WarehouseName = data.WarehouseName;
                                    pd.SupplierId = data.SupplierId;
                                    pd.SupplierName = data.SupplierName;
                                    pd.TotalQuantity = data.TotalQuantity;
                                    pd.PurchaseName = data.PurchaseName;
                                    pd.PurchaseSku = data.PurchaseSku;
                                    pd.DepoId = data.DepoId;
                                    pd.DepoName = data.DepoName;
                                    pd.ConversionFactor = data.ConversionFactor;
                                    pd.PurchaseOrderId = data.PurchaseOrderId;
                                    pd.Category = data.Category;
                                    pm.PurchaseOrderRequestDetail.Add(pd);

                                }
                                db.PurchaseRequestMasterDB.Add(pm);
                                db.Commit();
                            }
                            pOResult.Status = true;
                            pOResult.Message = "PR Approved successfully.";

                            string To = "", From = "", Bcc = "";
                            #region PR to PO Convert message send
                            string query = "select  p.DisplayName,p.PeopleID,p.Email,p.Mobile from People p with(nolock)  where exists (select u.Id from AspNetUsers u  with(nolock)  inner join AspNetUserRoles ur  with(nolock)  on u.Id=ur.UserId and p.Email=u.Email inner join AspNetRoles r  with(nolock)  on ur.RoleId=r.Id and r.name = 'HQ Purchase Executive') and p.Active=1 and p.WarehouseId=" + POMaster.WarehouseId;

                            string msg = "";
                            List<PeopleMinDc> PeopleMinDcs = db.Database.SqlQuery<PeopleMinDc>(query).ToList();
                            foreach (var people in PeopleMinDcs)
                            {
                                Bcc += string.IsNullOrEmpty(Bcc)? people.Email : ";" + people.Email;
                                Sms s = new Sms();
                                          
                                //  msg = "Purchase Request #" + POMaster.PurchaseOrderId + "Converted to Purchase Order on date :" + DateTime.Now.ToString("dd/MM/yyyy") + ". Shopkirana";
                                msg = ""; //"Purchase Request #{#var1#} Converted to Purchase Order on date :{#var2#}. Shopkirana";
                                var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "PR_Converted_To_PO");
                                msg = dltSMS == null ? "" : dltSMS.Template;                               
                                msg = msg.Replace("{#var1#}", purchaseorderid.ToString());
                                msg = msg.Replace("{#var2#}", DateTime.Now.ToString("dd/MM/yyyy").ToString());

                                if (people.Mobile != null && dltSMS!=null) { s.sendOtp(people.Mobile, msg, dltSMS.DLTId); } 

                                //string subject = "Purchase Request #" + POMaster.PurchaseOrderId + " Converted to Purchase Order ";
                                //string message = subject + " on dated :" + DateTime.Now.ToString("dd/MM/yyyy");
                                //string To = ConfigurationManager.AppSettings["FromEmailAddress"].ToString();
                                //string From = people.Email;
                                //if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                                //    EmailHelper.SendMail(From, To, "", subject, message, "");

                            }
                            #endregion


                            #region  PO Convert mail send to supplier
                            
                            var supplier = db.Suppliers.FirstOrDefault(x => x.SupplierId == POMaster.SupplierId);
                            To = supplier.EmailId;
                            if (!string.IsNullOrEmpty(To))
                            {
                                var Depo = db.DepoMasters.FirstOrDefault(x => x.DepoId == POMaster.DepoId);
                                var ctnam = db.Warehouses.Where(x => x.WarehouseId == POMaster.WarehouseId).Select(x => new { x.WarehouseName, x.Address, x.CityName, x.GSTin, x.Phone }).FirstOrDefault();
                                var buyeremail = db.Peoples.FirstOrDefault(x => x.PeopleID == POMaster.BuyerId).Email;
                                if (!string.IsNullOrEmpty(buyeremail))
                                    Bcc += string.IsNullOrEmpty(Bcc) ? buyeremail : ";" + buyeremail;
                                string path = System.Web.Hosting.HostingEnvironment.MapPath(@"~\Templates\Invoice_Template.html");
                                string htmldata = "";
                                string replacetext = "";

                                From = AppConstants.MasterEmail;
                                if (File.Exists(path))
                                {

                                    htmldata = File.ReadAllText(path);
                                    if (!string.IsNullOrEmpty(htmldata))
                                    {
                                        replacetext = $"<span style='float:left'> <strong class='ng-binding'> Delivery at: {ctnam.WarehouseName} </strong></span><br>" +
                                        $"<span style='float:left' class='ng-binding'>C/O: {ctnam.Address} </span><br>" +
                                        $"<span style='float:left' class='ng-binding'>City:{ctnam.CityName}</span><br>" +
                                        $"<span style='float:left' class='ng-binding'>Tel.No: {ctnam.Phone}</span><br>" +
                                        $"<span style='float:left' class='ng-binding'>GSTIN: {ctnam.GSTin}</span>";
                                        htmldata = htmldata.Replace("{{Deliveryat}}", replacetext);

                                        replacetext = $"<span style = 'float:left' class='ng-binding'>Invoice:{POMaster.PoInvoiceNo}  </span> <br>" +
                                           $"<span style = 'float:left' class='ng-binding'>PO.No: {POMaster.PurchaseOrderId} </span> <br>" +
                                           $"<span style = 'float:left' class='ng-binding'>Date: {POMaster.CreationDate}</span> <br>" +
                                           $"<span style = 'float:left' class='ng-binding''>PO made by: {POMaster.CreatedBy}</span> <br>" +
                                           $"<span style = 'float:left' class='ng-binding'>Buyer: {POMaster.BuyerName}</span><br>" +
                                           $"<span style = 'float:left' class='ng-binding'>Picker Type: {POMaster.PickerType}</span>";
                                        htmldata = htmldata.Replace("{{InvoiceDetail}}", replacetext);

                                        replacetext = $"<span style='float:left' class='ng-binding'>{supplier.Name}</span><br><br>" +
                                        $"<span style='float:left' class='ng-binding'>C/O: {supplier.BillingAddress} </span><br>" +
                                        $"<span style='float:left' class='ng-binding'>Tel.No: {supplier.MobileNo}</span><br>" +
                                        $"<span style='float:left' class='ng-binding'>GSTIN: {supplier.GstInNumber}</span>";
                                        htmldata = htmldata.Replace("{{SupplierDetail}}", replacetext);

                                        replacetext = $"<span style='float:left' class='ng-binding'> {Depo.DepoName}</span><br>" +
                                        $"<span style='float:left' class='ng-binding'>C/O:  {Depo.Address}</span><br>" +
                                        $"<span style='float:left' class='ng-binding'>Tel.No: {Depo.OfficePhone}</span><br>" +
                                        $"<span style='float:left' class='ng-binding'>GSTIN: {Depo.GSTin}</span>";
                                        htmldata = htmldata.Replace("{{DepoDetail}}", replacetext);

                                        if (POItemDetails != null && POItemDetails.Any())
                                        {
                                            string podetailhtml = "";
                                            double totalPrice = 0, totalWeight = 0;

                                            foreach (var item in POItemDetails)
                                            {
                                                podetailhtml += $"<tr >" +
                                                     $"<td>{item.PurchaseSku}</td>" +
                                                     $"<td></td>" +
                                                     $"<td width='500px'><span style=' float:left;margin-left: 2em;'>{item.ItemName}</span></td>" +
                                                     //$"<td >{item.Category}</td>" +
                                                     $"<td>{item.HSNCode}</td>" +
                                                     $"<td>{item.Price}</td>" +
                                                     $"<td>{item.MOQ}</td>" +
                                                     $"<td>{((item.TotalQuantity) / (item.MOQ))}</td>" +
                                                     $"<td>{(item.MOQ * (item.TotalQuantity) / (item.MOQ))}</td>" +
                                                     $"<td>{item.Weight + " " + item.WeightType}</td>" +
                                                     $"<td>{Math.Round(((double)item.Weight * (double)item.TotalQuantity) / 1000 ,2)}&nbsp;Kg</td>" +
                                                     $"<td><span style=' float:right;color: black;font-weight:bold; font-size:large;' >{(item.Price) * (item.TotalQuantity)}&nbsp;<i class='fa fa-inr'></i></span></td>" +
                                                     $"</tr>";
                                                totalPrice += (item.Price) * (item.TotalQuantity);
                                                totalWeight += item.Weight.HasValue ? ((item.Weight.Value * item.TotalQuantity) / 1000) : 0;
                                            }
                                            if (!string.IsNullOrEmpty(podetailhtml))
                                            {
                                                totalWeight = Math.Round(totalWeight, 2);
                                                podetailhtml += $"<tr> <td colspan='12'> <span style='float:left'><strong>Grand Total:</strong></span></td><td>" +
                                                                $"<span style=' float:right;color: black;font-weight:bold; font-size:large;' class='ng-binding'>{totalPrice}&nbsp;<i class='fa fa-inr'></i></span>" +
                                                                $"</td></tr><tr>" +
                                                                $"<td colspan='12'> <span style='float:left'><strong>Total Weight:</strong></span></td><td>" +
                                                                $"<span style=' float:right;color: black;font-weight:bold; font-size:large;' class='ng-binding'>{totalWeight}&nbsp;Kg </span>" +
                                                                $" </td></tr>";
                                            }
                                            htmldata = htmldata.Replace("{{POItemDetail}}", podetailhtml);
                                        }


                                    }
                                }


                                if (!string.IsNullOrEmpty(htmldata))
                                {
                                    string fileUrl = "";
                                    string fullPhysicalPath = "";
                                    string thFileName = "";
                                    string TartgetfolderPath = "";

                                    TartgetfolderPath = HttpContext.Current.Server.MapPath(@"~\PDFForeCast");
                                    if (!Directory.Exists(TartgetfolderPath))
                                        Directory.CreateDirectory(TartgetfolderPath);


                                    thFileName = "PO_" + POMaster.PurchaseOrderId + "_" + DateTime.Now.ToString("MMddyyyyhhmmss") + ".pdf";
                                    fileUrl = "/PDFForeCast" + "/" + thFileName;
                                    fullPhysicalPath = TartgetfolderPath + "\\" + thFileName;

                                    var OutPutFile = Path.Combine(HttpContext.Current.Server.MapPath("~/PDFForeCast"), thFileName);

                                    byte[] pdf = null;

                                    pdf = Pdf
                                          .From(htmldata)
                                          //.WithGlobalSetting("orientation", "Landscape")
                                          //.WithObjectSetting("web.defaultEncoding", "utf-8")
                                          .OfSize(OpenHtmlToPdf.PaperSize.A4)
                                          .WithTitle("Invoice")
                                          .WithoutOutline()
                                          .WithMargins(PaperMargins.All(0.0.Millimeters()))
                                          .Portrait()
                                          .Comressed()
                                          .Content();
                                    FileStream file = File.Create(OutPutFile);
                                    file.Write(pdf, 0, pdf.Length);
                                    file.Close();


                                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " PO";
                                    string message = " ShopKirana has raised a  New PO " + POMaster.PurchaseOrderId + " : for amount " + Math.Round(POMaster.ETotalAmount, 2) + " : In Hub: " + POMaster.WarehouseName ;
                                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                                    {
                                        EmailHelper.SendMail(From, To, Bcc, subject, message, fullPhysicalPath);
                                    }
                                }
                            }
                            #endregion 
                        }
                    }
                    else
                    {
                        pOResult.Status = false;
                        pOResult.Message = "Invalid OTP please entry valid OTP.";
                    }
                }
                else
                {
                    pOResult.Status = false;
                    pOResult.Message = "You are not authorize to approve this PR.";
                }

            }
            return pOResult;

        }

        [Route("ResendForApproval")]
        [HttpGet]
        public POResult ResendForApprove(int PurchaseOrderId)
        {
            POResult pOResult = new POResult();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<PRApproval> pr = new List<PRApproval>();
            using (var db = new AuthContext())
            {

                var pRPaymentAppoved = db.PRPaymentAppoved.Where(x => x.PRId == PurchaseOrderId).FirstOrDefault();

                if (pRPaymentAppoved.ApprovedBY == userid)
                {
                    PurchaseOrderMaster PRData = db.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == PurchaseOrderId && x.IsPR == true).FirstOrDefault();
                    PRData.PRStatus = 3;
                    db.Entry(pRPaymentAppoved).State = EntityState.Modified;
                    if (db.Commit() > 0)
                    {
                        pOResult.Status = true;
                        pOResult.Message = "Reset For Approval";
                    }
                }
                else
                {
                    pOResult.Status = false;
                    pOResult.Message = "You Are Not Authorized Person.";
                }
            }
            return pOResult;
        }

        [Route("RejectPR")]
        [HttpPost]
        public POResult rejectpr(RejectPR rejectpr)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            POResult pOResult = new POResult();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext db = new AuthContext())
            {
                var pRPaymentAppoved = db.PRPaymentAppoved.Where(x => x.PRId == rejectpr.PurchaseOrderId && x.IsActive == true).FirstOrDefault();
                if (pRPaymentAppoved.ApprovedBY == userid)
                {
                    PurchaseOrderMaster pr = db.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == rejectpr.PurchaseOrderId && x.IsPR == true).FirstOrDefault();

                    if (pr != null)
                    {
                        pRPaymentAppoved.Comment = rejectpr.Comment;
                        db.Entry(pRPaymentAppoved).State = EntityState.Modified;
                        // pr.Comment = rejectpr.Comment;
                        pr.CanceledById = userid;
                        pr.PRStatus = 6;
                        db.Entry(pr).State = EntityState.Modified;
                        if (db.Commit() > 0)
                        {
                            pOResult.Status = true;
                            pOResult.Message = "PR Rejected successfully.";
                        }
                    }
                }
                else
                {
                    pOResult.Status = false;
                    pOResult.Message = "You are not Authorize to Reject this PR.";
                }
            }
            return pOResult;

        }

        private string GenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)
        {
            string sOTP = String.Empty;
            string sTempChars = String.Empty;
            Random rand = new Random();

            for (int i = 0; i < iOTPLength; i++)
            {
                int p = rand.Next(0, saAllowedCharacters.Length);
                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
                sOTP += sTempChars;
            }
            return sOTP;
        }



        [Authorize]
        [Route("GetApprovedList")]
        [HttpGet]
        public dynamic getAllHubData()
        {
            try
            {
                using (var db = new AuthContext())
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    string sqlquery = "SELECT * FROM PurchaseOrderMasters INNER JOIN PRPaymentAppoveds"
                                    + " ON PurchaseOrderMasters.PurchaseOrderId = PRPaymentAppoveds.PRId where"
                                    + " PRPaymentAppoveds.IsApproved = 1" + "and PurchaseOrderMasters.PRStatus = 5 " + "and  PRPaymentAppoveds.IsActive = 1";

                    List<PurchaseOrderMasterDTO> PRSts = db.Database.SqlQuery<PurchaseOrderMasterDTO>(sqlquery).OrderByDescending(a => a.PurchaseOrderId).ToList();
                    return PRSts;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }




        [Authorize]
        [Route("GetApprovedWHWise")]
        [HttpGet]
        public dynamic GetApprovedWHWise(int warehouseid)
        {
            try
            {
                using (var db = new AuthContext())
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    string sqlquery = "SELECT * FROM PurchaseOrderMasters INNER JOIN PRPaymentAppoveds"
                                    + " ON PurchaseOrderMasters.PurchaseOrderId = PRPaymentAppoveds.PRId where"
                                    + " PRPaymentAppoveds.IsApproved = 1" + "and PurchaseOrderMasters.PRStatus = 5 " + "and PRPaymentAppoveds.IsActive = 1 " + "and PurchaseOrderMasters.WarehouseId= " + warehouseid;

                    List<PurchaseOrderMasterDTO> PRSts = db.Database.SqlQuery<PurchaseOrderMasterDTO>(sqlquery).OrderByDescending(a => a.PurchaseOrderId).ToList();
                    return PRSts;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        [Route("PaymentConfirm")]
        [HttpPost]
        public POResult Confirmpayment(ConfirmPayment confirm)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            POResult pOResult = new POResult();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext db = new AuthContext())
            {
                People pdata = db.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();

                var PRPayment = db.PRPaymentAppoved.Where(x => x.PRId == confirm.PurchaseOrderId).FirstOrDefault();
                if (PRPayment != null)
                {
                    PRPayment.Comment = confirm.Comment;
                    PRPayment.IsPaymentDone = confirm.IsPaymentDone;
                    PRPayment.ModifiedBy = pdata.PeopleID;
                    PRPayment.ModifiedDate = DateTime.Now;
                    db.Entry(PRPayment).State = EntityState.Modified;
                    if (db.Commit() > 0)
                    {
                        pOResult.Status = true;
                        pOResult.Message = "PR Payment Confirmed";
                    }
                }
                else
                {
                    pOResult.Status = false;
                    pOResult.Message = "Something went Wrong";
                }

            }
            return pOResult;
        }

        [Authorize]
        [Route("GenerateExcelPRPaymentApproval")]
        [HttpPost]
        public string GetExcelPRPaymentApproval(FilterPRDTO FilterPRDTO)
        {
            string result = "";
            if (string.IsNullOrEmpty(FilterPRDTO.start.ToString()) || string.IsNullOrEmpty(FilterPRDTO.end.ToString()))
            {
                return result;
            }
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);
            var fileName = "PRPaymentApproval.xlsx";
            string filePath = ExcelSavePath + fileName;
            var returnPath = string.Format("{0}://{1}{2}/{3}", HttpContext.Current.Request.UrlReferrer.Scheme
                                                            , HttpContext.Current.Request.Url.DnsSafeHost
                                                            , HttpContext.Current.Request.Url.Port == 80 || HttpContext.Current.Request.Url.Port == 443 ? "" : ":" + HttpContext.Current.Request.Url.Port
                                                            , string.Format("ExcelGeneratePath/{0}", fileName));


            string sqlquery = " SELECT  FORMAT( CreationDate, 'dd/MM/yyyy') as CreatedDate , PurchaseOrderId , ETotalAmount  ,BuyerName, SupplierName,WarehouseName,case when  PRStatus=5 then 'PR Appoved for Payment' end as [Status],PRPaymentType as [PRType],case when  IsPaymentDone = 1 then 'Payment Confirmed' when IsPaymentDone = 0 then 'Payment Inprogress' end as [PaymentStatus] FROM PurchaseOrderMasters INNER JOIN PRPaymentAppoveds  ON PurchaseOrderMasters.PurchaseOrderId = PRPaymentAppoveds.PRId where  PRPaymentAppoveds.IsApproved = 1 and PurchaseOrderMasters.PRStatus = 5 and  PRPaymentAppoveds.IsActive = 1 and PurchaseOrderMasters.CreationDate between '" + FilterPRDTO.start + "'  and '" + FilterPRDTO.end + "'";

            using (var db = new AuthContext())
            {
                List<ExcelPRPayment> PRSts = db.Database.SqlQuery<ExcelPRPayment>(sqlquery).OrderByDescending(a => a.PurchaseOrderId).ToList();
                var dataTables = new List<DataTable>();
                var purchaseOrderMasterDTOs = ClassToDataTable.CreateDataTable(PRSts);
                purchaseOrderMasterDTOs.TableName = "PRSts";
                dataTables.Add(purchaseOrderMasterDTOs);
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    result = returnPath;
                }
            }
            return result;
        }


        [Authorize]
        [Route("SearchPRData")]
        [HttpPost]
        public dynamic getAllSearchData(FilterPRDTO FilterPRDTO)
        {
            try
            {
                using (var db = new AuthContext())
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0; int compid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    string whereclause = compid > 0 ? " and PurchaseOrderMasters.CompanyId = " + compid : "";

                    if (FilterPRDTO.WarehouseId > 0)
                        whereclause += " and PurchaseOrderMasters.WarehouseId = " + FilterPRDTO.WarehouseId;

                    if (FilterPRDTO.PurchaseOrderId > 0)
                        whereclause += " and PurchaseOrderMasters.PurchaseOrderId = " + FilterPRDTO.PurchaseOrderId;

                    if (!string.IsNullOrEmpty(FilterPRDTO.SupplierName))
                        whereclause += " and PurchaseOrderMasters.SupplierName Like " + "'%" + FilterPRDTO.SupplierName + "%'";

                    if (FilterPRDTO.IsPaymentDone == 1)
                    {
                        whereclause += " and PRPaymentAppoveds.IsPaymentDone = 1 ";
                    }
                    if (FilterPRDTO.IsPaymentDone == 2)
                    {
                        whereclause += " and PRPaymentAppoveds.IsPaymentDone = 0 ";
                    }


                    if (FilterPRDTO.start.HasValue && FilterPRDTO.end.HasValue)
                    {
                        whereclause += " and (PurchaseOrderMasters.CreationDate >= " + "'" + FilterPRDTO.start.Value.ToString("MM-dd-yyyy HH:mm:ss") + "'" + " And  PurchaseOrderMasters.CreationDate <=" + "'" + FilterPRDTO.end.Value.ToString("MM-dd-yyyy HH:mm:ss") + "')";

                    }


                    string sqlquery = "SELECT * FROM PurchaseOrderMasters INNER JOIN PRPaymentAppoveds"
                                    + " ON PurchaseOrderMasters.PurchaseOrderId = PRPaymentAppoveds.PRId where"
                                    + " PRPaymentAppoveds.IsApproved = 1" + "and PurchaseOrderMasters.PRStatus = 5 " + "and  PRPaymentAppoveds.IsActive = 1" + whereclause;

                    List<PurchaseOrderMasterDTO> PRSts = db.Database.SqlQuery<PurchaseOrderMasterDTO>(sqlquery).OrderByDescending(a => a.PurchaseOrderId).Skip((FilterPRDTO.page - 1) * FilterPRDTO.list).Take(FilterPRDTO.list).ToList();

                    string sqlqueryCount = "SELECT count(*) as POcount FROM PurchaseOrderMasters INNER JOIN PRPaymentAppoveds"
                                    + " ON PurchaseOrderMasters.PurchaseOrderId = PRPaymentAppoveds.PRId where"
                                    + " PRPaymentAppoveds.IsApproved = 1" + "and PurchaseOrderMasters.PRStatus = 5 " + "and  PRPaymentAppoveds.IsActive = 1" + whereclause;

                    int count = db.Database.SqlQuery<int>(sqlqueryCount).FirstOrDefault();


                    PaggingDTO obj = new PaggingDTO();
                    obj.purchaseOrderMasterdto = PRSts;
                    obj.total_count = count;

                    return obj;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        [Authorize]
        [Route("SearchPRPending")]
        [HttpPost]
        public dynamic getSearchPending(FilterPRDTO FilterPRDTO)
        {
            try
            {
                using (var db = new AuthContext())
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0; int compid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    string whereclause = compid > 0 ? " and PurchaseOrderMasters.CompanyId = " + compid : "";

                    if (FilterPRDTO.WarehouseId > 0)
                        whereclause += " and PurchaseOrderMasters.WarehouseId = " + FilterPRDTO.WarehouseId;

                    if (FilterPRDTO.PurchaseOrderId > 0)
                        whereclause += " and PurchaseOrderMasters.PurchaseOrderId = " + FilterPRDTO.PurchaseOrderId;

                    if (!string.IsNullOrEmpty(FilterPRDTO.SupplierName))
                        whereclause += " and PurchaseOrderMasters.SupplierName Like " + "'%" + FilterPRDTO.SupplierName + "%'";

                    //if (FilterPRDTO.IsPaymentDone == 1)
                    //{
                    //    whereclause += " and PRPaymentAppoveds.IsPaymentDone = 1 ";
                    //}
                    //if (FilterPRDTO.IsPaymentDone == 2)
                    //{
                    //    whereclause += " and PRPaymentAppoveds.IsPaymentDone = 0 ";
                    //}


                    if (FilterPRDTO.start.HasValue && FilterPRDTO.end.HasValue)
                        whereclause += " and (PurchaseOrderMasters.CreationDate >= " + "'" + FilterPRDTO.start.Value.ToString("MM-dd-yyyy HH:mm:ss") + "'" + " And  PurchaseOrderMasters.CreationDate <=" + "'" + FilterPRDTO.end.Value.ToString("MM-dd-yyyy HH:mm:ss") + "')";


                    string sqlquery = "SELECT * FROM PurchaseOrderMasters INNER JOIN PRPaymentAppoveds"
                                    + " ON PurchaseOrderMasters.PurchaseOrderId = PRPaymentAppoveds.PRId where"
                                    + " PRPaymentAppoveds.IsApproved = 0" + "and PurchaseOrderMasters.PRStatus = 3 " + "and  PRPaymentAppoveds.IsActive = 1" + whereclause;

                    List<PurchaseOrderMasterDTO> PRSts = db.Database.SqlQuery<PurchaseOrderMasterDTO>(sqlquery).OrderByDescending(a => a.PurchaseOrderId).Skip((FilterPRDTO.page - 1) * FilterPRDTO.list).Take(FilterPRDTO.list).ToList();

                    string sqlqueryCount = "SELECT count(*) as POcount FROM PurchaseOrderMasters INNER JOIN PRPaymentAppoveds"
                                   + " ON PurchaseOrderMasters.PurchaseOrderId = PRPaymentAppoveds.PRId where"
                                   + " PRPaymentAppoveds.IsApproved = 0" + "and PurchaseOrderMasters.PRStatus = 3 " + "and  PRPaymentAppoveds.IsActive = 1 and PurchaseOrderMasters.WarehouseId= " + FilterPRDTO.WarehouseId;
                    int count = db.Database.SqlQuery<int>(sqlqueryCount).FirstOrDefault();


                    PaggingDTO obj = new PaggingDTO();
                    obj.purchaseOrderMasterdto = PRSts;
                    obj.total_count = count;

                    return obj;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [Authorize]
        [Route("SearchPRRejected")]
        [HttpPost]
        public dynamic getSearchRejectd(FilterPRDTO FilterPRDTO)
        {
            try
            {
                using (var db = new AuthContext())
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0; int compid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    string whereclause = compid > 0 ? " and PurchaseOrderMasters.CompanyId = " + compid : "";

                    if (FilterPRDTO.WarehouseId > 0)
                        whereclause += " and PurchaseOrderMasters.WarehouseId = " + FilterPRDTO.WarehouseId;

                    if (FilterPRDTO.PurchaseOrderId > 0)
                        whereclause += " and PurchaseOrderMasters.PurchaseOrderId = " + FilterPRDTO.PurchaseOrderId;

                    if (!string.IsNullOrEmpty(FilterPRDTO.SupplierName))
                        whereclause += " and PurchaseOrderMasters.SupplierName Like " + "'%" + FilterPRDTO.SupplierName + "%'";

                    //if (FilterPRDTO.IsPaymentDone == 1)
                    //{
                    //    whereclause += " and PRPaymentAppoveds.IsPaymentDone = 1 ";
                    //}
                    //if (FilterPRDTO.IsPaymentDone == 2)
                    //{
                    //    whereclause += " and PRPaymentAppoveds.IsPaymentDone = 0 ";
                    //}


                    if (FilterPRDTO.start.HasValue && FilterPRDTO.end.HasValue)
                        whereclause += " and (PurchaseOrderMasters.CreationDate >= " + "'" + FilterPRDTO.start.Value.ToString("MM-dd-yyyy HH:mm:ss") + "'" + " And  PurchaseOrderMasters.CreationDate <=" + "'" + FilterPRDTO.end.Value.ToString("MM-dd-yyyy HH:mm:ss") + "')";


                    string sqlquery = "SELECT * FROM PurchaseOrderMasters INNER JOIN PRPaymentAppoveds"
                                    + " ON PurchaseOrderMasters.PurchaseOrderId = PRPaymentAppoveds.PRId where"
                                    + " PRPaymentAppoveds.IsApproved = 0" + "and PurchaseOrderMasters.PRStatus = 6 " + "and  PRPaymentAppoveds.IsActive = 1" + whereclause;

                    List<PurchaseOrderMasterDTO> PRSts = db.Database.SqlQuery<PurchaseOrderMasterDTO>(sqlquery).OrderByDescending(a => a.PurchaseOrderId).Skip((FilterPRDTO.page - 1) * FilterPRDTO.list).Take(FilterPRDTO.list).ToList();

                    string sqlqueryCount = "SELECT count(*) as POcount FROM PurchaseOrderMasters INNER JOIN PRPaymentAppoveds"
                                    + " ON PurchaseOrderMasters.PurchaseOrderId = PRPaymentAppoveds.PRId where"
                                    + " PRPaymentAppoveds.IsApproved = 0" + "and PurchaseOrderMasters.PRStatus = 6 " + "and  PRPaymentAppoveds.IsActive = 1 and PurchaseOrderMasters.WarehouseId= " + FilterPRDTO.WarehouseId;

                    int count = db.Database.SqlQuery<int>(sqlqueryCount).FirstOrDefault();


                    PaggingDTO obj = new PaggingDTO();
                    obj.purchaseOrderMasterdto = PRSts;
                    obj.total_count = count;

                    return obj;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [Authorize]
        [Route("Get")]
        [HttpGet]
        public dynamic Get()
        {
            try
            {
                using (var db = new AuthContext())
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    string sqlquery = "SELECT * FROM PurchaseOrderMasters INNER JOIN PRPaymentAppoveds"
                                    + " ON PurchaseOrderMasters.PurchaseOrderId = PRPaymentAppoveds.PRId where"
                                    + " PRPaymentAppoveds.IsApproved = 0" + "and PurchaseOrderMasters.PRStatus = 3 " + "and  PRPaymentAppoveds.IsActive = 1";

                    List<PurchaseOrderMasterDTO> PRSts = db.Database.SqlQuery<PurchaseOrderMasterDTO>(sqlquery).OrderByDescending(a => a.PurchaseOrderId).ToList();
                    return PRSts;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        [Authorize]
        [Route("GetReject")]
        [HttpGet]
        public dynamic GetRejerct()
        {
            try
            {
                using (var db = new AuthContext())
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    string sqlquery = "SELECT * FROM PurchaseOrderMasters INNER JOIN PRPaymentAppoveds"
                                    + " ON PurchaseOrderMasters.PurchaseOrderId = PRPaymentAppoveds.PRId where"
                                    + " PRPaymentAppoveds.IsApproved = 0" + "and PurchaseOrderMasters.PRStatus = 6 " + "and  PRPaymentAppoveds.IsActive = 1";

                    List<PurchaseOrderMasterDTO> PRSts = db.Database.SqlQuery<PurchaseOrderMasterDTO>(sqlquery).OrderByDescending(a => a.PurchaseOrderId).ToList();
                    return PRSts;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        [Route("RejectPRAccount")]
        [HttpPost]
        public POResult rejectpraccount(RejectPR rejectpr)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            POResult pOResult = new POResult();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext db = new AuthContext())
            {
                var pRPaymentAppoved = db.PRPaymentAppoved.Where(x => x.PRId == rejectpr.PurchaseOrderId && x.IsActive == true).FirstOrDefault();
                var POStatus = db.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == rejectpr.PurchaseOrderId).FirstOrDefault();
                var IRMaster = db.IRMasterDB.Where(x => x.PurchaseOrderId == rejectpr.PurchaseOrderId).ToList();
                if (rejectpr.check == true)
                {
                    var Supplier = db.Suppliers.Where(x => x.SupplierId == POStatus.SupplierId).FirstOrDefault();
                    POStatus.PRPaymentType = "CreditPR";
                    POStatus.SupplierCreditDay = Supplier != null ? Supplier.PaymentTerms : 0;
                    // POStatus.Comment = rejectpr.Comment;
                    db.Entry(POStatus).State = EntityState.Modified;

                    pRPaymentAppoved.Comment = rejectpr.Comment;
                    pRPaymentAppoved.ModifiedBy = userid;
                    db.Entry(pRPaymentAppoved).State = EntityState.Modified;

                    if (IRMaster.Count > 0)
                    {
                        foreach (var i in IRMaster)
                        {
                            i.DueDays = Supplier != null ? Supplier.PaymentTerms : 0;
                            db.Entry(i).State = EntityState.Modified;

                        }
                    }
                }
                else
                {
                    POStatus.Status = "PR Send for Approval";
                    POStatus.PRStatus = 6;
                    POStatus.PRType = 1;
                    // POStatus.Comment = rejectpr.Comment;                  
                    db.Entry(POStatus).State = EntityState.Modified;

                    pRPaymentAppoved.IsApproved = false;
                    pRPaymentAppoved.Comment = rejectpr.Comment;
                    pRPaymentAppoved.ModifiedBy = userid;
                    db.Entry(pRPaymentAppoved).State = EntityState.Modified;
                }
                if (db.Commit() > 0)
                {
                    pOResult.Status = true;
                    pOResult.Message = "PR Rejected successfully.";
                }

            }
            return pOResult;

        }

        [Route("CheckGR")]
        [HttpGet]
        public bool CheckGR(int PurchaseOrderId)
        {
            POResult pOResult = new POResult();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            bool result = false;
            List<GRcheck> pr = new List<GRcheck>();

            using (var db = new AuthContext())
            {
                string query = "select distinct b.PurchaseOrderDetailId from PurchaseOrderDetails a inner join GoodsReceivedDetails b on a.PurchaseOrderDetailId = b.PurchaseOrderDetailId Where a.PurchaseOrderId =" + PurchaseOrderId;
                pr = db.Database.SqlQuery<GRcheck>(query).ToList();
                if (pr.Count > 0)
                {
                    result = true;
                }
                return result;

            }
        }

        private bool UpdateItemWeight(List<PurchaseOrderDetail> detailList, AuthContext context)
        {
            if(detailList != null && detailList.Any())
            {
                foreach (var detail in detailList)
                {
                    var itemList = context.ItemMasterCentralDB.Where(x => x.Number == detail.ItemNumber && x.Deleted == false).ToList();
                    if(itemList != null && itemList.Any())
                    {
                        foreach (var item in itemList)
                        {
                            if(detail.Weight != null && !string.IsNullOrEmpty(detail.WeightType))
                            {
                                item.weight = detail.Weight;
                                item.weighttype = detail.WeightType;

                                if(item.weighttype == "pc" && detail.WeightInGram.HasValue && detail.WeightInGram > 0)
                                {
                                    item.WeightInGram = detail.WeightInGram;
                                }

                                context.Entry(item).State = EntityState.Modified;
                            }
                        }
                    }
                }
                
            }
            return true;
        }
    }



    public class RejectPR
    {
        public bool check { get; set; }
        public string Comment { get; set; }
        public long PurchaseOrderId { get; set; }
    }
    public class PurchaseOrderMasterDTO
    {
        public int PurchaseOrderId { get; set; }
        public string PoInvoiceNo { get; set; }
        public string TransactionNumber { get; set; }
        public int? CompanyId { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string WarehouseCity { get; set; }
        public string Status { get; set; }
        public double? discount1 { get; set; }
        public double TotalAmount { get; set; }
        public double Advance_Amt { get; set; }
        public double ETotalAmount { get; set; }
        public string PoType { get; set; }
        public int? BuyerId { get; set; }
        public string BuyerName { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; }
        public string ApprovedBy { get; set; }
        public string RejectedBy { get; set; }
        public bool Acitve { get; set; }
        public int? DepoId { get; set; }
        public string DepoName { get; set; }
        public bool IsCashPurchase { get; set; }
        public string CashPurchaseName { get; set; }
        public bool Deleted { get; set; }
        public bool Active { get; set; }
        public int? DueDays { get; set; }
        public int SupplierStatus { get; set; }
        public int PRStatus { get; set; }
        public bool IsPR { get; set; }
        public long PRId { get; set; }
        public int ApprovedBY { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? PaymentApprovedDate { get; set; }
        public bool IsPaymentDone { get; set; }
        public string Comment { get; set; }
        public string PRPaymentType { get; set; }
        public int total_count { get; set; }
        public bool IsAdjustmentPo { get; set; }
        public double TDSPercentage { get; set; }
        //
        public double PaymentTillDate { get; set; }
        //
    }
    public class ConfirmPayment
    {
        public string Comment { get; set; }
        public long PurchaseOrderId { get; set; }
        public bool IsPaymentDone { get; set; }
    }
    public class FilterPRDTO
    {
        public int WarehouseId { get; set; }
        public DateTime? start { get; set; }
        public DateTime? end { get; set; }
        public int PurchaseOrderId { get; set; }
        public string SupplierName { get; set; }
        public int IsPaymentDone { get; set; }
        public int list { get; set; }
        public int page { get; set; }

        public int isexcelfile { get; set; }

    }
    public class GRcheck
    {
        public int PurchaseOrderDetailId { get; set; }
    }
    public class PaggingDTO
    {
        public List<PurchaseOrderMasterDTO> purchaseOrderMasterdto { get; set; }
        public int total_count { get; set; }

    }
    public class Pocount
    {
        public int POcount { get; set; }
    }
    public class ExcelPRPayment
    {
        public string CreatedDate { get; set; }
        public Int32 PurchaseOrderId { get; set; }
        public double ETotalAmount { get; set; }
        public string SupplierName { get; set; }
        public string BuyerName { get; set; }
        public string WarehouseName { get; set; }
        public string Status { get; set; }
        public string PRType { get; set; }
        public string PaymentStatus { get; set; }
    }
}
