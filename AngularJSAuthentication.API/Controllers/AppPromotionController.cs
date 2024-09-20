using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/AppPromotion")]
    public class AppPromotionController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [HttpPost]
        public void UploadFile()
        { 
        }


        [Route("")]
        public IEnumerable<MarginImage> Get()
        {
            using (var db = new AuthContext())
            {
                logger.Info("start Category: ");
                List<MarginImage> ass = new List<MarginImage>();
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
                    ass = db.MarginImageDB.Where(x => x.Deleted == false && x.ImageType == "MarginImage").ToList();
                    logger.Info("End  BaseCategory: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in BaseCategory " + ex.Message);
                    logger.Info("End  BaseCategory: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(MarginImage))]
        [Route("")]
        [AcceptVerbs("POST")]
        public MarginImage add(MarginImage item)
        {
            using (var db = new AuthContext())
            {
                logger.Info("start addCategory: ");
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
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    item.CreatedDate = indianTime;
                    item.UpdatedDate = indianTime;
                    item.IsActive = true;
                    item.Deleted = false;
                    item.ImageType = "MarginImage";
                    db.MarginImageDB.Add(item);
                    int id = db.Commit();
                    logger.Info("End  addCategory: ");
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCategory " + ex.Message);
                    logger.Info("End  addCategory: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(MarginImage))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public MarginImage Put(MarginImage item)
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

                    var items = db.MarginImageDB.Where(x => x.Id == item.Id).FirstOrDefault();

                    if (items != null)
                    {

                        items.UpdatedDate = indianTime;
                        items.LogoUrl = item.LogoUrl;
                        items.Name = item.Name;
                        items.IsActive = item.IsActive;
                        item.ImageType = "MarginImage";
                        db.MarginImageDB.Attach(items);
                        db.Entry(items).State = EntityState.Modified;
                        db.Commit();

                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCategory " + ex.Message);
                    logger.Info("End  addCategory: ");
                }
                return item;
            }
        }

        [ResponseType(typeof(MarginImage))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            using (var db = new AuthContext())
            {
                logger.Info("start del Category: ");
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
                    MarginImage category = db.MarginImageDB.Where(x => x.Id == id && x.Deleted == false).FirstOrDefault();
                    category.Deleted = true;
                    category.IsActive = false;
                    category.UpdatedDate = indianTime;
                    db.MarginImageDB.Attach(category);
                    db.Entry(category).State = EntityState.Modified;
                    db.Commit();
                    logger.Info("End  delete Category: ");
                }
                catch (Exception ex)
                {

                    logger.Error("Error in del Category " + ex.Message);


                }
            }
        }

        // removed by Harry : 21 May 2019 FindItemHighDP
        [Route("NewlyAddedBrands")]
        [HttpGet]
        public IEnumerable<SubsubCategory> AllSubsubCat()
        {
            using (var db = new AuthContext())
            {
                logger.Info("start Category: ");
                List<SubsubCategory> ass = new List<SubsubCategory>();
                try
                {
                    var firstDay = indianTime.AddDays(-30);
                    ass = db.SubsubCategorys.Where(x => x.Deleted == false && x.IsActive == true && x.CreatedDate >= firstDay).ToList();
                    logger.Info("End  : ");
                    // return ass;
                    return null;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in  " + ex.Message);
                    logger.Info("End  : ");
                    return null;
                }
            }
        }
        // removed by Harry : 21 May 2019 
        [Route("AllTopAddedItem")]
        [HttpGet]
        public IEnumerable<ItemMaster> AllTopAddedItem(int warehouseid)
        {
            using (var db = new AuthContext())
            {
                logger.Info("start Category: ");
                List<ItemMaster> ass = new List<ItemMaster>();
                try
                {
                    if (warehouseid > 0)
                    {
                        var firstDay = indianTime.AddDays(-10);
                        ass = db.itemMasters.Where(x => x.Deleted == false && x.active == true && x.CreatedDate >= firstDay && x.WarehouseId == warehouseid).ToList();
                        logger.Info("End  : ");
                        //return ass;
                        return null;
                    }
                    else
                    {
                        var firstDay = indianTime.AddDays(-10);
                        ass = db.itemMasters.Where(x => x.Deleted == false && x.active == true && x.CreatedDate >= firstDay).ToList();
                        logger.Info("End  : ");
                        // return ass;
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in  " + ex.Message);
                    logger.Info("End  : ");
                    return null;
                }
            }
        }
        // removed by Harry : 21 May 2019 FindItemHighDP
        [Route("GetTodayDhamaka")]
        [HttpGet]
        public IEnumerable<MarginImage> GetTodayDhamaka()
        {
            using (var db = new AuthContext())
            {
                logger.Info("start Category: ");
                List<MarginImage> ass = new List<MarginImage>();
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
                    ass = db.MarginImageDB.Where(x => x.Deleted == false && x.ImageType == "TodayDhamaka").ToList();
                    logger.Info("End  BaseCategory: ");
                    // return ass;
                    return null;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in BaseCategory " + ex.Message);
                    logger.Info("End  BaseCategory: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(MarginImage))]
        [Route("AddTodayDhamaka")]
        [AcceptVerbs("POST")]
        public MarginImage AddTotalDhamaka(MarginImage item)
        {
            using (var db = new AuthContext())
            {
                logger.Info("start addCategory: ");
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
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    item.CreatedDate = indianTime;
                    item.UpdatedDate = indianTime;
                    item.IsActive = true;
                    item.Deleted = false;
                    item.ImageType = "TodayDhamaka";
                    db.MarginImageDB.Add(item);
                    int id = db.Commit();
                    logger.Info("End  addCategory: ");
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCategory " + ex.Message);
                    logger.Info("End  addCategory: ");
                    return null;
                }
            }
        }
        [ResponseType(typeof(MarginImage))]
        [Route("PutTodayDhamaka")]
        [AcceptVerbs("PUT")]
        public MarginImage PutTotalDhamaka(MarginImage item)
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

                    var items = db.MarginImageDB.Where(x => x.Id == item.Id).FirstOrDefault();

                    if (items != null)
                    {

                        items.UpdatedDate = indianTime;
                        items.LogoUrl = item.LogoUrl;
                        items.Name = item.Name;
                        items.IsActive = item.IsActive;
                        item.ImageType = "TodayDhamaka";
                        db.MarginImageDB.Attach(items);
                        db.Entry(items).State = EntityState.Modified;
                        db.Commit();

                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCategory " + ex.Message);
                    logger.Info("End  addCategory: ");
                }
                return item;
            }
        }


        [ResponseType(typeof(MarginImage))]
        [Route("RemoveTodayDhamaka")]
        [AcceptVerbs("Delete")]
        public void RemoveTodayDhamaka(int id)
        {
            using (var db = new AuthContext())
            {
                logger.Info("start del Category: ");
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
                    MarginImage category = db.MarginImageDB.Where(x => x.Id == id && x.Deleted == false).FirstOrDefault();
                    category.Deleted = true;
                    category.IsActive = false;
                    category.UpdatedDate = indianTime;
                    db.MarginImageDB.Attach(category);
                    db.Entry(category).State = EntityState.Modified;
                    db.Commit();
                    logger.Info("End  delete Category: ");
                }
                catch (Exception ex)
                {

                    logger.Error("Error in del Category " + ex.Message);


                }
            }
        }


        // removed by Harry : 21 May 2019 GetBulkItem
        [Route("GetBulkItem")]
        [HttpGet]
        public IEnumerable<ItemMaster> GetBulkItem(int warehouseid)
        {
            using (var db = new AuthContext())
            {
                logger.Info("start Category: ");
                List<ItemMaster> ass = new List<ItemMaster>();
                try
                {
                    if (warehouseid > 0)
                    {
                        ass = db.itemMasters.Where(x => x.Deleted == false && x.active == true && x.MinOrderQty >= 10 && x.IsBulkItem == true && x.WarehouseId == warehouseid).ToList();
                        logger.Info("End  : ");
                        //return ass;  // removed by Harry : 21 May 2019 GetBulkItem
                        return null;
                    }
                    else
                    {
                        ass = db.itemMasters.Where(x => x.Deleted == false && x.active == true && x.MinOrderQty >= 10 && x.IsBulkItem == true).ToList();
                        logger.Info("End  : ");
                        // return ass;  // removed by Harry : 21 May 2019 GetBulkItem
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in  " + ex.Message);
                    logger.Info("End  : ");
                    return null;
                }
            }
        }

        [Route("GetBulkItemForWeb")]
        [HttpGet]
        public IEnumerable<ItemMaster> GetBulkItemForWeb()
        {
            using (var db = new AuthContext())
            {
                logger.Info("start Category: ");
                List<ItemMaster> ass = new List<ItemMaster>();
                try
                {
                    ass = db.itemMasters.Where(x => x.Deleted == false && x.active == true && x.MinOrderQty >= 10).ToList();
                    logger.Info("End  : ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in  " + ex.Message);
                    logger.Info("End  : ");
                    return null;
                }
            }
        }
        // removed by Harry : 21 May 2019 
        [Route("MostSelledItem")]
        [HttpGet]
        public IEnumerable<ItemMaster> MostSelledItem(int WarehouseId)
        {
            using (var db = new AuthContext())
            {
                logger.Info("start Category: ");
                List<ItemMaster> ass = new List<ItemMaster>();
                try
                {
                    var firstDay = indianTime.AddDays(-30);
                    //ass = db.itemMasters.Where(x => x.Deleted == false && x.active == true && x.CreatedDate >= firstDay).ToList();
                    //db.Connection.open();
                    var maxRepeated = db.DbOrderDetails.Where(s => s.CreatedDate >= firstDay && s.WarehouseId == WarehouseId).GroupBy(s => s.ItemId)
                             .OrderByDescending(s => s.Count())
                             .ToList().Take(5);
                    List<int> data1 = new List<int>();
                    foreach (var data2 in maxRepeated)
                    {
                        data1.Add(data2.Key);
                    }
                    foreach (var kk in data1)
                    {
                        var data = db.itemMasters.Where(x => x.ItemId == kk && x.active == true && x.Deleted == false).SingleOrDefault();
                        ass.Add(data);
                    }
                    logger.Info("End  : ");
                    // return ass; // removed by Harry : 21 May 2019 FindItemHighDP
                    return null;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in  " + ex.Message);
                    logger.Info("End  : ");
                    return null;
                }
            }
        }

        [Route("MostSelledBrand")]
        [HttpGet]
        public IEnumerable<SubsubCategory> MostSelledBrand()
        {
            using (var db = new AuthContext())
            {
                logger.Info("start Category: ");
                List<SubsubCategory> ass = new List<SubsubCategory>();
                try
                {
                    var firstDay = indianTime.AddDays(-30);
                    //ass = db.itemMasters.Where(x => x.Deleted == false && x.active == true && x.CreatedDate >= firstDay).ToList();
                    //db.Connection.open();
                    var maxRepeated = db.DbOrderDetails.Where(s => s.CreatedDate >= firstDay).GroupBy(s => s.SubsubcategoryName)
                             .OrderByDescending(s => s.Count())
                             .ToList().Take(5);
                    List<string> data1 = new List<string>();
                    foreach (var data2 in maxRepeated)
                    {
                        data1.Add(data2.Key);
                    }
                    foreach (var kk in data1)
                    {
                        var data = db.SubsubCategorys.Where(x => x.SubsubcategoryName == kk).FirstOrDefault();
                        ass.Add(data);
                    }
                    logger.Info("End  : ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in  " + ex.Message);
                    logger.Info("End  : ");
                    return null;
                }
            }
        }

        [Route("SelectedItem")]
        [HttpPost]
        public string SelectedItem(List<ItemMaster> SelectedItem)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start current stock: ");
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

                    int CompanyId = compid;
                    string result = context.UpdateBulkItemMaster(CompanyId, Warehouse_id, SelectedItem);
                    logger.Info("End  current stock: ");
                    return result;

                }
                catch (Exception ex)
                {
                    logger.Error("Error in current stock " + ex.Message);
                    logger.Info("End  current stock: ");
                    return "";
                }
            }
        }
    }
}





