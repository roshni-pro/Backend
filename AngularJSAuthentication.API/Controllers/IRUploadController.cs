using NLog;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/IRUpload")]
    public class aIRUploadpostController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
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
                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/images/GrDraftInvoices")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/images/GrDraftInvoices"));
                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        filename = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;


                        ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/GrDraftInvoices"), filename);
                        httpPostedFile.SaveAs(ImageUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/images/GrDraftInvoices", ImageUrl);
                        res res = new res
                        {
                            path = filename,
                            Status = true
                        };
                        return Request.CreateResponse(System.Net.HttpStatusCode.OK, res);
                    }
                    else
                    {
                        res res = new res
                        {
                            path = null,
                            Status = false
                        };
                        return Request.CreateResponse(System.Net.HttpStatusCode.OK, res);
                    }
                }
                else
                {
                    res res = new res
                    {
                        path = null,
                        Status = false
                    };
                    return Request.CreateResponse(System.Net.HttpStatusCode.OK, res);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in  image upload " + ex.Message);
                logger.Info("End  image upload: ");
                res res = new res
                {
                    path = null,
                    Status = false
                };
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, res);
            }
        }
    }
    public class res
    {
        public string path { get; set; }
        public bool Status { get; set; }
    }
}