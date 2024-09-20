using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction
{
   
    public class comboItemlistDC
    {
        public int ItemId { get; set; }
        public string itemname { get; set; }
        public string ItemImage { get; set; }
        public string Qty { get; set; }
        public double UnitPrice { get; set; }
        public string MinOrderQty { get; set; }
        public decimal Parcentage { get; set; }
        public decimal AfterPercentage { get; set; }
        public decimal TotalPriceItem { get; set; }
        public double Mrp { get; set; }


    }



    public class colistDC
    {
        public  string Id { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string ComboName { get; set; }
        public double ComboPrice { get; set; }
        public string ComboImage { get; set; }
        public int Qty { get; set; }
        public int ComboOrderQty { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsPublish { get; set; }
        public bool IsDeleted { get; set; }
        //public int ItemId { get; set; }
        public string GUID { get; set; }
        public double TotalComboPrice { get; set; }
        public decimal Discount { get; set; }
         public int SellQty { get; set; }
        public List<comboItemlistDC> CoItemlist { get; set; }
    }
    public class GetUPITransactionDataDC
    {

        public string OrderId { get; set; }
        public DateTime? Fromdate { get; set; }
        public DateTime? Todate { get; set; }
        public int Skip { get; set; }
        public int take { get; set; }
        public List<int> warehouses { get; set; }

    }
    public class ExportGetUPITransactionDataDC
    {

        public string OrderId { get; set; }
        public DateTime? Fromdate { get; set; }
        public DateTime? Todate { get; set; }
        public List<int> warehouses { get; set; }

    }
}
