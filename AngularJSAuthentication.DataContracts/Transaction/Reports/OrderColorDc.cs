using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Reports
{
    public class OrderColorDc
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int orderid { get; set; }
        public int OrderDetailsId { get; set; }
        public string ItemName { get; set; }
        public string Number { get; set; }
        public DateTime CreatedDate { get; set; }
        public int itemmultimrpid { get; set; }
        public int Qty { get; set; }
        public bool IsFreeItem { get; set; }
        public int CurrentInventory { get; set; }
        public int totalQty { get; set; }
        public int QtyAvailableStatus { get; set; }
        public double TotalAmount { get; set; }
        public double detailTotalAmount { get; set; }

    }

    public class OrderColorRequest
    {
        public List<int> WarehouseIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ReportType { get; set; }
    }

    public class DisplayOrderColorCount
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int YellowOrderCount { get; set; }
        public int RedOrderCount { get; set; }
        public int BlueOrderCount { get; set; }
        public int WhiteOrderCount { get; set; }

        public int GrandTotal
        {
            get
            {
                return RedOrderCount + BlueOrderCount + WhiteOrderCount + YellowOrderCount;
            }
        }

        public int RedOrderPercent
        {
            get
            {
                return (RedOrderCount * 100) / (RedOrderCount + BlueOrderCount + WhiteOrderCount + YellowOrderCount);
            }
        }

        public int YellowOrderPercent
        {
            get
            {
                return (YellowOrderCount * 100) / (RedOrderCount + BlueOrderCount + WhiteOrderCount + YellowOrderCount);
            }
        }

        public int BlueOrderPercent
        {
            get
            {
                return (BlueOrderCount * 100) / (RedOrderCount + BlueOrderCount + WhiteOrderCount + YellowOrderCount);
            }
        }

        public int WhiteOrderPercent
        {
            get
            {
                return (WhiteOrderCount * 100) / (RedOrderCount + BlueOrderCount + WhiteOrderCount + YellowOrderCount);
            }
        }
    }

    public class DisplayOrderColorAmount
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }        
        public double RedOrderAmount { get; set; }
        public double YellowOrderAmount { get; set; }
        public double BlueOrderAmount { get; set; }
        public double WhiteOrderAmount { get; set; }

        public double GrandTotal
        {
            get
            {
                return RedOrderAmount + BlueOrderAmount + WhiteOrderAmount + YellowOrderAmount;
            }
        }

        public int RedOrderPercent
        {
            get
            {
                return RedOrderAmount > 0 ? Convert.ToInt32((RedOrderAmount * 100) / (RedOrderAmount + BlueOrderAmount + WhiteOrderAmount + YellowOrderAmount)) : 0;
            }
        }

        public int YellowOrderPercent
        {
            get
            {
                return YellowOrderAmount > 0 ? Convert.ToInt32((YellowOrderAmount * 100) / (RedOrderAmount + BlueOrderAmount + WhiteOrderAmount+ YellowOrderAmount)) : 0;
            }
        }

        public int BlueOrderPercent
        {
            get
            {
                return BlueOrderAmount > 0 ? Convert.ToInt32((BlueOrderAmount * 100) / (RedOrderAmount + BlueOrderAmount + WhiteOrderAmount + YellowOrderAmount)) : 0;
            }
        }

        public int WhiteOrderPercent
        {
            get
            {
                return WhiteOrderAmount > 0 ? Convert.ToInt32((WhiteOrderAmount * 100) / (RedOrderAmount + BlueOrderAmount + WhiteOrderAmount + YellowOrderAmount)) : 0;
            }
        }
    }

    public class OrderCountTime
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public DateTime CreatedDate { get; set; }        
        public int OrderId { get; set; }
        public int Hour24OrderCount
        {
            get
            {
                return RemainingTime.TotalHours >= 0 && RemainingTime.TotalHours <= 24 ? 1 : 0;

            }
        }
        public int Hour48OrderCount
        {
            get
            {
                return RemainingTime.TotalHours > 24 && RemainingTime.TotalHours <= 48 ? 1 : 0;

            }
        }
        public int Hour72OrderCount
        {
            get
            {
                return RemainingTime.TotalHours > 48 && RemainingTime.TotalHours <= 72 ? 1 : 0;

            }
        }
        public int Hour100OrderCount
        {
            get
            {
                return RemainingTime.TotalHours > 72 && RemainingTime.TotalHours <= 100 ? 1 : 0;

            }
        }
        public int Hour100GreaterOrderCount
        {
            get
            {
                return RemainingTime.TotalHours > 100 ? 1 : 0;

            }
        }
        public TimeSpan RemainingTime
        {
            get
            {
                return (DateTime.Now - CreatedDate);
            }
        }

    }

    public class DisplayOrderCountTime
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int Hour24OrderCount { get; set; }
        public int Hour48OrderCount { get; set; }
        public int Hour72OrderCount { get; set; }
        public int Hour100OrderCount { get; set; }
        public int Hour100GreaterOrderCount { get; set; }

        public int GrandTotal
        {
            get
            {
                return Hour24OrderCount + Hour48OrderCount + Hour72OrderCount + Hour100OrderCount+ Hour100GreaterOrderCount;
            }
        }

        public int Hour24OrderPercent
        {
            get
            {
                return Convert.ToInt32( Hour24OrderCount > 0 ? (Hour24OrderCount * 100) / (Hour24OrderCount + Hour48OrderCount + Hour72OrderCount + Hour100OrderCount + Hour100GreaterOrderCount) : 0);
            }
        }
        public int Hour48OrderPercent
        {
            get
            {
                return Convert.ToInt32(Hour48OrderCount > 0 ? (Hour48OrderCount * 100) / (Hour24OrderCount + Hour48OrderCount + Hour72OrderCount + Hour100OrderCount + Hour100GreaterOrderCount) : 0);
            }
        }

        public int Hour72OrderPercent
        {
            get
            {
                return Convert.ToInt32(Hour72OrderCount > 0?(Hour72OrderCount * 100) / (Hour24OrderCount + Hour48OrderCount + Hour72OrderCount + Hour100OrderCount + Hour100GreaterOrderCount):0);
            }
        }

        public int Hour100OrderPercent
        {
            get
            {
                return Convert.ToInt32(Hour100OrderCount > 0 ? (Hour100OrderCount * 100) / (Hour24OrderCount + Hour48OrderCount + Hour72OrderCount + Hour100OrderCount + Hour100GreaterOrderCount) : 0);
            }
        }
        public int Hour100GreaterOrderPercent
        {
            get
            {
                return Convert.ToInt32(Hour100GreaterOrderCount > 0 ? (Hour100GreaterOrderCount * 100) / (Hour24OrderCount + Hour48OrderCount + Hour72OrderCount + Hour100OrderCount + Hour100GreaterOrderCount) : 0);
            }
        }

    }

    public class OPReportExportDc
    {
        public string OrderColor { get; set; }
        public int OrderId { get; set; }
        public string CityName { get; set; }
        public string WarehouseName { get; set; }
        public DateTime CreatedDate { get; set; }
        public double TotalAmount { get; set; }
        public string Status { get; set; }
        public string StoreName { get; set; }
        public DateTime? ETADate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string CustomerType { get; set; }
    }
    public class OP_Report_Export_Response
    {
        public string Message { get; set; }
        public bool Status { get; set; }
        public object Data { get; set; }
    }
}
