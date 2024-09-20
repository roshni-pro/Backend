using AngularJSAuthentication.Model;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/WarehouseSupplierUpload")]
    public class WarehouseSupplierUploadController : ApiController
    {
        [HttpPost]
        public string UploadFile()
        {
            return "";
        }
    }

}
