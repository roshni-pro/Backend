using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Mongo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class CustomerOtpHelper
    {
        public async Task<string> GenerateOtp(string mobile, int expiryTimeInMins = 10)
        {
            MongoDbHelper<CustomerOtp> mongoDbHelper = new MongoDbHelper<CustomerOtp>();
            Random rand = new Random(100);
            int randomNumber = rand.Next(000000, 999999);
            string otp = OtpGenerator.GetSixDigitNumber(randomNumber);

            CustomerOtp customerOtp = mongoDbHelper.Select(x => x.Mobile == mobile && x.ExpiryTime >= DateTime.Now ).FirstOrDefault();
            if (customerOtp == null)
            {
                customerOtp = new CustomerOtp();
                customerOtp.Mobile = mobile;
                customerOtp.OTP = otp;
                customerOtp.GenertedTime = DateTime.Now;
                customerOtp.ExpiryTime = DateTime.Now.AddMinutes(expiryTimeInMins);
                await mongoDbHelper.InsertAsync(customerOtp);
               
            }
            else
            {
                customerOtp.OTP = otp;
                customerOtp.GenertedTime = DateTime.Now;
                customerOtp.ExpiryTime = DateTime.Now.AddMinutes(expiryTimeInMins);
                await mongoDbHelper.InsertAsync(customerOtp);
                otp = customerOtp.OTP;
              
            }
            var Otpmessage = "Your one time otp:-" + customerOtp.OTP;
            bool messagesend = SendSMSHelper.SendSMS(customerOtp.Mobile, Otpmessage, null,"");
            return otp;

        }

        public async Task<bool> MatchOtp(string mobile, string otp)
        {
            int validUptoMinutes = 5;
            MongoDbHelper<CustomerOtp> mongoDbHelper = new MongoDbHelper<CustomerOtp>();
            CustomerOtp customerOtp = mongoDbHelper.Select(x => x.Mobile == mobile).FirstOrDefault();


            if (customerOtp != null && !string.IsNullOrEmpty(customerOtp.OTP) && customerOtp.OTP == otp &&  customerOtp.ExpiryTime >= DateTime.Now)
            {
                customerOtp.OTP = "";
                customerOtp.GenertedTime = customerOtp.GenertedTime.AddMinutes(-validUptoMinutes);
                await mongoDbHelper.ReplaceAsync(customerOtp.Id, customerOtp);
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}