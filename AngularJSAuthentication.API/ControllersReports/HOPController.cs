using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.Transaction.Reports;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllersReports
{
    [RoutePrefix("api/HOP")]
    public class HOPController : BaseApiController
    {
        [Route("CalculateAchievement/{startDate?}/{endDate?}")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<bool> CalculateAchievement(DateTime? startDate = null, DateTime? endDate = null)
        {
            bool result = false;
            var procedures = new List<Procedures>();
            endDate = endDate.HasValue ? endDate.Value.AddDays(1).AddSeconds(-1) : endDate;

            using (var context = new AuthContext())
            {
                var query = $"Exec [HOP].[GetProcedureNames]";
                procedures = context.Database.SqlQuery<Procedures>(query).ToList();
            }

            List<string> procedureNames = procedures.Where(x => x.RoundNo == 1).Select(x => x.ProcName).ToList();

            List<string> procedureNamesRound2 = procedures.Where(x => x.RoundNo == 2).Select(x => x.ProcName).ToList();

            //ConcurrentBag<bool> concurrentBag1 = new ConcurrentBag<bool>();
            //ParallelLoopResult parallelResult = Parallel.ForEach(procedureNames, (x) =>
            //{
            foreach (var x in procedureNames)
            {
                try
                {
                    using (var context = new AuthContext())
                    {
                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();

                        context.Database.CommandTimeout = 1800;
                        context.Database.ExecuteSqlCommand($"Exec {x} '{startDate}', '{endDate}'");

                        //concurrentBag1.Add(true);
                    }
                }
                catch (System.Exception ex)
                {
                    TextFileLogHelper.LogError(ex.ToString());
                }
            }
            //});

            //if (parallelResult.IsCompleted && procedureNamesRound2 != null && procedureNamesRound2.Any())
            //{
            //    ConcurrentBag<bool> concurrentBag2 = new ConcurrentBag<bool>();
            //ParallelLoopResult parallelResult2 = Parallel.ForEach(procedureNamesRound2, (x) =>
            //{
            foreach (var x in procedureNamesRound2)
            {
                try
                {
                    using (var context = new AuthContext())
                    {
                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();
                        context.Database.CommandTimeout = 1800;
                        context.Database.ExecuteSqlCommand($"Exec {x} '{startDate}', '{endDate}'");

                        //concurrentBag2.Add(true);
                    }
                }
                catch (System.Exception ex)
                {
                    TextFileLogHelper.LogError(ex.ToString());
                }
            }
            //});

            //if (parallelResult2.IsCompleted)
            //    result = true;

            return result;

        }

        [Route("GetAchievedOverallSummary")]
        [HttpPost]
        public async Task<List<ParentList>> GetAchievedOverallSummary(FilterSummaryObj obj)
        {
            var result = new List<AchievedOverallSummaryDc>();
            using (var context = new AuthContext())
            {
                var query = $"Exec [HOP].[GetAchievedOverallSummary] {obj.type}, '{obj.plantype}'";
                result = await context.Database.SqlQuery<AchievedOverallSummaryDc>(query).ToListAsync();

                result.Where(x => string.IsNullOrEmpty(x.HeadName)).ToList().ForEach(x =>
                 {
                     var headname = result.FirstOrDefault(s => s.PlanType == x.PlanType && !string.IsNullOrEmpty(s.HeadName))?.HeadName;
                     if (!string.IsNullOrEmpty(headname))
                         x.HeadName = headname;

                 });

                var dataToReturn = result.OrderByDescending(x => x.HeadName).GroupBy(x => new { x.Month, x.Year })
                  .Select(x => new ParentList
                  {
                      Month = x.Key.Month,
                      Year = x.Key.Year,
                      Summary = x.OrderByDescending(s => s.HeadName).ToList(),
                      Heads = x.Where(s => !string.IsNullOrEmpty(s.HeadName)).GroupBy(s => s.HeadName).Select(s => new Heads
                      {
                          HeadName = s.Key,
                          PlanTypes = result.Where(d => d.HeadName == s.Key).Select(d => d.PlanType).Distinct().ToList()
                      }).ToList()
                  }).ToList();

                return dataToReturn;

                //if (type == 1)
                //{
                //    var dataToReturn = result.GroupBy(x => new { x.Month, x.Year })
                //        .Select(x => new ParentList
                //        {
                //            Month = x.Key.Month,
                //            Year = x.Key.Year,
                //            Summary = x.ToList()
                //        }).ToList();

                //    return dataToReturn;
                //}
                //else
                //{
                //    var dataToReturn = result.GroupBy(x => new { Quarter = DateTimeExtensions.GetFinancialQuarter(new DateTime(x.Year, x.Month, 1)) })
                //       .Select(x => new ParentList
                //       {
                //           Month = x.Key.Quarter,
                //           Year = 0,
                //           Summary = x.Where(z => DateTimeExtensions.GetFinancialQuarter(new DateTime(z.Year, z.Month, 1)) == x.Key.Quarter)
                //                        .GroupBy(y => new { y.HeadName, y.PlanType })
                //                        .Select(z => new
                //                        {
                //                            id = (long?)0,
                //                            z.Key.HeadName,
                //                            z.Key.PlanType,
                //                            PlannedValue = z.Where(d => d.PlannedValue.HasValue).Sum(s => s.PlannedValue),
                //                            AchievedValue = z.Where(d => d.PlannedValue.HasValue).Sum(s => s.AchievedValue),
                //                            AchievedPercent = Math.Round(100 *
                //                                                (double)(z.Where(d => d.PlannedValue.HasValue).Sum(s => s.AchievedValue)
                //                                                /
                //                                                z.Where(d => d.PlannedValue.HasValue).Sum(s => s.PlannedValue) > 0
                //                                                    ? z.Where(d => d.PlannedValue.HasValue).Sum(s => s.PlannedValue)
                //                                                    : 1
                //                                                ), 2)
                //                        }).ToList()
                //       }).ToList();


                //    return dataToReturn;
                //}
            }
            //return result;
        }
        [Route("GetAchievedWarehouseData")]
        [HttpPost]
        public async Task<List<ParentList>> GetAchievedWarehouseData(FilterSummaryObj hopSummary)
        {
            var result = new List<AchievedWarehouseDataDc>();
            using (var context = new AuthContext())
            {
                var query = $"Exec [HOP].[GetAchievedWarehouseData] {hopSummary.type}, '{hopSummary.plantype}',{hopSummary.storeid}";
                result = await context.Database.SqlQuery<AchievedWarehouseDataDc>(query).ToListAsync();

                var dataToReturn = result.GroupBy(x => new { x.Month, x.Year })
                   .Select(x => new ParentList
                   {
                       Month = x.Key.Month,
                       Year = x.Key.Year,
                       Summary = x.OrderBy(d => d.ObjectName).ToList()
                   }).ToList();

                return dataToReturn;
            }
            //return result;
        }
        [Route("GetAchievedStoreData")]
        [HttpPost]
        public async Task<List<ParentList>> GetAchievedStoreData(FilterSummaryObj hopSummary)
        {
            var result = new List<AchievedStoreDataDc>();
            using (var context = new AuthContext())
            {
                var query = $"Exec [HOP].[GetAchievedStoreData] {hopSummary.type}, '{hopSummary.plantype}',{hopSummary.warehouseid}";
                result = await context.Database.SqlQuery<AchievedStoreDataDc>(query).ToListAsync();

                var dataToReturn = result.GroupBy(x => new { x.Month, x.Year })
                   .Select(x => new ParentList
                   {
                       Month = x.Key.Month,
                       Year = x.Key.Year,
                       Summary = x.OrderBy(d => d.ObjectName).ToList()
                   }).ToList();

                return dataToReturn;
            }
            //return result;
        }

        [Route("GetAchievedBrandData")]
        [HttpPost]
        public async Task<List<ParentList>> GetAchievedBrandData(FilterSummaryObj hopSummary)
        {
            var result = new List<AchievedBrandDataDc>();
            using (var context = new AuthContext())
            {
                var query = $"Exec [HOP].[GetAchievedBrandData] {hopSummary.type}, '{hopSummary.plantype}',{hopSummary.storeid},{hopSummary.warehouseid}";
                result = await context.Database.SqlQuery<AchievedBrandDataDc>(query).ToListAsync();

                var dataToReturn = result.GroupBy(x => new { x.Month, x.Year })
                   .Select(x => new ParentList
                   {
                       Month = x.Key.Month,
                       Year = x.Key.Year,
                       Summary = x.OrderBy(d => d.ObjectName).ToList()
                   }).ToList();

                return dataToReturn;

            }
            //return result;
        }

        [Route("GetDropDownValues")]
        [HttpGet]
        public async Task<List<Heads>> GetDropDownValues()
        {
            using (var context = new AuthContext())
            {
                var query = $"select HeadName,PlanType from hop.HeadPlanMaster";
                var headsFromDB = await context.Database.SqlQuery<HeadsFromDB>(query).ToListAsync();

                var dataToReturn = headsFromDB.GroupBy(x => new { x.HeadName })
                   .Select(x => new Heads
                   {
                       HeadName = x.Key.HeadName,
                       PlanTypes = x.Select(s => s.PlanType).OrderBy(s => s).ToList()
                   }).ToList();

                return dataToReturn;

            }
        }

        [Route("LoadFieldDashboard")]
        [HttpPost]
        public async Task<FieldDashboardData> LoadFieldDashboard(FieldDashboardMasterFilter filter)
        {
            var dataToReturn = new FieldDashboardData();

            var taskList = new List<Task>();
            var task1 = Task.Factory.StartNew(() =>
            {
                using (var context = new AuthContext())
                {
                    var query = $"exec [HOP].[GetFieldDashboardGraphs] {filter.warehouseid},'{filter.HeadName}'";
                    dataToReturn.Graphs = context.Database.SqlQuery<FieldDashboardMainData>(query).ToList();
                }
            });

            taskList.Add(task1);

            var task2 = Task.Factory.StartNew(() =>
            {
                using (var context = new AuthContext())
                {
                    var query = $"exec [HOP].[GetFieldDashboardMainList] {filter.warehouseid},'{filter.PlanType}'";
                    dataToReturn.MainList = context.Database.SqlQuery<FieldDashboardMainData>(query).ToList();
                }
            });

            taskList.Add(task2);

            var task3 = Task.Factory.StartNew(() =>
            {
                using (var context = new AuthContext())
                {
                    var query = $"exec [HOP].[GetFieldDashboardClusterDetailList] {filter.warehouseid},'{filter.PlanType}'";
                    dataToReturn.ClusterData = context.Database.SqlQuery<AchievedSummaryDc>(query).ToList();
                }
            });

            taskList.Add(task3);

            var task4 = Task.Factory.StartNew(() =>
            {
                using (var context = new AuthContext())
                {
                    var query = $"exec [HOP].[GetFieldDashboardStoreDetailList] {filter.warehouseid},'{filter.PlanType}'";
                    dataToReturn.StoreData = context.Database.SqlQuery<AchievedSummaryDc>(query).ToList();
                }
            });

            taskList.Add(task4);


            var task5 = Task.Factory.StartNew(() =>
            {
                using (var context = new AuthContext())
                {
                    var query = $"exec [HOP].[GetFieldDashboardBrandDetailList] {filter.warehouseid},'{filter.PlanType}'";
                    dataToReturn.BrandData = context.Database.SqlQuery<AchievedSummaryDc>(query).ToList();
                }
            });

            taskList.Add(task5);


            Task.WaitAll(taskList.ToArray());

            return dataToReturn;
        }


    }
}