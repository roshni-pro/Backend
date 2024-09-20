using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model.Account;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/LadgerUI")]
    public class LadgerUIController : ApiController
    {

        int compid = 0, userid = 0;
        public LadgerUIController()
        {
            var identity = User.Identity as ClaimsIdentity;
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

        }




        [Route("Get")]
        [HttpGet]
        public dynamic GetLadgerBankDetail()
        {
            using (AuthContext db = new AuthContext())
            {
                var query = "select * from Ladgers Where LadgertypeID=7 ";
                var result = db.Database.SqlQuery<Ladger>(query).ToList();

                return result;
            }
        }

        [Route("Save")]
        [HttpGet]
        public dynamic AddLadgerBankDetail(string BankName)
        {
            using (var authContext = new AuthContext())
            {

                var sqlquery = "INSERT INTO Ladgers(Name, Alias, InventoryValuesAreAffected, ProvidedBankDetails, LadgertypeID, Active, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)" +
                    " VALUES ('" + BankName + "','" + BankName + "', '1', '0', '7', '1','" + userid + "','" + DateTime.Now + "','" + userid + "','" + DateTime.Now + "')";
                var result = authContext.Database.ExecuteSqlCommand(sqlquery);
                return Ok(result);
            }
        }


        [Authorize]
        [Route("GetMatch")]
        [HttpGet]
        public dynamic add(string Name)
        {
            using (AuthContext context = new AuthContext())
            {
                var Ladger = context.LadgerDB.Where(c => c.Name == Name).Select(y => y.Name).FirstOrDefault();
                if (Ladger == null)
                {

                    return Ok(Name);

                }
                else
                {
                    return null;
                }

            }
        }


        [Route("Get/{id}")]
        [HttpGet]
        public IHttpActionResult Get(int id)
        {
            using (AuthContext context = new AuthContext())
            {
                Ladger ladger = context.LadgerDB.Where(x => x.ID == id).FirstOrDefault();
                return Ok(ladger);
            }


        }

        [Route("Delete/{id}")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteAsync(int id)
        {
            using (AuthContext context = new AuthContext())
            {
                Ladger ladger = context.LadgerDB.Where(x => x.ID == id).FirstOrDefault();
                ladger.Active = false;
                context.Entry(ladger).State = EntityState.Modified;
                await context.CommitAsync();
                return Ok(ladger);
            }
        }


        [Route("GetList")]
        [HttpPost]
        public IHttpActionResult GetList(PagerDataUIViewModel pager)
        {
            using (AuthContext context = new AuthContext())
            {
                SqlParameter containsParam = new SqlParameter("Contains", pager.Contains);
                SqlParameter firstParam = new SqlParameter("First", pager.First);
                SqlParameter lastParam = new SqlParameter("Last", pager.Last);
                SqlParameter columnNameParam = new SqlParameter("ColumnName", pager.ColumnName);
                SqlParameter isAscendingParam = new SqlParameter("IsAscending", pager.IsAscending);
                object[] parameters = new object[] { containsParam, firstParam, lastParam, columnNameParam, isAscendingParam };

                var list = context.Database.SqlQuery<LadgerPaginatorViewModel>("LadgerPaginator @Contains,@First,@Last,@ColumnName,@IsAscending", parameters).ToList();
                return Ok(list);
            }


        }
        
        [Route("GetAgentByName/name/{name}/ledgerTypeId/{ledgerTypeId}")]
        [HttpGet]
        public IHttpActionResult GetAgentByName(string name, int ledgerTypeId)
        {
            using (var authContext = new AuthContext())
            {
                var ledgerTypeList = authContext.LadgerTypeDB.ToList();
                if (ledgerTypeList.Where(x => x.ID == ledgerTypeId).FirstOrDefault().code.ToLower() == "customer")
                {
                    var query = from lb in authContext.LadgerDB
                                join cus in authContext.Customers
                                on lb.ObjectID equals cus.CustomerId

                                where (cus.Skcode.ToLower().Contains(name.ToLower()) || cus.Name.ToLower().Contains(name.ToLower()) || cus.ShopName.ToLower().Contains(name.ToLower()) && lb.LadgertypeID == ledgerTypeId)
                                && lb.ObjectType == "Customer"
                                select new
                                {
                                    lb.ID,
                                    Name = cus.Name + " - " + cus.ShopName + " - " + cus.Skcode
                                };
                    var ladgerList = query.Take(50).ToList();
                    return Ok(ladgerList);
                }
                else if (ledgerTypeList.Where(x => x.ID == ledgerTypeId).FirstOrDefault().code.ToLower() == "supplier")
                {
                    var query = from lb in authContext.LadgerDB
                                join cus in authContext.Suppliers
                                on lb.ObjectID equals cus.SupplierId
                                where (cus.SUPPLIERCODES.ToLower().Contains(name.ToLower()) || cus.Name.ToLower().Contains(name.ToLower()) || cus.Brand.ToLower().Contains(name.ToLower()) && lb.LadgertypeID == ledgerTypeId)
                                 && lb.ObjectType == "Supplier"
                                select new
                                {
                                    lb.ID,
                                    lb.ObjectID,
                                    Name = cus.Name + " - " + cus.Brand + " - " + cus.SUPPLIERCODES
                                };
                    var ladgerList = query.Take(50).ToList();
                    return Ok(ladgerList);
                }
                else if (ledgerTypeList.Where(x => x.ID == ledgerTypeId).FirstOrDefault().code.ToLower() == "agent")
                {

                    var query = from lb in authContext.LadgerDB
                                join pop in authContext.Peoples
                                on lb.ObjectID equals pop.PeopleID
                                where (pop.DisplayName.ToLower().Contains(name.ToLower()) || pop.PeopleFirstName.ToLower().Contains(name.ToLower()) || pop.PeopleLastName.ToLower().Contains(name.ToLower()))
                                && lb.LadgertypeID == ledgerTypeId
                                && lb.ObjectType == "Agent"
                                select new
                                {
                                    lb.ID,
                                    lb.ObjectID,
                                    Name = pop.DisplayName + " - " + pop.Mobile
                                };
                    var ladgerList = query.Take(50).ToList();
                    return Ok(ladgerList);

                }
                else if (ledgerTypeList.Where(x => x.ID == ledgerTypeId).FirstOrDefault().code.ToLower() == "vendor")
                {
                    var query = from lb in authContext.LadgerDB
                                join pop in authContext.VendorDB
                                on lb.ObjectID equals pop.Id
                                where (pop.Name.ToLower().Contains(name.ToLower()) || pop.Code.ToLower().Contains(name.ToLower()))
                                && lb.LadgertypeID == ledgerTypeId
                                && lb.ObjectType == "Vendor"
                                select new
                                {
                                    ID = lb.ID,
                                    Name = pop.Name + " (" + pop.Code + ")",
                                    pop.DepartmentId,
                                    pop.WorkingCompanyId,
                                    pop.WorkingLocationId,
                                    pop.IsTDSApplied,
                                    pop.ExpenseTDSMasterID
                                };
                    var ladgerList = query.Take(50).ToList();

                    return Ok(ladgerList);
                }
                else
                {
                    var query = from lb in authContext.LadgerDB
                                where (lb.Name.ToLower().Contains(name.ToLower()) || lb.Alias.ToLower().Contains(name.ToLower()) && lb.LadgertypeID == ledgerTypeId)
                                && lb.LadgertypeID == ledgerTypeId
                                select new
                                {
                                    lb.ID,
                                    lb.ObjectID,
                                    Name = lb.Name + " - " + lb.Alias
                                };
                    var ladgerList = query.Take(50).ToList();
                    return Ok(ladgerList);

                }
            }

        }
        [Route("GetOpenPO")]
        [HttpGet]
        public List<int> GetOpenPO(int LadgerID)
        {
            using (var context = new AuthContext())
            {
                var OpenPODataList = new List<int>();
                var supplierID = context.LadgerDB.Where(x => x.ObjectID == LadgerID).Select(w=>w.ObjectID).FirstOrDefault();
                OpenPODataList = context.DPurchaseOrderMaster.Where(x => (x.Status != "Closed" && x.Status != "Auto Closed" && x.Status != "Canceled") && x.Active==true && x.Deleted==false).Select(y => y.PurchaseOrderId).ToList();
             return OpenPODataList;
            }
        }


        [Route("GetByName/name/{name}/ledgerTypeId/{ledgerTypeId}")]
        [HttpGet]
        public IHttpActionResult GetByName(string name, int ledgerTypeId)
        {
            using (var authContext = new AuthContext())
            {
                var ledgerTypeList = authContext.LadgerTypeDB.ToList();
                if (ledgerTypeList.Where(x => x.ID == ledgerTypeId).FirstOrDefault().code.ToLower() == "customer")
                {
                    var query = from lb in authContext.LadgerDB
                                join cus in authContext.Customers
                                on lb.ObjectID equals cus.CustomerId

                                where (cus.Skcode.ToLower().Contains(name.ToLower()) || cus.Name.ToLower().Contains(name.ToLower()) || cus.ShopName.ToLower().Contains(name.ToLower()) && lb.LadgertypeID == ledgerTypeId)
                                && lb.ObjectType == "Customer"
                                select new
                                {
                                    lb.ID,
                                    Name = cus.Name + " - " + cus.ShopName + " - " + cus.Skcode
                                };
                    var ladgerList = query.Take(50).ToList();
                    return Ok(ladgerList);
                }
                else if (ledgerTypeList.Where(x => x.ID == ledgerTypeId).FirstOrDefault().code.ToLower() == "supplier")
                {
                    var query = from lb in authContext.LadgerDB
                                join cus in authContext.Suppliers
                                on lb.ObjectID equals cus.SupplierId
                                where (cus.SUPPLIERCODES.ToLower().Contains(name.ToLower()) || cus.Name.ToLower().Contains(name.ToLower()) || cus.Brand.ToLower().Contains(name.ToLower()) && lb.LadgertypeID == ledgerTypeId)
                                 && lb.ObjectType == "Supplier"
                                select new
                                {
                                    lb.ID,
                                    Name = cus.Name + " - " + cus.Brand + " - " + cus.SUPPLIERCODES
                                };
                    var ladgerList = query.Take(50).ToList();
                    return Ok(ladgerList);
                }
                else if (ledgerTypeList.Where(x => x.ID == ledgerTypeId).FirstOrDefault().code.ToLower() == "agent")
                {

                    var query = from lb in authContext.LadgerDB
                                join pop in authContext.Peoples
                                on lb.ObjectID equals pop.PeopleID
                                where (pop.DisplayName.ToLower().Contains(name.ToLower()) || pop.PeopleFirstName.ToLower().Contains(name.ToLower()) || pop.PeopleLastName.ToLower().Contains(name.ToLower()))
                                && lb.LadgertypeID == ledgerTypeId
                                && lb.ObjectType == "Agent"
                                select new
                                {
                                    lb.ID,
                                    Name = pop.DisplayName + " - " + pop.Mobile
                                };
                    var ladgerList = query.Take(50).ToList();
                    return Ok(ladgerList);

                }
                else if (ledgerTypeList.Where(x => x.ID == ledgerTypeId).FirstOrDefault().code.ToLower() == "vendor")
                {
                    var query = from lb in authContext.LadgerDB
                                join pop in authContext.VendorDB
                                on lb.ObjectID equals pop.Id
                                where (pop.Name.ToLower().Contains(name.ToLower()) || pop.Code.ToLower().Contains(name.ToLower()))
                                && lb.LadgertypeID == ledgerTypeId
                                && lb.ObjectType == "Vendor"
                                select new
                                {
                                    ID = lb.ID,
                                    Name = pop.Name + " (" + pop.Code + ")",
                                    pop.DepartmentId,
                                    pop.WorkingCompanyId,
                                    pop.WorkingLocationId,
                                    pop.IsTDSApplied,
                                    pop.ExpenseTDSMasterID
                                };
                    var ladgerList = query.Take(50).ToList();

                    return Ok(ladgerList);
                }
                else if(ledgerTypeList.Where(x => x.ID == ledgerTypeId).FirstOrDefault().code.ToLower() == "transporter")
                {
                    var query = from lb in authContext.LadgerDB
                                join pop in authContext.FleetMasterDB
                                on lb.ObjectID equals pop.Id
                                where (pop.TransportName.ToLower().Contains(name.ToLower()) || pop.FleetType.ToLower().Contains(name.ToLower()))
                                && lb.LadgertypeID == ledgerTypeId
                                && lb.ObjectType == "Transporter"
                                select new
                                {
                                    ID = lb.ID,
                                    Name = pop.TransportName + " (" + pop.FleetType + ")",
                                    DepartmentId =0,
                                    WorkingCompanyId  = 0,
                                    WorkingLocationId= 0,
                                    IsTDSApplied  = 0,
                                    ExpenseTDSMasterID= 0
                                };
                    var ladgerList = query.Take(50).ToList();

                    return Ok(ladgerList);
                }
                else
                {
                    var query = from lb in authContext.LadgerDB
                                where (lb.Name.ToLower().Contains(name.ToLower()) || lb.Alias.ToLower().Contains(name.ToLower()) && lb.LadgertypeID == ledgerTypeId)
                                && lb.LadgertypeID == ledgerTypeId
                                select new
                                {
                                    lb.ID,
                                    Name = lb.Name + " - " + lb.Alias
                                };
                    var ladgerList = query.Take(50).ToList();
                    return Ok(ladgerList);

                }
            }

        }

        [Route("GetTDSLedgerList")]
        [HttpGet]
        public IHttpActionResult GetTDSLedgerList()
        {
            using(var authContext = new AuthContext())
            {
                var query = from lb in authContext.LadgerDB
                            join pop in authContext.ExpenseTDSMasterDB
                            on lb.ID equals pop.LedgerId
                            where lb.ObjectType == "TDS"
                            select new
                            {
                                Id = pop.Id,
                                LedgerId = lb.ID,
                                SectionCode = pop.SectionCode, 
                                pop.RateOfTDS,
                                pop.Assessee,
                                lb.Name
                            };
                var ledgerList = query.ToList();
                return Ok(ledgerList);
            }
        }


        [Route("GetByLedgerId/{ledgerId}")]
        [HttpGet]
        public string GetByLedgerId(long ledgerId)
        {
            LadgerHelper helper = new LadgerHelper();
            string ledgerName = helper.GetByLedgerID(ledgerId);
            return ledgerName;
        }

        [Route("Save")]
        [HttpPost]
        public async Task<IHttpActionResult> SaveAsync(Ladger ladger)
        {
            using (var authContext = new AuthContext())
            {
                if (ladger != null)
                {
                    ladger.CreatedBy = userid;
                    ladger.UpdatedBy = userid;
                    ladger.CreatedDate = DateTime.Now;
                    ladger.UpdatedDate = DateTime.Now;
                }
                if (ladger.ID > 0)
                {
                    //this.authContext.Ladgers.Attach(ladger);
                    authContext.Entry(ladger).State = EntityState.Modified;
                }
                else
                {
                    authContext.LadgerDB.Add(ladger);
                }

                await authContext.CommitAsync();
                return Ok(ladger);
            }
        }

        [Route("GetLadgerExport/{fromdate}/{todate}")]
        [AcceptVerbs("Get")]
        //[HttpGet]
        public IHttpActionResult GetDataForExport(DateTime fromdate, DateTime todate)
        {
            using (AuthContext db = new AuthContext())
            {
                var query = from Le in db.LadgerDB
                            join ag in db.AccountGroups
                            on Le.GroupID equals ag.ID
                            where Le.CreatedDate >= fromdate && Le.CreatedDate <= todate
                            //where ac.
                            select new
                            {
                                ID = Le.ID,
                                Name = Le.Name,
                                Alias = Le.Alias,
                                Group = ag.Name,
                                InventoryValuesAreAffected = Le.InventoryValuesAreAffected,
                                Address = Le.Address,
                                Country = Le.Country,
                                PinCode = Le.PinCode,
                                PAN = Le.PAN,
                                RegistrationType = Le.RegistrationType,
                                GSTno = Le.GSTno,



                            };


                var result = query.ToList();


                DataTable dt = Common.Helpers.ListtoDataTableConverter.ToDataTable(result);

                DataSet ds = new DataSet();
                ds.Tables.Add(dt);

                //API.Helper.ExportServices.DataSet_To_Excel(ds, @"C:\Users\ShopKirana\Desktop\angular_backend\Shopkirana-Backend\AngularJSAuthentication.API\Reports\ladger.xlsx");
                return Ok();
            }
        }


        [Route("CustomerConsolidatedLedger/{fromdate}/{todate}")]
        [AcceptVerbs("Get")]
        //[HttpGet]
        public IHttpActionResult CustomerConsolidatedLedger(DateTime fromdate, DateTime todate)
        {
            var fileName = "consolidated_ledger_" + DateTime.Today.ToString("yyyy_MM_dd") + ".xlsx";
            fromdate = fromdate.Date;
            todate = todate.Date.AddDays(1).AddSeconds(-1);
            DataTable dataTable = new DataTable();
            string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandTimeout = 60 * 30; //30 minutes
                command.CommandText = "CustomerConsolidatedLedger";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@StartDate", fromdate));
                command.Parameters.Add(new SqlParameter("@EndDate", todate));
                try
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    using (var da = new SqlDataAdapter(command))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        da.Fill(dataTable);
                    }


                    var folderPath = HttpContext.Current.Server.MapPath(@"~\ReportDownloads");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }



                    var fullPhysicalPath = folderPath + "\\" + fileName;

                    DataSet dataSet = new DataSet();
                    dataSet.Tables.Add(dataTable);
                    ExportServices.DataSet_To_Excel(dataSet, fullPhysicalPath);

                    connection.Close();
                }
                catch (Exception ex)
                {
                    return Ok();
                }

            }

            fileName = "/ReportDownloads/" + fileName;
            return Ok(fileName);
        }

        [Route("SupplierConsolidatedLedgerV1")]
        [AcceptVerbs("Get")]
        //[HttpGet]
        public IHttpActionResult SupplierConsolidatedLedger()
        {
            var fileName = "consolidated_ledger_" + DateTime.Today.ToString("yyyy_MM_dd") + ".xls";
            DataTable dataTable = new DataTable();
            string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandTimeout = 60 * 30; //30 minutes
                command.CommandText = "SupplierOutstandingReport";
                command.CommandType = CommandType.StoredProcedure;
                try
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    using (var da = new SqlDataAdapter(command))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        da.Fill(dataTable);
                    }


                    var folderPath = HttpContext.Current.Server.MapPath(@"~\ReportDownloads");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }



                    var fullPhysicalPath = folderPath + "\\" + fileName;

                    DataSet dataSet = new DataSet();
                    dataSet.Tables.Add(dataTable);
                    ExportServices.DataSet_To_Excel(dataSet, fullPhysicalPath);

                    connection.Close();
                }
                catch (Exception ex)
                {
                    return Ok();
                }

            }

            fileName = "/ReportDownloads/" + fileName;
            return Ok(fileName);
        }
        [Route("SupplierConsolidatedLedger/{fromdate}/{todate}")]
        [AcceptVerbs("Get")]
        //[HttpGet]
        public IHttpActionResult SupplierConsolidatedLedger(DateTime fromdate, DateTime todate)
        {
            var fileName = "consolidated_ledger_" + DateTime.Today.ToString("yyyy_MM_dd") + ".xls";
            fromdate = fromdate.Date;
            todate = todate.Date.AddDays(1).AddSeconds(-1);
            DataTable dataTable = new DataTable();
            string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandTimeout = 60 * 30; //30 minutes
                command.CommandText = "SupplierConsolidatedLedger";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@StartDate", fromdate));
                command.Parameters.Add(new SqlParameter("@EndDate", todate));
                try
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    using (var da = new SqlDataAdapter(command))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        da.Fill(dataTable);
                    }


                    var folderPath = HttpContext.Current.Server.MapPath(@"~\ReportDownloads");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }



                    var fullPhysicalPath = folderPath + "\\" + fileName;

                    DataSet dataSet = new DataSet();
                    dataSet.Tables.Add(dataTable);
                    ExportServices.DataSet_To_Excel(dataSet, fullPhysicalPath);

                    connection.Close();
                }
                catch (Exception ex)
                {
                    return Ok();
                }

            }

            fileName = "/ReportDownloads/" + fileName;
            return Ok(fileName);
        }


        [Route("GetBankTypeLedger")]
        [AcceptVerbs("Get")]
        public IHttpActionResult GetBankTypeLedger()
        {
            using (AuthContext context = new AuthContext())
            {
                var query = from l in context.LadgerDB
                            join lt in context.LadgerTypeDB
                            on l.LadgertypeID equals lt.ID
                            where (lt.code.Trim().ToLower() == "bank" || lt.code.Trim().ToLower() == "purchasediscount")
                            select l;

                var ledgerList = query.ToList();
                return Ok(ledgerList);
            }
        }



        #region get Dashboard for Ledger
        [AllowAnonymous]
        [HttpGet]
        [Route("GetLedgerDashboard")]
        public dynamic GetLedgerDashboard()
        {

            using (var db = new AuthContext())
            {
                var SupplierChat = db.Database.SqlQuery<ledgerDashboardDTO>("LedgerDashboardUI").ToList();
                return SupplierChat;

            }
        }
        #endregion

        [Route("GetDepoLIst/{id}")]
        [HttpGet]
        public dynamic GetDepoLIst(int id)
        {
            using (AuthContext context = new AuthContext())
            {
                long SupplierId = context.LadgerDB.Where(x => x.ID == id).Select(x => x.ObjectID ?? 0).FirstOrDefault();
                var depolist = context.DepoMasters.Where(x => x.SupplierId == SupplierId).Select(x => new { x.DepoId, x.DepoName }).ToList();
                return depolist;
            }


        }

        [Route("GetByLedgerType/ledgerTypeId/{ledgerTypeId}")]
        [HttpGet]
        public IHttpActionResult GetByLedgerType(int ledgerTypeId)
        {
            using (var authContext = new AuthContext())
            {
                    var query = from lb in authContext.LadgerDB
                                where lb.LadgertypeID == ledgerTypeId
                                select new
                                {
                                    lb.ID,
                                    Name = lb.Name + " - " + lb.Alias
                                };
                    var ladgerList = query.ToList();
                    return Ok(ladgerList);
            }

        }

        [Route("GetPOId")]
        [HttpGet]
        public int GeTPoId(long LedgerId) 
        {
            using (AuthContext context = new AuthContext()) {
                int PoId = (from le in context.LadgerEntryDB
                            join ir in context.IRMasterDB
                            on le.ObjectID equals ir.Id
                            where le.ID == LedgerId && le.ObjectType == "IR"
                            select
                                ir.PurchaseOrderId
                            ).FirstOrDefault();
                return PoId;
            }
        }



    }
    public class ledgerDashboardDTO
    {
        public double Credit { get; set; }

        public double Debit { get; set; }

        public double Balance { get; set; }
        public string ObjectType { get; set; }
    }
}
