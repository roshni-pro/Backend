using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Companys")]
    public class CompanyController : ApiController
    {       
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Authorize]
        [Route("")]
        public Company Get()
        {
            logger.Info("Get Company: ");
            using (AuthContext db = new AuthContext())
            {

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
                   

                        return db.GetCompanybyCompanyId(compid);
                   
                }

                catch (Exception ex)
                {
                    logger.Error("Error in getting Company " + ex.Message);
                    return null;
                }
            }
        }



        [ResponseType(typeof(Company))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public Company Put(Company item)
        {
            using (AuthContext db = new AuthContext())
            {
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
                    logger.Info("End Get Company: ");


                    return db.PutCompany(item);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting Company " + ex.Message);
                    return null;
                }
            }
        }

        [HttpGet]
        [Route("GetCompanyDetails")]

        public HttpResponseMessage GetCompanyDetails()
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    
                        var companydetails = db.CompanyDetailsDB.Where(x => x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                        var response = new
                        {
                            companydetails = companydetails,
                            Status = true,
                            Message = "companydetails"
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

        }


        [HttpGet]
        [Route("GetSupplierHelpDeskNo")]
        public string GetSupplierHelpDeskNo()
        {
            var helpDeskNo = "";
            
                using (var con = new AuthContext())
                {
                    helpDeskNo = con.CompanyDetailsDB.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => x.SupplierHelpDeskNo).FirstOrDefault();
                }
           
            return helpDeskNo;
        }

        //To Insert Video link
        [Route("Videohelperlink")]
        [HttpPost]
        public bool Videohelperlink(string Title, string Url)
        {
            var result = false;
            var videohelper = new videoHelperDc();
            MongoDbHelper<videoHelperDc> mongoDbHelper = new MongoDbHelper<videoHelperDc>();
            videohelper = new videoHelperDc
            {
                title = Title,
                videoUrl = Url
            };
            var data = videohelper;
            if (videohelper != null)
            {
                mongoDbHelper.Insert(data);
                result = true;
            }
            return result;
        }


        [Route("VideoHelper")]
        [HttpGet]
        public HttpResponseMessage VideoHelper()
        {
            var videohelper = new MongoDbHelper<videoHelperDc>();
            MongoDbHelper<videoHelperDc> mongoDbHelper = new MongoDbHelper<videoHelperDc>();
            var data = mongoDbHelper.Select(x => x.Id != null).ToList();
            return Request.CreateResponse(HttpStatusCode.OK, data);

        }

        [Route("UpdateUpComingFlashDealImage")]
        [HttpPost]
        public bool UpdateUpComingFlashDealImage(FlashDealImageDc FlashDealImageDc)
        {
            bool result = false;
            using (AuthContext db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int  userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                if (db.NoFlashDealImage.Any(x => x.WarehouseId == FlashDealImageDc.WarehouseId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)))
                {
                    var noFlashDealImage = db.NoFlashDealImage.FirstOrDefault(x => x.WarehouseId == FlashDealImageDc.WarehouseId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                    noFlashDealImage.IsActive = false;
                    noFlashDealImage.IsDeleted = true;
                    noFlashDealImage.ModifiedBy = userid;
                    noFlashDealImage.ModifiedDate = DateTime.Now;
                    db.Entry(noFlashDealImage).State = System.Data.Entity.EntityState.Modified;
                }

                var NewNoFlashDealImage = new NoFlashDealImage
                {
                    CreatedBy=userid,
                    CreatedDate=DateTime.Now,
                    ImagePath= FlashDealImageDc.ImagePath,
                    IsActive=true,
                    IsDeleted=false,
                    WarehouseId= FlashDealImageDc.WarehouseId
                };
                db.NoFlashDealImage.Add(NewNoFlashDealImage);
                result = db.Commit() > 0;
            }
            return result;
        }











        [Route("RemoveUpComingFlashDealImage")]
        [HttpPost]
        public bool RemoveUpComingFlashDealImage(FlashDealImageDc FlashDealImageDc)
        {
            bool result = false;
            using (AuthContext db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                if (db.NoFlashDealImage.Any(x => x.WarehouseId == FlashDealImageDc.WarehouseId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)))
                {
                    var noFlashDealImage = db.NoFlashDealImage.FirstOrDefault(x => x.WarehouseId == FlashDealImageDc.WarehouseId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                    noFlashDealImage.IsActive = false;
                    noFlashDealImage.IsDeleted = true;
                    noFlashDealImage.ModifiedBy = userid;
                    noFlashDealImage.ModifiedDate = DateTime.Now;
                    db.Entry(noFlashDealImage).State = System.Data.Entity.EntityState.Modified;
                }

                var NewNoFlashDealImage = new NoFlashDealImage
                {
                    CreatedBy = userid,
                    CreatedDate = DateTime.Now,
                    ImagePath = FlashDealImageDc.ImagePath,
                    IsActive = false,
                    IsDeleted = true,
                    WarehouseId = FlashDealImageDc.WarehouseId
                };
                db.NoFlashDealImage.Add(NewNoFlashDealImage);
                result = db.Commit() > 0;
            }
            return result;
        }

        [Route("GetUpComingFlashDealImage")]
        [HttpGet]
        public List<FlashDealImageDc> GetUpComingFlashDealImage()
        {
            List<FlashDealImageDc> FlashDealImageDcs = new List<FlashDealImageDc>();
            string query = "select a.WarehouseId,a.WarehouseName,b.ImagePath from Warehouses a left join NoFlashDealImages b on a.WarehouseId=b.WarehouseId and b.IsActive=1 and b.IsDeleted=0 where a.Deleted = 0 and(a.IsKPP = 0 or a.IsKppShowAsWH = 1) and a.active = 1";
            using (AuthContext db = new AuthContext())
            {
                FlashDealImageDcs = db.Database.SqlQuery<FlashDealImageDc>(query).ToList();
            }
            return FlashDealImageDcs;
        }
    }

    public class FlashDealImageDc
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string ImagePath { get; set; }
    }

}



