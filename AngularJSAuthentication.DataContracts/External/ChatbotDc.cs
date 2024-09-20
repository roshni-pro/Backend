using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External
{
    public class RegistorChatbotUser
    {
        public string MobileNumber { get; set; }
        public string ShopName { get; set; }
        public string Shopimage { get; set; }
        public string Name { get; set; }
        public string ShippingAddress { get; set; }
        public string LatlngUrl { get; set; }
        public string City { get; set; }
    }

    public class ChatbotUserAddress
    {
        public int CustomerId { get; set; }
        public string ShippingAddress { get; set; }
        public string LatlngUrl { get; set; }
    }

    public class UpdateDocChatbotUser
    {
        public int CustomerId { get; set; }
        public string DocType { get; set; }
        public string DocNumber { get; set; }
        public string DocPath { get; set; }
    }
}
