using AgileObjects.AgileMapper;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.FileUpload;
using AngularJSAuthentication.Model.Agent;
using Nito.AspNetBackgroundTasks;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.Agent
{
    [RoutePrefix("api/AgentPitch")]
    public class AgentPitchMasterController : ApiController
    {

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);


        #region Get Pending Pitching
        [HttpGet]
        [Route("GetPendingPitching")]
        public HttpResponseMessage GetPendingPitching(int PeopleId)
        {
            bool status = false;
            string Msg = "";
            var resultObj = new AgentPitchMasterDc();
            using (var context = new AuthContext())
            {
                var apm = (from c in context.AgentPitchMasters.Where(x => x.ExecutiveId == PeopleId && x.IsDeleted == false && x.EndDate == null)
                           join p in context.Customers on c.CustomerId equals p.CustomerId into ps
                           from p in ps.DefaultIfEmpty()
                           select new
                           {
                               p.Active,
                               c.CustomerId,
                               c.ExecutiveId,
                               c.StartDate,
                               c.ShopImageUrl,
                               p.Skcode,
                               p.Name,
                               c.Id,
                           }).FirstOrDefault();
                if (apm != null)
                {
                    resultObj = Mapper.Map(apm).ToANew<AgentPitchMasterDc>();
                    status = true;
                    Msg = "Record found ";
                }
                else
                {
                    Msg = "No record found ";
                    status = false;
                }
            }
            var res = new ResMsg()
            {
                Status = status,
                Message = Msg,
                AgentPitchMasters = resultObj
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        #endregion


        #region Start End Pitching
        [HttpPost]
        [Route("StartUpdatePitching")]

        public HttpResponseMessage StartUpdatePitching(AgentPitchMasterDc AgentPitchMaster)
        {
            bool status = false;
            string Msg = "";
            var resultObj = new AgentPitchMasterDc();

            using (var context = new AuthContext())
            {
                if (AgentPitchMaster != null && AgentPitchMaster.ExecutiveId > 0) //&& AgentPitchMaster.CustomerId > 0
                {
                    AgentPitchMaster Addapm = new AgentPitchMaster();
                    Addapm.StartDate = AgentPitchMaster.StartDate;
                    Addapm.EndDate = indianTime;
                    Addapm.CustomerId = AgentPitchMaster.CustomerId;
                    Addapm.ExecutiveId = AgentPitchMaster.ExecutiveId;
                    Addapm.ShopImageUrl = AgentPitchMaster.ShopImageUrl;
                    Addapm.AudioUrl = AgentPitchMaster.AudioUrl;
                    Addapm.lat = AgentPitchMaster.lat;
                    Addapm.lg = AgentPitchMaster.lg;
                    Addapm.IsDeleted = false;
                    Addapm.IsActive = true;
                    Addapm.CreatedBy = AgentPitchMaster.ExecutiveId;
                    Addapm.CreatedDate = indianTime;
                    context.AgentPitchMasters.Add(Addapm);
                    if (context.Commit() > 0)
                    {
                        status = true;
                        Msg = "Successfully Started....";
                        resultObj = Mapper.Map(Addapm).ToANew<AgentPitchMasterDc>();
                    }
                    else { Msg = "Something went wrong "; }
                }
                else { Msg = "Something went wrong "; }
            }
            var res = new ResMsg()
            {
                Status = status,
                Message = Msg,
                AgentPitchMasters = resultObj
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        #endregion



        //[HttpPost]
        //[AllowAnonymous]
        //[Route("UploadShopImage")]
        //public IHttpActionResult UploadShopImage()
        //{
        //    string LogoUrl = "";
        //    if (HttpContext.Current.Request.Files.AllKeys.Any())
        //    {
        //        var httpPostedFile = HttpContext.Current.Request.Files["file"];
        //        if (httpPostedFile != null)
        //        {
        //            string extension = Path.GetExtension(httpPostedFile.FileName);
        //            string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;
        //            //BackgroundTaskManager.Run(() =>
        //            //{
        //            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/images/AgentPitch/image")))
        //                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/images/AgentPitch/image"));


        //            LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/AgentPitch/image"), fileName);
        //            httpPostedFile.SaveAs(LogoUrl);
        //            // });
        //            LogoUrl = "images/AgentPitch/image/" + fileName;
        //            LogoUrl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
        //                                           , HttpContext.Current.Request.Url.DnsSafeHost
        //                                           , HttpContext.Current.Request.Url.Port
        //                                           , LogoUrl);
        //        }
        //    }
        //    return Created<string>(LogoUrl, LogoUrl);
        //}


        [HttpPost]
        [AllowAnonymous]
        [Route("UploadAudio")]
        public IHttpActionResult UploadAudio()
        {
            string LogoUrl = "";
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                if (httpPostedFile != null)
                {
                    if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/images/AgentPitch/audio")))
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/images/AgentPitch/audio"));

                    string extension = Path.GetExtension(httpPostedFile.FileName);
                    string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;
                    LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/AgentPitch/audio"), fileName);
                    httpPostedFile.SaveAs(LogoUrl);

                    var uploader = new List<Uploader> { new Uploader() };
                    uploader.FirstOrDefault().FileName = fileName;
                    uploader.FirstOrDefault().RelativePath = "~/images/AgentPitch/audio";


                    uploader.FirstOrDefault().SaveFileURL = LogoUrl;

                    uploader = AsyncContext.Run(() => FileUploadHelper.UploadFileToOtherApi(uploader));

                    //AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/images/AgentPitch/audio", LogoUrl);

                    LogoUrl = "images/AgentPitch/audio/" + fileName;
                   // LogoUrl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                   // , HttpContext.Current.Request.Url.DnsSafeHost
                                                    //, HttpContext.Current.Request.Url.Port
                                                   // , LogoUrl);
                }
            }
            return Created<string>(LogoUrl, LogoUrl);
        }



        [Route("GetAllAgentPitch")]
        [HttpPost]
        public HttpResponseMessage GetWarehousebasedV7(AgentPitchSearchDc pager)
        {

            using (AuthContext context = new AuthContext())
            {
                var param = new SqlParameter("AgentId", pager.AgentId);
                var param1 = new SqlParameter("StartDate", pager.StartDate);
                var param2 = new SqlParameter("EndDate", pager.EndDate);
                var result = new List<AgentPitchListDc>();
                result = context.Database.SqlQuery<AgentPitchListDc>("exec GetAgentPitchByIdDate @StartDate,@EndDate,@AgentId", param1, param2, param).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
        }

    }

    public class AgentPitchSearchDc
    {
        public int AgentId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class AgentPitchListDc
    {
        public long Id { get; set; }
        public string lat { get; set; }
        public string lg { get; set; }
        public string DisplayName { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string Name { get; set; }
        public string ShippingAddress { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ShopImageUrl { get; set; }
        public string AudioUrl { get; set; }
        public string Conversation { get; set; }
    }
    public class ResMsg
    {
        public string Message { get; set; }
        public bool Status { get; set; }
        public AgentPitchMasterDc AgentPitchMasters { get; set; }
    }

    public class AgentPitchMasterDc
    {
        public long Id { get; set; }
        public int CustomerId { get; set; }
        public int ExecutiveId { get; set; }
        public DateTime StartDate { get; set; }
        public string ShopImageUrl { get; set; }//imageurl
        public string lat { get; set; }
        public string lg { get; set; }
        public string AudioUrl { get; set; }  //MP3
        public string Conversation { get; set; }//text
        public DateTime EndDate { get; set; }
        public string Skcode { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }

    }
}