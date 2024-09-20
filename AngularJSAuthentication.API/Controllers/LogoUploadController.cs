using AngularJSAuthentication.Model;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using GenricEcommers.Models;
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
    [RoutePrefix("api/logoUpload")]
    public class LogoUploadController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private DateTime indianTime;

        [HttpPost]
        public IHttpActionResult UploadFile()
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
                        // Get the complete file path
                        var LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedLogos"), httpPostedFile.FileName);

                        string returnPath = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                       , HttpContext.Current.Request.Url.DnsSafeHost
                                                                       , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                       , "/UploadedLogos/" + httpPostedFile.FileName);

                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/UploadedLogos", LogoUrl);

                        if (ConfigurationManager.AppSettings["Environment"] == "Production")
                        {
                            Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);
                            Cloudinary cloudinary = new Cloudinary(account);
                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(LogoUrl),
                                PublicId = "home/" + httpPostedFile.FileName,
                                Overwrite = true,
                                Invalidate = true,
                                Backup = false
                            };
                            var uploadResult = cloudinary.Upload(uploadParams);
                            if (System.IO.File.Exists(LogoUrl))
                            {
                                System.IO.File.Delete(LogoUrl);
                            }
                            Logourl = Convert.ToString(uploadResult.SecureUri);
                        }
                        else
                        {
                            Logourl = returnPath; ;
                        }
                        if (!string.IsNullOrEmpty(Logourl))
                        {
                            return Created<string>(Logourl, Logourl);

                        }
                        //return Logourl;
                    }
                    return BadRequest();
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }
            return BadRequest();

        }

        [Route("UploadCategoryImage")]
        [HttpPost]
        public string UploadCategoryImage()
        {
            string Logourl = "";
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
                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string filename = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                        // Get the complete file path
                        var LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedLogos"), filename);

                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadedLogos", LogoUrl);
                        if (ConfigurationManager.AppSettings["Environment"] == "Production")
                        {
                            Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                            Cloudinary cloudinary = new Cloudinary(account);

                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(LogoUrl),
                                PublicId = "category/" + httpPostedFile.FileName,
                                Overwrite = true,
                                Invalidate = true,
                                Backup = false
                            };

                            var uploadResult = cloudinary.Upload(uploadParams);

                            if (System.IO.File.Exists(LogoUrl))
                            {
                                System.IO.File.Delete(LogoUrl);
                            }

                            Logourl = Convert.ToString(uploadResult.SecureUri);
                        }
                        else
                        {
                            Logourl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                    , HttpContext.Current.Request.Url.DnsSafeHost
                                                                    , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                    , "/UploadedLogos/" + filename);

                        }
                        return Logourl;
                    }
                    return Logourl;
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

        [Route("UploadSubCategoryImage")]
        [HttpPost]
        public string UploadSubCategoryImage()
        {
            string Logourl = "";
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
                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string filename = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;


                        // Get the complete file path
                        var LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedLogos"), filename);



                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadedLogos", LogoUrl);

                        if (ConfigurationManager.AppSettings["Environment"] == "Production")
                        {
                            Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                            Cloudinary cloudinary = new Cloudinary(account);

                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(LogoUrl),
                                PublicId = "sub-category/" + httpPostedFile.FileName,
                                Overwrite = true,
                                Invalidate = true,
                                Backup = false
                            };

                            var uploadResult = cloudinary.Upload(uploadParams);

                            if (System.IO.File.Exists(LogoUrl))
                            {
                                System.IO.File.Delete(LogoUrl);
                            }

                            Logourl = Convert.ToString(uploadResult.SecureUri);
                        }
                        else
                        {
                            Logourl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                    , HttpContext.Current.Request.Url.DnsSafeHost
                                                                    , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                    , "/UploadedLogos/" + filename);

                        }
                        return Logourl;
                    }
                    return Logourl;
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

        [Route("UploadSubSubCategoryImage")]
        [HttpPost]
        public string UploadSubSubCategoryImage()
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

                    string extension = Path.GetExtension(httpPostedFile.FileName);

                    string filename = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                    var LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedLogos"), filename);


                    httpPostedFile.SaveAs(LogoUrl);
                    AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadedLogos", LogoUrl);

                    if (ConfigurationManager.AppSettings["Environment"] == "Production")
                    {
                        Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                        Cloudinary cloudinary = new Cloudinary(account);

                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(LogoUrl),
                            PublicId = "sub sub/" + httpPostedFile.FileName,
                            Overwrite = true,
                            Invalidate = true,
                            Backup = false
                        };

                        var uploadResult = cloudinary.Upload(uploadParams);

                        if (System.IO.File.Exists(LogoUrl))
                        {
                            System.IO.File.Delete(LogoUrl);
                        }

                        Logourl = Convert.ToString(uploadResult.SecureUri);
                    }
                    else
                    {
                        Logourl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                , HttpContext.Current.Request.Url.DnsSafeHost
                                                                , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                , "/UploadedLogos/" + filename);

                    }
                    return Logourl;
                }
                return Logourl;
            }
            return Logourl;

        }

        [Route("UploadOfferImage")]
        [HttpPost]
        public void OfferUploadFile() // Upload Offer Image
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
                    // Get the uploaded image from the Files collection
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];

                    if (httpPostedFile != null)
                    {
                        // Validate the uploaded image(optional)
                        // Get the complete file path
                        var LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/offerimages"), httpPostedFile.FileName);
                        //var ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles"), httpPostedFile.FileName);
                        //string physicalPath = ("~/images/" + ImageName);
                        //httpPostedFile.SaveAs(physicalPath);
                        //customer newRecord = new customer();
                        //newRecord.username = customer.username;
                        ////.......saving picture url......
                        //newRecord.picture = physicalPath;

                        //// Save the uploaded file to "UploadedFiles" folder
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/offerimages", LogoUrl);

                        logger.Info("Uploded Offer Image on: " + LogoUrl);

                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }
        }
        [Route("UploadDocument")]
        [HttpPost]
        public void UploadDocument()
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
                    // Get the uploaded image from the Files collection
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];

                    if (httpPostedFile != null)
                    {
                        // Validate the uploaded image(optional)

                        // Get the complete file path
                        var LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadDocuments"), httpPostedFile.FileName);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/UploadDocuments", LogoUrl);

                        Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                        Cloudinary cloudinary = new Cloudinary(account);

                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(LogoUrl),
                            PublicId = "items_images/item_1/" + httpPostedFile.FileName,
                            Overwrite = true,
                            Invalidate = true,
                            Backup = false
                        };

                        var uploadResult = cloudinary.Upload(uploadParams);

                        if (System.IO.File.Exists(LogoUrl))
                        {
                            System.IO.File.Delete(LogoUrl);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }
        }

        [Route("UploadQuestionImage")]
        [HttpPost]
        public string UploadQuestionImage()
        {
            string LogoUrl = "";
            string ImageUrl = "";
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
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedLogos"), httpPostedFile.FileName);

                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/UploadedLogos", LogoUrl);

                        Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                        Cloudinary cloudinary = new Cloudinary(account);

                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(LogoUrl),
                            PublicId = "Questions/" + httpPostedFile.FileName,
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
            }
            return ImageUrl;
        }

        [Route("UploadPeopleDocument")]
        [HttpPost]
        public string UploadPeopleDocument()
        {
            string LogoUrl = "";
            string ImageUrl = "";

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
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadDocuments"), httpPostedFile.FileName);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/UploadDocuments", LogoUrl);

                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }
            return ImageUrl;
        }

        #region  to save customer Shop images
        /// <summary>
        /// Created by tejas 18-06-2019
        /// Get Customer udhar limits.
        /// </summary>
        /// <returns></returns>
        [Route("UploadCXimagesSHOP")]
        [HttpPost]
        public string UploadCXimagesSHOP(int CustomerId)
        {
            string LogoUrl = "";
            string ImageUrl = "";
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
                        var filename = "SHOP_" + httpPostedFile.FileName;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedImages"), filename);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/UploadedImages", LogoUrl);

                        using (var con = new AuthContext())
                        {
                            Customer c = con.Customers.Where(s => s.CustomerId == CustomerId).FirstOrDefault();
                            c.Shopimage = filename;
                            con.Customers.Attach(c);
                            con.Entry(c).State = System.Data.Entity.EntityState.Modified;
                            con.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }
            return ImageUrl;
        }
        #endregion

        #region  to save customer GST images
        /// <summary>
        /// Created by tejas 18-06-2019
        /// Get Customer udhar limits.
        /// </summary>
        /// <returns></returns>
        [Route("UploadCXimagesGST")]
        [HttpPost]
        public string UploadCXimagesGST(int CustomerId)
        {
            string LogoUrl = "";
            string ImageUrl = "";
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
                        var filename = "GST_" + httpPostedFile.FileName;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedImages"), filename);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/UploadedImages", LogoUrl);


                        using (var con = new AuthContext())
                        {
                            Customer c = con.Customers.Where(s => s.CustomerId == CustomerId).FirstOrDefault();
                            c.UploadGSTPicture = filename;
                            con.Customers.Attach(c);
                            con.Entry(c).State = System.Data.Entity.EntityState.Modified;
                            con.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }
            return ImageUrl;
        }
        #endregion

        #region  to save customer UploadRegistration images
        /// <summary>
        /// Created by tejas 18-06-2019
        /// Get Customer udhar limits.
        /// </summary>
        /// <returns></returns>
        [Route("UploadCXimagesREG")]
        [HttpPost]
        public string UploadCXimagesREG(int CustomerId)
        {
            string LogoUrl = "";
            string ImageUrl = "";
            logger.Info("start customer images upload");
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
                        var filename = "REG_" + httpPostedFile.FileName;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedImages"), filename);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadedImages", LogoUrl);

                        using (var con = new AuthContext())
                        {
                            Customer c = con.Customers.Where(s => s.CustomerId == CustomerId).FirstOrDefault();
                            c.UploadRegistration = filename;
                            con.Customers.Attach(c);
                            con.Entry(c).State = System.Data.Entity.EntityState.Modified;
                            con.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }
            return ImageUrl;
        }
        #endregion

        #region  Save image of Id Proof
        [Route("UploadId_Proof")]
        [HttpPost]
        public string UploadIdProof(string PeopleID)
        {
            string LogoUrl = "";
            string ImageUrl = "";
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
                        var filename = "IdProof_" + httpPostedFile.FileName;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadDocuments"), filename);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        httpPostedFile.SaveAs(LogoUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadDocuments", LogoUrl);

                        using (var con = new AuthContext())
                        {
                            PeopleDocument PDC = con.PeopleDocumentDB.Where(s => s.PeopleId == PeopleID).FirstOrDefault();
                            if (PDC == null)
                            {
                                PeopleDocument PC = new PeopleDocument();
                                PC.PeopleId = PeopleID;
                                PC.Id_Proof = filename;
                                con.PeopleDocumentDB.Add(PC);
                                con.Commit();
                            }
                            else
                            {
                                PDC.Id_Proof = filename;
                                con.PeopleDocumentDB.Attach(PDC);
                                con.Entry(PDC).State = System.Data.Entity.EntityState.Modified;
                                con.Commit();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }
            return ImageUrl;
        }

        #endregion


        #region  Save image of AddressProof
        [Route("UploadAddressProof")]
        [HttpPost]
        public string UploadAddressProof(string PeopleID)
        {
            string LogoUrl = "";
            string ImageUrl = "";
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
                        var filename = "AddressProof_" + httpPostedFile.FileName;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadDocuments"), filename);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadDocuments", LogoUrl);

                        using (var con = new AuthContext())
                        {
                            PeopleDocument PDC = con.PeopleDocumentDB.Where(s => s.PeopleId == PeopleID).FirstOrDefault();
                            if (PDC == null)
                            {
                                PeopleDocument PC = new PeopleDocument();
                                PC.PeopleId = PeopleID;
                                PC.Address_Proof = filename;
                                con.PeopleDocumentDB.Add(PC);
                                con.Commit();
                            }
                            else
                            {
                                PDC.Address_Proof = filename;
                                con.PeopleDocumentDB.Attach(PDC);
                                con.Entry(PDC).State = System.Data.Entity.EntityState.Modified;
                                con.Commit();
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }
            return ImageUrl;
        }




        #endregion

        #region  Save image of Marksheet
        [Route("UploadMarksheet")]
        [HttpPost]
        public string UploadMarksheet(string PeopleID)
        {
            string LogoUrl = "";
            string ImageUrl = "";
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
                        var filename = "Marksheet_" + httpPostedFile.FileName;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadDocuments"), filename);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadDocuments", LogoUrl);

                        using (var con = new AuthContext())
                        {
                            PeopleDocument PDC = con.PeopleDocumentDB.Where(s => s.PeopleId == PeopleID).FirstOrDefault();
                            if (PDC == null)
                            {
                                PeopleDocument PC = new PeopleDocument();
                                PC.PeopleId = PeopleID;
                                PC.MarkSheet = filename;
                                con.PeopleDocumentDB.Add(PC);
                                con.Commit();
                            }
                            else
                            {
                                PDC.MarkSheet = filename;
                                con.PeopleDocumentDB.Attach(PDC);
                                con.Entry(PDC).State = System.Data.Entity.EntityState.Modified;
                                con.Commit();
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }
            return ImageUrl;
        }




        #endregion

        #region  Save image of UploadPreSalary
        [Route("UploadPreSalary")]
        [HttpPost]
        public string UploadPreSalary(string PeopleID)
        {
            string LogoUrl = "";
            string ImageUrl = "";
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
                        var filename = "PreSalary_" + httpPostedFile.FileName;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadDocuments"), filename);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadDocuments", LogoUrl);

                        using (var con = new AuthContext())
                        {
                            PeopleDocument PDC = con.PeopleDocumentDB.Where(s => s.PeopleId == PeopleID).FirstOrDefault();
                            if (PDC == null)
                            {
                                PeopleDocument PC = new PeopleDocument();
                                PC.PeopleId = PeopleID;
                                PC.Pre_SalarySlip = filename;
                                con.PeopleDocumentDB.Add(PC);
                                con.Commit();
                            }
                            else
                            {
                                PDC.Pre_SalarySlip = filename;
                                con.PeopleDocumentDB.Attach(PDC);
                                con.Entry(PDC).State = System.Data.Entity.EntityState.Modified;
                                con.Commit();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }
            return ImageUrl;
        }

        #endregion

        #region upload file from suppiler app
        /// <summary>
        /// tejas 25-07-2019  upload file from suppiler app
        /// </summary>
        /// <returns></returns>
        [Route("UploadImageForSupplier")]
        [HttpPost]
        [AllowAnonymous] // remove in final build
        public async Task<HttpResponseMessage> UploadImageForSupplier()
        {
            try
            {
                string LogoUrl = "";
                var fileuploadPath = HttpContext.Current.Server.MapPath("~/UploadedImages/ImagesfromSupplierApp");

                if (!Directory.Exists(fileuploadPath))
                {
                    Directory.CreateDirectory(fileuploadPath);
                }

                var provider = new MultipartFormDataStreamProvider(fileuploadPath);
                var content = new StreamContent(HttpContext.Current.Request.GetBufferlessInputStream(true));
                foreach (var header in Request.Content.Headers)
                {
                    content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                await content.ReadAsMultipartAsync(provider);

                string uploadingFileName = provider.FileData.Select(x => x.LocalFileName).FirstOrDefault();
                string originalFileName = String.Concat(fileuploadPath, "\\" + (provider.Contents[0].Headers.ContentDisposition.FileName).Trim(new Char[] { '"' }));

                if (File.Exists(originalFileName))
                {
                    File.Delete(originalFileName);
                }
                File.Move(uploadingFileName, originalFileName);


                string filename = Path.GetFileName(originalFileName);
                var response = new
                {
                    LogoUrl = "/UploadedImages/ImagesfromSupplierApp/" + filename,
                    Status = true
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Message = ex,
                    Status = false
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }
        #endregion

        [Route("UploadBaseCategoryImageV7")]
        [HttpPost]
        public IHttpActionResult UploadBaseCategoryImageV7()
        {
            string Logourl = "";
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
                        var LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedLogos"), httpPostedFile.FileName);

                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/UploadedLogos", LogoUrl);

                        Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                        Cloudinary cloudinary = new Cloudinary(account);

                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(LogoUrl),
                            PublicId = "home/" + httpPostedFile.FileName.Split('.')[0],
                            Overwrite = true,
                            Invalidate = true,
                            Backup = false
                        };

                        var uploadResult = cloudinary.Upload(uploadParams);

                        if (System.IO.File.Exists(LogoUrl))
                        {
                            System.IO.File.Delete(LogoUrl);
                        }

                        Logourl = Convert.ToString(uploadResult.SecureUri);
                        return Created<string>(Logourl, Logourl);
                    }
                    return BadRequest();
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }
            return BadRequest();
        }

        [Route("UploadCategoryImageV7")]
        [HttpPost]
        public IHttpActionResult UploadCategoryImageV7()
        {
            string CustPanurl = "", returnPath = "";
            logger.Info("start logo upload");
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
                        var ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedLogos"), filename);
                        returnPath = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                     , HttpContext.Current.Request.Url.DnsSafeHost
                                                                     , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                     , "/UploadedLogos/" + filename);
                        httpPostedFile.SaveAs(ImageUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadedLogos", ImageUrl);
                        if (ConfigurationManager.AppSettings["Environment"] == "Production")
                        {
                            CloudinaryDotNet.Account account = new CloudinaryDotNet.Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                            Cloudinary cloudinary = new Cloudinary(account);

                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(ImageUrl),
                                PublicId = "sub-category/" + httpPostedFile.FileName.Split('.')[0],
                                Overwrite = true,
                                Invalidate = true,
                                Backup = false

                            };

                            var uploadResult = cloudinary.Upload(uploadParams);
                            if (uploadResult.SecureUri != null && !string.IsNullOrEmpty(uploadResult.SecureUri.ToString()))
                            {
                                CustPanurl = uploadResult.SecureUri.ToString();

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
                            return Created<string>(CustPanurl, CustPanurl);
                        }
                    }
                    return BadRequest();
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }
            return BadRequest();
        }

        [Route("UploadSubCategoryImageV7")]
        [HttpPost]
        public IHttpActionResult UploadSubCategoryImageV7()
        {
            string CustPanurl = "", returnPath = "";
            logger.Info("start logo upload");
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
                        var ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedLogos"), filename);
                        returnPath = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                     , HttpContext.Current.Request.Url.DnsSafeHost
                                                                     , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                     , "/UploadedLogos/" + filename);
                        httpPostedFile.SaveAs(ImageUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadedLogos", ImageUrl);
                        if (ConfigurationManager.AppSettings["Environment"] == "Production")
                        {
                            CloudinaryDotNet.Account account = new CloudinaryDotNet.Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                            Cloudinary cloudinary = new Cloudinary(account);

                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(ImageUrl),
                                PublicId = "sub-category/" + httpPostedFile.FileName.Split('.')[0],
                                Overwrite = true,
                                Invalidate = true,
                                Backup = false

                            };

                            var uploadResult = cloudinary.Upload(uploadParams);
                            if (uploadResult.SecureUri != null && !string.IsNullOrEmpty(uploadResult.SecureUri.ToString()))
                            {
                                CustPanurl = uploadResult.SecureUri.ToString();

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
                            return Created<string>(CustPanurl, CustPanurl);
                        }
                    }
                    return BadRequest();
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }
            return BadRequest();
        }
        //[Route("UploadSubCategoryImageV7")]
        //[HttpPost]
        //public async Task<HttpResponseMessage> UploadSubCategoryImageV7()
        //{
        //    UploadImageResponse uploadImageResponse = new UploadImageResponse { status = false, Name = "" };

        //    string CustPanurl = "", returnPath = "";
        //    logger.Info("start image upload");
        //    try
        //    {
        //        if (HttpContext.Current.Request.Files.AllKeys.Any())
        //        {
        //            var httpPostedFile = HttpContext.Current.Request.Files["file"];
        //            if (httpPostedFile != null)
        //            {
        //                string extension = Path.GetExtension(httpPostedFile.FileName);

        //                string filename = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

        //                // Validate the uploaded image(optional)
        //                // Get the complete file path
        //                var ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedLogos"), filename);

        //                returnPath = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
        //                                                             , HttpContext.Current.Request.Url.DnsSafeHost
        //                                                             , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
        //                                                             , "/UploadedLogos/" + filename);
        //                httpPostedFile.SaveAs(ImageUrl);
        //                AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadedLogos", ImageUrl);
        //                if (ConfigurationManager.AppSettings["Environment"] == "Production")
        //                {
        //                    CloudinaryDotNet.Account account = new CloudinaryDotNet.Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

        //                    Cloudinary cloudinary = new Cloudinary(account);

        //                    var uploadParams = new ImageUploadParams()
        //                    {
        //                        File = new FileDescription(ImageUrl),
        //                        PublicId = "sub-category/" + httpPostedFile.FileName.Split('.')[0],
        //                        Overwrite = true,
        //                        Invalidate = true,
        //                        Backup = false

        //                    };

        //                    var uploadResult = cloudinary.Upload(uploadParams);
        //                    if (uploadResult.SecureUri != null && !string.IsNullOrEmpty(uploadResult.SecureUri.ToString()))
        //                    {
        //                        CustPanurl = uploadResult.SecureUri.ToString();
        //                        uploadImageResponse.Name = filename;
        //                        uploadImageResponse.status = true;
        //                        if (System.IO.File.Exists(ImageUrl))
        //                        {
        //                            System.IO.File.Delete(ImageUrl);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    CustPanurl = returnPath;
        //                }
        //                if (!string.IsNullOrEmpty(CustPanurl))
        //                {
        //                    uploadImageResponse.Name = CustPanurl;
        //                    uploadImageResponse.status = true;
        //                }

        //            }
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, uploadImageResponse);
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //    //string Logourl = "";
        //    //logger.Info("start logo upload");
        //    //try
        //    //{
        //    //    var identity = User.Identity as ClaimsIdentity;
        //    //    int compid = 0, userid = 0;

        //    //    foreach (Claim claim in identity.Claims)
        //    //    {
        //    //        if (claim.Type == "compid")
        //    //        {
        //    //            compid = int.Parse(claim.Value);
        //    //        }
        //    //        if (claim.Type == "userid")
        //    //        {
        //    //            userid = int.Parse(claim.Value);
        //    //        }
        //    //    }

        //    //    if (HttpContext.Current.Request.Files.AllKeys.Any())
        //    //    {
        //    //        // Get the uploaded image from the Files collection
        //    //        var httpPostedFile = HttpContext.Current.Request.Files["file"];

        //    //        if (httpPostedFile != null)
        //    //        {
        //    //            // Validate the uploaded image(optional)

        //    //            // Get the complete file path
        //    //            var LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedLogos"), httpPostedFile.FileName);

        //    //            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

        //    //            httpPostedFile.SaveAs(LogoUrl);
        //    //            AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/UploadedLogos", LogoUrl);

        //    //            Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

        //    //            Cloudinary cloudinary = new Cloudinary(account);

        //    //            var uploadParams = new ImageUploadParams()
        //    //            {
        //    //                File = new FileDescription(LogoUrl),
        //    //                PublicId = "sub-category/" + httpPostedFile.FileName.Split('.')[0],
        //    //                Overwrite = true,
        //    //                Invalidate = true,
        //    //                Backup = false
        //    //            };

        //    //            var uploadResult = cloudinary.Upload(uploadParams);

        //    //            if (System.IO.File.Exists(LogoUrl))
        //    //            {
        //    //                System.IO.File.Delete(LogoUrl);
        //    //            }

        //    //            Logourl = Convert.ToString(uploadResult.SecureUri);
        //    //            return Created<string>(Logourl, Logourl);
        //    //        }
        //    //        return BadRequest();
        //    //    }
        //    //    return BadRequest();
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    logger.Error("Error in  logo upload " + ex.Message);
        //    //    logger.Info("End   logo upload: ");
        //    //}
        //    //return BadRequest();
        //}

        [Route("UploadSubSubCategoryImageV7")]
        [HttpPost]
        public IHttpActionResult UploadSubSubCategoryImageV7()
        {
            string Logourl = "";
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
                        var LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedLogos"), httpPostedFile.FileName);


                        Logourl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                     , HttpContext.Current.Request.Url.DnsSafeHost
                                                                     , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                     , "/UploadedLogos/" + httpPostedFile.FileName);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/UploadedLogos", LogoUrl);
                        if (ConfigurationManager.AppSettings["Environment"] == "Production")
                        {
                            Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                            Cloudinary cloudinary = new Cloudinary(account);

                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(LogoUrl),
                                PublicId = "sub sub/" + httpPostedFile.FileName.Split('.')[0],
                                Overwrite = true,
                                Invalidate = true,
                                Backup = false
                            };

                            var uploadResult = cloudinary.Upload(uploadParams);

                            if (System.IO.File.Exists(LogoUrl))
                            {
                                System.IO.File.Delete(LogoUrl);
                            }

                            Logourl = Convert.ToString(uploadResult.SecureUri);
                        }
                        return Created<string>(Logourl, Logourl);
                    }
                    return BadRequest();
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }
            return BadRequest();
        }

        [Route("UploadId_ProofV7")]
        [HttpPost]
        public IHttpActionResult UploadId_ProofV7(string PeopleID)
        {
            string LogoUrl = "";
            string ImageUrl = "";
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
                        string TartgetfolderPath = "";
                        TartgetfolderPath = HttpContext.Current.Server.MapPath(@"~\UploadDocuments");
                        if (!Directory.Exists(TartgetfolderPath))
                            Directory.CreateDirectory(TartgetfolderPath);

                        var filename = "IdProof_" + httpPostedFile.FileName;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadDocuments"), filename);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadDocuments", LogoUrl);
                        using (var con = new AuthContext())
                        {
                            PeopleDocument PDC = con.PeopleDocumentDB.Where(s => s.PeopleId == PeopleID).FirstOrDefault();
                            if (PDC == null)
                            {
                                PeopleDocument PC = new PeopleDocument();
                                PC.PeopleId = PeopleID;
                                PC.Id_Proof = "/UploadDocuments/" + filename;
                                con.PeopleDocumentDB.Add(PC);
                                con.Commit();
                                return Created(PC.Id_Proof, PC.Id_Proof);
                            }
                            else
                            {
                                PDC.Id_Proof = "/UploadDocuments/" + filename;
                                con.PeopleDocumentDB.Attach(PDC);
                                con.Entry(PDC).State = System.Data.Entity.EntityState.Modified;
                                con.Commit();
                                return Created(PDC.Id_Proof, PDC.Id_Proof);
                            }
                        }

                    }
                    return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
                }
                return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
                return InternalServerError();
            }

        }

        [Route("UploadPanCard_ProofV7")]
        [HttpPost]
        public IHttpActionResult UploadPanCard_ProofV7(string PeopleID)
        {
            string LogoUrl = "";
            string ImageUrl = "";
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
                        var filename = "PanCardProof_" + httpPostedFile.FileName;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadDocuments"), filename);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadDocuments", LogoUrl);


                        using (var con = new AuthContext())
                        {
                            PeopleDocument PCP = con.PeopleDocumentDB.Where(s => s.PeopleId == PeopleID).FirstOrDefault();
                            if (PCP == null)
                            {
                                PeopleDocument PanC = new PeopleDocument();
                                PanC.PeopleId = PeopleID;
                                PanC.PanCard_Proof = "/UploadDocuments/" + filename;
                                con.PeopleDocumentDB.Add(PanC);
                                con.Commit();
                                return Created(PanC.PanCard_Proof, PanC.PanCard_Proof);
                            }
                            else
                            {

                                PCP.PanCard_Proof = "/UploadDocuments/" + filename;
                                con.PeopleDocumentDB.Attach(PCP);
                                con.Entry(PCP).State = System.Data.Entity.EntityState.Modified;
                                con.Commit();
                                return Created(PCP.PanCard_Proof, PCP.PanCard_Proof);
                            }
                        }

                    }
                    return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
                }
                return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                return InternalServerError();
            }

        }

        [Route("UploadAddressProofV7")]
        [HttpPost]
        public IHttpActionResult UploadAddressProofV7(string PeopleID)
        {
            string LogoUrl = "";
            string ImageUrl = "";
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
                        var filename = "AddressProof_" + httpPostedFile.FileName;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadDocuments"), filename);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadDocuments", LogoUrl);

                        using (var con = new AuthContext())
                        {
                            PeopleDocument PDC = con.PeopleDocumentDB.Where(s => s.PeopleId == PeopleID).FirstOrDefault();
                            if (PDC == null)
                            {
                                PeopleDocument PC = new PeopleDocument();
                                PC.PeopleId = PeopleID;
                                PC.Address_Proof = "/UploadDocuments/" + filename;
                                con.PeopleDocumentDB.Add(PC);
                                con.Commit();
                                return Created(PC.Address_Proof, PC.Address_Proof);
                            }
                            else
                            {
                                PDC.Address_Proof = "/UploadDocuments/" + filename;
                                con.PeopleDocumentDB.Attach(PDC);
                                con.Entry(PDC).State = System.Data.Entity.EntityState.Modified;
                                con.Commit();
                                return Created(PDC.Address_Proof, PDC.Address_Proof);
                            }

                        }
                    }
                    return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
                }
                return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
                return InternalServerError();
            }

        }

        [Route("UploadMarksheetV7")]
        [HttpPost]
        public IHttpActionResult UploadMarksheetV7(string PeopleID)
        {
            string LogoUrl = "";
            string ImageUrl = "";
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
                        var filename = "Marksheet_" + httpPostedFile.FileName;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadDocuments"), filename);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadDocuments", LogoUrl);

                        using (var con = new AuthContext())
                        {
                            PeopleDocument PDC = con.PeopleDocumentDB.Where(s => s.PeopleId == PeopleID).FirstOrDefault();
                            if (PDC == null)
                            {
                                PeopleDocument PC = new PeopleDocument();
                                PC.PeopleId = PeopleID;
                                PC.MarkSheet = "/UploadDocuments/" + filename;
                                con.PeopleDocumentDB.Add(PC);
                                con.Commit();
                                return Created(PC.MarkSheet, PC.MarkSheet);
                            }
                            else
                            {
                                PDC.MarkSheet = "/UploadDocuments/" + filename;
                                con.PeopleDocumentDB.Attach(PDC);
                                con.Entry(PDC).State = System.Data.Entity.EntityState.Modified;
                                con.Commit();
                                return Created(PDC.MarkSheet, PDC.MarkSheet);
                            }

                        }
                    }
                    return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
                }
                return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
                return InternalServerError();
            }

        }

        [Route("UploadPreSalaryV7")]
        [HttpPost]
        public IHttpActionResult UploadPreSalaryV7(string PeopleID)
        {
            string LogoUrl = "";
            string ImageUrl = "";
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
                        var filename = "PreSalary_" + httpPostedFile.FileName;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadDocuments"), filename);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadDocuments", LogoUrl);

                        using (var con = new AuthContext())
                        {
                            PeopleDocument PDC = con.PeopleDocumentDB.Where(s => s.PeopleId == PeopleID).FirstOrDefault();
                            if (PDC == null)
                            {
                                PeopleDocument PC = new PeopleDocument();
                                PC.PeopleId = PeopleID;
                                PC.Pre_SalarySlip = "/UploadDocuments/" + filename;
                                con.PeopleDocumentDB.Add(PC);
                                con.Commit();
                                return Created(PC.Pre_SalarySlip, PC.Pre_SalarySlip);
                            }
                            else
                            {
                                PDC.Pre_SalarySlip = "/UploadDocuments/" + filename;
                                con.PeopleDocumentDB.Attach(PDC);
                                con.Entry(PDC).State = System.Data.Entity.EntityState.Modified;
                                con.Commit();
                                return Created(PDC.Pre_SalarySlip, PDC.Pre_SalarySlip);
                            }
                        }
                    }
                    return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
                }
                return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
                return InternalServerError();
            }

        }

        [Route("UploadCXimagesSHOPV7")]
        [HttpPost]
        public IHttpActionResult UploadCXimagesSHOPV7(int CustomerId)
        {
            string LogoUrl = "";
            string ImageUrl = "";
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
                        var filename = "SHOP_" + httpPostedFile.FileName;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedImages"), filename);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadedImages", LogoUrl);

                        using (var con = new AuthContext())
                        {
                            Customer c = con.Customers.Where(s => s.CustomerId == CustomerId).FirstOrDefault();
                            c.Shopimage = filename;
                            con.Customers.Attach(c);
                            con.Entry(c).State = System.Data.Entity.EntityState.Modified;
                            con.Commit();
                            return Created(c.Shopimage, c.Shopimage);
                        }
                    }
                    return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
                }
                return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
                return InternalServerError();
            }
        }

        [Route("UploadCXimagesGSTV7")]
        [HttpPost]
        public IHttpActionResult UploadCXimagesGSTV7(int CustomerId)
        {
            string LogoUrl = "";
            string ImageUrl = "";
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
                        var filename = "GST_" + httpPostedFile.FileName;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedImages"), filename);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadedImages", LogoUrl);

                        using (var con = new AuthContext())
                        {
                            Customer c = con.Customers.Where(s => s.CustomerId == CustomerId).FirstOrDefault();
                            c.UploadGSTPicture = filename;
                            con.Customers.Attach(c);
                            con.Entry(c).State = System.Data.Entity.EntityState.Modified;
                            con.Commit();
                            return Created(c.UploadGSTPicture, c.UploadGSTPicture);
                        }
                    }
                    return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
                }
                return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
                return InternalServerError(); ;
            }

        }

        [Route("UploadOrangeBookV7")]
        [HttpPost]
        public IHttpActionResult UploadOrangeBookV7(string version)
        {
            string LogoUrl = "";
            logger.Info("start Orange Book Upload");

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                if (httpPostedFile != null)
                {
                    var fileuploadPath = HttpContext.Current.Server.MapPath("~/OrangeBook");

                    if (!Directory.Exists(fileuploadPath))
                    {
                        Directory.CreateDirectory(fileuploadPath);
                    }

                    string NewVersion = version + DateTime.Now.ToString("ddMMyyyyhhmmss");

                    var filename = NewVersion + ".pdf";
                    LogoUrl = Path.Combine(fileuploadPath, filename);
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    httpPostedFile.SaveAs(LogoUrl);
                    AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/OrangeBook", LogoUrl);

                    var response = new
                    {
                        Status = true,
                        FileName = filename
                    };
                    return Created(filename, response);
                }
                return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
            }
            return Created("Upload Unsuccessfull", "Upload Unsuccessfull");

        }

        [Route("UploadCXimagesREGV7")]
        [HttpPost]
        public IHttpActionResult UploadCXimagesREGV7(int CustomerId)
        {
            string LogoUrl = "";
            string ImageUrl = "";
            logger.Info("start customer images upload");
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
                        var filename = "REG_" + httpPostedFile.FileName;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedImages"), filename);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadedImages", LogoUrl);
                        using (var con = new AuthContext())
                        {
                            Customer c = con.Customers.Where(s => s.CustomerId == CustomerId).FirstOrDefault();
                            c.UploadRegistration = filename;
                            con.Customers.Attach(c);
                            con.Entry(c).State = System.Data.Entity.EntityState.Modified;
                            con.Commit();
                            return Created(c.UploadRegistration, c.UploadRegistration);
                        }
                    }
                    return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
                }
                return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
                return InternalServerError();
            }

        }
        [Route("UploadCustomerDoc")]
        [HttpPost]
        public IHttpActionResult UploadCustomerDoc(int value, int CustomerId)
        {
            string LogoUrl = "";
            string ImageUrl = "";
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
                if (value == 1)
                {
                    if (HttpContext.Current.Request.Files.AllKeys.Any())
                    {
                        var httpPostedFile = HttpContext.Current.Request.Files["file"];
                        if (httpPostedFile != null)
                        {
                            var filename = "SHOP_" + httpPostedFile.FileName;
                            string ext = Path.GetExtension(filename);
                            if (ext == ".jpg" || ext == ".png" || ext == ".jpeg")
                            {
                                LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedImages"), filename);
                                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                                httpPostedFile.SaveAs(LogoUrl);
                                AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadedImages", LogoUrl);

                                using (var con = new AuthContext())
                                {
                                    Customer c = con.Customers.Where(s => s.CustomerId == CustomerId).FirstOrDefault();
                                    c.Shopimage = filename;
                                    //con.Customers.Attach(c);
                                    con.Entry(c).State = System.Data.Entity.EntityState.Modified;
                                    con.Commit();
                                    CustomerDocument check = con.CustomerDocumentDB.Where(x => x.CustomerId == CustomerId).FirstOrDefault();
                                    if (check != null)
                                    {

                                        check.CustomerId = c.CustomerId;
                                        check.Shopimage = filename;
                                        check.UpdateDate = DateTime.Now;
                                        con.Commit();
                                    }
                                    else
                                    {
                                        CustomerDocument Doc = new CustomerDocument();
                                        Doc.CustomerId = c.CustomerId;
                                        Doc.Shopimage = filename;
                                        Doc.UpdateDate = DateTime.Now;
                                        con.CustomerDocumentDB.Add(Doc);
                                        con.Commit();
                                    }
                                    return Created(c.Shopimage, c.Shopimage);

                                }
                            }
                        }
                        return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
                    }
                    return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
                }
                if (value == 2)
                {
                    if (HttpContext.Current.Request.Files.AllKeys.Any())
                    {
                        var httpPostedFile = HttpContext.Current.Request.Files["file"];
                        if (httpPostedFile != null)
                        {
                            var filename = "GST_" + httpPostedFile.FileName;
                            string ext = Path.GetExtension(filename);
                            if (ext == ".jpg" || ext == ".png" || ext == ".jpeg")
                            {
                                LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedImages"), filename);
                                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                                httpPostedFile.SaveAs(LogoUrl);
                                AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadedImages", LogoUrl);

                                using (var con = new AuthContext())
                                {
                                    Customer c = con.Customers.Where(s => s.CustomerId == CustomerId).FirstOrDefault();
                                    c.UploadGSTPicture = filename;
                                    //con.Customers.Attach(c);
                                    con.Entry(c).State = System.Data.Entity.EntityState.Modified;
                                    con.Commit();
                                    CustomerDocument check = con.CustomerDocumentDB.Where(x => x.CustomerId == CustomerId).FirstOrDefault();
                                    if (check != null)
                                    {

                                        check.CustomerId = c.CustomerId;
                                        check.UploadGSTPicture = filename;
                                        check.UpdateDate = DateTime.Now;
                                        con.Commit();
                                    }
                                    else
                                    {
                                        CustomerDocument Doc = new CustomerDocument();
                                        Doc.CustomerId = c.CustomerId;
                                        Doc.UploadGSTPicture = filename;
                                        Doc.UpdateDate = DateTime.Now;
                                        con.CustomerDocumentDB.Add(Doc);
                                        con.Commit();
                                    }
                                    return Created(c.UploadGSTPicture, c.UploadGSTPicture);
                                }
                            }
                        }
                        return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
                    }
                    return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
                }
                if (value == 3)
                {
                    if (HttpContext.Current.Request.Files.AllKeys.Any())
                    {
                        var httpPostedFile = HttpContext.Current.Request.Files["file"];
                        if (httpPostedFile != null)
                        {
                            var filename = "REG_" + httpPostedFile.FileName;
                            string ext = Path.GetExtension(filename);
                            if (ext == ".jpg" || ext == ".png" || ext == ".jpeg")
                            {
                                LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedImages"), filename);
                                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                                httpPostedFile.SaveAs(LogoUrl);
                                AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadedImages", LogoUrl);

                                using (var con = new AuthContext())
                                {
                                    Customer c = con.Customers.Where(s => s.CustomerId == CustomerId).FirstOrDefault();
                                    c.UploadRegistration = filename;
                                    //con.Customers.Attach(c);
                                    con.Entry(c).State = System.Data.Entity.EntityState.Modified;
                                    con.Commit();
                                    CustomerDocument check = con.CustomerDocumentDB.Where(x => x.CustomerId == CustomerId).FirstOrDefault();
                                    if (check != null)
                                    {

                                        check.CustomerId = c.CustomerId;
                                        check.UploadRegistration = filename;
                                        check.UpdateDate = DateTime.Now;
                                        con.Commit();
                                    }
                                    else
                                    {
                                        CustomerDocument Doc = new CustomerDocument();
                                        Doc.CustomerId = c.CustomerId;
                                        Doc.UploadRegistration = filename;
                                        Doc.UpdateDate = DateTime.Now;
                                        con.CustomerDocumentDB.Add(Doc);
                                        con.Commit();
                                    }
                                    return Created(c.UploadRegistration, c.UploadRegistration);
                                }
                            }
                        }
                        return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
                    }
                    return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
                }
                return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
                return InternalServerError();
            }

        }


        [Route("UpdateComingFlashDealImage")]
        [HttpPost]
        public IHttpActionResult UpdateComingFlashDealImage(int WarehoueId)
        {
            string LogoUrl = "";
            string ImageUrl = "";
            string ImagePath = "";
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        var filename = "FDI_" + httpPostedFile.FileName;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedImages"), filename);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadedImages", LogoUrl);

                        using (var con = new AuthContext())
                        {
                            NoFlashDealImage check = con.NoFlashDealImage.Where(s => s.WarehouseId == WarehoueId).FirstOrDefault();

                            if (check != null)
                            {
                                ImagePath = filename;
                            }
                            else
                            {
                                ImagePath = filename;
                            }
                            return Created(ImagePath, ImagePath);
                        }
                    }
                    return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
                }
                return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
                return InternalServerError();
            }

        }

        [Route("UploadRDSImageV7")]
        [HttpPost]
        public IHttpActionResult UploadRDSImageV7()
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

                        // Get the complete file path
                        var filename = "RDS_" + httpPostedFile.FileName;
                        var LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedLogos"), filename);

                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadedImages", LogoUrl);

                        Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                        Cloudinary cloudinary = new Cloudinary(account);

                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(LogoUrl),
                            PublicId = "category/" + filename.Split('.')[0],
                            Overwrite = true,
                            Invalidate = true,
                            Backup = false
                        };

                        var uploadResult = cloudinary.Upload(uploadParams);

                        if (System.IO.File.Exists(LogoUrl))
                        {
                            System.IO.File.Delete(LogoUrl);
                        }

                        Logourl = Convert.ToString(uploadResult.SecureUri);
                        return Created<string>(Logourl, Logourl);
                    }
                    return BadRequest();
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }
            return BadRequest();
        }


        [Route("UploadMembershipLogoV7")]
        [HttpPost]
        public IHttpActionResult UploadMembershipLogoV7()
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

                        // Get the complete file path
                        var filename = "MemLogo_" + httpPostedFile.FileName;
                        var LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedImages"), filename);

                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadedImages", LogoUrl);

                        Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                        Cloudinary cloudinary = new Cloudinary(account);

                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(LogoUrl),
                            PublicId = "memberShip/" + filename.Split('.')[0],
                            Overwrite = true,
                            Invalidate = true,
                            Backup = false
                        };

                        var uploadResult = cloudinary.Upload(uploadParams);

                        if (System.IO.File.Exists(LogoUrl))
                        {
                            System.IO.File.Delete(LogoUrl);
                        }

                        Logourl = Convert.ToString(uploadResult.SecureUri);
                        return Created<string>(Logourl, Logourl);
                    }
                    return BadRequest();
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }
            return BadRequest();
        }

        [Route("StoreImageUpload")]
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult StoreImageUpload()
        {
            string LogoUrl = "";
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {

                        if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath("~/StoreImage")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/StoreImage"));

                        string extension = Path.GetExtension(httpPostedFile.FileName);
                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/StoreImage"), fileName);

                        httpPostedFile.SaveAs(LogoUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(fileName, "~/StoreImage", LogoUrl);

                        LogoUrl = "/StoreImage/" + fileName;

                    }

                }
                return Created<string>(LogoUrl, LogoUrl);
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        [Route("CommanCategoryImagesA7")]
        [HttpPost]
        public IHttpActionResult CommanCategoryImagesA7()
        {
            string Logourl = "";
            logger.Info("start logo upload");
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                // Get the uploaded image from the Files collection
                var httpPostedFile = HttpContext.Current.Request.Files["file"];

                if (httpPostedFile != null)
                {
                    // Validate the uploaded image(optional)
                    // Get the complete file path
                    var LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedLogos"), httpPostedFile.FileName);

                    string returnPath = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                   , HttpContext.Current.Request.Url.DnsSafeHost
                                                                   , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                   , "/UploadedLogos/" + httpPostedFile.FileName);

                    httpPostedFile.SaveAs(LogoUrl);
                    AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/UploadedLogos", LogoUrl);

                    if (ConfigurationManager.AppSettings["Environment"] == "Production")
                    {
                        Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);
                        Cloudinary cloudinary = new Cloudinary(account);
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(LogoUrl),
                            PublicId = "home/" + httpPostedFile.FileName,
                            Overwrite = true,
                            Invalidate = true,
                            Backup = false
                        };
                        var uploadResult = cloudinary.Upload(uploadParams);
                        if (System.IO.File.Exists(LogoUrl))
                        {
                            System.IO.File.Delete(LogoUrl);
                        }
                        Logourl = Convert.ToString(uploadResult.SecureUri);
                    }
                    else
                    {
                        Logourl = returnPath;
                    }
                    return Created<string>(Logourl, Logourl);
                }
            }
            return Created<string>(Logourl, Logourl);
        }
        [Route("UploadReatilerOfferImage")]
        [HttpPost]
        public string UploadReatilerOfferImage()
        {
            string Logourl = "";
            logger.Info("start logo upload");
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];

                    if (httpPostedFile != null)
                    {
                        var contenttype = Common.Helpers.MIMEAssistant.MIMETypesDictionary.FirstOrDefault(x => x.Value == httpPostedFile.ContentType);
                        // Get the complete file path
                        var LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/RetailerOffers"), httpPostedFile.FileName + "." + contenttype.Key);
                        httpPostedFile.SaveAs(LogoUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/images/RetailerOffers", LogoUrl);
                        Logourl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                           , HttpContext.Current.Request.Url.DnsSafeHost
                                                           , HttpContext.Current.Request.Url.Port
                                                           , string.Format("images/RetailerOffers/{0}", httpPostedFile.FileName + "." + contenttype.Key));
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

        [Route("UploadNewImage")]
        [HttpPost]
        public IHttpActionResult UploadNewImage()
        {
            string Logourl = "";
            string result = string.Empty;

            logger.Info("start logo upload");
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];

                    if (httpPostedFile != null)
                    {
                       
                        {
                            if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath("~/images/RetailerOffers")))
                                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/images/RetailerOffers"));

                            string extension = Path.GetExtension(httpPostedFile.FileName);
                            string Filename = DateTime.Now.ToString("ddMMyyyyhhmmss") + extension;

                            var contenttype = Common.Helpers.MIMEAssistant.MIMETypesDictionary.FirstOrDefault(x => x.Value == httpPostedFile.ContentType);
                            // Get the complete file path
                            var LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/RetailerOffers"), Filename);
                            httpPostedFile.SaveAs(LogoUrl);

                            AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(Filename, "~/images/RetailerOffers", LogoUrl);
                            Logourl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                               , HttpContext.Current.Request.Url.DnsSafeHost
                                                               , HttpContext.Current.Request.Url.Port
                                                               , string.Format("/images/RetailerOffers/", Filename));
                            result = string.Format("/images/RetailerOffers/" + Filename);

                        }

                    }
                    return Created(result, result);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }
            return Created(result, result);

        }

        [Route("UploadWhatsAppTemplateImage")]
        [HttpPost]
        public IHttpActionResult UploadWhatsAppTemplateImage()
        {
            string Logourl = "";
            string resutl = string.Empty;
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];

                    if (httpPostedFile != null)
                    {
                        if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath("~/UploadedWhatsAppTemplateImages")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/UploadedWhatsAppTemplateImages"));

                        string extension = Path.GetExtension(httpPostedFile.FileName);
                        string Filename = DateTime.Now.ToString("ddMMyyyyhhmmss") + extension;

                        var contenttype = Common.Helpers.MIMEAssistant.MIMETypesDictionary.FirstOrDefault(x => x.Value == httpPostedFile.ContentType);
                        // Get the complete file path
                        var LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedWhatsAppTemplateImages"), Filename);
                        httpPostedFile.SaveAs(LogoUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(Filename, "~/UploadedWhatsAppTemplateImages", LogoUrl);
                        Logourl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                           , HttpContext.Current.Request.Url.DnsSafeHost
                                                           , HttpContext.Current.Request.Url.Port
                                                           , string.Format("UploadedWhatsAppTemplateImages/", Filename));
                        resutl = string.Format("UploadedWhatsAppTemplateImages/" + Filename);

                    }
                }
                return Created(resutl, resutl);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }





        [Route("UploadWhatsAppTemplateImage2")]
        [HttpPost]
        public string UploadWhatsAppTemplateImage2()
        {
            string imagepath = "";
            logger.Info("start Template Image upload");
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
                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string filename = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                        // Get the complete file path
                        var templateImage = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedWhatsAppTemplateImages"), filename);

                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                        httpPostedFile.SaveAs(templateImage);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadedWhatsAppTemplateImages", templateImage);
                        if (ConfigurationManager.AppSettings["Environment"] == "Production")
                        {
                            Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                            Cloudinary cloudinary = new Cloudinary(account);

                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(templateImage),
                                PublicId = "category/" + httpPostedFile.FileName,
                                Overwrite = true,
                                Invalidate = true,
                                Backup = false
                            };

                            var uploadResult = cloudinary.Upload(uploadParams);

                            if (System.IO.File.Exists(templateImage))
                            {
                                System.IO.File.Delete(templateImage);
                            }

                            imagepath = Convert.ToString(uploadResult.SecureUri);
                        }
                        else
                        {
                            imagepath = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                    , HttpContext.Current.Request.Url.DnsSafeHost
                                                                    , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                    , "/UploadedWhatsAppTemplateImages/" + filename);

                        }
                        return imagepath;
                    }
                    return imagepath;
                }
                return imagepath;
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }



            return imagepath;
        }

    }
}
