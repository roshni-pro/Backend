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
    [RoutePrefix("api/CustomerCategorys")]

    public class CustomerCategoryController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Authorize]
        [Route("")]
        public IEnumerable<CustomerCategory> Get()
        {
            logger.Info("start CustomerCategory: ");
            using (AuthContext context = new AuthContext())
            {
                List<CustomerCategory> ass = new List<CustomerCategory>();
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
                    ass = context.AllCustomerCategory(compid).ToList();
                    logger.Info("End  CustomerCategory: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in CustomerCategory " + ex.Message);
                    logger.Info("End  CustomerCategory: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(CustomerCategory))]
        [Route("")]
        [AcceptVerbs("POST")]
        public CustomerCategory add(CustomerCategory item)
        {
            logger.Info("start addCustomerCategory: ");
            using (AuthContext context = new AuthContext())
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
                    item.CompanyId = compid;
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.AddCustomerCategory(item);
                    logger.Info("End  Customer: ");
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCustomerCategory " + ex.Message);
                    logger.Info("End  addCustomerCategory: ");
                    return null;
                }
            }
        }
        [ResponseType(typeof(CustomerCategory))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public CustomerCategory Put(CustomerCategory item)
        {
            using (AuthContext context = new AuthContext())
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
                    return context.PutCustomerCategory(item);
                }
                catch
                {
                    return null;
                }
            }
        }


        [ResponseType(typeof(CustomerCategory))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("start addCustomerCategory: ");
            using (AuthContext context = new AuthContext())
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
                    context.DeleteCustomerCategory(id);
                    logger.Info("End  delete CustomerCategory: ");
                }
                catch (Exception ex)
                {

                    logger.Error("Error in deleteCustomerCategory " + ex.Message);


                }
            }
        }
    }
}
