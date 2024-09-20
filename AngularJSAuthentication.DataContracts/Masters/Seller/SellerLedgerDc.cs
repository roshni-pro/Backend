using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.Seller
{
    public class SellerLedgerDc
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public double SaleAmount { get; set; }
        public double ClosingStock { get; set; }
        public double GrossMargin { get; set; }
        public double InventoryDays { get; set; }
        public double NetSale { get; set; }
        public double MarginPercent { get; set; }
    }
    public class BrandParamsDc
    {
        public int SubCatId { get; set; }
        public List<int> CityIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int WarehouseId { get; set; }
    }
    public class BrandLedgerDetailDc
    {
        public int ItemMultiMrpId { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public int BrandId { get; set; }
        public string Brand { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public double? MRP { get; set; }
        public double? TaxRate { get; set; }
        public double? CancelAmt { get; set; }
        public double? SaleAmt { get; set; }
        public double? NetSale { get; set; }
        public double? GrossMargin { get; set; }
        public double? ClosingAmount { get; set; }
        public double? MarginPercent { get; set; }
        public double? InventoryDays { get; set; }

    }

    public class SellerMonthlyChargeMasterDc
    {
        public int SubCatId { get; set; }
        public string ChargeType { get; set; }
        public double ChargeAmount { get; set; }
    }

    public class SellerMonthlyChargeDetailsDc
    {
        public double Amount { get; set; }
        public string ChargeType { get; set; }
        public double ChargeAmount { get; set; }
        public string WarehouseName { get; set; }
        public string CityName { get; set; }
    }
    public class ExportSellerLineItemsDc
    {
        public string WarehouseName { get; set; }
        public string CityName { get; set; }
        public int OrderId { get; set; }
        public int OrderDetailsId { get; set; }
        public int OrderedQty { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public double MRP { get; set; }
        public double TotalAmount { get; set; }

    }

    public class SellerActivetedLineItemDc
    {
        public string WarehouseName { get; set; }
        public string CityName { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public DateTime ActivatedDate { get; set; }
        
    }

    public class SellerClosingLineItemsDc
    {
        public string WarehouseName { get; set; }
        public string CityName { get; set; }
        public string itemBaseName { get; set; }
        public string ItemNumber { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int AvgClosingQty { get; set; }
        public double AvgClosingAmt { get; set; }
        public double Charge { get; set; }

    }
}
