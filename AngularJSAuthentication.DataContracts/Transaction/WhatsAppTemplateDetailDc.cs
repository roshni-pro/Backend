using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction
{
    public class WhatsAppTemplateDetailDc
    {
        public WhatsAppTemplateDc WhatsAppTemplateDc { get; set; }
        public List<WhatsAppTemplateVariableDetailDc> WhatsAppTemplateVariableDetailDcs { get; set; }
        public List<WhatsAppTemplateValConfigurationDetailDc> WhatsAppTemplateValConfigurationDcs { get; set; }
        public List<WhatsAppCustomerDetail> whatsAppCustomerDetails { get; set; }

        
    }
    public class WhatsAppTemplateVariableDetailDc
    {
        public int WATemplateValConfigurationId { get; set; }
        public string ReplacingValue { get; set; }
        public string VariableType { get; set; }
        public string SQLQuary { get; set; }

    }
    public class WhatsAppTemplateValConfigurationDetailDc
    {
       
        public string textVal  { get; set; }
        public string SKCode { get; set; }
        public string VariableType { get; set; }
    }

    public class WhatsAppCustomerDetail
    {
        public string CustomerMobile { get; set; }
        public string SKCode { get; set; }
    }


}
