using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    //AllItemPramotion

    [RoutePrefix("api/billpramotion")]
    public class BillPramotionController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("")]
        public IEnumerable<BillPramotion> Get()
        {
            //using (var context = new AuthContext())
            //{

            //    List<BillPramotion> ass = new List<BillPramotion>();
            //    try
            //    {
            //        var identity = User.Identity as ClaimsIdentity;
            //        int compid = 0, userid = 0;

            //        foreach (Claim claim in identity.Claims)
            //        {
            //            if (claim.Type == "compid")
            //            {
            //                compid = int.Parse(claim.Value);
            //            }
            //            if (claim.Type == "userid")
            //            {
            //                userid = int.Parse(claim.Value);
            //            }
            //        }
            //        return context.AllBillPramtion(compid).ToList();
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.Error("Error in Item Master " + ex.Message);
            //        logger.Info("End  Item Master: ");
            //        return null;
            //    }
            //}
            return null;
        }

        [Route("")]
        public IEnumerable<BillPramotion> Get(int id)
        {
            using (var context = new AuthContext())
            {

                List<BillPramotion> ass = new List<BillPramotion>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

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
                    }
                    List<BillPramotion> itempramotion = new List<BillPramotion>();

                    itempramotion = (from ip in context.BillPramotions where ip.WarehouseId == id && ip.CompanyId == compid && (ip.StartDate < indianTime && ip.EndDate > indianTime) select ip).ToList();
                    return itempramotion;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Item Master " + ex.Message);
                    logger.Info("End  Item Master: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(BillPramotion))]
        [Route("")]
        [AcceptVerbs("POST")]
        public BillPramotion Post(BillPramotion pramotion)
        {
            using (var context = new AuthContext())
            {
                try
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

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
                    }

                    pramotion.CompanyId = compid;
                    context.AddBillPramtion(pramotion);

                    return pramotion;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Item Master " + ex.Message);
                    logger.Info("End  Item Master: ");
                    return null;
                }
            }
        }

    }
}
