using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Security.Claims;
using AngularJSAuthentication.Model;
using GenricEcommers.Models;
using LinqKit;
using AngularJSAuthentication.API.Controllers.Base;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/incidentReport")]
    public class IncidentReportController : BaseAuthController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        [Route("AddIncidentReport")]
        [HttpPost]
        public bool addincidentReport(IncidentReportDc report)
        {
            using (AuthContext context = new AuthContext())
            {
                string tm = report.TimeofIncident.ToLongTimeString();
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (report == null)
                {
                    throw new ArgumentNullException("report");
                }
                bool Isexists = true;
                bool result = false;
                DateTime date1 = report.DateofIncident;
                DateTime date2 = report.DateOfReport;
                IncidentReport data = new IncidentReport();
                List<IncidentReport> reportdata = new List<IncidentReport>();
                IncidentReport reports = new IncidentReport();
                reports.CaseNo = report.CaseNo;
                reports.CityId = report.CityId;
                reports.DateofIncident = date1;
                reports.DateOfReport = date2;
                reports.Department = report.Department;
                reports.InBound = report.InBound;
                reports.OutBound = report.OutBound;
                reports.PersonName = report.PersonName;
                reports.PersonRole = report.PersonRole;
                reports.ReportedBy = report.ReportedBy;
                reports.ReportedRole = report.ReportedRole;
                reports.TimeofIncident = tm;
                reports.TypeOfLoss = report.TypeOfLoss;
                reports.IncidentDescription = report.IncidentDescription;
                reports.Location = report.Location;
                reports.WarehouseId = report.WarehouseId;
                reports.WitnessName = report.WitnessName;
                reports.MobileNo = report.MobileNo;
                reports.CreatedDate = indianTime;
                reports.Status = "Open";
                reports.IsActive = true;
                reports.IsDeleted = false;
                context.IncidentReport.Add(reports);
                result = context.Commit() > 0;
                return true;
            }
        }

        [HttpGet]
        [Route("GetIncidentReportOpenList")]
        public dynamic GetIncidentReportOpenList()
        {
            using (AuthContext db = new AuthContext())
            {
                
                string sqlquery = "select * from IncidentReports";
                List<IncidentReportListDc> data = db.Database.SqlQuery<IncidentReportListDc>(sqlquery).Where(x=>x.Status == "Open" || x.Status == "ReOpen").OrderByDescending(x=>x.CreatedDate).ToList();
                var reportedby = data.Select(x => x.ReportedBy).Distinct().ToList();
                var reportedname = db.Peoples.Where(x => reportedby.Contains(x.PeopleID)).ToList();
                var outboundby = data.Select(x => x.OutBound).Distinct().ToList();
                var outboundname = db.Peoples.Where(x => outboundby.Contains(x.PeopleID)).ToList();
                var inboundby = data.Select(x => x.InBound).Distinct().ToList();
                var inboundname = db.Peoples.Where(x => inboundby.Contains(x.PeopleID)).ToList();
                var city = data.Select(x => x.CityId).Distinct().ToList();
                var cityname = db.Cities.Where(x => city.Contains(x.Cityid)).ToList();
                var dept = data.Select(x => x.Department).Distinct().ToList();
                var deptname = db.Departments.Where(x => dept.Contains(x.DepId)).ToList();
                var wh = data.Select(x => x.WarehouseId).Distinct().ToList();
                var whname = db.Warehouses.Where(x => wh.Contains(x.WarehouseId)).ToList();
                var location = data.Select(x => x.Location).Distinct().ToList();
                var locationname = db.Cities.Where(x => location.Contains(x.Cityid)).ToList();
                var reopenby = data.Select(x => x.ReOpenBy).Distinct().ToList();
                var reopenbyname = db.Peoples.Where(x => reopenby.Contains(x.PeopleID)).ToList();
                foreach (var item in data)
                {
                    item.ReportedByName = reportedname.Where(x => x.PeopleID == item.ReportedBy).Select(x => x.DisplayName).FirstOrDefault();
                    item.OutBoundName = outboundname.Where(x => x.PeopleID == item.OutBound).Select(x => x.DisplayName).FirstOrDefault();
                    item.InBoundName = inboundname.Where(x => x.PeopleID == item.InBound).Select(x => x.DisplayName).FirstOrDefault();
                    item.CityName = cityname.Where(x => x.Cityid == item.CityId).Select(x => x.CityName).FirstOrDefault();
                    item.DepartmentName = deptname.Where(x => x.DepId == item.Department).Select(x => x.DepName).FirstOrDefault();
                    item.WarehouseName = whname.Where(x => x.WarehouseId == item.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                    item.LocationName = locationname.Where(x => x.Cityid == item.Location).Select(x => x.CityName).FirstOrDefault();
                    item.ReOpenByName = reopenbyname.Where(x => x.PeopleID == item.ReOpenBy).Select(x => x.DisplayName).FirstOrDefault();
                }
                    return data;
            }
        }


        [HttpGet]
        [Route("GetIncidentReportClosedList")]
        public dynamic GetIncidentReportClosedList()
        {
            using (AuthContext db = new AuthContext())
            {

                string sqlquery = "select * from IncidentReports";
                List<IncidentReportListDc> data = db.Database.SqlQuery<IncidentReportListDc>(sqlquery).Where(x => x.Status == "Closed").OrderByDescending(x => x.CreatedDate).ToList();
                var reportedby = data.Select(x => x.ReportedBy).Distinct().ToList();
                var reportedname = db.Peoples.Where(x => reportedby.Contains(x.PeopleID)).ToList();
                var outboundby = data.Select(x => x.OutBound).Distinct().ToList();
                var outboundname = db.Peoples.Where(x => outboundby.Contains(x.PeopleID)).ToList();
                var inboundby = data.Select(x => x.InBound).Distinct().ToList();
                var inboundname = db.Peoples.Where(x => inboundby.Contains(x.PeopleID)).ToList();
                var city = data.Select(x => x.CityId).Distinct().ToList();
                var cityname = db.Cities.Where(x => city.Contains(x.Cityid)).ToList();
                var dept = data.Select(x => x.Department).Distinct().ToList();
                var deptname = db.Departments.Where(x => dept.Contains(x.DepId)).ToList();
                var wh = data.Select(x => x.WarehouseId).Distinct().ToList();
                var whname = db.Warehouses.Where(x => wh.Contains(x.WarehouseId)).ToList();
                var location = data.Select(x => x.Location).Distinct().ToList();
                var locationname = db.Cities.Where(x => location.Contains(x.Cityid)).ToList();
                var closedby = data.Select(x => x.ClosedBy).Distinct().ToList();
                var closedbyname = db.Peoples.Where(x => closedby.Contains(x.PeopleID)).ToList();
               
                foreach (var item in data)
                {
                    item.ReportedByName = reportedname.Where(x => x.PeopleID == item.ReportedBy).Select(x => x.DisplayName).FirstOrDefault();
                    item.OutBoundName = outboundname.Where(x => x.PeopleID == item.OutBound).Select(x => x.DisplayName).FirstOrDefault();
                    item.InBoundName = inboundname.Where(x => x.PeopleID == item.InBound).Select(x => x.DisplayName).FirstOrDefault();
                    item.CityName = cityname.Where(x => x.Cityid == item.CityId).Select(x => x.CityName).FirstOrDefault();
                    item.DepartmentName = deptname.Where(x => x.DepId == item.Department).Select(x => x.DepName).FirstOrDefault();
                    item.WarehouseName = whname.Where(x => x.WarehouseId == item.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                    item.LocationName = locationname.Where(x => x.Cityid == item.Location).Select(x => x.CityName).FirstOrDefault();
                    item.ClosedByName = closedbyname.Where(x => x.PeopleID == item.ClosedBy).Select(x => x.DisplayName).FirstOrDefault();
                }
                return data;
            }
        }


        [Route("ClosingStatus/{CaseNo}/{isActive}/{Comment}")]
        [HttpGet]
        public bool ClosingStatus(string CaseNo, bool isActive, string Comment)
        {
            DateTime currentTime = DateTime.Now;
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var context = new AuthContext())
            {
                var closingstatus = context.IncidentReport.FirstOrDefault(x => x.CaseNo == CaseNo);

                if (closingstatus != null)
                {
                    closingstatus.Status = "Closed";
                    closingstatus.IsActive = !isActive;
                    closingstatus.UpdateDate = currentTime;
                    closingstatus.ClosedBy = userid;
                    closingstatus.ClosedComment = Comment;
                    context.Commit();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Route("ReOpenStatus/{CaseNo}/{isActive}/{Comment}")]
        [HttpGet]
        public bool ReOpenStatus(string CaseNo, bool isActive, string Comment)
        {
            DateTime currentTime = DateTime.Now;
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var context = new AuthContext())
            {
                var reopenstatus = context.IncidentReport.FirstOrDefault(x => x.CaseNo == CaseNo);

                if (reopenstatus != null)
                {
                    reopenstatus.Status = "ReOpen";
                    reopenstatus.IsActive = !isActive;
                    reopenstatus.UpdateDate = currentTime;
                    reopenstatus.ReOpenBy = userid;
                    reopenstatus.ReOpenComment = Comment;
                    context.Commit();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Route("CaseNoGenterated")]
        [HttpGet]
        public string CaseNoGenterated()
        {
            using (AuthContext db = new AuthContext())
            {
                var query = "select max(cast(replace(CaseNo,'CN','') as bigint)) from IncidentReports";
                var intCaseNo = db.Database.SqlQuery<long>(query).FirstOrDefault();
                var caseno = "CN" + (intCaseNo + 1);
                bool flag = false;
                while (flag == false)
                {
                    var result = db.IncidentReport.Any(s => s.CaseNo.Trim().ToLower() == caseno.Trim().ToLower());

                    if (!result)
                    {
                        flag = true;
                        return caseno;
                    }
                    else
                    {
                        intCaseNo += 1;
                        caseno = "CN" + intCaseNo;
                    }
                }

                return caseno;
            }
        }

    }
}


public class IncidentReportDc
{

    public int ReportedBy { get; set; }
    public string ReportedRole { get; set; }
    public string CaseNo { get; set; }
    public int OutBound { get; set; }
    public int InBound { get; set; }
    public string PersonName { get; set; }

    public string PersonRole { get; set; }

    public int Department { get; set; }

    public string TypeOfLoss { get; set; }
    public DateTime DateofIncident { get; set; }

    public int CityId { get; set; }
    public DateTime DateOfReport { get; set; }
    public DateTime TimeofIncident { get; set; }
    public string IncidentDescription { get; set; }
    public int Location { get; set; }
    public int WarehouseId { get; set; }
    public string WitnessName { get; set; }
    public string MobileNo { get; set; }
    //public bool IsDeleted { get; set; }
    //public bool IsActive { get; set; }
    //public string Status { get; set; }
    //public DateTime CreatedDate { get; set; }
}


public class IncidentReportListDc
{

    public int ReportedBy { get; set; }
    public string ReportedByName { get; set; }
    public string ReportedRole { get; set; }
    public string CaseNo { get; set; }
    public int OutBound { get; set; }
    public string OutBoundName { get; set; }
    public int InBound { get; set; }
    public string InBoundName { get; set; }
    public string PersonName { get; set; }

    public string PersonRole { get; set; }

    public int Department { get; set; }
    public string DepartmentName { get; set; }

    public string TypeOfLoss { get; set; }
    public DateTime DateofIncident { get; set; }

    public int CityId { get; set; }
    public string CityName { get; set; }
    public DateTime DateOfReport { get; set; }
    public string TimeofIncident { get; set; }
    public string IncidentDescription { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public int Location { get; set; }
    public string LocationName { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; }
    public int? ClosedBy { get; set; }
    public string ClosedByName { get; set; }
    public int? ReOpenBy { get; set; }
    public string ReOpenByName { get; set; }
    public string ClosedComment { get; set; }
    public string ReOpenComment { get; set; }
    public string WitnessName { get; set; }
    public string MobileNo { get; set; }

}