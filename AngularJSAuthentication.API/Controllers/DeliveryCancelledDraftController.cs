using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.API.Results;
using AngularJSAuthentication.Model;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using NLog;
using AngularJSAuthentication.Infrastructure;
using AngularJSAuthentication.Controllers;
using System.Data.Entity;
using AspNetIdentity.WebApi.Infrastructure;
using AngularJSAuthentication.DataContracts.Transaction.RequestAccess;
using AngularJSAuthentication.API.DataContract;
using AngularJSAuthentication.Model.Permission;
using System.Text.RegularExpressions;
using System.IO;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/DeliveryCancelledDraft")]
    public class DeliveryCancelledDraftController : BaseApiController
    {


        [Route("GetImage")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage Get(int OrderId, int userID)//get all 
        {
           
            using (AuthContext context = new AuthContext())
            {
               var dell = context.OrderDispatchedMasters.Where(x => x.OrderId == OrderId).SingleOrDefault();
                if (dell.Status == "Delivery Canceled" || dell.Status == "Post Order Canceled")
                {
                    var conn = context.DeliveryIssuanceDb.Where(s => s.DeliveryIssuanceId == dell.DeliveryIssuanceId && s.Status == "Freezed").FirstOrDefault();
                    
                    return Request.CreateResponse(HttpStatusCode.OK, conn);
                }

                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "add only Freezed assignment");
                }
               



            }
        }







        [Route("PostImage")]
        [HttpPost]
        [Authorize]
        public HttpResponseMessage postImage()
        {
            string ImageUrl = string.Empty;
            string filename = string.Empty;


            using (AuthContext context = new AuthContext())
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                   
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        
                        var ImageUrll = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedImages"), httpPostedFile.FileName);
                      

                        httpPostedFile.SaveAs(ImageUrll);
                    }
                }
              }
            
            return Request.CreateResponse(System.Net.HttpStatusCode.OK, false);
        }


    }
       




      
}
