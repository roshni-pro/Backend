using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using NLog;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;
using System.Text;
using System;
using System.Security.Cryptography;
using System.Data.Entity;
using System.Web;
using System.Configuration;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("ICICI")]
    public class ICICIController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();
        public int QRExpireTime = AppConstants.QRExpireTime;
        public static List<long> TxnInProcess = new List<long>();

        [HttpPost]
        [Route("ICICIcallBackRes")]
        [AllowAnonymous]
        public async Task<bool> ICICIcallBackRes(HttpRequestMessage request)
        {
            bool Response = false;
            if (!(HttpContext.Current.Request.Headers.GetValues("ICICIUserName")?.FirstOrDefault() == ConfigurationManager.AppSettings["ICICIUserName"].ToString()
                && HttpContext.Current.Request.Headers.GetValues("ICICIPassword")?.FirstOrDefault() == ConfigurationManager.AppSettings["ICICIPassword"].ToString()))
            {
                return Response;
            }
            string ReqhashKey = string.Empty;
            string GenhashKey = string.Empty;
            StringBuilder secureHashNew = new StringBuilder();
            List<ResICICIData> resData = new List<ResICICIData>();
            var content = request.Content;
            string resContent = content.ReadAsStringAsync().Result;
            TextFileLogHelper.TraceLog("ICICI meRes Json Decrypt : " + resContent);
            JsonObject data = System.Text.Json.JsonSerializer.Deserialize<JsonObject>(resContent);
            using (var context = new AuthContext())
            {
                var Tid = data.FirstOrDefault(y => y.Key == "txnID").Value.ToString();
                var resCode = data.FirstOrDefault(x => x.Key == "responseCode").Value.ToString();
                var companyDetails = await context.CompanyDetailsDB.FirstOrDefaultAsync();
                var PaymentRes = await context.PaymentResponseRetailerAppDb.FirstOrDefaultAsync(x => x.PaymentFrom.ToLower() == "icici" && x.status == "Success" && x.GatewayTransId == Tid);
                if (companyDetails != null && !string.IsNullOrEmpty(companyDetails.ICICISecretKey) && (resCode == "000" || resCode == "0000"))
                {
                    foreach (var item in data)
                    {
                        if (item.Key != "secureHash")
                        {
                            resData.Add(new ResICICIData
                            {
                                Key = item.Key,
                                Value = item.Value.ToString()
                            });
                        }
                        else
                        {
                            ReqhashKey = item.Value.ToString();
                        }
                    }
                    var FInalData = resData.OrderBy(x => x.Key).ToList();
                    FInalData.ForEach(x =>
                    {
                        secureHashNew.Append(x.Value.ToString());
                    });
                    GenhashKey = calculateRFC2104HMAC(secureHashNew.ToString(), companyDetails.ICICISecretKey);
                    if (String.Equals(ReqhashKey, GenhashKey))
                    {
                        if (PaymentRes != null && PaymentRes.status == "Success")
                        {
                            Response = true;
                        }
                    }
                    else
                    {
                        if (PaymentRes != null)
                        {
                            PaymentRes.status = "Failed";
                            PaymentRes.UpdatedDate = DateTime.Now;
                            context.Entry(PaymentRes).State = System.Data.Entity.EntityState.Modified;
                            context.Commit();
                        }
                    }
                }
                else
                {
                    if (PaymentRes != null)
                    {
                        PaymentRes.status = "Failed";
                        PaymentRes.UpdatedDate = DateTime.Now;
                        context.Entry(PaymentRes).State = System.Data.Entity.EntityState.Modified;
                        context.Commit();
                    }
                }
            }
            return Response;
        }
        private string calculateRFC2104HMAC(string data, string SecretKey)
        {
            string result = string.Empty;
            try
            {
                using (HMACSHA256 hmac = new HMACSHA256(Encoding.ASCII.GetBytes(SecretKey)))
                {
                    var payload = Encoding.ASCII.GetBytes(data);
                    var rawhmac = hmac.ComputeHash(payload);
                    result = HashEncode(rawhmac);
                }
            }
            catch (Exception e)
            {

                throw;
            }
            return result;
        }
        private string HashEncode(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }


        public class ResICICIData
        {
            public string Key { get; set; }
            public string Value { get; set; }

        }
    }
}
