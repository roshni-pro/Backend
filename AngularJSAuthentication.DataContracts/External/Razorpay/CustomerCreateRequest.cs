namespace AngularJSAuthentication.DataContracts.External.Razorpay
{
    public class CustomerCreateRequest
    {
        public string name { get; set; }
        public string contact { get; set; }
        //public string email { get; set; }
        public string fail_existing { get; set; }
        //public string gstin { get; set; }
        public CreateCustNotes notes { get; set; }

    }

    public class CreateCustNotes
    {
        public string notes_key_1 { get; set; }
        public string notes_key_2 { get; set; }

    }
}
