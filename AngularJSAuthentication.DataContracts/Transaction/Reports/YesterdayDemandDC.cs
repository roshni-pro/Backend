using System;

namespace AngularJSAuthentication.DataContracts.Transaction.Reports
{
    public class YesterdayDemandDC
    {
        public int itemmultimrpid { get; set; }
        public int warehouseid { get; set; }
        public string warehousename { get; set; }
        public int cityid { get; set; }
        public string itemnumber { get; set; }
        public string ItemName { get; set; }
        public int YesterdayDemand { get; set; }
        public int OldDemand { get; set; }
        public int Demand { get; set; }

        public int CurrentStock { get; set; }
        public int NetDemand { get; set; }
        public int? BuyerId { get; set; }
        public string BuyerName { get; set; }
        public string BuyerEmail { get; set; }
        public int openpoqty { get; set; }
        public int DelCancel { get; set; }

    }

    public class DFRPercentage
    {
        public int itemmultimrpid { get; set; }
        public string warehousename { get; set; }
        public string itemnumber { get; set; }
        public string ItemName { get; set; }
        public int YesterdayDemand { get; set; }
        public int OldDemand { get; set; }
        public int Demand { get; set; }
        public int CurrentStock { get; set; }
        public int NetDemand { get; set; }
        public string BuyerName { get; set; }
        public string BuyerEmail { get; set; }
        public int openpoqty { get; set; }
        public int DelCancel { get; set; }
        public int OtherHubsStock { get; set; }
        public int OtherHubsDemand { get; set; }
        public int NewDemand { get; set; }
        public DateTime DemandDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalGrQty { get; set; }
        public int TotalInternalTransfer { get; set; }
        public double DFRPercent { get; set; }
        public int ClosingStock { get; set; }
        public double DfrOnInventory { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int RTPCount { get; set; }

    }

}
