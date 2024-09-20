using AngularJSAuthentication.BusinessLayer.Managers;
using AngularJSAuthentication.DataContracts.BatchCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.BatchManager.Actions.InvoiceActions
{
    public class InvoiceNoGenerateAction : BatchCodeAction
    {
        protected override async Task<bool> Run(BatchCodeSubject subject)
        {
            OrderMasterManager manager = new OrderMasterManager();
            bool result = await manager.InvoiceNoGenerate(subject.ObjectId, subject.ObjectDetailId);
            return result;
        }

    }
}
