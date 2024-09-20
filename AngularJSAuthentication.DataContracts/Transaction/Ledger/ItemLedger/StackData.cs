using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Ledger.ItemLedger
{
    public class StackData
    {
        public List<LastMonthClosing> LastMonthClosings { get; set; }
        public List<GrDC> GRIn { get; set; }
        public List<IrDC> IRIn { get; set; }
        public List<OutDc> Outs { get; set; }
        public List<LastPurchase> LastPurchases { get; set; }
        public List<ItemMasterPurchase> ItemMasterPurchases { get; set; }
        public List<LastMonthApp> LastMonthApps { get; set; }
        public List<WarehouseDto> Warehouses { get; set; }

    }

    public class LastPurchase
    {
        public int itemmultimrpid { get; set; }
        public int warehouseid { get; set; }
        public double LastPurchasePrice { get; set; }
    }

    public class LastMonthApp
    {
        public int itemmultimrpid { get; set; }
        public int warehouseid { get; set; }
        public double APP { get; set; }
    }

    public class ItemMasterPurchase
    {
        public int itemmultimrpid { get; set; }
        public int warehouseid { get; set; }
        public double UnitPrice { get; set; }
    }


    public class LastMonthClosing
    {
        public int itemmultimrpid { get; set; }
        public int warehouseid { get; set; }
        public int RemQty { get; set; }
        public double Price { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ObjectId { get; set; }
    }

    public class GrDC
    {
        public int Id { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int WarehouseId { get; set; }
        public int POId { get; set; }
        public string SupplierName { get; set; }
        public int Qty { get; set; }
        public double Price { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? InDate { get; set; }
        public int RemQty { get; set; }
        public string Source { get; set; }
        public double ItemPurchasePrice { get; set; }
        public double UnitPrice { get; set; }
        public int FinalRemQty { get; set; }
        public string TransactionId { get; set; }
        public double CancelInPP { get; set; }
        public string CancelInTransactionId { get; set; }
    }

    public class IrDC
    {
        public int ItemMultiMrpId { get; set; }
        public int WarehouseId { get; set; }
        public int POId { get; set; }
        public int Qty { get; set; }
        public double Price { get; set; }
        public int IrNumber { get; set; }
    }

    public class OutDc
    {
        public int Id { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int WarehouseId { get; set; }
        public int ObjectId { get; set; }
        public int Qty { get; set; }
        public double? SellingPrice { get; set; }
        public string Destination { get; set; }
        public DateTime CreatedDate { get; set; }
        public double? PurchasePrice { get; set; }
        public int? InWarehouseId { get; set; }
        public string InTransactionId { get; set; }
        public string InTransType { get; set; }
        public bool IsDone { get; set; }
        public int? InMrpId { get; set; }
    }

    public class ItemMultiMrp
    {
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }
    }

    public class ItemMultiMrpPurchasePrice
    {
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }
        public double PurchasePrice { get; set; }
    }

    public class WarehouseDto
    {
        public int warehouseid { get; set; }
        public bool IsCnf { get; set; }
    }

}
