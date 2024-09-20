using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/RBLCustomerInformation")]
    public class RBLCustomerInformationController : ApiController
    {
       
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private DateTime indianTime;
        // public DateTime IndianTime { get => IndianTime1; set => IndianTime1 = value; }
        //  public DateTime IndianTime1 { get => indianTime; set => indianTime = value; }

        [Route("RBLCustInformation")]
        [HttpPost]
        public HttpResponseMessage Post(RBLCustomerInformation obj)
        {
            logger.Info("start Registration RBLCustInformation: ");
            try
            {
                using (var db = new AuthContext())
                {
                    Customer customer = db.Customers.Where(x => x.CustomerId == obj.customerId).FirstOrDefault();
                    RBLCustomerInformation RblCust = db.RBLCustomerInformationDB.Where(x => x.customerId == obj.customerId).FirstOrDefault();
                    if (customer == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Do Registration of customer first then update RBLCustInformation ");
                    }
                    else if (RblCust == null)
                    {
                        obj.createdDate = DateTime.Now;
                        obj.updatedDate = DateTime.Now;
                        obj.deleted = false;
                        db.RBLCustomerInformationDB.Add(obj);
                        db.Commit();
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }
                    else
                    {
                        RblCust.peopleId = obj.peopleId;
                        RblCust.gender = obj.gender;
                        RblCust.title = obj.title;
                        RblCust.firstName = obj.firstName;
                        RblCust.lastName = obj.lastName;
                        RblCust.dateOfBirth = obj.dateOfBirth;
                        RblCust.motherMaidenName = obj.motherMaidenName;
                        RblCust.community = obj.community;
                        RblCust.maritalStatus = obj.maritalStatus;
                        RblCust.grossIncome = obj.grossIncome;
                        RblCust.doNotCall = obj.doNotCall;
                        RblCust.panNumber = obj.panNumber;
                        RblCust.address1 = obj.address1;
                        RblCust.addressFormat1 = obj.addressFormat1;
                        RblCust.addressType1 = obj.addressType1;
                        RblCust.addressLabel1 = obj.addressLabel1;
                        RblCust.addressLine11 = obj.addressLine11;
                        RblCust.addressLine21 = obj.addressLine21;
                        RblCust.addressLine31 = obj.addressLine31;
                        RblCust.city1 = obj.city1;
                        RblCust.state1 = obj.state1;
                        RblCust.postalCode1 = obj.postalCode1;
                        RblCust.phone1 = obj.phone1;
                        RblCust.shopImage = obj.shopImage;
                        RblCust.createdBy = obj.createdBy;
                        RblCust.updatedDate = DateTime.Now;
                        db.RBLCustomerInformationDB.Attach(RblCust);
                        db.Entry(RblCust).State = EntityState.Modified;
                        db.Commit();
                        return Request.CreateResponse(HttpStatusCode.OK, RblCust);
                    }
                }
            }

            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Something issue in RBLCustomerInformation" + ex);
            }
        }

        #region  Get RBL Data Using CustomerId 
        [Route("GetRBLCustInformation")]
        [HttpGet]
        public HttpResponseMessage Get(int customerId)
        {
            logger.Info("start get RBLCustInformation: ");
            try
            {
                using (var db = new AuthContext())
                {
                    RBLCustomerInformation RblCust = db.RBLCustomerInformationDB.Where(x => x.customerId == customerId).FirstOrDefault();
                    if (RblCust != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, RblCust);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "No record found");

                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Something issue " + ex);
            }
        }
        //...........................................Rbl........................................................//

        [Route("rbldata")]
        [HttpGet]
        public IEnumerable<RBLCustomerInformation> Getrbldata()
        {
            logger.Info("start RBLCustomerInformation: ");
            // SupplierStatus sup = new SupplierStatus();
            List<RBLCustomerInformation> ass = new List<RBLCustomerInformation>();
          
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
                using (AuthContext db = new AuthContext())
                {
                    ass = db.RBLCustomerInformationDB.Where(x => x.deleted == false).ToList();
                    //tt = context.DPurchaseOrderMaster.Where(x => x.Status == "pending" ).ToList();

                    return ass;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in RBLCustomerInformation " + ex.Message);
                logger.Info("End RBLCustomerInformation: ");
                return null;
            }
        }
        [Route("putshopimage")]
        [AcceptVerbs("PUT")]
        public RBLCustomerInformation Putshopimage(RBLCustomerInformation item)
        {
            logger.Info("start putRBLCustomerInformation: ");
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

                if (item == null)
                {
                    throw new ArgumentNullException("putRBLCustomerInformation");
                }
                if (compid == 0)
                {
                    compid = 1;
                }

                using (var db = new AuthContext())
                {
                    RBLCustomerInformation cust = db.RBLCustomerInformationDB.Where(x => x.deleted == false).FirstOrDefault();
                    db.Entry(cust).State = EntityState.Modified;
                    db.Commit();
                    return cust;
                }
            }
            catch (Exception ex)
            {

                return null;
            }
        }
        //.............................Delete............................//
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("start delete RBLCustomerInformation: ");
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
                int CompanyId = compid;
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                using (var context = new AuthContext())
                {
                    context.DeleteCustomer(id);
                }
                logger.Info("End  delete RBLCustomerInformation: ");
            }
            catch (Exception ex)
            {
                logger.Error("Error in delete RBLCustomerInformation " + ex.Message);
            }
        }
    }
}


#endregion