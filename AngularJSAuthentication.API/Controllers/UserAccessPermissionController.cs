using AngularJSAuthentication.Model;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/UserAccessPermission")]
    public class UserAccessPermissionController : ApiController
    {

        [Route("")]
        [HttpGet]
        public List<UserAccessPermission> Get()
        {
            using (AuthContext dc = new AuthContext())
            {
                var data = dc.UserAccessPermissionDB.ToList();

                return data;
            }
        }
        [Route("")]
        [HttpGet]
        public object Get(string rollid)
        {
            using (AuthContext dc = new AuthContext())
            {

                var data = dc.UserAccessPermissionDB.Where(z => z.RoleId == rollid).SingleOrDefault();

                return data;
            }
        }
        [Route("")]
        [HttpPost]
        public HttpResponseMessage CreateLead(UserAccessPermission name)
        {
            using (AuthContext dc = new AuthContext())
            {
                var us = dc.UserAccessPermissionDB.Where(t => t.RoleId == name.RoleId).SingleOrDefault();
                us.Admin = name.Admin;
                us.AppAdmin = name.AppAdmin;
                us.CaseManagement = name.CaseManagement;
                us.Delivery = name.Delivery;
                us.PurchaseOrder = name.PurchaseOrder;
                us.TaxMaster = name.TaxMaster;
                us.Customer = name.Customer;
                us.Supplier = name.Supplier;
                us.Warehouse = name.Warehouse;
                us.CurrentStock = name.CurrentStock;
                us.OrderMaster = name.OrderMaster;
                us.DamageStock = name.DamageStock;
                us.ItemMaster = name.ItemMaster;
                us.Reports = name.Reports;
                us.StatisticalRep = name.StatisticalRep;
                us.Offer = name.Offer;
                us.Sales = name.Sales;
                us.AppPromotion = name.AppPromotion;
                us.ItemCategory = name.ItemCategory;
                us.CRM = name.CRM;
                us.Request = name.Request;
                us.UnitEconomics = name.UnitEconomics;
                us.PromoPoint = name.PromoPoint;
                us.News = name.News;
                us.CashManagment = name.CashManagment;
                us.Account = name.Account;
                dc.Commit();

                return Request.CreateResponse(HttpStatusCode.OK, "Success");
            }
        }
    }
}
