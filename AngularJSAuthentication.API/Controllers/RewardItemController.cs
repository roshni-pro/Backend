using AngularJSAuthentication.API.Controllers.Base;
using GenricEcommers.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/RewardItem")]
    public class RewardItemController : BaseAuthController
    {
       
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        [Authorize]
        [Route("GetAll")]
        public IEnumerable<RewardItems> Get()
        {
            logger.Info("start News: ");
            List<RewardItems> List = new List<RewardItems>();
            try
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 1, userid = 0;
                int Warehouse_id = 0;

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
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                using (var context = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {

                        List = context.RewardItemsDb.Where(r => r.IsDeleted == false && r.WarehouseId == Warehouse_id).OrderBy(o => o.rPoint).ToList();
                        logger.Info("End  News: ");
                        return List;
                    }
                    else
                    {
                        List = context.RewardItemsDb.Where(r => r.IsDeleted == false && r.CompanyId == compid).OrderBy(o => o.rPoint).ToList();
                        logger.Info("End  News: ");
                        return List;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in News " + ex.Message);
                logger.Info("End  News: ");
                return null;
            }
        }

        [Authorize]
        [Route("GetRewardItem")]
        public IEnumerable<RewardItems> GetRewardItem()
        {
            logger.Info("start News: ");
            List<RewardItems> List = new List<RewardItems>();
            try
            {
                using (var context = new AuthContext())
                {
                    //if (WarehouseId > 0)
                    //{

                    //    List = context.RewardItemsDb.Where(r => r.IsActive == true && r.IsDeleted == false && r.WarehouseId == WarehouseId).OrderBy(o => o.rPoint).ToList();
                    //    logger.Info("End  News: ");
                    //    return List;
                    //}
                    //else
                    //{
                    List = context.RewardItemsDb.Where(r => r.IsActive == true && r.IsDeleted == false).OrderBy(o => o.rPoint).ToList();
                    logger.Info("End  News: ");
                    return List;
                    //}
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in News " + ex.Message);
                logger.Info("End  News: ");
                return null;
            }
        }

        [Route("")]
        public IEnumerable<RewardItems> GetMobile()
        {
            logger.Info("start News: ");
            List<RewardItems> List = new List<RewardItems>();
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 1, userid = 0;
                int Warehouse_id = 0;

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
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                using (var context = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {
                        List = context.RewardItemsDb.Where(r => r.IsActive == true && r.IsDeleted == false && r.WarehouseId == Warehouse_id).OrderByDescending(o => o.rPoint).ToList();
                        logger.Info("End  News: ");
                        return List;
                    }
                    else
                    {
                        List = context.RewardItemsDb.Where(r => r.IsActive == true && r.IsDeleted == false && r.CompanyId == compid).OrderByDescending(o => o.rPoint).ToList();
                        logger.Info("End  News: ");
                        return List;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in News " + ex.Message);
                logger.Info("End  News: ");
                return null;
            }
        }
        [Route("")]
        public RewardItems Get(int id)
        {
            logger.Info("start single News: ");
            RewardItems News = new RewardItems();
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

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
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                logger.Info("in News");
                using (var context = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {
                        News = context.RewardItemsDb.Where(x => x.rItemId == id && x.IsDeleted == false && x.WarehouseId == Warehouse_id).SingleOrDefault();
                        logger.Info("End Get News by item id: ");
                        return News;
                    }
                    else
                    {


                        return null;
                    }
                }


            }
            catch (Exception ex)
            {
                logger.Error("Error in Get News by item id " + ex.Message);
                logger.Info("End  single News: ");
                return null;
            }
        }
        [Route("")]
        [AcceptVerbs("POST")]
        public RewardItems add(RewardItems news)
        {
            logger.Info("Add News: ");
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

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
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }

                //if (Warehouse_id > 0)
                //{
                using (var context = new AuthContext())
                {
                    news.CompanyId = compid;
                    news.WarehouseId = Warehouse_id;
                    news.CreateDate = indianTime;
                    news.IsActive = true;
                    news.IsDeleted = false;
                    context.RewardItemsDb.Add(news);
                    context.Commit();
                    return news;
                }
                //}
                //else {

                //    return null;

                //}

            }
            catch (Exception ex)
            {
                logger.Error("Error in Add News " + ex.Message);
                return null;
            }
        }
        [Route("")]
        [AcceptVerbs("PUT")]
        public RewardItems Put(RewardItems News)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

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
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }

                //if (Warehouse_id > 0)
                //{
                using (var context = new AuthContext())
                {
                    RewardItems comp = context.RewardItemsDb.Where(x => x.rItemId == News.rItemId).FirstOrDefault();
                    if (comp != null)
                    {
                        comp.rName = News.rName;
                        comp.rPoint = News.rPoint;
                        comp.rItem = News.rItem;
                        comp.Description = News.Description;
                        comp.ImageUrl = News.ImageUrl;
                        comp.IsDeleted = News.IsDeleted;
                        comp.IsActive = News.IsActive;
                        context.RewardItemsDb.Attach(comp);
                        context.Entry(comp).State = EntityState.Modified;
                        context.Commit();
                    }
                    return comp;
                }
                //}
                //else {
                //    return null;
                //}


            }
            catch (Exception ex)
            {
                logger.Error("Error in Put News " + ex.Message);
                return null;
            }
        }
        [Route("")]
        [AcceptVerbs("Delete")]
        public string Remove(int id)
        {
            logger.Info("DELETE Remove: ");
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

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
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                using (var context = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {

                        RewardItems comp = context.RewardItemsDb.Where(x => x.rItemId == id && x.WarehouseId == Warehouse_id).SingleOrDefault();
                        if (comp != null)
                        {
                            context.RewardItemsDb.Remove(comp);
                            context.Commit();
                            return "success";
                        }
                        else
                        {
                            return "Record doen't exist";
                        }
                    }
                    else
                    {

                        return null;
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error("Error in Remove News " + ex.Message);
                return "error";
            }
        }
        #region
        /// <summary>
        /// Delete reward item
        /// </summary>
        /// <param name="id"></param>
        [Authorize]
        [Route("Deleteitem")]
        [AcceptVerbs("Delete")]
        public void Removeitem(int id)
        {
            logger.Info("start delete RewardItems: ");
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
                using (var context = new AuthContext())
                {
                    context.Deleteredeemitem(id);
                }
                logger.Info("End  delete RewardItems: ");
            }
            catch (Exception ex)
            {
                logger.Error("Error in delete RewardItems " + ex.Message);
            }
        }
        #endregion

    }
}