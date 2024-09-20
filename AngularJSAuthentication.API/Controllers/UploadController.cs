using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/upload")]
    public class UploadController : ApiController
    {

        [HttpPost]
        public string UploadFile()
        {
            string Logourl = "";
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                // Get the uploaded image from the Files collection
                var httpPostedFile = HttpContext.Current.Request.Files["file"];

                if (httpPostedFile != null)
                {
                    // Validate the uploaded image(optional)
                    // Get the complete file path
                    var FileUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles"), httpPostedFile.FileName);
                    httpPostedFile.SaveAs(FileUrl);
                    AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/UploadedFiles", FileUrl);

                    Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                    Cloudinary cloudinary = new Cloudinary(account);

                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(FileUrl),
                        PublicId = "items_images/item_1/" + httpPostedFile.FileName,
                        Overwrite = true,
                        Invalidate = true,
                        Backup = false
                    };

                    var uploadResult = cloudinary.Upload(uploadParams);

                    if (System.IO.File.Exists(FileUrl))
                    {
                        System.IO.File.Delete(FileUrl);
                    }

                    Logourl = uploadResult.SecureUri.ToString();
                    return Logourl;
                }
                return Logourl;
            }
            return Logourl;
        }
    }
}