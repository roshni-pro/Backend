using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.CustomerGroup
{
    public class CustomerGroupCriteria
    {
        public int? WarehouseId { get; set; }
        public int? TypeCode { get; set; }
        public string ConditionValue { get; set; }
        public string SelectedCategoryName { get; set; }
        public string SelectedSubCategoryName { get; set; }
        public string SelectedBrandName { get; set; }
        public double? OrderAmount { get; set; }
        public int? OrderCount { get; set; }
        public double? OrderValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
