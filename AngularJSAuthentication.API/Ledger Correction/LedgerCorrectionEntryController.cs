using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Routing;
using AngularJSAuthentication.Model.LegderCorrection;

namespace AngularJSAuthentication.API.Ledger_Correction
{
    [RoutePrefix("api/LedgerCorrection")]
    public class LedgerCorrectionEntryController : ApiController
    {
        [Route("AddLegerEntry")]
        [HttpPost]
        public bool LedgerCorrectionEntry(List<LedgerData> LedgerData)
        {
            // List<LedgerData> Ld = new List<LedgerData>();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                    foreach (var item in LedgerData)
                    {
                        if (item.ActionType == "Sales Exist But Receipt Not Exist" || item.ActionType== "Sales Exist But Receipt Not Exist")
                        {
                            item.SkCode = item.orderId + '~' + item.SkCode;
                        }
                        else
                        {
                            item.SkCode = item.SkCode;
                        }

                        LedgerCorrection DbLC = new LedgerCorrection();
                        DbLC.ActionType = item.ActionType;
                        DbLC.SkCode = item.SkCode;
                        DbLC.Status = "Pending";
                        DbLC.CreatedDate = DateTime.Now;
                        DbLC.CreatedBy = userid;
                        DbLC.IsActive = true;
                        DbLC.IsDeleted = false;
                        if (item.Status == "")
                            context.LedgerCorrections.Add(DbLC);
                    }
                    context.Commit();
                    return true;


                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet]
        [Route("GetLegerEntry")]
        public List<ResponseDataList> GetLedgerData()
        {
            using (AuthContext context = new AuthContext())
            {

                List<ResponseDataList> Ledger = new List<ResponseDataList>();
                Ledger = context.Database.SqlQuery<ResponseDataList>("EXEC GetLedgerCorrectionData").ToList();

                return Ledger;
            }
        }
        [HttpGet]
        [Route("GetHistroyLegerEntry")]
        public List<ResponseDataList> GetHistroyLegerEntry()
        {
            using (AuthContext context = new AuthContext())
            {
                List<ResponseDataList> Ledger = new List<ResponseDataList>();
                context.Database.CommandTimeout = 900;
                Ledger = context.Database.SqlQuery<ResponseDataList>("EXEC GetLedgerHistroyData").ToList();
                return Ledger;
            }

        }

        [Route("RemoveLedgerItem")]
        [HttpDelete]
        [AllowAnonymous]
        public bool RemoveSkuItems(int id)
        {

            using (var context = new AuthContext())
            {
                if (id != 0)
                {
                    var data = context.LedgerCorrections.FirstOrDefault(x => x.Id == id && x.IsActive == true && x.IsDeleted == false);
                    if (data != null)
                    {
                        data.IsActive = false;
                        data.IsDeleted = true;
                        data.ModifiedDate = DateTime.Now;
                        context.Commit();
                        return true;
                    }
                    else
                        return false;
                }
            }
            return true;
        }
        [HttpGet]
        [Route("ProcesJob")]
        public string GetIncorrectLedgerEnteryJob()
        {
            using (AuthContext context = new AuthContext())
            {
                context.Database.CommandTimeout = 900;
                var data = context.Database.SqlQuery<string>("EXEC ExecIncorrectLedgerEntery").FirstOrDefault();
                return data;
            }
        }

        [HttpGet]
        [Route("GetLedgerIsssueTypeList")]
        public List<LedgerIsssueTypeDC> GetLedgerIsssueTypeList()
        {
            using (AuthContext context = new AuthContext())
            {
                List<LedgerIsssueTypeDC> LedgerIssueTypeList = new List<LedgerIsssueTypeDC>();
                string query = "select * from vwLedgerIsssueType";
                LedgerIssueTypeList = context.Database.SqlQuery<LedgerIsssueTypeDC>(query).ToList();
                return LedgerIssueTypeList;
            }
        }

        public class LedgerData
        {
            public string ActionType { get; set; }
            public string SkCode { get; set; }
            public string orderId { get; set; }
            public string Status { get; set; }

        }
        public class ResponseDataList
        {
            public string ActionType { get; set; }
            public string SkCode { get; set; }
            public string Status { get; set; }
            public long Id { get; set; }
            public int orderId { get; set; }
            public DateTime CreatedDate { get; set; }
        }
        public class LedgerIsssueTypeDC
        {
            public string LedgerIsssueTypeLabel { get; set; }
            public string Ledgervalue { get; set; }
        }


    }
}








