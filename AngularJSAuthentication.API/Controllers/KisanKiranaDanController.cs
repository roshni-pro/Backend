using AngularJSAuthentication.API.DataContract;
using AngularJSAuthentication.Model.KisanDan;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{

    [RoutePrefix("api/KishanDan")]
    public class KisanKiranaDanController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("GetKishan")]
        [HttpGet]
        public async Task<KisanDaan> Getkisandan(int warehouseid, string lang, int Customerid)
        {
            KisanDaan kisanDaan = null;
            using (var authContext = new AuthContext())
            {
                string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
                var query = "select sum(Kisandanamount) TotalDaan, sum(case when customerid = " + Customerid + " then KisanDanAmount else 0 end) CustomerTotalDaan  from customerkisandans";
                kisanDaan = await authContext.Database.SqlQuery<KisanDaan>(query).FirstOrDefaultAsync();
                query = "select * from KisanDaanGallaries where IsActive= 1 and IsDeleted=0 and IsGallaryImage=0";
                List<KisanDaanGallary> image = await authContext.Database.SqlQuery<KisanDaanGallary>(query).ToListAsync();
                query = "select * from KisanDanDescriptions where IsActive= 1 and IsDeleted=0";
                KisanDanDescription kisanDanDescription = await authContext.Database.SqlQuery<KisanDanDescription>(query).FirstOrDefaultAsync();
                if (image != null && image.Any())
                    image.ForEach(x => x.KisanDaanImage = baseUrl + x.KisanDaanImage);
                kisanDaan.KisanDaanGallarys = image;
                kisanDaan.KisanDanDescription = kisanDanDescription;
            }
            return kisanDaan;
        }


        [Route("GetkisandaanDetail")]
        [HttpGet]
        public async Task<List<CustomerKisanDan>> GetkisandaanDetail(int warehouseid, string lang, int Customerid)
        {
            List<CustomerKisanDan> CustomerKisanDans = new List<CustomerKisanDan>();
            using (var authContext = new AuthContext())
            {
                CustomerKisanDans = await authContext.CustomerKisanDan.Where(x => x.CustomerId == Customerid && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToListAsync();
            }
            return CustomerKisanDans;
        }

        [Route("GetKisandaanImage")]
        [HttpGet]
        public async Task<List<KisanDaanGallary>> GetKisandaanImage()
        {
            using (AuthContext context = new AuthContext())
            {
                string query = "select * from KisanDaanGallaries where IsActive= 1 and IsDeleted=0 and IsGallaryImage=1";
                List<KisanDaanGallary> image = await context.Database.SqlQuery<KisanDaanGallary>(query).ToListAsync();


                string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                foreach (var item in image)
                {
                    item.KisanDaanImage = baseUrl + "" + item.KisanDaanImage;
                }


                return image;

            }
        }

        [Route("AddDescription")]
        [HttpPost]
        public KisanDanDescription AddDescription(KisanDanDescriptionDC KisanDanDescriptionDC)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            KisanDanDescription Kd = new KisanDanDescription();
            using (AuthContext db = new AuthContext())
            {
                if (KisanDanDescriptionDC != null)
                {
                    if (KisanDanDescriptionDC.Id > 0)
                    {
                        Kd = db.KisanDanDescription.Where(x => x.Id == KisanDanDescriptionDC.Id).FirstOrDefault();
                        if (Kd != null)
                        {
                            Kd.WhyUseKd = KisanDanDescriptionDC.WhyUseKd;
                            Kd.HowItWorkKd = KisanDanDescriptionDC.HowItWorkKd;
                            Kd.ModifiedDate = DateTime.Now;
                            Kd.ModifiedBy = userid;
                            db.Entry(Kd).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        Kd.WhyUseKd = KisanDanDescriptionDC.WhyUseKd;
                        Kd.HowItWorkKd = KisanDanDescriptionDC.HowItWorkKd;
                        Kd.IsActive = true;
                        Kd.IsDeleted = false;
                        Kd.CreatedDate = DateTime.Now;
                        Kd.CreatedBy = userid;
                        db.KisanDanDescription.Add(Kd);

                    }
                    db.Commit();
                }
                return Kd;
            }
        }

        [Route("UploadKisanDanImage")]
        [HttpPost]
        public IHttpActionResult UploadKisanDanImage()
        {
            string LogoUrl = "";
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {

                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/images/KisandanImages")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/images/KisandanImages"));

                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/KisandanImages"), httpPostedFile.FileName);
                        string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                        httpPostedFile.SaveAs(LogoUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/KisandanImages", LogoUrl);

                        LogoUrl = "/images/KisandanImages/" + httpPostedFile.FileName;

                    }

                }
                return Created<string>(LogoUrl, LogoUrl);
            }
            catch (Exception ex)
            {
                logger.Error("Error in Kisandan Method: " + ex.Message);
                return null;
            }
        }


        [Route("AddGallaryImage")]
        [HttpPost]
        public KisanDaanGallary AddGallaryImage(KisanDanGallaryDC KisanDanGallaryDC)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            KisanDaanGallary Image = new KisanDaanGallary();

            using (AuthContext db = new AuthContext())
            {
                if (KisanDanGallaryDC != null)
                {
                    if (KisanDanGallaryDC.Id > 0)
                    {
                        Image = db.KisanDaanGallary.Where(x => x.Id == KisanDanGallaryDC.Id).FirstOrDefault();

                        if (Image != null)
                        {
                            Image.ModifiedDate = DateTime.Now;
                            Image.IsActive = KisanDanGallaryDC.IsActive;
                            Image.IsDeleted = KisanDanGallaryDC.IsDeleted;
                            Image.ModifiedBy = userid;
                            db.Entry(Image).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        Image.Description = KisanDanGallaryDC.Description;
                        Image.KisanDaanImage = KisanDanGallaryDC.LogoUrl;
                        Image.IsGallaryImage = KisanDanGallaryDC.IsGallaryImage;
                        Image.IsActive = true;
                        Image.IsDeleted = false;
                        Image.CreatedDate = DateTime.Now;
                        Image.CreatedBy = userid;
                        db.KisanDaanGallary.Add(Image);

                    }
                    db.Commit();
                }
                return Image;
            }
        }

        [Route("GetImageData")]
        [HttpGet]
        public async Task<List<KisanDaanGallary>> Get()
        {
            using (AuthContext context = new AuthContext())
            {
                string query = "select * from KisanDaanGallaries where IsDeleted=0 ";
                List<KisanDaanGallary> image = await context.Database.SqlQuery<KisanDaanGallary>(query).ToListAsync();
                string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                foreach (var item in image)
                {
                    item.KisanDaanImage = baseUrl + "" + item.KisanDaanImage;
                }


                return image;

            }
        }

        [Route("GetDescriptiondata")]
        [HttpGet]
        public async Task<KisanDanDescription> GetDescriptionsdata()
        {
            using (AuthContext context = new AuthContext())
            {
                string query = "select * from KisanDanDescriptions where IsActive= 1 and IsDeleted=0 ";
                KisanDanDescription detailsdata = await context.Database.SqlQuery<KisanDanDescription>(query).FirstOrDefaultAsync();

                return detailsdata;
            }

        }


        [Route("Getcustkdaan")]
        [HttpGet]
        public async Task<List<CustomerKisanDaanDc>> Getcustkdaan()
        {
            using (AuthContext context = new AuthContext())
            {
                string query = "Select Top 10 c.ShopName,c.Shopimage,c.Skcode,Sum(kd.KisanKiranaAmount) KisanKiranaAmount,Sum(kd.KisanDanAmount) KisanDanAmount from Customers c inner join CustomerKisanDans kd on c.CustomerId = kd.CustomerId Group by c.ShopName,c.Shopimage,c.Skcode Order by Sum(kd.KisanDanAmount) Desc";
                List<CustomerKisanDaanDc> data = await context.Database.SqlQuery<CustomerKisanDaanDc>(query).ToListAsync();
                string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                foreach (var item in data)
                {
                    item.Shopimage = baseUrl + "/" + item.Shopimage;
                }

                return data;
            }

        }

        [Route("GetCustomerDashboard")]
        [HttpPost]
        public kisandaanSearchDC CustomerDashboard(KisanDaanDashboardDc KisanDaanDashboard)
        {
            using (AuthContext context = new AuthContext())
            {
                string query = "";
                query = "exec GetKisanDaanDetail " + (KisanDaanDashboard.WarehouseId.HasValue ? KisanDaanDashboard.WarehouseId.Value : 0);
                query += "," + (string.IsNullOrEmpty(KisanDaanDashboard.Skcode) ? "null" : ("'" + KisanDaanDashboard.Skcode + "'")) + ",";
                query += (!KisanDaanDashboard.StartDate.HasValue ? "null,null" : "'" + KisanDaanDashboard.StartDate.Value.ToString("yyyy-MM-dd") + "','" + KisanDaanDashboard.EndDate.Value.ToString("yyyy-MM-dd") + "'") + "," + KisanDaanDashboard.PageNumber.Value.ToString() + "," + KisanDaanDashboard.PageSize.Value.ToString();

                var result = context.Database.SqlQuery<CustomerdashboardDc>(query).ToList();
                kisandaanSearchDC kisandaansearchdc = new kisandaanSearchDC();
                if (result != null)
                {
                    kisandaansearchdc.total_count = result.Count;
                    kisandaansearchdc.Totaldan = result.Any() ? result.FirstOrDefault().Total : 0;
                    kisandaansearchdc.Customerdashboard = result;
                }
                return kisandaansearchdc;
            }

        }

    }



    public class CustomerKisanDandc
    {
        public decimal TotalDan { get; set; }
        public List<CustomerKisanDan> customerKisanDan { get; set; }

    }

    public class KisanDaan
    {
        public decimal TotalDaan { get; set; }
        public decimal CustomerTotalDaan { get; set; }

        public List<KisanDaanGallary> KisanDaanGallarys { get; set; }
        public KisanDanDescription KisanDanDescription { get; set; }
    }

}

