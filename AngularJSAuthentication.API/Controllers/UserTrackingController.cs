using NLog;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/trackuser")]
    public class UserTrackingController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
        [HttpGet]
        public object Get(int pid)
        {
            using (AuthContext dc = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 1, userid = 0;
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

                //User Traking System...
                try
                {
                    
                    var maxid = dc.UserTrakings.Where(t => t.PeopleId == userid.ToString()).Max(w => w.Id);
                    var ut = dc.UserTrakings.Where(t => t.PeopleId == userid.ToString() && t.Id == maxid).SingleOrDefault();
                    ut.Action = "View";
                    ut.Action_t = "View";
                    ut.Remark = "TrackingPage";
                    dc.UserTrakings.Add(ut);
                    dc.Commit();
                }
                catch (Exception ex)
                {

                }
                //END User Traking System...

                var data = (from c in dc.UserTrakings
                            join s in dc.Peoples on c.PeopleId equals s.PeopleID.ToString()
                            where c.PeopleId == pid.ToString()
                            select new
                            {
                                c.PeopleId,
                                s.PeopleFirstName,
                                c.Type,
                                c.Action,
                                c.LoginTime,
                                c.LogOutTime,
                                c.Remark
                            }).ToList();

                return (data);
            }
        }
        [Route("Searchtype")]
        [HttpGet]
        public object Searchtype(string type)
        {
            using (AuthContext dc = new AuthContext())
            {
                var data = (from c in dc.UserTrakings
                            join s in dc.Peoples on c.PeopleId equals s.PeopleID.ToString()
                            where c.Type == type
                            select new
                            {
                                c.PeopleId,
                                s.PeopleFirstName,
                                c.Type,
                                c.Action,
                                c.LoginTime,
                                c.LogOutTime,
                                c.Remark
                            }).ToList();

                return (data);
            }
        }
        [Route("searchpage")]
        [HttpGet]
        public object searchpage(string type)
        {
            //using (AuthContext dc = new AuthContext())
            //{
            //    var data = (from c in dc.UserTrakings
            //                join s in dc.Peoples on c.PeopleId equals s.PeopleID.ToString()
            //                where c.Action_t == type
            //                select new
            //                {
            //                    c.PeopleId,
            //                    s.PeopleFirstName,
            //                    c.Type,
            //                    c.Action,
            //                    c.LoginTime,
            //                    c.LogOutTime,
            //                    c.Remark
            //                }).ToList();

            //    return (data);

            //}
            return null;
        }


        [Route("SearchByDate")]
        [HttpGet]
        public object SearchByDate(string aaa, string bbb)
        {
            using (AuthContext dc = new AuthContext())
            {

                DateTime? dt = Convert.ToDateTime(aaa);
                DateTime? dtt = Convert.ToDateTime(bbb);

                var data = (from c in dc.UserTrakings
                            join s in dc.Peoples on c.PeopleId equals s.PeopleID.ToString()
                            where c.LoginTime >= dt && c.LoginTime <= dtt
                            select new
                            {
                                c.PeopleId,
                                s.PeopleFirstName,
                                c.Type,
                                c.Action,
                                c.LoginTime,
                                c.LogOutTime,
                                c.Remark
                            }).ToList();

                return data;
            }
        }

        [Route("")]
        [HttpPost]
        public HttpResponseMessage post(string action, string item)
        {
            string action_type = null;
            if (action.Contains("View")) { action_type = "View"; }
            else if (action.Contains("Delete")) { action_type = "Delete"; }
            else if (action.Contains("Edit")) { action_type = "Edit"; }
            else if (action.Contains("Add")) { action_type = "Add"; }

            using (AuthContext dc = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 1, userid = 0;
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

                //User Traking System...
                try
                {
                    
                    var maxid = dc.UserTrakings.Where(t => t.PeopleId == userid.ToString()).Max(w => w.Id);
                    var ut = dc.UserTrakings.Where(t => t.PeopleId == userid.ToString() && t.Id == maxid).SingleOrDefault();
                    ut.Remark = item;
                    ut.Action = action;
                    ut.Action_t = action_type;
                    dc.UserTrakings.Add(ut);
                    dc.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK, "Success");
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Failed");
                }

            }
        }
    }
}




