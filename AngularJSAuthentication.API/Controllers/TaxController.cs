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
    [RoutePrefix("api/TaxMaster")]
    public class TaxController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Authorize]
        [Route("")]
        public IEnumerable<TaxMaster> Get()
        {
            logger.Info("start Tax: ");
            List<TaxMaster> ass = new List<TaxMaster>();
            try
            {
                using (AuthContext context = new AuthContext())
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
                    ass = context.AllTaxMaster(compid).ToList();
                    logger.Info("End  TaxMaster: ");
                    return ass;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in TaxMaster " + ex.Message);
                logger.Info("End  TaxMaster: ");
                return null;
            }
        }


        [ResponseType(typeof(TaxMaster))]
        [Route("")]
        [AcceptVerbs("POST")]
        public TaxMaster add(TaxMaster item)
        {
            logger.Info("start addTaxMaster: ");
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
                    item.CompanyId = compid;
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.AddTaxMaster(item);
                    logger.Info("End  Add : ");
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in AddTaxMaster " + ex.Message);
                    logger.Info("End  AddTaxMaster: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(TaxMaster))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public TaxMaster Put(TaxMaster item)
        {
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
                    item.userid = userid;
                    item.CompanyId = compid;
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    return context.PutTaxMaster(item);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }


        [ResponseType(typeof(TaxMaster))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("start delete TaxMaster: ");
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

                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.DeleteTaxMaster(id, CompanyId);
                    logger.Info("End  delete TaxMaster: ");
                }
                catch (Exception ex)
                {

                    logger.Error("Error in delete TaxMaster " + ex.Message);


                }
            }
        }

        [ResponseType(typeof(TaxMaster))]
        [Route("DeleteV7")]
        [AcceptVerbs("Delete")]
        public Boolean DeleteV7(int id)
        {
            logger.Info("start delete TaxMaster: ");
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

                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    return context.DeleteTaxMaster(id, CompanyId);
                    logger.Info("End  delete TaxMaster: ");
                }
                catch (Exception ex)
                {

                    logger.Error("Error in delete TaxMaster " + ex.Message);
                    return false;

                }
            }
        }
        //#region get ItemMaster history
        //[Route("ItemMasterhistory")]
        //[HttpGet]
        //public dynamic ItemMasterhistory(string TaxName)
        //{
      
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0, Warehouse_id = 0;
        //        // Access claims
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
        //            if (claim.Type == "Warehouseid")
        //            {
        //                Warehouse_id = int.Parse(claim.Value);
        //            }
        //        }
        //        int CompanyId = compid;
        //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

        //        var data = odd.TaxMasterHistoriesDB.Where(x => x.TaxName == TaxName).ToList().OrderByDescending(x => x.CreatedDate);
        //        return data;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}
        //#endregion
    }
}



