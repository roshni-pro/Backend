using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Shared
{
    public class OfferResponseDC
    {
        public bool status { get; set; }
        public bool ShowValidationSkipmsg { get; set; }
        public string msg { get; set; }
        public GenricEcommers.Models.Offer Offer { get; set; }
    }
}
