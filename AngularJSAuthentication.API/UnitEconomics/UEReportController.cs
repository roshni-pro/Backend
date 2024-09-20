using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/uniteconomicreport")]
    public class UEReportController : ApiController
    {
       
        public static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
        [HttpGet]
        public dynamic GetMonth(DateTime? datefrom, DateTime? dateto, int Warehouseid, string lab1)
        {
            logger.Info("start OrderMaster: ");
            List<Reportsss> reports = new List<Reportsss>();
            DateTime start = DateTime.Parse("01-01-2017 00:00:00");
            DateTime end = DateTime.Today.AddDays(1);
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            int Warehouse_id = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                if (datefrom == null)
                {
                    datefrom = DateTime.Parse("01-01-2017 00:00:00");
                    dateto = DateTime.Today.AddDays(1);
                }
                else
                {   
                    start = datefrom.GetValueOrDefault();
                    end = dateto.GetValueOrDefault();
                }
            using (var context = new AuthContext())
            {
                var array1 = lab1.Split(',');
                foreach (var lab in array1)
                {
                    Reportsss reports1 = new Reportsss();
                    List<unitReport> uniqueMonth = new List<unitReport>();

                    List<UnitEconomic> list = context.UnitEconomicDb.Where(i => i.ExpenseDate >= datefrom && i.ExpenseDate <= dateto && i.WarehouseId == Warehouseid && i.Label1.Trim() == lab.Trim()).ToList();
                    List<OrderMaster> order = context.DbOrderMaster.Where(i => i.CreatedDate >= datefrom && i.CreatedDate <= dateto && i.WarehouseId == Warehouseid && i.Status!= "Post Order Canceled" 
                    && i.Status!= "Pending" && i.Status!= "Order Canceled" && i.Status!= "Failed" && i.Status!= "Dummy Order Cancelled" && i.Status!= "Delivery Canceled").ToList();
                    List<orderMasterlist> report = new List<orderMasterlist>();
                    if (list.Count != 0)
                    {
                        foreach (var a in order)
                        {
                            unitReport l = uniqueMonth.Where(x => x.createdDate.Month == a.CreatedDate.Month && x.createdDate.Year == a.CreatedDate.Year && x.name.Trim() == lab.Trim()).SingleOrDefault();
                            if (l == null)
                            {
                                unitReport unq = new unitReport();
                                unq.totalOrder += 1;
                                unq.totalSale = a.TotalAmount;
                                unq.name = lab.Trim();
                                unq.createdDate = new DateTime(a.CreatedDate.Year, a.CreatedDate.Month, 1);
                                List<UnitEconomic> unql = list.Where(x => x.ExpenseDate.Month == a.CreatedDate.Month && x.ExpenseDate.Year == a.CreatedDate.Year).ToList();
                                unq.totalExpence = 0;
                                if (unql.Count != 0)
                                {
                                    foreach (var ab in unql)
                                    {
                                        unq.totalExpence += ab.Amount;
                                    }
                                }
                                uniqueMonth.Add(unq);
                            }
                            else
                            {
                                l.totalOrder += 1;
                                l.totalSale = l.totalSale + a.TotalAmount;
                            }
                        }

                        reports1.reportts = uniqueMonth;
                        reports.Add(reports1);
                    }
                }
                return reports;
            }
        }
    }
    public class unitReport
    {
        public int totalOrder { get; set; }
        public double totalSale { get; set; }
        public double totalExpence { get; set; }
        public string name { get; set; }
        public DateTime createdDate { get; set; }
    }
    public class Reportsss
    {
        public List<unitReport> reportts { get; set; }
    }
}
