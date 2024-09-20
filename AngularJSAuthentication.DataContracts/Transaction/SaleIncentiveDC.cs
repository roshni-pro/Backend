using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction
{
    public class SaleIncentiveDC
    {
        public class SalesComissionTransactionDc
        {
            public long Id { get; set; }
            public int WarehouseId { get; set; }
            public string EventName { get; set; }
            public long? EventMasterId { get; set; }
            public double BookedValue { get; set; }
            public int IncentiveType { get; set; } //0-Value 1-Percent
            public double IncentiveValue { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string Condition { get; set; } //Sql for Event master table and replce [Id]
            public DateTime CreatedDate { get; set; }
            public DateTime? ModifiedDate { get; set; }
            public bool IsActive { get; set; }
            public bool? IsDeleted { get; set; }
            public int CreatedBy { get; set; }
            public int? ModifiedBy { get; set; }     
            public long CommissionCatMasterId { get; set; }
            public int Customers { get; set;  }
            public List<SalesComTransDetailDc> salesComTransDetailDC { get; set; }
            public List<int> ExecutiveIds { get; set; }
            public List<int> ItemIds { get; set; }
            public List<int> SubCategoryId { get; set; }
            public int StoreId { get; set; }
        }

        public class SalesComTransDetailDc
        {
            public long SalesComissionTransactionId { get; set; }
            public int SubCategoryMappingId { get; set; }
            public int BrandMappingId { get; set; }
            public int? ItemId { get; set; }
        }

        public class ExecutiveSalesCommissions
        {
            public int ExecutiveId { get; set; }
            public long SalesComissionTransactionId { get; set; }
        }
    }
}
