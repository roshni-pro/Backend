using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    public class Pie
    {
        public string label { get; set; }
        public double value { get; set; }
        public double data { get; set; }

    }
    public class PieChartController : ApiController
    {
       
        public static Logger logger = LogManager.GetCurrentClassLogger();
        [Authorize]
        [HttpGet]
        [Route("api/PieChart")]
        [System.Web.Http.AcceptVerbs("GET")]
        public IEnumerable<Pie> Get(string startdate, string enddate, int userid)
        {
            logger.Info("Start Piechart");
            using (var context = new AuthContext())
            {
                try
                {
                    DateTime startDate = DateTime.ParseExact(startdate, "dd/MM/yyyy", null);  //DateTime.Parse(startdate);
                    DateTime endDate = DateTime.ParseExact(enddate, "dd/MM/yyyy", null);
                    int days = startDate.DayOfWeek - DayOfWeek.Sunday;

                    //DateTime weekStart = startDate.AddDays(-days);
                    //DateTime weekEnd = weekStart.AddDays(6);
                    List<Pie> resources = new List<Pie>();

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0;

                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        //if (claim.Type == "userid")
                        //{
                        //    userid = int.Parse(claim.Value);
                        //}


                    }

                    List<Event> events = null;
                    if (userid == 0)
                    {
                        events = context.FilteredEvents(startDate, endDate, compid).ToList();
                    }
                    else
                    {
                        events = context.FilteredEvents(startDate, endDate, userid, compid).ToList();
                    }
                    double total = 0;
                    foreach (Event e in events)
                    {
                        TaskType tt = context.GetTaskTypeById(e.TaskType);
                        Pie r = resources.Where(t => t.label.Equals(tt.Name)).SingleOrDefault();
                        // ResourceReport r = resources.Where(t => t.customerId == client.CustomerId).SingleOrDefault();

                        if (r == null)
                        {
                            r = new Pie();
                            //r.personId = e.PeopleID;


                            //// r.customerId = client.CustomerId;
                            //r.projectId = e.ProjectId;
                            // People p = context.AllPeoples(compid).Where(c => c.PeopleID == e.PeopleID).SingleOrDefault();
                            r.label = tt.Name;
                            //  r.Name = client.Name;
                            //   r.Rate = p.BillableRate;
                            r.value = e.Hours;
                            total += e.Hours;
                            resources.Add(r);
                        }
                        else
                        {

                            r.value += e.Hours;

                        }

                    }
                    foreach (Pie p in resources)
                    {
                        p.data = p.value / total * 100;

                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    logger.Info("End  Piechart: ");
                    return resources;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Piechart " + ex.Message);

                    return null;
                }
            }
        }
    }
}
