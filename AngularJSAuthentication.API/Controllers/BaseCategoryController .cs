using AngularJSAuthentication.API.Controllers.Base;
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
    [RoutePrefix("api/BaseCategory")]
    public class BaseCategoryController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        [Authorize]
        [Route("")]
        public IEnumerable<BaseCategory> Get()
        {
            using (var context = new AuthContext())
            {
                logger.Info("start Category: ");
                List<BaseCategory> ass = new List<BaseCategory>();
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
                    ass = context.BaseCategoryDb.Where(x => x.Deleted == false && x.IsActive == true).ToList();
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

        [ResponseType(typeof(BaseCategory))]
        [Route("")]
        [AcceptVerbs("POST")]
        public BaseCategory add(BaseCategory item)
        {
            using (var db = new AuthContext())
            {
                logger.Info("start addCategory: ");
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
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    //BaseCategory Ischecked = db.BaseCategoryDb.Where(c => c.BaseCategoryName.Contains(item.BaseCategoryName) && c.Deleted == false).FirstOrDefault();
                    BaseCategory Ischecked = db.BaseCategoryDb.Where(c => c.BaseCategoryName.Trim().Equals(item.BaseCategoryName.Trim()) && c.Deleted == false).FirstOrDefault();
                    People people = db.Peoples.Where(p => p.PeopleID == userid).FirstOrDefault();
                    if (Ischecked == null)
                    {
                        item.CreatedDate = indianTime;
                        item.UpdatedDate = indianTime;
                        item.CreatedBy = people.DisplayName;
                        //item.IsActive = true;
                        item.Deleted = false;
                        db.BaseCategoryDb.Add(item);
                        int id = db.Commit();
                    }
                    else
                    {
                        item.Isalreadyadd = true;
                    }
                    //CommonHelper.refreshCategory();
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

        [ResponseType(typeof(BaseCategory))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public BaseCategory Put(BaseCategory item)
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

                    var items = db.BaseCategoryDb.Where(x => x.BaseCategoryId == item.BaseCategoryId).FirstOrDefault();
                    People people = db.Peoples.Where(p => p.PeopleID == userid).FirstOrDefault();
                    if (items != null)
                    {

                        items.UpdatedDate = indianTime;
                        string logourl = item.LogoUrl.Trim('"');
                        items.LogoUrl = logourl;
                        items.BaseCategoryName = item.BaseCategoryName;
                        items.HindiName = item.HindiName;
                        items.Code = item.Code;
                        items.IsActive = item.IsActive;
                        items.UpdateBy = people.DisplayName;
                        //db.BaseCategoryDb.Attach(items);
                        db.Entry(items).State = EntityState.Modified;
                        db.Commit();
                    }
                    else
                    {
                        return null;
                    }
                    CommonHelper.refreshCategory();
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCategory " + ex.Message);
                    logger.Info("End  addCategory: ");
                }
                return item;
            }
        }


        [ResponseType(typeof(BaseCategory))]
        [Route("PUTV7")]
        [AcceptVerbs("PUT")]
        public BaseCategory PUTV7(BaseCategory item)
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

                    var items = db.BaseCategoryDb.Where(x => x.BaseCategoryId == item.BaseCategoryId).FirstOrDefault();
                    People people = db.Peoples.Where(p => p.PeopleID == userid).FirstOrDefault();

                    if (items != null)
                    {
                        if(items.BaseCategoryName == item.BaseCategoryName)
                        {
                                items.UpdatedDate = indianTime;
                                items.UpdateBy = people.DisplayName;
                                string logourl = item.LogoUrl.Trim('"');
                                items.LogoUrl = logourl;
                                items.BaseCategoryName = item.BaseCategoryName;
                                items.HindiName = item.HindiName;
                                items.Code = item.Code;
                                items.Discription = item.Discription;
                                items.IsActive = item.IsActive;
                                //db.BaseCategoryDb.Attach(items);
                                db.Entry(items).State = EntityState.Modified;
                                db.Commit();
                        }
                        else
                        {
                            BaseCategory IscheckedName = db.BaseCategoryDb.Where(c => c.BaseCategoryName.Trim().Equals(item.BaseCategoryName.Trim()) && c.Deleted == false).FirstOrDefault();
                            if (IscheckedName == null)
                            {
                                items.UpdatedDate = indianTime;
                                items.UpdateBy = people.DisplayName;
                                string logourl = item.LogoUrl.Trim('"');
                                items.LogoUrl = logourl;
                                items.BaseCategoryName = item.BaseCategoryName;
                                items.HindiName = item.HindiName;
                                items.Code = item.Code;
                                items.Discription = item.Discription;
                                items.IsActive = item.IsActive;
                                //db.BaseCategoryDb.Attach(items);
                                db.Entry(items).State = EntityState.Modified;
                                db.Commit();
                            }
                            else
                            {
                                item.Isalreadyadd = true;
                            }
                        }                                             
                       
                    }
                    else
                    {
                        return null;
                    }
                    CommonHelper.refreshCategory();
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCategory " + ex.Message);
                    logger.Info("End  addCategory: ");
                }
                return item;
            }
        }


        [ResponseType(typeof(BaseCategory))]
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
                    BaseCategory category = db.BaseCategoryDb.Where(x => x.BaseCategoryId == id && x.Deleted == false).FirstOrDefault();
                    category.Deleted = true;
                    category.IsActive = false;
                    category.UpdatedDate = indianTime;
                    //db.BaseCategoryDb.Attach(category);
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

        //[ResponseType(typeof(BaseCategory))]
        [Route("DeleteV7")]
        [AcceptVerbs("Delete")]
        [HttpDelete]
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
                    BaseCategory category = db.BaseCategoryDb.Where(x => x.BaseCategoryId == id && x.Deleted == false).FirstOrDefault();
                    category.Deleted = true;
                    category.IsActive = false;
                    category.UpdatedDate = indianTime;
                    //db.BaseCategoryDb.Attach(category);
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

        #region get Active  base category 
        /// <summary>
        /// Created Date 22/08/2019
        /// Created By Raj
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("activeBase")]
        public IEnumerable<BaseCategory> GetactiveBase()
        {
            using (var context = new AuthContext())
            {
                logger.Info("start Category: ");
                List<BaseCategory> ass = new List<BaseCategory>();
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
                    ass = context.BaseCategoryDb.Where(x => x.Deleted == false && x.IsActive == true).ToList();
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
        #endregion

        #region GetBaseCategoryListA7
        [HttpGet]
        [Route("GetBaseCategoryList")]
        [AllowAnonymous]
        public List<BaseCategory> GetBaseCategoryList()
        {
            using (AuthContext context = new AuthContext())
            {
                var list = context.BaseCategoryDb.Where(x => x.Deleted == false).ToList();

                return list;
            }

        }
        #endregion
    }
}



