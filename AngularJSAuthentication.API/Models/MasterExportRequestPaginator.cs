using System;

namespace AngularJSAuthentication.API.Models
{
    public class MasterExportRequestPaginator
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public string Contains { get; set; }
        public int? RequesterID { get; set; }
        public int? ApproverID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}