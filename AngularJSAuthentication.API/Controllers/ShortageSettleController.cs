using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/ShortageSettle")]
    public class ShortageSettleController : ApiController
    {
      
        public static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
        public PaggingData Get(int list, int page, string DBoyNo, DateTime? datefrom, DateTime? dateto)
        {
            List<ShortSetttle> displist = new List<ShortSetttle>();
            logger.Info("start OrderSettle: ");
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                using (var db = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {

                       
                            
                        var lst = db.AllShortagePaging(list, page, DBoyNo, datefrom, dateto, compid, Warehouse_id);
                       
                        return lst;
                    }
                    else
                    {
                        
                        var lst = db.AllShortagePaging(list, page, DBoyNo, datefrom, dateto, compid);
                     
                        return lst;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in OrderSettle " + ex.Message);
                logger.Info("End  OrderSettle: ");
                return null;
            }
        }


    }
}
