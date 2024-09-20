using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.WarehouseUtilization
{
    #region For WarehouseUtilization
    public class WarehouseUtilizationDc
    {

        public DateTime ETADate { get; set; }
        public int DemandOrderCount { get; set; }
        public int FixedThresholdOrderCount { get; set; }
        public int PlannedThresholdOrderCount { get; set; }
        public int ExecutedOrderCount { get; set; }
        public double OrderCountUtilPercentage { get; set; }
        public int DeliveredOrderCount { get; set; }
        public double DeliveredPercentage { get; set; }
        public int CumulativePendingCount { get; set; }
        public int VehicleCountAvailable { get; set; }
        public int TouchPointCapacity { get; set; }
        public int TouchPointUtilization { get; set; }
        public double TouchPointUtilPercentage { get; set; }
        public double PlannedOrderAmount { get; set; }
        public int OrderAmountUtilization { get; set; }
        public double OrderAmountUtilPercentage { get; set; }
        public double OverallUtilPercentage { get; set; }
        public int ExtraVehicleCount { get; set; }
        public int ExtraVehicleCapacityInKg { get; set; }
        public double ThresholdOrderAmount { get; set; }
        public int WarehouseId { get; set; }
        public double CumulativePendingAmount { get; set; }
        public double DeliveredOrderAmount { get; set; }
        public double DemandOrderAmount { get; set; }
    }

    public class UtilResponseDc
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class UpdateVehicleCountDc
    {
        public int WarehouseId { get; set; }
        public DateTime ETADate { get; set; }
        public int ExtraVehicleCount { get; set; }
        public int ExtraVehicleCapacityInKg { get; set; }
        public int UserId { get; set; }
    }

    public class GetUtilizationList
    {
        public string WarehouseName { get; set; }
        public DateTime ETADate { get; set; }
        public int CumulativePendingETACount { get; set; }
        public int CumulativePendingChangeETACount { get; set; }
        public int CumulativePendingCount { get; set; }
        public int DemandOrderCount { get; set; }
        public int VehicleCountRequired { get; set; }
        public int VehicleCountAvailable { get; set; }
        public int PlannedThresholdOrderCount { get; set; }
        public int TouchPointCapacity { get; set; }
        public int TouchPointUtilization { get; set; }
        public int ExecutedOrderCount { get; set; }
        public int DeliveredOrderCount { get; set; }
        public double DemandOrderAmount { get; set; }
        public double PlannedOrderAmount { get; set; }
        public int OrderAmountUtilization { get; set; }
        public double DeliveredOrderAmount { get; set; }
        public double OrderCountUtilPercentage { get; set; }
        public double DeliveredPercentage { get; set; }
        public double TouchPointUtilPercentage { get; set; }
        public double OrderAmountUtilPercentage { get; set; }
        public double OverallUtilPercentage { get; set; }
        public int ExtraVehicleCount { get; set; }
        public int ExtraVehicleCapacityInKg { get; set; }
        public double CumulativePendingOrderAmount { get; set; }
        public int WarehouseId { get; set; }
        public int ThisOrNextDayPendingETACount { get; set; }
        public double DboyCost { get; set; }
        public int RedOrderCount { get; set; }
        public int FixedThresholdOrderCount { get; set; }
        public double ThresholdOrderAmount { get; set; }
        public int MaxVehicleOrderCount { get; set; }

    }
    public class GetUtilizationListReport
    {
        public string WarehouseName { get; set; }
        public DateTime ETADate { get; set; }
        public int PendingETACount { get; set; }
        public int PendingChangeETACount { get; set; }
        public int CumulativePendingCount { get; set; }
        public int TotalDemandOrderCount { get; set; }
        public int VehicleCountRequired { get; set; }
        public int VehicleCountAvailable { get; set; }
        public int PlannedThresholdOrderCount { get; set; }
        public int TouchPointCapacity { get; set; }
        public int TouchPointUtilization { get; set; }
        public int ExecutedOrderCount { get; set; }
        public int DeliveredOrderCount { get; set; }
        public double DemandOrderAmount { get; set; }
        public double PlannedOrderAmount { get; set; }
        public int OrderAmountUtilization { get; set; }
        public double DeliveredOrderAmount { get; set; }
        public double OrderCountUtilPercentage { get; set; }
        public double DeliveredPercentage { get; set; }
        public double TouchPointUtilPercentage { get; set; }
        public double OrderAmountUtilPercentage { get; set; }
        public double OverallUtilPercentage { get; set; }
        public int ExtraVehicleCount { get; set; }
        public int ExtraVehicleCapacityInKg { get; set; }
        public double CumulativePendingOrderAmount { get; set; }
        public int WarehouseId { get; set; }
        public int ThisOrNextDayPendingETACount { get; set; }
        public double DboyCost { get; set; }
        public int RedOrderCount { get; set; }
        public int FixedThresholdOrderCount { get; set; }
        public double ThresholdOrderAmount { get; set; }
        public int MaxVehicleOrderCount { get; set; }

    }
    public class WarehouseUtilVm
    {
        public int WarehouseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
    public class ExportUtilizationList
    {
        //public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        //public DateTime DeliveryDate { get; set; }
        public int OrderedCount { get; set; }
        public double? DeliveredPercentage { get; set; }
        public int CumulativePendingCount { get; set; }
        public string Date { get; set; }
        public int PlannedTouchPoints { get; set; }
        public double PlannedOrderAmount { get; set; }
        public int VisitedOrderAmount { get; set; }
        //public double? OverallUtilPercentage { get; set; }
        public int VehicleCountAvailable { get; set; }
        //public int VehicleCountRequired { get; set; }
        public double? TouchPointUtilizedPercentage { get; set; }


        public int TotalDemand { get; set; }
        //public int ETADemand { get; set; }
        //public int ETAChangedDemand { get; set; }

        public int RequiredVehicleCount { get; set; }

        public int ExecutedOrderCount { get; set; }
        public int DeliveredOrderCount { get; set; }
        public int VisitedTouchPoints { get; set; }
        public double DboyCost { get; set; }
    }
    public class VehicleTypeDc
    {
        public string Type { get; set; }
        public int ThresholdTouchPoint { get; set; }
        public int ThresholdOrderCount { get; set; }
        public double ThresholdOrderAmount { get; set; }
        public int ThresholdLoadInKg { get; set; }
        public int WarehouseId { get; set; }
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }

    }
    public class VehicleTypeList
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public int ThresholdTouchPoint { get; set; }
        public int ThresholdOrderCount { get; set; }
        public double ThresholdOrderAmount { get; set; }
        public int ThresholdLoadInKg { get; set; }
        public int WarehouseId { get; set; }
        public bool IsActive { get; set; }

    }
    public class InsertVehicleTypeDc
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public int ThresholdTouchPoint { get; set; }
        public int ThresholdOrderCount { get; set; }
        public double ThresholdOrderAmount { get; set; }
        public int ThresholdLoadInKg { get; set; }
        public int WarehouseId { get; set; }

    }
    public class VehicleTypeResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class WarehouseUtilReport
    {
        public string WarehouseName { get; set; }
        public int LoadOrderCount { get; set; }
        public double LoadOrderAmount { get; set; }
        public int CapacityOrderCount { get; set; }
        public int VehicleAvailable { get; set; }
    }

    #endregion

    #region For TransporterPayment

    public class GetTranspoterPaymentDetailDc
    {
        public long VehicleMasterId { get; set; }
        public int Attendance { get; set; }
        public string WarehouseName { get; set; }
        public string TransportName { get; set; }
        public string VehicleType { get; set; }
        public string VehicleNo { get; set; }
        public double? MonthlyContractKm { get; set; }
        public double? UtilizedKm { get; set; }
        public double? MonthlyContractAmount { get; set; }
        public double? UtilizedAmount { get; set; }
        public double? PayableAmount { get; set; }
        public int TransporterPaymentDetailId { get; set; }
        public int ExtraKm { get; set; }
        public double ExtraKmAmt { get; set; }
        public double TollAmount { get; set; }
        public double OtherExpense { get; set; }
        public bool IsManuallyEdit { get; set; }
        public string VehicleNumber { get; set; }
        public string Remark { get; set; }
    }
    public class TransporterPayWithDetailDc
    {
        public int TransporterPaymentId { get; set; }
        public int PaymentStatus { get; set; }
        public string GeneratedInvoicePath { get; set; }
        public string TransporterInvoicePath { get; set; }
        public string TaxType { get; set; }
        public int ApprovalStatus { get; set; }
        public string RegionalComment { get; set; }
        public string RegionalLeadComment { get; set; }
        public string HQOpsLeadComment { get; set; }
        public string AccountComment { get; set; }
        public string AccountLeadComment { get; set; }

        public List<GetTranspoterPaymentDetailDc> getTranspoterPaymentDetailList { get; set; }
    }
    public class GetFleetListDc
    {
        public string TransportName { get; set; }
        public string FleetType { get; set; }
        public long fleetId { get; set; }
    }
    public class TransporterPaymentVm
    {
        public int WarehouseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long FleetId { get; set; }
    }
    public class TransporterVehicleAttadanceDc
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long VehicleMasterId { get; set; }
    }
    public class TransporterPayVehicleAttadanceListDc
    {
        public DateTime AttendanceDate { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleType { get; set; }
        public string FleetType { get; set; }
        public double? StartKm { get; set; }
        public double? ClosingKm { get; set; }
        public double? TotalKm { get; set; }
        public double PayableAmount { get; set; }
    }
    public class paymentuploadvm
    {
        public long VehicleMasterId { get; set; }
        public DateTime MontlyDate { get; set; }
    }
    public class PaYmentUploadListDc
    {
        public int TransporterPaymentId { get; set; }
        public double? PayableAmount { get; set; }
        public string InvoicePath { get; set; }
        public string LogBookPath { get; set; }
    }
    public class TransporterPaymentDetailVm
    {
        public int TransporterPaymentDetailId { get; set; }
        public double TollAmount { get; set; }
        public double OtherExpense { get; set; }
        public string Remark { get; set; }
        public double MonthlyContractAmount { get; set; }
        public double UtilizedAmount { get; set; }
    }

    public class TransporterPayDetailDocDC
    {
        public int TransporterPaymentDetailId { get; set; }
        public int DocType { get; set; }
        public string DocPath { get; set; }
    }
    public class TransporterPayDetailDocDc
    {
        public int TransporterPaymentDetailId { get; set; }
        public int DocType { get; set; }
        public string DocPath { get; set; }
    }
    public class TransporterResponseDc
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class FinalizeVm
    {
        public int TransporterPaymentId { get; set; }
        public string TransporterInvoicePath { get; set; }
        public string TaxType { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
    }
    public class GetRegionalListDc
    {
        public int PaymentId { get; set; }
        public string TransportertName { get; set; }
        public string FleetType { get; set; }
        public double UtilizedAmount { get; set; }
        public double ExtraKmAmount { get; set; }
        public double TollAmount { get; set; }
        public double OtherCharges { get; set; }
        public double TotalPayable { get; set; }
        public int ApprovalStatus { get; set; }
        public int PaymentStatus { get; set; }
        public long FleetId { get; set; }
        public string AccountComment { get; set; }
        public string RegionalComment { get; set; }
        public string RegionalLeadComment { get; set; }
        public string HQOpsLeadComment { get; set; }
        public string AccountLeadComment { get; set; }
        public string GeneratedInvoicePath { get; set; }
        public string TransporterInvoicePath { get; set; }
        public double TaxableAmount { get; set; }
        public double RemainingAmount { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public double CGST { get; set; }
        public double SGST { get; set; }
        public double IGST { get; set; }
        public double TDS { get; set; }
        public double Netpayableamount { get; set; }
        public string TaxType { get; set; }
        public string TransactionId { get; set; }
    }
    public class TransporterPaymentDetailDocDc
    {
        public long TransporterPaymentDetailDocId { get; set; }
        public int TransporterPaymentDetailId { get; set; }
        public int DocType { get; set; }
        public string DocPath { get; set; }
    }
    public class ApprovedRejByRegionalVm
    {
        public string Comment { get; set; }
        public long TransporterPaymentId { get; set; }
        public bool IsApprove { get; set; }
    }
    public class paymentWarehouseDc //Working Capital
    {
        public string WarehouseName { get; set; }
        public int WarehouseId { get; set; }
        public int RegionId { get; set; }
        public int Cityid { get; set; }
    }
    public class PaymentInvoiceDc
    {
        public int InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string TransporterAddress { get; set; }
        public string TransporterCity { get; set; }
        public string TranporterState { get; set; }
        public string TransporterPanNumber { get; set; }
        public string TransporterGSTIN { get; set; }
        public string IsRCMTax { get; set; }
        public string VendorName { get; set; }
        public string HubAddress { get; set; }
        public string HubLocation { get; set; }
        public string HubState { get; set; }
        public string HubPanNo { get; set; }
        public string HubGSTIN { get; set; }
        public string TransporterName { get; set; }
        public double Amount { get; set; }
        public double ExtraKmCharges { get; set; }
        public double OtherCharges { get; set; }
        public double CGST { get; set; }
        public double IGST { get; set; }
        public double TDSAmount { get; set; }
        public double SGST { get; set; }
        public double SGSTPer { get; set; }
        public double CGSTPer { get; set; }
        public double IGSTPer { get; set; }
        public double TDSPer { get; set; }
    }
    public class UpdateInvoiceDc
    {
        public int PaymentId { get; set; }
        public string TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }
        public double PaidAmount { get; set; }
    }

    public class TallyFileListDc
    {
        public string InvoiceNumbers { get; set; }
        public string InvoiceDate { get; set; }
        public string BookingDate { get; set; }

        public string ExpenseLedger { get; set; }
        public double TaxableValue { get; set; }
        public double TaxRate { get; set; }
        public double CGST25Per { get; set; }
        public double SGST25per { get; set; }
        public double IGST5per { get; set; }
        public double CGST6per { get; set; }
        public double SGST6per { get; set; }
        public double IGST12per { get; set; }
        public double CGST9per { get; set; }
        public double SGST9per { get; set; }
        public double IGST18per { get; set; }
        public double CGST14per { get; set; }
        public double SGST14per { get; set; }
        public double IGST28per { get; set; }
        public double TDS194C { get; set; }
        public double TDS194JProfessional { get; set; }
        public double TDS194JTechnical { get; set; }
        public double TDS194H { get; set; }
        public double TDS194I { get; set; }
        public double RCMCGST25per { get; set; }
        public double RCMSGST25per { get; set; }
        public double RCMCGST25PerPAYABLE { get; set; }
        public double RCMSGST25perPAYABLE { get; set; }
        public double RCMIGST5per { get; set; }
        public double RCMIGST5perPAYABLE { get; set; }
        public double TCSonPurchase { get; set; }
        public double NetPayableAmount { get; set; }
        public string CostCentre { get; set; }
        public string Department { get; set; }
        public string VendorName { get; set; }
        public string Narretion { get; set; }
        public double Roundoff { get; set; }
        public string RefNo { get; set; }
        public string WarehouseName { get; set; }
        public DateTime TransporterMonthlyDate { get; set; }
        //public string RCMapplicable { get; set; }
        //public string BillingAddress { get; set; }
        //public string CostCenterGroup1 { get; set; }
        //public string FinlyrefNumber { get; set; }
        //public DateTime ApprovalDate { get; set; }
    }
    public class PaymentListDc
    {
        public string Payment { get; set; }
        public string Beneficiaryname { get; set; }
        public string Beneficiaryaccountname { get; set; }
        public string Beneficiaryaccountnumber { get; set; }
        public string Beneficiaryifsc { get; set; }
        public string Paymentmode { get; set; }
        public string Initiatoremail { get; set; }
        public string Invoicenumber { get; set; }
        public DateTime Invoicedate { get; set; }
        public string Invoice { get; set; }
        public double Netpayableamount { get; set; }
        public string Bankreferencenumber { get; set; }
        public string Paymentstatus { get; set; }
        public string HubName { get; set; }
    }

    public class TransporterPaymentHistoryListDc
    {
        public long TransporterPaymentActionHistoryId { get; set; }
        public string Action { get; set; }
        public string Role { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DisplayName { get; set; }
    }
    public class TallyFileExportDc
    {
        public List<int> paymentId { get; set; }
    }

    public class ExportRegionalListDc
    {
        public int PaymentId { get; set; }
        public string TransportertName { get; set; }
        public string FleetType { get; set; }
        public double UtilizedAmount { get; set; }
        public double ExtraKmAmount { get; set; }
        public double TollAmount { get; set; }
        public double OtherCharges { get; set; }
        public double TotalPayable { get; set; }
        public int ApprovalStatus { get; set; }
        public int PaymentStatus { get; set; }
        public double TaxableAmount { get; set; }
        public double RemainingAmount { get; set; }
    }

    public class WarehouseUtilReportDc
    {
        public string WarehouseName { get; set; }
        public int? DEmandExcludingRedOrder { get; set; }
        public int? ExecutedOrderCount { get; set; }
        public int? DeliveredOrderCount { get; set; }
        public int? DeliveryCanceledCount { get; set; }
        public int? DeliveryRedispatchCount { get; set; }
        public int? InTransitOrders { get; set; }
        public int? DemandOrderCount { get; set; }
        public int? NotProcessedOrderCount { get; set; }
        public int? ReattemptCount { get; set; }
    }

    public class TransporterPaymentVehicleList
    {
        public long VehicleMasterId { get; set; }
        public string RegistrationNo { get; set; }
        public string VehicleNo { get; set; }
    }

    public class TransporterPayVehicleInfo
    {
        public double? MonthlyContactKm { get; set; }
        public double? MonthlyContactAmt { get; set; }
        public double? ExtraKmCharge { get; set; }
    }

    public class TransporterPayDetailVehicleInsertInput
    {
        public int Id { get; set; }
        public int TransporterPaymentId { get; set; }
        public long VehicleMasterId { get; set; }
        public double UtilizedKm { get; set; }
        public double UtilizedAmount { get; set; }
        public double TollAmount { get; set; }
        public double OtherExpense { get; set; }
        public int ExtraKm { get; set; }
        public double ExtraKmAmt { get; set; }
    }

    public class TransporterDetailListDc
    {
        public int Id { get; set; }
        public int TransporterPaymentId { get; set; }
        public long VehicleMasterId { get; set; }
        public double UtilizedKm { get; set; }
        public double UtilizedAmount { get; set; }
        public double TollAmount { get; set; }
        public double OtherExpense { get; set; }
        public int ExtraKm { get; set; }
        public double ExtraKmAmt { get; set; }
        public double MonthlyContactKm { get; set; }
        public double MonthlyContactAmt { get; set; }
        public double PayableAmount { get; set; }

    }

    public class LmdVendorList
    {
        public int Id { get; set; }
        public string VendorName { get; set; }
    }

    public class ReginalSummaryInput
    {
        public List<int> WarehouseId { get; set; }
        public DateTime ForMonth { get; set; }
        public string Keyword { get; set; }
    }

    public class GetRegionalListByAllWh
    {
        public string WarehouseName { get; set; }
        public int PaymentId { get; set; }
        public string TransportertName { get; set; }
        public string FleetType { get; set; }
        public double UtilizedAmount { get; set; }
        public double ExtraKmAmount { get; set; }
        public double TollAmount { get; set; }
        public double OtherCharges { get; set; }
        public double TotalPayable { get; set; }
        public int ApprovalStatus { get; set; }
        public int PaymentStatus { get; set; }
        public long FleetId { get; set; }
        public string AccountComment { get; set; }
        public string RegionalComment { get; set; }
        public string RegionalLeadComment { get; set; }
        public string HQOpsLeadComment { get; set; }
        public string AccountLeadComment { get; set; }
        public string GeneratedInvoicePath { get; set; }
        public string TransporterInvoicePath { get; set; }
        public double TaxableAmount { get; set; }
        public double RemainingAmount { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public double CGST { get; set; }
        public double SGST { get; set; }
        public double IGST { get; set; }
        public double TDS { get; set; }
        public double Netpayableamount { get; set; }
        public string TaxType { get; set; }
        public string TransactionId { get; set; }
    }


    public class RegionalAllWHInput
    {
        public List<int> Warehouseids { get; set; }
        public DateTime ForDate { get; set; }
        public string Keyword { get; set; }

    }

    public class TransporterDocumentDc
    {
        public int TransporterId { get; set; }
        public string TransportName { get; set; }
        public string FleetType { get; set; }
        public string GeneratedInvoicePath { get; set; }
        public string TransporterInvoicePath { get; set; }
    }

    #endregion
}

