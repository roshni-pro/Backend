using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.DataContracts.APIParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.Expense
{
    [RoutePrefix("api/Vendor")]
    public class VendorController : ApiController
    {

        [Route("GetExpenseTDS")]
        [HttpGet]
        public List<ExpenseTDSList> GetExpenseTDSList()
        {
            using (var authContext = new AuthContext())
            {
                    List<ExpenseTDSList> expenseList = new List<ExpenseTDSList>();
                    var query = "Select ex.Id as ID,LD.Name as Name from ExpenseTDSMasters ex left Join Ladgers LD On ex.LedgerId = LD.ID  where LD.ObjectType='TDS' and IsDeleted = 0";
                    expenseList = authContext.Database.SqlQuery<ExpenseTDSList>(query).ToList();
                    return expenseList;
            }
        }
        [Route("AddVendor")]
        [HttpPost]
        public VendorDC AddVendor(VendorDC vendorDC) 
        {
            vendorDC.CreatedBy = GetUserId();
            VendorHelper vendorHelper = new VendorHelper();
            vendorDC = vendorHelper.AddVendor(vendorDC);
            return vendorDC;
        }

        [Route("GetVendorList")]
        [HttpPost]
        public VendorPageDC GetVendorList(VendorPager vendorPager)
        {
            VendorHelper vendorHelper = new VendorHelper();
            VendorPageDC vendorPageDC = new VendorPageDC();
            vendorPageDC = vendorHelper.GetList(vendorPager);
            return vendorPageDC;
        }
        
        [Route("GetVendorById")]
        [HttpGet]
        public VendorDC GetVendorById(int Id)
        {
            VendorHelper vendorHelper = new VendorHelper();
            VendorDC vendorDC = new VendorDC();
            vendorDC = vendorHelper.GetById(Id);
            return vendorDC;
        }
        private int GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }
        public class ExpenseTDSList {
            public long ID { get; set; }
            public string Name { get; set; }
        }
    }
}
