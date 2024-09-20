using NLog;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Transaction")]
    public class TransactionController : ApiController
    {
      
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static string IV = "A9A4455FB5415E10";
        private static string KEY = "BFEE23C36C78CFAE47A1B8515E64C3BF";

        //[Authorize]
        [HttpPost]
        [Route("paymentcallback")]
        public HttpResponseMessage paymentcallback(PaymentDTO paymentDTO)
        {
            try
            {
                paymentDTO.encdata = DecodeAndDecrypt(paymentDTO.encdata);
                var res = new
                {
                    encdata = paymentDTO.encdata,
                    Status = 200,
                    Message = "Success"
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);

            }
            catch (Exception ex)
            {
                var res = new
                {
                    encdata = paymentDTO.encdata,
                    Status = 400,
                    Message = "Fail"
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        public static string EncryptAndEncode(string raw)
        {
            using (var csp = new AesCryptoServiceProvider())
            {
                ICryptoTransform e = GetCryptoTransform(csp, true);
                byte[] inputBuffer = Encoding.UTF8.GetBytes(raw);
                byte[] output = e.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
                string encrypted = Convert.ToBase64String(output);
                return encrypted;
            }
        }

        public static string DecodeAndDecrypt(string encrypted)
        {
            using (var csp = new AesCryptoServiceProvider())
            {
                var d = GetCryptoTransform(csp, false);
                byte[] output = Convert.FromBase64String(encrypted);
                byte[] decryptedOutput = d.TransformFinalBlock(output, 0, output.Length);
                string decypted = Encoding.UTF8.GetString(decryptedOutput);
                return decypted;
            }
        }

        public static string createChecksum(string rawData)
        {
            // Create a SHA256  
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array 
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string  
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString().ToUpper();
            }
        }

        private static ICryptoTransform GetCryptoTransform(AesCryptoServiceProvider csp, bool encrypting)
        {
            csp.Mode = CipherMode.CBC;
            csp.Padding = PaddingMode.PKCS7;
            //var spec = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(PASSWORD), Encoding.UTF8.GetBytes(SALT), 65536);
            //byte[] key = spec.GetBytes(16);


            csp.IV = Encoding.UTF8.GetBytes(IV);
            csp.Key = Encoding.UTF8.GetBytes(KEY);
            if (encrypting)
            {
                return csp.CreateEncryptor();
            }
            return csp.CreateDecryptor();
        }



    }

    public class PaymentDTO
    {
        public string encdata
        {
            get; set;
        }
        public string checksum
        {
            get; set;
        }
        public string mCode
        {
            get; set;
        }
    }
}