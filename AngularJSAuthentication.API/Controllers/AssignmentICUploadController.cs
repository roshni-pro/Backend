using NLog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/AssignmentICUpload")]
    public class AssignmentIcUploadController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("UploadFile")]
        [HttpPost]
        public HttpResponseMessage UploadICImage()
        {
            string ImageUrl = string.Empty;
            string filename = string.Empty;
            logger.Info("UploadICImage upload");

            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                // Get the uploaded image from the Files collection
                var httpPostedFile = HttpContext.Current.Request.Files["file"];

                if (httpPostedFile != null)
                {
                    var fileuploadPath = HttpContext.Current.Server.MapPath("~/images/AssigmentIcImages");
                    if (!Directory.Exists(fileuploadPath))
                    {
                        Directory.CreateDirectory(fileuploadPath);
                    }
                    ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/AssigmentIcImages"), httpPostedFile.FileName);
                    httpPostedFile.SaveAs(ImageUrl);
                    filename = httpPostedFile.FileName.ToString();

                    var uploader = new List<AngularJSAuthentication.DataContracts.FileUpload.Uploader> { new AngularJSAuthentication.DataContracts.FileUpload.Uploader() };
                    uploader.FirstOrDefault().FileName = httpPostedFile.FileName;
                    uploader.FirstOrDefault().RelativePath = "~/images/AssigmentIcImages";

                    uploader.FirstOrDefault().SaveFileURL = ImageUrl;

                    uploader = Nito.AsyncEx.AsyncContext.Run(() => AngularJSAuthentication.Common.Helpers.FileUploadHelper.UploadFileToOtherApi(uploader));

                    return Request.CreateResponse(System.Net.HttpStatusCode.OK, true);
                }
            }
            return Request.CreateResponse(System.Net.HttpStatusCode.OK, false);
        }
    }

}