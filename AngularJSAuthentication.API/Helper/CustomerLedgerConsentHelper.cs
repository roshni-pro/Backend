using AngularJSAuthentication.API.ControllerV7;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Transaction.Ledger;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using static AngularJSAuthentication.API.ControllerV7.CustomerLedgerConsentController;

namespace AngularJSAuthentication.API.Helper
{
    public class CustomerLedgerConsentHelper
    {
        public CustomerLedgerConsentDC AddCustomerLedgerConsent(CustomerLedgerConsentDC customerLedgerConsentDC)
        {
            using (var context = new AuthContext())
            {
                CustomerLedgerConsentMaster customerLedgerConsentMaster = new CustomerLedgerConsentMaster();

                if (customerLedgerConsentDC != null)
                {
                    customerLedgerConsentMaster.Name = customerLedgerConsentDC.Name;
                    customerLedgerConsentMaster.CreatedBy = customerLedgerConsentDC.Createdby;
                    customerLedgerConsentMaster.CreatedDate = DateTime.Now;
                    customerLedgerConsentMaster.IsActive = true;
                    customerLedgerConsentMaster.IsDeleted = false;
                    customerLedgerConsentMaster.StartDate = customerLedgerConsentDC.StartDate;
                    customerLedgerConsentMaster.EndDate = customerLedgerConsentDC.EndDate;

                    context.CustomerLedgerConsentMasterDB.Add(customerLedgerConsentMaster);
                    context.Commit();
                    customerLedgerConsentDC.Id = customerLedgerConsentMaster.Id;

                    if (customerLedgerConsentDC.customerLedgerConsentDetails.Count > 0)
                    {
                        List<CustomerLedgerConsentDetails> customerLedgerConsentDetails = AddCustomerLedgerConsentDetail(customerLedgerConsentDC, context);
                        context.CustomerLedgerConsentDetailsDB.AddRange(customerLedgerConsentDetails);
                    }
                    context.Commit();
                    //SentLedgerSms(customerLedgerConsentMaster.Id);
                    BackgroundJob.Schedule(() => SentLedgerSms(customerLedgerConsentMaster.Id), TimeSpan.FromSeconds(1));
                }
            }
            return customerLedgerConsentDC;
        }


        public void GenerateConsent()
        {
            using (var context = new AuthContext())
            {
                CustomerConsentConfiguration configuration
                    = context.CustomerOtpConfigurationDB.FirstOrDefault();

                if (configuration != null && configuration.LastRunDate.HasValue)
                {
                    DateTime nextRundate = configuration.LastRunDate.Value.AddDays(configuration.GenerateAfterDay).Date;
                    DateTime secondTrialRunDate = configuration.LastRunDate.Value.AddDays(configuration.SecondTryAftersNumberOfDays);
                    DateTime thirdTrialRunDate = configuration.LastRunDate.Value.AddDays( configuration.ThirdTryAftersNumberOfDays);

                    if (nextRundate == DateTime.Now.Date || secondTrialRunDate == DateTime.Now.Date || thirdTrialRunDate == DateTime.Now.Date)
                    {
                        int retryCount = 0;
                        var endDate = DateTime.Now.Date.AddDays(configuration.ShowLedgertBeforeNumberOfDays * -1);
                        if (secondTrialRunDate == DateTime.Now.Date)
                        {
                            retryCount = 1;
                            endDate = endDate.AddDays(configuration.SecondTryAftersNumberOfDays * -1);
                        }
                        else if (thirdTrialRunDate == DateTime.Now.Date)
                        {
                            retryCount = 2;
                            endDate = endDate.AddDays(configuration.ThirdTryAftersNumberOfDays * -1);
                        }

                        List<CustomerLedgerConsentDCNew> list
                            = context.Database.SqlQuery<CustomerLedgerConsentDCNew>("exec GetCustomerConsent @EndDate", new SqlParameter("@EndDate", endDate)).ToList();

                        if (list != null && list.Count > 0)
                        {
                            CustomerLedgerConsentMaster customerLedgerConsentMaster = new CustomerLedgerConsentMaster();
                            customerLedgerConsentMaster.Name = "";
                            customerLedgerConsentMaster.CreatedBy = 0;
                            customerLedgerConsentMaster.CreatedDate = DateTime.Now;
                            customerLedgerConsentMaster.StartDate = nextRundate;
                            customerLedgerConsentMaster.EndDate = nextRundate;
                            customerLedgerConsentMaster.IsActive = true;
                            customerLedgerConsentMaster.IsDeleted = false;
                            customerLedgerConsentMaster.CustomerOtpConfigurationId = configuration.Id;
                            context.CustomerLedgerConsentMasterDB.Add(customerLedgerConsentMaster);
                            context.Commit();
                            Random rand = new Random(100);
                            List<CustomerLedgerConsentDetails> customerLedgerConsentDetailsList = new List<CustomerLedgerConsentDetails>();
                            foreach (var item in list)
                            {
                                int randomNumber = rand.Next(000000, 999999);
                                CustomerLedgerConsentDetails detail = new CustomerLedgerConsentDetails();
                                detail.CustomerId = item.CustomerId;
                                detail.MasterId = customerLedgerConsentMaster.Id;
                                detail.Otp = int.Parse(OtpGenerator.GetSixDigitNumber(randomNumber));
                                detail.OtpMessage = null;
                                detail.CreatedBy = 0;
                                detail.CreatedDate = DateTime.Now;
                                detail.IsActive = true;
                                detail.IsDeleted = false;
                                detail.Consent = null;
                                detail.StartDate = item.StartDate;
                                detail.EndDate = item.EndDate;
                                detail.MobileNo = item.Mobile;
                                detail.Guid = Convert.ToString(Guid.NewGuid());
                                detail.IsMessageSend = false;
                                detail.RetryCount = retryCount;
                                detail.ConsentStatus = 1;
                                customerLedgerConsentDetailsList.Add(detail);
                            }

                            context.CustomerLedgerConsentDetailsDB.AddRange(customerLedgerConsentDetailsList);
                            if (retryCount == 0)
                            {
                                configuration.LastRunDate = DateTime.Today.Date;
                            }
                            context.Commit();

                            SentMessage(customerLedgerConsentDetailsList);
                        }
                    }

                }

            }
        }

