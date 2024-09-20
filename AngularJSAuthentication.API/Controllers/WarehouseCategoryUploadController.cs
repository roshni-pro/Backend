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

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/WarehouseCategoryUpload")]
    public class WarehouseCategoryUploadController : ApiController
    {
        [HttpPost]
        public void UploadFile()
        {
        }
    }

}
