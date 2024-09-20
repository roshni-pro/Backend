using AngularJSAuthentication.DataContracts.Transaction.Accounts;
using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.API.Models
{
    public class AgentCommissionPager
    {
        public List<AgentCommissionPaymentDc> AgentList { get; set; }
        public int NetRecords { get; set; }
    }
}