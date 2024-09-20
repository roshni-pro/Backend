using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.MapApi;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Transaction.Customer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using static AngularJSAuthentication.Common.Helpers.GooglePlaceApiHelper;

namespace AngularJSAuthentication.API.ControllerV7
{
    [AllowAnonymous]
    [RoutePrefix("api/GooglePlace")]
    public class GooglePlaceController : ApiController
    {
        [Route("GetCityList")]
        public async Task<List<PlaceApiResultComponent>> GetCityList(string keyword)
        {
            GooglePlaceApiHelper helper = new GooglePlaceApiHelper();
            var result = await helper.GetCityList(keyword);
            return result;
        }

        [Route("GetArea")]
        public async Task<List<PlaceApiResultComponent>> GetArea(string keyword, string placeId)
        {
            GooglePlaceApiHelper helper = new GooglePlaceApiHelper();
            var placeResult = await helper.GetAddressByPlaceId(placeId);
            int distanceInMeter = int.Parse(ConfigurationManager.AppSettings["AreaDistanceInMeter"]);
            var result = await helper.GetAddress(keyword, placeResult.results.First().geometry.location.lat, placeResult.results.First().geometry.location.lng, distanceInMeter);
            return result;
        }

        [Route("GetAddress")]
        public async Task<List<PlaceApiResultComponent>> GetAddress(string keyword, string placeId)
        {
            GooglePlaceApiHelper helper = new GooglePlaceApiHelper();
            var placeResult = await helper.GetAddressByPlaceId(placeId);
            int distanceInMeter = int.Parse(ConfigurationManager.AppSettings["AddressDistanceInMeter"]);
            var result = await helper.GetAddress(keyword, placeResult.results.First().geometry.location.lat, placeResult.results.First().geometry.location.lng, distanceInMeter);
            return result;
        }

        [HttpGet]
        [Route("GetAddressByPlaceId")]
        public async Task<GeocodeApiResult> GetAddressByPlaceId(string placeId)
        {
            GooglePlaceApiHelper helper = new GooglePlaceApiHelper();
            var placeResult = await helper.GetAddressByPlaceId(placeId);
            return placeResult;
        }

        [HttpGet]
        [Route("GetCustomer")]
        public CustomerAddressVM GetCustomer(int customerId)
        {
            using(var authContext = new AuthContext())
            {
                var customerIdIdParam = new SqlParameter
                {
                    ParameterName = "CustomerId",
                    Value = customerId
                };
                string query = @"Select	 CA.Id as CustomerAddressId
		                                ,CA.AddressLat
		                                ,CA.AddressLng
		                                ,CA.AddressPlaceId
		                                ,CA.AddressText
		                                ,CA.AreaLat
		                                ,CA.AreaLng
		                                ,CA.AreaPlaceId
		                                ,CA.AreaText
		                                ,CA.ZipCode
		                                ,CA.AddressLineTwo
		                                ,CA.AddressLineOne 
		                                ,C.ShippingAddress
		                                ,C.CustomerId		
                                        ,C.City as CityName	
                                        ,CA.CityPlaceId
                                from Customers C
                                Left JOIN CustomerAddresses CA  ON C.CustomerId = CA.CustomerId AND CA.IsActive=1 and CA.IsDeleted=0
                                WHERE C.CustomerId=@CustomerId";

                var customer = authContext.Database.SqlQuery<CustomerAddressVM>(query, customerIdIdParam).FirstOrDefault();
                return customer;
            }
        }

    }
}
