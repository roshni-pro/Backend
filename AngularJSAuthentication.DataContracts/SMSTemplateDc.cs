using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts
{
    public  class SMSTemplateDc
    { 
            public string Id { get; set; }    
            public string TemplateType { get; set; }
            public string DLTID { get; set; }
            public int AppType { get; set; }
            public string Template { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? ModifiedDate { get; set; }
            public bool IsActive { get; set; }
            public bool? IsDeleted { get; set; }
            public int CreatedBy { get; set; }
            public int? ModifiedBy { get; set; }
            public string AppTypeName { get; set; }
            public bool Result { get; set; }
            public string msg { get; set; }
        
    }
}
