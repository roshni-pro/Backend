using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers.CRM;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.NotMapped;
using LinqKit;
using MongoDB.Bson;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/CRM")]
    public class CRMController : ApiController
    {
      
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);


        [Authorize]
        [Route("getcust")]
        public HttpResponseMessage get(int id, int idtype, DateTime start, DateTime end, string subsubcatname) //get customers 
        {
            try
            {

                logger.Info("getcustomerby level");
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                // Access claims
                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                }
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                if (compid > 0)
                {
                    var objsend = getcrmdashboarddata(id, idtype, start, end, "Kisan Kirana");

                    //if (months == 0) {
                    //    var firstDayOfMonth = new DateTime(start.Year, start.Month, 1);
                    //    var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                    //    var crmdata = getcrmdashboarddata(id, idtype, firstDayOfMonth, lastDayOfMonth, "Garner");
                    //    if (crmdata != null)
                    //    {
                    //        crmmnthdata.Add(crmdata);
                    //    }
                    //}


                    return Request.CreateResponse(HttpStatusCode.OK, objsend);

                }
                return Request.CreateResponse(HttpStatusCode.OK, "No data");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        //[Authorize]
        //[Route("getcust")]
        //public HttpResponseMessage get(string level, int clusterid) //get customers 
        //{
        //    try
        //    {
        //        DateTime start = DateTime.Now.AddDays(-30).Date;
        //        DateTime end = DateTime.Now.Date;
        //        logger.Info("getcustomerby level");
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        // Access claims
        //        foreach (Claim claim in identity.Claims)
        //        {
        //            if (claim.Type == "compid")
        //            {
        //                compid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "userid")
        //            {
        //                userid = int.Parse(claim.Value);
        //            }
        //        }
        //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

        //        if (level == "Aware/Intereseted")
        //        {//active but not ordered
        //            var customers = context.Customers.Where(x => x.ClusterId == clusterid && x.Active == true && x.ordercount == 0).ToList();
        //            return Request.CreateResponse(HttpStatusCode.OK, customers);
        //        }
        //        else if (level == "Ordered")//ordered once
        //        {
        //            var customers = context.Customers.Where(x => x.ClusterId == clusterid && x.Active == true && x.ordercount == 1).ToList();
        //            return Request.CreateResponse(HttpStatusCode.OK, customers);
        //        }
        //        else if (level == "Keep/DropCustomer") //keepor drop
        //        {
        //            var customers = context.Customers.Where(x => x.ClusterId == clusterid && x.Active == true).ToList();
        //            return Request.CreateResponse(HttpStatusCode.OK, customers);
        //        }
        //        else if (level == "EngagedCustomer")//>3 orders
        //        {
        //            //var clist = getcustomerordercounts(clusterid, start, end);
        //            //return Request.CreateResponse(HttpStatusCode.OK, clist);
        //        }
        //        else if (level == "RelyingCustomer")//>5 orders
        //        {

        //        }
        //        else if (level == "EnabledCustomer")//>10 orders
        //        {

        //        }
        //        else if (level == "EmpoweredCustomer")//>15 orders
        //        {

        //        }


        //        return Request.CreateResponse(HttpStatusCode.OK, "None");
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
        //    }
        //}

        public List<CRMMonthdata> getcrmdashboarddata(int id, int idtype, DateTime start, DateTime end, string subsubcatname)
        {

            using (AuthContext context = new AuthContext())
            {
                List<CRMMonthdata> crmmnthdata = new List<CRMMonthdata>();
                int months = (end.Year - start.Year) * 12 + end.Month - start.Month;
                //get Shopkirana brands list
                List<SubsubCategory> brands = context.SubsubCategorys.Where(x => x.SubcategoryName.Contains(subsubcatname) && x.Deleted == false).ToList();
                //get customers where by id 
                List<Customer> cs = new List<Customer>();
                if (idtype == 1)
                {
                    cs = context.Customers.Where(x => x.Cityid == id && x.Active == true).ToList();
                }
                else if (idtype == 2)
                {
                    cs = context.Customers.Where(x => x.Warehouseid == id && x.Active == true).ToList();
                }
                else if (idtype == 3)
                {
                    cs = context.Customers.Where(x => x.Cityid == id && x.Active == true).ToList();
                }
                //else if (idtype == 4)
                //{
                //    cs = context.Customers.Where(x => x.ExecutiveId == id && x.Active == true).ToList();
                //}
                else if (idtype == 5)
                {
                    cs = context.Customers.Where(x => x.ClusterId == id && x.Active == true).ToList();
                }

                for (int i = 0; i <= months; i++)
                {
                    DateTime st = start.Date.AddMonths(i);
                    var firstDayOfMonth = new DateTime(st.Year, st.Month, 1);
                    var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                    CRMMonthdata mdata = new CRMMonthdata();
                    try
                    {
                        mdata.dt = firstDayOfMonth;
                        CRMDashboard l0 = new CRMDashboard(); l0.level = 0;
                        CRMDashboard l1 = new CRMDashboard(); l1.level = 1;
                        CRMDashboard l2 = new CRMDashboard(); l2.level = 2;
                        CRMDashboard l3 = new CRMDashboard(); l3.level = 3;
                        CRMDashboard l4 = new CRMDashboard(); l4.level = 4;
                        CRMDashboard l5 = new CRMDashboard(); l5.level = 5;
                        l0.KisanKiranaqty = 0;
                        l0.customercount = 0;
                        l0.skbrandordercount = 0;
                        l0.brandsordered = 0;
                        l0.RetailerAppordercount = 0;
                        l0.SalesmanAppordercount = 0;
                        l0.volume = 0;
                        l1.customercount = 0;
                        l1.KisanKiranaqty = 0;
                        l1.skbrandordercount = 0;
                        l1.brandsordered = 0;
                        l1.RetailerAppordercount = 0;
                        l1.SalesmanAppordercount = 0;
                        l1.volume = 0;
                        l2.customercount = 0;
                        l2.KisanKiranaqty = 0;
                        l2.skbrandordercount = 0;
                        l2.brandsordered = 0;
                        l2.RetailerAppordercount = 0;
                        l2.SalesmanAppordercount = 0;
                        l2.volume = 0;
                        l3.customercount = 0;
                        l3.KisanKiranaqty = 0;
                        l3.skbrandordercount = 0;
                        l3.brandsordered = 0;
                        l3.RetailerAppordercount = 0;
                        l3.SalesmanAppordercount = 0;
                        l3.volume = 0;
                        l4.customercount = 0;
                        l4.KisanKiranaqty = 0;
                        l4.skbrandordercount = 0;
                        l4.brandsordered = 0;
                        l4.RetailerAppordercount = 0;
                        l4.SalesmanAppordercount = 0;
                        l4.volume = 0;
                        l5.customercount = 0;
                        l5.KisanKiranaqty = 0;
                        l5.skbrandordercount = 0;
                        l5.brandsordered = 0;
                        l5.RetailerAppordercount = 0;
                        l5.SalesmanAppordercount = 0;
                        l5.volume = 0;
                        List<CrmCustomer> cclist = new List<CrmCustomer>();
                        foreach (var c in cs)
                        {
                            double kkVolume = 0;

                            List<OrderMaster> orders = context.DbOrderMaster.Where(x => x.CustomerId == c.CustomerId && x.CreatedDate >= firstDayOfMonth && x.CreatedDate <= lastDayOfMonth && x.Deleted == false).Include("orderDetails").ToList();
                            if (subsubcatname == "Kisan Kirana")
                            {
                                var kkVolumes = context.DbOrderDetails.Where(x => x.CustomerId == c.CustomerId && x.OrderDate >= firstDayOfMonth && x.OrderDate <= lastDayOfMonth && x.SubsubcategoryName == subsubcatname).ToList();

                                foreach (var od in kkVolumes)
                                {
                                    kkVolume = kkVolume + od.TotalAmt;
                                }
                            }

                            int ordercnt = orders.Count();
                            int brandcnt = 0;
                            double volume = 0;
                            int RetailerAppordercount = 0;
                            int SalesmanAppordercount = 0;
                            int skbrandordercount = 0;
                            int canceled = 0;
                            int del = 0;
                            int redisp = 0;
                            int pending = 0;


                            CrmCustomer ccc = new CrmCustomer();

                            List<brandorderedlist> brandordered = new List<brandorderedlist>();
                            List<brandorderedlist> SKbrandordered = new List<brandorderedlist>();
                            foreach (var o in orders)
                            {
                                volume = volume + o.GrossAmount;
                                if (o.OrderTakenSalesPersonId == 0)
                                {
                                    RetailerAppordercount = RetailerAppordercount + 1;
                                }
                                else
                                {
                                    SalesmanAppordercount = SalesmanAppordercount + 1;
                                }
                                bool skod = false;
                                if (o.Status == "Pending")
                                {
                                    pending = pending + 1;
                                }
                                else if (o.Status == "Delivery Redispatch")
                                {
                                    redisp = redisp + 1;
                                }
                                else if (o.Status == "Delivered" || o.Status == "sattled" || o.Status == "Account settled" || o.Status == "Partial receiving -Bounce" || o.Status == "Partial settled")
                                {
                                    del = del + 1;
                                }
                                else if (o.Status == "Order Canceled" || o.Status == "Delivery Canceled")
                                {
                                    canceled = canceled + 1;
                                }

                                try
                                {
                                    foreach (var od in o.orderDetails)
                                    {
                                        var item = context.itemMasters.Where(x => x.ItemId == od.ItemId && x.WarehouseId == od.WarehouseId).FirstOrDefault();
                                        if (item != null)
                                        {
                                            bool check = brands.Any(x => x.SubsubCategoryid == item.SubsubCategoryid); //check if SK brand
                                            if (check)
                                            {
                                                if (!SKbrandordered.Any(x => x.brandordered == item.SubsubcategoryName)) //check for SK brands count
                                                {
                                                    SKbrandordered.Add(new brandorderedlist { brandordered = item.SubsubcategoryName });  //Skbrand = Skbrand + 1;
                                                }
                                                skod = true;
                                            }
                                            if (!brandordered.Any(x => x.brandordered == item.SubsubcategoryName))//check for all brands count
                                            {
                                                brandordered.Add(new brandorderedlist { brandordered = item.SubsubcategoryName });
                                                brandcnt = brandcnt + 1;
                                            }

                                        }
                                    }
                                }
                                catch (Exception sd)
                                {

                                }

                                if (skod) { skbrandordercount = skbrandordercount + 1; }
                            }



                            decimal app = 0;
                            decimal apps = 0;
                            if (ordercnt > 0)
                            {
                                app = (RetailerAppordercount / ordercnt) * 100;
                                apps = RetailerAppordercount / ((SalesmanAppordercount + RetailerAppordercount)) * 100;

                            }
                            if (ordercnt >= 12 && volume >= 500000 && brandcnt >= 50 && kkVolume >= 15000 && apps == 100)
                            {
                                l5.customercount = l5.customercount + 1;
                                l5.skbrandordercount = l5.skbrandordercount + skbrandordercount;
                                l5.brandsordered = l5.brandsordered + brandcnt;
                                l5.RetailerAppordercount = l5.RetailerAppordercount + RetailerAppordercount;
                                l5.SalesmanAppordercount = l5.SalesmanAppordercount + SalesmanAppordercount;
                                l5.volume = l5.volume + volume;
                            }
                            else if (ordercnt >= 12 && volume >= 50000 && brandcnt >= 50 && kkVolume >= 15000 && app >= 100)
                            {
                                l4.customercount = l4.customercount + 1;
                                l4.skbrandordercount = l4.skbrandordercount + skbrandordercount;
                                l4.brandsordered = l4.brandsordered + brandcnt;
                                l4.RetailerAppordercount = l4.RetailerAppordercount + RetailerAppordercount;
                                l4.SalesmanAppordercount = l4.SalesmanAppordercount + SalesmanAppordercount;
                                l4.volume = l4.volume + volume;
                            }
                            else if (ordercnt >= 5 && volume >= 20000 && brandcnt >= 10 && kkVolume >= 2000)
                            {
                                l3.customercount = l3.customercount + 1;
                                l3.skbrandordercount = l3.skbrandordercount + skbrandordercount;
                                l3.brandsordered = l3.brandsordered + brandcnt;
                                l3.RetailerAppordercount = l3.RetailerAppordercount + RetailerAppordercount;
                                l3.SalesmanAppordercount = l3.SalesmanAppordercount + SalesmanAppordercount;
                                l3.volume = l3.volume + volume;
                            }
                            else if (ordercnt >= 3 && volume >= 10000 && brandcnt >= 5)
                            {
                                l2.customercount = l2.customercount + 1;
                                l2.skbrandordercount = l2.skbrandordercount + skbrandordercount;
                                l2.brandsordered = l2.brandsordered + brandcnt;
                                l2.RetailerAppordercount = l2.RetailerAppordercount + RetailerAppordercount;
                                l2.SalesmanAppordercount = l2.SalesmanAppordercount + SalesmanAppordercount;
                                l2.volume = l2.volume + volume;
                            }
                            else if (ordercnt > 0)
                            {
                                l1.customercount = l1.customercount + 1;
                                l1.skbrandordercount = l1.skbrandordercount + skbrandordercount;
                                l1.brandsordered = l1.brandsordered + brandcnt;
                                l1.RetailerAppordercount = l1.RetailerAppordercount + RetailerAppordercount;
                                l1.SalesmanAppordercount = l1.SalesmanAppordercount + SalesmanAppordercount;
                                l1.volume = l1.volume + volume;

                            }
                            else
                            {
                                l0.customercount = l0.customercount + 1;
                                l0.skbrandordercount = l0.skbrandordercount + skbrandordercount;
                                l0.brandsordered = l0.brandsordered + brandcnt;
                                l0.RetailerAppordercount = l0.RetailerAppordercount + RetailerAppordercount;
                                l0.SalesmanAppordercount = l0.SalesmanAppordercount + SalesmanAppordercount;
                                l0.volume = l0.volume + volume;

                            }

                            ccc.ReportDate = mdata.dt;
                            ccc.CustomerId = c.CustomerId;
                            ccc.WarehouseName = c.Warehouseid.GetValueOrDefault();
                            ccc.Skcode = c.Skcode;
                            ccc.ShopName = c.ShopName;
                            ccc.Mobile = c.Mobile;
                            ccc.thisordercount = ordercnt;
                            ccc.thisordervalue = volume;
                            ccc.thisRAppordercount = RetailerAppordercount;
                            ccc.thisSAppordercount = SalesmanAppordercount;
                            ccc.thisordercountCancelled = canceled;
                            ccc.thisordercountdelivered = del;
                            ccc.thisordercountRedispatch = redisp;
                            ccc.thisordercountpending = pending;
                            ccc.noofbrands = brandcnt;
                            ccc.KisanKiranaVolumne = kkVolume;

                            if (ordercnt >= 12 && volume >= 500000 && brandcnt >= 50 && kkVolume >= 15000 && apps == 100)
                            {
                                ccc.Level = "Level 5";
                            }
                            else if (ordercnt >= 12 && volume >= 50000 && brandcnt >= 50 && kkVolume >= 15000 && app >= 100)
                            {
                                ccc.Level = "Level 4";
                            }
                            else if (ordercnt >= 5 && volume >= 20000 && brandcnt >= 10 && kkVolume >= 2000)
                            {
                                ccc.Level = "Level 3";
                            }
                            else if (ordercnt >= 3 && volume >= 10000 && brandcnt >= 5)
                            {
                                ccc.Level = "Level 2";
                            }
                            else if (ordercnt > 0)
                            {
                                ccc.Level = "Level 1";
                            }
                            else
                            {
                                ccc.Level = "Level 0";
                            }
                            cclist.Add(ccc);
                        }
                        mdata.L0 = l0;
                        mdata.L1 = l1;
                        mdata.L2 = l2;
                        mdata.L3 = l3;
                        mdata.L4 = l4;
                        mdata.L5 = l5;
                        mdata.customer = cclist;

                    }
                    catch (Exception ex)
                    { return null; }

                    if (mdata != null)
                    {
                        crmmnthdata.Add(mdata);
                    }
                }

                return crmmnthdata;
            }
        }

        ////Boc Pravesh 07-08-2019
        ////In CRM Report due to chart generation, in export list only current month 1st date was coming, so to resolve this issue, created to separate function for export.
        //[Authorize]
        //[Route("getcrmdashboarddataExport")]
        //public List<CRMMonthdata> getcrmdashboarddataExport(int id, int idtype, DateTime start, DateTime end, string subsubcatname)
        //{


        //    List<CRMMonthdata> crmmnthdata = new List<CRMMonthdata>();

        //    //get Shopkirana brands list
        //    List<SubsubCategory> brands = context.SubsubCategorys.Where(x => x.SubcategoryName.Contains(subsubcatname) && x.Deleted == false).ToList();
        //    //get customers where by id 
        //    List<Customer> cs = new List<Customer>();
        //    if (idtype == 1)
        //    {
        //        cs = context.Customers.Where(x => x.Cityid == id && x.Active == true).ToList();
        //    }
        //    else if (idtype == 2)
        //    {
        //        cs = context.Customers.Where(x => x.Warehouseid == id && x.Active == true).ToList();
        //    }
        //    else if (idtype == 3)
        //    {
        //        cs = context.Customers.Where(x => x.Cityid == id && x.Active == true).ToList();
        //    }
        //    else if (idtype == 4)
        //    {
        //        cs = context.Customers.Where(x => x.ExecutiveId == id && x.Active == true).ToList();
        //    }
        //    else if (idtype == 5)
        //    {
        //        cs = context.Customers.Where(x => x.ClusterId == id && x.Active == true).ToList();
        //    }


        //    CRMMonthdata mdata = new CRMMonthdata();
        //    try
        //    {

        //        CRMDashboard l0 = new CRMDashboard(); l0.level = 0;
        //        CRMDashboard l1 = new CRMDashboard(); l1.level = 1;
        //        CRMDashboard l2 = new CRMDashboard(); l2.level = 2;
        //        CRMDashboard l3 = new CRMDashboard(); l3.level = 3;
        //        CRMDashboard l4 = new CRMDashboard(); l4.level = 4;
        //        CRMDashboard l5 = new CRMDashboard(); l5.level = 5;
        //        l0.customercount = 0;
        //        l0.skbrandordercount = 0;
        //        l0.brandsordered = 0;
        //        l0.RetailerAppordercount = 0;
        //        l0.SalesmanAppordercount = 0;
        //        l0.volume = 0;
        //        l1.customercount = 0;
        //        l1.skbrandordercount = 0;
        //        l1.brandsordered = 0;
        //        l1.RetailerAppordercount = 0;
        //        l1.SalesmanAppordercount = 0;
        //        l1.volume = 0;
        //        l2.customercount = 0;
        //        l2.skbrandordercount = 0;
        //        l2.brandsordered = 0;
        //        l2.RetailerAppordercount = 0;
        //        l2.SalesmanAppordercount = 0;
        //        l2.volume = 0;
        //        l3.customercount = 0;
        //        l3.skbrandordercount = 0;
        //        l3.brandsordered = 0;
        //        l3.RetailerAppordercount = 0;
        //        l3.SalesmanAppordercount = 0;
        //        l3.volume = 0;
        //        l4.customercount = 0;
        //        l4.skbrandordercount = 0;
        //        l4.brandsordered = 0;
        //        l4.RetailerAppordercount = 0;
        //        l4.SalesmanAppordercount = 0;
        //        l4.volume = 0;
        //        l5.customercount = 0;
        //        l5.skbrandordercount = 0;
        //        l5.brandsordered = 0;
        //        l5.RetailerAppordercount = 0;
        //        l5.SalesmanAppordercount = 0;
        //        l5.volume = 0;
        //        List<CrmCustomer> cclist = new List<CrmCustomer>();
        //        foreach (var c in cs)
        //        {
        //            double kkVolume = 0;

        //            List<OrderMaster> orders = context.DbOrderMaster.Where(x => x.CustomerId == c.CustomerId && x.CreatedDate >= start && x.CreatedDate <= end && x.Deleted == false).Include("orderDetails").ToList();
        //            foreach (var o in orders)
        //            {
        //                mdata.dt = o.CreatedDate;
        //                foreach (var od in o.orderDetails)
        //                {

        //                    subsubcatname = od.SubsubcategoryName;
        //                    if (subsubcatname == "Kisan Kirana")
        //                    {
        //                        var kkVolumes = context.DbOrderDetails.Where(x => x.CustomerId == c.CustomerId && x.OrderDate >= start && x.OrderDate <= end && x.SubsubcategoryName == subsubcatname).ToList();

        //                        foreach (var odk in kkVolumes)
        //                        {
        //                            kkVolume = kkVolume + odk.TotalAmt;
        //                        }
        //                    }
        //                }
        //            }
        //            int ordercnt = orders.Count();
        //            int brandcnt = 0;
        //            double volume = 0;
        //            int RetailerAppordercount = 0;
        //            int SalesmanAppordercount = 0;
        //            int skbrandordercount = 0;
        //            int canceled = 0;
        //            int del = 0;
        //            int redisp = 0;
        //            int pending = 0;

        //            CrmCustomer ccc = new CrmCustomer();

        //            List<brandorderedlist> brandordered = new List<brandorderedlist>();
        //            List<brandorderedlist> SKbrandordered = new List<brandorderedlist>();
        //            foreach (var o in orders)
        //            {
        //                volume = volume + o.GrossAmount;
        //                if (o.OrderTakenSalesPersonId == 0)
        //                {
        //                    RetailerAppordercount = RetailerAppordercount + 1;
        //                }
        //                else
        //                {
        //                    SalesmanAppordercount = SalesmanAppordercount + 1;
        //                }
        //                bool skod = false;
        //                if (o.Status == "Pending")
        //                {
        //                    pending = pending + 1;
        //                }
        //                else if (o.Status == "Delivery Redispatch")
        //                {
        //                    redisp = redisp + 1;
        //                }
        //                else if (o.Status == "Delivered" || o.Status == "sattled" || o.Status == "Account settled" || o.Status == "Partial receiving -Bounce" || o.Status == "Partial settled")
        //                {
        //                    del = del + 1;
        //                }
        //                else if (o.Status == "Order Canceled" || o.Status == "Delivery Canceled")
        //                {
        //                    canceled = canceled + 1;
        //                }

        //                try
        //                {
        //                    foreach (var od in o.orderDetails)
        //                    {
        //                        var item = context.itemMasters.Where(x => x.ItemId == od.ItemId && x.WarehouseId == od.WarehouseId).FirstOrDefault();
        //                        if (item != null)
        //                        {
        //                            bool check = brands.Any(x => x.SubsubCategoryid == item.SubsubCategoryid); //check if SK brand
        //                            if (check)
        //                            {
        //                                if (!SKbrandordered.Any(x => x.brandordered == item.SubsubcategoryName)) //check for SK brands count
        //                                {
        //                                    SKbrandordered.Add(new brandorderedlist { brandordered = item.SubsubcategoryName });  //Skbrand = Skbrand + 1;
        //                                }
        //                                skod = true;
        //                            }
        //                            if (!brandordered.Any(x => x.brandordered == item.SubsubcategoryName))//check for all brands count
        //                            {
        //                                brandordered.Add(new brandorderedlist { brandordered = item.SubsubcategoryName });
        //                                brandcnt = brandcnt + 1;
        //                            }
        //                        }
        //                    }
        //                }
        //                catch (Exception sd)
        //                {

        //                }

        //                if (skod) { skbrandordercount = skbrandordercount + 1; }
        //            }

        //            decimal app = 0;
        //            decimal apps = 0;
        //            if (ordercnt > 0)
        //            {
        //                app = (RetailerAppordercount / ordercnt) * 100;
        //                apps = RetailerAppordercount / ((SalesmanAppordercount + RetailerAppordercount)) * 100;

        //            }
        //            if (ordercnt >= 12 && volume >= 500000 && brandcnt >= 50 && kkVolume >= 15000 && apps == 100)
        //            {
        //                l5.customercount = l5.customercount + 1;
        //                l5.skbrandordercount = l5.skbrandordercount + skbrandordercount;
        //                l5.brandsordered = l5.brandsordered + brandcnt;
        //                l5.RetailerAppordercount = l5.RetailerAppordercount + RetailerAppordercount;
        //                l5.SalesmanAppordercount = l5.SalesmanAppordercount + SalesmanAppordercount;
        //                l5.volume = l5.volume + volume;
        //            }
        //            else if (ordercnt >= 12 && volume >= 50000 && brandcnt >= 50 && kkVolume >= 15000 && app >= 100)
        //            {
        //                l4.customercount = l4.customercount + 1;
        //                l4.skbrandordercount = l4.skbrandordercount + skbrandordercount;
        //                l4.brandsordered = l4.brandsordered + brandcnt;
        //                l4.RetailerAppordercount = l4.RetailerAppordercount + RetailerAppordercount;
        //                l4.SalesmanAppordercount = l4.SalesmanAppordercount + SalesmanAppordercount;
        //                l4.volume = l4.volume + volume;
        //            }
        //            else if (ordercnt >= 5 && volume >= 20000 && brandcnt >= 10 && kkVolume >= 2000)
        //            {
        //                l3.customercount = l3.customercount + 1;
        //                l3.skbrandordercount = l3.skbrandordercount + skbrandordercount;
        //                l3.brandsordered = l3.brandsordered + brandcnt;
        //                l3.RetailerAppordercount = l3.RetailerAppordercount + RetailerAppordercount;
        //                l3.SalesmanAppordercount = l3.SalesmanAppordercount + SalesmanAppordercount;
        //                l3.volume = l3.volume + volume;
        //            }
        //            else if (ordercnt >= 3 && volume >= 10000 && brandcnt >= 5)
        //            {
        //                l2.customercount = l2.customercount + 1;
        //                l2.skbrandordercount = l2.skbrandordercount + skbrandordercount;
        //                l2.brandsordered = l2.brandsordered + brandcnt;
        //                l2.RetailerAppordercount = l2.RetailerAppordercount + RetailerAppordercount;
        //                l2.SalesmanAppordercount = l2.SalesmanAppordercount + SalesmanAppordercount;
        //                l2.volume = l2.volume + volume;
        //            }
        //            else if (ordercnt > 0)
        //            {
        //                l1.customercount = l1.customercount + 1;
        //                l1.skbrandordercount = l1.skbrandordercount + skbrandordercount;
        //                l1.brandsordered = l1.brandsordered + brandcnt;
        //                l1.RetailerAppordercount = l1.RetailerAppordercount + RetailerAppordercount;
        //                l1.SalesmanAppordercount = l1.SalesmanAppordercount + SalesmanAppordercount;
        //                l1.volume = l1.volume + volume;
        //            }
        //            else
        //            {
        //                l0.customercount = l0.customercount + 1;
        //                l0.skbrandordercount = l0.skbrandordercount + skbrandordercount;
        //                l0.brandsordered = l0.brandsordered + brandcnt;
        //                l0.RetailerAppordercount = l0.RetailerAppordercount + RetailerAppordercount;
        //                l0.SalesmanAppordercount = l0.SalesmanAppordercount + SalesmanAppordercount;
        //                l0.volume = l0.volume + volume;
        //            }


        //            if (mdata.dt.ToString() != "01-01-0001 00:00:00")
        //            {
        //                ccc.ReportDate = mdata.dt;
        //                ccc.CustomerId = c.CustomerId;
        //                ccc.WarehouseName = c.Warehouseid.GetValueOrDefault();
        //                ccc.Skcode = c.Skcode;
        //                ccc.ShopName = c.ShopName;
        //                ccc.Mobile = c.Mobile;
        //                ccc.thisordercount = ordercnt;
        //                ccc.thisordervalue = volume;
        //                ccc.thisRAppordercount = RetailerAppordercount;
        //                ccc.thisSAppordercount = SalesmanAppordercount;
        //                ccc.thisordercountCancelled = canceled;
        //                ccc.thisordercountdelivered = del;
        //                ccc.thisordercountRedispatch = redisp;
        //                ccc.thisordercountpending = pending;
        //                ccc.noofbrands = brandcnt;
        //                ccc.KisanKiranaVolumne = kkVolume;
        //                if (ordercnt >= 12 && volume >= 500000 && brandcnt >= 50 && kkVolume >= 15000 && apps == 100)
        //                {
        //                    ccc.Level = "Level 5";
        //                }
        //                else if (ordercnt >= 12 && volume >= 50000 && brandcnt >= 50 && kkVolume >= 15000 && app >= 100)
        //                {
        //                    ccc.Level = "Level 4";
        //                }
        //                else if (ordercnt >= 5 && volume >= 20000 && brandcnt >= 10 && kkVolume >= 2000)
        //                {
        //                    ccc.Level = "Level 3";
        //                }
        //                else if (ordercnt >= 3 && volume >= 10000 && brandcnt >= 5)
        //                {
        //                    ccc.Level = "Level 2";
        //                }
        //                else if (ordercnt > 0)
        //                {
        //                    ccc.Level = "Level 1";
        //                }
        //                else
        //                {
        //                    ccc.Level = "Level 0";
        //                }
        //                cclist.Add(ccc);
        //            }

        //        }
        //        mdata.L0 = l0;
        //        mdata.L1 = l1;
        //        mdata.L2 = l2;
        //        mdata.L3 = l3;
        //        mdata.L4 = l4;
        //        mdata.L5 = l5;
        //        if (cclist.Count > 0)
        //        {
        //            mdata.customer = cclist.OrderBy(x => x.ReportDate).ToList();
        //        }

        //    }
        //    catch (Exception ex)
        //    { return null; }

        //    if (mdata != null)
        //    {
        //        crmmnthdata.Add(mdata);
        //    }


        //    return crmmnthdata;
        //}
        ////Eoc Pravesh 07-08-2019

        [Route("AllCustomer")]
        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage AllCustomer()
        {
            try
            {
                using (var con = new AuthContext())
                {
                    DateTime now = indianTime;
                    DateTime startDate = new DateTime(now.Year, now.Month, 1);//now.Hour, now.Minute, now.Second, now.Kind
                    DateTime endDate = startDate.AddMonths(1).AddDays(-1).AddHours(now.Hour).AddMinutes(now.Minute);

                    //var customer = con.Customers.Where(x => x.Deleted == false).Count();
                    CustomerCount ObjCRMCustomer = new CustomerCount
                    {
                        TotalCustomer = con.Customers.Count(),
                        TotalActiveCustomer = con.Customers.Where(x => x.Deleted == false && x.Active == true).Count(),
                        TotalActiveCustomerByOrder = con.DbOrderMaster.Where(o => o.Deleted == false && o.CreatedDate >= startDate && o.CreatedDate <= endDate && o.Status != "Payment Pending" && o.Status != "Inactive" && o.Status != "Failed" && o.Status != "Dummy Order Cancelled").Select(x => x.Skcode).Distinct().Count()
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, ObjCRMCustomer);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }

        }


        [Route("GenearteCRMDashboard")]
        [HttpPost]
        public async Task<string> GenearteCRMDashboard(int Month, int Year)
        {
            List<CRMCustomerLevel> CustomerList = new List<CRMCustomerLevel>();
            int month = Month;
            int year = Year;
            if (Year > 0 && Month > 0)
            {
                using (var context = new AuthContext())
                {
                    bool IsmonthYearexits = context.CRMCustomerLevels.Any(x => x.Month == month && x.Year == year);
                    if (!IsmonthYearexits)
                    {
                        CustomerList = await ReadCRMCustomerLevel(month, year, context);
                    }
                    else
                    {
                        return "Already Inserted";
                    }
                }
                if (CustomerList != null && CustomerList.Any())
                {
                    string Inserted = InsertCRMCustomerLevel(CustomerList);
                    return Inserted;
                }
            }
            return "Month or year is null";
        }
        public async Task<List<CRMCustomerLevel>> ReadCRMCustomerLevel(int month, int year, AuthContext context)
        {
            List<CRMCustomerLevel> CustomerList = new List<CRMCustomerLevel>();
            var monthParam = new SqlParameter("@month", month);
            var yearParam = new SqlParameter("@year", year);
            context.Database.CommandTimeout = 180;
            var OrdersList = await context.Database.SqlQuery<CustDetailLabelResponse>("CRMLevelCustomerDetail @month,@year", monthParam, yearParam).ToListAsync();
            if (OrdersList != null && OrdersList.Any())
            {
                CustomerList = Mapper.Map(OrdersList).ToANew<List<CRMCustomerLevel>>();
                if (CustomerList != null && CustomerList.Any())
                {
                    foreach (var GetLevel in CustomerList)
                    {
                        GetLevel.CreatedBy = 1;
                        GetLevel.CreatedDate = indianTime;
                        GetLevel.Month = month;
                        GetLevel.Year = year;
                        bool IsLevelAdded = false;
                        if (GetLevel.OrderCount >= 12 && GetLevel.Volume >= 75000 && GetLevel.BrandCount >= 40 && GetLevel.KKvolume >= 15000 && (GetLevel.SelfOrderCount / (GetLevel.OrderCount) * 100) > 60)
                        {
                            GetLevel.Level = 5;
                            IsLevelAdded = true;
                        }
                        else if (GetLevel.OrderCount >= 8 && GetLevel.Volume >= 30000 && GetLevel.BrandCount >= 20 && GetLevel.KKvolume >= 8000 && (GetLevel.SelfOrderCount / (GetLevel.OrderCount) * 100) > 30)
                        {
                            GetLevel.Level = 4;
                            IsLevelAdded = true;
                        }
                        else if (GetLevel.OrderCount >= 5 && GetLevel.Volume >= 20000 && GetLevel.BrandCount >= 10 && GetLevel.KKvolume >= 2000)
                        {
                            GetLevel.Level = 3;
                            IsLevelAdded = true;
                        }
                        else if (GetLevel.OrderCount >= 3 && GetLevel.Volume >= 10000 && GetLevel.BrandCount >= 5)
                        {
                            GetLevel.Level = 2;
                            IsLevelAdded = true;
                        }
                        else if (GetLevel.OrderCount > 0)
                        {
                            GetLevel.Level = 1;
                            IsLevelAdded = true;
                        }
                        else if (GetLevel.OrderCount == 0)
                        {
                            GetLevel.Level = 0;
                            IsLevelAdded = true;
                        }
                        else
                        {
                            if (!IsLevelAdded)
                            {
                                GetLevel.Level = -0;
                            }
                        }
                    }
                }
            }
            return CustomerList;
        }
        public async Task<List<CustomersCRMDC>> ReadCRMCustomerLevelExport(int month, int year, AuthContext context)
        {
            List<CustomersCRMDC> CustomerList = new List<CustomersCRMDC>();
            var monthParam = new SqlParameter("@month", month);
            var yearParam = new SqlParameter("@year", year);
            context.Database.CommandTimeout = 180;

            var OrdersList = await context.Database.SqlQuery<CustDetailLabelResponse>("CRMLevelCustomerDetail @month,@year", monthParam, yearParam).ToListAsync();
            if (OrdersList != null && OrdersList.Any())
            {
                CustomerList = Mapper.Map(OrdersList).ToANew<List<CustomersCRMDC>>();
                if (CustomerList != null && CustomerList.Any())
                {
                    foreach (var GetLevel in CustomerList)
                    {
                        GetLevel.CreatedBy = 1;
                        GetLevel.CreatedDate = indianTime;
                        GetLevel.Month = month;
                        GetLevel.Year = year;
                        bool IsLevelAdded = false;
                        if (GetLevel.OrderCount >= 12 && GetLevel.Volume >= 75000 && GetLevel.BrandCount >= 40 && GetLevel.KKvolume >= 15000 && (GetLevel.SelfOrderCount / (GetLevel.OrderCount) * 100) > 60)
                        {
                            GetLevel.Level = 5;
                            IsLevelAdded = true;
                        }
                        else if (GetLevel.OrderCount >= 8 && GetLevel.Volume >= 30000 && GetLevel.BrandCount >= 20 && GetLevel.KKvolume >= 8000 && (GetLevel.SelfOrderCount / (GetLevel.OrderCount) * 100) > 30)
                        {
                            GetLevel.Level = 4;
                            IsLevelAdded = true;
                        }
                        else if (GetLevel.OrderCount >= 5 && GetLevel.Volume >= 20000 && GetLevel.BrandCount >= 10 && GetLevel.KKvolume >= 2000)
                        {
                            GetLevel.Level = 3;
                            IsLevelAdded = true;
                        }
                        else if (GetLevel.OrderCount >= 3 && GetLevel.Volume >= 10000 && GetLevel.BrandCount >= 5)
                        {
                            GetLevel.Level = 2;
                            IsLevelAdded = true;
                        }
                        else if (GetLevel.OrderCount > 0)
                        {
                            GetLevel.Level = 1;
                            IsLevelAdded = true;
                        }
                        else if (GetLevel.OrderCount == 0)
                        {
                            GetLevel.Level = 0;
                            IsLevelAdded = true;
                        }
                        else
                        {
                            if (!IsLevelAdded)
                            {
                                GetLevel.Level = -0;
                            }
                        }
                    }
                }
            }
            return CustomerList;
        }
        public string InsertCRMCustomerLevel(List<CRMCustomerLevel> InsertedList)
        {
            if (InsertedList != null && InsertedList.Any())
            {
                using (var conn = new SqlConnection(DbConstants.AuthContextDbConnection))
                {
                    conn.Open();
                    using (SqlBulkCopy copy = new SqlBulkCopy(conn))
                    {
                        copy.BulkCopyTimeout = 3600;
                        copy.BatchSize = 20000;
                        copy.DestinationTableName = "[dbo].[CRMCustomerLevels]";
                        DataTable table = ClassToDataTable.CreateDataTable(InsertedList);
                        table.TableName = "[dbo].[CRMCustomerLevels]";
                        copy.WriteToServer(table);
                    }
                }
            }
            return "Data Inserted Successfully";
        }

        [Route("CurrentMonthCRMDashboard")]
        [HttpGet]

        public async Task<CRMCustomerLevelMaster> CurrentMonthCRMDashboard(int Month, int Year)
        {
            CRMCustomerLevelMaster Level = new CRMCustomerLevelMaster();
            int month = Month;
            int year = Year;
            if (Year > 0 && Month > 0)
            {
                using (var context = new AuthContext())
                {
                    List<CRMCustomerLevel> CustomerList = new List<CRMCustomerLevel>();
                    CustomerList = await ReadCRMCustomerLevel(month, year, context);

                    if (CustomerList != null && indianTime.Month > month && indianTime.Year == year)
                    {
                        bool IsmonthYearexits = context.CRMCustomerLevels.Any(x => x.Month == month && x.Year == year);
                        if (!IsmonthYearexits && CustomerList != null && CustomerList.Any())
                        {
                            string Inserted = InsertCRMCustomerLevel(CustomerList);
                        }
                    }
                    if (CustomerList != null && CustomerList.Any())
                    {
                        Level.L0 = CustomerList.Count(x => x.Level == 0);
                        Level.L1 = CustomerList.Count(x => x.Level == 1);
                        Level.L2 = CustomerList.Count(x => x.Level == 2);
                        Level.L3 = CustomerList.Count(x => x.Level == 3);
                        Level.L4 = CustomerList.Count(x => x.Level == 4);
                        Level.L5 = CustomerList.Count(x => x.Level == 5);
                        Level.Month = month;
                        Level.Year = year;
                        // Level.CRMCustomerLevelDc = CustomerList;        
                    }
                }
            }

            return Level;
        }




        [Route("CurrentMonthCRMExport")]
        [HttpGet]
        public async Task<List<CRMCustomerLevelExportDc>> CurrentMonthCRMExport(int Month, int Year)
        {
            List<CRMCustomerLevelExportDc> ExportCRMMonthYear = new List<CRMCustomerLevelExportDc>();

            int month = Month;
            int year = Year;
            if (Year > 0 && Month > 0)
            {
                using (var context = new AuthContext())
                {
                    List<CustomersCRMDC> CustomerList = new List<CustomersCRMDC>();
                    CustomerList = await ReadCRMCustomerLevelExport(month, year, context);


                    if (CustomerList != null && CustomerList.Any())
                    {
                        ExportCRMMonthYear = Mapper.Map(CustomerList).ToANew<List<CRMCustomerLevelExportDc>>();
                    }
                }
            }
            return ExportCRMMonthYear;
        }


        [Route("CRMYearDashboard")]
        [HttpGet]
        public async Task<List<CRMCustomerLevelMaster>> CRMYearDashboard(int Year)
        {
            using (AuthContext context = new AuthContext())
            {
                var CRMMonthYearDashBoard = new List<CRMCustomerLevelMaster>();
                //if (CRMMonthYearDashBoard != null && CRMMonthYearDashBoard.Any())
                //{
                var yearParam = new SqlParameter("@year", Year);
                CRMMonthYearDashBoard = await context.Database.SqlQuery<CRMCustomerLevelMaster>("GetCRMYearDashboard @year", yearParam).ToListAsync();
                // }
                return CRMMonthYearDashBoard;
            }
        }

        [Route("ExportMonthYear")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<CRMCustomerLevelExportDc>> ExportMonthYearCRMDashboard(int Month, int Year)
        {
            List<CRMCustomerLevelExportDc> ExportCRMMonthYear = null;

            int month = Month;
            int year = Year;
            if (Year > 0 && Month > 0)
            {
                using (var context = new AuthContext())
                {
                    var monthParam = new SqlParameter("@month", month);
                    var yearParam = new SqlParameter("@year", year);
                    context.Database.CommandTimeout = 180;
                    ExportCRMMonthYear = await context.Database.SqlQuery<CRMCustomerLevelExportDc>("ExportCRMMonthYearDashboard @month, @year", monthParam, yearParam).ToListAsync();

                }
            }
            return ExportCRMMonthYear;
        }
        
        [AllowAnonymous]
        [HttpGet]
        [Route("InsertCRMtoCRMDetail")]
        public async Task<bool> InsertCRMtoCRMDetail()
        {
            using (var httpClient = new HttpClient())
            {
                List<KeyValuePair<string, IEnumerable<string>>> extraDataAsHeader = new List<KeyValuePair<string, IEnumerable<string>>>();
                extraDataAsHeader.Add(new KeyValuePair<string, IEnumerable<string>>("NoEncryption", new List<string> { "1" }));
               // extraDataAsHeader.Add(new KeyValuePair<string, IEnumerable<string>>("Authorization", new List<string> { "Bearer 1AmGtIIj9MbvAFbr0LH5zU_VXWUCwW9wVjjW52cbIZocFUQL4acmCGeS3i-l_Z-nD6rbvaKtLfZkm2KeDTwM5yy-4ZiSmED8ofIOBPDWvtXy9-BhDTYtdxGJHahkzZLApjhKR8rndMyHcUnJ_t0M0V2hOFg973niNMfrVAqItroNS9Kc7c1itbS2dQTkYWlk9y-i1MIw51A3B4YOzD0RtGehwhjBSU3eIr0XDpV83Oucb_NnXuBKHfM_30b50MDbM2hpsgHK_zlDF-XY-TZVclnaGy0dHm4JWUw3pNyhMMCPDUmxNNxte4GC3dUP1LATUzBEobmTGSb6tVCt7_hR0ni5fYTa0b1VoOz5US93oguigI8CePeDA2k7HjTjvtB5qGx7OD0WeU14h2qOXOV1qZpk2eLq4dIFjskW1S2OE4LyHs-zyyYAtlb1Qi5K35TntKKRoerQaiIS4z8s0K1Mj4fsW5AmRHE0HVTRn5VeIKuVce2X6nw-Q7CVvU3d_YRdLwTWFScqcsmhrQW7vtDMokKbCPV2zbQjviPYkwOFSVY" }));
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using (var httpclient = new GenericRestHttpClient<dynamic, bool>(ConfigurationManager.AppSettings["CRMAPIUrl"], "api/CRMTag/InsertIntoIndex", extraDataAsHeader))
                {
                    try
                    {
                        bool res = false;
                        bool result = await httpclient.GetAsync();
                        if(result)
                        {
                            CRMManager cRMManager = new CRMManager();
                            res = await cRMManager.GetCRMPDetail();
                        }
                        return res;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }
    }

    public class brandorderedlist
    {
        public string brandordered { get; set; }
    }

    public class CRMDashboard
    {
        public int level { get; set; }
        public int customercount { get; set; }
        public int skbrandordercount { get; set; }
        public double volume { get; set; }
        public int brandsordered { get; set; }
        public int RetailerAppordercount { get; set; }
        public int SalesmanAppordercount { get; set; }

        public int KisanKiranaqty { get; set; }
    }
    public class CRMMonthdata
    {
        public CRMDashboard L0 { get; set; }
        public CRMDashboard L1 { get; set; }
        public CRMDashboard L2 { get; set; }
        public CRMDashboard L3 { get; set; }
        public CRMDashboard L4 { get; set; }
        public CRMDashboard L5 { get; set; }
        public DateTime dt { get; set; }
        public List<CrmCustomer> customer { get; set; }

    }
    public class CrmCustomer
    {
        public int CustomerId { get; set; }
        public DateTime ReportDate { get; set; }
        public int WarehouseName { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string Mobile { get; set; }
        public int thisordercount { get; set; }
        public double thisordervalue { get; set; }
        public int thisordercountpending { get; set; }
        public int thisordercountdelivered { get; set; }
        public int thisordercountRedispatch { get; set; }
        public int thisordercountCancelled { get; set; }
        public int thisRAppordercount { get; set; }
        public int thisSAppordercount { get; set; }
        public int noofbrands { get; set; }
        public double Volume { get; set; }
        public double KisanKiranaVolumne { get; set; }
        public string Level { get; set; }
        public int KisanKiranaQ { get; set; }
    }
    //Db Customer access
    public class CRMDashCustDTO
    {
        public int CustomerId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Skcode { get; set; }
        public string WarehouseName { get; set; }
        public string Mobile { get; set; }
        public bool Active { get; set; }
    }

    //for each customer
    public class CRMCustomerDashBoardReport
    {
        public string Skcode { get; set; }
        public string WarehouseName { get; set; }
        public string Mobile { get; set; }
        public int OrderCount { get; set; }
        public int CustomerId { get; set; }
        public int BrandCount { get; set; }
        public int SelfOrderCount { get; set; }
        public double Volume { get; set; }
        public double KKvolume { get; set; }
        public int Level { get; set; }
        public bool Active { get; set; }

    }
    //Month wise report
    public class CRMMonthYearDashBoard
    {
        public ObjectId Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int L0 { get; set; }
        public int L1 { get; set; }
        public int L2 { get; set; }
        public int L3 { get; set; }
        public int L4 { get; set; }
        public int L5 { get; set; }
        public List<CRMCustomerDashBoardReport> CRMCustomerDashBoardReport { get; set; }

    }



    public class CRMCustomerLevelExportDc
    {
        public string Skcode { get; set; }
        public int BrandCount { get; set; }
        public double Volume { get; set; }
        public double KKvolume { get; set; }
        public int OrderCount { get; set; }
        public int SelfOrderCount { get; set; }
        public int Salespersonordercount { get; set; }
        public int Level { get; set; }

    }

    public class CRMCustomerLevelDc
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int OrderCount { get; set; }
        public int CustomerId { get; set; }
        public int BrandCount { get; set; }
        public string Skcode { get; set; }
        public int SelfOrderCount { get; set; }
        public double Volume { get; set; }
        public double KKvolume { get; set; }
        public int Level { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int Salespersonordercount { get; set; }
    }
    public class CRMCustomerLevelMaster
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int L0 { get; set; }
        public int L1 { get; set; }
        public int L2 { get; set; }
        public int L3 { get; set; }
        public int L4 { get; set; }
        public int L5 { get; set; }
        public List<CRMCustomerLevelDc> CRMCustomerLevelDc { get; set; }
    }
    public class CustomerCount
    {
        public int TotalCustomer { get; set; }
        public int TotalActiveCustomer { get; set; }
        public int TotalActiveCustomerByOrder { get; set; }
    }
    public class CustomersCRMDC
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int OrderCount { get; set; }
        public int CustomerId { get; set; }
        public int BrandCount { get; set; }
        public int SelfOrderCount { get; set; }
        public double Volume { get; set; }
        public double KKvolume { get; set; }
        public int Level { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int Salespersonordercount { get; set; }
        public string Skcode { get; set; }
    }
}


