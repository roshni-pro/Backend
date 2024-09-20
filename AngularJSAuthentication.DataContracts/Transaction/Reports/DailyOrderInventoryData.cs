using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Transaction.Reports
{
    public class DailyOrderInventoryData
    {
        public string WarehouseName { get; set; }
        public double ShippedAmount { get; set; }
        public double IssuedAmount { get; set; }
        public double ReadyToDispatchAmount { get; set; }
        public double DeliveryRedispatchAmount { get; set; }
        public double DeliveryCanceledAmount { get; set; }
        public double InventoryAmount { get; set; }
        public double DeliveredButNotReconciled { get; set; }
    }

    public class WorkingCapitalData
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string WarehouseName { get; set; }
        public int WarehouseId { get; set; }
        public double AvgSale { get; set; }
        public double AgentDues { get; set; }
        public double ShippedAmount { get; set; }
        public double IssuedAmount { get; set; }
        public double ReadyToDispatchAmount { get; set; }
        public double DeliveryRedispatchAmount { get; set; }
        public double DeliveryCanceledAmount { get; set; }
        public double InventoryAmount { get; set; }
        public double DeliveredButNotReconciled { get; set; }
        public double CashInOperation { get; set; }
        public decimal ChequeInOperation { get; set; }
        public decimal ChequeInBank { get; set; }
        public decimal ChequeBounce { get; set; }
        public double SupplierCredit { get; set; }
        public double SupplierAdvances { get; set; }
        public double Invoiceintransit { get; set; }
        public double AverageAssetDays { get; set; }
        public double OnlinePrePaidAmount { get; set; }
        public double? OnlinePrePaidAmounthdfc { get; set; }
        public double? OnlinePrePaidAmountePaylater { get; set; }
        public double? OnlinePrePaidAmountmPos { get; set; }
        public double DamageStockAmount { get; set; }
        public double? NonSellableStockAmount { get; set; }
        public double PendingGRNAmount { get; set; }
        public double GoodsReceivedNotInvoiced { get; set; }
        public double? IRPendingBuyerSide { get; set; }
        public List<OnlinePayment> OnlinePayments { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
    }

    public class WarehouseCashAmount
    {
        public int WarehouseId { get; set; }
        public decimal Amount { get; set; }
    }

    public class OnlinePayment
    {
        public string PaymentFrom { get; set; }
        public double? Amount { get; set; }
    }

    public class OnlinePaymentV2WithpaymentFrom 
    {
        public int WarehouseId { get; set; }
        public decimal Amount { get; set; }
        public string paymentFrom { get; set; }
    }
    public class WarehouseChequeAmount
    {
        public int WarehouseId { get; set; }
        public int ChequeStatus { get; set; }
        public decimal Amount { get; set; }
    }
    public class WorkingCapitalDC
    {
        public string WarehouseName { get; set; }
        public int WarehouseId { get; set; }
        public double AvgSale { get; set; }
        public double AgentDues { get; set; }
        public double ShippedAmount { get; set; }
        public double IssuedAmount { get; set; }
        public double ReadyToDispatchAmount { get; set; }
        public double DeliveryRedispatchAmount { get; set; }
        public double DeliveryCanceledAmount { get; set; }
        public double InventoryAmount { get; set; }
        public double DeliveredButNotReconciled { get; set; }
        public double CashInOperation { get; set; }
        public double ChequeInOperation { get; set; }
        public double ChequeInBank { get; set; }
        public double ChequeBounce { get; set; }
        public double SupplierCredit { get; set; }
        public double SupplierAdvances { get; set; }
        public double Invoiceintransit { get; set; }
        public double OnlinePrePaidAmount { get; set; }
        public double DamageStockAmount { get; set; }
        public double NonSellableStockAmount { get; set; }
        public double PendingGRNAmount { get; set; }
        public double? OnlinePrePaidAmounthdfc { get; set; }
        public double? OnlinePrePaidAmountePaylater { get; set; }
        public double? OnlinePrePaidAmountmPos { get; set; }
        public double GoodsReceivedNotInvoiced { get; set; }
        public double? IRPendingBuyerSide { get; set; }
        public DateTime CreateDate { get; set; }

        public double Inventory
        {
            get
            {
                return InventoryAmount + DamageStockAmount + PendingGRNAmount;

            }

        }

        public double InventoryInTransit
        {
            get
            {
                return IssuedAmount + ShippedAmount + DeliveryRedispatchAmount + DeliveryCanceledAmount;

            }

        }

        public double Cheque
        {
            get
            {
                return ChequeInOperation + ChequeInBank ;

            }

        }
        public double TotalAssets
        {
            get
            {
                return Inventory + IssuedAmount + ShippedAmount + DeliveryRedispatchAmount + DeliveryCanceledAmount
                       + ReadyToDispatchAmount + DeliveredButNotReconciled + CashInOperation + ChequeInOperation + ChequeInBank 
                       + AgentDues + OnlinePrePaidAmount;

                    //AgentDues + IssuedAmount + ReadyToDispatchAmount
                    //+ ShippedAmount + DeliveredButNotReconciled + DeliveryCanceledAmount
                    //+ DeliveryRedispatchAmount + CashInOperation + ChequeInOperation
                    //+ ChequeInBank + Inventory + SupplierAdvances;

            }
        }

        public double Liability
        {
            get
            {
                return (SupplierCredit) + GoodsReceivedNotInvoiced + (IRPendingBuyerSide.HasValue? IRPendingBuyerSide.Value:0);
            }
        }
        public double SupplierCreditDays
        {
            get
            {
                return AvgSale == 0 ? 0 : (SupplierCredit / (AvgSale));
            }
        }
        public double InventoryDays
        {
            get
            {
                return AvgSale == 0 ? 0 : (Inventory / (AvgSale));
            }
        }

        public double ReadyToDispatchDays
        {
            get
            {
                return AvgSale == 0 ? 0 : (ReadyToDispatchAmount / (AvgSale));
            }
        }
        public double CashDays
        {
            get
            {
                //return AvgSale == 0 ? 0 : ((AgentDues + IssuedAmount
                //    + ShippedAmount + DeliveredButNotReconciled + DeliveryCanceledAmount
                //    + DeliveryRedispatchAmount + CashInOperation + ChequeInOperation
                //    + ChequeInBank) / (AvgSale));

                //  return AvgSale == 0 ? 0 : ((TotalAssets / (AvgSale)) - (InventoryAmount / (AvgSale))- (ReadyToDispatchAmount / (AvgSale)));

                return AvgSale == 0 ? 0 : (InventoryInTransit+ DeliveredButNotReconciled+CashInOperation+ Cheque +AgentDues+OnlinePrePaidAmount) / (AvgSale);
            }
        }
        public double AssetDays
        {
            get
            {
                return AvgSale == 0 ? 0 : (TotalAssets / (AvgSale));
            }
        }

        public double WorkingCapital
        {
            get
            {
                return TotalAssets - Liability;
            }
        }
        public double IncreaseInWC { get; set; }
        public double AverageCashDays { get; set; }
        public double AverageAssetDays { get; set; }
        public List<OnlinePayment> OnlinePayments { get; set; }
    }

    public class DisplayWorkingCapitalDc
    {
        public double AverageInventoryDays { get; set; }
        public double AverageRTDDays { get; set; }
        public double AverageCashDays { get; set; }
        public double AverageAssetDays { get; set; }
        public double NetWorkingCapital { get; set; }
        public double AvgSuplierCreditDays { get; set; }
        public List<newdc> newdc { get; set; }
        //public List<WorkingCapitalDC> WorkingCapitalDCs { get; set; }
    }

    public class newdc
    {
        public DateTime CreateDate { get; set; }
        public WorkingCapitalDC WorkingCapitalDCs { get; set; }
    }


}