        public void SentMessage(List<CustomerLedgerConsentDetails> list)
        {
            using (var context = new AuthContext())
            {
                //var query = from conf in context.CustomerOtpConfigurationDB
                //            join cc in context.CustomerLedgerConsentMasterDB
                //                on conf.Id equals cc.CustomerOtpConfigurationId
                //            join cd in context.CustomerLedgerConsentDetailsDB
                //                on cc.Id equals cd.MasterId
                //            //where conf.GenerateDate < DateTime.Now
                //            //    && conf.IsGenerated == true
                //            //    && (cd.ConsentStatus != 2 || cd.ConsentStatus != 4)
                //            select cd;

                //List<CustomerLedgerConsentDetails> list = query.ToList();

                string baseURL = ConfigurationManager.AppSettings["RetailerWebviewURL"].ToString();
                string relativeURL = "customerledgerconsentwebview/customerLedgerConsent/{#customerID#}/{#guid#}";


                if (list != null && list.Any())
                {
                    foreach (var item in list)
                    {
                        string uri = relativeURL.Replace("{#customerID#}", item.CustomerId.ToString());
                        uri = uri.Replace("{#guid#}", item.Guid);
                        uri = baseURL + uri;
                        item.OtpMessage = ConfigurationManager.AppSettings["LedgerMessage"];
                        item.OtpMessage = item.OtpMessage.Replace("{#URI#}", uri);
                        item.OtpMessage = item.OtpMessage.Replace("{#OTP#}", item.Otp.ToString());

                        bool messagesend = SendSMSHelper.SendSMS(item.MobileNo, item.OtpMessage, null,"");
                        if (messagesend)
                        {
                            item.IsMessageSend = true;
                            item.MessageSendDate = DateTime.Now;
                            context.Entry(item).State = EntityState.Modified;
                            context.Commit();
                        }
                    }
                   
                }
            }
        }

