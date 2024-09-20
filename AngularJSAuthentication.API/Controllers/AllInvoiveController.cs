using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/AllInvoice")]
    public class AllInvoiceController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        //[Authorize]
        //[Route("")]
        //public IEnumerable<AllInvoice> Get()
        //{
        //    //return Helper.CreateProjects().AsEnumerable();

        //    return context.AllInvoice;
        //}



        [ResponseType(typeof(AllInvoice))]
        [Route("")]
        [AcceptVerbs("POST")]
        public AllInvoice Post(AllInvoice item)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    // Access claims
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    item.CompanyId = compid;
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }

                    context.AddInvoice(item);

                    return item;
                }

                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }

            }

        }


    }
}



