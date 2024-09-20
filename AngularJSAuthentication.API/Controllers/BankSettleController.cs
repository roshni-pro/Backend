using NLog;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/BanksettleApi")]
    public class BankSettleController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        //[Authorize]


        //[Route(" ")]
        //[AcceptVerbs("PUT")]
        //public CurrencyBankSettle Put(CurrencyBankSettle obj)
        //{
        //    return null;
        //    //try
        //    //{
        //    //    return db.BankCurrencyPut(obj);
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    logger.Error("Error in Put News " + ex.Message);
        //    //    return null;
        //    //}
        //}



    }
}




