using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;


namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/SupplierBrands")]
    public class SupplierBrandsController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [Route("myBrands")]
        public HttpResponseMessage get() //get SUppliers Brands
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    var clist = context.GetmyBrands(compid);
                    if (clist == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "No Brands");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, clist);

                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("Brands")]
        public HttpResponseMessage getbrands() //get brands
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
                    var clist = context.getBrands(compid);
                    if (clist == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "No Brands");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, clist);

                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }


        [Route("myBrandsADUD")]
        public HttpResponseMessage post(List<SupplierBrandsDTO> obj)
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
                    if (compid == obj[0].CompanyId)
                    {
                        var addbrands = context.ADUDBrands(obj);
                        if (addbrands == true)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "Done !");
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, "Cant Add");
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }



    }
}



