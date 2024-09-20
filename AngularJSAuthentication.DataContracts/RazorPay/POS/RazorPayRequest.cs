using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.RazorPay.POS
{
    public class RazorPayPosNotificationRequest 
    {
        public string appKey { get; set; }
        public string username { get; set; }
        public decimal amount { get; set; }
        public string customerMobileNumber { get; set; }
        public string externalRefNumber { get; set; }
        public string externalRefNumber2 { get; set; }
        public string externalRefNumber3 { get; set; }
        public string accountLabel { get; set; }
        public string customerEmail { get; set; }
        public Pushto pushTo { get; set; }
        public string mode { get; set; }
    }

    public class Pushto
    {
        public string deviceId { get; set; }
    }
    public class RazorPayPosRequest
    {
        public int OrderId { get; set; }
        public int WarehouseId { get; set; }
        public double Amount { get; set; }
        public long? WarehousePosMachineId { get; set; }
    }
    public class RazorPayPosResponse
    {
        public string Message { get; set; }
        public bool Status { get; set; }
    }
    public class RazorPayPosStatusResponse
    {
        public string TransactionId { get; set; }
        public string PaymentMode { get; set; }
        public bool Status { get; set; }
        public string Message{ get; set; }
    }
    public class CheckPosRequest
    {
        public string username { get; set; }
        public string appKey { get; set; }
        public string origP2pRequestId { get; set; }
    }
    public class CancelNoticationRequest
    {
        public string username { get; set; }
        public string appKey { get; set; }
        public string origP2pRequestId { get; set; }
        public Pushto pushTo { get; set; }
    }
}