        public List<CustomerLedgerConsentDetails> AddCustomerLedgerConsentDetail(CustomerLedgerConsentDC customerLedgerConsentDC, AuthContext context)
        {
            List<CustomerLedgerConsentDetails> customerLedgerConsentDetails = new List<CustomerLedgerConsentDetails>();
            Random rand = new Random(100);
            foreach (var consentdetails in customerLedgerConsentDC.customerLedgerConsentDetails)
            {
                int randomNumber = rand.Next(000000, 999999);
                CustomerLedgerConsentDetails consentDetailsData = new CustomerLedgerConsentDetails();
                consentDetailsData.CustomerId = consentdetails.CustomerId;
                consentDetailsData.MasterId = customerLedgerConsentDC.Id;
                consentDetailsData.Otp = int.Parse(OtpGenerator.GetSixDigitNumber(randomNumber));
                consentDetailsData.OtpMessage = null;
                consentDetailsData.CreatedBy = customerLedgerConsentDC.Createdby;
                consentDetailsData.CreatedDate = DateTime.Now;
                consentDetailsData.IsActive = true;
                consentDetailsData.IsDeleted = false;
                consentDetailsData.Consent = null;
                consentDetailsData.StartDate = customerLedgerConsentDC.StartDate;
                consentDetailsData.EndDate = customerLedgerConsentDC.EndDate;
                consentDetailsData.MobileNo = context.Customers.Where(x => x.CustomerId == consentdetails.CustomerId).Select(x => x.Mobile).FirstOrDefault();
                consentDetailsData.Guid = Convert.ToString(Guid.NewGuid());
                consentDetailsData.IsMessageSend = false;
                consentDetailsData.ConsentStatus =1;
                customerLedgerConsentDetails.Add(consentDetailsData);
            }
            return customerLedgerConsentDetails;

        }



        public List<CustomerLedgerConsentDetails> AddCustomerLedgerConsentDetail(List<Customer> customerList, long customerLedgerConsentMasterId, int userid)
        {
            List<CustomerLedgerConsentDetails> customerLedgerConsentDetails = new List<CustomerLedgerConsentDetails>();
            string id = Convert.ToString(Guid.NewGuid());
            Random rand = new Random(100);

            foreach (var cust in customerList)
            {

                int randomNumber = rand.Next(000000, 999999);
                CustomerLedgerConsentDetails consentDetailsData = new CustomerLedgerConsentDetails();
                consentDetailsData.CustomerId = cust.CustomerId;
                consentDetailsData.MasterId = customerLedgerConsentMasterId;
                consentDetailsData.Otp = int.Parse(OtpGenerator.GetSixDigitNumber(randomNumber));
                consentDetailsData.OtpMessage = null;
                consentDetailsData.CreatedBy = userid;
                consentDetailsData.CreatedDate = DateTime.Now;
                consentDetailsData.IsActive = true;
                consentDetailsData.IsDeleted = false;
                consentDetailsData.Consent = null;
                consentDetailsData.MobileNo = cust.Mobile;
                consentDetailsData.Guid = id;
                consentDetailsData.IsMessageSend = false;
                consentDetailsData.ConsentStatus = 1;
                customerLedgerConsentDetails.Add(consentDetailsData);

            }
            return customerLedgerConsentDetails;


        }



        public CustomerLedgerConsentPageDC GetCustomerLedgerConsent(CustomerLedgerConsentPagerList pagerlist)
        {
            using (var context = new AuthContext())
            {
                if (pagerlist.ToDate.HasValue)
                {
                    pagerlist.ToDate = pagerlist.ToDate.Value.AddDays(1).AddSeconds(-1);
                }
                CustomerLedgerConsentPageDC ledgerconsentPageDc = new CustomerLedgerConsentPageDC();
                var query = from lconsent in context.CustomerLedgerConsentMasterDB
                            where (string.IsNullOrEmpty(pagerlist.Name)
                            || (!string.IsNullOrEmpty(lconsent.Name) && lconsent.Name.ToLower().Contains(pagerlist.Name.ToLower()))
                            && (!pagerlist.FromDate.HasValue || pagerlist.ToDate.HasValue || (lconsent.CreatedDate >= pagerlist.FromDate && lconsent.CreatedDate < pagerlist.ToDate))
                            && lconsent.IsActive == true && lconsent.IsDeleted == false)
                            select new
                            {
                                lconsent.Id,
                                lconsent.Name,
                                lconsent.CreatedDate
                            };

                ledgerconsentPageDc.Count = query.Count();
                ledgerconsentPageDc.PageList = query.OrderByDescending(x => x.Id).Skip(pagerlist.Skip).Take(pagerlist.Take).ToList();
                return ledgerconsentPageDc;
            }

        }

