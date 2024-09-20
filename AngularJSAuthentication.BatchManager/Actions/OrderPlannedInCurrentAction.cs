﻿using AngularJSAuthentication.BusinessLayer.Managers.Transactions.BatchCode;
using AngularJSAuthentication.DataContracts.BatchCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.BatchManager.Actions
{

    public class OrderPlannedInCurrentAction : BatchCodeAction
    {
        protected override async Task<bool> Run(BatchCodeSubject subject)
        {
            StockBatchTransactionManager stockBatchTransactionManager
               = new StockBatchTransactionManager();
            bool result = await stockBatchTransactionManager.OnOrderIn(subject);
            return result;
        }
    }
}
