using AngularJSAuthentication.API.Controllers;
using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.API.DataContract
{
    public class HubCurrencyCollectionDc
    {
        public long Id { get; set; }
        public int TotalAssignmentCount { get; set; }
        public int Warehouseid { get; set; }
        public string WarehouseName { get; set; }
        public decimal TotalCashAmt { get; set; }
        public decimal TotalOnlineAmt { get; set; }
        public decimal TotalCheckAmt { get; set; }
        public decimal TotalDeliveryissueAmt { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public decimal TotalDueAmt { get; set; }
        public string DueResion { get; set; }
        public string Status { get; set; }
        public DateTime BOD { get; set; }
        public DateTime? EOD { get; set; }
        public decimal TotalBankCashAmt { get; set; }
        public decimal TotalBankChequeAmt { get; set; }
        public DateTime? TotalBankDepositDate { get; set; }
        public string ReturnComment { get; set; }

        // public List<MTDCollectionDC> MTDCollectionDCs { get; set; }
    }
    public class MTDCollectionDC
    {
        //  public List<HubCurrencyCollectionDc> hubCurrencyCollectionDcs { get; set; }
        public double MTDTotalCollection { get; set; }
        public decimal MTDTotalBankCashDeposit { get; set; }
        public decimal MTDTotalBankChequeDeposit { get; set; }
        public double TodayCollection { get; set; }

        public double TodaybankCashDeposit { get; set; }
        public double TodaybankChequeDeposit { get; set; }


    }
    public class TotalAmountDC
    {


    }
    public class HubCurrencyCollectionANDMTDCollectionDC
    {
        public List<HubCurrencyCollectionDc> hubCurrencyCollectionDcs { get; set; }
        public MTDCollectionDC mtDCollectionDC { get; set; }
        public decimal TotalCollections { get; set; }
        public decimal TotalBankCashAmt { get; set; }
        public decimal TotalBankChequeAmt { get; set; }

    }

    public class hubCashCollectionDc
    {
        public long Id { get; set; }
        public long currencyHubStockId { get; set; }
        public int CurrencyDenominationId { get; set; }
        public int CurrencyCount { get; set; }
        public int? WarehousePeopleId { get; set; }
        public string CurrencyDenominationTitle { get; set; }
        public int CurrencyDenominationValue { get; set; }
        public string CashCurrencyType { get; set; }
        public int ExchangeInCurrencyCount { get; set; }
        public int ExchangeOutCurrencyCount { get; set; }
        public int CurrencyDenominationTotal
        {
            get
            {
                return CurrencyCount * CurrencyDenominationValue;
            }
        }
        public int BankDepositCurrencyCount { get; set; }
        public int? OpenCurrencyCount { get; set; }
        public string ExchangeComment { get; set; }
        public DateTime? CreateDate { get; set; }
        public int Warehouseid { get; set; }
        public string WarehouseName { get; set; }
    }

    public class CashCollectionDc
    {


        public long Id { get; set; }
        public long CurrencyCollectionId { get; set; }
        public int CurrencyDenominationId { get; set; }
        public int CurrencyCountByDBoy { get; set; }
        public int DBoyPeopleId { get; set; }
        public int? CurrencyCountByWarehouse { get; set; }
        public int? WarehousePeopleId { get; set; }
        public string CurrencyDenominationTitle { get; set; }
        public int CurrencyDenominationValue { get; set; }
        public string CashCurrencyType { get; set; }

        public int TotalCashCurrencyValue
        {
            get
            {
                return CurrencyDenominationValue * CurrencyCountByDBoy;
            }
        }
    }
    public class ChequeCollectionPaggingData
    {
        public int total_count { get; set; }
        public List<ChequeCollectionDc> ChequeCollectionDcs { get; set; }
    }
    public class ChequeCollectionDc
    {
        public long Id { get; set; }
        public long? CurrencyCollectionId { get; set; }
        public string ChequeNumber { get; set; }
        public decimal ChequeAmt { get; set; }
        public int? DBoyPeopleId { get; set; }
        public int? WarehousePeopleId { get; set; }
        public DateTime ChequeDate { get; set; }
        public DateTime? BankSubmitDate { get; set; }
        public int ChequeStatus { get; set; }
        public string ChequeimagePath { get; set; }
        public bool IsChequeClear { get; set; }
        public string SKCode { get; set; }
        public string ChequeStatusText
        {
            get
            {
                ChequeStatusEnum chequeStatusEnum = (ChequeStatusEnum)ChequeStatus;
                if (chequeStatusEnum != null)
                    return chequeStatusEnum.ToString();
                else
                    return "";
            }
        }
        public int? CurrencyHubStockId { get; set; }
        public int? AgentId { get; set; }
        public int? WarehouseId { get; set; }
        public int? AgentSettelmentId { get; set; }
        public bool Ischecked { get; set; }
        public int? OrderId { get; set; }

        public string DBoyName { get; set; }
        public int? Deliveryissueid { get; set; }
        public string ChequeBankName { get; set; }
        public string DepositBankName { get; set; }
        public long? CurrencySettlementid { get; set; }
        public bool IsEdit { get; set; }
        public string CurrencyCollectionStatus { get; set; }
        public string ReturnComment { get; set; }
        public string BounceImage { get; set; }
        public string WarehouseName { get; set; }
        public string AgentName { get; set; }

    }

    public class CheuqueCollectionNewDc
    {
        public int OrderId { get; set; }
        public decimal ChequeAmt { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public class PeopleListDc
    {
        public int PeopleID { get; set; }
        public string DisplayName { get; set; }
    }

    public class ReturnChequeCollectionDc
    {
        public long Id { get; set; }
        public long ChequeCollectionId { get; set; }
        public string HQReceiverName { get; set; }
        public DateTime? HQReceiveDate { get; set; }
        public string CourierName { get; set; }
        public DateTime? CourierDate { get; set; }
        public string PodNo { get; set; }
        public string HubSenderName { get; set; }
        public string HubReceiverName { get; set; }
        public DateTime? HubReceiveDate { get; set; }
        public DateTime? HandOverDate { get; set; }
        public string HandOverAgentName { get; set; }
        public int? HandOverAgentId { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string ChequeNumber { get; set; }
        public decimal ChequeAmt { get; set; }
        public DateTime ChequeDate { get; set; }
        public string SKCode { get; set; }
        public string StatusText
        {
            get
            {
                ChequeReturnStatusEnum chequeStatusEnum = (ChequeReturnStatusEnum)Status;
                if (Status > 0)
                    return chequeStatusEnum.ToString();
                else
                    return "";
            }
        }

        public int Deliveryissueid { get; set; }
        public string ChequeBankName { get; set; }
        public string DepositBankName { get; set; }
        public int OrderId { get; set; }

        public string Type { get; set; }

        public bool IsHQReceive { get; set; }
        public bool IsHQSentCourier { get; set; }
        public bool IsHubReceive { get; set; }
        public bool IsHubHandOverAgent { get; set; }
        public int Warehouseid { get; set; }
        public string WarehouseName { get; set; }
        public int Fine { get; set; }
        public int? ReturnchequeId { get; set; }
        public string Note { get; set; }
    }

    public class ReturnChequeChargeDc
    {
        public long Id { get; set; }
        public long ChequeCollectionId { get; set; }
        public string HQReceiverName { get; set; }
        public DateTime? HQReceiveDate { get; set; }
        public string CourierName { get; set; }
        public DateTime? CourierDate { get; set; }
        public string PodNo { get; set; }
        public string HubSenderName { get; set; }
        public string HubReceiverName { get; set; }
        public DateTime? HubReceiveDate { get; set; }
        public DateTime? HandOverDate { get; set; }
        public string HandOverAgentName { get; set; }
        public int? HandOverAgentId { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string ChequeNumber { get; set; }
        public decimal ChequeAmt { get; set; }
        public DateTime ChequeDate { get; set; }
        public string SKCode { get; set; }
        public string StatusText
        {
            get
            {
                ChequeReturnStatusEnum chequeStatusEnum = (ChequeReturnStatusEnum)Status;
                if (Status > 0)
                    return chequeStatusEnum.ToString();
                else
                    return "";
            }
        }

        public int Deliveryissueid { get; set; }
        public string ChequeBankName { get; set; }
        public string DepositBankName { get; set; }
        public int OrderId { get; set; }

        public string Type { get; set; }

        public bool IsHQReceive { get; set; }
        public bool IsHQSentCourier { get; set; }
        public bool IsHubReceive { get; set; }
        public bool IsHubHandOverAgent { get; set; }
        public int Warehouseid { get; set; }
        public string WarehouseName { get; set; }
    }

    public class AgentChequeCollectionDc
    {
        public int BankId { get; set; }
        public string ChequeNumber { get; set; }
        public decimal CollectAmount { get; set; }
        public int Mod { get; set; }
        public DateTime Date { get; set; }
        public long CurrencycollectionId { get; set; }
        public string ChequeimagePath { get; set; }
        public int OrderId { get; set; }
    }

    public class OnlineCollectionDc
    {

        public long? Id { get; set; }
        public long? CurrencyCollectionId { get; set; }
        public decimal MPOSAmt { get; set; }
        public decimal PaymentGetwayAmt { get; set; }
        public string MPOSReferenceNo { get; set; }
        public string PaymentReferenceNO { get; set; }
        public int Orderid { get; set; }
        public int? Deliveryissueid { get; set; }
        public DateTime CreatedDate { get; set; }
        public string PaymentFrom { get; set; }
        public int? AgentId { get; set; }
        public int? WarehouseId { get; set; }
        public int? AgentSettelmentId { get; set; }
        public decimal UPIAmt { get; set; }
    }
    public class AgentOnlineCollectionDc
    {

        public string PaymentType { get; set; }
        public decimal PaymentGetwayAmt { get; set; }
        public string MPOSReferenceNo { get; set; }
        public string PaymentReferenceNO { get; set; }
        public DateTime? Date { get; set; }
        public int Mod { get; set; }
        public decimal CollectAmount { get; set; }
        public long CurrencycollectionId { get; set; }
        public int Orderid { get; set; }
    }

    public class OnlinePaymentDc
    {
        public long? Id { get; set; }
        public long? CurrencyCollectionId { get; set; }
        public decimal Amount { get; set; }
        public string ReferenceNo { get; set; }
        public string Type { get; set; }
        public int Orderid { get; set; }
        public int? Deliveryissueid { get; set; }
        public string SkCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public string PaymentFrom { get; set; }
        public bool IsSettled { get; set; }

    }

    public class cashpaymentDc
    {
        public int Id { get; set; }
        public int CurrencyCount { get; set; }
        public string Title { get; set; }
        public int Value { get; set; }
        public string currencyType { get; set; }
        public string currencyImage { get; set; }



    }
    public class CashdataDc
    {
        public List<cashpaymentDc> cashList { get; set; }
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public int WarehouseId { get; set; }
    }

    public class ChequeCollectionSearchDc
    {
        public long Id { get; set; }
        public long? CurrencyCollectionId { get; set; }
        public string ChequeNumber { get; set; }
        public decimal ChequeAmt { get; set; }
        public int? DBoyPeopleId { get; set; }
        public int? WarehousePeopleId { get; set; }
        public DateTime ChequeDate { get; set; }
        public DateTime? BankSubmitDate { get; set; }
        public int? ChequeStatus { get; set; }
        public string ChequeimagePath { get; set; }
        public bool IsChequeClear { get; set; }
        public string SKCode { get; set; }
        public long? CurrencyHubStockId { get; set; }
        public int? AgentId { get; set; }
        public int? WarehouseId { get; set; }
        public int? AgentSettelmentId { get; set; }
        public bool Ischecked { get; set; }
        public int? OrderId { get; set; }

        public string DBoyName { get; set; }
        public int? Deliveryissueid { get; set; }
        public string ChequeBankName { get; set; }
        public string DepositBankName { get; set; }
        public long CurrencySettlementid { get; set; }
        public bool IsEdit { get; set; }
        public string CurrencyCollectionStatus { get; set; }
        public string ReturnComment { get; set; }
        public string BounceImage { get; set; }
        public string WarehouseName { get; set; }
        public string AgentName { get; set; }
        public string Status { get; set; }
        public string ChequeStatusText
        {
            get
            {
                ChequeStatusEnum chequeStatusEnum = (ChequeStatusEnum)ChequeStatus;
                if (chequeStatusEnum != null)
                    return chequeStatusEnum.ToString();
                else
                    return "";
            }
        }
    }

    public class OnlineExportDc
    {

        public long Id { get; set; }
        public long? CurrencyCollectionId { get; set; }
        public decimal MPOSAmt { get; set; }
        public decimal PaymentGetwayAmt { get; set; }
        public string MPOSReferenceNo { get; set; }

        public string PaymentReferenceNO { get; set; }

        public string PaymentFrom { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public int Orderid { get; set; }
        public int? WarehouseId { get; set; }
        public int? AgentId { get; set; }
        public long? CurrencyHubStockId { get; set; }

    }

    public class OrderListByAssignIdDC
    {
        public int CustomerId { get; set; }

        public int OrderId { get; set; }
        public string Skcode { get; set; }
        public string Status { get; set; }
        public string invoice_no { get; set; }
        public double TotalAmount { get; set; }
        public double GrossAmount { get; set; }
        public double DiscountAmount { get; set; }
        public double TaxAmount { get; set; }
        public string WarehouseName { get; set; }
        public string paymentMode { get; set; }
        public int DeliveryBoyid { get; set; }
        public string DeliveryBoyName { get; set; }
        public string RequestBy { get; set; }
        public string RequestDate { get; set; }
        public double CaseMemoAmt { get; set; }
    }



    public class CMSPageAccessResultDc
    {
        public string Message { get; set; }
        public bool Status { get; set; }
        public bool ButtonShowStart { get; set; }
        public bool ButtonShowStop { get; set; }
    }


    public class CMSCashierVerificationDC
    {
        public string Message { get; set; }
        public bool Status { get; set; }
        public bool ButtonShowRequestPeople { get; set; }
        public bool ButtonShowAcceptPeople { get; set; }
        public bool ButtonShowVerifiePeople { get; set; }
    }

    public class ExchangeStockDc
    {
        public long Id { get; set; }
        public int WarehouseId { get; set; }
    }

    ////public class CMSPageAccessDC
    ////{
    ////    public int PageName { get; set; }
    ////    public int UserID { get; set; }
    ////    public DateTime AccessStartTime { get; set; }
    ////    public DateTime? AccessEndTime { get; set; }
    ////}



    public class TotalOnlineCollectionDc
    {
        public decimal TotalCollection { get; set; }
        public decimal PaymentGetwayAmt { get; set; }
        public string PaymentFrom { get; set; }


    }
    public class StoreCreditLimitDC
    {
        public string StoreName { get; set; }
        public string SkCode { get; set; }
        public double CreditLimit { get; set; }
        public int DueDays { get; set; }
    }
    public class DownloadStoreCreditLimitDC
    {
        public string StoreName { get; set; }
        public string SkCode { get; set; }
        public double CreditLimit { get; set; }
        public int DueDays { get; set; }
    }
    public class GetStoreCreditLimitDC
    {
        public int CustomerId { get; set; }
        public long StoreId { get; set; }
        public double CreditLimit { get; set; }
    }
    public class GetCreditStoreListDC
    {
        public long StoreId { get; set; }
        public string Name { get; set; }
    }
    public class GetCreditCustomerListDC
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
    }

    public class GetCreditSearchListDc
    {
        public long Id { get; set; }
        public int CustomerId { get; set; }
        public long StoreId { get; set; }
        public double AvailableRemainingAmount { get; set; }
        public double UsedCreaditLimit { get; set; }
        public double CreditLimit { get; set; }
        public string StoreName { get; set; }
        public string SkCode { get; set; }
        public string Mobile { get; set; }
        public string CustomerName { get; set; }
        public bool IsActive { get; set; }
        public int TotalCount { get; set; }
    }
    public class GetCreditSearchPayload
    {
        public List<long> StoreId { get; set; }
        public List<long> WarehouseId { get; set; }
        public string SkCode { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public int Status { get; set; }
        public bool IsPaymentpending { get; set; }

    }
    public class PayLaterCollectionShowDc
    {
        public long Id { get; set; }
        public DateTime CreatedDate { get;set; }
        public string StoreName { get;set; }
        public int OrderId { get;set; }
        public string InvoiceNo { get;set; }
        public double Amount { get;set; }
        public double RefundAmount { get;set; }
        public string PaymentStatus { get;set; }
        public double PaidAmount { get;set; }
        public double RemainingAmount { get;set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Skcode { get; set; }
        public string CustomerName { get; set; }
        public string ShopName { get; set; }
        public string Mobile { get; set; }
        public string Status { get; set; }
        public bool IsAddPayment { get; set; }
        public int WarehouseId { get; set; }
        public int TotalCount { get;set; }
        public string OrderTakenSalesPerson { get; set; }
        public string WarehouseName { get; set; }
        public string ReceivedDate { get; set; }
        public string ReceiverName { get; set; }
        public double? ReturnAmount { get; set; }
        public List<PayLaterCollectionHistoryDC> payLaterCollectionHistoryDCs { get; set; }
        public List<PayLaterCollectionReturnOrderDC> payLaterCollectionReturnOrderDCs { get; set; }
    }
    public class PayLaterCollectionHistoryDC
    {
        public long Id { get; set; }
        public long PayLaterCollectionId { get; set; }
        public double Amount { get; set; }
        public string PaymentMode { get; set; }
        public string Comment { get; set; }
        public string RefNo { get; set; }
        public DateTime CreatedDate { get; set; }
        public string PaymentStatus { get; set; }
    }
    public class AddCustomerLimitDC
    {
        public int StoreId { get; set; }
        public int CustomerId { get; set; }
        public double CreditLimit { get; set; }
        public int DueDays { get; set; }
    }
    public enum PayCollectionEnum
    {
        Pending = 0,
        Partial = 1,
        Due = 2,
        OverDue = 3,
        Paid = 4
    }
    public class PayLaterCollectionLimitDC
    {
        public string StoreName { get; set; }
        public double CreditLimit { get; set; }
        public double AvailableRemainingAmount { get; set; }
    }
    public class PayLaterCollectionResponse
    {
        public string Message { get; set; }
        public bool Status { get; set; }
        public List<PayLaterCollectionLimitDC> payLaterCollectionLimitDCs { get; set; }
    }
    public class WarehouseListShowDCPayLater
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
    }
    public class PayLaterUpdateDc
    {
        public long Id { get; set; }
        public int PaymentStatus { get; set; }
    }
    public class PayLaterCollectionReturnOrderDC
    {
        public int ReturnorderId { get; set; }
        public string OrderTakenSalesPerson { get; set; }
        public string Skcode { get; set; }
        public string Mobile { get; set; }
        public string OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ? DeliveredDate { get; set; }
        public int OrderId { get; set; }
        public double GrossAmount { get; set; }
        public string Paymentstatus { get; set; }
        
    }
    public class PayLaterMopWiseData
    {
        public int OrderId { get; set; }
        public string PaymentMode { get; set; }
        public double Amount { get; set; }
        public string RefNo { get; set; }
        public DateTime PaymentDate { get; set; }
        public string GatewayTransId { get; set; }
        public string PaymentStatus { get; set; }
        public string InvoiceNumber { get; set; }
        public string StoreName { get; set; }
        public string OrderStatus { get; set; }
        public string WarehouseName { get; set; }
        public int TotalRecords { get; set; }
    }

    public class GetMopwisePayload
    {
        public DateTime startdate { get; set; }
        public DateTime enddate { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public bool IsExport { get; set; }

    }
}



