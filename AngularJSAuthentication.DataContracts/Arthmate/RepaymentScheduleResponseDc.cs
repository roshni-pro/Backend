using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Arthmate
{
    public class RepaymentScheduleResponseDc
    {

        public bool error { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
        public RepaymentData data { get; set; }
    }
    public class RepaymentData
    {
        public List<Row> rows { get; set; }
        public int count { get; set; }
    }


    public class Row
    {
        public int _id { get; set; }
        public int repay_schedule_id { get; set; }
        public int company_id { get; set; }
        public int product_id { get; set; }
        public string loan_id { get; set; }
        public int emi_no { get; set; }
        public DateTime due_date { get; set; }
        public int emi_amount { get; set; }
        public int prin { get; set; }
        public int int_amount { get; set; }
        public int __v { get; set; }
    }
}
