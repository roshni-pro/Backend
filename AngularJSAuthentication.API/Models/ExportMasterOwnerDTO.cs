using System;

namespace AngularJSAuthentication.API.Models
{
    public class ExportMasterOwnerDTO
    {
        public int Id { get; set; }
        public int MasterId { get; set; }
        public int ApproverId { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public string MasterName { get; set; }

        public string Field { get; set; }
    }
}