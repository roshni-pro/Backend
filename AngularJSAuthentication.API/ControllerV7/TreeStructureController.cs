using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Data;
using System.IO;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.Transaction;

using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/TreeStructure")]

    public class TreeStructureController : BaseAuthController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [AllowAnonymous]
        [Route("getData")]
        [HttpGet]
        public List<TreeStructureDc> Getlist()

        {
            MongoDbHelper<TreeStructure> mongoDbHelper = new MongoDbHelper<TreeStructure>();
            var whdata = mongoDbHelper.Select(x => x.IsDeleted == false && x.IsActive == true).ToList();
            var result = whdata.Select(x => new TreeStructureDc
            {
                id = x.guid,
                img = x.img,
                name = x.name,
                pid = x.pguid,
                title = x.title,
                Designation=x.Designation,
                Department=x.Department,
                EmployeeLevel=x.EmployeeLevel
            }).ToList();
            return result;
        }

        [AllowAnonymous]
        [Route("savedata")]
        [HttpPost]

        public List<TreeStructureDc> insert(TreeStructureDc treeStructure)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            MongoDbHelper<TreeStructure> mongoDbHelper = new MongoDbHelper<TreeStructure>();


            if (!string.IsNullOrEmpty(treeStructure.id))
            {
                var data = mongoDbHelper.Select(x => x.guid == treeStructure.id).FirstOrDefault();
                if (data != null)
                {
                    data.name = treeStructure.name;
                    data.title = treeStructure.title;
                    data.img = treeStructure.img;
                    data.Department = treeStructure.Department;
                    data.Designation = treeStructure.Designation;
                    data.EmployeeLevel = treeStructure.EmployeeLevel;
                    data.IsActive = true;
                    data.IsDeleted = false;
                    mongoDbHelper.ReplaceWithoutFind(data.id, data);
                }
                else
                {
                    TreeStructure dbtreeStructure = new TreeStructure
                    {
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        name = treeStructure.name,
                        title = treeStructure.title,
                        guid = treeStructure.id,
                        pguid = treeStructure.pid,
                        img = treeStructure.img,
                        Department = treeStructure.Department,
                      Designation = treeStructure.Designation,
                    EmployeeLevel = treeStructure.EmployeeLevel
                };

                    mongoDbHelper.Insert(dbtreeStructure);
                }

            }
           

            //var objid = new ObjectId();


            //if (data != null)
            //{
                
            //}
            //else
            //{

            //}

            var whdata = mongoDbHelper.Select(x => x.IsDeleted == false && x.IsActive == true).ToList();
            var result = whdata.Select(x => new TreeStructureDc
            {
                id = x.guid,
                img = x.img,
                name = x.name,
                pid = x.pguid,
                title = x.title,
                Designation=x.Designation,
                Department=x.Department,
                EmployeeLevel=x.EmployeeLevel
            }).ToList();
            return result;
        }

        //[AllowAnonymous]
        //[Route("savenewdata")]
        //[HttpPost]

        //public TreeStructure insert(TreeStructure treeStructure)
        //{
        //    var identity = User.Identity as ClaimsIdentity;
        //    int compid = 0, userid = 0;

        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //    using (var db = new AuthContext())
        //    {
        //        MongoDbHelper<TreeStructure> mongoDbHelper = new MongoDbHelper<TreeStructure>();
        //        MongoDbHelper<TreeStructure> mongoDbHelperitem = new MongoDbHelper<TreeStructure>();
        //        var objid = new ObjectId(treeStructure.id.ToString());
        //        var whdata = mongoDbHelper.Select(x => x.IsActive == true).ToList();


        //        // ComboItemAB item = new ComboItemAB();
        //        TreeStructure tree = new TreeStructure();
        //        tree.id = treeStructure.id;
        //        tree.IsActive = true;
        //        tree.IsDeleted = false;
        //        tree.CreatedDate = DateTime.Now;
        //        tree.title = treeStructure.title;
        //        tree.name = treeStructure.name;

        //        mongoDbHelper.Insert(treeStructure);

        //        return treeStructure;
        //    }

        //}
        [AllowAnonymous]
        [Route("Delete")]
        [HttpGet]
        public async Task<List<TreeStructure>> TreeStructure(string id)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


            MongoDbHelper<TreeStructure> mongoDbHelper = new MongoDbHelper<TreeStructure>();
            var com = new MongoDbHelper<TreeStructure>();
            var whdata = mongoDbHelper.Select(x => x.IsDeleted == false && x.IsActive == true).ToList();
            var data = whdata.Where(x => x.guid == id).FirstOrDefault();
            if (data != null)
            {
                data.IsDeleted = true;
                data.IsActive = false;
                var res = await mongoDbHelper.ReplaceAsync(data.id, data);
            }
            return whdata.Where(x => x.IsDeleted == false && x.IsActive == true).ToList();


        }

        [AllowAnonymous]
        [Route("UploadOrganizationImage")]
        [HttpPost]
        public IHttpActionResult UploadcomboImage()
        {
            string LogoUrl = "";
              string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {


                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/images/Organization")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/images/Organization"));

                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/Organization"), fileName);

                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/images/Organization", LogoUrl);

                        LogoUrl = "/images/Organization/" + fileName;


                    }

                }
                return Created<string>(LogoUrl, LogoUrl);
            }
            catch (Exception ex)
            {
                logger.Error("Error in TreeStructure Method: " + ex.Message);
                return null;
            }
        }
    }

    //[AllowAnonymous]
    //[Route("GetList")]
    //[HttpGet]
    //public HttpResponseMessage GetList()

    //{
    //    try
    //    {
    //        using (var con = new AuthContext())
    //        {
    //            var combolist = new MongoDbHelper<Combo>();

    //            var combo = combolist.Select(x => x.IsDeleted == false).OrderByDescending(x => x.CreatedDate).ToList();
    //            string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

    //            foreach (var item in combo)
    //            {
    //                item.ComboImage = baseUrl + "" + item.ComboImage;
    //            }



    //            return Request.CreateResponse(HttpStatusCode.OK, combo);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        return Request.CreateResponse(HttpStatusCode.BadRequest, false);
    //    }


    //}

    //[Route("UploadTicketImage")]
    //[HttpPost]
    //public void UploadTicketImage()
    //{
    //    logger.Info("start image upload");

    //    var identity = User.Identity as ClaimsIdentity;
    //    int compid = 0, userid = 0;
    //    string returnfilepath = string.Empty;

    //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
    //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

    //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
    //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

    //    using (AuthContext context = new AuthContext())
    //    {
    //        if (HttpContext.Current.Request.Files.AllKeys.Any())
    //        {
    //            var httpPostedFile = HttpContext.Current.Request.Files["file"];
    //            if (httpPostedFile != null)
    //            {
    //                // Validate the uploaded image(optional)
    //                // Get the complete file path
    //                var exte = httpPostedFile.ContentType;
    //                var ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedImages"), httpPostedFile.FileName);

    //                if (!Directory.Exists(ImageUrl))
    //                    Directory.CreateDirectory(ImageUrl);


    //                var fullPhysicalPath = ImageUrl + "\\" + exte;
    //                var fileUrl = "/UploadedImages" + "/" + exte;

    //                FileStream file = File.Create(fullPhysicalPath);
    //                file.Close();

    //                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
    //                httpPostedFile.SaveAs(ImageUrl);

    //            }


    //        }


    //    }

    //}



    public class TreeStructureDc
    {
        public string id { get; set; }
        public string pid { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public string img { get; set; }
        public string Designation { get; set; }
        public string EmployeeLevel { get; set; }
        public string Department { get; set; }

    }
}

