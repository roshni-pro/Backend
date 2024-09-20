using AngularJSAuthentication.API.Helper.SupplierOnboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.GstVerifySupplier
{
    [RoutePrefix("api/SupplierApp")]
    public class GstverifyController : ApiController
    {
        [Route("suppliergstverify")]
        [HttpGet]
        public async Task<HttpResponseMessage> gstverify(string GstNo)
        {
            using (AuthContext db = new AuthContext())
            {
                //bool anyGstnNumberExists = db.SupplierTempDB.Any(x => x.GstInNumber == GstNo);
                //bool anyGstnNumberExistsSupplierWise = db.Suppliers.Any(x => x.TINNo == GstNo);
                //if (anyGstnNumberExists || anyGstnNumberExistsSupplierWise)
                //{
                //    var response = new
                //    {
                //        Status = false,
                //        Message = "GSTN Number already Exists..." + GstNo,
                //    };
                //    return Request.CreateResponse(HttpStatusCode.OK, response);
                //}
                //else
                //{
                    SupplierHelper supplierHelper = new SupplierHelper();
                    GSTdetailsDc i = await supplierHelper.GSTVeifyAsync(GstNo);
                    if (!i.IsVerify)
                    {
                        var response = new
                        {
                            Status = false,
                            Message = "GSTN Number Not Valid...."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                    else
                        return Request.CreateResponse(i);
                //}
            }
           
        }

        //[Route("Skcodegenerate")]
        //[HttpGet]
        public string getautoscode()
        {
            
            using (AuthContext db = new AuthContext())
            {
                bool flag = false;
               string code = "03000";

                //var query = "select top 1(cast(replace(SUPPLIERCODES,'SKS','') as bigint)) from suppliers where SUPPLIERCODES like '%SKS%' order by SupplierId desc";
                var query = "exec getskcode";
                var ii = db.Database.SqlQuery<string>(query).FirstOrDefault();
                //225

                //var ii = Convert.ToDouble(intSksCode);//intSksCode;
                if ((ii.Length >= code.Length ) && (int.Parse(ii)>=int.Parse(code)) )//225>0300
                {
                    flag = true;
                }
                else
                {
                    flag = false;
                }
               // var SUPPLIERCODES = "SKS" + (intSksCode + 1);
                string SKScode = "SKS";
               
                if (!flag)
                {
                    flag = true;
                    string newCode = SKScode + code;//SKS0300
                    return newCode;//sks03000
                }
                else
                {
                    string skscode;
                    int j = int.Parse(ii) + 1;
                    if (j.ToString().Length < 5)
                    {
                        //
                       skscode = SKScode + "0" + j.ToString();
                    }
                    else
                    {
                        skscode= SKScode + j.ToString();
                    }
                    
                    //int code = Convert.ToInt32(s1)+1; 
                    return skscode;
                }
                //var res = db.Suppliers.Select(x => x.SUPPLIERCODES).ToList();
                
            }
        }
        //public class scodeDc
        //{
        //    public int count { get; set; } = 0;
        //}
    }
    public class Gstindc
    {
        public List<string> Gstinnumber { get; set; }
    }

    
}