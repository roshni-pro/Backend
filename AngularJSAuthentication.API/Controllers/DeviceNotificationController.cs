using AngularJSAuthentication.API.Controllers.Base;
using GenricEcommers.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/DeviceNotificationApi")]
    public class DeviceNotificationController : BaseAuthController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
        public IEnumerable<DeviceNotification> Get()
        {
            logger.Info("start DeviceNotification: ");
            List<DeviceNotification> CategoriesList = new List<DeviceNotification>();
            using (AuthContext context = new AuthContext())
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

                    CategoriesList = context.GetAllDeviceNotification(compid).ToList();
                    logger.Info("End  DeviceNotification: ");
                    return CategoriesList;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in DeviceNotification " + ex.Message);
                    logger.Info("End  DeviceNotification: ");
                    return null;
                }
            }

        }

        [Route("")]
        public bool Get(string RegId, string imei)
        {
            logger.Info("start DeviceNotification: ");
            using (AuthContext context = new AuthContext())
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
                    var CategoriesList = context.GetAllDeviceNotification(RegId, imei, compid);
                    logger.Info("End  DeviceNotification: ");

                    return CategoriesList;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in DeviceNotification " + ex.Message);
                    logger.Info("End  DeviceNotification: ");
                    return false;
                }
            }
        }

        [Route("")]
        public DeviceNotification Get(int id)
        {
            logger.Info("start single DeviceNotification: ");
            DeviceNotification deviceNotification = new DeviceNotification();
            using (AuthContext context = new AuthContext())
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
                    logger.Info("in deviceNotification");

                    deviceNotification = context.GetByDeviceNotificationId(id, compid);
                    logger.Info("End Get DeviceNotification id: " + deviceNotification.DeviceId);
                    return deviceNotification;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Get deviceNotification by deviceNotification id " + ex.Message);
                    logger.Info("End  single deviceNotification: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(DeviceNotification))]
        [Route("")]
        [AcceptVerbs("POST")]
        public DeviceNotification add(DeviceNotification deviceNotification)
        {
            logger.Info("Add DeviceNotification: ");
            using (AuthContext context = new AuthContext())
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
                    deviceNotification.CompanyId = compid;
                    if (deviceNotification == null)
                    {
                        throw new ArgumentNullException("deviceNotification");
                    }
                    context.AddDeviceNotification(deviceNotification);
                    logger.Info("End  Add deviceNotification: ");
                    return deviceNotification;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add deviceNotification " + ex.Message);

                    return null;
                }
            }
        }


        [Route("")]
        [AcceptVerbs("PUT")]
        public DeviceNotification Put(DeviceNotification deviceNotification)
        {
            using (AuthContext context = new AuthContext())
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
                    deviceNotification.CompanyId = compid;
                    return context.PutDeviceNotification(deviceNotification);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Put deviceNotification " + ex.Message);
                    return null;
                }
            }
        }



        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("DELETE Remove: ");
            using (AuthContext context = new AuthContext())
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
                    context.DeleteDeviceNotification(id, compid);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Remove DeviceNotification " + ex.Message);

                }
            }
        }


    }
}



