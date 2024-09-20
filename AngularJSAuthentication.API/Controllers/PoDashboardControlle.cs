using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model;
using NLog;
using OpenHtmlToPdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/podash")]
    public class PoDashboardController : ApiController
    {
       
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Get PO for approval by approval people
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        [Authorize]
        [Route("")]
        public IEnumerable<PurchaseOrderMaster> Get(string status, int WarehouseId)
        {
            logger.Info("start Category: ");
            List<PurchaseOrderMaster> ass = new List<PurchaseOrderMaster>();
            using (var db = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = db.DPurchaseOrderMaster.Where(a => a.WarehouseId == WarehouseId && (a.Status == status && a.Approval1 == userid || a.Approval2 == userid || a.Approval3 == userid || a.Approval4 == userid || a.Approval5 == userid)).OrderByDescending(a => a.PurchaseOrderId).ToList();
                    logger.Info("End  Category: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
        }

        /// <summary>
        /// get PO for review by reviewer
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        [Authorize]
        [Route("getReviewer")]
        public IEnumerable<PurchaseOrderMaster> GetReviewer(string status, int WarehouseId)
        {
            logger.Info("start Category: ");
            List<PurchaseOrderMaster> ass = new List<PurchaseOrderMaster>();
            using (var db = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = db.DPurchaseOrderMaster.Where(a => a.WarehouseId == WarehouseId && (a.Rewiever1 == userid || a.Rewiever2 == userid || a.Rewiever3 == userid || a.Rewiever4 == userid || a.Rewiever5 == userid)).OrderByDescending(a => a.PurchaseOrderId).ToList();
                    logger.Info("End  Category: ");
                    return ass.AsEnumerable().Where(a => a.Status == status).ToList();
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
        }

        /// <summary>
        /// Get All data fro po master
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("getAll")]
        public IEnumerable<PurchaseOrderMaster> GetAll(int WarehouseId)
        {
            logger.Info("start Category: ");
            List<PurchaseOrderMaster> ass = new List<PurchaseOrderMaster>();
            using (var db = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    List<Warehouse> wh = db.Warehouses.ToList();
                    ass = db.DPurchaseOrderMaster.Where(a => a.WarehouseId == WarehouseId && (a.Approval1 == userid || a.Approval2 == userid || a.Approval3 == userid || a.Approval4 == userid || a.Approval5 == userid)).OrderByDescending(a => a.PurchaseOrderId).ToList();

                    foreach (PurchaseOrderMaster a in ass)
                    {
                        a.WarehouseCity = wh.Where(q => q.WarehouseId == a.WarehouseId).Select(e => e.CityName).SingleOrDefault();
                    }
                    logger.Info("End  Category: ");
                    return ass.AsEnumerable().Where(a => a.Status == "Self Approved" || a.Status == "Send for Approval" || a.Status == "Rejected" || a.Status == "Send for Reviewer" || a.Status == "Approved").ToList();
                    //&& a.Rewiever1 == userid || a.Rewiever2 == userid || a.Rewiever3 == userid || a.Rewiever4 == userid || a.Rewiever5 == userid
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
        }

        /// <summary>
        /// Approved PO By Approver people and send to Reviewer if exist.
        /// if PO approved then send PO PDF to Supplier
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Authorize]
        [Route("sendtoReviewer")]
        [HttpPut]
        public PurchaseOrderMaster SendtoReviewer(PurchaseOrderMaster data)
        {
            logger.Info("start Category: ");
            using (var db = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    
                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    People p = db.Peoples.Where(q => q.PeopleID == userid).FirstOrDefault();
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    PoApproval IsRevHere = db.PoApprovalDB.Where(a => a.Level == data.Level).FirstOrDefault();
                    PurchaseOrderMaster po = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == data.PurchaseOrderId && a.Status == "Send for Approval").SingleOrDefault();
                    if (po != null)
                    {
                        po.Status = "Approved";
                        po.progress = "50";
                        po.ApprovedBy = p.DisplayName;
                        db.Entry(po).State = EntityState.Modified;
                        db.Commit();
                        SendMailCreditWalletNotification(po.PurchaseOrderId);

                        var POItemDetails = db.DPurchaseOrderDeatil.Where(x => x.PurchaseOrderId == po.PurchaseOrderId).ToList();
                        var supplier = db.Suppliers.Where(a => a.SupplierId == po.SupplierId).SingleOrDefault();
                        if (supplier != null)
                        {
                            Sms s = new Sms();
                            //string msg = " ShopKirana raise New PO #: " + po.PurchaseOrderId + " for amount :" + Math.Round(po.ETotalAmount, 2) + " In Hub:" + po.WarehouseName;
                            string msg = ""; //"ShopKirana raise New PO {#var1#} : for amount {#var2#} : In Hub: {#var3#}.ShopKirana";
                            var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "PO_Raise");
                            msg = dltSMS == null ? "" : dltSMS.Template;                           
                            msg = msg.Replace("{#var1#}", po.PurchaseOrderId.ToString());
                            msg = msg.Replace("{#var2#}", Math.Round(po.ETotalAmount, 2).ToString());
                            msg = msg.Replace("{#var3#}", po.WarehouseName.ToString());
                            string Mob = supplier?.MobileNo;
                            string FCMID = supplier?.fcmId;
                            string FCMNotification = "{title:'New PO Generated',body:'" + msg + "',icon:'',notify_type:'PO',ObjectId:" + po.PurchaseOrderId + "}";

                            if (!string.IsNullOrEmpty(Mob) && dltSMS!=null) { s.sendOtp(Mob, msg, dltSMS.DLTId); }

                            if (!string.IsNullOrEmpty(FCMID)) { s.SupplierSendNotification(FCMID, FCMNotification); }
                        }
                        string To = "", From = "", Bcc = "";
                        //var supplier = db.Suppliers.FirstOrDefault(x => x.SupplierId == POMaster.SupplierId);
                        To = supplier.EmailId;
                        if (!string.IsNullOrEmpty(To))
                        {
                            var Depo = db.DepoMasters.FirstOrDefault(x => x.DepoId == po.DepoId);
                            var ctnam = db.Warehouses.Where(x => x.WarehouseId == po.WarehouseId).Select(x => new { x.WarehouseName, x.Address, x.CityName, x.GSTin, x.Phone }).FirstOrDefault();
                            var buyeremail = db.Peoples.FirstOrDefault(x => x.PeopleID == po.BuyerId).Email;
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

                                    replacetext = $"<span style = 'float:left' class='ng-binding'>Invoice:{po.PoInvoiceNo}  </span> <br>" +
                                       $"<span style = 'float:left' class='ng-binding'>PO.No: {po.PurchaseOrderId} </span> <br>" +
                                       $"<span style = 'float:left' class='ng-binding'>Date: {po.CreationDate}</span> <br>" +
                                       $"<span style = 'float:left' class='ng-binding''>PO made by: {po.CreatedBy}</span> <br>" +
                                       $"<span style = 'float:left' class='ng-binding'>Buyer: {po.BuyerName}</span><br>" +
                                       $"<span style = 'float:left' class='ng-binding'>Picker Type: {po.PickerType}</span>";
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
                                                 $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'>{item.PurchaseOrderDetailId}</td>" +
                                                 $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'>{item.PurchaseSku}</td>" +
                                                 $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'></td>" +
                                                 $"<td width='300px' style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'><span style=' float:left;'>{item.ItemName}</span></td>" +
                                                 //$"<td >{item.Category}</td>" +
                                                 $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'>{item.HSNCode}</td>" +
                                                 $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'>{item.Price}</td>" +
                                                 $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'>{item.MOQ}</td>" +
                                                 $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'>{((item.TotalQuantity) / (item.MOQ))}</td>" +
                                                 $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'>{(item.MOQ * (item.TotalQuantity) / (item.MOQ))}</td>" +
                                                 $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'>{item.Weight + " " + item.WeightType}</td>" +
                                                 $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'>{(item.Weight * item.TotalQuantity) / 1000}&nbsp;Kg</td>" +
                                                 $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'><span style=' float:right;color: black;font-weight:bold; font-size:large;' >{(item.Price) * (item.TotalQuantity)}&nbsp;<i class='fa fa-inr'></i></span></td>" +
                                                 $"</tr>";
                                            totalPrice += (item.Price) * (item.TotalQuantity);
                                            totalWeight += item.Weight.HasValue ? ((item.Weight.Value * item.TotalQuantity) / 1000) : 0;
                                        }
                                        if (!string.IsNullOrEmpty(podetailhtml))
                                        {
                                            podetailhtml += $"<tr> <td colspan='11'> <span style='float:left'><strong>Grand Total:</strong></span></td><td>" +
                                                            $"<span style=' float:right;color: black;font-weight:bold; font-size:large;' class='ng-binding'>{totalPrice}&nbsp;<i class='fa fa-inr'></i></span>" +
                                                            $"</td></tr><tr>" +
                                                            $"<td colspan='11'> <span style='float:left'><strong>Total Weight:</strong></span></td><td>" +
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
                                thFileName = "PO_" + po.PurchaseOrderId + "_" + DateTime.Now.ToString("MMddyyyyhhmmss") + ".pdf";
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
                                string subject = DateTime.Now.ToString("dd MMM yyyy") + "Revised PO";
                                string message = " ShopKirana has raised a  New Revised PO " + po.PurchaseOrderId + " : for amount " + Math.Round(po.ETotalAmount, 2) + " : In Hub: " + po.WarehouseName;
                                if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                                {
                                     EmailHelper.SendMail(From, To, Bcc, subject, message, fullPhysicalPath);
                                    
                                }
                            }
                        }
                    }
                    return po;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
        }

        /// <summary>
        /// Approved PO by Reviewer
        /// if PO approved then send PO PDF to Supplier
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Authorize]
        [Route("ApprovedbyReviewer")]
        [HttpPut]
        public PurchaseOrderMaster ApprvedByReviewer(PurchaseOrderMaster data)
        {
            logger.Info("start Category: ");
            using (var db = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    People p = db.Peoples.Where(q => q.PeopleID == userid).FirstOrDefault();
                    PoApproval IsRevHere = db.PoApprovalDB.Where(a => a.Level == data.Level).FirstOrDefault();
                    PurchaseOrderMaster po = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == data.PurchaseOrderId).SingleOrDefault();
                    po.Status = "Approved";
                    po.progress = "50";
                    po.ApprovedBy = p.DisplayName;
                    db.Entry(po).State = EntityState.Modified;
                    db.Commit();
                    SendMailCreditWalletNotification(po.PurchaseOrderId);
                    return po;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
        }
        #region Function SendMail

        /// <summary>
        /// Reject po by reviewer
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Authorize]
        [Route("cmtbtapvl")]
        [HttpPut]
        public PurchaseOrderMaster CommentbyApprovar(PurchaseOrderMaster data)
        {
            logger.Info("start Category: ");
            using (var db = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    PoApproval IsRevHere = db.PoApprovalDB.Where(a => a.Level == data.Level).FirstOrDefault();
                    PurchaseOrderMaster po = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == data.PurchaseOrderId).SingleOrDefault();
                    //po.Status = "Rejected";
                    po.CommentApvl = data.CommentApvl;
                    db.Entry(po).State = EntityState.Modified;
                    db.Commit();
                    return po;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
        }


        /// <summary>
        /// Comment by reviewer  
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Authorize]
        [Route("RejectbyReviewer")]
        [HttpPut]
        public PurchaseOrderMaster RejectByReviewer(PurchaseOrderMaster data)
        {
            logger.Info("start Category: ");
            using (var db = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    PoApproval IsRevHere = db.PoApprovalDB.Where(a => a.Level == data.Level).FirstOrDefault();
                    PurchaseOrderMaster po = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == data.PurchaseOrderId).SingleOrDefault();
                    //po.Status = "Rejected";
                    po.Comment = data.Comment;
                    db.Entry(po).State = EntityState.Modified;
                    db.Commit();
                    return po;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
        }

        /// <summary>
        /// Reject po by Approver
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Authorize]
        [Route("RejectbyApprover")]
        [HttpPut]
        public PurchaseOrderMaster RejectByApprover(PurchaseOrderMaster data)
        {
            logger.Info("start Category: ");
            using (var db = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    People p = db.Peoples.Where(q => q.PeopleID == userid).FirstOrDefault();
                    PoApproval IsRevHere = db.PoApprovalDB.Where(a => a.Level == data.Level).FirstOrDefault();
                    PurchaseOrderMaster po = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == data.PurchaseOrderId && a.Status == "Send for Approval").SingleOrDefault();
                    if (po != null)
                    {
                        po.Status = "Rejected";
                        po.RejectedBy = p.DisplayName;
                        po.CommentApvl = data.CommentApvl;
                        db.Entry(po).State = EntityState.Modified;
                        db.Commit();
                    }
                    return po;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
        }


        /// <summary>
        /// //SendMail
        /// </summary>
        /// <param name="poid"></param>
        /// <param name="email"></param>
        public bool SendMailCreditWalletNotification(int poid)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["authcontext"].ConnectionString);
            con.Open();
            int ord = 0;
            int sid = 0;
            using (AuthContext db = new AuthContext())
            {
                Supplier sup = new Supplier();
                PurchaseOrderMaster pm = db.DPurchaseOrderMaster.Where(x => x.IsPDFsentGmail == false & x.PurchaseOrderId == poid).SingleOrDefault();
                ord = pm.PurchaseOrderId;
                sid = pm.SupplierId;

                if (sid != 0)
                {
                    sup = db.Suppliers.Where(a => a.SupplierId == sid).SingleOrDefault();
                }
                if (ord != 0)
                {
                    string masteremail = ConfigurationManager.AppSettings["MasterEmail"];
                    string masterpassword = ConfigurationManager.AppSettings["MasterPassword"];
                    string pdflocation = ConfigurationManager.AppSettings["PDFLocation"];
                    try
                    {
                        string body = "<div style='background: #FAFAFA; color: #333333; padding-left: 30px;font-family: arial,sans-serif; font-size: 14px;'>";
                        body += "<img style='padding-top: 10px;' src='http://shopkirana.com/wp-content/uploads/2015/07/ShopKirana-Logo11.png'><br/>";
                        body += "<h3 style='background-color: rgb(241, 89, 34);'>Alert! added more than 5000 point</h3>";
                        body += "Hello,";
                        body += "<p> Please find attechment of PO <strong>";
                        body += poid + "</strong>" + "below.</p>";
                        body += "Thanks,";
                        body += "<br />";
                        body += "<b>IT Team</b>";
                        body += "</div>";
                        var msg = new MailMessage(masteremail, masteremail, "Attechment of PO PDF. ", body);
                        msg.To.Add(sup.EmailId);
                        msg.IsBodyHtml = true;
                        System.Net.Mail.Attachment attachment;
                        attachment = new System.Net.Mail.Attachment(@"" + pdflocation + "" + poid + ".pdf");
                        msg.Attachments.Add(attachment);
                        var smtpClient = new SmtpClient("smtp.gmail.com", 587); //if your from email address is "from@hotmail.com" then host should be "smtp.hotmail.com"
                        smtpClient.UseDefaultCredentials = true;
                        smtpClient.Credentials = new NetworkCredential(masteremail, masterpassword);
                        smtpClient.EnableSsl = true;
                        smtpClient.Send(msg);
                        SqlCommand cmd2 = new SqlCommand("Update [PurchaseOrderMasters] set [IsPDFsentGmail] = 'True' where [PurchaseOrderId] = " + poid + "  ", con);
                        cmd2.ExecuteNonQuery();
                        return true;
                    }
                    catch (Exception ex) { return false; }
                }
                return false;
            }
        }
        #endregion

        /// <summary>
        /// Get All data fro po master
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("getAllHub")]
        public IEnumerable<PurchaseOrderMaster> GetAllHub()
        {
            logger.Info("start Category: ");
            List<PurchaseOrderMaster> ass = new List<PurchaseOrderMaster>();
            using (var db = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    List<Warehouse> wh = db.Warehouses.ToList();
                    ass = db.DPurchaseOrderMaster.Where(a => a.Approval1 == userid || a.Approval2 == userid || a.Approval3 == userid || a.Approval4 == userid || a.Approval5 == userid).OrderByDescending(a => a.PurchaseOrderId).ToList();

                    foreach (PurchaseOrderMaster a in ass)
                    {
                        a.WarehouseCity = wh.Where(q => q.WarehouseId == a.WarehouseId).Select(e => e.CityName).SingleOrDefault();
                    }

                    logger.Info("End  Category: ");
                    return ass.AsEnumerable().Where(a => a.Status == "Self Approved" || a.Status == "Send for Approval" || a.Status == "Rejected" || a.Status == "Send for Reviewer" || a.Status == "Approved").ToList();
                    //&& a.Rewiever1 == userid || a.Rewiever2 == userid || a.Rewiever3 == userid || a.Rewiever4 == userid || a.Rewiever5 == userid
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
        }
    }


}
