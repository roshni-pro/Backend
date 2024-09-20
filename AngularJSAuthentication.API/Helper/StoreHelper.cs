using AngularJSAuthentication.DataContracts.Masters.Store;
using AngularJSAuthentication.Model.SalesApp;
using AngularJSAuthentication.Model.Store;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class StoreHelper
    {
        public long SaveStore(StoreViewModel storeViewModel, AuthContext context, int userId, DateTime time)
        {

            if(storeViewModel.Id > 0)
            {
                Store store = context.StoreDB.FirstOrDefault(x => x.Id == storeViewModel.Id);
                store.Name = storeViewModel.Name;
                store.OwnerId = storeViewModel.OwnerId;
                store.IsUniversal = storeViewModel.IsUniversal;
                context.Commit();
                return store.Id;
            }
            else
            {
                Store store = new Store
                {
                    CreatedBy = userId,
                    CreatedDate = time,
                    //ImagePath = storeViewModel.ImagePath,
                    IsActive = true,
                    IsDeleted = false,
                    ModifiedBy = userId,
                    ModifiedDate = time,
                    Name = storeViewModel.Name,
                    OwnerId = storeViewModel.OwnerId,
                    IsUniversal = storeViewModel.IsUniversal
                };
                context.StoreDB.Add(store);
                context.Commit();
                BeatEditConfig beatEditConfig = new BeatEditConfig
                {
                    IsBeatEdit = false,
                    CreatedDate = time,
                    CreatedBy = userId,
                    IsActive = true,
                    IsDeleted = false,
                    StartDate = DateTime.Now,
                    StoreId = (int)store.Id,
                    IsAnytime = true,
                    FromDate = 0,
                    ToDate = 0
                };
                context.BeatEditConfigs.Add(beatEditConfig);
                context.Commit();
                return store.Id;
            }
        }
        public void UpdateBrandList(List<StoreBrandViewModel> brandList, AuthContext context, int userId, DateTime time, long storeId)
        {
            if(brandList != null && brandList.Any())
            {
                foreach (var brandVM in brandList)
                {
                    if(brandVM.Id > 0 && brandVM.IsDeleted == true)
                    {
                        var brand = context.StoreBrandDB.FirstOrDefault(x => x.Id == brandVM.Id);
                        brand.IsActive = false;
                        brand.IsDeleted = true;
                    }
                    else if(brandVM.Id == 0)
                    {
                        StoreBrand brand = new StoreBrand
                        {
                            BrandCategoryMappingId = brandVM.BrandCategoryMappingId,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedBy = userId,
                            CreatedDate = time,
                            StoreId = storeId
                        };
                        context.StoreBrandDB.Add(brand);
                    }
                }
                context.Commit();
            }
        }

        public List<StoreBrandDc> GetBrandToDisplay(long storeId)
        {
            using (var context = new AuthContext())
            {
                var mappingParam = new SqlParameter
                {
                    ParameterName = "BrandCategoryMappingIdList",
                    Value = DBNull.Value
                };

                var storeParam = new SqlParameter
                {
                    ParameterName = "StoreId",
                    Value = storeId
                };

                var list = context.Database.SqlQuery<StoreBrandDc>("exec Store_GetBrandToDisplay @BrandCategoryMappingIdList, @StoreId", mappingParam, storeParam).ToList();
                return list;
            }
        }

    }
}