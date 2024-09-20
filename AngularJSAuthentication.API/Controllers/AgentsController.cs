using AngularJSAuthentication.DataContracts.Transaction;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Agents")]
    public class AgentsController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public object MainReport { get; private set; }

        [Route("")]
        public IEnumerable<People> Get()
        {

            using (var context = new AuthContext())
            {
                logger.Info("Get Peoples: ");
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
                string email = "";
                try
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

                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                        if (claim.Type == "email")
                        {
                            email = claim.Value;
                        }
                    }
                    logger.Info("End Get Company: ");
                    if (Warehouse_id > 0)
                    {
                        List<People> person = context.AllPeoplesWidAgent(compid, Warehouse_id).ToList();
                        return person;
                    }
                    else
                    {
                        List<People> person = context.AllPeoplesAgent(compid).ToList();
                        return person;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting Peoples " + ex.Message);
                    return null;
                }

            }
        }

        #region Only active agent Get API
        /// <summary>
        /// Created Date 19/04/2019
        /// Created By Raj
        /// this api used to get all active agent 
        /// </summary>
        /// <returns>person</returns>
        [Route("Activeagent")]
        public IEnumerable<People> GetActive(int WarehouseId)
        {
            using (var context = new AuthContext())
            {
                logger.Info("Get Peoples: ");
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
                string email = "";
                try
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

                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                        if (claim.Type == "email")
                        {
                            email = claim.Value;
                        }
                    }
                    logger.Info("End Get Company: ");

                    Warehouse_id = WarehouseId;
                    if (Warehouse_id > 0)
                    {
                        List<People> person = context.AllPeoplesWidActiveAgent(compid, Warehouse_id).ToList();
                        return person;
                    }
                    else
                    {
                        List<People> person = context.AllPeoplesActiveAgent(compid).ToList();
                        return person;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting Peoples " + ex.Message);
                    return null;
                }
            }

        }

        #endregion
        [Route("user")]
        public People Get(int PeopleId)
        {
            using (var context = new AuthContext())
            {
                int compid = 0, userid = 0;
                try
                {
                    var identity = User.Identity as ClaimsIdentity;

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
                    People person = context.Peoples.Where(u => u.PeopleID == PeopleId && u.CompanyId == compid).SingleOrDefault();
                    return person;

                }

                catch (Exception ex)
                {
                    logger.Error("Error in getting Peoples " + ex.Message);
                    return null;
                }
            }
        }
        [Route("")]
        public IEnumerable<People> Get(string department)
        {
            using (var context = new AuthContext())
            {
                logger.Info("Get Peoples: ");
                int compid = 0, userid = 0;
                try
                {
                    var identity = User.Identity as ClaimsIdentity;

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
                    logger.Info("End Get Company: ");
                    int CompanyId = compid;
                    List<People> person = context.AllPeoplesDep(department, CompanyId).ToList();
                    return person;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting Peoples " + ex.Message);
                    return null;
                }
            }
        }

        [ResponseType(typeof(People))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public People Put(People item)
        {
            using (var context = new AuthContext())
            {
                try
                {
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

                    item.CompanyId = compid;
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    return context.PutPeoplebyAdmin(item);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add Peoples " + ex.Message);

                    return null;
                }
            }
        }

        [Route("GetAgentOrderData")]
        public PaggingData_AgentAmount GetAgentOrderData(string AgentCode, int list, int page)
        {
            using (var context = new AuthContext())
            {
                logger.Info("Get Peoples: ");
                int compid = 0, userid = 0; int Warehouse_id = 0;
                try
                {
                    var identity = User.Identity as ClaimsIdentity;

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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }
                    logger.Info("End Get Company: ");
                    int CompanyId = compid;
                    var OrderData = context.GetAgentAllOrder(Warehouse_id, compid, AgentCode, list, page);
                    return OrderData;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting Peoples " + ex.Message);
                    return null;
                }
            }
        }

        [Route("GetAgentDataById")]
        public List<AgentData> GetAgentDataById(string issurenceid)
        {
            using (var context = new AuthContext())
            {
                logger.Info("Get Peoples: ");
                int compid = 0, userid = 0; int Warehouse_id = 0;
                try
                {
                    var identity = User.Identity as ClaimsIdentity;

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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }
                    logger.Info("End Get Company: ");
                    var data = context.GetAgentData(issurenceid, compid, Warehouse_id).ToList();
                    return data;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting Peoples " + ex.Message);
                    return null;
                }
            }
        }

        [Route("AgentAmountData")]
        [AcceptVerbs("POST")]
        public int AgentAmountData(AgentAmount aj)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0; int Warehouse_id = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }
                    int result = context.AddAgentAmount(compid, Warehouse_id, aj);
                    return result;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add Peoples " + ex.Message);
                    return 0;
                }
            }
        }

        [Route("AddAgent")]
        [AcceptVerbs("POST")]
        public int AddAgent(People aj)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0; int Warehouse_id = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }
                    int result = context.AddAgent(compid, Warehouse_id, aj);
                    return result;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add Peoples " + ex.Message);
                    return 0;
                }
            }
        }


        [HttpGet]
        [Route("GetAgentAmount")]
        public List<AgentAmountDatas> GetAgentAmount()
        {
            using (var db = new AuthContext())
            {
                int compid = 0, userid = 0; int Warehouse_id = 0;
                try
                {
                    var identity = User.Identity as ClaimsIdentity;

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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }
                    List<AgentAmountDatas> data = (from a in db.AgentAmountDb
                                                   join p in db.Peoples on a.AgentId equals p.PeopleID
                                                   where a.IsDeleted == false && a.IsActive == true
                                                  && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                                   select new AgentAmountDatas
                                                   {
                                                       AgentName = p.DisplayName,
                                                       AgentAmount = a.AgentAmounts,
                                                       AgentAmountId = a.AgentAmountId,
                                                       AgentId = a.AgentId
                                                   }).ToList();
                    return data;

                }

                catch (Exception ex)
                {
                    logger.Error("Error in getting Peoples " + ex.Message);
                    return null;
                }
            }

        }

        [Route("DeleteAgent")]
        [AcceptVerbs("Delete")]
        public bool Remove(int AgentId)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start delete Item Master: ");
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0; int Warehouse_id = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    bool data = context.DeleteAgent(AgentId, CompanyId, Warehouse_id);
                    return data;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in delete Item Master " + ex.Message);
                    return false;
                }
            }
        }

        [Route("")]
        [AcceptVerbs("Delete")]
        public bool RemoveAgent(int AgentAmountId)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start delete Item Master: ");
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0; int Warehouse_id = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    bool data = context.DeleteAgentAmount(AgentAmountId, CompanyId, Warehouse_id);
                    return data;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in delete Item Master " + ex.Message);
                    return false;
                }
            }
        }

        [Route("UpdateAgentAmountData")]
        [AcceptVerbs("POST")]
        public int UpdateAgentAmountData(AgentAmount aj)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0; int Warehouse_id = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }
                    int result = context.UpdateAgentAmount(compid, Warehouse_id, aj);
                    return result;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add Peoples " + ex.Message);
                    return 0;
                }
            }
        }

        [Route("GenerateAgentCode")]
        [HttpGet]
        public dynamic GenerateAgentCode()
        {
            using (var context = new AuthContext())
            {
                try
                {

                    //int CompanyId = compid;
                    //if (warehouseid > 0)
                    //{
                    //    p.AgentCode = db.gtAgentCodeByID(warehouseid);
                    //    var atm = Int32.Parse(p.AgentCode);
                    //    return atm;
                    //}
                    //else
                    //{
                    //    return null;
                    //}

                    return context.Peoples.Max(x => x.PeopleID) + 1;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in ItemMaster " + ex.Message);
                    logger.Info("End  ItemMaster: ");
                    return null;
                }
            }
        }

        public class bound
        {
            public int TotalOrders { get; set; }
            public int cuscount { get; set; }
            public int TotalCustomersInMonth { get; set; }
            public int TotalCustomerslMonth { get; set; }
            public int TotalCustomersyDay { get; set; }
            public int TotalCustomersToday { get; set; }
            public double Totalsell { get; set; }
            public double TotalCostInMonthsell { get; set; }
            public double TotalCostTodaysell { get; set; }
            public double TotalCostYDaysell { get; set; }
            public int status { get; set; }
            public int statusor { get; set; }
            public int statusTotal { get; set; }
            public int TotalActive { get; set; }
            public int TotalInActives { get; set; }
            public int TotalActiveCustomerInMonth { get; set; }
            public int TotalActiveCustomerlMonth { get; set; }
            public int TotalActiveCustomerYDay { get; set; }
            public int TotalActiveCustomerToday { get; set; }
            public int TotalInActiveCustomerToday { get; set; }
            public int TotalInActiveCustomerInMonth { get; set; }
            public int TotalInActiveCustomerYDay { get; set; }
            public int TotalOrdersInmonth { get; set; }
            public int TotalOrdersToday { get; set; }
            public int TotalOrdersyDay { get; set; }
            public double totalcommission { get; set; }
        }

        [Route("GetAgentCustomer")]
        [HttpGet]

        public dynamic GetAgent(string AgentCode)
        {
            using (var db = new AuthContext())
            {
                bound b = new Controllers.AgentsController.bound();
                logger.Info("start City: ");
                Customer customer = new Customer();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
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
                    int CompanyId = compid;
                    DateTime now = DateTime.Now;
                    var startDate = new DateTime(now.Year, now.Month, 1);
                    var endDate = startDate.AddMonths(1).AddDays(-1);

                    DateTime lM = DateTime.Now.AddMonths(-1).AddYears(-1);
                    int lMonth = lM.Month;
                    int lYear = lM.Year;

                    DateTime yesterday = DateTime.Now.AddDays(-1);
                    int ydMonth = yesterday.Month;
                    int ydYear = yesterday.Year;
                    int ydDay = yesterday.Day;
                    #region Total Customer
                    b.TotalCustomersInMonth = 0;
                    var cust = db.Customers.Where(x => x.AgentCode == AgentCode && x.Deleted == false).ToList();
                    var custcount = cust.Count();
                    b.cuscount = custcount;

                    //var TotalCustomerInMonth = db.CustWarehouseDB.AsQueryable().Where(x => x.AgentCode == AgentCode && x.Deleted == false && x.CreatedDate >= startDate && x.CreatedDate <= endDate).Count();
                    //b.TotalCustomersInMonth = b.TotalCustomersInMonth + TotalCustomerInMonth;

                    //var TotalCustomerlMonth = db.CustWarehouseDB.AsQueryable().Where(x => x.AgentCode == AgentCode && x.Deleted == false && x.CreatedDate.Month == lMonth).Count();
                    //b.TotalCustomerslMonth = b.TotalCustomerslMonth + TotalCustomerlMonth;

                    //var TotalCustomerToday = db.CustWarehouseDB.AsQueryable().Where(x => x.AgentCode == AgentCode && x.Deleted == false && x.CreatedDate > DateTime.Today.Date).Count();
                    //b.TotalCustomersToday = b.TotalCustomersToday + TotalCustomerToday;

                    //var TotalCustomeryDay = db.CustWarehouseDB.AsQueryable().Where(x => x.AgentCode == AgentCode && x.Deleted == false && x.CreatedDate == yesterday.Date).Count();
                    //b.TotalCustomersyDay = b.TotalCustomersyDay + TotalCustomerToday;
                    #endregion
                    b.status = 0;
                    b.statusor = 0;
                    b.statusTotal = 0;
                    if (cust != null)
                    {
                        var ActiveCustomer = cust.AsEnumerable().Where(x => x.Active == true);
                        var TotalActive = ActiveCustomer.Count();
                        b.TotalActive = TotalActive;
                        var InActive = cust.AsEnumerable().Where(x => x.Active == false);
                        var TotalInActives = InActive.Count();
                        b.TotalInActives = TotalInActives;
                        b.statusTotal = 0;
                        b.TotalOrders = 0;
                        b.Totalsell = 0;
                        b.TotalCostInMonthsell = 0;
                        b.TotalCostTodaysell = 0;
                        b.TotalCostYDaysell = 0;
                        b.totalcommission = 0;
                        b.TotalActiveCustomerlMonth = 0;
                        b.TotalActiveCustomerInMonth = 0;
                        b.TotalActiveCustomerToday = 0;
                        b.TotalInActiveCustomerInMonth = 0;
                        b.TotalInActiveCustomerToday = 0;
                        b.TotalInActiveCustomerYDay = 0;
                        b.TotalOrdersInmonth = 0;
                        b.TotalOrdersToday = 0;
                        b.TotalOrdersyDay = 0;
                        var comtotalbasedagent = 0;


                        foreach (var i in cust)
                        {
                            #region Total Commission 

                            var agentMapeddata = db.OrderDispatchedDetailss.Where(x => x.CustomerId == i.CustomerId && /*x.Status == "Account settled" &&*/ x.Deleted == false).ToList();
                            var totalAmount = 0;
                            foreach (var a in agentMapeddata)
                            {
                                var getItem = db.itemMasters.AsQueryable().Where(x => x.ItemId == a.ItemId).Select(x => x.SubsubcategoryName).SingleOrDefault();

                                try
                                {
                                    var susubcat = db.SubsubCategorys.AsQueryable().Where(e => e.SubsubcategoryName == getItem).ToList();
                                    var singlevalue = susubcat.AsQueryable().GroupBy(x => x.SubsubcategoryName).SingleOrDefault();
                                    var commission = singlevalue.AsQueryable().Select(e => e.CommisionPercent).SingleOrDefault();
                                    var com = a.TotalAmt * commission / 100;
                                    b.totalcommission = b.totalcommission + Convert.ToDouble(com);
                                }
                                catch (Exception ex)
                                {

                                }

                            }
                            #endregion

                            #region Active Customer
                            var ActiveCustomerInMonth = db.DbOrderMaster.AsQueryable().Where(pkg => pkg.CustomerId == i.CustomerId && pkg.active == true && pkg.CreatedDate >= startDate && pkg.CreatedDate <= endDate).GroupBy(x => x.CustomerId).Count();
                            b.TotalActiveCustomerInMonth = b.TotalActiveCustomerInMonth + ActiveCustomerInMonth;

                            //var ActiveCustomerlMonth = db.DbOrderMaster.AsQueryable().Where(pkg => pkg.CustomerId == i.CustomerId && pkg.active == true && pkg.CreatedDate.Month == lMonth).GroupBy(x => x.CustomerId).Count();
                            //b.TotalActiveCustomerlMonth = b.TotalActiveCustomerlMonth + ActiveCustomerlMonth;

                            var ActiveCustomerToday = db.DbOrderMaster.AsQueryable().Where(pkg => pkg.CustomerId == i.CustomerId && pkg.active == true && pkg.CreatedDate > DateTime.Today.Date).GroupBy(x => x.CustomerId).Count();
                            b.TotalActiveCustomerToday = b.TotalActiveCustomerToday + ActiveCustomerToday;

                            var ActiveCustomerYDay = db.DbOrderMaster.AsQueryable().Where(pkg => pkg.CustomerId == i.CustomerId && pkg.active == true && pkg.CreatedDate == yesterday.Date).GroupBy(x => x.CustomerId).Count();
                            b.TotalActiveCustomerYDay = b.TotalActiveCustomerYDay + ActiveCustomerYDay;

                            #endregion

                            #region InActive Customer
                            var InActiveCustomerInMonth = db.DbOrderMaster.AsQueryable().Where(pkg => pkg.CustomerId == i.CustomerId && pkg.active == false && pkg.CreatedDate >= startDate && pkg.CreatedDate <= endDate).GroupBy(x => x.CustomerId).Count();
                            b.TotalInActiveCustomerInMonth = b.TotalInActiveCustomerInMonth + InActiveCustomerInMonth;

                            //var ActiveCustomerlMonth = db.DbOrderMaster.AsQueryable().Where(pkg => pkg.CustomerId == i.CustomerId && pkg.active == false && pkg.CreatedDate.Month == lMonth).GroupBy(x => x.CustomerId).Count();
                            //b.TotalActiveCustomerlMonth = b.TotalActiveCustomerlMonth + ActiveCustomerlMonth;

                            var InActiveCustomerToday = db.DbOrderMaster.AsQueryable().Where(pkg => pkg.CustomerId == i.CustomerId && pkg.active == false && pkg.CreatedDate > DateTime.Today.Date).GroupBy(x => x.CustomerId).Count();
                            b.TotalInActiveCustomerToday = b.TotalInActiveCustomerToday + InActiveCustomerToday;

                            var InActiveCustomerYDay = db.DbOrderMaster.AsQueryable().Where(pkg => pkg.CustomerId == i.CustomerId && pkg.active == false && pkg.CreatedDate == yesterday.Date).GroupBy(x => x.CustomerId).Count();
                            b.TotalInActiveCustomerYDay = b.TotalInActiveCustomerYDay + InActiveCustomerYDay;

                            #endregion

                            #region Total Order
                            var TotalOrder = db.DbOrderMaster.Where(x => x.CustomerId == i.CustomerId && x.Deleted == false).ToList();
                            var OrderTotal = TotalOrder.Count();
                            b.TotalOrders = b.TotalOrders + OrderTotal;

                            var TotalOrderInmonth = db.DbOrderMaster.Where(x => x.CustomerId == i.CustomerId && x.Deleted == false && x.CreatedDate >= startDate && x.CreatedDate <= endDate).Count();
                            b.TotalOrdersInmonth = b.TotalOrdersInmonth + TotalOrderInmonth;

                            var TotalOrderToday = db.DbOrderMaster.Where(x => x.CustomerId == i.CustomerId && x.Deleted == false && x.CreatedDate > DateTime.Today.Date).Count();
                            b.TotalOrdersToday = b.TotalOrdersToday + TotalOrderToday;

                            var TotalOrderyDay = db.DbOrderMaster.Where(x => x.CustomerId == i.CustomerId && x.Deleted == false && x.CreatedDate == yesterday.Date).Count();
                            b.TotalOrdersyDay = b.TotalOrdersyDay + TotalOrderyDay;
                            #endregion

                            #region Status
                            foreach (var s in TotalOrder)
                            {
                                if (s.Status == "Delivered")
                                {
                                    b.status = b.status + 1;
                                }
                                if (s.Status == "Order Canceled")
                                {
                                    b.statusor = b.statusor + 1;
                                }
                                b.statusTotal = b.status + b.statusor;
                            }
                            b.statusTotal = b.statusTotal;
                            #endregion


                            #region Sale
                            double TotalCost = 0;
                            double TotalCostInMonth = 0;
                            double TotalCostToday = 0;
                            double TotalCostYDay = 0;
                            try
                            {
                                TotalCost = db.DbOrderMaster.AsQueryable().Where(pkg => pkg.CustomerId == i.CustomerId).Sum(pkg => pkg.TotalAmount);
                                TotalCostInMonth = db.DbOrderMaster.AsQueryable().Where(pkg => pkg.CustomerId == i.CustomerId && pkg.Deleted == false && pkg.CreatedDate >= startDate && pkg.CreatedDate <= endDate).Sum(pkg => pkg.TotalAmount);
                                TotalCostToday = db.DbOrderMaster.AsQueryable().Where(pkg => pkg.CustomerId == i.CustomerId && pkg.Deleted == false && pkg.CreatedDate > DateTime.Today.Date).Sum(pkg => pkg.TotalAmount);
                                TotalCostYDay = db.DbOrderMaster.AsQueryable().Where(pkg => pkg.CustomerId == i.CustomerId && pkg.Deleted == false && pkg.CreatedDate == yesterday.Date).Sum(pkg => pkg.TotalAmount);
                            }
                            catch { }
                            b.Totalsell = b.Totalsell + TotalCost;
                            b.TotalCostInMonthsell = b.TotalCostInMonthsell + TotalCostInMonth;
                            b.TotalCostTodaysell = b.TotalCostTodaysell + TotalCostToday;
                            b.TotalCostYDaysell = b.TotalCostYDaysell + TotalCostYDay;
                        }
                        b.Totalsell = b.Totalsell;
                        b.TotalCostInMonthsell = b.TotalCostInMonthsell;
                        b.TotalCostTodaysell = b.TotalCostTodaysell;
                        b.TotalCostYDaysell = b.TotalCostYDaysell;
                        #endregion
                        return b;
                    }
                    logger.Info("End  Customer: ");
                    return b;
                }


                catch (Exception ex)
                {
                    logger.Error("Error in Customer " + ex.Message);
                    logger.Info("End  Customer: ");
                    return 0;
                }
            }
        }




        //[Route("GetAgentDboyName")]
        //[HttpGet]

        //public List<People> GetAgentDboyName(string AgentCode1)
        //{      
        //    int compid = 0, userid = 0;
        //    //List<People> person = new List<People>;
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;

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
        //        var person = db.Peoples.Where(u => u.AgentCode == AgentCode1 && u.CompanyId == compid && u.Type == "Delivery Boy").ToList();
        //        return person;

        //    }

        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in getting Peoples " + ex.Message);
        //        return null;
        //    }

        //}


        [Route("GetAgentDboyName")]
        [HttpPost]

        public dynamic Post(DBOYAgeninfo DBI)
        {
            using (var db = new AuthContext())
            {
                logger.Info("start OrderMaster: ");
                List<OrderDetailsExport> mainreport = new List<OrderDetailsExport>();

                DateTime start = DateTime.Parse("01-01-2017 00:00:00");
                DateTime end = DateTime.Today.AddDays(1);
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
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

                    if (DBI.datefrom == null)
                    {
                        DBI.datefrom = DateTime.Parse("01-01-2017 00:00:00");
                        DBI.dateto = DateTime.Today.AddDays(1);
                    }
                    else
                    {
                        start = DBI.datefrom.GetValueOrDefault();
                        end = DBI.dateto.GetValueOrDefault();
                    }
                    foreach (var s in DBI.ids)
                    {
                        // OrderDetailsExport oh = new OrderDetailsExport();
                        string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.AgentCode='" + s.AgentCode + "' and r.Name='Delivery Boy' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                        var peoplledata = db.Database.SqlQuery<People>(query).SingleOrDefault();

                        //var peoplledata = db.Peoples.Where(x => x.AgentCode == s.AgentCode && x.Department == "Delivery Boy").SingleOrDefault();
                        var mo = peoplledata.Mobile;
                        var olist = getDBoyAgentsHistory(mo, DBI.datefrom, DBI.dateto);
                        mainreport = olist;
                        //mainreport.Add(olist);
                        return mainreport;
                    }
                    return mainreport;

                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return null;
                }
            }
        }
        public dynamic getDBoyAgentsHistory(string mob, DateTime? start, DateTime? end)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    List<OrderDetailsExport> newdata = new List<OrderDetailsExport>();


                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouseid = 1;
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


                    if (mob != null)
                    {
                        newdata = (from od in db.DbOrderDetails
                                   join odm in db.OrderDispatchedMasters on od.OrderId equals odm.OrderId
                                   where od.Deleted == false && od.OrderDate >= start && od.OrderDate <= end && od.WarehouseId == Warehouseid
                                   && odm.DboyMobileNo == mob

                                   select new OrderDetailsExport
                                   {
                                       ItemId = od.ItemId,
                                       itemname = od.itemname,
                                       itemNumber = od.itemNumber,
                                       CustomerName = od.CustomerName,
                                       //ShopName = od.ShopName,
                                       //Skcode = od.Skcode,
                                       OrderId = od.OrderId,
                                       UnitPrice = od.UnitPrice,
                                       Mobile = od.Mobile,
                                       qty = od.qty,
                                       WarehouseName = od.WarehouseName,
                                       Date = od.OrderDate,
                                       Status = od.Status,
                                       TotalAmt = od.TotalAmt,
                                       DboyName = odm.DboyName,
                                       DboyMobileNo = odm.DboyMobileNo,
                                       orderDispatchedDetailsExport = (from d in db.OrderDispatchedDetailss
                                                                       where d.Deleted == false && d.OrderDetailsId == od.OrderDetailsId && d.WarehouseId == Warehouseid
                                                                       && d.Mobile == mob
                                                                       select new OrderDispatchedDetailsExport
                                                                       {
                                                                           ItemId = d.ItemId,
                                                                           itemname = d.itemname,
                                                                           itemNumber = d.itemNumber,
                                                                           CustomerName = d.CustomerName,
                                                                           //ShopName = iii.ShopName,
                                                                           //Skcode = iii.Skcode,
                                                                           Mobile = d.Mobile,
                                                                           QtyChangeReason = d.QtyChangeReason,
                                                                           OrderId = d.OrderId,
                                                                           dUnitPrice = d.UnitPrice,
                                                                           TaxPercentage = d.TaxPercentage,
                                                                           dqty = d.qty,
                                                                           WarehouseName = d.WarehouseName,
                                                                           Date = d.OrderDate,
                                                                           //Status = d.Status,
                                                                           dTotalAmt = d.TotalAmt,

                                                                       }).ToList()
                                   }).OrderByDescending(x => x.OrderId).ToList();
                    }
                    else
                    {
                        newdata = (from od in db.DbOrderDetails
                                   where od.WarehouseId.Equals(Warehouseid)
                                   join item in db.itemMasters on od.ItemId equals item.ItemId
                                   join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                   join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                   select new OrderDetailsExport
                                   {
                                       ItemId = od.ItemId,
                                       itemname = item.itemname,
                                       itemNumber = item.Number,
                                       sellingSKU = item.SellingSku,
                                       price = od.price,
                                       UnitPrice = od.UnitPrice,
                                       MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                       qty = od.qty,
                                       DiscountPercentage = od.DiscountPercentage,
                                       DiscountAmmount = od.DiscountAmmount,
                                       TaxPercentage = od.TaxPercentage,
                                       TaxAmmount = od.TaxAmmount,
                                       TotalAmt = od.TotalAmt,
                                       CategoryName = cat.CategoryName,
                                       BrandName = sbcat.SubsubcategoryName,
                                       orderDispatchedDetailsExport = (from d in db.OrderDispatchedDetailss
                                                                       join items in db.itemMasters on od.ItemId equals item.ItemId
                                                                       //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                       //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                       select new OrderDispatchedDetailsExport
                                                                       {
                                                                           ItemId = d.ItemId,
                                                                           itemname = items.itemname,
                                                                           itemNumber = items.Number,
                                                                           sellingSKU = items.SellingSku,
                                                                           price = d.price,
                                                                           dUnitPrice = d.UnitPrice,
                                                                           MinOrderQtyPrice = d.UnitPrice * d.MinOrderQty,
                                                                           dqty = d.qty,
                                                                           DiscountPercentage = d.DiscountPercentage,
                                                                           DiscountAmmount = d.DiscountAmmount,
                                                                           TaxPercentage = d.TaxPercentage,
                                                                           TaxAmmount = d.TaxAmmount,
                                                                           dTotalAmt = d.TotalAmt,
                                                                           CategoryName = cat.CategoryName,
                                                                           BrandName = sbcat.SubsubcategoryName
                                                                       }).ToList()     /*a.orderDetails,*/
                                   }).OrderByDescending(x => x.OrderId).ToList();
                    }
                    if (newdata.Count == 0)
                    {
                        return newdata;
                    }
                    return newdata;
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }

        }

        #region get Active Agent of company month wise
        [Route("ActiveAgent")]
        [HttpGet]
        [AllowAnonymous]
        public dynamic ActiveAgent()
        {
            using (AuthContext context = new AuthContext())
            {
                List<ActiveAgentDc> result = new List<ActiveAgentDc>();

                string Query = " select cast(month(CreatedDate)as int) as Months, count(*) as TotalActiveAgent" +
             " from People where YEAR(CreatedDate) = 2019 and Active = 1 and Type = 'Agent'  group by MONTH(CreatedDate)";
                result = context.Database.SqlQuery<ActiveAgentDc>(Query).ToList();
                foreach (var item in result)
                {

                    if (item.Months == 1)
                    {
                        item.MonthName = "January";

                    }
                    else if (item.Months == 2)
                    {
                        item.MonthName = "February";

                    }
                    else if (item.Months == 3)
                    {
                        item.MonthName = "March";
                    }
                    else if (item.Months == 4)
                    {
                        item.MonthName = "April";
                    }
                    else if (item.Months == 5)
                    {
                        item.MonthName = "May";
                    }
                    else if (item.Months == 6)
                    {
                        item.MonthName = "June";
                    }
                    else if (item.Months == 7)
                    {
                        item.MonthName = "July";
                    }
                    else if (item.Months == 8)
                    {
                        item.MonthName = "August";
                    }
                    else if (item.Months == 9)
                    {
                        item.MonthName = "September";
                    }
                    else if (item.Months == 10)
                    {
                        item.MonthName = "October";
                    }
                    else if (item.Months == 11)
                    {
                        item.MonthName = "November";
                    }
                    else if (item.Months == 12)
                    {
                        item.MonthName = "December";
                    }

                }

                return result;
            }
            #endregion
        }


        #region get Active KPP of company month wise
        [Route("ActiveKPP")]
        [HttpGet]
        [AllowAnonymous]
        public dynamic ActiveKPP()
        {
            using (AuthContext context = new AuthContext())
            {
                List<ActiveKpp> result = new List<ActiveKpp>();

                string Query = " select month(CreatedDate) as Months, count(*) as TotalActivekpp from Warehouses where YEAR(CreatedDate) = 2019 and IsKPP = 1 group by MONTH(CreatedDate)";
                result = context.Database.SqlQuery<ActiveKpp>(Query).ToList();
                foreach (var item in result)
                {

                    if (item.Months == 1)
                    {
                        item.MonthName = "January";

                    }
                    else if (item.Months == 2)
                    {
                        item.MonthName = "February";

                    }
                    else if (item.Months == 3)
                    {
                        item.MonthName = "March";
                    }
                    else if (item.Months == 4)
                    {
                        item.MonthName = "April";
                    }
                    else if (item.Months == 5)
                    {
                        item.MonthName = "May";
                    }
                    else if (item.Months == 6)
                    {
                        item.MonthName = "June";
                    }
                    else if (item.Months == 7)
                    {
                        item.MonthName = "July";
                    }
                    else if (item.Months == 8)
                    {
                        item.MonthName = "August";
                    }
                    else if (item.Months == 9)
                    {
                        item.MonthName = "September";
                    }
                    else if (item.Months == 10)
                    {
                        item.MonthName = "October";
                    }
                    else if (item.Months == 11)
                    {
                        item.MonthName = "November";
                    }
                    else if (item.Months == 12)
                    {
                        item.MonthName = "December";
                    }

                }
                return result;
            }
            #endregion


        }


        #region get  Active Cluster of company month wise
        [Route("ActiveClusters")]
        [HttpGet]
        [AllowAnonymous]
        public dynamic ActiveClusters()
        {
            using (AuthContext context = new AuthContext())
            {
                List<ActiveCluster> result = new List<ActiveCluster>();

                string Query = " select month(CreatedDate) as Months, count(*) as TotalActiveCluster from Clusters  where YEAR(CreatedDate) = 2019 and Active = 1 group by MONTH(CreatedDate)";
                result = context.Database.SqlQuery<ActiveCluster>(Query).ToList();
                foreach (var item in result)
                {

                    if (item.Months == 1)
                    {
                        item.MonthName = "January";

                    }
                    else if (item.Months == 2)
                    {
                        item.MonthName = "February";

                    }
                    else if (item.Months == 3)
                    {
                        item.MonthName = "March";
                    }
                    else if (item.Months == 4)
                    {
                        item.MonthName = "April";
                    }
                    else if (item.Months == 5)
                    {
                        item.MonthName = "May";
                    }
                    else if (item.Months == 6)
                    {
                        item.MonthName = "June";
                    }
                    else if (item.Months == 7)
                    {
                        item.MonthName = "July";
                    }
                    else if (item.Months == 8)
                    {
                        item.MonthName = "August";
                    }
                    else if (item.Months == 9)
                    {
                        item.MonthName = "September";
                    }
                    else if (item.Months == 10)
                    {
                        item.MonthName = "October";
                    }
                    else if (item.Months == 11)
                    {
                        item.MonthName = "November";
                    }
                    else if (item.Months == 12)
                    {
                        item.MonthName = "December";
                    }

                }

                return result;
            }

        }
        #endregion
        #region get cheque bounce  info on behalf agent
        /// <summary>
        /// Created Date:04/03/2020
        /// Created by Raj
        /// </summary>
        /// <returns></returns>
        [Route("GetChequeBounceinfo")]
        [HttpPost]
        [Authorize]
        public AgentChequeBounceInfoList GetChequeBounceinfo(AgentChequeBouncePaginator agentChequeBouncePaginator)
        {

            using (var context = new AuthContext())
            {

                AgentChequeBounceInfoList agentChequeBounceInfoList = new AgentChequeBounceInfoList();
                if (agentChequeBouncePaginator.AgentId > 0)
                {
                    var query = "Select od.CustomerName,Cust.Skcode,Cust.ShopName,cheq.ChequeNumber,cheq.ChequeAmt,cheq.ChequeDate,cheq.ChequeBankName,cheq.Orderid  from Customers cust " +
                                "inner join OrderDispatchedMasters od on cust.CustomerId=od.CustomerId " +
                                "inner join ChequeCollections cheq on cheq.Orderid=od.OrderId where cheq.ChequeStatus=4 and od.OrderTakenSalesPersonId=" + agentChequeBouncePaginator.AgentId;

                    agentChequeBounceInfoList.agentChequeBounceInfo = context.Database.SqlQuery<AgentChequeBounceInfo>(query).Skip(agentChequeBouncePaginator.Skip * agentChequeBouncePaginator.Take).Take(agentChequeBouncePaginator.Take).ToList();
                    agentChequeBounceInfoList.Count = context.Database.SqlQuery<AgentChequeBounceInfo>(query).Count();
                    agentChequeBounceInfoList.TotalAmount = Convert.ToDouble(context.Database.SqlQuery<AgentChequeBounceInfo>(query).Sum(x => x.ChequeAmt));
                }
                else if(agentChequeBouncePaginator.WarehouseId>0) {
                    var query = "Select od.CustomerName,Cust.Skcode,Cust.ShopName,cheq.ChequeNumber,cheq.ChequeAmt,cheq.ChequeDate,cheq.ChequeBankName,cheq.Orderid  from Customers cust " +
                                    "inner join OrderDispatchedMasters od on cust.CustomerId=od.CustomerId " +
                                    "inner join ChequeCollections cheq on cheq.Orderid=od.OrderId where cheq.ChequeStatus=4 and Cust.WarehouseId=" + agentChequeBouncePaginator.WarehouseId;

                    agentChequeBounceInfoList.agentChequeBounceInfo = context.Database.SqlQuery<AgentChequeBounceInfo>(query).Skip(agentChequeBouncePaginator.Skip * agentChequeBouncePaginator.Take).Take(agentChequeBouncePaginator.Take).ToList();
                    agentChequeBounceInfoList.Count = context.Database.SqlQuery<AgentChequeBounceInfo>(query).Count();
                    agentChequeBounceInfoList.TotalAmount = Convert.ToDouble(context.Database.SqlQuery<AgentChequeBounceInfo>(query).Sum(x => x.ChequeAmt));

                }
                return agentChequeBounceInfoList;
                
            }
        }
        #endregion
        #region get executive info
        /// <summary>
        /// Created Date:12/04/2020
        /// Created by Raj
        /// </summary>
        /// <returns></returns>
        [Route("GetSalesLeadExecutive")]
        [HttpGet]
        [AllowAnonymous]
        //[Authorize]
        public List<ExecutiveInfo> GetSalesLeadExecutive(int Id)
        {

            using (var context = new AuthContext())
            {
                var data = context.Peoples.Where(x => x.ReportPersonId == Id).Select(x=> new ExecutiveInfo {
                    ExecutiveId = x.PeopleID,
                    ExecutiveName= x.DisplayName }).ToList();
                return data;

            }
        }
        #endregion


        public class ActiveCluster
        {
            public int Months { get; set; }
            public int TotalActiveCluster { get; set; }
            public string MonthName { get; set; }

        }



        public class ActiveKpp
        {

            public int Months { get; set; }
            public int TotalActivekpp { get; set; }
            public string MonthName { get; set; }

        }

        public class ActiveAgentDc
        {

            //public int datefrom { get; set; }
            //public int dateto { get; set; }

            public int Months { get; set; }
            public int TotalActiveAgent { get; set; }
            public string MonthName { get; set; }
        }

        public class DBOYAgeninfo
        {
            public List<dbinf1> ids { get; set; }
            public DateTime? datefrom { get; set; }
            public DateTime? dateto { get; set; }
        }
        public class dbinf1
        {
            public int id { get; set; }
            public string AgentCode { get; set; }
        }
        public class OrderDetailsExport
        {

            public int ItemId { get; set; }
            public string itemname { get; set; }
            public string WarehouseName { get; set; }

            public string itemNumber { get; set; }
            public string CategoryName { get; set; }
            public string BrandName { get; set; }
            public string sellingSKU { get; set; }
            public double price { get; set; }
            public double UnitPrice { get; set; }
            public double MinOrderQtyPrice { get; set; }
            public int qty { get; set; }
            public double DiscountPercentage { get; set; }
            public double DiscountAmmount { get; set; }
            public double TaxPercentage { get; set; }
            public double SGSTTaxPercentage { get; set; }
            public double CGSTTaxPercentage { get; set; }
            public double TaxAmmount { get; set; }
            public double SGSTTaxAmmount { get; set; }
            public double CGSTTaxAmmount { get; set; }
            public double TotalAmt { get; set; }

            // for export ftr
            public double dUnitPrice;
            public int dqty;
            public int OrderId { get; set; }
            public string CustomerName { get; set; }
            public string Skcode { get; set; }
            public string ShopName { get; set; }
            public string Mobile { get; set; }
            public string Status { get; set; }

            public string DboyName
            {
                get; set;
            }
            public string DboyMobileNo
            {
                get; set;
            }

            public DateTime Date { get; set; }

            public virtual ICollection<OrderDispatchedDetails> orderDispatchedDetails { get; set; }
            public List<OrderDispatchedDetailsExport> orderDispatchedDetailsExport { get; set; }
        }
        public class AgentAmountDatas
        {
            public string AgentName { get; set; }

            public double? AgentAmount { get; set; }

            public int AgentAmountId
            {
                get; set;
            }

            public int AgentId
            {
                get; set;
            }
        }

        public class ExecutiveInfo
        {
            public int ExecutiveId{ get; set; }
            public  string ExecutiveName { get; set; }
        }
       
    }
}



