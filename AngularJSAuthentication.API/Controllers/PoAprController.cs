using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/PoApr")]
    public class PoAprController : ApiController
    {
        
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [ResponseType(typeof(PoApproval))]
        [Route("add")]
        [HttpPost]
        public PoApproval add(PoApproval item)
        {
            logger.Info("start addCategory: ");
            using (var db = new AuthContext())
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

                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);


                    var WarehouseName = db.Warehouses.Where(a => a.WarehouseId == item.Warehouseid).SingleOrDefault();
                    PoApproval pa = db.PoApprovalDB.Where(a => a.Poapprovelid == item.Poapprovelid).SingleOrDefault();
                    People p = new People();
                    People p2 = new People();
                    People p3 = new People();
                    People p4 = new People();
                    People p5 = new People();
                    People r = new People();
                    People r2 = new People();
                    People r3 = new People();
                    People r4 = new People();
                    People r5 = new People();
                    try
                    {
                        // Get Apprver name
                        p = db.Peoples.Where(a => a.PeopleID == item.Approval1).SingleOrDefault();
                        p2 = db.Peoples.Where(a => a.PeopleID == item.Approval2).SingleOrDefault();
                        p3 = db.Peoples.Where(a => a.PeopleID == item.Approval3).SingleOrDefault();
                        p4 = db.Peoples.Where(a => a.PeopleID == item.Approval4).SingleOrDefault();
                        p5 = db.Peoples.Where(a => a.PeopleID == item.Approval5).SingleOrDefault();
                        // Get Reviewer name
                        r = db.Peoples.Where(a => a.PeopleID == item.Reviewer1).SingleOrDefault();
                        r2 = db.Peoples.Where(a => a.PeopleID == item.Reviewer2).SingleOrDefault();
                        r3 = db.Peoples.Where(a => a.PeopleID == item.Reviewer3).SingleOrDefault();
                        r4 = db.Peoples.Where(a => a.PeopleID == item.Reviewer4).SingleOrDefault();
                        r5 = db.Peoples.Where(a => a.PeopleID == item.Reviewer5).SingleOrDefault();

                    }
                    catch (Exception ex) { }

                    // Approver
                    pa.Approval1 = item.Approval1;
                    pa.Approval2 = item.Approval2;
                    pa.Approval3 = item.Approval3;
                    pa.Approval4 = item.Approval4;
                    pa.Approval5 = item.Approval5;
                    try
                    {
                        pa.ApprovalName1 = p.DisplayName ?? null;
                    }
                    catch { pa.ApprovalName1 = null; }
                    try
                    {
                        pa.ApprovalName2 = p2.DisplayName ?? null;
                    }
                    catch { pa.ApprovalName2 = null; }
                    try
                    {
                        pa.ApprovalName3 = p3.DisplayName ?? null;
                    }
                    catch { pa.ApprovalName3 = null; }
                    try
                    {
                        pa.ApprovalName4 = p4.DisplayName ?? null;
                    }
                    catch { pa.ApprovalName4 = null; }
                    try
                    {
                        pa.ApprovalName5 = p5.DisplayName ?? null;
                    }
                    catch { pa.ApprovalName5 = null; }

                    // Reviewer

                    pa.Reviewer1 = item.Reviewer1;
                    pa.Reviewer2 = item.Reviewer2;
                    pa.Reviewer3 = item.Reviewer3;
                    pa.Reviewer4 = item.Reviewer4;
                    pa.Reviewer5 = item.Reviewer5;

                    try
                    {
                        pa.ReviewerName1 = r.DisplayName ?? null;
                    }
                    catch { pa.ReviewerName1 = null; }
                    try
                    {
                        pa.ReviewerName2 = r2.DisplayName ?? null;
                    }
                    catch { pa.ReviewerName2 = null; }
                    try
                    {
                        pa.ReviewerName3 = r3.DisplayName ?? null;
                    }
                    catch { pa.ReviewerName3 = null; }
                    try
                    {
                        pa.ReviewerName4 = r4.DisplayName ?? null;
                    }
                    catch { pa.ReviewerName4 = null; }
                    try
                    {
                        pa.ReviewerName5 = r5.DisplayName ?? null;
                    }
                    catch { pa.ReviewerName5 = null; }

                    pa.AmountlmtMin = item.AmountlmtMin;
                    pa.AmountlmtMax = item.AmountlmtMax;
                    //pa.Warehouseid = item.Warehouseid;
                    //pa.WarehouseName = WarehouseName.WarehouseName;
                    pa.IsDeleted = false;
                    pa.CreatedDate = DateTime.Now;
                    pa.UpdatedTime = DateTime.Now;
                    pa.userid = userid;
                    db.Entry(pa).State = EntityState.Modified;
                    db.Commit();
                    try
                    {
                        db.AddPOApprovalHistory(pa);
                    }
                    catch (Exception ex) { }

                    logger.Info("End  addCategory: ");
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCategory " + ex.Message);
                    logger.Info("End  addCategory: ");
                    return null;
                }
            }
        }
        [Authorize]
        [Route("")]
        [AllowAnonymous]
        public IEnumerable<PoApproval> Get(int wid)
        {
            logger.Info("start Category: ");
            List<PoApproval> ass = new List<PoApproval>();
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = db.PoApprovalDB.Where(a => a.Warehouseid == wid).ToList();
                    logger.Info("End  Category: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
        }
        #region get POApprovalhistory
        /// <summary>
        /// Get PO Approval History
        /// Created By Ashwin
        /// </summary>
        /// <param name="poapprovalid"></param>
        /// <returns></returns>
        [Authorize]
        [Route("POApprovalhistory")]
        [HttpGet]
        public dynamic poapprovalhistory(int poapprovalid)
        {
            using (var odd = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0, Warehouse_id = 0;
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

                    var data = odd.POApprovalHistoryDB.Where(x => x.Poapprovelid == poapprovalid).ToList();
                    return data;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
        #endregion

        [Authorize]
        [Route("getapr1")]
        public IEnumerable<PeopleDTo> Getppl(int warehouseid)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    List<PeopleDTo> ass = new List<PeopleDTo>();
                    // var subcate = db.Peoples.Where(x => x.WarehouseId == warehouseid).Select(s => new PeopleDTo() { Approval1 = s.PeopleID, ApprovalName1 = s.DisplayName });
                    var subcate = db.Peoples.Where(x => x.Active == true).Select(s => new PeopleDTo() { Approval1 = s.PeopleID, ApprovalName1 = s.DisplayName }).ToList();
                    //ass = db.Peoples.Where(a=>a.WarehouseId == warehouseid).ToList();
                    logger.Info("End  Category: ");
                    return subcate;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
        }

        [Authorize]
        [Route("getapr2")]
        public IEnumerable<PeopleDTo2> Getppl2(int warehouseid)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    List<PeopleDTo2> ass = new List<PeopleDTo2>();
                    //  var subcate = db.Peoples.Where(x => x.WarehouseId == warehouseid).Select(s => new PeopleDTo2() { Approval2 = s.PeopleID, ApprovalName2 = s.DisplayName });
                    var subcate = db.Peoples.Where(x => x.Active == true).Select(s => new PeopleDTo2() { Approval2 = s.PeopleID, ApprovalName2 = s.DisplayName }).ToList();
                    logger.Info("End  Category: ");
                    return subcate;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
        }

        [Authorize]
        [Route("getapr3")]
        public IEnumerable<PeopleDTo3> Getppl3(int warehouseid)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    List<PeopleDTo3> ass = new List<PeopleDTo3>();
                    //  var subcate = db.Peoples.Where(x => x.WarehouseId == warehouseid).Select(s => new PeopleDTo3() { Approval3 = s.PeopleID, ApprovalName3 = s.DisplayName });
                    var subcate = db.Peoples.Where(x => x.Active == true).Select(s => new PeopleDTo3() { Approval3 = s.PeopleID, ApprovalName3 = s.DisplayName }).ToList();
                    logger.Info("End  Category: ");
                    return subcate;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
        }

        [Authorize]
        [Route("getapr4")]
        public IEnumerable<PeopleDTo4> Getppl4(int warehouseid)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    List<PeopleDTo4> ass = new List<PeopleDTo4>();
                    //  var subcate = db.Peoples.Where(x => x.WarehouseId == warehouseid).Select(s => new PeopleDTo4() { Approval4 = s.PeopleID, ApprovalName4 = s.DisplayName });
                    var subcate = db.Peoples.Where(x => x.Active == true).Select(s => new PeopleDTo4() { Approval4 = s.PeopleID, ApprovalName4 = s.DisplayName }).ToList();
                    logger.Info("End  Category: ");
                    return subcate;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
        }

        [Authorize]
        [Route("getapr5")]
        public IEnumerable<PeopleDTo5> Getppl5(int warehouseid)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    List<PeopleDTo5> ass = new List<PeopleDTo5>();
                    //  var subcate = db.Peoples.Where(x => x.WarehouseId == warehouseid).Select(s => new PeopleDTo5() { Approval5 = s.PeopleID, ApprovalName5 = s.DisplayName });
                    var subcate = db.Peoples.Where(x => x.Active == true).Select(s => new PeopleDTo5() { Approval5 = s.PeopleID, ApprovalName5 = s.DisplayName }).ToList();
                    logger.Info("End  Category: ");
                    return subcate;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
        }
        /// End Approver        

        /// Reviewer 
        [Authorize]
        [Route("getrvr1")]
        public IEnumerable<RvrDTO1> GetRev1(int warehouseid)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    List<RvrDTO1> ass = new List<RvrDTO1>();
                    //  var subcate = db.Peoples.Where(x => x.WarehouseId == warehouseid).Select(s => new RvrDTO1() { Reviewer1 = s.PeopleID, ReviewerName1 = s.DisplayName });
                    var subcate = db.Peoples.Where(x => x.Active == true).Select(s => new RvrDTO1() { Reviewer1 = s.PeopleID, ReviewerName1 = s.DisplayName }).ToList();
                    logger.Info("End  Category: ");
                    return subcate;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
        }

        [Authorize]
        [Route("getrvr2")]
        public IEnumerable<RvrDTO2> GetRev2(int warehouseid)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    List<RvrDTO2> ass = new List<RvrDTO2>();
                    //var subcate = db.Peoples.Where(x => x.WarehouseId == warehouseid).Select(s => new RvrDTO2() { Reviewer2 = s.PeopleID, ReviewerName2 = s.DisplayName });
                    var subcate = db.Peoples.Where(x => x.Active == true).Select(s => new RvrDTO2() { Reviewer2 = s.PeopleID, ReviewerName2 = s.DisplayName }).ToList();
                    logger.Info("End  Category: ");
                    return subcate;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
        }

        [Authorize]
        [Route("getrvr3")]
        public IEnumerable<RvrDTO3> GetRev3(int warehouseid)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    List<RvrDTO3> ass = new List<RvrDTO3>();
                    // var subcate = db.Peoples.Where(x => x.WarehouseId == warehouseid).Select(s => new RvrDTO3() { Reviewer3 = s.PeopleID, ReviewerName3 = s.DisplayName });
                    var subcate = db.Peoples.Where(x => x.Active == true).Select(s => new RvrDTO3() { Reviewer3 = s.PeopleID, ReviewerName3 = s.DisplayName }).ToList();
                    logger.Info("End  Category: ");
                    return subcate;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
        }

        [Authorize]
        [Route("getrvr4")]
        public IEnumerable<RvrDTO4> GetRev4(int warehouseid)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    List<RvrDTO4> ass = new List<RvrDTO4>();
                    // var subcate = db.Peoples.Where(x => x.WarehouseId == warehouseid).Select(s => new RvrDTO4() { Reviewer4 = s.PeopleID, ReviewerName4 = s.DisplayName });
                    var subcate = db.Peoples.Where(x => x.Active == true).Select(s => new RvrDTO4() { Reviewer4 = s.PeopleID, ReviewerName4 = s.DisplayName }).ToList();
                    logger.Info("End  Category: ");
                    return subcate;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
        }

        [Authorize]
        [Route("getrvr5")]
        public IEnumerable<RvrDTO5> GetRev5(int warehouseid)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    List<RvrDTO5> ass = new List<RvrDTO5>();
                    //  var subcate = db.Peoples.Where(x => x.WarehouseId == warehouseid).Select(s => new RvrDTO5() { Reviewer5 = s.PeopleID, ReviewerName5 = s.DisplayName });
                    var subcate = db.Peoples.Where(x => x.Active == true).Select(s => new RvrDTO5() { Reviewer5 = s.PeopleID, ReviewerName5 = s.DisplayName }).ToList();
                    logger.Info("End  Category: ");
                    return subcate;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
            /// End Reviewer
        }
    }

    /// <summary>
    /// Approver DTO
    /// </summary>
    public class PeopleDTo
    {
        public int? Approval1 { get; set; }
        public string ApprovalName1 { get; set; }
    }
    public class PeopleDTo2
    {
        public int? Approval2 { get; set; }
        public string ApprovalName2 { get; set; }
    }
    public class PeopleDTo3
    {
        public int? Approval3 { get; set; }
        public string ApprovalName3 { get; set; }
    }
    public class PeopleDTo4
    {
        public int? Approval4 { get; set; }
        public string ApprovalName4 { get; set; }
    }
    public class PeopleDTo5
    {
        public int? Approval5 { get; set; }
        public string ApprovalName5 { get; set; }
    }


    public class RvrDTO1
    {
        public int? Reviewer1 { get; set; }
        public string ReviewerName1 { get; set; }
    }
    public class RvrDTO2
    {
        public int? Reviewer2 { get; set; }
        public string ReviewerName2 { get; set; }
    }
    public class RvrDTO3
    {
        public int? Reviewer3 { get; set; }
        public string ReviewerName3 { get; set; }
    }
    public class RvrDTO4
    {
        public int? Reviewer4 { get; set; }
        public string ReviewerName4 { get; set; }
    }
    public class RvrDTO5
    {
        public int? Reviewer5 { get; set; }
        public string ReviewerName5 { get; set; }
    }
}