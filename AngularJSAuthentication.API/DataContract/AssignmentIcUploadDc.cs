namespace AngularJSAuthentication.API.DataContract
{
    public class AssignmentIcUploadDc
    {
        public bool IsIcVerified { get; set; }
        public bool IsPhysicallyVerify { get; set; }
        public int DeliveryIssuanceId { get; set; }
        public string IcUploadedFile { get; set; }
        public string Comment { get; set; }

    }
}