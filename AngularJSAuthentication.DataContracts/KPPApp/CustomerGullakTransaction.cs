using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.KPPApp
{
    public class CustomerGullakTransaction
    {
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CustomerId { get; set; }       
        public double Amount { get; set; }
        public string ObjectType { get; set; }
        public string ObjectId { get; set; }
        public string Comment { get; set; }
    }
}
