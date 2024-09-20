using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model;
using LinqKit;
using MongoDB.Bson;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/BlockBrand")]
    public class BlockBrandController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [HttpGet]
        [Route("GetBlockBrandList")]
        public ResponseBlockBrandDc BlockBrandList(int warehouseId, int CustomerType, int AppType, int skip, int take)
        {
            int Skiplist = (skip - 1) * take;
            ResponseBlockBrandDc obj = new ResponseBlockBrandDc();
            MongoDbHelper<BlockBrands> mongoDbHelper = new MongoDbHelper<BlockBrands>();

            BlockBrandDc blockBrandDcs = new BlockBrandDc();
            List<BlockBrands> BBlist = new List<BlockBrands>();
            var searchPredicate = PredicateBuilder.New<BlockBrands>(x => x.IsDeleted == false);

            if (CustomerType == 0 && AppType == 0 && warehouseId == 0)
            {
                searchPredicate = PredicateBuilder.New<BlockBrands>(x => x.IsDeleted == false);
            }
            if (CustomerType > 0 && AppType > 0 && warehouseId > 0)
            {
                searchPredicate = PredicateBuilder.New<BlockBrands>(x => x.IsDeleted == false && x.CustomerType == CustomerType && x.AppType == AppType && x.WarehouseId == warehouseId);
            }
            if (CustomerType > 0 && AppType > 0 && warehouseId == 0)
            {
                searchPredicate = PredicateBuilder.New<BlockBrands>(x => x.IsDeleted == false && x.CustomerType == CustomerType && x.AppType == AppType);
            }
            if (CustomerType == 0 && AppType == 0 && warehouseId > 0)
            {
                searchPredicate = PredicateBuilder.New<BlockBrands>(x => x.IsDeleted == false && x.WarehouseId == warehouseId);
            }
            //if ((CustomerType > 0 && AppType == 0 && warehouseId == 0) || (CustomerType == 0 && AppType > 0 && warehouseId == 0) || (CustomerType > 0 && AppType == 0 && warehouseId > 0) || (CustomerType == 0 && AppType > 0 && warehouseId > 0))
            if ((CustomerType > 0 && AppType == 0 ) || (CustomerType == 0 && AppType > 0))
            {
                searchPredicate = PredicateBuilder.New<BlockBrands>(x => x.IsDeleted == false && (x.CustomerType == CustomerType || x.AppType == AppType));
                //searchPredicate = PredicateBuilder.New<BlockBrands>(x => x.IsDeleted == false && ((x.CustomerType == CustomerType && x.AppType == 0 && x.WarehouseId == 0) || (x.CustomerType == 0 && x.AppType == AppType && x.WarehouseId == 0) || (x.CustomerType ==CustomerType && x.AppType == 0 && x.WarehouseId ==warehouseId) || (x.CustomerType == 0 && x.AppType == AppType && x.WarehouseId == warehouseId)));
            
            }
            BBlist = mongoDbHelper.Select(searchPredicate, x => x.OrderByDescending(y => y.CreatedDate), Skiplist, take).ToList();

            List<BlockBrandDc> list = new List<BlockBrandDc>();
            list = Mapper.Map(BBlist).ToANew<List<BlockBrandDc>>();
            int count = BBlist.Count();
            obj.BlockBrandDcs = list;
            obj.TotalItem = count;

            return obj;
        }
        [HttpPost]
        [Route("BlockBrandSave")]
        public ResBlockBrandSave BlockBrandSave(BlockBrands obj)
        {
            ResBlockBrandSave res = new ResBlockBrandSave
            {
                msg = "",
                Result = false
            };
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                //var chkBlockBrond = context.BlockBrands.Where(x => x.CatId == obj.CatId && x.SubCatId == obj.SubCatId && x.SubSubCatId == obj.SubSubCatId).ToList();

                MongoDbHelper<BlockBrands> mongoDbHelper = new MongoDbHelper<BlockBrands>();
                var chkBlockBrond = mongoDbHelper.Select(x => x.CatId == obj.CatId && x.SubCatId == obj.SubCatId && x.SubSubCatId == obj.SubSubCatId && x.WarehouseId==obj.WarehouseId && x.IsDeleted == false).ToList();

                if (chkBlockBrond != null && chkBlockBrond.Any())
                {
                    //if (obj.AppType == 3)
                    //{
                    //    if (chkBlockBrond.Any(x => x.CustomerType == obj.CustomerType && (x.AppType == 1 || x.AppType == 2)))
                    //    {
                    //        res.Result = false;
                    //        res.msg = "Already Saved";
                    //        return res;
                    //    }
                    //}
                    //if (chkBlockBrond.Any(x => x.CustomerType == obj.CustomerType && (x.AppType == obj.AppType || x.AppType == 3)))
                    //{
                    //    res.Result = false;
                    //    res.msg = "Already Saved";
                    //    return res;
                    //}
                    if (obj.CustomerType == 4)
                    {
                        if (chkBlockBrond.Any(x => x.AppType == obj.AppType && (x.CustomerType == 1 || x.CustomerType == 2)))
                        {
                            res.Result = false;
                            res.msg = "Already Saved";
                            return res;
                        }
                    }
                    if (chkBlockBrond.Any(x => x.AppType == obj.AppType && x.CustomerType == obj.CustomerType))
                    {
                        res.Result = false;
                        res.msg = "Already Saved";
                        return res;
                    }

                    //if (obj.CustomerType == 4)
                    //{
                    //    if (chkBlockBrond.Any(x => x.AppType == obj.AppType && (x.CustomerType == 1 || x.CustomerType == 2 || x.CustomerType == 3)))
                    //    {
                    //        res.Result = false;
                    //        res.msg = "Already Saved";
                    //        return res;
                    //    }
                    //}
                    if (chkBlockBrond.Any(x => x.AppType == obj.AppType && (x.CustomerType == obj.CustomerType || x.CustomerType == 4)))
                    {
                        res.Result = false;
                        res.msg = "Already Saved";
                        return res;
                    }


                }
                bool flag = false;
                BlockBrands BlockBranddc = new BlockBrands
                {
                    CatId = obj.CatId,
                    SubCatId = obj.SubCatId,
                    SubSubCatId = obj.SubSubCatId,
                    CustomerType = obj.CustomerType,
                    AppType = obj.AppType,
                    CategoryName = obj.CategoryName,
                    SubcategoryName = obj.SubcategoryName,
                    SubsubcategoryName = obj.SubsubcategoryName,
                    ApplicationType = obj.ApplicationType,
                    CustomerTypeName = obj.CustomerTypeName,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userid,
                    WarehouseId=obj.WarehouseId,
                    WarehouseName=obj.WarehouseName
                };
                flag = mongoDbHelper.Insert(BlockBranddc);
                if (flag)
                {
                    res.Result = true;
                    res.msg = "Block Brand save successfully.";
                }


            }
            catch (Exception ex)
            {

                //res.Result = false;
                //res.msg = "Block Brand save failed";
            }

            return res;
        }

        [HttpGet]
        [Route("BlockBrandStatusChange")]
        public ResBlockBrandSave BlockBrandStatusChange(string Id, bool status)
        {
            ResBlockBrandSave res = new ResBlockBrandSave
            {
                msg = "",
                Result = false
            };
            try
            {
                //using (var context = new AuthContext())
                {
                    MongoDbHelper<BlockBrands> mongoDbHelper = new MongoDbHelper<BlockBrands>();

                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    bool flag = false;
                    var BlockBrand = mongoDbHelper.Select(x => x.Id == ObjectId.Parse(Id)).FirstOrDefault();
                    if (BlockBrand != null)
                    {
                        BlockBrand.IsActive = status;
                        BlockBrand.IsDeleted = false;
                        BlockBrand.ModifiedBy = userid;
                        BlockBrand.ModifiedDate = DateTime.Now;
                        flag = mongoDbHelper.Replace(ObjectId.Parse(Id), BlockBrand);

                        if (flag)
                        {
                            string strstatus = status ? "Active" : "Inactive";
                            res.msg = "Block Brand " + strstatus + " successfully.";
                            return res;
                        }

                    }
                    else
                    {
                        res.Result = false;
                        res.msg = "Block Brand Item Not Found";
                    }
                }
            }
            catch (Exception ex)
            {

                res.Result = false;
                res.msg = "Failed";
            }
            return res;
        }

        [HttpGet]
        [Route("BlockBrandDelete")]
        public ResBlockBrandSave BlockBrandDelete(string Id)
        {
            ResBlockBrandSave res = new ResBlockBrandSave
            {
                msg = "",
                Result = false
            };
            try
            {
                MongoDbHelper<BlockBrands> mongoDbHelper = new MongoDbHelper<BlockBrands>();
                {
                    bool flag = false;
                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    var BlockBrand = mongoDbHelper.Select(x => x.Id == ObjectId.Parse(Id)).FirstOrDefault();

                    if (BlockBrand != null)
                    {
                        BlockBrand.IsActive = false;
                        BlockBrand.IsDeleted = true;
                        BlockBrand.ModifiedBy = userid;
                        BlockBrand.ModifiedDate = DateTime.Now;
                        flag = mongoDbHelper.Replace(ObjectId.Parse(Id), BlockBrand);

                        if (flag)
                        {
                            res.msg = "Block Brand Item Deleted";
                        }
                    }
                }
            }
            catch (Exception)
            {
                res.msg = "Failed";
                res.Result = false;
            }
            return res;
        }

        [HttpGet]
        [Route("GetSubSubCategoryBySubCategoryId")]
        public dynamic GetSubSubCategory(int Categoryid, int SubCategoryId)
        {

            try
            {
                using (var Context = new AuthContext())
                {
                    var param = new SqlParameter("@Categoryid", Categoryid);
                    var param2 = new SqlParameter("@SubCategoryId", SubCategoryId);
                    var List = Context.Database.SqlQuery<SubSubCatDc>("Exec SPSubSubCategory @Categoryid, @SubCategoryId", param, param2).ToList();
                    return List;

                }
            }
            catch (Exception ex)
            {
                ///logger.Error(ex.Message);
                return InternalServerError();
            }

        }
        [HttpGet]
        [Route("ExportBlockBrandList")]
        public List<BlockBrandData> ExportBlockBrandList()
        {
                ResponseBlockBrandDc obj = new ResponseBlockBrandDc();
                MongoDbHelper<BlockBrands> mongoDbHelper = new MongoDbHelper<BlockBrands>();

                BlockBrandDc blockBrandDcs = new BlockBrandDc();
                List<BlockBrands> BBlist = new List<BlockBrands>();
                var searchPredicate = PredicateBuilder.New<BlockBrands>(x => x.IsDeleted == false);

                BBlist = mongoDbHelper.Select(searchPredicate, x => x.OrderByDescending(y => y.CreatedDate), null, null).ToList();

                List<BlockBrandData> list = new List<BlockBrandData>();
                list = Mapper.Map(BBlist).ToANew<List<BlockBrandData>>();
                return list;
        }
    }
}
public class SubSubCatDc
{
    public int SubCategoryId { get; set; }
    public int SubsubCategoryid { get; set; }
    public string SubsubcategoryName { get; set; }
    public int SubCategoryMappingId { get; set; }

}