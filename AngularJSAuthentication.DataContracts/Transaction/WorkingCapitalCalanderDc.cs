using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction
{
    public class WorkingCapitalCalanderDc
    {
        public long Id { get; set; }
        public long monthId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public long CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public List<WcDaysListDc> DaysList { get; set; }
    }

    public class WcDaysListDc
    {
        public long MonthId { get; set; }
        public DateTime date { get; set; }
        public bool IsHoliday { get; set; }
    }
}
