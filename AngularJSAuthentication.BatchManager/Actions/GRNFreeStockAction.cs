using AngularJSAuthentication.BusinessLayer.Managers.Transactions.BatchCode;
using AngularJSAuthentication.DataContracts.BatchCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.BatchManager.Actions
{
    public class GRNFreeStockAction : BatchCodeAction
    {
        protected override async Task<bool> Run(BatchCodeSubject subject)
        {
            StockBatchTransactionManager manager = new StockBatchTransactionManager();
            bool result = await manager.OnGRN(subject);
            //FileWriter fileWriter = new FileWriter();
            //fileWriter.WriteToFile("Subscriber called at " + DateTime.Now);
            return true;
        }
    }
}
