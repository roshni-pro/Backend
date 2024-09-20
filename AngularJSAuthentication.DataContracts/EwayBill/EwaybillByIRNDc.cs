using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.ClearTax;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.EwayBill
{
    public class EwaybillByIRNDc
    {
        public class ewaybillirnParam
        {
            public string Irn { get; set; }
            public int Distance { get; set; }
            public string TransMode { get; set; }// (Road-1, Rail-2, Air-3, Ship-4)
            public string TransId { get; set; }
            public string TransName { get; set; }
            public string TransDocDt { get; set; }
            public string TransDocNo { get; set; }
            public string VehNo { get; set; }
            public string VehType { get; set; }//O-ODC or R-Regular
            public ExpShipDtls ExpShipDtls { get; set; }
            public DispDtls DispDtls { get; set; }
        }

        public class DispDtls
        {
            public string Nm { get; set; }
            public string Addr1 { get; set; }
            public string Addr2 { get; set; }
            public string Loc { get; set; }
            public int Pin { get; set; }
            public string Stcd { get; set; }
        }

        public class ExpShipDtls
        {
            public string Addr1 { get; set; }
            public string Addr2 { get; set; }
            public string Loc { get; set; }
            public int Pin { get; set; }
            public string Stcd { get; set; }
        }

        public class GovtResponse
        {
            public string Success { get; set; }
            public long EwbNo { get; set; }
            public string EwbDt { get; set; }
            public string EwbValidTill { get; set; }
            public List<Error> ErrorDetails { get; set; }
        }
        //public class CancelEwayResponse
        //{
        //    public string ownerId { get; set; }
        //    public string gstin { get; set; }
        //    public string irn { get; set; }
        //    public long ewbNumber { get; set; }
        //    public string ewbStatus { get; set; }
        //}
        public class EwayBillBackendDc
        {
            public long TripPlannerConfirmedMasterid { get; set; }
            public string TransportGST { get; set; }
            public string TransportName { get; set; }
            public string vehicleno { get; set; }
            public long VehicleId { get; set; }
            public int distance { get; set; }
            public bool? IsReplacementVehicleNo { get; set; }
            public string ReplacementVehicleNo { get; set; }
        }

        public class EwayBillBackendDcNonIRN
        {
            public int Orderid { get; set; }
            public string TransportGST { get; set; }
            public string TransportName { get; set; }
            public string vehicleno { get; set; }
            public string TransDocNo { get; set; }
            public string TransDocDt { get; set; }
            public int distance { get; set; }

        }
        public class EwayResponceDc
        {
            public string TransId { get; set; }
            public string TransName { get; set; }
            public string TransMode { get; set; }
            public int Distance { get; set; }
            public string TransDocNo { get; set; }
            public string TransDocDt { get; set; }
            public string VehNo { get; set; }
            public string VehType { get; set; }
            public string Irn { get; set; }
            public GovtResponse govt_response { get; set; }
            public string ewb_status { get; set; }
        }
        public class EwayResponceDcAll
        {
            public int OrderId { get; set; }
            public ClearTaxReqResp ClearTaxRequest { get; set; }
            public ClearTaxReqResp ClearTaxResponce { get; set; }
            public EwayResponceDc ewayResponceDc { get; set; }
            public ClearTaxIntegration clearTaxIntegration { get; set; }

        }
        public class ResponseEwayBillbyIRN
        {
            public string Success { get; set; }
            public long EwbNo { get; set; }
            public string EwbDt { get; set; }
            public string EwbValidTill { get; set; }
            public string Remarks { get; set; }
        }
        public class ErrorResponseEwayBillByIRN
        {
            public string Irn { get; set; }
            public string TransId { get; set; }
            public string TransName { get; set; }
            public string TransMode { get; set; }
            public int Distance { get; set; }
            public string TransDocNo { get; set; }
            public string TransDocDt { get; set; }
            public string VehNo { get; set; }
            public string VehType { get; set; }
            public string EwbStatus { get; set; }
            public Error errors { get; set; }
        }
        public class Error
        {
            public string error_code { get; set; }
            public string error_message { get; set; }
            public string error_source { get; set; }
        }

        public class UpdatePartBRequest //B2B and B2C
        {
            public int orderid { get; set; }
            public long EwbNumber { get; set; }
            public string FromPlace { get; set; }
            public int FromState { get; set; }
            public string ReasonCode { get; set; }
            public string ReasonRemark { get; set; }
            public string TransDocNo { get; set; }
            public string TransDocDt { get; set; }
            public string TransMode { get; set; }
            public string DocumentNumber { get; set; }
            //public string TransporterGstin { get; set; }
            public string DocumentType { get; set; }
            public string DocumentDate { get; set; }
            public string VehicleType { get; set; }
            public string VehNo { get; set; }
            public int Customertype { get; set; }
            public int APITypes { get; set; }
        }

        public class UpdatePartBResponse
        {
            public long EwbNumber { get; set; }
            public string UpdatedDate { get; set; }
            public string ValidUpto { get; set; }
            public List<Errorlist> errors { get; set; }
        }

        public class UpdatePartA
        {
            public long EwbNumber { get; set; } 
            public string TransporterId { get; set; }
            public int APITypes { get; set; }

            public int Orderid { get; set; }
            public int Customertype { get; set; }
        }
        public class Errorlist
        {
            public string error_code { get; set; }
            public string error_message { get; set; }
            public string error_source { get; set; }
        }
        public class EwaybillBackendResponceDc
        {
            public string Message { get; set; }
            public bool status { get; set; }
            public string EwayBillPDF { get; set; }
        }
        public class OrderB2CNonIRNResponse
        {
            public string owner_id { get; set; }
            public string ewb_status { get; set; }
            public EwbRequest ewb_request { get; set; }
            public GovtResponse govt_response { get; set; }
            public string transaction_id { get; set; }
        }
        public class EwbRequest
        {
            public string TransId { get; set; }
            public string TransName { get; set; }
            public string TransMode { get; set; }
            public int Distance { get; set; }
            public string TransDocNo { get; set; }
            public string TransDocDt { get; set; }
            public string VehNo { get; set; }
            public string VehType { get; set; }
            public object DispDtls { get; set; }
            public object ExpShipDtls { get; set; }
            public BuyerDtls BuyerDtls { get; set; }
            public SellerDtls SellerDtls { get; set; }
            public string DocumentNumber { get; set; }
            public string DocumentType { get; set; }
            public string DocumentDate { get; set; }
            public string SupplyType { get; set; }
            public string SubSupplyType { get; set; }
            public string SubSupplyTypeDesc { get; set; }
            public string TransactionType { get; set; }
            public List<ItemList> ItemList { get; set; }
            public double TotalInvoiceAmount { get; set; }
            public object IsSupplyToOrSezUnit { get; set; }
            public double TotalCgstAmount { get; set; }
            public double TotalSgstAmount { get; set; }
            public double TotalIgstAmount { get; set; }
            public double TotalCessAmount { get; set; }
            public double TotalCessNonAdvolAmount { get; set; }
            public double TotalAssessableAmount { get; set; }
            public double OtherAmount { get; set; }
            public double OtherTcsAmount { get; set; }
        }
        public class BuyerDtls
        {
            public string Gstin { get; set; }
            public string LglNm { get; set; }
            public string TrdNm { get; set; }
            public string Addr1 { get; set; }
            public string Addr2 { get; set; }
            public string Loc { get; set; }
            public int Pin { get; set; }
            public string Stcd { get; set; }
        }
        public class SellerDtls
        {
            public string Gstin { get; set; }
            public string LglNm { get; set; }
            public string TrdNm { get; set; }
            public string Addr1 { get; set; }
            public string Addr2 { get; set; }
            public string Loc { get; set; }
            public int Pin { get; set; }
            public string Stcd { get; set; }
        }

        #region

        //NonIRN DC's
        public class OrderB2CNonIRNRequestDc
        {
            public string DocumentNumber { get; set; }
            public string DocumentType { get; set; }
            public string DocumentDate { get; set; }
            public string SupplyType { get; set; }
            public string SubSupplyType { get; set; }
            public string SubSupplyTypeDesc { get; set; }
            public string TransactionType { get; set; }
            public BuyerDtls BuyerDtls { get; set; }
            public SellerDtls SellerDtls { get; set; }
            // public ExpShipDtls ExpShipDtls { get; set; }
            // public DispDtls DispDtls { get; set; }
            public List<ItemList> ItemList { get; set; }
            public double TotalInvoiceAmount { get; set; }
            public double TotalCgstAmount { get; set; }
            public double TotalSgstAmount { get; set; }
            public double TotalIgstAmount { get; set; }
            public double TotalCessAmount { get; set; }
            public double TotalCessNonAdvolAmount { get; set; }
            public double TotalAssessableAmount { get; set; }
            public double OtherAmount { get; set; }
            public double OtherTcsAmount { get; set; }
            public string TransId { get; set; }
            public string TransName { get; set; }
            public string TransMode { get; set; }
            public int Distance { get; set; }
            public string TransDocNo { get; set; }
            public string TransDocDt { get; set; }
            public string VehNo { get; set; }
            public string VehType { get; set; }
        }
        public class ItemList
        {
            public string ProdName { get; set; }
            public string ProdDesc { get; set; }
            public string HsnCd { get; set; }
            public double Qty { get; set; }
            public string Unit { get; set; }
            public double AssAmt { get; set; }
            public double CgstRt { get; set; }
            public double CgstAmt { get; set; }
            public double SgstRt { get; set; }
            public double SgstAmt { get; set; }
            public double IgstRt { get; set; }
            public double IgstAmt { get; set; }
            public double CesRt { get; set; }
            public double CesAmt { get; set; }
            public double OthChrg { get; set; }
            public double CesNonAdvAmt { get; set; }
        }


        #endregion
        public class UpdatePartbB2C
        {

            public long ewbNo { get; set; }
            public string vehicleNo { get; set; }
            public string fromPlace { get; set; }
            public int fromState { get; set; }
            public int reasonCode { get; set; }
            public string reasonRem { get; set; }
            public string transDocNo { get; set; }
            public string transDocDate { get; set; }
            public int transMode { get; set; }
            public string vehicleType { get; set; }
        }
        public class CancelRequestParam //B2B and B2C
        {
            public int orderid { get; set; }
            public long ewbNo { get; set; }
            public int cancelRsnCode { get; set; }
            public string cancelRmrk { get; set; }
            public int Customertype { get; set; }
            public int APITypes { get; set; }
        }
        public class ExtendRequestParam  //B2B 
        {
            public int orderid { get; set; }
            public long EwbNumber { get; set; }
            public string FromPlace { get; set; }
            public string FromState { get; set; }
            public string FromPincode { get; set; }
            public string ReasonCode { get; set; }
            public string ReasonRemark { get; set; }
            public string TransDocNo { get; set; }
            public string TransDocDt { get; set; }
            public string TransMode { get; set; }
            public string DocumentNumber { get; set; }
            public string DocumentType { get; set; }
            public string RemainingDistance { get; set; }
            public string ConsignmentStatus { get; set; }
            public string DocumentDate { get; set; }
            public string VehicleType { get; set; }
            public string VehNo { get; set; }
        }
        public class ExtendRequestParamB2C //B2C
        {
            public int orderid { get; set; }
            public long ewbNo { get; set; }
            public string vehicleNo { get; set; }
            public string fromPlace { get; set; }
            public string fromState { get; set; }
            public int remainingDistance { get; set; }
            public string transDocNo { get; set; }
            public string transDocDate { get; set; }
            public string transMode { get; set; }
            public string extnRsnCode { get; set; }
            public string extnRemarks { get; set; }
            public int fromPincode { get; set; }
            public string consignmentStatus { get; set; }
            public string transitType { get; set; }
            public string addressLine1 { get; set; }
            public string addressLine2 { get; set; }
            public string addressLine3 { get; set; }
            public int Customertype { get; set; }
            public int APITypes { get; set; }
        }
        public class ExtendResponseDc
        {
            public long EwbNumber { get; set; }
            public string UpdatedDate { get; set; }
            public string ValidUpto { get; set; }
            public object errors { get; set; }
        }
        public class ErrorDetails
        {
            public string error_code { get; set; }
            public string error_message { get; set; }
            public string error_source { get; set; }
        }

        public class CancelEwayResponse
        {
            public string ownerId { get; set; }
            public string gstin { get; set; }
            public string irn { get; set; }
            public long ewbNumber { get; set; }
            public string ewbStatus { get; set; }
            public ErrorDetails errorDetails { get; set; }
        }
        public class CancelParam
        {
            public long ewbNo { get; set; }
            public string cancelRsnCode { get; set; }
            public string cancelRmrk { get; set; }
        }
        public class EwayBillOrderList
        {
            public int OrderId { get; set; }
            public bool CustomerGstFlag { get; set; }
            public string IRNNo { get; set; }
            public string VehicleNo { get; set; }
            public string GSTin { get; set; }
            public string WarehousePincode { get; set; }
            public string ZipCode { get; set; }
        }


        public class GetCustomerGenerateEwayBillDC//sql
        {
            public int OrderId { get; set; }
            public string toGstin { get; set; }
            public string docNo { get; set; }
            public string docDate { get; set; }

            public string fromGstin { get; set; }
            public string actFromStateCode { get; set; }
            public string actToStateCode { get; set; }
            public double totInvValue { get; set; }
            public string fromTrdName { get; set; }
            public string fromAddr1 { get; set; }
            public string fromAddr2 { get; set; }
            public string fromPlace { get; set; }
            public int fromPincode { get; set; }
            public string fromStateCode { get; set; }
            //public string toGstin { get; set; }
            public string toTrdName { get; set; }
            public string toAddr1 { get; set; }
            public string toAddr2 { get; set; }
            public string toPlace { get; set; }
            public string toPincode { get; set; }
            public string toStateCode { get; set; }
            public int transactionType { get; set; }
            public double otherValue { get; set; }
            public double totalValue { get; set; }
            public double cgstValue { get; set; }
            public double sgstValue { get; set; }
            public double igstValue { get; set; }
            public double cessValue { get; set; }
            public double cessNonAdvolValue { get; set; }
            public string transporterName { get; set; }
            public string transporterId { get; set; }
            public string transDocNo { get; set; }
            public string transMode { get; set; }
            public string transDocDate { get; set; }
            public string transDistance { get; set; }
            public string vehicleType { get; set; }
            public string vehicleNo { get; set; }
            public string productName { get; set; }
            public string hsnCode { get; set; }
            public int quantity { get; set; }
            public string qtyUnit { get; set; }
            public double cgstRate { get; set; }
            public double sgstRate { get; set; }
            public double igstRate { get; set; }
            public double cessRate { get; set; }
            public double cessAdvol { get; set; }
            public double taxableAmount { get; set; }
            public double OtherAmount { get; set; }
        }
        public class generateewaybilldataDC
        {
            public string supplyType { get; set; }
            public string subSupplyType { get; set; }
            public string subSupplyDesc { get; set; }
            public string DocType { get; set; }
            public string docNo { get; set; }
            public string docDate { get; set; }
            public string fromGstin { get; set; }
            public int actFromStateCode { get; set; }
            public int actToStateCode { get; set; }
            public double totInvValue { get; set; }
            public string fromTrdName { get; set; }
            public string fromAddr1 { get; set; }
            public string fromAddr2 { get; set; }
            public string fromPlace { get; set; }
            public int fromPincode { get; set; }
            public int fromStateCode { get; set; }
            public string toGstin { get; set; }
            public string toTrdName { get; set; }
            public string toAddr1 { get; set; }
            public string toAddr2 { get; set; }
            public string toPlace { get; set; }
            public int toPincode { get; set; }
            public int toStateCode { get; set; }
            public int transactionType { get; set; }
            public double otherValue { get; set; }
            public double totalValue { get; set; }
            public double cgstValue { get; set; }
            public double sgstValue { get; set; }
            public double igstValue { get; set; }
            public double cessValue { get; set; }
            public double cessNonAdvolValue { get; set; }
            public string transporterName { get; set; }
            public string transporterId { get; set; }
            public string transDocNo { get; set; }
            public int transMode { get; set; }
            public string transDocDate { get; set; }
            public int transDistance { get; set; }
            public string vehicleType { get; set; }
            public string vehicleNo { get; set; }
            public List<EwayBillDataListnew> ItemList { get; set; }
        }
        public class EwayBillDataListnew
        {
            public string productName { get; set; }
            public string productDesc { get; set; }
            public string hsnCode { get; set; }
            public int quantity { get; set; }
            public string qtyUnit { get; set; }
            public double cgstRate { get; set; }
            public double sgstRate { get; set; }
            public double igstRate { get; set; }
            public double cessRate { get; set; }
            public double cessAdvol { get; set; }
            public double taxableAmount { get; set; }
        }

        public class PostPDFParam
        {
            public int OrderId { get; set; }
            public List<long> ewb_numbers { get; set; }
            public int custometType { get; set; }
            public int Apitypes { get; set; }
        }
        public class GetPDFParam
        {
            public List<long> ewb_numbers { get; set; }
            public string print_type { get; set; }
        }
        public class EwaybillDetails
        {
            public long ewbNo { get; set; }
            public string ewayBillDate { get; set; }
            public string genMode { get; set; }
            public string userGstin { get; set; }
            public string supplyType { get; set; }
            public string subSupplyType { get; set; }
            public string docType { get; set; }
            public string docNo { get; set; }
            public string docDate { get; set; }
            public string fromGstin { get; set; }
            public string fromTrdName { get; set; }
            public string fromAddr1 { get; set; }
            public string fromAddr2 { get; set; }
            public string fromPlace { get; set; }
            public int fromPincode { get; set; }
            public int fromStateCode { get; set; }
            public string toGstin { get; set; }
            public string toTrdName { get; set; }
            public string toAddr1 { get; set; }
            public string toAddr2 { get; set; }
            public string toPlace { get; set; }
            public int toPincode { get; set; }
            public int toStateCode { get; set; }
            public double totalValue { get; set; }
            public double totInvValue { get; set; }
            public double cgstValue { get; set; }
            public double sgstValue { get; set; }
            public double igstValue { get; set; }
            public double cessValue { get; set; }
            public string transporterId { get; set; }
            public string transporterName { get; set; }
            public string status { get; set; }
            public int actualDist { get; set; }
            public int noValidDays { get; set; }
            public string validUpto { get; set; }
            public int extendedTimes { get; set; }
            public string rejectStatus { get; set; }
            public string vehicleType { get; set; }
            public int actFromStateCode { get; set; }
            public int actToStateCode { get; set; }
            public int transactionType { get; set; }
            public double otherValue { get; set; }
            public double cessNonAdvolValue { get; set; }
            public List<Itemlist> itemList { get; set; }
            public List<VehiclListDetail> VehiclListDetails { get; set; }
        }
        public class Itemlist
        {
            public int itemNo { get; set; }
            public int productId { get; set; }
            public string productName { get; set; }
            public string productDesc { get; set; }
            public int hsnCode { get; set; }
            public double quantity { get; set; }
            public string qtyUnit { get; set; }
            public double cgstRate { get; set; }
            public double sgstRate { get; set; }
            public double igstRate { get; set; }
            public double cessRate { get; set; }
            public double cessNonadvol { get; set; }
            public double taxableAmount { get; set; }
        }
        public class VehiclListDetail
        {
            public string updMode { get; set; }
            public string vehicleNo { get; set; }
            public string fromPlace { get; set; }
            public int fromState { get; set; }
            public int tripshtNo { get; set; }
            public string userGSTINTransin { get; set; }
            public string enteredDate { get; set; }
            public string transMode { get; set; }
            public string transDocNo { get; set; }
            public string transDocDate { get; set; }
            public string groupNo { get; set; }
        }
        public class InternalParamDc
        {
            public string TransferOrderID { get; set; }
            public string vehicleNo { get; set; }
            public int TransperterGst { get; set; }
            public int Distance { get; set; }
            public int TransperterDocNo { get; set; }
            public int TransperterDocDate { get; set; }
        }
        public class InternalEwayBillOrderList
        {
            public long? EwayBillId { get; set; }
            public int TotalCount { get; set; }
            public double RequestAmount { get; set; }
            public int TransferOrderId { get; set; }
            public string Status { get; set; }
            public string WarehouseName { get; set; }
            public int WarehouseId { get; set; }
            public int RequestToWarehouseid { get; set; }
            public string RequestToWarehouseName { get; set; }
            public DateTime? CreationDate { get; set; }
            public string EwayBillNumber { get; set; }
            public string IRNno { get; set; }
            public string VehicleNo { get; set; }
            public string DeliveryChallanNo { get; set; }
            public double DispatchAmount { get; set; }
            public bool IsExtendEwayBill { get; set; }
            public bool IsCancelEwayBill { get; set; }
            public DateTime? EwayBillExtendDate { get; set; }
            public DateTime? EwayBillCancelDate { get; set; }
            public DateTime? EwayBillDate { get; set; }
            public DateTime? EwayBillValidTill { get; set; }
        }
        public class InternalEwayBillOrderListAll
        {
            public int TotalCount { get; set; }
            public List<InternalEwayBillOrderList> internalEwayBillOrderLists { get; set; }
        }

    }
}
