using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.PurchaseRequest
{
    public class PurchaseRequestSettlementContainerDc
    {
        public List<PurchaseRequestSettlementPageDc> PageList { get; set; }
        public int TotalRecords { get; set; }
    }
}
