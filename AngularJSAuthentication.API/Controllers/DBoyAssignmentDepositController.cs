
using AgileObjects.AgileMapper;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.Model;
using OpenHtmlToPdf;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/DBoyAssignmentDeposit")]
    public class DBoyAssignmentDepositController : BaseApiController
    {

        [Route("getCurrencyCollectionSettlementData")]
        [HttpGet]
        public dynamic getCurrencyCollectionSettlementData(int DboyId, long AssignmentId)
        {
            using (var context = new AuthContext())
            {

                //if (AssignmentId == 0) {
                string Query = " select distinct c.Deliveryissueid,c.TotalDeliveryissueAmt, c.Status,c.TotalCashAmt,c.TotalCheckAmt,c.TotalOnlineAmt,c.TotalDueAmt,c.CreatedDate " +
                              " from CurrencyCollections c" +
                              " Where c.Status = 'Settlement' and c.IsActive = 1 and c.IsDeleted = 0 and c.CreatedDate >= '2019-11-01' and" +
                              " NOT EXISTS(SELECT * FROM  DBoyAssignmentDeposits db WHERE  c.Deliveryissueid = db.Deliveryissueid ) and  c.DBoyPeopleId =" + DboyId + (AssignmentId == 0 ? "" : "and  c.Deliveryissueid =" + AssignmentId);


                var result = context.Database.SqlQuery<AgentCurrencyCollectionDc>(Query).ToList();
                return result;
                //}
                //else
                //{
                //    string Query = " select distinct  c.Deliveryissueid,c.TotalDeliveryissueAmt, c.Status,c.TotalCashAmt,c.TotalCheckAmt,c.TotalOnlineAmt,c.TotalDueAmt,c.CreatedDate " +
                //                " from CurrencyCollections c" +
                //                 " inner join DBoyAssignmentDepositMasters d on c.DBoyPeopleId = d.DBoyId" +
                //                  " and c.Status = 'Settlement' and c.IsActive = 1 and c.IsDeleted = 0 and c.CreatedDate >= '2019-11-01' and" +
                //                  " NOT EXISTS(SELECT * FROM  DBoyAssignmentDeposits db WHERE  c.Deliveryissueid = db.Deliveryissueid) and  c.DBoyPeopleId =" + DboyId +
                //                  " and  c.Deliveryissueid =" + AssignmentId;

                //    var result = context.Database.SqlQuery<AgentCurrencyCollectionDc>(Query).ToList();
                //    return result;
                //}
            }
        }


        [Route("getSignedDepositSlip")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<DboySignedAssignment>> getSignedDepositSlip(long SlipId, long AssignmentId, long DboyId, DateTime? StartDate, DateTime? EndDate, int PageNumber, int PageSize, string Type = "SignOff")
        {
            List<DboySignedAssignment> dboySignedAssignments = new List<DboySignedAssignment>();
            using (var context = new AuthContext())
            {
                if (Type == "SignOff")
                {
                    dboySignedAssignments = context.Database.SqlQuery<DboySignedAssignment>("exec GetDboyDepositeSlip " + SlipId + ", " + AssignmentId + "," + DboyId + "," + (StartDate.HasValue ? "'" + StartDate.Value.ToString() + "'" : "null") + "," + (EndDate.HasValue ? "'" + EndDate.Value.ToString() + "'" : "null") + "," + PageNumber + "," + PageSize).ToList();
                }
                else
                {
                    dboySignedAssignments = context.Database.SqlQuery<DboySignedAssignment>("exec GetDboyDepositeUnsignoffSlip " + SlipId + ", " + AssignmentId + "," + DboyId + "," + (StartDate.HasValue ? "'" + StartDate.Value.ToString() + "'" : "null") + "," + (EndDate.HasValue ? "'" + EndDate.Value.ToString() + "'" : "null") + "," + PageNumber + "," + PageSize).ToList();
                }
            }

            return dboySignedAssignments;
        }
        /// <summary>
        /// Create From  AG7 Page
        /// </summary>
        /// <param name="AgentDBoyList"></param>
        /// <returns></returns>
        [Route("saveAssignmentDeposits")]
        [HttpPost]
        public async Task<bool> saveAssignmentDeposits(List<long> assignmentIds , long dueamount)
        {
            bool result = false;
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var db = new AuthContext())
            {
                if (assignmentIds != null && assignmentIds.Count > 0)
                {
                    var Assignmentids = assignmentIds;
                    var AnyExist = db.DboyAssignmentDepositDB.Any(z => Assignmentids.Contains(z.Deliveryissueid));
                    var FirstId = Assignmentids.First();
                    var DBoyID = db.DeliveryIssuanceDb.FirstOrDefault(z => z.DeliveryIssuanceId == FirstId).PeopleID;
                    if (!AnyExist && DBoyID > 0)
                    {

                        DBoyAssignmentDepositMaster DBoyAssignmentDepositMaster = new DBoyAssignmentDepositMaster();
                        DBoyAssignmentDepositMaster.AgentDue = dueamount;
                        DBoyAssignmentDepositMaster.DBoyId = DBoyID;
                        DBoyAssignmentDepositMaster.IsActive = true;
                        DBoyAssignmentDepositMaster.IsDeleted = false;
                        DBoyAssignmentDepositMaster.CreatedDate = DateTime.Now;
                        DBoyAssignmentDepositMaster.CreatedBy = userid;
                        DBoyAssignmentDepositMaster.DBoyAssignmentDeposits = new List<DBoyAssignmentDeposit>();
                        foreach (var item in assignmentIds)
                        {
                            DBoyAssignmentDepositMaster.DBoyAssignmentDeposits.Add(new DBoyAssignmentDeposit
                            {
                                Deliveryissueid = item,
                                Comment = string.Empty
                            });
                        }                        
                        db.DBoyAssignmentDepositMasterDB.Add(DBoyAssignmentDepositMaster);
                        db.Commit();
                        DBoyAssignmentDepositMaster.IsUNSignOffUrl = await GetAssignmentPDF(DBoyAssignmentDepositMaster.DBoyAssignmentDeposits.ToList(), DBoyAssignmentDepositMaster.AgentDue, DBoyAssignmentDepositMaster.CreatedBy, DBoyAssignmentDepositMaster.Id.ToString(), "");
                        result = db.Commit() > 0;
                    }
                }
                return result;
            }
        }



        #region Provide  Assignment Data
        /// <summary>
        ///  Retrive Data in DBoy App after Currency Sattlement 
        /// </summary>
        /// <param name="BoyId"></param>
        /// <returns></returns>
        [Route("GetDBoySattlementAssignment")]
        [HttpGet]
        public async Task<List<DBoyAssignmentDepositMasterDC>> GetDBoySattlementAssignment(long BoyId)
        {
            var DBoyAssignmentDepositMasterDCList = new List<DBoyAssignmentDepositMasterDC>();
            if (BoyId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    var DBoyAssignmentDepositMasterList = new List<DBoyAssignmentDepositMaster>();
                    var DBoyAssignmentDepositList = context.DBoyAssignmentDepositMasterDB.Where(x => x.DBoyId == BoyId && x.IsDeleted == false && x.Signature == null).Include(x => x.DBoyAssignmentDeposits).ToList();
                    DBoyAssignmentDepositMasterDCList = Mapper.Map(DBoyAssignmentDepositList).ToANew<List<DBoyAssignmentDepositMasterDC>>();
                }
            }
            return DBoyAssignmentDepositMasterDCList;
        }
        #endregion


        [Route("GetAssignmentPDF")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<string> GetAssignmentPDF(List<DBoyAssignmentDeposit> dboyAssignmentDepositDBs,double AgentDue,int createdBy, string SlipNo, string signature)
        {
            string returnfile = "";

            using (var context = new AuthContext())
            {

                var assignmentIds = dboyAssignmentDepositDBs.Select(x => x.Deliveryissueid).ToList();

                string Query = "select ass.WarehouseId,ass.DeliveryIssuanceId ,cc.Id currencyCollectionId,cc.TotalDeliveryissueAmt,ass.DisplayName dboyName,agent.displayname AgentName,cc.TotalCashAmt,cc.TotalCheckAmt,ass.CreatedDate AssignmentDate," +
                    "cc.TotalDueAmt,cc.TotalOnlineAmt from   DeliveryIssuances ass inner join CurrencyCollections CC on ass.DeliveryIssuanceId = CC.Deliveryissueid And cc.isactive=1 and cc.isdeleted=0" +
                       "Left join People agent on ass.AgentId=agent.PeopleID where cc.Status='Settlement' and ass.DeliveryIssuanceId in(" + string.Join(",", assignmentIds) + ")";

                var DboyAssignments = context.Database.SqlQuery<DboySlipDc>(Query).ToList();

                if (DboyAssignments != null && DboyAssignments.Any())
                {
                    Query = "Select WarehouseName + '-' + CityName Warehouse from Warehouses where WarehouseId=" + DboyAssignments.FirstOrDefault().WarehouseId;

                    var WarehouseName = context.Database.SqlQuery<string>(Query).FirstOrDefault();

                    List<long> collectionId = DboyAssignments.Select(x => x.CurrencyCollectionId).ToList();
                    Query = "Select b.Title,sum(a.CurrencyCountByDBoy) Qty,sum(a.CurrencyCountByDBoy * b.Value) Amount from CashCollections a inner join CurrencyDenominations b on a.CurrencyDenominationId = b.Id where CurrencyCollectionId in (" + string.Join(",", collectionId) + " ) and IsActive = 1 and IsDeleted = 0  group by b.Title,b.Id,b.currencyType order by b.currencyType desc,b.Id ";

                    var CashCollection = context.Database.SqlQuery<DenominationDc>(Query).ToList();

                    Query = "Select orderid,ChequeDate,ChequeNumber,ChequeAmt from ChequeCollections where CurrencyCollectionId in (" + string.Join(",", collectionId) + " ) and IsActive=1 and IsDeleted=0";

                    var ChequeCollection = context.Database.SqlQuery<ChequecollectionDc>(Query).ToList();


                    Query = "Select orderid,MPOSReferenceNo tranctionno, MPOSAmt amount,PaymentFrom,CreatedDate  from OnlineCollections where CurrencyCollectionId in (" + string.Join(",", collectionId) + ") and MPOSAmt> 0 and IsActive = 1 and IsDeleted = 0 union all Select orderid, PaymentReferenceNO, PaymentGetwayAmt, PaymentFrom ,CreatedDate from OnlineCollections where CurrencyCollectionId in (" + string.Join(",", collectionId) + ") and PaymentGetwayAmt> 0 and IsActive = 1 and IsDeleted = 0 ";

                    var onlinecollections = context.Database.SqlQuery<onlinecollectionDc>(Query).ToList();

                    //int orderids = result.FirstOrDefault().orderids;
                    string dboyName = DboyAssignments.FirstOrDefault().dboyName;
                    string AgentName = DboyAssignments.FirstOrDefault().AgentName;
                    decimal TotalDeliveryissueAmt = DboyAssignments.Sum(x => x.TotalDeliveryissueAmt);
                    decimal TotalCashAmt = DboyAssignments.Sum(x => x.TotalCashAmt);
                    decimal TotalOnlineAmt = DboyAssignments.Sum(x => x.TotalOnlineAmt);
                    decimal TotalCheckAmt = DboyAssignments.Sum(x => x.TotalCheckAmt);
                    decimal TotalDueAmt = DboyAssignments.Sum(x => x.TotalDueAmt);


                    string pathToHTMLFile = HttpContext.Current.Server.MapPath("~/Templates") + "/assingment-deposit-sheet.html";
                    string content = File.ReadAllText(pathToHTMLFile);

                    Query = "SELECT DI.DeliveryIssuanceId,STRING_AGG(ODm.OrderId,',') OrderId FROM DeliveryIssuances DI with(nolock) inner JOIN OrderDispatchedMasters  ODM with(nolock) ON DI.DeliveryIssuanceId = ODM.DeliveryIssuanceIdOrderDeliveryMaster and ODM.CanceledStatus in('Delivery Canceled') and  DI.DeliveryIssuanceId in(" + string.Join(",", assignmentIds) + ") group by DI.DeliveryIssuanceId";

                    var AssignmentCancelDcs = context.Database.SqlQuery<AssignmentCancelDc>(Query).ToList();


                    if (!string.IsNullOrEmpty(content))
                    {

                        content = content.Replace("@WarehouseName", WarehouseName);
                        content = content.Replace("@SlipNo", SlipNo);
                        content = content.Replace("@NowDate", DateTime.Now.ToString("dd/MM/yyyy"));
                        content = content.Replace("@AgentNAME", AgentName);
                        content = content.Replace("@NameOfDelivery", dboyName);

                        int i = 1;
                        string htmlstring = string.Empty;
                        foreach (var Assignment in DboyAssignments)
                        {
                            string cancelOrderId = AssignmentCancelDcs != null && AssignmentCancelDcs.Any(x => x.DeliveryIssuanceId == Assignment.DeliveryIssuanceId) ? AssignmentCancelDcs.FirstOrDefault(x => x.DeliveryIssuanceId == Assignment.DeliveryIssuanceId).OrderId : "";
                            htmlstring += "<tr> <td style='padding: 5px'>Total assignment Delivered Amount : " + i + "</td>";
                            htmlstring += "<td style= 'padding: 5px;' > " + Assignment.DeliveryIssuanceId + "</td>";
                            htmlstring += "<td style= 'padding: 5px;' > " + Assignment.AssignmentDate.ToString("dd/MM/yyyy") + "</td>";
                            htmlstring += "<td style= 'padding: 5px;' > Rs." + Math.Round(Assignment.TotalDeliveryissueAmt, 2) + "</td>";
                            htmlstring += "<td style='padding: 5px; '>Cancelled Bill No: </td>";
                            htmlstring += "<td style='padding: 5px;' > "+ cancelOrderId + "</td> </tr>";
                            i++;
                        }
                        content = content.Replace("@AssingmentDetail", htmlstring);                        
                        htmlstring = "";
                        foreach (var cash in CashCollection)
                        {
                            htmlstring += "<tr> <td style='padding: 5px'>" + cash.Title + "</td>";
                            htmlstring += "<td style= 'padding: 5px;' > " + cash.QTY + "</td>";
                            htmlstring += "<td style= 'padding: 5px;' > Rs." + Math.Round(Convert.ToDecimal(cash.Amount), 2) + "</td>";
                        }
                        content = content.Replace("@CashDetail", htmlstring);
                        content = content.Replace("@TotalCashAmount", CashCollection.Sum(cash => cash.Amount).ToString());

                        htmlstring = "";
                        if (ChequeCollection != null && ChequeCollection.Any())
                        {
                            foreach (var cheque in ChequeCollection)
                            {
                                htmlstring += "<tr> <td style='padding: 5px'>" + cheque.Orderid + "</td>";
                                htmlstring += "<td style= 'padding: 5px;' > " + cheque.ChequeDate.ToString("dd/MM/yyyy") + "</td>";
                                htmlstring += "<td style='padding: 5px'>" + cheque.ChequeNumber + "</td>";
                                htmlstring += "<td style= 'padding: 5px;' > Rs." + Math.Round(cheque.ChequeAmt, 2) + "</td>";
                            }
                        }
                        else
                        {
                            htmlstring = "<tr> <td style='padding: 5px' colspan='4'>No Cheque Detail Available</td></tr>";
                        }
                        content = content.Replace("@ChequeDetail", htmlstring);
                        content = content.Replace("@TotalChequeAmount", Math.Round(ChequeCollection.Sum(c => c.ChequeAmt), 2).ToString());

                        htmlstring = "";
                        if (onlinecollections != null && onlinecollections.Any())
                        {
                            foreach (var online in onlinecollections)
                            {
                                htmlstring += "<tr> <td style='padding: 5px'>" + online.orderid + "</td>";
                                htmlstring += "<td style= 'padding: 5px;' > " + online.CreatedDate.ToString("dd/MM/yyyy") + "</td>";
                                htmlstring += "<td style='padding: 5px'>" + online.tranctionno + "</td>";
                                htmlstring += "<td style= 'padding: 5px;' > Rs." + Math.Round(online.amount, 2) + "</td>";
                            }
                        }
                        else
                        {
                            htmlstring = "<tr> <td style='padding: 5px' colspan='4'>No Online Detail Available</td></tr>";
                        }
                        content = content.Replace("@OnlineDetail", htmlstring);
                        content = content.Replace("@TotalOnlineAMOUNT", Math.Round(onlinecollections.Sum(c => c.amount), 2).ToString());

                        content = content.Replace("@TotalAllCashAmount", Math.Round(TotalCashAmt, 2).ToString());
                        content = content.Replace("@TotalAllChequeAmount", Math.Round(TotalCheckAmt, 2).ToString());
                        content = content.Replace("@TotalAllOnlineAMOUNT", Math.Round(TotalOnlineAmt, 2).ToString());
                        content = content.Replace("@ReceivedTotal", Math.Round(TotalCashAmt + TotalCheckAmt + TotalOnlineAmt, 2).ToString());
                        // content = content.Replace("@TotalDueAmount", Math.Round(TotalDueAmt, 2).ToString());
                        content = content.Replace("@TotalDueAmount", Math.Round(AgentDue, 2).ToString());
                        //DboyAssignments.Sum(x => x.TotalDueAmt)

                        if (DboyAssignments.Count() >= 1)
                            content = content.Replace("@AssignmentDueAmount1", Math.Round(DboyAssignments[0].TotalDeliveryissueAmt, 2).ToString());

                        if (DboyAssignments.Count() >= 2)
                            content = content.Replace("@AssignmentDueAmount2", Math.Round(DboyAssignments[1].TotalDeliveryissueAmt, 2).ToString());

                        if (DboyAssignments.Count() >= 3)
                            content = content.Replace("@AssignmentDueAmount3", Math.Round(DboyAssignments[2].TotalDeliveryissueAmt, 2).ToString());

                        if (DboyAssignments.Count() == 4)
                            content = content.Replace("@AssignmentDueAmount4", Math.Round(DboyAssignments[3].TotalDeliveryissueAmt, 2).ToString());

                        content = content.Replace("@AssignmentDueAmount1", "0");
                        content = content.Replace("@AssignmentDueAmount2", "0");
                        content = content.Replace("@AssignmentDueAmount3", "0");
                        content = content.Replace("@AssignmentDueAmount4", "0");








                        content = content.Replace("@AssignmentTotal", Math.Round(DboyAssignments.Sum(x => x.TotalDeliveryissueAmt), 2).ToString());


                        content = content.Replace("@TotalDueTillDate", Math.Round(DboyAssignments.Sum(x => x.TotalDeliveryissueAmt) - (TotalCashAmt + TotalCheckAmt + TotalOnlineAmt), 2).ToString());



                        int z = 1;
                        string htmlstringcomm = string.Empty;
                        foreach (var Assignmentcomment in dboyAssignmentDepositDBs)
                        {
                            htmlstringcomm += "<tr> <td style='padding: 5px'>Remarks : " + z + "</td>";
                            htmlstringcomm += "<td style= 'padding: 5px;' > " + Assignmentcomment.Comment + "</td>";


                            z++;
                        }
                        content = content.Replace("@Comments", htmlstringcomm);
                        //if (dboyAssignmentDepositDBs.Count() == 1)
                        //    content = content.Replace("@remark1", dboyAssignmentDepositDBs[0].Comment);
                        //if (dboyAssignmentDepositDBs.Count() == 2)
                        //    content = content.Replace("@remark2", dboyAssignmentDepositDBs[1].Comment);
                        //if (dboyAssignmentDepositDBs.Count() == 3)
                        //    content = content.Replace("@remark3", dboyAssignmentDepositDBs[2].Comment);
                        //if (dboyAssignmentDepositDBs.Count() == 4)
                        //    content = content.Replace("@remark4", dboyAssignmentDepositDBs[3].Comment);
                        //if (dboyAssignmentDepositDBs.Count() == 4)
                        //    content = content.Replace("@remark5", dboyAssignmentDepositDBs[4].Comment);


                        var signaturepath = string.IsNullOrEmpty(signature) ? "" : string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                              , HttpContext.Current.Request.Url.DnsSafeHost
                                                              , HttpContext.Current.Request.Url.Port
                                                              , signature);

                        //content = content.Replace("@remark1", "0");
                        //content = content.Replace("@remark2", "0");
                        //content = content.Replace("@remark3", "0");
                        //content = content.Replace("@remark4", "0");
                        //content = content.Replace("@remark5", "0");

                        Query = "select STRING_AGG(p.DisplayName,',') DisplayName,r.name RoleName from People p inner join AspNetUsers u  on  p.Email=u.Email and p.WarehouseId=" + DboyAssignments.FirstOrDefault().WarehouseId + " inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id and r.name in ('Hub Cashier','Hub Service Lead') Group by r.name";

                        var DepositSlipPerson = context.Database.SqlQuery<DepositSlipPerson>(Query).ToList();
                        string HubLeadName = DepositSlipPerson.Any(x => x.RoleName == "Hub Service Lead") ? DepositSlipPerson.FirstOrDefault(x => x.RoleName == "Hub Service Lead").DisplayName : "";
                        string CashierName = DepositSlipPerson.Any(x => x.RoleName == "Hub Cashier") ? DepositSlipPerson.FirstOrDefault(x => x.RoleName == "Hub Cashier").DisplayName : "";

                        Query = "select DisplayName from People where PeopleID=" + createdBy;
                        CashierName = context.Database.SqlQuery<string>(Query).FirstOrDefault();

                        content = content.Replace("@DBoyNAME", dboyName);
                        content = content.Replace("@AgentNAME", AgentName);
                        content = content.Replace("@AssignmentPrepaired", CashierName);

                        content = content.Replace("@HubHeadName", HubLeadName);
                        content = content.Replace("@AssingmentSettledName", "");
                        content = content.Replace("@DBoySigned", string.IsNullOrEmpty(signature) ? "" : " <img width='50px' height='50px' src='" + signaturepath + "' title='" + dboyName + "' />");
                        content = content.Replace("@AgentSigned", "");
                        content = content.Replace("@HubHeadSigned", "");
                        content = content.Replace("@AssingmentSettledSigned", "");

                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/Assignmentcollectionpdf")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/Assignmentcollectionpdf"));

                        string fileName = DateTime.Now.ToString("ddMMyyyyHHmmss") + (string.IsNullOrEmpty(signature) ? "_Assignmentcollection.pdf" : "_Assignmentcollectionsignature.pdf");

                        var OutPutFile = Path.Combine(HttpContext.Current.Server.MapPath("~/Assignmentcollectionpdf"), fileName);

                        byte[] pdf = null;

                        pdf = Pdf
                              .From(content)
                              //.WithGlobalSetting("orientation", "Landscape")
                              //.WithObjectSetting("web.defaultEncoding", "utf-8")
                              .OfSize(PaperSize.A4)
                              .WithTitle("Invoice")
                              .WithoutOutline()
                              .WithMargins(PaperMargins.All(0.0.Millimeters()))
                              .Portrait()
                              .Comressed()
                              .Content();
                        FileStream file = File.Create(OutPutFile);
                        file.Write(pdf, 0, pdf.Length);
                        file.Close();

                        returnfile = "/Assignmentcollectionpdf/" + fileName;
                    }
                }
            }


            return returnfile;
        }


        [Route("UpdateAssignment")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<bool> UpdateAssignment(DBoyAssignmentDepositMasterDC assignmentMasterDc)
        {
            var identity = User.Identity as ClaimsIdentity;
            string username = "";
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "username"))
                username = identity.Claims.FirstOrDefault(x => x.Type == "username").Value;

            using (var db = new AuthContext())
            {
                var assignmentids = assignmentMasterDc.DBoyAssignmentDeposits.Select(x => x.Deliveryissueid).ToList();
                var dBoyAssignmentDepositMasterDB = db.DBoyAssignmentDepositMasterDB.Where(x => x.Id == assignmentMasterDc.Id).Include(x => x.DBoyAssignmentDeposits).FirstOrDefault();
                /*var dboyAssignmentDepositDBs = dBoyAssignmentDepositMasterDB.DBoyAssignmentDeposits.ToList();*/ //db.DboyAssignmentDepositDB.Where(x => assignmentids.Contains(x.Deliveryissueid)).ToList();
                var SlipNo = assignmentMasterDc.Id;
                foreach (var item in dBoyAssignmentDepositMasterDB.DBoyAssignmentDeposits.ToList())
                {
                    var dboyAssignmentDepositDB = assignmentMasterDc.DBoyAssignmentDeposits.FirstOrDefault(x => x.Deliveryissueid == item.Deliveryissueid);
                    item.Comment = dboyAssignmentDepositDB.Comment;
                }
                string signedPDf = await GetAssignmentPDF(dBoyAssignmentDepositMasterDB.DBoyAssignmentDeposits.ToList(), dBoyAssignmentDepositMasterDB  .AgentDue, dBoyAssignmentDepositMasterDB.CreatedBy, SlipNo.ToString(), assignmentMasterDc.Signature);
                dBoyAssignmentDepositMasterDB.ModifiedBy = userid;
                dBoyAssignmentDepositMasterDB.ModifiedDate = DateTime.Now;
                dBoyAssignmentDepositMasterDB.Signature = assignmentMasterDc.Signature;
                dBoyAssignmentDepositMasterDB.SignOffUrl = signedPDf;

                db.Entry(dBoyAssignmentDepositMasterDB).State = EntityState.Modified;
                return db.Commit() > 0;
            }
        }

        [Route("UploadDboySignature")]
        [HttpPost]
        [AllowAnonymous]
        public string UploadDboySignature()
        {
            string LogoUrl = "";

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                if (httpPostedFile != null)
                {
                    if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/Assignmentcollectionpdf")))
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/Assignmentcollectionpdf"));

                    string extension = Path.GetExtension(httpPostedFile.FileName);

                    string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                    LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/Assignmentcollectionpdf"), fileName);

                    httpPostedFile.SaveAs(LogoUrl);

                    AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/Assignmentcollectionpdf", LogoUrl);

                    LogoUrl = "/Assignmentcollectionpdf/" + fileName;
                }
            }

            return LogoUrl;
        }

        #region Provide  DBoyDelivered Data
        [Route("GetDBoyDeliveredData")]
        [HttpGet]
        public async Task<List<DBoyDeliveredDetailDc>> GetDBoyDeliveredData(int DboyId)
        {
            List<DBoyDeliveredDetailDc> DBoyDeliveredDetails = new List<DBoyDeliveredDetailDc>();
            if (DboyId > 0)
            {
                using (AuthContext context = new AuthContext())
                {                    
                    var DboyIdParam = new SqlParameter("@DboyId", DboyId);
                    DBoyDeliveredDetails = context.Database.SqlQuery<DBoyDeliveredDetailDc>("GetDBoyDeliveredDetails @DboyId", DboyIdParam).ToList();
                }
            }
            return DBoyDeliveredDetails;
        }
        #endregion





        public class DBoyAssignmentDepositMasterDC
        {
            public long Id { get; set; }
            public long DBoyId { get; set; }
            public string IsUNSignOffUrl { get; set; }
            public string SignOffUrl { get; set; }
            public string Signature { get; set; }
            public DateTime CreatedDate { get; set; }
            public List<DBoyAssignmentDepositDc> DBoyAssignmentDeposits { get; set; }
        }
        public class DBoyAssignmentDepositDc
        {
            public long Id { get; set; }
            public long Deliveryissueid { get; set; }
            public string Comment { get; set; }
        }
        public class DBoyDeliveredDetailDc
        {
            public int OrderId { get; set; }
            //public string Status { get; set; }
            public double TotalAmount { get; set; }
            public double GrossAmount { get; set; }
            public int DBoyId { get; set; } // Update old records 
            public string DboyName { get; set; }
            public string paymentThrough { get; set; }
            public string paymentMode { get; set; }
            public double PaymentAmount { get; set; }
        }
        public class AgentCurrencyCollectionDc
        {
            public int Deliveryissueid { get; set; }
            public decimal TotalCashAmt { get; set; }
            public decimal TotalOnlineAmt { get; set; }
            public decimal TotalCheckAmt { get; set; }
            public decimal TotalDeliveryissueAmt { get; set; }
            public DateTime CreatedDate { get; set; }
            public decimal TotalDueAmt { get; set; }
            public string Status { get; set; }

        }
        public class DboySlipDc
        {
            public long CurrencyCollectionId { get; set; }
            public int DeliveryIssuanceId { get; set; }
            public int WarehouseId { get; set; }
            public decimal TotalCashAmt { get; set; }
            public decimal TotalOnlineAmt { get; set; }
            public decimal TotalCheckAmt { get; set; }
            public decimal TotalDeliveryissueAmt { get; set; }
            public decimal TotalDueAmt { get; set; }
            public string dboyName { get; set; }
            public string AgentName { get; set; }

            public DateTime AssignmentDate { get; set; }

        }
        public class DenominationDc
        {
            public string Title { get; set; }
            public int QTY { get; set; }
            public int Amount { get; set; }

        }
        public class ChequecollectionDc
        {
            public int Orderid { get; set; }
            public DateTime ChequeDate { get; set; }
            public string ChequeNumber { get; set; }
            public decimal ChequeAmt { get; set; }

        }
        public class onlinecollectionDc
        {
            public int orderid { get; set; }
            public string PaymentFrom { get; set; }
            public string tranctionno { get; set; }
            public decimal amount { get; set; }
            public DateTime CreatedDate { get; set; }

        }

        public class DboySignedAssignment
        {
            public long Id { get; set; }
            public long DBoyId { get; set; }
            public string DboyName { get; set; }
            public string IsUNSignOffUrl { get; set; }
            public string SignOffUrl { get; set; }
            public DateTime CreatedDate { get; set; }
            public string AssignmentId { get; set; }
        }

        public class DepositSlipPerson
        {
            public string DisplayName { get; set; }
            public string RoleName { get; set; }
        }
        public class AssignmentCancelDc
        {
            public int DeliveryIssuanceId { get; set; }
            public string OrderId { get; set; }
        }

    }
}
