namespace AngularJSAuthentication.DataContracts.External.Razorpay
{
    public class WebHookDbResponse
    {
        public bool IsCaptured { get; set; }
        public string DeliveryBoyFcmId { get; set; }
        public int OrderId { get; set; }
        public int AmountCaptured { get; set; }
    }
}
