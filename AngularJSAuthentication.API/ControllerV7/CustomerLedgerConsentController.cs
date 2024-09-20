using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Transaction.Ledger;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/CustomerLedgerConsent")]
    public class CustomerLedgerConsentController : BaseApiController
    {
        [Route("GetPage")]
        [HttpPost]
        public CustomerLedgerConsentPageDC GetPage(CustomerLedgerConsentPager pager)
        {
            using (var context = new AuthContext())
            {
                CustomerLedgerConsentPageDC ledgerconsentPageDc = new CustomerLedgerConsentPageDC();
                if (pager.CustomerId>0)
                {
                   
                    var query = from cust in context.Customers
                                where cust.Deleted == false && cust.Active == true && cust.Warehouseid == pager.WarehouseId && cust.CustomerId == pager.CustomerId
                                select new
                                {
                                    cust.CustomerId,
                                    cust.Warehouseid,
                                    cust.Name,
                                    cust.Skcode
                                };
                    ledgerconsentPageDc.Count = query.Count();
                    ledgerconsentPageDc.PageList = query.ToList();
                }
                else {
                        var query = from cust in context.Customers
                                    where cust.Deleted == false && cust.Active == true && cust.Warehouseid == pager.WarehouseId
                                    || (cust.Skcode.ToLower().Contains(pager.Name.ToLower())
                                    || cust.Name.ToLower().Contains(pager.Name.ToLower()) || cust.ShopName.ToLower().Contains(pager.Name.ToLower()))
                                    select new
                                    {
                                        cust.CustomerId,
                                        cust.Warehouseid,
                                        cust.Name,
                                        cust.Skcode
                                    };
                        ledgerconsentPageDc.Count = query.Count();
                        ledgerconsentPageDc.PageList = query.OrderByDescending(x => x.CustomerId).Skip(pager.SkipCount).Take(pager.Take).ToList();
                    }
                
                return ledgerconsentPageDc;
            }
        }


        [Route("GetCustomer")]
        [HttpGet]
        public IHttpActionResult GetByName(int WarehouseId,string name)
       {
            using (var context = new AuthContext())
            {
                if (WarehouseId > 0)
                {
                    var customerList = context.Customers.Where(x => x.Warehouseid == WarehouseId && x.Deleted == false && x.Active == true && (x.Skcode.ToLower()
                    .Contains(name.ToLower()) || x.Name.ToLower().Contains(name.ToLower()) || x.ShopName.ToLower().Contains(name.ToLower())))
                    .Select(x => new { x.CustomerId, Name = x.Name + "-" + x.Skcode }).Take(50).ToList();
                    return Ok(customerList);
                }
                else {
                    var customerList = context.Customers.Where(x =>  x.Deleted == false && x.Active == true && (x.Skcode.ToLower()
                      .Contains(name.ToLower()) || x.Name.ToLower().Contains(name.ToLower()) || x.ShopName.ToLower().Contains(name.ToLower())))
                      .Select(x => new { x.CustomerId, Name = x.Name + "-" + x.Skcode }).Take(50).ToList();
                    return Ok(customerList);
                }
               
            }
        }
        [Route("AddConsent")]
        [HttpPost]
        public CustomerLedgerConsentDC Add(CustomerLedgerConsentDC customerLedgerConsentDC) {

            CustomerLedgerConsentHelper customerLedgerConsentHelper = new CustomerLedgerConsentHelper();
            customerLedgerConsentDC = customerLedgerConsentHelper.AddCustomerLedgerConsent(customerLedgerConsentDC);
            return customerLedgerConsentDC;
        }

        [Route("GetConsentList")]
        [HttpPost]
        public CustomerLedgerConsentPageDC GetLedgerConsent(CustomerLedgerConsentPagerList customerLedgerConsentPagerList)
        {
            CustomerLedgerConsentPageDC customerLedgerConsentPageDC = new CustomerLedgerConsentPageDC();
            CustomerLedgerConsentHelper customerLedgerConsentHelper = new CustomerLedgerConsentHelper();
            customerLedgerConsentPageDC = customerLedgerConsentHelper.GetCustomerLedgerMsgSend(customerLedgerConsentPagerList);
            return customerLedgerConsentPageDC;
        }
        //[Route("GetMsgSendList")]
        //[HttpPost]
        //public CustomerLedgerConsentPageDC GetMsgSendList(CustomerLedgerMessagePager customerLedgerMessagePager)
        //{
        //    CustomerLedgerConsentPageDC customerLedgerConsentPageDC = new CustomerLedgerConsentPageDC();
        //    CustomerLedgerConsentHelper customerLedgerConsentHelper = new CustomerLedgerConsentHelper();
        //    customerLedgerConsentPageDC = customerLedgerConsentHelper.GetCustomerLedgerMsgSend(customerLedgerMessagePager);
        //    return customerLedgerConsentPageDC;
           
        //}

        [Route("GetLedgerDetails")]
        [HttpPost]
        public CustomerLedgerConsentPagerVM GetOTP(CustomerOTPVerification customerOTPVerification)
        {
            using (var context = new AuthContext()) {
                CustomerLedgerConsentPagerVM customerLedgerConsentPagerVM = new CustomerLedgerConsentPagerVM();
                CustomerLedgerConsentHelper customerLedgerConsentHelper = new CustomerLedgerConsentHelper();
                customerLedgerConsentPagerVM = customerLedgerConsentHelper.GetLedgerEntryforCustomer(customerOTPVerification);
                return customerLedgerConsentPagerVM;
            }

        }


        [Route("UploadNewCustomerList")]
        [HttpPost]
        public bool UploadNewCustomerList(List<string> skCodeList)
        {
            int userId = GetLoginUserId();
            if (skCodeList != null && skCodeList.Count > 0)
            {
                skCodeList = skCodeList.Where(x => !string.IsNullOrEmpty(x)).ToList();
                skCodeList.ForEach(d => d.ToLower());



            }
            using (var context = new AuthContext())
            {

                List<Customer> customerList = context.Customers.Where(x => !string.IsNullOrEmpty(x.Skcode) && skCodeList.Contains(x.Skcode.ToLower())).ToList();

                if (customerList != null && customerList.Count > 0)
                {
                    CustomerLedgerConsentMaster customerLedgerConsentMaster = new CustomerLedgerConsentMaster();
                    customerLedgerConsentMaster.Name = "uploaded_file_on_" + DateTime.Now.ToString("yyyy_MM_dd");
                    customerLedgerConsentMaster.CreatedBy = userId;
                    customerLedgerConsentMaster.CreatedDate = DateTime.Now;
                    customerLedgerConsentMaster.IsActive = true;
                    customerLedgerConsentMaster.IsDeleted = false;
                    context.CustomerLedgerConsentMasterDB.Add(customerLedgerConsentMaster);
                    context.Commit();

                    CustomerLedgerConsentHelper helper = new CustomerLedgerConsentHelper();
                    List<CustomerLedgerConsentDetails> customerLedgerConsentDetails = helper.AddCustomerLedgerConsentDetail(customerList, customerLedgerConsentMaster.Id, userId);
                    context.CustomerLedgerConsentDetailsDB.AddRange(customerLedgerConsentDetails);
                    context.Commit();
                    BackgroundJob.Schedule(() => helper.SentLedgerSms(customerLedgerConsentMaster.Id), TimeSpan.FromSeconds(1));


                }

            }
            return true;
        }

        [Route("AddLedgerconsent")]
        [HttpPost]
        public bool AddLedgerconsent(ConnsentDetailsVM consentDetails)
        {
            int userId = GetLoginUserId();
            using (var context = new AuthContext())
            {
                CustomerLedgerConsentHelper customerLedgerConsentHelper = new CustomerLedgerConsentHelper();
                bool IsSave = customerLedgerConsentHelper.AddConsent(consentDetails, userId);
                return IsSave;
            }

        }


        [Route("GenerateConsent")]
        [HttpGet]
        [AllowAnonymous]
        public void GenerateConsent()
        {
            CustomerLedgerConsentHelper helper = new CustomerLedgerConsentHelper();
            helper.GenerateConsent();
        }
        [Route("GenerateExcel")]
        [HttpPost]
        public List<CustomerLedgerConsentMsgSendDC> GenerateConsentExcel(CustomerLedgerConsentPagerList customerLedgerConsentPagerList)
        {
            CustomerLedgerConsentHelper helper = new CustomerLedgerConsentHelper();
            var data=helper.GenerateExcel(customerLedgerConsentPagerList);
            return data;
        }

        public class CustomerLedgerConsentPagerVM
        {
            public string Message { get; set; }
            public LedgerHistoryViewModel ledgerHistoryViewModel { get; set; }
            public DateTime  StartDate { get; set; }
            public DateTime EndDate { get; set; }

            public int? ConsentStatus { get; set; }
        }



    }
}
