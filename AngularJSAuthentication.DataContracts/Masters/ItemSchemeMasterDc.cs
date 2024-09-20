using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{

    public class ItemSchemeMasterdetailsDc
    {
        public long ItemSchemeDetailId { get; set; }
        public long ItemSchemeMasterId { get; set; }
        public bool IsActive { get; set; }

    }

    public class ItemSchemeMasterDC
    {
        public long Id { get; set; }
        public int Cityid { get; set; }
        public int SubSubCatId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsApproved { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public ICollection<ItemSchemeDetailDc> ItemSchemeDetails { get; set; }
        public string UploadedSheetUrl { get; set; }
        public string BrandName { get; set; }
        public string CityName { get; set; }
        public string ApprovedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsDeleted { get; set; }
        public int totalRecord { get; set; }
        public int Status { get; set; }
    }
    public class ItemSchemeDetailDc
    {
        public long Id { get; set; }
        public long ItemSchemeMasterId { get; set; }
        public string ItemNumber { get; set; }
        public string CompanyCode { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string CompanyStockCode { get; set; }
        public string ItemName { get; set; }
        public double MRP { get; set; }
        public double PTR { get; set; } //in  % 
        public double BaseScheme { get; set; }
        public bool IncludeBaseSchmePOPrice { get; set; }
        public bool IsIncludeMaxSlabPOPrice { get; set; }
        public bool IsIncludeOnInvoiceMarginPOPrice { get; set; }
        public ICollection<ItemSchemeSlabDc> Slabs { get; set; }
        public ICollection<ItemSchemeFreebiesDc> ItemSchemeFreebiess { get; set; }
        public double onvoiceMargin { get; set; }
        public double offinvoicemargin { get; set; }
        public bool IsActive { get; set; }
        public string ErrorMessage { get; set; }
        public string ItemIds { get; set; }
        public string OfferIds { get; set; }

    }
    public class ItemSchemeSlabDc
    {

        public long Id { get; set; }
        public int SlabPurchaseQTY { get; set; } //in MOQ
        public double SlabScheme { get; set; }//in  %
        public long ItemSchemeDetailId { get; set; }


    }
    public class ItemSchemeFreebiesDc
    {

        public long Id { get; set; }
        public string ItemName { get; set; }
        public int BaseItemQty { get; set; }
        public string ItemCompanyCode { get; set; }
        public string ItemCompanyStockCode { get; set; }
        public double MRP { get; set; }
        public int Qty { get; set; }
        public int StockQty { get; set; }
        public long ItemSchemeDetailId { get; set; }


    }

}
