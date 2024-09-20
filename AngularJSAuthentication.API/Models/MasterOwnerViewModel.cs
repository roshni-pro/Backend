using AngularJSAuthentication.Model;
using System.Collections.Generic;

namespace AngularJSAuthentication.API.Models
{
    public class MasterOwnerViewModel
    {
        public ExportMaster MasterObject { get; set; }
        public List<ExportMasterOwnerDTO> MasterOwnerList { get; set; }
    }
}