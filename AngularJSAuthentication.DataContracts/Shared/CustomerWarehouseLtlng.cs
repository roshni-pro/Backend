using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Shared
{
    public class WalletPointsDetails
    {
        public List<LtlngDetail> CustomerLtlng { get; set; }
        public List<LtlngDetail> WarehouseLtlng { get; set; }
    }

    public class LtlngDetail
    {
        public int Id { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
    }


    public class WalletPointsSummary
    {
        public double? TotalEarnPoint { get; set; }
        public double? TotalUsedPoints { get; set; }
        public double? UpcomingPoints { get; set; }
        public DateTime? TransactionDate { get; set; }
        public double? ExpiredPoints { get; set; }
        public int? totalCount { get; set; }
        public string HowToUseWalletPoints { get; set; }

        public double point { get; set; }
        public double rupee { get; set; }
        public double? ExpiringPoints { get; set; }

        public List<GetCustomerWalletHistory> CustomerWalletHistory { get; set; }

    }

    public class GetCustomerWalletHistory
    {
        public int? OrderId { get; set; }
        public double? NewAddedWAmount { get; set; }
        public double? NewOutWAmount { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string Through { get; set; }
    }



    public class GamerWalletHistoryDc
    {
        public double? Point { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime? ExpiringDate { get; set; }
        public string Through { get; set; }
    }
    public class VANTransactionDC
    {    
        public long id { get; set; }
        public string Skcode { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public double Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public double UsedAmount { get; set; }
        public string AlertSequenceNo { get; set; }
        public string UserReferenceNumber { get; set; }
        public string WarehouseName { get; set; }
        public string ObjectType { get; set; }
        public List<VANOrderList> VANOrderLists { get; set; }
    }
    public class VANOrderList
    {
        public long id { get; set; }
        public DateTime CreatedDate { get; set; }
        public long ObjectId { get; set; }
        public double OrderAmount { get; set; }
        public double UsedAmount { get; set; }
        public long VANTransactionParentId { get; set; }
    }
    public class GetRTGSpaymentReconcilationlistDC 
    {
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string WarehouseName { get; set; }
        public string Skcode { get; set; }
        public string BenefDetails2 { get; set; }
        public string AlertSequenceNo { get; set; }
        public string UserReferenceNumber { get; set; }
        public double Amount { get; set; }
        public double ReconciledAmount { get; set; }
        public double TxnAmount { get; set; }
        public string Status { get; set; }
        public int TotalCount { get; set; }
    }
    public class VANOrderListDc
    {
        public DateTime CreatedDate { get; set; }
        public int? AssignmentId { get; set; }
        public int OrderId { get; set; }
        public double GrossAmount { get; set; }
        public double TxnAmount { get; set; }
        public string Status { get; set; }
        public string FileUploadStatus { get; set; }
        public string GatewayTransId { get; set; }
        public double refundamount { get; set; }
        public bool IsRefund { get; set; }
    }
}
