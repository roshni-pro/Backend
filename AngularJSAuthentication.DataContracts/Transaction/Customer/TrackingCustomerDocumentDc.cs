using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Customer
{
    public class TrackingCustomerDocumentDc
    {
        public int CustomerId { set; get; }
        public string GSTNo { set; get; }
        public string GSTImage { set; get; }
        public string LicenceNo { set; get; }
        public string OtherDocumetImage { set; get; }
        public bool IsGSTCustomer { set; get; }
        public int DocumentStatus { set; get; }
        public DateTime? LicenseExpiryDate { get; set; }
        public DateTime? GstExpiryDate { get; set; }
        public long DocTypeId { set; get; }
        public string DocType { set; get; }

    }
}
