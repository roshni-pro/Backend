using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/OrderDispatchedDetailsFinal")]
    public class OrderDispatchedDetailsFinalController : ApiController
    {
      
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        


        [Authorize]
        [Route("")]
        public IEnumerable<FinalOrderDispatchedDetails> GetallFinaldispatchDetailbyId(string id)
        {
            logger.Info("start : ");
            List<FinalOrderDispatchedDetails> ass = new List<FinalOrderDispatchedDetails>();
            using (var context = new AuthContext())
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
                    int idd = Int32.Parse(id);
                    ass = context.AllFOrderDispatchedDetails(idd, compid).ToList();
                    logger.Info("End  : ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in returnorderby id " + ex.Message);
                    logger.Info("End  returnorderby id: ");
                    return null;
                }
            }
        }


        public List<filtered> arrange(List<FinalOrderDispatchedDetails> lst)
        {
            using (var context = new AuthContext())
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

                    List<filtered> fl = new List<filtered>();
                    foreach (FinalOrderDispatchedDetails o in lst)
                    {
                        bool found = false;
                        foreach (filtered f in fl)
                        {
                            if (f.OrderDate == o.OrderDate)
                            {
                                found = true;
                                f.lst.Add(o);
                            }
                        }
                        if (found == false)
                        {
                            filtered obj = new filtered();
                            obj.OrderDate = o.OrderDate;
                            obj.lst.Add(o);
                            fl.Add(obj);

                        }
                    }
                    return fl;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in returnorderby id " + ex.Message);
                    logger.Info("End  returnorderby id: ");
                    return null;
                }
            }
        }




        [Route("GetReport")]

        public IEnumerable<filtered> Get(DateTime datefrom, DateTime dateto)
        {
            logger.Info("start : ");
            List<filtered> ass = new List<filtered>();
            using (var context = new AuthContext())
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
                    ass = context.AllFOrderDispatchedReportDetails(datefrom, dateto, compid).ToList();
                    logger.Info("End  : ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in returnorderby id " + ex.Message);
                    logger.Info("End  returnorderby id: ");
                    return null;
                }
            }
        }

        //for report class

        public class filtered
        {
            public DateTime OrderDate { get; set; }


            public List<FinalOrderDispatchedDetails> lst = new List<FinalOrderDispatchedDetails>();
            public double pricetotal;
            public double Taxtotal;
            public double priceTotaltotal;
            public double TaxAftertotal;

        }
        //

        [ResponseType(typeof(FinalOrderDispatchedDetails))]
        [Route("")]
        [AcceptVerbs("POST")]
        public List<FinalOrderDispatchedDetails> add(List<FinalOrderDispatchedDetails> po)

        {
            using (var db = new AuthContext())
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

                    foreach (FinalOrderDispatchedDetails x1 in po)
                    {


                        x1.Status = "sattled";
                        db.FinalOrderDispatchedDetailsDb.Add(x1);
                        int id = db.Commit();

                    }


                    return po;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in returnorderby id " + ex.Message);
                    logger.Info("End  returnorderby id: ");
                    return null;
                }

            }
        }

        [ResponseType(typeof(FinalOrderDispatchedDetails))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public List<FinalOrderDispatchedDetails> add(Int32 oID, Int32 fID)
        {
            using (var db = new AuthContext())
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

                    var listDetail = db.FinalOrderDispatchedDetailsDb.Where(x => x.OrderId == oID && x.CompanyId == compid).ToList();
                    foreach (FinalOrderDispatchedDetails x1 in listDetail)
                    {
                        x1.FinalOrderDispatchedMasterId = fID;
                        x1.UpdatedDate = indianTime;
                        //db.FinalOrderDispatchedDetailsDb.Attach(x1);
                        db.Entry(x1).State = EntityState.Modified;
                        db.Commit();

                    }
                    return listDetail;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in returnorderby id " + ex.Message);
                    logger.Info("End  returnorderby id: ");
                    return null;
                }

            }

        }
    }
}