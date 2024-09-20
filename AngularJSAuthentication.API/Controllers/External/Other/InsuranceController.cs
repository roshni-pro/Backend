using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.External.Other
{
    [RoutePrefix("api/Insurance")]
    public class InsuranceController : ApiController
    {
        [Route("UpdateInsuranceDetail")]
        [HttpPost]
        [AllowAnonymous]
        public InsuranceWebHookResponse UpdateInsuranceDetail(InsuranceWebHookRequest insuranceWebHookRequest)
        {
            InsuranceWebHookResponse insuranceWebHookResponse = new InsuranceWebHookResponse();

            if (!(HttpContext.Current.Request.Headers.GetValues("UserName")?.FirstOrDefault() == ConfigurationManager.AppSettings["InsuranceUserName"].ToString() && HttpContext.Current.Request.Headers.GetValues("Password")?.FirstOrDefault() == ConfigurationManager.AppSettings["InsurancePassword"].ToString()))
            {
                insuranceWebHookResponse.Message = "Authorization has been denied for this request.";
                insuranceWebHookResponse.Status = false;
                return insuranceWebHookResponse;
            }

            if (string.IsNullOrEmpty(insuranceWebHookRequest.SkCode))
            {
                insuranceWebHookResponse.Message = "Retailer Skcode Required!";
                insuranceWebHookResponse.Status = false;
                return insuranceWebHookResponse;
            }

            using (AuthContext context = new AuthContext())
            {

                var customer = context.Customers.FirstOrDefault(x => x.Skcode == insuranceWebHookRequest.SkCode);
                if (customer != null)
                {
                    InsuranceCustomer insuranceCustomer =context.InsuranceCustomers.FirstOrDefault(x => x.CustomerId == customer.CustomerId);

                    if (insuranceCustomer == null)
                    {
                        insuranceCustomer = new InsuranceCustomer
                        {
                            Comment = insuranceWebHookRequest.Comment,
                            CreatedBy = 0,
                            CreatedDate = DateTime.Now,
                            CustomerId = customer.CustomerId,
                            InsuranceUrl = insuranceWebHookRequest.InsuranceUrl,
                            IPROCode = insuranceWebHookRequest.IPROCode,
                            IsActive = true,
                            IsDeleted = false,
                        };
                        context.InsuranceCustomers.Add(insuranceCustomer);
                    }
                    else
                    {
                        insuranceCustomer.IPROCode = insuranceWebHookRequest.IPROCode;
                        insuranceCustomer.InsuranceUrl = insuranceWebHookRequest.InsuranceUrl;
                        insuranceCustomer.Comment = insuranceWebHookRequest.Comment;
                        insuranceCustomer.ModifiedBy = 0;
                        insuranceCustomer.ModifiedDate = DateTime.Now;
                        context.Entry(insuranceCustomer).State = EntityState.Modified;
                    }
                    if (context.Commit() > 0)
                    {
                        insuranceWebHookResponse.Status = true;
                        insuranceWebHookResponse.Message = "Retailer Insurance Detail Save Successfully.";
                    }
                }
                else
                {
                    insuranceWebHookResponse.Status = false;
                    insuranceWebHookResponse.Message = "Invalid Retailer. Please Enter valid skcode";
                    
                }
                return insuranceWebHookResponse;


            }



        }


        [Route("RegisterRetailer")]
        [HttpPost]
        [AllowAnonymous]
        public InsuranceWebHookResponse RegisterRetailer(InsuranceRegistorRequest insuranceRegistorRequest)
        {
            InsuranceWebHookResponse insuranceWebHookResponse = new InsuranceWebHookResponse();

            if (!(HttpContext.Current.Request.Headers.GetValues("UserName")?.FirstOrDefault() == ConfigurationManager.AppSettings["InsuranceUserName"].ToString() && HttpContext.Current.Request.Headers.GetValues("Password")?.FirstOrDefault() == ConfigurationManager.AppSettings["InsurancePassword"].ToString()))
            {
                insuranceWebHookResponse.Message = "Authorization has been denied for this request.";
                insuranceWebHookResponse.Status = false;
                return insuranceWebHookResponse;
            }

            if (string.IsNullOrEmpty(insuranceRegistorRequest.SkCode))
            {
                insuranceWebHookResponse.Message = "Retailer Skcode Required!";
                insuranceWebHookResponse.Status = false;
                return insuranceWebHookResponse;
            }

            using (AuthContext context = new AuthContext())
            {

                var customer = context.Customers.FirstOrDefault(x => x.Skcode == insuranceRegistorRequest.SkCode);
                if (customer != null)
                {
                    InsuranceCustomer insuranceCustomer = context.InsuranceCustomers.FirstOrDefault(x => x.CustomerId == customer.CustomerId);

                    if (insuranceCustomer == null)
                    {
                        insuranceCustomer = new InsuranceCustomer
                        {
                            RegisteredMessage= insuranceRegistorRequest.Message,
                            Comment = "",
                            CreatedBy = 0,
                            CreatedDate = DateTime.Now,
                            CustomerId = customer.CustomerId,
                            InsuranceUrl = "",
                            IPROCode = "",
                            IsActive = true,
                            IsDeleted = false,
                        };
                        context.InsuranceCustomers.Add(insuranceCustomer);
                    }
                    else
                    {                       
                        insuranceCustomer.RegisteredMessage = insuranceRegistorRequest.Message;
                        insuranceCustomer.ModifiedBy = 0;
                        insuranceCustomer.ModifiedDate = DateTime.Now;
                        context.Entry(insuranceCustomer).State = EntityState.Modified;
                    }
                    if (context.Commit() > 0)
                    {
                        insuranceWebHookResponse.Status = true;
                        insuranceWebHookResponse.Message = "Retailer Registration Detail Save Successfully.";
                    }
                }
                else
                {
                    insuranceWebHookResponse.Status = false;
                    insuranceWebHookResponse.Message = "Invalid Retailer. Please Enter valid skcode";

                }
                return insuranceWebHookResponse;


            }



        }
    }

    public class InsuranceWebHookResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class InsuranceWebHookRequest
    {
        public string SkCode { get; set; }
        public string IPROCode { get; set; }
        public string InsuranceUrl { get; set; }
        public string Comment { get; set; }
    }

    public class InsuranceRegistorRequest
    {
        public string SkCode { get; set; }        
        public string Message { get; set; }
    }
}