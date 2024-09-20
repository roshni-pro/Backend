using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.API.DataContract
{
    public class LiveWarehouseCashDc
    {
        public long CurrencyHubStockId { get; set; }
        public DateTime BOD { get; set; }
        public bool IsBOD { get; set; }
        public bool IsEOD { get; set; }
        public DateTime? EOD { get; set; }
        public DateTime? previousDate { get; set; }
        public string comment { get; set; }
        public string Deliveryissueids { get; set; }
        public decimal TotalOpeingamount { get;set;}
        public List<hubCashCollectionDc> WarehouseOpeningCash { get; set; }
        public List<hubCashCollectionDc> WarehouseTodayCash { get; set; }
        public List<hubCashCollectionDc> WarehouseClosingCash { get; set; }

    }

    public class LiveCasherWarehouseCashDc
    {
        public long CurrencyHubStockId { get; set; }
        public DateTime BOD { get; set; }
        public bool IsBOD { get; set; }
        public bool IsEOD { get; set; }
        public DateTime? EOD { get; set; }
        public DateTime? previousDate { get; set; }
        public string comment { get; set; }
        public string Deliveryissueids { get; set; }
        public decimal TotalOpeingamount { get; set; }       
        public List<hubCashCollectionDc> WarehouseTodayCash { get; set; }
        public int WarehouseTotalTodayChequeCount { get; set; }
        public decimal WarehouseTotalChequeAmount { get; set; }
        public string CasherPeopleName { get; set; }
        public decimal TotalSubmittedChequeamount { get; set; }
        public List<OtherCasherDataDc> OtherCasherDataDcs { get; set; }


    }

    public class OtherCasherDataDc
    {
        public int? OtherPeopleId { get; set; }
        public string OtheCasherPeopleName { get; set; }
        public int OtherCasherTotalTodayChequeCount { get; set; }
        public decimal OtherCasherTotalChequeAmount { get; set; }
        public List<hubCashCollectionDc> WarehouseTodayCash { get; set; }
    }

    public class CashExchangeDetailDc
    {
        public List<hubCashCollectionDc> hubCashCollectionDc { get; set; }
        public string Comment { get; set; }

    }

    public class ExchangeCommentDc
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public DateTime ExchangeDate { get; set; }

        public double TotalInAmount { get; set; }
        public double TotalOutAmount { get; set; }
        public string comment { get; set; }
    }

    public class ExchangeCashDc
    {
        public List<hubCashCollectionDc> hubCashCollectionDcs { get; set; }
        public List<ExchangeCommentDc> ExchangeCommentDcs { get; set; }

    }
}