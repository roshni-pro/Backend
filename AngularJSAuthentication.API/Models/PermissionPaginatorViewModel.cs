using System;

namespace AngularJSAuthentication.API.Models
{
    public class PermissionPaginatorViewModel
    {
        public string Contains { get; set; }
        public int First { get; set; }
        public int Last { get; set; }
        public string ColumnName { get; set; }
        public Boolean IsAscending { get; set; }
    }


    public class MasterExportViewModel
    {
        public string Contains { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}


