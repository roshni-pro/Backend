using AngularJSAuthentication.BusinessLayer.Managers.Transactions.BatchCode;
using AngularJSAuthentication.DataContracts.BatchCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.BatchManager.Actions
{
    public class OrderOutCurrentAction : BatchCodeAction
    {

        protected override async Task<bool> Run(BatchCodeSubject subject)
        {
            StockBatchTransactionManager stockBatchTransactionManager
                = new StockBatchTransactionManager();
            bool result = await stockBatchTransactionManager.OnOrderOut(subject);
            return result;
        }
    }
}
