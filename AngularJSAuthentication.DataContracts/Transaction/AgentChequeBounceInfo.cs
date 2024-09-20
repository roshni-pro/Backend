using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction
{
    public class AgentChequeBounceInfo
    {
        public string CustomerName { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string ChequeNumber { get; set; }
        public decimal ChequeAmt { get; set; }
        public DateTime ChequeDate { get; set; }
        public string ChequeBankName { get; set; }
        public int Orderid { get; set; }

    }

    public class AgentChequeBouncePaginator {
        public int AgentId { get; set; }
        public int WarehouseId { get; set; }
        public int  Skip { get; set; }
        public int Take { get; set; }
    }

    public class AgentChequeBounceInfoList 
    {
        public List<AgentChequeBounceInfo> agentChequeBounceInfo { get; set; }
        public int Count { get; set; }
        public double TotalAmount{ get; set; }
    }
}
