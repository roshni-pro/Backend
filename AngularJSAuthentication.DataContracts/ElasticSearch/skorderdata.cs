using Nest;
using System;

namespace AngularJSAuthentication.DataContracts.ElasticSearch
{

    public class skorderdata
    {
        public int orderid { get; set; }
        public int orderdetailid { get; set; }
        public int itemmultimrpid { get; set; }
        [Keyword]
        public string itemnumber { get; set; }
        [Keyword]
        public string itemname { get; set; }
        public int? cityid { get; set; }
        [Keyword]
        public string cityname { get; set; }
        public int whid { get; set; }
        [Keyword]
        public string whname { get; set; }
        public int clusterid { get; set; }
        [Keyword]
        public string clustername { get; set; }
        public int brandid { get; set; }
        [Keyword]
        public string brandname { get; set; }
        public int catid { get; set; }
        [Keyword]
        public string catname { get; set; }
        [Keyword]
        public string status { get; set; }
        public int compid { get; set; }
        [Keyword]
        public string compname { get; set; }
        public DateTime createddate { get; set; }
        public DateTime? dispatchdate { get; set; }
        public DateTime updateddate { get; set; }
        public int ordqty { get; set; }
        public int? dispatchqty { get; set; }
        public double price { get; set; }
        public double mrp { get; set; }
        public int custid { get; set; }
        [Keyword]
        public string skcode { get; set; }
        [Keyword]
        public string shopname { get; set; }
        [Keyword]
        public string orderby { get; set; }
        public int? ordertakensalespersonid { get; set; }
        [Keyword]
        public string paymenttype { get; set; }
        public string orderremark { get; set; }
        public DateTime deliverydate { get; set; }
        public DateTime? delivereddate { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }

        public double grossamount { get; set; }
        public double discountamount { get; set; }
        public double taxamount { get; set; }
        public double tcsamount { get; set; }
        public double? deliverycharge { get; set; }
        public double? walletamount { get; set; }
        public double? billdiscountamount { get; set; }
        public long storeid { get; set; }
        public int executiveid { get; set; }
        public bool? isdigitalorder { get; set; }
        public string customertype { get; set; }
        public string customerclass { get; set; }
        public bool isfreeitem { get; set; }
        public int ordertype { get; set; }
        public long channelmasterid { get; set; }
    }

    public class orderdataschedule
    {
        public DateTime rundate { get; set; }
    }

    public class ElasticItemNumber
    {
        public string itemnumber { get; set; }
        public string itemname { get; set; }
        public DateTime createddate { get; set; }
        public int ordqty { get; set; }
        public int itemmultimrpid { get; set; }
    }
    public class ordcountamount
    {
        public int ordcount { get; set; }
        public double ordamount { get; set; }
        public int linecount { get; set; }
        public int billedcust { get; set; }
        public int skusold { get; set; }

    }

    public class StoreViewCfr
    {
        public int StoreView { get; set; }
        public int LiveCfrSKU { get; set; }
    }

    public class doubleVal
    {
        public double val { get; set; }
    }
    public class SalesPersonKPIOrderData
    {
        public string skcode { get; set; }
        public long storeid { get; set; }
        public string itemnumber { get; set; }
        public double dispatchqty { get; set; }
        public double price { get; set; }
        public int custid { get; set; }
    }

    public class ItemIdCls
    {
        public int ItemId { get; set; }
    }

    public class MultiMrpIdCls
    {
        public int itemmultimrpid { get; set; }
        public int warehouseid { get; set; }
        public bool IsFreeItem { get; set; }
    }
}
