using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.MapApi
{
    public class PlaceApiResult
    {
        public List<PlaceApiResultComponent> predictions { get; set; }
    }

    public class PlaceApiResultComponent
    {
        public string description { get; set; }
        public string place_id { get; set; }
        public PlaceStructuredFormatting structured_formatting { get; set; }
    }

    public class PlaceStructuredFormatting
    {
        public string main_text { get; set; }

    }
}
