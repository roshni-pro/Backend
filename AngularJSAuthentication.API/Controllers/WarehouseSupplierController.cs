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
    [RoutePrefix("api/WarehouseSupplier")]
    public class WarehouseSupplierController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Authorize]
        [Route("")]
        public IEnumerable<WarehouseSupplier> Get()
        {
            logger.Info("start WarehouseSupplier: ");
            using (AuthContext context = new AuthContext())
            {
                List<WarehouseSupplier> ass = new List<WarehouseSupplier>();
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = context.AllWarehouseSupplier(compid).ToList();
                    logger.Info("End  Warehouse: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Warehouse " + ex.Message);
                    logger.Info("End  Warehouse: ");
                    return null;
                }
            }
        }
        [ResponseType(typeof(Warehouse))]
        [Route("")]
        [AcceptVerbs("POST")]
        public WarehouseSupplier add(WarehouseSupplier item)
        {
            logger.Info("start addWarehouseSupplier: ");
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
                    item.CompanyId = compid;
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.AddWarehouseSupplier(item);
                    logger.Info("End  Warehouse: ");
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addQuesAns " + ex.Message);
                    logger.Info("End  addWarehouseSupplier: ");
                    return null;
                }
            }
        }
        [ResponseType(typeof(WarehouseSupplier))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public WarehouseSupplier Put(WarehouseSupplier item)
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
                    item.CompanyId = compid;

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    return context.PutWarehouseSupplier(item);
                }
                catch
                {
                    return null;
                }
            }
        }


        [ResponseType(typeof(WarehouseSupplier))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("start WarehouseSupplier: ");
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.DeleteWarehouseSupplier(id, compid);
                    logger.Info("End  delete Warehouse: ");
                }
                catch (Exception ex)
                {

                    logger.Error("Error in deleteWarehouseSupplier " + ex.Message);

                }
            }
        }




        //==========================================================================================//
        //  [Authorize]
        //[Route("")]
        //public IEnumerable<State> Get(string recordtype)
        //{
        //    if (recordtype=="states")
        //    {
        //        logger.Info("start Warehouse: ");
        //        List<Warehouse> ass = new List<Warehouse>();
        //        List<State> st = new List<State>();
        //        try
        //        {
        //            var identity = User.Identity as ClaimsIdentity;
        //            int compid = 0, userid = 0;

        //            foreach (Claim claim in identity.Claims)
        //            {
        //                if (claim.Type == "compid")
        //                {
        //                    compid = int.Parse(claim.Value);
        //                }
        //                if (claim.Type == "userid")
        //                {
        //                    userid = int.Parse(claim.Value);
        //                }
        //            }

        //            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //            ass = context.AllWarehouse(compid).ToList();

        //            var distinctrecords = (from r in ass
        //                                   orderby r.Stateid
        //                                   select new {r.Stateid,r.StateName }).Distinct();
        //            foreach (var item in distinctrecords)
        //            {
        //                State s = new State();
        //                s.Stateid = item.Stateid;
        //                s.StateName = item.StateName;
        //                st.Add(s);
        //            }



        //            logger.Info("End  Warehouse: ");
        //            return st;
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error in Warehouse " + ex.Message);
        //            logger.Info("End  Warehouse: ");
        //            return null;
        //        }
        //    }
        //    return null;
        //}


        //[Authorize]
        //[Route("")]
        //public IEnumerable<string> Get(string recordtype,string state)
        //{
        //    if (recordtype == "city")
        //    {
        //        logger.Info("start Warehouse: ");
        //        List<Warehouse> ass = new List<Warehouse>();
        //        try
        //        {
        //            var identity = User.Identity as ClaimsIdentity;
        //            int compid = 0, userid = 0;

        //            foreach (Claim claim in identity.Claims)
        //            {
        //                if (claim.Type == "compid")
        //                {
        //                    compid = int.Parse(claim.Value);
        //                }
        //                if (claim.Type == "userid")
        //                {
        //                    userid = int.Parse(claim.Value);
        //                }
        //            }

        //            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //            ass = context.AllWarehouse(compid).ToList();

        //            var distinctrecords = (from r in ass
        //                                   orderby r.Cityid where r.StateName.Equals(state)
        //                                   select r.CityName).Distinct();

        //            logger.Info("End  Warehouse: ");
        //            return distinctrecords;
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error in Warehouse " + ex.Message);
        //            logger.Info("End  Warehouse: ");
        //            return null;
        //        }
        //    }
        //    return null;
        //}


    }
}



