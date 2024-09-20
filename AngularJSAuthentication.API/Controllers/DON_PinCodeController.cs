using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/DON_PinCode")]
  
    
    
    
    public class DON_PinCodeController : ApiController
    {
        
        
        
        
        
        [Route("GetPincodeList")]
        [HttpGet]
        public List<DON_PinCodeDc> GetAllPincodeData()
        {
            List<DON_PinCodeDc> List = new List<DON_PinCodeDc>();
            using (AuthContext context = new AuthContext())
            {
                List = context.AllDONPincode(context);
                return List;
            }
        }


        [Route("SubmitPincode")]
        [HttpPost]
        public DON_PinCodeDc SubmitPincodeData(DON_PinCodeDc DONPinCodeObj)
        {
            //List<DONPinCodeDc> List = new List<DONPinCodeDc>();
            using (AuthContext context = new AuthContext())
            {
                var tt = context.submitDONPincode(context, DONPinCodeObj);
                return tt;
                //return List;
            }
        }

        [Route("RemovePincode")]
        [HttpDelete]
        public int DeletePincodeData(int deleteID)
        {
            //List<DONPinCodeDc> List = new List<DONPinCodeDc>();
            using (AuthContext context = new AuthContext())
            {
                var tt = context.deleteDONPincode(deleteID);
                return tt;
                //return List;
            }
        }
    }
}

public class DON_PinCodeDc
{
    public int PinCode { get; set; }
    public int MappingWarehouseId { get; set; }
    public int DefaultClusterId { get; set; }
    public string WarehoueName { get; set; }
    public string ClusterName { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public int? id { get; set; }
    public string ErrorMsg { get; set; }

}
