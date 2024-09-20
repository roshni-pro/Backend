using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class WorkingCapitalCalander 
    {
       
        public ObjectId Id { get; set; }
        public long monthId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public long CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public List<WcDaysList> DaysList { get; set; }


    }

    public class WcDaysList
    {
        public long MonthId { get; set; }
        public DateTime date { get; set; }
         public bool IsHoliday { get; set; }
    }
}
