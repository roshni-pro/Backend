using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.ForCast
{
    public class SalesIntentApprovalMainListDC
    {
        public int TotalRec { get; set; }
        public List<SalesIntentApprovalDC> salesIntentApprovalDCs { get; set; }
    }

    public class SalesIntentApprovalDC
    {
        public long Id { get; set; }
        public string Category { get; set; }
        public string Brand { get; set; }
        public string Product { get; set; }

        public string Item_Display_Name { get; set; }
        public string WarehouseName { get; set; }
        public int System_Forecasted_Qty { get; set; }
        public int Sellers_Additional_Qty { get; set; }
        public double Selling_Price {get;set;}
        public string Requested_By  { get; set; }
        public int?  IsSalesLeadApproved { get; set; }
        public int? IsBuyerLeadApproved { get; set; }
        public int Status { get; set; }
        public int? MinOrderQty { get; set; } //New Change

        public int? NoOfSet { get; set; } //New Change

        public int? TotalQty { get; set; } //New Change for export MOQ*NoOfSet
        public DateTime? ETADate { get; set; } //New Change

        public double? BuyingPrice { get; set; } // New Change

        public string Comments { get; set; } //New Change

        public int? isReject { get; set; } //New Change

        public string SellerName { get; set; } //New Change for export MOQ*NoOfSet
        public string BuyerName { get; set; } //New Change for export MOQ*NoOfSet
        public string SubcategoryName { get; set; } //New Change for export
        public int? ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; } //New Add for ROC
        public int Tag { get; set; } //New Add for ROC




    }

    public class SalesIntentTotalApproavalOldDC
    {

        public List<SalesIntentApprovalOldDC> SalesIntentApprovalOldDCs { get;set;}        
        public int TotRec { get; set; }
    }

    public class SalesIntentDashboardDC
    {

        public List<SalesIntentmtdDC> SalesIntentApprovalOldDCs { get; set; }

        public List<SalesIntentytdDC> SalesIntentytdobjDC { get; set; }
       // public int TotalCount { get; set; }
    }

    public class SalesIntentDashboardALLDC
    {

        public List<SalesIntentalldDC> SalesIntentApprovalOldDCs { get; set; }

      //  public List<SalesIntentytdDC> SalesIntentytdobjDC { get; set; }
        // public int TotalCount { get; set; }
    }

    public class SalesIndentExportDC
    {
        // public  long Id  { get; set; }
        public string ItemName { get; set; }
        public string BrandName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int? Indent_Qty  { get; set; }
        public double? SaleIndentPrice   { get; set; }
        public double? IndentValue { get; set; }
        public int? GR_Qty    { get; set; }
        public double? GR_Price { get; set; }
        public double? GRValue { get; set; }
        public int? SaleQty { get; set; }
        public double? SalePrice { get; set; }

        public string RegionalSalesLead   { get; set; }
        public string BuyerName { get; set; }
        public string SalesExecutiveName  { get; set; }
        public DateTime? ETADate { get; set; }
        public DateTime? CreatedDate { get; set; }



    }
    public class SalesIndentExportYTDDC
    {
        // public  long Id  { get; set; }
        public string Hub_Name { get; set; }
        public string ItemName { get; set; }
        public string BrandName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int? Indent_Qty { get; set; }
        public double? SaleIndentPrice { get; set; }
        public double? IndentValue { get; set; }
        public int? GR_Qty { get; set; }
        public double? GR_Price { get; set; }
        public double? GRValue { get; set; }
        public int? SaleQty { get; set; }
        public double? SalePrice { get; set; }

        public string RegionalSalesLead { get; set; }
        public string BuyerName { get; set; }
        public string SalesExecutiveName { get; set; }
        public DateTime? ETADate { get; set; }
        public DateTime? CreatedDate { get; set; }



    }

    public class SalesIntentmtdDC
    {
        public int ItemMultiMRPId { get; set; }
        public int Categoryid { get; set; }
        public int SubsubCategoryid  { get; set; }
        public int SubCategoryId { get; set; }
        public int TotalCount { get; set; }

        public double? IndentPrice { get; set; }
        public double? SalePrice { get; set; }
        public double? GRQtyPrice { get; set; }
        public double? Performance   { get; set; }
        public string BrandName    { get; set; }


    }
    public class SalesIntentalldDC
    {
        public int TotalRecord { get; set; }
        public int SubsubCategoryid { get; set; }
        public string SubsubcategoryName { get; set; }
        public int BrandId { get; set; }
        public string y_BrandName { get; set; }
        public double? y_IndentPrice { get; set; }
        public double? y_GRQtyPrice { get; set; }
        public double? y_SalePrice { get; set; }
        public double? y_Performance { get; set; }
        public int m_TotalCount { get; set; }
        public string m_BrandName { get; set; }
        public double? m_IndentPrice { get; set; }
        public double? m_GRQtyPrice { get; set; }
        public double? m_SalePrice { get; set; }
        public double? m_Performance { get; set; }

    }
    public class SalesIntentytdDC
    {
        public int ItemMultiMRPId { get; set; }
        public int Categoryid { get; set; }
        public int SubsubCategoryid { get; set; }
        public int SubCategoryId { get; set; }
        public int TotalCount { get; set; }
        public double? IndentPrice { get; set; }
        public double? SalePrice { get; set; }
        public double? GRQtyPrice { get; set; }
        public double? Performance { get; set; }
        public string BrandName { get; set; }

    }
    public class SalesIntentApprovalOldDC
    {
        public long Id { get; set; }
        public int Categoryid { get; set; }
        public int SubsubCategoryid { get; set; }
        public string Category { get; set; }
        public string Brand { get; set; }
        public string Product { get; set; }
        public string WarehouseName { get; set; }
        public string Item_Display_Name { get; set; }
        public int System_Forecasted_Qty { get; set; }
        public int Sellers_Additional_Qty { get; set; }
        public double Selling_Price { get; set; }
        public string Requested_By { get; set; }
        public int Status { get; set; }
        public string IsSalesLeadApproved { get; set; }
        public string IsBuyerLeadApproved { get; set; }

        public int? MinOrderQty { get; set; } //New Change


        public int? NoOfSet { get; set; } //New Change
        public int? TotalQty { get; set; } //New change for Export  MOQ*NoOfSet

        public DateTime? ETADate { get; set; } //New Change

        public double? BuyingPrice { get; set; } // New Change

        public string Comments { get; set; } //New Change

        public int? isReject { get; set; } //New Change
        public string SellerName { get; set; } //New Change for export MOQ*NoOfSet
        public string BuyerName { get; set; } //New Change for export MOQ*NoOfSet

        public string SubcategoryName { get; set; } //New Change for export
        public int? ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; } //New Add for ROC
        public int Tag { get; set; } //New Add for ROC

    }
}
