using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class CategoryMaster
    {
        public ObjectId Id { get; set; }
        public string CategoryType { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? ParentCategoryId { get; set; }
        public bool? IsDbValue { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
