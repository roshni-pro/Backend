using AngularJSAuthentication.API.Helper.Stock;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.Stocks
{
    [RoutePrefix("api/StockHistory")]
    public class StockHistoryController : ApiController
    {
        [HttpPost]
        [Route("GetStockList")]
        public StockHistoryListDc GetStockList(StockHistoryPageFilterDc filter)
        {
            StockHistoryHelper stockHistoryHelper = new StockHistoryHelper();
            return stockHistoryHelper.GetStockList(filter);
        }
    }
}
