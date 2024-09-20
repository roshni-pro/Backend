using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Cases")]
    public class CaseController : ApiController
    {

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();


        /// <summary>
        /// Get Api For All Case
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("")]
        public IEnumerable<CaseModule> Get()
        {
            using (var context = new AuthContext())
            {
                logger.Info("start Case: ");
                List<CaseModule> ass = new List<CaseModule>();
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
                    ass = context.AllCase().ToList();
                    logger.Info("End  Case: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Case " + ex.Message);
                    logger.Info("End  Case: ");
                    return null;
                }
            }
        }

        [Route("select")]
        [HttpGet]
        public dynamic Get(int value)
        {
            using (var db = new AuthContext())
            {
                dynamic result = null;
                logger.Info("start Get Report2: ");
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

                    if (value == 4)
                    {
                        //result = db.Customers.Where(x => x.Deleted == false && x.CompanyId == compid).ToList();
                        string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where r.Name='Sales Executive' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                        result = db.Database.SqlQuery<People>(query).ToList();

                        //result = db.Peoples.Where(x => x.Department == "Sales Executive" && x.Deleted == false).ToList();

                    }
                    return result;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report  " + ex.Message);
                    logger.Info("End  Customers Report: ");
                    return null;
                }
            }
        }

        /// <summary>
        /// get api For Search Customer data 
        /// behalf SkCode and mobile
        /// </summary>
        /// <param name="SkCode"></param>
        /// <param name="mobile"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("search1")]
        public HttpResponseMessage serach(string SkCode, string mobile)
        {
            using (var db = new AuthContext())
            {
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
                    var customer = db.Customers.Where(c => (c.Skcode.Contains(SkCode) || c.Mobile.Contains(mobile)) && c.Deleted == false).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, customer);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        /// <summary>
        /// get Api For notification Case Managemant
        /// </summary>
        /// <param name="Assignto"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Notification")]
        public HttpResponseMessage serachNoti(string Assignto)
        {
            using (var db = new AuthContext())
            {
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

                    var CaseModules = db.Cases.Where(c => c.Assignto == Assignto && c.Deleted == false && (c.Status == "Open" || c.Status == "Reassign")).Count();
                    return Request.CreateResponse(HttpStatusCode.OK, CaseModules);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        //end

        /// <summary>
        ///  get api For Search cases data 
        /// behalf SkCode and mobile
        /// </summary>
        /// <param name="skcode"></param>
        /// <param name="mobile"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("casesdata")]
        public HttpResponseMessage serachs(string skcode, string mobile)
        {
            using (var db = new AuthContext())
            {
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
                    var casesdata = db.Cases.Where(c => c.SKCode == skcode || c.MobileNumber == mobile && c.Deleted == false).OrderByDescending(x => x.CaseId).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, casesdata);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [HttpGet]
        [Route("search2")]
        public HttpResponseMessage serachview(string skcode, string mobile, string status)
        {
            using (var db = new AuthContext())
            {
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
                    // var list = db.Cases.Where(c => (c.SKCode.Contains(skcode) || c.MobileNumber.Contains(mobile)) && c.Deleted == false).ToList();
                    List<CaseData> caseModules = (from c in db.Cases 
                                                  where c.SKCode.Contains(skcode) || c.MobileNumber.Contains(mobile) || c.Status.Contains(status)  
                                                  join cc in db.IssueCategoryDB on c.IssueCategoryId equals cc.IssueCategoryId
                                                  join cs in db.IssueSubCategoryDB on c.IssueSubCategoryId equals cs.IssueSubCategoryId 
                                                  select new CaseData
                                                  {
                                                      CaseId = c.CaseId,
                                                      Skcode = c.SKCode,
                                                      CategoryName = cc.IssueCategoryName,
                                                      SubCategoryName = cs.IssueSubCategoryName,
                                                      IssueSubCategoryId = cs.IssueSubCategoryId,
                                                      Priority = c.Priority,
                                                      Issue = c.Issue,
                                                      Summary = c.Summary,
                                                      Status = c.Status,
                                                      MobileNumber = c.MobileNumber,
                                                      CreatedByName = c.CreatedbyName,
                                                      Statuscall = c.Statuscall,
                                                      CreatedDate = c.CreatedDate,
                                                      Assignto = c.Assignto,
                                                      Description = c.Description
                                                  }).ToList();



                    return Request.CreateResponse(HttpStatusCode.OK, caseModules);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        //end
        /// <summary>
        /// post api for add case behalf of item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [ResponseType(typeof(CaseModule))]
        [Route("AddCase")]
        [AcceptVerbs("POST")]
        public CaseModule add(CaseModule item)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start Add CaseModule: ");
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    string D_Name = null;

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
                        if (claim.Type == "DisplayName")
                        {
                            D_Name = (claim.Value);
                        }
                    }

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    UploadFile();
                    context.AddCase(item);
                    logger.Info("End  Add Case: ");
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add Case " + ex.Message);
                    logger.Info("End  Add Case: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(CaseModule))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public CaseModule put(CaseModule item)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start putcases: ");
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

                    // item.CaseId = userid;
                    if (item == null)
                    {
                        throw new ArgumentNullException("putcase");
                    }
                    if (compid == 0)
                    {
                        item.CaseId = compid;
                    }

                    return context.SetCase(item);
                    return null;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in put Customer " + ex.Message);
                    return null;
                }
            }
        }
        //CaseHistory
        [Route("Casehistory")]
        [HttpGet]
        public dynamic CaseHistory(int CaseId)
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

                    var data = odd.CaseHistoryDB.Where(x => x.CaseId == CaseId).ToList();
                    return data;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
        //
        [HttpPost]
        [Route("Setstatus")]
        public CaseModule add(int CaseId, string status, string discription, string Assign)
        {
            using (var db = new AuthContext())
            {
                logger.Info("Update Casedata: ");
                try
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    string D_Name = null;

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
                        if (claim.Type == "DisplayName")
                        {
                            D_Name = (claim.Value);
                        }
                    }

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    CaseModule aa = db.Cases.Where(x => x.CaseId == CaseId).SingleOrDefault();
                    aa.Status = status;
                    aa.Summary = discription;
                    aa.Assignto = Assign;

                    db.Entry(aa).State = EntityState.Modified;
                    db.Commit();
                    return aa;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add Case " + ex.Message);
                    logger.Info("End  Add Case: ");
                    return null;
                }
            }
        }


        [HttpGet]
        [Route("getAllCase")]
        public List<CaseData> getAllCase()
        {
            using (var db = new AuthContext())
            {
                logger.Info("start View CaseModule: ");
                try
                {
                    DateTime now = DateTime.Now;
                    string thisMonth = now.AddDays(1).ToString("yyyy-MM-dd");
                    string lastMonth = now.AddMonths(-1).ToString("yyyy-MM-dd");
                    var query = " select CM.CaseId,CM.SKCode,IC.IssueCategoryName as CategoryName, "+
                                    "  IC.IssueCategoryId,ISC.IssueSubCategoryId,ISC.IssueSubCategoryName as SubCategoryName , " +
                                    "  CM.Priority,CM.Issue,CM.Summary,CM.Status,CM.MobileNumber,CM.CreatedbyName, " +
                                    "  CM.Statuscall,CM.CreatedDate,CM.UpdatedDate,CM.Assignto,CM.Description " +
                                    "  from CaseModules CM  " +
                                    "  join IssueCategories IC on CM.IssueCategoryId = IC.IssueCategoryId " +
                                    "  join IssueSubCategories ISC on CM.IssueSubCategoryId = ISC.IssueSubCategoryId " +
                                    " and CM.CreatedDate between '" + lastMonth + "' and  '" + thisMonth + "' order by CM.CaseId desc  ";
                    var result = db.Database.SqlQuery<CaseData>(query).ToList();
                    return result;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add Case " + ex.Message);
                    logger.Info("End  Add Case: ");
                    return null;
                }
            }
        }
        //end
        //case view setstatus
        [HttpPost]
        [Route("viewSetstatus")]
        public CaseModule caseviewsat(int CaseId, string status, string discription, string Assign)
        {
            using (var db = new AuthContext())
            {
                logger.Info("Update Casedata: ");
                try
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    string D_Name = null;

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
                        if (claim.Type == "DisplayName")
                        {
                            D_Name = (claim.Value);
                        }
                    }

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    CaseModule aa = db.Cases.Where(x => x.CaseId == CaseId).SingleOrDefault();
                    aa.Status = status;
                    aa.Description = discription;
                    aa.Assignto = Assign;

                    db.Entry(aa).State = EntityState.Modified;
                    db.Commit();
                    return aa;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in  Case view" + ex.Message);
                    logger.Info("End   view: ");
                    return null;
                }
            }
        }
        //ends
        //Caseviewstatus
        [ResponseType(typeof(CaseModule))]
        [Route("Caseviewstatus")]
        [AcceptVerbs("POST")]
        public CaseModule viewaddstatus(CaseModule item)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start Add CaseModule: ");
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    string D_Name = null;

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
                        if (claim.Type == "DisplayName")
                        {
                            D_Name = (claim.Value);
                        }
                    }

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    UploadFile();
                    context.Caseviewstatus(item);
                    logger.Info("End  Add Case: ");
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add Case " + ex.Message);
                    logger.Info("End  Add Case: ");
                    return null;
                }
            }
        }
        //end
        [ResponseType(typeof(CaseComment))]
        [Route("AddComment")]
        [AcceptVerbs("POST")]
        public CaseComment AddComment(CaseComment comment)
        {
            using (var db = new AuthContext())
            {
                logger.Info("start Add Case Comment: ");
                try
                {
                    var identity = User.Identity as ClaimsIdentity;

                    int compid = 0, userid = 0;
                    string UserName = null;
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
                        if (claim.Type == "Email")
                        {
                            UserName = (claim.Value);
                        }
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    //context.AddCommentCase(comment);
                    var cases = (from course in db.CaseComment where course.Comments == comment.Comments select course).ToList();
                    CaseComment objCase = new CaseComment();
                    if (cases.Count == 0)
                    {
                        comment.CaseId = comment.CaseId;
                        comment.Comments = comment.Comments;
                        comment.UserId = userid;
                        comment.UserName = UserName;
                        comment.active = true;
                        comment.Deleted = false;
                        comment.CreatedDate = indianTime;
                        comment.UpdatedDate = indianTime;
                        db.CaseComment.Add(comment);
                        // this.SaveChanges();
                        int id = db.Commit();
                        return comment;
                    }
                    else
                    {
                        return objCase;

                    }
                    //UploadFile();
                    logger.Info("End  Add Case: ");
                    return comment;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add Case " + ex.Message);
                    logger.Info("End  Add Case: ");
                    return null;
                }
            }
        }

        [Route("unique")]
        [HttpGet]
        public dynamic uniqueItem()
        {
            using (var db = new AuthContext())
            {

                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int warehouseid = 0;

                    //List<ItemMaster> ItemData = new List<ItemMaster>();
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
                            warehouseid = int.Parse(claim.Value);
                        }
                    }

                    if (warehouseid > 0)
                    {
                        var ItemData = db.Customers.Where(x => x.Deleted == false && x.CompanyId == compid && x.Warehouseid == warehouseid).GroupBy(x => x.Name).ToList().Select(x => x.FirstOrDefault()).ToList();
                        //var  ItemData = db.itemMasters.Where(x => x.active == true && x.Deleted == false && x.CompanyId == compid && x.WarehouseId == warehouseid).GroupBy(x=> x.Number).FirstOrDefault().ToList();
                        return ItemData;
                    }
                    else
                    {
                        var ItemData = db.Customers.Where(x => x.Deleted == false && x.CompanyId == compid).GroupBy(x => x.Name).ToList().Select(x => x.FirstOrDefault()).ToList();
                        return ItemData;

                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in CUstomer " + ex.Message);
                    logger.Info("End  ItemMaster: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(CaseModule))]
        [Route("SetCase")]
        [AcceptVerbs("PUT")]
        public CaseModule PutEditt(CaseModule item)
        {
            using (var context = new AuthContext())
            {
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
                    return context.PutCase(item);
                }
                catch
                {
                    return null;
                }
            }
        }

        [Route("EditCase")]
        [HttpPut]
        public HttpResponseMessage Edit(CaseModule item)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    if (item != null)
                    {
                        CaseModule objcase = db.Cases.Where(c => c.CaseId == item.CaseId).FirstOrDefault();
                        if (objcase != null)
                        {
                            objcase.UpdatedDate = indianTime;
                            db.Cases.Add(objcase);

                            db.Entry(objcase).State = EntityState.Modified;
                            db.Commit();
                            return Request.CreateResponse(HttpStatusCode.OK, "Case Update Successfully!!");
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Error in Case Update!!");
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error in catch method call!!");
                }
            }
        }

        [ResponseType(typeof(CaseModule))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start deleteCase: ");
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
                    context.DeleteCase(id);
                    logger.Info("End  delete Case: ");
                }
                catch (Exception ex)
                {
                    logger.Error("Error in delete case " + ex.Message);
                }
            }
        }

        [Route("search")]
        [HttpGet]
        public dynamic search(string ProjectName)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;

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
                    if (ProjectName != null)
                    {
                        var data = context.Cases.Where(x => x.Deleted == false && x.ProjectName.Contains(ProjectName)).ToList();

                        return data;
                    }
                    else
                    {
                        return null;
                    }
                }

                catch (Exception ex)
                {
                    logger.Error("Error in Case Master " + ex.Message);
                    logger.Info("End  Case Master: ");
                    return null;
                }
            }
        }


        [ResponseType(typeof(CaseModule))]
        [Route("PostFileWithData")]
        public async Task<HttpResponseMessage> Post()
        {
            using (var db = new AuthContext())
            {
                // Check whether the POST operation is MultiPart?
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                // Prepare CustomMultipartFormDataStreamProvider in which our multipart form
                // data will be loaded.
                string fileSaveLocation = HttpContext.Current.Server.MapPath("~/UploadedImages");
                CustomMultipartFormDataStreamProvider provider = new CustomMultipartFormDataStreamProvider(fileSaveLocation);
                List<string> files = new List<string>();

                try
                {
                    // Read all contents of multipart message into CustomMultipartFormDataStreamProvider.
                    var result = await Request.Content.ReadAsMultipartAsync(provider);
                    var model = result.FormData["jsonData"];
                    if (model == null)
                    {
                        throw new HttpResponseException(HttpStatusCode.BadRequest);
                    }
                    string CaseNumber = model;

                    foreach (MultipartFileData file in provider.FileData)
                    {
                        files.Add(Path.GetFileName(file.LocalFileName));
                        CaseImage obj = new CaseImage()
                        {
                            CaseNumber = CaseNumber,
                            CaseImageName = file.LocalFileName,
                            active = true,
                            Deleted = false,
                            CreatedDate = indianTime,
                            UpdatedDate = indianTime
                        };
                        db.CaseImage.Add(obj);
                        db.Commit();
                    }

                    // Send OK Response along with saved file names to the client.
                    return Request.CreateResponse(HttpStatusCode.OK, files);
                }
                catch (System.Exception e)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
                }
            }
        }

        public class CustomMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
        {
            public CustomMultipartFormDataStreamProvider(string path) : base(path) { }

            public override string GetLocalFileName(HttpContentHeaders headers)
            {
                return headers.ContentDisposition.FileName.Replace("\"", string.Empty);
            }
        }




        [HttpPost]
        public void UploadFile()
        {
            logger.Info("start image upload");
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

                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    // Get the uploaded image from the Files collection
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];

                    if (httpPostedFile != null)
                    {
                        // Validate the uploaded image(optional)

                        // Get the complete file path
                        var ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedImages"), httpPostedFile.FileName);

                        //// Save the uploaded file to "UploadedFiles" folder
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        httpPostedFile.SaveAs(ImageUrl);

                        var uploader = new System.Collections.Generic.List<AngularJSAuthentication.DataContracts.FileUpload.Uploader> { new AngularJSAuthentication.DataContracts.FileUpload.Uploader() };
                        uploader.FirstOrDefault().FileName = httpPostedFile.FileName;
                        uploader.FirstOrDefault().RelativePath = "~/UploadedImages";


                        uploader.FirstOrDefault().SaveFileURL = ImageUrl;

                        uploader = Nito.AsyncEx.AsyncContext.Run(() => AngularJSAuthentication.Common.Helpers.FileUploadHelper.UploadFileToOtherApi(uploader));


                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in  image upload " + ex.Message);
                logger.Info("End  image upload: ");

            }
        }
    }
    public class CaseData
    {
        public int CaseId
        {
            get; set;
        }
        public string Skcode
        {
            get; set;
        }
        public string CategoryName
        {
            get; set;
        }
        public string SubCategoryName
        {
            get; set;
        }
        public string Priority
        {
            get; set;
        }
        public string Issue
        {
            get; set;
        }
        public string Summary
        {
            get; set;
        }
        public string Status
        {
            get; set;
        }
        public string MobileNumber
        {
            get; set;
        }
        public string CreatedByName
        {
            get; set;
        }
        public string Statuscall
        {
            get; set;
        }
        public DateTime CreatedDate
        {
            get; set;
        }
        public DateTime? UpdatedDate { get; set; }
        public string Assignto
        {
            get; set;
        }
        public int IssueSubCategoryId
        {
            get; set;
        }

        public int IssueCategoryId
        {
            get; set;
        }
        public string Description
        {
            get; set;
        }
    }
}
