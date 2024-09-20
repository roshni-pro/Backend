using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AngularJSAuthentication.API.DataContract
{

    public class PageMasterDc
    {
        public long Id { get; set; }
        [StringLength(255)]
        [Required]
        public string PageName { get; set; }
        [StringLength(255)]
        [Required]
        public string RouteName { get; set; }
        [StringLength(255)]
        [Required]
        public string ClassName { get; set; }
        public string IconClassName { get; set; }
        public int Sequence { get; set; }
        public long? ParentId { get; set; }
        public bool IsNewPortalUrl { get; set; }
        public bool IsGroup2PortalUrl { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
    }


    public class ButtonMasterDc
    {
        public long Id { get; set; }
        [StringLength(255)]
        [Required]
        public string ButtonName { get; set; }
        [StringLength(255)]
        public string ButtonHtmlId { get; set; }
        [StringLength(255)]
        public string ButtonClassName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public bool IsChecked { get; set; }
    }

    public class PeoplePageAccessPermissionDc
    {
        public long Id { get; set; }
        public long PeopleId { get; set; }
        public long PageButtonId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string ButtonName { get; set; }

    }

    public class RolePagePermissionDc
    {

        public long RolePageId { get; set; }
        [StringLength(128)]
        [Required]
        public string RoleId { get; set; }
        public long PageMasterId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string PageName { get; set; }
        public long? ParentPageId { get; set; }
        public int Sequence { get; set; }
        public bool IsChecked { get; set; }

        public List<RolePagePermissionDc> ChildRolePagePermissionDcs { get; set; }
    }
    public class RolRequestDC
    {

        public int PeopleId { get; set; }
        [StringLength(128)]
        [Required]
        public string RoleId { get; set; }
        public DateTime? validFrom { get; set; }
        public DateTime? validTo { get; set; }
        public int ApproverId { get; set; }
        public bool Status { get; set; }
        public string RejectReason { get; set; }
        public DateTime? ApproveDt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int? UACId { get; set; }
        public bool UACApproved { get; set; }
        public int? PageId { get; set; }


    }

    public class PeoplePageDc
    {
        public long Id { get; set; }
        public string PageName { get; set; }
        public string RouteName { get; set; }
        public string ClassName { get; set; }
        public string IconClassName { get; set; }
        public int Sequence { get; set; }
        public long? ParentId { get; set; }
        public bool IsNewPortalUrl { get; set; }
        public bool IsGroup2PortalUrl { get; set; }

        public List<PeoplePageDc> ChildPageDcs { get; set; }
        public List<PeoplePageButtonDc> PeoplePageButtonDcs { get; set; }
    }

    public class PeoplePageButtonDc
    {
        public long Id { get; set; }
        public long PageId { get; set; }
        public string ButtonName { get; set; }
        public string ButtonHtmlId { get; set; }
        public string ButtonClassName { get; set; }
        public bool Active { get; set; }
    }


    public class OverrideRolePagePermissionDc
    {
        public long PeopePageId { get; set; }
        public long PeopleId { get; set; }
        public long PageMasterId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string PageName { get; set; }
        public long? ParentPageId { get; set; }
        public int Sequence { get; set; }
        public bool IsChecked { get; set; }

        public List<OverrideRolePagePermissionDc> OverrideRolePagePermissionDcs { get; set; }
    }

}