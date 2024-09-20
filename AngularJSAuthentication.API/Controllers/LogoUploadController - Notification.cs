using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using NLog;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/logoUploadNotification")]
    public class LogoUploadNotificationController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        [HttpPost]
        public string UploadFile()
        {
            string Logourl = "";
            logger.Info("start logo upload");
            try
            {              
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    // Get the uploaded image from the Files collection
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];

                    if (httpPostedFile != null)
                    {
                        // Validate the uploaded image(optional)
                        var contenttype = Common.Helpers.MIMEAssistant.MIMETypesDictionary.FirstOrDefault(x => x.Value == httpPostedFile.ContentType);
                        // Get the complete file path
                        var LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/notificationimages"), httpPostedFile.FileName+"."+ contenttype.Key);
                        httpPostedFile.SaveAs(LogoUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/notificationimages", LogoUrl);

                        if (httpPostedFile.ContentType.Contains("image/") &&  (ConfigurationManager.AppSettings["Environment"] == "Production"))
                        {
                            Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                            Cloudinary cloudinary = new Cloudinary(account);

                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(LogoUrl),
                                PublicId = "Notifications/" + httpPostedFile.FileName ,
                                Overwrite = true,
                                Invalidate = true,
                                Backup = false
                            };

                            var uploadResult = cloudinary.Upload(uploadParams);

                            //if (System.IO.File.Exists(LogoUrl))
                            //{
                            //    System.IO.File.Delete(LogoUrl);
                            //}

                            Logourl = uploadResult.SecureUri.ToString();
                        }
                        else
                        {
                            
                            Logourl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                               , HttpContext.Current.Request.Url.DnsSafeHost
                                                               , HttpContext.Current.Request.Url.Port
                                                               , string.Format("notificationimages/{0}", httpPostedFile.FileName + "." + contenttype.Key));
                            //LogoUrl = "~/notificationimages/" + httpPostedFile.FileName + "." + contenttype.Key;
                        }
                    }
                }
                return Logourl;
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }
            return Logourl;
        }
    }
}
