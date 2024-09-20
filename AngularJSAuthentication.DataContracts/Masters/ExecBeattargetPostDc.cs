using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngularJSAuthentication.DataContracts.Mongo;

namespace AngularJSAuthentication.DataContracts.Masters
{
  public class ExecBeattargetPostDc
    {
        public string Id { get; set; }
        public int CityId { get; set; }
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public decimal VisitedPercent { get; set; }
        public decimal ConversionPercent { get; set; }
        public decimal CustomerPercent { get; set; }
        public decimal OrderPercent { get; set; }
        public double ProductPareto { get; set; }
        public double CustomerPareto { get; set; }
        public int AvgLineItem { get; set; }
        public int AvgOrderAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
    }

    public class ResExecuteBeatTarget
    {
        public bool Result { get; set; }
        public string msg { get; set; }
        public int Count { get; set; }
        public List<ExecuteBeatTarget> ExecuteBeatTargets { get; set; }
        public object Data { get; set; }
    }
}
