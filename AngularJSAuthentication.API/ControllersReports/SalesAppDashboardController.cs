using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.Masters.Store;
using AngularJSAuthentication.DataContracts.Mongo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllersReports
{
    [RoutePrefix("api/SalesAppDashboard")]
    public class SalesAppDashboardController : ApiController
    {
        [Route("GetExecutiveListByCityId/{CityId}")]
        [HttpGet]
        public List<ClusterStoreExecutiveDc> GetExecutiveListByCityId(int CityId)
        {
            List<ClusterStoreExecutiveDc> result = new List<ClusterStoreExecutiveDc>();
            using (var context = new AuthContext())
            {
                var idParam = new SqlParameter
                {
                    ParameterName = "CityId",
                    Value = CityId
                };
                result = context.Database.SqlQuery<ClusterStoreExecutiveDc>("exec Store_GetDistinctClusterExecutiveByCityId @CityId", idParam).ToList();
            }
            return result;
        }


        /// <summary>
        /// Get Sales App Dashboard Report
        [Route("GetSalesAppDashboardReport")]
        [HttpPost]
        public async Task<List<StoreSalesDashboardReportDc>> GetSalesAppDashboardReport(StoreSalesAppDashboardReqDc req)
        {
            List<string> WeekDays = new List<string>();
            var count = (req.EndDate.AddDays(1) - req.StartDate).TotalDays;
            if (count >= 7)
            {
                WeekDays.Add("Monday");
                WeekDays.Add("Tuesday");
                WeekDays.Add("Wednesday");
                WeekDays.Add("Thursday");
                WeekDays.Add("Friday");
                WeekDays.Add("Saturday");
                WeekDays.Add("Sunday");
            }
            else
            {
                for (var date = req.StartDate; date <= req.EndDate; date = date.AddDays(1))
                {
                    WeekDays.Add(date.DayOfWeek.ToString());
                }
                WeekDays.ToArray();
            }
            List<StoreSalesDashboardReportDc> result = new List<StoreSalesDashboardReportDc>();
            if (req != null && req.CityId > 0 && req.PeopleIds.Any())
            {
                using (var context = new AuthContext())
                {
                    MongoDbHelper<ExecutiveBeats> mongoDbHelper = new MongoDbHelper<ExecutiveBeats>();
                    var MonthBeat = (await mongoDbHelper.SelectAsync(x => req.PeopleIds.Contains(x.PeopleId) && x.PlannedRoutes != null && x.PlannedRoutes.Count() > 0 && x.AssignmentDate >= req.StartDate && x.AssignmentDate <= req.EndDate)).ToList();

                    if (MonthBeat != null && MonthBeat.Any())
                    {
                        #region GetPeopleBeatCustomerOrder
                        var beatCustomerids = MonthBeat.Where(x => x.PlannedRoutes != null).SelectMany(x => x.PlannedRoutes.Select(y => y.CustomerId)).ToList();
                        var actualCustiomerids = MonthBeat.Where(x => x.ActualRoutes != null).SelectMany(x => x.ActualRoutes).Any() ? beatCustomerids : new List<int>();
                        if (actualCustiomerids != null && actualCustiomerids.Any())
                        {
                            beatCustomerids.AddRange(actualCustiomerids);
                        }
                        beatCustomerids = beatCustomerids.Distinct().ToList();

                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();
                        //CustomerIds
                        var customerIdDt = new DataTable();
                        customerIdDt.Columns.Add("IntValue");
                        foreach (var item in beatCustomerids)
                        {
                            var dr = customerIdDt.NewRow();
                            dr["IntValue"] = item;
                            customerIdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("customerId", customerIdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";

                        //PeopleIDs
                        var peopleIdDt = new DataTable();
                        peopleIdDt.Columns.Add("IntValue");
                        foreach (var item in req.PeopleIds)
                        {
                            var dr = peopleIdDt.NewRow();
                            dr["IntValue"] = item;
                            peopleIdDt.Rows.Add(dr);
                        }
                        var param1 = new SqlParameter("ExectiveIds", peopleIdDt);
                        param1.SqlDbType = SqlDbType.Structured;
                        param1.TypeName = "dbo.IntValues";


                        var SubCateId = new SqlParameter("SubCateId", req.SubCateId);


                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetPeopleBeatCustomerStoreOrder]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandTimeout = 600;
                        cmd.Parameters.Add(param);
                        cmd.Parameters.Add(param1);
                        cmd.Parameters.Add(SubCateId);

                        // Run the sproc
                        var reader = cmd.ExecuteReader();
                        var beatCustomerOrders = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<PeopleBeatCustomerOrder>(reader).ToList();
                        #endregion

                        #region GetPeopleBeatCustomers

                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();

                        var DaysIdDt = new DataTable();
                        DaysIdDt.Columns.Add("stringValues");
                        foreach (var item in WeekDays)
                        {
                            var dr = DaysIdDt.NewRow();
                            dr["stringValues"] = item;
                            DaysIdDt.Rows.Add(dr);
                        }
                        var param2 = new SqlParameter("Days", DaysIdDt);
                        param2.SqlDbType = SqlDbType.Structured;
                        param2.TypeName = "dbo.stringValues";

                        //PeopleIDs
                        var peopleIdDts = new DataTable();
                        peopleIdDts.Columns.Add("IntValue");
                        foreach (var item in req.PeopleIds)
                        {
                            var dr = peopleIdDts.NewRow();
                            dr["IntValue"] = item;
                            peopleIdDts.Rows.Add(dr);
                        }
                        var param3 = new SqlParameter("PeopleIds", peopleIdDts);
                        param3.SqlDbType = SqlDbType.Structured;
                        param3.TypeName = "dbo.IntValues";

                        var cmd1 = context.Database.Connection.CreateCommand();
                        cmd1.CommandText = "[dbo].[GetPeopleBeatCustomers]";
                        cmd1.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd1.CommandTimeout = 600;
                        cmd1.Parameters.Add(param2);
                        cmd1.Parameters.Add(param3);
                        // Run the sproc
                        var reader1 = cmd1.ExecuteReader();
                        var PeopleBeatCustomers = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<PeopleBeatCustomers>(reader1).ToList();
                        #endregion

                        result = MonthBeat.GroupBy(x => x.PeopleId).Select(x => new StoreSalesDashboardReportDc
                        {
                            ExectiveId = (PeopleBeatCustomers != null && PeopleBeatCustomers.Count() > 0) ? PeopleBeatCustomers.Where(q => q.PeopleId == x.Key).Select(w => w.PeopleId).FirstOrDefault() : 0,
                            ExectiveName = (PeopleBeatCustomers != null && PeopleBeatCustomers.Count() > 0) ? PeopleBeatCustomers.FirstOrDefault(q => q.PeopleId == x.Key).ExectiveName : " ",
                            WarehouseName = (PeopleBeatCustomers != null && PeopleBeatCustomers.Count() > 0) ? PeopleBeatCustomers.FirstOrDefault(q => q.PeopleId == x.Key).WarehouseName : " ",
                            ClusterName = (PeopleBeatCustomers != null && PeopleBeatCustomers.Count() > 0) ? PeopleBeatCustomers.FirstOrDefault(q => q.PeopleId == x.Key).ClusterName : " ",
                            TotalCustomer = (PeopleBeatCustomers != null && PeopleBeatCustomers.Count() > 0) ? PeopleBeatCustomers.FirstOrDefault(q => q.PeopleId == x.Key).TotalCustomer : 0,
                            TotalBeat = (PeopleBeatCustomers != null && PeopleBeatCustomers.Count() > 0) ? PeopleBeatCustomers.FirstOrDefault(q => q.PeopleId == x.Key).TotalBeat : 0,
                            CustomerPlann = x.Any(u => u.PlannedRoutes != null && u.PlannedRoutes.Count() > 0) ? x.Where(u => u.PlannedRoutes != null && u.PlannedRoutes.Count() > 0).Sum(u => u.PlannedRoutes.Count()) : 0,
                            Visited = x.Any(u => u.PlannedRoutes != null && u.PlannedRoutes.Count() > 0) ? x.Where(u => u.PlannedRoutes != null && u.PlannedRoutes.Count() > 0).Sum(u => u.PlannedRoutes.Where(e => e.IsVisited == true).Count()) : 0,
                            Ordercount = beatCustomerOrders != null && beatCustomerOrders.Any(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key) ? Convert.ToInt32(beatCustomerOrders.Where(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key).Count()) : 0,
                            OrderAmount = beatCustomerOrders != null && beatCustomerOrders.Any(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key) ? Convert.ToInt32(Math.Round(beatCustomerOrders.Where(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key).Sum(z => z.GrossAmount), 0)) : 0,
                            AvgLine = beatCustomerOrders != null && beatCustomerOrders.Any(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key) ? Convert.ToDecimal(beatCustomerOrders.Where(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key).Sum(z => z.lineItem) / beatCustomerOrders.Count(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key)) : 0,
                            Conversion = beatCustomerOrders != null ? beatCustomerOrders.Where(y => y.OrderId > 0).Select(z => z.CustomerId).Distinct().Count() : 0,
                            VisitDetails = x.Any(u => u.PlannedRoutes != null && u.PlannedRoutes.Count() > 0) ? x.SelectMany(i => i.PlannedRoutes.Where(p => p.IsVisited).Select(y => new CustomerVisitDc
                            {
                                Address = y.ShippingAddress,
                                SKcode = y.Skcode,
                                Date = y.VisitedOn,
                                ShopName = y.ShopName
                            })).ToList() : null
                        }).ToList();
                    }
                }
            }
            return result;
        }
    }
}
