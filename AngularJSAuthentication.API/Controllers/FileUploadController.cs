using AngularJSAuthentication.DataContracts.FileUpload;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/FileUploadToOther")]
    public class FileUploadController : ApiController
    {
        [Route("")]
        [HttpPost]
        public List<Uploader> UploadDocument()
        {
            List<string> fileUrls = new List<string>();
            List<Uploader> uploadList = new List<Uploader>();
            for (int i = 0; i < HttpContext.Current.Request.Files.Count; i++)
            {
                uploadList = this.GetJsonDataHeader<List<Uploader>>(); ;

                var httpPostedFile = HttpContext.Current.Request.Files[i];
                if (httpPostedFile != null)
                {
                    var upload = uploadList.FirstOrDefault(x => x.FileName == httpPostedFile.FileName);
                    if (upload != null)
                    {
                        string ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath(upload.RelativePath), httpPostedFile.FileName);

                        if (!Directory.Exists(HttpContext.Current.Server.MapPath(upload.RelativePath)))
                        {
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath(upload.RelativePath));
                        }

                        var fileUrl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                               , HttpContext.Current.Request.Url.DnsSafeHost
                                                               , HttpContext.Current.Request.Url.Port
                                                               , string.Format("{0}/{1}", upload.RelativePath.Replace("~/", ""), httpPostedFile.FileName));
                        httpPostedFile.SaveAs(ImageUrl);

                        upload.AbsoluteUrl = fileUrl;

                    }
                }
            }

            return uploadList;

        }


        private T GetJsonDataHeader<T>()
        {
            IEnumerable<string> headerCollection;

            if (!this.Request.Headers.TryGetValues("X-JsonData", out headerCollection))
            {
                return default(T);
            }

            var headerItems = headerCollection.ToList();

            if (headerItems.Count() != 1)
            {
                return default(T);
            }

            var meta = headerItems.FirstOrDefault();

            return !string.IsNullOrWhiteSpace(meta) ? JsonConvert.DeserializeObject<T>(meta) : default(T);
        }
    }

}