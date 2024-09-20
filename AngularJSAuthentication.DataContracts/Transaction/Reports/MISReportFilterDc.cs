using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Reports
{
    #region Open Orders
    public class RDAndCancellationFilterDc
    {
        public List<int> WarehouseList { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<string> CustomerTypeList { get; set; }
        public List<int> StoreList { get; set; }
    }
    public class PostRTDOrdersFilterDc
    {
        public List<int> WarehouseList { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<int> StoreList { get; set; }
        public List<string> InRTDForList { get; set; }
        public List<string> OrderStatusList { get; set; }
    }
    public class DeliveryBoyFilterDc
    {
        public List<int> WarehouseList { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<string> CustomerTypeList { get; set; }
        public List<int> StoreList { get; set; }
        public List<string> OrderStatusList { get; set; }
    }
    public class WarehouseReportFilterDc
    {
        public List<int> WarehouseList { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<string> CustomerTypeList { get; set; }
        public List<int> StoreList { get; set; }
        public List<string> OrderStatusList { get; set; }
    }
    public class DayWiseFilterDc
    {
        public List<int> WarehouseList { get; set; }
        public List<string> CustomerTypeList { get; set; }
        public List<int> StoreList { get; set; }
        public List<string> OrderStatusList { get; set; }
    }
    #endregion
    #region EPay / ETA
    public class EPayFilterDc
    {
        public List<int> WarehouseList { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<string> CustomerTypeList { get; set; }
        public List<string> PaymentModeList { get; set; }
    }

    public class ETAFilterDc
    {
        public List<int> WarehouseList { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<string> CustomerTypeList { get; set; }

    }
    #endregion
    #region Delivery Reports
    public class DeliveryT0T1FilterDc
    {
        public List<int> WarehouseList { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<string> CustomerTypeList { get; set; }
        public List<string> Store { get; set; }
    }
    public class DeliveryT0T4FilterDc
    {
        public List<int> WarehouseList { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<string> CustomerTypeList { get; set; }
        public List<string> Store { get; set; }
    }
    #endregion
    #region Cancellation Reports
    public class SalesVsCancellationFilterDc
    {
        public List<int> WarehouseList { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<String> CustomerTypeList { get; set; }
        public List<string> Store { get; set; }
    }
    public class Top30ExecutiveFiterDc
    {
        public List<int> WarehouseList { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<String> CustomerTypeList { get; set; }
        public List<string> Store { get; set; }
    }
    public class GetTop30DBoyFilterDc
    {
        public List<int> WarehouseList { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<string> CustomerTypeList { get; set; }
        public List<int> StoreList { get; set; }
    }
    #endregion
    #region TAT & KPI Reports
    public class TATMasterFilterDc
    {
        public List<int> WarehouseList { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<string> CustomerTypeList { get; set; }
        public List<int> StoreList { get; set; }
        public List<string> OrderValueList { get; set; }
    }
    public class DailyKPIFilterDc
    {
        public List<int> WarehouseList { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<string> CustomerTypeList { get; set; }
        public List<int> StoreList { get; set; }
    }
    public class SpillOrderFilterDc
    {
        public List<int> WarehouseList { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<string> CustomerTypeList { get; set; }
        public List<int> StoreList { get; set; }
    }
    #endregion
}
