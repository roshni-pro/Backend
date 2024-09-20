using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.constants
{
    public static class StockTypeTableNames
    {
        public static string CurrentStocks
        {
            get { return "CurrentStocks"; }
        }

        public static string VirtualStock
        {
            get { return "VirtualStocks"; }
        }

        public static string DamagedStock
        {
            get { return "DamageStocks"; }
        }

        public static string DeliveredStock
        {
            get { return "DeliveredStocks"; }
        }

        public static string ExpiredStock
        {
            get { return "ExpiredStocks"; }
        }

        public static string FreebieStock
        {
            get { return "FreeStocks"; }
        }

        public static string InReceivedStock
        {
            get { return "InReceivedStocks"; }
        }

        public static string InTransitStock
        {
            get { return "InTransitStocks"; }
        }

        public static string ReservedStock
        {
            get { return "ReservedStocks"; }
        }

        public static string RTDStock
        {
            get { return "RTDStocks"; }
        }
        public static string RTVStock
        {
            get { return "RTVStocks"; }
        }

        public static string DeliveryCancelStock
        {
            get { return "DeliveryCancelStocks"; }
        }

        public static string DeliveryRedispatchStock
        {
            get { return "DeliveryRedispatchStocks"; }
        }

        public static string IssuedStock
        {
            get { return "IssuedStocks"; }
        }

        public static string ITIssueStock
        {
            get { return "ITIssueStocks"; }
        }
        public static string LostAndFoundStock
        {
            get { return "LostAndFoundStocks"; }
        }
        public static string PlannedStock
        {
            get { return "PlannedStocks"; }
        }

        public static string QuarantineStock
        {
            get { return "QuarantineStocks"; }
        }

        public static string ShippedStock
        {
            get { return "ShippedStocks"; }
        }
        public static string DeliveryCanceledRequestStock
        {
            get { return "DeliveryCanceledRequestStocks"; }
        }
        public static string NonSellableStock
        {
            get { return "NonSellableStocks"; }
        }
        public static string ClearanceStockNews
        {
            get { return "ClearanceStockNews"; }
        }
        public static string InventoryReserveStocks
        {
            get { return "InventoryReserveStocks"; }
        }
        public static string ClearancePlannedStocks
        {
            get { return "ClearancePlannedStocks"; }
        }
        //new add for non revenue
        public static string NonRevenueStocks
        {
            get { return "NonRevenueStocks"; }
        }

        public static string GetStockTypeChildTableNames(string tableName)
        {
            if (tableName == CurrentStocks)
            {
                return "CurrentStockHistory";
            }
            else if (tableName == ClearancePlannedStocks)
            {
                return "ClearancePlannedStockskHistory";
            }
            else if (tableName == DamagedStock)
            {
                return "DamageStockHistory";
            }
            else if (tableName == FreebieStock)
            {
                return "FreeStockHistory";
            }
            else if (tableName == NonSellableStock)
            {
                return "NonSellableStockHistory";
            }
            else if (tableName == ClearanceStockNews)
            {
                return "ClearanceStockNewHistory";
            }
            else if (tableName == InventoryReserveStocks)
            {
                return "InventoryReserveStockHistory";
            }
            else
            {
                return tableName;
            }
        }
    }

    public class StockTransferTypeName
    {
        public static string CurrentStocks
        {
            get { return "CurrentStocks"; }
        }

        public static string DamagedStocks
        {
            get { return "DamagedStocks"; }
        }
        public static string PurchaseInventory
        {
            get { return "PurchaseInventory"; }
        }
        public static string OrderCancelInventory
        {
            get { return "OrderCancelInventory"; }
        }
        public static string ManualInventory
        {
            get { return "ManualInventory"; }
        }
        public static string ExpiredStock
        {
            get { return "ExpiredStocks"; }
        }
        public static string InventoryReserveStocks
        {
            get { return "InventoryReserveStocks"; }
        }
        public static string ClearancePlannedStocks
        {
            get { return "ClearancePlannedStocks"; }
        }

    }
}
