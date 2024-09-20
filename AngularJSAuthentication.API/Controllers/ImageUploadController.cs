using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using NLog;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/imageupload")]
    public class ImageUploadController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();
        [HttpPost]
        public async Task<HttpResponseMessage> UploadFile()
        {
            UploadImageResponse uploadImageResponse = new UploadImageResponse { status = false, Name = "" };

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext context = new AuthContext())
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    // Get the uploaded image from the Files collection
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/UploadedImages")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/UploadedImages"));
                        // Validate the uploaded image(optional)
                        // Get the complete file path
                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string filename = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                        var ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedImages"), filename);

                        httpPostedFile.SaveAs(ImageUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadedImages", ImageUrl);
                        if (ImageUrl != null)
                        {
                            uploadImageResponse.Name = filename;
                            uploadImageResponse.status = true;
                        }

                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, uploadImageResponse);
            }

        }


        [Route("UploadShopCloseFile")]
        [HttpPost]
        public async Task<HttpResponseMessage> UploadShopCloseFile()
        {
            UploadImageResponse uploadImageResponse = new UploadImageResponse { status = false, Name = "" };

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext context = new AuthContext())
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    // Get the uploaded image from the Files collection
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/images/ShopCloseImage")))
                                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/images/ShopCloseImage"));

                            // Validate the uploaded image(optional)
                            // Get the complete file path
                            string extension = Path.GetExtension(httpPostedFile.FileName);

                        string filename = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                        var ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/ShopCloseImage"), filename);

                        httpPostedFile.SaveAs(ImageUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/images/ShopCloseImage", ImageUrl);
                        if (ImageUrl != null)
                        {
                            uploadImageResponse.Name = filename;
                            uploadImageResponse.status = true;
                        }

                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, uploadImageResponse);
            }

        }


        /// <summary>
        /// Upload image on folder: images\Customerdoc
        /// </summary>
        [Route("customerUpload")]
        [HttpPost]
        public void UploadFilecustomer()
        {
            logger.Info("start image upload");
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    // Get the uploaded image from the Files collection
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string filename = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                        // Validate the uploaded image(optional)
                        // Get the complete file path
                        var ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/Customerdoc"), filename);
                        httpPostedFile.SaveAs(ImageUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/images/Customerdoc", ImageUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in  image upload " + ex.Message);
                logger.Info("End  image upload: ");
            }
        }
        #region Customer Pan Upload
        /// <summary>
        /// Upload image on folder: images\CustomerPan
        /// </summary>
        [Route("customerPan")]
        [HttpPost]
        public async Task<HttpResponseMessage> UploadPancustomer()
        {
            UploadImageResponse uploadImageResponse = new UploadImageResponse { status = false, Name = "" };

            string CustPanurl = "", returnPath = "";
            logger.Info("start image upload");
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string filename = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                        // Validate the uploaded image(optional)
                        // Get the complete file path
                        var ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/CustomerPan"), filename);

                        returnPath = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                     , HttpContext.Current.Request.Url.DnsSafeHost
                                                                     , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                     , "/images/CustomerPan/" + filename);
                        httpPostedFile.SaveAs(ImageUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/images/CustomerPan", ImageUrl);
                        if (ConfigurationManager.AppSettings["Environment"] == "Production")
                        {
                            CloudinaryDotNet.Account account = new CloudinaryDotNet.Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                            Cloudinary cloudinary = new Cloudinary(account);

                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(ImageUrl),
                                PublicId = "My_Udhaar/Customer_Pan/" + filename,
                                Overwrite = true,
                                Invalidate = true,
                                Backup = false

                            };

                            var uploadResult = cloudinary.Upload(uploadParams);
                            if (uploadResult.SecureUri != null && !string.IsNullOrEmpty(uploadResult.SecureUri.ToString()))
                            {
                                CustPanurl = uploadResult.SecureUri.ToString();
                                uploadImageResponse.Name = filename;
                                uploadImageResponse.status = true;
                                if (System.IO.File.Exists(ImageUrl))
                                {
                                    System.IO.File.Delete(ImageUrl);
                                }
                            }
                        }
                        else
                        {
                            CustPanurl = returnPath;
                        }
                        if (!string.IsNullOrEmpty(CustPanurl))
                        {
                            uploadImageResponse.Name = CustPanurl;
                            uploadImageResponse.status = true;
                        }

                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, uploadImageResponse);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion

        #region Customer Idproff Upload
        /// <summary>
        /// Upload image on folder: images\CustomerIdproff
        /// </summary>
        [Route("customerIdProff")]
        [HttpPost]
        public async Task<HttpResponseMessage> UploadIdProffcustomer()
        {
            UploadImageResponse uploadImageResponse = new UploadImageResponse { status = false, Name = "" };

            string CustAddProff = "", returnPath = "";

            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    using (AuthContext context = new AuthContext())
                    {
                        // Get the uploaded image from the Files collection
                        var httpPostedFile = HttpContext.Current.Request.Files["file"];
                        if (httpPostedFile != null)
                        {
                            string extension = Path.GetExtension(httpPostedFile.FileName);

                            string filename = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                            // Validate the uploaded image(optional)
                            // Get the complete file path
                            var ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/CustomerIdProff"), filename);
                            // returnPath = ImageUrl;
                            returnPath = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                    , HttpContext.Current.Request.Url.DnsSafeHost
                                                                    , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                    , "/images/CustomerIdProff/" + filename);
                            httpPostedFile.SaveAs(ImageUrl);

                            AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/images/CustomerIdProff", ImageUrl);
                            if (ConfigurationManager.AppSettings["Environment"] == "Production")
                            {
                                CloudinaryDotNet.Account account = new CloudinaryDotNet.Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                                Cloudinary cloudinary = new Cloudinary(account);

                                var uploadParams = new ImageUploadParams()
                                {
                                    File = new FileDescription(ImageUrl),
                                    PublicId = "My_Udhaar/Customer_Id_Proff/" + filename,
                                    Overwrite = true,
                                    Invalidate = true,
                                    Backup = false
                                };

                                var uploadResult = cloudinary.Upload(uploadParams);
                                if (uploadResult.SecureUri != null && !string.IsNullOrEmpty(uploadResult.SecureUri.ToString()))
                                {
                                    CustAddProff = uploadResult.SecureUri.ToString();
                                    uploadImageResponse.Name = filename;
                                    uploadImageResponse.status = true;
                                    if (System.IO.File.Exists(ImageUrl))
                                    {
                                        System.IO.File.Delete(ImageUrl);
                                    }
                                }
                            }
                            else
                            {
                                CustAddProff = returnPath;
                            }
                            if (!string.IsNullOrEmpty(CustAddProff))
                            {
                                uploadImageResponse.Name = CustAddProff;
                                uploadImageResponse.status = true;
                            }
                        }
                    }

                }
                return Request.CreateResponse(HttpStatusCode.OK, uploadImageResponse);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion
        #region Customer Back Upload
        /// <summary>
        /// Upload image on folder: images\CustomerPan
        /// </summary>
        [Route("customerBack")]
        [HttpPost]
        public async Task<HttpResponseMessage> UploadBackcustomer()
        {
            UploadImageResponse uploadImageResponse = new UploadImageResponse { status = false, Name = "" };

            string CustBackurl = "", returnPath = "";
            logger.Info("start image upload");
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    // Get the uploaded image from the Files collection
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string filename = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                        // Validate the uploaded image(optional)
                        // Get the complete file path
                        var ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/BackImage"), filename);
                        //returnPath = "/images/BackImage"+filename;
                        httpPostedFile.SaveAs(ImageUrl);
                        returnPath = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                    , HttpContext.Current.Request.Url.DnsSafeHost
                                                                    , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                    , "/images/BackImage/" + filename);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/images/BackImage", ImageUrl);
                        if (ConfigurationManager.AppSettings["Environment"] == "Production")
                        {
                            CloudinaryDotNet.Account account = new CloudinaryDotNet.Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                            Cloudinary cloudinary = new Cloudinary(account);

                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(ImageUrl),
                                PublicId = "My_Udhaar/Back_Image/" + filename,
                                Overwrite = true,
                                Invalidate = true,
                                Backup = false
                            };

                            var uploadResult = cloudinary.Upload(uploadParams);
                            if (uploadResult.SecureUri != null && !string.IsNullOrEmpty(uploadResult.SecureUri.ToString()))
                            {
                                CustBackurl = uploadResult.SecureUri.ToString();
                                uploadImageResponse.Name = filename;
                                uploadImageResponse.status = true;
                                if (System.IO.File.Exists(ImageUrl))
                                {
                                    System.IO.File.Delete(ImageUrl);
                                }
                            }
                        }
                        else
                        {
                            CustBackurl = returnPath;
                        }
                        if (!string.IsNullOrEmpty(CustBackurl))
                        {
                            uploadImageResponse.Name = CustBackurl;
                            uploadImageResponse.status = true;
                        }
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, uploadImageResponse);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion

        /// <summary>
        /// Home section Image upload
        /// </summary>
        //[Route("HomeSectionImages")]
        //[HttpPost]
        //public void homesectionimgupload()
        //{

        //    logger.Info("start image upload");
        //    try
        //    {

        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in  image upload " + ex.Message);
        //        logger.Info("End  image upload: ");
        //    }
        //}

        [Route("HomeSectionImages")]
        [HttpPost]
        public async Task<string> homesectionimgupload()
        {
            string LogoUrl = "", returnPath = "";
            logger.Info("start image upload");
            try
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string filename = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                        //var filename = httpPostedFile.FileName + "_" + DateTime.Now.ToString("ddMMyyyyhhmmss");
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedLogos"), filename);
                        returnPath = "/UploadedLogos/" + filename;

                        //string path2 = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                        //                                            , HttpContext.Current.Request.Url.DnsSafeHost
                        //                                            , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                        //                                            , "/UploadedLogos/" + filename); /////pz

                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadedLogos", LogoUrl);
                        //LogoUrl = Startup.BaseUrl + "UploadedLogos/" + httpPostedFile.FileName;
                        if (ConfigurationManager.AppSettings["Environment"] == "Production")
                        {
                            CloudinaryDotNet.Account account = new CloudinaryDotNet.Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);
                            Cloudinary cloudinary = new Cloudinary(account);
                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(LogoUrl),
                                PublicId = "AppHome/" + filename,
                                Overwrite = true,
                                Invalidate = true,
                                Backup = false
                            };
                            var uploadResult = cloudinary.Upload(uploadParams);

                            if (uploadResult.SecureUri != null && !string.IsNullOrEmpty(uploadResult.SecureUri.ToString()))
                            {
                                System.IO.File.Delete(LogoUrl);
                                LogoUrl = uploadResult.SecureUri.ToString();
                            }
                        }
                        else
                        {
                            LogoUrl = returnPath;
                                //LogoUrl = path2; ////pz
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in image upload " + ex.Message);
            }
            return LogoUrl;
        }
    }

    public class UploadImageResponse
    {
        public string Name { get; set; }
        public bool status { get; set; }
    }
}
