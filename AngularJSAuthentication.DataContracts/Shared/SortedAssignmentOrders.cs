using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Shared
{
    public class SortedAssignmentOrders
    {
        public List<OrdersWithLatLng> SortedOrders { get; set; }
    }

    public class OrdersWithLatLng
    {
        public double Distance { get; set; }
        public int OrderId { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class SortedAssignmentOrdersPostParams
    {
        public int DeliveryIssuanceId { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
    // Added int list  warehouseids
    public class POCinterfaceV1
    {
        public int skip { get; set; }
        public int take { get; set; }
        public int WarehouseId { get; set; }
        public List<int> WarehouseIds { get; set; }
        public int AgentId { get; set; }
        public int IsPocVerified { get; set; }

    }
    public class POCPagerListDc
    {
        public List<getPOCVerification> getPOCVerificationlist { get; set; }
        public int TotalRecords { get; set; }

    }
    public class getPOCVerification
    {
        public string DboyName { get; set; }
        public string ClusterName { get; set; }//AgentName
        public string Status { get; set; }
        public int OrderId { get; set; }
        public string WarehouseName { get; set; }
        public int? DeliveryIssuanceId { get; set; }
        public string Skcode { get; set; }
        public bool isPocVerified { get; set; }
        public string Comment { get; set; }
        public double GrossAmount { get; set; }
        public string Remarks { get; set; }
        public DateTime OrderedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
       

    }
}
