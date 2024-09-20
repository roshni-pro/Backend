using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/RIUpload")]
    public class RIUploadController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public string Logourl { get; private set; }

        [HttpPost]
        public string UploadFile()
        {
            logger.Info("start logo upload");
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                }

                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        var LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/RewardItem"), httpPostedFile.FileName);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/images/RewardItem", LogoUrl);
                        Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                        Cloudinary cloudinary = new Cloudinary(account);

                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(LogoUrl),
                            PublicId = "RewardImages/" + httpPostedFile.FileName,
                            Overwrite = true,
                            Invalidate = true,
                            Backup = false
                        };

                        var uploadResult = cloudinary.Upload(uploadParams);

                        if (System.IO.File.Exists(LogoUrl))
                        {
                            System.IO.File.Delete(LogoUrl);
                        }
                        Logourl = uploadResult.SecureUri.ToString();
                        return Logourl;
                    }
                }
                return Logourl;
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
                return Logourl;
            }
        }
    }
}
