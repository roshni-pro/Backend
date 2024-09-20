//using System.DirectoryServices.AccountManagement;
using NLog;
using System;
using System.Configuration;
//using System.DirectoryServices;

using System.Security.Cryptography;
using System.Text;

namespace AngularJSAuthentication.API
{

    public static class Util
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        public static bool NotifyUsersForConfirmingRegistration(string emailid, string Password)
        {


            //logger.Info("In function NotifyUsersForScheduling with site id {0}", iSiteId);
            bool _bIsNotified = false;


            try
            {
                string _sSubject = ConfigurationManager.AppSettings["Subject"].ToString();
                string _sEmailEncrypted = "", _sHostNameEncrypted = "", _sMessage = "";
                string _sUrl = ConfigurationManager.AppSettings["ConfirmEmailURL"].ToString();
                string fromemail = ConfigurationManager.AppSettings["FromEmailAddress"].ToString();
                string _sUserUrl = "";
                //logger.Info("subject ,url = {0} ,{1} ", _sSubject, _sUrl);
                bool _bIsEmailSent = false;

                //try
                //{
                //    _sUserUrl = "";
                //    _sEmailEncrypted = _sHostNameEncrypted = "";
                //    string finalstring = emailid + "{GreenTime}" + Password;
                //    _sEmailEncrypted = Util.Encrypt(finalstring, true);

                //    _sEmailEncrypted = HttpServerUtility.UrlTokenEncode(Encoding.ASCII.GetBytes(_sEmailEncrypted));

                //    logger.Info("Encription success");
                //    _sUserUrl = _sUrl + "?id="+ _sEmailEncrypted;
                //    _sMessage = "<h2>GreenTime</h2><BR/><BR/>";
                //    _sMessage += "Dear User,<br/>Please click the below link to Confirm you email id.<br/><br/><br/>";
                //    _sMessage += string.Format("URL: <a href='{0}'>Confirm Email</a><br/><br/>", _sUserUrl, _sHostNameEncrypted);
                //    logger.Info("got user: {0} " + emailid);
                //    _bIsEmailSent = Util.SendEmail(emailid, "akhilesh.gandhi@sandisk.com", _sSubject, _sMessage, fromemail);
                //    logger.Info("email send to user: {0} ", emailid);
                //}
                //catch (Exception ex)
                //{

                //    logger.Info("Unable to process user {0}  error  {1}", emailid, ex.Message);
                //}

                _bIsNotified = true;
            }
            catch (Exception ex)
            {
                _bIsNotified = false;
                throw ex;
            }
            return _bIsNotified;
        }

        public static string Encrypt(string toEncrypt, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            System.Configuration.AppSettingsReader settingsReader =
                                                new AppSettingsReader();
            // Get the key from config file

            string key = (string)settingsReader.GetValue("SecurityKey",
                                                             typeof(String));
            //System.Windows.Forms.MessageBox.Show(key);
            //If hashing use get hashcode regards to your key
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                //Always release the resources and flush data
                // of the Cryptographic service provide. Best Practice

                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes.
            //We choose ECB(Electronic code Book)
            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)

            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            //transform the specified region of bytes array to resultArray
            byte[] resultArray =
              cTransform.TransformFinalBlock(toEncryptArray, 0,
              toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor
            tdes.Clear();
            //Return the encrypted data into unreadable string format
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        public static string Decrypt(string cipherString, bool useHashing)
        {
            byte[] keyArray;
            //get the byte code of the string

            byte[] toEncryptArray = null;

            try
            {
                toEncryptArray = Convert.FromBase64String(cipherString);
            }
            catch (Exception ex) { }
            System.Configuration.AppSettingsReader settingsReader =
                                                new AppSettingsReader();
            //Get your key from config file to open the lock!
            string key = (string)settingsReader.GetValue("SecurityKey",
                                                         typeof(String));

            if (useHashing)
            {
                //if hashing was used get the hash code with regards to your key
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                //release any resource held by the MD5CryptoServiceProvider

                hashmd5.Clear();
            }
            else
            {
                //if hashing was not implemented get the byte code of the key
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            }

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes. 
            //We choose ECB(Electronic code Book)

            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(
                                 toEncryptArray, 0, toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor                
            tdes.Clear();
            //return the Clear decrypted TEXT
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        //public static bool SendEmail(string sToEmail, string sCcEmail, string sSubject, string sMessage, string _sFromAddress)
        //{
        //        bool _bIsEmailSent = true;
        //        //  From address will be fetched from Web.config file.
        //        string _sSmtpAddress = "";
        //        string _from = "info@moreyahs.com";
        //        //    _sSmtpAddress = ConfigurationManager.AppSettings["SmtpAddress"].ToString();
        //        try
        //        {
        //            var smtp = new SmtpClient
        //            {
        //                Host = "smtp.gmail.com",
        //                Port = 587,
        //                EnableSsl = true,
        //                DeliveryMethod = SmtpDeliveryMethod.Network,
        //                UseDefaultCredentials = false,
        //                Credentials = new NetworkCredential(_sFromAddress, "More@Yeahs")

        //            };
        //            using (var message = new MailMessage(_sFromAddress, sToEmail)
        //            {
        //                Subject = sSubject,
        //                Body = sMessage,
        //                IsBodyHtml = true
        //            })
        //            {
        //                smtp.Send(message);
        //            }
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        //bool _bIsEmailSent = true;
        //        ////  From address will be fetched from Web.config file.
        //        //string _sSmtpAddress = ""; 

        //        //_sSmtpAddress = ConfigurationManager.AppSettings["SmtpAddress"].ToString();


        //        //MailMessage msg = new MailMessage();
        //        //if (sToEmail != null)
        //        //{
        //        //  foreach (string email in sToEmail.Split(';'))
        //        //  {
        //        //    if (email.Trim().Length != 0)
        //        //      msg.To.Add(email);
        //        //  }
        //        //}
        //        //if (sCcEmail != null)
        //        //{
        //        //  foreach (string email in sCcEmail.Split(';'))
        //        //  {
        //        //    if (email.Trim().Length != 0)
        //        //    {
        //        //      msg.Bcc.Add(email);
        //        //    }
        //        //  }
        //        //}

        //        //msg.From = new MailAddress(_sFromAddress);
        //        //msg.Subject = sSubject;
        //        //msg.Body = sMessage;
        //        //msg.IsBodyHtml = true;

        //        //SmtpClient smtp = new SmtpClient();
        //        //smtp.Host = _sSmtpAddress;
        //        //smtp.Send(msg);

        //        return _bIsEmailSent;
        //}

    }
}