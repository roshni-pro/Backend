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
    [RoutePrefix("api/WarehouseCategory")]
    public class WarehoseCategoryController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [Authorize]
        [Route("")]
        public IEnumerable<WarehouseSubsubCategory> Get()
        {
            logger.Info("start Category: ");
            using (AuthContext context = new AuthContext())
            {
                List<WarehouseSubsubCategory> ass = new List<WarehouseSubsubCategory>();
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouse_id);
                    if (Warehouse_id > 0)
                    {

                        ass = context.AllWarehouseCategoryWid(compid, Warehouse_id).ToList();
                        logger.Info("End  WarehouseCategory: ");
                        return ass;
                    }
                    else
                    {
                        ass = context.AllWarehouseCategory(compid).ToList();
                        logger.Info("End  WarehouseCategory: ");
                        return ass;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in WarehouseCategory " + ex.Message);
                    logger.Info("End  WarehouseCategory: ");
                    return null;
                }
            }
        }
        #region Get warehouse sub  Category
        [Route("WarehouseSubCategory")]
        public IEnumerable<WarehouseSubCategory> Getsubcatdata(int WarehouseId)
        {
            logger.Info("start Category: ");
            using (AuthContext db = new AuthContext())
            {
                List<WarehouseSubCategory> subcat = new List<WarehouseSubCategory>();
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouse_id);
                    var list = db.DbWarehouseSubCategory.Where(x => x.WarehouseId == WarehouseId).ToList();

                    return list;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in WarehouseSubCategory " + ex.Message);
                    logger.Info("End  WarehouseSubCategory: ");
                    return null;
                }
            }
        }
        #endregion
        #region Get warehouse  Category
        [Route("WarehouseCategory")]
        public IEnumerable<WarehouseCategory> Getalldata(int WarehouseId)
        {
            logger.Info("start Category: ");
            using (AuthContext db = new AuthContext())
            {
                List<WarehouseCategory> ass = new List<WarehouseCategory>();
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouse_id);
                    var list = db.DbWarehouseCategory.Where(x => x.WarehouseId == WarehouseId).ToList();

                    return list;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in WarehouseCategory " + ex.Message);
                    logger.Info("End  WarehouseCategory: ");
                    return null;
                }
            }
        }
        #endregion
        #region Get warehouse Base Category
        [Route("WHBaseCategory")]
        public IEnumerable<WarehouseBaseCategory> GetBasedata(int WarehouseId)
        {
            logger.Info("start Category: ");
            using (AuthContext db = new AuthContext())
            {
                List<WarehouseBaseCategory> ass = new List<WarehouseBaseCategory>();
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouse_id);
                    var list = db.WarehouseBaseCategoryDB.Where(x => x.WarehouseId == WarehouseId).ToList();

                    return list;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in WarehouseCategory " + ex.Message);
                    logger.Info("End  WarehouseCategory: ");
                    return null;
                }
            }
        }
        #endregion
        #region Get warehouse Sub Category
        [Route("sscategory")]
        [HttpGet]
        public IEnumerable<WarehouseSubsubCategory> GetSubSubdata(int WarehouseId)
        {
            logger.Info("start Category: ");
            using (AuthContext db = new AuthContext())
            {
                List<WarehouseSubsubCategory> ass = new List<WarehouseSubsubCategory>();
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouse_id);
                    var list = db.DbWarehousesubsubcats.Where(x => x.WarehouseId == WarehouseId).ToList();

                    return list;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in WarehouseCategory " + ex.Message);
                    logger.Info("End  WarehouseCategory: ");
                    return null;
                }
            }
        }

        #endregion

        #region Category Activated and Deactivated
        [Route("WHCatAct")]
        [AcceptVerbs("PUT")]
        public WarehouseCategory ActPut(WarehouseCategory item)
        {
            using (AuthContext db = new AuthContext())
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
                    WarehouseCategory act = db.DbWarehouseCategory.Where(x => x.WhCategoryid == item.WhCategoryid && x.Deleted == false).FirstOrDefault();
                    if (act != null)
                    {
                        act.IsActive = item.IsActive;
                        db.Entry(act).State = EntityState.Modified;
                        db.Commit();

                    }

                    //If category is Active or Deactive so all the child relation is affected and active or deactive
                    //Updated by Praveen Goswami on 21-Feb-2019

                    List<WarehouseSubCategory> listsub = new List<WarehouseSubCategory>();
                    listsub = db.DbWarehouseSubCategory.Where(x => x.Categoryid == item.WhCategoryid && x.Deleted == false).ToList();
                    foreach (var x in listsub.ToList())
                    {
                        x.IsActive = item.IsActive;
                        db.Entry(x).State = EntityState.Modified;
                        db.Commit();
                    }

                    List<WarehouseSubsubCategory> listsubsub = new List<WarehouseSubsubCategory>();
                    listsubsub = db.DbWarehousesubsubcats.Where(x => x.Categoryid == item.WhCategoryid && x.Deleted == false).ToList();
                    foreach (var y in listsubsub.ToList())
                    {
                        y.IsActive = item.IsActive;
                        db.Entry(y).State = EntityState.Modified;
                        db.Commit();
                    }
                    return item;
                }
                catch
                {
                    return null;
                }
            }
        }
        #endregion
        #region Base Category Activated and Deactivated
        [Route("WHBaseCatAct")]
        [AcceptVerbs("PUT")]
        public WarehouseBaseCategory BaseActPut(WarehouseBaseCategory item)
        {
            using (AuthContext db = new AuthContext())
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
                    WarehouseBaseCategory act = db.WarehouseBaseCategoryDB.Where(x => x.id == item.id && x.Deleted == false).FirstOrDefault();
                    if (act != null)
                    {
                        act.IsActive = item.IsActive;
                        db.Entry(act).State = EntityState.Modified;
                        db.Commit();

                    }

                    //If base category is Active or Deactive so all the child relation is affected and active or deactive
                    //Updated by Praveen Goswami on 21-Feb-2019

                    List<WarehouseCategory> listcat = new List<WarehouseCategory>();
                    listcat = db.DbWarehouseCategory.Where(x => x.BaseCategoryid == item.id && x.Deleted == false).ToList();
                    foreach (var z in listcat.ToList())
                    {
                        z.IsActive = item.IsActive;
                        db.Entry(z).State = EntityState.Modified;
                        db.Commit();
                    }

                    List<WarehouseSubCategory> listsub = new List<WarehouseSubCategory>();
                    listsub = db.DbWarehouseSubCategory.Where(x => x.BaseCategoryid == item.id && x.Deleted == false).ToList();
                    foreach (var x in listsub.ToList())
                    {
                        x.IsActive = item.IsActive;
                        db.Entry(x).State = EntityState.Modified;
                        db.Commit();
                    }

                    List<WarehouseSubsubCategory> listsubsub = new List<WarehouseSubsubCategory>();
                    listsubsub = db.DbWarehousesubsubcats.Where(x => x.BaseCategoryId == item.id && x.Deleted == false).ToList();
                    foreach (var y in listsubsub.ToList())
                    {
                        y.IsActive = item.IsActive;
                        db.Entry(y).State = EntityState.Modified;
                        db.Commit();
                    }
                    return item;
                }
                catch
                {
                    return null;
                }
            }
        }
        #endregion
        #region Sub Category Activated and Deactivated
        [Route("WHSubCatAct")]
        [AcceptVerbs("PUT")]
        public WarehouseSubCategory SubActPut(WarehouseSubCategory item)
        {
            using (AuthContext db = new AuthContext())
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
                    WarehouseSubCategory act = db.DbWarehouseSubCategory.Where(x => x.WhSubCategoryId == item.WhSubCategoryId && x.Deleted == false).FirstOrDefault();
                    if (act != null)
                    {
                        act.IsActive = item.IsActive;
                        db.Entry(act).State = EntityState.Modified;
                        db.Commit();

                    }

                    //If Subcategory is Active or Deactive so all the child relation is affected and active or deactive
                    //Updated by Praveen Goswami on 21-Feb-2019

                    List<WarehouseSubsubCategory> listsubsub = new List<WarehouseSubsubCategory>();
                    listsubsub = db.DbWarehousesubsubcats.Where(x => x.SubCategoryId == item.WhSubCategoryId && x.Deleted == false).ToList();
                    foreach (var y in listsubsub.ToList())
                    {
                        y.IsActive = item.IsActive;
                        db.Entry(y).State = EntityState.Modified;
                        db.Commit();
                    }
                    return item;
                }
                catch
                {
                    return null;
                }
            }
        }
        #endregion

        #region Sub Sub Category Activated and Deactivated
        [Route("WHSubSubCatAct")]
        [AcceptVerbs("PUT")]
        public WarehouseSubsubCategory SubsubActPut(WarehouseSubsubCategory item)
        {
            using (AuthContext db = new AuthContext())
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
                    WarehouseSubsubCategory act = db.DbWarehousesubsubcats.Where(x => x.WhSubsubCategoryid == item.WhSubsubCategoryid && x.Deleted == false).FirstOrDefault();
                    if (act != null)
                    {
                        act.IsActive = item.IsActive;
                        db.Entry(act).State = EntityState.Modified;
                        db.Commit();

                    }
                    return item;
                }
                catch
                {
                    return null;
                }
            }
        }
        #endregion


        #region Mapp warehouse Base Category
        [Route("BaseCategory")]
        [HttpGet]
        public IEnumerable<WHBaseCategoryDTO> BasecategoryGet(int WarehouseId)
        {
            logger.Info("start Category: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    List<WHBaseCategoryDTO> NotMappeDataBase = new List<WHBaseCategoryDTO>();
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    //Access claims
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

                    var Cat = db.BaseCategoryDb.Where(x => x.Deleted == false).ToList();
                    var Whdata = db.Warehouses.Where(x => x.WarehouseId == WarehouseId).SingleOrDefault();
                    var MappedData = db.WarehouseBaseCategoryDB.Where(x => x.Deleted == false && x.WarehouseId == WarehouseId).ToList();

                    foreach (var Warehouse in Cat)
                    {
                        if (MappedData.Any(x => x.BaseCategoryId == Warehouse.BaseCategoryId))
                        {

                        }
                        else
                        {


                            WHBaseCategoryDTO BData = new WHBaseCategoryDTO()
                            {
                                BaseCategoryId = Warehouse.BaseCategoryId,
                                BaseCategoryName = Warehouse.BaseCategoryName,
                                Code = Warehouse.Code,
                                LogoUrl = Warehouse.LogoUrl,
                                WarehouseId = Whdata.WarehouseId,
                                WarehouseName = Whdata.WarehouseName,
                                CreatedDate = Warehouse.CreatedDate,
                                UpdatedDate = Warehouse.UpdatedDate,
                                Deleted = Warehouse.Deleted,
                                CompanyId = compid,
                                CreatedBy = Warehouse.CreatedBy,
                                UpdateBy = Warehouse.UpdateBy,
                                IsActive = Warehouse.IsActive,
                                HindiName = Warehouse.HindiName

                            };
                            //MappeData.Add(Warehouse.Categoryid);

                            NotMappeDataBase.Add(BData);
                        }

                    }

                    logger.Info("End  WarehouseCategory: ");


                    return NotMappeDataBase;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in WarehouseCategory " + ex.Message);
                    logger.Info("End  WarehouseCategory: ");
                    return null;
                }
            }
        }
        #endregion
        #region Mapp warehouse Category
        [Route("NotMapped")]
        [HttpGet]
        public IEnumerable<WarehouseCategoryDTO> NotMappedGet(int WarehouseId)
        {
            logger.Info("start Category: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    List<WarehouseCategoryDTO> NotMappeData = new List<WarehouseCategoryDTO>();
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    //Access claims
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

                    var Cat = db.Categorys.Where(x => x.Deleted == false).ToList();
                    var Whdata = db.Warehouses.Where(x => x.WarehouseId == WarehouseId).SingleOrDefault();
                    var MappedData = db.DbWarehouseCategory.Where(x => x.Deleted == false && x.WarehouseId == WarehouseId).ToList();

                    List<WarehouseCategoryDTO> MappeData = new List<WarehouseCategoryDTO>();
                    foreach (var Warehouse in Cat)
                    {
                        if (MappedData.Any(x => x.Categoryid == Warehouse.Categoryid))
                        {

                        }
                        else
                        {
                            try
                            {
                                var MappedBasecatData = db.WarehouseBaseCategoryDB.Where(x => x.Deleted == false && x.WarehouseId == WarehouseId && x.BaseCategoryId == Warehouse.BaseCategoryId).SingleOrDefault();
                                if (MappedBasecatData != null)
                                {
                                    WarehouseCategoryDTO MData = new WarehouseCategoryDTO()
                                    {


                                        Categoryid = Warehouse.Categoryid,
                                        CategoryName = Warehouse.CategoryName,
                                        BaseCategoryID = MappedBasecatData.id,
                                        Code = Warehouse.Code,
                                        LogoUrl = Warehouse.LogoUrl,
                                        CompanyId = compid,
                                        WarehouseId = Whdata.WarehouseId,
                                        WarehouseName = Whdata.WarehouseName,
                                        CreatedDate = Warehouse.CreatedDate,
                                        UpdatedDate = Warehouse.UpdatedDate,
                                        Deleted = Warehouse.Deleted,
                                        CreatedBy = Warehouse.CreatedBy,
                                        UpdateBy = Warehouse.UpdateBy,
                                        IsActive = Warehouse.IsActive

                                    };
                                    //MappeData.Add(Warehouse.Categoryid);

                                    NotMappeData.Add(MData);
                                }
                            }
                            catch (Exception tt)
                            {
                                logger.Error("Error in WarehouseCategory " + tt.Message);
                            }
                        }

                    }

                    logger.Info("End  WarehouseCategory: ");


                    return NotMappeData;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in WarehouseCategory " + ex.Message);
                    logger.Info("End  WarehouseCategory: ");
                    return null;
                }
            }
        }

        #endregion
        #region Mapp warehouse Sub Category
        [Route("WhSubcategory")]
        [HttpGet]
        public IEnumerable<WarehouseSubCategoryDTO> NotMappedGetsubcat(int WarehouseId)
        {
            logger.Info("start Category: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    List<WarehouseSubCategoryDTO> NotMappeData = new List<WarehouseSubCategoryDTO>();
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    //Access claims
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

                    var Cat = db.SubCategorys.Where(x => x.Deleted == false).ToList();
                    var Whdata = db.Warehouses.Where(x => x.WarehouseId == WarehouseId).SingleOrDefault();
                    var MappedData = db.DbWarehouseSubCategory.Where(x => x.Deleted == false && x.WarehouseId == WarehouseId).ToList();
                    List<WarehouseSubCategoryDTO> MappeData = new List<WarehouseSubCategoryDTO>();
                    foreach (var Warehouse in Cat)
                    {
                        if (MappedData.Any(x => x.SubCategoryId == Warehouse.SubCategoryId))
                        {

                        }
                        else
                        {

                            var MappedBaseSubcatData = db.DbWarehouseCategory.Where(x => x.Deleted == false && x.WarehouseId == WarehouseId && x.Categoryid == Warehouse.Categoryid).SingleOrDefault();
                            if (MappedBaseSubcatData != null)
                            {
                                WarehouseSubCategoryDTO MData = new WarehouseSubCategoryDTO()
                                {
                                    BaseCategoryID = MappedBaseSubcatData.BaseCategoryid,
                                    SubCategoryId = Warehouse.SubCategoryId,
                                    SubcategoryName = Warehouse.SubcategoryName,
                                    Categoryid = Warehouse.Categoryid,
                                    CategoryName = Warehouse.CategoryName,
                                    HindiName = Warehouse.HindiName,
                                    SortOrder = Warehouse.SortOrder,
                                    IsPramotional = Warehouse.IsPramotional,
                                    Code = Warehouse.Code,
                                    LogoUrl = Warehouse.LogoUrl,
                                    CompanyId = compid,
                                    WarehouseId = Whdata.WarehouseId,
                                    WarehouseName = Whdata.WarehouseName,
                                    CreatedDate = Warehouse.CreatedDate,
                                    UpdatedDate = Warehouse.UpdatedDate,
                                    Deleted = Warehouse.Deleted,
                                    CreatedBy = Warehouse.CreatedBy,
                                    UpdateBy = Warehouse.UpdateBy,
                                    IsActive = Warehouse.IsActive

                                };

                                NotMappeData.Add(MData);
                            }
                        }

                    }

                    logger.Info("End  WarehouseCategory: ");


                    return NotMappeData;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in WarehouseCategory " + ex.Message);
                    logger.Info("End  WarehouseCategory: ");
                    return null;
                }
            }
        }

        #endregion
        #region Mapp warehouse Sub Sub Category
        [Route("WhSubSubcategory")]
        [HttpGet]
        public IEnumerable<WarehouseSubsubCategoryDTO> NotMappedGetsubsubcat(int WarehouseId)
        {
            logger.Info("start Category: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    List<WarehouseSubsubCategoryDTO> NotMappeData = new List<WarehouseSubsubCategoryDTO>();
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    //Access claims
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

                    var Cat = db.SubsubCategorys.Where(x => x.Deleted == false).ToList();
                    var Whdata = db.Warehouses.Where(x => x.WarehouseId == WarehouseId).SingleOrDefault();
                    var MappedData = db.DbWarehousesubsubcats.Where(x => x.Deleted == false && x.WarehouseId == WarehouseId).ToList();
                    List<WarehouseSubsubCategoryDTO> MappeData = new List<WarehouseSubsubCategoryDTO>();
                    foreach (var Warehouse in Cat)
                    {
                        if (MappedData.Any(x => x.SubsubCategoryid == Warehouse.SubsubCategoryid))
                        {

                        }
                        else
                        {

                            var MappedBaseSubSubcatData = db.DbWarehouseSubCategory.Where(x => x.Deleted == false && x.WarehouseId == WarehouseId && x.SubCategoryId == Warehouse.SubCategoryId).SingleOrDefault();
                            if (MappedBaseSubSubcatData != null)
                            {
                                WarehouseSubsubCategoryDTO MData = new WarehouseSubsubCategoryDTO()
                                {
                                    BaseCategoryId = MappedBaseSubSubcatData.BaseCategoryid,
                                    SubCategoryId = Warehouse.SubCategoryId,
                                    SubcategoryName = Warehouse.SubcategoryName,
                                    SubsubCategoryid = Warehouse.SubsubCategoryid,
                                    SubsubcategoryName = Warehouse.SubsubcategoryName,
                                    Categoryid = Warehouse.Categoryid,
                                    //BaseCategoryId = Warehouse.BaseCategoryId,
                                    CategoryName = Warehouse.CategoryName,
                                    HindiName = Warehouse.HindiName,
                                    SortOrder = Warehouse.SortOrder,
                                    IsPramotional = Warehouse.IsPramotional,
                                    Code = Warehouse.Code,
                                    LogoUrl = Warehouse.LogoUrl,
                                    CompanyId = compid,
                                    WarehouseId = Whdata.WarehouseId,
                                    WarehouseName = Whdata.WarehouseName,
                                    CreatedDate = Warehouse.CreatedDate,
                                    UpdatedDate = Warehouse.UpdatedDate,
                                    Deleted = Warehouse.Deleted,
                                    CreatedBy = Warehouse.CreatedBy,
                                    UpdateBy = Warehouse.UpdateBy,
                                    IsActive = Warehouse.IsActive,
                                    Type = Warehouse.Type,
                                    CommisionPercent = Warehouse.CommisionPercent,
                                    IsExclusive = Warehouse.IsExclusive
                                };

                                NotMappeData.Add(MData);
                            }
                        }

                    }

                    logger.Info("End  WarehouseCategory: ");


                    return NotMappeData;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in WarehouseCategory " + ex.Message);
                    logger.Info("End  WarehouseCategory: ");
                    return null;
                }
            }
        }

        #endregion

        [ResponseType(typeof(WarehouseCategory))]
        [Route("")]
        public IEnumerable<WarehouseCategory> GetAllCategory(string i)
        {
            logger.Info("start Category: ");
            using (AuthContext context = new AuthContext())
            {
                List<WarehouseCategory> warehouseCategory = new List<WarehouseCategory>();
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
                    warehouseCategory = context.AllWhCategory().ToList();
                    logger.Info("End  WarehouseCategory: ");
                    return warehouseCategory;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in WarehouseCategory " + ex.Message);
                    logger.Info("End  WarehouseCategory: ");
                    return null;
                }
            }
        }
        [ResponseType(typeof(WarehouseCategory))]
        [Route("")]
        [AcceptVerbs("POST")]
        public List<WarehouseCategory> add(List<WarehouseCategory> item)
        {
            logger.Info("start addWarehouseCategory: ");
            using (AuthContext context = new AuthContext())
            {
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
                    context.AddToWarehousesCategorys(item);
                    return item;


                }
                catch (Exception ex)
                {
                    logger.Error("Error in addWarehouseCategory " + ex.Message);
                    logger.Info("End  addWarehouseCategory: ");
                    return null;
                }
            }
        }
        [ResponseType(typeof(WarehouseBaseCategory))]
        [Route("BaseCategory")]
        [AcceptVerbs("POST")]
        public List<WarehouseBaseCategory> add(List<WarehouseBaseCategory> item)
        {
            logger.Info("start addWarehouseCategory: ");
            using (AuthContext context = new AuthContext())
            {
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
                    context.AddToWarehousesBaseCategorys(item);
                    return item;


                }
                catch (Exception ex)
                {
                    logger.Error("Error in addWarehouseCategory " + ex.Message);
                    logger.Info("End  addWarehouseCategory: ");
                    return null;
                }
            }
        }
        #region mapped data save to Sub Category table
        [ResponseType(typeof(WarehouseSubCategory))]
        [Route("SubCategory")]
        [AcceptVerbs("POST")]
        public List<WarehouseSubCategory> add(List<WarehouseSubCategory> item)
        {
            logger.Info("start addWarehouseCategory: ");
            using (AuthContext context = new AuthContext())
            {
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
                    context.AddToWarehousesSubCategorys(item);
                    return item;


                }
                catch (Exception ex)
                {
                    logger.Error("Error in addWarehouseCategory " + ex.Message);
                    logger.Info("End  addWarehouseCategory: ");
                    return null;
                }
            }
        }

        #endregion
        #region mapped data save to Sub Sub Category table
        [ResponseType(typeof(WarehouseSubsubCategory))]
        [Route("SubSubCategory")]
        [AcceptVerbs("POST")]
        public List<WarehouseSubsubCategory> addsubsub(List<WarehouseSubsubCategory> item)
        {
            logger.Info("start addWarehouseCategory: ");
            using (AuthContext context = new AuthContext())
            {
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
                    context.AddToWarehousesSubSubCategorys(item);
                    return item;


                }
                catch (Exception ex)
                {
                    logger.Error("Error in addWarehouseCategory " + ex.Message);
                    logger.Info("End  addWarehouseCategory: ");
                    return null;
                }
            }
        }
        #endregion

        [ResponseType(typeof(WarehouseSubsubCategory))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public List<WarehouseSubsubCategory> Put(List<WarehouseSubsubCategory> item)
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    return context.PutWarehouseCategory(item, compid);
                }
                catch
                {
                    return null;
                }
            }
        }

        [ResponseType(typeof(WarehouseSubsubCategory))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("start del WarehouseCategory: ");
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
                    context.DeleteWarehouseCategory(id, compid);
                    logger.Info("End  delete WarehouseCategory: ");
                }
                catch (Exception ex)
                {

                    logger.Error("Error in del WarehouseCategory " + ex.Message);


                }
            }
        }

    }
    public class WarehouseCategoryDTO
    {
        public int BaseCategoryID { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string Code { get; set; }
        public int Categoryid { get; set; }
        public string CategoryName { get; set; }
        public string LogoUrl { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public int CompanyId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdateBy { get; set; }
        public bool Deleted { get; set; }
    }
    public class WHBaseCategoryDTO
    {
        public int BaseCategoryId { get; set; }
        public string BaseCategoryName { get; set; }
        public string HindiName { get; set; }
        public string Code { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdateBy { get; set; }
        public string LogoUrl { get; set; }
        public bool Deleted { get; set; }
        public int CompanyId { get; set; }
        public bool IsActive { get; set; }
        public string WarehouseName { get; set; }
        public int WarehouseId { get; set; }

    }

    public class WarehouseSubCategoryDTO
    {
        public int BaseCategoryID { get; set; }
        public int SubCategoryId { get; set; }
        public int Categoryid { get; set; }
        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string HindiName { get; set; }
        public int? SortOrder { get; set; }
        public bool? IsPramotional { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdateBy { get; set; }
        public string Code { get; set; }
        public string LogoUrl { get; set; }
        public bool Deleted { get; set; }
        public bool IsActive { get; set; }
        public int CompanyId { get; set; }

    }
    public class WarehouseSubsubCategoryDTO
    {

        public int SubsubCategoryid { get; set; }
        public string SubsubcategoryName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string HindiName { get; set; }
        public int BaseCategoryId { get; set; }
        public int Categoryid { get; set; }
        public int? SortOrder { get; set; }
        public bool? IsPramotional { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public string CategoryName { get; set; }
        public int SubCategoryId { get; set; }
        public string SubcategoryName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdateBy { get; set; }
        public string LogoUrl { get; set; }
        public bool IsActive { get; set; }
        public bool Deleted { get; set; }
        public double? CommisionPercent { get; set; }
        public int CompanyId { get; set; }
        public bool? IsExclusive
        {
            get; set;
        }



    }
}



