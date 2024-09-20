using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/GroupSMS")]
    public class GroupSMSController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("all")]
        public IEnumerable<GroupSMS> Get()
        {

            var userid = GetUserId();

            logger.Info("start Group: ");
            List<GroupSMS> ass = new List<GroupSMS>();
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    ass = context.GroupsSms.Where(x => x.Deleted == false && x.peoples.PeopleID == userid).ToList();
                    logger.Info("End Group: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Group " + ex.Message);
                    logger.Info("End Group: ");
                    return null;
                }
            }
        }

        [Route("all")]
        public  IEnumerable<GroupSMSdc> Get(string GroupAssociation)
        {
            
            var userid = GetUserId();

            logger.Info("start Group: ");
          //  List<GroupSMS> ass = new List<GroupSMS>();
            List<GroupSMSdc> res = new List<GroupSMSdc>();
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    //ass = context.GroupsSms.Where(x => x.Deleted == false && x.peoples.PeopleID == userid && x.GroupAssociation == GroupAssociation).ToList();
                    //logger.Info("End Group: ");
                    //return ass;

                    // var uid = new SqlParameter("@userid", userid);
                    var GroupAss = new SqlParameter("@GroupAss", GroupAssociation);
                    res = context.Database.SqlQuery<GroupSMSdc>("Exec GroupNotification @GroupAss", GroupAss).ToList();
                    logger.Info("End Group: ");
                    return res;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Group " + ex.Message);
                    logger.Info("End Group: ");
                    return null;
                }
            }
        }

        [Route("add")]
        [AcceptVerbs("POST")]
        public GroupSMS add(GroupSMS group)
        {
            var userid = GetUserId();

            logger.Info("start Group: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    if (group == null)
                    {
                        throw new ArgumentNullException("state");
                    }
                    group.peoples = context.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();
                    context.AddGroup(group);
                    logger.Info("End Group: ");
                    return group;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Group" + ex.Message);
                    logger.Info("End  AddGroup: ");
                    return null;
                }
            }
        }

        [Route("put")]
        [AcceptVerbs("PUT")]
        public GroupSMS Put(GroupSMS item)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    if (item != null)
                    {
                        return context.PutGroup(item);
                    }
                    else
                    {
                        throw new ArgumentNullException("state");
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        [Route("delete")]
        public bool delete(int id)
        {
            logger.Info("Start Delete Group: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    context.DeleteGroup(id);
                    logger.Info("End Delete Group: ");
                    return true;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Group " + ex.Message);
                    logger.Info("End  Group: ");
                    return false;
                }
            }
        }

        [Route("Duplicacy")]
        [HttpGet]
        public HttpResponseMessage CheckMobile(string GroupName, string GroupAssociation)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var RDMobile = context.GroupsSms.Where(x => x.GroupName == GroupName && x.GroupAssociation == GroupAssociation && x.Deleted == false).FirstOrDefault();

                    return Request.CreateResponse(HttpStatusCode.OK, RDMobile);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, false);
                }
            }
        }


        public int GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            //Access claims
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
            return userid;
        }
       public class GroupSMSdc
        {

            public string GroupName { get; set; }
            public long GroupID { get; set; }
        }



    }
}
