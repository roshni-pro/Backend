using AngularJSAuthentication.API.ControllerV7;
using AngularJSAuthentication.Model.Item;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/itemimageupload")]
    public class ItemImageUploadController : ApiController
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
                    var FileUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/itemimages"), httpPostedFile.FileName);
                    httpPostedFile.SaveAs(FileUrl);
                    AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/images/itemimages", FileUrl);
                    //CloudinaryDotNet.Account account = new CloudinaryDotNet.Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

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

        [Route("itemimageuploadV7")]
        [HttpPost]
        public IHttpActionResult UploadFileV7()
        {
            string Logourl = ""; string returnPath = "";
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

                    Logourl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/itemimages/"), filename);
                    returnPath = "images/itemimages/" + filename;
                    // var FileUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/itemimages"), httpPostedFile.FileName);
                    //returnPath = "/images/itemimages/" + FileUrl;
                    //LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedLogos"), filename);
                    //returnPath = "/UploadedLogos/" + filename;
                    httpPostedFile.SaveAs(Logourl);
                    AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~images/itemimages/", Logourl);
                    if (ConfigurationManager.AppSettings["Environment"] == "Production")
                    {
                        Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                        Cloudinary cloudinary = new Cloudinary(account);

                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(Logourl),
                            PublicId = "items_images/item_1/" + httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension,
                            Overwrite = true,
                            Invalidate = true,
                            Backup = false
                        };

                        var uploadResult = cloudinary.Upload(uploadParams);

                        if (uploadResult.SecureUri != null && !string.IsNullOrEmpty(uploadResult.SecureUri.ToString()))
                        {
                            System.IO.File.Delete(Logourl);
                            Logourl = uploadResult.SecureUri.ToString();
                        }

                        return Created<string>(Logourl, Logourl);
                    }
                    else
                    {
                        Logourl = returnPath;
                        //Logourl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                        //                                              , HttpContext.Current.Request.Url.DnsSafeHost
                        //                                              , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                        //                                              , "images/itemimages/" + filename);
                        return Created<string>(Logourl, Logourl);
                    }
                }

                return BadRequest();

            }
            return BadRequest();
        }

        [HttpPost]
        [Route("UploadSalesReturnImage")]
        public async Task<HttpResponseMessage> UploadSalesReturnImage()
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
                        // Validate the uploaded image(optional)
                        // Get the complete file path
                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string filename = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;
                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/UploadSalesReturnImage")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/UploadSalesReturnImage"));
                        var ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadSalesReturnImage"), filename);

                        httpPostedFile.SaveAs(ImageUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadSalesReturnImage", ImageUrl);
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

        [Route("UploadKKReturnReplaceImages")]
        [HttpPost]
        public string UploadPeopleDocument()
        {
            string LogoUrl = "";
            string ImageUrl = "";


            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    // Get the uploaded image from the Files collection
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];

                    if (httpPostedFile != null)
                    {
                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/UploadKKReturnReplaceImages")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/UploadKKReturnReplaceImages"));
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadKKReturnReplaceImages"), httpPostedFile.FileName);


                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/UploadKKReturnReplaceImages", LogoUrl);
                        ImageUrl = LogoUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                ImageUrl = null;
            }
            return ImageUrl;
        }

        [Route("UploadExcel")]
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult UploadExcel(int WarehouseId)
        {
            bool flag = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (HttpContext.Current.Request.Files.Count > 0)
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];

                if (httpPostedFile != null)
                {
                    string path = HttpContext.Current.Server.MapPath("~/UploadedFiles/WarehouseBasedMRPSensitive/");
                    string a1, b;
                    
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    a1 = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + httpPostedFile.FileName;
                    b = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles/WarehouseBasedMRPSensitive/"), a1);
                    httpPostedFile.SaveAs(b);

                    AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/UploadedFiles/WarehouseBasedMRPSensitive/", b);

                    byte[] buffer = new byte[httpPostedFile.ContentLength];
                    using (BinaryReader br = new BinaryReader(File.OpenRead(b)))
                    {
                        br.Read(buffer, 0, buffer.Length);
                    }
                    XSSFWorkbook hssfwb;
                    List<uploadIsPriceSensitiveExcel> uploaditemlist = new List<uploadIsPriceSensitiveExcel>();

                    using (MemoryStream memStream = new MemoryStream())
                    {
                        BinaryFormatter binForm = new BinaryFormatter();
                        memStream.Write(buffer, 0, buffer.Length);
                        memStream.Seek(0, SeekOrigin.Begin);
                        hssfwb = new XSSFWorkbook(memStream);
                        string sSheetName = hssfwb.GetSheetName(0);
                        ISheet sheet = hssfwb.GetSheet(sSheetName);
                        IRow rowData;
                        ICell cellData = null;
                        for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                        {
                            if (iRowIdx == 0)
                            {
                                rowData = sheet.GetRow(iRowIdx);
                                if (rowData != null)
                                {
                                    string strJSON = null;
                                    string field = string.Empty;
                                    field = rowData.GetCell(0).ToString();
                                    if (field != "ItemMultiMRPId")
                                    {
                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                                        strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                        return Created("",strJSON);
                                    }
                                    field = rowData.GetCell(1).ToString();

                                    if (field != "ISSensitive")
                                    {
                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                                        strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                        return Created("", strJSON);
                                    }
                                }
                            }
                            else
                            {
                                uploadIsPriceSensitiveExcel additem = new uploadIsPriceSensitiveExcel();
                                rowData = sheet.GetRow(iRowIdx);

                                if (rowData != null && rowData.GetCell(0) != null)
                                {
                                    cellData = rowData.GetCell(0);
                                    additem.WarehouseId = WarehouseId;

                                    string col = null;
                                    cellData = rowData.GetCell(0);
                                    col = cellData == null ? "" : cellData.ToString().Trim();
                                    if (string.IsNullOrEmpty(col)) continue;
                                    else
                                        additem.ItemMultiMRPId = Convert.ToInt32(col); 

                                   
                                    string col2 = null;
                                    cellData = rowData.GetCell(1);
                                    col2 = cellData == null ? "" : cellData.ToString().Trim();

                                    if (string.IsNullOrEmpty(col2)) continue;
                                    else
                                    {
                                        if (col2.ToLower() == "yes" || col2.ToLower() == "y" || col2.ToLower() == "true" || col2 == 1.ToString())
                                            additem.ISSensitive = true;
                                        else if (col2.ToLower() == "no" || col2.ToLower() == "n" || col2.ToLower() == "false" || col2 == 0.ToString())
                                            additem.ISSensitive = false;
                                        else continue;
                                    }

                                    
                                    uploaditemlist.Add(additem);    
                                }
                               
                            }
                        }
                    }
                    if (uploaditemlist != null && uploaditemlist.Any())
                    {
                        using (AuthContext context = new AuthContext())
                        {
                            
                            if (  uploaditemlist != null && uploaditemlist.Any())
                            {
                                List<WarehouseBasedMRPSensitive> warehouseBasedMRPSensitivesList = new List<WarehouseBasedMRPSensitive>();

                                foreach (var item in uploaditemlist)
                                {
                                    var isexist = context.WarehouseBasedMRPSensitives.Where(x => x.WarehouseId == WarehouseId && x.ItemMultiMRPId == item.ItemMultiMRPId && x.IsDeleted == false).FirstOrDefault();
                                    if (isexist == null)
                                    {
                                        WarehouseBasedMRPSensitive warehouseBasedMRPSensitives = new WarehouseBasedMRPSensitive();
                                        warehouseBasedMRPSensitives.ItemMultiMRPId = item.ItemMultiMRPId;
                                        warehouseBasedMRPSensitives.WarehouseId = WarehouseId;
                                        warehouseBasedMRPSensitives.IsSensitive = item.ISSensitive;
                                        warehouseBasedMRPSensitives.CreatedBy = userid ;
                                        warehouseBasedMRPSensitives.CreatedDate = DateTime.Now;
                                        warehouseBasedMRPSensitives.IsActive = true;
                                        warehouseBasedMRPSensitives.IsDeleted = false;
                                        warehouseBasedMRPSensitivesList.Add(warehouseBasedMRPSensitives);

                                    }
                                    else
                                    {
                                        isexist.IsSensitive = item.ISSensitive;
                                        isexist.ModifiedBy = userid;
                                        isexist.IsActive = true;
                                        isexist.ModifiedDate = DateTime.Now;
                                        context.Entry(isexist).State = System.Data.Entity.EntityState.Modified;
                                    }
                                }
                                context.WarehouseBasedMRPSensitives.AddRange(warehouseBasedMRPSensitivesList);
                            }
                            string MSG="";
                            if (context.Commit() > 0) { MSG = "File Saved Sucessfully"; return Created(MSG, MSG);
                                ;
                            } else { MSG="Failed To Save"; return  Created(MSG, MSG);}
                        }
                    }
                }
                else
                {
                    return Created("", "file not uploaded");
                }


            }
            return Created("", "file not uploaded");
        }

        [Route("ExportExcel")]
        [HttpGet]
        public async Task<List<uploadIsPriceSensitiveExcel>> ExportIsPriceSensitiveExcel(int WarehouseId)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            {
                uploadIsPriceSensitiveExcel uploadIspriceSensitiveExcel = new uploadIsPriceSensitiveExcel();
                using (AuthContext context = new AuthContext())
                {

                    var list = context.WarehouseBasedMRPSensitives.Where(x => x.WarehouseId == WarehouseId && x.IsActive == true && x.IsDeleted == false)
                            .Select(y=>new uploadIsPriceSensitiveExcel {ItemMultiMRPId=y.ItemMultiMRPId,WarehouseId=y.WarehouseId,ISSensitive=y.IsSensitive }).ToList();

                    return list;
                }
            }
            
        }



    }
    public class uploadIsPriceSensitiveExcel
    {
        public int ItemMultiMRPId  { get; set; }
        public int WarehouseId { get; set; }
        public bool ISSensitive { get; set; }

    }
}
