using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using AngularJSAuthentication.DataContracts.Transaction.supplier;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Base;
using AngularJSAuthentication.Model.PurchaseOrder;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/IR")]
    public class IRController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        #region Get Invoice image behalf of POID
        [Authorize]
        [Route("")]
        public HttpResponseMessage Get(int PurchaseOrderId)
        {
            using (var context = new AuthContext())
            {
                List<InvoiceImage> ass = new List<InvoiceImage>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

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
                    ass = context.InvoiceImageDb.Where(c => c.PurchaseOrderId == PurchaseOrderId && c.CompanyId == compid).ToList();
                    logger.Info("End  Return: ");
                    return Request.CreateResponse(HttpStatusCode.OK, ass);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        #endregion

        #region Get IR master detail Behalf of IRID
        /// <summary>
        /// created by 22/04/2019
        /// Get IR master Detail where IRID is this
        /// </summary>
        /// <param name="irid"></param>
        /// <returns></returns>
        [Authorize]
        [Route("getIRmaster")]
        public HttpResponseMessage getIRmaster(string irid)
        {
            using (var context = new AuthContext())
            {
                IRMaster ass = new IRMaster();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0; int WarehouseId = 0;

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
                        if (claim.Type == "Warehouseid")
                        {
                            WarehouseId = int.Parse(claim.Value);
                        }
                    }
                    ass = context.IRMasterDB.Where(c => c.IRID == irid).FirstOrDefault();

                    logger.Info("End  Return: ");
                    return Request.CreateResponse(HttpStatusCode.OK, ass);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        #endregion

        [Authorize]
        [Route("getIR")]
        public HttpResponseMessage getIR(int PurchaseOrderId)
        {
            using (var context = new AuthContext())
            {
                //InvoiceReceive ass = new InvoiceReceive();
                IRMaster ass = new IRMaster();

                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0; int WarehouseId = 0;

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
                        if (claim.Type == "Warehouseid")
                        {
                            WarehouseId = int.Parse(claim.Value);
                        }
                    }
                    ass = context.IRMasterDB.Where(q => q.PurchaseOrderId == PurchaseOrderId).FirstOrDefault();

                    if (ass != null)
                    {

                        ass.purDetails = context.IR_ConfirmDb.Where(c => c.PurchaseOrderId == ass.PurchaseOrderId).ToList();
                        if (ass.purDetails != null)
                        {
                            foreach (var a in ass.purDetails)
                            {
                                var item = context.ItemMasterCentralDB.Where(c => c.Number == a.ItemNumber && c.CompanyId == compid /*&& c.WarehouseId == WarehouseId*/).FirstOrDefault();
                                if (item != null)
                                {
                                    a.MRP = item.MRP;
                                    a.TotalTaxPercentage = item.TotalTaxPercentage;
                                    a.Qty = a.QtyRecived - a.IRQuantity;
                                }
                            }
                        }
                    }
                    logger.Info("End  Return: ");
                    return Request.CreateResponse(HttpStatusCode.OK, ass);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Authorize]
        [Route("getIRSupplier")]
        public HttpResponseMessage getIRSupplier(int id)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

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
                    var ir = context.InvoiceReceiveDb.Where(c => c.supplierId == id && c.CompanyId == compid).ToList();
                    List<IRGR> supplierIr = new List<IRGR>();
                    foreach (var spIr in ir)
                    {
                        try
                        {
                            var gr = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == spIr.PurchaseOrderId && x.SupplierId == spIr.supplierId && x.CompanyId == compid).SingleOrDefault();
                            IRGR ass = new IRGR();
                            if (gr != null)
                            {
                                ass.PurchaseOrderId = gr.PurchaseOrderId;
                                ass.GRAmount1 = gr.Gr1_Amount;
                                ass.GRAmount2 = gr.Gr2_Amount;
                                ass.GRAmount3 = gr.Gr3_Amount;
                                ass.GRAmount4 = gr.Gr4_Amount;
                                ass.GRAmount5 = gr.Gr5_Amount;
                                ass.GRTotal = gr.TotalAmount;
                                ass.IRAmount1 = spIr.IRAmount1WithTax;
                                ass.IRAmount2 = spIr.IRAmount2WithTax;
                                ass.IRAmount3 = spIr.IRAmount2WithTax;
                                ass.IRTotal = spIr.TotalAmount;

                                supplierIr.Add(ass);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.Message);
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, supplierIr);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        /// <summary>
        /// Post IR invoice by Backend 
        /// </summary>
        /// <param name="IR"></param>
        /// <returns></returns>
        [Route("add")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage add(List<InvoiceImage> IR) // by sachin
        {
            using (var context = new AuthContext())
            {
                logger.Info("start Return: ");
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

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
                    foreach (InvoiceImage IRR in IR)
                    {
                        if (IRR != null)
                        {
                            if (IRR.Id != 0)
                            {
                                InvoiceImage invoice = context.InvoiceImageDb.Where(x => x.Id == IRR.Id && x.CompanyId == compid).SingleOrDefault();
                                if (invoice != null)
                                {
                                    invoice.CompanyId = compid;
                                    invoice.IRAmount = Math.Round(IRR.IRAmount, 2);
                                    invoice.IRLogoURL = IRR.IRLogoURL;
                                    invoice.Remark = IRR.Remark;
                                    context.InvoiceImageDb.Attach(invoice);
                                    context.Entry(invoice).State = EntityState.Modified;
                                    context.Commit();
                                }
                                else
                                {
                                    IRR.CompanyId = compid;
                                    IRR.CreationDate = indianTime;
                                    context.InvoiceImageDb.Add(IRR);
                                    context.Commit();
                                }
                            }
                            else
                            {
                                IRR.CompanyId = compid;
                                IRR.CreationDate = indianTime;
                                context.InvoiceImageDb.Add(IRR);
                                context.Commit();
                            }
                            PurchaseOrderMaster pm = context.DPurchaseOrderMaster.Where(q => q.PurchaseOrderId == IRR.PurchaseOrderId).SingleOrDefault();
                            pm.IrStatus = "IR Uploaded";
                            //context.DPurchaseOrderMaster.Attach(pm);
                            context.Entry(pm).State = EntityState.Modified;
                            context.Commit();

                            IRMaster ir = context.IRMasterDB.Where(a => a.PurchaseOrderId == IRR.PurchaseOrderId && a.IRID == IRR.InvoiceNumber).SingleOrDefault();
                            ir.IRStatus = "IR Uploaded";
                            ir.Remark = IRR.Remark;
                            context.IRMasterDB.Attach(ir);
                            context.Entry(ir).State = EntityState.Modified;
                            context.Commit();
                        }

                        ///// ----------------------------Start----------------------------------/// 
                        ///// --Supplier Payment from upload Invoice recieved Amount and Reciept--// 
                        //try
                        //{
                        //    SupplierPaymentData spd = new SupplierPaymentData();
                        //    spd.PurchaseOrderId = IRR.PurchaseOrderId;
                        //    spd.InVoiceNumber = IRR.InvoiceNumber;
                        //    spd.CreditInVoiceAmount = IRR.IRAmount;
                        //    spd.active = false;
                        //    spd.CompanyId = compid;
                        //    spd.WarehouseId = IRR.WarehouseId;
                        //    spd.InVoiceDate = IRR.InvoiceDate;
                        //    spd.IRLogoURL = IRR.IRLogoURL;
                        //    spd.SupplierId = IRR.SupplierId;
                        //    spd.SupplierName = IRR.SupplierName;
                        //    spd.IRLogoURL = IRR.IRLogoURL;
                        //    spd.CreatedDate = DateTime.Now;
                        //    spd.UpdatedDate = DateTime.Now;
                        //    spd.PaymentStatusCorD = "Credit";
                        //    spd.Deleted = false;
                        //    spd.VoucherType = "Purchase";
                        //    spd.Perticular = IRR.Perticular;
                        //    var supplierpay = context.SupplierPaymentDataDB.Where(x => x.SupplierId == IRR.SupplierId && x.WarehouseId == IRR.WarehouseId && x.CompanyId == compid).ToList();
                        //    if (supplierpay.Count != 0)
                        //    {
                        //        List<SupplierPaymentData> maxTimestamp2 = context.SupplierPaymentDataDB.Where(x => x.SupplierId == IRR.SupplierId && x.WarehouseId == IRR.WarehouseId && x.CompanyId == compid).ToList();
                        //        SupplierPaymentData LastSupplierData = maxTimestamp2.LastOrDefault();                           
                        //        spd.ClosingBalance = LastSupplierData.ClosingBalance + IRR.IRAmount;
                        //    }
                        //    else
                        //    {
                        //        spd.ClosingBalance = IRR.IRAmount;
                        //    }
                        //    context.SupplierPaymentDataDB.Add(spd);
                        //    context.SaveChanges();

                        //    var SupplierPaymentData = context.FullSupplierPaymentDataDB.Where(x => x.SupplierId == IRR.SupplierId && x.CompanyId == compid && x.WarehouseId == IRR.WarehouseId).SingleOrDefault();
                        //    var supplier = context.Suppliers.Where(x => x.SupplierId == IRR.SupplierId /*&& x.WarehouseId == IRR.WarehouseId*/ && x.CompanyId == compid).SingleOrDefault();
                        //    if (SupplierPaymentData == null)
                        //    {
                        //        FullSupplierPaymentData dd = new FullSupplierPaymentData();
                        //        dd.InVoiceAmount = IRR.IRAmount;
                        //        dd.InVoiceRemainingAmount = IRR.IRAmount;
                        //        dd.active = true;
                        //        dd.CompanyId = compid;
                        //        dd.WarehouseId = IRR.WarehouseId;
                        //        dd.SupplierPaymentStatus = "UnPaid";
                        //        dd.SupplierPaymentTerms = supplier.PaymentTerms;
                        //        dd.SupplierId = IRR.SupplierId;
                        //        dd.SupplierName = supplier.Name;
                        //        dd.CreatedDate = DateTime.Now;
                        //        dd.UpdatedDate = DateTime.Now;
                        //        dd.Deleted = false;
                        //        context.FullSupplierPaymentDataDB.Add(dd);
                        //        context.SaveChanges();
                        //    }
                        //    else
                        //    {
                        //        FullSupplierPaymentData dd = context.FullSupplierPaymentDataDB.Where(x => x.SupplierId == IRR.SupplierId && x.CompanyId == compid && x.WarehouseId == IRR.WarehouseId).SingleOrDefault();
                        //        dd.InVoiceAmount = SupplierPaymentData.InVoiceAmount + IRR.IRAmount;
                        //        dd.InVoiceRemainingAmount = SupplierPaymentData.InVoiceRemainingAmount + IRR.IRAmount;
                        //        dd.SupplierPaymentStatus = SupplierPaymentData.SupplierPaymentStatus;
                        //        dd.UpdatedDate = DateTime.Now;
                        //        context.FullSupplierPaymentDataDB.Attach(dd);
                        //        context.Entry(dd).State = EntityState.Modified;
                        //        context.SaveChanges();
                        //    }
                        //}
                        //catch (Exception ex)
                        //{
                        //    return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
                        //}
                        /////-----------------------------End-----------------------------------///
                        break;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, IR);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        /// <summary>
        /// Post IR invoice by app 
        /// </summary>
        /// <param name="IRR"></param>
        /// <returns></returns>
        [Route("addByApp")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage add(InvoiceImage IRR)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start Return: ");
                try
                {
                    dtores res = new dtores();
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

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

                    PurchaseOrderMaster pm = new PurchaseOrderMaster();
                    IRMaster ir = new IRMaster();
                    ir = context.IRMasterDB.Where(a => a.PurchaseOrderId == IRR.PurchaseOrderId && a.IRID == IRR.InvoiceNumber).SingleOrDefault();

                    if (ir != null)
                    {
                        ir = context.IRMasterDB.Where(a => a.PurchaseOrderId == ir.PurchaseOrderId && a.Id == ir.Id).SingleOrDefault();
                        //InvoiceImage invoice = new InvoiceImage();
                        if (ir.IRStatus != "IR Uploaded")
                        {
                            foreach (IRImages IM in IRR.IRImages)
                            {
                                IRR.IRAmount = Math.Round(IRR.IRAmount, 2);
                                IRR.IRLogoURL = IM.IRLogoURL.Trim('"');
                                IRR.CompanyId = compid;
                                IRR.CreationDate = indianTime;
                                IRR.PurchaseOrderId = ir.PurchaseOrderId;
                                IRR.WarehouseId = ir.WarehouseId;
                                IRR.SupplierId = ir.supplierId;
                                IRR.SupplierName = ir.SupplierName;
                                context.InvoiceImageDb.Add(IRR);
                                context.Commit();
                            }

                            pm = context.DPurchaseOrderMaster.Where(q => q.PurchaseOrderId == IRR.PurchaseOrderId).SingleOrDefault();
                            pm.IrStatus = "IR Uploaded";
                            context.Entry(pm).State = EntityState.Modified;
                            ir = context.IRMasterDB.Where(a => a.PurchaseOrderId == IRR.PurchaseOrderId && a.IRID == IRR.InvoiceNumber).SingleOrDefault();
                            ir.IRStatus = "IR Uploaded";
                            context.Entry(ir).State = EntityState.Modified;
                            context.Commit();

                            res = new dtores()
                            {
                                IM = IRR,
                                Status = true,
                                Message = "Success."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        else
                        {
                            res = new dtores()
                            {
                                IM = null,
                                Status = false,
                                Message = "IR Already Uploded."
                            };

                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                    else
                    {
                        res = new dtores()
                        {
                            IM = null,
                            Status = false,
                            Message = "Failed."
                        };
                        return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                    }
                }
                catch (Exception ex)
                {
                    dtores res = new dtores()
                    {
                        IM = null,
                        Status = false,
                        Message = "Failed." + ex.Message.ToString()
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }
            }
        }
        /// <summary>
        /// Post IR OR Confirm IR
        /// </summary>
        /// <param name="pom"></param>
        /// <returns></returns>
        [Route("")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage confiirmIR(InvoiceReceive pom)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0, Warehouse_id = 0;
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                        Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                    var GRItemCount = context.PurchaseOrderRecivedDetails.Where(q => q.PurchaseOrderId == pom.PurchaseOrderId).Count();
                    if (GRItemCount == pom.purDetails.Count())
                    {
                        int? count = 0;
                        List<IRMaster> POValidate = context.IRMasterDB.Where(q => q.PurchaseOrderId == pom.PurchaseOrderId && q.IRID != null).ToList();
                        People peoplea = context.Peoples.Where(q => q.PeopleID == pom.BuyerId && q.Active == true).SingleOrDefault();
                        PurchaseOrderMaster pm = context.DPurchaseOrderMaster.Where(q => q.PurchaseOrderId == pom.PurchaseOrderId).SingleOrDefault();
                        People people = context.Peoples.Where(q => q.PeopleID == userid && q.Active == true).SingleOrDefault();
                        var supplierdata = context.Suppliers.Where(x => x.SupplierId == pm.SupplierId).SingleOrDefault();
                        int IRCount = POValidate.Where(q => q.IRType == pom.IRType).Count();
                        int IRIDCount = POValidate.Where(q => q.IRID == pom.TempIRID).Count();

                        if (IRCount == 0 && IRIDCount == 0)
                        {
                            PurchaseOrderMaster pmdetail = new PurchaseOrderMaster();
                            if (pom.BuyerId != 0 && pom.BuyerId != null)
                            {
                                pm.IrStatus = "IR Posted";
                                pm.BuyerId = peoplea.PeopleID;
                                pm.BuyerName = peoplea.DisplayName;
                                context.Entry(pm).State = EntityState.Modified;
                            }
                            IRMaster IRM = new IRMaster();
                            IRM.Gstamt = 0;
                            //foreach (var dd in pom.purDetails)
                            //{
                            //    IRM.TotalAmount += Math.Round(Convert.ToDouble(dd.TtlAmt), 2);
                            //    IRM.TotalAmountRemaining += Math.Round(Convert.ToDouble(dd.TtlAmt), 2);
                            //    IRM.IRAmountWithTax += Math.Round(Convert.ToDouble(dd.TtlAmt), 2);
                            //    IRM.Gstamt += Math.Round(Convert.ToDouble(dd.gstamt), 2);
                            //    IRM.IRAmountWithOutTax += Math.Round(Convert.ToDouble(dd.Price), 2);
                            //}
                            IRM.TotalAmount = Math.Round(Convert.ToDouble(pom.purDetails.Sum(a => a.TtlAmt)), 2);
                            IRM.TotalAmountRemaining = IRM.TotalAmount;
                            IRM.IRAmountWithTax = IRM.TotalAmount;
                            IRM.Gstamt = Math.Round(Convert.ToDouble(pom.purDetails.Sum(a => a.gstamt)), 2);
                            IRM.IRAmountWithOutTax = Math.Round(Convert.ToDouble(IRM.TotalAmount - IRM.Gstamt), 2);

                            if (pom.discount == null)
                            {
                                pom.discount = 0;
                            }
                            else
                            {
                                IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount - pom.discount), 2);
                                IRM.TotalAmountRemaining = Convert.ToDouble(IRM.TotalAmount);
                                IRM.IRAmountWithTax = Convert.ToDouble(IRM.TotalAmount);
                            }
                            if (pom.OtherAmountType == "ADD" && pom.OtherAmount != null)
                            {
                                IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount + pom.OtherAmount ?? 0), 2);
                            }
                            else if (pom.OtherAmountType == "MINUS" && pom.OtherAmount != null)
                            {
                                IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount - pom.OtherAmount ?? 0), 2);
                            }
                            if (pom.ExpenseAmountType == "ADD" && pom.ExpenseAmount != null)
                            {
                                IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount + pom.ExpenseAmount ?? 0), 2);
                            }
                            else if (pom.ExpenseAmountType == "MINUS" && pom.ExpenseAmount != null)
                            {
                                IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount - pom.ExpenseAmount ?? 0), 2);
                            }
                            if (pom.RoundoffAmountType == "ADD" && pom.RoundofAmount != null)
                            {
                                IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount + pom.RoundofAmount ?? 0), 2);
                            }
                            else if (pom.RoundoffAmountType == "MINUS" && pom.RoundofAmount != null)
                            {
                                IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount - pom.RoundofAmount ?? 0), 2);
                            }
                            IRM.Discount = Math.Round(Convert.ToDouble(pom.discount), 2);
                            IRM.IRStatus = "IR Posted";
                            IRM.PaymentStatus = "Unpaid";
                            IRM.PurchaseOrderId = pom.PurchaseOrderId;
                            IRM.supplierId = pom.supplierId;
                            IRM.SupplierName = pom.SupplierName;
                            IRM.BuyerId = pm.BuyerId;
                            IRM.BuyerName = pm.BuyerName;
                            IRM.WarehouseId = Convert.ToInt32(pom.WarehouseId);
                            IRM.CreationDate = DateTime.Now;
                            IRM.ExpenseAmount = pom.ExpenseAmount;
                            IRM.OtherAmount = pom.OtherAmount;
                            IRM.ExpenseAmountRemark = pom.ExpenseAmountRemark;
                            IRM.OtherAmountRemark = pom.OtherAmountRemark;
                            IRM.RoundofAmount = pom.RoundofAmount;
                            IRM.ExpenseAmountType = pom.ExpenseAmountType;
                            IRM.OtherAmountType = pom.OtherAmountType;
                            IRM.RoundoffAmountType = pom.RoundoffAmountType;
                            IRM.CreatedBy = people.DisplayName;
                            IRM.FreightAmount = pom.FreightAmount;
                            if (pom.DueDays > 0)
                            {
                                IRM.DueDays = pom.DueDays;
                            }
                            else
                            {
                                IRM.DueDays = 0;
                            }
                            switch (pom.IRType)  // Add IR detail behalf of IR type
                            {
                                case "IR1":
                                    IRM.IRID = pom.IR1ID;
                                    IRM.IRType = "IR1";
                                    pm.IRcount = 1;
                                    break;
                                case "IR2":
                                    IRM.IRID = pom.IR2ID;
                                    IRM.IRType = "IR2";
                                    pm.IRcount = 2;
                                    break;
                                case "IR3":
                                    IRM.IRID = pom.IR3ID;
                                    IRM.IRType = "IR3";
                                    pm.IRcount = 3;

                                    break;
                                case "IR4":
                                    IRM.IRID = pom.IR4ID;
                                    IRM.IRType = "IR4";
                                    pm.IRcount = 4;
                                    break;
                                case "IR5":
                                    IRM.IRID = pom.IR5ID;
                                    IRM.IRType = "IR5";
                                    pm.IRcount = 5;
                                    break;
                                default:
                                    break;
                            }

                            //foreach (var dd in pom.purDetails)
                            //{
                            //    if (dd.Qty != null)
                            //    {
                            //        //var InvoiceReceivedata = context.InvoiceReceiveDb.Where(x => x.PurchaseOrderId == dd.PurchaseOrderId && x.ItemId == dd.ItemId && x.CompanyId == dd.CompanyId).FirstOrDefault();

                            //        switch (pom.IRType)  // Add IR detail behalf of IR type
                            //        {
                            //            case "IR1":
                            //                //InvoiceReceive irdata = new InvoiceReceive();
                            //                //irdata.CompanyId = dd.CompanyId;
                            //                //irdata.PurchaseOrderId = dd.PurchaseOrderId;
                            //                //irdata.supplierId = dd.SupplierId;
                            //                //irdata.SupplierName = dd.SupplierName;
                            //                //irdata.WarehouseId = Convert.ToInt32(dd.WarehouseId);
                            //                //irdata.TotalAmount = Convert.ToDouble(dd.TtlAmt);
                            //                //irdata.Status = "Partial Received";
                            //                //irdata.PaymentStatus = "Unpaid";
                            //                //irdata.CreationDate = DateTime.Now;
                            //                //irdata.Deleted = false;
                            //                //irdata.gstamt = dd.gstamt;
                            //                //irdata.PaymentTerms = supplierdata.PaymentTerms;
                            //                //irdata.TotalAmountRemaining = Convert.ToDouble(dd.TtlAmt);
                            //                //irdata.IRAmount1WithTax = Convert.ToDouble(dd.TtlAmt);
                            //                //irdata.TotalTaxPercentage = dd.TotalTaxPercentage;
                            //                //irdata.ItemId = dd.ItemId;
                            //                //irdata.ItemName = dd.ItemName;
                            //                //irdata.irCount = 0;
                            //                //irdata.IR1ID = pom.IR1ID;
                            //                //irdata.IR2ID = pom.IR2ID;
                            //                //irdata.IR3ID = pom.IR3ID;
                            //                //irdata.IRAmount1WithOutTax = Convert.ToDouble(dd.Price1);
                            //                //context.InvoiceReceiveDb.Add(irdata);
                            //                IRM.IRID = pom.IR1ID;
                            //                IRM.IRType = "IR1";
                            //                pm.IRcount = 1;
                            //                break;
                            //            case "IR2":
                            //                //InvoiceReceivedata.IRAmount2WithTax = Convert.ToDouble(dd.TtlAmt);
                            //                //InvoiceReceivedata.irCount = 1;
                            //                //InvoiceReceivedata.TotalAmount = InvoiceReceivedata.TotalAmount + Convert.ToDouble(dd.TtlAmt);
                            //                //InvoiceReceivedata.TotalAmountRemaining = Math.Round(InvoiceReceivedata.TotalAmountRemaining + Convert.ToDouble(dd.TtlAmt));
                            //                //InvoiceReceivedata.PaymentStatus = InvoiceReceivedata.PaymentStatus;
                            //                //InvoiceReceivedata.IRAmount1WithOutTax = Convert.ToDouble(dd.Price2);
                            //                //InvoiceReceivedata.IR2ID = pom.IR2ID;
                            //                IRM.IRID = pom.IR2ID;
                            //                IRM.IRType = "IR2";
                            //                pm.IRcount = 2;
                            //                //context.InvoiceReceiveDb.Attach(InvoiceReceivedata);
                            //                //context.Entry(InvoiceReceivedata).State = EntityState.Modified;
                            //                break;
                            //            case "IR3":
                            //                //InvoiceReceivedata.IRAmount3WithTax = Convert.ToInt32(dd.TtlAmt);
                            //                //InvoiceReceivedata.irCount = 2;
                            //                //InvoiceReceivedata.TotalAmount = InvoiceReceivedata.TotalAmount + Convert.ToDouble(dd.TtlAmt);
                            //                //InvoiceReceivedata.TotalAmountRemaining = InvoiceReceivedata.TotalAmountRemaining + Convert.ToDouble(dd.TtlAmt);
                            //                //InvoiceReceivedata.PaymentStatus = InvoiceReceivedata.PaymentStatus;
                            //                //InvoiceReceivedata.IRAmount3WithOutTax = Convert.ToDouble(dd.Price3);
                            //                //InvoiceReceivedata.IR3ID = pom.IR3ID;
                            //                IRM.IRID = pom.IR3ID;
                            //                IRM.IRType = "IR3";
                            //                pm.IRcount = 3;
                            //                //context.InvoiceReceiveDb.Attach(InvoiceReceivedata);
                            //                //context.Entry(InvoiceReceivedata).State = EntityState.Modified;

                            //                break;
                            //            case "IR4":
                            //                //InvoiceReceivedata.IRAmount4WithTax = Convert.ToInt32(dd.TtlAmt);
                            //                //InvoiceReceivedata.irCount = 3;
                            //                //InvoiceReceivedata.TotalAmount = InvoiceReceivedata.TotalAmount + Convert.ToDouble(dd.TtlAmt);
                            //                //InvoiceReceivedata.TotalAmountRemaining = InvoiceReceivedata.TotalAmountRemaining + Convert.ToDouble(dd.TtlAmt);
                            //                //InvoiceReceivedata.PaymentStatus = InvoiceReceivedata.PaymentStatus;
                            //                //InvoiceReceivedata.IRAmount4WithOutTax = Convert.ToDouble(dd.Price4);
                            //                //InvoiceReceivedata.IR4ID = pom.IR4ID;
                            //                IRM.IRID = pom.IR4ID;
                            //                IRM.IRType = "IR4";
                            //                pm.IRcount = 4;
                            //                //context.InvoiceReceiveDb.Attach(InvoiceReceivedata);
                            //                //context.Entry(InvoiceReceivedata).State = EntityState.Modified;
                            //                break;
                            //            case "IR5":
                            //                //InvoiceReceivedata.IRAmount4WithTax = Convert.ToInt32(dd.TtlAmt);
                            //                //InvoiceReceivedata.irCount = 4;
                            //                //InvoiceReceivedata.TotalAmount = InvoiceReceivedata.TotalAmount + Convert.ToDouble(dd.TtlAmt);
                            //                //InvoiceReceivedata.TotalAmountRemaining = InvoiceReceivedata.TotalAmountRemaining + Convert.ToDouble(dd.TtlAmt);
                            //                //InvoiceReceivedata.PaymentStatus = InvoiceReceivedata.PaymentStatus;
                            //                //InvoiceReceivedata.IRAmount5WithOutTax = Convert.ToDouble(dd.Price5);
                            //                //InvoiceReceivedata.IR5ID = pom.IR5ID;
                            //                IRM.IRID = pom.IR5ID;
                            //                IRM.IRType = "IR5";
                            //                pm.IRcount = 5;
                            //                //context.InvoiceReceiveDb.Attach(InvoiceReceivedata);
                            //                //context.Entry(InvoiceReceivedata).State = EntityState.Modified;
                            //                break;
                            //            default:
                            //                break;
                            //        }
                            //    }
                            //}

                            context.IRMasterDB.Add(IRM);
                            //double dics = 0;
                            count = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == pom.PurchaseOrderId).Select(a => a.IRcount).SingleOrDefault();
                            List<IR_Confirm> p = pom.purDetails;
                            foreach (IR_Confirm pc in p)
                            {
                                try
                                {
                                    if (pc.Qty != null)
                                    {
                                        ItemMaster HSNcode = context.itemMasters.Where(a => a.Number == pc.ItemNumber && a.WarehouseId == pm.WarehouseId).FirstOrDefault();

                                        if (pc.discount > 0)
                                        {
                                        }
                                        else
                                        {
                                            pc.discount = 0;
                                        }
                                        var irconfirm = context.IR_ConfirmDb.Where(x => x.PurchaseOrderId == pc.PurchaseOrderId && x.ItemNumber == pc.ItemNumber && x.CompanyId == compid).SingleOrDefault();
                                        if (irconfirm != null)
                                        {
                                            if (count == null)
                                            {
                                                count = pm.IRcount - 1;
                                            }

                                            if (count == 1)
                                            {
                                                irconfirm.Ir2PersonId = people.PeopleID;
                                                irconfirm.Ir2PersonName = people.DisplayName;
                                                irconfirm.Ir2Date = indianTime;
                                                irconfirm.IR2ID = pom.IR2ID;
                                                pc.distype2 = pc.distype;
                                                pc.DesA2 = pc.DesA;
                                                pc.DesP2 = pc.DesP;
                                                if (pc.Qty > 0)
                                                {
                                                    irconfirm.IRQuantity = irconfirm.IRQuantity + pc.Qty;
                                                    irconfirm.Status = pc.Status;
                                                    irconfirm.QtyRecived2 = pc.Qty;
                                                    irconfirm.PriceRecived = irconfirm.PriceRecived + pc.PriceRecived;
                                                    irconfirm.Price2 = pc.Price;
                                                    irconfirm.dis2 = pc.discount;
                                                }
                                                else
                                                {
                                                    irconfirm.Price2 = 0;
                                                    irconfirm.dis2 = 0;
                                                }
                                                //dics += irconfirm.dis2.GetValueOrDefault();
                                                //ir.discount2 = pom.discount; //+ dics;
                                                //ir.irCount = 1;
                                                //ir.IRAmount2WithTax = pom.TotalAmount;
                                            }
                                            else if (count == 2)
                                            {
                                                irconfirm.Ir3PersonId = pc.Ir1PersonId;
                                                irconfirm.Ir3PersonName = people.DisplayName;
                                                irconfirm.Ir3Date = indianTime;
                                                irconfirm.IR3ID = pom.IR3ID;
                                                pc.distype3 = pc.distype;
                                                pc.DesA3 = pc.DesA;
                                                pc.DesP3 = pc.DesP;
                                                if (pc.Qty > 0)
                                                {
                                                    irconfirm.PriceRecived = irconfirm.PriceRecived + pc.PriceRecived;
                                                    irconfirm.IRQuantity = irconfirm.IRQuantity + pc.Qty;
                                                    irconfirm.Status = pc.Status;
                                                    irconfirm.QtyRecived3 = pc.Qty;
                                                    irconfirm.Price3 = pc.Price;
                                                    irconfirm.dis3 = pc.discount;
                                                }
                                                else
                                                {
                                                    irconfirm.Price3 = 0;
                                                    irconfirm.dis3 = 0;
                                                }
                                                //dics += irconfirm.dis3.GetValueOrDefault();
                                                //ir.discount3 = pom.discount; //+ dics;
                                                //ir.irCount = 2;
                                                //ir.IRAmount3WithTax = pom.TotalAmount;

                                            }
                                            else if (count == 3)
                                            {
                                                irconfirm.Ir4PersonId = pc.Ir4PersonId;
                                                irconfirm.Ir4PersonName = people.DisplayName;
                                                irconfirm.Ir4Date = indianTime;
                                                irconfirm.IR4ID = pom.IR4ID;
                                                pc.distype4 = pc.distype;
                                                pc.DesA4 = pc.DesA;
                                                pc.DesP4 = pc.DesP;
                                                if (pc.Qty > 0)
                                                {
                                                    irconfirm.PriceRecived = irconfirm.PriceRecived + pc.PriceRecived;
                                                    irconfirm.IRQuantity = irconfirm.IRQuantity + pc.Qty;
                                                    irconfirm.Status = pc.Status;
                                                    irconfirm.QtyRecived4 = pc.Qty;
                                                    irconfirm.Price4 = pc.Price;
                                                    irconfirm.dis4 = pc.discount;
                                                }
                                                else
                                                {
                                                    irconfirm.Price4 = 0;
                                                    irconfirm.dis4 = 0;
                                                }
                                                //dics += irconfirm.dis4.GetValueOrDefault();
                                                //ir.discount4 = pom.discount; //+ dics;
                                                //ir.irCount = 3;
                                                //ir.IRAmount4WithTax = pom.TotalAmount;
                                            }
                                            else if (count == 4)
                                            {
                                                irconfirm.Ir5PersonId = pc.Ir5PersonId;
                                                irconfirm.Ir5PersonName = people.DisplayName;
                                                irconfirm.Ir5Date = indianTime;
                                                irconfirm.IR5ID = pom.IR5ID;
                                                pc.distype5 = pc.distype;
                                                pc.DesA5 = pc.DesA;
                                                pc.DesP5 = pc.DesP;
                                                if (pc.Qty > 0)
                                                {
                                                    irconfirm.PriceRecived = irconfirm.PriceRecived + pc.PriceRecived;
                                                    irconfirm.IRQuantity = irconfirm.IRQuantity + pc.Qty;
                                                    irconfirm.Status = pc.Status;
                                                    irconfirm.QtyRecived5 = pc.Qty;
                                                    irconfirm.Price5 = pc.Price;
                                                    irconfirm.dis5 = pc.discount;
                                                }
                                                else
                                                {
                                                    irconfirm.Price5 = 0;
                                                    irconfirm.dis5 = 0;
                                                }
                                                //dics += irconfirm.dis5.GetValueOrDefault();
                                                //ir.discount5 = pom.discount; //+ dics;
                                                //ir.irCount = 4;
                                                //ir.IRAmount5WithTax = pom.TotalAmount;
                                            }

                                            context.IR_ConfirmDb.Attach(irconfirm);
                                            context.Entry(irconfirm).State = EntityState.Modified;

                                        }
                                        else
                                        {
                                            // pc.PurchaseOrderDetailId = ir.Id;  // wrong PurchaseOrderDetailId inserting.
                                            if (pc.Qty > 0)
                                            {
                                                pc.IRQuantity = pc.Qty;
                                            }
                                            else
                                            {
                                                pc.IRQuantity = 0;
                                            }
                                            pc.QtyRecived1 = pc.IRQuantity;
                                            pc.Ir1Date = indianTime;
                                            pc.Ir1PersonId = people.PeopleID;
                                            pc.Ir1PersonName = people.DisplayName;
                                            pc.IR1ID = pom.IR1ID;
                                            pc.distype1 = pc.distype;
                                            pc.DesA1 = pc.DesA;
                                            pc.DesP1 = pc.DesP;
                                            if (pc.QtyRecived1 > 0)
                                            {
                                                pc.Price1 = pc.Price;
                                                if (pc.discount > 0)
                                                {
                                                    pc.dis1 = pc.discount;
                                                }
                                                else
                                                {
                                                    pc.dis1 = 0;
                                                }

                                            }
                                            else
                                            {
                                                pc.QtyRecived1 = 0;
                                                pc.Price1 = 0;
                                                pc.dis1 = 0;
                                            }
                                            pc.QtyRecived2 = 0;
                                            pc.Price2 = 0;
                                            pc.dis2 = 0;
                                            pc.QtyRecived3 = 0;
                                            pc.Price3 = 0;
                                            pc.dis3 = 0;
                                            pc.HSNCode = HSNcode.HSNCode ?? null;
                                            pc.CreationDate = indianTime;
                                            context.IR_ConfirmDb.Add(pc);
                                            pom.irCount = 1;
                                            //ir.IRAmount1WithTax = pom.TotalAmount;
                                            //ir.IRAmount2WithTax = 0;
                                            //ir.IRAmount2WithTax = 0;
                                            //dics += pc.dis1.GetValueOrDefault();
                                            //ir.discount1 = pom.discount; //+ dics; 
                                        }
                                    }
                                }
                                catch (Exception ee)
                                {
                                    logger.Error(ee.Message);
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "got Excepion");
                                }
                            }
                            context.Commit();
                            return Request.CreateResponse(HttpStatusCode.OK, pom);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Validation- IR id already exist.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Validation- GR and IR Item count are mismatch,\nSo please try again after refresh the page.");
                    }
                }
                catch (Exception exe)
                {
                    Console.Write(exe.Message);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Got Excepion- " + exe.Message);
                }
            }
        }

        /// <summary>
        /// Post IR OR Confirm IR as a drafted
        /// created by 27/04/2019
        /// </summary>
        /// <param name="pom"></param>
        /// <returns></returns>
        [Route("IrpostedDraft")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage confiirmIRDrafted(InvoiceReceive pom) // by sachin
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0, Warehouse_id = 0;
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                        Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                    var GRItemCount = context.PurchaseOrderRecivedDetails.Where(q => q.PurchaseOrderId == pom.PurchaseOrderId).Count();
                    if (GRItemCount == pom.purDetails.Count())
                    {
                        int? count = 0;
                        List<IRMaster> POValidate = context.IRMasterDB.Where(q => q.PurchaseOrderId == pom.PurchaseOrderId && q.IRID != null).ToList();
                        People peoplea = context.Peoples.Where(q => q.PeopleID == pom.BuyerId && q.Active == true).SingleOrDefault();
                        PurchaseOrderMaster pm = context.DPurchaseOrderMaster.Where(q => q.PurchaseOrderId == pom.PurchaseOrderId).SingleOrDefault();
                        People people = context.Peoples.Where(q => q.PeopleID == userid && q.Active == true).SingleOrDefault();
                        var supplierdata = context.Suppliers.Where(x => x.SupplierId == pm.SupplierId).SingleOrDefault();
                        int IRCount = POValidate.Where(q => q.IRType == pom.IRType).Count();
                        int IRIDCount = POValidate.Where(q => q.IRID == pom.TempIRID).Count();

                        if (IRCount == 0 && IRIDCount == 0)
                        {
                            PurchaseOrderMaster pmdetail = new PurchaseOrderMaster();
                            if (pom.BuyerId != 0 && pom.BuyerId != null)
                            {
                                pm.IrStatus = "IR Posted as a Draft";
                                pm.BuyerId = peoplea.PeopleID;
                                pm.BuyerName = peoplea.DisplayName;
                                //context.DPurchaseOrderMaster.Attach(pm);
                                context.Entry(pm).State = EntityState.Modified;
                            }
                            IRMaster IRM = new IRMaster();
                            IRM.Gstamt = 0;

                            foreach (var dd in pom.purDetails)
                            {
                                IRM.TotalAmount += Math.Round(Convert.ToDouble(dd.TtlAmt), 2);
                                IRM.TotalAmountRemaining += Math.Round(Convert.ToDouble(dd.TtlAmt), 2);
                                IRM.IRAmountWithTax += Math.Round(Convert.ToDouble(dd.TtlAmt), 2);
                                IRM.Gstamt += Math.Round(Convert.ToDouble(dd.gstamt), 2);
                                IRM.IRAmountWithOutTax += Math.Round(Convert.ToDouble(dd.Price), 2);
                            }

                            if (pom.discount == null)
                            {
                                pom.discount = 0;
                            }
                            else
                            {
                                IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount - pom.discount), 2);
                                IRM.TotalAmountRemaining = Convert.ToDouble(IRM.TotalAmount);
                                IRM.IRAmountWithTax = Convert.ToDouble(IRM.TotalAmount);
                            }
                            try
                            {
                                if (pom.OtherAmountType == "ADD" && pom.OtherAmount != null)
                                {
                                    IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount + pom.OtherAmount ?? 0), 2);
                                }
                                else if (pom.OtherAmountType == "MINUS" && pom.OtherAmount != null)
                                {
                                    IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount - pom.OtherAmount ?? 0), 2);
                                }
                                if (pom.ExpenseAmountType == "ADD" && pom.ExpenseAmount != null)
                                {
                                    IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount + pom.ExpenseAmount ?? 0), 2);
                                }
                                else if (pom.ExpenseAmountType == "MINUS" && pom.ExpenseAmount != null)
                                {
                                    IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount - pom.ExpenseAmount ?? 0), 2);
                                }
                                if (pom.RoundoffAmountType == "ADD" && pom.RoundofAmount != null)
                                {
                                    IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount + pom.RoundofAmount ?? 0), 2);
                                }
                                else if (pom.RoundoffAmountType == "MINUS" && pom.RoundofAmount != null)
                                {
                                    IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount - pom.RoundofAmount ?? 0), 2);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Info(ex.Message);
                            }

                            IRM.Discount = Math.Round(Convert.ToDouble(pom.discount), 2);
                            IRM.IRStatus = "IR Posted as a Draft";
                            IRM.PaymentStatus = "Unpaid";
                            IRM.PurchaseOrderId = pom.PurchaseOrderId;
                            IRM.supplierId = pom.supplierId;
                            IRM.SupplierName = pom.SupplierName;
                            IRM.BuyerId = pm.BuyerId;
                            IRM.BuyerName = pm.BuyerName;
                            IRM.WarehouseId = Convert.ToInt32(pom.WarehouseId);
                            IRM.CreationDate = DateTime.Now;
                            IRM.ExpenseAmount = pom.ExpenseAmount;
                            IRM.OtherAmount = pom.OtherAmount;
                            IRM.ExpenseAmountRemark = pom.ExpenseAmountRemark;
                            IRM.OtherAmountRemark = pom.OtherAmountRemark;
                            IRM.RoundofAmount = pom.RoundofAmount;
                            IRM.ExpenseAmountType = pom.ExpenseAmountType;
                            IRM.OtherAmountType = pom.OtherAmountType;
                            IRM.RoundoffAmountType = pom.RoundoffAmountType;
                            IRM.CreatedBy = people.DisplayName;

                            if (pom.DueDays > 0)
                            {
                                IRM.DueDays = pom.DueDays;
                            }
                            else
                            {
                                IRM.DueDays = 0;
                            }
                            List<IR_Confirm> p = pom.purDetails;
                            foreach (var dd in pom.purDetails)
                            {
                                if (dd.Qty != null)
                                {
                                    //var InvoiceReceivedata = context.InvoiceReceiveDb.Where(x => x.PurchaseOrderId == dd.PurchaseOrderId && x.ItemId == dd.ItemId && x.CompanyId == dd.CompanyId).FirstOrDefault();

                                    switch (pom.IRType)  // Add IR detail behalf of IR type
                                    {
                                        case "IR1":
                                            //InvoiceReceive irdata = new InvoiceReceive();
                                            //irdata.CompanyId = dd.CompanyId;
                                            //irdata.PurchaseOrderId = dd.PurchaseOrderId;
                                            //irdata.supplierId = dd.SupplierId;
                                            //irdata.SupplierName = dd.SupplierName;
                                            //irdata.WarehouseId = Convert.ToInt32(dd.WarehouseId);
                                            //irdata.TotalAmount = Convert.ToDouble(dd.TtlAmt);
                                            //irdata.Status = "Partial Received";
                                            //irdata.PaymentStatus = "Unpaid";
                                            //irdata.CreationDate = DateTime.Now;
                                            //irdata.Deleted = false;
                                            //irdata.gstamt = dd.gstamt;
                                            //irdata.PaymentTerms = supplierdata.PaymentTerms;
                                            //irdata.TotalAmountRemaining = Convert.ToDouble(dd.TtlAmt);
                                            //irdata.IRAmount1WithTax = Convert.ToDouble(dd.TtlAmt);
                                            //irdata.TotalTaxPercentage = dd.TotalTaxPercentage;
                                            //irdata.ItemId = dd.ItemId;
                                            //irdata.ItemName = dd.ItemName;
                                            //irdata.irCount = 0;
                                            //irdata.IR1ID = pom.IR1ID;
                                            //irdata.IR2ID = pom.IR2ID;
                                            //irdata.IR3ID = pom.IR3ID;
                                            //irdata.IRAmount1WithOutTax = Convert.ToDouble(dd.Price1);
                                            //context.InvoiceReceiveDb.Add(irdata);
                                            IRM.IRID = pom.IR1ID;
                                            IRM.IRType = "IR1";
                                            pm.IRcount = 1;
                                            break;
                                        case "IR2":
                                            //InvoiceReceivedata.IRAmount2WithTax = Convert.ToDouble(dd.TtlAmt);
                                            //InvoiceReceivedata.irCount = 1;
                                            //InvoiceReceivedata.TotalAmount = InvoiceReceivedata.TotalAmount + Convert.ToDouble(dd.TtlAmt);
                                            //InvoiceReceivedata.TotalAmountRemaining = Math.Round(InvoiceReceivedata.TotalAmountRemaining + Convert.ToDouble(dd.TtlAmt));
                                            //InvoiceReceivedata.PaymentStatus = InvoiceReceivedata.PaymentStatus;
                                            //InvoiceReceivedata.IRAmount1WithOutTax = Convert.ToDouble(dd.Price2);
                                            //InvoiceReceivedata.IR2ID = pom.IR2ID;
                                            IRM.IRID = pom.IR2ID;
                                            IRM.IRType = "IR2";
                                            pm.IRcount = 2;
                                            //context.InvoiceReceiveDb.Attach(InvoiceReceivedata);
                                            //context.Entry(InvoiceReceivedata).State = EntityState.Modified;
                                            break;
                                        case "IR3":
                                            //InvoiceReceivedata.IRAmount3WithTax = Convert.ToInt32(dd.TtlAmt);
                                            //InvoiceReceivedata.irCount = 2;
                                            //InvoiceReceivedata.TotalAmount = InvoiceReceivedata.TotalAmount + Convert.ToDouble(dd.TtlAmt);
                                            //InvoiceReceivedata.TotalAmountRemaining = InvoiceReceivedata.TotalAmountRemaining + Convert.ToDouble(dd.TtlAmt);
                                            //InvoiceReceivedata.PaymentStatus = InvoiceReceivedata.PaymentStatus;
                                            //InvoiceReceivedata.IRAmount3WithOutTax = Convert.ToDouble(dd.Price3);
                                            //InvoiceReceivedata.IR3ID = pom.IR3ID;
                                            IRM.IRID = pom.IR3ID;
                                            IRM.IRType = "IR3";
                                            pm.IRcount = 3;
                                            //context.InvoiceReceiveDb.Attach(InvoiceReceivedata);
                                            //context.Entry(InvoiceReceivedata).State = EntityState.Modified;

                                            break;
                                        case "IR4":
                                            //InvoiceReceivedata.IRAmount4WithTax = Convert.ToInt32(dd.TtlAmt);
                                            //InvoiceReceivedata.irCount = 3;
                                            //InvoiceReceivedata.TotalAmount = InvoiceReceivedata.TotalAmount + Convert.ToDouble(dd.TtlAmt);
                                            //InvoiceReceivedata.TotalAmountRemaining = InvoiceReceivedata.TotalAmountRemaining + Convert.ToDouble(dd.TtlAmt);
                                            //InvoiceReceivedata.PaymentStatus = InvoiceReceivedata.PaymentStatus;
                                            //InvoiceReceivedata.IRAmount4WithOutTax = Convert.ToDouble(dd.Price4);
                                            //InvoiceReceivedata.IR4ID = pom.IR4ID;
                                            IRM.IRID = pom.IR4ID;
                                            IRM.IRType = "IR4";
                                            pm.IRcount = 4;
                                            //context.InvoiceReceiveDb.Attach(InvoiceReceivedata);
                                            //context.Entry(InvoiceReceivedata).State = EntityState.Modified;
                                            break;
                                        case "IR5":
                                            //InvoiceReceivedata.IRAmount4WithTax = Convert.ToInt32(dd.TtlAmt);
                                            //InvoiceReceivedata.irCount = 4;
                                            //InvoiceReceivedata.TotalAmount = InvoiceReceivedata.TotalAmount + Convert.ToDouble(dd.TtlAmt);
                                            //InvoiceReceivedata.TotalAmountRemaining = InvoiceReceivedata.TotalAmountRemaining + Convert.ToDouble(dd.TtlAmt);
                                            //InvoiceReceivedata.PaymentStatus = InvoiceReceivedata.PaymentStatus;
                                            //InvoiceReceivedata.IRAmount5WithOutTax = Convert.ToDouble(dd.Price5);
                                            //InvoiceReceivedata.IR5ID = pom.IR5ID;
                                            IRM.IRID = pom.IR5ID;
                                            IRM.IRType = "IR5";
                                            pm.IRcount = 5;
                                            //context.InvoiceReceiveDb.Attach(InvoiceReceivedata);
                                            //context.Entry(InvoiceReceivedata).State = EntityState.Modified;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            context.IRMasterDB.Add(IRM);
                            //double dics = 0;
                            foreach (IR_Confirm pc in p)
                            {
                                try
                                {
                                    if (pc.Qty != null)
                                    {
                                        ItemMaster HSNcode = context.itemMasters.Where(a => a.Number == pc.ItemNumber && a.WarehouseId == pm.WarehouseId).FirstOrDefault();
                                        count = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == pom.PurchaseOrderId).Select(a => a.IRcount).SingleOrDefault();

                                        if (pc.discount > 0)
                                        {
                                        }
                                        else
                                        {
                                            pc.discount = 0;
                                        }
                                        var irconfirm = context.IR_ConfirmDb.Where(x => x.PurchaseOrderId == pc.PurchaseOrderId && x.ItemNumber == pc.ItemNumber && x.CompanyId == compid).SingleOrDefault();
                                        if (irconfirm != null)
                                        {
                                            if (count == 1)
                                            {
                                                irconfirm.Ir2PersonId = people.PeopleID;
                                                irconfirm.Ir2PersonName = people.DisplayName;
                                                irconfirm.Ir2Date = indianTime;
                                                irconfirm.IR2ID = pom.IR2ID;
                                                pc.distype2 = pc.distype;
                                                pc.DesA2 = pc.DesA;
                                                pc.DesP2 = pc.DesP;
                                                if (pc.Qty > 0)
                                                {
                                                    irconfirm.IRQuantity = irconfirm.IRQuantity + pc.Qty;
                                                    irconfirm.Status = pc.Status;
                                                    irconfirm.QtyRecived2 = pc.Qty;
                                                    irconfirm.PriceRecived = irconfirm.PriceRecived + pc.PriceRecived;
                                                    irconfirm.Price2 = pc.Price;
                                                    irconfirm.dis2 = pc.discount;
                                                }
                                                else
                                                {
                                                    irconfirm.Price2 = 0;
                                                    irconfirm.dis2 = 0;
                                                }
                                                //dics += irconfirm.dis2.GetValueOrDefault();
                                                //ir.discount2 = pom.discount; //+ dics;
                                                //ir.irCount = 1;
                                                //ir.IRAmount2WithTax = pom.TotalAmount;
                                            }
                                            else if (count == 2)
                                            {
                                                irconfirm.Ir3PersonId = pc.Ir1PersonId;
                                                irconfirm.Ir3PersonName = people.DisplayName;
                                                irconfirm.Ir3Date = indianTime;
                                                irconfirm.IR3ID = pom.IR3ID;
                                                pc.distype3 = pc.distype;
                                                pc.DesA3 = pc.DesA;
                                                pc.DesP3 = pc.DesP;
                                                if (pc.Qty > 0)
                                                {
                                                    irconfirm.PriceRecived = irconfirm.PriceRecived + pc.PriceRecived;
                                                    irconfirm.IRQuantity = irconfirm.IRQuantity + pc.Qty;
                                                    irconfirm.Status = pc.Status;
                                                    irconfirm.QtyRecived3 = pc.Qty;
                                                    irconfirm.Price3 = pc.Price;
                                                    irconfirm.dis3 = pc.discount;
                                                }
                                                else
                                                {
                                                    irconfirm.Price3 = 0;
                                                    irconfirm.dis3 = 0;
                                                }
                                                //dics += irconfirm.dis3.GetValueOrDefault();
                                                //ir.discount3 = pom.discount; //+ dics;
                                                //ir.irCount = 2;
                                                //ir.IRAmount3WithTax = pom.TotalAmount;

                                            }
                                            else if (count == 3)
                                            {
                                                irconfirm.Ir4PersonId = pc.Ir4PersonId;
                                                irconfirm.Ir4PersonName = people.DisplayName;
                                                irconfirm.Ir4Date = indianTime;
                                                irconfirm.IR4ID = pom.IR4ID;
                                                pc.distype4 = pc.distype;
                                                pc.DesA4 = pc.DesA;
                                                pc.DesP4 = pc.DesP;
                                                if (pc.Qty > 0)
                                                {
                                                    irconfirm.PriceRecived = irconfirm.PriceRecived + pc.PriceRecived;
                                                    irconfirm.IRQuantity = irconfirm.IRQuantity + pc.Qty;
                                                    irconfirm.Status = pc.Status;
                                                    irconfirm.QtyRecived4 = pc.Qty;
                                                    irconfirm.Price4 = pc.Price;
                                                    irconfirm.dis4 = pc.discount;
                                                }
                                                else
                                                {
                                                    irconfirm.Price4 = 0;
                                                    irconfirm.dis4 = 0;
                                                }
                                                //dics += irconfirm.dis4.GetValueOrDefault();
                                                //ir.discount4 = pom.discount; //+ dics;
                                                //ir.irCount = 3;
                                                //ir.IRAmount4WithTax = pom.TotalAmount;
                                            }
                                            else if (count == 4)
                                            {
                                                irconfirm.Ir5PersonId = pc.Ir5PersonId;
                                                irconfirm.Ir5PersonName = people.DisplayName;
                                                irconfirm.Ir5Date = indianTime;
                                                irconfirm.IR5ID = pom.IR5ID;
                                                pc.distype5 = pc.distype;
                                                pc.DesA5 = pc.DesA;
                                                pc.DesP5 = pc.DesP;
                                                if (pc.Qty > 0)
                                                {
                                                    irconfirm.PriceRecived = irconfirm.PriceRecived + pc.PriceRecived;
                                                    irconfirm.IRQuantity = irconfirm.IRQuantity + pc.Qty;
                                                    irconfirm.Status = pc.Status;
                                                    irconfirm.QtyRecived5 = pc.Qty;
                                                    irconfirm.Price5 = pc.Price;
                                                    irconfirm.dis5 = pc.discount;
                                                }
                                                else
                                                {
                                                    irconfirm.Price5 = 0;
                                                    irconfirm.dis5 = 0;
                                                }
                                                //dics += irconfirm.dis5.GetValueOrDefault();
                                                //ir.discount5 = pom.discount; //+ dics;
                                                //ir.irCount = 4;
                                                //ir.IRAmount5WithTax = pom.TotalAmount;
                                            }

                                            context.IR_ConfirmDb.Attach(irconfirm);
                                            context.Entry(irconfirm).State = EntityState.Modified;

                                        }
                                        else
                                        {
                                            // pc.PurchaseOrderDetailId = ir.Id;  // wrong PurchaseOrderDetailId inserting.
                                            if (pc.Qty > 0)
                                            {
                                                pc.IRQuantity = pc.Qty;
                                            }
                                            else
                                            {
                                                pc.IRQuantity = 0;
                                            }
                                            pc.QtyRecived1 = pc.IRQuantity;
                                            pc.Ir1Date = indianTime;
                                            pc.Ir1PersonId = people.PeopleID;
                                            pc.Ir1PersonName = people.DisplayName;
                                            pc.IR1ID = pom.IR1ID;
                                            pc.distype1 = pc.distype;
                                            pc.DesA1 = pc.DesA;
                                            pc.DesP1 = pc.DesP;
                                            if (pc.QtyRecived1 > 0)
                                            {
                                                pc.Price1 = pc.Price;
                                                if (pc.discount > 0)
                                                {
                                                    pc.dis1 = pc.discount;
                                                }
                                                else
                                                {
                                                    pc.dis1 = 0;
                                                }

                                            }
                                            else
                                            {
                                                pc.QtyRecived1 = 0;
                                                pc.Price1 = 0;
                                                pc.dis1 = 0;
                                            }
                                            pc.QtyRecived2 = 0;
                                            pc.Price2 = 0;
                                            pc.dis2 = 0;
                                            pc.QtyRecived3 = 0;
                                            pc.Price3 = 0;
                                            pc.dis3 = 0;
                                            pc.HSNCode = HSNcode.HSNCode ?? null;
                                            pc.CreationDate = indianTime;
                                            context.IR_ConfirmDb.Add(pc);
                                            pom.irCount = 1;
                                            //ir.IRAmount1WithTax = pom.TotalAmount;
                                            //ir.IRAmount2WithTax = 0;
                                            //ir.IRAmount2WithTax = 0;
                                            //dics += pc.dis1.GetValueOrDefault();
                                            //ir.discount1 = pom.discount; //+ dics; 
                                        }
                                    }
                                }
                                catch (Exception ee)
                                {
                                    logger.Error(ee.Message);
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "got Excepion");
                                }
                            }
                            context.Commit();
                            return Request.CreateResponse(HttpStatusCode.OK, pom);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Validation- IR id already exist.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Validation- GR and IR Item count are mismatch,\nSo please try again after refresh the page.");
                    }
                }
                catch (Exception exe)
                {
                    Console.Write(exe.Message);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Got Excepion- " + exe.Message);
                }
            }
        }

        [Route("sendtoapp")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage sendtoapp(IRMaster pom) // by sachin
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                    People peoplea = context.Peoples.Where(q => q.PeopleID == pom.BuyerId && q.Active == true).SingleOrDefault();
                    PurchaseOrderMaster pm = context.DPurchaseOrderMaster.Where(q => q.PurchaseOrderId == pom.PurchaseOrderId).SingleOrDefault();
                    pm.IrStatus = "pending";
                    //context.DPurchaseOrderMaster.Attach(pm);
                    context.Entry(pm).State = EntityState.Modified;
                    context.Commit();

                    IRMaster irm = context.IRMasterDB.Where(q => q.IRID == pom.IRID && q.PurchaseOrderId == pom.PurchaseOrderId).SingleOrDefault();
                    if (irm != null)
                    {
                        irm.IRStatus = "Pending from Buyer side";
                        context.IRMasterDB.Attach(irm);
                        context.Entry(irm).State = EntityState.Modified;
                        context.Commit();
                    }
                }
                catch (Exception exe)
                {
                    Console.Write(exe.Message);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "got Excepion");
                }
                return Request.CreateResponse(HttpStatusCode.OK, pom);
            }
        }

        [Authorize]
        [Route("GetIR")]
        public InvIrCnf GetIR(int id)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start : ");
                List<IR_Confirm> IRCData = new List<IR_Confirm>();
                List<IRMaster> IRM = new List<IRMaster>();
                InvoiceReceive IRData = new InvoiceReceive();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouseid = 0;
                    // Access claims
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                        Warehouseid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    IRM = context.IRMasterDB.Where(q => q.PurchaseOrderId == id).ToList();
                    IRCData = context.IR_ConfirmDb.Where(q => q.PurchaseOrderId == id).ToList();
                    IRData = context.InvoiceReceiveDb.Where(q => q.PurchaseOrderId == id).FirstOrDefault();

                    foreach (IRMaster ir in IRM)
                    {
                        ir.purDetails = IRCData;
                    }

                    InvIrCnf inv = new InvIrCnf
                    {
                        IRC = IRCData,
                        IR = IRData,
                        IRM = IRM
                    };

                    logger.Info("End  : ");
                    return inv;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                    logger.Info("End  PurchaseOrderDetail: ");
                    return null;
                }
            }
        }

        [Authorize]
        [Route("GetIRRec")]
        public List<InvoiceImage> GetIRRec(string id, int Poid)//get data from invoice image behalf of irid and poid
        {
            using (var context = new AuthContext())
            {
                logger.Info("start : ");
                List<InvoiceImage> IRRec = new List<InvoiceImage>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouseid = 0;
                    // Access claims
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                        Warehouseid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    //IRRec = context.InvoiceImageDb.Where(q => q.InvoiceNumber == id ).ToList();
                    IRRec = context.InvoiceImageDb.Where(q => q.InvoiceNumber == id && q.PurchaseOrderId == Poid).ToList();//get data from invoice image behalf of irid and poid
                    logger.Info("End  : ");
                    return IRRec;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                    logger.Info("End  PurchaseOrderDetail: ");
                    return null;
                }
            }
        }

        [HttpGet]
        [Route("SearchIO")]
        public InvoiceImage GetIRReca(string Searchstring)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start : ");
                InvoiceImage IRRec = new InvoiceImage();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouseid = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouseid = int.Parse(claim.Value);
                        }
                    }

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    IRRec = context.InvoiceImageDb.Where(q => q.InvoiceNumber == Searchstring).FirstOrDefault();
                    logger.Info("End  : ");
                    return IRRec;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                    logger.Info("End  PurchaseOrderDetail: ");
                    return null;
                }
            }
        }
        /// <summary>
        /// Update buyer name on PO
        /// </summary>
        /// <param name="pom"></param>
        /// <returns></returns>
        [Route("UpdateBuyer")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage UpdateBuyer(Buyerchange pom)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
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
                    People people = context.Peoples.Where(q => q.PeopleID == pom.PeopleID && q.Active == true).SingleOrDefault();
                    PurchaseOrderMaster pm = context.DPurchaseOrderMaster.Where(q => q.PurchaseOrderId == pom.PurchaseOrderId).SingleOrDefault();
                    pm.IrStatus = "pending";
                    pm.BuyerId = people.PeopleID;
                    pm.BuyerName = people.DisplayName;
                    //context.DPurchaseOrderMaster.Attach(pm);
                    context.Entry(pm).State = EntityState.Modified;
                    context.Commit();
                }
                catch (Exception exe)
                {
                    Console.Write(exe.Message);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "got Excepion");
                }
                return Request.CreateResponse(HttpStatusCode.OK, pom);
            }
        }
        /// <summary>
        /// Verify IR Number
        /// </summary>
        /// <param name="IrId"></param>
        /// <returns></returns>
        [Route("verifyIR")]
        [HttpGet]
        public bool verifyIR(string IrId)
        {
            using (var context = new AuthContext())
            {

                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouseid = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouseid = int.Parse(claim.Value);
                        }
                    }
                    // List<InvoiceReceive> IRData = context.InvoiceReceiveDb.Where(q => q.IR1ID == IrId || q.IR2ID == IrId || q.IR3ID == IrId).ToList();
                    List<IRMaster> IRData = context.IRMasterDB.Where(q => q.IRID == IrId).ToList();//new changes for using the IR Master By Raj

                    if (IRData.Count != 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                    logger.Info("End  PurchaseOrderDetail: ");
                    return false;
                }

            }
        }

        /// <summary>
        /// Verify PO Number
        /// </summary>
        /// <param name="Poid"></param>
        /// <returns></returns>
        [Route("verifyPo")]
        [HttpGet]
        public bool verifyPo(int Poid)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouseid = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouseid = int.Parse(claim.Value);
                        }
                    }
                    List<PurchaseOrderMaster> PoData = context.DPurchaseOrderMaster.Where(q => q.PurchaseOrderId == Poid).ToList();
                    if (PoData.Count != 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                    logger.Info("End  PurchaseOrderDetail: ");
                    return false;
                }
            }
        }

        /// <summary>
        /// Get IR detail from IR master table on IR view page
        /// </summary>
        /// <returns></returns>
        [Route("IrViewDetail")]
        [HttpGet]
        public List<IRMasterDTO> IrViewDetail()
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouseid = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouseid = int.Parse(claim.Value);
                        }
                    }
                    List<IRMasterDTO> IRdata = (from a in context.IRMasterDB
                                                join b in context.Warehouses
                                                on a.WarehouseId equals b.WarehouseId
                                                select new IRMasterDTO
                                                {
                                                    CreationDate = a.CreationDate,
                                                    IRID = a.IRID,
                                                    PurchaseOrderId = a.PurchaseOrderId,
                                                    IRType = a.IRType,
                                                    WarehouseName = b.WarehouseName,
                                                    SupplierName = a.SupplierName,
                                                    IRStatus = a.IRStatus,
                                                    TotalAmount = a.TotalAmount,
                                                    WarehouseId = a.WarehouseId
                                                }).ToList();
                    return IRdata.Where(a => a.WarehouseId == Warehouseid).ToList();
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                    logger.Info("End  PurchaseOrderDetail: ");
                    return null;
                }
            }
        }
        /// <summary>
        /// Search IR behalf of date and warehouse
        /// </summary>
        /// <param name="WarehouseId"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <returns></returns>
        [Route("IRserachdetail")]
        [HttpGet]
        public List<IRMasterDTO> IrViewDetaila(int WarehouseId, DateTime? enddate, DateTime? startdate)
        {
            using (var context = new AuthContext())
            {

                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouseid = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouseid = int.Parse(claim.Value);
                        }
                    }
                    List<IRMasterDTO> IRdata = (from a in context.IRMasterDB
                                                join b in context.Warehouses
                                                on a.WarehouseId equals b.WarehouseId
                                                select new IRMasterDTO
                                                {
                                                    CreationDate = a.CreationDate,
                                                    IRID = a.IRID,
                                                    PurchaseOrderId = a.PurchaseOrderId,
                                                    IRType = a.IRType,
                                                    WarehouseName = b.WarehouseName + " (" + b.CityName + ")",
                                                    SupplierName = a.SupplierName,
                                                    IRStatus = a.IRStatus,
                                                    PaymentStatus = a.PaymentStatus,
                                                    TotalAmount = a.TotalAmount,
                                                    Discount = a.Discount,
                                                    BuyerName = a.BuyerName,
                                                    WarehouseId = a.WarehouseId
                                                }).Where(a => a.CreationDate >= startdate && a.CreationDate <= enddate).OrderByDescending(a => a.CreationDate).ToList();
                    return IRdata.Where(a => a.WarehouseId == WarehouseId).ToList();
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                    logger.Info("End  PurchaseOrderDetail: ");
                    return null;
                }
            }
        }
        /// <summary>
        /// Get IR List created by 08/July/2019
        /// </summary>
        /// <param name="WarehouseId"></param>
        /// <returns></returns>
        [Route("IRserachdetailAll")]
        [HttpGet]
        public List<IRMasterDTO> IrViewDetailaAll(int WarehouseId)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouseid = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouseid = int.Parse(claim.Value);
                        }
                    }

                    if (Warehouseid == 0)
                    {
                        Warehouseid = WarehouseId;
                    }

                    List<IRMasterDTO> IRdata = (from a in context.IRMasterDB
                                                join b in context.Warehouses
                                                on a.WarehouseId equals b.WarehouseId
                                                select new IRMasterDTO
                                                {
                                                    CreationDate = a.CreationDate,
                                                    IRID = a.IRID,
                                                    PurchaseOrderId = a.PurchaseOrderId,
                                                    IRType = a.IRType,
                                                    WarehouseName = b.WarehouseName + " (" + b.CityName + ")",
                                                    SupplierName = a.SupplierName,
                                                    IRStatus = a.IRStatus,
                                                    PaymentStatus = a.PaymentStatus,
                                                    TotalAmount = a.TotalAmount,
                                                    Discount = a.Discount,
                                                    BuyerName = a.BuyerName,
                                                    WarehouseId = a.WarehouseId
                                                }).ToList();
                    return IRdata.Where(a => a.WarehouseId == WarehouseId).OrderByDescending(a => a.CreationDate).ToList();
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                    logger.Info("End  PurchaseOrderDetail: ");
                    return null;
                }
            }
        }

        /// <summary>
        /// search IR with poid created by 08/July/2019
        /// </summary>
        /// <param name="poid"></param>
        /// <returns></returns>
        [Route("IRserachdetailPoid")]
        [HttpGet]
        public List<IRMasterDTO> IrViewDetailaPoid(int poid)
        {
            using (var context = new AuthContext())
            {

                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouseid = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouseid = int.Parse(claim.Value);
                        }
                    }
                    List<IRMasterDTO> IRdata = (from a in context.IRMasterDB
                                                where a.PurchaseOrderId == poid
                                                join b in context.Warehouses
                                                on a.WarehouseId equals b.WarehouseId
                                                select new IRMasterDTO
                                                {
                                                    CreationDate = a.CreationDate,
                                                    IRID = a.IRID,
                                                    PurchaseOrderId = a.PurchaseOrderId,
                                                    IRType = a.IRType,
                                                    WarehouseName = b.WarehouseName + " (" + b.CityName + ")",
                                                    SupplierName = a.SupplierName,
                                                    IRStatus = a.IRStatus,
                                                    TotalAmount = a.TotalAmount,
                                                    Discount = a.Discount,
                                                    BuyerName = a.BuyerName
                                                }).ToList();
                    return IRdata;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                    logger.Info("End  PurchaseOrderDetail: ");
                    return null;
                }
            }
        }


        /// <summary>
        /// Get Rejected IR
        /// Created by 09/04/2019
        /// </summary>
        /// <param name="WarehouseId"></param>
        /// <returns></returns>
        [Route("getRejetcedIR")]
        [HttpGet]
        public List<IRMasterDTO> getRejetcedIR(int WarehouseId)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouseid = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouseid = int.Parse(claim.Value);
                        }
                    }
                    List<IRMasterDTO> IRdata = (from a in context.IRMasterDB
                                                join c in context.DPurchaseOrderMaster on a.PurchaseOrderId equals c.PurchaseOrderId
                                                join b in context.Warehouses
                                                on a.WarehouseId equals b.WarehouseId
                                                select new IRMasterDTO
                                                {
                                                    Id = a.Id,
                                                    CreationDate = a.CreationDate,
                                                    IRID = a.IRID,
                                                    PurchaseOrderId = a.PurchaseOrderId,
                                                    IRType = a.IRType,
                                                    WarehouseId = a.WarehouseId,
                                                    WarehouseName = b.WarehouseName,
                                                    SupplierName = a.SupplierName,
                                                    IRStatus = a.IRStatus,
                                                    TotalAmount = a.TotalAmount,
                                                    supplierId = a.supplierId,
                                                    RejectedComment = a.RejectedComment,
                                                    BuyerName = c.BuyerName
                                                }).ToList();
                    return IRdata.Where(a => a.WarehouseId == WarehouseId && a.IRStatus == "Rejected from Buyer side").ToList();
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                    logger.Info("End  PurchaseOrderDetail: ");
                    return null;
                }
            }
        }

        [Route("getRejetcedIRDetail")]
        [HttpPost]
        public dynamic getRejetcedIRDetail(IRMaster obj)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    if (obj.IRType == "IR1")
                    {
                        List<IR_Detail1> ics1 = (from a in context.IR_ConfirmDb
                                                 join b in context.IRMasterDB on a.PurchaseOrderId equals b.PurchaseOrderId
                                                 where a.PurchaseOrderId == obj.PurchaseOrderId && b.IRID == obj.IRID
                                                 select new IR_Detail1
                                                 {
                                                     //  MRP = a.MRP, 
                                                     IRreceiveid = a.IRreceiveid,
                                                     ItemNumber = a.ItemNumber,
                                                     CompanyId = a.CompanyId,
                                                     PurchaseOrderDetailId = a.PurchaseOrderDetailId,
                                                     PurchaseOrderId = a.PurchaseOrderId,
                                                     SupplierName = a.SupplierName,
                                                     WarehouseId = a.WarehouseId,
                                                     WarehouseName = a.WarehouseName,
                                                     PurchaseSku = a.PurchaseSku,
                                                     ItemName = a.ItemName,
                                                     HSNCode = a.HSNCode,
                                                     PriceRecived = a.PriceRecived,
                                                     TotalQuantity = a.TotalQuantity,
                                                     IRQuantity = a.IRQuantity,
                                                     Status = a.Status,
                                                     CreationDate = a.CreationDate,
                                                     IR1ID = a.IR1ID,
                                                     QtyRecived1 = a.QtyRecived1,
                                                     Price1 = a.Price1,
                                                     Ir1Date = a.Ir1Date,
                                                     Ir1PersonId = a.Ir1PersonId,
                                                     Ir1PersonName = a.Ir1PersonName,
                                                     dis1 = a.dis1,
                                                     QtyRecived = a.QtyRecived,
                                                     TotalTaxPercentage = a.TotalTaxPercentage,
                                                     TtlAmt = a.TtlAmt,
                                                     IRType = "IR1",
                                                     gstamt = a.gstamt,
                                                     SupplierId = a.SupplierId,
                                                     discountAll = b.Discount,
                                                     CessTaxPercentage = a.CessTaxPercentage,
                                                     distype1 = a.distype1,
                                                     DesA1 = a.DesA1,
                                                     DesP1 = a.DesP1,
                                                     OtherAmount = b.OtherAmount,
                                                     OtherAmountRemark = b.OtherAmountRemark,
                                                     ExpenseAmount = b.ExpenseAmount,
                                                     ExpenseAmountRemark = b.ExpenseAmountRemark,
                                                     RoundofAmount = b.RoundofAmount,
                                                     OtherAmountType = b.OtherAmountType,
                                                     ExpenseAmountType = b.ExpenseAmountType,
                                                     RoundoffAmountType = b.RoundoffAmountType
                                                 }).ToList();
                        return ics1;
                    }
                    else if (obj.IRType == "IR2")
                    {
                        List<IR_Detail2> ics2 = (from a in context.IR_ConfirmDb
                                                 join b in context.IRMasterDB on a.PurchaseOrderId equals b.PurchaseOrderId
                                                 where a.PurchaseOrderId == obj.PurchaseOrderId && b.IRID == obj.IRID
                                                 select new IR_Detail2
                                                 {
                                                     //  MRP = a.MRP,
                                                     IRreceiveid = a.IRreceiveid,
                                                     ItemNumber = a.ItemNumber,
                                                     CompanyId = a.CompanyId,
                                                     PurchaseOrderDetailId = a.PurchaseOrderDetailId,
                                                     PurchaseOrderId = a.PurchaseOrderId,
                                                     SupplierName = a.SupplierName,
                                                     WarehouseId = a.WarehouseId,
                                                     WarehouseName = a.WarehouseName,
                                                     PurchaseSku = a.PurchaseSku,
                                                     ItemName = a.ItemName,
                                                     HSNCode = a.HSNCode,
                                                     PriceRecived = a.PriceRecived,
                                                     TotalQuantity = a.TotalQuantity,
                                                     IRQuantity = a.IRQuantity,
                                                     Status = a.Status,
                                                     CreationDate = a.CreationDate,
                                                     IR2ID = a.IR2ID,
                                                     QtyRecived2 = a.QtyRecived2,
                                                     Price2 = a.Price2,
                                                     Ir2Date = a.Ir2Date,
                                                     Ir2PersonId = a.Ir2PersonId,
                                                     Ir2PersonName = a.Ir2PersonName,
                                                     dis2 = a.dis2,
                                                     QtyRecived = a.QtyRecived,
                                                     TotalTaxPercentage = a.TotalTaxPercentage,
                                                     TtlAmt = a.TtlAmt,
                                                     IRType = "IR2",
                                                     SupplierId = a.SupplierId,
                                                     discountAll = b.Discount,
                                                     CessTaxPercentage = a.CessTaxPercentage,
                                                     distype2 = a.distype2,
                                                     DesA2 = a.DesA2,
                                                     DesP2 = a.DesP2,
                                                     OtherAmount = b.OtherAmount,
                                                     OtherAmountRemark = b.OtherAmountRemark,
                                                     ExpenseAmount = b.ExpenseAmount,
                                                     ExpenseAmountRemark = b.ExpenseAmountRemark,
                                                     RoundofAmount = b.RoundofAmount,
                                                     OtherAmountType = b.OtherAmountType,
                                                     ExpenseAmountType = b.ExpenseAmountType,
                                                     RoundoffAmountType = b.RoundoffAmountType

                                                 }).ToList();
                        return ics2;
                    }
                    else if (obj.IRType == "IR3")
                    {
                        List<IR_Detail3> ics3 = (from a in context.IR_ConfirmDb
                                                 join b in context.IRMasterDB on a.PurchaseOrderId equals b.PurchaseOrderId
                                                 where a.PurchaseOrderId == obj.PurchaseOrderId && b.IRID == obj.IRID
                                                 select new IR_Detail3
                                                 {
                                                     //  MRP = a.MRP,
                                                     IRreceiveid = a.IRreceiveid,
                                                     ItemNumber = a.ItemNumber,
                                                     CompanyId = a.CompanyId,
                                                     PurchaseOrderDetailId = a.PurchaseOrderDetailId,
                                                     PurchaseOrderId = a.PurchaseOrderId,
                                                     SupplierName = a.SupplierName,
                                                     WarehouseId = a.WarehouseId,
                                                     WarehouseName = a.WarehouseName,
                                                     PurchaseSku = a.PurchaseSku,
                                                     ItemName = a.ItemName,
                                                     HSNCode = a.HSNCode,
                                                     PriceRecived = a.PriceRecived,
                                                     TotalQuantity = a.TotalQuantity,
                                                     IRQuantity = a.IRQuantity,
                                                     Status = a.Status,
                                                     CreationDate = a.CreationDate,
                                                     IR3ID = a.IR3ID,
                                                     QtyRecived3 = a.QtyRecived3,
                                                     Price3 = a.Price3,
                                                     Ir3Date = a.Ir3Date,
                                                     Ir3PersonId = a.Ir3PersonId,
                                                     Ir3PersonName = a.Ir3PersonName,
                                                     dis3 = a.dis3,
                                                     QtyRecived = a.QtyRecived,
                                                     TotalTaxPercentage = a.TotalTaxPercentage,
                                                     TtlAmt = a.TtlAmt,
                                                     IRType = "IR3",
                                                     gstamt = a.gstamt,
                                                     SupplierId = a.SupplierId,
                                                     discountAll = b.Discount,
                                                     CessTaxPercentage = a.CessTaxPercentage,
                                                     distype3 = a.distype3,
                                                     DesA3 = a.DesA3,
                                                     DesP3 = a.DesP3,
                                                     OtherAmount = b.OtherAmount,
                                                     OtherAmountRemark = b.OtherAmountRemark,
                                                     ExpenseAmount = b.ExpenseAmount,
                                                     ExpenseAmountRemark = b.ExpenseAmountRemark,
                                                     RoundofAmount = b.RoundofAmount,
                                                     OtherAmountType = b.OtherAmountType,
                                                     ExpenseAmountType = b.ExpenseAmountType,
                                                     RoundoffAmountType = b.RoundoffAmountType
                                                 }).ToList();
                        return ics3;
                    }
                    else if (obj.IRType == "IR4")
                    {
                        List<IR_Detail4> ics4 = (from a in context.IR_ConfirmDb
                                                 join b in context.IRMasterDB on a.PurchaseOrderId equals b.PurchaseOrderId
                                                 where a.PurchaseOrderId == obj.PurchaseOrderId && b.IRID == obj.IRID
                                                 select new IR_Detail4
                                                 {
                                                     // MRP = a.MRP,
                                                     IRreceiveid = a.IRreceiveid,
                                                     ItemNumber = a.ItemNumber,
                                                     CompanyId = a.CompanyId,
                                                     PurchaseOrderDetailId = a.PurchaseOrderDetailId,
                                                     PurchaseOrderId = a.PurchaseOrderId,
                                                     SupplierName = a.SupplierName,
                                                     WarehouseId = a.WarehouseId,
                                                     WarehouseName = a.WarehouseName,
                                                     PurchaseSku = a.PurchaseSku,
                                                     ItemName = a.ItemName,
                                                     HSNCode = a.HSNCode,
                                                     PriceRecived = a.PriceRecived,
                                                     TotalQuantity = a.TotalQuantity,
                                                     IRQuantity = a.IRQuantity,
                                                     Status = a.Status,
                                                     CreationDate = a.CreationDate,
                                                     IR4ID = a.IR4ID,
                                                     QtyRecived4 = a.QtyRecived4,
                                                     Price4 = a.Price4,
                                                     Ir4Date = a.Ir4Date,
                                                     Ir4PersonId = a.Ir4PersonId,
                                                     Ir4PersonName = a.Ir4PersonName,
                                                     dis4 = a.dis4,
                                                     QtyRecived = a.QtyRecived,
                                                     TotalTaxPercentage = a.TotalTaxPercentage,
                                                     TtlAmt = a.TtlAmt,
                                                     IRType = "IR4",
                                                     gstamt = a.gstamt,
                                                     SupplierId = a.SupplierId,
                                                     discountAll = b.Discount,
                                                     CessTaxPercentage = a.CessTaxPercentage,
                                                     distype4 = a.distype4,
                                                     DesA4 = a.DesA4,
                                                     DesP4 = a.DesP4,
                                                     OtherAmount = b.OtherAmount,
                                                     OtherAmountRemark = b.OtherAmountRemark,
                                                     ExpenseAmount = b.ExpenseAmount,
                                                     ExpenseAmountRemark = b.ExpenseAmountRemark,
                                                     RoundofAmount = b.RoundofAmount,
                                                     OtherAmountType = b.OtherAmountType,
                                                     ExpenseAmountType = b.ExpenseAmountType,
                                                     RoundoffAmountType = b.RoundoffAmountType
                                                 }).ToList();
                        return ics4;
                    }
                    else if (obj.IRType == "IR5")
                    {
                        List<IR_Detail5> ics5 = (from a in context.IR_ConfirmDb
                                                 join b in context.IRMasterDB on a.PurchaseOrderId equals b.PurchaseOrderId
                                                 where a.PurchaseOrderId == obj.PurchaseOrderId && b.IRID == obj.IRID
                                                 select new IR_Detail5
                                                 {
                                                     //MRP = a.MRP,
                                                     IRreceiveid = a.IRreceiveid,
                                                     ItemNumber = a.ItemNumber,
                                                     CompanyId = a.CompanyId,
                                                     PurchaseOrderDetailId = a.PurchaseOrderDetailId,
                                                     PurchaseOrderId = a.PurchaseOrderId,
                                                     SupplierName = a.SupplierName,
                                                     WarehouseId = a.WarehouseId,
                                                     WarehouseName = a.WarehouseName,
                                                     PurchaseSku = a.PurchaseSku,
                                                     ItemName = a.ItemName,
                                                     HSNCode = a.HSNCode,
                                                     PriceRecived = a.PriceRecived,
                                                     TotalQuantity = a.TotalQuantity,
                                                     IRQuantity = a.IRQuantity,
                                                     Status = a.Status,
                                                     CreationDate = a.CreationDate,
                                                     IR5ID = a.IR5ID,
                                                     QtyRecived5 = a.QtyRecived5,
                                                     Price5 = a.Price5,
                                                     Ir5Date = a.Ir5Date,
                                                     Ir5PersonId = a.Ir5PersonId,
                                                     Ir5PersonName = a.Ir5PersonName,
                                                     dis5 = a.dis5,
                                                     QtyRecived = a.QtyRecived,
                                                     TotalTaxPercentage = a.TotalTaxPercentage,
                                                     TtlAmt = a.TtlAmt,
                                                     IRType = "IR5",
                                                     gstamt = a.gstamt,
                                                     SupplierId = a.SupplierId,
                                                     discountAll = b.Discount,
                                                     CessTaxPercentage = a.CessTaxPercentage,
                                                     distype5 = a.distype5,
                                                     DesA5 = a.DesA5,
                                                     DesP5 = a.DesP5,
                                                     OtherAmount = b.OtherAmount,
                                                     OtherAmountRemark = b.OtherAmountRemark,
                                                     ExpenseAmount = b.ExpenseAmount,
                                                     ExpenseAmountRemark = b.ExpenseAmountRemark,
                                                     RoundofAmount = b.RoundofAmount,
                                                     OtherAmountType = b.OtherAmountType,
                                                     ExpenseAmountType = b.ExpenseAmountType,
                                                     RoundoffAmountType = b.RoundoffAmountType
                                                 }).ToList();
                        return ics5;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Get rejected IR detail:" + ex.Message);
                    return null;
                }
            }
        }


        /// <summary>
        /// Get IR Detail in excel file
        /// </summary>
        /// <param name="WarehouseId"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <returns></returns>
        [Route("IRExceldetail")]
        [HttpGet]
        public List<IR_Confirm> IrExportDetail(int WarehouseId, DateTime startdate, DateTime enddate)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouseid = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouseid = int.Parse(claim.Value);
                        }
                    }
                    List<IR_Confirm> IRdata = context.IR_ConfirmDb.Where(q => q.CreationDate >= startdate && q.CreationDate <= enddate).ToList();
                    return IRdata.Where(q => q.WarehouseId == WarehouseId).ToList();
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                    logger.Info("End  PurchaseOrderDetail: ");
                    return null;
                }
            }
        }

        /// <summary>
        /// Get Pending IR  
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        [Route("PendingIR")]
        [HttpGet]
        public List<PurchaseOrderMaster> PendingIR(int sid)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouseid = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouseid = int.Parse(claim.Value);
                        }
                    }
                    List<PurchaseOrderMaster> pm = context.DPurchaseOrderMaster.Where(q => q.IrStatus == "pending" && q.SupplierId == sid).ToList();
                    return pm;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                    logger.Info("End  PurchaseOrderDetail: ");
                    return null;
                }
            }
        }

        /// <summary>
        /// Get Buyer Approved IR 
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        [Route("getIRmaster")]
        [HttpGet]
        public HttpResponseMessage getIRmaster(int sid)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouseid = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouseid = int.Parse(claim.Value);
                        }
                    }
                    // List<IRMaster> IRM = context.IRMasterDB.Where(q => q.IRStatus == "pending" && q.supplierId == sid).ToList();
                    var IRM = (from a in context.IRMasterDB
                               join b in context.DPurchaseOrderMaster on a.PurchaseOrderId equals b.PurchaseOrderId
                               where b.IrStatus == "IrApproved"
                               select new
                               {
                                   a.IRID,
                                   a.IRType,
                                   a.supplierId,
                                   a.PurchaseOrderId,
                                   a.IRStatus,
                                   a.TotalAmount,
                                   b.IrStatus
                               }).ToList();
                    var IRMF = IRM.Where(a => a.supplierId == sid).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, IRMF);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                    logger.Info("End  PurchaseOrderDetail: ");
                    return null;
                }
            }
        }

        /// <summary>
        /// get IR invoice numbers behalf of po id
        /// </summary>
        /// <param name="poid"></param>
        /// <returns></returns>
        [Route("getIRNumber")]
        [HttpGet]
        public HttpResponseMessage getIRNumber(int poid)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouseid = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouseid = int.Parse(claim.Value);
                        }
                    }
                    List<IRMaster> IRM = context.IRMasterDB.Where(q => q.PurchaseOrderId == poid).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, IRM);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                    logger.Info("End  PurchaseOrderDetail: ");
                    return null;
                }
            }
        }

        /// <summary>
        /// RePost IR aftre reject from buyer side
        /// </summary>
        /// <param name="pom"></param>
        /// <returns></returns>
        [Route("PutIr")]
        [HttpPut]
        public HttpResponseMessage PutIr(PutIrDTO pom) // by sachin
        {
            using (var context = new AuthContext())
            using (var dbContextTransaction = context.Database.BeginTransaction())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    double? TotalAmount = 0;
                    double? gstAmount = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    #region Updating IR Master

                    foreach (IR_Confirm m in pom.IrItem)
                    {
                        double cessamt = Convert.ToDouble(((m.Price1 * m.QtyRecived1) - m.dis1) * m.CessTaxPercentage / 100);
                        TotalAmount += m.TtlAmt + cessamt;
                        gstAmount += m.gstamt;
                    }

                    IRMaster IRM = context.IRMasterDB.Where(q => q.PurchaseOrderId == pom.PurchaseOrderId && q.IRID == pom.IRID).SingleOrDefault();
                    IRM.TotalAmount = TotalAmount - pom.discount ?? 0;
                    try
                    {
                        if (pom.OtherAmountType == "ADD")
                        {
                            IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount + pom.OtherAmount), 2);
                        }
                        else if (pom.OtherAmountType == "MINUS")
                        {
                            IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount - pom.OtherAmount), 2);
                        }
                        if (pom.ExpenseAmountType == "ADD")
                        {
                            IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount + pom.ExpenseAmount), 2);
                        }
                        else if (pom.ExpenseAmountType == "MINUS")
                        {
                            IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount - pom.ExpenseAmount), 2);
                        }
                        if (pom.RoundoffAmountType == "ADD")
                        {
                            IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount + pom.RoundofAmount), 2);
                        }
                        else if (pom.RoundoffAmountType == "MINUS")
                        {
                            IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount - pom.RoundofAmount), 2);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Info(ex.Message);
                    }
                    IRM.IRAmountWithTax = IRM.TotalAmount;
                    IRM.TotalAmountRemaining = IRM.TotalAmount;
                    IRM.Gstamt = gstAmount;
                    IRM.IRStatus = "Pending from Buyer side";
                    IRM.Discount = pom.discount;
                    IRM.OtherAmount = pom.OtherAmount;
                    IRM.ExpenseAmount = pom.ExpenseAmount;
                    IRM.RoundofAmount = pom.RoundofAmount;
                    IRM.OtherAmountType = pom.OtherAmountType;
                    IRM.ExpenseAmountType = pom.ExpenseAmountType;
                    IRM.RoundoffAmountType = pom.RoundoffAmountType;
                    context.IRMasterDB.Attach(IRM);
                    context.Entry(IRM).State = EntityState.Modified;
                    #endregion

                    #region Updating Ir Item detail

                    foreach (IR_Confirm i in pom.IrItem)
                    {
                        switch (i.IRType)
                        {
                            case "IR1":
                                foreach (IR_Confirm j in pom.IrItem)
                                {
                                    IR_Confirm iR = context.IR_ConfirmDb.Where(a => a.IRreceiveid == j.IRreceiveid).SingleOrDefault();
                                    if (iR != null)
                                    {
                                        iR.Price1 = j.Price1;
                                        iR.dis1 = j.dis1;
                                        iR.TtlAmt = j.TtlAmt;
                                        context.IR_ConfirmDb.Attach(iR);
                                        context.Entry(iR).State = EntityState.Modified;

                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                break;
                            case "IR2":
                                foreach (IR_Confirm k in pom.IrItem)
                                {
                                    IR_Confirm iR = context.IR_ConfirmDb.Where(a => a.IRreceiveid == k.IRreceiveid).SingleOrDefault();

                                    if (iR != null)
                                    {
                                        iR.Price2 = k.Price2;
                                        iR.dis2 = k.dis2;
                                        iR.TtlAmt = k.TtlAmt;
                                        context.IR_ConfirmDb.Attach(iR);
                                        context.Entry(iR).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                break;
                            case "IR3":
                                foreach (IR_Confirm l in pom.IrItem)
                                {
                                    IR_Confirm iR = context.IR_ConfirmDb.Where(a => a.IRreceiveid == l.IRreceiveid).SingleOrDefault();

                                    if (iR != null)
                                    {
                                        iR.Price3 = l.Price3;
                                        iR.dis3 = l.dis3;
                                        iR.TtlAmt = l.TtlAmt;
                                        context.IR_ConfirmDb.Attach(iR);
                                        context.Entry(iR).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                break;
                            case "IR4":
                                foreach (IR_Confirm l in pom.IrItem)
                                {
                                    IR_Confirm iR = context.IR_ConfirmDb.Where(a => a.IRreceiveid == l.IRreceiveid).SingleOrDefault();

                                    if (iR != null)
                                    {
                                        iR.Price4 = l.Price4;
                                        iR.dis4 = l.dis4;
                                        iR.TtlAmt = l.TtlAmt;
                                        context.IR_ConfirmDb.Attach(iR);
                                        context.Entry(iR).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                break;
                            case "IR5":
                                foreach (IR_Confirm l in pom.IrItem)
                                {
                                    IR_Confirm iR = context.IR_ConfirmDb.Where(a => a.IRreceiveid == l.IRreceiveid).SingleOrDefault();

                                    if (iR != null)
                                    {
                                        iR.Price5 = l.Price5;
                                        iR.dis5 = l.dis5;
                                        iR.TtlAmt = l.TtlAmt;
                                        context.IR_ConfirmDb.Attach(iR);
                                        context.Entry(iR).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                break;
                            default:
                                return null;
                        }
                        break;
                    }

                    #endregion

                    context.Commit();
                    dbContextTransaction.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK, pom);
                }
                catch (Exception exe)
                {
                    dbContextTransaction.Rollback();
                    Console.Write(exe.Message);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "got Excepion");
                }
            }
        }

        #region  Drafted IR  to post IR
        /// <summary>
        /// Created Date:02/05/2019
        /// created by Raj
        /// </summary>
        /// <param name="pom"></param>
        /// <returns></returns>
        [Route("DraftIRPost")]
        [HttpPut]
        public HttpResponseMessage DraftIR(PutIrDTO pom)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    double? TotalAmount = 0;
                    double? gstAmount = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    foreach (IR_Confirm i in pom.IrItem)
                    {
                        switch (i.IRType)
                        {
                            case "IR1":

                                foreach (IR_Confirm m in pom.IrItem)
                                {
                                    double cessamt = Convert.ToDouble(((m.Price1 * m.QtyRecived1) - m.dis1) * m.CessTaxPercentage / 100);
                                    TotalAmount += m.TtlAmt + cessamt;
                                    gstAmount += m.gstamt;
                                }

                                IRMaster IRM = context.IRMasterDB.Where(q => q.Id == pom.Id && q.PurchaseOrderId == i.PurchaseOrderId).SingleOrDefault();
                                IRM.IRID = pom.IRID;
                                IRM.TotalAmount = TotalAmount - pom.discount ?? 0;
                                try
                                {
                                    if (pom.OtherAmountType == "ADD")
                                    {
                                        IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount + pom.OtherAmount), 2);
                                    }
                                    else if (pom.OtherAmountType == "MINUS")
                                    {
                                        IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount - pom.OtherAmount), 2);
                                    }
                                    if (pom.ExpenseAmountType == "ADD")
                                    {
                                        IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount + pom.ExpenseAmount), 2);
                                    }
                                    else if (pom.ExpenseAmountType == "MINUS")
                                    {
                                        IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount - pom.ExpenseAmount), 2);
                                    }
                                    if (pom.RoundoffAmountType == "ADD")
                                    {
                                        IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount + pom.RoundofAmount), 2);
                                    }
                                    else if (pom.RoundoffAmountType == "MINUS")
                                    {
                                        IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount - pom.RoundofAmount), 2);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Info(ex.Message);
                                }
                                IRM.IRAmountWithTax = IRM.TotalAmount;
                                IRM.TotalAmountRemaining = IRM.TotalAmount;
                                IRM.Gstamt = gstAmount;
                                IRM.IRStatus = "IR Posted";
                                IRM.Discount = pom.discount;
                                IRM.OtherAmount = pom.OtherAmount;
                                IRM.OtherAmountRemark = pom.OtherAmountRemark;
                                IRM.OtherAmountType = pom.OtherAmountType;
                                IRM.ExpenseAmount = pom.ExpenseAmount;
                                IRM.ExpenseAmountRemark = pom.ExpenseAmountRemark;
                                IRM.ExpenseAmountType = pom.ExpenseAmountType;
                                IRM.RoundofAmount = pom.RoundofAmount;
                                IRM.RoundoffAmountType = pom.RoundoffAmountType;
                                context.IRMasterDB.Attach(IRM);
                                context.Entry(IRM).State = EntityState.Modified;
                                context.Commit();

                                PurchaseOrderMaster POM = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == IRM.PurchaseOrderId).SingleOrDefault();
                                POM.IrStatus = "IR Posted";
                                //context.DPurchaseOrderMaster.Attach(POM);
                                context.Entry(POM).State = EntityState.Modified;
                                context.Commit();

                                foreach (IR_Confirm j in pom.IrItem)
                                {
                                    IR_Confirm iR = context.IR_ConfirmDb.Where(a => a.IRreceiveid == j.IRreceiveid).SingleOrDefault();

                                    if (iR != null)
                                    {
                                        iR.Price1 = j.Price1;
                                        iR.dis1 = j.dis1;
                                        iR.TtlAmt = j.TtlAmt;
                                        iR.distype1 = j.distype1;
                                        iR.DesP1 = j.DesP1;
                                        iR.DesA1 = j.DesA1;
                                        iR.QtyRecived1 = j.QtyRecived1;
                                        try
                                        {
                                            iR.IRQuantity = Convert.ToInt32(j.QtyRecived1);
                                        }
                                        catch (Exception es) { }
                                        context.IR_ConfirmDb.Attach(iR);
                                        context.Entry(iR).State = EntityState.Modified;
                                        context.Commit();
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                break;
                            case "IR2":

                                foreach (IR_Confirm m in pom.IrItem)
                                {
                                    double cessamt = Convert.ToDouble((m.Price2 * m.QtyRecived2) * m.CessTaxPercentage / 100);
                                    TotalAmount += m.TtlAmt + cessamt;
                                    gstAmount += m.gstamt;
                                }

                                IRMaster IRM2 = context.IRMasterDB.Where(q => q.Id == pom.Id && q.PurchaseOrderId == i.PurchaseOrderId).SingleOrDefault();
                                IRM2.IRID = pom.IRID;
                                IRM2.TotalAmount = TotalAmount - pom.discount ?? 0;
                                try
                                {
                                    if (pom.OtherAmountType == "ADD")
                                    {
                                        IRM2.TotalAmount = Math.Round(Convert.ToDouble(IRM2.TotalAmount + pom.OtherAmount), 2);
                                    }
                                    else if (pom.OtherAmountType == "MINUS")
                                    {
                                        IRM2.TotalAmount = Math.Round(Convert.ToDouble(IRM2.TotalAmount - pom.OtherAmount), 2);
                                    }
                                    if (pom.ExpenseAmountType == "ADD")
                                    {
                                        IRM2.TotalAmount = Math.Round(Convert.ToDouble(IRM2.TotalAmount + pom.ExpenseAmount), 2);
                                    }
                                    else if (pom.ExpenseAmountType == "MINUS")
                                    {
                                        IRM2.TotalAmount = Math.Round(Convert.ToDouble(IRM2.TotalAmount - pom.ExpenseAmount), 2);
                                    }
                                    if (pom.RoundoffAmountType == "ADD")
                                    {
                                        IRM2.TotalAmount = Math.Round(Convert.ToDouble(IRM2.TotalAmount + pom.RoundofAmount), 2);
                                    }
                                    else if (pom.RoundoffAmountType == "MINUS")
                                    {
                                        IRM2.TotalAmount = Math.Round(Convert.ToDouble(IRM2.TotalAmount - pom.RoundofAmount), 2);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Info(ex.Message);
                                }
                                IRM2.IRAmountWithTax = IRM2.TotalAmount;
                                IRM2.TotalAmountRemaining = IRM2.TotalAmount;
                                IRM2.Gstamt = gstAmount;
                                IRM2.IRStatus = "IR Posted";
                                IRM2.Discount = pom.discount;
                                IRM2.OtherAmount = pom.OtherAmount;
                                IRM2.OtherAmountRemark = pom.OtherAmountRemark;
                                IRM2.OtherAmountType = pom.OtherAmountType;
                                IRM2.ExpenseAmount = pom.ExpenseAmount;
                                IRM2.ExpenseAmountRemark = pom.ExpenseAmountRemark;
                                IRM2.ExpenseAmountType = pom.ExpenseAmountType;
                                IRM2.RoundofAmount = pom.RoundofAmount;
                                IRM2.RoundoffAmountType = pom.RoundoffAmountType;
                                context.IRMasterDB.Attach(IRM2);
                                context.Entry(IRM2).State = EntityState.Modified;
                                context.Commit();

                                PurchaseOrderMaster POM2 = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == IRM2.PurchaseOrderId).SingleOrDefault();
                                POM2.IrStatus = "IR Posted";
                                //context.DPurchaseOrderMaster.Attach(POM2);
                                context.Entry(POM2).State = EntityState.Modified;
                                context.Commit();

                                foreach (IR_Confirm k in pom.IrItem)
                                {
                                    IR_Confirm iR = context.IR_ConfirmDb.Where(a => a.IRreceiveid == k.IRreceiveid).SingleOrDefault();

                                    if (iR != null)
                                    {
                                        iR.Price2 = k.Price2;
                                        iR.dis2 = k.dis2;
                                        iR.TtlAmt = k.TtlAmt;
                                        iR.distype2 = k.distype2;
                                        iR.DesP2 = k.DesP2;
                                        iR.DesA2 = k.DesA2;
                                        iR.QtyRecived2 = k.QtyRecived2;
                                        try
                                        {
                                            iR.IRQuantity = Convert.ToInt32(iR.QtyRecived1 + k.QtyRecived2);
                                        }
                                        catch (Exception es) { }
                                        context.IR_ConfirmDb.Attach(iR);
                                        context.Entry(iR).State = EntityState.Modified;
                                        context.Commit();
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                break;
                            case "IR3":

                                foreach (IR_Confirm m in pom.IrItem)
                                {
                                    double cessamt = Convert.ToDouble((m.Price3 * m.QtyRecived3) * m.CessTaxPercentage / 100);
                                    TotalAmount += m.TtlAmt + cessamt;
                                    gstAmount += m.gstamt;
                                }

                                IRMaster IRM3 = context.IRMasterDB.Where(q => q.Id == pom.Id && q.PurchaseOrderId == i.PurchaseOrderId).SingleOrDefault();
                                IRM3.IRID = pom.IRID;
                                IRM3.TotalAmount = TotalAmount - pom.discount ?? 0;
                                try
                                {
                                    if (pom.OtherAmountType == "ADD")
                                    {
                                        IRM3.TotalAmount = Math.Round(Convert.ToDouble(IRM3.TotalAmount + pom.OtherAmount), 2);
                                    }
                                    else if (pom.OtherAmountType == "MINUS")
                                    {
                                        IRM3.TotalAmount = Math.Round(Convert.ToDouble(IRM3.TotalAmount - pom.OtherAmount), 2);
                                    }
                                    if (pom.ExpenseAmountType == "ADD")
                                    {
                                        IRM3.TotalAmount = Math.Round(Convert.ToDouble(IRM3.TotalAmount + pom.ExpenseAmount), 2);
                                    }
                                    else if (pom.ExpenseAmountType == "MINUS")
                                    {
                                        IRM3.TotalAmount = Math.Round(Convert.ToDouble(IRM3.TotalAmount - pom.ExpenseAmount), 2);
                                    }
                                    if (pom.RoundoffAmountType == "ADD")
                                    {
                                        IRM3.TotalAmount = Math.Round(Convert.ToDouble(IRM3.TotalAmount + pom.RoundofAmount), 2);
                                    }
                                    else if (pom.RoundoffAmountType == "MINUS")
                                    {
                                        IRM3.TotalAmount = Math.Round(Convert.ToDouble(IRM3.TotalAmount - pom.RoundofAmount), 2);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Info(ex.Message);
                                }
                                IRM3.IRAmountWithTax = IRM3.TotalAmount;
                                IRM3.TotalAmountRemaining = IRM3.TotalAmount;
                                IRM3.Gstamt = gstAmount;
                                IRM3.IRStatus = "IR Posted";
                                IRM3.Discount = pom.discount;
                                IRM3.OtherAmount = pom.OtherAmount;
                                IRM3.OtherAmountRemark = pom.OtherAmountRemark;
                                IRM3.OtherAmountType = pom.OtherAmountType;
                                IRM3.ExpenseAmount = pom.ExpenseAmount;
                                IRM3.ExpenseAmountRemark = pom.ExpenseAmountRemark;
                                IRM3.ExpenseAmountType = pom.ExpenseAmountType;
                                IRM3.RoundofAmount = pom.RoundofAmount;
                                IRM3.RoundoffAmountType = pom.RoundoffAmountType;
                                context.IRMasterDB.Attach(IRM3);
                                context.Entry(IRM3).State = EntityState.Modified;
                                context.Commit();

                                PurchaseOrderMaster POM3 = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == IRM3.PurchaseOrderId).SingleOrDefault();
                                POM3.IrStatus = "IR Posted";
                                //context.DPurchaseOrderMaster.Attach(POM3);
                                context.Entry(POM3).State = EntityState.Modified;
                                context.Commit();

                                foreach (IR_Confirm l in pom.IrItem)
                                {
                                    IR_Confirm iR = context.IR_ConfirmDb.Where(a => a.IRreceiveid == l.IRreceiveid).SingleOrDefault();

                                    if (iR != null)
                                    {
                                        iR.Price3 = l.Price3;
                                        iR.dis3 = l.dis3;
                                        iR.TtlAmt = l.TtlAmt;
                                        iR.distype3 = l.distype3;
                                        iR.DesP3 = l.DesP3;
                                        iR.DesA3 = l.DesA3;
                                        iR.QtyRecived3 = l.QtyRecived3;
                                        try
                                        {
                                            iR.IRQuantity = Convert.ToInt32(iR.QtyRecived1 + iR.QtyRecived2 + l.QtyRecived3);
                                        }
                                        catch (Exception es) { }
                                        context.IR_ConfirmDb.Attach(iR);
                                        context.Entry(iR).State = EntityState.Modified;
                                        context.Commit();
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                break;
                            case "IR4":
                                foreach (IR_Confirm m in pom.IrItem)
                                {
                                    double cessamt = Convert.ToDouble((m.Price4 * m.QtyRecived4) * m.CessTaxPercentage / 100);
                                    TotalAmount += m.TtlAmt + cessamt;
                                    gstAmount += m.gstamt;
                                }

                                IRMaster IRM4 = context.IRMasterDB.Where(q => q.Id == pom.Id && q.PurchaseOrderId == i.PurchaseOrderId).SingleOrDefault();
                                IRM4.IRID = pom.IRID;
                                IRM4.TotalAmount = TotalAmount - pom.discount ?? 0;
                                try
                                {
                                    if (pom.OtherAmountType == "ADD")
                                    {
                                        IRM4.TotalAmount = Math.Round(Convert.ToDouble(IRM4.TotalAmount + pom.OtherAmount), 2);
                                    }
                                    else if (pom.OtherAmountType == "MINUS")
                                    {
                                        IRM4.TotalAmount = Math.Round(Convert.ToDouble(IRM4.TotalAmount - pom.OtherAmount), 2);
                                    }
                                    if (pom.ExpenseAmountType == "ADD")
                                    {
                                        IRM4.TotalAmount = Math.Round(Convert.ToDouble(IRM4.TotalAmount + pom.ExpenseAmount), 2);
                                    }
                                    else if (pom.ExpenseAmountType == "MINUS")
                                    {
                                        IRM4.TotalAmount = Math.Round(Convert.ToDouble(IRM4.TotalAmount - pom.ExpenseAmount), 2);
                                    }
                                    if (pom.RoundoffAmountType == "ADD")
                                    {
                                        IRM4.TotalAmount = Math.Round(Convert.ToDouble(IRM4.TotalAmount + pom.RoundofAmount), 2);
                                    }
                                    else if (pom.RoundoffAmountType == "MINUS")
                                    {
                                        IRM4.TotalAmount = Math.Round(Convert.ToDouble(IRM4.TotalAmount - pom.RoundofAmount), 2);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Info(ex.Message);
                                }
                                IRM4.IRAmountWithTax = IRM4.TotalAmount;
                                IRM4.TotalAmountRemaining = IRM4.TotalAmount;
                                IRM4.Gstamt = gstAmount;
                                IRM4.IRStatus = "IR Posted";
                                IRM4.Discount = pom.discount;
                                IRM4.OtherAmount = pom.OtherAmount;
                                IRM4.OtherAmountRemark = pom.OtherAmountRemark;
                                IRM4.OtherAmountType = pom.OtherAmountType;
                                IRM4.ExpenseAmount = pom.ExpenseAmount;
                                IRM4.ExpenseAmountRemark = pom.ExpenseAmountRemark;
                                IRM4.ExpenseAmountType = pom.ExpenseAmountType;
                                IRM4.RoundofAmount = pom.RoundofAmount;
                                IRM4.RoundoffAmountType = pom.RoundoffAmountType;
                                context.IRMasterDB.Attach(IRM4);
                                context.Entry(IRM4).State = EntityState.Modified;
                                context.Commit();

                                PurchaseOrderMaster POM4 = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == IRM4.PurchaseOrderId).SingleOrDefault();
                                POM4.IrStatus = "IR Posted";
                                //context.DPurchaseOrderMaster.Attach(POM4);
                                context.Entry(POM4).State = EntityState.Modified;
                                context.Commit();

                                foreach (IR_Confirm l in pom.IrItem)
                                {
                                    IR_Confirm iR = context.IR_ConfirmDb.Where(a => a.IRreceiveid == l.IRreceiveid).SingleOrDefault();

                                    if (iR != null)
                                    {
                                        iR.Price4 = l.Price4;
                                        iR.dis4 = l.dis4;
                                        iR.TtlAmt = l.TtlAmt;
                                        iR.distype4 = l.distype4;
                                        iR.DesP4 = l.DesP4;
                                        iR.DesA4 = l.DesA4;
                                        iR.QtyRecived4 = l.QtyRecived4;
                                        try
                                        {
                                            iR.IRQuantity = Convert.ToInt32(iR.QtyRecived1 + iR.QtyRecived2 + iR.QtyRecived3 + l.QtyRecived4);
                                        }
                                        catch (Exception es) { }
                                        context.IR_ConfirmDb.Attach(iR);
                                        context.Entry(iR).State = EntityState.Modified;
                                        context.Commit();
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                break;
                            case "IR5":
                                foreach (IR_Confirm m in pom.IrItem)
                                {
                                    double cessamt = Convert.ToDouble((m.Price5 * m.QtyRecived5) * m.CessTaxPercentage / 100);
                                    TotalAmount += m.TtlAmt + cessamt;
                                    gstAmount += m.gstamt;
                                }

                                IRMaster IRM5 = context.IRMasterDB.Where(q => q.Id == pom.Id && q.PurchaseOrderId == i.PurchaseOrderId).SingleOrDefault();
                                IRM5.IRID = pom.IRID;
                                IRM5.TotalAmount = TotalAmount - pom.discount ?? 0;
                                try
                                {
                                    if (pom.OtherAmountType == "ADD")
                                    {
                                        IRM5.TotalAmount = Math.Round(Convert.ToDouble(IRM5.TotalAmount + pom.OtherAmount), 2);
                                    }
                                    else if (pom.OtherAmountType == "MINUS")
                                    {
                                        IRM5.TotalAmount = Math.Round(Convert.ToDouble(IRM5.TotalAmount - pom.OtherAmount), 2);
                                    }
                                    if (pom.ExpenseAmountType == "ADD")
                                    {
                                        IRM5.TotalAmount = Math.Round(Convert.ToDouble(IRM5.TotalAmount + pom.ExpenseAmount), 2);
                                    }
                                    else if (pom.ExpenseAmountType == "MINUS")
                                    {
                                        IRM5.TotalAmount = Math.Round(Convert.ToDouble(IRM5.TotalAmount - pom.ExpenseAmount), 2);
                                    }
                                    if (pom.RoundoffAmountType == "ADD")
                                    {
                                        IRM5.TotalAmount = Math.Round(Convert.ToDouble(IRM5.TotalAmount + pom.RoundofAmount), 2);
                                    }
                                    else if (pom.RoundoffAmountType == "MINUS")
                                    {
                                        IRM5.TotalAmount = Math.Round(Convert.ToDouble(IRM5.TotalAmount - pom.RoundofAmount), 2);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Info(ex.Message);
                                }
                                IRM5.IRAmountWithTax = IRM5.TotalAmount;
                                IRM5.TotalAmountRemaining = IRM5.TotalAmount;
                                IRM5.Gstamt = gstAmount;
                                IRM5.IRStatus = "IR Posted";
                                IRM5.Discount = pom.discount;
                                IRM5.OtherAmount = pom.OtherAmount;
                                IRM5.OtherAmountRemark = pom.OtherAmountRemark;
                                IRM5.OtherAmountType = pom.OtherAmountType;
                                IRM5.ExpenseAmount = pom.ExpenseAmount;
                                IRM5.ExpenseAmountRemark = pom.ExpenseAmountRemark;
                                IRM5.ExpenseAmountType = pom.ExpenseAmountType;
                                IRM5.RoundofAmount = pom.RoundofAmount;
                                IRM5.RoundoffAmountType = pom.RoundoffAmountType;
                                context.IRMasterDB.Attach(IRM5);
                                context.Entry(IRM5).State = EntityState.Modified;
                                context.Commit();

                                PurchaseOrderMaster POM5 = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == IRM5.PurchaseOrderId).SingleOrDefault();
                                POM5.IrStatus = "IR Posted";
                                //context.DPurchaseOrderMaster.Attach(POM5);
                                context.Entry(POM5).State = EntityState.Modified;
                                context.Commit();

                                foreach (IR_Confirm l in pom.IrItem)
                                {
                                    IR_Confirm iR = context.IR_ConfirmDb.Where(a => a.IRreceiveid == l.IRreceiveid).SingleOrDefault();

                                    if (iR != null)
                                    {
                                        iR.Price5 = l.Price5;
                                        iR.dis5 = l.dis5;
                                        iR.TtlAmt = l.TtlAmt;
                                        iR.distype5 = l.distype5;
                                        iR.DesP5 = l.DesP5;
                                        iR.DesA5 = l.DesA5;
                                        iR.QtyRecived5 = l.QtyRecived5;
                                        try
                                        {
                                            iR.IRQuantity = Convert.ToInt32(iR.QtyRecived1 + iR.QtyRecived2 + iR.QtyRecived3 + iR.QtyRecived4 + l.QtyRecived5);
                                        }
                                        catch (Exception es) { }
                                        context.IR_ConfirmDb.Attach(iR);
                                        context.Entry(iR).State = EntityState.Modified;
                                        context.Commit();
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                break;
                            default:
                                return null;
                        }
                        break;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, pom);
                }
                catch (Exception exe)
                {
                    Console.Write(exe.Message);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Excepion: got multiple ir based on single irid.");
                }
            }
        }

        [Route("ReDraftIR")]
        [HttpPut]
        public HttpResponseMessage ReDraftIR(PutIrDTO pom)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    double? TotalAmount = 0;
                    double? gstAmount = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                    foreach (IR_Confirm i in pom.IrItem)
                    {
                        switch (i.IRType)
                        {
                            case "IR1":

                                foreach (IR_Confirm m in pom.IrItem)
                                {
                                    double cessamt = Convert.ToDouble(((m.Price1 * m.QtyRecived1) - m.dis1) * m.CessTaxPercentage / 100);
                                    TotalAmount += m.TtlAmt + cessamt;
                                    gstAmount += m.gstamt;
                                }

                                IRMaster IRM = context.IRMasterDB.Where(q => q.Id == pom.Id && q.PurchaseOrderId == i.PurchaseOrderId).SingleOrDefault();
                                IRM.IRID = pom.IRID;
                                IRM.TotalAmount = TotalAmount - pom.discount ?? 0;
                                try
                                {
                                    if (pom.OtherAmountType == "ADD")
                                    {
                                        IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount + pom.OtherAmount), 2);
                                    }
                                    else if (pom.OtherAmountType == "MINUS")
                                    {
                                        IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount - pom.OtherAmount), 2);
                                    }
                                    if (pom.ExpenseAmountType == "ADD")
                                    {
                                        IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount + pom.ExpenseAmount), 2);
                                    }
                                    else if (pom.ExpenseAmountType == "MINUS")
                                    {
                                        IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount - pom.ExpenseAmount), 2);
                                    }
                                    if (pom.RoundoffAmountType == "ADD")
                                    {
                                        IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount + pom.RoundofAmount), 2);
                                    }
                                    else if (pom.RoundoffAmountType == "MINUS")
                                    {
                                        IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount - pom.RoundofAmount), 2);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Info(ex.Message);
                                }
                                IRM.IRAmountWithTax = IRM.TotalAmount;
                                IRM.TotalAmountRemaining = IRM.TotalAmount;
                                IRM.Gstamt = gstAmount;
                                IRM.IRStatus = "IR Posted as a Draft";
                                IRM.Discount = pom.discount;
                                IRM.OtherAmount = pom.OtherAmount;
                                IRM.OtherAmountRemark = pom.OtherAmountRemark;
                                IRM.OtherAmountType = pom.OtherAmountType;
                                IRM.ExpenseAmount = pom.ExpenseAmount;
                                IRM.ExpenseAmountRemark = pom.ExpenseAmountRemark;
                                IRM.ExpenseAmountType = pom.ExpenseAmountType;
                                IRM.RoundofAmount = pom.RoundofAmount;
                                IRM.RoundoffAmountType = pom.RoundoffAmountType;
                                context.IRMasterDB.Attach(IRM);
                                context.Entry(IRM).State = EntityState.Modified;
                                context.Commit();

                                PurchaseOrderMaster POM = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == IRM.PurchaseOrderId).SingleOrDefault();
                                POM.IrStatus = "IR Posted as a Draft";
                                context.DPurchaseOrderMaster.Attach(POM);
                                context.Entry(POM).State = EntityState.Modified;
                                context.Commit();

                                foreach (IR_Confirm j in pom.IrItem)
                                {
                                    IR_Confirm iR = context.IR_ConfirmDb.Where(a => a.IRreceiveid == j.IRreceiveid).SingleOrDefault();

                                    if (iR != null)
                                    {
                                        iR.Price1 = j.Price1;
                                        iR.dis1 = j.dis1;
                                        iR.TtlAmt = j.TtlAmt;
                                        iR.distype1 = j.distype1;
                                        iR.DesP1 = j.DesP1;
                                        iR.DesA1 = j.DesA1;
                                        iR.QtyRecived1 = j.QtyRecived1;
                                        try
                                        {
                                            iR.IRQuantity = Convert.ToInt32(j.QtyRecived1);
                                        }
                                        catch (Exception ex) { }
                                        context.IR_ConfirmDb.Attach(iR);
                                        context.Entry(iR).State = EntityState.Modified;
                                        context.Commit();
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                break;
                            case "IR2":

                                foreach (IR_Confirm m in pom.IrItem)
                                {
                                    double cessamt = Convert.ToDouble((m.Price2 * m.QtyRecived2) * m.CessTaxPercentage / 100);
                                    TotalAmount += m.TtlAmt + cessamt;
                                    gstAmount += m.gstamt;
                                }

                                IRMaster IRM2 = context.IRMasterDB.Where(q => q.Id == pom.Id && q.PurchaseOrderId == i.PurchaseOrderId).SingleOrDefault();
                                IRM2.IRID = pom.IRID;
                                IRM2.TotalAmount = TotalAmount - pom.discount ?? 0;
                                try
                                {
                                    if (pom.OtherAmountType == "ADD")
                                    {
                                        IRM2.TotalAmount = Math.Round(Convert.ToDouble(IRM2.TotalAmount + pom.OtherAmount), 2);
                                    }
                                    else if (pom.OtherAmountType == "MINUS")
                                    {
                                        IRM2.TotalAmount = Math.Round(Convert.ToDouble(IRM2.TotalAmount - pom.OtherAmount), 2);
                                    }
                                    if (pom.ExpenseAmountType == "ADD")
                                    {
                                        IRM2.TotalAmount = Math.Round(Convert.ToDouble(IRM2.TotalAmount + pom.ExpenseAmount), 2);
                                    }
                                    else if (pom.ExpenseAmountType == "MINUS")
                                    {
                                        IRM2.TotalAmount = Math.Round(Convert.ToDouble(IRM2.TotalAmount - pom.ExpenseAmount), 2);
                                    }
                                    if (pom.RoundoffAmountType == "ADD")
                                    {
                                        IRM2.TotalAmount = Math.Round(Convert.ToDouble(IRM2.TotalAmount + pom.RoundofAmount), 2);
                                    }
                                    else if (pom.RoundoffAmountType == "MINUS")
                                    {
                                        IRM2.TotalAmount = Math.Round(Convert.ToDouble(IRM2.TotalAmount - pom.RoundofAmount), 2);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Info(ex.Message);
                                }
                                IRM2.IRAmountWithTax = IRM2.TotalAmount;
                                IRM2.TotalAmountRemaining = IRM2.TotalAmount;
                                IRM2.Gstamt = gstAmount;
                                IRM2.IRStatus = "IR Posted as a Draft";
                                IRM2.Discount = pom.discount;
                                IRM2.OtherAmount = pom.OtherAmount;
                                IRM2.OtherAmountRemark = pom.OtherAmountRemark;
                                IRM2.OtherAmountType = pom.OtherAmountType;
                                IRM2.ExpenseAmount = pom.ExpenseAmount;
                                IRM2.ExpenseAmountRemark = pom.ExpenseAmountRemark;
                                IRM2.ExpenseAmountType = pom.ExpenseAmountType;
                                IRM2.RoundofAmount = pom.RoundofAmount;
                                IRM2.RoundoffAmountType = pom.RoundoffAmountType;
                                context.IRMasterDB.Attach(IRM2);
                                context.Entry(IRM2).State = EntityState.Modified;
                                context.Commit();

                                PurchaseOrderMaster POM2 = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == IRM2.PurchaseOrderId).SingleOrDefault();
                                POM2.IrStatus = "IR Posted as a Draft";
                                context.DPurchaseOrderMaster.Attach(POM2);
                                context.Entry(POM2).State = EntityState.Modified;
                                context.Commit();

                                foreach (IR_Confirm k in pom.IrItem)
                                {
                                    IR_Confirm iR = context.IR_ConfirmDb.Where(a => a.IRreceiveid == k.IRreceiveid).SingleOrDefault();
                                    if (iR != null)
                                    {
                                        iR.Price2 = k.Price2;
                                        iR.dis2 = k.dis2;
                                        iR.TtlAmt = k.TtlAmt;
                                        iR.distype2 = k.distype2;
                                        iR.DesP2 = k.DesP2;
                                        iR.DesA2 = k.DesA2;
                                        iR.QtyRecived2 = k.QtyRecived2;
                                        try
                                        {
                                            iR.IRQuantity = Convert.ToInt32(iR.QtyRecived1 + k.QtyRecived2);
                                        }
                                        catch (Exception ex) { }
                                        context.IR_ConfirmDb.Attach(iR);
                                        context.Entry(iR).State = EntityState.Modified;
                                        context.Commit();
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                break;
                            case "IR3":

                                foreach (IR_Confirm m in pom.IrItem)
                                {
                                    double cessamt = Convert.ToDouble((m.Price3 * m.QtyRecived3) * m.CessTaxPercentage / 100);
                                    TotalAmount += m.TtlAmt + cessamt;
                                    gstAmount += m.gstamt;
                                }

                                IRMaster IRM3 = context.IRMasterDB.Where(q => q.Id == pom.Id && q.PurchaseOrderId == i.PurchaseOrderId).SingleOrDefault();
                                IRM3.IRID = pom.IRID;
                                IRM3.TotalAmount = TotalAmount - pom.discount ?? 0;
                                try
                                {
                                    if (pom.OtherAmountType == "ADD")
                                    {
                                        IRM3.TotalAmount = Math.Round(Convert.ToDouble(IRM3.TotalAmount + pom.OtherAmount), 2);
                                    }
                                    else if (pom.OtherAmountType == "MINUS")
                                    {
                                        IRM3.TotalAmount = Math.Round(Convert.ToDouble(IRM3.TotalAmount - pom.OtherAmount), 2);
                                    }
                                    if (pom.ExpenseAmountType == "ADD")
                                    {
                                        IRM3.TotalAmount = Math.Round(Convert.ToDouble(IRM3.TotalAmount + pom.ExpenseAmount), 2);
                                    }
                                    else if (pom.ExpenseAmountType == "MINUS")
                                    {
                                        IRM3.TotalAmount = Math.Round(Convert.ToDouble(IRM3.TotalAmount - pom.ExpenseAmount), 2);
                                    }
                                    if (pom.RoundoffAmountType == "ADD")
                                    {
                                        IRM3.TotalAmount = Math.Round(Convert.ToDouble(IRM3.TotalAmount + pom.RoundofAmount), 2);
                                    }
                                    else if (pom.RoundoffAmountType == "MINUS")
                                    {
                                        IRM3.TotalAmount = Math.Round(Convert.ToDouble(IRM3.TotalAmount - pom.RoundofAmount), 2);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Info(ex.Message);
                                }
                                IRM3.IRAmountWithTax = IRM3.TotalAmount;
                                IRM3.TotalAmountRemaining = IRM3.TotalAmount;
                                IRM3.Gstamt = gstAmount;
                                IRM3.IRStatus = "IR Posted as a Draft";
                                IRM3.Discount = pom.discount;
                                IRM3.OtherAmount = pom.OtherAmount;
                                IRM3.OtherAmountRemark = pom.OtherAmountRemark;
                                IRM3.OtherAmountType = pom.OtherAmountType;
                                IRM3.ExpenseAmount = pom.ExpenseAmount;
                                IRM3.ExpenseAmountRemark = pom.ExpenseAmountRemark;
                                IRM3.ExpenseAmountType = pom.ExpenseAmountType;
                                IRM3.RoundofAmount = pom.RoundofAmount;
                                IRM3.RoundoffAmountType = pom.RoundoffAmountType;
                                context.IRMasterDB.Attach(IRM3);
                                context.Entry(IRM3).State = EntityState.Modified;
                                context.Commit();

                                PurchaseOrderMaster POM3 = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == IRM3.PurchaseOrderId).SingleOrDefault();
                                POM3.IrStatus = "IR Posted as a Draft";
                                context.DPurchaseOrderMaster.Attach(POM3);
                                context.Entry(POM3).State = EntityState.Modified;
                                context.Commit();

                                foreach (IR_Confirm l in pom.IrItem)
                                {
                                    IR_Confirm iR = context.IR_ConfirmDb.Where(a => a.IRreceiveid == l.IRreceiveid).SingleOrDefault();

                                    if (iR != null)
                                    {
                                        iR.Price3 = l.Price3;
                                        iR.dis3 = l.dis3;
                                        iR.TtlAmt = l.TtlAmt;
                                        iR.distype3 = l.distype3;
                                        iR.DesP3 = l.DesP3;
                                        iR.DesA3 = l.DesA3;
                                        iR.QtyRecived3 = l.QtyRecived3;
                                        try
                                        {
                                            iR.IRQuantity = Convert.ToInt32(iR.QtyRecived1 + iR.QtyRecived2 + l.QtyRecived3);
                                        }
                                        catch (Exception ex) { }
                                        context.IR_ConfirmDb.Attach(iR);
                                        context.Entry(iR).State = EntityState.Modified;
                                        context.Commit();
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                break;
                            case "IR4":
                                foreach (IR_Confirm m in pom.IrItem)
                                {
                                    double cessamt = Convert.ToDouble((m.Price4 * m.QtyRecived4) * m.CessTaxPercentage / 100);
                                    TotalAmount += m.TtlAmt + cessamt;
                                    gstAmount += m.gstamt;
                                }

                                IRMaster IRM4 = context.IRMasterDB.Where(q => q.Id == pom.Id && q.PurchaseOrderId == i.PurchaseOrderId).SingleOrDefault();
                                IRM4.IRID = pom.IRID;
                                IRM4.TotalAmount = TotalAmount - pom.discount ?? 0;
                                try
                                {
                                    if (pom.OtherAmountType == "ADD")
                                    {
                                        IRM4.TotalAmount = Math.Round(Convert.ToDouble(IRM4.TotalAmount + pom.OtherAmount), 2);
                                    }
                                    else if (pom.OtherAmountType == "MINUS")
                                    {
                                        IRM4.TotalAmount = Math.Round(Convert.ToDouble(IRM4.TotalAmount - pom.OtherAmount), 2);
                                    }
                                    if (pom.ExpenseAmountType == "ADD")
                                    {
                                        IRM4.TotalAmount = Math.Round(Convert.ToDouble(IRM4.TotalAmount + pom.ExpenseAmount), 2);
                                    }
                                    else if (pom.ExpenseAmountType == "MINUS")
                                    {
                                        IRM4.TotalAmount = Math.Round(Convert.ToDouble(IRM4.TotalAmount - pom.ExpenseAmount), 2);
                                    }
                                    if (pom.RoundoffAmountType == "ADD")
                                    {
                                        IRM4.TotalAmount = Math.Round(Convert.ToDouble(IRM4.TotalAmount + pom.RoundofAmount), 2);
                                    }
                                    else if (pom.RoundoffAmountType == "MINUS")
                                    {
                                        IRM4.TotalAmount = Math.Round(Convert.ToDouble(IRM4.TotalAmount - pom.RoundofAmount), 2);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Info(ex.Message);
                                }
                                IRM4.IRAmountWithTax = IRM4.TotalAmount;
                                IRM4.TotalAmountRemaining = IRM4.TotalAmount;
                                IRM4.Gstamt = gstAmount;
                                IRM4.IRStatus = "IR Posted as a Draft";
                                IRM4.Discount = pom.discount;
                                IRM4.OtherAmount = pom.OtherAmount;
                                IRM4.OtherAmountRemark = pom.OtherAmountRemark;
                                IRM4.OtherAmountType = pom.OtherAmountType;
                                IRM4.ExpenseAmount = pom.ExpenseAmount;
                                IRM4.ExpenseAmountRemark = pom.ExpenseAmountRemark;
                                IRM4.ExpenseAmountType = pom.ExpenseAmountType;
                                IRM4.RoundofAmount = pom.RoundofAmount;
                                IRM4.RoundoffAmountType = pom.RoundoffAmountType;
                                context.IRMasterDB.Attach(IRM4);
                                context.Entry(IRM4).State = EntityState.Modified;
                                context.Commit();

                                PurchaseOrderMaster POM4 = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == IRM4.PurchaseOrderId).SingleOrDefault();
                                POM4.IrStatus = "IR Posted as a Draft";
                                context.DPurchaseOrderMaster.Attach(POM4);
                                context.Entry(POM4).State = EntityState.Modified;
                                context.Commit();

                                foreach (IR_Confirm l in pom.IrItem)
                                {
                                    IR_Confirm iR = context.IR_ConfirmDb.Where(a => a.IRreceiveid == l.IRreceiveid).SingleOrDefault();

                                    if (iR != null)
                                    {
                                        iR.Price4 = l.Price4;
                                        iR.dis4 = l.dis4;
                                        iR.TtlAmt = l.TtlAmt;
                                        iR.distype4 = l.distype4;
                                        iR.DesP4 = l.DesP4;
                                        iR.DesA4 = l.DesA4;
                                        iR.QtyRecived4 = l.QtyRecived4;
                                        try
                                        {
                                            iR.IRQuantity = Convert.ToInt32(iR.QtyRecived1 + iR.QtyRecived2 + iR.QtyRecived3 + l.QtyRecived4);
                                        }
                                        catch (Exception es) { }
                                        context.IR_ConfirmDb.Attach(iR);
                                        context.Entry(iR).State = EntityState.Modified;
                                        context.Commit();
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                break;
                            case "IR5":
                                foreach (IR_Confirm m in pom.IrItem)
                                {
                                    double cessamt = Convert.ToDouble((m.Price5 * m.QtyRecived5) * m.CessTaxPercentage / 100);
                                    TotalAmount += m.TtlAmt + cessamt;
                                    gstAmount += m.gstamt;
                                }

                                IRMaster IRM5 = context.IRMasterDB.Where(q => q.Id == pom.Id && q.PurchaseOrderId == i.PurchaseOrderId).SingleOrDefault();
                                IRM5.IRID = pom.IRID;
                                IRM5.TotalAmount = TotalAmount - pom.discount ?? 0;
                                try
                                {
                                    if (pom.OtherAmountType == "ADD")
                                    {
                                        IRM5.TotalAmount = Math.Round(Convert.ToDouble(IRM5.TotalAmount + pom.OtherAmount), 2);
                                    }
                                    else if (pom.OtherAmountType == "MINUS")
                                    {
                                        IRM5.TotalAmount = Math.Round(Convert.ToDouble(IRM5.TotalAmount - pom.OtherAmount), 2);
                                    }
                                    if (pom.ExpenseAmountType == "ADD")
                                    {
                                        IRM5.TotalAmount = Math.Round(Convert.ToDouble(IRM5.TotalAmount + pom.ExpenseAmount), 2);
                                    }
                                    else if (pom.ExpenseAmountType == "MINUS")
                                    {
                                        IRM5.TotalAmount = Math.Round(Convert.ToDouble(IRM5.TotalAmount - pom.ExpenseAmount), 2);
                                    }
                                    if (pom.RoundoffAmountType == "ADD")
                                    {
                                        IRM5.TotalAmount = Math.Round(Convert.ToDouble(IRM5.TotalAmount + pom.RoundofAmount), 2);
                                    }
                                    else if (pom.RoundoffAmountType == "MINUS")
                                    {
                                        IRM5.TotalAmount = Math.Round(Convert.ToDouble(IRM5.TotalAmount - pom.RoundofAmount), 2);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Info(ex.Message);
                                }
                                IRM5.IRAmountWithTax = IRM5.TotalAmount;
                                IRM5.TotalAmountRemaining = IRM5.TotalAmount;
                                IRM5.Gstamt = gstAmount;
                                IRM5.IRStatus = "IR Posted as a Draft";
                                IRM5.Discount = pom.discount;
                                IRM5.OtherAmount = pom.OtherAmount;
                                IRM5.OtherAmountRemark = pom.OtherAmountRemark;
                                IRM5.OtherAmountType = pom.OtherAmountType;
                                IRM5.ExpenseAmount = pom.ExpenseAmount;
                                IRM5.ExpenseAmountRemark = pom.ExpenseAmountRemark;
                                IRM5.ExpenseAmountType = pom.ExpenseAmountType;
                                IRM5.RoundofAmount = pom.RoundofAmount;
                                IRM5.RoundoffAmountType = pom.RoundoffAmountType;
                                context.IRMasterDB.Attach(IRM5);
                                context.Entry(IRM5).State = EntityState.Modified;
                                context.Commit();

                                PurchaseOrderMaster POM5 = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == IRM5.PurchaseOrderId).SingleOrDefault();
                                POM5.IrStatus = "IR Posted as a Draft";
                                context.DPurchaseOrderMaster.Attach(POM5);
                                context.Entry(POM5).State = EntityState.Modified;
                                context.Commit();

                                foreach (IR_Confirm l in pom.IrItem)
                                {
                                    IR_Confirm iR = context.IR_ConfirmDb.Where(a => a.IRreceiveid == l.IRreceiveid && a.PurchaseOrderId == l.PurchaseOrderId).SingleOrDefault();

                                    if (iR != null)
                                    {
                                        iR.Price5 = l.Price5;
                                        iR.dis5 = l.dis5;
                                        iR.TtlAmt = l.TtlAmt;
                                        iR.distype5 = l.distype5;
                                        iR.DesP5 = l.DesP5;
                                        iR.DesA5 = l.DesA5;
                                        iR.QtyRecived5 = l.QtyRecived5;
                                        try
                                        {
                                            iR.IRQuantity = Convert.ToInt32(iR.QtyRecived1 + iR.QtyRecived2 + iR.QtyRecived3 + iR.QtyRecived4 + l.QtyRecived5);
                                        }
                                        catch (Exception es) { }
                                        context.IR_ConfirmDb.Attach(iR);
                                        context.Entry(iR).State = EntityState.Modified;
                                        context.Commit();
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                break;
                            default:
                                return null;
                        }
                        break;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, pom);
                }
                catch (Exception exe)
                {
                    Console.Write(exe.Message);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "got Excepion");
                }
            }
        }


        /// <summary>
        /// Get IR Type based on poid
        /// </summary>
        /// <param name="PurchaseOrderId"></param>
        /// <returns></returns>
        [Authorize]
        [Route("getIRmasterss")]
        [HttpGet]
        public string getIRmasterss(int PurchaseOrderId)
        {
            using (var context = new AuthContext())
            {
                List<IRMaster> ass = new List<IRMaster>();
                try
                {
                    ass = context.IRMasterDB.Where(c => c.PurchaseOrderId == PurchaseOrderId).ToList();

                    string returnvalue;

                    if (ass.Count != 0)
                    {

                        foreach (IRMaster im in ass.OrderByDescending(a => a.Id))
                        {
                            if (im.IRType == "IR4")
                            {
                                return returnvalue = "IR5";
                            }
                            else if (im.IRType == "IR3")
                            {

                                return returnvalue = "IR4";
                            }
                            else if (im.IRType == "IR2")
                            {

                                return returnvalue = "IR3";
                            }
                            else if (im.IRType == "IR1")
                            {
                                return returnvalue = "IR2";
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                    else
                    {
                        return returnvalue = "IR1";
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
        #endregion

        #region Accept Ir from buyer side
        /// <summary>
        /// Created date 26/07/2019
        /// Created by Raj
        /// </summary>
        /// <param name="pom"></param>
        /// <returns></returns>
        [Route("AcceptIR")]
        [HttpPut]
        public HttpResponseMessage AcceptIR(PutIrDTO pom)
        {
            try
            {

                double? TotalAmount = 0;
                double? gstAmount = 0;
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                using (AuthContext context = new AuthContext())
                {
                    foreach (IR_Confirm i in pom.IrItem)
                    {
                        switch (i.IRType)
                        {
                            case "IR1":

                                foreach (IR_Confirm m in pom.IrItem)
                                {
                                    double cessamt = Convert.ToDouble(((m.Price1 * m.QtyRecived1) - m.dis1) * m.CessTaxPercentage / 100);
                                    TotalAmount += m.TtlAmt + cessamt;
                                    gstAmount += m.gstamt;
                                }

                                IRMaster IRM = context.IRMasterDB.Where(q => q.Id == pom.Id && q.PurchaseOrderId == i.PurchaseOrderId).SingleOrDefault();
                                IRM.IRID = pom.IRID;
                                if (pom.discount > 0)
                                {
                                    IRM.TotalAmount = TotalAmount - pom.discount ?? 0;
                                }
                                IRM.Gstamt = gstAmount;
                                IRM.IRStatus = "Approved from Buyer side";
                                if (pom.CashDiscount > 0)
                                {
                                    IRM.TotalAmount = IRM.TotalAmount - pom.CashDiscount ?? 0;
                                }
                                IRM.Discount = pom.discount + pom.CashDiscount??0;
                                IRM.OtherAmount = pom.OtherAmount;
                                IRM.OtherAmountRemark = pom.OtherAmountRemark;
                                IRM.OtherAmountType = pom.OtherAmountType;
                                IRM.ExpenseAmount = pom.ExpenseAmount;
                                IRM.ExpenseAmountRemark = pom.ExpenseAmountRemark;
                                IRM.ExpenseAmountType = pom.ExpenseAmountType;
                                IRM.RoundofAmount = pom.RoundofAmount;
                                IRM.RoundoffAmountType = pom.RoundoffAmountType;
                                IRM.CashDiscount = pom.CashDiscount;

                                try
                                {
                                    if (pom.OtherAmountType == "ADD")
                                    {
                                        IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount + pom.OtherAmount), 2);
                                    }
                                    else if (pom.OtherAmountType == "MINUS")
                                    {
                                        IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount - pom.OtherAmount), 2);
                                    }
                                    if (pom.ExpenseAmountType == "ADD")
                                    {
                                        IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount + pom.ExpenseAmount), 2);
                                    }
                                    else if (pom.ExpenseAmountType == "MINUS")
                                    {
                                        IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount - pom.ExpenseAmount), 2);
                                    }
                                    if (pom.RoundoffAmountType == "ADD")
                                    {
                                        IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount + pom.RoundofAmount), 2);
                                    }
                                    else if (pom.RoundoffAmountType == "MINUS")
                                    {
                                        IRM.TotalAmount = Math.Round(Convert.ToDouble(IRM.TotalAmount - pom.RoundofAmount), 2);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Info(ex.Message);
                                }

                                IRM.IRAmountWithTax = IRM.TotalAmount;
                                IRM.TotalAmountRemaining = IRM.TotalAmount;
                                context.Entry(IRM).State = EntityState.Modified;
                                context.Commit();

                                PurchaseOrderMaster POM = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == IRM.PurchaseOrderId).SingleOrDefault();
                                POM.IrStatus = "Approved from Buyer side";
                                //context.DPurchaseOrderMaster.Attach(POM);
                                context.Entry(POM).State = EntityState.Modified;
                                context.Commit();

                                foreach (IR_Confirm j in pom.IrItem)
                                {
                                    IR_Confirm iR = context.IR_ConfirmDb.Where(a => a.IRreceiveid == j.IRreceiveid).SingleOrDefault();

                                    if (iR != null)
                                    {
                                        iR.Price1 = j.Price1;
                                        iR.dis1 = j.dis1;
                                        iR.TtlAmt = j.TtlAmt;
                                        iR.distype1 = j.distype1;
                                        iR.DesP1 = j.DesP1;
                                        iR.DesA1 = j.DesA1;
                                        iR.QtyRecived1 = j.QtyRecived1;
                                        try
                                        {
                                            iR.IRQuantity = Convert.ToInt32(j.QtyRecived1);
                                        }
                                        catch (Exception es) { }
                                        context.Entry(iR).State = EntityState.Modified;
                                        context.Commit();
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                break;
                            case "IR2":

                                foreach (IR_Confirm m in pom.IrItem)
                                {
                                    double cessamt = Convert.ToDouble((m.Price2 * m.QtyRecived2) * m.CessTaxPercentage / 100);
                                    TotalAmount += m.TtlAmt + cessamt;
                                    gstAmount += m.gstamt;
                                }

                                IRMaster IRM2 = context.IRMasterDB.Where(q => q.Id == pom.Id && q.PurchaseOrderId == i.PurchaseOrderId).SingleOrDefault();
                                IRM2.IRID = pom.IRID;
                                IRM2.TotalAmount = TotalAmount - pom.discount ?? 0;
                                IRM2.Gstamt = gstAmount;
                                IRM2.IRStatus = "Approved from Buyer side";
                                IRM2.Discount = pom.discount;
                                IRM2.OtherAmount = pom.OtherAmount;
                                IRM2.OtherAmountRemark = pom.OtherAmountRemark;
                                IRM2.OtherAmountType = pom.OtherAmountType;
                                IRM2.ExpenseAmount = pom.ExpenseAmount;
                                IRM2.ExpenseAmountRemark = pom.ExpenseAmountRemark;
                                IRM2.ExpenseAmountType = pom.ExpenseAmountType;
                                IRM2.RoundofAmount = pom.RoundofAmount;
                                IRM2.RoundoffAmountType = pom.RoundoffAmountType;
                                IRM2.CashDiscount = pom.CashDiscount;
                                context.IRMasterDB.Attach(IRM2);


                                try
                                {
                                    if (pom.OtherAmountType == "ADD")
                                    {
                                        IRM2.TotalAmount = Math.Round(Convert.ToDouble(IRM2.TotalAmount + pom.OtherAmount), 2);
                                    }
                                    else if (pom.OtherAmountType == "MINUS")
                                    {
                                        IRM2.TotalAmount = Math.Round(Convert.ToDouble(IRM2.TotalAmount - pom.OtherAmount), 2);
                                    }
                                    if (pom.ExpenseAmountType == "ADD")
                                    {
                                        IRM2.TotalAmount = Math.Round(Convert.ToDouble(IRM2.TotalAmount + pom.ExpenseAmount), 2);
                                    }
                                    else if (pom.ExpenseAmountType == "MINUS")
                                    {
                                        IRM2.TotalAmount = Math.Round(Convert.ToDouble(IRM2.TotalAmount - pom.ExpenseAmount), 2);
                                    }
                                    if (pom.RoundoffAmountType == "ADD")
                                    {
                                        IRM2.TotalAmount = Math.Round(Convert.ToDouble(IRM2.TotalAmount + pom.RoundofAmount), 2);
                                    }
                                    else if (pom.RoundoffAmountType == "MINUS")
                                    {
                                        IRM2.TotalAmount = Math.Round(Convert.ToDouble(IRM2.TotalAmount - pom.RoundofAmount), 2);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Info(ex.Message);
                                }
                                IRM2.IRAmountWithTax = IRM2.TotalAmount;
                                IRM2.TotalAmountRemaining = IRM2.TotalAmount;
                                context.Entry(IRM2).State = EntityState.Modified;
                                context.Commit();

                                PurchaseOrderMaster POM2 = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == IRM2.PurchaseOrderId).SingleOrDefault();
                                POM2.IrStatus = "Approved from Buyer side";
                                //context.DPurchaseOrderMaster.Attach(POM2);
                                context.Entry(POM2).State = EntityState.Modified;
                                context.Commit();

                                foreach (IR_Confirm k in pom.IrItem)
                                {
                                    IR_Confirm iR = context.IR_ConfirmDb.Where(a => a.IRreceiveid == k.IRreceiveid).SingleOrDefault();

                                    if (iR != null)
                                    {
                                        iR.Price2 = k.Price2;
                                        iR.dis2 = k.dis2;
                                        iR.TtlAmt = k.TtlAmt;
                                        iR.distype2 = k.distype2;
                                        iR.DesP2 = k.DesP2;
                                        iR.DesA2 = k.DesA2;
                                        iR.QtyRecived2 = k.QtyRecived2;
                                        try
                                        {
                                            iR.IRQuantity = Convert.ToInt32(iR.QtyRecived1 + k.QtyRecived2);
                                        }
                                        catch (Exception es) { }
                                        context.IR_ConfirmDb.Attach(iR);
                                        context.Entry(iR).State = EntityState.Modified;
                                        context.Commit();
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                break;
                            case "IR3":

                                foreach (IR_Confirm m in pom.IrItem)
                                {
                                    double cessamt = Convert.ToDouble((m.Price3 * m.QtyRecived3) * m.CessTaxPercentage / 100);
                                    TotalAmount += m.TtlAmt + cessamt;
                                    gstAmount += m.gstamt;
                                }

                                IRMaster IRM3 = context.IRMasterDB.Where(q => q.Id == pom.Id && q.PurchaseOrderId == i.PurchaseOrderId).SingleOrDefault();
                                IRM3.IRID = pom.IRID;
                                IRM3.TotalAmount = TotalAmount - pom.discount ?? 0;

                                IRM3.Gstamt = gstAmount;
                                IRM3.IRStatus = "Approved from Buyer side";
                                IRM3.Discount = pom.discount;
                                IRM3.OtherAmount = pom.OtherAmount;
                                IRM3.OtherAmountRemark = pom.OtherAmountRemark;
                                IRM3.OtherAmountType = pom.OtherAmountType;
                                IRM3.ExpenseAmount = pom.ExpenseAmount;
                                IRM3.ExpenseAmountRemark = pom.ExpenseAmountRemark;
                                IRM3.ExpenseAmountType = pom.ExpenseAmountType;
                                IRM3.RoundofAmount = pom.RoundofAmount;
                                IRM3.RoundoffAmountType = pom.RoundoffAmountType;
                                IRM3.CashDiscount = pom.CashDiscount;
                                context.IRMasterDB.Attach(IRM3);

                                try
                                {
                                    if (pom.OtherAmountType == "ADD")
                                    {
                                        IRM3.TotalAmount = Math.Round(Convert.ToDouble(IRM3.TotalAmount + pom.OtherAmount), 2);
                                    }
                                    else if (pom.OtherAmountType == "MINUS")
                                    {
                                        IRM3.TotalAmount = Math.Round(Convert.ToDouble(IRM3.TotalAmount - pom.OtherAmount), 2);
                                    }
                                    if (pom.ExpenseAmountType == "ADD")
                                    {
                                        IRM3.TotalAmount = Math.Round(Convert.ToDouble(IRM3.TotalAmount + pom.ExpenseAmount), 2);
                                    }
                                    else if (pom.ExpenseAmountType == "MINUS")
                                    {
                                        IRM3.TotalAmount = Math.Round(Convert.ToDouble(IRM3.TotalAmount - pom.ExpenseAmount), 2);
                                    }
                                    if (pom.RoundoffAmountType == "ADD")
                                    {
                                        IRM3.TotalAmount = Math.Round(Convert.ToDouble(IRM3.TotalAmount + pom.RoundofAmount), 2);
                                    }
                                    else if (pom.RoundoffAmountType == "MINUS")
                                    {
                                        IRM3.TotalAmount = Math.Round(Convert.ToDouble(IRM3.TotalAmount - pom.RoundofAmount), 2);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Info(ex.Message);
                                }
                                IRM3.IRAmountWithTax = IRM3.TotalAmount;
                                IRM3.TotalAmountRemaining = IRM3.TotalAmount;
                                context.Entry(IRM3).State = EntityState.Modified;
                                context.Commit();

                                PurchaseOrderMaster POM3 = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == IRM3.PurchaseOrderId).SingleOrDefault();
                                POM3.IrStatus = "Approved from Buyer side";
                                //context.DPurchaseOrderMaster.Attach(POM3);
                                context.Entry(POM3).State = EntityState.Modified;
                                context.Commit();

                                foreach (IR_Confirm l in pom.IrItem)
                                {
                                    IR_Confirm iR = context.IR_ConfirmDb.Where(a => a.IRreceiveid == l.IRreceiveid).SingleOrDefault();

                                    if (iR != null)
                                    {
                                        iR.Price3 = l.Price3;
                                        iR.dis3 = l.dis3;
                                        iR.TtlAmt = l.TtlAmt;
                                        iR.distype3 = l.distype3;
                                        iR.DesP3 = l.DesP3;
                                        iR.DesA3 = l.DesA3;
                                        iR.QtyRecived3 = l.QtyRecived3;
                                        try
                                        {
                                            iR.IRQuantity = Convert.ToInt32(iR.QtyRecived1 + iR.QtyRecived2 + l.QtyRecived3);
                                        }
                                        catch (Exception es) { }
                                        context.IR_ConfirmDb.Attach(iR);
                                        context.Entry(iR).State = EntityState.Modified;
                                        context.Commit();
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                break;
                            case "IR4":
                                foreach (IR_Confirm m in pom.IrItem)
                                {
                                    double cessamt = Convert.ToDouble((m.Price4 * m.QtyRecived4) * m.CessTaxPercentage / 100);
                                    TotalAmount += m.TtlAmt + cessamt;
                                    gstAmount += m.gstamt;
                                }

                                IRMaster IRM4 = context.IRMasterDB.Where(q => q.Id == pom.Id && q.PurchaseOrderId == i.PurchaseOrderId).SingleOrDefault();
                                IRM4.IRID = pom.IRID;
                                IRM4.TotalAmount = TotalAmount - pom.discount ?? 0;

                                IRM4.Gstamt = gstAmount;
                                IRM4.IRStatus = "Approved from Buyer side";
                                IRM4.Discount = pom.discount;
                                IRM4.OtherAmount = pom.OtherAmount;
                                IRM4.OtherAmountRemark = pom.OtherAmountRemark;
                                IRM4.OtherAmountType = pom.OtherAmountType;
                                IRM4.ExpenseAmount = pom.ExpenseAmount;
                                IRM4.ExpenseAmountRemark = pom.ExpenseAmountRemark;
                                IRM4.ExpenseAmountType = pom.ExpenseAmountType;
                                IRM4.RoundofAmount = pom.RoundofAmount;
                                IRM4.RoundoffAmountType = pom.RoundoffAmountType;
                                IRM4.CashDiscount = pom.CashDiscount;
                                context.IRMasterDB.Attach(IRM4);
                                try
                                {
                                    if (pom.OtherAmountType == "ADD")
                                    {
                                        IRM4.TotalAmount = Math.Round(Convert.ToDouble(IRM4.TotalAmount + pom.OtherAmount), 2);
                                    }
                                    else if (pom.OtherAmountType == "MINUS")
                                    {
                                        IRM4.TotalAmount = Math.Round(Convert.ToDouble(IRM4.TotalAmount - pom.OtherAmount), 2);
                                    }
                                    if (pom.ExpenseAmountType == "ADD")
                                    {
                                        IRM4.TotalAmount = Math.Round(Convert.ToDouble(IRM4.TotalAmount + pom.ExpenseAmount), 2);
                                    }
                                    else if (pom.ExpenseAmountType == "MINUS")
                                    {
                                        IRM4.TotalAmount = Math.Round(Convert.ToDouble(IRM4.TotalAmount - pom.ExpenseAmount), 2);
                                    }
                                    if (pom.RoundoffAmountType == "ADD")
                                    {
                                        IRM4.TotalAmount = Math.Round(Convert.ToDouble(IRM4.TotalAmount + pom.RoundofAmount), 2);
                                    }
                                    else if (pom.RoundoffAmountType == "MINUS")
                                    {
                                        IRM4.TotalAmount = Math.Round(Convert.ToDouble(IRM4.TotalAmount - pom.RoundofAmount), 2);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Info(ex.Message);
                                }
                                IRM4.IRAmountWithTax = IRM4.TotalAmount;
                                IRM4.TotalAmountRemaining = IRM4.TotalAmount;
                                context.Entry(IRM4).State = EntityState.Modified;
                                context.Commit();

                                PurchaseOrderMaster POM4 = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == IRM4.PurchaseOrderId).SingleOrDefault();
                                POM4.IrStatus = "Approved from Buyer side";
                                //context.DPurchaseOrderMaster.Attach(POM4);
                                context.Entry(POM4).State = EntityState.Modified;
                                context.Commit();

                                foreach (IR_Confirm l in pom.IrItem)
                                {
                                    IR_Confirm iR = context.IR_ConfirmDb.Where(a => a.IRreceiveid == l.IRreceiveid).SingleOrDefault();

                                    if (iR != null)
                                    {
                                        iR.Price4 = l.Price4;
                                        iR.dis4 = l.dis4;
                                        iR.TtlAmt = l.TtlAmt;
                                        iR.distype4 = l.distype4;
                                        iR.DesP4 = l.DesP4;
                                        iR.DesA4 = l.DesA4;
                                        iR.QtyRecived4 = l.QtyRecived4;
                                        try
                                        {
                                            iR.IRQuantity = Convert.ToInt32(iR.QtyRecived1 + iR.QtyRecived2 + iR.QtyRecived3 + l.QtyRecived4);
                                        }
                                        catch (Exception es) { }
                                        context.IR_ConfirmDb.Attach(iR);
                                        context.Entry(iR).State = EntityState.Modified;
                                        context.Commit();
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                break;
                            case "IR5":
                                foreach (IR_Confirm m in pom.IrItem)
                                {
                                    double cessamt = Convert.ToDouble((m.Price5 * m.QtyRecived5) * m.CessTaxPercentage / 100);
                                    TotalAmount += m.TtlAmt + cessamt;
                                    gstAmount += m.gstamt;
                                }

                                IRMaster IRM5 = context.IRMasterDB.Where(q => q.Id == pom.Id && q.PurchaseOrderId == i.PurchaseOrderId).SingleOrDefault();
                                IRM5.IRID = pom.IRID;
                                IRM5.TotalAmount = TotalAmount - pom.discount ?? 0;
                                IRM5.Gstamt = gstAmount;
                                IRM5.IRStatus = "Approved from Buyer side";
                                IRM5.Discount = pom.discount;
                                IRM5.OtherAmount = pom.OtherAmount;
                                IRM5.OtherAmountRemark = pom.OtherAmountRemark;
                                IRM5.OtherAmountType = pom.OtherAmountType;
                                IRM5.ExpenseAmount = pom.ExpenseAmount;
                                IRM5.ExpenseAmountRemark = pom.ExpenseAmountRemark;
                                IRM5.ExpenseAmountType = pom.ExpenseAmountType;
                                IRM5.RoundofAmount = pom.RoundofAmount;
                                IRM5.RoundoffAmountType = pom.RoundoffAmountType;
                                IRM5.CashDiscount = pom.CashDiscount;
                                context.IRMasterDB.Attach(IRM5);

                                try
                                {
                                    if (pom.OtherAmountType == "ADD")
                                    {
                                        IRM5.TotalAmount = Math.Round(Convert.ToDouble(IRM5.TotalAmount + pom.OtherAmount), 2);
                                    }
                                    else if (pom.OtherAmountType == "MINUS")
                                    {
                                        IRM5.TotalAmount = Math.Round(Convert.ToDouble(IRM5.TotalAmount - pom.OtherAmount), 2);
                                    }
                                    if (pom.ExpenseAmountType == "ADD")
                                    {
                                        IRM5.TotalAmount = Math.Round(Convert.ToDouble(IRM5.TotalAmount + pom.ExpenseAmount), 2);
                                    }
                                    else if (pom.ExpenseAmountType == "MINUS")
                                    {
                                        IRM5.TotalAmount = Math.Round(Convert.ToDouble(IRM5.TotalAmount - pom.ExpenseAmount), 2);
                                    }
                                    if (pom.RoundoffAmountType == "ADD")
                                    {
                                        IRM5.TotalAmount = Math.Round(Convert.ToDouble(IRM5.TotalAmount + pom.RoundofAmount), 2);
                                    }
                                    else if (pom.RoundoffAmountType == "MINUS")
                                    {
                                        IRM5.TotalAmount = Math.Round(Convert.ToDouble(IRM5.TotalAmount - pom.RoundofAmount), 2);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Info(ex.Message);
                                }
                                IRM5.IRAmountWithTax = IRM5.TotalAmount;
                                IRM5.TotalAmountRemaining = IRM5.TotalAmount;
                                context.Entry(IRM5).State = EntityState.Modified;
                                context.Commit();

                                PurchaseOrderMaster POM5 = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == IRM5.PurchaseOrderId).SingleOrDefault();
                                POM5.IrStatus = "Approved from Buyer side";
                                //context.DPurchaseOrderMaster.Attach(POM5);
                                context.Entry(POM5).State = EntityState.Modified;
                                context.Commit();

                                foreach (IR_Confirm l in pom.IrItem)
                                {
                                    IR_Confirm iR = context.IR_ConfirmDb.Where(a => a.IRreceiveid == l.IRreceiveid).SingleOrDefault();

                                    if (iR != null)
                                    {
                                        iR.Price5 = l.Price5;
                                        iR.dis5 = l.dis5;
                                        iR.TtlAmt = l.TtlAmt;
                                        iR.distype5 = l.distype5;
                                        iR.DesP5 = l.DesP5;
                                        iR.DesA5 = l.DesA5;
                                        iR.QtyRecived5 = l.QtyRecived5;
                                        try
                                        {
                                            iR.IRQuantity = Convert.ToInt32(iR.QtyRecived1 + iR.QtyRecived2 + iR.QtyRecived3 + iR.QtyRecived4 + l.QtyRecived5);
                                        }
                                        catch (Exception es) { }
                                        context.IR_ConfirmDb.Attach(iR);
                                        context.Entry(iR).State = EntityState.Modified;
                                        context.Commit();
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                break;
                            default:
                                return null;
                        }
                        pom.PurchaseOrderId = i.PurchaseOrderId;
                        IRHelper.UpdateSupplierData(pom, compid, context, userid);

                        break;
                    }

                }
                return Request.CreateResponse(HttpStatusCode.OK, pom);
            }
            catch (Exception exe)
            {
                Console.Write(exe.Message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Excepion: got multiple ir based on single irid.");
            }
        }

        #endregion

        #region IR To Payment
        /// <summary>
        /// Created date 27/07/2019
        /// Created by Raj
        /// </summary>
        /// <param name="pom"></param>
        /// <returns></returns>
        [Route("PaymentIR")]
        [HttpPost]
        public HttpResponseMessage PaymentIR(PaymentIrDTO details)
        {
            try
            {

                double? TotalAmount = 0;
                double? gstAmount = 0;
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                using (AuthContext context = new AuthContext())
                {
                    IRMaster irdata = context.IRMasterDB.Where(x => x.PurchaseOrderId == details.PurchaseOrderId && x.IRID == details.IRID && x.Deleted == false).FirstOrDefault();
                    var amount = irdata.TotalAmount;
                    IRHelper.DebitLedgerEntry(irdata.supplierId, context, amount, irdata.IRID, irdata.Id, userid, irdata.Discount, "IR");


                    SupplierPaymentData paydata = new SupplierPaymentData();
                    paydata.PurchaseOrderId = irdata.PurchaseOrderId;
                    paydata.InVoiceNumber = irdata.IRID;
                    paydata.CreditInVoiceAmount = irdata.TotalAmount;
                    paydata.PaymentStatusCorD = "Credit";
                    paydata.VoucherType = "Purchase";
                    paydata.ClosingBalance = irdata.TotalAmount;
                    paydata.CompanyId = 1;
                    paydata.WarehouseId = irdata.WarehouseId;
                    paydata.InVoiceDate = irdata.CreationDate;
                    paydata.SupplierId = irdata.supplierId;
                    paydata.SupplierName = irdata.SupplierName;
                    paydata.CreatedDate = DateTime.Now;
                    paydata.UpdatedDate = DateTime.Now;
                    context.SupplierPaymentDataDB.Add(paydata);
                    FullSupplierPaymentData suppdata = context.FullSupplierPaymentDataDB.Where(x => x.SupplierId == irdata.supplierId && x.Deleted == false).FirstOrDefault();
                    if (suppdata != null)
                    {
                        suppdata.InVoiceRemainingAmount = suppdata.InVoiceRemainingAmount - irdata.TotalAmount;
                        if (suppdata.InVoiceRemainingAmount == 0)
                        {
                            suppdata.SupplierPaymentStatus = "Full Paid";

                        }
                        else
                        {
                            suppdata.SupplierPaymentStatus = "Partial Paid";

                        }
                        context.Entry(suppdata).State = EntityState.Modified;
                        irdata.PaymentStatus = "Paid";
                        irdata.IRStatus = "Paid";
                    }
                    context.Entry(irdata).State = EntityState.Modified;
                    context.Commit();



                }
                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            catch (Exception exe)
            {
                Console.Write(exe.Message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Excepion: got multiple ir based on single irid.");
            }
        }

        [Route("IsIgst")]
        [HttpGet]
        public bool IsIgst(int PurchaseOrderId)
        {
            PurchaseOrderMaster pm = new PurchaseOrderMaster();
            using (AuthContext context = new AuthContext())
            {
                pm = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == PurchaseOrderId).SingleOrDefault();
                if (pm.DepoId > 0)
                {
                    DepoMaster DepoDetail = context.DepoMasters.Where(a => a.DepoId == pm.DepoId).SingleOrDefault();

                    //for igst case if true then apply condion to hide column of cgst sgst cess
                    if (!string.IsNullOrEmpty(DepoDetail.GSTin) && DepoDetail.GSTin.Length >= 11)
                    {
                        string DepoTin_No = DepoDetail.GSTin.Substring(0, 2);

                        //if (!DepoTin_No.StartsWith("0"))
                        //{
                        pm.IsIgstIR = !context.Warehouses.Any(x => x.GSTin != null && x.WarehouseId == pm.WarehouseId && x.GSTin.Substring(0, 2) == DepoTin_No);
                        // }
                    }
                }
            }
            return pm.IsIgstIR;
        }
        #endregion

        #region IR dashboard
        [Route("IRDashboard")]
        [HttpGet]
        public IRDashboarObject IRDashboard(DateTime? start, DateTime? end, int wid, int? SupplierId)
        {
            List<IRMaster> IRMasteData = new List<IRMaster>();
            IRDashboarObject Data = new IRDashboarObject();
            using (AuthContext db = new AuthContext())
            {
                if (wid > 0) {
                    if (SupplierId > 0 && start != null && end != null)
                    {
                        IRMasteData = db.IRMasterDB.Where(a => a.CreationDate > start && a.CreationDate < end && a.WarehouseId == wid && a.supplierId == SupplierId && a.Deleted == false).ToList();
                    }
                    else
                    {
                        IRMasteData = db.IRMasterDB.Where(a => a.WarehouseId == wid && a.Deleted == false).ToList();
                    }
                }


                if (IRMasteData != null && IRMasteData.Any())
                {
                    Data.IRPosted = IRMasteData.Count();
                    Data.IRPostedAmount = IRMasteData.Sum(a => a.TotalAmount);
                    Data.IRApproved = IRMasteData.Where(a => a.IRStatus == "Approved from Buyer side").Count();
                    Data.IRApprovedAmount = IRMasteData.Where(a => a.IRStatus == "Approved from Buyer side").Sum(a => a.TotalAmount);
                    Data.IRPending = IRMasteData.Where(a => a.IRStatus == "Pending from Buyer side").Count();
                    Data.IRPendingAmount = IRMasteData.Where(a => a.IRStatus == "Pending from Buyer side").Sum(a => a.TotalAmount);
                    Data.IRUploded = IRMasteData.Where(a => a.IRStatus == "IR Uploaded").Count();
                    Data.IRUplodedAmount = IRMasteData.Where(a => a.IRStatus == "IR Uploaded").Sum(a => a.TotalAmount);
                    Data.IRRejected = IRMasteData.Where(a => a.IRStatus == "Rejected from Buyer side").Count();
                    Data.IRRejectedAmount = IRMasteData.Where(a => a.IRStatus == "Rejected from Buyer side").Sum(a => a.TotalAmount);
                    Data.IRMasterList = IRMasteData;
                }
            }
            return Data;
        }
        #endregion

        #region Get Drafted GR
        /// <summary>
        /// created by 22/04/2019
        /// Get IR master Detail where IRID is this
        /// </summary>
        /// <param name="irid"></param>
        /// <returns></returns>
        [Authorize]
        [Route("GetdraftedGR")]
        public HttpResponseMessage GetdraftedGR(string GRID)
        {
            using (var context = new AuthContext())
            {
                GrDraftInvoice ass = new GrDraftInvoice();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0; int WarehouseId = 0;

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
                        if (claim.Type == "Warehouseid")
                        {
                            WarehouseId = int.Parse(claim.Value);
                        }
                    }
                    ass = context.GrDraftInvoiceDB.Where(c => c.GrNumber == GRID).FirstOrDefault();

                    logger.Info("End  Return: ");
                    return Request.CreateResponse(HttpStatusCode.OK, ass);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        #endregion


        #region
        [HttpGet]
        [Route("SearchIR")]
        public List<IRMaster> GetIRReca(int PurchaseOrderId)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start : ");
                List<IRMaster> IRRec = new List<IRMaster>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                        Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                    int CompanyId = compid;

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    IRRec = context.IRMasterDB.AsEnumerable().Where(x => x.PurchaseOrderId == PurchaseOrderId).Where(a => a.IRStatus == "Pending from Buyer side" || a.IRStatus == "Approved from Buyer side" || a.IRStatus == "Rejected from Buyer side").OrderByDescending(x => x.PurchaseOrderId).ToList();
                    logger.Info("End  : ");
                    return IRRec;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                    logger.Info("End  PurchaseOrderDetail: ");
                    return null;
                }
            }
        }

        #endregion

        #region
        [HttpGet]
        [Route("SearchRejectedIR")]
        public List<IRMaster> GetRejectedIR(int PurchaseOrderId)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start : ");
                List<IRMaster> IRRec = new List<IRMaster>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                        Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                    int CompanyId = compid;

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    IRRec = context.IRMasterDB.AsEnumerable().Where(x => x.PurchaseOrderId == PurchaseOrderId).Where(a => a.IRStatus == "Rejected from Buyer side").OrderByDescending(x => x.PurchaseOrderId).ToList();
                    logger.Info("End  : ");
                    return IRRec;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                    logger.Info("End  PurchaseOrderDetail: ");
                    return null;
                }
            }
        }


        #endregion

        [Authorize]
        [Route("GetIrOutstandingList")]
        [HttpPost]
        public async Task<IrOutstandingListDC> GetIrOutstandingList(IrOutstandingPaginator paginator)
        {
            IrOutstandingHelper helper = new IrOutstandingHelper();
            IrOutstandingListDC dc = await helper.GetListAsync(paginator);
            return dc;
        }

        [Authorize]
        [Route("GetIRStaus")]
        [HttpGet]
        public async Task<List<string>> GetIRStaus()
        {
            using(var context = new AuthContext())
            {
                var statusList = await context.IRMasterDB.Where(x => !string.IsNullOrEmpty(x.IRStatus)).Select(m => m.IRStatus).Distinct().ToListAsync();
                return statusList;
            }
        }

        [Authorize]
        [Route("GetIrOutstandingViewList")]
        [HttpPost]
        public async Task<IrOutstandingViewListDC> GetIrOutstandingViewList(IrOutstandingViewPaginator paginator)
        {
            IrOutstandingHelper helper = new IrOutstandingHelper();
            IrOutstandingViewListDC dc = await helper.GetViewListAsync(paginator);
            return dc;
        }
        [Authorize]
        [Route("IRPaymentSummariesGet")]
        [HttpPost]
        public async Task<IrPaymentSummaryListDC> IRPaymentSummariesGet(IrPaymentSummaryPaginator paginator)
        {
            IrOutstandingHelper helper = new IrOutstandingHelper();
            IrPaymentSummaryListDC dc = await helper.GetPaymentSummaryListAsync(paginator);
            return dc;
        }

        [Route("GetBySummaryId/{irPaymentSummaryId}")]
        [HttpGet]
        public List<IRPaymentDetailsDC> GetBySummaryId(int irPaymentSummaryId)
        {
            IrOutstandingHelper helper = new IrOutstandingHelper();
            var list =  helper.GetBySummaryId(irPaymentSummaryId);
            return list;
        }


        [Route("Status")]
        [HttpGet]
        public HttpResponseMessage GetIr(int Warehouseid, string value)
        {
            logger.Info("start ItemMaster: ");
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                int CompanyId = compid;
                List<IRMaster> objs = new List<IRMaster>();
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouse_id);

                using (var context = new AuthContext())
                {
                    objs = context.IRMasterDB.Where(x => x.WarehouseId == Warehouseid && x.BuyerId == userid && x.IRStatus == value).OrderByDescending(x => x.PurchaseOrderId).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, objs);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in ItemMaster " + ex.Message);
                logger.Info("End  ItemMaster: ");
                return null;
            }
        }


        [Route("StatusFilter")]
        [HttpPost]
        public HttpResponseMessage Getstatuswise(objDTIR obj)
        {
            logger.Info("start ItemMaster: ");
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                int CompanyId = compid;

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouse_id);
                using (var context = new AuthContext())
                {
                    List<IRMaster> objs = new List<IRMaster>();
                    if (obj.WHID > 0 && obj.From != null)
                    {
                        var newdatareport = context.IRMasterDB.Where(x => x.WarehouseId == obj.WHID && obj.status.Contains(x.IRStatus) && x.BuyerId == userid && x.CreationDate > obj.From && x.CreationDate <= obj.TO).OrderByDescending(x => x.PurchaseOrderId).Take(1000).ToList();
                        objs.AddRange(newdatareport);

                    }
                    else
                    {
                        var newdatareport = context.IRMasterDB.Where(x => x.WarehouseId == obj.WHID && obj.status.Contains(x.IRStatus) && x.BuyerId == userid).OrderByDescending(x => x.PurchaseOrderId).Take(1000).ToList();
                        objs.AddRange(newdatareport);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, objs);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in ItemMaster " + ex.Message);
                logger.Info("End  ItemMaster: ");
                return null;
            }
        }


        [Route("updatebankname")]
        [HttpGet]
        public string UpdateBankName(int Id, int BankId)
        {
            try
            {
                string result = "";
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                int CompanyId = compid;

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouse_id);
                using (var context = new AuthContext())
                {
                    var data = context.IRPaymentDetailsDB.Where(x => x.Id == Id).FirstOrDefault();

                    if (data != null)
                    {
                        var dataa = context.IRPaymentDetailsDB.Where(x => x.IRPaymentSummaryId == data.IRPaymentSummaryId && x.PaymentStatus == "Pending" && x.IsActive == true && x.Deleted == false && x.IsIROutstandingPending == true).ToList();
                        if (dataa.Count > 0)
                        {
                            foreach (var d in dataa)
                            {
                                d.BankId = BankId;
                                d.Updateby = userid;
                                d.UpdateDate = DateTime.Now;

                                //data.BankName = BankName;
                                context.Entry(d).State = EntityState.Modified;
                                context.Commit();
                                result = "Updated";
                            }

                        }
                        else
                        {
                            result = "Data Not Exists";
                        }

                    }
                    else
                    {
                        result = "Data Not Exists";
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in ItemMaster " + ex.Message);
                logger.Info("End  ItemMaster: ");
                return null;
            }
        }


       

    }
    #region DTO Class
    public class IRDashboarObject
    {
        //public string Type { get; set; }
        //public int? TotalPO { get; set; }
        //public double? TotalPOAmount { get; set; }
        //public int? GRDone { get; set; }
        //public double? GRDoneAmount { get; set; }
        public int? IRPosted { get; set; }
        public double? IRPostedAmount { get; set; }
        public int? IRUploded { get; set; }
        public double? IRUplodedAmount { get; set; }
        public int? IRPending { get; set; }
        public double? IRPendingAmount { get; set; }
        public int? IRApproved { get; set; }
        public double? IRApprovedAmount { get; set; }
        public int? IRRejected { get; set; }
        public double? IRRejectedAmount { get; set; }

        public List<IRMaster> IRMasterList { get; set; }
    }
    public class Buyerchange
    {
        public int PurchaseOrderId { get; set; }
        public int PeopleID { get; set; }
    }
    public class InvoiceImage
    {
        [Key]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string InvoiceNumber { get; set; }
        public int PurchaseOrderId { get; set; }
        public int WarehouseId { get; set; }
        public double IRAmount { get; set; }
        public string IRLogoURL { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime InvoiceDate
        {
            get; set;
        }
        public double GSTPercentage
        {
            get; set;
        }
        public double GSTAmount
        {
            get; set;
        }
        public double CGSTPercentage
        {
            get; set;
        }
        public double CGSTAmount
        {
            get; set;
        }
        public double SGSTPercentage
        {
            get; set;
        }
        public double SGSTAmount
        {
            get; set;
        }
        public double IGSTPercentage
        {
            get; set;
        }
        public double IGSTAmount
        {
            get; set;
        }
        public int ItemId
        {
            get; set;
        }
        public string ItemName
        {
            get; set;
        }
        public double TotalAmountWithoutTax
        {
            get; set;
        }
        public int SupplierId
        {
            get; set;
        }
        public string SupplierName
        {
            get; set;
        }
        public int InvoiceReceiveId
        {
            get; set;
        }
        public string IRPaymentStatus
        {
            get; set;
        }
        public double IRRemainingAmount
        {
            get; set;
        }
        public string Remark { get; set; }
        public bool IsActive { get; set; }
        public string Perticular
        {
            get; set;
        }
        [NotMapped]
        public List<IRImages> IRImages { get; set; }
    }

    public class InvoiceReceive
    {
        [Key]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int PurchaseOrderId { get; set; }
        public int supplierId { get; set; }
        public string SupplierName { get; set; }
        public int WarehouseId { get; set; }
        public double TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreationDate { get; set; }
        public bool Deleted { get; set; }
        public double? discount1 { get; set; }
        public double? discount2 { get; set; }
        public double? discount3 { get; set; }
        public double? discount4 { get; set; }
        public double? discount5 { get; set; }
        public int? irCount { get; set; }
        public double? gstamt { get; set; }
        public double TotalTaxPercentage { get; set; }
        public string PaymentStatus { get; set; }
        public int PaymentTerms { get; set; }
        public double TotalAmountRemaining { get; set; }
        public string IR1ID { get; set; }
        public string IR2ID { get; set; }
        public string IR3ID { get; set; }
        public string IR4ID { get; set; }
        public string IR5ID { get; set; }
        [NotMapped]
        public string TempIRID { get; set; }
        public double IRAmount1WithTax { get; set; }
        public double IRAmount2WithTax { get; set; }
        public double IRAmount3WithTax { get; set; }
        public double IRAmount4WithTax { get; set; }
        public double IRAmount5WithTax { get; set; }
        public double IRAmount1WithOutTax { get; set; }
        public double IRAmount2WithOutTax { get; set; }
        public double IRAmount3WithOutTax { get; set; }
        public double IRAmount4WithOutTax { get; set; }
        public double IRAmount5WithOutTax { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        [NotMapped]
        public double? OtherAmount { get; set; }
        [NotMapped]
        public string OtherAmountRemark { get; set; }
        [NotMapped]
        public double? ExpenseAmount { get; set; }
        [NotMapped]
        public string ExpenseAmountRemark { get; set; }
        [NotMapped]
        public double? RoundofAmount { get; set; }
        [NotMapped]
        public string ExpenseAmountType { get; set; }
        [NotMapped]
        public string OtherAmountType { get; set; }
        [NotMapped]
        public string RoundoffAmountType { get; set; }

        [NotMapped]
        public int? BuyerId { get; set; }
        [NotMapped]
        public double? discount { get; set; }
        [NotMapped]
        public List<IR_Confirm> purDetails { get; set; }
        [NotMapped]
        public string IRType { get; set; }
        [NotMapped]
        public int? DueDays { get; set; }
        [NotMapped]
        public double? FreightAmount { get; set; }
    }
    //public class IR_Confirm
    //{
    //    [Key]
    //    public int IRreceiveid { get; set; }
    //    public int CompanyId { get; set; }
    //    public int PurchaseOrderDetailId { get; set; }
    //    public int PurchaseOrderId { get; set; }
    //    public int SupplierId { get; set; }
    //    public int? WarehouseId { get; set; }
    //    public string WarehouseName { get; set; }
    //    public string SupplierName { get; set; }
    //    public int ItemId { get; set; }
    //    public string PurchaseSku { get; set; }
    //    public string ItemName { get; set; }
    //    public string HSNCode { get; set; }
    //    public double PriceRecived { get; set; }
    //    public int? TotalQuantity { get; set; }
    //    public int? IRQuantity { get; set; }
    //    public string Status { get; set; }
    //    public DateTime CreationDate { get; set; }
    //    public string IR1ID { get; set; }
    //    public string IR2ID { get; set; }
    //    public string IR3ID { get; set; }
    //    public string IR4ID { get; set; }
    //    public string IR5ID { get; set; }
    //    public double? QtyRecived1 { get; set; }
    //    public double? QtyRecived2 { get; set; }
    //    public double? QtyRecived3 { get; set; }
    //    public double? QtyRecived4 { get; set; }
    //    public double? QtyRecived5 { get; set; }
    //    public double? Price1 { get; set; }
    //    public double? Price2 { get; set; }
    //    public double? Price3 { get; set; }
    //    public double? Price4 { get; set; }
    //    public double? Price5 { get; set; }
    //    public DateTime? Ir1Date { get; set; }
    //    public DateTime? Ir2Date { get; set; }
    //    public DateTime? Ir3Date { get; set; }
    //    public DateTime? Ir4Date { get; set; }
    //    public DateTime? Ir5Date { get; set; }
    //    public int? Ir1PersonId { get; set; }
    //    public int? Ir2PersonId { get; set; }
    //    public int? Ir3PersonId { get; set; }
    //    public int? Ir4PersonId { get; set; }
    //    public int? Ir5PersonId { get; set; }
    //    public string Ir1PersonName { get; set; }
    //    public string Ir2PersonName { get; set; }
    //    public string Ir3PersonName { get; set; }
    //    public string Ir4PersonName { get; set; }
    //    public string Ir5PersonName { get; set; }

    //    public string distype1 { get; set; }
    //    public string distype2 { get; set; }
    //    public string distype3 { get; set; }
    //    public string distype4 { get; set; }
    //    public string distype5 { get; set; }
    //    public double? DesP1 { get; set; }
    //    public double? DesP2 { get; set; }
    //    public double? DesP3 { get; set; }
    //    public double? DesP4 { get; set; }
    //    public double? DesP5 { get; set; }
    //    public double? DesA1 { get; set; }
    //    public double? DesA2 { get; set; }
    //    public double? DesA3 { get; set; }
    //    public double? DesA4 { get; set; }
    //    public double? DesA5 { get; set; }
    //    public double? dis1 { get; set; }
    //    public double? dis2 { get; set; }
    //    public double? dis3 { get; set; }
    //    public double? dis4 { get; set; }
    //    public double? dis5 { get; set; }
    //    public int? QtyRecived { get; set; }
    //    public double TotalTaxPercentage { get; set; }
    //    public double? CessTaxPercentage { get; set; }
    //    public double? cessamt { get; set; }
    //    public double? TtlAmt { get; set; }
    //    [NotMapped]
    //    public int? Qty { get; set; }
    //    [NotMapped]
    //    public double? Price { get; set; }
    //    [NotMapped]
    //    public double? discount { get; set; }
    //    public double? gstamt
    //    {
    //        get; set;
    //    }
    //    [NotMapped]
    //    public string IrStatus
    //    {
    //        get; set;
    //    }
    //    [NotMapped]
    //    public string IRType { get; set; }

    //    [NotMapped]
    //    public string distype { get; set; }
    //    [NotMapped]
    //    public double? DesP { get; set; }
    //    [NotMapped]
    //    public double? DesA { get; set; }
    //    [NotMapped]
    //    public double? MRP { get; set; }

    //    [StringLength(200)]
    //    public string Q1Settled { get; set; }
    //    [StringLength(200)]
    //    public string Q2Settled { get; set; }
    //    [StringLength(200)]
    //    public string Q3Settled { get; set; }
    //    [StringLength(200)]
    //    public string Q4Settled { get; set; }
    //    [StringLength(200)]
    //    public string Q5Settled { get; set; }
    //    [StringLength(200)]
    //    public string ItemNumber { get; set; }
    //}
    public class IRGR
    {
        public int PurchaseOrderId { get; set; }
        public double? IRAmount1 { get; set; }
        public double? IRAmount2 { get; set; }
        public double? IRAmount3 { get; set; }
        public double? IRAmount4 { get; set; }
        public double? IRAmount5 { get; set; }
        public double? GRAmount1 { get; set; }
        public double? GRAmount2 { get; set; }
        public double? GRAmount3 { get; set; }
        public double? GRAmount4 { get; set; }
        public double? GRAmount5 { get; set; }
        public double? IRTotal { get; set; }
        public double? GRTotal { get; set; }
    }
    public class InvIrCnf
    {
        public List<IR_Confirm> IRC { get; set; }
        public InvoiceReceive IR { get; set; }
        public List<IRMaster> IRM { get; set; }
    }
    public class dtores
    {
        public InvoiceImage IM { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class IRMasterDTO
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public string IRID { get; set; }
        public int supplierId { get; set; }
        public string SupplierName { get; set; }
        public int WarehouseId { get; set; }
        public double TotalAmount { get; set; }
        public string IRStatus { get; set; }
        public double? Gstamt { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double? Discount { get; set; }
        public double IRAmountWithTax { get; set; }
        public double IRAmountWithOutTax { get; set; }
        public double TotalAmountRemaining { get; set; }
        public string PaymentStatus { get; set; }
        public int PaymentTerms { get; set; }
        public string IRType { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public string WarehouseName { get; set; }
        public string RejectedComment { get; set; }
        public string BuyerName { get; set; }
        public double? ReamainingAmt { get; set; }
        public double? TDSAmount { get; set; }

    }
    public class IR_Detail1
    {
        public int IRreceiveid { get; set; }
        public int CompanyId { get; set; }
        public int PurchaseOrderDetailId { get; set; }
        public int PurchaseOrderId { get; set; }
        public int SupplierId { get; set; }
        public int? WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string SupplierName { get; set; }
        public int ItemId { get; set; }
        public string PurchaseSku { get; set; }
        public string ItemName { get; set; }
        public string HSNCode { get; set; }
        public double PriceRecived { get; set; }
        public int? TotalQuantity { get; set; }
        public int? IRQuantity { get; set; }
        public string Status { get; set; }
        public DateTime CreationDate { get; set; }
        public string IR1ID { get; set; }
        public double? QtyRecived1 { get; set; }
        public double? Price1 { get; set; }
        public DateTime? Ir1Date { get; set; }
        public int? Ir1PersonId { get; set; }
        public string Ir1PersonName { get; set; }
        public double? dis1 { get; set; }
        public int? QtyRecived { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double? TtlAmt { get; set; }
        public string IRType { get; set; }
        public int? Qty { get; set; }
        public double? gstamt
        {
            get; set;
        }
        public double? discountAll { get; set; }
        public double? CessTaxPercentage { get; set; }
        public string distype1 { get; set; }
        public double? DesP1 { get; set; }
        public double? DesA1 { get; set; }
        public double? OtherAmount { get; set; }
        public string OtherAmountRemark { get; set; }
        public double? ExpenseAmount { get; set; }
        public string ExpenseAmountRemark { get; set; }
        public double? RoundofAmount { get; set; }
        public string ExpenseAmountType { get; set; }
        public string OtherAmountType { get; set; }
        public string RoundoffAmountType { get; set; }
        public double MRP { get; set; }
        public string ItemNumber { get; set; }
    }
    public class IR_Detail2
    {
        public int IRreceiveid { get; set; }
        public int CompanyId { get; set; }
        public int PurchaseOrderDetailId { get; set; }
        public int PurchaseOrderId { get; set; }
        public int SupplierId { get; set; }
        public int? WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string SupplierName { get; set; }
        public int ItemId { get; set; }
        public string PurchaseSku { get; set; }
        public string ItemName { get; set; }
        public string HSNCode { get; set; }
        public double PriceRecived { get; set; }
        public int? TotalQuantity { get; set; }
        public int? IRQuantity { get; set; }
        public string Status { get; set; }
        public DateTime CreationDate { get; set; }
        public string IR2ID { get; set; }
        public double? QtyRecived2 { get; set; }
        public double? Price2 { get; set; }
        public DateTime? Ir2Date { get; set; }
        public int? Ir2PersonId { get; set; }
        public string Ir2PersonName { get; set; }
        public double? dis2 { get; set; }
        public int? QtyRecived { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double? TtlAmt { get; set; }
        public string IRType { get; set; }
        public int? Qty { get; set; }
        public double gstamt
        {
            get; set;
        }
        public double? discountAll { get; set; }
        public double? CessTaxPercentage { get; set; }
        public string distype2 { get; set; }
        public double? DesP2 { get; set; }
        public double? DesA2 { get; set; }
        public double? OtherAmount { get; set; }
        public string OtherAmountRemark { get; set; }
        public double? ExpenseAmount { get; set; }
        public string ExpenseAmountRemark { get; set; }
        public double? RoundofAmount { get; set; }
        public string ExpenseAmountType { get; set; }
        public string OtherAmountType { get; set; }
        public string RoundoffAmountType { get; set; }
        public double MRP { get; set; }
        public string ItemNumber { get; set; }
    }
    public class IR_Detail3
    {
        public int IRreceiveid { get; set; }
        public int CompanyId { get; set; }
        public int PurchaseOrderDetailId { get; set; }
        public int PurchaseOrderId { get; set; }
        public int SupplierId { get; set; }
        public int? WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string SupplierName { get; set; }
        public int ItemId { get; set; }
        public string PurchaseSku { get; set; }
        public string ItemName { get; set; }
        public string HSNCode { get; set; }
        public double PriceRecived { get; set; }
        public int? TotalQuantity { get; set; }
        public int? IRQuantity { get; set; }
        public string Status { get; set; }
        public DateTime CreationDate { get; set; }
        public string IR3ID { get; set; }
        public double? QtyRecived3 { get; set; }
        public double? Price3 { get; set; }
        public DateTime? Ir3Date { get; set; }
        public int? Ir3PersonId { get; set; }
        public string Ir3PersonName { get; set; }
        public double? dis3 { get; set; }
        public int? QtyRecived { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double? TtlAmt { get; set; }
        public string IRType { get; set; }
        public int? Qty { get; set; }
        public double? gstamt
        {
            get; set;
        }
        public double? discountAll { get; set; }
        public double? CessTaxPercentage { get; set; }

        public string distype3 { get; set; }
        public double? DesP3 { get; set; }
        public double? DesA3 { get; set; }
        public double? OtherAmount { get; set; }
        public string OtherAmountRemark { get; set; }
        public double? ExpenseAmount { get; set; }
        public string ExpenseAmountRemark { get; set; }
        public double? RoundofAmount { get; set; }
        public string ExpenseAmountType { get; set; }
        public string OtherAmountType { get; set; }
        public string RoundoffAmountType { get; set; }
        public double MRP { get; set; }
        public string ItemNumber { get; set; }
    }
    public class IR_Detail4
    {
        public int IRreceiveid { get; set; }
        public int CompanyId { get; set; }
        public int PurchaseOrderDetailId { get; set; }
        public int PurchaseOrderId { get; set; }
        public int SupplierId { get; set; }
        public int? WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string SupplierName { get; set; }
        public int ItemId { get; set; }
        public string PurchaseSku { get; set; }
        public string ItemName { get; set; }
        public string HSNCode { get; set; }
        public double PriceRecived { get; set; }
        public int? TotalQuantity { get; set; }
        public int? IRQuantity { get; set; }
        public string Status { get; set; }
        public DateTime CreationDate { get; set; }
        public string IR4ID { get; set; }
        public double? QtyRecived4 { get; set; }
        public double? Price4 { get; set; }
        public DateTime? Ir4Date { get; set; }
        public int? Ir4PersonId { get; set; }
        public string Ir4PersonName { get; set; }
        public double? dis4 { get; set; }
        public int? QtyRecived { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double? TtlAmt { get; set; }
        public string IRType { get; set; }
        public int? Qty { get; set; }
        public double? gstamt
        {
            get; set;
        }
        public double? discountAll { get; set; }
        public double? CessTaxPercentage { get; set; }
        public string distype4 { get; set; }
        public double? DesP4 { get; set; }
        public double? DesA4 { get; set; }
        public double? OtherAmount { get; set; }
        public string OtherAmountRemark { get; set; }
        public double? ExpenseAmount { get; set; }
        public string ExpenseAmountRemark { get; set; }
        public double? RoundofAmount { get; set; }
        public string ExpenseAmountType { get; set; }
        public string OtherAmountType { get; set; }
        public string RoundoffAmountType { get; set; }

        public double MRP { get; set; }
        public string ItemNumber { get; set; }
    }
    public class IR_Detail5
    {
        public int IRreceiveid { get; set; }
        public int CompanyId { get; set; }
        public int PurchaseOrderDetailId { get; set; }
        public int PurchaseOrderId { get; set; }
        public int SupplierId { get; set; }
        public int? WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string SupplierName { get; set; }
        public int ItemId { get; set; }
        public string PurchaseSku { get; set; }
        public string ItemName { get; set; }
        public string HSNCode { get; set; }
        public double PriceRecived { get; set; }
        public int? TotalQuantity { get; set; }
        public int? IRQuantity { get; set; }
        public string Status { get; set; }
        public DateTime CreationDate { get; set; }
        public string IR5ID { get; set; }
        public double? QtyRecived5 { get; set; }
        public double? Price5 { get; set; }
        public DateTime? Ir5Date { get; set; }
        public int? Ir5PersonId { get; set; }
        public string Ir5PersonName { get; set; }
        public double? dis5 { get; set; }
        public int? QtyRecived { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double? TtlAmt { get; set; }
        public string IRType { get; set; }
        public int? Qty { get; set; }
        public double? gstamt
        {
            get; set;
        }
        public double? discountAll { get; set; }
        public double? CessTaxPercentage { get; set; }
        public string distype5 { get; set; }
        public double? DesP5 { get; set; }
        public double? DesA5 { get; set; }
        public double? OtherAmount { get; set; }
        public string OtherAmountRemark { get; set; }
        public double? ExpenseAmount { get; set; }
        public string ExpenseAmountRemark { get; set; }
        public double? RoundofAmount { get; set; }
        public string ExpenseAmountType { get; set; }
        public string OtherAmountType { get; set; }
        public string RoundoffAmountType { get; set; }
        public double MRP { get; set; }
        public string ItemNumber { get; set; }
    }
    public class PutIrDTO
    {
        public List<IR_Confirm> IrItem { get; set; }
        public int Id { get; set; }
        public double? discount { get; set; }
        public string IRID { get; set; }
        public string IRType { get; set; }
        public int PurchaseOrderId { get; set; }
        public double? OtherAmount { get; set; }
        public string OtherAmountRemark { get; set; }
        public double? ExpenseAmount { get; set; }
        public string ExpenseAmountRemark { get; set; }
        public double? RoundofAmount { get; set; }
        public string ExpenseAmountType { get; set; }
        public string OtherAmountType { get; set; }
        public string RoundoffAmountType { get; set; }
        public double? CashDiscount { get; set; }
    }
    public class PaymentIrDTO
    {
        public int PurchaseOrderId { get; set; }
        public int WarehouseId { get; set; }
        public string SupplierName { get; set; }
        public string PaymentStatus { get; set; }
        public string IRStatus { get; set; }
        public string BuyerName { get; set; }
        public double TotalAmount { get; set; }

        public string IRID { get; set; }
        public string IRType { get; set; }


    }
    public class IRImages
    {
        public string IRLogoURL { get; set; }
    }

    public class Status
    {
        public string statuss { get; set; }
    }

    public class objDTIR
    {
        public int WHID { get; set; }
        public DateTime? From { get; set; }
        public DateTime? TO { get; set; }
        public List<string> status { get; set; }
        public int list { get; set; }

        public int page { get; set; }
    }
    #endregion
}