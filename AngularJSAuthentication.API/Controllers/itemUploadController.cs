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
    [RoutePrefix("api/itemUpload")]
    public class itemUploadController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        [HttpPost]
        public string UploadFile(string type)
        {
            string ImageUrl = null;
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
                    // Get the uploaded image from the Files collection
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];

                    if (httpPostedFile != null)
                    {
                        // Validate the uploaded image(optional)

                        // Get the complete file path
                        //var LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedLogos"), httpPostedFile.FileName);

                        var LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedLogos"), type + ".png");

                        //string physicalPath = ("~/images/" + ImageName);
                        //httpPostedFile.SaveAs(physicalPath);
                        //customer newRecord = new customer();
                        //newRecord.username = customer.username;
                        ////.......saving picture url......
                        //newRecord.picture = physicalPath;

                        //// Save the uploaded file to "UploadedFiles" folder
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                        httpPostedFile.SaveAs(LogoUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/UploadedLogos", LogoUrl);

                        CloudinaryDotNet.Account account = new CloudinaryDotNet.Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);
                        Cloudinary cloudinary = new Cloudinary(account);
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(LogoUrl),
                            PublicId = "items_images/item_1" + httpPostedFile.FileName,
                            Overwrite = true,
                            Invalidate = true,
                            Backup = false
                        };
                        var uploadResult = cloudinary.Upload(uploadParams);
                        if (System.IO.File.Exists(LogoUrl))
                        {
                            System.IO.File.Delete(LogoUrl);
                        }
                        ImageUrl = uploadResult.SecureUri.ToString();
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
                return null;
            }
            return ImageUrl;
        }
    }
}
