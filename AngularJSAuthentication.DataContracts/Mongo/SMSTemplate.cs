using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.Model
{
    public class SMSTemplate
    {
        [BsonId]
        public ObjectId Id { get; set; }
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
        
        [NotMapped]
        public string Msg { get; set; }

    }
}
