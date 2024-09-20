using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction
{
    public class WhatsAppTemplateDc
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }

        public string Language { get; set; }
        public string TemplateType { get; set; }
        public string ImageName { get; set; }
        public string ImagePath { get; set; }
        public string TemplateJson { get; set; }
        public string TemplateNewName { get; set; }
        public string TemplateDescription { get; set; }


        public List<WhatsAppTemplateVariableDetail> WhatsAppTemplateVariableDetails { get; set; }
    }
}
