using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Shared
{
    public class Msg91MsgBody
    {
        public string sender { get; set; }
        public string route { get; set; }
        public string country { get; set; }
        public List<Sms> sms { get; set; }
    }

    public class Sms
    {
        public string message { get; set; }
        public List<string> to { get; set; }
    }
}
