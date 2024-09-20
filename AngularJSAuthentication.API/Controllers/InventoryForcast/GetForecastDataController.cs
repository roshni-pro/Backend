using AngularJSAuthentication.BusinessLayer.Managers;
using AngularJSAuthentication.DataContracts.ForCast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.InventoryForcast
{//SaleCompaireActualVS
    [RoutePrefix("api/GetForecastData")]

    public class GetForecastDataController : ApiController
    {


        [Route("GetItemSaleCompaireActualVSForecast")]
        [HttpPost]
        public ItemSaleCompaireActualVSForecastMainList GetItemSaleCompaireActualVSForecastList(GetItemSaleCompaireActualVSForecastDc postData)
        {
            using (var myContext = new AuthContext())
            {
                ItemSaleCompaireActualVSForecastMainList list = new ItemSaleCompaireActualVSForecastMainList();
                ForcastManager forcastManager = new ForcastManager();
                list = forcastManager.GetItemSaleCompaireActualVSForecastList(postData);
                return list;
            }
        }

        [Route("GetItemSaleCompaireActualVSForecastExport")]
        [HttpPost]
        public List<ItemSaleCompaireActualVSForecastListDc> GetItemSaleCompaireActualVSForecastExport(GetItemSaleCompaireActualVSForecastDc postData)
        {
            using (var myContext = new AuthContext())
            {
                List<ItemSaleCompaireActualVSForecastListDc> list = new List<ItemSaleCompaireActualVSForecastListDc>();
                ForcastManager forcastManager = new ForcastManager();
                list = forcastManager.GetItemSaleCompaireActualVSForecastListExport(postData);
                return list;
            }
        }


        [Route("GetBrandSaleCompaireActualVSForecast")]
        [HttpPost]
        public BrandSaleCompaireActualVSForecastMainList GetBrandSaleCompaireActualVSForecastList(GetBrandSaleCompaireActualVSForecastDc postData)
        {
            using (var myContext = new AuthContext())
            {
                BrandSaleCompaireActualVSForecastMainList list = new BrandSaleCompaireActualVSForecastMainList();
                ForcastManager forcastManager = new ForcastManager();
                list = forcastManager.GetBrandSaleCompaireActualVSForecastList(postData);
                return list;
            }
        }

         
        [Route("GetBrandSaleCompaireActualVSForecastExport")]
        [HttpPost]
        public List<BrandSaleCompaireActualVSForecastListDc> GetBrandSaleCompaireActualVSForecastListExport(GetBrandSaleCompaireActualVSForecastDc postData)
        {
            using (var myContext = new AuthContext())
            {
                List<BrandSaleCompaireActualVSForecastListDc> list = new List<BrandSaleCompaireActualVSForecastListDc>();
                ForcastManager forcastManager = new ForcastManager();
                list = forcastManager.GetBrandSaleCompaireActualVSForecastListExport(postData);
                return list;
            }
        }
    }
}
