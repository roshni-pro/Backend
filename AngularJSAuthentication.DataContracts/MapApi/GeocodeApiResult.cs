using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.MapApi
{
    public class GeocodeApiResult
    {
        public List<GeocodeApiResultComponent> results { get; set; }
    }

    public class GeocodeApiResultComponent
    {
        public List<GeocodeApiAddressComponents> address_components { get; set; }
        public string formatted_address { get; set; }
        public GeocodeApiGeometry geometry { get; set; }
        public string place_id { get; set; }
        public List<string> types { get; set; }
    }

    public class GeocodeApiAddressComponents
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
        public List<string> types { get; set; }
    }

    public class GeocodeApiGeometry
    {
        public GeocodeApiBound bounds { get; set; }
        public GeocodeApiLatLng location { get; set; }
        public string location_type { get; set; }
        public GeocodeApiBound viewport { get; set; }
    }

    public class GeocodeApiLatLng
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class GeocodeApiBound
    {
        public GeocodeApiLatLng northeast { get; set; }
        public GeocodeApiLatLng southwest { get; set; }
    }
}
