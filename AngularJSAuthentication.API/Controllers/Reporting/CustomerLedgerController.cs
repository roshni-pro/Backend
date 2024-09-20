using AngularJSAuthentication.API.ControllerV7;
using AngularJSAuthentication.API.NewHelper;
using AngularJSAuthentication.Model.Account;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.Reporting
{
    [RoutePrefix("api/CustomerLedger")]
    public class CustomerLedgerController : ApiController
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("CorrectMpos")]
        public IHttpActionResult CorrectMPos()
        {
            using (var context = new AuthContext())
            {
                var query = from app in context.PaymentResponseRetailerAppDb
                            join oh in context.OrderMasterHistoriesDB
                            on app.OrderId equals oh.orderid
                            where ((oh.Status == "Sattled" || oh.Status == "Delivered") && app.PaymentFrom == "mPos" && app.status == "Success")
                            select new
                            {
                                app,
                                oh.CreatedDate,
                                oh.Status

                            };

                var list = query.ToList();
                list = list.OrderBy(x => x.app.OrderId).ToList();
                if (list != null && list.Count > 0)
                {
                    int oldOrderID = 0;
                    foreach (var item in list)
                    {

                        int year = item.CreatedDate.Year;
                        int month = item.CreatedDate.Month;
                        int day = item.CreatedDate.Day;
                        int orderid = item.app.OrderId;

                        if (oldOrderID != orderid)
                        {
                            string newQuery = "delete from LadgerEntries where objectid = " + orderid.ToString() + " and objectType = 'Order'";

                            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["authcontext"].ToString()))
                            {
                                // Create the Command and Parameter objects.
                                SqlCommand command = new SqlCommand(newQuery, connection);
                                command.CommandType = System.Data.CommandType.Text;

                                // Open the connection in a try/catch block. 
                                // Create and execute the DataReader, writing the result
                                // set to the console window.
                                try
                                {
                                    connection.Open();
                                    int i = command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {

                                }

                            }
                        }
                        oldOrderID = orderid;
                        if (item.Status.ToLower() == "sattled")
                        {
                            AngularJSAuthentication.API.NewHelper.CustomerLedgerHelper.GetList(month, year, day, false, true, orderid);
                        }
                        else if (item.Status.ToLower() == "delivered")
                        {
                            AngularJSAuthentication.API.NewHelper.CustomerLedgerHelper.GetList(month, year, day, true, false, orderid);
                        }


                    }
                }
            }
            return Ok("done");
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("CorrectOldSattled")]
        public IHttpActionResult CorrectOldSattled()
        {
            string query = @"select ObjectId, Diff from
                            (
                            select LE.ObjectId, SUM(ISNULL(Debit, 0)) D, Sum(ISNULL(Credit, 0)) C, SUM(ISNULL(Debit, 0)) - Sum(ISNULL(Credit, 0)) Diff
                            from LadgerEntries LE
                            INNER JOIN Ladgers L
                            ON L.Id = LE.LagerID AND L.ObjectType = 'Customer'
                            Right JOIN  OrderMasterHistories OMH
                            ON OMH.orderid = LE.ObjectId
                            AND LE.ObjectType = 'Order'
                            WHERE  OMH.Status = 'Sattled'
                            AND OMH.CreatedDate > cast('2018-04-01' as date)
                            GROUP BY LE.ObjectId, LE.ObjectType
                            ) d
                            Where(Diff > 10 OR Diff < -10)";
            using (var context = new AuthContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.CommandTimeout = 180;
                try
                {
                    List<CorrectSattleData> list = context.Database.SqlQuery<CorrectSattleData>(query).ToList();
                    foreach (var item in list)
                    {
                        var historyList = context.OrderMasterHistoriesDB.Where(x => x.orderid == item.ObjectId && (x.Status == "Delivered" || x.Status == "Sattled")).ToList();


                        string newQuery = "delete from LadgerEntries where objectid = " + item.ObjectId.ToString() + " and objectType = 'Order'";

                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["authcontext"].ToString()))
                        {
                            // Create the Command and Parameter objects.
                            SqlCommand command = new SqlCommand(newQuery, connection);
                            command.CommandType = System.Data.CommandType.Text;

                            // Open the connection in a try/catch block. 
                            // Create and execute the DataReader, writing the result
                            // set to the console window.
                            try
                            {
                                connection.Open();
                                int i = command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {

                            }

                        }


                        if (historyList.Any(x => x.Status.ToLower() == "delivered"))
                        {
                            int year = historyList.First(x => x.Status.ToLower() == "delivered").CreatedDate.Year;
                            int month = historyList.First(x => x.Status.ToLower() == "delivered").CreatedDate.Month;
                            int day = historyList.First(x => x.Status.ToLower() == "delivered").CreatedDate.Day;

                            AngularJSAuthentication.API.NewHelper.CustomerLedgerHelper.GetList(month, year, day, true, false, (int)item.ObjectId);

                        }

                        if (historyList.Any(x => x.Status.ToLower() == "sattled"))
                        {
                            int year = historyList.First(x => x.Status.ToLower() == "sattled").CreatedDate.Year;
                            int month = historyList.First(x => x.Status.ToLower() == "sattled").CreatedDate.Month;
                            int day = historyList.First(x => x.Status.ToLower() == "sattled").CreatedDate.Day;

                            AngularJSAuthentication.API.NewHelper.CustomerLedgerHelper.GetList(month, year, day, false, true, (int)item.ObjectId);

                        }

                    }
                }
                catch (Exception ex)
                {

                }

            }
            return Ok();


        }


        [AllowAnonymous]
        [HttpGet]
        [Route("Insert/year/{year}/month/{month}/day/{day}")]
        public IHttpActionResult Insert(int year, int month, int day)
        {
            AngularJSAuthentication.API.NewHelper.CustomerLedgerHelper.GetList(month, year, day, true, false);
            AngularJSAuthentication.API.NewHelper.CustomerLedgerHelper.GetList(month, year, day, false, true);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Insert/year/{year}/month/{month}/day/{day}/orderId/{orderid}")]
        public IHttpActionResult Insert(int year, int month, int day, int orderid)
        {
            AngularJSAuthentication.API.NewHelper.CustomerLedgerHelper.GetList(month, year, day, true, false, orderid);
            AngularJSAuthentication.API.NewHelper.CustomerLedgerHelper.GetList(month, year, day, false, true, orderid);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("MakeEntriesOfAllDelivered")]
        public void MakeEntriesOfAllDelivered()
        {
            //string query = @"select distinct ODM.orderid, ODM.CreatedDate from OrderMasterHistories ODM 
            //                    inner join OrderMasters OM on ODM.orderid = OM.OrderId
            //                    where not exists (
            //                    select 1 from LadgerEntries LE where ODM.orderid = LE.ObjectID and LE.ObjectType = 'order' AND  LE.VouchersTypeID =1
            //                    )and ODM.CreatedDate > cast('2019-09-01' as datetime)
            //                    and OM.Status IN ('Delivered',
            //                    'sattled')
            //                    AND ODM.Status = 'Delivered'";

            string query = @"select  ODM.orderid, Max(ODM.CreatedDate) CreatedDate
                                from OrderMasterHistories ODM     
                                where Status = 'Ready to dispatch' and
                                Orderid in (385376
                                ,386089
                                ,386086
                                ,385377
                                ,386085
                                ,385378
                                ,386065
                                ,386056
                                ,386055
                                ,385379
                                ,385382
                                ,385392
                                ,386448
                                ,385404
                                ,386405
                                ,386399
                                ,386375
                                ,386372
                                ,386323
                                ,386317
                                ,386253
                                ,386220
                                ,386132
                                ,386499
                                ,386504
                                ,386506
                                ,386507
                                ,384865
                                ,386508
                                ,386512
                                ,386515
                                ,386522
                                ,386526
                                ,386533
                                ,385636
                                ,386549
                                ,386553
                                ,386564
                                ,385638
                                ,386566
                                ,386570
                                ,385719
                                ,386234
                                ,386650
                                ,386645
                                ,386644
                                ,386635
                                ,386614
                                ,386603
                                ,386581
                                ,386580
                                ,386577
                                ,386573
                                ,386571
                                ,384676
                                ,384716
                                ,384861
                                ,386274
                                ,386285
                                ,386321
                                ,386558
                                ,376128
                                )
                                group by ODM.orderid";

            using (var context = new AuthContext())
            {
                List<CorrectOrderMasterHistory> list = context.Database.SqlQuery<CorrectOrderMasterHistory>(query).ToList();
                if (list != null && list.Any())
                {
                    foreach (var item in list)
                    {
                        AngularJSAuthentication.API.NewHelper.CustomerLedgerHelper.GetList(item.CreatedDate.Month, item.CreatedDate.Year, item.CreatedDate.Day, true, false, item.orderid);
                    }
                }

            }

        }


        [AllowAnonymous]
        [HttpGet]
        [Route("InsertOld/year/{year}/month/{month}/day/{day}")]
        public IHttpActionResult InsertOld(int year, int month, int day)
        {
            //outdated method will not work now
            //AngularJSAuthentication.API.NewHelper.CustomerLedgerHelper.GetListOld(month, year, day, true, false);
            return InternalServerError(new Exception("outdated method will not work now"));
        }




        [AllowAnonymous]
        [HttpGet]
        [Route("from/{from_mm_ddy_yyyy}/to/{to_mm_ddy_yyyy}")]
        public IHttpActionResult InsertBetweenDate(string from_mm_ddy_yyyy, string to_mm_ddy_yyyy)
        {
            int year = 2019;
            int month = 07;
            int day = 21;
            //AngularJSAuthentication.API.NewHelper.CustomerLedgerHelper.GetList(month, year, day);
            return Ok();
        }



        [HttpPost]
        [Route("GenerateReport")]
        public IHttpActionResult GenerateReport(LedgerInputViewModel viewModel)
        {
            string url = CustomerLedgerHelper.GenerateReport(viewModel);
            return Ok(url);
        }



        [HttpPost]
        [Route("GenerateReportAll")]
        [AllowAnonymous]
        public IHttpActionResult GenerateReportAll()
        {
            LedgerInputViewModel viewModel = new LedgerInputViewModel();
            viewModel.FromDate = new DateTime(2019, 04, 01);
            viewModel.ToDate = new DateTime(2020, 01, 30);
            viewModel.IsGenerateExcel = true;
            viewModel.ReportCode = "SR";

            using (var context = new AuthContext())
            {
                var query = from l in context.LadgerDB
                            join c in context.Customers
                            on l.ObjectID equals c.CustomerId
                            where l.ObjectType == "Customer"
                            select l;
                List<Ladger> ledgerList = query.ToList();

                List<string> ledgerIdList = System.IO.File.ReadAllLines(HttpContext.Current.Server.MapPath("~/ReportDownloads/WriteLine.txt")).ToList();

                if (ledgerIdList != null && ledgerIdList.Any())
                {
                    ledgerList = ledgerList.Where(x => !ledgerIdList.Contains(x.ID.ToString())).ToList();
                }

                //DataTable dt =  ConvertToDataTableExportData(ledgerList);
                //Exceldownload(dt);

                foreach (var ledger in ledgerList)
                {
                    //DataTable dt = new DataTable();
                    //dt.Rows.Add(ledger.ID);
                    //Exceldownload(dt); @"C:\Users\Rajkumar\Desktop\Publish!!\ReportDownloads\WriteLine.txt"
                    System.IO.File.AppendAllLines(HttpContext.Current.Server.MapPath("~/ReportDownloads/WriteLine.txt"), new string[] { ledger.ID.ToString() });
                    viewModel.ToDate = new DateTime(2020, 01, 30);
                    viewModel.LedgerID = (int)ledger.ID;
                    viewModel.LedgerTypeID = (int)ledger.LadgertypeID;
                    CustomerLedgerHelper.GenerateReport(viewModel);
                    //return Ok(url);
                }

            }

            return Ok();

        }
        [HttpPost]
        [Route("GenerateForReportAllSupplier")]
        [AllowAnonymous]
        public IHttpActionResult GenerateForReportAllSupplier()
        {
            LedgerInputViewModel viewModel = new LedgerInputViewModel();
            viewModel.FromDate = new DateTime(2019, 04, 01);
            viewModel.ToDate = new DateTime(2020, 01, 30);
            viewModel.IsGenerateExcel = true;
            viewModel.ReportCode = "SR";

            using (var context = new AuthContext())
            {
                var query = from l in context.LadgerDB
                            join s in context.Suppliers
                            on l.ObjectID equals s.SupplierId
                            where l.ObjectType == "Supplier"
                            select l;
                List<Ladger> ledgerList = query.ToList();

                List<string> ledgerIdList = System.IO.File.ReadAllLines(HttpContext.Current.Server.MapPath("~/ReportDownloads/WriteLine.txt")).ToList();

                if (ledgerIdList != null && ledgerIdList.Any())
                {
                    ledgerList = ledgerList.Where(x => !ledgerIdList.Contains(x.ID.ToString())).ToList();
                }

                //DataTable dt =  ConvertToDataTableExportData(ledgerList);
                //Exceldownload(dt);

                foreach (var ledger in ledgerList)
                {
                    //DataTable dt = new DataTable();
                    //dt.Rows.Add(ledger.ID);
                    //Exceldownload(dt); @"C:\Users\Rajkumar\Desktop\Publish!!\ReportDownloads\WriteLine.txt"
                    System.IO.File.AppendAllLines(HttpContext.Current.Server.MapPath("~/ReportDownloads/WriteLine.txt"), new string[] { ledger.ID.ToString() });
                    viewModel.ToDate = new DateTime(2020, 01, 30);
                    viewModel.LedgerID = (int)ledger.ID;
                    viewModel.LedgerTypeID = (int)ledger.LadgertypeID;
                    CustomerLedgerHelper.GenerateReport(viewModel);
                    //return Ok(url);
                }

            }

            return Ok();

        }




        [Route("GenerateReportRaw")]
        public IHttpActionResult GenerateReportRaw(LedgerInputViewModel viewModel)
        {
            viewModel.IsGetRawReportForExcel = true;
            string url = CustomerLedgerHelper.GenerateReport(viewModel);
            return Ok(url);
        }



        [AllowAnonymous]
        [HttpPost]
        [Route("GenerateSupplierReport")]
        public IHttpActionResult GenerateSupplierReport(LedgerInputViewModelForSupplierApp viewModel1)
        {
            var checkDate = new DateTime(2019, 4, 1);
            if (viewModel1.FromDate >= checkDate)
            {
                using (var authContext = new AuthContext())
                {
                    var LedgerID = authContext.LadgerDB.Where(x => x.ObjectType == "Supplier" && x.ObjectID == viewModel1.SupplierID).SingleOrDefault();
                    viewModel1.ToDate = viewModel1.ToDate.AddDays(1);
                    var viewModel = new LedgerInputViewModel();
                    viewModel.LedgerID = Convert.ToInt32(LedgerID.ID);
                    viewModel.FromDate = viewModel1.FromDate;
                    viewModel.ToDate = viewModel1.ToDate;
                    viewModel.IsGenerateExcel = viewModel1.IsGenerateExcel;
                    viewModel.LedgerTypeID = viewModel1.LedgerTypeID;
                    viewModel.ReportCode = viewModel1.ReportCode;
                    viewModel.RefNo = viewModel1.RefNo;
                    string url = CustomerLedgerHelper.GenerateReport(viewModel);


                    var name = url.Split('.');
                    String filename = name[0];
                    String fileext = name[1];

                    var response = new
                    {
                        URL = url,
                        Status = true,
                        Message = "ReportURL",
                        FileName = filename,
                        FileExtension = fileext
                    };

                    return Ok(response);

                }
            }
            else
            {
                string url = null;
                var response = new
                {
                    URL = url,
                    Status = true,
                    Message = "Please Change Date To This Financial Year",
                    FileName = "",
                    FileExtension = ""
                };
                return Ok(response);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PoDetailsReport")]
        public IHttpActionResult PoDetailsReport(int Poid, bool IsGenerateExcel)
        {            
            if (Poid > 0)
            {
                using (var authContext = new AuthContext())
                {
                    string url = CustomerLedgerHelper.PODetailsGenerateReport(Poid, IsGenerateExcel);

                    if (!string.IsNullOrEmpty(url))
                    {
                        var name = url.Split('.');
                        String filename = name[0];
                        String fileext = name[1];

                        var response = new
                        {
                            URL = url,
                            Status = true,
                            Message = "ReportURL",
                            FileName = filename,
                            FileExtension = fileext
                        };
                        return Ok(response);
                    }
                    else
                    {
                       var response1 = new
                        {
                            URL = "",
                            Status = false,
                            Message = "Data not exists",
                            FileName = "",
                            FileExtension = ""
                        };
                        return Ok(response1);
                    }
                   
                }
            }
            else
            {
                string url = null;
                var response = new
                {
                    URL = url,
                    Status = true,
                    Message = "Please Change Date To This Financial Year",
                    FileName = "",
                    FileExtension = ""
                };
                return Ok(response);
            }
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("GenerateCustomerReport")]
        public IHttpActionResult GenerateCustomerReport(LedgerInputViewModelForRetailerApp viewModelV1)
        {
            //var checkDate = new DateTime(2019, 4, 1);
            //if (viewModelV1.FromDate >= checkDate)
            //{
            //    using (var authContext = new AuthContext())
            //    {
            //        LedgerInputViewModel viewModel = new LedgerInputViewModel();
            //        var LedgerID = authContext.LadgerDB.Where(x => x.ObjectType == "Customer" && x.ObjectID == viewModelV1.CustomerId).SingleOrDefault();
            //        viewModelV1.ToDate = viewModelV1.ToDate.AddDays(1);
            //        viewModel.LedgerID = Convert.ToInt32(LedgerID.ID);
            //        viewModel.FromDate = viewModelV1.FromDate;
            //        viewModel.ToDate = viewModelV1.ToDate;
            //        viewModel.IsGenerateExcel = viewModelV1.IsGenerateExcel;
            //        viewModel.LedgerTypeID = viewModelV1.LedgerTypeID;
            //        viewModel.ReportCode = viewModelV1.ReportCode;
            //        string url = CustomerLedgerHelper.GenerateReport(viewModel);
            //        var name = url.Split('.');

            //        String filename = name[0];
            //        String fileext = name[1];


            //        var response = new
            //        {
            //            URL = url,
            //            Status = true,
            //            Message = "ReportURL",
            //            FileName = filename,
            //            FileExtension = fileext
            //        };

            //        return Ok(response);

            //    }
            //}
            //else
            //{
            //    string url = null;
            //    var response = new
            //    {
            //        URL = url,
            //        Status = true,
            //        Message = "Please Change Date To This Financial Year",
            //        FileName = "",
            //        FileExtension = ""

            //    };
            //    return Ok(response);
            //}
            string url = null;
            var response = new
            {
                URL = url,
                Status = false,
                Message = "Services currently under maintenance!!",
                FileName = "",
                FileExtension = ""

            };
            return Ok(response);
        }

        class CorrectOrderMasterHistory
        {
            public int orderid { get; set; }
            public DateTime CreatedDate { get; set; }
        }

    }


    public class CorrectSattleData
    {
        public long ObjectId { get; set; }
        public Double Diff { get; set; }
    }
}
