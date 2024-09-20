using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.Reporting
{
    [RoutePrefix("api/OrderReporting")]
    public class OrderReportingController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("GetDboyOrderDeliveryData")]
        [HttpGet]
        public List<OrderDBoyDeliverData> GetDboyOrderDeliveryData(string dBoyMobileNo, int? warehouseid, string fromDate, string toDate)
        {
            DateTime start = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            DateTime end = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            List<OrderDBoyDeliverData> OrderDBoyDeliverDatas = new List<OrderDBoyDeliverData>();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    string whareclase = "";

                    if (fromDate != null && toDate != null)
                        whareclase += " And (Cast(a.DeliveredDate as date) >= cast('" + start.Date.ToString("yyyy-MM-dd") + "' as date) and cast(a.DeliveredDate as date) <= cast('" + end.Date.ToString("yyyy-MM-dd") + "' as  date))";

                    if (warehouseid.HasValue)
                        whareclase += " And a.WarehouseId=" + warehouseid;

                    if (!string.IsNullOrEmpty(dBoyMobileNo))
                        whareclase += " And b.DboyMobileNo='" + dBoyMobileNo + "'";

                    string sqlquery = "Select c.DboyName,c.WarehouseName, sum(TotalOrder) as TotalOrder, case when sum(TotalOrder)>0 then (sum(c.HIT)/sum(TotalOrder) ) *100 else 0 end HIT, "
                                   + " case when sum(TotalOrder)>0 then (sum(c.MISS)/sum(TotalOrder)) *100 else 0 end MISS,case when sum(TotalOrder)>0 then (sum(c.FAIL)/sum(TotalOrder)) *100 else 0 end FAIL, "
                                   + " case when sum(TotalOrder)>0 then (sum(c.BOLD)/sum(TotalOrder)) *100 else 0 end BOLD "
                                   + " from ( select a.orderid,a.WarehouseId, d.WarehouseName + ' ' + d.CityName WarehouseName, b.DboyName,a.CreatedDate, a.DeliveredDate, "
                                   + " Cast((case when  datediff (hour, a.CreatedDate,a.DeliveredDate) >=0 and datediff (hour, a.CreatedDate,a.DeliveredDate) <=55 then 1 else 0 end) as decimal(5,2)) HIT, "
                                   + " Cast( (case when   datediff (hour, a.CreatedDate,a.DeliveredDate) >55 and datediff (hour, a.CreatedDate,a.DeliveredDate) <=72 then 1 else 0 end) as decimal(5,2)) MISS, "
                                   + " Cast( (case  when   datediff (hour, a.CreatedDate,a.DeliveredDate) >72 and datediff (hour, a.CreatedDate,a.DeliveredDate) <=100 then 1 else 0 end) as decimal(5,2)) FAIL, "
                                   + " Cast( (case when   datediff (hour, a.CreatedDate,a.DeliveredDate) >100  then 1 else 0 end) as decimal(5,2)) BOLD, "
                                   + " Cast( (case when  datediff (hour, a.CreatedDate,a.DeliveredDate) >=0 and datediff (hour, a.CreatedDate,a.DeliveredDate) <=55 then 1 else 0 end) as decimal(5,2)) + "
                                   + " Cast(  (case when   datediff (hour, a.CreatedDate,a.DeliveredDate) >55 and datediff (hour, a.CreatedDate,a.DeliveredDate) <=72 then 1 else 0 end) as decimal(5,2)) + "
                                   + " Cast( (case  when   datediff (hour, a.CreatedDate,a.DeliveredDate) >72 and datediff (hour, a.CreatedDate,a.DeliveredDate) <=100 then 1 else 0 end) as decimal(5,2)) + "
                                   + " Cast( (case when   datediff (hour, a.CreatedDate,a.DeliveredDate) >100  then 1 else 0 end) as decimal(5,2)) TotalOrder "
                                   + " from OrderMasters a inner join [dbo].[OrderDispatchedMasters] b on a.orderid=b.orderid   inner join Warehouses d on a.WarehouseId=d.WarehouseId "
                                   + " where a.[Status] in ('sattled','Delivered') " + whareclase + " ) c  Group by c.DboyName,c.WarehouseName";

                    logger.Info(sqlquery);
                    OrderDBoyDeliverDatas = context.Database.SqlQuery<OrderDBoyDeliverData>(sqlquery).ToList();

                }
                return OrderDBoyDeliverDatas;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetDboyOrderDeliveryData Method: " + ex.ToString());
                return null;
            }
        }
    }



    public class OrderDBoyDeliverData
    {
        public string WarehouseName { get; set; }
        public string DboyName { get; set; }
        public decimal TotalOrder { get; set; }
        public decimal HIT { get; set; }
        public decimal MISS { get; set; }
        public decimal FAIL { get; set; }
        public decimal BOLD { get; set; }

    }
}
