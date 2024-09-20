using AngularJSAuthentication.API.App_Code.SmeLoan;
using AngularJSAuthentication.BusinessLayer.SMELoan.BO;
using AngularJSAuthentication.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.SMELoan
{
    [RoutePrefix("api/SMELoan")]
    public class SMEController : ApiController
    {

        private SMERepository SMERepository;
        public SMEController()
        {
            this.SMERepository = new SMERepository(new AuthContext());
        }

        public SMEController(SMERepository SMERepository)
        {
            this.SMERepository = SMERepository;
        }

       
        [Route("Getcustomer")]
        [HttpGet]
        public HttpResponseMessage GetCustomerDetails(int CustomerId)
        {
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    //int Userid = GetUserId();
                    Customer ObjCustomer = context.Customers.Where(x => x.CustomerId == CustomerId && x.Deleted == false).FirstOrDefault();
                    SMECustomerLoan ObjSmecustomerLoan = context.SMECustomerLoan.Where(x => x.CustomerId == CustomerId ).FirstOrDefault();

                    SMECustomerDC ObjSMECustomerDC = new SMECustomerDC()
                    {
                        Customer = ObjCustomer,
                        SMECustomerLoan = ObjSmecustomerLoan
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, ObjSMECustomerDC);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.OK, ex.GetBaseException().Message.ToString());
            }
        }

        [Route("PostCustomerDetails")]
        [HttpPost]
        public HttpResponseMessage PostCustomerDetails(SMECustomerLoan objSMECustomerLoan)
        {
            try
            {
                
                objSMECustomerLoan.CreatedBy = GetUserId();
                objSMECustomerLoan.CreatedDate = DateTime.Now;
                objSMECustomerLoan.Status = 0;
                objSMECustomerLoan.DateofBirth = Convert.ToDateTime(objSMECustomerLoan.DateofBirth);
                objSMECustomerLoan.UpdateDate = DateTime.Now;
                objSMECustomerLoan.UpdatedBy = GetUserId();
                SMEDc ObjSmeDc = SMERepository.PostCustomer(objSMECustomerLoan);

                return Request.CreateResponse(HttpStatusCode.OK, ObjSmeDc);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }
        }

        
        [Route("UploadDocument")]
        [HttpPost]
        public IHttpActionResult UploadDocument()
        {
            SMECustomerLoan objSMECustomerLoan = new SMECustomerLoan();
            bool Res = SMERepository.Uploaddocument();
            return Created("true", Res);
        }
        //private SMEResponse PostDataTOSME(int id)
        //{
        //    try
        //    {
        //        HttpClient client = new HttpClient();

        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(
        //            new MediaTypeWithQualityHeaderValue("application/json"));

        //        client.DefaultRequestHeaders.Add("api_key", "5116528b6c24692d1a189750143297d3");
        //        client.DefaultRequestHeaders.Add("package_id", "SME_WEB_1.0");
        //        SMECustomerLoan ObjsMECustomerLoan = 

        //        Application ObjApplication = new Application();
        //        //Dictionary<object, object> ObjDictionary = new Dictionary<object, object>();



        //        ObjApplication.product = "Small Retailer Business Loan";
        //        ObjApplication.applicant_1_gender = "Male";
        //        ObjApplication.constitution = "Proprietorship";
        //        ObjApplication.lead_source = "Unassisted";
        //        ObjApplication.applicant_1_residence_address = "test";
        //        ObjApplication.source = "MSK007";
        //        ObjApplication.loan_requested = "2800000";
        //        ObjApplication.applicant_1_dob = "1988-08-27";
        //        ObjApplication.applicant_1_is_mainapplicant = true;
        //        ObjApplication.lead_city = "Kolkata";
        //        ObjApplication.existing_emi_obligation = "25000";
        //        ObjApplication.residence_pincode = "400013";
        //        ObjApplication.business_type = "Proprietorship";
        //        ObjApplication.applicant_1_aadhaar = "646454646434";
        //        ObjApplication.company = "ShopKirana";
        //        ObjApplication.applicant_1_first_name = "Ravindra2";
        //        ObjApplication.itr_filed = true;
        //        ObjApplication.applicant_1_phone = "8586552866";
        //        ObjApplication.submitter_phone = "1234123482";

        //        ObjApplication.applicant_1_applicant_id = "1";
        //        ObjApplication.source_channel = "marketplace";
        //        ObjApplication.turnover_curr_year = "20000000";
        //        ObjApplication.applicant_1_residence_Ownership = "Self Owned";
        //        ObjApplication.pat_curr_year = "5000000";
        //        ObjApplication.itr_filed_prev_year = true;
        //        ObjApplication.applicant_1_pan = "gsysy7373j";
        //        ObjApplication.office_pincode = "400013";
        //        ObjApplication.office_address = "Ddat parrl 8383mahatashyta infai";
        //        ObjApplication.vintage = "52";
        //        ObjApplication.applicant_1_email = "gagag@jsus.cok";
        //        ObjApplication.applicant_1_residence_pincode = "400013";
        //        ObjApplication.lead_received_on = "2018-08-27";
        //        ObjApplication.office_ownership = "Self Owned";


        //        var res = client.PostAsJsonAsync("https://demo.smecorner.com/SME/loan/application/complete/save?phoneNo=" + "1234123482", ObjApplication).Result;
        //        string jsonString = res.Content.ReadAsStringAsync().Result.Replace("\\", "").Trim(new char[1] { '"' });

        //        if (res.IsSuccessStatusCode)
        //        {
        //            SMEResponse objSmeResponse = JsonConvert.DeserializeObject<SMEResponse>(jsonString);


        //            return objSmeResponse;
        //        }
        //        else
        //        {
        //            throw new Exception("Something went wrong please try again later!!");

        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;

        //    }
        //}



        //[HttpPost]
        //[Route("PostSmeLoan")]
        //public HttpResponseMessage PostSmeLoan()
        //{
        //    try
        //    {
        //        HttpClient client = new HttpClient();

        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(
        //            new MediaTypeWithQualityHeaderValue("application/json"));

        //        client.DefaultRequestHeaders.Add("api_key", "5116528b6c24692d1a189750143297d3");
        //        client.DefaultRequestHeaders.Add("package_id", "SME_WEB_1.0");
        //        Application ObjApplication = new Application();
        //        //Dictionary<object, object> ObjDictionary = new Dictionary<object, object>();

        //        ObjApplication.product = "Small Retailer Business Loan";
        //        ObjApplication.applicant_1_gender = "Male";
        //        ObjApplication.constitution = "Proprietorship";
        //        ObjApplication.lead_source = "Unassisted";
        //        ObjApplication.applicant_1_residence_address = "test";
        //        ObjApplication.source = "MSK007";
        //        ObjApplication.loan_requested = "2800000";
        //        ObjApplication.applicant_1_dob = "1988-08-27";
        //        ObjApplication.applicant_1_is_mainapplicant = true;
        //        ObjApplication.lead_city = "Kolkata";
        //        ObjApplication.existing_emi_obligation = "25000";
        //        ObjApplication.residence_pincode = "400013";
        //        ObjApplication.business_type = "Proprietorship";
        //        ObjApplication.applicant_1_aadhaar = "646454646434";
        //        ObjApplication.company = "ShopKirana";
        //        ObjApplication.applicant_1_first_name = "Ravindra2";
        //        ObjApplication.itr_filed = true;
        //        ObjApplication.applicant_1_phone = "8586552866";
        //        ObjApplication.submitter_phone = "1234123482";

        //        ObjApplication.applicant_1_applicant_id = "1";
        //        ObjApplication.source_channel = "marketplace";
        //        ObjApplication.turnover_curr_year = "20000000";
        //        ObjApplication.applicant_1_residence_Ownership = "Self Owned";
        //        ObjApplication.pat_curr_year = "5000000";
        //        ObjApplication.itr_filed_prev_year = true;
        //        ObjApplication.applicant_1_pan = "gsysy7373j";
        //        ObjApplication.office_pincode = "400013";
        //        ObjApplication.office_address = "Ddat parrl 8383mahatashyta infai";
        //        ObjApplication.vintage = "52";
        //        ObjApplication.applicant_1_email = "gagag@jsus.cok";
        //        ObjApplication.applicant_1_residence_pincode = "400013";
        //        ObjApplication.lead_received_on = "2018-08-27";
        //        ObjApplication.office_ownership = "Self Owned";

        //        //ObjDictionary.Add("product", "Small Retailer Business Loan");
        //        //ObjDictionary.Add("applicant_1_gender", "Male");
        //        //ObjDictionary.Add("constitution", "Proprietorship");
        //        //ObjDictionary.Add("lead_source", "Unassisted");
        //        //ObjDictionary.Add("applicant_1_residence_address", "test");
        //        //ObjDictionary.Add("source", "MSK007");
        //        //ObjDictionary.Add("loan_requested", "2800000");
        //        //ObjDictionary.Add("applicant_1_dob", "1988-08-27");
        //        //ObjDictionary.Add("applicant_1_is_mainapplicant", true);
        //        //ObjDictionary.Add("lead_city", "Kolkata");
        //        //ObjDictionary.Add("existing_emi_obligation", "25000");
        //        //ObjDictionary.Add("residence_pincode", "400013");
        //        //ObjDictionary.Add("business_type", "Proprietorship");
        //        //ObjDictionary.Add("applicant_1_aadhaar", "646454646434");
        //        //ObjDictionary.Add("applicant_1_first_name", "Ravindra2");
        //        //ObjDictionary.Add("itr_filed", true);
        //        //ObjDictionary.Add("applicant_1_phone", "8586552866");
        //        //ObjDictionary.Add("submitter_phone", "1234123482");
        //        //ObjDictionary.Add("applicant_1_applicant_id", "1");
        //        //ObjDictionary.Add("source_channel", "marketplace");
        //        //ObjDictionary.Add("turnover_curr_year", "20000000");
        //        //ObjDictionary.Add("applicant_1_residence_Ownership", "Self Owned");
        //        //ObjDictionary.Add("pat_curr_year", "5000000");
        //        //ObjDictionary.Add("itr_filed_prev_year", true);
        //        //ObjDictionary.Add("applicant_1_pan", "gsdsy7373j");
        //        //ObjDictionary.Add("office_pincode", "400013");
        //        //ObjDictionary.Add("office_address", "Ddat parrl 8383mahatashyta infai");
        //        //ObjDictionary.Add("vintage", "52");
        //        //ObjDictionary.Add("applicant_1_email", "gagag@jsus.cok");
        //        //ObjDictionary.Add("applicant_1_residence_pincode", "400013");
        //        //ObjDictionary.Add("lead_received_on", "2018-08-27");
        //        //ObjDictionary.Add("office_ownership", "Self Owned");




        //        var res = client.PostAsJsonAsync("https://demo.smecorner.com/SME/loan/application/complete/save?phoneNo=" + "1234123482", ObjApplication).Result;
        //        string jsonString = res.Content.ReadAsStringAsync().Result.Replace("\\", "").Trim(new char[1] { '"' });

        //        if (res.IsSuccessStatusCode)
        //        {
        //            SMEResponse objSmeResponse = JsonConvert.DeserializeObject<SMEResponse>(jsonString);



        //            return Request.CreateResponse(HttpStatusCode.OK, res.Content.ReadAsStringAsync().Result);
        //        }
        //        else
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Something went wrong please try again later!!");

        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.InnerException.Message.ToString());
        //    }



        //}
        private int GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }

        [HttpGet]
        [Route("GetLoanAppliedDetails")]
        public HttpResponseMessage GetLoanAppliedDetails(int? WarehouseId, string SkCode,int First,int Last)
        {
            try
            {

                SMECustomerDC objLoanAppliedDetails = SMERepository.GetLoanAppliedDetails(WarehouseId, SkCode, First,Last);
                return Request.CreateResponse(HttpStatusCode.OK, objLoanAppliedDetails);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }
        }

        [HttpGet]
        [Route("GetWarehouse")]
        public HttpResponseMessage GetWarehouse()
        {
            try
            {

                IEnumerable<Warehouse> ObjWarehouse  = SMERepository.GetWarehouse();
                return Request.CreateResponse(HttpStatusCode.OK, ObjWarehouse);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }
        }
    }
}
