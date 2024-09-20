using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/OrangeBook")]
    public class OrangeBookController : ApiController
    {
        #region Global
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        #endregion

        #region OrangeBook
        [Route("AddOrangeBookVersion")]
        [HttpPost]
        public HttpResponseMessage AddOrangeBookVersion(int CategoryId, int SubCategoryId, string filepath)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


            using (var con = new AuthContext())
            {
                string version = "";
                List<OrangeBook> ob = new List<OrangeBook>();

                if (CategoryId > 0 && SubCategoryId == 0)
                {
                    var lastversion = con.OrangeBookDB.Where(x => x.CategoryId == CategoryId).Count();
                    version = "1." + lastversion.ToString();
                    ob = con.OrangeBookDB.Where(x => x.CategoryId == CategoryId && x.IsActive == true).ToList();
                }
                else if (CategoryId > 0 && SubCategoryId > 0)
                {
                    var lastversion = con.OrangeBookDB.Where(x => x.SubCategoryId == SubCategoryId).Count();
                    version = "1." + lastversion.ToString();
                    ob = con.OrangeBookDB.Where(x => x.SubCategoryId == SubCategoryId && x.IsActive == true).ToList();
                }

                var obv = new OrangeBook();
                obv.Version = version;
                obv.CategoryId = CategoryId;
                obv.SubCategoryId = SubCategoryId;
                obv.FilePath = filepath;
                obv.CreatedDate = indianTime;
                obv.CreatedBy = userid;
                obv.IsActive = true;
                obv.IsDeleted = false;
                con.OrangeBookDB.Add(obv);
                con.Commit();
                foreach (var item in ob)
                {
                    item.IsActive = false;
                    item.ModifiedDate = indianTime;
                    item.ModifiedBy = userid;
                    con.SaveChanges();
                }

                var FileList = new List<string>();

                var Orangebookfiles = con.Database.SqlQuery<OBFiles>("exec GetOBFiles").ToList();

                string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                var oblist = Orangebookfiles.Select(x => x.FilePath).ToList();

                foreach (var item in oblist)
                {
                    FileList.Add(baseUrl + "/OrangeBook/" + item);
                }

                var OBlastversion = con.OrangeBookDB.Where(x => x.SubCategoryId == 0 && x.CategoryId == 0).Count();

                var OBversion = "OB" + ++OBlastversion + ".pdf";

                var obve = con.OrangeBookDB.Where(x => x.SubCategoryId == 0 && x.CategoryId == 0 && x.IsActive == true).ToList();

                var destinationpath = HttpContext.Current.Server.MapPath("~/OrangeBook/") + OBversion;

                var status = MergePDFs(FileList, destinationpath);

                if (status == true)
                {
                    var Wobv = new OrangeBook();
                    Wobv.Version = "1." + OBlastversion.ToString();
                    Wobv.CategoryId = 0;
                    Wobv.SubCategoryId = 0;
                    Wobv.FilePath = OBversion;
                    Wobv.CreatedDate = indianTime;
                    Wobv.CreatedBy = userid;
                    Wobv.IsActive = true;
                    Wobv.IsDeleted = false;
                    con.OrangeBookDB.Add(Wobv);
                    con.Commit();
                    foreach (var item in obve)
                    {
                        item.IsActive = false;
                        item.ModifiedDate = indianTime;
                        item.ModifiedBy = userid;
                        con.SaveChanges();
                    }
                }
                int row = con.Database.ExecuteSqlCommand("update People set OrangeBookAcceptance = 0 where Active = 1 and Deleted = 0");

                var response = new
                {
                    Status = true,
                    Message = "Record Inserted"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);

            }

        }

        public static bool MergePDFs(List<string> fileNames, string targetPdf)
        {
            bool merged = true;
            using (FileStream stream = new FileStream(targetPdf, FileMode.Create))
            {
                Document document = new Document();
                PdfCopy pdf = new PdfCopy(document, stream);
                PdfReader reader = null;
                try
                {
                    document.Open();
                    foreach (string file in fileNames)
                    {
                        reader = new PdfReader(file);
                        pdf.AddDocument(reader);
                        reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    //LogHelper.LogError(new StringBuilder("Error while Merging Pdf: ").Append(ex.ToString()).ToString(), true);
                    merged = false;
                    if (reader != null)
                        reader.Close();
                }
                finally
                {
                    if (document != null)
                        document.Close();
                }
            }
            return merged;
        }

        [Route("GetOrangeBookActiveVersion")]
        [HttpGet]
        public HttpResponseMessage GetOrangeBookActiveVersion()
        {
            using (var con = new AuthContext())
            {
                var obv = con.OrangeBookDB.Where(x => x.CategoryId == 0 && x.SubCategoryId == 0 && x.IsActive == true).FirstOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, obv);
            }
        }

        [Route("GetOrangeBookVersion")]
        [HttpGet]
        public HttpResponseMessage GetOrangeBookVersion()
        {
            using (var con = new AuthContext())
            {
                var obv = con.OrangeBookDB.Where(x => x.IsActive == true && x.IsDeleted == false).OrderByDescending(x => x.Id).Take(1).Select(x => x.Version).ToList().FirstOrDefault();
                if (obv == null)
                {
                    obv = "0";
                }
                return Request.CreateResponse(HttpStatusCode.OK, obv);
            }

        }

        [Route("GetOrangeBookVersionPeople")]
        [HttpGet]
        public HttpResponseMessage GetOrangeBookVersionPeople()
        {
            using (var con = new AuthContext())
            {
                var obv = con.OrangeBookDB.Where(x => x.IsActive == true && x.IsDeleted == false).OrderByDescending(x => x.Id).Take(1).Select(x => new { Id = x.Id, Version = x.Version, FilePath = x.FilePath }).ToList().FirstOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, obv);
            }

        }

        [Route("AddOrangeBookVersionAcceptance")]
        [HttpPost]
        public HttpResponseMessage AddOrangeBookVersionAcceptance(int cat, int subcat, string version)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var con = new AuthContext())
            {
                int IsExits = 0;
                if (subcat > 0)
                {
                    IsExits = con.OrangeBookAcceptedDB.Where(x => x.PeopleId == userid && x.CategoryId == cat && x.SubCategoryId == subcat && x.Version == version).Count();
                }
                else
                {
                    IsExits = con.OrangeBookAcceptedDB.Where(x => x.PeopleId == userid && x.CategoryId == cat && x.Version == version).Count();
                }

                if (IsExits > 0)
                {
                    var response = new
                    {
                        Status = true,
                        Message = "Allready Accepted"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    string forwardedIps = "";
                    var ip = IPHelper.GetVisitorIPAddress(out forwardedIps);

                    var obv = new OrangeBookAccepted();
                    obv.PeopleId = userid;
                    obv.CategoryId = cat;
                    obv.SubCategoryId = subcat;
                    obv.Version = version;
                    obv.IsAccept = true;
                    obv.IP = forwardedIps;
                    obv.CreatedDate = indianTime;
                    obv.CreatedBy = userid;
                    obv.IsActive = true;
                    obv.IsDeleted = false;
                    con.OrangeBookAcceptedDB.Add(obv);
                    con.Commit();

                    var check = con.OrangeBookAcceptedDB.Where(x => x.PeopleId == obv.PeopleId && x.IP == obv.IP && x.CreatedDate.Day == indianTime.Day && x.CreatedDate.Month == indianTime.Month && x.CreatedDate.Year == indianTime.Year).ToList().Count();

                    if (check > 10)
                    {
                        var people = con.Peoples.Where(x => x.PeopleID == obv.PeopleId).FirstOrDefault();
                        var from = "nitesh.porwal@shopkirana.com";//"praveen.g@shopkirana.com";
                        var to = AppConstants.MasterEmail;//"praveen.goswami1404@gmail.com";
                        var bcc = "";
                        var subject = "Regarding Orange Book Maximum download";
                        var Message = "Hello Concern, " + people.DisplayName + " has been downloaded the orangebook SOP more than 10 times: IPAddress : " + obv.IP + " Times : " + check;
                        var filepath = "";
                        EmailHelper.SendMail(from, to, bcc, subject, Message, filepath);
                    }

                    var response = new
                    {
                        Status = true,
                        Message = "Saved Successfully"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
        }


        [Route("GetOrangeBookVersionData")]
        [HttpGet]
        public HttpResponseMessage GetOrangeBookVersionData()
        {
            using (var con = new AuthContext())
            {
                var obv = con.OrangeBookDB.Where(x => x.IsActive == true && x.IsDeleted == false).OrderByDescending(x => x.Id).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, obv);
            }
        }

        [Route("GetOrangeBookNotification")]
        [HttpGet]
        public int GetOrangeBookNotification()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var con = new AuthContext())
            {
                var param = new SqlParameter("peopleid", userid);

                var peopleAcceptance = con.Database.SqlQuery<PeopleAcceptenace>("exec GetPeopleAcceptance @peopleid", param).ToList();

                var count = peopleAcceptance.Where(x => x.IsAccept == false).ToList().Count();
                //var obv = con.Peoples.Where(x => x.PeopleID == userid).Select(x => x.OrangeBookAcceptance).FirstOrDefault();
                return count;
            }
        }


        [Route("GetBaseCategoryMongo")]
        [HttpGet]
        public HttpResponseMessage GetBaseCategoryMongo()
        {
            using (var con = new AuthContext())
            {
                var obv = con.OrangeBookDB.Where(x => x.IsActive == true && x.IsDeleted == false).OrderByDescending(x => x.Id).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, obv);
            }
        }

        [Route("GetOBCategory")]
        [HttpGet]
        public dynamic GetOBCategory()
        {

            using (var con = new AuthContext())
            {
                try
                {
                    var CheckerList = con.Database.SqlQuery<OBCategory>("exec GetOrangeBookCategory").ToList();

                    return Ok(CheckerList);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        [Route("GetOBSubCategory")]
        [HttpGet]
        public dynamic GetOBSubCategory(int categoryid)
        {

            using (var con = new AuthContext())
            {
                try
                {
                    var catid = new SqlParameter("categoryid", categoryid);

                    var CheckerList = con.Database.SqlQuery<OBSubCategory>("exec GetOrangeBookSubCategory @categoryid", catid).ToList();

                    return Ok(CheckerList);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        [Route("GetOBVersionList")]
        [HttpGet]
        public dynamic GetOBVersionList()
        {

            using (var con = new AuthContext())
            {
                try
                {
                    var VersionList = new List<OBVersionList>();
                    var OBVersionList = con.OrangeBookCategoryDB.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                    foreach (var item in OBVersionList)
                    {
                        var list = new OBVersionList();
                        var subcategorylist = con.OrangeBookSubCategoryDB.Where(x => x.IsActive == true && x.IsDeleted == false && x.CategoryId == item.Id).Select(x => new OBSubVersionList { CategoryID = x.CategoryId, SubCategoryName = x.SubCategoryName, Id = x.Id, IsAccept = true }).ToList();
                        list.Id = item.Id;
                        list.CategoryName = item.CategoryName;
                        list.IsAccept = true;
                        list.orangeBookSubCategory = subcategorylist;
                        VersionList.Add(list);
                    }

                    return Ok(VersionList);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        [Route("GetPeopleAcceptance")]
        [HttpGet]
        public dynamic GetPeopleAcceptance()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var con = new AuthContext())
            {
                try
                {

                    var param = new SqlParameter("peopleid", userid);

                    var peopleAcceptance = con.Database.SqlQuery<PeopleAcceptenace>("exec GetPeopleAcceptance @peopleid", param).ToList();
                    List<OBVersionList> OBVersionLists = new List<OBVersionList>();
                    foreach (var item in peopleAcceptance.GroupBy(x => new { x.CategoryName, x.Id }))
                    {
                        OBVersionList cat = new OBVersionList();
                        cat.CategoryName = item.Key.CategoryName;
                        cat.Id = item.Key.Id;
                        var select = peopleAcceptance.Where(x => x.Id == cat.Id && x.CategoryName == cat.CategoryName).FirstOrDefault();
                        cat.Version = select.Version;
                        cat.FilePath = select.FilePath;
                        cat.IsAccept = select.IsAccept;
                        cat.Sequence = select.Sequence;
                        cat.orangeBookSubCategory = item.Where(y => y.subcategoryid > 0).Select(x => new OBSubVersionList
                        {
                            Id = x.subcategoryid,
                            CategoryID = item.Key.Id,
                            IsAccept = x.IsAccept,
                            SubCategoryName = x.SubCategoryName,
                            Version = x.Version,
                            FilePath = x.FilePath
                        }).ToList();

                        OBVersionLists.Add(cat);
                    }

                    return Ok(OBVersionLists);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        [Route("GetOBVersionHistory")]
        [HttpGet]
        public dynamic GetOBVersionHistory(int catid, int subcatid)
        {

            using (var con = new AuthContext())
            {
                try
                {
                    var OBVersionList = new List<OrangeBook>();
                    if (catid > 0 && subcatid == 0)
                    {
                        OBVersionList = con.OrangeBookDB.Where(x => x.CategoryId == catid).OrderByDescending(x => x.CreatedDate).ToList();
                    }

                    if (catid > 0 && subcatid > 0)
                    {
                        OBVersionList = con.OrangeBookDB.Where(x => x.CategoryId == catid && x.SubCategoryId == subcatid).OrderByDescending(x => x.CreatedDate).ToList();
                    }

                    return Ok(OBVersionList);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
    }

    public class OBFiles
    {
        public int catsequence { get; set; }
        public int subsequence { get; set; }
        public string FilePath { get; set; }
    }

    public class PeopleAcceptenace
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public int subcategoryid { get; set; }
        public string SubCategoryName { get; set; }
        public string FilePath { get; set; }
        public int Sequence { get; set; }
        public bool IsAccept { get; set; }
        public string Version { get; set; }
    }
    public class OBVersionList
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public bool IsAccept { get; set; }
        public int Sequence { get; set; }
        public string Version { get; set; }
        public string FilePath { get; set; }
        public List<OBSubVersionList> orangeBookSubCategory { get; set; }
    }

    public class OBSubVersionList
    {
        public int Id { get; set; }
        public int CategoryID { get; set; }
        public string SubCategoryName { get; set; }
        public bool IsAccept { get; set; }
        public string Version { get; set; }
        public string FilePath { get; set; }
    }

    public class OBCategory
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
    }

    public class OBSubCategory
    {
        public int Id { get; set; }
        public string SubCategoryName { get; set; }
    }
}
#endregion