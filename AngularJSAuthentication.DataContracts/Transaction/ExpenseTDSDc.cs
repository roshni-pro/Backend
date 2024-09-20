using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction
{
    public class ExpenseTDSDc
    {
        public long ID { get; set; }
        public int LadgertypeID { get; set; }
        public int? LadgerID { get; set; }
        public string LadgerName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string SectionCode { get; set; }
        public int RateOfTDS { get; set; }
        public string Assessee { get; set; }
    }
}
