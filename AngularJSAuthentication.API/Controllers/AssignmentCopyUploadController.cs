using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.FileUpload;
using Nito.AsyncEx;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/AssignmentCopyUpload")]
    public class AssignmentCopyUploadController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
        [HttpPost]
        public HttpResponseMessage UploadIrImage()
        {
            string ImageUrl = string.Empty;
            string filename = string.Empty;
            logger.Info("start image upload");
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    // Get the uploaded image from the Files collection
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        // Validate the uploaded image(optional)
                        // Get the complete file path
                        ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/AssigmentImages"), httpPostedFile.FileName);
                        httpPostedFile.SaveAs(ImageUrl);
                        filename = httpPostedFile.FileName.ToString();

                        var uploader = new List<Uploader> { new Uploader() };
                        uploader.FirstOrDefault().FileName = httpPostedFile.FileName;
                        uploader.FirstOrDefault().RelativePath = "~/images/AssigmentImages";


                        uploader.FirstOrDefault().SaveFileURL = ImageUrl;

                        uploader = AsyncContext.Run(()=> FileUploadHelper.UploadFileToOtherApi(uploader));



                    }
                }
                Ares res = new Ares
                {
                    Status = true
                };
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                logger.Error("Error in  image upload " + ex.Message);
                logger.Info("End  image upload: ");
                Ares res = new Ares
                {
                    Status = false
                };
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, res);
            }
        }
    }
    public class Ares
    {
        public bool Status { get; set; }
    }
}