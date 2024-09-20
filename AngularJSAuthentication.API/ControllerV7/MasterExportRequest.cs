using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using System.Security.Claims;
using System.Web.Http;
using NLog;
using System.Collections.Concurrent;
using System.Data.Entity;
using System.Configuration;
using System.Net.Mail;
using LinqKit;
using AngularJSAuthentication.API.Models;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/MasterExportRequest")]
    public class MasterExportRequest : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("GetMasterRExportRequest")]
        [HttpGet]
        public HttpResponseMessage GetMasterRExportRequest(string type)
        {
            using (var db = new AuthContext())
            {
              
                var MasterExportList = db.MasterExportRequestDB.Where(x => x.IsDeleted == false).ToList();
                if (MasterExportList.Count > 0)
                { return Request.CreateResponse(HttpStatusCode.OK, MasterExportList); }
                else { return Request.CreateResponse(HttpStatusCode.OK, MasterExportList); }
            }

        }

        [Route("GetMasterRExportApproval")]
        [HttpGet]
        public HttpResponseMessage GetMasterRExportApproval(int ApproverId)
        {
            using (var db = new AuthContext())
            {
                var MasterExportList = db.MasterExportRequestDB.Where(x => x.ApproverId == ApproverId && x.IsGenerated == false && x.IsDeleted==false).ToList();
                if (MasterExportList.Count > 0)
                { return Request.CreateResponse(HttpStatusCode.OK, MasterExportList); }
                else { return Request.CreateResponse(HttpStatusCode.OK, MasterExportList); }
            }

        }


        [Route("GetAll")]
        [HttpGet]
        public HttpResponseMessage GetAll()
        {
            using (var db = new AuthContext())
            {
                var MasterExportList = db.MasterExportRequestDB.Where(x => x.IsGenerated == false).ToList();
                if (MasterExportList.Count > 0)
                { return Request.CreateResponse(HttpStatusCode.OK, MasterExportList); }
                else { return Request.CreateResponse(HttpStatusCode.OK, MasterExportList); }
            }

        }

    }
}