using AngularJSAuthentication.BatchManager.Actions;
using AngularJSAuthentication.DataContracts.BatchCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.BatchManager
{
    public class Dispatcher
    {
        public Action<BatchCodeSubject> GetAction(BatchCodeSubject subject)
        {
            GRNCurrentStockAction acion = new GRNCurrentStockAction();
            return new Action<BatchCodeSubject>((s) =>
            {
                acion.FinalRun(s);
            });
        }
    }
}
