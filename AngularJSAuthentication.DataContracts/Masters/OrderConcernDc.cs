using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class OrderConcernDc
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LinkId { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; }
        public string CustComment { get; set; }
        public string CDComment { get; set; }
        public bool IsCustomerRaiseConcern { get; set; }
        public string Skcode { get; set; }
        public string Name { get; set; }
        public int CustomerId { get; set; }
        public string ShopName { get; set; }
        public string ModifiedByName { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime OrderDate { get; set; }
        public double Amount { get; set; }
        public DateTime Deliverydate { get; set; }
        public int TotalCount { get; set; }
        public string Msg { get; set; }
        public string CustomerMobile { get; set; }
        public string TurnAroundTime { get; set; }
        public int OrderConcernMasterId { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        // public List<OrderForStatus> OrderForStatus { get; set; }
    }

    public class OrderForStatus
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; }
        public string CDComment { get; set; }
    }
    public class CustomerRaiseCommentDc
    {
        public int OrderId { get; set; }
        public string CustComment { get; set; }
        public string LinkId { get; set; }
        public int OrderConcernMasterId { get; set; }
    }

    public class OrderConcernResDC
    {
        public int TotalCount { get; set; }
        public List<OrderConcernDc> orderConcernDcs { get; set; }
    }
    public class OrderConcernCount
    {
        public string keyword { get; set; }
        public List<int> WarehouseIds { get; set; }
        public string Status { get; set; }
        //public DateTime? selectedMonth { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int skip { get; set; }
        public int take { get; set; }

    }
    public class OrderConcernFilterDcs
    {
        public int OrderId { get; set; }
        public int skip { get; set; }
        public int take { get; set; }

    }
    public class OrderConcernDataDc
    {
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime Deliverydate { get; set; }
        public double Amount { get; set; }
        public string CDComment { get; set; }       
        public string Status { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CustComment { get; set; }
    }

    public class OrderConcernMasterDc
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public string Msg { get; set; }
    }

    public class OrderConcernMasterFilterDc
    {
        public int skip { get; set; }
        public int take { get; set; }
    }
    public class OrderConcernMasterListDC
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string CreatedByName { get; set; }
        public string ModifiedByName { get; set; }
        public string Description { get; set; }
        public string Msg { get; set; }
    }
    public class ConcernMasterListDc
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }


}
