using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.Masters.Store;
using AngularJSAuthentication.Model.Store;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.Store
{
    [RoutePrefix("api/Store")]
    public class StoreController : BaseApiController
    {
        [Route("GetCategoryList")]
        public List<SelectItemDc> GetCategoryList()
        {
            using (var context = new AuthContext())
            {
                var catList = context.Categorys.Where(x => x.IsActive == true && x.Deleted == false)
                    .Select(y => new SelectItemDc
                    {
                        value = y.Categoryid,
                        label = y.CategoryName
                    }).ToList();
                return catList;
            }
        }


        [Route("GetSubCategoryList/{categoryId}")]
        public List<SelectItemDc> GetSubCategoryList(int categoryId)
        {
            using (var context = new AuthContext())
            {
                var idParam = new SqlParameter
                {
                    ParameterName = "CategoryId",
                    Value = categoryId
                };

                var list = context.Database.SqlQuery<SelectItemDc>("exec Store_GetSubCategory @CategoryId", idParam).ToList();
                return list;
            }
        }

        [Route("GetBrand/{subCategoryIdString}")]
        public List<SelectItemDc> GetBrand(string subCategoryIdString)
        {
            using (var context = new AuthContext())
            {
                var idParam = new SqlParameter
                {
                    ParameterName = "SubCategoryMappingIdList",
                    Value = subCategoryIdString
                };

                var list = context.Database.SqlQuery<SelectItemDc>("exec Store_GetBrand @SubCategoryMappingIdList", idParam).ToList();
                return list;
            }
        }
        [Route("GetAllStore")]
        [HttpGet]
        public dynamic GetAllStores()
        {
            using (var context = new AuthContext())
            {
                var results = context.StoreDB.Where(x => x.IsDeleted == false).ToList();
                return results;
            }
        }



        [Route("GetBrandToDisplay")]
        [HttpPost]
        public List<StoreBrandDc> GetBrandToDisplay(PostBrandToDisplayDc brandCategoryMappingIdList)
        {
            using (var context = new AuthContext())
            {
                var idParam = new SqlParameter
                {
                    ParameterName = "BrandCategoryMappingIdList",
                    Value = brandCategoryMappingIdList.brandCategoryMappingIdList
                };

                var storeParam = new SqlParameter
                {
                    ParameterName = "StoreId",
                    Value = DBNull.Value
                };

                var list = context.Database.SqlQuery<StoreBrandDc>("exec Store_GetBrandToDisplay @BrandCategoryMappingIdList, @StoreId", idParam, storeParam).ToList();
                return list;
            }
        }


        [Route("GetBrandUsingStoreId/{storeId}")]
        public List<StoreBrandDc> GetBrandToDisplay(long storeId)
        {
            StoreHelper storeHelper = new StoreHelper();
            List<StoreBrandDc> brandList = storeHelper.GetBrandToDisplay(storeId);
            return brandList;
        }

        [Route("SaveStore")]
        [HttpPost]
        public long SaveStore(StoreViewModel store)
        {
            long storeId = 0;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var context = new AuthContext())
                {
                    int userId = GetLoginUserId();
                    DateTime time = DateTime.Now;
                    StoreHelper storeHelper = new StoreHelper();
                    storeId = storeHelper.SaveStore(store, context, userId, time);
                    storeHelper.UpdateBrandList(store.BrandList, context, userId, time, storeId);
                }
                scope.Complete();
            }
            if (storeId > 0)
            {
#if !DEBUG
            Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
            _cacheProvider.Remove(Caching.CacheKeyHelper.StoreCacheKey());
#endif
            }

            return storeId;
        }

        [Route("GetStoreList")]
        [HttpGet]
        public List<StoreViewModel> GetStoreList()
        {
            using (var context = new AuthContext())
            {
                List<StoreViewModel> list = context.StoreDB.Where(x => x.IsDeleted == false && x.IsActive == true).Select(x => new StoreViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsActive = x.IsActive,
                    IsDeleted = x.IsDeleted.HasValue ? x.IsDeleted.Value : false,
                    ImagePath = x.ImagePath
                }).ToList();
                return list;
            }
        }

        [Route("GetStoreById/{storeId}")]
        [HttpGet]
        public StoreViewModel GetStoreById(long storeId)
        {
            using (var context = new AuthContext())
            {
                StoreViewModel storeVM = context.StoreDB.Where(x => x.Id == storeId).Select(y => new StoreViewModel
                {
                    Id = y.Id,
                    ImagePath = y.ImagePath,
                    IsActive = y.IsActive,
                    IsDeleted = y.IsDeleted.HasValue ? y.IsDeleted.Value : false,
                    Name = y.Name,
                    OwnerId = y.OwnerId,
                    IsUniversal = y.IsUniversal
                }).FirstOrDefault();
                return storeVM;
            }
        }


        [Route("GetPeopleList")]
        [HttpGet]
        public List<StoreOwnerDc> GetPeople(string keyword)
        {
            using (var context = new AuthContext())
            {
                var Searchkeyword = new SqlParameter("@keyword", keyword);
                var list = context.Database.SqlQuery<StoreOwnerDc>("GetRoleStoreOwner @keyword", Searchkeyword).ToList();
                //List<StoreOwnerDc> list = context.Peoples.Where(x =>
                //    (!string.IsNullOrEmpty(x.Email) && x.Email.Contains(keyword))
                //  || (!string.IsNullOrEmpty(x.DisplayName) && x.DisplayName.Contains(keyword))
                //  || (!string.IsNullOrEmpty(x.UserName) && x.UserName.Contains(keyword))
                //).Select(y => new StoreOwnerDc
                //{
                //    DisplayName = y.DisplayName,
                //    UserName = y.UserName,
                //    Email = y.Email,
                //    PeopleId = y.PeopleID
                //}).Take(50).ToList();
                return list;
            }
        }

        [Route("GetOwnerById/{peopleId}")]
        [HttpGet]
        public StoreOwnerDc GetOwnerById(int peopleId)
        {
            using (var context = new AuthContext())
            {
                StoreOwnerDc owner = context.Peoples.Where(x => x.PeopleID == peopleId)
                .Select(y => new StoreOwnerDc
                {
                    DisplayName = y.DisplayName,
                    UserName = y.UserName,
                    Email = y.Email,
                    PeopleId = y.PeopleID
                }).FirstOrDefault();
                return owner;
            }
        }
        [Route("GetWarehouseWiseSaleLead")]
        [HttpGet]
        public List<WarehouseWiseSaleLeadDC> GetWarehouseWiseSaleLead(int warehouseId)
        {
            using (var context = new AuthContext())
            {
                var warehouseIds = new SqlParameter("@warehouseId", warehouseId);
                var list = context.Database.SqlQuery<WarehouseWiseSaleLeadDC>("GetWarehouseWiseSaleLead @warehouseId", warehouseIds).ToList();
                return list;
            }
        }
        [Route("GetWarehouseStoreMapping")]
        [HttpGet]
        public List<GetWarehouseWiseSaleLeadDC> GetWarehouseStoreMapping(int StoreId)
        {
            List<GetWarehouseWiseSaleLeadDC> List = new List<GetWarehouseWiseSaleLeadDC>();
            using (var db = new AuthContext())
            {
                var warehouseStoreMapping = db.WarehouseStoreMappings.Where(x => x.StoreId == StoreId && x.IsActive == true && x.IsDeleted == false).ToList();
                if (warehouseStoreMapping != null && warehouseStoreMapping.Any())
                {
                    List = Mapper.Map(warehouseStoreMapping).ToANew<List<GetWarehouseWiseSaleLeadDC>>();
                    var WarehouseIds = warehouseStoreMapping.Select(x => x.WarehouseId).ToList();
                    var SalesLeadIds = warehouseStoreMapping.Select(x => x.SalesLeadId).ToList();
                    var Warehouse = db.Warehouses.Where(x => WarehouseIds.Contains(x.WarehouseId)).Select(x => new { x.WarehouseId, x.WarehouseName }).ToList();
                    var people = db.Peoples.Where(x => SalesLeadIds.Contains(x.PeopleID)).ToList();
                    var store = db.StoreDB.Where(x => x.Id == StoreId).FirstOrDefault();

                    List.ForEach(x =>
                    {
                        x.StoreName = store.Name;
                        x.WarehouseName = Warehouse.Where(y => y.WarehouseId == x.WarehouseId).FirstOrDefault().WarehouseName;
                        x.SalesLeadName = people.Where(y => y.PeopleID == x.SalesLeadId).FirstOrDefault().DisplayName;
                    });
                }
                return List;
            }
        }

        [Route("PostWarehouseStoreMapping")]
        [HttpPost]
        public HttpResponseMessage PostWarehouseStoreMapping(List<PostStoreDc> postStoreDc)
        {
            PostWarehouseWiseSaleLeadDC res;
            List<WarehouseStoreMapping> List = new List<WarehouseStoreMapping>();
            int userId = GetLoginUserId();
            if (postStoreDc != null && postStoreDc.Any(x => x.SalesLeadId > 0 && x.StoreId > 0 && userId>0))
            {
                using (var db = new AuthContext())
                {

                    foreach (var item in postStoreDc)
                    {
                        var warehouseStoreMapping = db.WarehouseStoreMappings.Where(x => x.StoreId == item.StoreId && x.WarehouseId == item.WarehouseId && x.SalesLeadId == item.SalesLeadId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                        if (warehouseStoreMapping == null)
                        {
                            WarehouseStoreMapping add = new WarehouseStoreMapping();
                            add.WarehouseId = item.WarehouseId;
                            add.StoreId = item.StoreId;
                            add.SalesLeadId = item.SalesLeadId;
                            add.IsActive = true;
                            add.IsDeleted = false;
                            add.CreatedDate = DateTime.Now;
                            add.CreatedBy = userId;
                            List.Add(add);
                        }
                        else
                        {
                            res = new PostWarehouseWiseSaleLeadDC()
                            {
                                warehouseStoreMapping = null,
                                Status = true,
                                Message = "Data Already Exists!!."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }

                    }
                    db.WarehouseStoreMappings.AddRange(List);
                    db.Commit();
                    res = new PostWarehouseWiseSaleLeadDC()
                    {
                        warehouseStoreMapping = List,
                        Status = true,
                        Message = "Success!"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
            else
            {
                res = new PostWarehouseWiseSaleLeadDC()
                {
                    warehouseStoreMapping = null,
                    Status = false,
                    Message = "Data not found!!"
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        [Route("PUTtWarehouseStoreMapping")]
        [HttpPut]
        public HttpResponseMessage PUTtWarehouseStoreMapping(PostStoreDc postStoreDc)
        {
            PostWarehouseWiseSaleLeadDC res;
            int userId = GetLoginUserId();
            using (var db = new AuthContext())
            {
                var warehouseStoreMapping = db.WarehouseStoreMappings.Where(x => x.Id == postStoreDc.Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                if (warehouseStoreMapping != null && postStoreDc.SalesLeadId > 0)
                {
                    warehouseStoreMapping.SalesLeadId = postStoreDc.SalesLeadId;
                    warehouseStoreMapping.IsActive = postStoreDc.Active;
                    warehouseStoreMapping.ModifiedBy = userId;
                    warehouseStoreMapping.ModifiedDate = DateTime.Now;
                    db.Entry(warehouseStoreMapping).State = EntityState.Modified;
                    db.Commit();
                    res = new PostWarehouseWiseSaleLeadDC()
                    {
                        warehouseStoreMapping = null,
                        Status = true,
                        Message = "Record Updated Successfully!"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    res = new PostWarehouseWiseSaleLeadDC()
                    {
                        warehouseStoreMapping = null,
                        Status = false,
                        Message = "Data not found!!"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
            }
        }
        [Route("DeleteWarehouseStoreMapping")]
        [HttpDelete]
        public HttpResponseMessage DeleteWarehouseStoreMapping(int Id)
        {
            PostWarehouseWiseSaleLeadDC res;
            int userId = GetLoginUserId();
            using (var db = new AuthContext())
            {
                var warehouseStoreMapping = db.WarehouseStoreMappings.Where(x => x.Id == Id).FirstOrDefault();
                if (warehouseStoreMapping != null)
                {
                    warehouseStoreMapping.IsActive = false;
                    warehouseStoreMapping.IsDeleted = true;
                    warehouseStoreMapping.ModifiedBy = userId;
                    warehouseStoreMapping.ModifiedDate = DateTime.Now;
                    db.Entry(warehouseStoreMapping).State = EntityState.Modified;
                    db.Commit();
                    res = new PostWarehouseWiseSaleLeadDC()
                    {
                        warehouseStoreMapping = null,
                        Status = true,
                        Message = "Success!"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    res = new PostWarehouseWiseSaleLeadDC()
                    {
                        warehouseStoreMapping = null,
                        Status = false,
                        Message = "Data not found!!"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
            }
        }

    }
}
