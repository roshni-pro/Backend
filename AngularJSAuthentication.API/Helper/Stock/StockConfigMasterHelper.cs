using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper.Stock
{
    public class StockConfigMasterHelper
    {
        public StockConfigDc GetByStockEnum(StockEnum stockEnum, AuthContext context)
        {
            StockConfigDc config = null;
            string description = Enum.GetName(typeof(StockEnum), stockEnum);

            config = new StockConfigDc();
            config.Master = context.StockConfigMasterDB.FirstOrDefault(x => x.EnumDescription == description);
            config.DetailList = context.StockConfigDetailDB.Where(x => x.StockConfigMasterId == config.Master.Id).ToList();

            return config;
        }
    }
}