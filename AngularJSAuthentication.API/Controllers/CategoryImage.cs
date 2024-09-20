using AngularJSAuthentication.Model;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/CategoryImage")]
    public class CategoryImageController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Authorize]
        [Route("")]
        public IEnumerable<Category> Get()
        {
            using (var context = new AuthContext())
            {
                logger.Info("start Category: ");
                List<Category> ass = new List<Category>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    // Access claims
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = context.AllCategory(compid).ToList();
                    logger.Info("End  Category: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
        }


        //[Authorize]
        [Route("GetCategoryImage")]
        public IEnumerable<CategoryImageData> GetCategoryImage()
        {
            using (var context = new AuthContext())
            {
                logger.Info("start Category: ");
                List<CategoryImageData> ass = new List<CategoryImageData>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    // Access claims
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = context.AllCategoryImages().ToList();
                    logger.Info("End  Category: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(CategoryImage))]
        [Route("")]
        [AcceptVerbs("POST")]
        public int add(CategoryImage item)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start addCategory: ");
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    // Access claims
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
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    int result = context.AddCategoryImage(item);
                    UploadFile();
                    logger.Info("End  AddCategoryImage: ");
                    if (result > 0)
                    {
                        return result;
                    }
                    else
                    {
                        return 0;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCategory " + ex.Message);
                    logger.Info("End  addCategory: ");
                    return 0;
                }
            }
        }

        [ResponseType(typeof(CategoryImage))]
        [Route("AddCategoryImage")]
        [AcceptVerbs("POST")]
        public CategoryImage AddCategoryImage(CategoryImage item)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start addCategory: ");
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
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.AddCategoryImages(item);
                    UploadFile();
                    logger.Info("End  AddCategoryImage: ");
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCategory " + ex.Message);
                    logger.Info("End  addCategory: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(CategoryImage))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public int Put(CategoryImage item)
        {
            using (var context = new AuthContext())
            {
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    int result = context.PutCategoryImage(item);
                    return result;
                }
                catch
                {
                    return 0;
                }
            }
        }

        [ResponseType(typeof(CategoryImage))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            using (var db = new AuthContext())
            {
                logger.Info("start del Category: ");
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    CategoryImage categoryimage = db.CategoryImageDB.Where(x => x.CategoryImageId == id && x.Deleted == false).FirstOrDefault();
                    categoryimage.Deleted = true;
                    categoryimage.IsActive = false;
                    db.CategoryImageDB.Attach(categoryimage);
                    db.Entry(categoryimage).State = EntityState.Modified;
                    db.Commit();

                    CommonHelper.refreshCategory();


                    logger.Info("End  delete Category: ");
                }
                catch (Exception ex)
                {
                    logger.Error("Error in del Category " + ex.Message);
                }
            }
        }

        [Route("GetCategoryImageByCId")]
        public dynamic GetCategoryImageByCId(int CategoryId)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    List<CategoryImage> ass = new List<CategoryImage>();
                    ass = db.CategoryImageDB.Where(c => c.CategoryId == CategoryId && c.Deleted == false && c.IsActive == true).ToList();
                    if (ass != null)
                    {
                        return ass;
                    }
                    else
                    {
                        var obj = new
                        {
                        };
                        return obj;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }


        [Route("GetCatImageByCIdSCId")]
        public dynamic GetCategoryImageByCId(int CategoryId,int SubCategoryId,int WarehouseId)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    List<CategoryImage> ass = new List<CategoryImage>();
                    List<int> catIds = new List<int>();
                    if (CategoryId == 0)
                    {
                       var categoryids= db.itemMasters.Where(x => x.SubCategoryId == SubCategoryId && x.WarehouseId== WarehouseId && (x.ItemAppType==0 || x.ItemAppType==2) && x.active && !x.IsDisContinued && x.DistributorShow).Select(x => x.Categoryid).ToList();
                        catIds.AddRange(categoryids);
                    }
                    else
                    {
                        catIds.Add(CategoryId);
                    }
                    ass = db.CategoryImageDB.Where(c => catIds.Contains(c.CategoryId) && c.Deleted == false && c.IsActive == true).ToList();
                    if (ass != null)
                    {
                        return ass;
                    }
                    else
                    {
                        var obj = new
                        {
                        };
                        return obj;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        [HttpPost]
        public string UploadFile()
        {

            string Logourl = "";
            logger.Info("start image upload");
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
                        var ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedImages"), httpPostedFile.FileName);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        httpPostedFile.SaveAs(ImageUrl);

                        var uploader = new System.Collections.Generic.List<AngularJSAuthentication.DataContracts.FileUpload.Uploader> { new AngularJSAuthentication.DataContracts.FileUpload.Uploader() };
                        uploader.FirstOrDefault().FileName = httpPostedFile.FileName;
                        uploader.FirstOrDefault().RelativePath = "~/UploadedImages";


                        uploader.FirstOrDefault().SaveFileURL = ImageUrl;

                        uploader = Nito.AsyncEx.AsyncContext.Run(() => AngularJSAuthentication.Common.Helpers.FileUploadHelper.UploadFileToOtherApi(uploader));



                        Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                        Cloudinary cloudinary = new Cloudinary(account);

                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(ImageUrl),
                            PublicId = "items_images/item_1/" + httpPostedFile.FileName,
                            Overwrite = true,
                            Invalidate = true,
                            Backup = false
                        };

                        var uploadResult = cloudinary.Upload(uploadParams);

                        if (System.IO.File.Exists(ImageUrl))
                        {
                            System.IO.File.Delete(ImageUrl);
                        }

                        Logourl = uploadResult.SecureUri.ToString();
                        return Logourl;

                    }
                    return Logourl;
                }
                return Logourl;
            }
            catch (Exception ex)
            {
                logger.Error("Error in  image upload " + ex.Message);
                logger.Info("End  image upload: ");

            }
            return Logourl;
        }
    }

    public class CategoryImageData
    {
        public int CategoryId { get; set; }
        public int CategoryImageId { get; set; }
        public string CategoryName { get; set; }

        public string CategoryImg { get; set; }
        public string AppType { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

    }
}



