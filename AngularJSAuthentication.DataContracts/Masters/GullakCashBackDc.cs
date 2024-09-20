using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class GullakCashBackDc
    {
        public long Id { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double AmountFrom { get; set; }
        public double AmountTo { get; set; }
        public double MaximumCashBackAmount { get; set; }
        public double CashBackPercent { get; set; }
        public bool IsVerify { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public int CustomerType { get; set; }
        public string Customer { get; set; }
    }
    public class ResponseGullakCashBackDc
    {
        public int TotalItem { get; set; }
        public List<GullakCashBackDc> GullakCashBackDcs { get; set; }
    }
    public class ResGullakCashBackSave
    {
        public bool Result { get; set; }
        public string msg { get; set; }
    }

}
