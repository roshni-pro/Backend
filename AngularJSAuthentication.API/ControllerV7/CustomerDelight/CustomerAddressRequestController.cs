using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.Masters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.CustomerDelight
{
    [RoutePrefix("api/CustomerAddressRequest")]
    public class CustomerAddressRequestController : BaseApiController
    {
        [Route("GetList")]
        [HttpGet]
        public List<CustomerAddressRequestDc> GetList(int warehouseId, int skip, int take)
        {
            CustomerAddressRequestManager manager = new CustomerAddressRequestManager();
            var list = manager.GetList(warehouseId, skip, take);
            return list;
        }

        [Route("GetLists")]
        [HttpGet]
        public List<CustomerAddressOperationRequestVM> GetLists(int warehouseId, int skip, int take)
        {
            CustomerAddressRequestManager manager = new CustomerAddressRequestManager();
            var list = manager.GetLists(warehouseId, skip, take);
            return list;
        }

        [Route("MakeRequest")]
        [HttpPost]
        public bool MakeRequest(CustomerAddressRequestDc request)
        {
            CustomerAddressRequestManager manager = new CustomerAddressRequestManager();
            bool result = manager.MakeRequest(request);
            return result;
        }

        [Route("Approve")]
        [HttpPost]
        public bool Approve(CustomerAddressOperationRequestVM obj)
        {
            int userId = 0;
            var identity = User.Identity as ClaimsIdentity;
            foreach (Claim claim in identity.Claims)
            {
                if (claim.Type == "userid")
                {
                    userId = int.Parse(claim.Value);
                }
            }
            CustomerAddressRequestManager manager = new CustomerAddressRequestManager();
            bool result = manager.Approve(obj, userId);
            return result;
        }
     
        [Route("Reject")]
        [HttpPost]
        public bool Reject(CustomerAddressOperationRequestVM obj)
        {
            int userId = 0;
            var identity = User.Identity as ClaimsIdentity;
            foreach (Claim claim in identity.Claims)
            {
                if (claim.Type == "userid")
                {
                    userId = int.Parse(claim.Value);
                }
            }
            CustomerAddressRequestManager manager = new CustomerAddressRequestManager();
            bool result = manager.Reject(obj, userId);
            return result;
        }

        [Route("GetCount")]
        [HttpGet]
        public int GetCount(int warehouseId)
        {
            CustomerAddressRequestManager manager = new CustomerAddressRequestManager();
            int count = manager.GetCount(warehouseId);
            return count;
        }
    }
}
