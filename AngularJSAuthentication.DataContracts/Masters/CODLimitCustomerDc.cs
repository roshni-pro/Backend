using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class CODLimitCustomerDc
    {

        public long Id { get; set; }
        public int CustomerId { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string WarehouseName { get; set; }
        public string Mobile { get; set; }
        public string Name { get; set; }
        public double CODLimit { set; get; }
        public bool IsCustomCODLimit { set; get; }
        public double Last90DaysCancelPercent { set; get; }
        public double Last90DaysRedispatchCount { set; get; } // order count
        public double Last90DaysTotalCount { set; get; } //total count

    }
    public class CODLimitCustomerSearchDc
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public int WarehouseId { get; set; }
        public string Type { get; set; }//Potential Defaulter (10% above cancellation in last 90 days), CustomCODLimit
        public string Keyward { get; set; }
    }
    public class CODLimitCustomerRes
    {
        public List<CODLimitCustomerDc> CODLimitCustomer { get; set; }
        public int TotalCount { get; set; }
    }
    public class CODLimitCustomerHistory
    {
        public long Id { get; set; }
    }
    public class UpdateCustomersCODLimitDc
    {
        public int CustomerId { set; get; }
        public double CODLimit { set; get; }
        public bool IsCustomCODLimit { set; get; }
    }



}
