using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper.IRN
{
    public class GenIRNExcelDC
    {
        
        public string DocumentDate { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentTypeCode { get; set; }
        public string SupplyTypeCode { get; set; }
        public string RecipientLegalName { get; set; }
        public string RecipientTradeName { get; set; }
        public string RecipientGSTIN { get; set; }
        public string PlaceofSupply { get; set; }
        public string RecipientAddress1 { get; set; }
        public string RecipientPlace { get; set; }
        public string RecipientStateCode { get; set; }
        public string RecipientPINCode { get; set; }
        public string SlNo { get; set; }
        public string ItemDescription { get; set; }
        public string IstheitemaGOODGOrSERVICES { get; set; }
        public string HSNorSACcode { get; set; }
        public double Quantity { get; set; }
        public string UnitofMeasurement { get; set; }
        public double ItemPrice { get; set; }
        public double GrossAmount { get; set; }
        public double ItemTaxableValue { get; set; }
        public double GSTRate { get; set; }
        public double IGSTAmount { get; set; }
        public double CGSTAmount { get; set; }
        public double SGSTUTGSTAmount { get; set; }
        public double CompCessAmountAdValorem { get; set; }
        public double StateCessAmountAdValorem { get; set; }
        public double OtherCharges { get; set; }
        public double ItemTotalAmount { get; set; }
        public double TotalTaxableValue { get; set; }
        public double IGSTAmountTotal { get; set; }
        public double CGSTAmountTotal { get; set; }
        public double SGSTUTGSTAmountTotal { get; set; }
        public double CompCessAmountTotal { get; set; }
        public double StateCessAmountTotal { get; set; }
        public double OtherCharge { get; set; }
        public double RoundOffAmount { get; set; }
        public double TotalInvoiceValueinINR { get; set; }
        public string Isreversechargeapplicable { get; set; }
        public string IsSec7IGSTActapplicable { get; set; }
        public string PrecedingDocumentNumber { get; set; }
        public string PrecedingDocumentDate { get; set; }
        public string SupplierLegalName { get; set; }
        public string GSTINofSupplier { get; set; }
        public string SupplierAddress1 { get; set; }
        public string SupplierPlace { get; set; }
        public string SupplierStateCode { get; set; }
        public string SupplierPINCode { get; set; }
        public string TypeofExport { get; set; }
        public string ShippingPortCode { get; set; }
        public string ShippingBillNumber { get; set; }
        public string ShippingBillDate { get; set; }
        public string PayeeName { get; set; }
        public string PayeeBankAccountNumber { get; set; }
        public string ModeofPayment { get; set; }
        public string BankBranchCode { get; set; }
        public string PaymentTerms { get; set; }
        public string PaymentInstruction { get; set; }
        public string CreditTransferTerms { get; set; }
        public string DirectDebitTerms { get; set; }
        public string CreditDays { get; set; }
        public string ShipToLegalName { get; set; }
        public string ShipToGSTIN { get; set; }
        public string ShipToAddress1 { get; set; }
        public string ShipToPlace { get; set; }
        public string ShipToPincode { get; set; }
        public string ShipToStateCode { get; set; }
        public string DispatchFromName { get; set; }
        public string DispatchFromAddress1 { get; set; }
        public string DispatchFromPlace { get; set; }
        public string DispatchFromStateCode { get; set; }
        public string DispatchFromPincode { get; set; }

        // set as a null
        public string TaxScheme { get; set; }
        public string TransporterID { get; set; }
        public string TransMode { get; set; }
        public string TransDistance { get; set; }
        public string TransporterName { get; set; }
        public string TransDocNo { get; set; }
        public string TransDocDate { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleType { get; set; }
        public string ReceiptAdviceReference { get; set; }
        public string ReceiptAdviceDate { get; set; }
        public string TenderorLotReference { get; set; }
        public string ContractReference { get; set; }
        public string ExternalReference { get; set; }
        public string ProjectReference { get; set; }
        public string POReferenceNumber { get; set; }
        public string POReferenceDate { get; set; }
        public string AdditionalSupportingDocumentsURL { get; set; }
        public string AdditionalSupportingDocumentsbase64 { get; set; }
        public string AdditionalInformation { get; set; }
        //end set as a null

        public string DocumentPeriodStartDate { get; set; }
        public string DocumentPeriodEndDate { get; set; }

        // set as a null
        public string AdditionalCurrencyCode { get; set; }
        public string Barcode { get; set; }
        public string FreeQuantity { get; set; }
        public string PreTaxValue { get; set; }
        public string CompCessRateAdValorem { get; set; }
        public string CompCessAmountNonAdValorem { get; set; }
        public string StateCessRateAdValorem { get; set; }
        public string StateCessAmountNonAdValorem { get; set; }
        public string PurchaseOrderLineReference { get; set; }
        public string OriginCountryCode { get; set; }
        public string UniqueSerialNumber { get; set; }
        public string BatchNumber { get; set; }
        public string BatchExpiryDate { get; set; }
        public string WarrantyDate { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
        public string CountryCodeofExport { get; set; }
        public string RecipientPhone { get; set; }
        public string RecipienteMailID { get; set; }
        public string RecipientAddress2 { get; set; }
        public string TotalInvoiceValueinFCNR { get; set; }
        public string PaidAmount { get; set; }
        public string AmountDue { get; set; }
        public string DiscountAmountInvoiceLevel { get; set; }
        public string TradeNameofSupplier { get; set; }
        public string SupplierAddress2 { get; set; }
        public string SupplierPhone { get; set; }
        public string SuppliereMail { get; set; }
        public string ShipToTradeName { get; set; }
        public string ShipToAddress2 { get; set; }
        public string DispatchFromAddress2 { get; set; }
        public string Remarks { get; set; }
        public string ExportDutyAmount { get; set; }
        public string SupplierCanOptRefund { get; set; }
        public string ECOMGSTIN { get; set; }
        public string OtherReference { get; set; }

        // end set as null
    }



}