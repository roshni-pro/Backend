using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class ClusterMinDc
    {
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
        public List<LatLngs> LatLngs { get; set; }
    }

    public class LatLngs
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }


    public class ClusterWithLatLng
    {
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
        public double Latitude { get; set; }
        public double Longitude{ get; set; }
    }
}