        public CustomerLedgerConsentPageDC GetCustomerLedgerMsgSend(CustomerLedgerConsentPagerList pagerlist)
        {
            using (var context = new AuthContext())
            {
                CustomerLedgerConsentPageDC customerLedgerConsentMsgSendDC = new CustomerLedgerConsentPageDC();
                var query = from lconsentDetails in context.CustomerLedgerConsentDetailsDB
                            join cust in context.Customers
                            on lconsentDetails.CustomerId equals cust.CustomerId
                            where (string.IsNullOrEmpty(pagerlist.Name)
                            || (!string.IsNullOrEmpty(pagerlist.Name) && cust.Name.ToLower().Contains(pagerlist.Name.ToLower()) || cust.Skcode.ToLower().Contains(pagerlist.Name.ToLower()) || cust.Mobile.Contains(pagerlist.Name))) &&
                            lconsentDetails.IsActive == true && lconsentDetails.IsDeleted == false && (!pagerlist.FromDate.HasValue || pagerlist.ToDate.HasValue || (lconsentDetails.CreatedDate >= pagerlist.FromDate && lconsentDetails.CreatedDate < pagerlist.ToDate))
                            && ((pagerlist.IsConsent == false) || !string.IsNullOrEmpty(lconsentDetails.Consent))
                            select new CustomerLedgerConsentMsgSendDC
                            {
                                Name = cust.Name,
                                Skcode = cust.Skcode,
                                MobileNo = cust.Mobile,
                                MessageSendDate = lconsentDetails.MessageSendDate,
                                IsMessageSend = lconsentDetails.IsMessageSend,
                                Consent = lconsentDetails.Consent,
                                LedgerStartDate= lconsentDetails.StartDate,
                                LedgerEndDate=lconsentDetails.EndDate,
                                ConsentStatus=lconsentDetails.ConsentStatus??0
                            };
                customerLedgerConsentMsgSendDC.Count = query.Count();
                customerLedgerConsentMsgSendDC.PageList = query.OrderByDescending(x => x.MessageSendDate).Skip(pagerlist.Skip).Take(pagerlist.Take).ToList();

                return customerLedgerConsentMsgSendDC;
            }
        }

        public CustomerLedgerConsentPagerVM GetLedgerEntryforCustomer(CustomerOTPVerification customerOTPVerification)
        {
            using (var context = new AuthContext())
            {
                CustomerLedgerConsentPagerVM customerLedgerConsentPagerVM = new CustomerLedgerConsentPagerVM();
                var consentdata = context.CustomerLedgerConsentDetailsDB.
                    Where(x => x.CustomerId == customerOTPVerification.CustomerId && x.Guid == customerOTPVerification.GUID && x.Otp == customerOTPVerification.OTP).Select(x => new { x.ConsentStatus,x.StartDate,x.EndDate } ).FirstOrDefault();
                if (consentdata.ConsentStatus >0)
                {
                    LedgerInputViewModel ledgerInputViewModel = new LedgerInputViewModel();
                    ledgerInputViewModel.LedgerTypeID = context.LadgerTypeDB.Where(x => x.code == "Customer").Select(x => x.ID).FirstOrDefault();
                    ledgerInputViewModel.LedgerID = Convert.ToInt32(context.LadgerDB.Where(x => x.ObjectType == "Customer" && x.ObjectID == customerOTPVerification.CustomerId).Select(x => x.ID).FirstOrDefault());
                    ledgerInputViewModel.ToDate = consentdata.EndDate;
                    ledgerInputViewModel.FromDate = consentdata.StartDate;
                    LedgerWorker ledgerWorker = new LedgerWorker();
                    ledgerInputViewModel.ReportCode = "SR";
                    customerLedgerConsentPagerVM.ledgerHistoryViewModel = ledgerWorker.GetLedger(ledgerInputViewModel);
                    customerLedgerConsentPagerVM.Message = "Success";
                    customerLedgerConsentPagerVM.ConsentStatus = consentdata.ConsentStatus;
                    customerLedgerConsentPagerVM.StartDate = consentdata.StartDate;
                    customerLedgerConsentPagerVM.EndDate = consentdata.EndDate;
                }
                else
                {
                    customerLedgerConsentPagerVM.ledgerHistoryViewModel = null;
                    customerLedgerConsentPagerVM.Message = "Failed";
                    //customerLedgerConsentPagerVM.ConsentStatus = null; 

                }
                return customerLedgerConsentPagerVM;
            }
        }

