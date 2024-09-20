using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AngularJSAuthentication.API.Helpers;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Promotional")]
    public class PromotionalController : ApiController
    {

        [Route("PromotionLogin")]
        [HttpGet]
        [AllowAnonymous]

        public async Task<ReturnResponse> PromotionLogin(string Mobile, string Password)
        {
            ReturnResponse returnResponse = new ReturnResponse { status = false, Message = "" };

            using (AuthContext context = new AuthContext())
            {

                PromotionalRetailer customer = context.Customers.Where(x => x.Deleted == false && x.Mobile == Mobile).Select(x => new PromotionalRetailer
                {
                    Active = x.Active,
                    CustomerId = x.CustomerId,
                    Mobile = x.Mobile,
                    Name = x.Name,
                    ShopName = x.ShopName,
                    Skcode = x.Skcode,
                    Warehouseid = x.Warehouseid,
                    Password=x.Password
                }).FirstOrDefault();

                if (customer != null)
                {
                    if (customer.Active)
                    {
                        if (customer.Password == Password)
                        {
                            returnResponse.status = true;
                            returnResponse.Message = "Login successfully.";
                            returnResponse.Customer = customer;
                        }
                        else
                        {
                            returnResponse.status = false;
                            returnResponse.Message = "Please enter correct password.";
                        }
                    }
                    else
                    {
                        returnResponse.status = false;
                        returnResponse.Message = "Customer not active.";
                    }
                    //new Sms().sendOtp(customer.Mobile, "Hi " + customer.ShopName + " \n\t You Recently requested a forget password on ShopKirana. Your account Password is '" + customer.Password + "'\n If you didn't request then ingore this message\n\t\t Thanks\n\t\t Shopkirana.com");

                    //res = new MessageDetails()
                    //{

                    //    Status = true,
                    //    Message = "Message send to your registered mobile number."
                    //};

                    //return res;
                }
                else
                {
                    returnResponse.status = false;
                    returnResponse.Message = "Customer not exist.";


                }
            }
            return returnResponse;
        }

        [Route("SendPromotionalMessage")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ReturnResponse> SendPromotionalMessage(string mobile, int customerId,string brandName)
        {
            ReturnResponse returnResponse = new ReturnResponse { status = false, Message = "" };
            MongoDbHelper<PromotionalCustomers> mongoDbHelper = new MongoDbHelper<PromotionalCustomers>();

            if (mongoDbHelper.Count(x => x.Mobile == mobile && x.BrandName== brandName) == 0)
            {
                string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                string sRandomOTP = "KK" + GenerateRandomOTP(6, saAllowedCharacters);
                bool result= new Sms().sendOtp(mobile, "Hi  \n\t Thank you for choosing " + brandName + ". Please use below promocode "+ sRandomOTP + "\n\t\t Thanks\n\t\t Shopkirana.com","");
                PromotionalCustomers CustomerOTP = new PromotionalCustomers
                {
                    CreatedDate = DateTime.Now,                   
                    IsActive = true,
                    Mobile = mobile,
                    Otp = sRandomOTP,
                    BrandName=brandName,
                    CustomerId=customerId,
                    IsSMSSend= result
                };
                mongoDbHelper.Insert(CustomerOTP);
                returnResponse.status = true;
                returnResponse.Message = "Thank you for choosing "+ brandName +".";
            }
            else
            {
                returnResponse.status = true;
                returnResponse.Message = "Thank you for choosing " + brandName + ". ";
            }
            return returnResponse;
        }

        private string GenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)
        {
            using (AuthContext db = new AuthContext())
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
        }
    }


public class PromotionalRetailer
{
    public int CustomerId { get; set; }
    public int? Warehouseid { get; set; }
    public string Skcode { get; set; }
    public string Password { get; set; }
    public string ShopName { get; set; }
    public string Mobile { get; set; }      
    public string Name { get; set; }
    public bool Active { get; set; }
}

public class ReturnResponse
{
    public bool status { get; set; }
    public string Message { get; set; }

    public PromotionalRetailer Customer { get; set; }
}

    public class PromotionalCustomers
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string BrandName { get; set; }
        public string Mobile { get; set; }
        public int CustomerId { get; set; }
        public bool IsSMSSend { get; set; }
        public string Otp { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }

}
