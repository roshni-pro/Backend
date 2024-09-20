using AngularJSAuthentication.DataContracts.Transaction.PurchaseOrder;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
using AngularJSAuthentication.Model.PurchaseOrder;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class PurchaseOrderReturnHelper
    {

        public string ApproveCancelRequest(long poReturnRequestId, int userid)
        {
            string message = "";
            using (var context = new AuthContext())
            {
                int? approverId = Approvedby(userid, context);

                POReturnRequest poReturn = context.POReturnRequestDb.FirstOrDefault(x => x.Id == poReturnRequestId);
                if (poReturn != null && approverId.Value > 0)
                {

                    if (poReturn.CancelType == "PO")
                    {
                        CancelPurchaseOrder((int)poReturn.ItemId);
                        message = "Purchase Order Cancelled";
                    }
                    else if (poReturn.CancelType == "GR")
                    {
                        CancelGR((int)poReturn.ItemId, poReturn.PurchaseOrderId);
                        message = "GR Cancelled";
                    }
                    else if (poReturn.CancelType == "IR")
                    {
                        message = CancelIR((int)poReturn.ItemId);
                    }

                    poReturn.Status = "Approved";
                    poReturn.ModifiedDate = DateTime.Now;
                    poReturn.ModifiedBy = userid;
                    poReturn.ApprovedBy = userid;
                    poReturn.ApprovedDate = DateTime.Now;
                    context.Commit();
                }
                else
                {
                    message = "Access Denied";
                }

            }
            return message;
        }


        public string RejectCancelRequest(long poReturnRequestId, int userid)
        {
            string message = "";
            using (var context = new AuthContext())
            {
                int? approverId = Approvedby(userid, context);
                POReturnRequest poReturn = context.POReturnRequestDb.FirstOrDefault(x => x.Id == poReturnRequestId);
                if (poReturn != null && approverId.Value > 0)
                {
                    poReturn.Status = "Rejected";
                    poReturn.ModifiedDate = DateTime.Now;
                    poReturn.ModifiedBy = userid;
                    poReturn.ApprovedBy = userid;
                    poReturn.ApprovedDate = DateTime.Now;
                    context.Commit();
                    message = "Request Rejected";
                }
                else
                {
                    message = "Access Denied";
                }

            }
            return message;
        }


        public POReturnRequestDc MakeCancelRequest(POReturnRequestDc returnRequest, int userid)
        {
            if (returnRequest != null)
            {
                using (var context = new AuthContext())
                {
                    //string query = @"select p.PeopleID from People P
                    //                    inner join WarehousePermissions wp on p.PeopleID = wp.PeopleID and wp.IsActive = 1 
                    //                    --inner join Warehouses W on P.WarehouseId = W.WarehouseId
                    //                    inner Join AspNetUsers U on P.Email = U.Email
                    //                    inner join AspNetUserRoles UR on U.Id = UR.UserId 
                    //                    inner join AspNetRoles R on UR.RoleId = R.Id
                    //                    WHERE R.Name = 'IC department Lead' AND wp.WarehouseId = " + returnRequest.WarehouseId.ToString();

                    //int? approverId = context.Database.SqlQuery<int>(query).FirstOrDefault();

                    POReturnRequest poReturnRequest = new POReturnRequest();
                    poReturnRequest.ApprovedBy = 0;
                    poReturnRequest.Guid = Guid.NewGuid().ToString();
                    poReturnRequest.RequestDate = DateTime.Now;
                    poReturnRequest.ItemId = returnRequest.ItemId;
                    poReturnRequest.CancelType = returnRequest.CancelType;
                    poReturnRequest.PurchaseOrderId = returnRequest.PurchaseOrderId;
                    poReturnRequest.Status = "Pending";
                    poReturnRequest.IsActive = true;
                    poReturnRequest.IsDeleted = false;
                    poReturnRequest.CreatedBy = userid;
                    poReturnRequest.CreatedDate = DateTime.Now;
                    context.POReturnRequestDb.Add(poReturnRequest);
                    context.Commit();
                    returnRequest.Guid = poReturnRequest.Guid;
                }

            }
            return returnRequest;
        }

        private bool CancelPurchaseOrder(int purchaseOrderId)
        {
            using (var context = new AuthContext())
            {
                var purchaseOrderMaster = context.DPurchaseOrderMaster.FirstOrDefault(x => x.PurchaseOrderId == purchaseOrderId);

                var purchaseOrderDetailList = context.DPurchaseOrderDeatil.Where(x => x.PurchaseOrderId == purchaseOrderId).ToList();

                purchaseOrderMaster.Acitve = false;
                purchaseOrderMaster.Deleted = true;
                if (purchaseOrderDetailList != null && purchaseOrderDetailList.Count > 0)
                {
                    foreach (var item in purchaseOrderDetailList)
                    {
                        item.IsDeleted = true;

                    }
                }

                context.Commit();
            }

            return true;
        }

        private bool CancelGR(int serialNumber, int purchaseOrderId)
        {
            using (var context = new AuthContext())
            {
                var grDetailQuery = from gr in context.GoodsReceivedDetail
                                    join pod in context.DPurchaseOrderDeatil
                                    on gr.PurchaseOrderDetailId equals pod.PurchaseOrderDetailId
                                    join pom in context.DPurchaseOrderMaster
                                    on pod.PurchaseOrderId equals pom.PurchaseOrderId
                                    where pom.PurchaseOrderId == purchaseOrderId
                                    && gr.GrSerialNumber == serialNumber
                                    select gr;


                var grDetailList = context.GoodsReceivedDetail.ToList();
                if (grDetailList != null && grDetailList.Count > 0)
                {
                    foreach (var grDetail in grDetailList)
                    {
                        grDetail.IsDeleted = true;
                        grDetail.IsActive = false;
                    }
                    context.Commit();
                }
            }

            return true;
        }

        private string CancelIR(int irMasterId)
        {
            using (var context = new AuthContext())
            {
                var irMaster = context.IRMasterDB.FirstOrDefault(x => x.Id == irMasterId);
                string cancelledMessage = "";
                if (irMaster.IRStatus == "IR Posted" || irMaster.IRStatus == "Pending from Buyer side" || irMaster.IRStatus == "IR Uploaded" || irMaster.IRStatus == "Approved from Buyer side" || irMaster.IRStatus == "Paid")
                {

                    var irDetailList = context.InvoiceReceiptDetail.Where(x => x.IRMasterId == irMasterId).ToList();
                    if (irDetailList != null && irDetailList.Count > 0)
                    {
                        foreach (var irDetail in irDetailList)
                        {
                            irDetail.IsDeleted = true;
                            irDetail.IsActive = false;
                        }
                    }
                    var ircreditnotelist = context.IRCreditNoteMaster.Where(x=>x.IRMasterId == irMasterId).Include(x=>x.IRCreditNoteDetails).ToList();
                    if(ircreditnotelist != null && ircreditnotelist.Count > 0)
                    {
                        foreach (var ircreditDetail in ircreditnotelist)
                        {
                            ircreditDetail.IsDeleted = true;
                            ircreditDetail.IsActive = false;
                            if(ircreditDetail.IRCreditNoteDetails.Any())
                            {
                                foreach(var ircreditnoteDetail in ircreditDetail.IRCreditNoteDetails)
                                {
                                    ircreditnoteDetail.IsDeleted = true;
                                    ircreditnoteDetail.IsActive = false;
                                }
                            }
                        }
                    }
                    if (irMaster != null)
                    {
                        ReturnIR(irMaster.Id);
                        irMaster.Deleted = true;
                    }
                     if (irMaster.PaymentStatus == "paid" || irMaster.PaymentStatus == "Paid" || irMaster.PaymentStatus == "partial paid") {
                        ReturnLedgerPayment(irMaster.Id);
                      }
                    context.Commit();
                    cancelledMessage = "IR Cancelled";
                }
                else
                {
                    cancelledMessage = "IR Already Cancelled";
                }
                return cancelledMessage;
            }
        }

        private bool ReturnIR(int irid)
        {
            using (var context = new AuthContext())
            {
                LadgerHelper helper = new LadgerHelper();
                return helper.MakeReverseEntries("IRCancel", "IR", irid);
            }
        }
        private bool ReturnLedgerPayment(int irid)
        {
            LadgerHelper helper = new LadgerHelper();
            return helper.ReturnPayment("IR", irid);

        }

        private int? Approvedby(int userid,AuthContext context) {

            string query = @"select p.PeopleID from People P
                                        inner join WarehousePermissions wp on p.PeopleID = wp.PeopleID and wp.IsActive = 1 
                                        --inner join Warehouses W on P.WarehouseId = W.WarehouseId
                                        inner Join AspNetUsers U on P.Email = U.Email
                                        inner join AspNetUserRoles UR on U.Id = UR.UserId 
                                        inner join AspNetRoles R on UR.RoleId = R.Id
                                        WHERE R.Name = 'IC department Lead' AND P.PeopleID = " + userid;

             int? approverId = context.Database.SqlQuery<int>(query).FirstOrDefault();
            return approverId;

        }
    }
}