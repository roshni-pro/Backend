using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Description;


namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Category")]
    public class CategoryController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Authorize]
        [Route("")]
        public IEnumerable<Category> Get()
        {
            using (var context = new AuthContext())
            {
                logger.Info("start Category: ");
                List<Category> ass = new List<Category>();
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
                    ass = context.AllCategory(compid).ToList();
                    logger.Info("End  Category: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
        }

        [Authorize]
        [Route("")]
        public IEnumerable<WarehouseSubsubCategory> Get(string recordtype, int whid)
        {
            using (var context = new AuthContext())
            {
                if (recordtype == "warehouse")
                {
                    logger.Info("start Category: ");
                    List<SubsubCategory> sCategory = new List<SubsubCategory>();
                    List<Category> Category = new List<Category>();
                    List<Warehouse> Warehouse = new List<Warehouse>();
                    List<WarehouseCategory> WarehouseCategory = new List<WarehouseCategory>();
                    List<WarehouseSubsubCategory> sWarehouseCategory = new List<WarehouseSubsubCategory>();
                    List<WarehouseSubsubCategory> wareH = new List<WarehouseSubsubCategory>();
                    List<string> Subcode = new List<string>();
                    try
                    {
                        var identity = User.Identity as ClaimsIdentity;
                        int compid = 0, userid = 0;
                        int Warehouse_id = 0;
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
                                Warehouse_id = int.Parse(claim.Value);
                            }
                        }



                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        Category = context.AllCategory(compid).ToList();
                        sCategory = context.sAllCategory(compid).ToList();
                        Warehouse = context.AllWarehouse(compid).ToList();

                        wareH = context.AllWarehouseCategory(compid).ToList();


                        var cat = Category;
                        var scat = sCategory;
                        var war = (from c in Warehouse where c.WarehouseId.Equals(whid) && c.CompanyId == compid select c).SingleOrDefault();

                        for (int i = 0; i < scat.Count; i++)
                        {

                            List<WarehouseSubsubCategory> wcat = (from c in wareH where c.WarehouseId == whid && c.Deleted == false && c.CompanyId == compid select c).ToList();


                            WarehouseSubsubCategory wc = new WarehouseSubsubCategory();
                            wc.SubsubCategoryid = scat[i].SubsubCategoryid;
                            wc.SubsubcategoryName = scat[i].SubcategoryName;
                            wc.SubsubCode = scat[i].Code;
                            wc.IsActive = true;
                            wc.LogoUrl = scat[i].LogoUrl;
                            wc.WarehouseId = whid;



                            foreach (var c in wcat)
                            {


                                if (c.SubsubCategoryid.Equals(scat[i].SubsubCategoryid))
                                {
                                    wc.SubsubCategoryid = c.SubsubCategoryid;
                                    wc.IsActive = true;
                                    wc.SortOrder = c.SortOrder;
                                }
                            }

                            if (wc != null && !Subcode.Any(x => x == wc.SubsubCode))
                            {

                                sWarehouseCategory.Add(wc);
                                Subcode.Add(wc.SubsubCode);
                            }


                        }
                        logger.Info("End  Category: ");
                        return sWarehouseCategory;
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error in Category " + ex.Message);
                        logger.Info("End  Category: ");
                        return null;
                    }
                }
            }
            return null;
        }

        [Authorize]
        [Route("")]
        public IEnumerable<WarehouseCategory> Get(string recordtype, int whid, int whcatid)
        {
            using (var context = new AuthContext())
            {
                if (recordtype == "warehouse")
                {
                    logger.Info("start Category: ");
                    List<Category> Category = new List<Category>();
                    List<Warehouse> Warehouse = new List<Warehouse>();
                    List<WarehouseCategory> WarehouseCategory = new List<WarehouseCategory>();
                    List<WarehouseCategory> wareH = new List<WarehouseCategory>();
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
                        Category = context.AllCategory(compid).ToList();
                        Warehouse = context.AllWarehouse(compid).ToList();


                        var cat = Category;
                        var war = (from c in Warehouse where c.WarehouseId.Equals(whid) && c.CompanyId == compid select c).SingleOrDefault();

                        List<WarehouseCategory> wcat = (from c in wareH where c.WarehouseId == whid && c.CompanyId == compid select c).ToList();
                        foreach (var i in wcat)
                        {

                            if (i.IsActive)
                            {
                                WarehouseCategory wc = new WarehouseCategory();
                                wc.WhCategoryid = whcatid;
                                wc.Categoryid = i.Categoryid;
                                wc.CategoryName = i.CategoryName;
                                wc.WarehouseId = whid;
                                //wc.Stateid = war.Stateid;
                                //wc.State = war.StateName;
                                //wc.Cityid = war.Cityid;
                                //wc.City = war.CityName;
                                wc.IsActive = true;
                                wc.SortOrder = i.SortOrder;

                                WarehouseCategory.Add(wc);
                            }

                        }
                        logger.Info("End  Category: ");
                        return WarehouseCategory;
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error in Category " + ex.Message);
                        logger.Info("End  Category: ");
                        return null;
                    }
                }
            }
            return null;
        }
        #region get Category code
        public string convertnumber2String(int number)
        {
            string text = "";
            switch (number)
            {
                case 1:
                    {
                        text = "A";
                        break;
                    }
                case 2:
                    {
                        text = "B";
                        break;
                    }
                case 3:
                    {
                        text = "C";
                        break;
                    }
                case 4:
                    {
                        text = "D";
                        break;
                    }
                case 5:
                    {
                        text = "E";
                        break;
                    }
                case 6:
                    {
                        text = "F";
                        break;
                    }
                case 7:
                    {
                        text = "G";
                        break;
                    }
                case 8:
                    {
                        text = "H";
                        break;
                    }
                case 9:
                    {
                        text = "I";
                        break;
                    }
                case 10:
                    {
                        text = "J";
                        break;
                    }
                case 11:
                    {
                        text = "K";
                        break;
                    }
                case 12:
                    {
                        text = "L";
                        break;
                    }
                case 13:
                    {
                        text = "M";
                        break;
                    }
                case 14:
                    {
                        text = "N";
                        break;
                    }
                case 15:
                    {
                        text = "O";
                        break;
                    }
                case 16:
                    {
                        text = "P";
                        break;
                    }
                case 17:
                    {
                        text = "Q";
                        break;
                    }
                case 18:
                    {
                        text = "R";
                        break;
                    }
                case 19:
                    {
                        text = "S";
                        break;
                    }
                case 20:
                    {
                        text = "T";
                        break;
                    }
                case 21:
                    {
                        text = "U";
                        break;
                    }
                case 22:
                    {
                        text = "V";
                        break;
                    }
                case 23:
                    {
                        text = "W";
                        break;
                    }
                case 24:
                    {
                        text = "X";
                        break;
                    }
                case 25:
                    {
                        text = "Y";
                        break;
                    }
                case 26:
                    {
                        text = "Z";
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            return text;
        }

        public string GetCategoryCode()
        {

            int cat1 = int.Parse(WebConfigurationManager.AppSettings["cat1"]);
            int cat2 = int.Parse(WebConfigurationManager.AppSettings["cat2"]);

            if (cat2 > 26)
            {
                cat2 = 1;
                cat1 += 1;
            }
            else
            {
                cat2 += 1;
            }
            string newcat1 = convertnumber2String(cat1);
            string newcat2 = convertnumber2String(cat2);

            //Update Configure Cat1 and Cat2 value 

            ////Helps to open the Root level web.config file.
            //Configuration webConfigApp = WebConfigurationManager.OpenWebConfiguration("~");

            ////Modifying the AppKey from AppValue to AppValue1

            //webConfigApp.AppSettings.Settings["cat1"].Value = Convert.ToString(cat1);
            //webConfigApp.AppSettings.Settings["cat2"].Value = Convert.ToString(cat2);
            ////Save the Modified settings of AppSettings.
            //webConfigApp.Save();

            var data = newcat1.Trim() + newcat2.Trim();
            data = data.Replace("\"", string.Empty).Trim();
            return data;
        }

        #endregion


        [ResponseType(typeof(Category))]
        [Route("")]
        [AcceptVerbs("POST")]
        public Category add(Category item)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start addCategory: ");
                try
                {
                    int cat1 = int.Parse(WebConfigurationManager.AppSettings["cat1"]);
                    int cat2 = int.Parse(WebConfigurationManager.AppSettings["cat2"]);
                    Configuration webConfigApp = WebConfigurationManager.OpenWebConfiguration("~");
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
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.AddCategory(item);
                    //if (cat2 > 26)
                    //{
                    //    cat2 = 1;
                    //    cat1 += 1;
                    //}
                    //else
                    //{
                    //    cat2 += 1;
                    //}
                    //webConfigApp.AppSettings.Settings["cat1"].Value = Convert.ToString(cat1);
                    //webConfigApp.AppSettings.Settings["cat2"].Value = Convert.ToString(cat2);
                    ////Save the Modified settings of AppSettings.
                    //webConfigApp.Save();
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

        [ResponseType(typeof(Category))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public Category Put(Category item)
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

                    return context.PutCategory(item);
                }
                catch
                {
                    return null;
                }
            }
        }


        [ResponseType(typeof(Category))]
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

                    Category category = db.Categorys.Where(x => x.Categoryid == id && x.Deleted == false).FirstOrDefault();
                    category.Deleted = true;
                    category.IsActive = false;
                    //db.Categorys.Attach(category);
                    db.Entry(category).State = EntityState.Modified;
                    db.Commit();

                    CommonHelper.refreshCategory();


                    logger.Info("End  delete Category: ");
                }
                catch (Exception ex)
                {
                    logger.Error("Error in del Category " + ex.Message);
                }
            }
        }

        [ResponseType(typeof(Category))]
        [Route("DeleteV7")]
        [AcceptVerbs("Delete")]
        public Boolean DeleteV7(int id)
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

                    Category category = db.Categorys.Where(x => x.Categoryid == id && x.Deleted == false).FirstOrDefault();
                    category.Deleted = true;
                    category.IsActive = false;
                    //db.Categorys.Attach(category);
                    db.Entry(category).State = EntityState.Modified;
                    db.Commit();

                    CommonHelper.refreshCategory();


                    logger.Info("End  delete Category: ");
                    return true;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in del Category " + ex.Message);
                    return false;
                }
            }
        }

        #region Get all active category 
        /// <summary>
        /// Created Date 22/08/2019
        /// Created By Raj
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("activeCat")]
        public IEnumerable<Category> GetactiveCat()
        {
            using (var context = new AuthContext())
            {
                logger.Info("start Category: ");
                List<Category> ass = new List<Category>();
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
                    ass = context.Categorys.Where(x => x.Deleted == false && x.IsActive == true).ToList();
                    logger.Info("End  Category: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
            #endregion
        }
    }
}