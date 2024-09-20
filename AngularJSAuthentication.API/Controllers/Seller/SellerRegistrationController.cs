using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using AngularJSAuthentication.API.ControllerV7;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Seller;
using ClosedXML.Excel;
using NLog;


namespace AngularJSAuthentication.API.Controllers.Seller
{

    [RoutePrefix("api/SellerRegistration")]
    [AllowAnonymous]
    public class SellerRegistrationController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("SellerReg")]
        [HttpPost]
        public HttpResponseMessage SellerReg(SellerRegistration seller)
        {
            SellerRegistrationRes res = new SellerRegistrationRes();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (seller != null)
                    {
                        if (seller.GSTNo == null)
                        {
                            res = new SellerRegistrationRes
                            {
                                message = "Required GST No"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        bool CheckGSTNoMobNo = context.SellerRegistrations.Any(x => x.GSTNo == seller.GSTNo || x.MobileNo == seller.MobileNo && x.IsDeleted == false);
                        if (CheckGSTNoMobNo)
                        {
                            res = new SellerRegistrationRes
                            {
                                message = "User Allready Exist"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        //People people = new People();
                        //people.Mobile = seller.MobileNo;
                        //people.Cityid = seller.CityId;
                        //people.IfscCode = seller.IFSCCode;
                        //people.Account_Number = Convert.ToInt32(seller.AccountNo);
                        //context.Peoples.Add(people);

                        seller.CreatedBy = userid;
                        seller.CreatedDate = DateTime.Now;
                        seller.IsActive = false;
                        seller.IsDeleted = false;

                        context.SellerRegistrations.Add(seller);
                        context.Commit();

                        res = new SellerRegistrationRes
                        {
                            message = "Registration Successfully"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        res = new SellerRegistrationRes
                        {
                            message = "Error"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
            }
            catch (Exception ex)
            {
                res = new SellerRegistrationRes
                {
                    message = ex.ToString()
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        [HttpPost]
        [Route("UploadSellerDoc")]
        public IHttpActionResult UploadSellerDoc()
        {
            string LogoUrl = "";
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                if (httpPostedFile != null)
                {
                    string extension = Path.GetExtension(httpPostedFile.FileName);
                    string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                    if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/images/AgentPitch/image")))
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/images/AgentPitch/image"));

                    LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/AgentPitch/image"), fileName);
                    httpPostedFile.SaveAs(LogoUrl);
                    AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/images/AgentPitch/image", LogoUrl);
                    LogoUrl = "images/AgentPitch/image/" + fileName;
                }
            }
            return Created<string>(LogoUrl, LogoUrl);
        }

        [Route("GetSellerRegList")]
        [HttpGet]
        public IEnumerable<SellerRegistration> GetSellerRegistration()
        {
            try
            {
                using (var context = new AuthContext())
                {
                    List<SellerRegistration> SellerList = new List<SellerRegistration>();
                    SellerList = context.SellerRegistrations.Where(x => x.IsDeleted == false).ToList();
                    if (SellerList != null && !SellerList.Any())
                    {
                        return SellerList;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [Route("PaggingSellerReg")]
        [HttpPost]
        public PaggingSellerRegistration Get(int list, int page, string status)
        {
            logger.Info("start SellerRegistration: ");
            try
            {
                using (var context = new AuthContext())
                {
                    PaggingSellerRegistration obj = new PaggingSellerRegistration();
                    //var SellerPagedList = context.AllSellerRegForPaging(list, page, status);
                    if (status == "true")
                    {
                        obj.total_count = context.SellerRegistrations.Where(x => x.IsDeleted == false && x.IsActive == true).Count();
                        obj.SellarRegister = context.SellerRegistrations.AsEnumerable().Where(x => x.IsDeleted == false && x.IsActive == true).OrderByDescending(s => s.Id).Skip((page - 1) * list).Take(list).ToList();
                    }
                    else if (status == "false")
                    {
                        obj.total_count = context.SellerRegistrations.Where(x => x.IsDeleted == false && x.IsActive == false).Count();
                        obj.SellarRegister = context.SellerRegistrations.AsEnumerable().Where(x => x.IsDeleted == false && x.IsActive == false).OrderByDescending(s => s.Id).Skip((page - 1) * list).Take(list).ToList();
                    }
                    else
                    {
                        obj.total_count = context.SellerRegistrations.Where(x => x.IsDeleted == false).Count();
                        obj.SellarRegister = context.SellerRegistrations.AsEnumerable().Where(x => x.IsDeleted == false).OrderByDescending(s => s.Id).Skip((page - 1) * list).Take(list).ToList();
                    }
                    return obj;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [Route("SellerLogin/{MobileNo}")]
        [HttpGet]
        [AllowAnonymous]
        public SellerRegOTP SellerLogin(string MobileNo)
        {
            try
            {
                if (MobileNo != null)
                {
                    OTPMaster obj = new OTPMaster();
                    using (AuthContext context = new AuthContext())
                    {
                        bool IsExist = context.Peoples.Any(x => x.Mobile == MobileNo && x.Deleted == false && x.Active == true);

                        if (IsExist)
                        {
                            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                            string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);
                            // string OtpMessage = " : is Your Verification Code. :).ShopKirana";
                            string OtpMessage = ""; //"{#var1#} : is Your Verification Code. {#var2#}.ShopKirana";
                            var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.SalesApp, "Customer_Verification_Code");
                            OtpMessage = dltSMS == null ? "" : dltSMS.Template;
                           
                            OtpMessage = OtpMessage.Replace("{#var1#}", sRandomOTP);
                            OtpMessage = OtpMessage.Replace("{#var2#}", ":)");

                           // string message = sRandomOTP + " :" + OtpMessage;
                           if(dltSMS!=null)
                            Common.Helpers.SendSMSHelper.SendSMS(MobileNo, OtpMessage, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);

                            obj.MobileNo = MobileNo;
                            obj.OTPno = Convert.ToInt32(sRandomOTP);
                            obj.IsActive = true;
                            obj.IsDeleted = false;
                            obj.IsVerified = false;
                            obj.CreatedDate = DateTime.Now;

                            context.OTPMasters.Add(obj);
                            context.Commit();

                            SellerRegOTP a = new SellerRegOTP()
                            {
                                OtpNo = sRandomOTP
                            };
                            return a;
                        }
                        else
                        {
                            SellerRegOTP b = new SellerRegOTP()
                            {
                                OtpNo = "You are not authorize"
                            };
                            return b;
                        }
                    }
                }
                else
                {
                    SellerRegOTP b = new SellerRegOTP()
                    {
                        OtpNo = "Mobile No. Required"
                    };
                    return b;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return null;
            }
        }

        [Route("OTPVerification")]
        [HttpGet]
        public SellerRegistrationRes OTPVerification(string MobileNo, int OTP)
        {
            try
            {
                //SellerRegistration seller = new SellerRegistration();
                SellerRegistrationRes seller = new SellerRegistrationRes();

                if (MobileNo != null && OTP > 0)
                {
                    using (AuthContext context = new AuthContext())
                    {
                        bool IsVerify = context.OTPMasters.Any(x => x.MobileNo.ToString() == MobileNo && x.OTPno == OTP && x.IsDeleted == false && x.IsActive == true);
                        if (IsVerify)
                        {
                            //SellerRegistration person = context.SellerRegistrations.Where(x => x.MobileNo == MobileNo && x.IsDeleted == false && x.IsActive == true).FirstOrDefault();
                            People person = context.Peoples.Where(x => x.Mobile == MobileNo && x.Deleted == false && x.Active == true).FirstOrDefault();
                            if (person != null)
                            {
                                OTPMaster obj = context.OTPMasters.Where(a => a.MobileNo == MobileNo && a.OTPno == OTP && a.IsActive == true && a.IsDeleted == false).FirstOrDefault();
                                seller.UserName = person.UserName;
                                seller.Passward = person.Password;
                                seller.IsVerified = true;

                                //ClaimsIdentity identity = await user.GenerateUserIdentityAsync(userManager, "JWT");
                                //identity.AddClaims(ExtendedClaimsProvider.GetClaims(user));
                                //identity.AddClaims(RolesFromClaims.CreateRolesBasedOnClaims(identity));
                                //identity.AddClaim(new Claim("userid", p.PeopleID.ToString()));
                                //var props = new AuthenticationProperties(new Dictionary<string, string> { });
                                //props = new AuthenticationProperties(new Dictionary<string, string>
                                //{
                                //    { "as:client_id", (context.ClientId == null) ? string.Empty : context.ClientId },
                                //    { "userName", context.UserName },
                                //    { "UserType", userType.ToString()},
                                //    { "userid", p.PeopleID.ToString() },

                                //});
                                //var ticket = new AuthenticationTicket(identity, props);
                                //context.Validated(ticket);

                                obj.IsVerified = true;
                                obj.IsActive = false;
                                obj.IsDeleted = false;
                                obj.UpdateDate = DateTime.Now;
                                context.Entry(obj).State = EntityState.Modified;
                                context.Commit();
                            }
                            return seller;
                        }
                        else
                        {
                            seller.IsVerified = false;
                            seller.message = "Mobile No. Not Exist";
                            return seller;
                        }
                    }
                }
                return seller;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #region Generate Random OTP
        private string GenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)
        {
            using (AuthContext db = new AuthContext())
            {
                string sOTP = String.Empty;
                string sTempChars = String.Empty;
                Random rand = new Random();

                for (int i = 0; i < iOTPLength; i++)
                {
                    int p = rand.Next(0, saAllowedCharacters.Length);
                    sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
                    sOTP += sTempChars;
                }
                return sOTP;
            }
        }
        #endregion

        [Route("uploadExcel")]
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult uploadExcel()
        {
            try
            {
                bool flag = false;
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                using (AuthContext context = new AuthContext())
                {
                    if (HttpContext.Current.Request.Files.Count > 0)
                    {
                        var SubCatId = HttpContext.Current.Request.Form["SubCatId"];
                        var httpPostedFile = HttpContext.Current.Request.Files["xlsfile"];

                        if (httpPostedFile != null)
                        {
                            string path = HttpContext.Current.Server.MapPath("~/UploadedFiles/CFR/");
                            string a1, b;
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                            a1 = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + httpPostedFile.FileName;
                            b = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles/CFR/"), a1);
                            httpPostedFile.SaveAs(b);
                            AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/UploadedFiles/CFR/", b);

                            DataTable dt = new DataTable();
                            using (XLWorkbook workBook = new XLWorkbook(b))
                            {
                                IXLWorksheet workSheet = workBook.Worksheet(1);
                                bool firstRow = true;
                                foreach (IXLRow row in workSheet.Rows())
                                {
                                    if (firstRow)
                                    {
                                        foreach (IXLCell cell in row.Cells())
                                        {
                                            dt.Columns.Add(cell.Value.ToString());
                                        }
                                        firstRow = false;
                                    }
                                    else
                                    {
                                        dt.Rows.Add();
                                        int i = 0;
                                        foreach (IXLCell cell in row.Cells())
                                        {
                                            dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
                                            i++;
                                        }
                                    }
                                }
                                DataTable dt1 = new DataTable();

                                dt1.Columns.Add("ItemNumber", typeof(string));
                                DataRow r = dt1.NewRow();
                                r = dt1.NewRow();
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    r = dt1.NewRow();
                                    r["ItemNumber"] = dt.Rows[i]["ItemNumber"];
                                    dt1.Rows.Add(r);
                                }
                                List<uploaditemlistDTO> uploaditemlist = new List<uploaditemlistDTO>();
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    uploaditemlistDTO obj = new uploaditemlistDTO();
                                    obj.ItemNumber = dt.Rows[i]["ItemNumber"].ToString();
                                    obj.SubCatId = Convert.ToInt32(SubCatId);
                                    uploaditemlist.Add(obj);
                                }
                                UploadCfrArticleController controller = new UploadCfrArticleController();
                                flag = controller.UploadCFr(uploaditemlist, userid, context);
                                return Ok(flag);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Ok();
        }

        //[HttpPost]
        //public ActionResult OfflinePosting(HttpPostedFileBase postedFile)
        //{
        //    try
        //    {
        //        if (postedFile.FileName.EndsWith(".xlsx"))
        //        {
        //            PL_OfflinePosting plPost = new PL_OfflinePosting();
        //            APIResponse myres = new APIResponse();
        //            if (postedFile != null)
        //            {
        //                string path = Server.MapPath("~/Uploads/");
        //                string a1, b;
        //                if (!Directory.Exists(path))
        //                {
        //                    Directory.CreateDirectory(path);
        //                }
        //                a1 = Path.GetFileName(postedFile.FileName);
        //                b = Path.Combine(Server.MapPath("~/Uploads/"), a1);
        //                // b = postedFile.FileName;
        //                postedFile.SaveAs(b);
        //                using (XLWorkbook workBook = new XLWorkbook(b))
        //                {
        //                    IXLWorksheet workSheet = workBook.Worksheet(1);
        //                    DataTable dt = new DataTable();
        //                    bool firstRow = true;
        //                    foreach (IXLRow row in workSheet.Rows())
        //                    {
        //                        if (firstRow)
        //                        {
        //                            foreach (IXLCell cell in row.Cells())
        //                            {
        //                                dt.Columns.Add(cell.Value.ToString());
        //                            }
        //                            firstRow = false;
        //                        }
        //                        else
        //                        {
        //                            dt.Rows.Add();
        //                            int i = 0;
        //                            foreach (IXLCell cell in row.Cells())
        //                            {
        //                                dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
        //                                i++;
        //                            }
        //                        }
        //                    }
        //                    DataTable dt1 = new DataTable();

        //                    dt1.Columns.Add("ItemNumber", typeof(string));
        //                    DataRow r = dt1.NewRow();
        //                    r = dt1.NewRow();
        //                    for (int i = 0; i < dt.Rows.Count; i++)
        //                    {
        //                        r = dt1.NewRow();
        //                        r["ItemNumber"] = dt.Rows[i]["ItemNumber"];
        //                        dt1.Rows.Add(r);
        //                    }
        //                    plPost.Ind = 1;
        //                    plPost.XlDt = dt1;

        //                }

        //            }
        //        }
        //        else
        //        {

        //            return ok();
        //        }
        //    }
        //    catch (Exception ex)
        //    {


        //        //return RedirectToAction("UserLogin", "Authentication");
        //    }


        //    return ok();
        //}
    }
}
public class SellerRegistrationRes
{
    public string UserName { get; set; }
    public string Passward { get; set; }
    public string message { get; set; }
    public bool IsVerified { get; set; }
}
public class PaggingSellerRegistration
{
    public int total_count { get; set; }
    public dynamic SellarRegister { get; set; }
}
public class SellerRegOTP
{
    public string OtpNo { get; set; }
}

