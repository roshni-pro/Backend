using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
   public class CustomerAddressDc
    {
        public string CityPlaceId { get; set; }
        public string AreaPlaceId { get; set; }
        public string AreaText { get; set; }
        public double AreaLat { get; set; }
        public double AreaLng { get; set; }


        public string AddressPlaceId { get; set; }
        public string AddressText { get; set; }
        public double AddressLat { get; set; }
        public double AddressLng { get; set; }

        public string AddressLineOne { get; set; }
        public string AddressLineTwo { get; set; }

        public int CustomerId { get; set; }
        public string ZipCode { get; set; }

        public string CityName { get; set; }
    }
}
