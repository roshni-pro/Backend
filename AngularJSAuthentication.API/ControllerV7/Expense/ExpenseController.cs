using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.BusinessLayer.Managers.Transactions.Expense;
using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.Transaction;
using AngularJSAuthentication.Model.Account;
using AngularJSAuthentication.Model.Expense;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;


namespace AngularJSAuthentication.API.ControllerV7.Expense
{
    [RoutePrefix("api/Expense")]
    public class ExpenseController : ApiController
    {

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [HttpPost]
        [Route("AddExpenseTDS")]
        public async Task<ExpenseTDSMaster> AddTDS(ExpenseTDSMaster expenseTDSMaster)
        {
            var manager = new ExpenseManager();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            logger.Info("User ID : {0} , userid");
            expenseTDSMaster.CreatedBy = userid;

            if (expenseTDSMaster.Id == 0)
            {
                return await manager.InsertExpenseAsync(expenseTDSMaster);
            }
            else
            {
                return await manager.UpdateExpenseAsync(expenseTDSMaster);
            }
        }
        [HttpPost]
        [Route("AddExpenseGST")]
        public async Task<ExpenseGSTMaster> AddGST(ExpenseGSTMaster expenseGSTMaster)
        {
            var manager = new ExpenseManager();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            logger.Info("User ID : {0} , userid");
            expenseGSTMaster.CreatedBy = userid;

            if (expenseGSTMaster.Id == 0)
            {
                return await manager.InsertExpenseGSTAsync(expenseGSTMaster);
            }
            else
            {
                return await manager.UpdateExpenseGSTAsync(expenseGSTMaster);
            }
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ExpenseTDSMaster> GetbyId(int Id)
        {
            var manager = new ExpenseManager();
            return await manager.GetExpenseAsync(Id);
        }

        [HttpGet]
        [Route("GetAll")]
        public async Task<List<ExpenseTDSDc>> GetAllExpenseAsync()
        {
            ExpenseHelper helper = new ExpenseHelper();
            ExpenseManager manager = new ExpenseManager();
            return await manager.GetAllExpenseAsync();


        }

        //[AllowAnonymous]
        //[Route("GetAll")]
        //[HttpGet]
        //public dynamic GetAll(long LedgerId)
        //{
        //    using (var odd = new AuthContext())
        //    {
        //        var query = from e in odd.ExpenseTDSMasterDB
        //                    join lt in odd.LadgerTypeDB
        //                    on e.LedgerId equals lt.ID
        //                    join l in odd.LadgerDB
        //                    on e.LedgerId equals l.ID
        //                    where e.Id == LedgerId && e.IsActive == true && e.IsDeleted != true
        //                    select new TDSDc
        //                    {
        //                        ID = e.LedgerId,
        //                        SectionCode = e.SectionCode,
        //                        RateOfTDS = e.RateOfTDS,
        //                        Assessee = e.Assessee,

        //                    };
        //        TDSDc expense = query.First();
        //        return expense;
        //    }


        //}

        //public TDSDc GetAll(long ID)
        //{
        //    using (var authContext = new AuthContext())
        //    {
        //    var query = from e in authContext.ExpenseTDSMasterDB
        //                join lt in authContext.LadgerTypeDB
        //                on e.LedgerId equals lt.ID
        //                join l in authContext.LadgerDB
        //                on e.LedgerId equals l.ID
        //                where e.Id == ID && e.IsActive == true && e.IsDeleted != true
        //                select new TDSDc
        //                {
        //                    ID = e.LedgerId,
        //                    SectionCode = e.SectionCode,
        //                    RateOfTDS = e.RateOfTDS,
        //                    Assessee = e.Assessee,

        //                };
        //    TDSDc expense = query.First();
        //    return expense;
        //}
        //}



        [HttpGet]
        [Route("GetDetailsList")]
        public List<ExpenseDetailDc> GetExpenseTDSList(int expenseId)
        {
            ExpenseHelper helper = new ExpenseHelper();
            List<ExpenseDetailDc> expensedetailsList = helper.GetExpenseDetailsList(expenseId);
            return expensedetailsList;
        }

        [HttpGet]
        [Route("GetGSTbyId")]
        public async Task<ExpenseGSTMaster> GetGSTbyId(int Id)
        {

            var manager = new ExpenseManager();
            return await manager.GetExpenseGSTAsync(Id);

        }

        [HttpGet]
        [Route("GetExpenseList")]
        public List<ExpenseListDC> GetExpenseList()
        {
            ExpenseHelper helper = new ExpenseHelper();
            List<ExpenseListDC> list = helper.GetExpenseList();
            return list;
        }


        [HttpGet]
        [Route("GetAllGST")]
        public async Task<List<ExpenseGSTMaster>> GetAllExpenseGSTAsync()

        {
            ExpenseManager manager = new ExpenseManager();
            return await manager.GetAllExpenseGSTAsync();


        }


        [HttpGet]
        [Route("GetById/{expenseId}")]
        public ExpenseDc GetById(int expenseId)
        {
            ExpenseHelper helper = new ExpenseHelper();
            ExpenseDc expense = helper.GetById(expenseId);
            return expense;
        }

        [HttpPost]
        [Route("AddExpense")]
        public ExpenseDc AddExpense(ExpenseDc expenseDc)
        {
            expenseDc.CreatedBy = GetUserId();
            ExpenseHelper helper = new ExpenseHelper();
            ExpenseDc expense = new ExpenseDc();
            if (expenseDc.Id == 0)
            {
                expense = helper.AddExpense(expenseDc);
            }
            else {
                expense = helper.UpdateExpense(expenseDc);
            }
            return expense;
        }
        [HttpPost]
        [Route("AddExpenseDetails")]
        public ExpenseDetailDc AddExpenseDetails(ExpenseDetailDc expenseDetailDc)
        {
            expenseDetailDc.CreatedBy = GetUserId();
            ExpenseHelper helper = new ExpenseHelper();
            ExpenseDetailDc expensedetails = helper.AddExpenseDetails(expenseDetailDc);
            return expensedetails;
        }
        [HttpGet]
        [Route("GetDetailsList")]
        public List<ExpenseDetailDc> GetExpenseDetailsList(int expenseId)
        {
            ExpenseHelper helper = new ExpenseHelper();
            List<ExpenseDetailDc> expensedetailsList = helper.GetExpenseDetailsList(expenseId);
            return expensedetailsList;
        }
        private int GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }

