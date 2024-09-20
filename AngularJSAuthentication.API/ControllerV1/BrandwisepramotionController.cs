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
    [RoutePrefix("api/Brandwisepramotion")]
    public class BrandwisepramotionController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
        public List<ItemMaster> Get()
        {

            logger.Info("start subcategorybyWarehouse: ");
            List<ItemMaster> ass = new List<ItemMaster>();
            using (var c = new AuthContext())
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

                    if (Warehouse_id > 0)
                    {
                        ass = c.itemMasters.Where(i => i.IsPramotionalItem == true && i.Deleted == false && i.active == true && i.WarehouseId == Warehouse_id).ToList();
                    }
                    else if (Warehouse_id == 0)
                    {
                        ass = c.itemMasters.Where(i => i.IsPramotionalItem == true && i.Deleted == false && i.active == true).ToList();
                    }
                    else
                    {
                        return null;
                    }

                    logger.Info("End  subcategorybyWarehouse: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in subcategorybyWarehouse " + ex.Message);
                    logger.Info("End  subcategorybyWarehouse: ");
                    return null;
                }
            }
        }

        // removed by Harry : 21 May 2019 
        [Route("GetBrandWiseData")]
        public List<ItemMaster> GetBrandWisedata(int warehouseid)
        {

            logger.Info("start subcategorybyWarehouse: ");
            List<ItemMaster> ass = new List<ItemMaster>();
            using (var c = new AuthContext())
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
                    if (warehouseid > 0)
                    {
                        ass = c.itemMasters.Where(i => i.IsPramotionalItem == true && i.Deleted == false && i.active == true && i.WarehouseId == warehouseid).ToList();
                    }
                    else
                    {
                        return null;
                    }
                    logger.Info("End  subcategorybyWarehouse: ");
                    //return ass;
                    return null;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in subcategorybyWarehouse " + ex.Message);
                    logger.Info("End  subcategorybyWarehouse: ");
                    return null;
                }
            }
        }


        [Authorize]
        [Route("")]
        public List<ItemMaster> GetItemByBrand(string recordtype, int SubSubCategoryId, int WarehouseId)
        {
            if (recordtype == "warehouse")
            {
                logger.Info("start Category: ");
                List<ItemMaster> ass = new List<ItemMaster>();
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
                        ass = context.ItembyBrand(SubSubCategoryId, WarehouseId).ToList();
                        logger.Info("End  WarehouseCategory: ");
                        return ass;
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error in WarehouseCategory " + ex.Message);
                        logger.Info("End  WarehouseCategory: ");

                    }
                }
            }
            return null;
        }

        [Authorize]
        [Route("")]
        public List<SubsubCategory> GetSubSubCategory(string recordtype, int warehouseid)
        {

            logger.Info("start Category: ");
            List<SubsubCategory> ass = new List<SubsubCategory>();
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
                    ass = context.subsubcategorybyWarehouse(warehouseid, compid).ToList();
                    logger.Info("End  WarehouseCategory: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in WarehouseCategory " + ex.Message);
                    logger.Info("End  WarehouseCategory: ");

                }
            }
            return null;
        }


        [ResponseType(typeof(ItemMaster))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public List<ItemMaster> Put(List<ItemMaster> item)
        {
            using (var context = new AuthContext())
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
                    return context.UpdateItemMaster(item, compid);
                }
                catch
                {
                    return null;
                }
            }
        }


        [ResponseType(typeof(ItemMaster))]
        [Route("activedeactive")]
        [AcceptVerbs("PUT")]
        public ItemMaster activedeactive(ItemMaster item)
        {
            using (var db = new AuthContext())
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
                    ItemMaster Itmt = db.itemMasters.Where(s => s.ItemId == item.ItemId && s.Deleted == false).SingleOrDefault();
                    Itmt.IsPramotionalItem = false;
                    db.Entry(Itmt).State = System.Data.Entity.EntityState.Modified;
                    db.Commit();
                    return Itmt;
                }
                catch
                {
                    return null;
                }
            }
        }

    }
}
