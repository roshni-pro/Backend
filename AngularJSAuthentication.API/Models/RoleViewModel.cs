using System.Collections.Generic;

namespace AngularJSAuthentication.API.Models
{
    public class RoleViewModel
    {
        public string Email { get; set; }
        public string UserId { get; set; }
        public string SearchedUserId { get; set; }
        public string RoleID { get; set; }
        public string RoleName { get; set; }
    }



    public class RoleComparatorViewModel
    {
        public List<RoleViewModel> OldRoles { get; set; }
        public List<RoleViewModel> NewRoles { get; set; }
    }
}