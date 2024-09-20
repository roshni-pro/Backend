using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Transaction.Reports
{
    public class GstReportData
    {
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public string ShopName { get; set; }
        public string Tin_No { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime createddate { get; set; }
        public DateTime? PocCreditNoteDate { get; set; }
        public string PocCreditNoteNumber { get; set; }
        public double GrossAmount { get; set; }
        public string Status{ get; set; }
        public double TaxPercentage { get; set; }
        public double CessTaxAmount { get; set; }
        public double AmtWithoutAfterTaxDisc { get; set; }
        public double AmtWithoutTaxDisc { get; set; }
        public double CGSTTaxAmmount { get; set; }
        public double SGSTTaxAmmount { get; set; }
        public string CategoryName { get; set; }
        public string HSNCode { get; set; }
        public int ItemId { get; set; }
        public int qty { get; set; }
        public int OrderStateId { get; set; }
        public int CustomerStateId { get; set; }
    }

    public class GSTREPORT
    {
        public string GSTIN { get; set; }
        public string ReceiverName { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceValue { get; set; }
        public string PlaceOfSupply { get; set; }
        public string ReverseCharge { get; set; }
        public string ApplicableofTaxRate { get; set; }
        public string InvoiceType { get; set; }
        public string ECommerceGSTIN { get; set; }
        public string Rate { get; set; }
        public string TaxableValue { get; set; }
        public string CessTaxAmount { get; set; }
    }

    public class B2CS
    {
        public string Type { get; set; }
        public string PlaceOfSupply { get; set; }
        public string ApplicableofTaxRate { get; set; }
        public string Rate { get; set; }
        public string TaxableValue { get; set; }
        public string CessTaxAmount { get; set; }
        public string ECommerceGSTIN { get; set; }
    }

    public class CreditDetails
    {
        public string GSTIN { get; set; }
        public string ReceiverName { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public string RefundVoucherNo { get; set; }
        public string RefundVoucherDate { get; set; }
        public string DocumentType { get; set; }
        public string InvoiceValue { get; set; }
        public string PlaceOfSupply { get; set; }
        public string ApplicableofTaxRate { get; set; }
        public string Rate { get; set; }
        public string TaxableValue { get; set; }
        public string CessTaxAmount { get; set; }
        public string PreGST { get; set; }
    }

    public class CreditDetailsB2CS
    {
        public string Type { get; set; }
        public string PlaceOfSupply { get; set; }
        public string ApplicableofTaxRate { get; set; }
        public string Rate { get; set; }
        public string TaxableValue { get; set; }
        public string CessTaxAmount { get; set; }
        public string ECommerceGSTIN { get; set; }
    }
    public class documentList
    {
        public string NameOfDocument { get; set; }
        public string SrNoFrom { get; set; }
        public string SrNoTo { get; set; }
        public int TotalNumber { get; set; }
        public int Cancelled { get; set; }
    }
    public class ExemptDetail
    {
        public string Description { get; set; }
        public string NilRatedSupplies { get; set; }
        public string Exempted { get; set; }
        public int NonGSTSupplies { get; set; }
    }

    public class HSNSummary
    {
        public string HSNCode { get; set; }
        public string UQC { get; set; }
        public string TotalQuantity { get; set; }
        public string TotalValue { get; set; }
        public string TaxableValue { get; set; }
        public string IntegratedTaxAmount { get; set; }
        public string CentralTaxAmount { get; set; }
        public string StateTaxAmount { get; set; }
        public string CessAmount { get; set; }
    }

    public class GstAllList
    {
        public List<GSTREPORT> gstList { get; set; }
        public List<B2CS> B2CSList { get; set; }
        public List<CreditDetails> CreditDetailsList { get; set; }
        public List<CreditDetailsB2CS> CreditDetailsListB2Cs { get; set; }
        public List<HSNSummary> HSNList { get; set; }
        public List<documentList> document { get; set; }
    }



    public class SaleGraphDetailDataDC
    {
        public int ActiveSellerToday { get; set; }
        public int ActiveSellerYesterday { get; set; }
        public int ActiveSellerMtd { get; set; }
        public int TotalLineitemMtd { get; set; }
        public int TotalLineitemYesterday { get; set; }
        public int TotalLineitemToday { get; set; }
        public int ActiveBuyerToday { get; set; }
        public int ActiveBuyerMtd { get; set; }
        public int ActiveBuyerYesterday { get; set; }
        public double TotalSaleToday { get; set; }
        public double TotalSaleYesterday { get; set; }
        public double TotalSaleMtd { get; set; }
        public double AvgOrderValueToday { get; set; }
        public double AvgOrderValueYesterday { get; set; }
        public double AvgOrderValueMtd { get; set; }
        public int NoofOrderToday { get; set; }
        public int NoofOrderYesterday { get; set; }
        public int NoofOrderMtd { get; set; }
        public double AvgOrderValuePMonth { get; set; }
        public double OrderValuePMonth { get; set; }
        public int ActiveBuyerPMonth { get; set; }
        public int NoofOrderPMonth { get; set; }
        public double TotalSalePMonth { get; set; }
        public int ActiveSellerPMonth { get; set; }
        public int TotalLineitemPMonth { get; set; }


    }
}