        public void SentLedgerSms(long customerLedgerConsentId)
        {
            if (customerLedgerConsentId > 0)
            {
                using (var context = new AuthContext())
                {
                    var list = context.CustomerLedgerConsentDetailsDB
                        .Where(x => x.MasterId == customerLedgerConsentId && x.IsActive == true && x.IsDeleted != true && x.IsMessageSend != true)
                        .ToList();



                    string baseURL = ConfigurationManager.AppSettings["RetailerWebviewURL"].ToString();
                    string relativeURL = "customerledgerconsentwebview/customerLedgerConsent/{#customerID#}/{#guid#}";


                    if (list != null && list.Any())
                    {
                        foreach (var item in list)
                        {
                            string uri = relativeURL.Replace("{#customerID#}", item.CustomerId.ToString());
                            uri = uri.Replace("{#guid#}", item.Guid);
                            uri = baseURL + uri;
                            item.OtpMessage = ConfigurationManager.AppSettings["LedgerMessage"];
                            item.OtpMessage = item.OtpMessage.Replace("{#URI#}", uri);
                            item.OtpMessage = item.OtpMessage.Replace("{#OTP#}", item.Otp.ToString());

                            bool messagesend = SendSMSHelper.SendSMS(item.MobileNo, item.OtpMessage, null,"");
                            if (messagesend)
                            {
                                item.IsMessageSend = true;
                                item.MessageSendDate = DateTime.Now;
                                context.Entry(item).State = EntityState.Modified;
                                context.Commit();
                            }

                        }
                    }



                }
            }
        }

        public bool AddConsent(ConnsentDetailsVM consentDetails, int userid)
        {
            using (var context = new AuthContext())
            {
                CustomerLedgerConsentDetails customerLedgerConsentDetails = context.CustomerLedgerConsentDetailsDB
                    .Where(x => x.CustomerId == consentDetails.CustomerId && x.Guid == consentDetails.Guid).FirstOrDefault();
                customerLedgerConsentDetails.ModifiedBy = userid;
                customerLedgerConsentDetails.ModifiedDate = DateTime.Now;
                customerLedgerConsentDetails.Consent = consentDetails.Consent;
                customerLedgerConsentDetails.ConsentStatus = consentDetails.Status;
                context.Entry(customerLedgerConsentDetails).State = EntityState.Modified;
                if (context.Commit() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }


        public List<CustomerLedgerConsentMsgSendDC> GenerateExcel(CustomerLedgerConsentPagerList pagerlist) {
            using (var context = new AuthContext())
            {
                var query = from lconsentDetails in context.CustomerLedgerConsentDetailsDB
                            join cust in context.Customers
                            on lconsentDetails.CustomerId equals cust.CustomerId
                            where (string.IsNullOrEmpty(pagerlist.Name)
                            || (!string.IsNullOrEmpty(pagerlist.Name) && cust.Name.ToLower().Contains(pagerlist.Name.ToLower()) || cust.Skcode.ToLower().Contains(pagerlist.Name.ToLower()) || cust.Mobile.Contains(pagerlist.Name))) &&
                            lconsentDetails.IsActive == true && lconsentDetails.IsDeleted == false && (!pagerlist.FromDate.HasValue || pagerlist.ToDate.HasValue || (lconsentDetails.CreatedDate >= pagerlist.FromDate && lconsentDetails.CreatedDate < pagerlist.ToDate))
                            && ((pagerlist.IsConsent == false) || !string.IsNullOrEmpty(lconsentDetails.Consent))
                            select new CustomerLedgerConsentMsgSendDC
                            {
                                Name = cust.Name,
                                Skcode = cust.Skcode,
                                MobileNo = cust.Mobile,
                                MessageSendDate = lconsentDetails.MessageSendDate,
                                IsMessageSend = lconsentDetails.IsMessageSend,
                                Consent = lconsentDetails.Consent,
                                ConsentStatus= lconsentDetails.ConsentStatus??0
                                
                               
                            };
                
                var data= query.OrderByDescending(x => x.MessageSendDate).ToList();
                return data;
            }
           
        }
    }
}