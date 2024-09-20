using AngularJSAuthentication.Model.Base.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helpers
{
    public class EntityReflactionHelper
    {
        public String GetClassType(Audit entity)
        {
            string name = entity.AuditEntity;
            return name;
        }

        public string GetPropertyType(Type entity)
        {
            string type = entity.Name;
            return type;
        }

        public object GetValue(Audit entity, string propertyName)
        {
            return entity.AuditFields.FirstOrDefault(x => x.FieldName == propertyName).NewValue;
        }
    }
}