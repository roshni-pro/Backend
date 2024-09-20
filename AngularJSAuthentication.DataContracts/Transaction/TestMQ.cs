using AngularJSAuthentication.Model.Base.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction
{
    public class TestMQ
    {
        public string PropName { get; set; }
    }



    public class StockTransactionMQ
    {
        public Audit Entity { get; set; }
        public List<long> StockTransactionMasterIdList { get; set; }
    }
}
