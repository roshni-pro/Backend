using System;

namespace AngularJSAuthentication.API.DataContract
{
    public class AgentSettelDc
    {
        public string AgentName { get; set; }
        public int Warehouseid { get; set; }
        public int DBoyPeopleId { get; set; }
        public int Deliveryissueid { get; set; }
        public int Agentid { get; set; }
        public decimal TotalDueAmt { get; set; }
        public DateTime? WareHouseSettleDate { get; set; }
        public bool check { get; set; }
        public string DBoyPeopleName { get; set; }
        public string WarehouseName { get; set; }


    }


    public class AgentSettlementd2dc
    {
        public int Warehouseid { get; set; }
        public int DBoyPeopleId { get; set; }
        public int Deliveryissueid { get; set; }
        public decimal TotalDueAmt { get; set; }



    }

    public class ChequePaymentDC
    {
        public int Id { get; set; }
        public int Deliveryissueid { get; set; }
        public string ChequeBankName { get; set; }
        public string DepositBankName { get; set; }
        public int Fine { get; set; }
        public int OrderId { get; set; }
        public long ChequeCollectionId { get; set; }
        public int Warehouseid { get; set; }
        public int? ChequeStatus { get; set; }
        public int? ReturnchequeId { get; set; }
        public long? CurrencyCollectionId { get; set; }
        public string ChequeNumber { get; set; }
        public decimal ChequeAmt { get; set; }
        public DateTime ChequeDate { get; set; }
        public int CreatedBy { get; set; }
        public string HandOverAgentName { get; set; }
        public int Status { get; set; }
        public DateTime? HandOverDate { get; set; }
        public string PodNo { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }

        public int BankId { get; set; }
        public string ChequeimagePath { get; set; }

    }




}







