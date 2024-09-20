using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helper.VehicleMasterHelper;
using AngularJSAuthentication.DataContracts.WarehouseUtilization;
using AngularJSAuthentication.Model.DeliveryOptimization.FleetMaster;
using AngularJSAuthentication.Model.FleetMaster;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.VehicleMaster
{
    [RoutePrefix("api/FleetMasters")]
    public class FleetMasterController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        #region [FleetMaster]
        [AllowAnonymous]
        [Route("AddFleetMaster")]
        [HttpPost]
        public async Task<ResFleetMasterDC> AddFleetMaster(FleetMasterDC AddFleetMasterdc)
        {
            ResFleetMasterDC res = new ResFleetMasterDC();
            var Message = "";
            int userid = 0;
            FleetMasterHelper fleetMasterHelper = new FleetMasterHelper();
            userid = GetUserId();
            using (var context = new AuthContext())
            {
                var IsDuplicateTransportName = context.FleetMasterDB.Where(x => x.TransportName == AddFleetMasterdc.TransportName && x.CityId == AddFleetMasterdc.CityId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (IsDuplicateTransportName != null)
                {
                    res.msg = "Already Added Transport Name";
                    res.Result = false;
                    return res;
                }
                var IsDuplicateTransportAgentMobileNo = context.FleetMasterDB.Where(x => x.TransportAgentMobileNo == AddFleetMasterdc.TransportAgentMobileNo && x.CityId == AddFleetMasterdc.CityId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (IsDuplicateTransportAgentMobileNo != null)
                {
                    res.msg = "Already Added Transport Agent Mobile No.";
                    res.Result = false;
                    return res;
                }

                bool result = fleetMasterHelper.AddnewFleetMaster(AddFleetMasterdc, userid);
                if (result)
                {
                    res.msg = "Fleet Master Added";
                    res.Result = true;
                }
                else
                {
                    res.msg = "Fleet Master not Added";
                    res.Result = false;
                }
            }
            return res;
        }

        [AllowAnonymous]
        [Route("UpdateFleetMaster/V7")]
        [HttpPost]
        public async Task<ResFleetMasterDC> UpdateFleetMasterAsync(FleetMasterDC EditFleetMasterdc)
        {
            var Message = "";
            int userid = 0;
            ResFleetMasterDC res = new ResFleetMasterDC();
            FleetMasterHelper fleetMasterHelper = new FleetMasterHelper();
            userid = GetUserId();

            bool result = fleetMasterHelper.UpdateFleetMaster(EditFleetMasterdc, userid);
            if (result)
            {
                res.msg = "Fleet Master Update!!";
                res.Result = true;
            }
            else
            {

                res.msg = "Fleet Master not Update!!";
                res.Result = false;
            }
            return res;
        }


        [AllowAnonymous]
        [Route("GetFleetMasterList")]
        [HttpGet]
        public HttpResponseMessage GetFleetMasterList(int skip, int take)
        {

            FleetMasterHelper fleetMasterHelper = new FleetMasterHelper();
            FleetMasterPaggingDC result = new FleetMasterPaggingDC();
            result = fleetMasterHelper.getAllListFleetMaster(skip, take);

            if (result != null)
            {
                var response = new
                {
                    Status = true,
                    data = result,
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            else
            {
                var response = new
                {
                    Status = false,
                    data = "No Data found"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }
        [AllowAnonymous]
        [Route("ExportFleetMasterList")]
        [HttpGet]
        public AllDatas ExportFleetMasterList(DateTime? startDate, DateTime? endDate, string search, int statusValue, int Cityid, int skip, int take)
        {
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                AllDatas data = new AllDatas();
                FleetMasterHelper fleetMasterHelper = new FleetMasterHelper();
                List<FleetMasterExportDC> result = new List<FleetMasterExportDC>();
                data = fleetMasterHelper.getAllListFleetMaster(startDate, endDate, search, statusValue, Cityid, skip, take, userid);

                if (data != null)
                {

                    return data;
                }
                else
                {
                    return null;
                }
            }
        }
        [AllowAnonymous]
        [Route("FleetMasterbyId")]
        [HttpGet]
        public HttpResponseMessage FleetMasterbyId(long Id)
        {
            try
            {
                FleetMasterHelper fleetMasterHelper = new FleetMasterHelper();
                FleetMasterDC result = new FleetMasterDC();
                result = fleetMasterHelper.FleetMasterbyId(Id);
                if (result != null)
                {
                    var response = new
                    {
                        Status = true,
                        data = result,
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        data = "No Data found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    data = "No Data found"
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }
        }
        private int GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }
        #endregion

        [Route("FleetActiveDeactiveList")]
        [HttpGet]
        [AllowAnonymous]
        public bool FleetActiveDeactiveList(int Id, bool IsActive)
        {
            using (var db = new AuthContext())
            {
                var list = db.FleetMasterDB.Where(x => x.Id == Id).FirstOrDefault();
                if (list != null)
                {
                    list.IsActive = IsActive;
                    db.Entry(list).State = EntityState.Modified;
                    db.Commit();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        [Route("FleetBlockById")]
        [HttpGet]
        [AllowAnonymous]
        public bool FleetBlockById(int Id, bool IsBlocked)
        {
            using (var db = new AuthContext())
            {
                var list = db.FleetMasterDB.Where(x => x.Id == Id).FirstOrDefault();
                if (list != null)
                {
                    list.IsBlocked = IsBlocked;
                    db.Entry(list).State = EntityState.Modified;
                    db.Commit();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        [Route("DocumentImageUpload")]
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult DocumentImageUpload()
        {
            string LogoUrl = "";
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {

                        if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath("~/FleetDoc")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/FleetDoc"));

                        string extension = Path.GetExtension(httpPostedFile.FileName);
                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/FleetDoc"), fileName);

                        httpPostedFile.SaveAs(LogoUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/FleetDoc ", LogoUrl);

                        LogoUrl = "/FleetDoc/" + fileName;

                    }

                }
                return Created<string>(LogoUrl, LogoUrl);
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        [Route("DeleteFleetAccountDeatilsById")]
        [HttpGet]
        public bool DeleteFleetAccountDeatils(int Id)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var db = new AuthContext())
            {
                var list = db.fleetMasterAccountDetailDB.Where(x => x.Id == Id).FirstOrDefault();
                if(list != null)
                {
                    list.IsActive = false;
                    list.IsDeleted = true;
                    list.ModifiedBy = userid;
                    list.ModifiedDate = DateTime.Now;
                    db.Entry(list).State = EntityState.Modified;
                    db.Commit();
                    return true;
                }
                else 
                {
                    return false;
                }

            } 
        }

        [Route("ApprovedByAccount")]
        [HttpGet]
        public async Task<TransporterResponseDc> ApprovedByAccount(long FleetMasterId , bool Isapprove)
        {
            TransporterResponseDc res;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            if (!roleNames.Any(x => x == "Accounts executive" || x == "Accounts associates" || x == "HQ Master login"))
            {
                res = new TransporterResponseDc
                {
                    Status = false,
                    Message = "User not have Accounts executive or Accounts associates!! "
                };
                return res;
            }
            using (var db = new AuthContext())
            {
                var list = db.FleetMasterDB.Where(x => x.Id == FleetMasterId).FirstOrDefault();
                if (list != null)
                {
                    list.IsAppprovedByAccount = Isapprove;                 
                    list.ModifiedBy = userid;
                    list.ModifiedDate = DateTime.Now;
                    db.Entry(list).State = EntityState.Modified;
                    db.Commit();
                    res = new TransporterResponseDc
                    {
                        Status = true,
                        Message = Isapprove == true ? "Approved SuccessFully!! " : "DisApprove SuccessFully!!"
                    };
                    return res;
                }
                else
                {
                    res = new TransporterResponseDc
                    {
                        Status = false,
                        Message = "Data Not Found!! "
                    };
                    return res;
                }

            }
        }
    }
    public class FleetMasterDC
    {
        public int Id { get; set; }
        public string FleetType { get; set; }
        public string Channel { get; set; }
        public string OperatedBy { get; set; }
        public string TransportName { get; set; }
        public string TransportAgentName { get; set; }
        public double FreightDiscount { get; set; }
        public double TollAmt { get; set; }
        public DateTime ContractStart { get; set; }
        public DateTime ContractEnd { get; set; }
        public bool IsActive { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public bool IsBlocked { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string TransportAgentMobileNo { get; set; }
        public List<fleetMasterDetailDC> fleetMasterDetails { get; set; }
        public List<FleetMasterAccountDetailDC> fleetMasterAccountDetailDc { get; set; }
        public double TollAmount { get; set; }
        public double OtherExpense { get; set; }
        public string PanNo { get; set; }
        public string PanImagePath { get; set; }
        public string AadharNo { get; set; }
        public string AadharImagePath { get; set; }
        public string GSTIN { get; set; }
        public string Address { get; set; }
        public int TransporterCityId { get; set; }
        public int TransporterStateId { get; set; }
        public bool IsAppprovedByAccount { get; set; }
        public bool? IsMSME { get; set; }
        public string MSMECertificatePath { get; set; }
        public string AgreementPath { get; set; }
    }
    public class AllDatas
    {
        public int totalcount { get; set; }
        public List<FleetMasterDC> FleetMasterDCList { get; set; }

    }
    public class fleetMasterDetailDC
    {
        public int Id { get; set; }
        public long FleetMasterId { get; set; }
        public string VehicleType { get; set; }
        public string NoOfVehicle { get; set; }
        public double FixedCost { get; set; }
        public double ExtraKmCharge { get; set; }
        public double ExtraHrCharge { get; set; }
        public double WaitingCharge { get; set; }
        public double VehicleCapacity { get; set; }
        public string Make { get; set; }
        public double NonworkingDayAmt { get; set; }
        public double DistanceContract { get; set; }
        public int DaysContract { get; set; }
        public double HrContract { get; set; }
    }
    public class FleetMasterExportDC
    {
        public int Id { get; set; }
        public string FleetType { get; set; }
        public string Channel { get; set; }
        public string OperatedBy { get; set; }
        public string TransportName { get; set; }
        public string TransportAgentName { get; set; }
        public double FreightDiscount { get; set; }
        public double TollAmt { get; set; }
        public DateTime ContractStart { get; set; }
        public DateTime ContractEnd { get; set; }
        public bool IsActive { get; set; }
        public bool IsBlocked { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string TransportAgentMobileNo { get; set; }
    }
    public class FleetMasterPaggingDC
    {
        public int totalcount { get; set; }
        public List<FleetMasterDC> FleetMasterList { get; set; }
    }
    public class ResFleetMasterDC
    {
        public int totalCount { get; set; }
        public List<FleetMasterDC> FleetMasterList { get; set; }
        public bool Result { get; set; }
        public string msg { get; set; }
    }
    public class FleetMasterAccountDetailDC
    {
        public long Id { get; set; }
        public long FleetMasterId { get; set; }
        public string AccountNo { get; set; }
        public string IFSCcode { get; set; }
        public string BranchName { get; set; }
        public string BankName { get; set; }
        public string HolderName { get; set; }
        public string CancelledChequePath { get; set; }
    }
}
