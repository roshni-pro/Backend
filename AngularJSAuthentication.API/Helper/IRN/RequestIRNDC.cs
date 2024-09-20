using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper.IRN
{
    public class RequestIRNDC
    {
        public Transaction transaction { get; set; }
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class TranDtls
    {
        public string TaxSch { get; set; }
        public string SupTyp { get; set; }
        public string RegRev { get; set; }
        public object EcmGstin { get; set; }
        public string IgstOnIntra { get; set; }
    }

    public class DocDtls
    {
        public string Typ { get; set; }
        public string No { get; set; }
        public string Dt { get; set; }
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
        public string Ph { get; set; }
        public string Em { get; set; }
    }

    public class BuyerDtls
    {
        public string Gstin { get; set; }
        public string LglNm { get; set; }
        public string TrdNm { get; set; }
        public string Pos { get; set; }
        public string Addr1 { get; set; }
        public string Addr2 { get; set; }
        public string Loc { get; set; }
        public int Pin { get; set; }
        public string Stcd { get; set; }
        public string Ph { get; set; }
        public string Em { get; set; }
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

    public class ShipDtls
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

    public class BchDtls
    {
        public string Nm { get; set; }
        public string ExpDt { get; set; }
        public string WrDt { get; set; }
    }

    public class AttribDtl
    {
        public string Nm { get; set; }
        public string Val { get; set; }
    }

    public class ItemList
    {
        public string SlNo { get; set; }
        public string PrdDesc { get; set; }
        public string IsServc { get; set; }
        public string HsnCd { get; set; }
        public string Barcde { get; set; }
        public double Qty { get; set; }
        public double FreeQty { get; set; }
        public string Unit { get; set; }
        public double UnitPrice { get; set; }
        public double TotAmt { get; set; }
        public double Discount { get; set; }
        public double PreTaxVal { get; set; }
        public double AssAmt { get; set; }
        public double GstRt { get; set; }
        public double IgstAmt { get; set; }
        public double CgstAmt { get; set; }
        public double SgstAmt { get; set; }
        public double CesRt { get; set; }
        public double CesAmt { get; set; }
        public double CesNonAdvlAmt { get; set; }
        public double StateCesRt { get; set; }
        public double StateCesAmt { get; set; }
        public double StateCesNonAdvlAmt { get; set; }
        public double OthChrg { get; set; }
        public double TotItemVal { get; set; }
        public string OrdLineRef { get; set; }
        public string OrgCntry { get; set; }
        public string PrdSlNo { get; set; }
        public BchDtls BchDtls { get; set; }
        public List<AttribDtl> AttribDtls { get; set; }
    }

    public class ValDtls
    {
        public double AssVal { get; set; }
        public double CgstVal { get; set; }
        public double SgstVal { get; set; }
        public double IgstVal { get; set; }
        public double CesVal { get; set; }
        public double StCesVal { get; set; }
        public double Discount { get; set; }
        public double OthChrg { get; set; }
        public double RndOffAmt { get; set; }
        public double TotInvVal { get; set; }
        public double TotInvValFc { get; set; }
    }

    public class PayDtls
    {
        public string Nm { get; set; }
        public string AccDet { get; set; }
        public string Mode { get; set; }
        public string FinInsBr { get; set; }
        public string PayTerm { get; set; }
        public string PayInstr { get; set; }
        public string CrTrn { get; set; }
        public string DirDr { get; set; }
        public int CrDay { get; set; }
        public double PaidAmt { get; set; }
        public double PaymtDue { get; set; }
    }

    public class DocPerdDtls
    {
        public string InvStDt { get; set; }
        public string InvEndDt { get; set; }
    }

    public class PrecDocDtl
    {
        public string InvNo { get; set; }
        public string InvDt { get; set; }
        public string OthRefNo { get; set; }
    }

    public class ContrDtl
    {
        public string RecAdvRefr { get; set; }
        public string RecAdvDt { get; set; }
        public string TendRefr { get; set; }
        public string ContrRefr { get; set; }
        public string ExtRefr { get; set; }
        public string ProjRefr { get; set; }
        public string PORefr { get; set; }
        public string PORefDt { get; set; }
    }

    public class RefDtls
    {
        public string InvRm { get; set; }
        public DocPerdDtls DocPerdDtls { get; set; }
        public List<PrecDocDtl> PrecDocDtls { get; set; }
        public List<ContrDtl> ContrDtls { get; set; }
    }

    public class AddlDocDtl
    {
        public string Url { get; set; }
        public string Docs { get; set; }
        public string Info { get; set; }
    }

    public class ExpDtls
    {
        public string ShipBNo { get; set; }
        public string ShipBDt { get; set; }
        public string Port { get; set; }
        public string RefClm { get; set; }
        public string ForCur { get; set; }
        public string CntCode { get; set; }
    }

    public class EwbDtls
    {
        public string TransId { get; set; }
        public string TransName { get; set; }
        public int Distance { get; set; }
        public string TransDocNo { get; set; }
        public string TransDocDt { get; set; }
        public string VehNo { get; set; }
        public string VehType { get; set; }
        public string TransMode { get; set; }
    }

    public class CustomFields
    {
        public string customfieldLable1 { get; set; }
        public string customfieldLable2 { get; set; }
        public string customfieldLable3 { get; set; }
    }

    public class Transaction
    {
        public string Version { get; set; }
        public TranDtls TranDtls { get; set; }
        public DocDtls DocDtls { get; set; }
        public SellerDtls SellerDtls { get; set; }
        public BuyerDtls BuyerDtls { get; set; }
        public DispDtls DispDtls { get; set; }
        public ShipDtls ShipDtls { get; set; }
        public List<ItemList> ItemList { get; set; }
        public ValDtls ValDtls { get; set; }
        public PayDtls PayDtls { get; set; }
        public RefDtls RefDtls { get; set; }
        public List<AddlDocDtl> AddlDocDtls { get; set; }
        public ExpDtls ExpDtls { get; set; }
        public EwbDtls EwbDtls { get; set; }
        public CustomFields custom_fields { get; set; }
    }

    


}