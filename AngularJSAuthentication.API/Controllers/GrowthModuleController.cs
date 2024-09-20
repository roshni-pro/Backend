using AngularJSAuthentication.Model;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/GrowthModule")]
    public class GrowthModuleController : ApiController
    {
        #region Global
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        #endregion

        #region GrowthModule

        /// <summary>
        /// Warehouse Creation
        /// </summary>
        /// <param name="warehouseid"></param>
        /// <param name="createcony"></param>
        /// <returns>Status</returns>
        [Route("WarehouseCreation")]
        [HttpGet]
        public bool WarehouseCreation(int warehouseid, int createdby)
        {
            try
            {
                using (var connection = new AuthContext())
                {
                    //var warehouseprocess = new GMWarehouseProgress();
                    //warehouseprocess.WarehouseID = warehouseid;
                    //warehouseprocess.Progress = 0;
                    //connection.GMWarehouseProgressDB.Add(warehouseprocess);
                    //connection.SaveChanges();

                    var gminfra = new GMInfrastructure();
                    gminfra.WarehouseId = warehouseid;
                    gminfra.LegalPercent = 0;
                    gminfra.TaskPercent = 0;
                    gminfra.CreatedDate = DateTime.Now;
                    gminfra.CreatedBy = createdby;
                    gminfra.IsActive = true;
                    gminfra.IsDeleted = false;
                    gminfra.WarehouseLaunchDays = 30;
                    connection.GMInfrastructureDB.Add(gminfra);
                    connection.Commit();
                    int id = gminfra.Id;

                    var legaldoc = connection.GMLegalDocMasterDB.ToList();
                    foreach (var item in legaldoc)
                    {
                        var infralegal = new GMInfrastructureLegal();
                        infralegal.GMInfrastructureId = id;
                        infralegal.LegalDocId = item.Id;
                        infralegal.IsActive = true;
                        infralegal.IsDeleted = false;
                        //infralegal.ImagePath = "";
                        infralegal.CreatedBy = createdby;
                        infralegal.CreatedDate = DateTime.Now;
                        infralegal.IsUploaded = false;
                        connection.GMInfrastructureLegalDB.Add(infralegal);
                        connection.Commit();
                    }

                    var Tasklist = connection.GMTaskListMasterDB.ToList();
                    foreach (var item in Tasklist)
                    {
                        var infraTask = new GMInfrastructureTask();
                        infraTask.GMInfrastructureId = id;
                        infraTask.TaskListId = item.Id;
                        infraTask.IsActive = true;
                        infraTask.IsDeleted = false;
                        //infraTask.ImagePath = "";
                        infraTask.CreatedBy = createdby;
                        infraTask.CreatedDate = DateTime.Now;
                        infraTask.IsUploaded = false;
                        connection.GMInfrastructureTaskDB.Add(infraTask);
                        connection.Commit();
                    }

                    var citymapping = connection.GMCityMappingMasterDB.ToList();
                    foreach (var item in citymapping)
                    {
                        var cm = new GMCityMapping();
                        cm.CityMappingMasterId = item.Id;
                        cm.RequiredQuantity = item.RequiredQuantity;
                        cm.FilledQuantity = 0;
                        cm.IsActive = true;
                        cm.IsDeleted = false;
                        cm.CreatedBy = createdby;
                        cm.CreatedDate = DateTime.Now;
                        cm.WarehouseId = warehouseid;
                        connection.GMCityMappingDB.Add(cm);
                        connection.Commit();
                    }

                    var people = connection.GMDivisionMasterDB.ToList();
                    foreach (var item in people)
                    {
                        var pp = new GMPeople();
                        pp.DivisionId = item.Id;
                        pp.RequiredQuantity = item.RequiredQty;
                        pp.FilledQuantity = 0;
                        pp.WarehouseId = warehouseid;
                        pp.IsActive = true;
                        pp.IsDeleted = false;
                        pp.CreatedBy = createdby;
                        pp.CreatedDate = DateTime.Now;
                        connection.GMPeopleDB.Add(pp);
                        connection.Commit();

                        var td = new GMTrainingDevlopment();
                        td.DivisionId = item.Id;
                        //td.UploadOnboardSheetPath = "";
                        td.WarehouseId = warehouseid;
                        td.IsActive = true;
                        td.IsDeleted = false;
                        td.CreatedBy = createdby;
                        td.CreatedDate = DateTime.Now;
                        connection.GMTrainingDevlopmentDB.Add(td);
                        connection.Commit();
                    }

                    var propart = connection.GMProductPartnerMasterDB.ToList();
                    foreach (var item in propart)
                    {
                        var pp = new GMProductPartners();
                        pp.ProductPartnersId = item.Id;
                        pp.RequiredQuantity = item.Quantity;
                        pp.FilledQuantity = 0;
                        pp.WarehouseId = warehouseid;
                        pp.IsActive = true;
                        pp.IsDeleted = false;
                        pp.CreatedBy = createdby;
                        pp.CreatedDate = DateTime.Now;
                        connection.GMProductPartnersDB.Add(pp);
                        connection.Commit();
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Get Infrastructure list
        /// </summary>
        /// <returns></returns>
        [Route("GetInfrastructure")]
        [HttpGet]
        public HttpResponseMessage GetInfrastructure(int warehouseid)
        {
            try
            {
                using (var con = new AuthContext())
                {
                    var infraid = con.GMInfrastructureDB.Where(x => x.WarehouseId == warehouseid).Select(x => x.Id).SingleOrDefault();

                    var Legal = (from a in con.GMInfrastructureLegalDB
                                 join b in con.GMLegalDocMasterDB on a.LegalDocId equals b.Id
                                 where a.GMInfrastructureId == infraid
                                 select new
                                 {
                                     Id = a.Id,
                                     GMInfrastructureId = a.GMInfrastructureId,
                                     LegalDocName = b.Name,
                                     IsUploaded = a.IsUploaded
                                     //ImagePath = a.ImagePath
                                 }).ToList();

                    var Tasklist = (from a in con.GMInfrastructureTaskDB
                                    join b in con.GMTaskListMasterDB on a.TaskListId equals b.Id
                                    where a.GMInfrastructureId == infraid
                                    select new
                                    {
                                        Id = a.Id,
                                        GMInfrastructureId = a.GMInfrastructureId,
                                        TaskName = b.Name,
                                        IsRequiredDoc = b.IsRequireddoc,
                                        IsInstalled = a.IsInstalled,
                                        IsUploaded = a.IsUploaded
                                        //ImagePath = a.ImagePath
                                    }).ToList();

                    var response = new
                    {
                        infra = new
                        {
                            Legal = Legal,
                            TaskList = Tasklist
                        },
                        Status = true,
                        Message = "Infrastructure List"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        /// <summary>
        /// Get Infrastructure Legal Images
        /// </summary>
        /// <param name="InfraId"></param>
        /// <param name="InfraLegalId"></param>
        /// <returns>InfraLegalImages List</returns>
        [Route("GetInfrastructureLegalImages")]
        [HttpGet]
        public HttpResponseMessage GetInfrastructureLegalImages(int InfraLegalId)
        {
            try
            {
                using (var con = new AuthContext())
                {
                    var InfraLegalImages = con.GMInfraLegalImagesDB.Where(x => x.GMInfrastructureLegalID == InfraLegalId).ToList();

                    var response = new
                    {
                        InfraLegalImages = InfraLegalImages,
                        Status = true,
                        Message = "InfraLegalImages List"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        /// <summary>
        /// Add Infrastructure Legal Images
        /// </summary>
        /// <param name="imagedata"></param>
        /// <returns>Status</returns>
        [Route("AddInfrastructureLegalImages")]
        [HttpPost]
        public HttpResponseMessage AddInfrastructureLegalImages(List<InfraLegalImage> imagedata)
        {
            try
            {
                using (var con = new AuthContext())
                {
                    foreach (var item in imagedata)
                    {
                        var infralegalimage = new GMInfraLegalImages();
                        infralegalimage.GMInfrastructureLegalID = item.InfraLegalId;
                        infralegalimage.ImagePath = item.ImagePath;
                        con.GMInfraLegalImagesDB.Add(infralegalimage);
                        con.Commit();

                        var infraimage = con.GMInfrastructureLegalDB.Where(x => x.Id == item.InfraLegalId).SingleOrDefault();
                        if (infraimage != null)
                        {
                            infraimage.IsUploaded = true;
                            con.Commit();
                        }
                    }
                    var response = new
                    {
                        Status = true,
                        Message = "Record Inserted"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        /// <summary>
        /// Add Infrastructure Task Images
        /// </summary>
        /// <param name="imagedata"></param>
        /// <returns>Status</returns>
        [Route("AddInfrastructureTaskImages")]
        [HttpPost]
        public HttpResponseMessage AddInfrastructureTaskImages(List<InfraTaskImage> imagedata)
        {
            try
            {
                using (var con = new AuthContext())
                {
                    foreach (var item in imagedata)
                    {
                        var infrataskimage = new GMInfraTaskImages();
                        infrataskimage.GMInfrastructureTaskID = item.InfraTaskId;
                        infrataskimage.ImagePath = item.ImagePath;
                        con.GMInfraTaskImagesDB.Add(infrataskimage);
                        con.Commit();

                        var infraimage = con.GMInfrastructureTaskDB.Where(x => x.Id == item.InfraTaskId).SingleOrDefault();
                        if (infraimage != null)
                        {
                            infraimage.IsUploaded = true;
                            con.Commit();
                        }
                    }
                    var response = new
                    {
                        Status = true,
                        Message = "Record Inserted"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        /// <summary>
        /// Add Trainging Developer Images
        /// </summary>
        /// <param name="imagedata"></param>
        /// <returns>Status</returns>
        [Route("AddTraingingDeveloperImages")]
        [HttpPost]
        public HttpResponseMessage AddTraingingDeveloperImages(List<TrainingDevelopmentImage> imagedata)
        {
            try
            {
                using (var con = new AuthContext())
                {
                    foreach (var item in imagedata)
                    {
                        var traindevelimage = new GMTrainingDevlopmentImages();
                        traindevelimage.GMTrainingDevlopmentId = item.GMTrainingDevlopmentId;
                        traindevelimage.ImagePath = item.ImagePath;
                        con.GMTrainingDevlopmentImagesDB.Add(traindevelimage);
                        con.Commit();

                        var trainimage = con.GMTrainingDevlopmentDB.Where(x => x.Id == item.GMTrainingDevlopmentId).SingleOrDefault();
                        if (trainimage != null)
                        {
                            trainimage.IsUploaded = true;
                            con.Commit();
                        }
                    }
                    var response = new
                    {
                        Status = true,
                        Message = "Record Inserted"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }


        /// <summary>
        /// Get Infrastructure Task Images
        /// </summary>
        /// <param name="InfraId"></param>
        /// <param name="InfraTaskId"></param>
        /// <returns>InfrastructureTaskImages List</returns>
        [Route("GetInfrastructureTaskImages")]
        [HttpGet]
        public HttpResponseMessage GetInfrastructureTaskImages(int InfraTaskId)
        {
            try
            {
                using (var con = new AuthContext())
                {
                    var InfraTaskImages = con.GMInfraTaskImagesDB.Where(x => x.GMInfrastructureTaskID == InfraTaskId).ToList();

                    var response = new
                    {
                        InfraTaskImages = InfraTaskImages,
                        Status = true,
                        Message = "InfraTaskImages List"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        /// <summary>
        /// Get People
        /// </summary>
        /// <returns></returns>
        [Route("GetPeople")]
        [HttpGet]
        public HttpResponseMessage GetPeople(int warehouseid)
        {
            try
            {
                using (var con = new AuthContext())
                {
                    var People = (from a in con.GMPeopleDB
                                  join b in con.GMDivisionMasterDB on a.DivisionId equals b.Id
                                  where a.WarehouseId == warehouseid
                                  select new
                                  {
                                      Id = a.Id,
                                      DivisionId = a.DivisionId,
                                      Name = b.Name,
                                      RequiredQuantity = a.RequiredQuantity,
                                      FilledQuantity = a.FilledQuantity
                                  }).ToList();



                    var response = new
                    {
                        People = People,
                        Status = true,
                        Message = "People List"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        /// <summary>
        /// Get Product Partners List
        /// </summary>
        /// <returns></returns>
        [Route("GetProductPartners")]
        [HttpGet]
        public HttpResponseMessage GetProductPartners(int warehouseid)
        {
            try
            {
                using (var con = new AuthContext())
                {
                    var ProductPartners = (from a in con.GMProductPartnersDB
                                           join b in con.GMProductPartnerMasterDB on a.ProductPartnersId equals b.Id
                                           where a.WarehouseId == warehouseid
                                           select new
                                           {
                                               Id = a.Id,
                                               ProductPartnersId = a.ProductPartnersId,
                                               Type = b.Type,
                                               Name = b.Name,
                                               RequiredQuantity = a.RequiredQuantity,
                                               FilledQuantity = a.FilledQuantity
                                           }).ToList();

                    var response = new
                    {
                        ProductPartners = ProductPartners,
                        Status = true,
                        Message = "Product and Partners List"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        [Route("GetOrganizationData")]
        [HttpGet]
        public IHttpActionResult GetOrganizationData(int warehouseid)
        {
            GMPeople people = null;
            try
            {
                using (var con = new AuthContext())
                {
                    people = con.GMPeopleDB.Where(x => x.WarehouseId == warehouseid).First();
                }
            }
            catch
            {

            }
            return Ok(people);
        }

        [Route("GetBrandImages")]
        [HttpGet]
        public HttpResponseMessage GetBrandImages(int warehouseid)
        {
            try
            {
                var BrandImages = new List<BrandImages>();
                using (var con = new AuthContext())
                {
                    var GetBrandcategory = con.itemMasters.Where(x => x.WarehouseId == warehouseid && x.active == true && x.Deleted == false).Select(x => x.SubsubCategoryid).Distinct().ToList();
                    var brandimages = con.SubsubCategorys.ToList();
                    foreach (var item in GetBrandcategory)
                    {
                        var bi = new BrandImages();
                        var brand = brandimages.Where(x => x.SubsubCategoryid == item).SingleOrDefault();
                        bi.Id = brand.SubsubCategoryid;
                        bi.Name = brand.SubsubcategoryName;
                        bi.imagepath = brand.LogoUrl;
                        BrandImages.Add(bi);
                    }
                    var response = new
                    {
                        brandimages = BrandImages,
                        Status = true,
                        Message = "brand images List"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        /// <summary>
        /// Get City Mapping List
        /// </summary>
        /// <returns></returns>
        [Route("GetCityMapping")]
        [HttpGet]
        public HttpResponseMessage GetCityMapping(int warehouseid)
        {
            try
            {
                using (var con = new AuthContext())
                {
                    var CityMapping = (from a in con.GMCityMappingDB
                                       join b in con.GMCityMappingMasterDB on a.CityMappingMasterId equals b.Id
                                       where a.WarehouseId == warehouseid
                                       select new
                                       {
                                           Id = a.Id,
                                           CityMappingMasterId = a.CityMappingMasterId,
                                           Name = b.Name,
                                           RequiredQuantity = a.RequiredQuantity,
                                           FilledQuantity = a.FilledQuantity
                                       }).ToList();

                    var response = new
                    {
                        CityMapping = CityMapping,
                        Status = true,
                        Message = "City Mapping List"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        /// <summary>
        /// GetTrainingDevelopment
        /// </summary>
        /// <returns></returns>
        [Route("GetTrainingDevelopment")]
        [HttpGet]
        public HttpResponseMessage GetTrainingDevelopment(int warehouseid)
        {
            try
            {
                using (var con = new AuthContext())
                {
                    var TrainingDevelopment = (from a in con.GMTrainingDevlopmentDB
                                               join b in con.GMDivisionMasterDB on a.DivisionId equals b.Id
                                               where b.IsTrainingRequired == true && a.WarehouseId == warehouseid
                                               select new
                                               {
                                                   Id = a.Id,
                                                   DivisionId = a.DivisionId,
                                                   Name = b.Name,
                                                   IsTrainingRequired = b.IsTrainingRequired
                                                   //UploadOnboardSheetPath = a.UploadOnboardSheetPath
                                               }).ToList();

                    var response = new
                    {
                        TrainingDevelopment = TrainingDevelopment,
                        Status = true,
                        Message = "Training and Development List"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        /// <summary>
        /// Get Training Development Images
        /// </summary>
        /// <param name="DivisionId"></param>
        /// <param name="GMTrainingDevlopmentId"></param>
        /// <returns>Training Development Images List</returns>
        [Route("GetTrainingDevelopmentImages")]
        [HttpGet]
        public HttpResponseMessage GetTrainingDevelopmentImages(int GMTrainingDevlopmentId)
        {
            try
            {
                using (var con = new AuthContext())
                {
                    var TrainingDevelopmentImages = con.GMTrainingDevlopmentImagesDB.Where(x => x.GMTrainingDevlopmentId == GMTrainingDevlopmentId).ToList();

                    var response = new
                    {
                        TrainingDevelopmentImages = TrainingDevelopmentImages,
                        Status = true,
                        Message = "Training Development Images List"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        /// <summary>
        /// Get Warehouse City Wise
        /// </summary>
        /// <returns>Warehouse City Wise list</returns>
        [Route("GetWarehouseCityWise")]
        [HttpGet]
        public HttpResponseMessage GetWarehouseCityWise()
        {
            try
            {
                var Citylist = new List<WarehouseListCityWise>();
                using (var con = new AuthContext())
                {
                    var citylist = con.Warehouses.Where(x => x.active == true && x.Deleted == false).Select(x => x.Cityid).Distinct();
                    var hublist = con.Warehouses.Where(x => x.active == true && x.Deleted == false && x.IsKPP == false).ToList();

                    foreach (var city in citylist)
                    {
                        var citywiselist = new WarehouseListCityWise();
                        var hubs = hublist.Where(x => x.Cityid == city).ToList();
                        var whlist = new List<WarehouseList>();
                        foreach (var hub in hubs)
                        {
                            var whl = new WarehouseList();
                            whl.WarehouseID = hub.WarehouseId;
                            whl.WarehouseName = hub.WarehouseName;
                            whlist.Add(whl);
                        }
                        citywiselist.CityID = city;
                        citywiselist.CityName = hublist.Where(x => x.Cityid == city).Select(x => x.CityName).FirstOrDefault();
                        citywiselist.warehouselist = whlist;
                        Citylist.Add(citywiselist);
                    }
                }
                var response = new
                {
                    WarehouseListCityWise = Citylist,
                    Status = true,
                    Message = "warehouselist List"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        /// <summary>
        /// Get Warehouse Progress
        /// </summary>
        /// <returns>warehouselist</returns>
        [Route("GetWarehouseProgress")]
        [HttpPost]
        public HttpResponseMessage GetWarehouseProgress(List<int> warehouse)
        {
            try
            {
                var warehouselist = new List<WarehouseProgress>();
                using (var con = new AuthContext())
                {
                    var hulist = con.GMWarehouseProgressDB.ToList();
                    foreach (var item in warehouse)
                    {
                        var wh = new WarehouseProgress();
                        var WP = con.GMWarehouseProgressDB.Where(x => x.WarehouseID == item).SingleOrDefault();
                        if (WP != null)
                        {
                            wh.WarehouseID = WP.WarehouseID;
                            wh.WarehouseName = WP.WarehouseName;
                            wh.Progress = WP.Progress;
                            warehouselist.Add(wh);
                        }
                    }
                }
                var response = new
                {
                    warehouselist = warehouselist,
                    Status = true,
                    Message = "warehouselist List"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        /// <summary>
        /// Upload Image for documents
        /// </summary>
        /// <returns>Image path</returns>
        [Route("UploadImage")]
        public async Task<HttpResponseMessage> HubLaunchImage()
        {
            try
            {
                //string LogoUrl = "";
                var fileuploadPath = HttpContext.Current.Server.MapPath("~/Warehouselaunch");

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

                //CloudinaryDotNet.Account account = new CloudinaryDotNet.Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);
                //Cloudinary cloudinary = new Cloudinary(account);
                //string filename = Path.GetFileNameWithoutExtension(originalFileName);

                string filename = Path.GetFileName(originalFileName);

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(originalFileName),
                    PublicId = "Warehouselaunch/" + filename,
                    Overwrite = true,
                    Invalidate = true,
                    Backup = false
                };

                //var uploadResult = cloudinary.Upload(uploadParams);
                //if (System.IO.File.Exists(fileuploadPath))
                //{
                //    System.IO.File.Delete(fileuploadPath);
                //}
                //LogoUrl = uploadResult.SecureUri.ToString();

                var response = new
                {
                    LogoUrl = filename,
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

        /// <summary>
        /// Add Infrastructure data 
        /// </summary>
        /// <param name="data">List<Infrastructure></param>
        /// <returns></returns>
        [Route("AddInfrastructuredata")]
        [HttpPost]
        public HttpResponseMessage PostInfrastructureData(List<Infrastructure> data)
        {
            bool result = false;
            try
            {
                using (var con = new AuthContext())
                {
                    foreach (var item in data)
                    {
                        if (item.isTask)
                        {
                            var infratask = con.GMInfrastructureTaskDB.Where(x => x.Id == item.Id && x.GMInfrastructureId == item.GMInfrastructureId).SingleOrDefault();
                            infratask.IsInstalled = item.IsInstalled;
                            infratask.IsUploaded = item.IsUploaded;
                            //infratask.ImagePath = item.ImagePath;
                            infratask.ModifiedDate = DateTime.Now;
                            infratask.ModifiedBy = item.UserId;
                            con.GMInfrastructureTaskDB.Attach(infratask);
                            con.Entry(infratask).State = System.Data.Entity.EntityState.Modified;
                        }
                        else
                        {
                            var infraLegal = con.GMInfrastructureLegalDB.Where(x => x.Id == item.Id && x.GMInfrastructureId == item.GMInfrastructureId).SingleOrDefault();
                            infraLegal.IsUploaded = item.IsUploaded;
                            //infraLegal.ImagePath = item.ImagePath;
                            infraLegal.ModifiedDate = DateTime.Now;
                            infraLegal.ModifiedBy = item.UserId;
                            con.GMInfrastructureLegalDB.Attach(infraLegal);
                            con.Entry(infraLegal).State = System.Data.Entity.EntityState.Modified;
                        }
                    }

                    if (con.Commit() > 0)
                    {
                        result = true;
                    }
                    var response = new
                    {
                        Status = result,
                        Message = "Data Updated"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        /// <summary>
        /// Post Trainging data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Route("AddTraingingdata")]
        [HttpPost]
        public HttpResponseMessage PostTraingingdata(List<Training> data)
        {
            try
            {
                using (var con = new AuthContext())
                {
                    foreach (var item in data)
                    {
                        if (item.UploadOnboardSheetPath != null || item.UploadOnboardSheetPath != string.Empty)
                        {
                            var infratask = con.GMTrainingDevlopmentDB.Where(x => x.Id == item.Id && x.DivisionId == item.DivisionId).SingleOrDefault();
                            //infratask.UploadOnboardSheetPath = item.UploadOnboardSheetPath;
                            infratask.ModifiedDate = DateTime.Now;
                            infratask.ModifiedBy = item.UserId;
                            infratask.WarehouseId = item.WarehouseId;
                            con.Commit();
                        }
                    }
                    var response = new
                    {
                        Status = true,
                        Message = "Data Updated"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        [Route("GetHubLaunchProgress")]
        [HttpGet]
        public HttpResponseMessage GetHubLaunchProgress(int Warehouseid)
        {
            var hubLaunchPercentDclist = new List<HubLaunchPercentDc>();
            try
            {
                HubLaunchPercentDc hubLaunchPercentDc = new HubLaunchPercentDc();
                hubLaunchPercentDc.Warehouseid = Warehouseid;
                using (var con = new AuthContext())
                {
                    var infraId = con.GMInfrastructureDB.Where(x => x.WarehouseId == Warehouseid).Select(x => x.Id).SingleOrDefault();
                    var GMInfrastructureLegal = con.GMInfrastructureLegalDB.Where(x => x.GMInfrastructureId == infraId).ToList();
                    var legalper = GMInfrastructureLegal.Where(x => x.IsUploaded).Count() * 100 / GMInfrastructureLegal.Count();
                    var GMInfrastructureTask = (from a in con.GMInfrastructureTaskDB.Where(x => x.GMInfrastructureId == infraId)
                                                join b in con.GMTaskListMasterDB on a.TaskListId equals b.Id
                                                select new
                                                {
                                                    Id = a.Id,
                                                    GMInfrastructureId = a.GMInfrastructureId,
                                                    TaskName = b.Name,
                                                    IsRequiredDoc = b.IsRequireddoc,
                                                    IsInstalled = a.IsInstalled,
                                                    IsUploaded = a.IsUploaded
                                                    //ImagePath = a.ImagePath
                                                }).ToList();

                    var taskdoclist = GMInfrastructureTask.Where(x => (x.IsRequiredDoc.HasValue && x.IsRequiredDoc.Value));
                    var tasklist = GMInfrastructureTask.Where(x => (x.IsRequiredDoc.HasValue && !x.IsRequiredDoc.Value));
                    var taskper = tasklist.Where(x => x.IsInstalled).Count() * 100 / tasklist.Count();
                    var taskDocper = taskdoclist.Where(x => x.IsUploaded.HasValue && x.IsUploaded.Value).Count() * 100 / taskdoclist.Count();
                    hubLaunchPercentDc.InfraPercent = (taskper / 4) + (taskDocper / 4) + (legalper / 2);

                    var GMPeople = con.GMPeopleDB.Where(x => x.WarehouseId == Warehouseid).ToList();
                    //hubLaunchPercentDc.PeoplePercent = GMPeople.Where(x => x.RequiredQuantity <= x.FilledQuantity).Count() * 100 / GMPeople.Count();
                    hubLaunchPercentDc.PeoplePercent = GMPeople.Select(x => x.RequiredQuantity < x.FilledQuantity ? x.RequiredQuantity : x.FilledQuantity).Sum() * 100 / GMPeople.Select(x => x.RequiredQuantity).Sum();
                    var GMProductPartners = con.GMProductPartnersDB.Where(x => x.WarehouseId == Warehouseid).ToList();
                    hubLaunchPercentDc.ProdPartnersPercent = GMProductPartners.Where(x => x.RequiredQuantity <= x.FilledQuantity).Count() * 100 / GMProductPartners.Count();
                    var GMTrainingDevlopment = (from a in con.GMTrainingDevlopmentDB.Where(x => x.WarehouseId == Warehouseid)
                                                join b in con.GMDivisionMasterDB on a.DivisionId equals b.Id
                                                select new
                                                {
                                                    TaskName = b.Name,
                                                    IsRequiredDoc = b.IsTrainingRequired,
                                                    IsUploaded = a.IsUploaded
                                                    //UploadOnboardSheetPath = a.UploadOnboardSheetPath                                                
                                                }).ToList();


                    //con.GMTrainingDevlopmentDB.Where(x => x.WarehouseId == Warehouseid).ToList();
                    hubLaunchPercentDc.TrainingPercent = GMTrainingDevlopment.Where(x => !x.IsUploaded).Count() * 100 / GMTrainingDevlopment.Count();

                    var GMCityMapping = con.GMCityMappingDB.Where(x => x.WarehouseId == Warehouseid).ToList();
                    hubLaunchPercentDc.CityMappingPercent = GMCityMapping.Where(x => x.RequiredQuantity <= x.FilledQuantity).Count() * 100 / GMCityMapping.Count();

                    hubLaunchPercentDc.TotalCompletePercent = (hubLaunchPercentDc.InfraPercent == 0 ? 0 : (hubLaunchPercentDc.InfraPercent / 4))
                        + (hubLaunchPercentDc.PeoplePercent == 0 ? 0 : hubLaunchPercentDc.PeoplePercent / 4)
                        + (hubLaunchPercentDc.ProdPartnersPercent == 0 ? 0 : hubLaunchPercentDc.ProdPartnersPercent / 4)
                        //+ (hubLaunchPercentDc.TrainingPercent == 0 ? 0 : hubLaunchPercentDc.TrainingPercent / 5)
                        + (hubLaunchPercentDc.CityMappingPercent == 0 ? 0 : hubLaunchPercentDc.CityMappingPercent / 4);

                    if (hubLaunchPercentDc.TotalCompletePercent > 0)
                    {
                        var warehouse = con.GMWarehouseProgressDB.Where(x => x.WarehouseID == Warehouseid).FirstOrDefault();
                        if (warehouse != null)
                        {
                            if (warehouse.Progress != hubLaunchPercentDc.TotalCompletePercent)
                            {
                                if (warehouse.Progress != 100)
                                {
                                    warehouse.Progress = hubLaunchPercentDc.TotalCompletePercent;
                                    con.Commit();
                                }

                            }
                        }
                    }

                    var cd = con.GMInfrastructureDB.Where(x => x.WarehouseId == Warehouseid).Select(t => new
                    {
                        WarehouseLaunchDays = t.WarehouseLaunchDays,
                        CreatedDate = t.CreatedDate
                    }).FirstOrDefault();
                    var days = Convert.ToInt16((DateTime.Now - cd.CreatedDate).TotalDays);
                    hubLaunchPercentDc.DaysRemaining = cd.WarehouseLaunchDays - days;

                    var islaunched = con.GMWarehouseProgressDB.Where(x => x.WarehouseID == Warehouseid).FirstOrDefault();
                    hubLaunchPercentDc.Islaunched = islaunched.IsLaunched;

                    hubLaunchPercentDclist.Add(hubLaunchPercentDc);
                }

                var response = new
                {
                    HubLaunchPercent = hubLaunchPercentDclist,
                    Status = true,
                    Message = "hub Launch Progress"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                string[] hub = { };
                var response = new
                {
                    HubLaunchPercent = hub,
                    Status = false,
                    Message = "data not found"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        [Route("FillHubProgress")]
        [HttpGet]
        public bool FillHubProgress()
        {
            try
            {
                using (var con = new AuthContext())
                {
                    var warehouse = con.GMWarehouseProgressDB.Where(x => x.IsLaunched == false).Select(x => new { x.WarehouseID }).ToList();
                    foreach (var Witem in warehouse)
                    {

                        int Warehouseid = Witem.WarehouseID;
                        //var agents = con.Peoples.Where(x => x.Active == true && x.Deleted == false && x.WarehouseId == Warehouseid && x.Type == "Agent").Select(t => new
                        //{
                        //    Name = t.DisplayName,
                        //    Id = t.PeopleID,
                        //}).ToList();


                        string query = "select count(distinct p.PeopleID) count from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.WarehouseId={0} and r.Name='Agent'  and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                        query = string.Format(query, Warehouseid);

                        var AgentCount = con.Database.SqlQuery<int>(query).FirstOrDefault();

                        query = "select r.Name RoleName, count(ur.UserId) RoleCount from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where warehouseid={0} group by r.Name";
                        query = string.Format(query, Warehouseid);

                        var Rolecount = con.Database.SqlQuery<WhRoleCount>(query).ToList();

                        var SalesExecutiveCount = Rolecount.FirstOrDefault(x => x.RoleName == "WH sales executives")?.RoleCount ?? 0;
                        var ServiceExecutiveCount = Rolecount.FirstOrDefault(x => x.RoleName == "WH Service lead")?.RoleCount ?? 0;
                        var BusinessDevCount = Rolecount.FirstOrDefault(x => x.RoleName == "Sales Executive")?.RoleCount ?? 0;
                        var DispatchExecutive = Rolecount.FirstOrDefault(x => x.RoleName == "Delivery Excecutive")?.RoleCount ?? 0;
                        var InventoryExecutiveCount = Rolecount.FirstOrDefault(x => x.RoleName == "WH Super visor")?.RoleCount ?? 0;
                        var CashierCount = Rolecount.FirstOrDefault(x => x.RoleName == "WH cash manager")?.RoleCount ?? 0;
                        var VehicleCount = con.VehicleDb.Where(x => x.WarehouseId == Warehouseid && x.isActive == true && x.isDeleted == false).Count();

                        var RetailersCount = con.Customers.Where(x => x.Warehouseid == Warehouseid && x.Active == true && x.Deleted == false).Count();
                        var KPPCount = con.Warehouses.Where(x => x.IsKPP == true && x.WarehouseId == Warehouseid).Count();



                        var peoples = (from a in con.GMPeopleDB
                                       join b in con.GMDivisionMasterDB on a.DivisionId equals b.Id
                                       where a.WarehouseId == Warehouseid
                                       select new
                                       {
                                           Id = a.Id,
                                           DivisionId = a.DivisionId,
                                           Name = b.Name,
                                           RequiredQuantity = a.RequiredQuantity,
                                           FilledQuantity = a.FilledQuantity
                                       }).ToList();

                        foreach (var item in peoples)
                        {
                            if (item.Name == "Sales Executive")
                            {
                                var ps = con.GMPeopleDB.Where(x => x.WarehouseId == Warehouseid && x.Id == item.Id).SingleOrDefault();
                                ps.FilledQuantity = SalesExecutiveCount;
                                con.Commit();
                            }
                            if (item.Name == "Service Executive")
                            {
                                var ps = con.GMPeopleDB.Where(x => x.WarehouseId == Warehouseid && x.Id == item.Id).SingleOrDefault();
                                ps.FilledQuantity = ServiceExecutiveCount;
                                con.Commit();
                            }
                            if (item.Name == "Business Development")
                            {
                                var ps = con.GMPeopleDB.Where(x => x.WarehouseId == Warehouseid && x.Id == item.Id).SingleOrDefault();
                                ps.FilledQuantity = BusinessDevCount;
                                con.Commit();
                            }
                            if (item.Name == "Dispatch Executive")
                            {
                                var ps = con.GMPeopleDB.Where(x => x.WarehouseId == Warehouseid && x.Id == item.Id).SingleOrDefault();
                                ps.FilledQuantity = DispatchExecutive;
                                con.Commit();
                            }
                            if (item.Name == "Inventory Executive")
                            {
                                var ps = con.GMPeopleDB.Where(x => x.WarehouseId == Warehouseid && x.Id == item.Id).SingleOrDefault();
                                ps.FilledQuantity = InventoryExecutiveCount;
                                con.Commit();
                            }
                            if (item.Name == "Cashier")
                            {
                                var ps = con.GMPeopleDB.Where(x => x.WarehouseId == Warehouseid && x.Id == item.Id).SingleOrDefault();
                                ps.FilledQuantity = CashierCount;
                                con.Commit();
                            }
                            if (item.Name == "Purchase Vehicle")
                            {
                                var ps = con.GMPeopleDB.Where(x => x.WarehouseId == Warehouseid && x.Id == item.Id).SingleOrDefault();
                                ps.FilledQuantity = VehicleCount;
                                con.Commit();
                            }
                        }


                        var itemmaster = (from a in con.itemMasters
                                          where a.WarehouseId == Warehouseid && a.SupplierId > 0 && a.Deleted == false
                                          select new
                                          {
                                              ItemId = a.ItemId,
                                              SubsubCategoryid = a.SubsubCategoryid
                                          }).ToList();

                        var ProductCount = itemmaster.Select(x => x.ItemId).Distinct().Count();
                        var BrandCount = itemmaster.Select(x => x.SubsubCategoryid).Distinct().Count();

                        var ProductPartners = (from a in con.GMProductPartnersDB
                                               join b in con.GMProductPartnerMasterDB on a.ProductPartnersId equals b.Id
                                               where a.WarehouseId == Warehouseid
                                               select new
                                               {
                                                   Id = a.Id,
                                                   ProductPartnersId = a.ProductPartnersId,
                                                   Type = b.Type,
                                                   Name = b.Name,
                                                   RequiredQuantity = a.RequiredQuantity,
                                                   FilledQuantity = a.FilledQuantity
                                               }).ToList();

                        foreach (var item in ProductPartners)
                        {
                            if (item.Name == "Brand")
                            {
                                var ps = con.GMProductPartnersDB.Where(x => x.WarehouseId == Warehouseid && x.Id == item.Id).SingleOrDefault();
                                ps.FilledQuantity = BrandCount;
                                con.Commit();
                            }
                            if (item.Name == "Products")
                            {
                                var ps = con.GMProductPartnersDB.Where(x => x.WarehouseId == Warehouseid && x.Id == item.Id).SingleOrDefault();
                                ps.FilledQuantity = ProductCount;
                                con.Commit();
                            }
                            if (item.Name == "Agents")
                            {
                                var ps = con.GMProductPartnersDB.Where(x => x.WarehouseId == Warehouseid && x.Id == item.Id).SingleOrDefault();
                                ps.FilledQuantity = AgentCount;
                                con.Commit();
                            }
                            if (item.Name == "KPP")
                            {
                                var ps = con.GMProductPartnersDB.Where(x => x.WarehouseId == Warehouseid && x.Id == item.Id).SingleOrDefault();
                                ps.FilledQuantity = KPPCount;
                                con.Commit();
                            }
                        }

                        var Retailers = con.Customers.Where(x => x.Active == true && x.Deleted == false && x.Warehouseid == Warehouseid).Count();
                        var CityMapping = (from a in con.GMCityMappingDB
                                           join b in con.GMCityMappingMasterDB on a.CityMappingMasterId equals b.Id
                                           where a.WarehouseId == Warehouseid
                                           select new
                                           {
                                               Id = a.Id,
                                               CityMappingMasterId = a.CityMappingMasterId,
                                               Name = b.Name,
                                               RequiredQuantity = a.RequiredQuantity,
                                               FilledQuantity = a.FilledQuantity
                                           }).ToList();
                        var ClusterCount = con.Clusters.Where(x => x.Active == true && x.Deleted == false && x.WarehouseId == Warehouseid).Count();

                        foreach (var item in CityMapping)
                        {
                            if (item.Name == "Cluster Mapping")
                            {
                                var ps = con.GMCityMappingDB.Where(x => x.WarehouseId == Warehouseid && x.Id == item.Id).SingleOrDefault();
                                ps.FilledQuantity = ClusterCount;
                                con.Commit();
                            }

                            if (item.Name == "Retailer Signups")
                            {
                                var ps = con.GMCityMappingDB.Where(x => x.WarehouseId == Warehouseid && x.Id == item.Id).SingleOrDefault();
                                ps.FilledQuantity = Retailers;
                                con.Commit();
                            }
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [Route("WarehouseLaunched")]
        [HttpGet]
        public HttpResponseMessage WarehouseLaunched(int WarehouseId)
        {
            try
            {
                using (var con = new AuthContext())
                {
                    var Warehouse = con.GMWarehouseProgressDB.Where(x => x.WarehouseID == WarehouseId).FirstOrDefault();
                    if (Warehouse != null)
                    {
                        if (Warehouse.Progress == 100)
                        {
                            if (Warehouse.IsLaunched == false)
                            {
                                Warehouse.IsLaunched = true;
                                con.Commit();
                                var response = new
                                {
                                    Status = true,
                                    Message = "warehouse launched"
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, response);
                            }
                            else
                            {
                                var response = new
                                {
                                    Status = true,
                                    Message = "warehouse allready launched"
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, response);
                            }
                        }
                        else
                        {
                            var response = new
                            {
                                Status = false,
                                Message = "warehouse launch criteria not fulfill"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, response);
                        }
                    }
                    else
                    {
                        var response = new
                        {
                            Status = false,
                            Message = "warehouse not found"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }
        #endregion

        #region GrowthModuleLogin
        [Route("GetPeople")]
        [HttpGet]
        public HttpResponseMessage GetPeople()
        {
            try
            {
                using (var con = new AuthContext())
                {
                    var peoples = con.Peoples.Where(x => x.Active == true && x.Deleted == false).Select(x => new { x.PeopleID, x.DisplayName }).OrderBy(x => x.DisplayName).ToList();

                    var response = new
                    {
                        peopleslist = peoples,
                        Status = true,
                        Message = "Peoples List"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        [Route("GetWarehousePeoplewise")]
        [HttpGet]
        public HttpResponseMessage GetWarehousePeoplewise(int PeopleID)
        {
            try
            {
                using (var con = new AuthContext())
                {
                    var Warehouse = con.GMLoginDB.Where(x => x.PeopleID == PeopleID).Select(x => new { x.WarehouseId }).ToList();
                    if (Warehouse.Count > 0)
                    {
                        var response = new
                        {
                            WarehouseList = Warehouse,
                            Status = true,
                            Message = "Warehouse List"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                    else
                    {
                        var response = new
                        {
                            WarehouseList = Warehouse,
                            Status = false,
                            Message = "User not found"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        [Route("AddGMLoginData")]
        [HttpPost]
        public HttpResponseMessage AddGMLoginData(GMPermission permission)
        {
            try
            {
                using (var con = new AuthContext())
                {
                    foreach (var item in permission.WarehouseIds)
                    {
                        var login = con.GMLoginDB.Where(x => x.PeopleID == permission.PeopleId && x.WarehouseId == item).Count();
                        if (login == 0)
                        {
                            var gmlogin = new GMLogin();
                            gmlogin.PeopleID = permission.PeopleId;
                            gmlogin.PeopleName = permission.PeopleName;
                            gmlogin.WarehouseId = item;
                            con.GMLoginDB.Add(gmlogin);
                            con.Commit();
                        }
                    }
                    var response = new
                    {
                        Status = true,
                        Message = "Record Inserted"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        [Route("GetGMLoginpermission")]
        [HttpGet]
        public HttpResponseMessage GetGMLoginpermission()
        {
            try
            {
                using (var db = new AuthContext())
                {
                    var GMLoginpermissionList = (from c in db.GMLoginDB
                                                 join p in db.Warehouses
                                                 on c.WarehouseId equals p.WarehouseId
                                                 select new
                                                 {
                                                     ID = c.ID,
                                                     WarehouseId = p.WarehouseId,
                                                     WarehouseName = p.CityName + " " + p.WarehouseName,
                                                     PeopleName = c.PeopleName,
                                                     PeopleID = c.PeopleID
                                                 }).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, GMLoginpermissionList);

                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex);
            }
        }

        [Route("DeleteLoginpermission")]
        [HttpGet]
        public HttpResponseMessage DeleteLoginpermission(int id)
        {
            try
            {
                using (var con = new AuthContext())
                {
                    var user = con.GMLoginDB.Where(x => x.ID == id).SingleOrDefault();
                    if (user != null)
                    {
                        con.Entry(user).State = EntityState.Deleted;
                        con.Commit();
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, "delete permission");

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex);
            }
        }

        #endregion

    }

    public class BrandImages
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string imagepath { get; set; }
    }
    public class WarehouseListCityWise
    {
        public int CityID { get; set; }
        public string CityName { get; set; }
        public List<WarehouseList> warehouselist { get; set; }
    }
    public class WarehouseList
    {
        public int WarehouseID { get; set; }
        public string WarehouseName { get; set; }
    }
    public class WarehouseProgress
    {
        public int WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public int Progress { get; set; }
    }
    public class Training
    {
        public int Id { get; set; }
        public int DivisionId { get; set; }
        public string Name { get; set; }
        public bool IsTrainingRequired { get; set; }
        public string UploadOnboardSheetPath { get; set; }
        public int UserId { get; set; }
        public int WarehouseId { get; set; }
    }
    public class Infrastructure
    {
        public int Id { get; set; }
        public int GMInfrastructureId { get; set; }
        public bool IsInstalled { get; set; }
        public bool IsUploaded { get; set; }
        public string ImagePath { get; set; }
        public int UserId { get; set; }
        public bool isTask { get; set; }
    }
    public class HubLaunchPercentDc
    {
        public int Warehouseid { get; set; }
        public int TotalCompletePercent { get; set; }
        public int InfraPercent { get; set; }
        public int PeoplePercent { get; set; }
        public int ProdPartnersPercent { get; set; }
        public int TrainingPercent { get; set; }
        public int CityMappingPercent { get; set; }
        public int DaysRemaining { get; set; }
        public bool Islaunched { get; set; }
    }
    public class InfraLegalImage
    {
        public int InfraLegalId { get; set; }
        public string ImagePath { get; set; }
    }
    public class GMPermission
    {
        public int PeopleId { get; set; }
        public string PeopleName { get; set; }
        public List<int> WarehouseIds { get; set; }
    }
    public class WarehouseIds
    {
        public int WarehouseID { get; set; }
    }
    public class InfraTaskImage
    {
        public int InfraTaskId { get; set; }
        public string ImagePath { get; set; }
    }
    public class TrainingDevelopmentImage
    {
        public int GMTrainingDevlopmentId { get; set; }
        public string ImagePath { get; set; }
    }
    public class InfraPercentDc
    {
        public int TotalCompletePercent { get; set; }
    }
    public class PeoplePercentDc
    {
        public int TotalCompletePercent { get; set; }
    }
    public class ProdPartnersPercentDc
    {
        public int TotalCompletePercent { get; set; }
    }
    public class TrainingPercentDc
    {
        public int TotalCompletePercent { get; set; }
    }
    public class CityMappingPercentDc
    {
        public int TotalCompletePercent { get; set; }
    }

    public class WhRoleCount
    {
        public string RoleName { get; set; }
        public int RoleCount { get; set; }
    }
}