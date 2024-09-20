namespace AngularJSAuthentication.DataContracts.External.Razorpay
{
    public class VirtualAccountRequest
    {
        public string customer_id { get; set; }
        public string description { get; set; }
        public VirtualAccountRequestReceivers receivers { get; set; }
        public int amount_expected { get; set; }
        //public Notes notes { get; set; }
        public BharatQrNotes notes { get; set; }
    }

    public class VirtualAccountRequestReceivers
    {
        public string[] types { get; set; }
        //public QrCodeType qr_code { get; set; }
    }

    public class QrCodeType
    {
        public Method method { get; set; }
    }
    public class Method
    {
        public bool card { get; set; }
        public bool upi { get; set; }
    }

    public class Notes
    {
        public string purpose { get; set; }
    }

    public class BharatQrNotes
    {
        public string reference_key { get; set; }
    }
}
