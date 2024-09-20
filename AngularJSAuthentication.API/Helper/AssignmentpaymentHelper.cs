using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AngularJSAuthentication.API.Helper
{
    public class AssignmentpaymentHelper
    {
        public OldAssignmentPayment AssignmentPaymentupdate(OldAssignmentPayDC payemntdetails, AuthContext context, int? userid = 0)
        {
            OldAssignmentPayment payementdetails = null;
            if (payemntdetails != null)
            {
                // add payment details in old assignment apyment  and old assignment payment details
                payementdetails = addpaymentdatails(payemntdetails, context, userid);
                //end

                //get Order ids assignment based  
                var assignmentdata = context.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == payemntdetails.DeliveryIssuanceId).Select(x => new { x.OrderIds, x.DisplayName }).FirstOrDefault();
                //end

                //add entry in ledger table 
                AgentLedgerHelper agenthelper = new AgentLedgerHelper();
                bool isledgerentry = agenthelper.OnPaymentAccepted(payemntdetails.AgentId, payemntdetails.TotalAssignmentDeliverdAmount ?? 0, assignmentdata.DisplayName, userid ?? 0, payemntdetails.DeliveryIssuanceId, assignmentdata.OrderIds, payemntdetails.PaymentDate);
                //end
                //add entry agent TDS
                bool isledgercommsions = agenthelper.OnGetCommision(payementdetails.DeliveryIssuanceId, userid ?? 0, payementdetails.AgentId, assignmentdata.DisplayName, payemntdetails.PaymentDate);
                //end
            }

            return payementdetails;
        }

        public OldAssignmentPayment addpaymentdatails(OldAssignmentPayDC payemntdetails, AuthContext context, int? userid = 0)
        {

            //add old assignment payment  
            OldAssignmentPayment asspaymentdetails = new OldAssignmentPayment();
            asspaymentdetails.DeliveryIssuanceId = payemntdetails.DeliveryIssuanceId;
            asspaymentdetails.TotalAssignmentAmount = payemntdetails.TotalAssignmentAmount;
            asspaymentdetails.RefNo = payemntdetails.RefNo;
            asspaymentdetails.PaymentDate = payemntdetails.PaymentDate;
            asspaymentdetails.Remark = payemntdetails.Remark;
            asspaymentdetails.Createdby = userid;
            asspaymentdetails.Updatedby = userid;
            asspaymentdetails.CreatedDate = DateTime.Now;
            asspaymentdetails.UpdateDate = DateTime.Now;
            asspaymentdetails.Deleted = false;
            asspaymentdetails.IsActive = true;
            asspaymentdetails.Status = payemntdetails.Status;
            asspaymentdetails.AgentId = payemntdetails.AgentId;
            asspaymentdetails.TotalAssignmentDeliverdAmount = payemntdetails.TotalAssignmentDeliverdAmount;
            asspaymentdetails.CashAmount = payemntdetails.CashAmount;
            asspaymentdetails.ChequeAmount = payemntdetails.ChequeAmount;
            asspaymentdetails.OnlineAmount = payemntdetails.OnlineAmount;
            context.OldAssignmentPaymentDB.Add(asspaymentdetails);
            context.Commit();
            //end old assignment payment

            //convert  json to list 

            if (payemntdetails.AssignmentorderDetails != null)
            {
                addorderinfo(payemntdetails.AssignmentorderDetails, asspaymentdetails.Id, context);
            }
            //end  
            return asspaymentdetails;
        }

        private void addorderinfo(List<AssignmentOrderListPayDC> orderdetails, int OldAssignmentPaymentId, AuthContext context)
        {

            //add order details 
            foreach (var details in orderdetails)
            {
                OldAssignmentPaymentDetails orderinfo = new OldAssignmentPaymentDetails();
                orderinfo.OldAssignmentPaymentId = OldAssignmentPaymentId;
                orderinfo.OrderId = details.OrderId;
                orderinfo.OrderAmount = details.GrossAmount;
                orderinfo.status = details.Status;
                context.OldAssignmentPaymentDetailsDB.Add(orderinfo);
                context.Commit();
            }
            //end
        }
    }
}