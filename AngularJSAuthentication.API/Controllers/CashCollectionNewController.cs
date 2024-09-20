using AngularJSAuthentication.API.ControllerV1;
using AngularJSAuthentication.API.DataContract;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Managers.NotificationApprovalMatrix;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.CashManagement;
using AngularJSAuthentication.Model.Store;
using NLog;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Transactions;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/TestCashCollection")]
    public class CashCollectionNewController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [Route("GenerateOTPForCurrency")]
        [HttpPost]
        public string GenerateOTPForCurrency(OTPGenerate OtpGen)
        {
            string result = "";
            try
            {

                using (AuthContext context = new AuthContext())
                {
                    var people = context.Peoples.FirstOrDefault(x => x.PeopleID == OtpGen.peopleid);
                    string Msg = "";
                    if (people != null)
                    {
                        var otp = CurrencyGetotp(people.Mobile);
                        if (otp != null && !string.IsNullOrEmpty(otp.OtpNo))
                        {
                            //var CurrencyVerification = context.CurrencyVerification.FirstOrDefault(x => x.PeopleId == userid && x.MobileNo == people.Mobile && x.WarehouseId == people.WarehouseId && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(DateTime.Now));
                            var OtpVerification = context.OTPVerificationDb.FirstOrDefault(x => x.DBoyPeopleId == OtpGen.peopleid && x.AssignmentID == OtpGen.assignmentID && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(DateTime.Now));
                            if (OtpVerification != null)
                            {
                                OtpVerification.AssignmentID = OtpGen.assignmentID;
                                OtpVerification.DBoyPeopleId = OtpGen.peopleid;
                                OtpVerification.OTP = otp.OtpNo;
                                OtpVerification.CashierPeopleId = 0;
                                OtpVerification.CreatedDate = DateTime.Now;
                                OtpVerification.IsActive = true;
                                OtpVerification.IsDeleted = false;
                                OtpVerification.CreatedBy = OtpGen.peopleid;
                                context.Entry(OtpVerification).State = EntityState.Modified;
                                context.Commit();
                                result = otp.OtpNo;


                            }
                            else
                            {
                                OTPVerification otpverify = new OTPVerification();
                                otpverify.AssignmentID = OtpGen.assignmentID;
                                otpverify.DBoyPeopleId = OtpGen.peopleid;
                                otpverify.OTP = otp.OtpNo;
                                otpverify.CashierPeopleId = 0;
                                otpverify.CreatedDate = DateTime.Now;
                                otpverify.ModifiedDate = null;
                                otpverify.IsActive = false;
                                otpverify.IsDeleted = true;
                                otpverify.CreatedBy = OtpGen.peopleid;

                                //context.OTPVerification.Add(otpverify);
                                context.OTPVerificationDb.Add(otpverify);
                                context.Commit();
                                result = otp.OtpNo;
                            }

                        }

                    }
                }
                return result;

            }
            catch (Exception ex)
            {
                logger.Error("Error in GenerateOTPForCurrency Method: " + ex.Message);
                return result = null;
            }

        }


        private string GenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)
        {
            string sOTP = String.Empty;
            string sTempChars = String.Empty;
            Random rand = new Random();

            for (int i = 0; i < iOTPLength; i++)
            {
                int p = rand.Next(0, saAllowedCharacters.Length);
                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
                sOTP += sTempChars;
            }
            return sOTP;
        }




        public OTP CurrencyGetotp(string MobileNumber)
        {
            //logger.Info("start Gen OTP: ");
            try
            {

                string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);
                // string OtpMessage = " : is Your Verification Code. :).ShopKirana";
                string OtpMessage = ""; //"{#var1#} : is Your Verification Code. {#var2#}.ShopKirana";
                var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "Currency_Verification_Code");
                OtpMessage = dltSMS == null ? "" : dltSMS.Template;

                OtpMessage = OtpMessage.Replace("{#var1#}", sRandomOTP);
                OtpMessage = OtpMessage.Replace("{#var2#}", ":)");

                //string message = sRandomOTP + " :" + OtpMessage;
                if (dltSMS != null)
                    Common.Helpers.SendSMSHelper.SendSMS(MobileNumber, OtpMessage, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);

                // logger.Info("OTP Genrated: " + sRandomOTP);
                OTP a = new OTP()
                {
                    OtpNo = sRandomOTP
                };
                return a;
            }
            catch (Exception ex)
            {
                //logger.Error("Error in OTP Genration.");
                return null;
            }
        }

        //[Route("Genotp")]
        //public OTP Getotp(string MobileNumber)
        //{
        //    //logger.Info("start Gen OTP: ");
        //    try
        //    {

        //        string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
        //        string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);
        //        // string OtpMessage = " : is Your Verification Code. :).ShopKirana";
        //        string OtpMessage = ""; //"{#var1#} : is Your Verification Code. {#var2#}.ShopKirana";
        //        var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "PR_Waitng_Approval");
        //        OtpMessage = dltSMS == null ? "" : dltSMS.Template;

        //        OtpMessage = OtpMessage.Replace("{#var1#}", sRandomOTP);
        //        OtpMessage = OtpMessage.Replace("{#var2#}", ":)");

        //        //string message = sRandomOTP + " :" + OtpMessage;
        //        if (dltSMS != null)
        //            Common.Helpers.SendSMSHelper.SendSMS(MobileNumber, OtpMessage, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);

        //        // logger.Info("OTP Genrated: " + sRandomOTP);
        //        OTP a = new OTP()
        //        {
        //            OtpNo = sRandomOTP
        //        };
        //        return a;
        //    }
        //    catch (Exception ex)
        //    {
        //        //logger.Error("Error in OTP Genration.");
        //        return null;
        //    }
        //}

        [HttpGet]
        [Route("ValidateOTPForDBoy")]
        public UploadAssignment ValidateOTPForDboy(string otp, int DeliveryBoyID, int AssignmentID)
        {

            string result = "";
            string Msg = "";
            UploadAssignment response = null;
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                List<string> roleNames = new List<string>();
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                    roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();
                if (roleNames.Any(x => x == "Hub Cashier"))
                {
                    TransactionOptions option = new TransactionOptions();
                    option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                    option.Timeout = TimeSpan.FromSeconds(120);
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                    {

                        using (AuthContext context = new AuthContext())
                        {
                            var OtpVerification = context.OTPVerificationDb.FirstOrDefault(x => x.DBoyPeopleId == DeliveryBoyID && x.OTP == otp && x.AssignmentID == AssignmentID);
                            var people = context.Peoples.FirstOrDefault(x => x.PeopleID == DeliveryBoyID);
                            if (OtpVerification != null)
                            {


                                var GenDateTime = OtpVerification.CreatedDate;
                                var cashDateTime = DateTime.Now;
                                var verifyDateTime = cashDateTime.Subtract(GenDateTime);
                                double TimeDiff = verifyDateTime.TotalMinutes;

                                if (OtpVerification.CreatedDate.Date == DateTime.Now.Date && TimeDiff < 10)
                                {
                                    if (OtpVerification != null && people != null)
                                    {
                                        OtpVerification.CashierPeopleId = userid;
                                        OtpVerification.IsActive = true;
                                        OtpVerification.IsDeleted = false;
                                        OtpVerification.ModifiedDate = DateTime.Now;
                                        OtpVerification.ModifiedBy = userid;
                                        context.Entry(OtpVerification).State = EntityState.Modified;
                                        //context.Commit();
                                        CashManagementHelper cashManagementHelper = new CashManagementHelper();
                                        bool islastmileapp = false;
                                        var res = cashManagementHelper.PaymentSubmittedAssignmentHelper(DeliveryBoyID, AssignmentID, "", context, out islastmileapp);
                                        if (context.Commit() > 0 && res.Status)
                                        {
                                            scope.Complete();

                                            if (!string.IsNullOrEmpty(people.FcmId))
                                            {
                                                BaseAppConfiguration bsa = new BaseAppConfiguration();
                                                string msg = "Submit Code Sucessfully!";
                                                string FCMKey = "";
                                                if (islastmileapp)
                                                {
                                                    FCMKey = ConfigurationManager.AppSettings["DeliveryFcmApiKey"];
                                                }
                                                else
                                                {
                                                    FCMKey = ConfigurationManager.AppSettings["OldDeliveryFcmApiKey"];
                                                }
                                                bsa.CashMgSendNotificationForApproval(people.FcmId, msg, FCMKey, AssignmentID);
                                            }
                                            response = new UploadAssignment()
                                            {

                                                Message = "Submit Code Sucessfully!!",
                                                Status = true
                                            };
                                            return response;
                                        }
                                        else
                                        {
                                            response = new UploadAssignment()
                                            {

                                                Message = "Data not Verified",
                                                Status = false
                                            };

                                            scope.Dispose();

                                            return response;
                                        }
                                    }
                                    else
                                    {
                                        response = new UploadAssignment()
                                        {
                                            Message = "Invalid OTP",
                                            Status = false
                                        };
                                        scope.Dispose();
                                        return response;
                                    }
                                }
                                else
                                {
                                    response = new UploadAssignment()
                                    {
                                        Message = "Time Out",
                                        Status = false
                                    };
                                    scope.Dispose();
                                    return response;
                                }

                            }
                            else
                            {
                                response = new UploadAssignment()
                                {
                                    Message = "Invalid OTP",
                                    Status = false
                                };
                                scope.Dispose();
                                return response;
                            }
                        }
                    }
                }
                else
                {
                    response = new UploadAssignment()
                    {
                        Message = "You are not authorize to subimt OTP !!",
                        Status = false
                    };
                    return response;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in GenerateOTPForCurrency Method: " + ex.Message);
            }
            return response;
        }



        [HttpGet]
        [Route("CMSPageAccess")]
        public CMSPageAccessResultDc CMSPageAccess(int PageNumber, bool action)
        {
            string result = "", Msg = "";
            CMSPageAccessResultDc response = null;
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0, warehouseid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    warehouseid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                using (AuthContext context = new AuthContext())
                {
                    var PageAccessList = context.CMSPageAccessDB.FirstOrDefault(x => x.WarehouseId == warehouseid && x.PageName == PageNumber && x.AccessEndTime == null);

                    if (PageAccessList != null)
                    {
                        if (PageAccessList.UserID == userid)
                        {
                            if (action && PageAccessList.AccessStartTime != null && PageAccessList.AccessEndTime == null)
                            {
                                PageAccessList.AccessEndTime = DateTime.Now;
                                context.Entry(PageAccessList).State = EntityState.Modified;
                                context.Commit();
                                response = new CMSPageAccessResultDc()
                                {

                                    Message = "Process stop Sucessfully",
                                    Status = true,
                                    ButtonShowStart = true,
                                    ButtonShowStop = false

                                };
                                return response;
                            }
                            else if (!action && PageAccessList.AccessStartTime != null && PageAccessList.AccessEndTime == null)
                            {
                                response = new CMSPageAccessResultDc()
                                {
                                    Message = "Enable Stop button",
                                    Status = true,
                                    ButtonShowStart = false,
                                    ButtonShowStop = true
                                };
                                return response;

                            }
                        }
                        else
                        {
                            response = new CMSPageAccessResultDc()
                            {
                                Message = "process started by another cashier",
                                Status = false,
                                ButtonShowStart = false,
                                ButtonShowStop = false
                            };
                            return response;

                        }

                    }
                    else
                    {
                        if (!action)
                        {
                            response = new CMSPageAccessResultDc()
                            {
                                Message = "Enable Start button",
                                Status = true,
                                ButtonShowStart = true,
                                ButtonShowStop = false
                            };
                            return response;
                        }
                        else
                        {
                            CMSPageAccess CMS = new CMSPageAccess();
                            CMS.PageName = PageNumber;
                            CMS.WarehouseId = warehouseid;
                            CMS.UserID = userid;
                            CMS.AccessStartTime = DateTime.Now;
                            CMS.AccessEndTime = null;
                            context.CMSPageAccessDB.Add(CMS);
                            if (context.Commit() > 0)
                            {
                                response = new CMSPageAccessResultDc()
                                {
                                    Message = "Process start Sucessfully",
                                    Status = true,
                                    ButtonShowStart = false,
                                    ButtonShowStop = true
                                };
                                return response;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //logger.Error("Error in GenerateOTPForCurrency Method: " + ex.Message);               
            }
            return response;
        }

        [Route("MTDCollection")]
        [HttpPost]
        public HubCurrencyCollectionANDMTDCollectionDC MTDCollection(MTD MTDcollection)
        {
            //List<HubCurrencyCollectionDc> hubCurrencyCollectionDcs = new List<HubCurrencyCollectionDc>();
            try
            {
                HubCurrencyCollectionANDMTDCollectionDC hubMTD = new HubCurrencyCollectionANDMTDCollectionDC();
                using (AuthContext context = new AuthContext())
                {

                    var month = DateTime.Now.Month;
                    var year = DateTime.Now.Year;
                    var DATE = DateTime.Now.Date;
                    string status = "Settlement";
                    // hubCurrencyCollectionDcs = context.CurrencyHubStock.Where(x => x.Warehouseid== Warehouseid && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(dateFilter)).OrderByDescending(x => x.CreatedDate).Select(x =>
                    var warids = context.Warehouses.Where(x => MTDcollection.warehouseIds.Contains(x.WarehouseId) && x.active && !x.Deleted).Select(x => x.WarehouseId).Distinct().ToList();
                    if (MTDcollection.Fromdate != null && MTDcollection.Todate != null)
                    {
                        hubMTD.hubCurrencyCollectionDcs = context.CurrencyHubStock.Where(x => warids.Contains(x.Warehouseid) && EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(MTDcollection.Fromdate) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(MTDcollection.Todate)).GroupBy(x => x.Warehouseid).Select(x =>
                                   new HubCurrencyCollectionDc
                                   {

                                       TotalBankCashAmt = context.CurrencySettlementSource.Where(z => z.Warehouseid == x.Key && EntityFunctions.TruncateTime(z.CreatedDate) >= EntityFunctions.TruncateTime(MTDcollection.Fromdate) && EntityFunctions.TruncateTime(z.CreatedDate) <= EntityFunctions.TruncateTime(MTDcollection.Todate)).Sum(z => z.TotalCashAmt) > 0 ? context.CurrencySettlementSource.Where(z => z.Warehouseid == x.Key && EntityFunctions.TruncateTime(z.CreatedDate) >= EntityFunctions.TruncateTime(MTDcollection.Fromdate) && EntityFunctions.TruncateTime(z.CreatedDate) <= EntityFunctions.TruncateTime(MTDcollection.Todate)).Sum(z => z.TotalCashAmt) : 0,
                                       TotalBankChequeAmt = context.CurrencySettlementSource.Where(z => z.Warehouseid == x.Key && EntityFunctions.TruncateTime(z.CreatedDate) >= EntityFunctions.TruncateTime(MTDcollection.Fromdate) && EntityFunctions.TruncateTime(z.CreatedDate) <= EntityFunctions.TruncateTime(MTDcollection.Todate)).Sum(z => z.TotalChequeAmt) > 0 ? context.CurrencySettlementSource.Where(z => z.Warehouseid == x.Key && EntityFunctions.TruncateTime(z.CreatedDate) >= EntityFunctions.TruncateTime(MTDcollection.Fromdate) && EntityFunctions.TruncateTime(z.CreatedDate) <= EntityFunctions.TruncateTime(MTDcollection.Todate)).Sum(z => z.TotalChequeAmt) : 0,


                                       TotalCashAmt = x.Sum(y => y.TotalCashAmt),
                                       TotalCheckAmt = x.Sum(y => y.TotalCheckAmt),
                                       TotalDeliveryissueAmt = x.Sum(y => y.TotalDeliveryissueAmt),
                                       TotalDueAmt = x.Sum(y => y.TotalDueAmt),
                                       TotalOnlineAmt = x.Sum(y => y.TotalOnlineAmt),
                                       Warehouseid = x.Key,
                                       TotalAssignmentCount = context.CurrencyCollection.Count(z => z.Warehouseid == x.Key && EntityFunctions.TruncateTime(z.CreatedDate) >= EntityFunctions.TruncateTime(MTDcollection.Fromdate) && EntityFunctions.TruncateTime(z.CreatedDate) <= EntityFunctions.TruncateTime(MTDcollection.Todate) && z.Status == status)

                                   }).ToList();
                        //TotalAmountDC totalAmt = new TotalAmountDC();
                        var CurrencySettlementSource = context.CurrencySettlementSource.Where(x => warids.Contains(x.Warehouseid) && EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(MTDcollection.Fromdate) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(MTDcollection.Todate)).Select(x => new { x.TotalCashAmt, x.TotalChequeAmt }).ToList();
                        hubMTD.TotalBankCashAmt = CurrencySettlementSource != null ? CurrencySettlementSource.Sum(x => x.TotalCashAmt) : 0;
                        hubMTD.TotalBankChequeAmt = CurrencySettlementSource != null ? CurrencySettlementSource.Sum(x => x.TotalChequeAmt) : 0;
                        //hubMTD.TotalBankCashAmt = context.CurrencySettlementSource.Where(x => warids.Contains(x.Warehouseid) && EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(MTDcollection.Fromdate) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(MTDcollection.Todate)).Sum(x => x.TotalCashAmt)> 0 ? context.CurrencySettlementSource.Where(x => warids.Contains(x.Warehouseid) && EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(MTDcollection.Fromdate) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(MTDcollection.Todate)).Sum(x => x.TotalCashAmt):0;
                        //hubMTD.TotalBankChequeAmt = context.CurrencySettlementSource.Where(x => warids.Contains(x.Warehouseid) && EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(MTDcollection.Fromdate) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(MTDcollection.Todate)).Sum(x => x.TotalChequeAmt) >0 ? context.CurrencySettlementSource.Where(x => warids.Contains(x.Warehouseid) && EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(MTDcollection.Fromdate) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(MTDcollection.Todate)).Sum(x => x.TotalChequeAmt) :0;
                        hubMTD.TotalCollections = hubMTD.hubCurrencyCollectionDcs.Sum(x => x.TotalCashAmt + x.TotalCheckAmt + x.TotalOnlineAmt);
                        //return hubMTD;


                    }
                    else
                    {

                        hubMTD.hubCurrencyCollectionDcs = context.CurrencyHubStock.Where(x => warids.Contains(x.Warehouseid) && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(DateTime.Now)).Select(x =>
                                  new HubCurrencyCollectionDc
                                  {
                                      BOD = x.BOD,
                                      CreatedBy = x.CreatedBy,
                                      CreatedDate = x.CreatedDate,
                                      DueResion = x.DueResion,

                                      EOD = x.EOD,
                                      Id = x.Id,
                                      IsActive = x.IsActive,
                                      IsDeleted = x.IsDeleted,
                                      ModifiedBy = x.ModifiedBy,
                                      ModifiedDate = x.ModifiedDate,
                                      Status = x.Status,
                                      //TotalAssignmentCount = x.CashCollections.Any() ? x.CashCollections.GroupBy(y => y.CurrencyCollection.Deliveryissueid).Count() : x.ChequeCollections.Any() ? x.ChequeCollections.GroupBy(y => y.CurrencyCollection.Deliveryissueid).Count() : 0,


                                      TotalBankCashAmt = x.CurrencySettlementSources.Any() ? x.CurrencySettlementSources.Sum(y => y.TotalCashAmt) : 0,
                                      TotalBankChequeAmt = x.CurrencySettlementSources.Any() ? x.CurrencySettlementSources.Sum(y => y.TotalChequeAmt) : 0,
                                      TotalBankDepositDate = x.CurrencySettlementSources.Any() ? x.CurrencySettlementSources.FirstOrDefault().SettlementDate : (DateTime?)null,
                                      TotalCashAmt = x.TotalCashAmt,
                                      TotalCheckAmt = x.TotalCheckAmt,
                                      TotalDeliveryissueAmt = x.TotalDeliveryissueAmt,
                                      TotalDueAmt = x.TotalDueAmt,
                                      TotalOnlineAmt = x.TotalOnlineAmt,
                                      Warehouseid = x.Warehouseid,
                                      TotalAssignmentCount = context.CurrencyCollection.Count(z => z.Warehouseid == x.Warehouseid && z.CreatedDate.Month == month && z.CreatedDate.Year == year && z.Status == status)

                                  }).ToList();
                        MTDCollectionDC MTD = new MTDCollectionDC();

                        MTD.TodaybankCashDeposit = (double)hubMTD.hubCurrencyCollectionDcs.Sum(x => x.TotalBankCashAmt);
                        MTD.TodaybankChequeDeposit = (double)hubMTD.hubCurrencyCollectionDcs.Sum(x => x.TotalBankChequeAmt);
                        MTD.TodayCollection = (double)hubMTD.hubCurrencyCollectionDcs.Sum(x => x.TotalCashAmt + x.TotalCheckAmt + x.TotalOnlineAmt);

                        var MonthTillData = context.CurrencyHubStock.Where(x => warids.Contains(x.Warehouseid) && x.CreatedDate.Month == month && x.CreatedDate.Year == year).ToList();

                        foreach (var item in hubMTD.hubCurrencyCollectionDcs)
                        {
                            item.TotalCashAmt = MonthTillData.Where(y => y.Warehouseid == item.Warehouseid).Sum(y => y.TotalCashAmt);
                            item.TotalCheckAmt = MonthTillData.Where(y => y.Warehouseid == item.Warehouseid).Sum(y => y.TotalCheckAmt);
                            item.TotalOnlineAmt = MonthTillData.Where(y => y.Warehouseid == item.Warehouseid).Sum(y => y.TotalOnlineAmt);

                            //item.TotalBankCashAmt =  context.CurrencySettlementSource.Where(y => y.Warehouseid == item.Warehouseid && y.CreatedDate.Month  == month && y.CreatedDate.Year == year).Sum(y => y.TotalCashAmt);
                        }

                        var CurrencySettlementSource = context.CurrencySettlementSource.Where(x => warids.Contains(x.Warehouseid) && x.CreatedDate.Month == month && x.CreatedDate.Year == year).ToList();//.Select(x => new { x.TotalCashAmt, x.TotalChequeAmt }).ToList();

                        foreach (var data in hubMTD.hubCurrencyCollectionDcs)
                        {
                            data.TotalBankChequeAmt = CurrencySettlementSource.Where(z => z.Warehouseid == data.Warehouseid).Sum(z => z.TotalChequeAmt);
                            data.TotalBankCashAmt = CurrencySettlementSource.Where(z => z.Warehouseid == data.Warehouseid).Sum(z => z.TotalCashAmt);
                        }


                        //hubMTD.TotalBankCashAmt = CurrencySettlementSource != null ? CurrencySettlementSource.Sum(x => x.TotalCashAmt) : 0;
                        //hubMTD.TotalBankChequeAmt = CurrencySettlementSource != null ? CurrencySettlementSource.Sum(x => x.TotalChequeAmt) : 0;

                        MTD.MTDTotalBankCashDeposit = CurrencySettlementSource != null ? CurrencySettlementSource.Sum(x => x.TotalCashAmt) : 0;
                        MTD.MTDTotalBankChequeDeposit = CurrencySettlementSource != null ? CurrencySettlementSource.Sum(x => x.TotalChequeAmt) : 0;
                        MTD.MTDTotalCollection = (double)MonthTillData.Sum(x => x.TotalCashAmt + x.TotalCheckAmt + x.TotalOnlineAmt);

                        //MTD.MTDTotalBankCashDeposit = (double)context.CurrencySettlementSource.Where(x => warids.Contains(x.Warehouseid) && x.CreatedDate.Month == month && x.CreatedDate.Year == year).Sum(x => x.TotalCashAmt);
                        //MTD.MTDTotalBankChequeDeposit = (double)context.CurrencySettlementSource.Where(x => warids.Contains(x.Warehouseid) && x.CreatedDate.Month == month && x.CreatedDate.Year == year).Sum(x => x.TotalChequeAmt);
                        hubMTD.mtDCollectionDC = MTD;
                    }


                    if (hubMTD.hubCurrencyCollectionDcs != null && hubMTD.hubCurrencyCollectionDcs.Any())
                    {
                        var warehouse = context.Warehouses.Select(x => new { x.WarehouseId, WarehouseName = x.WarehouseName + "" + x.CityName }).ToList();
                        if (warehouse != null)
                        {
                            foreach (var item in hubMTD.hubCurrencyCollectionDcs)
                            {
                                item.WarehouseName = warehouse.FirstOrDefault(x => x.WarehouseId == item.Warehouseid).WarehouseName;
                                //item.TotalAssignmentCount = context.CurrencyCollection.Count(x => x.Warehouseid == item.Warehouseid && x.CreatedDate.Month == month && x.CreatedDate.Year == year);
                            }
                        }
                    }

                }

                return hubMTD;
            }
            catch (Exception ex)
            {
                // Logger.Error("Error in GetHubCurrencyCollection Method: " + ex.Message);
                return null;
            }
        }

        [Route("CMSCashierVerification")]
        [HttpGet]
        public CMSCashierVerificationDC CMSCashierVerification(int warehouseid, bool action, int CashierPeopleId, double TotalAmtCash, double TotalAmtcheque, DateTime dateTime, string buttonClickName)
        {
            string result = "", Msg = "";
            CMSCashierVerificationDC response = null;
            try
            {
                int userid = 0;
                userid = CashierPeopleId;

                using (AuthContext context = new AuthContext())
                {
                    //---------S---------------------------
                    var CMSCashierVerificationList = context.CMSCashierVerificationDB.FirstOrDefault(x => x.WarehouseId == warehouseid && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(dateTime) && x.VerifiePeopleID == 0);

                    if (CMSCashierVerificationList != null)
                    {
                        if (CMSCashierVerificationList.RequestPeopleID == userid && CMSCashierVerificationList.AcceptPeopleID == 0 && CMSCashierVerificationList.VerifiePeopleID == 0)
                        {
                            response = new CMSCashierVerificationDC()
                            {
                                Message = "Request Send",
                                Status = false
                            };
                        }
                        else if (CMSCashierVerificationList.RequestPeopleID != userid && CMSCashierVerificationList.AcceptPeopleID == 0 && CMSCashierVerificationList.VerifiePeopleID == 0)
                        {
                            response = new CMSCashierVerificationDC()
                            {
                                Message = "AcceptBtn",
                                Status = false
                            };

                            if (action && buttonClickName == "AcceptBtn")
                            {

                                var currentrquestAmount = context.CMSCashierVerificationDB.Where(x => x.WarehouseId == warehouseid && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(dateTime) && x.VerifiePeopleID != 0).Select(x => new { x.TotalAmtCash, x.TotalAmtcheque }).ToList(); ;
                                double submittedCashAmount = currentrquestAmount != null && currentrquestAmount.Any() ? currentrquestAmount.Sum(x => x.TotalAmtCash) : 0;
                                double submittedChequeAmount = currentrquestAmount != null && currentrquestAmount.Any() ? currentrquestAmount.Sum(x => x.TotalAmtcheque) : 0;
                                var date = DateTime.Now;
                                var TodayHubCurrencyCollection = context.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == warehouseid
                                                                                      && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(date));
                                var chequedetail = context.ChequeCollection.Where(x => x.CurrencyHubStockId == TodayHubCurrencyCollection.Id && EntityFunctions.TruncateTime(x.ModifiedDate) == EntityFunctions.TruncateTime(date) && x.IsActive == true && x.IsDeleted == false).Select(x => x.ChequeAmt).ToList();
                                decimal? chequeAmount = chequedetail != null && chequedetail.Any() ? chequedetail.Sum(x => x) : 0;

                                int? totalAmount = (
                                                from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                                join p in context.CashCollection.Where(o => o.CurrencyHubStockId == TodayHubCurrencyCollection.Id && o.IsActive && o.ModifiedBy == CMSCashierVerificationList.RequestPeopleID
                                                && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue))
                                                on c.Id equals p.CurrencyDenominationId into ps
                                                from p in ps.DefaultIfEmpty()
                                                select new
                                                {
                                                    CurrencyCount = p == null ? 0 : p.CurrencyCountByDBoy,
                                                    CurrencyDenominationValue = c == null ? 0 : c.Value
                                                }).Sum(p => p.CurrencyCount * p.CurrencyDenominationValue);


                                CMSCashierVerificationList.AcceptPeopleID = userid;
                                CMSCashierVerificationList.TotalAmtCash = (totalAmount.HasValue ? totalAmount.Value : 0) - submittedCashAmount;
                                CMSCashierVerificationList.TotalAmtcheque = Convert.ToDouble(chequeAmount.HasValue ? chequeAmount.Value : 0) - submittedChequeAmount;
                                CMSCashierVerificationList.VerifiePeopleID = 0;
                                context.Entry(CMSCashierVerificationList).State = EntityState.Modified;
                                if (context.Commit() > 0)
                                {

                                    response = new CMSCashierVerificationDC()
                                    {
                                        Message = "VerifyBtn",
                                        Status = false
                                    };
                                }
                            }
                        }
                        else if (CMSCashierVerificationList.RequestPeopleID == userid && CMSCashierVerificationList.AcceptPeopleID != 0 && CMSCashierVerificationList.VerifiePeopleID == 0)
                        {
                            response = new CMSCashierVerificationDC()
                            {
                                Message = "Request Accept ",
                                Status = false
                            };
                        }
                        else if (CMSCashierVerificationList.RequestPeopleID != userid && CMSCashierVerificationList.AcceptPeopleID == userid && CMSCashierVerificationList.VerifiePeopleID == 0)
                        {
                            response = new CMSCashierVerificationDC()
                            {
                                Message = "VerifyBtn",
                                Status = false
                            };
                            if (action && buttonClickName == "VerifyBtn")
                            {
                                CMSCashierVerificationList.VerifiePeopleID = userid;
                                CMSCashierVerificationList.ModifiedBy = userid;
                                CMSCashierVerificationList.ModifiedDate = dateTime;
                                //CMSCashierVerificationList.TotalAmtCash = TotalAmtCash > 0 ? TotalAmtCash : 0;
                                //CMSCashierVerificationList.TotalAmtcheque = TotalAmtcheque > 0 ? TotalAmtcheque : 0;
                                CMSCashierVerificationList.IsActive = true;
                                CMSCashierVerificationList.IsDeleted = false;
                                context.Entry(CMSCashierVerificationList).State = EntityState.Modified;
                                if (context.Commit() > 0)
                                {
                                    response = new CMSCashierVerificationDC()
                                    {
                                        Message = "Verified",
                                        Status = false
                                    };
                                }
                            }
                        }

                        return response;

                    }
                    else
                    {
                        response = new CMSCashierVerificationDC()
                        {
                            Message = "RequestBtn",
                            Status = false
                        };

                        if (action && buttonClickName == "RequestBtn")
                        {
                            CMSCashierVerification CMS = new CMSCashierVerification();
                            CMS.WarehouseId = warehouseid;
                            CMS.CashierID = userid;
                            CMS.RequestPeopleID = userid;
                            CMS.AcceptPeopleID = 0;
                            CMS.VerifiePeopleID = 0;
                            CMS.CreatedBy = userid;
                            CMS.CreatedDate = dateTime;

                            //CMS.TotalAmtCash = TotalAmtCash > 0 ? TotalAmtCash : 0;
                            //CMS.TotalAmtcheque = TotalAmtcheque > 0 ? TotalAmtcheque : 0;

                            context.CMSCashierVerificationDB.Add(CMS);
                            if (context.Commit() > 0)
                            {
                                response = new CMSCashierVerificationDC()
                                {
                                    Message = "Request Send",
                                    Status = false
                                };
                            }
                        }

                        return response;
                    }

                    /*
                                        if (CMSCashierVerificationList != null)
                                        {




                                            if (CMSCashierVerificationList.AcceptPeopleID == 0 && CMSCashierVerificationList.VerifiePeopleID == 0)
                                            {
                                                if (!action)
                                                {
                                                    if (CMSCashierVerificationList.CashierID == userid)
                                                    {
                                                        if (CMSCashierVerificationList.RequestPeopleID != 0)
                                                        {
                                                            response = new CMSCashierVerificationDC()
                                                            {
                                                                Message = "Request already done",
                                                                Status = false,
                                                                ButtonShowRequestPeople = false,
                                                                ButtonShowAcceptPeople = false,
                                                                ButtonShowVerifiePeople = false
                                                            };
                                                            return response;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        response = new CMSCashierVerificationDC()
                                                        {
                                                            Message = "Enable Accept People button",
                                                            Status = false,
                                                            ButtonShowRequestPeople = false,
                                                            ButtonShowAcceptPeople = true,
                                                            ButtonShowVerifiePeople = false
                                                        };
                                                        return response;
                                                    }
                                                }
                                                else if (action)
                                                {
                                                    CMSCashierVerificationList.AcceptPeopleID = userid;
                                                    CMSCashierVerificationList.VerifiePeopleID = 0;
                                                    context.Entry(CMSCashierVerificationList).State = EntityState.Modified;
                                                    if (context.Commit() > 0)
                                                    {
                                                        response = new CMSCashierVerificationDC()
                                                        {
                                                            Message = "Process Accept Sucessfully",
                                                            Status = false,
                                                            ButtonShowRequestPeople = false,
                                                            ButtonShowAcceptPeople = false,
                                                            ButtonShowVerifiePeople = true
                                                        };
                                                        return response;
                                                    }
                                                }
                                            }
                                            else if (CMSCashierVerificationList.AcceptPeopleID != 0 && CMSCashierVerificationList.VerifiePeopleID == 0)
                                            {
                                                if (!action)
                                                {
                                                    if (CMSCashierVerificationList.CashierID != userid && CMSCashierVerificationList.AcceptPeopleID == userid)
                                                    {
                                                        if (CMSCashierVerificationList.RequestPeopleID != 0)
                                                        {
                                                            response = new CMSCashierVerificationDC()
                                                            {
                                                                Message = "Enable Verifie People button",
                                                                Status = false,
                                                                ButtonShowRequestPeople = false,
                                                                ButtonShowAcceptPeople = false,
                                                                ButtonShowVerifiePeople = true
                                                            };
                                                            return response;
                                                        }
                                                    }
                                                }
                                                else if (action)
                                                {
                                                    CMSCashierVerificationList.VerifiePeopleID = userid;
                                                    CMSCashierVerificationList.ModifiedBy = userid;
                                                    CMSCashierVerificationList.ModifiedDate = DateTime.Now;
                                                    CMSCashierVerificationList.TotalAmtCash = TotalAmtCash > 0 ? TotalAmtCash : 0;
                                                    CMSCashierVerificationList.TotalAmtcheque = TotalAmtcheque > 0 ? TotalAmtcheque : 0;
                                                    CMSCashierVerificationList.IsActive = true;
                                                    CMSCashierVerificationList.IsDeleted = false;
                                                    context.Entry(CMSCashierVerificationList).State = EntityState.Modified;
                                                    if (context.Commit() > 0)
                                                    {
                                                        response = new CMSCashierVerificationDC()
                                                        {
                                                            Message = "Process Verifie sucessfully",
                                                            Status = true,
                                                            ButtonShowRequestPeople = false,
                                                            ButtonShowAcceptPeople = false,
                                                            ButtonShowVerifiePeople = false
                                                        };
                                                        return response;
                                                    }
                                                }
                                            }
                                            else if (CMSCashierVerificationList.AcceptPeopleID != 0 && CMSCashierVerificationList.VerifiePeopleID != 0)
                                            {
                                                //if (CMSCashierVerificationList.VerifiePeopleID == userid)
                                                {
                                                    response = new CMSCashierVerificationDC()
                                                    {
                                                        Message = "Process done by all cashier!!",
                                                        Status = false,
                                                        ButtonShowRequestPeople = false,
                                                        ButtonShowAcceptPeople = false,
                                                        ButtonShowVerifiePeople = false
                                                    };
                                                    return response;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (!action)
                                            {
                                                response = new CMSCashierVerificationDC()
                                                {
                                                    Message = "Enable Request People button",
                                                    Status = false,
                                                    ButtonShowRequestPeople = true,
                                                    ButtonShowAcceptPeople = false,
                                                    ButtonShowVerifiePeople = false
                                                };
                                                return response;
                                            }
                                            else if (action)
                                            {
                                                //if (CMSCashierVerificationList.CashierID == userid)
                                                {
                                                    CMSCashierVerification CMS = new CMSCashierVerification();
                                                    CMS.WarehouseId = warehouseid;
                                                    CMS.CashierID = userid;
                                                    CMS.RequestPeopleID = userid;
                                                    CMS.AcceptPeopleID = 0;
                                                    CMS.VerifiePeopleID = 0;
                                                    CMS.CreatedBy = userid;
                                                    CMS.CreatedDate = DateTime.Now;

                                                    //CMS.TotalAmtCash = TotalAmtCash > 0 ? TotalAmtCash : 0;
                                                    //CMS.TotalAmtcheque = TotalAmtcheque > 0 ? TotalAmtcheque : 0;

                                                    context.CMSCashierVerificationDB.Add(CMS);
                                                    if (context.Commit() > 0)
                                                    {
                                                        response = new CMSCashierVerificationDC()
                                                        {
                                                            Message = "Process Request Sucessfully",
                                                            Status = false,
                                                            ButtonShowRequestPeople = false,
                                                            ButtonShowAcceptPeople = false,
                                                            ButtonShowVerifiePeople = false
                                                        };
                                                        return response;
                                                    }
                                                }
                                            }
                                        }
                                        //---------E---------------------------

                                    }

                    */

                }
            }
            catch (Exception ex)
            {
                // logger.Error("Error in CMSCashierVerification Method: " + ex.Message);               
            }
            return response;
        }


        [Route("OTPVerificationPopupCLoseById")]
        [HttpGet]
        public string OTPVerificationPopupCLosedataById(int AssignmentId)
        {
            string result = "";
            OTPVerification response = new OTPVerification();
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                using (AuthContext context = new AuthContext())
                {
                    response = context.OTPVerificationDb.Where(x => x.AssignmentID == AssignmentId && x.IsActive == true && x.IsDeleted == false && x.ModifiedBy > 0).FirstOrDefault();
                    if (response != null)
                    {
                        result = "OTP Verified!";
                    }
                    else
                    {
                        result = "OTP not Verified!";
                    }
                }
            }
            catch (Exception ex)
            {
                //logger.Error("Error in GenerateOTPForCurrency Method: " + ex.Message);               
            }
            return result;
        }


        string col0, col1, col2, col3;
        [Route("StoreCreditLimitUploader")]
        [HttpPost]
        public IHttpActionResult StoreCreditLimitUploader()
        {
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                DateTime cdate = DateTime.Now;
                var formData1 = HttpContext.Current.Request.Form["compid"];
                // Access claims
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                string MSG = "";
                int count = 0;
                int duplicatecount = 0;
                if (httpPostedFile != null)
                {
                    using (AuthContext context = new AuthContext())
                    {
                        string ext = Path.GetExtension(httpPostedFile.FileName);
                        if (ext == ".xlsx" || ext == ".xls")
                        {
                            string path = HttpContext.Current.Server.MapPath("~/UploadedFiles/StoreCreditLimit");
                            string a1, b;

                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                            a1 = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + httpPostedFile.FileName;
                            b = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles/StoreCreditLimit/"), a1);
                            httpPostedFile.SaveAs(b);

                            byte[] buffer = new byte[httpPostedFile.ContentLength];

                            using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
                            {
                                br.Read(buffer, 0, buffer.Length);
                            }
                            XSSFWorkbook hssfwb;
                            List<StoreCreditLimitDC> uploaditemlist = new List<StoreCreditLimitDC>();
                            using (MemoryStream memStream = new MemoryStream())
                            {
                                BinaryFormatter binForm = new BinaryFormatter();
                                memStream.Write(buffer, 0, buffer.Length);
                                memStream.Seek(0, SeekOrigin.Begin);
                                hssfwb = new XSSFWorkbook(memStream);
                                string sSheetName = hssfwb.GetSheetName(0);
                                ISheet sheet = hssfwb.GetSheet(sSheetName);
                                IRow rowData;
                                ICell cellData = null;

                                int? StoreNameIndex = null;
                                int? SkCodeIndex = null;
                                int? CreditLimitIndex = null;
                                int? DueDaysIndex = null;


                                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                                {

                                    if (iRowIdx == 0)
                                    {
                                        rowData = sheet.GetRow(iRowIdx);
                                        if (rowData != null)
                                        {
                                            string strJSON = null;
                                            string field = string.Empty;
                                            field = rowData.GetCell(0).ToString();
                                            //WarehousenameIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Warehouse Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Warehouse Name").ColumnIndex : (int?)null;
                                            //if (!WarehousenameIndex.HasValue)
                                            //{
                                            //    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Warehouse Name does not exist..try again");
                                            //    return Created(strJSON, strJSON);
                                            //}

                                            StoreNameIndex = rowData.Cells.Any(x => x.ToString().Trim() == "StoreName") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "StoreName").ColumnIndex : (int?)null;

                                            if (!StoreNameIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("StoreName does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }

                                            SkCodeIndex = rowData.Cells.Any(x => x.ToString().Trim() == "SkCode") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "SkCode").ColumnIndex : (int?)null;

                                            if (!SkCodeIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("SkCode does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }

                                            CreditLimitIndex = rowData.Cells.Any(x => x.ToString().Trim() == "CreditLimit") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "CreditLimit").ColumnIndex : (int?)null;

                                            if (!CreditLimitIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("CreditLimit does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }

                                            DueDaysIndex = rowData.Cells.Any(x => x.ToString().Trim() == "DueDays") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "DueDays").ColumnIndex : (int?)null;

                                            if (!DueDaysIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("DueDays does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }
                                        }

                                    }
                                    else
                                    {

                                        StoreCreditLimitDC uploadDC = new StoreCreditLimitDC();
                                        rowData = sheet.GetRow(iRowIdx);

                                        if (rowData != null)
                                        {
                                            var c = rowData.GetCell(0);
                                            if (c.RichStringCellValue.Length > 0)
                                            {
                                                cellData = rowData.GetCell(StoreNameIndex.Value);
                                                col0 = cellData == null ? "" : cellData.ToString();
                                                uploadDC.StoreName = Convert.ToString(col0);
                                                logger.Info("Transaction Status :" + uploadDC.StoreName);

                                                string skcode = "";
                                                cellData = rowData.GetCell(SkCodeIndex.Value);
                                                col1 = cellData == null ? "" : cellData.ToString();
                                                skcode = Convert.ToString(col1);
                                                uploadDC.SkCode = skcode;
                                                logger.Info("Transaction Date :" + uploadDC.SkCode);

                                                cellData = rowData.GetCell(CreditLimitIndex.Value);
                                                col2 = cellData == null ? "" : cellData.ToString();
                                                uploadDC.CreditLimit = Convert.ToDouble(col2);
                                                logger.Info("ReferenceNumber :" + uploadDC.CreditLimit);

                                                cellData = rowData.GetCell(DueDaysIndex.Value);
                                                col3 = cellData == null ? "" : cellData.ToString();
                                                if (string.IsNullOrEmpty(col3))
                                                {
                                                    uploadDC.DueDays = 0;
                                                }
                                                else
                                                {
                                                    uploadDC.DueDays = Convert.ToInt16(col3);
                                                }

                                                logger.Info("ReferenceNumber :" + uploadDC.DueDays);

                                                uploaditemlist.Add(uploadDC);
                                            }
                                        }
                                    }
                                }
                            }

                            if (uploaditemlist != null && uploaditemlist.Any())
                            {
                                List<string> storenames = uploaditemlist.Select(x => x.StoreName.Trim().ToLower()).Distinct().ToList();
                                List<string> skcodes = uploaditemlist.Select(x => x.SkCode.Trim().ToLower()).Distinct().ToList();
                                List<Customer> customerlist = context.Customers.Where(x => skcodes.Contains(x.Skcode.Trim().ToLower())).Distinct().ToList();
                                List<Store> storelist = context.StoreDB.Where(x => storenames.Contains(x.Name.Trim().ToLower()) && x.IsActive == true).Distinct().ToList();
                                //List<StoreCreditLimit> storeCreditLimitslist = new List<StoreCreditLimit>();
                                List<StoreCreditLimit> storeCreditLimitslists = new List<StoreCreditLimit>();
                                if (!storelist.Any(x => storenames.Contains(x.Name.Trim().ToLower())))
                                {
                                    List<string> stnamelist = storelist.Select(x => x.Name.Trim().ToLower()).ToList();
                                    List<string> storenotpresent = storenames.Where(x => !stnamelist.Contains(x)).ToList();
                                    string abc = String.Join(",", storenotpresent);
                                    MSG = abc + " is not Present in our Databse"; return Created(MSG, MSG);
                                }
                                if (!customerlist.Any(x => skcodes.Contains(x.Skcode.Trim().ToLower())))
                                {
                                    List<string> customerlists = customerlist.Select(x => x.Skcode.Trim().ToLower()).ToList();
                                    List<string> skcodenotpresent = skcodes.Where(x => !customerlists.Contains(x)).ToList();
                                    string abc = String.Join(",", skcodenotpresent);
                                    MSG = "Skcode " + abc + " is not Present in our Databse"; return Created(MSG, MSG);
                                }
                                if (customerlist.Any(x => x.Active == false))
                                {
                                    List<string> skcodeinactive = customerlist.Where(x => x.Active == false).Select(y => y.Skcode).ToList();
                                    string abc = String.Join(",", skcodeinactive);
                                    MSG = "Skcode " + abc + " is inactive in our Databse"; return Created(MSG, MSG);
                                }
                                if (uploaditemlist.Any(x => x.CreditLimit <= 0))
                                {
                                    MSG = "Please Enter Credit Limit in Excel"; return Created(MSG, MSG);
                                }
                                string message = string.Empty;
                                string duplicatemessage = string.Empty;

                                List<int> customerids = customerlist.Select(x => x.CustomerId).Distinct().ToList();
                                List<long> storeids = storelist.Select(x => x.Id).Distinct().ToList();
                                var storecreditdata = context.StoreCreditLimitDb.Where(x => customerids.Contains(x.CustomerId) && storeids.Contains(x.StoreId)).ToList();
                                foreach (var a in uploaditemlist)
                                {
                                    bool isadd = true;
                                    long storeid = 0; int Customerid = 0;
                                    storeid = storelist.FirstOrDefault(x => x.Name.Trim().ToLower() == a.StoreName.Trim().ToLower()).Id;
                                    Customerid = customerlist.FirstOrDefault(x => x.Skcode.Trim().ToLower() == a.SkCode.Trim().ToLower()).CustomerId;
                                    var datas = storecreditdata != null && storecreditdata.Any() ? storecreditdata.Where(x => x.CustomerId == Customerid && x.StoreId == storeid).FirstOrDefault() : null;
                                    if (datas == null)
                                    {
                                        if (storeCreditLimitslists != null && storeCreditLimitslists.Any())
                                        {

                                            var ispresent = storeCreditLimitslists.Any(x => x.CustomerId == Customerid && x.StoreId == storeid);
                                            if (ispresent == true)
                                            {
                                                isadd = false;
                                                if (duplicatecount == 0)
                                                {
                                                    duplicatemessage = " SkCode " + a.SkCode + " and Store Name " + a.StoreName;
                                                }
                                                else
                                                {
                                                    duplicatemessage = duplicatemessage + " and" + " SkCode " + a.SkCode + " and Store Name " + a.StoreName;
                                                }
                                                duplicatecount++;
                                            }
                                        }
                                        if (isadd == true)
                                        {
                                            StoreCreditLimit store = new StoreCreditLimit();
                                            store.StoreId = storeid;
                                            store.CustomerId = Customerid;
                                            store.CreditLimit = a.CreditLimit;
                                            store.CreatedBy = userid;
                                            store.CreatedDate = DateTime.Now;
                                            store.IsDeleted = false;
                                            store.IsActive = true;
                                            store.DueDays = a.DueDays;
                                            storeCreditLimitslists.Add(store);
                                        }
                                    }
                                    else
                                    {
                                        if (count == 0)
                                        {
                                            message = " SkCode " + a.SkCode + " and Store Name " + a.StoreName;
                                        }
                                        else
                                        {
                                            message = message + " and" + " SkCode " + a.SkCode + " and Store Name " + a.StoreName;
                                        }

                                        count++;
                                    }
                                }
                                if (duplicatecount > 0)
                                {
                                    MSG = duplicatemessage + " is Duplicate in Your Sheet"; return Created(MSG, MSG);
                                }
                                if (count == 0 && storeCreditLimitslists != null && storeCreditLimitslists.Any())
                                {
                                    context.StoreCreditLimitDb.AddRange(storeCreditLimitslists);
                                    if (context.Commit() > 0)
                                    {
                                        MSG = "Added Successfully"; return Created(MSG, MSG);
                                    }
                                    else
                                    {
                                        MSG = "Something Error Occured"; return Created(MSG, MSG);
                                    }
                                }
                                else
                                {
                                    MSG = message + " is already Present"; return Created(MSG, MSG);
                                }
                            }
                            else


                            {
                                MSG = "Data Not Found in Excel"; return Created(MSG, MSG);
                            }
                        }
                        else
                        {
                            return Created("File extnsion required .xlsx", "File extnsion required .xlsx");
                        }
                    }
                }

                return Created("Error", "Error");
            }
            else
            {
                return Created("Error", "Error");
            }
        }

        [Route("DownloadStoreCreditLimitUploader")]
        [HttpGet]
        public string DownloadConfiguration()
        {
            string fileUrl = "";
            string fileName = "StoreCreditLimitUploaderSampleDownloadFiles" + ".xlsx";
            string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
            if (File.Exists(path))
            {
                fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri(HttpContext.Current.Request.UrlReferrer.AbsoluteUri).Scheme
                                                                , HttpContext.Current.Request.Url.DnsSafeHost
                                                                , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                , string.Format("ExcelGeneratePath/{0}", fileName));
            }
            else
            {
                List<DownloadStoreCreditLimitDC> Result = new List<DownloadStoreCreditLimitDC>();
                DownloadStoreCreditLimitDC mrp = new DownloadStoreCreditLimitDC();
                mrp.StoreName = "Store 1";
                mrp.SkCode = "Sk123";
                mrp.CreditLimit = 1;
                mrp.DueDays = 1;
                Result.Add(mrp);
                DownloadStoreCreditLimitDC unit = new DownloadStoreCreditLimitDC();
                unit.StoreName = "Store 2";
                unit.SkCode = "Sk124";
                unit.CreditLimit = 2;
                unit.DueDays = 0;
                Result.Add(unit);
                DataTable dt = ListtoDataTableConverter.ToDataTable(Result);
                string paths = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                ExcelGenerator.DataTable_To_Excel(dt, "StoreCreditLimitSample", paths);
                fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri(HttpContext.Current.Request.UrlReferrer.AbsoluteUri).Scheme
                                                                , HttpContext.Current.Request.Url.DnsSafeHost
                                                                , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                , string.Format("ExcelGeneratePath/{0}", fileName));
            }
            return fileUrl;
        }

        [HttpGet]
        [Route("GetStoreCreditLimit")]
        [AllowAnonymous]
        public GetStoreCreditLimitDC GetstorecreditLimit(List<int> Orderid)
        {
            using (var context = new AuthContext())
            {
                var orderid = new DataTable();
                orderid.Columns.Add("IntValue");
                foreach (var obj in Orderid)
                {
                    var dr = orderid.NewRow();
                    dr["IntValue"] = obj;
                    orderid.Rows.Add(dr);
                }
                var param = new SqlParameter("@orderid", orderid);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                GetStoreCreditLimitDC res = new GetStoreCreditLimitDC();
                res = context.Database.SqlQuery<GetStoreCreditLimitDC>("exec Sp_GetstorecreditLimit @orderid", param).FirstOrDefault();
                if (res.CustomerId > 0)
                {
                    return res;
                }
                else
                {
                    res = new GetStoreCreditLimitDC();
                }
                return res;
            }
        }
        [HttpGet]
        [Route("GetStoreList")]
        [AllowAnonymous]
        public List<GetCreditStoreListDC> GetStoreList()
        {
            using (var context = new AuthContext())
            {
                List<GetCreditStoreListDC> storelist = new List<GetCreditStoreListDC>();
                //List<GetCreditCustomerListDC> customerlist = new List<GetCreditCustomerListDC>();
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[Sp_GetLimitStores]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandTimeout = 600;
                var reader = cmd.ExecuteReader();
                storelist = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<GetCreditStoreListDC>(reader).ToList();
                context.Database.Connection.Close();
                return storelist;
            }
        }


        [HttpPost]
        [Route("SearchCreditList")]
        public List<GetCreditSearchListDc> SearchCreditList(GetCreditSearchPayload payload)
        {
            using (var db = new AuthContext())
            {
                List<GetCreditSearchListDc> res = new List<GetCreditSearchListDc>();
                var storeids = new DataTable();
                storeids.Columns.Add("IntValue");
                foreach (var obj in payload.StoreId)
                {
                    var dr = storeids.NewRow();
                    dr["IntValue"] = obj;
                    storeids.Rows.Add(dr);
                }
                var param = new SqlParameter("@StoreIds", storeids);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                var param2 = new SqlParameter("@SkCode", payload.SkCode != null ? payload.SkCode : (object)DBNull.Value);
                var param3 = new SqlParameter("@Skip", payload.Skip);
                var param4 = new SqlParameter("@Take", payload.Take);
                res = db.Database.SqlQuery<GetCreditSearchListDc>("exec Sp_GetCreditSearchList @StoreIds,@Skcode,@Skip,@Take", param, param2, param3, param4).ToList();
                return res;
            }
        }

        [HttpGet]
        [Route("EditCreditList")]
        [AllowAnonymous]
        public string EditCreditList(long Id, double Amount, bool IsActive)
        {
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                string res = "";
                var data = db.StoreCreditLimitDb.Where(x => x.Id == Id).FirstOrDefault();
                if (data != null)
                {
                    data.IsActive = IsActive;
                    if (IsActive == true)
                    {
                        data.CreditLimit = Amount;
                    }
                    data.ModifiedDate = DateTime.Now;
                    data.ModifiedBy = userid;
                    db.Entry(data).State = EntityState.Modified;
                    if (db.Commit() > 0)
                    {
                        res = "Updated Successfully";
                    }
                    else
                    {
                        res = "Something Went Wrong";
                    }
                }
                else
                {
                    res = "Data Not Found";
                }
                return res;
            }
        }


        [HttpPost]
        [Route("GetpayLaterCollection")]
        [AllowAnonymous]
        public List<PayLaterCollectionShowDc> GetpayLaterCollection(GetCreditSearchPayload payload)
        {
            using (var context = new AuthContext())
            {
                List<PayLaterCollectionShowDc> result = new List<PayLaterCollectionShowDc>();
                List<PayLaterCollectionHistoryDC> historyresult = new List<PayLaterCollectionHistoryDC>();
                List<PayLaterCollectionReturnOrderDC> returnresult = new List<PayLaterCollectionReturnOrderDC>();   
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var storeid = new DataTable();
                storeid.Columns.Add("IntValue");
                foreach (var obj in payload.StoreId)
                {
                    var dr = storeid.NewRow();
                    dr["IntValue"] = obj;
                    storeid.Rows.Add(dr);
                }
                var storeparam = new SqlParameter("@StoreIds", storeid);
                storeparam.SqlDbType = SqlDbType.Structured;
                storeparam.TypeName = "dbo.IntValues";

                var wareid = new DataTable();
                wareid.Columns.Add("IntValue");
                foreach (var obj in payload.WarehouseId)
                {
                    var dr = wareid.NewRow();
                    dr["IntValue"] = obj;
                    wareid.Rows.Add(dr);
                }
                var wareparam = new SqlParameter("@WarehouseIds", wareid);
                wareparam.SqlDbType = SqlDbType.Structured;
                wareparam.TypeName = "dbo.IntValues";

                var keywordparam = new SqlParameter("@keyword", payload.SkCode != null ? payload.SkCode : (object)DBNull.Value);
                var Skipparam = new SqlParameter("@Skip", payload.Skip);
                var Takeparam = new SqlParameter("@Take", payload.Take);
                var Statusparam = new SqlParameter("@Status", payload.Status);
                var paymentpendingparam = new SqlParameter("@IsPaymentpending", payload.IsPaymentpending == true ? true : false);
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[Sp_GetPayLaterCollection]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(storeparam);
                cmd.Parameters.Add(keywordparam);
                cmd.Parameters.Add(Skipparam);
                cmd.Parameters.Add(Takeparam);
                cmd.Parameters.Add(Statusparam);
                cmd.Parameters.Add(wareparam);
                cmd.Parameters.Add(paymentpendingparam);
                cmd.CommandTimeout = 1200;
                var reader = cmd.ExecuteReader();
                result = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<PayLaterCollectionShowDc>(reader).ToList();
                reader.NextResult();
                historyresult = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<PayLaterCollectionHistoryDC>(reader).ToList();
                reader.NextResult();
                returnresult = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<PayLaterCollectionReturnOrderDC>(reader).ToList();
                context.Database.Connection.Close();
                foreach (var i in result)
                {
                    i.payLaterCollectionHistoryDCs = historyresult.Where(x => x.PayLaterCollectionId == i.Id).ToList();
                    i.payLaterCollectionReturnOrderDCs = returnresult.Where(x => x.OrderId == i.OrderId).ToList();
                }
                return result;
            }
        }

        [HttpGet]
        [Route("GetPayLaterCollectionLimit")]
        public PayLaterCollectionResponse GetPayLaterCollectionLimit(int Customerid)
        {
            using (var db = new AuthContext())
            {
                PayLaterCollectionResponse response = new PayLaterCollectionResponse();
                if (Customerid > 0)
                {
                    List<PayLaterCollectionLimitDC> res = new List<PayLaterCollectionLimitDC>();
                    var param = new SqlParameter("@CustomerId", Customerid);
                    res = db.Database.SqlQuery<PayLaterCollectionLimitDC>("exec Sp_GetCollectionLimitForRetailer @CustomerId", param).ToList();
                    if (res != null && res.Any())
                    {
                        response.Status = true;
                        response.Message = "Data Found";
                        response.payLaterCollectionLimitDCs = res;
                        return response;
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "No Data Found";
                        return response;
                    }
                }
                else
                {
                    response.Status = false;
                    response.Message = "Customerid is Empty";
                    return response;
                }
            }
        }

        [HttpPost]
        [Route("ExportCreditList")]
        public List<GetCreditSearchListDc> ExportCreditList(GetCreditSearchPayload payload)
        {
            using (var db = new AuthContext())
            {
                List<GetCreditSearchListDc> res = new List<GetCreditSearchListDc>();
                var storeids = new DataTable();
                storeids.Columns.Add("IntValue");
                foreach (var obj in payload.StoreId)
                {
                    var dr = storeids.NewRow();
                    dr["IntValue"] = obj;
                    storeids.Rows.Add(dr);
                }
                var param = new SqlParameter("@StoreIds", storeids);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                var param2 = new SqlParameter("@SkCode", payload.SkCode != null ? payload.SkCode : (object)DBNull.Value);
                var param3 = new SqlParameter("@Skip", payload.Skip);
                var param4 = new SqlParameter("@Take", payload.Take);
                res = db.Database.SqlQuery<GetCreditSearchListDc>("exec Sp_ExportCreditSearchList @StoreIds,@Skcode,@Skip,@Take", param, param2, param3, param4).ToList();
                return res;
            }
        }

        [HttpPost]
        [Route("ExportPayLaterData")]
        public List<PayLaterCollectionShowDc> ExportPayLaterData(GetCreditSearchPayload payload)
        {
            using (var context = new AuthContext())
            {
                List<PayLaterCollectionShowDc> result = new List<PayLaterCollectionShowDc>();
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var storeid = new DataTable();
                storeid.Columns.Add("IntValue");
                foreach (var obj in payload.StoreId)
                {
                    var dr = storeid.NewRow();
                    dr["IntValue"] = obj;
                    storeid.Rows.Add(dr);
                }
                var storeparam = new SqlParameter("@StoreIds", storeid);
                storeparam.SqlDbType = SqlDbType.Structured;
                storeparam.TypeName = "dbo.IntValues";

                var wareid = new DataTable();
                wareid.Columns.Add("IntValue");
                foreach (var obj in payload.WarehouseId)
                {
                    var dr = wareid.NewRow();
                    dr["IntValue"] = obj;
                    wareid.Rows.Add(dr);
                }
                var wareparam = new SqlParameter("@WarehouseIds", wareid);
                wareparam.SqlDbType = SqlDbType.Structured;
                wareparam.TypeName = "dbo.IntValues";

                var keywordparam = new SqlParameter("@keyword", payload.SkCode != null ? payload.SkCode : (object)DBNull.Value);
                var Skipparam = new SqlParameter("@Skip", payload.Skip);
                var Takeparam = new SqlParameter("@Take", payload.Take);
                var Statusparam = new SqlParameter("@Status", payload.Status);
                var paymentpendingparam = new SqlParameter("@IsPaymentpending", payload.IsPaymentpending == true ? true : false);
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[Sp_GetPayLaterCollectionExport]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(storeparam);
                cmd.Parameters.Add(keywordparam);
                cmd.Parameters.Add(Skipparam);
                cmd.Parameters.Add(Takeparam);
                cmd.Parameters.Add(Statusparam);
                cmd.Parameters.Add(wareparam);
                cmd.Parameters.Add(paymentpendingparam);
                cmd.CommandTimeout = 1200;
                var reader = cmd.ExecuteReader();
                result = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<PayLaterCollectionShowDc>(reader).ToList();
                return result;
            }
        }

        //string col0, col1, col2, col3;
        [Route("RevisedStoreCreditLimitUploader")]
        [HttpPost]
        public IHttpActionResult RevisedStoreCreditLimitUploader()
        {
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                DateTime cdate = DateTime.Now;
                var formData1 = HttpContext.Current.Request.Form["compid"];
                // Access claims
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                string MSG = "";
                int newcount = 0;
                int duplicatecount = 0;
                int revisedcount = 0;
                int creditlimitcount = 0;
                string creditlimitmessage = string.Empty;
                if (httpPostedFile != null)
                {
                    using (AuthContext context = new AuthContext())
                    {
                        string ext = Path.GetExtension(httpPostedFile.FileName);
                        if (ext == ".xlsx" || ext == ".xls")
                        {
                            string path = HttpContext.Current.Server.MapPath("~/UploadedFiles/StoreCreditLimit");
                            string a1, b;

                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                            a1 = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + httpPostedFile.FileName;
                            b = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles/StoreCreditLimit/"), a1);
                            httpPostedFile.SaveAs(b);

                            byte[] buffer = new byte[httpPostedFile.ContentLength];

                            using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
                            {
                                br.Read(buffer, 0, buffer.Length);
                            }
                            XSSFWorkbook hssfwb;
                            List<StoreCreditLimitDC> uploaditemlist = new List<StoreCreditLimitDC>();
                            using (MemoryStream memStream = new MemoryStream())
                            {
                                BinaryFormatter binForm = new BinaryFormatter();
                                memStream.Write(buffer, 0, buffer.Length);
                                memStream.Seek(0, SeekOrigin.Begin);
                                hssfwb = new XSSFWorkbook(memStream);
                                string sSheetName = hssfwb.GetSheetName(0);
                                ISheet sheet = hssfwb.GetSheet(sSheetName);
                                IRow rowData;
                                ICell cellData = null;

                                int? StoreNameIndex = null;
                                int? SkCodeIndex = null;
                                int? CreditLimitIndex = null;
                                int? DueDaysIndex = null;


                                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                                {

                                    if (iRowIdx == 0)
                                    {
                                        rowData = sheet.GetRow(iRowIdx);
                                        if (rowData != null)
                                        {
                                            string strJSON = null;
                                            string field = string.Empty;
                                            field = rowData.GetCell(0).ToString();
                                            //WarehousenameIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Warehouse Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Warehouse Name").ColumnIndex : (int?)null;
                                            //if (!WarehousenameIndex.HasValue)
                                            //{
                                            //    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Warehouse Name does not exist..try again");
                                            //    return Created(strJSON, strJSON);
                                            //}

                                            StoreNameIndex = rowData.Cells.Any(x => x.ToString().Trim() == "StoreName") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "StoreName").ColumnIndex : (int?)null;

                                            if (!StoreNameIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("StoreName does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }

                                            SkCodeIndex = rowData.Cells.Any(x => x.ToString().Trim() == "SkCode") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "SkCode").ColumnIndex : (int?)null;

                                            if (!SkCodeIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("SkCode does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }

                                            CreditLimitIndex = rowData.Cells.Any(x => x.ToString().Trim() == "CreditLimit") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "CreditLimit").ColumnIndex : (int?)null;

                                            if (!CreditLimitIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("CreditLimit does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }

                                            DueDaysIndex = rowData.Cells.Any(x => x.ToString().Trim() == "DueDays") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "DueDays").ColumnIndex : (int?)null;

                                            if (!DueDaysIndex.HasValue)
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("DueDays does not exist..try again");
                                                return Created(strJSON, strJSON);
                                            }
                                        }

                                    }
                                    else
                                    {

                                        StoreCreditLimitDC uploadDC = new StoreCreditLimitDC();
                                        rowData = sheet.GetRow(iRowIdx);

                                        if (rowData != null)
                                        {
                                            var c = rowData.GetCell(0);
                                            if (c.RichStringCellValue.Length > 0)
                                            {
                                                cellData = rowData.GetCell(StoreNameIndex.Value);
                                                col0 = cellData == null ? "" : cellData.ToString();
                                                uploadDC.StoreName = Convert.ToString(col0);
                                                logger.Info("Transaction Status :" + uploadDC.StoreName);

                                                string skcode = "";
                                                cellData = rowData.GetCell(SkCodeIndex.Value);
                                                col1 = cellData == null ? "" : cellData.ToString();
                                                skcode = Convert.ToString(col1);
                                                uploadDC.SkCode = skcode;
                                                logger.Info("Transaction Date :" + uploadDC.SkCode);

                                                cellData = rowData.GetCell(CreditLimitIndex.Value);
                                                col2 = cellData == null ? "" : cellData.ToString();
                                                uploadDC.CreditLimit = Convert.ToDouble(col2);
                                                logger.Info("ReferenceNumber :" + uploadDC.CreditLimit);

                                                cellData = rowData.GetCell(DueDaysIndex.Value);
                                                col3 = cellData == null ? "" : cellData.ToString();
                                                if (string.IsNullOrEmpty(col3))
                                                {
                                                    uploadDC.DueDays = 0;
                                                }
                                                else
                                                {
                                                    uploadDC.DueDays = Convert.ToInt16(col3);
                                                }

                                                logger.Info("ReferenceNumber :" + uploadDC.DueDays);

                                                uploaditemlist.Add(uploadDC);
                                            }
                                        }
                                    }
                                }
                            }

                            if (uploaditemlist != null && uploaditemlist.Any())
                            {
                                List<string> storenames = uploaditemlist.Select(x => x.StoreName.Trim().ToLower()).Distinct().ToList();
                                List<string> skcodes = uploaditemlist.Select(x => x.SkCode.Trim().ToLower()).Distinct().ToList();
                                List<Customer> customerlist = context.Customers.Where(x => skcodes.Contains(x.Skcode.Trim().ToLower())).Distinct().ToList();
                                List<Store> storelist = context.StoreDB.Where(x => storenames.Contains(x.Name.Trim().ToLower()) && x.IsActive == true).Distinct().ToList();
                                //List<StoreCreditLimit> storeCreditLimitslist = new List<StoreCreditLimit>();
                                List<StoreCreditLimit> storeCreditLimitslists = new List<StoreCreditLimit>();
                                if (!storelist.Any(x => storenames.Contains(x.Name.Trim().ToLower())))
                                {
                                    List<string> stnamelist = storelist.Select(x => x.Name.Trim().ToLower()).ToList();
                                    List<string> storenotpresent = storenames.Where(x => !stnamelist.Contains(x)).ToList();
                                    string abc = String.Join(",", storenotpresent);
                                    MSG = abc + " is not Present in our Databse"; return Created(MSG, MSG);
                                }
                                if (!customerlist.Any(x => skcodes.Contains(x.Skcode.Trim().ToLower())))
                                {
                                    List<string> customerlists = customerlist.Select(x => x.Skcode.Trim().ToLower()).ToList();
                                    List<string> skcodenotpresent = skcodes.Where(x => !customerlists.Contains(x)).ToList();
                                    string abc = String.Join(",", skcodenotpresent);
                                    MSG = "Skcode " + abc + " is not Present in our Databse"; return Created(MSG, MSG);
                                }
                                if (customerlist.Any(x => x.Active == false))
                                {
                                    List<string> skcodeinactive = customerlist.Where(x => x.Active == false).Select(y => y.Skcode).ToList();
                                    string abc = String.Join(",", skcodeinactive);
                                    MSG = "Skcode " + abc + " is inactive in our Databse"; return Created(MSG, MSG);
                                }
                                if (uploaditemlist.Any(x => x.CreditLimit < 0))
                                {
                                    MSG = "Please Enter Credit Limit in Excel"; return Created(MSG, MSG);
                                }
                                string message = string.Empty;
                                string duplicatemessage = string.Empty;

                                List<int> customerids = customerlist.Select(x => x.CustomerId).Distinct().ToList();
                                List<long> storeids = storelist.Select(x => x.Id).Distinct().ToList();
                                var storecreditdata = context.StoreCreditLimitDb.Where(x => customerids.Contains(x.CustomerId) && storeids.Contains(x.StoreId)).ToList();
                                foreach (var a in uploaditemlist)
                                {
                                    bool isadd = true;
                                    bool isChecked = true;
                                    long storeid = 0; int Customerid = 0;
                                    storeid = storelist.FirstOrDefault(x => x.Name.Trim().ToLower() == a.StoreName.Trim().ToLower()).Id;
                                    Customerid = customerlist.FirstOrDefault(x => x.Skcode.Trim().ToLower() == a.SkCode.Trim().ToLower()).CustomerId;
                                    var datas = storecreditdata != null && storecreditdata.Any() ? storecreditdata.Where(x => x.CustomerId == Customerid && x.StoreId == storeid).FirstOrDefault() : null;
                                    if (datas != null)
                                    {
                                        if (storeCreditLimitslists != null && storeCreditLimitslists.Any())
                                        {

                                            var ispresent = storeCreditLimitslists.Any(x => x.CustomerId == Customerid && x.StoreId == storeid);
                                            if (ispresent == true)
                                            {
                                                isadd = false;
                                                if (duplicatecount == 0)
                                                {
                                                    duplicatemessage = " SkCode " + a.SkCode + " and Store Name " + a.StoreName;
                                                }
                                                else
                                                {
                                                    duplicatemessage = duplicatemessage + " and" + " SkCode " + a.SkCode + " and Store Name " + a.StoreName;
                                                }
                                                duplicatecount++;
                                            }
                                        }
                                        if (isadd == true)
                                        {
                                            int skip = 0, take = 1;
                                            string skcode = string.Empty;
                                            GetCreditSearchListDc res = new GetCreditSearchListDc();
                                            List<long> storeidlist = new List<long>();
                                            storeidlist.Add(storeid);
                                            var storeidss = new DataTable();
                                            storeidss.Columns.Add("IntValue");
                                            foreach (var obj in storeidlist)
                                            {
                                                var dr = storeidss.NewRow();
                                                dr["IntValue"] = obj;
                                                storeidss.Rows.Add(dr);
                                            }
                                            var param = new SqlParameter("@StoreIds", storeidss);
                                            param.SqlDbType = SqlDbType.Structured;
                                            param.TypeName = "dbo.IntValues";

                                            skcode = customerlist.FirstOrDefault(x => x.CustomerId == Customerid).Skcode;
                                            var param2 = new SqlParameter("@SkCode", skcode);
                                            var param3 = new SqlParameter("@Skip", skip);
                                            var param4 = new SqlParameter("@Take", take);
                                            res = context.Database.SqlQuery<GetCreditSearchListDc>("exec Sp_GetCreditSearchList @StoreIds,@Skcode,@Skip,@Take", param, param2, param3, param4).FirstOrDefault();
                                            if (res != null)
                                            {
                                                if (res.UsedCreaditLimit > 0)
                                                {
                                                    if (res.UsedCreaditLimit > a.CreditLimit)
                                                    {
                                                        isChecked = false;
                                                    }
                                                }
                                                if (isChecked == false)
                                                {

                                                    if (creditlimitcount == 0)
                                                    {
                                                        creditlimitmessage = "SkCode " + skcode + " CreditLimit " + a.CreditLimit + " can not be smaller then Used Credit Limit " + res.UsedCreaditLimit;
                                                    }
                                                    else
                                                    {
                                                        creditlimitmessage += " and " + "SkCode " + skcode + " CreditLimit " + a.CreditLimit + " can not be smaller then Used Credit Limit " + res.UsedCreaditLimit;
                                                    }
                                                    creditlimitcount++;

                                                }
                                            }
                                            if (isChecked == true)
                                            {
                                                revisedcount++;
                                                datas.CreditLimit = a.CreditLimit;
                                                datas.DueDays = a.DueDays;
                                                datas.ModifiedBy = userid;
                                                datas.ModifiedDate = DateTime.Now;
                                                context.Entry(datas).State = EntityState.Modified;

                                                StoreCreditLimit store = new StoreCreditLimit();
                                                store.StoreId = storeid;
                                                store.CustomerId = Customerid;
                                                store.CreditLimit = a.CreditLimit;
                                                store.CreatedBy = userid;
                                                store.CreatedDate = DateTime.Now;
                                                store.IsDeleted = false;
                                                store.IsActive = true;
                                                store.DueDays = a.DueDays;
                                                storeCreditLimitslists.Add(store);
                                            }

                                        }
                                    }
                                    else
                                    {
                                        if (newcount == 0)
                                        {
                                            message = " SkCode " + a.SkCode + " and Store Name " + a.StoreName;
                                        }
                                        else
                                        {
                                            message = message + " and" + " SkCode " + a.SkCode + " and Store Name " + a.StoreName;
                                        }
                                        newcount++;
                                    }
                                }
                                if (duplicatecount > 0)
                                {
                                    MSG = duplicatemessage + " is Duplicate in Your Sheet"; return Created(MSG, MSG);
                                }
                                if (newcount > 0)
                                {
                                    MSG = message + " is new for PayLater Configuration"; return Created(MSG, MSG);
                                }
                                if (creditlimitcount > 0)
                                {
                                    MSG = creditlimitmessage; return Created(MSG, MSG);
                                }
                                if (revisedcount > 0)
                                {
                                    if (context.Commit() > 0)
                                    {
                                        MSG = "Edited Successfully"; return Created(MSG, MSG);
                                    }
                                    else
                                    {
                                        MSG = "Something Error Occured"; return Created(MSG, MSG);
                                    }
                                }
                                else
                                {
                                    MSG = message + " is new for PayLater Configuration"; return Created(MSG, MSG);
                                }
                            }
                            else


                            {
                                MSG = "Data Not Found in Excel"; return Created(MSG, MSG);
                            }
                        }
                        else
                        {
                            return Created("File extnsion required .xlsx", "File extnsion required .xlsx");
                        }
                    }
                }

                return Created("Error", "Error");
            }
            else
            {
                return Created("Error", "Error");
            }
        }

        //[Route("GetAllWarehouseList")]
        //[HttpGet]
        //public List<Warehouse> GetAllWarehouseList()
        //{
        //    using (var db = new AuthContext())
        //    {
        //        List<Warehouse> wlist = new List<Warehouse>();
        //        wlist = db.Warehouses.Where(x => x.active == true && x.Deleted == false).ToList();
        //        return wlist;
        //    }
        //}

        [Route("GetAllWarehouseList")]
        [HttpGet]
        public List<WarehouseListShowDCPayLater> GetAllWarehouseList()
        {
            using (var db = new AuthContext())
            {
                List<WarehouseListShowDCPayLater> wlist = new List<WarehouseListShowDCPayLater>();
                int userid = GetLoginUserId();
                var query = "  select  w.WarehouseId ,  w.WarehouseName from WarehousePermissions wp with(nolock)  inner join Warehouses w   with(nolock) on w.WarehouseId = wp.WarehouseId  inner join GMWarehouseProgresses wps  with(nolock) on wps.WarehouseID = w.WarehouseId and wps.IsLaunched = 1 inner join Cities c   with(nolock) on c.Cityid = w.Cityid    inner join Clusters cl  with(nolock) on cl.WarehouseId = w.WarehouseId " +
                    "where w.active = 1 and w.Deleted = 0  and w.IsKPP = 0 " + "and wp.PeopleID= " + userid +
                    "and wp.IsActive = 1 and wp.IsDeleted = 0   group by  w.WarehouseId,w.WarehouseName";
                wlist = db.Database.SqlQuery<WarehouseListShowDCPayLater>(query).ToList();
                return wlist;
            }
        }
        protected int GetLoginUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }

        [Route("UpdatePayLaterStatus")]
        [HttpGet]
        public bool UpdatePayLaterStatus()
        {
            bool res = true;
            using (var db = new AuthContext())
            {
                List<PayLaterUpdateDc> resultdc = new List<PayLaterUpdateDc>();
                resultdc = db.Database.SqlQuery<PayLaterUpdateDc>("exec Sp_PaylaterDueOverDueUpdate").ToList();
                if (resultdc != null && resultdc.Any())
                {

                    List<long> ids = resultdc.Select(x => x.Id).ToList();
                    var collection = db.PayLaterCollectionDb.Where(x => ids.Contains(x.Id)).ToList();
                    if (collection != null && collection.Any())
                    {
                        foreach (var data in collection)
                        {
                            data.Status = resultdc.FirstOrDefault(x => x.Id == data.Id).PaymentStatus;
                            data.ModifiedBy = 1;
                            data.ModifiedDate = DateTime.Now;
                            db.Entry(data).State = EntityState.Modified;
                            db.Commit();
                        }
                    }

                }
            }
            return res;
        }

        [Route("OrderSettle")]
        [HttpGet]
        public bool OrderSettle(AuthContext context,int OrderId)
        {
            bool res = false;
            var ordermaster = context.DbOrderMaster.FirstOrDefault(x => x.OrderId == OrderId && x.Deleted == false);
            var orderdispatchmaster = context.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == OrderId && x.Deleted == false );
            if(ordermaster != null && orderdispatchmaster != null)
            {
                if(ordermaster.Status != "sattled")
                {
                    ordermaster.Status = "sattled";
                    ordermaster.UpdatedDate = DateTime.Now;
                    context.Entry(ordermaster).State = EntityState.Modified;
                    orderdispatchmaster.Status = "sattled";
                    orderdispatchmaster.UpdatedDate = DateTime.Now;
                    context.Entry(orderdispatchmaster).State = EntityState.Modified;
                    OrderMasterHistories h1 = new OrderMasterHistories();
                    h1.orderid = OrderId;
                    h1.Status = "sattled";
                    h1.Reasoncancel = "Order Settle";
                    h1.CreatedDate = DateTime.Now;
                    h1.username = "By System";
                    context.OrderMasterHistoriesDB.Add(h1);
                    res = true;
                }
            }
            return res;
        }

        [Route("GetMopWisepayLaterData")]
        [HttpPost]
        public List<PayLaterMopWiseData> GetMopWisepayLaterData(GetMopwisePayload getMopwisePayload)
        {
            using (var context = new AuthContext())
            {
                List<PayLaterMopWiseData> result = new List<PayLaterMopWiseData>();
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
                var startparam = new SqlParameter("@startdate", getMopwisePayload.startdate);
                var endparam = new SqlParameter("@enddate", getMopwisePayload.enddate);
                var skipparam = new SqlParameter("@Skip", getMopwisePayload.Skip);
                var takeparam = new SqlParameter("@Take", getMopwisePayload.Take);
                var exportparam = new SqlParameter("@IsExport", getMopwisePayload.IsExport);
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[Sp_PayLaterMopWiseDataSk]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(startparam);
                cmd.Parameters.Add(endparam);
                cmd.Parameters.Add(skipparam);
                cmd.Parameters.Add(takeparam);
                cmd.Parameters.Add(exportparam);
                cmd.CommandTimeout = 1200;
                var reader = cmd.ExecuteReader();
                result = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<PayLaterMopWiseData>(reader).ToList();
                context.Database.Connection.Close();
                return result;

            }
        }
    }

    public class OTPGenerate
    {
        public int peopleid { get; set; }
        public int assignmentID { get; set; }

    }
    public class MTD
    {
        public List<int> warehouseIds { get; set; }

        //public int Warehouseid { get; set; }
        public DateTime? Fromdate { get; set; }
        public DateTime? Todate { get; set; }

    }
    public class PayLaterAmountUpdateDC
    {
        public double Amount { get; set; }
        public string Comment { get; set; }
    }

}
