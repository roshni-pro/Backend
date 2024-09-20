using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public class RTVPaginatorDc
    {
        public int RowsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public List<int> WarehouseIds { get; set; }
        public string SupplierCode { get; set; }
        public int? RTVId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
      
    }
    public class RTVPaginatorReturn
    {

        public int TotalCount { get; set; }
        public List<RTVMasterGetDc> rTVMasterGetDcs { get; set; }

    }
    public class RTVMasterGetDc
    {
        public int Id { get; set; }
        public string RTVNo { get; set; } //Fyi 
        public DateTime? RTVNoCreatedDate { get; set; } //Fyi 
        public int WarehouseId { get; set; }
        public int SupplierId { get; set; }
        public int DepoId { get; set; }
        public string StockType { get; set; }
        public string SupName { get; set; }
        public string WarehouseName { get; set; }
        public string DepoName { get; set; }
        public double GSTAmount { get; set; }
        public double TotalAmount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Deleted {get; set; }
        public string Status { get; set; }
        public string SupplierCode { get; set; }
        public List<RTVMasterDetail> Detail { get; set; }
    }
    public class RTVMasterGetExportDc
    {
        public int Id { get; set; }
        public string RTVNo { get; set; } //Fyi 
        public DateTime? RTVNoCreatedDate { get; set; } //Fyi 

        
        public int WarehouseId { get; set; }
        public int SupplierId { get; set; }
        public int DepoId { get; set; }
        public string StockType { get; set; }
        public string SupplierName { get; set; }
        public string WarehouseName { get; set; }
        public string DepoName { get; set; }
        public double GSTAmount { get; set; }
        public double TotalAmount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
        public string SupplierCode { get; set; }
        public bool Deleted { get; set; }
        public string ItemName { get; set; }
        public long ItemQty { get; set; }
        public double ItemPrice { get; set; }
        
        public double TaxableAmount { get; set; }
        
    }
}
