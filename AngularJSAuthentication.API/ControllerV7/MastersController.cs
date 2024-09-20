using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.External;
using AngularJSAuthentication.DataContracts.Masters;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/Masters")]
    public class MastersController : BaseAuthController
    {
        [Authorize]
        [Route("GetCities")]
        [HttpGet]
        public async Task<List<CityMinDc>> GetCities()
        {
            MastersManager manager = new MastersManager();
            return await manager.GetCities();
        }

        [Authorize]
        [Route("Warehouses")]
        [HttpPost]
        public async Task<List<WarehouseMinDc>> Warehouses(List<int> CityId)
        {
            MastersManager manager = new MastersManager();
            return await manager.Warehouses(CityId);
        }

        [Authorize]
        [Route("Clusters")]
        [HttpPost]
        public async Task<List<ClusterMinDc>> Clusters(List<int> warehouseId)
        {
            MastersManager manager = new MastersManager();
            return await manager.Clusters(warehouseId);
        }

        [Authorize]
        [Route("BaseCategories")]
        [HttpGet]
        public async Task<List<BaseCategoryMinDc>> BaseCategories()
        {
            MastersManager manager = new MastersManager();
            return await manager.BaseCategories();
        }

        [Authorize]
        [Route("Categories")]
        [HttpPost]
        public async Task<List<CategoryMinDc>> Categories(List<int> baseCategoryId)
        {
            MastersManager manager = new MastersManager();
            return await manager.Categories(baseCategoryId);
        }

        [Authorize]
        [Route("AllCategories")]
        [HttpGet]
        public async Task<List<CategoryMinDc>> AllCategories()
        {
            MastersManager manager = new MastersManager();
            return await manager.AllCategories();
        }

        [Authorize]
        [Route("SubCategories")]
        [HttpPost]
        public async Task<List<SubCategoryMinDc>> SubCategories(List<int> categoryId)
        {
            MastersManager manager = new MastersManager();
            return await manager.SubCategories(categoryId);
        }

        [Authorize]
        [Route("Brands")]
        [HttpPost]
        public async Task<List<BrandMinDc>> Brands(List<int> subcategoryId)
        {
            MastersManager manager = new MastersManager();
            return await manager.SubSubCategories(subcategoryId);
        }

        [Authorize]
        [Route("Items")]
        [HttpPost]
        public async Task<List<ItemMinDc>> Items(GetItemsParams getItemsParams)
        {
            MastersManager manager = new MastersManager();
            return await manager.Items(getItemsParams.BrandId, getItemsParams.WarehouseId);
        }


        [Authorize]
        [Route("GetBrand")]
        [HttpGet]
        public async Task<List<BuyerBrands>> SubsubCategories()
        {
            MastersManager manager = new MastersManager();
            return await manager.GetSubSubCategories();
        }

        [Authorize]
        [Route("GetBrandbasedCatId")]
        [HttpPost]
        public async Task<List<BrandMinDc>> SubsubCategoriesbasedCatId(List<int> categoryId)
        {
            MastersManager manager = new MastersManager();
            return await manager.SubSubCategoriesBasedOnCatIds(categoryId);
        }


        [Route("SaveBrands")]
        [HttpPost]
        public async Task<List<BrandMinDc>> SaveBrands(List<BrandMinDc> brands)
        {
            return null;
        }

        [Route("ValidatePinCode")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> ValidatePinCode(string city, string pinCode)
        {
            return true;
            //var pincodeUrl = ConfigurationManager.AppSettings["PostalCodeApiUrl"] + pinCode;
            //bool isValid = false;



            //GeoHelper helper = new GeoHelper();
            //var responseList = helper.GetAddressResult(pinCode);

            //if (responseList != null && responseList.Any() && responseList.Any(x => x.address_components.Any()) &&
            //    !responseList.Any(x => x.address_components.Any(z => z.types.Contains("administrative_area_level_2")))
            //    && responseList.Any(x => x.geometry != null && x.geometry.location != null)
            //    )
            //{
            //    var lat = responseList.FirstOrDefault().geometry.location.lat;
            //    var lng = responseList.FirstOrDefault().geometry.location.lng;
            //    responseList = helper.GetLatLngResult((double)lat, (double)lng);

            //}

            //isValid = responseList != null && responseList.Any(x => x.address_components != null && x.address_components.Any())
            //           && responseList.SelectMany(x => x.address_components).Any(z => !string.IsNullOrEmpty(z.long_name) && z.long_name.ToLower() == city.ToLower());




            //return isValid;
        }


        [Route("BulkUpdateShippingCity")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<bool> BulkUpdateShippingCity(/*List<customerShippingAddress> shippingAddresses*/)
        {
            //var pincodeUrl = ConfigurationManager.AppSettings["PostalCodeApiUrl"] + pinCode;
            bool isValid = false;
            ConcurrentBag<customerShippingAddress> addressBag = new ConcurrentBag<customerShippingAddress>();
            List<customerShippingAddress> shippingAddresses = new List<customerShippingAddress>();

            GeoHelper helper = new GeoHelper();
            using (var context = new AuthContext())
            {
                var customers = context.Customers.Where(x => x.CustomerId == 110474).ToList();


                ParallelLoopResult parellelResult = Parallel.ForEach(customers, (cust) =>
                {
                    var responseList = helper.GetLatLngResult(cust.lat, cust.lg);

                    if (responseList != null && responseList.Any()
                            && responseList.Any(x => x.address_components != null && x.address_components.Any()))
                    {
                        var addressComponent1 = responseList.FirstOrDefault().address_components;

                        customerShippingAddress address = new customerShippingAddress
                        {
                            CustomerId = cust.CustomerId,
                            ShippingCity = addressComponent1.FirstOrDefault(x => x.types.Contains("administrative_area_level_2"))?.long_name,
                            ShippingStateShort = addressComponent1.FirstOrDefault(x => x.types.Contains("administrative_area_level_1"))?.short_name,
                            ShippingStateLong = addressComponent1.FirstOrDefault(x => x.types.Contains("administrative_area_level_1"))?.long_name,
                        };

                        addressBag.Add(address);
                    }
                });

                if (parellelResult.IsCompleted)
                {
                    shippingAddresses = addressBag.ToList();
                }

                var states = context.States.ToList();
                foreach (var item in shippingAddresses)
                {
                    var cust = context.Customers.FirstOrDefault(x => x.CustomerId == item.CustomerId);
                    var state = states.FirstOrDefault(x => x.StateName == item.ShippingStateLong || x.StateName == item.ShippingStateShort);

                    if (state != null)
                    {
                        cust.State = state.StateName;
                    }

                    cust.ShippingCity = item.ShippingCity;
                    context.Entry(cust).State = System.Data.Entity.EntityState.Modified;
                }

                context.Commit();
            }


            return isValid;
        }

        [Authorize]
        [Route("SubCategoriesList")]
        [HttpGet]
        public async Task<List<SubCategoryMinDc>> SubCategoriesList()
        {
            MastersManager manager = new MastersManager();
            return await manager.SubCategoriesList();
        }
        [Authorize]
        [Route("CategoriesList")]
        [HttpPost]
        public async Task<List<CategoryMinDc>> CategoriesList(List<int> subcategoryId)
        {
            using (var db = new AuthContext())
            {
                DataTable dtsubcategoryIds = new DataTable();
                dtsubcategoryIds.Columns.Add("IntValue");
                subcategoryId.ForEach(x =>
                {
                    DataRow dr = dtsubcategoryIds.NewRow();
                    dr["IntValue"] = x;
                    dtsubcategoryIds.Rows.Add(dr);
                });
                var subcategoryIds = new SqlParameter
                {
                    TypeName = "dbo.intvalues",
                    ParameterName = "@subcategoryId",
                    Value = dtsubcategoryIds
                };
                List<CategoryMinDc> list = db.Database.SqlQuery<CategoryMinDc>("GetCategoryMapping @subcategoryId", subcategoryIds).ToList();
                return list;
            }
        }
        [Authorize]
        [Route("subcategoryIdBrandList")]
        [HttpPost]
        public async Task<List<BrandlistsDc>> subcategoryIdBrandList(List<int> subcatMappingIdslist)
        {
            using (var db = new AuthContext())
            {
                DataTable dtsubcatMappingIds = new DataTable();
                dtsubcatMappingIds.Columns.Add("IntValue");
                subcatMappingIdslist.ForEach(x =>
                {
                    DataRow dr = dtsubcatMappingIds.NewRow();
                    dr["IntValue"] = x;
                    dtsubcatMappingIds.Rows.Add(dr);
                });
                var subcatMappingIds = new SqlParameter
                {
                    TypeName = "dbo.intvalues",
                    ParameterName = "@SubCategoryMappingId",
                    Value = dtsubcatMappingIds
                };
                List<BrandlistsDc> list = db.Database.SqlQuery<BrandlistsDc>("getSubcategoryIdBrandList @SubCategoryMappingId", subcatMappingIds).ToList();
                return list;
            }
        }

    }

    public class customerShippingAddress
    {
        public int CustomerId { get; set; }
        public string ShippingStateShort { get; set; }
        public string ShippingStateLong { get; set; }
        public string ShippingCity { get; set; }
    }
}