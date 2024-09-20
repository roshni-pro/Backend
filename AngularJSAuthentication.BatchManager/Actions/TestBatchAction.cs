using AngularJSAuthentication.DataContracts.BatchCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.BatchManager.Actions
{
    public class TestBatchAction : BatchCodeAction
    {
        protected override async Task<bool> Run(BatchCodeSubject subject)
        {
            //throw new Exception("Test Exception occurs");
            FileWriter fileWriter = new FileWriter();
            fileWriter.WriteToFile("Subscriber called at " + DateTime.Now);
            return true;
        }
    }
}
