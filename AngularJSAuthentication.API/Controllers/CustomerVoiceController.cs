using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/CustomerVoice")]
    public class CustomerVoiceController : ApiController
    {
      
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [ResponseType(typeof(CustomerVoice))]
        [Route("")]
        [AcceptVerbs("POST")]
        public CustomerVoice Post(CustomerVoice CustomerVoice)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int warehouseid = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            warehouseid = int.Parse(claim.Value);
                        }
                    }
                    CustomerVoice dm = new CustomerVoice();
                    dm.cId = CustomerVoice.cId;
                    dm.skcode = CustomerVoice.skcode;
                    dm.filepath = CustomerVoice.filepath;
                    dm.Date = DateTime.Now;
                    dm.messagetype = "IN";
                    dm.message = null;
                    context.CustomerVoiceDB.Add(dm);
                    context.Commit();
                    return dm;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
        //string skcode
        //.Where(c => c.skcode == skcode)
        [Route("GET")]
        [HttpGet]
        public object Get()
        {
            logger.Info("start single User: ");
            using (AuthContext context = new AuthContext())
            {
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
                    var CVoice = context.CustomerVoiceDB.ToList().Select(p => p.skcode).Distinct();
                    return CVoice;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Get coupon by skCode " + ex.Message);
                    logger.Info("End  single coupon: ");
                    return null;
                }
            }
        }
        [Route("GET1")]
        [HttpGet]
        public List<CustomerVoice> Get1(string Skcode)
        {
            logger.Info("start single User: ");
            using (AuthContext context = new AuthContext())
            {
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
                    List<CustomerVoice> CVoice = context.CustomerVoiceDB.Where(c => c.skcode == Skcode).ToList();
                    return CVoice;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Get coupon by skCode " + ex.Message);
                    logger.Info("End  single coupon: ");
                    return null;
                }
            }
        }

        [HttpPost]
        [Route("voicereply")]
        public CustomerVoice Post1(CustomerVoice voicereply)
        {
            using (AuthContext context = new AuthContext())
            {
                CustomerVoice dm = new CustomerVoice();
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
                    //
                    var CV = context.CustomerVoiceDB.Where(c => c.skcode == voicereply.skcode).FirstOrDefault(); ;
                    if (CV != null)
                    {

                        dm.skcode = CV.skcode;
                        dm.filepath = null;
                        dm.Date = DateTime.Now;
                        dm.messagetype = "Out";
                        dm.message = voicereply.message;
                        context.CustomerVoiceDB.Add(dm);
                        context.Commit();
                    }
                    return dm;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
        //[HttpPost]
        //[Route("voicereply")]
        //public CustomerVoiceReply Post(CustomerVoiceReply voicereply)
        //{
        //    CustomerVoiceReply dm = new CustomerVoiceReply();
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        foreach (Claim claim in identity.Claims)
        //        {
        //            if (claim.Type == "compid")
        //            {
        //                compid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "userid")
        //            {
        //                userid = int.Parse(claim.Value);
        //            }
        //        }
        //        var CV = context.CustomerVoiceDB.Where(c => c.skcode == voicereply.skcode).SingleOrDefault();
        //        if (CV != null)
        //        {

        //            dm.skcode = CV.skcode;
        //            dm.voicereply = voicereply.voicereply;
        //            dm.cId = CV.cId;
        //            dm.Date = DateTime.Now;
        //            context.CustomerVoiceReplyDB.Add(dm);
        //            context.SaveChanges();
        //        }
        //        return dm;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}
        //[Route("")]
        //[HttpGet]
        //public CustomerVoice Get(string skcode)
        //{
        //    logger.Info("start single User: ");
        //    CustomerVoice CVoice = new CustomerVoice();
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;

        //        foreach (Claim claim in identity.Claims)
        //        {
        //            if (claim.Type == "compid")
        //            {
        //                compid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "userid")
        //            {
        //                userid = int.Parse(claim.Value);
        //            }
        //        }
        //        CVoice = context.GetVoiceByskcode(skcode);
        //        logger.Info("End Get coupon by skcode: " + CVoice.skcode);
        //        return CVoice;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in Get coupon by skCode " + ex.Message);
        //        logger.Info("End  single coupon: ");
        //        return null;
        //    }
        //}
        [Route("voicereply")]
        [HttpGet]
        public List<CustomerVoiceReply> voicereply(string skcode)
        {
            using (AuthContext context = new AuthContext())
            {
               
                logger.Info("start single User: ");
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
                    List<CustomerVoiceReply> CVoice = context.CustomerVoiceReplyDB.Where(c => c.skcode == skcode).ToList();

                    return CVoice;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Get coupon by skCode " + ex.Message);
                    logger.Info("End  single coupon: ");
                    return null;
                }
            }
        }
    }
}
