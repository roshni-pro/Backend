using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Reports
{
    #region Open Orders
    public class RedispatchAndCancellationReportDc
    {
        public string Warehouse { get; set; }
        public long TotalOrderCount { get; set; }
        public double TotalOrderValue { get; set; }
        public long RedispatchedOrderCount { get; set; }
        public double RedispatchedOrderPercent { get; set; }
        public double RedispatchedOrderValue { get; set; }
        public long DeliveryCancelledOrderCount { get; set; }
        public double DeliveryCancelledOrderPercent { get; set; }
        public double DeliveryCancelledOrderValue { get; set; }
        public long DeliveredOrderCount { get; set; }
        public double DeliveredOrderPercent { get; set; }
        public double DeliveredOrderValue { get; set; }
    }
    public class PostRTDOrdersReportDc
    {
        public string Warehouse { get; set; }
        public long TotalOrderCount { get; set; }
        public long TotalOrderValue { get; set; }
        public long RetailOrderCount { get; set; }
        public long RetailOrderValue { get; set; }
        public long KPPOrderCount { get; set; }
        public long KPPOrderValue { get; set; }
        public long SKPOrderCount { get; set; }
        public long SKPOrderValue { get; set; }
    }
    public class DeliveryBoyReportDc
    {
        public string Warehouse { get; set; }
        public string DeliveryBoy { get; set; }
        public long TotalOrderCount { get; set; }
        public long TotalOrderValue { get; set; }
        public long CancelledOrderCount { get; set; }
        public long CancelledOrderValue { get; set; }
        public long RedispatchedOrderCount { get; set; }
        public long RedispatchedOrderValue { get; set; }
        public long IssuedOrderCount { get; set; }
        public long IssuedOrderValue { get; set; }
        public long RTDOrderCount { get; set; }
        public long RTDOrderValue { get; set; }
        public long ShippedOrderCount { get; set; }
        public long ShippedOrderValue { get; set; }
        public long ReadyToPickOrderCount { get; set; }
        public long ReadyToPickOrderValue { get; set; }
    }
    public class WarehouseReportDc
    {
        public string Warehouse { get; set; }
        public long CancelledOrderCount { get; set; }
        public long CancelledOrderValue { get; set; }
        public long RedispatchedOrderCount { get; set; }
        public long RedispatchedOrderValue { get; set; }
        public long IssuedOrderCount { get; set; }
        public long IssuedOrderValue { get; set; }
        public long RTDOrderCount { get; set; }
        public long RTDOrderValue { get; set; }
        public long ShippedOrderCount { get; set; }
        public long ShippedOrderValue { get; set; }
        public long ReadyToPickOrderCount { get; set; }
        public long ReadyToPickOrderValue { get; set; }
        public long TotalOrderCount { get; set; }
        public long TotalOrderValue { get; set; }
    }
    public class DayWiseReportDc
    {
        public string Warehouse { get; set; }
        public long TotalOrderCount { get; set; }
        public long TotalOrderValue { get; set; }
        public long Day29OrderCount { get; set; }
        public long Day29OrderValue { get; set; }
        public long Day27OrderCount { get; set; }
        public long Day27OrderValue { get; set; }
        public long Day19OrderCount { get; set; }
        public long Day19OrderValue { get; set; }
        public long Day3OrderCount { get; set; }
        public long Day3OrderValue { get; set; }
        public long Day1OrderCount { get; set; }
        public long Day1OrderValue { get; set; }
    }
    #endregion
    #region EPay / ETA
    public class EPayReportDc
    {
        public string Warehouse { get; set; }
        public long TotalOrderCount { get; set; }
        public long TotalOrderValue { get; set; }
        public long Store1OrderCount { get; set; }
        public long Store1OrderValue { get; set; }
        public long Store2OrderCount { get; set; }
        public long Store2OrderValue { get; set; }
        public long Store3OrderCount { get; set; }
        public long Store3OrderValue { get; set; }
        public long KisanKiranaOrderCount { get; set; }
        public long KisanKiranaOrderValue { get; set; }
        public long SafoyaOrderCount { get; set; }
        public long SafoyaOrderValue { get; set; }

    }
    public class ETAReportDc
    {
        public string Warehouse { get; set; }
        public long TotalStoreCount { get; set; }
        public long BlueStore1Count { get; set; }
        public long BlueStore2Count { get; set; }
        public long BlueStore3Count { get; set; }
        public long RedStore1Count { get; set; }
        public long RedStore2Count { get; set; }
        public long RedStore3Count { get; set; }
        public long WhiteStore1Count { get; set; }
        public long WhiteStore2Count { get; set; }
        public long WhiteStore3Count { get; set; }
        public long YellowStore1Count { get; set; }
        public long YellowStore2Count { get; set; }
        public long YellowStore3Count { get; set; }
    }
    #endregion
    #region Delivery Reports
    public class DeliveryT0T1ReportDc
    {
        public string Warehouse { get; set; }
        public long DeliveredCount { get; set; }
        public long DeliveredValue { get; set; }
        public long T0OrderCount { get; set; }
        public long T0OrderPercent { get; set; }
        public long T0CustomerCount { get; set; }
        public long T0CustomerPercent { get; set; }
        public long T1OrderCount { get; set; }
        public long T1OrderPercent { get; set; }
        public long T1CustomerCount { get; set; }
        public long T1CustomerPercent { get; set; }
    }
    public class DeliveryT0T4ReportDc
    {
        public string Warehouse { get; set; }
        public long TotalOrderCount { get; set; }
        public long T0OrderCount { get; set; }
        public long T0OrderPercent { get; set; }
        public long T1OrderCount { get; set; }
        public long T1OrderPercent { get; set; }
        public long T2OrderCount { get; set; }
        public long T2OrderPercent { get; set; }
        public long T3OrderCount { get; set; }
        public long T3OrderPercent { get; set; }
        public long T4OrderCount { get; set; }
        public long T4OrderPercent { get; set; }
        public long T4plusOrderCount { get; set; }
        public long T4PlusOrderPercent { get; set; }

    }
    #endregion
    #region Cancellation Reports
    public class SalesVsCancellationReportDc
    {
        public string Warehouse { get; set; }
        public long DateCancellationOrderCount { get; set; }
        public double DateCancellationPercent { get; set; }
        public double DateCancellationValue { get; set; }
        public double DateCancellationValuePercent { get; set; }
        public long MTDTotalSaleOrderCount { get; set; }
        public double MTDTotalSaleValue { get; set; }
        public long MTDCancellationOrderCount { get; set; }
        public double MTDCancellationPercent { get; set; }
        public double MTDCancellationSaleValue { get; set; }
        public double MTDCancellationSaleValuePercent { get; set; }
        public long MTDCNetDispatchOrderCount { get; set; }
        public double MTDCNetDispatchValue { get; set; }
        public double MTDCNetDispatchValuePercent { get; set; }
    }

    public class Top30ExecutiveReportDc
    {
        public string Warehouse { get; set; }
        public string Executive { get; set; }
        public long TotalOrderCount { get; set; }
        public long TotalSaleValue { get; set; }
        public long CancellationOrderCount { get; set; }
        public double CancellationOrderPercent { get; set; }
        public long CancellationValue { get; set; }
        public double CancellationValuePercent { get; set; }
    }
    public class GetTop30DBoyReportDc
    {
        public string Warehouse { get; set; }
        public string DeliveryBoy { get; set; }
        public long TotalOrderCount { get; set; }
        public long TotalSaleValue { get; set; }
        public long CancellationOrderCount { get; set; }
        public double CancellationOrderPercent { get; set; }
        public long CancellationValue { get; set; }
        public double CancellationValuePercent { get; set; }
    }
    #endregion
    #region TAT & KPI Reports
    public class TATMasterReportDc
    {
        public string Warehouse { get; set; }
        public long TotalOrdersDelivered { get; set; }
        public double AverageDeliveryHours { get; set; }
        public long HitOrderCount { get; set; }
        public long HitOrderPercent { get; set; }
        public long MissOrderCount { get; set; }
        public long MissOrderPercent { get; set; }
        public long FailOrderCount { get; set; }
        public long FailOrderPercent { get; set; }
        public long BowledOrderCount { get; set; }
        public long BowledOrderPercent { get; set; }
    }
    public class DailyKPIReportDc
    {
        public double LIFRPercent { get; set; }
        public double VFRPercent { get; set; }
        public double CFRPercent { get; set; }
        public long SpillOrdersCount { get; set; }
    }
    public class SpillOrderReportDc
    {
        public string Warehouse { get; set; }
        public long TotalOrderCount { get; set; }
        public long TotalOrderValue { get; set; }
        public long BlueOrderCount { get; set; }
        public long BlueOrderValue { get; set; }
        public long RedOrderCount { get; set; }
        public long RedOrderValue { get; set; }
        public long WhiteOrderCount { get; set; }
        public long WhiteOrderValue { get; set; }
        public long YellowOrderCount { get; set; }
        public long YellowOrderValue { get; set; }
    }
    #endregion
}
