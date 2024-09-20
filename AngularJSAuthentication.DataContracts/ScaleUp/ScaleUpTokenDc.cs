using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.ScaleUp
{
   public class ScaleUpTokenDc
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
    }

    public class ScaleUpConfigDc
    {
        public string Name { get; set; }
        public string ScaleUpUrl { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecretKey { get; set; }
        public string ApiUrl { get; set; }
    }
}
