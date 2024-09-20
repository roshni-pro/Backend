using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.API.DataContract
{
    public class WarehousecurrencycollectionDc
    {
        public long Id { get; set; }
        public int Warehouseid { get; set; }
        public int DBoyPeopleId { get; set; }
        public int Deliveryissueid { get; set; }
        public decimal TotalCashAmt { get; set; }
        public decimal TotalOnlineAmt { get; set; }
        public decimal TotalCheckAmt { get; set; }
        public decimal TotalDeliveryissueAmt { get; set; }
        public decimal TotalCollectionAmt
        {
            get
            {
                return TotalCashAmt + TotalOnlineAmt + TotalCheckAmt;
            }
        }
        public DateTime CreatedDate { get; set; }
        public decimal TotalDueAmt { get; set; }
        public string DBoyPeopleName { get; set; }
        public string Status { get; set; }
        public bool IsCashVerify { get; set; }
        public bool IsChequeVerify { get; set; }
        public bool IsOnlinePaymentVerify { get; set; }
        public bool Ischecked { get; set; }
        public string DeclineNote { get; set; }
        public string AgentName { get; set; }
        public string Comment { get; set; }
        public DateTime? WareHouseSettleDate { get; set; }
    }


    public class WcurrencycollectionPaggingData
    {
        public int total_count { get; set; }
        public List<WarehousecurrencycollectionDc> WarehousecurrencycollectionDcs { get; set; }
    }

    public class OnlineCollectionDcPaggingData
    {
        public int total_count { get; set; }
        public List<OnlinePaymentDc> onlinePaymentDcs { get; set; }
    }

    public class CurrencyCollectionUpdateDc
    {
        public long currencyCollectionId { get; set; }
        public long warehouseid { get; set; }
        public string status { get; set; }
        public string DeclineNote { get; set; }

    }
}