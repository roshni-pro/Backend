using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External
{
    public class FranchiseMaster
    {
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public WarehouseDc Warehouse { get; set; }
        public List<ItemMasterCentralDc> ItemMasterCentrals { get; set; }
        public List<FranchiseItem> FranchiseItems { get; set; }
    }

    public class FranchiseItem
    {
        public int Qty { get; set; }
        public string ItemNumber { get; set; }
        public string UnitofQuantity { get; set; }
        public string UOM { get; set; }
        public double MRP { get; set; }
        public double PurchasePrice { get; set; }
    }


    public class WarehouseDc 
    {
      
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string CompanyName { get; set; }
        public string GSTin { get; set; }
        public string aliasName { get; set; }
        public bool active { get; set; }

        public int CompanyId { get; set; }
        public int Stateid { get; set; }
        public string StateName { get; set; }
        public int Cityid { get; set; }
        public string CityName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdateBy { get; set; }
        public bool Deleted { get; set; }
        public int? GruopID { get; set; }
        public string TGrpName { get; set; }
        public bool IsKPP { get; set; }
        public bool Createactive { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public bool IsKppShowAsWH { get; set; }

        public virtual ICollection<WarehouseCategory> warehouseCategory { get; set; }
                    
        public int RegionId { get; set; }
        //Added By Ravindra
        public double CapacityinAmount { get; set; }
        //public string RegionName { get; set; }
        //end


    }

    public class ItemMasterCentralDc 
    {
        
        public int Id { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public int SubsubCategoryid { get; set; }
        public string SubSubCode { get; set; }
        public int CompanyId { get; set; }
        public string CategoryName { get; set; }
        public int BaseCategoryid { get; set; }
        public string BaseCategoryName { get; set; }
        public string SubcategoryName { get; set; }
        public string SubsubcategoryName { get; set; }

        public string itemcode { get; set; }
        public string SellingUnitName { get; set; }
        public string PurchaseUnitName { get; set; }
        public double price { get; set; }
        public double VATTax { get; set; }
        public string HSNCode { get; set; }
        public bool active { get; set; }
        public string LogoUrl { get; set; }
        public int MinOrderQty { get; set; }
        public int PurchaseMinOrderQty { get; set; }
        public int GruopID { get; set; }
        public string TGrpName { get; set; }

        //Code For Cess group
        public int? CessGrpID { get; set; }
        public string CessGrpName { get; set; }
        public double TotalCessPercentage { get; set; }

        public double Discount { get; set; }
        public double UnitPrice { get; set; }
        public string Number { get; set; }
        public string PurchaseSku { get; set; }
        public string SellingSku { get; set; }
        public double PurchasePrice { get; set; }
        public double? GeneralPrice { get; set; }
        public string Description { get; set; }
        public double PramotionalDiscount { get; set; }
        public double TotalTaxPercentage { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool Deleted { get; set; }
        public bool free { get; set; }
        public bool IsDailyEssential { get; set; }
        public double DisplaySellingPrice { get; set; }
        public string StoringItemName { get; set; }
        public double SizePerUnit { get; set; }
        public string HindiName { get; set; }
        public string Barcode { get; set; }
        public double Margin { get; set; }
        public int? marginPoint { get; set; }
        public int? promoPoint { get; set; }
        public int? promoPerItems { get; set; }
        public double NetPurchasePrice { get; set; }
        public bool IsPramotionalItem { get; set; }
       
        public int userid { get; set; }
        public bool IsBulkItem { get; set; }

        public bool IsHighestDPItem
        {
            get; set;
        }

        public bool IsOffer
        {
            get; set;
        }
        //By Anu
        public double DefaultBaseMargin { get; set; }
        public bool ShowMrp { get; set; }
        public bool ShowUnit { get; set; }//Min order qty

        public bool ShowUOM { get; set; }//

        public bool ShowType { get; set; }
        public string ShowTypes { get; set; } // fast slow non movinng
        public string Reason { get; set; } // MRP Issue Stock Unavailable  Price Issue Other

        public string itemUnit { get; set; }

        //ItemMultiMRPId 20/03/2019 Harry
        public string itemname { get; set; }
        public string itemBaseName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double MRP { get; set; }
        public string UpdateBy { get; set; }
        public string UnitofQuantity { get; set; }
        public string UOM { get; set; }//Unit of masurement like GM Kg 

        //Anu (29/04/2019)
        public bool IsSensitive { get; set; }//Is Sensitive Yes/No
        public bool IsSensitiveMRP { get; set; }//Is Sensitive Yes/No
        public bool IsTradeItem { get; set; }

        public int? ShelfLife { get; set; } //in Days

        public bool IsReplaceable { get; set; }// Is Item Is Replaceble       
        //By Ravindra
        public int Type { get; set; } // Material Type
        public int BomId { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
    }
}
