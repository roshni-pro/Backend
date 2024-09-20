using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class CustomerAddressHelper
    {
        public bool InsertCustomerAddress(CustomerAddress custaddress, int userid)
        {
            using (AuthContext context = new AuthContext())
            {
                var CustomerAddresses = context.CustomerAddressDB.Where(x => x.CustomerId == custaddress.CustomerId).FirstOrDefault();
                if (CustomerAddresses != null)
                {
                    CustomerAddresses.CityPlaceId = custaddress.CityPlaceId;
                    CustomerAddresses.AreaPlaceId = custaddress.AreaPlaceId;
                    CustomerAddresses.AreaText = custaddress.AreaText;
                    CustomerAddresses.AreaLat = custaddress.AreaLat;
                    CustomerAddresses.AreaLng = custaddress.AreaLng;
                    CustomerAddresses.AddressPlaceId = custaddress.AddressPlaceId;
                    CustomerAddresses.AddressText = custaddress.AddressText;
                    CustomerAddresses.AddressLat = custaddress.AddressLat;
                    CustomerAddresses.AddressLng = custaddress.AddressLng;
                    CustomerAddresses.AddressLineOne = custaddress.AddressLineOne;
                    CustomerAddresses.AddressLineTwo = custaddress.AddressLineTwo;
                    CustomerAddresses.ModifiedDate = DateTime.Now;
                    CustomerAddresses.ModifiedBy = userid;
                    context.Entry(CustomerAddresses).State = EntityState.Modified;

                }
                else
                {
                    var CustomerAddress = new CustomerAddress
                    {
                        CityPlaceId = custaddress.CityPlaceId,
                        AreaPlaceId = custaddress.AreaPlaceId,
                        AreaText = custaddress.AreaText,
                        AreaLat = custaddress.AreaLat,
                        AreaLng = custaddress.AreaLng,
                        AddressPlaceId = custaddress.AddressPlaceId,
                        AddressText = custaddress.AddressText,
                        AddressLat = custaddress.AddressLat,
                        AddressLng = custaddress.AddressLng,
                        AddressLineOne = custaddress.AddressLineOne,
                        AddressLineTwo = custaddress.AddressLineTwo,
                        CustomerId = custaddress.CustomerId,
                        ZipCode = custaddress.ZipCode,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                    };
                    context.CustomerAddressDB.Add(CustomerAddress);
                }
                return context.Commit() > 0;
            }
        }
    }
}