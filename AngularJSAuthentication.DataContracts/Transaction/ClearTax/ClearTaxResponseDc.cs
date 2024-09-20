namespace AngularJSAuthentication.DataContracts.Transaction.ClearTax
{
    public class ClearTaxResponseDc
    {
        public string IrnNo { get; set; }
        public string ApiType { get; set; }// GenerateIRN,GenerateIRNWithEWB, GetIRN, GenerateCN, GenerateEWB
        public string QrCode { get; set; }
        public long RequestId { get; set; }
        public long ResponseId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
