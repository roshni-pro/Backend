using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts
{
    public class PageVisitDC
    {
        public string Route { get; set; }
        public string UserName { get; set; }
        public DateTime VisitedOn { get; set; }
        public int RemainingTimeinHrs
        {
            get;set;
            //get
            //{
            //    if ((VisitedOn - DateTime.Now.AddDays(-7)).Hours > 168)
            //    {
            //      return ((VisitedOn - DateTime.Now.AddDays(-7)).Hours);
            //    }
            //    else
            //    {
            //        return 0;
            //    }
            //}
        }

    }

    public class AppVisitDC
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string AppType { get; set; }
        public DateTime VisitedOn { get; set; }
        public int RemainingTimeinHrs
        {
            get; set;
            //get
            //{
            //    if ((VisitedOn - DateTime.Now.AddDays(-7)).Hours > 168)
            //    {
            //      return ((VisitedOn - DateTime.Now.AddDays(-7)).Hours);
            //    }
            //    else
            //    {
            //        return 0;
            //    }
            //}
        }
    }

    public class PeopleDC
    {
        public int PeopleID { get; set; }
        public string PeopleFirstName { get; set; }
        public string PeopleLastName { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public string Empcode { get; set; }
        public string Desgination { get; set; }
        public string Department { get; set; }
        public string Mobile { get; set; }
        public string city { get; set; }
    }

    public class EmpVisitDC
    {
        public DateTime VisitedOn { get; set; }
        public int PeopleID { get; set; }
        public string PeopleFirstName { get; set; }
        public string PeopleLastName { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string Empcode { get; set; }
        public string Desgination { get; set; }
        public string Department { get; set; }
        public string Mobile { get; set; }
        public string city { get; set; }
    }

    public class PeopleInActiveDC
    {
        public int PeopleID { get; set; }
        public string PeopleFirstName { get; set; }
        public string PeopleLastName { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public string Empcode { get; set; }
        public string Desgination { get; set; }
        public string Department { get; set; }
        public string Mobile { get; set; }
        public string Status { get; set; }
        public string city { get; set; }
        public DateTime LastVisitedDate { get; set; }
    }

    public class InActivePeopleReportDC
    {
        public string PeopleFirstName { get; set; }
        public string PeopleLastName { get; set; }
        public string Empcode { get; set; }
        public string Department { get; set; }
        public string Mobile { get; set; }
        public string Status { get; set; }
        public string city { get; set; }
    }

}
