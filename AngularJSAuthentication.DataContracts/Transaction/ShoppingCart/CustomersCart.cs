using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Transaction.ShoppingCart
{
    public class SearchByCustomerDc
    {
        public int total_count { get; set; }
        public List<CustomersCart> Carts { get; set; }
    }


    public class CustomersCartFilters
    {
        public List<int> WarehouseIds { get; set; }
        public int totalitem { get; set; }
        public int page { get; set; }
        public string keyword { get; set; }
        public bool IsOrderNotPlaced { get; set; }

    }

    public class CustomersCart
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string SkCode { get; set; }
        public string Mobile { get; set; }
        public string ShopName { get; set; }
        public int OrderId { get; set; }
        public double GrossAmount { get; set; }
        //public int WarehouseId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public List<ItemCart> ItemList { get; set; }
    }
    public class ItemCart
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int qty { get; set; }
        public double UnitPrice { get; set; }
        public bool IsActive { get; set; }

        public string ABCClassification { get; set; }

    }
}
