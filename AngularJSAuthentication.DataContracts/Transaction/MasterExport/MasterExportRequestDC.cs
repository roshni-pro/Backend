using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.MasterExport
{
    public class MasterExportRequestDC
    {
        public long Id { get; set; }
        public int MasterId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsVerified { get; set; }
        public string Path { get; set; }
        public bool? IsGenerated { get; set; }
        public DateTime GeneratedDate { get; set; }
        public bool? IsEmailSent { get; set; }
        public int RequestedUserId { get; set; }
        public DateTime VerifiedDate { get; set; }
        public int ApproverId { get; set; }
        public string Parameter { get; set; }
        public string ParameterToShow { get; set; }
        public bool MarkAsBriefcase { get; set; }
        public string ApproverName { get; set; }
        public string RequesterName { get; set; }
    }


    public class MasterExportRequestOutput
    {
        public List<MasterExportRequestDC> MasterExportRequestListData { get; set; }
        public int Count { get; set; }
    }
}
