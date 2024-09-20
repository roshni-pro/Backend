using AngularJSAuthentication.Model;
using GenricEcommers.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/NotificationByDeviceId")]
    public class NotificationByDeviceIdController : ApiController
    {
       
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //[Authorize]
        [Route("")]
        public PaggingNotiByDevice Get(int list, int page)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start Message: ");
                PaggingNotiByDevice MessageList = new PaggingNotiByDevice();
                // List<DeviceNotification> MessageList = new List<DeviceNotification>();
                try
                {
                    logger.Info("start News: ");
                    List<News> List = new List<News>();

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


                    MessageList = context.GetAllNotification(list, page);
                    logger.Info("End  Message: ");
                    return MessageList;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Message " + ex.Message);
                    logger.Info("End  Message: ");
                    return null;
                }
            }
        }

        [Route("")]
        public IEnumerable<DeviceNotification> Get(int CustomerId)
        {
            logger.Info("start Message: ");
            List<DeviceNotification> MessageList = new List<DeviceNotification>();
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
                logger.Info("End  Message: ");
                return MessageList;
            }
            catch (Exception ex)
            {
                logger.Error("Error in Message " + ex.Message);
                logger.Info("End  Message: ");
                return null;
            }
        }
    }
}



