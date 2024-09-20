using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;


namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/SubCategory")]
    public class SubCategoryController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
        public IEnumerable<SubCategory> Get()
        {
            logger.Info("start SubCategory: ");
            List<SubCategory> ass = new List<SubCategory>();
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
                using (var db = new AuthContext())
                {
                    ass = db.AllSubCategory(compid).ToList();
                }
                logger.Info("End  SubCategory: ");
                return ass;
            }
            catch (Exception ex)
            {
                logger.Error("Error in SubCategory " + ex.Message);
                logger.Info("End  SubCategory: ");
                return null;
            }
        }

        [Authorize]
        [HttpGet]
        [Route("getallsubcategory")]
        public dynamic Getdata()
        {
            try
            {
                using (var db = new AuthContext())
                {
                    var sub = db.Database.SqlQuery<SubCategorysDTO>("GetSubCategory").ToList();
                    return sub;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in SubCategory " + ex.Message);
                logger.Info("End  SubCategory: ");
                return null;
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetItemSchemeSubCategory")]
        public dynamic GetItemSchemeSubCategory()
        {
            try
            {
                using (var db = new AuthContext())
                {
                    var sub = db.Database.SqlQuery<ItemSchemeSubCategorys>("GetItemSchemeSubCategory").ToList();
                    return sub;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in SubCategory " + ex.Message);
                logger.Info("End  SubCategory: ");
                return null;
            }
        }


        [Authorize]
        [Route("")]
        public IEnumerable<SubCategory> Get(string id)
        {
            logger.Info("start SubCategory: ");
            List<SubCategory> ass = new List<SubCategory>();
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
                int CompanyId = compid;
                using (var db = new AuthContext())
                {
                    ass = db.AllSubCategoryy(idd, CompanyId).ToList();
                }
                logger.Info("End  SubCategory: ");
                return ass;
            }
            catch (Exception ex)
            {
                logger.Error("Error in SubCategory " + ex.Message);
                logger.Info("End  SubCategory: ");
                return null;
            }
        }


        [Authorize]
        [Route("")]
        public IEnumerable<WarehouseSubCategory> Get(string recordtype, int whid)
        {
            if (recordtype == "warehouse")
            {
                logger.Info("start Category: ");
                List<SubCategory> subCategory = new List<SubCategory>();
                List<Warehouse> Warehouse = new List<Warehouse>();
                List<WarehouseSubCategory> warehouseSubCategory = new List<WarehouseSubCategory>();
                logger.Info("start SubCategory: ");

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
                    using (var db = new AuthContext())
                    {
                        subCategory = db.AllSubCategory(compid).ToList();
                        Warehouse = db.AllWarehouse(compid).ToList();
                        var cat = (from c in subCategory where c.Deleted.Equals(false) select c).ToList();
                        var war = (from c in Warehouse where c.WarehouseId.Equals(whid) select c).SingleOrDefault();
                        for (int i = 0; i < cat.Count(); i++)
                        {
                            WarehouseSubCategory wc = new WarehouseSubCategory();
                            wc.SubCategoryId = cat[i].SubCategoryId;
                            wc.SubcategoryName = cat[i].SubcategoryName;
                            // wc.Warehouseid = cat[i].Warehouseid;

                            wc.CompanyId = war.CompanyId;

                            warehouseSubCategory.Add(wc);
                        }
                        logger.Info("End   Sub Category: ");
                        return warehouseSubCategory;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in SubCategory " + ex.Message);
                    logger.Info("End  SubCategory: ");
                    return null;
                }
            }
            return null;
        }


        [ResponseType(typeof(SubCategory))]
        [Route("")]
        [AcceptVerbs("POST")]
        public SubCategoryRes add(SubCategory item)
        {
            logger.Info("start add SubCategory: ");
            try
            {
                SubCategoryRes res = new SubCategoryRes();

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

                using (var db = new AuthContext())
                {
                    if (item.Sequence > 0)
                    {
                        var objSubcat = db.SubCategorys.FirstOrDefault(x => x.Deleted == false && x.Sequence == item.Sequence);
                        if (objSubcat != null)
                        {
                            res.msg = "Sequence is already exists in " + objSubcat.SubcategoryName;
                            res.Result = false;
                            return res;
                        }
                    }
                    db.AddSubCategory(item);
                    res.Result = true;
                    res.SubCategory = item;
                    res.msg = "Add Successfully!";
                }
                logger.Info("End  add SubCategory: ");
                //res.SubCategory = item;
                return res;
            }
            catch (Exception ex)
            {
                logger.Error("Error in add SubCategory " + ex.Message);
                logger.Info("End  add SubCategory: ");
                return null;
            }
        }

        [ResponseType(typeof(SubCategory))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public SubCategoryRes Put(SubCategory item)
        {
            try
            {
                SubCategoryRes res = new SubCategoryRes();
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
                using (var db = new AuthContext())
                {
                    if (item.Sequence > 0)
                    {
                        var objSubcat = db.SubCategorys.FirstOrDefault(x => x.Deleted == false && x.Sequence == item.Sequence && x.SubCategoryId != item.SubCategoryId);
                        if (objSubcat != null)
                        {
                            res.msg = "Sequence is already exists in " + objSubcat.SubcategoryName;
                            res.Result = false;
                            return res;
                        }
                    }
                    db.PutSubCategory(item);
                    res.msg = "Update Successfully!";
                    res.Result = true;
                    return res;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }


        [ResponseType(typeof(SubCategory))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("start delete SubCategory: ");
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
                int CompanyId = compid;
                using (var db = new AuthContext())
                {
                    db.DeleteSubCategory(id, CompanyId);
                }
                logger.Info("End  delete  SubCategory: ");
            }
            catch (Exception ex)
            {

                logger.Error("Error in delete SubCategory " + ex.Message);


            }
        }


        [ResponseType(typeof(SubCategory))]
        [Route("DeleteV7")]
        [AcceptVerbs("Delete")]
        public Boolean DeleteV7(int id)
        {
            logger.Info("start delete SubCategory: ");
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
                int CompanyId = compid;
                using (var db = new AuthContext())
                {
                    db.DeleteSubCategory(id, CompanyId);
                }
                logger.Info("End  delete  SubCategory: ");
                return true;
            }
            catch (Exception ex)
            {

                logger.Error("Error in delete SubCategory " + ex.Message);
                return false;

            }
        }

        [Route("GetSubCategory")]
        [HttpGet]
        [AllowAnonymous]
        public dynamic GetSubCateogry(int CatId)
        {
            try
            {
                using (var db = new AuthContext())
                {

                    var query = " select c.CategoryName as categoryName ,sc.SubcategoryName as SubcategoryName from SubcategoryCategoryMappings scm inner join BrandCategoryMappings bcm on " +
                               "  scm.SubCategoryMappingId =bcm.SubCategoryMappingId  inner join Categories c on scm.Categoryid = c.Categoryid inner join SubCategories sc" +
                               "   on sc.SubCategoryId = scm.SubCategoryId  where scm.Categoryid = '" + CatId + "'";


                    var list = db.Database.SqlQuery<CategoryDTOM>(query).ToList();


                    return list;
                }

            }
            catch (Exception ex)
            {
                return null;
                ex.Message.ToString();
            }

        }



        [Route("GetSubCategoryMappingID")]
        [HttpGet]

        public List<BrandMappingDTO> getmappingid()//get orders to deliver
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var list1 = (from a in context.SubcategoryCategoryMappingDb
                                 where a.Deleted == false
                                 join i in context.SubCategorys on a.SubCategoryId equals i.SubCategoryId
                                 select new BrandMappingDTO
                                 {
                                     SubCategoryMappingId = a.SubCategoryMappingId,
                                     SubcategoryName = i.SubcategoryName,
                                     SubCategoryid = i.SubCategoryId,
                                     CategoryId = i.Categoryid
                                 }).ToList();

                    return list1;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    return null;
                }
            }
        }


        [Route("GetSubCategoryByCategoryId")]
        [HttpGet]

        public dynamic GetSubCategoryByCategoryId(int catID)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var list1 = (from scc in context.SubcategoryCategoryMappingDb
                                 join sc in context.SubCategorys on scc.SubCategoryId equals sc.SubCategoryId 
                                 where scc.Categoryid == catID && sc.Deleted == false
                                 select new
                                 {
                                     SubcategoryName = sc.SubcategoryName,
                                     SubCategoryId = sc.SubCategoryId,
                                     SubCategoryMappingId = scc.SubCategoryMappingId,
                                     Code = sc.Code
                                 }).ToList();

                    return list1;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    return InternalServerError();
                }
            }
        }

        [Route("GetActiveSubCategory")]
        [HttpGet]
        public List<SubCategorysDTO> GetActiveSubCategory()
        {
            using (var context = new AuthContext())
            {
                var subCategorylst = context.SubCategorys.Where(x => x.IsActive && !x.Deleted).Select(x => new SubCategorysDTO
                {
                    SubCategoryId = x.SubCategoryId,
                    SubcategoryName = x.SubcategoryName,
                    LogoUrl = x.LogoUrl
                }).ToList();
                return subCategorylst;
            }
        }

        [Route("StopPoSubWareWise")]
        [HttpGet]
        public string StopPoSubWareWise(int WarehouseId,int SubcatId)
        {
            string res = "";
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                var data = db.SubCategoryStopPos.FirstOrDefault(x => x.WarehouseId == WarehouseId && x.SubCatId == SubcatId && x.IsActive == true && x.IsDeleted == false);
                if(data != null)
                {
                    res = "Configuration Already Exists";
                }
                else
                {
                    SubCategoryStopPo s = new SubCategoryStopPo();
                    s.WarehouseId = WarehouseId;
                    s.SubCatId = SubcatId;
                    s.IsActive = true;
                    s.IsDeleted = false;
                    s.StopPo = true;
                    s.CreatedBy = userid;
                    s.CreatedDate = DateTime.Now;
                    db.SubCategoryStopPos.Add(s);
                    if(db.Commit() > 0)
                    {
                        res = "Configuration Add Successfully";
                    }
                    else
                    {
                        res = "Something Went wrong";
                    }
                }
            }
            return res;
        }


        [Route("EditStopPoSubWareWise")]
        [HttpGet]
        public string EditStopPoSubWareWise(long Id,bool StopPo)
        {
            string res = "";
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                var data = db.SubCategoryStopPos.FirstOrDefault(x => x.Id == Id && x.IsActive == true && x.IsDeleted == false);
                if(data != null)
                {
                    data.ModifiedDate = DateTime.Now;
                    data.ModifiedBy = userid;
                    data.StopPo = StopPo;
                    db.Entry(data).State = System.Data.Entity.EntityState.Modified;
                    if(db.Commit() > 0)
                    {
                        res = "Edit Successfully";
                    }
                    else
                    {
                        res = "Something Went Wrong";
                    }
                }
                else
                {
                    res = "Data Not Found";
                }
            }
            return res;
        }

        [Route("GetStopPoSubWareWise")]
        [HttpGet]
        public List<WarehouseSubStopPo> GetStopPoSubWareWise(int SubcatId)
        {
            using (var db = new AuthContext())
            {
                List<WarehouseSubStopPo> res = new List<WarehouseSubStopPo>();
                var subcatid = new SqlParameter("@SubcatId", SubcatId);
                res = db.Database.SqlQuery<WarehouseSubStopPo>("exec GetStopPoSubWareWise @SubcatId", subcatid).ToList();
                return res;
            }
        }
        public class CategoryDTOM
        {
            public int Categoryid { get; set; }
            public int SubsubCategoryid { get; set; } //brand Id
            public string categoryName { get; set; }
            public string SubcategoryName { get; set; }

            public DateTime CreatedDate { get; set; }
            public DateTime UpdatedDate { get; set; }

            public bool Deleted { get; set; }
            public bool IsActive { get; set; }
        }

        public class BrandMappingDTO
        {

            public int SubCategoryid { get; set; } //brand Id
            public int SubCategoryMappingId { get; set; }
            public int CategoryId { get; set; }
            public string SubcategoryName { get; set; }



        }

        public class ItemSchemeSubCategorys{
            public int SubCategoryId { get; set; }
            public string SubcategoryName { get; set; }

        }
        public class SubCategorysDTO
        {

            public string GroupedCategoryName { get; set; }
            public int CategoryId { get; set; }
            public string SubcategoryName { get; set; }
            public string CategoryName { get; set; }
            public string LogoUrl { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedDate { get; set; }
            public string HindiName { get; set; }
            public string Discription { get; set; }

            public bool Deleted { get; set; }
            public int SubCategoryId { get; set; }
            public string Code { get; set; }
            public int StoreType { get; set; }
            public string StoreTypeName { get; set; }
            public string StoreImage { get; set; }
            public int Sequence { get; set; }
            public string StoreBanner { get; set; }
            public bool AllowInactiveOrderToPending { get; set; }
        }

        public class WarehouseSubStopPo
        {
            public long Id { get; set; }
            public bool StopPo { get;set; }
            public string WarehouseName { get; set; }
        }

    }
}



