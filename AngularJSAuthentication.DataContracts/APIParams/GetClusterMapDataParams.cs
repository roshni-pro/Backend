using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public class GetClusterMapDataParams
    {
        public List<int> CityIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long StoreId { get; set; }
        public List<int> WareHouseIds { get; set; }
        public List<int> ClusterIds { get; set; }
        public List<int> BaseCatIds { get; set; }
        public List<int> CategoryIds { get; set; }
        public List<int> SubCatIds { get; set; }
        public List<int> BrandIds { get; set; }
        public List<string> ItemNumbers { get; set; }
        public List<int> MultiMrpIds { get; set; }
    }
}
