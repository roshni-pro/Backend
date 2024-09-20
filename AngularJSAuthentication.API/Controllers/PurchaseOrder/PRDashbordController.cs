using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.PurchaseOrder;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.PurchaseOrder
{
    [RoutePrefix("api/PRdashboard")]
    public class PRDashbordController : ApiController
    {

        [Authorize]
        [Route("")]
        [HttpGet]
        public dynamic getAllHubData(int status, int WarehouseId)
        {
            try
            {
                using (var db = new AuthContext())
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                    string sqlquery = "SELECT * FROM PurchaseOrderMasters INNER JOIN PRApprovelsStatus"
                                    + " ON PurchaseOrderMasters.PurchaseOrderId = PRApprovelsStatus.PurchaseOrderId where"
                                    + " PRApprovelsStatus.ApprovalID =" + userid + "  and  PRApprovelsStatus.IsApprove = " + status
                                    + " and PurchaseOrderMasters.WarehouseId = " + WarehouseId + "and PRApprovelsStatus.IsActive = 1 and PurchaseOrderMasters.PRStatus != 2";


                    List<PurchaseOrderMasterDTO> PRSts = db.Database.SqlQuery<PurchaseOrderMasterDTO>(sqlquery).OrderByDescending(a => a.PurchaseOrderId).ToList();
                    return PRSts;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [Authorize]
        [Route("getAllHub")]
        [HttpGet]
        public dynamic getAllHubData()
        {
            try
            {
                using (var db = new AuthContext())
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    string sqlquery = "SELECT * FROM PurchaseOrderMasters INNER JOIN PRApprovelsStatus"
                                    + " ON PurchaseOrderMasters.PurchaseOrderId = PRApprovelsStatus.PurchaseOrderId where"
                                    + " PRApprovelsStatus.ApprovalID =" + userid + "and PRApprovelsStatus.IsActive = 1 and PurchaseOrderMasters.PRStatus != 2";

                    List<PurchaseOrderMasterDTO> PRSts = db.Database.SqlQuery<PurchaseOrderMasterDTO>(sqlquery).OrderByDescending(a => a.PurchaseOrderId).ToList();
                    return PRSts;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        [Authorize]
        [Route("getReviewer")]
        public IEnumerable<PurchaseOrderMaster> GetReviewer(int status, int WarehouseId)
        {

            List<PurchaseOrderMaster> ass = new List<PurchaseOrderMaster>();
            using (var db = new AuthContext())
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

                    ass = db.DPurchaseOrderMaster.Where(a => a.WarehouseId == WarehouseId && a.PRType == 1 && (a.Rewiever1 == userid || a.Rewiever2 == userid || a.Rewiever3 == userid || a.Rewiever4 == userid || a.Rewiever5 == userid)).OrderByDescending(a => a.PurchaseOrderId).ToList();

                    return ass.AsEnumerable().Where(a => a.PRStatus == status).ToList();
                }
                catch (Exception ex)
                {

                    return null;
                }
        }

        /// <summary>
        /// Get All data fro po master
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("getAll")]
        public dynamic getAllHubData(int WarehouseId)
        {
            try
            {
                using (var db = new AuthContext())
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                    string sqlquery = "SELECT * FROM PurchaseOrderMasters INNER JOIN PRApprovelsStatus"
                                    + " ON PurchaseOrderMasters.PurchaseOrderId = PRApprovelsStatus.PurchaseOrderId where"
                                    + " PRApprovelsStatus.ApprovalID =" + userid
                                    + " and PurchaseOrderMasters.WarehouseId = " + WarehouseId + "and PRApprovelsStatus.IsActive = 1";


                    List<PurchaseOrderMasterDTO> PRSts = db.Database.SqlQuery<PurchaseOrderMasterDTO>(sqlquery).OrderByDescending(a => a.PurchaseOrderId).ToList();
                    return PRSts;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Approved PO By Approver people and send to Reviewer if exist.
        /// if PO approved then send PO PDF to Supplier
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Authorize]
        [Route("sendtoReviewer")]
        [HttpPut]
        public PurchaseOrderMaster SendtoReviewer(PurchaseOrderMaster data)
        {
            using (var db = new AuthContext())
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
                    People p = db.Peoples.Where(q => q.PeopleID == userid).FirstOrDefault();
                    PoApproval IsRevHere = db.PoApprovalDB.Where(a => a.Level == data.Level).FirstOrDefault();
                    PurchaseOrderMaster po = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == data.PurchaseOrderId && a.PRStatus == 1).SingleOrDefault();
                    if (po != null)
                    {
                        po.Status = "Approved";
                        po.PRStatus = 3;
                        po.progress = "50";
                        po.ApprovedBy = p.DisplayName;
                        db.Entry(po).State = EntityState.Modified;
                        db.Commit();
                        //  SendMailCreditWalletNotification(po.PurchaseOrderId);
                    }


                    return po;
                    //}
                }
                catch (Exception ex)
                {

                    return null;
                }
        }

        [Authorize]
        [Route("sendtoReviewerNew")]
        [HttpPut]
        public POResult SendtoReviewerNew(PurchaseOrderMaster data)
        {
            POResult pOResult = new POResult();
            List<PRApprovelsStatus> pdList = new List<PRApprovelsStatus>();
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0; string RoleNames = "";
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                    RoleNames = identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value;

                List<string> lstRoleNames = new List<string>();
                if (!string.IsNullOrEmpty(RoleNames))
                    lstRoleNames = RoleNames.Split(',').ToList();

                PurchaseOrderMaster pom = db.DPurchaseOrderMaster.Where(c => c.PurchaseOrderId == data.PurchaseOrderId).Include(x => x.PurchaseOrderDetail).SingleOrDefault();
                People p = db.Peoples.Where(q => q.PeopleID == userid).FirstOrDefault();
                PRApprovelsStatus PR = db.PRApprovelsStatus.Where(x => x.PurchaseOrderID == data.PurchaseOrderId && x.ApprovalID == userid && x.IsApprove == 0 && x.IsActive == true).FirstOrDefault();
                if (PR != null)
                {
                    PR.IsApprove = 1;
                    PR.ModifiedDate = DateTime.Now;
                    PR.Comments = "Approved By :" + p.DisplayName;
                    db.Entry(PR).State = EntityState.Modified;
                    db.Commit();

                    int PurchaseOrderId = data.PurchaseOrderId;
                    var prapprov = db.PRApprovelsStatus.Where(x => x.PurchaseOrderID == PurchaseOrderId && x.IsActive == true).ToList();

                    if (prapprov.All(x => x.IsApprove == 1))
                    {
                        PurchaseOrderMaster poid = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == data.PurchaseOrderId && a.PRStatus == 1).SingleOrDefault();
                        //var itemIds = pom.PurchaseOrderDetail.Select(x => x.ItemId).ToList();
                        //var items = db.itemMasters.Where(z => itemIds.Contains(z.ItemId)).ToList();
                        //var SubsubCategoryids = items.Select(x => x.SubsubCategoryid).Distinct().ToList();
                        //List<BrandBuyer> BDs = db.BrandBuyerDB.Where(x => SubsubCategoryids.Contains(x.BrandId) && x.WarehosueId == pom.WarehouseId).ToList();
                        string buquery = "select distinct br.BuyerId,p.DisplayName from PurchaseOrderMasters a inner join PurchaseOrderDetails c on  c.PurchaseOrderId =" + data.PurchaseOrderId + " inner join ItemMasters i on c.ItemId = i.ItemId left join BrandBuyers br on i.SubsubCategoryid = br.BrandId and i.WarehouseId = br.WarehosueId and br.Active = 1 left join People p on p.PeopleID = br.BuyerId";
                        List<POBuyerBrandCategory> BuyerList = db.Database.SqlQuery<POBuyerBrandCategory>(buquery).ToList();

                        //  int BuyerId = BDs != null && BDs.Any() ? BDs.FirstOrDefault().BuyerId : 2088;   
                        int BuyerId = BuyerList.Any() && poid.BuyerId.Value != 0 ? poid.BuyerId.Value : BuyerList.FirstOrDefault().BuyerId.Value;
                        People Name = db.Peoples.Where(x => x.PeopleID == BuyerId).FirstOrDefault();
                        if (poid != null)
                        {
                            //  poid.Status = "Approved";
                            poid.PRStatus = 3;
                            poid.BuyerId = BuyerId;
                            poid.BuyerName = Name.DisplayName;
                            // poid.ApprovedBy = p.DisplayName;
                            db.Entry(poid).State = EntityState.Modified;
                            if (db.Commit() > 0)
                            {
                                int warehouseid = db.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == data.PurchaseOrderId).Select(x => x.WarehouseId).FirstOrDefault();
                                string query = string.Empty;
                                var itemIds = db.DPurchaseOrderDeatil.Where(x => x.PurchaseOrderId == data.PurchaseOrderId && x.IsDeleted == false && x.CompanyId==compid && x.WarehouseId == warehouseid).Select(x => x.ItemId).Distinct().ToList();
                                var items = db.itemMasters.Where(z => itemIds.Contains(z.ItemId)).ToList();
                                var SubsubCategoryid = items.Select(x => x.SubsubCategoryid).Distinct().ToList();

                                string querys = @"select  distinct s.StoreId from StoreBrands s with (nolock)
                                                                     inner join BrandCategoryMappings b with (nolock) on s.BrandCategoryMappingId=b.BrandCategoryMappingId
                                                                     inner join SubcategoryCategoryMappings sc with (nolock) on b.SubCategoryMappingId=sc.SubCategoryMappingId
                                                                     where b.IsActive=1 and b.Deleted=0 and s.IsActive=1 and s.IsDeleted=0
                                                                     and sc.IsActive=1 and sc.Deleted=0 and b.SubsubCategoryId in (" + string.Join(",", SubsubCategoryid) + ")";
                                List<long> storeIds = db.Database.SqlQuery<long>(querys).ToList();
                                
                                var roleName = "";
                                foreach (var store in storeIds)
                                {
                                    roleName = db.PRApprovalDB.Where(x => x.AmountlmtMin <= poid.ETotalAmount && x.AmountlmtMax >= poid.ETotalAmount && !x.IsDeleted && x.StoreIds.Contains(store.ToString())).Select(x => x.RoleName).FirstOrDefault();
                                }
                                //var roleName = db.PRApprovalDB.Where(x => x.AmountlmtMin <= poid.ETotalAmount && x.AmountlmtMax >= poid.ETotalAmount && !x.IsDeleted).Select(x => x.RoleName).FirstOrDefault();
                                if (!string.IsNullOrEmpty(roleName))
                                {
                                    query = string.Format("select  p.DisplayName,p.PeopleID from People p where exists (select u.Id from AspNetUsers u inner join AspNetUserRoles ur on u.Id=ur.UserId and p.Email=u.Email inner join AspNetRoles r on ur.RoleId=r.Id and r.name in ('{0}')) and p.Active=1",
                                                              roleName);

                                    BuyerMinDc buyerMinDcs = db.Database.SqlQuery<BuyerMinDc>(query).FirstOrDefault();
                                    var check = db.PRPaymentAppoved.Where(x => x.ApprovedBY == buyerMinDcs.PeopleId && x.PRId == poid.PurchaseOrderId).FirstOrDefault();
                                    if (check == null)
                                    {
                                        PRPaymentAppoved pRPaymentAppoved = new PRPaymentAppoved();
                                        pRPaymentAppoved.ApprovedBY = buyerMinDcs.PeopleId;
                                        pRPaymentAppoved.CreatedBy = userid;
                                        pRPaymentAppoved.CreatedDate = DateTime.Now;
                                        pRPaymentAppoved.IsApproved = false;
                                        pRPaymentAppoved.IsActive = true;

                                        pRPaymentAppoved.PRId = poid.PurchaseOrderId;
                                        db.PRPaymentAppoved.Add(pRPaymentAppoved);
                                    }
                                    else
                                    {

                                        check.IsApproved = false;
                                        check.IsActive = true;
                                        check.IsDeleted = false;
                                        check.ModifiedDate = DateTime.Now;
                                        db.Entry(check).State = EntityState.Modified;

                                    }
                                    Sms s = new Sms();
                                    // string msg = " ShopKirana " + Environment.NewLine + " PR id: " + data.PurchaseOrderId + " are waiting for your Payment Approval.";
                                    string msg = "";//"ShopKirana PR id: {#var#} are waiting for your Payment Approval.";
                                    var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "PR_Payment_Waiting_Approval");
                                    msg = dltSMS == null ? "" : dltSMS.Template;                                   
                                    msg = msg.Replace("{#var#}", data.PurchaseOrderId.ToString());

                                    string Mob = db.Peoples.Where(q => q.PeopleID == buyerMinDcs.PeopleId).Select(q => q.Mobile).SingleOrDefault();
                                    if (Mob != null && dltSMS!=null) { s.sendOtp(Mob, msg, dltSMS.DLTId); }
                                    db.Commit();
                                }
                            }
                        }

                    }
                    pOResult.Status = true;
                    pOResult.Message = "Approved Successfully";
                }
                else
                {
                    pOResult.Status = false;
                    pOResult.Message = "Not Approved";
                }


                return pOResult;
            }
        }

        [Authorize]
        [Route("RejectbyApproverNew")]
        [HttpPut]
        public bool RejectbyApproverNew(PurchaseOrderMaster data)
        {
            bool result = false;

            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                People p = db.Peoples.Where(q => q.PeopleID == userid).FirstOrDefault();
                PRApprovelsStatus PR = db.PRApprovelsStatus.Where(x => x.PurchaseOrderID == data.PurchaseOrderId && x.ApprovalID == userid && x.IsApprove == 0 && x.IsActive == true).FirstOrDefault();
                if (PR != null)
                {
                    PR.IsApprove = 2;
                    PR.ModifiedDate = DateTime.Now;
                    PR.Comments = data.CommentApvl;
                    db.Entry(PR).State = EntityState.Modified;
                    db.Commit();


                    return result;
                }
                return result;
            }

        }
        /// <summary>
        /// Approved PO by Reviewer
        /// if PO approved then send PO PDF to Supplier
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Authorize]
        [Route("ApprovedbyReviewer")]
        [HttpPut]
        public PurchaseOrderMaster ApprvedByReviewer(PurchaseOrderMaster data)
        {

            using (var db = new AuthContext())
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

                    People p = db.Peoples.Where(q => q.PeopleID == userid).FirstOrDefault();
                    PoApproval IsRevHere = db.PoApprovalDB.Where(a => a.Level == data.Level).FirstOrDefault();
                    PurchaseOrderMaster po = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == data.PurchaseOrderId).SingleOrDefault();
                    po.Status = "Approved";
                    po.progress = "50";
                    po.ApprovedBy = p.DisplayName;
                    db.Entry(po).State = EntityState.Modified;
                    db.Commit();
                    // SendMailCreditWalletNotification(po.PurchaseOrderId);
                    return po;
                }
                catch (Exception ex)
                {

                    return null;
                }
        }
        #region Function SendMail

        /// <summary>
        /// Reject po by reviewer
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Authorize]
        [Route("cmtbtapvl")]
        [HttpPut]
        public PurchaseOrderMaster CommentbyApprovar(PurchaseOrderMaster data)
        {

            using (var db = new AuthContext())
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


                    PoApproval IsRevHere = db.PoApprovalDB.Where(a => a.Level == data.Level).FirstOrDefault();
                    PurchaseOrderMaster po = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == data.PurchaseOrderId).SingleOrDefault();
                    //po.Status = "Rejected";
                    po.CommentApvl = data.CommentApvl;
                    db.Entry(po).State = EntityState.Modified;
                    db.Commit();
                    return po;
                }
                catch (Exception ex)
                {

                    return null;
                }
        }


        /// <summary>
        /// Comment by reviewer  
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Authorize]
        [Route("RejectbyReviewer")]
        [HttpPut]
        public PurchaseOrderMaster RejectByReviewer(PurchaseOrderMaster data)
        {

            using (var db = new AuthContext())
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

                    PoApproval IsRevHere = db.PoApprovalDB.Where(a => a.Level == data.Level).FirstOrDefault();
                    PurchaseOrderMaster po = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == data.PurchaseOrderId).SingleOrDefault();
                    //po.Status = "Rejected";
                    po.Comment = data.Comment;
                    db.Entry(po).State = EntityState.Modified;
                    db.Commit();
                    return po;
                }
                catch (Exception ex)
                {

                    return null;
                }
        }

        /// <summary>
        /// Reject po by Approver
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Authorize]
        [Route("RejectbyApprover")]
        [HttpPut]
        public PurchaseOrderMaster RejectByApprover(PurchaseOrderMaster data)
        {

            using (var db = new AuthContext())
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

                    People p = db.Peoples.Where(q => q.PeopleID == userid).FirstOrDefault();
                    PoApproval IsRevHere = db.PoApprovalDB.Where(a => a.Level == data.Level).FirstOrDefault();
                    PurchaseOrderMaster po = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == data.PurchaseOrderId && a.PRStatus == 1).SingleOrDefault();
                    if (po != null)
                    {
                        //po.Status = "PR Rejected";
                        //po.PRStatus = 4;
                        po.RejectedBy = p.DisplayName;
                        po.CommentApvl = data.CommentApvl;
                        db.Entry(po).State = EntityState.Modified;
                        db.Commit();
                    }
                    return po;
                }
                catch (Exception ex)
                {

                    return null;
                }
        }

        #endregion


        [Route("PRPaymentApproval")]
        public IEnumerable<PRApproval> Get()
        {
            //logger.Info("start Category: ");
            List<PRApproval> pRlist = new List<PRApproval>();
            using (var db = new AuthContext())
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
                    pRlist = db.PRApprovalDB.Where(a => a.IsDeleted == false).ToList();

                    return pRlist;
                }
                catch (Exception ex)
                {

                    return null;
                }
            }
        }



        [Route("Add")]
        [HttpPost]
        public PRApproval add(PRApproval item)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    People p = new People();
                    try
                    {
                        p = db.Peoples.Where(a => a.PeopleID == item.ApprovalBy).SingleOrDefault();
                    }
                    catch (Exception ex) { }
                    PRApproval pa = new PRApproval();
                    pa.AmountlmtMin = item.AmountlmtMin;
                    pa.AmountlmtMax = item.AmountlmtMax;
                    pa.RoleName = item.RoleName;

                    try
                    {
                        pa.ApprovalName = p.DisplayName ?? null;
                    }
                    catch { pa.ApprovalName = null; }
                    pa.ApprovalBy = p.PeopleID;
                    pa.IsDeleted = false;
                    pa.CreatedDate = DateTime.Now;
                    pa.UpdatedTime = DateTime.Now;
                    db.PRApprovalDB.Add(pa);
                    db.Commit();

                    return item;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }


        [Route("Update")]
        [HttpPut]
        public PRApproval update(PRApproval item)
        {
            //logger.Info("start addCategory: ");
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    PRApproval pa = db.PRApprovalDB.Where(a => a.Poapprovelid == item.Poapprovelid).SingleOrDefault();
                    pa.AmountlmtMin = item.AmountlmtMin;
                    pa.AmountlmtMax = item.AmountlmtMax;
                    pa.RoleName = item.RoleName;
                    pa.IsDeleted = false;
                    pa.UpdatedTime = DateTime.Now;
                    db.Entry(pa).State = EntityState.Modified;
                    db.Commit();

                    return item;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        [Route("RemoveAprover")]
        [HttpPut]
        public PRApproval DeleteAprover(PRApproval item)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    PRApproval pa = db.PRApprovalDB.Where(a => a.Poapprovelid == item.Poapprovelid).SingleOrDefault();
                    pa.IsDeleted = true;
                    pa.UpdatedTime = DateTime.Now;
                    db.Entry(pa).State = EntityState.Modified;
                    db.Commit();

                    return item;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }



        [Authorize]
        [Route("getapr")]
        public IEnumerable<PRPeopleDTo> Getppl()
        {
            using (var db = new AuthContext())
            {
                try
                {
                    List<PRPeopleDTo> ass = new List<PRPeopleDTo>();
                    var subcate = db.Peoples.Where(x => x.Active == true).Select(s => new PRPeopleDTo() { Approval = s.PeopleID, ApprovalName = s.DisplayName }).ToList();

                    return subcate;
                }
                catch (Exception ex)
                {

                    return null;
                }
            }
        }

        [Authorize]
        [Route("GetNew")]
        [HttpPost] // via status and wh call
        public PaggingData getDataNew(filter filter)
        {
            PaggingData paggingData = new PaggingData();
            try
            {
                filter.skip = (filter.skip - 1) * filter.take;
                string whereconditon = "";
                using (var db = new AuthContext())
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (filter.WarehouseId != 0)
                    {
                        whereconditon += " and  PurchaseOrderMasters.WarehouseId= " + filter.WarehouseId;
                    }
                    if (!string.IsNullOrEmpty(filter.status))
                    {
                        whereconditon += " and PRApprovelsStatus.IsApprove= " + filter.status;
                    }

                    string sqlquery = "SELECT * FROM PurchaseOrderMasters INNER JOIN PRApprovelsStatus"
                                  + " ON PurchaseOrderMasters.PurchaseOrderId = PRApprovelsStatus.PurchaseOrderId where"
                                  + " PRApprovelsStatus.ApprovalID =" + userid + "and PRApprovelsStatus.IsActive = 1 and PurchaseOrderMasters.PRStatus != 2" + whereconditon;

                    string sqlqueryCount = "SELECT count(*) as cnt FROM PurchaseOrderMasters INNER JOIN PRApprovelsStatus"
                                 + " ON PurchaseOrderMasters.PurchaseOrderId = PRApprovelsStatus.PurchaseOrderId where"
                                 + " PRApprovelsStatus.ApprovalID =" + userid + "and PRApprovelsStatus.IsActive = 1 and PurchaseOrderMasters.PRStatus != 2" + whereconditon;


                    List<PurchaseOrderMasterDTO> PRSts = db.Database.SqlQuery<PurchaseOrderMasterDTO>(sqlquery).OrderByDescending(a => a.PurchaseOrderId).Skip(filter.skip).Take(filter.take).ToList();
                    int totalcount = db.Database.SqlQuery<int>(sqlqueryCount).FirstOrDefault();
                    paggingData.ordermaster = PRSts;
                    paggingData.total_count = totalcount;
                    return paggingData;
                }
            }
            catch (Exception ex)
            {
                return paggingData;
            }
        }


        public class filter
        {
            public string status { get; set; }
            public int WarehouseId { get; set; }
            public int take { get; set; }
            public int skip { get; set; }
        }
        public class PurchaseOrderMasterDTO
        {
            public int PurchaseOrderId { get; set; }
            public string PoInvoiceNo { get; set; }
            public string TransactionNumber { get; set; }
            public int? CompanyId { get; set; }
            public int SupplierId { get; set; }
            public string SupplierName { get; set; }
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public string WarehouseCity { get; set; }
            public string Status { get; set; }
            public double? discount1 { get; set; }
            public double TotalAmount { get; set; }
            public double Advance_Amt { get; set; }
            public double ETotalAmount { get; set; }
            public string PoType { get; set; }
            public string Comment { get; set; }
            public string CommentApvl { get; set; }
            public string Commentsystem { get; set; }
            public string Level { get; set; }
            public string progress { get; set; }
            public string IR_Progress { get; set; }
            public int? BuyerId { get; set; }
            public string BuyerName { get; set; }
            public string SupplierRejectReason { get; set; }
            public DateTime CreationDate { get; set; }
            public string CreatedBy { get; set; }
            public string ApprovedBy { get; set; }
            public string RejectedBy { get; set; }
            public bool Acitve { get; set; }
            public string IrStatus { get; set; }
            public string IrRejectComment { get; set; }
            public int? CanceledById { get; set; }
            public string CanceledByName { get; set; }
            public DateTime? CanceledDate { get; set; }
            public int? DepoId { get; set; }
            public string DepoName { get; set; }
            public bool IsCashPurchase { get; set; }
            public string CashPurchaseName { get; set; }
            public int? GRcount { get; set; }
            public bool IsLock { get; set; }
            public int? IRcount { get; set; }
            public bool Deleted { get; set; }
            public bool Active { get; set; }
            public int? DueDays { get; set; }
            public int SupplierStatus { get; set; }
            public int PRStatus { get; set; }
            public bool IsPR { get; set; }
            public int? IsApprove { get; set; }
            public bool IsActive { get; set; }
            public string PRPaymentType { get; set; }
            public bool IsAdjustmentPo { get; set; }
        }

        public class PRPeopleDTo
        {
            public int? Approval { get; set; }
            public string ApprovalName { get; set; }
        }

        public class POBuyerBrandCategory
        {
            public int? BuyerId { get; set; }
            public string DisplayName { get; set; }

        }

    }
}