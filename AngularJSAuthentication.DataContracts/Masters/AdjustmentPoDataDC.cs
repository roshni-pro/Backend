using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class AdjustmentPoDataDC
    {
        public string WarehouseName { get; set; }
        public int PurchaseOrderId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string POStatus { get; set; }
        public string PRPaymentType { get; set; }
        public double PriceRecived { get; set; }
        public string SupplierName { get; set; }
        public string itemname { get; set; }
        public string GRstatus { get; set; }
        public DateTime? CreationDate { get; set; }
        public string GRNo { get; set; }
        public DateTime? GRNDate { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int TotalQuantity { get; set; }
        public int GRQTY { get; set; }
        public int DamageQty { get; set; }
        public int ExpiryQty { get; set; }
        public bool IsFreeItem { get; set; }
        public double Price { get; set; }
        public DateTime? MFGDate { get; set; }
        public string VehicleType { get; set; }
        public string VehicleNumber { get; set; }
        public string AdjustmentPo { get; set; }
    }

    public class DispatchToSpendTrackerOpostDcs
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? DateType { get; set; }
        public List<int?> WarehouseIds { get; set; }
        public List<int?> ClusterIds { get; set; }
        public List<int?> SalesPersoneIds { get; set; }
    }




    public class DispatchToSpendTrackerResponse
    {
        public string WarehouseName { get; set; }
        public string Cluster { get; set; }
        public string OrderBy { get; set; }
        public string SalesPerson { get; set; }
        public string Skcode { get; set; }     
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; }
        public string Brand { get; set; }
        public string itemname { get; set; }
        public int orderid { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? RTDDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public String OrderToDeliveryHrs { get; set; }       
        public double BookingValue { get; set; }
        public int Bookingqty { get; set; }
        public double? DispatchValue { get; set; }    
        public int? Dispatchqty { get; set; }
        public double? CancelValue { get; set; }
        public int? CancelQty { get; set; }
        public string CancelRemarks { get; set; }       
        public double BillDiscountAmount { get; set; }
        public double? FreebiesValue { get; set; }       
        public double WalletAmount { get; set; }       

    }

    public class DispatchToSpendTrackerDC
    {
        public string WarehouseName { get; set; }
        public string ClusterName { get; set; }
        public string OrderTakenSalesPerson { get; set; }
        public string OrderBy { get; set; }
        public string Skcode { get; set; }
        public string CustomerName { get; set; }
        public string ShippingAddress { get; set; }
        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; }
        public string SubsubcategoryName { get; set; }
        public string itemname { get; set; }
        public int orderid { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveredDate { get; set; }

        public int qty { get; set; }
        public double UnitPrice { get; set; }
        public double NPPPrice { get; set; }
        
        public double GrossAmount { get; set; }
        public double TCSAmount { get; set; }
        public bool IsFreeItem { get; set; }      
        public int OrderDetailId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }
        public double BillDiscountAmount { get; set; }

        public double WalletAmount { get; set; }
        public int ClusterId { get; set; }

        public int OrderTakenPersonId { get; set; }

        public DateTime? RTDDate { get; set; }

        public DateTime? CancelationDate { get; set; }

        public int? Dispatchqty { get; set; }
        public double? DispatchUnitPrice { get; set; }
        public double? App { get; set; }
        public string CancelRemarks { get; set; }
    }

}
