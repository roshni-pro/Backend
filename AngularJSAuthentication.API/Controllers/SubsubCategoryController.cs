using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.DataContracts.JustInTime;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/SubsubCategory")]
    public class SubsubCategoryController : BaseAuthController
    {
       
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Authorize]
        [Route("")]
        public IEnumerable<SubsubCategory> Get()
        {
            logger.Info("start Subsubategory: ");
            List<SubsubCategory> ass = new List<SubsubCategory>();
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
                using (var db = new AuthContext())
                {
                    ass = db.AllSubsubCat(compid).ToList();
                }
                logger.Info("End  Subsubategory: ");
                return ass;
            }
            catch (Exception ex)
            {
                logger.Error("Error in Subsubategory " + ex.Message);
                logger.Info("End  Subsubategory: ");
                return null;
            }
        }


        [Authorize]
        [Route("Allsubsubcategory")]
        public dynamic Getsubsubcategory()
        {
            try
            {
                using (var db = new AuthContext())
                {
                    var Allsubsubcategory = db.Database.SqlQuery<SubsubCategoryDTO>("GetSubSubCategory").OrderByDescending(x=>x.Code).ToList();
                    return Allsubsubcategory;
                }


            }
            catch (Exception ex)
            {
                logger.Error("Error in Subsubategory " + ex.Message);
                logger.Info("End  Subsubategory: ");
                return null;
            }
        }
        [Authorize]
        [Route("GetSubSubCategoryByWid")]
        public IEnumerable<SubsubCategory> GetSubSubCategoryByWid(int warehouseId)
        {
            logger.Info("start Subsubategory: ");
            List<SubsubCategory> ass = new List<SubsubCategory>();
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
                using (var db = new AuthContext())
                {
                    ass = db.subsubcategorybyWarehouse(warehouseId, compid).ToList();
                    logger.Info("End  Subsubategory: ");
                    return ass;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Subsubategory " + ex.Message);
                logger.Info("End  Subsubategory: ");
                return null;
            }
        }

        [HttpGet]
        [Route("GenerateSubSubCode")]
        public dynamic GenerateSubSubCode()
        {
            logger.Info("start Subsubategory: ");
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
                using (var db = new AuthContext())
                {
                    var ssc = db.GenerateSubSubCode(compid);
                    var atm = Int32.Parse(ssc);
                    logger.Info("End  Subsubategory: ");
                    return atm;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Subsubategory " + ex.Message);
                logger.Info("End  Subsubategory: ");
                return null;
            }
        }


        [ResponseType(typeof(SubsubCategory))]
        [Route("")]
        [AcceptVerbs("POST")]
        public SubsubCategory add(SubsubCategory item)
        {
            logger.Info("start Subsubategory: ");
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
                using (var context = new AuthContext())
                {
                    context.AddSubsubCat(item);
                }
                logger.Info("End  Subsubategory: ");
                return item;
            }
            catch (Exception ex)
            {
                logger.Error("Error in Subsubategory " + ex.Message);
                logger.Info("End  Subsubategory: ");
                return null;
            }
        }

        [ResponseType(typeof(SubsubCategory))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public SubsubCategory Put(SubsubCategory item)
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
                using (var context = new AuthContext())
                {
                    return context.PutSubsubCat(item);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        [ResponseType(typeof(SubsubCategory))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("start Subsubategory: ");
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
                using (var context = new AuthContext())
                {
                    context.DeleteSubsubCat(id, CompanyId);
                }
                logger.Info("End  delete Subsubategory: ");
            }
            catch (Exception ex)
            {

                logger.Error("Error in Subsubategory " + ex.Message);


            }
        }



        [ResponseType(typeof(SubsubCategory))]
        [Route("DeleteV7")]
        [AcceptVerbs("Delete")]
        public Boolean DeleteV7(int id)
        {
            logger.Info("start Subsubategory: ");
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
                using (var context = new AuthContext())
                {
                    context.DeleteSubsubCat(id, CompanyId);
                }
                logger.Info("End  delete Subsubategory: ");
                return true;
            }
            catch (Exception ex)
            {

                logger.Error("Error in Subsubategory " + ex.Message);
                return false;

            }
        }


        //for Brand All 
        [Route("GetAllBrand")]
        public dynamic GetBrand()
        {
            logger.Info("start Subsubategory: ");
            List<SubsubCategory> ass = new List<SubsubCategory>();
            try
            {
                using (var db = new AuthContext())
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
                    List<string> BrandName = new List<string>();
                    var SubBrand = db.SubsubCategorys.Where(p => p.Deleted == false && p.IsActive == true).ToList();

                    if (SubBrand != null)
                    {
                        foreach (var subdate in SubBrand)
                        {

                            if (subdate != null && !BrandName.Any(x => x == subdate.SubsubcategoryName))
                            {
                                ass.Add(subdate);
                                BrandName.Add(subdate.SubsubcategoryName);
                            }
                        }
                    }
                    logger.Info("End  Subsubategory: ");
                    return ass;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Subsubategory " + ex.Message);
                logger.Info("End  Subsubategory: ");
                return null;
            }
        }

        /// <summary>
        ///for Brand All base by WarehouseId
        /// </summary>
        /// <param name="WarehouseId"></param>       
        /// <returns>SubsubCategoryDTO</returns>
        [Route("GetAllBrand/V2")]
        public dynamic GetBrandWarehouseId(int WarehouseId)
        {
            logger.Info("start Subsubategory: ");
            List<SubsubCategoryDTOM> ass = new List<SubsubCategoryDTOM>();
            try
            {
                using (var db = new AuthContext())
                {
                    string query = "select b.SubsubCategoryid,b.SubsubcategoryName,b.LogoUrl,b.HindiName,b.Categoryid,b.SubCategoryId,b.SubcategoryName from ItemMasters a with(nolock) inner join SubsubCategories b with(nolock) on a.SubsubCategoryid=b.SubsubCategoryid " +
                                  "and a.Deleted = 0  and a.active = 1  and a.WarehouseId = " + WarehouseId + "and b.Deleted =0 and b.IsActive =1 group by b.SubsubCategoryid,b.SubsubcategoryName,b.LogoUrl,b.HindiName,b.Categoryid,b.SubCategoryId,b.SubcategoryName ";
                    ass = db.Database.SqlQuery<SubsubCategoryDTOM>(query).ToList();
                    //List<string> BrandName = new List<string>();
                    ////var SubBrand = db.SubsubCategorys.Where(p => p.Deleted == false && p.IsActive == true).ToList();
                    //var SubBrand = db.itemMasters.Where(p => p.Deleted == false && p.active == true && p.WarehouseId == WarehouseId).ToList();

                    //if (SubBrand != null)
                    //{
                    //    foreach (var subdate in SubBrand)
                    //    {

                    //        if (subdate != null && !BrandName.Any(x => x == subdate.SubsubcategoryName))
                    //        {
                    //            var Data = db.SubsubCategorys.Where(p => p.Deleted == false && p.IsActive == true && p.SubsubCategoryid == subdate.SubsubCategoryid).FirstOrDefault();
                    //            try
                    //            {
                    //                SubsubCategoryDTOM item = new SubsubCategoryDTOM();
                    //                item.SubsubCategoryid = Data.SubsubCategoryid;
                    //                item.SubsubcategoryName = Data.SubsubcategoryName;
                    //                item.LogoUrl = Data.LogoUrl;
                    //                item.HindiName = Data.HindiName;
                    //                item.Categoryid = Data.Categoryid;
                    //                item.SubCategoryId = Data.SubCategoryId;
                    //                item.SubcategoryName = Data.SubcategoryName;
                    //                ass.Add(item);
                    //                BrandName.Add(subdate.SubsubcategoryName);
                    //            }
                    //            catch (Exception es) { }

                    //        }
                    //    }
                    //}
                    //logger.Info("End  Subsubategory: ");
                    return ass;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Subsubategory " + ex.Message);
                logger.Info("End  Subsubategory: ");
                return null;
            }
        }

        [Route("subsubCategorys")]
        [HttpGet]
        [AllowAnonymous]
        public dynamic GetSubsubCategorys(int submapid)
        {
            try
            {
                using (var db = new AuthContext())
                {
                    //int subcatid,
                    //var Getsubsubcat = db.SubsubCategorys.Where(x => x.SubCategoryId == subcatid).Select(x => new { x.SubsubCategoryid, x.SubsubcategoryName }).ToList();
                    var getmappid = db.SubcategoryCategoryMappingDb.Where(x => x.SubCategoryMappingId == submapid).Select(x => new { x.SubCategoryMappingId }).ToList();
                    return getmappid;
                }

            }
            catch (Exception ex)
            {
                return null;
                ex.Message.ToString();

            }

        }
        #region Get Active all sub sub category
        /// <summary>
        /// Created Date 22/08/2019
        /// Created By Raj
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("activeSubSub")]
        public IEnumerable<SubsubCategoryDTOM> GetactiveSubSub()
        {
            logger.Info("start Subsubategory: ");
            List<SubsubCategoryDTOM> ass = new List<SubsubCategoryDTOM>();
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
                using (var db = new AuthContext())
                {
                    string query = "select distinct e.CategoryName, d.SubcategoryName, a.SubsubcategoryName,  b.BrandCategoryMappingId SubsubCategoryid, a.LogoUrl" +
                                  " from SubsubCategories a inner join BrandCategoryMappings b on a.SubsubCategoryid = b.SubsubCategoryId " +
                                  " inner join SubcategoryCategoryMappings c on c.SubCategoryMappingId = b.SubCategoryMappingId inner " +
                                  " join SubCategories d on d.SubCategoryId = c.SubCategoryId inner join Categories e on e.Categoryid = c.Categoryid" +
                                  " where a.IsActive = 1 and b.IsActive = 1 and a.Deleted = 0 and b.Deleted = 0 and d.IsActive = 1 and d.Deleted = 0 and c.IsActive = 1 and c.Deleted = 0 and e.IsActive = 1 and e.Deleted = 0 ";
                    ass = db.Database.SqlQuery<SubsubCategoryDTOM>(query).ToList();
                    //ass = db.SubsubCategorys.Where(x=>x.Deleted==false && x.IsActive==true).ToList();
                }
                logger.Info("End  Subsubategory: ");
                return ass;
            }
            catch (Exception ex)
            {
                logger.Error("Error in Subsubategory " + ex.Message);
                logger.Info("End  Subsubategory: ");
                return null;
            }
        }
        #endregion
        [Route("GetSubSubCategoryBySubCategoryId")]
        [HttpGet]
        public dynamic GetSubSubCategoryBySubCategoryId(int subCatID)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var list1 = (from ssc in context.SubsubCategorys
                                 join sc in context.BrandCategoryMappingDb on ssc.SubsubCategoryid equals sc.SubsubCategoryId
                                 where sc.SubCategoryMappingId == subCatID && ssc.Deleted==false
                                 select new
                                 {
                                     SubsubcategoryName = ssc.SubsubcategoryName,
                                     SubsubCategoryid = ssc.SubsubCategoryid,
                                     Code = ssc.Code
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


        [Route("GetBrandBySubCategoryId")]
        [HttpGet]
        public dynamic GetBrandBySubCategoryId(int subCatID)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var list1 = (from ssc in context.SubsubCategorys
                                 join sc in context.BrandCategoryMappingDb on ssc.SubsubCategoryid equals sc.SubsubCategoryId
                                 join subcate in context.SubcategoryCategoryMappingDb on sc.SubCategoryMappingId equals subcate.SubCategoryMappingId
                                 where subcate.SubCategoryId == subCatID && ssc.IsActive == true && ssc.Deleted == false && sc.IsActive == true && sc.Deleted == false && subcate.Deleted == false && subcate.IsActive == true
                                 select new
                                 {
                                     SubsubcategoryName = ssc.SubsubcategoryName,
                                     SubsubCategoryid = ssc.SubsubCategoryid,
                                     Code = ssc.Code
                                 }).Distinct().ToList();

                    return list1;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    return InternalServerError();
                }
            }
        }

        [Route("GetItemByBrandIds")]
        [HttpPost]
        public dynamic GetItemByBrandIds(List<int> brandIds, int cityId)
        {
            using (AuthContext context = new AuthContext())
            {

                var itemList =
                (from i in context.itemMasters
                 join w in context.Warehouses on i.WarehouseId equals w.WarehouseId
                 where w.Cityid == cityId && brandIds.Contains(i.SubsubCategoryid)
                 group i by i.Number into g
                 select new
                 {
                     SellingSku = g.Key,
                     ItemName = g.FirstOrDefault().itemname
                 }).Distinct().ToList();

                return itemList;

            }
        }

        [Route("GetFreeItemsByBrandIds")]
        [HttpPost]
        public dynamic GetItemByBrandIds(List<int> brandIds, int cityId, bool IsFreeItem)
        {
            using (AuthContext context = new AuthContext())
            {
                if (!IsFreeItem)
                {
                    var itemList =
                    (from i in context.itemMasters
                     join w in context.Warehouses on i.WarehouseId equals w.WarehouseId
                     where w.Cityid == cityId && brandIds.Contains(i.SubsubCategoryid)
                     select new
                     {
                         ItemId = 0,
                         SellingSku = i.SellingSku,
                         ItemName = i.itemname
                     }).Distinct().ToList();

                    return itemList;
                }
                else
                {
                    var itemList = context.RewardItemsDb.Where(x => x.IsActive && !x.IsDeleted).Select(x =>
                          new
                          {
                              ItemId = x.rItemId,
                              SellingSku = "",
                              ItemName = x.rName
                          }
                    ).ToList();

                    return itemList;
                }
            }
        }

        [Route("GetallBrandA7")]
        [HttpGet]
        public dynamic GetallBrand()
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var list1 = (from ssc in context.SubsubCategorys
                                 join sc in context.BrandCategoryMappingDb on ssc.SubsubCategoryid equals sc.SubsubCategoryId
                                 join subcate in context.SubcategoryCategoryMappingDb on sc.SubCategoryMappingId equals subcate.SubCategoryMappingId
                                 where ssc.IsActive == true && ssc.Deleted == false && sc.IsActive == true && sc.Deleted == false && subcate.Deleted == false && subcate.IsActive == true
                                 select new
                                 {
                                     SubsubcategoryName = ssc.SubsubcategoryName,
                                     SubsubCategoryid = ssc.SubsubCategoryid,
                                     Code = ssc.Code
                                 }).Distinct().ToList().OrderBy(x => x.SubsubcategoryName);

                    return list1;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    return InternalServerError();
                }
            }
        }

        [Route("GetBrandidbyitem")]
        [HttpGet]
        public dynamic GetBrandidbyitem(int SubsubCategoryid)
        {
            List<BranditemmasterDTO> itemList = new List<BranditemmasterDTO>();
            using (AuthContext context = new AuthContext())
            {
                var query = "select distinct ItemMultiMRPId,max(itemname) as itemname from  ItemMasters where SubsubCategoryid= " + SubsubCategoryid + " group by ItemMultiMRPId ";

                itemList = context.Database.SqlQuery<BranditemmasterDTO>(query).ToList();
            }
            return itemList;
        }
        [Route("SPI")]
        [HttpGet]
        [AllowAnonymous]
        public dynamic SPI(int brandid, int itemmultimrpid)
        {
            using (var context = new AuthContext())
            {
                var Brandid = new SqlParameter("@brandid", brandid);
                var Itemmultimrpid = new SqlParameter("@itemmultimrpid", itemmultimrpid);

                var results = context.Database.SqlQuery<SPIDC>("SPI @brandid,@itemmultimrpid", Brandid, Itemmultimrpid).ToList();

                var warehouseNames = results.Select(x => x.WarehouseName).Distinct().ToList();

                DataTable dt = new DataTable();
                dt.Columns.Add("ItemMultiMrpId");
                dt.Columns.Add("ItemName");

                foreach (var item in warehouseNames)
                {
                    dt.Columns.Add(item + " (SP)");
                }

                foreach (var item in warehouseNames)
                {
                    dt.Columns.Add(item + " (SPI)");
                }

                foreach (var item in results.GroupBy(x => new { x.itemmultimrpid, x.itemname }))
                {
                    DataRow dr = dt.NewRow();
                    dr["ItemMultiMrpId"] = item.Key.itemmultimrpid;
                    dr["ItemName"] = item.Key.itemname;

                    var whs = item.Select(x => x.WarehouseName).Distinct().ToList();
                    foreach (var wh in whs)
                    {
                        var whRow = item.FirstOrDefault(x => x.WarehouseName == wh);
                        if (whRow != null)
                        {
                            dr[wh + " (SP)"] = whRow.SP;
                            dr[wh + " (SPI)"] = whRow.SPI;
                        }
                        else
                        {
                            dr[wh + " (SP)"] =0;
                            dr[wh + " (SPI)"] = 0;
                        }
                    }

                    dt.Rows.Add(dr);
                }

                return dt;
            }
        }
        [Route("PPI")]
        [HttpGet]
        [AllowAnonymous]
        public dynamic PPI(int brandid, int itemmultimrpid)
        {
            using (var context = new AuthContext())
            {
                var Brandid = new SqlParameter("@brandid", brandid);
                var Itemmultimrpid = new SqlParameter("@itemmultimrpid", itemmultimrpid);

                var results = context.Database.SqlQuery<PPIDC>("PPI @brandid,@itemmultimrpid", Brandid, Itemmultimrpid).ToList();

                var warehouseNames = results.Select(x => x.WarehouseName).Distinct().ToList();

                DataTable dt = new DataTable();
                dt.Columns.Add("ItemMultiMrpId");
                dt.Columns.Add("ItemName");

                foreach (var item in warehouseNames)
                {
                    dt.Columns.Add(item + " (APP)");
                }

                foreach (var item in warehouseNames)
                {
                    dt.Columns.Add(item + " (PPI)");
                }

                foreach (var item in results.GroupBy(x => new { x.itemmultimrpid, x.itemname }))
                {
                    DataRow dr = dt.NewRow();
                    dr["ItemMultiMrpId"] = item.Key.itemmultimrpid;
                    dr["ItemName"] = item.Key.itemname;

                    var whs = item.Select(x => x.WarehouseName).Distinct().ToList();
                    foreach (var wh in whs)
                    {
                        var whRow = item.FirstOrDefault(x => x.WarehouseName == wh);
                        if (whRow != null)
                        {
                            dr[wh + " (APP)"] = whRow.App;
                            dr[wh + " (PPI)"] = whRow.PPI;
                        }
                        else
                        {
                            dr[wh + " (APP)"] = 0;
                            dr[wh + " (PPI)"] = 0;
                        }
                    }

                    dt.Rows.Add(dr);
                }

                return dt;
            }

        }

        [Route("GetAllBrandList")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<BrandListDc>> GetAllBrandList()
        {
            using (var myContext = new AuthContext())
            {
                var result = myContext.Database.SqlQuery<BrandListDc>("GetAllBrandListSP").ToList();
                return result;
            }
        }

    }



    public class SubSubCategoryData
    {
        public IEnumerable<string> subsubcategoryname
        {
            get; set;
        }
    }
    public class BranditemmasterDTO
    {
        public string itemname { get; set; }
        public int ItemMultiMRPId { get; set; }


    }
    public class SPIDC
    {
        public int itemmultimrpid { get; set; }
        public string itemname { get; set; }
        public string WarehouseName { get; set; }
        public int warehouseid { get; set; }
        public double SP { get; set; }
        public double minSP { get; set; }
        public double SPI { get; set; }
        public bool color1 { get; set; }
        public bool color2 { get; set; }
        public bool color3 { get; set; }
        public bool color4 { get; set; }
        public bool color5 { get; set; }
        public bool color6 { get; set; }
    }
    public class PPIDC
    {
        public int itemmultimrpid { get; set; }
        public string itemname { get; set; }
        public string WarehouseName { get; set; }
        public int warehouseid { get; set; }
        public double App { get; set; }
        public double minAPp { get; set; }
        public double PPI { get; set; }
        public bool color1 { get; set; }
        public bool color2 { get; set; }
        public bool color3 { get; set; }
        public bool color4 { get; set; }
        public bool color5 { get; set; }
        public bool color6 { get; set; }
    }
    public class SubsubCategoryDTOM
    {
        public int SubsubCategoryid { get; set; }
        public string SubsubcategoryName { get; set; }
        public string LogoUrl { get; set; }
        public string HindiName { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public string SubcategoryName { get; set; }
        public string CategoryName { get; set; }
        public int? BaseCategoryId { get; set; }
    }
    public class SubsubCategoryDTO
    {
        public string CategoryAndSubCategory { get; set; }
        public int SubsubCategoryid { get; set; }
        public string SubsubcategoryName { get; set; }
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
        public bool? IsExclusive { get; set; }
        public int itemcount { get; set; }
        public double? AgentCommisionPercent { get; set; }

    }
}





