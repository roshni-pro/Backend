using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.FileUpload
{
    public class Uploader
    {
        public string FileName { get; set; }
        public string RelativePath { get; set; }
        public string AbsoluteUrl { get; set; }
        public string SaveFileURL { get; set; }
    }
}
