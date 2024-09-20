using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Mongo.BuyerDashboard
{
   public class ItemSaleVsInventoryData
    {
        public string ItemNumber { get; set; }
        public int ItemMultiMrpId { get; set; }
        public string ItemName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public double? MRP { get; set; }
        public double? TaxPercentage { get; set; }
        public List<reportSale> sales { get; set; }
        public List<reportInventory> inventories { get; set; }
    }
   public class reportSale
    {
        public string multimrpwarehouse { get; set; } // Inventory
        public int itemmultimrpid { get; set; }
        public int warehouseid { get; set; }
        public int Qty { get; set; }
        public double amount { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
   public class reportInventory
    {
        public string multimrpwarehouse { get; set; } // Inventory
        public int itemmultimrpid { get; set; }
        public int warehouseid { get; set; }
        public int Qty { get; set; }
        public double amount { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
