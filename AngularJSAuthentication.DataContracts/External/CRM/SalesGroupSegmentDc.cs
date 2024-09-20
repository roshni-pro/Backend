using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External.CRM
{
    public class SalesGroupSegmentDc
    {
        public long SegmentDetailId { get; set; }
        public String SegmentName { get; set; }
        public List<string>SkCodeList{ get; set; }

    }
}
