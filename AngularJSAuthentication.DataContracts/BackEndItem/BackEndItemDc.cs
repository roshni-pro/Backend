using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.BackEndItem
{
    public class GetCutomerDetailsDC
    {
        public int CustomerId { get; set; }
        public int? WarehouseId { get; set; }
        public string Mobile { get; set; }
        public string ShippingAddress { get; set; }
        public string Name { get; set; }
        public string RefNo { get; set; }
        public string Skcode { get; set; }
        public double ? WalletPoint { get; set; }
        public string CustomerType { get; set; }

    }

    public class GetOrderByWarehouseDetail
    {
        public int OrderId { get; set; }
        public string Skcode { get; set; }
        public string CustomerName { get; set; }
        public double TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string ShopName { get; set; }
        public string ClusterName { get; set; }
        public string WarehouseName { get; set; }
        public string MobileNumber { get; set; }
        public string Status { get; set; }
        public string  CreatedBy  { get; set; }
        public int total_records { get; set; }

    }

    public class CustomerDetailsDC
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public GetCutomerDetailsDC getCutomerDetailsDC { get; set; }

    }

    public class OrderDetailByWarehouse
    {
        public int WarehouseId { get; set; }
        public string KeyValue { get; set; }
        public string Status { get; set; }
        public DateTime ? Fromdate { get; set; }
        public DateTime ? Todate { get; set; }
        public int skip { get; set; }
        public int take { get; set; }

    }

    public class GSTResponse
    {
        public bool Status { get; set; }
        public string msg { get; set; }
    }
    public class WarehouseStoreTypeDC
    {
        public int WarehouseId{ get; set; }
        public string WarehouseName { get; set; }
    }
    public class WarehouseStoreDC
    {
        public bool IsQrEnabled { get; set; }
        public int Storetype{ get; set; }
    }
}
