using AngularJSAuthentication.Model.Base;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class TreeStructure
    {
        public ObjectId id { get; set; }
        public string guid { get; set; }
        public string pguid { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public string Designation { get; set; }
        public string EmployeeLevel { get; set; }
        public string Department { get; set; }
        public string img { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
      
    }




    public class TreeStructureChildDc
    {
        public ObjectId id { get; set; }
        public string pid { get; set; } //object Id of  parent TreeStructure
        public string name { get; set; }
        public string EmployeeLevel { get; set; }
        public string title { get; set; }
        public string img { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }

    }
}
