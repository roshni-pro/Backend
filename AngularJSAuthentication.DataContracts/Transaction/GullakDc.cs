using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Transaction
{
    public class GetGullak
    {
        public int Skip { get; set; }
        public int Take { get; set; }
       // public int? warehouseid { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string SKcode { get; set; }
    }

    public class GetGullakTransaction
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public int GullakId { get; set; }
        public int CustomerId { get; set; }

    }
    public class GullakPageData
    {
        public int total_count { get; set; }
        public List<GullakDc> GullakDTO { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class GullakTransactionPageData
    {
        public int total_count { get; set; }
        public List<GullakTransactionDc> GullakTransactionDc { get; set; }
    }
    public class GullakDc
    {
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int GullakCreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public int CustomerId { get; set; }
        public double TotalAmount { get; set; }
        public string SKcode { get; set; }
        public string Mobile { get; set; }
        public string ShopName { get; set; }
        public string Warehouse { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public List<GullakInPaymentDc> GullakInPayments { get; set; }
        public List<GullakTransactionDc> GullakTransactions { get; set; }
    }
    public class GullakPendingDc
    {
        public long id { get; set; }
        public int CustomerId { get; set; }
        public long GullakId { get; set; }
        public string GatewayTransId { get; set; }
        public string status { get; set; }
        public double Amount { get; set; }
        public string GatewayRequest { get; set; } // bank name // online payment Details        
        public string PaymentFrom { get; set; } // online // cash //cheque
        public string comment { get; set; }  // comment 
        public string GullakImage { get; set; }
    }

    public class GullakInPaymentDc
    {
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public int CustomerId { get; set; }
        public long GullakId { get; set; }
        public string GatewayTransId { get; set; }
        public double amount { get; set; }
        public string status { get; set; }
        public string PaymentFrom { get; set; }
        public string GatewayRequest { get; set; }
        public string GatewayResponse { get; set; }
        public string Comment { get; set; }
        public string SKcode { get; set; }
        public string Mobile { get; set; }
        public string ShopName { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string GullakImage { get; set; }
    }
    public class GullakTransactionDc
    {
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int GullakCreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public int CustomerId { get; set; }
        public long GullakId { get; set; }
        public double Amount { get; set; }
        public string ObjectType { get; set; }
        public string ObjectId { get; set; }
        public string Comment { get; set; }
        public string SKcode { get; set; }
        public string Mobile { get; set; }
        public string ShopName { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        //public DateTime? TransactionsDate { get; set; }
        public string RefrenceNo { get; set; }
    }

    public class AddGullakPayment
    {      
        public long id { get; set; }
        public int CustomerId { get; set; }
        public double Amount { get; set; }
        public string GatewayRequest { get; set; } // bank name // online payment Details 
        public string GatewayTransId { get; set; } // cheque number // online trX Number 
        public string PaymentFrom { get; set; } // online // cash //cheque
        public string comment { get; set; }  // comment 
        public string GullakImage { get; set; }
    }

}
