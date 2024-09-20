using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.EwayBill
{
    public class InternalTransferEwaybillDc
    {
        //public class InternalNonIRNRequestDc
        //{
        //    public InternalNonIRNRequestAll transaction { get; set; }
        //}
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
            public string LglNm { get; set; }
            public string Addr1 { get; set; }
            public string Loc { get; set; }
            public int Pin { get; set; }
            public string Stcd { get; set; }
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

        public class InternalNonIRNRequestDc
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
            public int OtherAmount { get; set; }
            public int OtherTcsAmount { get; set; }
            public string TransId { get; set; }
            public string TransName { get; set; }
            public string TransMode { get; set; }
            public int Distance { get; set; }
            public string TransDocNo { get; set; }
            public string TransDocDt { get; set; }
            public string VehNo { get; set; }
            public string VehType { get; set; }
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



        public class InternalNonIRNResponse
        {
            public string owner_id { get; set; }
            public string ewb_status { get; set; }
           public EwbRequest ewb_request { get; set; }
            public GovtResponse govt_response { get; set; }
            public string transaction_id { get; set; }
        }

       

        //public class InternalEwayResponceDcAll
        //{
        //    public int OrderId { get; set; }
        //    public ClearTaxReqResp ClearTaxRequest { get; set; }
        //    public ClearTaxReqResp ClearTaxResponce { get; set; }
        //    public EwayResponceDc ewayResponceDc { get; set; }
        //    public ClearTaxIntegration clearTaxIntegration { get; set; }

        //}
        public class GovtResponse
        {
            public string Success { get; set; }
            public string Status { get; set; }
            public long EwbNo { get; set; }
            public string EwbDt { get; set; }
            public string EwbValidTill { get; set; }
            public List<ErrorDetail> ErrorDetails { get; set; }
        }
        public class ErrorDetail
        {
            public string error_code { get; set; }
            public string error_message { get; set; }
            public string error_source { get; set; }
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

    }
}
