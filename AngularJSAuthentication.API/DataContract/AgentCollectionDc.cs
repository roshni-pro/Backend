using System;

namespace AngularJSAuthentication.API.DataContract
{
    public class AgentCollectionDc
    {

        public long Id { get; set; }
        public int CurrencyDenominationId { get; set; }
        public int AgentSettelmentId { get; set; }
        public int CurrencyHubstockId { get; set; }
        public int CurrencyCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public decimal TotalAmount { get; set; }
        public long CurrencycollectionId { get; set; }

    }

    public class CashBalanceCollectionDc
    {
        public long Id { get; set; }
        public int? CurrencyDenominationId { get; set; }
        public int? CurrencyCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string Reason { get; set; }

    }




    public class CashBalanceVerifiedDc
    {
        public long Id { get; set; }
        public int WarehouseId { get; set; }
        public bool IsVerify { get; set; }
        public int VerifyBy { get; set; }
        public string SubmittedRole { get; set; }
        public DateTime VerifyDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string VerifyName{ get; set; }

    }

}