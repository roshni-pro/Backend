using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction
{
    public class WhatsAppTemplateExcelDc
    {
        public string From_Number { get; set; }
        public string API_KEY { get; set; }
        public string To_Number { get; set; }
        public string Template_JSON { get; set; }

    }
}
