using System.Data;

namespace AngularJSAuthentication.API.Models
{
    public class TurnAroundTimeReportModel
    {
        public string ReportName { get; set; }
        public string PageTitle { get; set; }

        public string Tablename { get; set; }

        public DataTable DataTable { get; set; }
    }
}