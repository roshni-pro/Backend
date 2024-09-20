using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction
{
    public class OnlineTxnDC 
    {
            public int OrderId
            {
                get; set;
            }
            public string Skcode
            {
                get; set;
            }
            public DateTime Date
            {
                get; set;

            }
            public string Warehouse
            {
                get; set;
            }
            public double OrderAmount
            {
                get; set;
            }
            public double TxnAomunt
            {
                get; set;
            }
            public string TxnStatus
            {
                get; set;
            }
            public string TxnID
            {
                get; set;
            }
            public DateTime? SettleDate
            {
                get; set;
            }
            public string MOP
            {
                get; set;
            }
            public bool IsSettled
            {
                get; set;
            }
            public string SettlComments
            {
                get; set;
            }
            public int userid
            {
                get; set;
            }
            public int UploadId
            {
                get; set;
            }
            public string Orderstatus
            {
                get; set;
            }
            public bool IsRefund
            {
                get; set;
            }
            public int? DeliveryIssuanceId
            {
                get; set;
            }
            public double refundamount
            {
                    get; set;
            }
        public double DispatchedAmount
        {
            get; set;
        }
        public string TxnNo { get; set; }


    }

    public class onlineTxnPaggingDTO
    {
        public List<OnlineTxnDC> OnlineTxns { get; set; }
        public int Total_Count { get; set; }
    }

    public class onlineUpiTransactionDTO
    {
        public string OrderId { get; set; }
        public string Status { get; set; }
        public bool IsSuccess { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UPITxnID { get; set; }
        public string TxnNo { get; set; }
        public float Amount { get; set; }
        public string ResponseMsg { get; set; }
        public string WarehouseName { get; set; }
        public string Skcode { get; set; }
    }

    public class TotalTransactionsDTO
    {
        public List<onlineUpiTransactionDTO> UPItransactions { get; set; }
        public int totalRecords { get; set; }
    }
}
