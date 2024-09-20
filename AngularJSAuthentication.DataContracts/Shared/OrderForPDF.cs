using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Shared
{
    public class OrderDataForPDF
    {
        public OrderForPDF OrderData { get; set; }
        public List<OrderDetailForPDF> OrderDetailForPDFs { get; set; }
        public List<OrderPaymentForPDF> OrderPayments { get; set; }        
        public WarehouseForPDF Warehouse { get; set; }
        public List<InvoiceOrderOfferForPDF> invoiceOrderOffers { get; set; }
        public InActiveCustomerDetailForPDF InActiveCustomerDetailForPDF { get; set; }
    }
    public class OrderForPDF
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string ShopName { get; set; }
        public string Skcode { get; set; }
        public string Status { get; set; }
        public int WarehouseId { get; set; }
        public bool? IsPrimeCustomer { get; set; }
        public bool IsFirstOrder { get; set; }
        public double? BillDiscountAmount { get; set; }
        public string offertype { get; set; }
        public bool IsIgstInvoice { get; set; }
        public string Tin_No { get; set; }
        public string invoice_no { get; set; }
        public int? DeliveryIssuanceIdOrderDeliveryMaster { get; set; }
        public string paymentThrough { get; set; }
        public string paymentMode { get; set; }
        public byte[] InvoiceBarcodeImage { get; set; }
        public string Customerphonenum { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime OrderedDate { get; set; }
        public string EwayBillNumber { get; set; }
        public string PocCreditNoteNumber { get; set; }
        public string IRNNo { get; set; }
        public string IRNQRCodeUrl { get; set; }
        public string POCIRNNo { get; set; }
        public string POCIRNQRCodeURL { get; set; }
        public string DboyName { get; set; }
        public int OrderType { get; set; } = 1;
        public double deliveryCharge { get; set; }
        public double? WalletAmount { get; set; }
        public double TotalAmount { get; set; }
        public double GrossAmount { get; set; }
        public double TCSAmount { get; set; }
        public double DiscountAmount { get; set; }
        public double TaxAmount { get; set; }
        public string CustomerVerify { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public string UploadGSTPicture { get; set; }
        public string UploadLicensePicture { get; set; }
        public string RefNo { get; set; }
        public string InvoiceAmountInWord { get; set; }

        public string shippingStateName { get; set; }
        public string shippingStateCode { get; set; }
        public string BillingStateName { get; set; }
        public string BillingStateCode { get; set; }
        public string SalesPerson { get; set; }
        public string SalesMobile { get; set; }
        public DateTime? PocCreditNoteDate { get; set; }

    }

    public class OrderDetailForPDF
    {
        public string SalesMobile { get; set; }
        public string SalesPerson { get; set; }

        public string itemname { get; set; }
        public string HSNCode { get; set; }
        public string Barcode { get; set; }
        public bool IsFreeItem { get; set; }
        public double price { get; set; }
        public double UnitPrice { get; set; }
        public int MinOrderQty { get; set; }
        public int qty { get; set; }
        public int Noqty { get; set; }
        public double AmtWithoutTaxDisc { get; set; }
        //public double AmtWithoutAfterTaxDisc { get; set; }
        //public double TotalAmountAfterTaxDisc { get; set; }
        public double CessTaxAmount { get; set; }
        public double TaxAmmount { get; set; }
        public double? PTR { get; set; }
        public double TotalAmt { get; set; }
        public double SGSTTaxAmmount { get; set; }
        public double CGSTTaxAmmount { get; set; }
        public double SGSTTaxPercentage { get; set; }
        public double CGSTTaxPercentage { get; set; }
    }
    public class OrderPaymentForPDF
    {

        public int OrderId { get; set; }
        public string GatewayOrderId { get; set; }
        public string GatewayTransId { get; set; }
        public double amount { get; set; }
        public string status { get; set; }
        public string PaymentFrom { get; set; }
        public string GatewayRequest { get; set; }
        public string PaymentThrough { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ChequeStatus { get; set; }
        public string statusDesc { get; set; }

        public bool IsRefund { get; set; }
        public int Fine { get; set; }
        public bool IsOnline { get; set; }

    }
    public class WarehouseForPDF
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }       
        public string GSTin { get; set; }
        public string aliasName { get; set; }
        public int Stateid { get; set; }
        public string StateName { get; set; }
        public int Cityid { get; set; }
        public string CityName { get; set; }
        public string Address { get; set; }
        public string CompanyName { get; set; }
        public string FSSAILicenseNumber { get; set; }
        public string Phone { get; set; }
    }

    public class InvoiceOrderOfferForPDF
    {
        public int OrderId { get; set; }
        public string OfferCode { get; set; }
        public string ApplyOn { get; set; }
        public double BillDiscountTypeValue { get; set; }
        public double BillDiscountAmount { get; set; }
    }

    public class getSuminvoiceHSNCodeDataDCForPDF
    {
        public string HSNCode { get; set; }
        public double AmtWithoutTaxDisc { get; set; }
        public double SGSTTaxAmmount { get; set; }
        public double CGSTTaxAmmount { get; set; }
        public double TaxAmmount { get; set; }
        public double CessTaxAmount { get; set; }
        public double TotalSum { get; set; }
    }

    public class InActiveCustomerDetailForPDF
    {
        public int OrderCount { get; set; }
        public int MaxOrderLimit { get; set; }
    }
}
