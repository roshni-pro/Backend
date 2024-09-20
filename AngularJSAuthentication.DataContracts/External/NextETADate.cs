using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External
{
    public class NextETADate
    {
        public DateTime NextDeliveryDate { get; set; }
        public bool IsDefaultDeliveryChange { get; set; }
    }
}
