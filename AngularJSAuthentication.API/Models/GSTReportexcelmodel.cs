using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Models
{
    public class GSTReportexcelmodel
    {
        public string ReportName { get; set; }
        public string PageTitle { get; set; }
        public string Tablename { get; set; }
        public DataTable DataTable { get; set; }
    }
}