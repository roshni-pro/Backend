using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class ItemSchemeExcelUploaderMastersDc
    {
        public long Id { get; set; }
        public int Cityid { get; set; }
        public int SubSubCatId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public ICollection<ItemSchemeExcelUploaderDetailDc> ItemSchemeExcelUploaderDetails { get; set; }
        public string UploadedSheetUrl { get; set; }
        public int Status { get; set; } // 0 =Inprocess, 1=Failed , 2=Success
        public bool IsActive { get; set; }
        public long ItemSchemeMasterId { get; set; }
        public string BrandName { get; set; }
        public string CityName { get; set; }
        public string ApprovedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsDeleted { get; set; }
        public int totalRecord { get; set; }
        public string StatusType
        {
            get
            {
                if (Status == 0)
                    return "Inprocess";
                else if (Status == 1)
                    return "Failed";
                else
                    return "Success";
            }
        } //// 0 =Inprocess, 1=Pending , 2=Success

    }
    public class ItemSchemeExcelUploaderDetailDc
    {

        public long Id { get; set; }
        public long ExcelUploaderMasterId { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyStockCode { get; set; }
        public string ItemName { get; set; }
        public double MRP { get; set; }
        public double PTR { get; set; } //in  % 
        public double BaseScheme { get; set; }
        public bool IsIncludeBaseSchmePOPrice { get; set; }
        public bool IsIncludeMaxSlabPOPrice { get; set; }
        public bool IsIncludeOnInvoiceMarginPOPrice { get; set; }
        public int SlabPurchaseQTY1 { get; set; } //in MOQ
        public double SlabScheme1 { get; set; }//in  %
        public int SlabPurchaseQTY2 { get; set; } //in MOQ
        public double SlabScheme2 { get; set; }//in  %
        public int SlabPurchaseQTY3 { get; set; } //in MOQ
        public double SlabScheme3 { get; set; }//in  %
        public int SlabPurchaseQTY4 { get; set; } //in MOQ
        public double SlabScheme4 { get; set; }//in  %
        public int FreeBaseItemQty { get; set; }
        public string FreeChildItem { get; set; }
        public string FreeChildItemCompanycode { get; set; }
        public string FreeChildItemCompanyStockcode { get; set; }
        public double FreeItemMRP { get; set; }
        public int FreeItemQty { get; set; }
        public int FreeItemStockQty { get; set; }
        public bool IsFreeStock { get; set; }
        public double onvoiceMargin { get; set; }
        public double offinvoicemargin { get; set; }
        public string ErrorMessage { get; set; }


    }
    public class ItemSchExcelUploaderDetails
    {
        public long Id { get; set; }
        public long ExcelUploaderMasterId { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyStockCode { get; set; }
        public string ItemName { get; set; }
        public double MRP { get; set; }
        public double PTR { get; set; } //in  % 
        public double BaseScheme { get; set; }
        public bool IsIncludeBaseSchmePOPrice { get; set; }
        public bool IsIncludeMaxSlabPOPrice { get; set; }
        public bool IsIncludeOnInvoiceMarginPOPrice { get; set; }
        public int SlabPurchaseQTY1 { get; set; } //in MOQ
        public double SlabScheme1 { get; set; }//in  %
        public int SlabPurchaseQTY2 { get; set; } //in MOQ
        public double SlabScheme2 { get; set; }//in  %
        public int SlabPurchaseQTY3 { get; set; } //in MOQ
        public double SlabScheme3 { get; set; }//in  %
        public int SlabPurchaseQTY4 { get; set; } //in MOQ
        public double SlabScheme4 { get; set; }//in  %
        public int FreeBaseItemQty { get; set; }
        public string FreeChildItem { get; set; }
        public string FreeChildItemCompanycode { get; set; }
        public string FreeChildItemCompanyStockcode { get; set; }
        public double FreeItemMRP { get; set; }
        public int FreeItemQty { get; set; }
        public int FreeItemStockQty { get; set; }
        public bool IsFreeStock { get; set; }
        public double onvoiceMargin { get; set; }
        public double offinvoicemargin { get; set; }
        public string ErrorMessage { get; set; }
        public string EANNo { get; set; }
        public string ItemCode { get; set; }
        public int ItemMultiMRP { get; set; }
        public double? GST { get; set; }
        public string City { get; set; }
        public string Zone { get; set; }
        public double? Rate1 { get; set; }
        public double? Rate2 { get; set; }
        public double? Rate3 { get; set; }
        public double? QPSTarget { get; set; }
        public double? QPS { get; set; } //Percent
        public double? Promo { get; set; }
        public double? VisibilityPercentage { get; set; }
        public string KVIANDNonKVI { get; set; }
        public double? AdditionalScheme { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int totalRecord { get; set; } 
    }


}