        [HttpPut]
        [Route("DeleteExpense")]
        public async Task<ExpenseTDSMaster> DeleteExpense(int Id)
        {
            ExpenseManager manager = new ExpenseManager();
            ExpenseTDSMaster expenseTDSMasterta = await manager.GetExpenseAsync(Id);
            expenseTDSMasterta.IsActive = false;
            expenseTDSMasterta.IsDeleted = true;
            return await manager.UpdateExpenseAsync(expenseTDSMasterta);


        }

        [HttpPut]
        [Route("DeleteExpenseGST")]
        public async Task<ExpenseGSTMaster> DeleteExpenseGST(int Id)
        {
            ExpenseManager manager = new ExpenseManager();
            ExpenseGSTMaster expenseGSTMasterdata = await manager.GetExpenseGSTAsync(Id);
            expenseGSTMasterdata.IsActive = false;
            expenseGSTMasterdata.IsDeleted = true;
            return await manager.UpdateExpenseGSTAsync(expenseGSTMasterdata);


        }
        [HttpDelete]
        [Route("DeleteDetails")]
        public bool DeleteExpenseDetails(int Id)
        {
            ExpenseHelper helper = new ExpenseHelper();
            bool expense = helper.DeleteExpense(Id);
            return expense;
        }
        [HttpPut]
        [Route("UpdateExpenseDetails")]
        public ExpenseDetailDc UpdateExpenseDetails(ExpenseDetailDc expenseDetailDc)
        {
            expenseDetailDc.CreatedBy = GetUserId();
            ExpenseHelper helper = new ExpenseHelper();
            ExpenseDetailDc expensedetails = helper.UpdateExpenseDetails(expenseDetailDc);
            return expensedetails;
        }

        [AllowAnonymous]
        [Route("GetTDS")]
        [HttpGet]
        public dynamic GetTDS()
        {
            using (var odd = new AuthContext())
            {
                var TDSList = new List<ExpenseTDSDc>();

                string Query = "select ID, LadgertypeID,Name as LadgerName from Ladgers where ObjectType = 'TDS'";

                var _result = odd.Database.SqlQuery<ExpenseTDSDc>(Query).ToList();

                return _result;



            }
        }
        //      [HttpGet]
        //[Route("GetExpenseList")]
        //public List<ExpenseListDC> GetExpenseList()
        //{
        //    ExpenseHelper helper = new ExpenseHelper();
        //    List<ExpenseListDC> list = helper.GetExpenseList();
        //    return list;
        //}

        [HttpGet]
        [Route("GetExpenseListData")]
        public List<ExpenseDc> GetExpenseListData()
        {
            ExpenseHelper helper = new ExpenseHelper();
            List<ExpenseDc> expenseList = helper.GetExpenseListData();
            return expenseList;
        }

        [HttpDelete]
        [Route("DeleteExpense")]
        public bool DeleteExpenseById(int Id)
        {
            ExpenseHelper helper = new ExpenseHelper();
            bool expense = helper.DeleteExpenseById(Id);
            return expense;
        }
        [HttpGet]
        [Route("GetCheckerList")]
        public List<ExpenseChecker> GetCheckList(string name) 
        {
            using (var authContext = new AuthContext())
            {
                List<ExpenseChecker> checkerList = authContext.Peoples.Where(x => x.Deleted == false && x.Active == true && x.DisplayName.ToLower().Contains(name.ToLower())).Select(x=>new ExpenseChecker{ID=x.PeopleID,Name=x.DisplayName}).Take(50).ToList();
           
                return checkerList;
            }
        }    
    }


    }

