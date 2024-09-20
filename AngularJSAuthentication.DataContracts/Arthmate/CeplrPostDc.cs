using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Arthmate
{
    public class CeplrPostDc
    {
        public long Id { get; set; }

        public string filepath { get; set; }
        public string email { get; set; }
        public string ifsc_code { get; set; }
        public string fip_id { get; set; }
        public string callback_url { get; set; }
        public string mobile { get; set; }
        public string name { get; set; }
        public string file_password { get; set; }
        public string configuration_uuid { get; set; }
        public bool allow_multiple { get; set; }
        public int request_id { get; set; }
        public string token { get; set; }
        public bool last_file { get; set; }
    }
}
