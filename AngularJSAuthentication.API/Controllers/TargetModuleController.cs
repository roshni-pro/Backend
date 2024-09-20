using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.NotMapped;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using GenricEcommers.Models;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using SqlBulkTools;
using static AngularJSAuthentication.DataContracts.Mongo.CustomersTargets;
using NPOI.XSSF.UserModel;
using System.Runtime.Serialization.Formatters.Binary;
using NPOI.SS.UserModel;
using System.Web.Script.Serialization;
using Nito.AsyncEx;
using AngularJSAuthentication.BusinessLayer.Helpers.ElasticDataHelper;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.API.Controllers.External.SalesManApp;
using MongoDB.Bson.Serialization.Attributes;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using LinqKit;
using AgileObjects.AgileMapper;
using AngularJSAuthentication.DataContracts.External.MobileExecutiveDC;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/TargetModule")]
    public class TargetModuleController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public double xPointValue = AppConstants.xPoint;
        readonly string platformIdxName = $"skorderdata_{AppConstants.Environment}";

        [Route("ExecuteVirtualEnviornment")]
        [HttpGet]
        public HttpResponseMessage ExecuteVirtualEnviornment()
        {
            try
            {
                var mlexepath = Startup.MLExePath;
                int nProcessID = Process.Start(mlexepath).Id;
                System.Threading.Thread.Sleep(3000);
                var responce = "Virtual enviornment successfully running";
                return Request.CreateResponse(HttpStatusCode.OK, responce);
            }
            catch (Exception ex)
            {
                var responce = ex.Message;
                return Request.CreateResponse(HttpStatusCode.OK, responce);
            }
        }

        [Route("KillVirtualEnviornment")]
        [HttpGet]
        public HttpResponseMessage KillVirtualEnviornment()
        {
            try
            {
                foreach (Process proc in Process.GetProcessesByName("budget"))
                {
                    proc.Kill();
                }

                var responce = "Virtual enviornment successfully Terminated";
                return Request.CreateResponse(HttpStatusCode.OK, responce);
            }
            catch (Exception ex)
            {
                var responce = ex.Message;
                return Request.CreateResponse(HttpStatusCode.OK, responce);
            }
        }

        [Route("CreateMonthlyLevels")]
        [HttpGet]
        public HttpResponseMessage CreateMonthlyLevels()
        {
            try
            {
                using (var con = new AuthContext())
                {
                    string responce = "";
                    var predate = DateTime.Now.AddMonths(-1);
                    var getlevel = con.LevelMasterDB.Where(x => x.CreatedDate.Month == predate.Month && x.CreatedDate.Year == predate.Year && x.WarehouseId == 1).ToList();
                    if (!con.LevelMasterDB.Any(x => x.CreatedDate.Month == indianTime.Month && x.CreatedDate.Year == indianTime.Year && x.WarehouseId == 1))
                    {
                        foreach (var item in getlevel)
                        {
                            var addlevel = new LevelMaster();
                            addlevel = item;
                            addlevel.CreatedDate = indianTime;
                            con.LevelMasterDB.Add(addlevel);
                        }
                        con.Commit();
                        responce = "Levels Sucessfully Created";
                    }
                    else
                        responce = "Levels already Created for current month";
                    return Request.CreateResponse(HttpStatusCode.OK, responce);
                }

            }
            catch (Exception ex)
            {
                var responce = ex.Message;
                return Request.CreateResponse(HttpStatusCode.OK, responce);
            }
        }

        [Route("MonthlyTMData")]
        [HttpGet]
        public HttpResponseMessage MonthlyTMData(int month, int year)
        {
            var MonthlyTMData = new List<MonthlyTargetData>();
            using (var con = new AuthContext())
            {
                var date = new DateTime(year, month, 1);
                try
                {
                    var MTData = con.LevelMasterDB.Where(x => x.IsActive == true && x.IsDeleted == false && x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year).Select(x => new { x.Id, x.Name, x.Type, x.value }).ToList();
                    foreach (var item in MTData)
                    {
                        var bandsdata = con.CustomerBandsDB.Where(x => x.IsActive == true && x.IsDeleted == false && x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year && x.LevelId == item.Id).ToList();
                        foreach (var ban in bandsdata)
                        {
                            var data = new MonthlyTargetData();
                            data.Id = item.Id;
                            data.LevelName = item.Name;
                            data.Type = item.Type == true ? "Value" : "Percent";
                            data.Value = item.value;
                            data.CreatedDate = ban.CreatedDate;
                            data.BandName = ban.BandName;
                            data.LowerLimit = ban.LowerLimit;
                            data.UpperLimit = ban.UpperLimit;
                            MonthlyTMData.Add(data);
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, MonthlyTMData);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, MonthlyTMData);
                    throw;
                }
            }
        }

        [Route("GetLevel")]
        [HttpGet]
        public HttpResponseMessage GetLevel(int WarehouseId)
        {
            using (var con = new AuthContext())
            {
                var date = DateTime.Now;
                var GetLevel = con.LevelMasterDB.Where(x => x.IsActive == true && x.IsDeleted == false && x.WarehouseId == 1
                && x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year
                ).Select(x => new level { Id = x.Id, LevelName = x.Name, Value = x.value, Type = x.Type == true ? "Value" : "Percent" }).ToList();
                if (GetLevel != null && GetLevel.Any())
                    GetLevel[0].IsSelected = true;
                var response = new
                {
                    LevelData = GetLevel,
                    Status = true,
                    Message = "Level Data List"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        [Route("GetCustomerTarget")]
        [HttpGet]
        public HttpResponseMessage GetCustomerTarget(int WarehouseId)
        {
            using (var con = new AuthContext())
            {
                var currentmonth = DateTime.Now;

                var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
                string DocumentName = "MonthlyTargetData_" + currentmonth.Month.ToString() + currentmonth.Year.ToString();
                var Targets = MonthlyCustomerTarget.Select(x => x.WarehouseId == WarehouseId, null, null, null, false, "", DocumentName).ToList();

                var warehousename = con.Warehouses.Where(x => x.WarehouseId == WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();

                if (Targets != null)
                {
                    var GetCustomerTarget = Targets.Select(x => new
                    {
                        Skcode = x.Skcode,
                        WarehouseId = warehousename,
                        Volumn = x.Volume,
                        Levels = x.Levels,
                        Bands = x.Bands,
                        Target = x.Target,
                        CurrentVolume = x.CurrentVolume,
                        CreatedDate = x.CreatedDate,
                        IsClaimed = x.IsClaimed
                    }).ToList();


                    var response = new
                    {
                        GetCustomerTarget = GetCustomerTarget,
                        Status = true,
                        Message = "Customer Target Data List"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Customer Target Not Found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
        }


        [Route("GetCustomerTargetMonthWise")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetCustomerTargetMonthWise(int month, int year)
        {
            using (var con = new AuthContext())
            {
                var currentmonth = DateTime.Now;

                var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
                string DocumentName = "MonthlyTargetData_" + month + year;
                var Target = MonthlyCustomerTarget.Select(x => x.WarehouseId > -1, null, null, null, false, "", DocumentName).ToList();

                // var warehouseid = con.Warehouses.Where(x => x.IsKPP==false && x.Deleted==false).Select(x => x.WarehouseId).ToList();

                if (Target != null)
                {

                    var GetCustomerTarget = Target.Select(x => new
                    {
                        Skcode = x.Skcode,
                        WarehouseId = con.Warehouses.Where(y => y.WarehouseId == x.WarehouseId).Select(y => y.WarehouseName).FirstOrDefault(),
                        Volumn = x.Volume,
                        CityName = x.CityName,
                        Levels = x.Levels,
                        Bands = x.Bands,
                        TargetValue = x.Target,
                        TargetLineItem = x.TargetLineItem,
                        CurrentVolume = x.CurrentVolume,
                        CurrentLineItem = x.CurrentLineItem,
                        CreatedDate = x.CreatedDate,
                        IsClaimed = x.IsClaimed
                    }).ToList();


                    var response = new
                    {
                        GetCustomerTarget = GetCustomerTarget,
                        Status = true,
                        Message = "Customer Target Data List"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Customer Target Not Found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
        }

        [Route("Searchtarge")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Searchtarge(string skocde)
        {
            using (var con = new AuthContext())
            {
                var currentmonths = DateTime.Now;

                var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
                string DocumentName = "MonthlyTargetData_" + currentmonths.Month.ToString() + currentmonths.Year.ToString();
                var targetsearch = MonthlyCustomerTarget.Select(x => x.Skcode == skocde, null, null, null, false, "", DocumentName).Select(z => new { z.CurrentVolume, z.Target }).ToList();
                //var targetsearch = Target.Where(y => y.Skcode == skocde).Select(z => new { z.CurrentVolume, z.Target }).ToList();
                //var targetsearch = con.CustomersTargetDB.Where(x => x.Skcode == skocde && x.CreatedDate.Month== currentmonths.Month && x.CreatedDate.Year== currentmonths.Year).Select(y=> new { y.CurrentVolume,y.Target}).SingleOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, targetsearch);

            }
        }
        [Route("GetAllLevelBands")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetAllLevelBands(int month, int year)
        {
            using (var con = new AuthContext())
            {
                var date = DateTime.Now;
                bool AddPermission = false;
                var GetLevelBands = new List<GetLevelBandsDTO>();
                var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
                string DocumentName = "MonthlyTargetData_" + month + year;
                List<MonthlyCustomerTarget> Target = MonthlyCustomerTarget.Select(x => x.WarehouseId > -1, null, null, null, false, "", DocumentName).ToList();
                if (Target != null)
                {
                    var MTData = con.LevelMasterDB.Where(x => x.IsActive == true && x.IsDeleted == false && x.CreatedDate.Month == month && x.CreatedDate.Year == year && x.WarehouseId == 1).Select(x => new { x.Id, x.Name, x.Type, x.value }).ToList();
                    foreach (var item in MTData)
                    {
                        var bandsdata = con.CustomerBandsDB.Where(x => x.IsActive == true && x.IsDeleted == false && x.CreatedDate.Month == month && x.CreatedDate.Year == year && x.LevelId == item.Id).ToList();
                        foreach (var ban in bandsdata)
                        {
                            var data = new GetLevelBandsDTO();

                            data.LevelName = item.Name;
                            data.BandName = ban.BandName;
                            data.BandLowerLimit = ban.LowerLimit;
                            data.BandUpperLimit = ban.UpperLimit;
                            data.ByValue = item.value;
                            data.Types = item.Type == true ? "Value" : "Percent";
                            data.Reward = ban.value + ban.Type;
                            if (ban.Type == "Gift")
                            {
                                data.Reward = ban.Type;
                                RewardItems it = con.RewardItemsDb.Where(x => x.rItemId == ban.GiftId).SingleOrDefault();
                                data.GiftName = it.rName != null ? it.rName : null;
                                data.GiftDescription = it.Description != null ? it.Description : null;

                            }
                            data.Createdate = ban.CreatedDate;
                            GetLevelBands.Add(data);
                        }
                    }
                    GetLevelBands.ForEach(x =>
                    { x.NoOfCustomer = NoofCustomer(Target, x.LevelName, x.BandName); });


                    if (GetLevelBands.Count > 0)
                    {
                        AddPermission = true;
                    }
                    var response = new
                    {
                        GetLevelBands = GetLevelBands,
                        AddPermission = AddPermission,
                        Status = true,
                        Message = "Level Bands Data List"
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "no record found");
                }
            }
        }


        private int NoofCustomer(List<MonthlyCustomerTarget> ObjTraget, string Name, string BrandName)
        {

            int NoOfCustomer = ObjTraget.Where(x => x.Levels == Name && x.Bands == BrandName).Count();
            return NoOfCustomer;

        }
        [Route("GetLevelBands")]
        [HttpGet]
        public HttpResponseMessage GetLevelBands(int WarehouseId, int LevelId)
        {
            using (var con = new AuthContext())
            {
                var date = DateTime.Now;
                bool AddPermission = false;
                var GetLevelBands = (from a in con.CustomerBandsDB
                                     join b in con.LevelMasterDB on a.LevelId equals b.Id
                                     where b.WarehouseId == WarehouseId && a.LevelId == LevelId && a.CreatedDate.Month == date.Month && a.CreatedDate.Year == date.Year && b.CreatedDate.Month == date.Month && b.CreatedDate.Year == date.Year && a.IsActive == true && a.IsDeleted == false
                                     select new
                                     {
                                         Id = a.Id,
                                         LevelId = b.Id,
                                         LevelName = b.Name,
                                         BandName = a.BandName,
                                         ImagePath = a.ImagePath,
                                         lowerlimit = a.LowerLimit,
                                         upperlimit = a.UpperLimit,
                                         BandType = a.Type,
                                         Value = a.value
                                     }).ToList();

                if (GetLevelBands.Count > 0)
                {
                    AddPermission = true;
                }
                var response = new
                {
                    GetLevelBands = GetLevelBands,
                    AddPermission = AddPermission,
                    Status = true,
                    Message = "Level Bands Data List"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        [Route("SaveDashboard")]
        [HttpPost]
        public HttpResponseMessage SaveDashboard(Dashboard dashboard)
        {
            using (var con = new AuthContext())
            {
                var currentdate = DateTime.Now;
                var level = con.LevelMasterDB.Where(x => x.Id == dashboard.Id).SingleOrDefault();
                if (level != null)
                {
                    level.Type = dashboard.Type == "Value" ? true : false;
                    level.value = dashboard.Value;
                    con.Commit();
                }
                var response = new
                {
                    Status = true,
                    Message = "Save Records"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        [Route("SaveCustomerBand")]
        [HttpPost]
        public HttpResponseMessage SaveCustomerBand(Bands CB)
        {
            int id = 0;
            using (var con = new AuthContext())
            {
                try
                {
                    var band = con.CustomerBandsDB.Where(x => x.Id == CB.Id).SingleOrDefault();
                    if (band != null)
                    {
                        band.UpperLimit = CB.UpperLimit;
                        band.LowerLimit = CB.LowerLimit;
                        band.Type = CB.BandType;
                        if (CB.BandType == "Point")
                        {
                            band.value = CB.Value;
                            band.ImagePath = "";
                            band.GiftId = 0;
                        }
                        else
                        {
                            band.ImagePath = CB.ImagePath;
                            band.GiftId = CB.GiftId;
                            band.value = 0;
                        }
                        band.ModifiedDate = DateTime.Now;
                        con.Commit();
                        id = band.Id;

                        var response = new
                        {
                            Id = id,
                            Status = true,
                            Message = "Save Records"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                    else
                    {
                        var check = CB; string msg = "";
                        var isadd = con.CustomerBandsDB.Where(x => x.LevelId == CB.LevelId).Count();
                        isadd++;
                        if (CB.BandName == "Band " + isadd)
                        {
                            var customerband = new CustomerBands();
                            customerband.LevelId = CB.LevelId;
                            customerband.BandName = CB.BandName;
                            customerband.Type = CB.BandType;
                            if (CB.BandType == "Point")
                            {
                                customerband.value = CB.Value;
                                customerband.ImagePath = "";
                                customerband.GiftId = 0;
                            }
                            else
                            {
                                customerband.ImagePath = CB.ImagePath;
                                customerband.value = 0;
                                customerband.GiftId = CB.GiftId;
                            }
                            customerband.UpperLimit = CB.UpperLimit;
                            customerband.LowerLimit = CB.LowerLimit;
                            customerband.IsActive = true;
                            customerband.IsDeleted = false;
                            customerband.CreatedDate = DateTime.Now;
                            con.CustomerBandsDB.Add(customerband);
                            con.Commit();
                            id = customerband.Id;

                            var response = new
                            {
                                Id = id,
                                Status = true,
                                Message = "Save Records"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, response);
                        }
                        else
                        {
                            msg = "Please add Band  " + isadd + " First";
                            var response = new
                            {
                                Status = false,
                                Message = msg
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, response);
                        }
                    }
                }

                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        [Route("SaveBudgetAllocation")]
        [HttpPost]
        public HttpResponseMessage SaveBudgetAllocation(List<TA> TarAll)
        {
            using (var con = new AuthContext())
            {
                var cm = DateTime.Now;
                var check = TarAll.FirstOrDefault();
                var ta = con.TargetAllocationDB.Where(x => x.CreatedDate.Month == cm.Month && x.CreatedDate.Year == cm.Year && x.levels == check.levels && x.WarehouseId == check.WarehouseId).ToList().Count();
                if (ta == 0)
                {
                    foreach (var item in TarAll)
                    {
                        var targetallocation = new TargetAllocation();
                        targetallocation.WarehouseId = item.WarehouseId;
                        targetallocation.SkCode = item.SkCode;
                        targetallocation.levels = item.levels;
                        targetallocation.status = item.status;
                        targetallocation.allocation = item.allocation;
                        targetallocation.used = 0;
                        targetallocation.Source = "Python";
                        targetallocation.CreatedDate = DateTime.Now;
                        con.TargetAllocationDB.Add(targetallocation);
                        con.Commit();
                    }
                    var response = new
                    {
                        Status = true,
                        Message = "Budget Allocated Sucessfully"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Budget Allready Allocated"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
        }

        [Route("GetMarkettingBudget")]
        [HttpGet]
        public HttpResponseMessage GetMarkettingBudget(int WarehouseId, string Level)
        {
            using (var con = new AuthContext())
            {
                //var date = new DateTime(year, Month, 1);
                var date = DateTime.Now;
                var GetExportData = new List<TargetAllocation>();
                if (Level != "undefined")
                {
                    GetExportData = con.TargetAllocationDB.Where(x => x.WarehouseId == WarehouseId && x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year && x.levels == Level).ToList();
                }
                else
                {
                    GetExportData = con.TargetAllocationDB.Where(x => x.WarehouseId == WarehouseId && x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year).ToList();
                }
                var response = new
                {
                    GetExportData = GetExportData,
                    Status = true,
                    Message = "Export Data List"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        [Route("GetExportData")]
        [HttpGet]
        public HttpResponseMessage GetExportData(int WarehouseId)
        {
            using (var con = new AuthContext())
            {
                var GetExportData = new List<CustomerBands>();
                var date = DateTime.Now;
                var level = con.LevelMasterDB.Where(x => x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year && x.WarehouseId == WarehouseId).Select(x => new { x.Id, x.Name }).ToList();
                foreach (var item in level)
                {
                    var bands = con.CustomerBandsDB.Where(x => x.LevelId == item.Id && x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year).ToList();
                    foreach (var band in bands)
                    {
                        GetExportData.Add(band);
                    }
                }
                //var GetExportData = con.TargetBandAllocationDB.Where(x => x.WarehouseId == WarehouseId && x.levelId == LevelId && x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year).ToList();

                var response = new
                {
                    GetExportData = GetExportData,
                    Status = true,
                    Message = "Export Data List"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        [Route("GetCustomerTargetSQL")]
        [HttpGet]
        public HttpResponseMessage GetCustomerTargetSQL(int WarehouseId, string SkCode, int customerid)
        {
            var target = new CustomerTarget();
            using (var con = new AuthContext())
            {
                var date = DateTime.Now;
                var lastDayOfMonth = DateTime.DaysInMonth(date.Year, date.Month);
                var GetExportData = con.CustomersTargetDB.Where(x => x.WarehouseId == WarehouseId && x.Skcode == SkCode && x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year).FirstOrDefault();
                if (GetExportData != null)
                {
                    var levelid = con.LevelMasterDB.Where(x => x.Name == GetExportData.Levels && x.WarehouseId == 1 && x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year).Select(x => x.Id).FirstOrDefault();
                    var image = con.CustomerBandsDB.Where(x => x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year && x.LevelId == levelid && x.BandName == GetExportData.Bands).Select(x => new { x.Type, x.value, x.ImagePath }).SingleOrDefault();
                    var totalpuramt = GetExportData.CurrentVolume;
                    target.IsClaimed = GetExportData.IsClaimed;
                    target.SKCode = GetExportData.Skcode;
                    target.TargetAmount = GetExportData.Target;
                    target.LeftDays = lastDayOfMonth - date.Day;
                    target.GiftImage = image.ImagePath != null ? image.ImagePath : null;
                    target.Type = image.Type;
                    target.Value = image.value;
                    target.TotalPurchaseAmount = Convert.ToDecimal(totalpuramt);
                    try
                    {
                        target.AchivePercent = target.TotalPurchaseAmount / (target.TargetAmount / 100);
                    }
                    catch (Exception)
                    {
                        target.AchivePercent = 0;
                    }

                    var response = new
                    {
                        GetTargetData = target,
                        Status = true,
                        Message = "Target Data"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    string[] response = null;
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
        }

        [Route("UpdateCustomerTarget")]
        [HttpGet]
        public bool updatecustomertarget()
        {
            var date = DateTime.Now;
            var target = new CustomerTarget();
            var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
            string DocumentName = "MonthlyTargetData_" + date.Month.ToString() + date.Year.ToString();
            var Targets = MonthlyCustomerTarget.Select(x => x.WarehouseId > -1, null, null, null, false, "", DocumentName).ToList();
            if (Targets != null)
            {
                Targets.Where(x => x.Levels == "Level 2").ToList().ForEach(x => x.Bands = "Band 1");
                foreach (var item in Targets.Where(x => x.Levels == "Level 2").ToList())
                {
                    item.Bands = "Band 1";
                    MonthlyCustomerTarget.Replace(item.Id, item, DocumentName);
                }

            }
            return true;
        }

        [Route("GetCustomerTarget")]
        [HttpGet]
        public HttpResponseMessage GetCustomerTarget(int WarehouseId, string SkCode, int customerid)
        {
            var target = new CustomerTarget();
            using (var con = new AuthContext())
            {
                var date = DateTime.Now;
                var isMonthComplete = false;
                if (date.Day <= 10)
                {
                    isMonthComplete = true;
                    date = DateTime.Now.AddMonths(-1);
                }
                var lastDayOfMonth = DateTime.DaysInMonth(date.Year, date.Month);

                var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
                string DocumentName = "MonthlyTargetData_" + date.Month.ToString() + date.Year.ToString();
                var GetExportData = MonthlyCustomerTarget.Select(x => x.Skcode == SkCode, null, null, null, false, "", DocumentName).FirstOrDefault();
                if (GetExportData != null)
                {

                    var levelid = con.LevelMasterDB.Where(x => x.Name == GetExportData.Levels && x.WarehouseId == 1 && x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year).Select(x => x.Id).FirstOrDefault();
                    var image = con.CustomerBandsDB.Where(x => x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year && x.LevelId == levelid && x.BandName == GetExportData.Bands).Select(x => new { x.Type, x.value, x.ImagePath }).FirstOrDefault();

                    var totalpuramt = GetExportData.CurrentVolume;
                    target.IsClaimed = GetExportData.IsClaimed;
                    target.SKCode = GetExportData.Skcode;
                    target.TargetMonth = date.ToString("MMMM");
                    target.TargetAmount = GetExportData.Target;
                    target.LeftDays = isMonthComplete ? 0 : (lastDayOfMonth - date.Day);
                    target.TargetLineItem = GetExportData.TargetLineItem.HasValue ? GetExportData.TargetLineItem.Value : 0;
                    target.CurrentLineItem = GetExportData.CurrentLineItem.HasValue ? GetExportData.CurrentLineItem.Value : 0;
                    GetExportData.TargetLineItem = GetExportData.TargetLineItem.HasValue ? GetExportData.TargetLineItem.Value : 0;
                    GetExportData.CurrentLineItem = GetExportData.CurrentLineItem.HasValue ? GetExportData.CurrentLineItem.Value : 0;
                    target.targetConditions = new List<targetCondition>();
                    target.targetConditions.Add(new targetCondition
                    {
                        ConditionText = "Shop worth Rs. " + Convert.ToInt32(target.TargetAmount).ToString(),
                        ConditionCompleteText = ConvertNumberToWord.minifyLong(Convert.ToInt32(GetExportData.CurrentVolume)) + "/" + ConvertNumberToWord.minifyLong(Convert.ToInt32(target.TargetAmount)),
                        Target = Convert.ToInt32(target.TargetAmount),
                        CurrentValue = Convert.ToInt32(GetExportData.CurrentVolume),
                        AchivePercent = (totalpuramt / (Convert.ToDecimal(target.TargetAmount) / 100)) > 100 ? 100 : (totalpuramt / (Convert.ToDecimal(target.TargetAmount) / 100)),
                        Message = (target.TargetAmount > GetExportData.CurrentVolume ? "Only Rs. " + (target.TargetAmount - GetExportData.CurrentVolume) + " away from the remaining target." : "")
                    });

                    if (target.TargetLineItem.HasValue && target.TargetLineItem.Value > 0)
                    {
                        target.targetConditions.Add(new targetCondition
                        {
                            ConditionText = "Buy " + Convert.ToInt32(target.TargetLineItem).ToString() + " Unique line item",
                            ConditionCompleteText = Convert.ToInt32(GetExportData.CurrentLineItem).ToString() + "/" + Convert.ToInt32(target.TargetLineItem).ToString(),
                            Target = GetExportData.TargetLineItem.Value,
                            CurrentValue = target.CurrentLineItem.Value,
                            AchivePercent = (target.CurrentLineItem.Value / (Convert.ToDecimal(GetExportData.TargetLineItem.Value) / 100)) > 100 ? 100 : (target.CurrentLineItem.Value / (Convert.ToDecimal(GetExportData.TargetLineItem.Value) / 100)),
                            Message = (GetExportData.TargetLineItem.Value > target.CurrentLineItem.Value ? "Buy " + (GetExportData.TargetLineItem.Value - target.CurrentLineItem.Value) + " unique line item more." : "")
                        });
                    }

                    if (!(GetExportData.IsOffer.HasValue && GetExportData.IsOffer.Value))
                    {
                        target.GiftImage = image != null ? image.ImagePath : null;
                        target.Type = image != null ? image.Type : null;
                        target.Value = image != null ? image.value : 0;
                    }
                    else
                    {
                        target.Type = "Offer";
                        target.OfferDesc = GetExportData.OfferDesc;
                        target.Value = GetExportData.OfferValue.Value;
                        target.OfferValue = GetExportData.OfferValue.Value;
                        target.OfferType = GetExportData.OfferType.Value;
                    }
                    target.Level = GetExportData.Levels;
                    target.TotalPurchaseAmount = Convert.ToDecimal(totalpuramt);
                    target.TotalPendingPurchaseAmount = Convert.ToDecimal(GetExportData.PendingVolume);
                    try
                    {
                        var amtPer = target.TotalPurchaseAmount / (target.TargetAmount / 100);
                        amtPer = amtPer > 100 ? 100 : amtPer;
                        var lineitemPer = target.TargetLineItem.HasValue && target.TargetLineItem.Value > 0 ? (target.CurrentLineItem.HasValue ? target.CurrentLineItem.Value : 0) / (Convert.ToDecimal(target.TargetLineItem.Value) / 100) : 100;
                        lineitemPer = lineitemPer > 100 ? 100 : lineitemPer;
                        target.AchivePercent = (amtPer + lineitemPer) / 2;
                        if (target.AchivePercent > 100)
                            target.AchivePercent = 100;
                    }
                    catch (Exception)
                    {
                        target.AchivePercent = 0;
                    }

                    var response = new
                    {
                        GetTargetData = target,
                        Status = true,
                        Message = "Target Data"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);


                }
                else
                {
                    string[] response = null;
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
        }


        [Route("GetCustomerTargetForSalesApp")]
        [HttpGet]
        public HttpResponseMessage GetCustomerTargetForSalesApp(int peopleId, int WarehouseId, string SkCode, int customerid)
        {
            List<CustomerTarget> targets = new List<CustomerTarget>();

            var target = new CustomerTarget();
            using (var con = new AuthContext())
            {

                var storeIds = con.ClusterStoreExecutives.Where(x => x.ExecutiveId == peopleId && x.IsActive && x.IsDeleted == false).Select(x => x.StoreId).ToList();
                var stores = con.StoreDB.Where(x => storeIds.Contains(x.Id)).Select(x => new { x.Id, x.Name, x.ImagePath }).ToList();
                var date = DateTime.Now;
                var isMonthComplete = false;
                if (date.Day <= 10)
                {
                    isMonthComplete = true;
                    date = DateTime.Now.AddMonths(-1);
                }
                var lastDayOfMonth = DateTime.DaysInMonth(date.Year, date.Month);

                var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
                string DocumentName = "MonthlyTargetData_" + date.Month.ToString() + date.Year.ToString();
                var GetExportData = MonthlyCustomerTarget.Select(x => x.Skcode == SkCode, null, null, null, false, "", DocumentName).FirstOrDefault();
                if (GetExportData != null)
                {

                    var levelid = con.LevelMasterDB.Where(x => x.Name == GetExportData.Levels && x.WarehouseId == 1 && x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year).Select(x => x.Id).FirstOrDefault();
                    var image = con.CustomerBandsDB.Where(x => x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year && x.LevelId == levelid && x.BandName == GetExportData.Bands).Select(x => new { x.Type, x.value, x.ImagePath }).FirstOrDefault();
                    target.StoreId = 0;
                    target.StoreName = "SK";
                    var totalpuramt = GetExportData.CurrentVolume;
                    target.TargetMonth = date.ToString("MMMM");
                    target.IsClaimed = GetExportData.IsClaimed;
                    target.SKCode = GetExportData.Skcode;
                    target.TargetAmount = GetExportData.Target;
                    target.LeftDays = isMonthComplete ? 0 : (lastDayOfMonth - date.Day);
                    target.TargetLineItem = GetExportData.TargetLineItem;
                    target.CurrentLineItem = GetExportData.CurrentLineItem.HasValue ? GetExportData.CurrentLineItem.Value : 0;

                    target.targetConditions = new List<targetCondition>();
                    target.targetConditions.Add(new targetCondition
                    {
                        ConditionText = "Shop worth Rs. " + Convert.ToInt32(target.TargetAmount).ToString(),
                        ConditionCompleteText = ConvertNumberToWord.minifyLong(Convert.ToInt32(GetExportData.CurrentVolume)) + "/" + ConvertNumberToWord.minifyLong(Convert.ToInt32(target.TargetAmount)),
                        Target = Convert.ToInt32(target.TargetAmount),
                        CurrentValue = Convert.ToInt32(GetExportData.CurrentVolume),
                        AchivePercent = target.TargetAmount > 0 ? ((totalpuramt / (Convert.ToDecimal(target.TargetAmount) / 100)) > 100 ? 100 : totalpuramt / (Convert.ToDecimal(target.TargetAmount) / 100)) : 100,
                        Message = (target.TargetAmount > totalpuramt ? "Only Rs. " + (target.TargetAmount - totalpuramt) + " away from the remaining target." : "")
                    });

                    if (target.TargetLineItem.HasValue && target.TargetLineItem.Value > 0)
                    {
                        target.targetConditions.Add(new targetCondition
                        {
                            ConditionText = "Buy " + Convert.ToInt32(target.TargetLineItem).ToString() + " Unique line item",
                            ConditionCompleteText = Convert.ToInt32(target.CurrentLineItem.Value).ToString() + "/" + Convert.ToInt32(target.TargetLineItem).ToString(),
                            Target = GetExportData.TargetLineItem.Value,
                            CurrentValue = target.CurrentLineItem.Value,
                            AchivePercent = target.TargetLineItem.Value > 0 ? ((target.CurrentLineItem.Value / (Convert.ToDecimal(target.TargetLineItem.Value) / 100)) > 100 ? 100 : (target.CurrentLineItem.Value / (Convert.ToDecimal(target.TargetLineItem.Value) / 100))) : 100,
                            Message = (GetExportData.TargetLineItem.Value > target.CurrentLineItem.Value ? "Buy " + (GetExportData.TargetLineItem.Value - target.CurrentLineItem.Value) + " unique line item more." : "")
                        });
                    }

                    if (!(GetExportData.IsOffer.HasValue && GetExportData.IsOffer.Value))
                    {
                        target.GiftImage = image != null ? image.ImagePath : null;
                        target.Type = image != null ? image.Type : null;
                        target.Value = image != null ? image.value : 0;
                    }
                    else
                    {
                        target.Type = "Offer";
                        target.Value = GetExportData.OfferValue.Value;
                        target.OfferDesc = GetExportData.OfferDesc;
                        target.OfferValue = GetExportData.OfferValue.Value;
                        target.OfferType = GetExportData.OfferType.Value;
                    }
                    target.Level = GetExportData.Levels;
                    target.TotalPurchaseAmount = Convert.ToDecimal(totalpuramt);
                    target.TotalPendingPurchaseAmount = Convert.ToDecimal(GetExportData.PendingVolume);
                    try
                    {
                        var amtPer = target.TotalPurchaseAmount / (target.TargetAmount / 100);
                        var lineitemPer = target.TargetLineItem.HasValue && target.TargetLineItem.Value > 0 ? (target.CurrentLineItem.HasValue ? target.CurrentLineItem.Value : 0) / (target.TargetLineItem.Value / 100) : 100;

                        amtPer = amtPer > 100 ? 100 : amtPer;
                        lineitemPer = lineitemPer > 100 ? 100 : lineitemPer;

                        target.AchivePercent = (amtPer + lineitemPer) / 2;
                        if (target.AchivePercent > 100)
                            target.AchivePercent = 100;
                    }
                    catch (Exception)
                    {
                        target.AchivePercent = 0;
                    }

                    targets.Add(target);

                    if (GetExportData.TargetOnStores != null && GetExportData.TargetOnStores.Any(x => storeIds.Contains(x.StoreId)))
                    {
                        ElasticSqlHelper<DataContracts.External.MobileExecutiveDC.SalesPersonKpiElasticData> skcodeElasticHelper = new ElasticSqlHelper<DataContracts.External.MobileExecutiveDC.SalesPersonKpiElasticData>();
                        DateTime startDate = new DateTime(date.Year, date.Month, 1);
                        string sDate = startDate.ToString("yyyy-MM-dd");
                        string eDate = startDate.Date.AddMonths(1).ToString("yyyy-MM-dd");


                        foreach (var storetarget in GetExportData.TargetOnStores.Where(x => storeIds.Contains(x.StoreId)).ToList())
                        {

                            var query = $"select skcode, storeid, count(distinct itemnumber) linecount,  sum(dispatchqty * price) dispatchamount   from {platformIdxName} where createddate>='{sDate}' and createddate<'{eDate}' and status in ('Delivered','sattled') and storeid={storetarget.StoreId} and skcode ='{SkCode}'  group by skcode,storeid";
                            var skCodeDataList = AsyncContext.Run(() => skcodeElasticHelper.GetListAsync(query));

                            storetarget.CurrentVolume = skCodeDataList.FirstOrDefault()?.dispatchamount ?? 0;
                            storetarget.CurrentLineItem = Convert.ToInt32(skCodeDataList.FirstOrDefault()?.linecount ?? 0);



                            target = new CustomerTarget();
                            target.TargetMonth = date.ToString("MMMM");
                            target.StoreId = storetarget.StoreId;
                            target.StoreName = storetarget.StoreName;
                            target.StoreUrl = stores.Any(x => x.Id == storetarget.StoreId) ? stores.FirstOrDefault(x => x.Id == storetarget.StoreId).ImagePath : "";
                            target.SKCode = GetExportData.Skcode;
                            target.TargetAmount = Convert.ToDecimal(storetarget.Target);
                            target.TotalPurchaseAmount = Convert.ToDecimal(storetarget.CurrentVolume);
                            target.LeftDays = isMonthComplete ? 0 : (lastDayOfMonth - date.Day);
                            target.TargetLineItem = storetarget.TargetLineItem;
                            target.CurrentLineItem = storetarget.CurrentLineItem.HasValue ? storetarget.CurrentLineItem.Value : 0;

                            target.targetConditions = new List<targetCondition>();
                            target.targetConditions.Add(new targetCondition
                            {
                                ConditionText = "Shop worth Rs. " + Convert.ToInt32(storetarget.Target).ToString(),
                                ConditionCompleteText = ConvertNumberToWord.minifyLong(Convert.ToInt32(storetarget.CurrentVolume)) + "/" + ConvertNumberToWord.minifyLong(Convert.ToInt32(storetarget.Target)),
                                Target = Convert.ToInt32(storetarget.Target),
                                CurrentValue = Convert.ToInt32(storetarget.CurrentVolume),
                                AchivePercent = Convert.ToDecimal(storetarget.Target > 0 ? (storetarget.CurrentVolume / ((storetarget.Target) / 100)) > 100 ? 100 : (storetarget.CurrentVolume / ((storetarget.Target) / 100)) : 100),
                                Message = (storetarget.Target > storetarget.CurrentVolume ? "Only Rs. " + (storetarget.Target - storetarget.CurrentVolume) + " away from the remaining target." : "")
                            });

                            if (storetarget.TargetLineItem.HasValue && storetarget.TargetLineItem.Value > 0)
                            {
                                target.targetConditions.Add(new targetCondition
                                {
                                    ConditionText = "Buy " + Convert.ToInt32(storetarget.TargetLineItem).ToString() + " Unique line item",
                                    ConditionCompleteText = Convert.ToInt32(storetarget.CurrentLineItem.Value).ToString() + "/" + Convert.ToInt32(storetarget.TargetLineItem).ToString(),
                                    Target = storetarget.TargetLineItem.Value,
                                    CurrentValue = storetarget.CurrentLineItem.Value,
                                    AchivePercent = storetarget.TargetLineItem.Value > 0 ? (storetarget.CurrentLineItem.Value / (Convert.ToDecimal(storetarget.TargetLineItem.Value) / 100)) > 100 ? 100 : (storetarget.CurrentLineItem.Value / (Convert.ToDecimal(storetarget.TargetLineItem.Value) / 100)) : 100,
                                    Message = (storetarget.TargetLineItem.Value > storetarget.CurrentLineItem.Value ? "Buy " + (storetarget.TargetLineItem.Value - storetarget.CurrentLineItem.Value) + " unique line item more." : "")
                                });
                            }

                            if (!(GetExportData.IsOffer.HasValue && GetExportData.IsOffer.Value))
                            {
                                target.GiftImage = image != null ? image.ImagePath : null;
                                target.Type = image != null ? image.Type : null;
                                target.Value = image != null ? image.value : 0;
                            }
                            else
                            {
                                target.Type = "Offer";
                                target.OfferDesc = GetExportData.OfferDesc;
                                target.OfferValue = GetExportData.OfferValue.Value;
                                target.OfferType = GetExportData.OfferType.Value;
                            }
                            target.Level = GetExportData.Levels;
                            try
                            {
                                var amtPer = Convert.ToDecimal(storetarget.CurrentVolume / (storetarget.Target / 100));
                                var lineitemPer = storetarget.TargetLineItem.HasValue && storetarget.TargetLineItem.Value > 0 ? (storetarget.CurrentLineItem.HasValue ? storetarget.CurrentLineItem.Value : 0) / (Convert.ToDecimal(storetarget.TargetLineItem.Value) / 100) : 100;
                                amtPer = amtPer > 100 ? 100 : amtPer;
                                lineitemPer = lineitemPer > 100 ? 100 : lineitemPer;
                                target.AchivePercent = (amtPer + lineitemPer) / 2;
                                if (target.AchivePercent > 100)
                                    target.AchivePercent = 100;
                            }
                            catch (Exception)
                            {
                                target.AchivePercent = 0;
                            }
                            target.IsClaimed = target.AchivePercent >= 100;
                            targets.Add(target);
                        }

                    }

                    var response = new
                    {
                        GetTargetData = targets,
                        Status = true,
                        Message = "Target Data"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);


                }
                else
                {
                    string[] response = null;
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
        }


        [Route("GetBeatCustomerTargetNew")] //New Changes 
        [HttpGet]
        public BeatCustomerTargetDCs GetBeatCustomerTarget(int warehouseId, int salesExecutiveId,int StoreId,string KeyWord,int Skip, int Take)
        {
            BeatCustomerTargetDCs beatCustomerTargetDCs = new BeatCustomerTargetDCs();
            BeatCustomerTargetDc beatCustomerTargetDc = new BeatCustomerTargetDc();
            using (var con = new AuthContext())
            {
                //string query = "select distinct r.Name as Role from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where PeopleID='" + salesExecutiveId + "'and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                //var role = con.Database.SqlQuery<string>(query).ToList();
                //bool Isdigitalexecutive = role.Any(x => x.Contains("Digital sales executive") || x.Contains("Telecaller"));

                //if (!Isdigitalexecutive)
                {
                    var StoreClusterData = (from cse in con.ClusterStoreExecutives
                                            join s in con.StoreDB on cse.StoreId equals s.Id
                                            join c in con.Clusters on cse.ClusterId equals c.ClusterId
                                            where cse.IsActive == true && cse.IsDeleted == false && s.IsActive == true && s.IsDeleted == false
                                            && cse.ExecutiveId == salesExecutiveId && c.Active == true && c.Deleted == false
                                            select new
                                            {
                                                StoreId = cse.StoreId,
                                                StoreName = s.Name,
                                                c.ClusterId,
                                                c.ClusterName
                                            }).ToList();

                    var storeList = StoreClusterData.Select(x => new { StoreId = x.StoreId, StoreName = x.StoreName }).Distinct().ToList();
                    beatCustomerTargetDCs.StoreList = Mapper.Map(storeList).ToANew<List<DataContracts.External.MobileExecutiveDC.StoreList>>();
                }

                var sp = "GetExecutiveAllBeat";
                //if (Isdigitalexecutive)
                //    sp = "GetExecutiveAllBeatDigital";
                var peopleId = new SqlParameter("@peopleId", salesExecutiveId);
                var ExecutiveAllBeat = con.Database.SqlQuery<AllBeatCustomerDc>("exec " + sp + " @peopleId", peopleId);
                var Customerids = ExecutiveAllBeat.Select(y => y.CustomerId).Distinct().ToList();
                var executiveCustomers = con.Customers.Where(x => Customerids.Contains(x.CustomerId) && (KeyWord == null || x.Skcode.Contains(KeyWord) || x.Mobile.Contains(KeyWord))).Select(x => new { x.Skcode, x.Mobile, x.CustomerId, x.Warehouseid }).ToList();

                if (executiveCustomers.Count == 0)
                {
                    beatCustomerTargetDCs.beatCustomerTargets = new BeatCustomerTargetDc { CustomerTargetDcs = new List<CustomerTargetDc>() };
                    return beatCustomerTargetDCs;
                }

                var date = DateTime.Now;
                if (date.Day <= 7)
                {
                    date = DateTime.Now.AddMonths(-1);
                }
                var cartPredicate = PredicateBuilder.New<MonthlyCustomerTarget>(x => x.WarehouseId == warehouseId && executiveCustomers.Any(y => y.Skcode == x.Skcode));
                var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
                string DocumentName = "MonthlyTargetData_" + date.Month.ToString() + date.Year.ToString();
                var TargetCustomers = MonthlyCustomerTarget.Select(cartPredicate, null, null, null, false, "", DocumentName).ToList();
                
                if (TargetCustomers != null && TargetCustomers.Any())
                {
                    var customerTargetDcs = TargetCustomers.Where(x => x.TargetOnStores != null)
                                                                .Select(x => new CustomerTargetDc
                                                                {
                                                                    CustomerId = executiveCustomers.FirstOrDefault(y => y.Skcode == x.Skcode).CustomerId,
                                                                    MobileNo = executiveCustomers.FirstOrDefault(y => y.Skcode == x.Skcode).Mobile,
                                                                    WarehouseId = (int)executiveCustomers.FirstOrDefault(y => y.Skcode == x.Skcode).Warehouseid,
                                                                    SkCode = x.Skcode,
                                                                    TargetAmount = Convert.ToDecimal(x.TargetOnStores.Where(y => (StoreId == 0 || y.StoreId == StoreId)).Sum(y => y.Target)),
                                                                    AchieveAmount = Convert.ToDecimal(x.TargetOnStores.Where(y => (StoreId == 0 || y.StoreId == StoreId)).Sum(y => y.CurrentVolume)),
                                                                    RemainingAmount = Convert.ToDecimal(x.TargetOnStores.Where(y => (StoreId == 0 || y.StoreId == StoreId)).Sum(y => y.Target)) > Convert.ToDecimal(x.TargetOnStores.Sum(y => y.CurrentVolume))
                                                                                    ? Convert.ToDecimal(x.TargetOnStores.Where(y => (StoreId == 0 || y.StoreId == StoreId)).Sum(y => y.Target)) - Convert.ToDecimal(x.TargetOnStores.Sum(y => y.CurrentVolume)) : 0
                                                                }).ToList();


                    beatCustomerTargetDc.TargetAmount = customerTargetDcs != null ? customerTargetDcs.Sum(x => x.TargetAmount) : 0;
                    beatCustomerTargetDc.AchieveAmount = customerTargetDcs != null ? customerTargetDcs.Sum(x => x.AchieveAmount) : 0;
                    beatCustomerTargetDc.RemainingAmount = customerTargetDcs != null ? customerTargetDcs.Sum(x => x.RemainingAmount) : 0;
                    beatCustomerTargetDc.TargetCount = customerTargetDcs != null ? customerTargetDcs.Select(x => x.SkCode).Distinct().Count() : 0;
                    beatCustomerTargetDc.AchieveCount = customerTargetDcs != null ? customerTargetDcs.Where(x => x.TargetAmount <= x.AchieveAmount).Select(x => x.SkCode).Distinct().Count() : 0;
                    beatCustomerTargetDc.RemainingCount = customerTargetDcs != null ? customerTargetDcs.Where(x => x.RemainingAmount > 0).Select(x => x.SkCode).Distinct().Count() : 0;
                    beatCustomerTargetDc.CustomerTargetDcs = customerTargetDcs != null ? customerTargetDcs.OrderByDescending(x => x.TargetAmount).Skip(Skip).Take(Take).ToList() : new List<CustomerTargetDc>();

                }
                beatCustomerTargetDCs.beatCustomerTargets = beatCustomerTargetDc;
                
                //if(Isdigitalexecutive)
                //{
                //    var storeList = TargetCustomers.Where(x => x.TargetOnStores != null).SelectMany(x => x.TargetOnStores).Select(x=> new { x.StoreId,x.StoreName}).Distinct().ToList();
                //    beatCustomerTargetDCs.StoreList = Mapper.Map(storeList).ToANew<List<StoreList>>();
                //}
            }
            return beatCustomerTargetDCs;
        }

        #region Old Code
        [Route("GetBeatCustomerTarget")]
        [HttpGet]
        public BeatCustomerTargetDc GetBeatCustomerTarget(int warehouseId, int salesExecutiveId)
        {
            BeatCustomerTargetDc beatCustomerTargetDc = new BeatCustomerTargetDc();
            using (var con = new AuthContext())
            {
                var date = DateTime.Now;
                if (date.Day <= 7)
                {
                    date = DateTime.Now.AddMonths(-1);
                }
                var lastDayOfMonth = DateTime.DaysInMonth(date.Year, date.Month);

                var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
                string DocumentName = "MonthlyTargetData_" + date.Month.ToString() + date.Year.ToString();
                var TargetCustomers = MonthlyCustomerTarget.Select(x => x.WarehouseId == warehouseId, null, null, null, false, "", DocumentName).ToList();
                if (TargetCustomers != null && TargetCustomers.Any())
                {
                    var executiveCustomers = con.Customers.Where(x => x.ExecutiveId == salesExecutiveId).Select(x => new { x.Skcode, x.Mobile, x.CustomerId }).ToList();
                    if (executiveCustomers != null && executiveCustomers.Any())
                    {

                        List<CustomerTargetDc> customerTargetDcs = TargetCustomers.Where(x => executiveCustomers.Select(y => y.Skcode).Contains(x.Skcode)).
                                                                                  Select(x => new CustomerTargetDc
                                                                                  {
                                                                                      CustomerId = executiveCustomers.FirstOrDefault(y => y.Skcode == x.Skcode).CustomerId,
                                                                                      MobileNo = executiveCustomers.FirstOrDefault(y => y.Skcode == x.Skcode).Mobile,
                                                                                      SkCode = x.Skcode,
                                                                                      TargetAmount = x.Target,
                                                                                      AchieveAmount = x.CurrentVolume,
                                                                                      RemainingAmount = x.Target > x.CurrentVolume ? x.Target - x.CurrentVolume : 0
                                                                                  }).ToList();
                        beatCustomerTargetDc.AchieveAmount = customerTargetDcs != null ? customerTargetDcs.Sum(x => x.AchieveAmount) : 0;
                        beatCustomerTargetDc.TargetAmount = customerTargetDcs != null ? customerTargetDcs.Sum(x => x.TargetAmount) : 0;
                        beatCustomerTargetDc.RemainingAmount = customerTargetDcs != null ? customerTargetDcs.Sum(x => x.RemainingAmount) : 0;
                        beatCustomerTargetDc.CustomerTargetDcs = customerTargetDcs != null ? customerTargetDcs.OrderByDescending(x => x.TargetAmount).ToList() : new List<CustomerTargetDc>();
                    }

                }
            }
            return beatCustomerTargetDc;
        }
        #endregion

        [Route("CalculateOrderValueSQL")]
        [HttpGet]
        public HttpResponseMessage CalculateOrderValueSQL()
        {
            using (var con = new AuthContext())
            {

                var fromdate = DateTime.Now; //from.Date;
                                             //var todate = to.Date;
                                             //var ts = new TimeSpan(23, 59, 59);
                                             //todate = todate + ts;
                                             //var currentdate = DateTime.Now;
                var PreMonth = fromdate.AddMonths(-1);
                var customerstarget = con.CustomersTargetDB.Where(x => x.CreatedDate.Month == fromdate.Month && x.CreatedDate.Year == fromdate.Year).Select(x => x.Skcode).ToList();

                // MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
                // var orderMasters = new List<MongoOrderMaster>();

                // var torderMastersn = mongoDbHelper.find({ },{ }).tolis

                //var  torderMasters = mongoDbHelper.Select(x => x.CreatedDate >= from && x.CreatedDate <= to && (x.Status == "sattled" || x.Status == "Account settled" || x.Status == "Partial settled" || x.Status == "Delivered")
                //             , collectionName: "OrderMaster").Select(x => new { x.CustomerId, x.Status, x.CreatedDate, x.GrossAmount }).ToList();

                // var lorderMasters = orderMasters.Where(x=>x.CreatedDate.Month == from.Month && x.CreatedDate.Year == from.Year).Select(x => new { x.CustomerId, x.Status, x.CreatedDate, x.GrossAmount }).ToList();


                var orderdetails = con.DbOrderMaster.Where(x => x.CreatedDate.Month == fromdate.Month && x.CreatedDate.Year == fromdate.Year).Select(x => new { x.CustomerId, x.Status, x.CreatedDate, x.GrossAmount }).ToList();
                var customers = con.Customers.Where(x => x.Active == true && x.Deleted == false).Select(x => new { x.CustomerId, x.Skcode }).ToList();
                foreach (var item in customerstarget)
                {
                    var id = customers.Where(x => x.Skcode == item).Select(x => x.CustomerId).FirstOrDefault();
                    var ordervalue = orderdetails.Where(x => x.CustomerId == id && (x.Status == "sattled" || x.Status == "Account settled" || x.Status == "Partial settled" || x.Status == "Delivered") && x.CreatedDate.Month == fromdate.Month && x.CreatedDate.Year == fromdate.Year).Select(x => x.GrossAmount).Sum();

                    var update = con.CustomersTargetDB.Where(x => x.Skcode == item && x.CreatedDate.Month == fromdate.Month && x.CreatedDate.Year == fromdate.Year).FirstOrDefault();
                    update.CurrentVolume = Convert.ToDecimal(ordervalue); //'sattled','Account settled','Partial settled','Delivered'
                    if (update.CurrentVolume > 0)
                    {
                        con.Commit();
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, true);

            }




        }


        [Route("CalculateOrderValue")]
        [HttpGet]
        public HttpResponseMessage CalculateOrderValue()
        {
            using (var con = new AuthContext())
            {

                var fromdate = DateTime.Now;

                if (fromdate.Day <= 10)
                {
                    fromdate = DateTime.Now.AddMonths(-1);
                }
                var monthStart = new DateTime(fromdate.Year, fromdate.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var startDate = monthStart.ToString("yyyy-MM-dd");
                var enddate = monthEnd.ToString("yyyy-MM-dd");

                var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
                string DocumentName = "MonthlyTargetData_" + fromdate.Month.ToString() + fromdate.Year.ToString();
                int customerTargetCount = MonthlyCustomerTarget.Count(x => x.WarehouseId > -1, collectionName: DocumentName);
                if (customerTargetCount > 0)
                {
                    string query = $"SELECT  skcode,orderid,status,custid CustomerId,createddate,max(grossamount)  grossamount from skorderdata_{AppConstants.Environment} where  createddate>='{startDate}' and createddate <= '{enddate}'  group by orderid,status,custid,createddate,skcode";

                    ElasticSqlHelper<orderDataTarget> elasticSqlHelper = new ElasticSqlHelper<orderDataTarget>();

                    var orderdetails = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync(query)).ToList());

                    if (orderdetails != null)
                    {
                        query = $"SELECT  skcode,custid CustomerId, itemnumber,storeid, sum(dispatchqty*price) totalprice  from skorderdata_{AppConstants.Environment}" +
                            $" where  createddate>='{startDate}' and createddate <= '{enddate}' and (status='sattled' or status='Account settled' or status ='Partial settled' or status='Delivered')  group by custid,itemnumber,storeid,skcode";

                        ElasticSqlHelper<OrderCustomerLineItemTarget> elasticSqlHelper1 = new ElasticSqlHelper<OrderCustomerLineItemTarget>();

                        var customerorderdetails = AsyncContext.Run(async () => (await elasticSqlHelper1.GetListAsync(query)).ToList());

                        var skcodes = orderdetails.Select(x => x.skcode).ToList();

                        var customerstarget = MonthlyCustomerTarget.Select(x => skcodes.Contains(x.Skcode), null, null, null, false, "", DocumentName).ToList();

                        if (customerstarget != null && customerstarget.Any())
                        {
                            ParallelLoopResult parellelResult = Parallel.ForEach(customerstarget
                                //, new ParallelOptions { MaxDegreeOfParallelism = 10 }
                                , (update) =>
                                {
                                    var ordervalue = orderdetails.Where(x => x.skcode == update.Skcode && (x.Status == "sattled" || x.Status == "Account settled" || x.Status == "Partial settled" || x.Status == "Delivered") && x.CreatedDate.Month == fromdate.Month && x.CreatedDate.Year == fromdate.Year).Select(x => x.GrossAmount).Sum();
                                    var Pendingordervalue = orderdetails.Where(x => x.skcode == update.Skcode &&
                                     (x.Status == "Pending"
                                    || x.Status == "Delivery Redispatch"
                                    || x.Status == "Issued"
                                    || x.Status == "Ready to Dispatch"
                                    || x.Status == "Shipped")
                                    && x.CreatedDate.Month == fromdate.Month && x.CreatedDate.Year == fromdate.Year).Select(x => x.GrossAmount).Sum();

                                    var isUpdate = false;
                                    if (update.CurrentVolume != Convert.ToDecimal(ordervalue) || update.PendingVolume != Convert.ToDecimal(Pendingordervalue))
                                    {
                                        update.CurrentVolume = Convert.ToDecimal(ordervalue);
                                        update.PendingVolume = Convert.ToDecimal(Pendingordervalue);
                                        isUpdate = true;
                                    }
                                    if (update.TargetLineItem.HasValue && update.TargetLineItem.Value > 0 && (!update.CurrentLineItem.HasValue || update.CurrentLineItem <= update.TargetLineItem))
                                    {
                                        update.LineItemMinAmount = update.LineItemMinAmount.HasValue ? update.LineItemMinAmount.Value : 0;
                                        update.CurrentLineItem = customerorderdetails.Where(x => x.skcode == update.Skcode && x.totalprice >= update.LineItemMinAmount.Value).Count();
                                        isUpdate = true;
                                    }

                                    if (update.TargetOnStores != null && update.TargetOnStores.Any())
                                    {
                                        foreach (var targetOnStore in update.TargetOnStores)
                                        {
                                            if (customerorderdetails.Any(x => x.skcode == update.Skcode && x.storeid == targetOnStore.StoreId))
                                            {
                                                if (targetOnStore.CurrentVolume < targetOnStore.Target)
                                                {
                                                    targetOnStore.CurrentVolume = customerorderdetails.Where(x => x.skcode == update.Skcode && x.storeid == targetOnStore.StoreId && x.totalprice >= update.LineItemMinAmount.Value).Sum(x => x.totalprice);
                                                    isUpdate = true;
                                                }
                                                if (targetOnStore.CurrentLineItem < targetOnStore.TargetLineItem)
                                                {
                                                    targetOnStore.CurrentLineItem = customerorderdetails.Where(x => x.skcode == update.Skcode && x.storeid == targetOnStore.StoreId && x.totalprice >= update.LineItemMinAmount.Value).Select(x => x.itemnumber).Distinct().Count();
                                                    isUpdate = true;
                                                }
                                            }
                                        }

                                    }

                                    if (isUpdate)
                                        MonthlyCustomerTarget.ReplaceWithoutFind(update.Id, update, DocumentName);

                                });

                            if (parellelResult.IsCompleted)
                            {
                                var result = true;
                            }
                        }
                    }
                }

                #region old code

                //var Updateordervalue = MonthlyCustomerTarget.Select(x => x.WarehouseId > -1, null, null, null, false, "", DocumentName).ToList();
                //if (Updateordervalue != null)
                //{
                //    var customerstarget = Updateordervalue.Select(x => x.Skcode).ToList();//
                //    //var customerstarget = Updateordervalue.Where(x => x.Skcode == "SK153081").Select(x => x.Skcode).ToList();
                //    var skcodeList = new DataTable();
                //    skcodeList.Columns.Add("stringValues");
                //    if (customerstarget != null && customerstarget.Any())
                //    {
                //        foreach (var item in customerstarget)
                //        {
                //            var dr = skcodeList.NewRow();
                //            dr["stringValues"] = item;
                //            skcodeList.Rows.Add(dr);
                //        }
                //    }
                //    var Skcodeparam = new SqlParameter("Skcode", skcodeList);
                //    Skcodeparam.SqlDbType = SqlDbType.Structured;
                //    Skcodeparam.TypeName = "dbo.stringValues";

                //    if (con.Database.Connection.State != ConnectionState.Open)
                //        con.Database.Connection.Open();

                //    con.Database.CommandTimeout = 400;
                //    var cmd = con.Database.Connection.CreateCommand();
                //    cmd.CommandText = "[dbo].GetCustomerDataBySkcode";
                //    cmd.Parameters.Add(Skcodeparam);
                //    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                //    // Run the sproc
                //    var reader = cmd.ExecuteReader();
                //    List<GetCustomerTargetDC> customers = ((IObjectContextAdapter)con)
                //    .ObjectContext
                //    .Translate<GetCustomerTargetDC>(reader).ToList();
                //    //var customers = con.Customers.Where(x => x.Active == true && customerstarget.Contains(x.Skcode) && x.Deleted == false).Select(x => new { x.CustomerId, x.Skcode }).ToList();
                //    // string query = $"SELECT  orderid,status,custid CustomerId,createddate,max(grossamount)  grossamount from skorderdata_{AppConstants.Environment} where  custid=70077 and createddate>='{startDate}' and createddate <= '{enddate}'  group by orderid,status,custid,createddate";

                //    string query = $"SELECT  orderid,status,skcode,custid CustomerId,createddate,max(grossamount)  grossamount from skorderdata_{AppConstants.Environment} where  createddate>='{startDate}' and createddate <= '{enddate}'  group by orderid,status,custid,createddate";

                //    ElasticSqlHelper<orderDataTarget> elasticSqlHelper = new ElasticSqlHelper<orderDataTarget>();

                //    var orderdetails = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync(query)).ToList());


                //    query = $"SELECT  custid CustomerId, itemnumber,storeid, sum(dispatchqty*price) totalprice  from skorderdata_{AppConstants.Environment}" +
                //        $" where  createddate>='{startDate}' and createddate <= '{enddate}' and (status='sattled' or status='Account settled' or status ='Partial settled' or status='Delivered')  group by custid,itemnumber,storeid";

                //    ElasticSqlHelper<OrderCustomerLineItemTarget> elasticSqlHelper1 = new ElasticSqlHelper<OrderCustomerLineItemTarget>();

                //    var customerorderdetails = AsyncContext.Run(async () => (await elasticSqlHelper1.GetListAsync(query)).ToList());

                //    //var orderdetails = con.DbOrderMaster.Where(x => x.CreatedDate.Month == fromdate.Month && x.CreatedDate.Year == fromdate.Year).Select(x => new { x.CustomerId, x.Status, x.CreatedDate, x.GrossAmount }).ToList();

                //    //foreach (var item in customerstarget)
                //    ParallelLoopResult parellelResult = Parallel.ForEach(customerstarget, (item) =>
                //    {
                //        var id = customers.Where(x => x.Skcode == item).Select(x => x.CustomerId).FirstOrDefault();
                //        var ordervalue = orderdetails.Where(x => x.CustomerId == id && (x.Status == "sattled" || x.Status == "Account settled" || x.Status == "Partial settled" || x.Status == "Delivered") && x.CreatedDate.Month == fromdate.Month && x.CreatedDate.Year == fromdate.Year).Select(x => x.GrossAmount).Sum();
                //        var Pendingordervalue = orderdetails.Where(x => x.CustomerId == id &&
                //         (x.Status == "Pending"
                //        || x.Status == "Delivery Redispatch"
                //        || x.Status == "Issued"
                //        || x.Status == "Ready to Dispatch"
                //        || x.Status == "Shipped")
                //        && x.CreatedDate.Month == fromdate.Month && x.CreatedDate.Year == fromdate.Year).Select(x => x.GrossAmount).Sum();

                //        var update = Updateordervalue.Where(x => x.Skcode == item).FirstOrDefault();
                //        var isUpdate = false;
                //        if (update.CurrentVolume != Convert.ToDecimal(ordervalue) || update.PendingVolume != Convert.ToDecimal(Pendingordervalue))
                //        {
                //            update.CurrentVolume = Convert.ToDecimal(ordervalue);
                //            update.PendingVolume = Convert.ToDecimal(Pendingordervalue);
                //            isUpdate = true;
                //        }
                //        if (update.TargetLineItem.HasValue && update.TargetLineItem.Value > 0 && (!update.CurrentLineItem.HasValue || update.CurrentLineItem <= update.TargetLineItem))
                //        {
                //            update.LineItemMinAmount = update.LineItemMinAmount.HasValue ? update.LineItemMinAmount.Value : 0;
                //            update.CurrentLineItem = customerorderdetails.Where(x => x.customerid == id && x.totalprice >= update.LineItemMinAmount.Value).Count();
                //            isUpdate = true;
                //        }

                //        if (update.TargetOnStores != null && update.TargetOnStores.Any())
                //        {
                //            foreach (var targetOnStore in update.TargetOnStores)
                //            {
                //                if (customerorderdetails.Any(x => x.customerid == id && x.storeid == targetOnStore.StoreId))
                //                {
                //                    if (targetOnStore.CurrentVolume < targetOnStore.Target)
                //                    {
                //                        targetOnStore.CurrentVolume = customerorderdetails.Where(x => x.customerid == id && x.storeid == targetOnStore.StoreId && x.totalprice >= update.LineItemMinAmount.Value).Sum(x => x.totalprice);
                //                        isUpdate = true;
                //                    }
                //                    if (targetOnStore.CurrentLineItem < targetOnStore.TargetLineItem)
                //                    {
                //                        targetOnStore.CurrentLineItem = customerorderdetails.Where(x => x.customerid == id && x.storeid == targetOnStore.StoreId && x.totalprice >= update.LineItemMinAmount.Value).Select(x => x.itemnumber).Distinct().Count();
                //                        isUpdate = true;
                //                    }
                //                }
                //            }

                //        }

                //        if (isUpdate)
                //            MonthlyCustomerTarget.ReplaceWithoutFind(update.Id, update, DocumentName);

                //    });

                //    if (parellelResult.IsCompleted)
                //    {
                //        var result = true;
                //    }
                //}

                #endregion

                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
        }

        [AllowAnonymous]
        [Route("CalculateOrderValue")]
        [HttpGet]
        public HttpResponseMessage CalculateOrderValue(string SkCode)
        {
            using (var con = new AuthContext())
            {
                var fromdate = DateTime.Now;
                if (fromdate.Day <= 10)
                {
                    fromdate = DateTime.Now.AddMonths(-1);
                }

                var monthStart = new DateTime(fromdate.Year, fromdate.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var startDate = monthStart.ToString("yyyy-MM-dd");
                var enddate = monthEnd.ToString("yyyy-MM-dd");

                var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
                string DocumentName = "MonthlyTargetData_" + fromdate.Month.ToString() + fromdate.Year.ToString();
                var Updateordervalue = MonthlyCustomerTarget.Select(x => x.WarehouseId > -1, null, null, null, false, "", DocumentName).ToList();
                if (Updateordervalue != null)
                {
                    //var customerstarget = Updateordervalue.Select(x => x.Skcode).ToList();//
                    var customerstarget = Updateordervalue.Where(x => x.Skcode == SkCode).Select(x => x.Skcode).ToList();
                    var skcodeList = new DataTable();
                    skcodeList.Columns.Add("stringValues");
                    if (customerstarget != null && customerstarget.Any())
                    {
                        foreach (var item in customerstarget)
                        {
                            var dr = skcodeList.NewRow();
                            dr["stringValues"] = item;
                            skcodeList.Rows.Add(dr);
                        }
                    }
                    var Skcodeparam = new SqlParameter("Skcode", skcodeList);
                    Skcodeparam.SqlDbType = SqlDbType.Structured;
                    Skcodeparam.TypeName = "dbo.stringValues";

                    if (con.Database.Connection.State != ConnectionState.Open)
                        con.Database.Connection.Open();

                    con.Database.CommandTimeout = 400;
                    var cmd = con.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].GetCustomerDataBySkcode";
                    cmd.Parameters.Add(Skcodeparam);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    // Run the sproc
                    var reader = cmd.ExecuteReader();
                    List<GetCustomerTargetDC> customers = ((IObjectContextAdapter)con)
                    .ObjectContext
                    .Translate<GetCustomerTargetDC>(reader).ToList();
                    //var customers = con.Customers.Where(x => x.Active == true && customerstarget.Contains(x.Skcode) && x.Deleted == false).Select(x => new { x.CustomerId, x.Skcode }).ToList();
                    // string query = $"SELECT  orderid,status,custid CustomerId,createddate,max(grossamount)  grossamount from skorderdata_{AppConstants.Environment} where  custid=70077 and createddate>='{startDate}' and createddate <= '{enddate}'  group by orderid,status,custid,createddate";

                    string query = $"SELECT  orderid,status,custid CustomerId,createddate,max(grossamount)  grossamount from skorderdata_{AppConstants.Environment} where  createddate>='{startDate}' and createddate <= '{enddate}'  group by orderid,status,custid,createddate";

                    ElasticSqlHelper<orderDataTarget> elasticSqlHelper = new ElasticSqlHelper<orderDataTarget>();

                    var orderdetails = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync(query)).ToList());


                    query = $"SELECT  custid CustomerId, itemnumber,storeid, sum(dispatchqty*price) totalprice  from skorderdata_{AppConstants.Environment}" +
                        $" where  createddate>='{startDate}' and createddate <= '{enddate}' and (status='sattled' or status='Account settled' or status ='Partial settled' or status='Delivered')  group by custid,itemnumber,storeid";

                    ElasticSqlHelper<OrderCustomerLineItemTarget> elasticSqlHelper1 = new ElasticSqlHelper<OrderCustomerLineItemTarget>();

                    var customerorderdetails = AsyncContext.Run(async () => (await elasticSqlHelper1.GetListAsync(query)).ToList());

                    //var orderdetails = con.DbOrderMaster.Where(x => x.CreatedDate.Month == fromdate.Month && x.CreatedDate.Year == fromdate.Year).Select(x => new { x.CustomerId, x.Status, x.CreatedDate, x.GrossAmount }).ToList();

                    //foreach (var item in customerstarget)
                    ParallelLoopResult parellelResult = Parallel.ForEach(customerstarget, (item) =>
                    {
                        var id = customers.Where(x => x.Skcode == item).Select(x => x.CustomerId).FirstOrDefault();
                        var ordervalue = orderdetails.Where(x => x.CustomerId == id && (x.Status == "sattled" || x.Status == "Account settled" || x.Status == "Partial settled" || x.Status == "Delivered") && x.CreatedDate.Month == fromdate.Month && x.CreatedDate.Year == fromdate.Year).Select(x => x.GrossAmount).Sum();
                        var Pendingordervalue = orderdetails.Where(x => x.CustomerId == id &&
                         (x.Status == "Pending"
                        || x.Status == "Delivery Redispatch"
                        || x.Status == "Issued"
                        || x.Status == "Ready to Dispatch"
                        || x.Status == "Shipped")
                        && x.CreatedDate.Month == fromdate.Month && x.CreatedDate.Year == fromdate.Year).Select(x => x.GrossAmount).Sum();

                        var update = Updateordervalue.Where(x => x.Skcode == item).FirstOrDefault();
                        var isUpdate = false;
                        if (update.CurrentVolume != Convert.ToDecimal(ordervalue) || update.PendingVolume != Convert.ToDecimal(Pendingordervalue))
                        {
                            update.CurrentVolume = Convert.ToDecimal(ordervalue);
                            update.PendingVolume = Convert.ToDecimal(Pendingordervalue);
                            isUpdate = true;
                        }
                        if (update.TargetLineItem.HasValue && update.TargetLineItem.Value > 0 && (!update.CurrentLineItem.HasValue || update.CurrentLineItem <= update.TargetLineItem))
                        {
                            update.LineItemMinAmount = update.LineItemMinAmount.HasValue ? update.LineItemMinAmount.Value : 0;
                            update.CurrentLineItem = customerorderdetails.Where(x => x.customerid == id && x.totalprice >= update.LineItemMinAmount.Value).Count();
                            isUpdate = true;
                        }

                        if (update.TargetOnStores != null && update.TargetOnStores.Any())
                        {
                            foreach (var targetOnStore in update.TargetOnStores)
                            {
                                if (customerorderdetails.Any(x => x.customerid == id && x.storeid == targetOnStore.StoreId))
                                {
                                    if (targetOnStore.CurrentVolume < targetOnStore.Target)
                                    {
                                        targetOnStore.CurrentVolume = customerorderdetails.Where(x => x.customerid == id && x.storeid == targetOnStore.StoreId && x.totalprice >= update.LineItemMinAmount.Value).Sum(x => x.totalprice);
                                        isUpdate = true;
                                    }
                                    if (targetOnStore.CurrentLineItem < targetOnStore.TargetLineItem)
                                    {
                                        targetOnStore.CurrentLineItem = customerorderdetails.Where(x => x.customerid == id && x.storeid == targetOnStore.StoreId && x.totalprice >= update.LineItemMinAmount.Value).Select(x => x.itemnumber).Distinct().Count();
                                        isUpdate = true;
                                    }
                                }
                            }

                        }

                        if (isUpdate)
                            MonthlyCustomerTarget.ReplaceWithoutFind(update.Id, update, DocumentName);

                    });

                    if (parellelResult.IsCompleted)
                    {
                        var result = true;
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
        }

        [Route("CalculateOfferValue")]
        [HttpGet]
        public HttpResponseMessage CalculateOfferValue(DateTime from, DateTime to)
        {
            using (var con = new AuthContext())
            {
                var fromdate = from.Date;
                var todate = to.Date;
                var ts = new TimeSpan(23, 59, 59);
                todate = todate + ts;
                var currentdate = DateTime.Now;

                var customerstarget = con.TargetAllocationDB.Where(x => x.CreatedDate.Month == currentdate.Month && x.CreatedDate.Year == currentdate.Year).Select(x => x.SkCode).ToList();
                var billdiscount = con.BillDiscountDb.Where(x => x.IsActive == true && x.IsDeleted == false && x.CreatedDate.Month == currentdate.Month && x.CreatedDate.Year == currentdate.Year).ToList();
                var ordermaster = con.DbOrderMaster.Where(x => x.CreatedDate.Month == currentdate.Month && x.CreatedDate.Year == currentdate.Year).Select(x => new { x.CustomerId, x.WalletAmount, x.CreatedDate }).ToList();
                var customers = con.Customers.Where(x => x.Active == true && x.Deleted == false).Select(x => new { x.CustomerId, x.Skcode }).ToList();

                //var flashdeal = (from a in con.FlashDealItemConsumedDB
                //                 join b in con.DbOrderDetails on new { a.OrderId, a.ItemId } equals new { b.OrderId, b.ItemId }
                //                 where b.CreatedDate.Month == currentdate.Month && b.CreatedDate.Year == currentdate.Year
                //                 select new
                //                 {
                //                     OrderId = a.OrderId,
                //                     CustomerId = a.CustomerId,
                //                     ItemId = a.ItemId,
                //                     TotalAmt = b.TotalAmt,
                //                     CreatedDate = b.CreatedDate,
                //                 }).ToList();

                foreach (var item in customerstarget)
                {
                    var id = customers.Where(x => x.Skcode == item).Select(x => x.CustomerId).FirstOrDefault();
                    var ordervalue = ordermaster.Where(x => x.CustomerId == id && x.CreatedDate.Month == currentdate.Month && x.CreatedDate.Year == currentdate.Year).Select(x => x.WalletAmount).Sum();
                    var billdiscountvalue = billdiscount.Where(x => x.CustomerId == id && x.CreatedDate.Month == currentdate.Month && x.CreatedDate.Year == currentdate.Year).Select(x => x.BillDiscountAmount).Sum();
                    //var flashdealvalue = flashdeal.Where(x => x.CustomerId == id && x.CreatedDate >= fromdate && x.CreatedDate <= todate).Select(x => x.TotalAmt).Sum();

                    var allOffervalue = ordervalue + billdiscountvalue;
                    var update = con.TargetAllocationDB.Where(x => x.SkCode == item && x.CreatedDate.Month == currentdate.Month && x.CreatedDate.Year == currentdate.Year).FirstOrDefault();

                    update.used = update.used + allOffervalue ?? 0;
                    if (update.used > 0)
                    {
                        con.Commit();
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
        }

        [Route("ImageUpload")]
        [HttpPost]
        public string UploadFile()
        {
            string Logourl = "";
            logger.Info("start logo upload");
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    // Get the uploaded image from the Files collection
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];

                    if (httpPostedFile != null)
                    {
                        // Validate the uploaded image(optional)

                        // Get the complete file path
                        var LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedLogos"), httpPostedFile.FileName);

                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/UploadedLogos", LogoUrl);

                        Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                        Cloudinary cloudinary = new Cloudinary(account);

                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(LogoUrl),
                            PublicId = "TargetBands/" + httpPostedFile.FileName,
                            Overwrite = true,
                            Invalidate = true,
                            Backup = false
                        };

                        var uploadResult = cloudinary.Upload(uploadParams);

                        if (System.IO.File.Exists(LogoUrl))
                        {
                            System.IO.File.Delete(LogoUrl);
                        }

                        Logourl = Convert.ToString(uploadResult.SecureUri);
                        return Logourl;
                    }
                    return Logourl;
                }
                return Logourl;
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
            }
            return Logourl;
        }

        [Route("ClaimCustomerRewardSQL")]
        [HttpGet]
        public HttpResponseMessage ClaimCustomerRewardSQL(string SKCode, int CustomerId, int WarehouseId)
        {
            //var target = new CustomerTarget();
            var date = indianTime;
            var monthyear = DateTime.Now.Month + "-" + DateTime.Now.Year;
            using (var db = new AuthContext())
            {
                var GetExportData = db.CustomersTargetDB.Where(x => x.WarehouseId == WarehouseId && x.Skcode == SKCode && x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year).SingleOrDefault();
                var levelid = db.LevelMasterDB.Where(x => x.Name == GetExportData.Levels && x.WarehouseId == 1 && x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year).Select(x => x.Id).FirstOrDefault();
                var image = db.CustomerBandsDB.Where(x => x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year && x.LevelId == levelid && x.BandName == GetExportData.Bands).Select(x => new { x.Type, x.value, x.ImagePath, x.GiftId }).SingleOrDefault();

                if (GetExportData.IsClaimed == false && GetExportData.Target <= GetExportData.CurrentVolume)
                {
                    if (image.Type == "Point")
                    {
                        #region Reward Insertion in Wallet 

                        var cust = db.Customers.Where(x => x.Skcode == SKCode && x.CustomerId == CustomerId).SingleOrDefault();

                        var IsWalletEdit = db.WalletDb.Where(x => x.CustomerId == CustomerId).SingleOrDefault();
                        if (IsWalletEdit != null)
                        {
                            CustomerWalletHistory customerwallethistory = new CustomerWalletHistory();
                            try
                            {
                                customerwallethistory.CustomerId = CustomerId;
                                customerwallethistory.WarehouseId = WarehouseId;
                                customerwallethistory.Through = "Achieved " + monthyear + " Target Reward";
                                customerwallethistory.CompanyId = cust.CompanyId ?? 0;
                                if (Convert.ToDouble(image.value) >= 0)
                                {
                                    customerwallethistory.NewAddedWAmount = Convert.ToDouble(image.value);
                                    customerwallethistory.TotalWalletAmount = IsWalletEdit.TotalAmount + Convert.ToDouble(image.value);
                                }
                                customerwallethistory.UpdatedDate = indianTime;
                                customerwallethistory.TransactionDate = indianTime;
                                customerwallethistory.CreatedDate = indianTime;
                                db.CustomerWalletHistoryDb.Add(customerwallethistory);
                                db.Commit();
                            }
                            catch (Exception ex)
                            {

                            }

                            IsWalletEdit.TotalAmount = IsWalletEdit.TotalAmount + Convert.ToDouble(image.value);
                            IsWalletEdit.UpdatedDate = indianTime;
                            IsWalletEdit.TransactionDate = indianTime;
                            db.WalletDb.Attach(IsWalletEdit);
                            db.Entry(IsWalletEdit).State = EntityState.Modified;
                            db.Commit();

                            GetExportData.IsClaimed = true;
                            db.CustomersTargetDB.Attach(GetExportData);
                            db.Entry(GetExportData).State = EntityState.Modified;
                            db.Commit();
                        }
                        else
                        {
                            var wallet = new Wallet();
                            wallet.CompanyId = cust.CompanyId ?? 0;
                            wallet.CustomerId = CustomerId;
                            wallet.TransactionDate = indianTime;
                            wallet.TotalAmount = Convert.ToDouble(image.value);
                            wallet.CreatedDate = indianTime;
                            wallet.UpdatedDate = indianTime;
                            wallet.Deleted = false;
                            db.WalletDb.Add(wallet);
                            db.Commit();

                            CustomerWalletHistory customerwallethistory = new CustomerWalletHistory();
                            try
                            {
                                customerwallethistory.CustomerId = CustomerId;
                                customerwallethistory.WarehouseId = WarehouseId;
                                customerwallethistory.Through = "Achieved " + monthyear + " Target Reward";
                                customerwallethistory.CompanyId = cust.CompanyId ?? 0;
                                if (Convert.ToDouble(image.value) >= 0)
                                {
                                    customerwallethistory.NewAddedWAmount = Convert.ToDouble(image.value);
                                    customerwallethistory.TotalWalletAmount = Convert.ToDouble(image.value);
                                }
                                customerwallethistory.UpdatedDate = indianTime;
                                customerwallethistory.TransactionDate = indianTime;
                                customerwallethistory.CreatedDate = indianTime;
                                db.CustomerWalletHistoryDb.Add(customerwallethistory);
                                db.Commit();

                                GetExportData.IsClaimed = true;
                                db.CustomersTargetDB.Attach(GetExportData);
                                db.Entry(GetExportData).State = EntityState.Modified;
                                db.Commit();
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        #endregion

                        var response = new
                        {
                            Status = true,
                            Message = "Reward Points successfully Added in wallet"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                    else
                    {
                        #region Create Reward Ordered
                        Customer cust = new Customer();
                        Customer custdb = new Customer();
                        Warehouse w = new Warehouse();
                        var sc = new DreamOrder();
                        try
                        {
                            cust = db.Customers.Where(x => x.Skcode == SKCode && x.CustomerId == CustomerId).SingleOrDefault();
                        }
                        catch (Exception sad)
                        {
                        }
                        try
                        {
                            custdb = db.Customers.Where(x => x.CustomerId == CustomerId).SingleOrDefault();
                        }
                        catch (Exception sad)
                        {
                        }
                        try
                        {
                            w = db.Warehouses.Where(wr => wr.WarehouseId == custdb.Warehouseid).FirstOrDefault();
                        }
                        catch (Exception sad)
                        {
                        }
                        if (cust != null)
                        {
                            if (w != null)
                            {
                                sc.WarehouseId = w.WarehouseId;
                                sc.WarehouseName = w.WarehouseName;
                                sc.CityId = w.Cityid;
                            }
                            sc.ShopName = cust.ShopName;
                            sc.Status = "Pending";
                            sc.CustomerMobile = cust.Mobile;
                            sc.ShippingAddress = cust.ShippingAddress;
                            //sc.SalesPersonId = cust.ExecutiveId;
                            sc.CompanyId = cust.CompanyId;
                        }
                        sc.CreatedDate = indianTime;
                        sc.UpdatedDate = indianTime;
                        sc.Deliverydate = indianTime.AddDays(2);
                        sc.Deleted = false;
                        sc.ReDispatchCount = 0;

                        var dreamorder = new DreamItem();
                        RewardItems it = db.RewardItemsDb.Where(x => x.rItemId == image.GiftId).SingleOrDefault();
                        dreamorder.ShopName = cust.ShopName;
                        dreamorder.ItemId = it.rItemId;
                        dreamorder.OrderQty = 1;

                        dreamorder.ShopName = cust.ShopName;
                        dreamorder.Skcode = cust.Skcode;
                        dreamorder.ItemName = it.rItem;
                        dreamorder.Discription = it.Description;
                        dreamorder.Status = "Pending";
                        dreamorder.CreatedDate = indianTime;
                        dreamorder.UpdatedDate = indianTime;
                        dreamorder.Deleted = false;


                        try
                        {
                            if (sc.DreamItemDetails == null)
                            {
                                sc.DreamItemDetails = new List<DreamItem>();
                            }
                            sc.DreamItemDetails.Add(dreamorder);
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }

                        db.DreamOrderDb.Add(sc);
                        int id = db.Commit();

                        GetExportData.IsClaimed = true;
                        db.CustomersTargetDB.Attach(GetExportData);
                        db.Entry(GetExportData).State = EntityState.Modified;
                        db.Commit();
                        #endregion

                        var response = new
                        {
                            Status = true,
                            Message = "Reward order successfully created"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }

                }

                else
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Already claimed Reward"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
        }

        [Route("ClaimCustomerReward")]
        [HttpGet]
        public HttpResponseMessage ClaimCustomerReward(string SKCode, int CustomerId, int WarehouseId)
        {
            //var target = new CustomerTarget();
            var date = indianTime;
            if (date.Day <= 10)
            {
                date = DateTime.Now.AddMonths(-1);
            }
            var monthyear = DateTime.Now.Month + "-" + DateTime.Now.Year;
            using (var db = new AuthContext())
            {

                var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
                string DocumentName = "MonthlyTargetData_" + date.Month.ToString() + date.Year.ToString();
                var Target = MonthlyCustomerTarget.Select(x => x.Skcode == SKCode, null, null, null, false, "", DocumentName).FirstOrDefault();
                if (Target != null)
                {
                    var GetExportData = Target;

                    //var GetExportData = db.CustomersTargetDB.Where(x => x.WarehouseId == WarehouseId && x.Skcode == SKCode && x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year).SingleOrDefault();
                    var levelid = db.LevelMasterDB.Where(x => x.Name == GetExportData.Levels && x.WarehouseId == 1 && x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year).Select(x => x.Id).FirstOrDefault();
                    var image = db.CustomerBandsDB.Where(x => x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year && x.LevelId == levelid && x.BandName == GetExportData.Bands).Select(x => new { x.Type, x.value, x.ImagePath, x.GiftId }).SingleOrDefault();

                    if (!(Target.IsOffer.HasValue && Target.IsOffer.Value))
                    {
                        if (GetExportData.IsClaimed == false && Convert.ToInt32(GetExportData.Target) <= Convert.ToInt32(GetExportData.CurrentVolume))
                        {
                            if (image != null && image.Type == "Point")
                            {
                                #region Reward Insertion in Wallet 

                                var cust = db.Customers.Where(x => x.CustomerId == CustomerId).SingleOrDefault();

                                var IsWalletEdit = db.WalletDb.Where(x => x.CustomerId == CustomerId).SingleOrDefault();
                                if (IsWalletEdit != null)
                                {
                                    CustomerWalletHistory customerwallethistory = new CustomerWalletHistory();
                                    try
                                    {
                                        customerwallethistory.CustomerId = CustomerId;
                                        customerwallethistory.WarehouseId = cust.Warehouseid.Value;
                                        customerwallethistory.Through = "Achieved " + monthyear + " Target Reward";
                                        customerwallethistory.CompanyId = cust.CompanyId ?? 0;
                                        if (Convert.ToDouble(image.value) >= 0)
                                        {
                                            customerwallethistory.NewAddedWAmount = Convert.ToDouble(image.value);
                                            customerwallethistory.TotalWalletAmount = IsWalletEdit.TotalAmount + Convert.ToDouble(image.value);
                                        }
                                        customerwallethistory.UpdatedDate = indianTime;
                                        customerwallethistory.TransactionDate = indianTime;
                                        customerwallethistory.CreatedDate = indianTime;
                                        db.CustomerWalletHistoryDb.Add(customerwallethistory);
                                        db.Commit();
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                    IsWalletEdit.TotalAmount = IsWalletEdit.TotalAmount + Convert.ToDouble(image.value);
                                    IsWalletEdit.UpdatedDate = indianTime;
                                    IsWalletEdit.TransactionDate = indianTime;
                                    db.WalletDb.Attach(IsWalletEdit);
                                    db.Entry(IsWalletEdit).State = EntityState.Modified;
                                    db.Commit();



                                    Target.IsClaimed = true;
                                    MonthlyCustomerTarget.Replace(Target.Id, Target, DocumentName);

                                }
                                else
                                {
                                    var wallet = new Wallet();
                                    wallet.CompanyId = cust.CompanyId ?? 0;
                                    wallet.CustomerId = CustomerId;
                                    wallet.TransactionDate = indianTime;
                                    wallet.TotalAmount = Convert.ToDouble(image.value);
                                    wallet.CreatedDate = indianTime;
                                    wallet.UpdatedDate = indianTime;
                                    wallet.Deleted = false;
                                    db.WalletDb.Add(wallet);
                                    db.Commit();

                                    CustomerWalletHistory customerwallethistory = new CustomerWalletHistory();
                                    try
                                    {
                                        customerwallethistory.CustomerId = CustomerId;
                                        customerwallethistory.WarehouseId = cust.Warehouseid.Value;
                                        customerwallethistory.Through = "Achieved " + monthyear + " Target Reward";
                                        customerwallethistory.CompanyId = cust.CompanyId ?? 0;
                                        if (Convert.ToDouble(image.value) >= 0)
                                        {
                                            customerwallethistory.NewAddedWAmount = Convert.ToDouble(image.value);
                                            customerwallethistory.TotalWalletAmount = Convert.ToDouble(image.value);
                                        }
                                        customerwallethistory.UpdatedDate = indianTime;
                                        customerwallethistory.TransactionDate = indianTime;
                                        customerwallethistory.CreatedDate = indianTime;
                                        db.CustomerWalletHistoryDb.Add(customerwallethistory);
                                        db.Commit();

                                        Target.IsClaimed = true;
                                        MonthlyCustomerTarget.Replace(Target.Id, Target, DocumentName);

                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                                #endregion

                                var response = new
                                {
                                    Status = true,
                                    Message = "Reward Points successfully Added in wallet"
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, response);
                            }
                            else
                            {
                                #region Create Reward Ordered
                                Customer cust = new Customer();
                                Customer custdb = new Customer();
                                Warehouse w = new Warehouse();
                                var sc = new DreamOrder();
                                try
                                {
                                    cust = db.Customers.Where(x => x.Skcode == SKCode && x.CustomerId == CustomerId).SingleOrDefault();
                                }
                                catch (Exception sad)
                                {
                                }
                                try
                                {
                                    custdb = db.Customers.Where(x => x.CustomerId == CustomerId).SingleOrDefault();
                                }
                                catch (Exception sad)
                                {
                                }
                                try
                                {
                                    w = db.Warehouses.Where(wr => wr.WarehouseId == custdb.Warehouseid).FirstOrDefault();
                                }
                                catch (Exception sad)
                                {
                                }
                                if (cust != null)
                                {
                                    if (w != null)
                                    {
                                        sc.WarehouseId = w.WarehouseId;
                                        sc.WarehouseName = w.WarehouseName;
                                        sc.CityId = w.Cityid;
                                    }
                                    sc.ShopName = cust.ShopName;
                                    sc.Status = "Pending";
                                    sc.CustomerMobile = cust.Mobile;
                                    sc.ShippingAddress = cust.ShippingAddress;
                                    //sc.SalesPersonId = cust.ExecutiveId;
                                    sc.CompanyId = cust.CompanyId;
                                }
                                sc.CreatedDate = indianTime;
                                sc.UpdatedDate = indianTime;
                                sc.Deliverydate = indianTime.AddDays(2);
                                sc.Deleted = false;
                                sc.ReDispatchCount = 0;

                                var dreamorder = new DreamItem();
                                RewardItems it = db.RewardItemsDb.Where(x => x.rItemId == image.GiftId).SingleOrDefault();
                                dreamorder.ShopName = cust.ShopName;
                                dreamorder.ItemId = it.rItemId;
                                dreamorder.OrderQty = 1;

                                dreamorder.ShopName = cust.ShopName;
                                dreamorder.Skcode = cust.Skcode;
                                dreamorder.ItemName = it.rItem;
                                dreamorder.Discription = it.Description;
                                dreamorder.Status = "Pending";
                                dreamorder.CreatedDate = indianTime;
                                dreamorder.UpdatedDate = indianTime;
                                dreamorder.Deleted = false;


                                try
                                {
                                    if (sc.DreamItemDetails == null)
                                    {
                                        sc.DreamItemDetails = new List<DreamItem>();
                                    }
                                    sc.DreamItemDetails.Add(dreamorder);
                                }
                                catch (Exception ex)
                                {
                                    throw;
                                }

                                db.DreamOrderDb.Add(sc);
                                int id = db.Commit();

                                Target.IsClaimed = true;
                                MonthlyCustomerTarget.Replace(Target.Id, Target, DocumentName);
                                #endregion

                                var response = new
                                {
                                    Status = true,
                                    Message = "Reward order successfully created"
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, response);
                            }
                        }
                        else
                        {
                            var response = new
                            {
                                Status = false,
                                Message = "Already claimed Reward"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, response);
                        }
                    }
                    else
                    {
                        if (GetExportData.IsClaimed == false && Convert.ToInt32(GetExportData.Target) <= Convert.ToInt32(GetExportData.CurrentVolume))
                        {
                            int totalDiscount = GetExportData.OfferValue.Value;
                            if (GetExportData.OfferType.Value == 0)//Percent
                            {
                                totalDiscount = Convert.ToInt32(GetExportData.Target * Convert.ToDecimal(GetExportData.OfferValue.Value) / 100);
                            }
                            List<int> discounts = new List<int>();
                            if (totalDiscount > GetExportData.MaxDiscount)
                            {
                                var j = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(totalDiscount) / GetExportData.MaxDiscount.Value));
                                for (int i = 0; i < j; i++)
                                {
                                    discounts.Add(totalDiscount > GetExportData.MaxDiscount.Value ? GetExportData.MaxDiscount.Value : totalDiscount);
                                    totalDiscount = totalDiscount - GetExportData.MaxDiscount.Value;
                                }
                            }
                            else
                            {
                                discounts.Add(totalDiscount);
                            }

                            var offers = db.OfferDb.FirstOrDefault(x => x.OfferId == GetExportData.OfferId);
                            if (offers != null)
                            {
                                foreach (var item in discounts)
                                {
                                    db.BillDiscountDb.Add(new Model.BillDiscount.BillDiscount
                                    {
                                        BillDiscountAmount = item,
                                        BillDiscountType = offers.OfferOn,
                                        BillDiscountTypeValue = item,
                                        CreatedBy = CustomerId,
                                        CreatedDate = DateTime.Now,
                                        CustomerId = CustomerId,
                                        IsActive = true,
                                        IsAddNextOrderWallet = false,
                                        IsMultiTimeUse = offers.IsMultiTimeUse,
                                        IsDeleted = false,
                                        IsScratchBDCode = false,
                                        IsUsedNextOrder = false,
                                        IsUseOtherOffer = offers.IsUseOtherOffer,
                                        MinOrderAmount = item * GetExportData.MOVMultiplier.Value,
                                        MaxOrderAmount = item * GetExportData.MOVMultiplier.Value * 10,
                                        OfferId = offers.OfferId,
                                        OrderId = 0
                                    });
                                }
                            }
                            string successmsg = "Please connect our customer executive";
                            if (db.Commit() > 0)
                            {
                                Target.IsClaimed = true;
                                MonthlyCustomerTarget.Replace(Target.Id, Target, DocumentName);
                                successmsg = "Scretch card successfully Added in offer section.";
                            }
                            var response = new
                            {
                                Status = true,
                                Message = successmsg
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, response);
                        }
                        else
                        {
                            var response = new
                            {
                                Status = false,
                                Message = "Already claimed Reward"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, response);
                        }
                    }
                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Target Not Found!"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
        }

        [Route("GetMLData")]
        [HttpGet]
        public async Task<List<MLBudgetDTO>> GetMLData(int month, int year, int band, decimal amount, decimal l0amount, int levels, int cityid, int warehouseid)
        {
            var url = "allocation?month=" + month + "&year=" + year + "&band=" + band + "&amount=" + amount + "&l0amount=" + l0amount + "&levels=" + levels + "&cityid=" + cityid + "&warehouseid=" + warehouseid;

            List<MLBudgetDTO> MLBudgetDTOs = new List<MLBudgetDTO>();
            try
            {
                using (GenericRestHttpClient<string, string> memberClient
                                         = new GenericRestHttpClient<string, string>("http://127.0.0.1:5000",
                                         url, null))
                {
                    var str = await memberClient.GetStringAsync();
                    MLBudgetDTOs = JsonConvert.DeserializeObject<List<MLBudgetDTO>>(str);
                }
            }
            catch (Exception ex)
            {

            }
            return MLBudgetDTOs;
        }

        public List<MonthlyTargetData> TMMonthlyData(int month, int year)
        {
            var MonthlyTMData = new List<MonthlyTargetData>();
            using (var con = new AuthContext())
            {
                var date = new DateTime(year, month, 1);

                var MTData = con.LevelMasterDB.Where(x => x.IsActive == true && x.IsDeleted == false && x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year && x.WarehouseId == 1).Select(x => new { x.Id, x.Name, x.Type, x.value }).ToList();
                foreach (var item in MTData)
                {
                    var bandsdata = con.CustomerBandsDB.Where(x => x.IsActive == true && x.IsDeleted == false && x.CreatedDate.Month == date.Month && x.CreatedDate.Year == date.Year && x.LevelId == item.Id).ToList();
                    foreach (var ban in bandsdata)
                    {
                        var data = new MonthlyTargetData();
                        data.Id = item.Id;
                        data.LevelName = item.Name;
                        data.Type = item.Type == true ? "Value" : "Percent";
                        data.Value = item.value;
                        data.CreatedDate = ban.CreatedDate;
                        data.BandName = ban.BandName;
                        data.LowerLimit = ban.LowerLimit;
                        data.UpperLimit = ban.UpperLimit;
                        MonthlyTMData.Add(data);
                    }
                }

                return MonthlyTMData;
            }
        }

        [Route("InsertMLData")]
        [HttpGet]
        public async Task<HttpResponseMessage> CreateCustomerTarget()
        {

            //var MCTList = new MonthlyCustomerTarget();
            //MCTList.Month = indianTime.Month;
            //MCTList.Year = indianTime.Year;

            var level = new List<MonthlyCustomerTarget>();

            using (var con = new AuthContext())
            {
                var citylist = con.Cities.Where(x => x.active == true && x.Deleted == false).ToList();

                List<MonthlyTargetData> monthlydata = TMMonthlyData(indianTime.Month, indianTime.Year);
                string DocumentName = "MonthlyTargetData_" + indianTime.Month.ToString() + indianTime.Year.ToString();
                int month = DateTime.Now.AddMonths(-1).Month;
                int year = DateTime.Now.AddMonths(-1).Year;

                string qry = "Exec CRMLevelCustomerDetail " + month + ", " + year;
                con.Database.CommandTimeout = 600;
                var getdata = con.Database.SqlQuery<CustDetailLabelResponse>(qry).ToList();

                int i = 0;
                foreach (var item in getdata)
                {
                    i = i + 1;
                    if (item.Volume >= 75000 && item.OrderCount >= 12 && item.BrandCount >= 40 && item.KKvolume >= 15000 && Convert.ToDecimal((item.Selfordercount / (item.Selfordercount + item.Salespersonordercount))) * 100 > 60)
                    {
                        item.Level = "Level 5";
                    }
                    else if (item.Volume >= 30000 && item.OrderCount >= 8 && item.BrandCount >= 20 && item.KKvolume >= 8000 && Convert.ToDecimal((item.Selfordercount / (item.Selfordercount + item.Salespersonordercount))) * 100 > 30)
                    {
                        item.Level = "Level 4";
                    }
                    else if (item.Volume >= 20000 && item.OrderCount >= 5 && item.BrandCount >= 10 && item.KKvolume >= 2000)
                    {
                        item.Level = "Level 3";
                    }
                    else if (item.Volume >= 10000 && item.OrderCount >= 3 && item.BrandCount >= 5)
                    {
                        item.Level = "Level 2";
                    }
                    else if (item.Volume >= 1)
                    {
                        item.Level = "Level 1";
                    }
                    else
                    {
                        item.Level = "Level 0";
                    }

                    MonthlyTargetData leveldata = monthlydata.Where(x => x.LevelName == item.Level).FirstOrDefault();
                    var banddata = monthlydata.Where(x => x.LevelName == item.Level).ToList();
                    var ct = new MonthlyCustomerTarget();
                    ct.Skcode = item.SkCode;
                    ct.WarehouseId = item.WarehouseId;
                    ct.CityName = citylist.Where(x => x.Cityid == item.Cityid).Select(x => x.CityName).FirstOrDefault();
                    ct.Levels = item.Level;
                    if (leveldata.Type == "Value")
                    {
                        try { ct.Target = Convert.ToDecimal(item.Volume) + Convert.ToDecimal(leveldata.Value); } catch (Exception) { ct.Target = 0; }
                    }
                    else
                    {
                        try { ct.Target = Convert.ToDecimal(item.Volume) + (Convert.ToDecimal(leveldata.Value) * (Convert.ToDecimal(item.Volume) / 100)); } catch (Exception) { ct.Target = 0; }
                    }
                    ct.Volume = Convert.ToDecimal(item.Volume);

                    foreach (var bd in banddata)
                    {
                        if (Convert.ToDecimal(item.Volume) >= bd.LowerLimit && Convert.ToDecimal(item.Volume) <= bd.UpperLimit)
                        {
                            ct.Bands = bd.BandName;
                        }
                    }
                    //ct.Bands
                    ct.CreatedDate = indianTime;
                    ct.Source = "C#";
                    ct.CurrentVolume = 0;
                    ct.IsClaimed = false;
                    //ct.Id = i;
                    //MCTList.Add(ct);
                    MongoDbHelper<MonthlyCustomerTarget> mongoDbHelper = new MongoDbHelper<MonthlyCustomerTarget>();
                    mongoDbHelper.Insert(ct, DocumentName);
                }

                //MCTList.CustomerTargets = level;

                //level0 = level.Where(x => x.Levels == "Level 0").ToList();
                //level1 = level.Where(x => x.Levels == "Level 1").ToList();
                //level2 = level.Where(x => x.Levels == "Level 2").ToList();
                //level3 = level.Where(x => x.Levels == "Level 3").ToList();
                //level4 = level.Where(x => x.Levels == "Level 4").ToList();
                //level5 = level.Where(x => x.Levels == "Level 5").ToList();

                //MCTList.Level0 = level0;
                //MCTList.Level1 = level1;
                //MCTList.Level2 = level2;
                //MCTList.Level3 = level3;
                //MCTList.Level4 = level4;
                //MCTList.Level5 = level5;



            }
            return Request.CreateResponse(HttpStatusCode.OK, true);

        }

        [Route("InsertMLDataOld")]
        [HttpGet]
        public async Task<HttpResponseMessage> InsertMLDataAsync()
        {
            //string json = new WebClient().DownloadString("http://192.168.1.101:8989/FlaskRedirect/foo//target/percent?percentage=10&levels=1&ulimit=137166&llimit=47293");
            var MLList = new List<mldata>();
            var MLUrl = ConfigurationManager.AppSettings["MLURL"];

            using (var con = new AuthContext())
            {
                var citylist = con.Cities.Where(x => x.active == true && x.Deleted == false).ToList();

                List<MonthlyTargetData> monthlydata = TMMonthlyData(indianTime.Month, indianTime.Year);
                if (monthlydata != null)
                {
                    foreach (var item in monthlydata)
                    {
                        string vp = item.Type == "Value" ? "FlaskRedirect/foo//target/value?value=" + item.Value : "FlaskRedirect/foo//target/percent?percentage=" + item.Value;

                        var level = item.LevelName.Remove(0, 6);
                        var url = vp + "&levels=" + level + "&ulimit=" + item.UpperLimit + "&llimit=" + item.LowerLimit;

                        //using (GenericRestHttpClient<List<mldata>, string> memberClient
                        //                 = new GenericRestHttpClient<List<mldata>, string>(MLUrl,
                        //                 url, null))
                        //{
                        //var str = await memberClient.GetStringAsync();
                        string json = new WebClient().DownloadString(MLUrl + url);
                        MLList = JsonConvert.DeserializeObject<List<mldata>>(json);
                        //try
                        //{
                        //    MLList = await memberClient.GetAsync();
                        //}
                        //catch (Exception ex)
                        //{ }
                        // }

                        //http://192.168.1.101:8989/FlaskRedirect/foo//target/percent?percentage=10&levels=1&ulimit=137166.35&llimit=47293.56

                        foreach (var mlitem in MLList)
                        {
                            var ct = new CustomersTarget();
                            ct.Skcode = mlitem.SkCode;
                            ct.WarehouseId = mlitem.WarehouseId;
                            ct.CityName = citylist.Where(x => x.Cityid == mlitem.Cityid).Select(x => x.CityName).FirstOrDefault();
                            ct.Levels = mlitem.levels;
                            ct.Target = Convert.ToDecimal(mlitem.Target);
                            ct.Volume = Convert.ToDecimal(mlitem.Volume);
                            ct.Bands = item.BandName;
                            ct.CreatedDate = indianTime;
                            ct.Source = "python";
                            ct.CurrentVolume = 0;
                            ct.IsClaimed = false;
                            con.CustomersTargetDB.Add(ct);
                            con.Commit();
                        }
                    }
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, MLList);

        }

        [Route("SaveMonthlyCustomersLevel")]
        [HttpGet]
        public bool SaveMonthlyCustomersLevel(int month, int year)
        {
            var a = new MonthlyCustomerLevel();
            var Monthlycustlevel = new List<CustLevel>();
            using (var con = new AuthContext())
            {
                Monthlycustlevel = con.Database.SqlQuery<CustLevel>("exec GetLevelMonthly " + month + "," + year).ToList();

                a.Month = month;
                a.Year = year;
                a.CustomerLevels = Monthlycustlevel;

                MongoDbHelper<MonthlyCustomerLevel> mongoDbHelper = new MongoDbHelper<MonthlyCustomerLevel>();
                mongoDbHelper.Insert(a);

                return true;
            }
        }

        [Route("GetLevelcolour")]
        [HttpGet]
        public dynamic GetLevelcolour()
        {
            using (var con = new AuthContext())
            {

                var levelcolor = con.LevelMasterDB.Where(x => x.WarehouseId == 1 && x.CreatedDate.Month == indianTime.Month && x.CreatedDate.Year == indianTime.Year).Select(x => new { LevelName = x.Name, LevelColour = x.LevelColour }).ToList();

                return levelcolor;
            }
        }

        [Route("GetUpdateLevel")]
        [HttpGet]
        public List<levelupdate> GetUpdateLevel()
        {
            using (var con = new AuthContext())
            {

                List<levelupdate> lu = con.LevelMasterDB.Where(x => x.WarehouseId == 1 && x.CreatedDate.Month == indianTime.Month && x.CreatedDate.Year == indianTime.Year).Select(x => new levelupdate { LevelName = x.Name, LevelColor = x.LevelColour, LevelId = x.Id }).ToList();

                return lu;
            }
        }

        [Route("UpdateLevel")]
        [HttpPost]
        public bool UpdateLevel(List<levelupdate> updatelevel)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var con = new AuthContext())
            {
                foreach (var item in updatelevel)
                {
                    var leveldata = con.LevelMasterDB.Where(x => x.Id == item.LevelId).FirstOrDefault();
                    if (leveldata != null)
                    {
                        leveldata.LevelColour = item.LevelColor;
                        leveldata.ModifiedDate = indianTime;
                        leveldata.ModifiedBy = userid;
                        con.Commit();
                    }
                }

            }

            return true;
        }

        [Route("CreateTargetCheck")]
        [HttpGet]
        public bool CreateTargetCheck()
        {
            var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
            string DocumentName = "MonthlyTargetData_" + indianTime.Month.ToString() + indianTime.Year.ToString();
            var Target = MonthlyCustomerTarget.Count(x => x.WarehouseId > -1, false, "", DocumentName);
            if (Target > 0)
                return false;
            else
                return true;
        }

        [Route("UpdateTargetDoc")]
        [HttpGet]
        public async Task<bool> UpdateTargetDoc()
        {
            MongoDbHelper<MonthlyCustomerTarget1> mongoDbHelper = new MongoDbHelper<MonthlyCustomerTarget1>();
            MongoDbHelper<MonthlyCustomerTarget> mongoDbHelper1 = new MongoDbHelper<MonthlyCustomerTarget>();
            List<string> DocumentNames = new List<string> { "MonthlyTargetData_22020", "MonthlyTargetData_32020", "MonthlyTargetData_52020" };
            foreach (var item in DocumentNames)
            {
                string doc = "Old" + item + "_new";
                var Targets = mongoDbHelper.Select(x => x.Year == 2020, null, null, null, false, "", doc).FirstOrDefault();

                foreach (var item1 in Targets.CustomerTargets)
                {
                    MonthlyCustomerTarget MonthlyCustomerTarget = new MonthlyCustomerTarget
                    {
                        Bands = item1.Bands,
                        CityName = item1.CityName,
                        CreatedDate = item1.CreatedDate,
                        CurrentVolume = item1.CurrentVolume,
                        IsClaimed = item1.IsClaimed,
                        Levels = item1.Levels,
                        PendingVolume = item1.PendingVolume,
                        Skcode = item1.Skcode,
                        Source = item1.Source,
                        Target = item1.Target,
                        Volume = item1.Volume,
                        WarehouseId = item1.WarehouseId

                    };
                    mongoDbHelper1.Insert(MonthlyCustomerTarget, item);
                }
            }

            return true;
        }

        [Route("AssignCustomerCompanyTargetNotUse")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> AssignCustomerCompanyTarget()
        {
            var result = false;
            using (var con = new AuthContext())
            {
                DateTime startDate = DateTime.Now.Date;
                DateTime endDate = startDate.AddDays(-6).AddMilliseconds(-1);
                var subCatTargets = con.subCatTargets.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.EndDate >= endDate)
                    .Include(x => x.subCatTargetBrands.Select(y => y.SubCatTargetDetails.Select(z => z.SubCatTargetLevelBrands)))
                    .Include(x => x.subCatTargetBrands.Select(y => y.SubCatTargetDetails.Select(z => z.SubCatTargetLevelItems)))
                    .ToList();
                if (subCatTargets != null && subCatTargets.Any())
                {
                    DataTable cityDt = new DataTable();
                    cityDt.Columns.Add("IntValue");
                    foreach (var item in subCatTargets.GroupBy(x => x.CityId))
                    {
                        DataRow dr = cityDt.NewRow();
                        dr[0] = item.Key;
                        cityDt.Rows.Add(dr);
                    }

                    endDate = DateTime.Now.Date;
                    startDate = subCatTargets.Min(x => x.StartDate).AddDays(-45);
                    var param = new SqlParameter("cityIds", cityDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    var paramstartDate = new SqlParameter("startDate", startDate);
                    var paramendDate = new SqlParameter("endDate", endDate);
                    con.Database.CommandTimeout = 3000;
                    var CustomerOrderDetailDc = con.Database.SqlQuery<CustomerOrderDetailDc>("exec [GetOrderDetailForTarget] @startDate,@endDate,@cityIds", paramstartDate, paramendDate, param).ToList();
                    List<SubCategoryTargetCustomer> SubCategoryTargetCustomers = new List<SubCategoryTargetCustomer>();
                    var cityIds = subCatTargets.GroupBy(x => x.CityId).Select(x => x.Key).ToList();
                    var CustomerIds = con.Customers.Where(x =>
                    // (x.Mobile == "9977044088" || x.Mobile == "9691885247" || x.Mobile == "9926303404" || x.Mobile == "9617936306" || x.Mobile == "8370098839") &&
                    x.Active && !x.Deleted && x.Cityid.HasValue && cityIds.Contains(x.Cityid.Value)).Select(x => new { x.CustomerId, x.Cityid }).ToList();
                    if (CustomerIds != null && CustomerIds.Any())
                    {
                        foreach (var item in subCatTargets)
                        {
                            foreach (var customer in CustomerIds.Where(x => x.Cityid == item.CityId))
                            {
                                endDate = item.StartDate;
                                startDate = item.StartDate.AddDays(-45);
                                var CustomerOrderPreMonthSale = CustomerOrderDetailDc.Where(x => x.CustomerId == customer.CustomerId && x.CreatedDate >= startDate && x.CreatedDate <= endDate).ToList();
                                var CustomerOrderCurrentMonthSale = CustomerOrderDetailDc.Where(x => x.CustomerId == customer.CustomerId && x.CreatedDate >= item.StartDate && x.CreatedDate <= DateTime.Now).ToList();

                                var days = Convert.ToInt32((item.EndDate - item.StartDate).TotalDays);
                                foreach (var brand in item.subCatTargetBrands)
                                {
                                    List<int> brandIds = brand.BrandIds.Split(',').ToList().Select(x => Convert.ToInt32(x)).ToList();
                                    decimal preMonthSale = Convert.ToDecimal(CustomerOrderPreMonthSale.Where(x => brandIds.Contains(x.SubsubcategoryId) && x.CustomerId == customer.CustomerId).Sum(x => x.TotalAmt));
                                    decimal currentMonthSale = Convert.ToDecimal(CustomerOrderCurrentMonthSale.Where(x => brandIds.Contains(x.SubsubcategoryId) && x.CustomerId == customer.CustomerId).Sum(x => x.TotalAmt));
                                    var NoOfLineItem = CustomerOrderCurrentMonthSale.Where(x => brandIds.Contains(x.SubsubcategoryId) && x.CustomerId == customer.CustomerId).Select(x => x.ItemId).Distinct().Count();

                                    var PreMonthSale = (preMonthSale / 45) * days;
                                    var targetLevel = brand.SubCatTargetDetails.FirstOrDefault(x => x.SlabLowerLimit <= PreMonthSale
                                     && x.SlabUpperLimit >= PreMonthSale);
                                    if (targetLevel == null)
                                    {
                                        targetLevel = brand.SubCatTargetDetails.Where(x => x.SlabUpperLimit <= PreMonthSale).OrderByDescending(x => x.SlabUpperLimit).FirstOrDefault();
                                    }

                                    if (targetLevel != null)
                                    {
                                        var subCategoryTargetCustomer = new SubCategoryTargetCustomer
                                        {
                                            CreatedBy = 1,
                                            CreatedDate = DateTime.Now,
                                            CustomerId = customer.CustomerId,
                                            CurrentMonthSales = currentMonthSale,
                                            IsActive = true,
                                            IsDeleted = false,
                                            NoOfLineItem = NoOfLineItem,
                                            RequiredNoOfLineItem = targetLevel.NoOfLineItem,
                                            SubCatTargetDetailId = targetLevel.Id,
                                            SubCatTargetBrandId = targetLevel.SubCatTargetBrandId,
                                            IsClaimed = false,
                                            IsCompleted = false,
                                            PreviousMonthSales = preMonthSale,
                                            Target = targetLevel.ValueBy.ToLower() == "percent" ?
                                                     ((PreMonthSale * targetLevel.TargetbyValue / 100) < targetLevel.MinTargetValue ? targetLevel.MinTargetValue : PreMonthSale + (PreMonthSale * targetLevel.TargetbyValue / 100))
                                                     : targetLevel.TargetbyValue
                                        };
                                        List<TargetCustomerBrand> TargetCustomerBrands = new List<TargetCustomerBrand>();
                                        if (targetLevel.SubCatTargetLevelBrands != null && targetLevel.SubCatTargetLevelBrands.Any())
                                        {
                                            TargetCustomerBrands = targetLevel.SubCatTargetLevelBrands.Select(x =>
                                              new TargetCustomerBrand
                                              {
                                                  BrandId = x.BrandId,
                                                  Target = x.ValueType.ToLower() == "percent" ? subCategoryTargetCustomer.Target * x.Value / 100 : x.Value,
                                                  currentTarget = Convert.ToDecimal(CustomerOrderCurrentMonthSale.Where(y => y.SubsubcategoryId == x.BrandId).Sum(y => y.TotalAmt))
                                              }
                                            ).ToList();
                                        }
                                        subCategoryTargetCustomer.TargetCustomerBrands = TargetCustomerBrands;

                                        List<TargetCustomerItem> TargetCustomerItems = new List<TargetCustomerItem>();
                                        if (targetLevel.SubCatTargetLevelItems != null && targetLevel.SubCatTargetLevelItems.Any())
                                        {
                                            TargetCustomerItems = targetLevel.SubCatTargetLevelItems.Select(x =>
                                              new TargetCustomerItem
                                              {
                                                  SellingSku = x.SellingSku,
                                                  Target = x.ValueType.ToLower() == "percent" ? subCategoryTargetCustomer.Target * x.Value / 100 : x.Value,
                                                  currentTarget = Convert.ToDecimal(CustomerOrderCurrentMonthSale.Where(y => y.itemNumber == x.SellingSku).Sum(y => y.TotalAmt))
                                              }
                                            ).ToList();
                                        }
                                        subCategoryTargetCustomer.TargetCustomerItems = TargetCustomerItems;

                                        SubCategoryTargetCustomers.Add(subCategoryTargetCustomer);
                                    }
                                }
                            }
                        }

                        if (SubCategoryTargetCustomers != null && SubCategoryTargetCustomers.Any())
                        {
                            var customerids = SubCategoryTargetCustomers.Select(x => x.CustomerId).Distinct().ToList();
                            var SubCatTargetDetailIds = SubCategoryTargetCustomers.Select(x => x.SubCatTargetDetailId).Distinct().ToList();
                            var existsCustomerTargets = con.SubCategoryTargetCustomers.Where(x => customerids.Contains(x.CustomerId) && !x.IsTargetExpire && x.IsActive && SubCatTargetDetailIds.Contains(x.SubCatTargetDetailId)).Include(x => x.TargetCustomerBrands).Include(x => x.TargetCustomerItems).ToList();
                            ConcurrentBag<SubCategoryTargetCustomer> UpdateSubCategoryTargetCustomers = new ConcurrentBag<SubCategoryTargetCustomer>();
                            ParallelLoopResult parellelResult = Parallel.ForEach(SubCategoryTargetCustomers, (item) =>
                            {
                                //foreach (var item in SubCategoryTargetCustomers)
                                //{
                                var existsCustomerTarget = existsCustomerTargets.Where(x => x.CustomerId == item.CustomerId && x.SubCatTargetDetailId == item.SubCatTargetDetailId).FirstOrDefault();
                                if (existsCustomerTarget != null)
                                {
                                    if (!existsCustomerTarget.IsCompleted)
                                    {
                                        existsCustomerTarget.Target = item.Target;
                                        existsCustomerTarget.CurrentMonthSales = item.CurrentMonthSales;
                                        existsCustomerTarget.ModifiedBy = 1;
                                        existsCustomerTarget.ModifiedDate = DateTime.Now;
                                        existsCustomerTarget.RequiredNoOfLineItem = item.RequiredNoOfLineItem;
                                        existsCustomerTarget.NoOfLineItem = item.NoOfLineItem;
                                        existsCustomerTarget.IsCompleted = existsCustomerTarget.Target <= existsCustomerTarget.CurrentMonthSales && (existsCustomerTarget.RequiredNoOfLineItem > 0 ? existsCustomerTarget.RequiredNoOfLineItem <= existsCustomerTarget.NoOfLineItem : true);
                                        foreach (var targetbrand in existsCustomerTarget.TargetCustomerBrands)
                                        {
                                            if (item.TargetCustomerBrands.Any(x => x.BrandId == targetbrand.BrandId))
                                            {
                                                targetbrand.Target = item.TargetCustomerBrands.FirstOrDefault(x => x.BrandId == targetbrand.BrandId).Target;
                                                targetbrand.currentTarget = item.TargetCustomerBrands.FirstOrDefault(x => x.BrandId == targetbrand.BrandId).currentTarget;
                                            }
                                        }
                                        foreach (var targetItem in existsCustomerTarget.TargetCustomerItems)
                                        {
                                            if (item.TargetCustomerItems.Any(x => x.SellingSku == targetItem.SellingSku))
                                            {
                                                targetItem.Target = item.TargetCustomerItems.FirstOrDefault(x => x.SellingSku == targetItem.SellingSku).Target;
                                                targetItem.currentTarget = item.TargetCustomerItems.FirstOrDefault(x => x.SellingSku == targetItem.SellingSku).currentTarget;
                                            }
                                        }
                                        if (existsCustomerTarget.IsCompleted && existsCustomerTarget.TargetCustomerBrands.Any())
                                        {
                                            existsCustomerTarget.IsCompleted = existsCustomerTarget.TargetCustomerBrands.All(x => x.currentTarget >= x.Target);
                                        }
                                        if (existsCustomerTarget.IsCompleted && existsCustomerTarget.TargetCustomerItems.Any())
                                        {
                                            existsCustomerTarget.IsCompleted = existsCustomerTarget.TargetCustomerItems.All(x => x.currentTarget >= x.Target);
                                        }
                                        //con.Entry(existsCustomerTarget).State = EntityState.Modified;
                                        UpdateSubCategoryTargetCustomers.Add(existsCustomerTarget);
                                    }
                                    else
                                    {
                                        //code for next level target
                                        var brand = subCatTargets.Where(x => x.subCatTargetBrands.Any(y => y.Id == existsCustomerTarget.SubCatTargetBrandId)).SelectMany(x => x.subCatTargetBrands).FirstOrDefault(x => x.Id == existsCustomerTarget.SubCatTargetBrandId);

                                        var alreadyTakenTarget = existsCustomerTargets.Where(x => x.CustomerId == item.CustomerId && x.SubCatTargetBrandId == existsCustomerTarget.SubCatTargetBrandId).ToList();
                                        SubCategoryTargetCustomer lastTarget = null;
                                        var isLastTargetCompleted = true;
                                        foreach (var aTarget in alreadyTakenTarget)
                                        {
                                            if (!aTarget.IsCompleted)
                                            {
                                                isLastTargetCompleted = false;
                                                existsCustomerTarget.Target = item.Target;
                                                existsCustomerTarget.CurrentMonthSales = item.CurrentMonthSales;
                                                existsCustomerTarget.ModifiedBy = 1;
                                                existsCustomerTarget.ModifiedDate = DateTime.Now;
                                                existsCustomerTarget.RequiredNoOfLineItem = item.RequiredNoOfLineItem;
                                                existsCustomerTarget.NoOfLineItem = item.NoOfLineItem;
                                                existsCustomerTarget.IsCompleted = existsCustomerTarget.Target <= existsCustomerTarget.CurrentMonthSales && (existsCustomerTarget.RequiredNoOfLineItem > 0 ? existsCustomerTarget.RequiredNoOfLineItem <= existsCustomerTarget.NoOfLineItem : true);
                                                foreach (var targetbrand in existsCustomerTarget.TargetCustomerBrands)
                                                {
                                                    if (item.TargetCustomerBrands.Any(x => x.BrandId == targetbrand.BrandId))
                                                    {
                                                        targetbrand.Target = item.TargetCustomerBrands.FirstOrDefault(x => x.BrandId == targetbrand.BrandId).Target;
                                                        targetbrand.currentTarget = item.TargetCustomerBrands.FirstOrDefault(x => x.BrandId == targetbrand.BrandId).currentTarget;
                                                    }
                                                    con.Entry(targetbrand).State = EntityState.Modified;
                                                }
                                                foreach (var targetItem in existsCustomerTarget.TargetCustomerItems)
                                                {
                                                    if (item.TargetCustomerItems.Any(x => x.SellingSku == targetItem.SellingSku))
                                                    {
                                                        targetItem.Target = item.TargetCustomerItems.FirstOrDefault(x => x.SellingSku == targetItem.SellingSku).Target;
                                                        targetItem.currentTarget = item.TargetCustomerItems.FirstOrDefault(x => x.SellingSku == targetItem.SellingSku).currentTarget;
                                                    }
                                                    con.Entry(targetItem).State = EntityState.Modified;
                                                }
                                                if (existsCustomerTarget.IsCompleted && existsCustomerTarget.TargetCustomerBrands.Any())
                                                {
                                                    existsCustomerTarget.IsCompleted = existsCustomerTarget.TargetCustomerBrands.All(x => x.currentTarget >= x.Target);
                                                }
                                                if (existsCustomerTarget.IsCompleted && existsCustomerTarget.TargetCustomerItems.Any())
                                                {
                                                    existsCustomerTarget.IsCompleted = existsCustomerTarget.TargetCustomerItems.All(x => x.currentTarget >= x.Target);
                                                }

                                                UpdateSubCategoryTargetCustomers.Add(existsCustomerTarget);
                                            }
                                            lastTarget = aTarget;
                                        }

                                        if (isLastTargetCompleted && lastTarget != null)
                                        {
                                            var previouseTargetdetail = brand.SubCatTargetDetails.FirstOrDefault(x => x.Id == lastTarget.SubCatTargetDetailId);

                                            var CustomerOrderCurrentMonthSale = CustomerOrderDetailDc.Where(x => x.CustomerId == item.CustomerId && x.CreatedDate >= brand.SubCatTargets.StartDate && x.CreatedDate <= DateTime.Now).ToList();

                                            var targetLevel = brand.SubCatTargetDetails.Where(x => x.SlabLowerLimit >= previouseTargetdetail.SlabUpperLimit).OrderBy(x => x.SlabLowerLimit).FirstOrDefault(); ;

                                            if (targetLevel != null)
                                            {
                                                var subCategoryTargetCustomer = new SubCategoryTargetCustomer
                                                {
                                                    CreatedBy = 1,
                                                    CreatedDate = DateTime.Now,
                                                    CustomerId = item.CustomerId,
                                                    CurrentMonthSales = item.CurrentMonthSales,
                                                    IsActive = true,
                                                    IsDeleted = false,
                                                    SubCatTargetDetailId = targetLevel.Id,
                                                    SubCatTargetBrandId = targetLevel.SubCatTargetBrandId,
                                                    IsClaimed = false,
                                                    IsCompleted = false,
                                                    RequiredNoOfLineItem = item.RequiredNoOfLineItem,
                                                    NoOfLineItem = item.NoOfLineItem,
                                                    PreviousMonthSales = item.PreviousMonthSales,
                                                    Target = targetLevel.ValueBy.ToLower() == "percent" ?
                                                             ((targetLevel.SlabLowerLimit * targetLevel.TargetbyValue / 100) < targetLevel.MinTargetValue ? targetLevel.MinTargetValue : targetLevel.SlabLowerLimit + (targetLevel.SlabLowerLimit * targetLevel.TargetbyValue / 100))
                                                             : targetLevel.TargetbyValue
                                                };
                                                List<TargetCustomerBrand> TargetCustomerBrands = new List<TargetCustomerBrand>();
                                                if (targetLevel.SubCatTargetLevelBrands != null && targetLevel.SubCatTargetLevelBrands.Any())
                                                {
                                                    TargetCustomerBrands = targetLevel.SubCatTargetLevelBrands.Select(x =>
                                                      new TargetCustomerBrand
                                                      {
                                                          BrandId = x.BrandId,
                                                          Target = x.ValueType.ToLower() == "percent" ? subCategoryTargetCustomer.Target * x.Value / 100 : x.Value,
                                                          currentTarget = Convert.ToDecimal(CustomerOrderCurrentMonthSale.Where(y => y.SubsubcategoryId == x.BrandId).Sum(y => y.TotalAmt))
                                                      }
                                                    ).ToList();
                                                }
                                                subCategoryTargetCustomer.TargetCustomerBrands = TargetCustomerBrands;

                                                List<TargetCustomerItem> TargetCustomerItems = new List<TargetCustomerItem>();
                                                if (targetLevel.SubCatTargetLevelItems != null && targetLevel.SubCatTargetLevelItems.Any())
                                                {
                                                    TargetCustomerItems = targetLevel.SubCatTargetLevelItems.Select(x =>
                                                      new TargetCustomerItem
                                                      {
                                                          SellingSku = x.SellingSku,
                                                          Target = x.ValueType.ToLower() == "percent" ? subCategoryTargetCustomer.Target * x.Value / 100 : x.Value,
                                                          currentTarget = Convert.ToDecimal(CustomerOrderCurrentMonthSale.Where(y => y.itemNumber == x.SellingSku).Sum(y => y.TotalAmt))
                                                      }
                                                    ).ToList();
                                                }

                                                UpdateSubCategoryTargetCustomers.Add(subCategoryTargetCustomer);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    UpdateSubCategoryTargetCustomers.Add(item);
                                }

                                //}
                            });
                            if (parellelResult.IsCompleted && UpdateSubCategoryTargetCustomers.Any())
                            {
                                foreach (var item in UpdateSubCategoryTargetCustomers.Where(x => x.Id > 0))
                                {
                                    con.Entry(item).State = EntityState.Modified;
                                }
                                if (UpdateSubCategoryTargetCustomers.Where(x => x.Id == 0).Count() > 0)
                                {
                                    con.SubCategoryTargetCustomers.AddRange(UpdateSubCategoryTargetCustomers.Where(x => x.Id == 0));
                                }
                                con.Commit();
                            }
                        }
                    }
                }



                result = true;
            }

            return result;

        }


        [Route("AssignUpdateCustomerCompanyTarget")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> AssignUpdateCustomerCompanyTarget()
        {
            var result = false;
            ConcurrentBag<SubCategoryTargetCustomer> UpdateSubCategoryTargetCustomers = new ConcurrentBag<SubCategoryTargetCustomer>();
            ConcurrentBag<SubCategoryTargetCustomer> SubCategoryTargetCustomersCB = new ConcurrentBag<SubCategoryTargetCustomer>();
            List<SubCategoryTargetCustomer> SubCategoryTargetCustomers = new List<SubCategoryTargetCustomer>();
            List<SubCategoryTargetCustomer> SubCategoryTargetCustomersN = new List<SubCategoryTargetCustomer>();
            List<SubCategoryTargetCustomer> existsCustomerTargets = new List<SubCategoryTargetCustomer>();
            List<int> customerids = new List<int>();
            using (var con = new AuthContext())
            {
                DateTime startDate = DateTime.Now.Date;
                DateTime endDate = startDate.AddDays(-6).AddMilliseconds(-1);
                var subCatTargets = con.subCatTargets.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.EndDate >= endDate)
                    .Include(x => x.subCatTargetBrands.Select(y => y.SubCatTargetDetails.Select(z => z.SubCatTargetLevelBrands)))
                    .Include(x => x.subCatTargetBrands.Select(y => y.SubCatTargetDetails.Select(z => z.SubCatTargetLevelItems)))
                    .Include(x => x.SubCatTargetSpacificCusts)
                    .ToList();
                if (subCatTargets != null && subCatTargets.Any())
                {
                    DataTable cityDt = new DataTable();
                    cityDt.Columns.Add("IntValue");
                    foreach (var item in subCatTargets.GroupBy(x => x.CityId))
                    {
                        DataRow dr = cityDt.NewRow();
                        dr[0] = item.Key;
                        cityDt.Rows.Add(dr);
                    }

                    endDate = DateTime.Now.Date;
                    startDate = subCatTargets.Min(x => x.StartDate).AddDays(-45);
                    int totalRecord = 0;
                    if (con.Database.Connection.State != ConnectionState.Open)
                        con.Database.Connection.Open();
                    var param = new SqlParameter("cityIds", cityDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    var paramstartDate = new SqlParameter("startDate", startDate);
                    var paramendDate = new SqlParameter("endDate", endDate);
                    var paramskip = new SqlParameter("skip", SqlDbType.Int);
                    paramskip.Value = 0;
                    var paramtake = new SqlParameter("take", 10000);
                    var cmd = con.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetOrderDetailForTarget]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandTimeout = 3000;
                    cmd.Parameters.Add(paramstartDate);
                    cmd.Parameters.Add(paramendDate);
                    cmd.Parameters.Add(param);
                    cmd.Parameters.Add(paramskip);
                    cmd.Parameters.Add(paramtake);
                    var reader = cmd.ExecuteReader();
                    List<CustomerOrderDetailDc> CustomerOrderDetailDc = ((IObjectContextAdapter)con)
                    .ObjectContext
                    .Translate<CustomerOrderDetailDc>(reader).ToList();
                    // var CustomerOrderDetailDc = con.Database.SqlQuery<CustomerOrderDetailDc>("exec [GetOrderDetailForTarget] @startDate,@endDate,@cityIds", paramstartDate, paramendDate, param).ToList();

                    reader.NextResult();
                    if (reader.Read())
                    {
                        if ((reader["TotalCount"] is DBNull))
                        {
                            totalRecord = 0;
                        }
                        else
                        {
                            totalRecord = Convert.ToInt32(reader["TotalCount"]);
                        }
                    }

                    if (totalRecord - 10000 > 10000)
                    {
                        var skip = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(totalRecord - 10000) / 10000)) + 1;

                        ConcurrentBag<CustomerOrderDetailDc> concurrentBag = new ConcurrentBag<CustomerOrderDetailDc>();
                        ParallelLoopResult parallelResult2 = Parallel.For(1, skip, i =>
                        {
                            int progress = System.Threading.Interlocked.Increment(ref skip);
                            var param1 = new SqlParameter("cityIds", cityDt);
                            param.SqlDbType = SqlDbType.Structured;
                            param.TypeName = "dbo.IntValues";
                            var paramstartDate1 = new SqlParameter("startDate", startDate);
                            var paramendDate1 = new SqlParameter("endDate", endDate);
                            var paramskip1 = new SqlParameter("skip", i * 10000);
                            var paramtake1 = new SqlParameter("take", 10000);
                            cmd = con.Database.Connection.CreateCommand();
                            cmd.CommandText = "[dbo].[GetOrderDetailForTarget]";
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.CommandTimeout = 3000;
                            cmd.Parameters.Add(paramstartDate1);
                            cmd.Parameters.Add(paramendDate1);
                            cmd.Parameters.Add(param1);
                            cmd.Parameters.Add(paramskip1);
                            cmd.Parameters.Add(paramtake1);
                            reader = cmd.ExecuteReader();
                            List<CustomerOrderDetailDc> CustomerOrderDetailDc1 = ((IObjectContextAdapter)con)
                            .ObjectContext
                            .Translate<CustomerOrderDetailDc>(reader).ToList();
                            foreach (var cust in CustomerOrderDetailDc1)
                            {
                                concurrentBag.Add(cust);
                            }

                        });

                        if (parallelResult2.IsCompleted)
                        {
                            CustomerOrderDetailDc.AddRange(concurrentBag.ToList());
                        }
                    }

                    var cityIds = subCatTargets.GroupBy(x => x.CityId).Select(x => x.Key).ToList();
                    var CustomerIds = con.Customers.Where(x => /*( x.Mobile == "4455555558") &&*/
                    /* (x.Mobile == "9532547678" ||  || x.Mobile == "2244777000")*/
                    x.Active && !x.Deleted && x.Cityid.HasValue && cityIds.Contains(x.Cityid.Value)).Select(x => new { x.CustomerId, x.Cityid }).ToList();
                    if (CustomerIds != null && CustomerIds.Any())
                    {
                        // foreach (var customer in CustomerIds)
                        ParallelLoopResult parellelResult = Parallel.ForEach(CustomerIds, (customer) =>
                        {
                            foreach (var item in subCatTargets.Where(x => x.CityId == customer.Cityid && (!x.IsCustomerSpacific || (x.IsCustomerSpacific && x.SubCatTargetSpacificCusts.Any(y => y.CustomerId == customer.CustomerId && y.IsActive)))))
                            {

                                endDate = item.StartDate.Date;
                                startDate = item.StartDate.Date.AddDays(-45);
                                var orderEndDate = item.EndDate;
                                var CustomerOrderPreMonthSale = CustomerOrderDetailDc.Where(x => x.CustomerId == customer.CustomerId && x.CreatedDate >= startDate && x.CreatedDate <= endDate).ToList();
                                var CustomerOrderCurrentMonthSale = CustomerOrderDetailDc.Where(x => x.CustomerId == customer.CustomerId && x.CreatedDate >= item.StartDate && x.CreatedDate <= orderEndDate).ToList();

                                var days = Convert.ToInt32((item.EndDate - item.StartDate).TotalDays);

                                foreach (var brand in item.subCatTargetBrands)
                                {
                                    List<int> brandIds = brand.BrandIds.Split(',').ToList().Select(x => Convert.ToInt32(x)).ToList();
                                    decimal preMonthSale = Convert.ToDecimal(CustomerOrderPreMonthSale.Where(x => brandIds.Contains(x.SubsubcategoryId) && x.CustomerId == customer.CustomerId).Sum(x => x.TotalAmt));
                                    decimal currentMonthSale = Convert.ToDecimal(CustomerOrderCurrentMonthSale.Where(x => brandIds.Contains(x.SubsubcategoryId) && x.CustomerId == customer.CustomerId).Sum(x => x.TotalAmt));
                                    var NoOfLineItem = CustomerOrderCurrentMonthSale.Where(x => brandIds.Contains(x.SubsubcategoryId) && x.CustomerId == customer.CustomerId).Select(x => x.ItemId).Distinct().Count();

                                    var PreMonthSale = (preMonthSale / 45) * days;

                                    var targetLevel = brand.SubCatTargetDetails.FirstOrDefault(x => x.SlabLowerLimit <= PreMonthSale
                                    && x.SlabUpperLimit >= PreMonthSale);
                                    if (targetLevel == null)
                                    {
                                        targetLevel = brand.SubCatTargetDetails.Where(x => x.SlabUpperLimit <= PreMonthSale).OrderByDescending(x => x.SlabUpperLimit).FirstOrDefault();
                                    }

                                    if (targetLevel != null)
                                    {
                                        var Target = targetLevel.ValueBy.ToLower() == "percent" ?
                                                      ((PreMonthSale * targetLevel.TargetbyValue / 100) < targetLevel.MinTargetValue ? targetLevel.MinTargetValue : PreMonthSale + (PreMonthSale * targetLevel.TargetbyValue / 100))
                                                      : targetLevel.TargetbyValue;

                                        var isNewTargetgiven = false;


                                        var subCategoryTargetCustomer = new SubCategoryTargetCustomer();
                                        //{
                                        //    CreatedBy = 1,
                                        //    CreatedDate = DateTime.Now,
                                        //    CustomerId = customer.CustomerId,
                                        //    CurrentMonthSales = currentMonthSale,
                                        //    IsActive = true,
                                        //    IsDeleted = false,
                                        //    NoOfLineItem = NoOfLineItem,
                                        //    RequiredNoOfLineItem = targetLevel.NoOfLineItem,
                                        //    SubCatTargetDetailId = targetLevel.Id,
                                        //    SubCatTargetBrandId = targetLevel.SubCatTargetBrandId,
                                        //    IsClaimed = false,
                                        //    IsCompleted = false,
                                        //    PreviousMonthSales = preMonthSale,
                                        //    Target = targetLevel.ValueBy.ToLower() == "percent" ?
                                        //           ((PreMonthSale * targetLevel.TargetbyValue / 100) < targetLevel.MinTargetValue ? targetLevel.MinTargetValue : PreMonthSale + (PreMonthSale * targetLevel.TargetbyValue / 100))
                                        //           : targetLevel.TargetbyValue
                                        //};

                                        List<TargetCustomerBrand> TargetCustomerBrands = new List<TargetCustomerBrand>();
                                        //if (targetLevel.SubCatTargetLevelBrands != null && targetLevel.SubCatTargetLevelBrands.Any())
                                        //{
                                        //    TargetCustomerBrands = targetLevel.SubCatTargetLevelBrands.Select(x =>
                                        //      new TargetCustomerBrand
                                        //      {
                                        //          BrandId = x.BrandId,
                                        //          Target = x.ValueType.ToLower() == "percent" ? subCategoryTargetCustomer.Target * x.Value / 100 : x.Value,
                                        //          currentTarget = Convert.ToDecimal(CustomerOrderCurrentMonthSale.Where(y => y.SubsubcategoryId == x.BrandId).Sum(y => y.TotalAmt))
                                        //      }
                                        //    ).ToList();
                                        //}
                                        //subCategoryTargetCustomer.TargetCustomerBrands = TargetCustomerBrands;


                                        List<TargetCustomerItem> TargetCustomerItems = new List<TargetCustomerItem>();
                                        //if (targetLevel.SubCatTargetLevelItems != null && targetLevel.SubCatTargetLevelItems.Any())
                                        //{
                                        //    TargetCustomerItems = targetLevel.SubCatTargetLevelItems.Select(x =>
                                        //      new TargetCustomerItem
                                        //      {
                                        //          SellingSku = x.SellingSku,
                                        //          Target = x.ValueType.ToLower() == "percent" ? subCategoryTargetCustomer.Target * x.Value / 100 : x.Value,
                                        //          currentTarget = Convert.ToDecimal(CustomerOrderCurrentMonthSale.Where(y => y.itemNumber == x.SellingSku).Sum(y => y.TotalAmt))
                                        //      }
                                        //    ).ToList();
                                        //}
                                        //subCategoryTargetCustomer.TargetCustomerItems = TargetCustomerItems;
                                        //subCategoryTargetCustomer.IsCompleted = subCategoryTargetCustomer.Target <= subCategoryTargetCustomer.CurrentMonthSales && (subCategoryTargetCustomer.RequiredNoOfLineItem > 0 ? subCategoryTargetCustomer.RequiredNoOfLineItem <= subCategoryTargetCustomer.NoOfLineItem : true);

                                        //if (subCategoryTargetCustomer.IsCompleted && subCategoryTargetCustomer.TargetCustomerBrands.Any())
                                        //{
                                        //    subCategoryTargetCustomer.IsCompleted = subCategoryTargetCustomer.TargetCustomerBrands.All(x => x.currentTarget >= x.Target);
                                        //}
                                        //if (subCategoryTargetCustomer.IsCompleted && subCategoryTargetCustomer.TargetCustomerItems.Any())
                                        //{
                                        //    subCategoryTargetCustomer.IsCompleted = subCategoryTargetCustomer.TargetCustomerItems.All(x => x.currentTarget >= x.Target);
                                        //}
                                        //if (!subCategoryTargetCustomer.IsCompleted)
                                        //{
                                        //    isNewTargetgiven = true;
                                        //}

                                        //SubCategoryTargetCustomers.Add(subCategoryTargetCustomer);
                                        //&& subCategoryTargetCustomer.IsCompleted
                                        //if (Target < currentMonthSale)
                                        //{
                                        foreach (var completedTarget in brand.SubCatTargetDetails
                                            .Where(x => x.SlabUpperLimit <= currentMonthSale).OrderBy(x => x.SlabLowerLimit).ToList())
                                        {
                                            subCategoryTargetCustomer = new SubCategoryTargetCustomer
                                            {
                                                CreatedBy = 1,
                                                CreatedDate = DateTime.Now,
                                                CustomerId = customer.CustomerId,
                                                CurrentMonthSales = currentMonthSale,
                                                IsActive = true,
                                                IsDeleted = false,
                                                NoOfLineItem = NoOfLineItem,
                                                RequiredNoOfLineItem = completedTarget.NoOfLineItem,
                                                SubCatTargetDetailId = completedTarget.Id,
                                                SubCatTargetBrandId = completedTarget.SubCatTargetBrandId,
                                                IsClaimed = false,
                                                IsCompleted = false,
                                                PreviousMonthSales = preMonthSale,
                                                Target = completedTarget.ValueBy.ToLower() == "percent" ?
                                                 ((PreMonthSale * completedTarget.TargetbyValue / 100) < completedTarget.MinTargetValue ? completedTarget.MinTargetValue : PreMonthSale + (PreMonthSale * completedTarget.TargetbyValue / 100))
                                                 : completedTarget.TargetbyValue
                                            };

                                            TargetCustomerBrands = new List<TargetCustomerBrand>();
                                            if (completedTarget.SubCatTargetLevelBrands != null && completedTarget.SubCatTargetLevelBrands.Any())
                                            {
                                                TargetCustomerBrands = completedTarget.SubCatTargetLevelBrands.Select(x =>
                                                  new TargetCustomerBrand
                                                  {
                                                      BrandId = x.BrandId,
                                                      Target = x.ValueType.ToLower() == "percent" ? subCategoryTargetCustomer.Target * x.Value / 100 : x.Value,
                                                      currentTarget = Convert.ToDecimal(CustomerOrderCurrentMonthSale.Where(y => y.SubsubcategoryId == x.BrandId).Sum(y => y.TotalAmt))
                                                  }
                                                ).ToList();
                                            }
                                            subCategoryTargetCustomer.TargetCustomerBrands = TargetCustomerBrands;


                                            TargetCustomerItems = new List<TargetCustomerItem>();
                                            if (completedTarget.SubCatTargetLevelItems != null && completedTarget.SubCatTargetLevelItems.Any())
                                            {
                                                TargetCustomerItems = completedTarget.SubCatTargetLevelItems.Select(x =>
                                                  new TargetCustomerItem
                                                  {
                                                      SellingSku = x.SellingSku,
                                                      Target = x.ValueType.ToLower() == "percent" ? subCategoryTargetCustomer.Target * x.Value / 100 : x.Value,
                                                      currentTarget = Convert.ToDecimal(CustomerOrderCurrentMonthSale.Where(y => y.itemNumber == x.SellingSku).Sum(y => y.TotalAmt))
                                                  }
                                                ).ToList();
                                            }
                                            subCategoryTargetCustomer.TargetCustomerItems = TargetCustomerItems;


                                            subCategoryTargetCustomer.IsCompleted = subCategoryTargetCustomer.Target <= subCategoryTargetCustomer.CurrentMonthSales && (subCategoryTargetCustomer.RequiredNoOfLineItem > 0 ? subCategoryTargetCustomer.RequiredNoOfLineItem <= subCategoryTargetCustomer.NoOfLineItem : true);

                                            if (subCategoryTargetCustomer.IsCompleted && subCategoryTargetCustomer.TargetCustomerBrands.Any())
                                            {
                                                subCategoryTargetCustomer.IsCompleted = subCategoryTargetCustomer.TargetCustomerBrands.All(x => x.currentTarget >= x.Target);
                                            }
                                            if (subCategoryTargetCustomer.IsCompleted && subCategoryTargetCustomer.TargetCustomerItems.Any())
                                            {
                                                subCategoryTargetCustomer.IsCompleted = subCategoryTargetCustomer.TargetCustomerItems.All(x => x.currentTarget >= x.Target);
                                            }

                                            if (subCategoryTargetCustomer != null)
                                                SubCategoryTargetCustomersCB.Add(subCategoryTargetCustomer);
                                            //SubCategoryTargetCustomers.Add(subCategoryTargetCustomer);

                                            if (!subCategoryTargetCustomer.IsCompleted)
                                            {
                                                //SubCategoryTargetCustomers.AddRange(SubCategoryTargetCustomersCB.ToList());
                                                isNewTargetgiven = true;
                                                break;
                                            }
                                        }
                                        // }
                                        if (!isNewTargetgiven)
                                        {
                                            var nextTarget = brand.SubCatTargetDetails.Where(x => x.SlabLowerLimit <= currentMonthSale
                                                     && x.SlabUpperLimit > currentMonthSale).OrderBy(x => x.SlabUpperLimit).FirstOrDefault();

                                            if (nextTarget != null)
                                            {
                                                subCategoryTargetCustomer = new SubCategoryTargetCustomer
                                                {
                                                    CreatedBy = 1,
                                                    CreatedDate = DateTime.Now,
                                                    CustomerId = customer.CustomerId,
                                                    CurrentMonthSales = currentMonthSale,
                                                    IsActive = true,
                                                    IsDeleted = false,
                                                    NoOfLineItem = NoOfLineItem,
                                                    RequiredNoOfLineItem = nextTarget.NoOfLineItem,
                                                    SubCatTargetDetailId = nextTarget.Id,
                                                    SubCatTargetBrandId = nextTarget.SubCatTargetBrandId,
                                                    IsClaimed = false,
                                                    IsCompleted = false,
                                                    PreviousMonthSales = preMonthSale,
                                                    Target = nextTarget.ValueBy.ToLower() == "percent" ?
                                                         ((PreMonthSale * nextTarget.TargetbyValue / 100) < nextTarget.MinTargetValue ? nextTarget.MinTargetValue : PreMonthSale + (PreMonthSale * nextTarget.TargetbyValue / 100))
                                                         : nextTarget.TargetbyValue
                                                };

                                                TargetCustomerBrands = new List<TargetCustomerBrand>();
                                                if (nextTarget.SubCatTargetLevelBrands != null && nextTarget.SubCatTargetLevelBrands.Any())
                                                {
                                                    TargetCustomerBrands = nextTarget.SubCatTargetLevelBrands.Select(x =>
                                                      new TargetCustomerBrand
                                                      {
                                                          BrandId = x.BrandId,
                                                          Target = x.ValueType.ToLower() == "percent" ? subCategoryTargetCustomer.Target * x.Value / 100 : x.Value,
                                                          currentTarget = Convert.ToDecimal(CustomerOrderCurrentMonthSale.Where(y => y.SubsubcategoryId == x.BrandId).Sum(y => y.TotalAmt))
                                                      }
                                                    ).ToList();
                                                }
                                                subCategoryTargetCustomer.TargetCustomerBrands = TargetCustomerBrands;


                                                TargetCustomerItems = new List<TargetCustomerItem>();
                                                if (nextTarget.SubCatTargetLevelItems != null && nextTarget.SubCatTargetLevelItems.Any())
                                                {
                                                    TargetCustomerItems = nextTarget.SubCatTargetLevelItems.Select(x =>
                                                      new TargetCustomerItem
                                                      {
                                                          SellingSku = x.SellingSku,
                                                          Target = x.ValueType.ToLower() == "percent" ? subCategoryTargetCustomer.Target * x.Value / 100 : x.Value,
                                                          currentTarget = Convert.ToDecimal(CustomerOrderCurrentMonthSale.Where(y => y.itemNumber == x.SellingSku).Sum(y => y.TotalAmt))
                                                      }
                                                    ).ToList();
                                                }
                                                subCategoryTargetCustomer.TargetCustomerItems = TargetCustomerItems;
                                                subCategoryTargetCustomer.IsCompleted = subCategoryTargetCustomer.Target <= subCategoryTargetCustomer.CurrentMonthSales && (subCategoryTargetCustomer.RequiredNoOfLineItem > 0 ? subCategoryTargetCustomer.RequiredNoOfLineItem <= subCategoryTargetCustomer.NoOfLineItem : true);

                                                if (subCategoryTargetCustomer.IsCompleted && subCategoryTargetCustomer.TargetCustomerBrands.Any())
                                                {
                                                    subCategoryTargetCustomer.IsCompleted = subCategoryTargetCustomer.TargetCustomerBrands.All(x => x.currentTarget >= x.Target);
                                                }
                                                if (subCategoryTargetCustomer.IsCompleted && subCategoryTargetCustomer.TargetCustomerItems.Any())
                                                {
                                                    subCategoryTargetCustomer.IsCompleted = subCategoryTargetCustomer.TargetCustomerItems.All(x => x.currentTarget >= x.Target);
                                                }
                                                if (subCategoryTargetCustomer != null)
                                                    SubCategoryTargetCustomersCB.Add(subCategoryTargetCustomer);
                                            }
                                        }
                                    }
                                }
                            }
                        });


                        if (parellelResult.IsCompleted)
                        {
                            SubCategoryTargetCustomers.AddRange(SubCategoryTargetCustomersCB.ToList());
                        }
                    }
                }
                //if (SubCategoryTargetCustomers != null && SubCategoryTargetCustomers.Any())
                //{
                //customerids = SubCategoryTargetCustomers.Where(x => x.CustomerId != null && x.CustomerId > 0).Select(x => x.CustomerId).Distinct().ToList();
                //var SubCatTargetDetailIds = SubCategoryTargetCustomers.Select(x => x.SubCatTargetDetailId).Distinct().ToList();
                //existsCustomerTargets = con.SubCategoryTargetCustomers.Where(x => customerids.Contains(x.CustomerId) && !x.IsTargetExpire && x.IsActive && SubCatTargetDetailIds.Contains(x.SubCatTargetDetailId)).Include(x => x.TargetCustomerBrands).Include(x => x.TargetCustomerItems).ToList();
                //}


                if (SubCategoryTargetCustomers != null && SubCategoryTargetCustomers.Any())
                {
                    //using (var con = new AuthContext())
                    //{
                    SubCategoryTargetCustomers.ForEach(x =>
                    {
                        if (x != null)
                            SubCategoryTargetCustomersN.Add(x);
                    });
                    SubCategoryTargetCustomers = new List<SubCategoryTargetCustomer>();
                    SubCategoryTargetCustomers = SubCategoryTargetCustomersN;
                    customerids = SubCategoryTargetCustomers.Where(x => x != null && x.CustomerId != null && x.CustomerId > 0).Select(x => x.CustomerId).Distinct().ToList();
                    var SubCatTargetDetailIdss = SubCategoryTargetCustomers.Select(x => x.SubCatTargetDetailId).Distinct().ToList();

                    int customerTotalRecords = customerids.Count();
                    if (customerTotalRecords - 5000 > 5000)
                    {
                        var i = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(customerTotalRecords - 5000) / 5000)) + 1;
                        int skipCustomers = 0;
                        ConcurrentBag<SubCategoryTargetCustomer> SubCategoryTargetCustomerCB = new ConcurrentBag<SubCategoryTargetCustomer>();
                        List<SubCategoryTargetCustomer> SubCategoryTargetCustomerFinal = new List<SubCategoryTargetCustomer>();
                        for (int k = 0; k < i; k++)
                        {

                            existsCustomerTargets = new List<SubCategoryTargetCustomer>();
                            List<int> customeridsNew = customerids.Skip(skipCustomers).Take(5000).ToList();
                            existsCustomerTargets = con.SubCategoryTargetCustomers.Where(x => customeridsNew.Contains(x.CustomerId) && !x.IsTargetExpire && x.IsActive && SubCatTargetDetailIdss.Contains(x.SubCatTargetDetailId)).Include(x => x.TargetCustomerBrands).Include(x => x.TargetCustomerItems).ToList();
                            if (existsCustomerTargets != null && existsCustomerTargets.Any())
                                SubCategoryTargetCustomerFinal.AddRange(existsCustomerTargets);
                            skipCustomers = skipCustomers + 5000;
                        }
                        existsCustomerTargets = SubCategoryTargetCustomerFinal;
                    }
                    else
                    {
                        existsCustomerTargets = con.SubCategoryTargetCustomers.Where(x => customerids.Contains(x.CustomerId) && !x.IsTargetExpire && x.IsActive && SubCatTargetDetailIdss.Contains(x.SubCatTargetDetailId)).Include(x => x.TargetCustomerBrands).Include(x => x.TargetCustomerItems).ToList();
                    }
                    /*
                    //****** Parallel Loop
                     List<SubCategoryTargetCustomer> SubCategoryTargetCustomerFinal = new List<SubCategoryTargetCustomer>();
                        ConcurrentBag<SubCategoryTargetCustomer> existsCustomerTargetsCB = new ConcurrentBag<SubCategoryTargetCustomer>();
                        ParallelLoopResult parellelCustomerResult = Parallel.For(1, skipCustomers, i =>
                        {
                            int progress = System.Threading.Interlocked.Increment(ref skipCustomers);
                            int skip = i * 5000;
                            int take = 5000;
                            existsCustomerTargets = new List<SubCategoryTargetCustomer>();
                            List<int> customeridsNew = customerids.Skip(skip).Take(take).ToList();
                            existsCustomerTargets = con.SubCategoryTargetCustomers.Where(x => customeridsNew.Contains(x.CustomerId) && !x.IsTargetExpire && x.IsActive && SubCatTargetDetailIdss.Contains(x.SubCatTargetDetailId)).Include(x => x.TargetCustomerBrands).Include(x => x.TargetCustomerItems).ToList();
                            if (existsCustomerTargets != null && existsCustomerTargets.Any())
                            {
                                existsCustomerTargets.ForEach(x =>
                                {
                                    if (x != null)
                                        existsCustomerTargetsCB.Add(x);
                                });

                            }

                        });
                        if (parellelCustomerResult.IsCompleted && existsCustomerTargetsCB.Any())
                        {
                            SubCategoryTargetCustomerFinal.AddRange(existsCustomerTargetsCB);
                            existsCustomerTargets = new List<SubCategoryTargetCustomer>();
                            existsCustomerTargets = SubCategoryTargetCustomerFinal;
                        }
                    //***********
                    */
                    //}
                    ParallelLoopResult parellelResult = Parallel.ForEach(SubCategoryTargetCustomers, (item) =>
                    {
                    //    foreach (var item in SubCategoryTargetCustomers)
                    //{
                    var existsCustomerTarget = existsCustomerTargets.Where(x => x.CustomerId == item.CustomerId && x.SubCatTargetDetailId == item.SubCatTargetDetailId).FirstOrDefault();
                        if (existsCustomerTarget != null)
                        {
                            if (!existsCustomerTarget.IsClaimed || !existsCustomerTarget.IsTargetExpire)
                            {
                                existsCustomerTarget.Target = item.Target;
                                existsCustomerTarget.CurrentMonthSales = item.CurrentMonthSales;
                                existsCustomerTarget.ModifiedBy = 1;
                                existsCustomerTarget.ModifiedDate = DateTime.Now;
                                existsCustomerTarget.RequiredNoOfLineItem = item.RequiredNoOfLineItem;
                                existsCustomerTarget.NoOfLineItem = item.NoOfLineItem;
                                existsCustomerTarget.IsCompleted = item.IsCompleted;
                                existsCustomerTarget.GuildId = item.CustomerId.ToString() + "_" + item.SubCatTargetDetailId.ToString() + "_" + item.SubCatTargetBrandId.ToString();

                                foreach (var targetbrand in existsCustomerTarget.TargetCustomerBrands)
                                {
                                    if (item.TargetCustomerBrands.Any(x => x.BrandId == targetbrand.BrandId))
                                    {
                                        targetbrand.Target = item.TargetCustomerBrands.FirstOrDefault(x => x.BrandId == targetbrand.BrandId).Target;
                                        targetbrand.currentTarget = item.TargetCustomerBrands.FirstOrDefault(x => x.BrandId == targetbrand.BrandId).currentTarget;
                                        targetbrand.GuildId = existsCustomerTarget.GuildId;
                                    }
                                }
                                foreach (var targetItem in existsCustomerTarget.TargetCustomerItems)
                                {
                                    if (item.TargetCustomerItems.Any(x => x.SellingSku == targetItem.SellingSku))
                                    {
                                        targetItem.Target = item.TargetCustomerItems.FirstOrDefault(x => x.SellingSku == targetItem.SellingSku).Target;
                                        targetItem.currentTarget = item.TargetCustomerItems.FirstOrDefault(x => x.SellingSku == targetItem.SellingSku).currentTarget;
                                        targetItem.GuildId = existsCustomerTarget.GuildId;
                                    }
                                }

                                UpdateSubCategoryTargetCustomers.Add(existsCustomerTarget);
                            }

                        }
                        else
                        {
                            item.GuildId = item.CustomerId.ToString() + "_" + item.SubCatTargetDetailId.ToString() + "_" + item.SubCatTargetBrandId.ToString();
                            item.TargetCustomerItems.ForEach(x => x.GuildId = item.GuildId);
                            item.TargetCustomerBrands.ForEach(x => x.GuildId = item.GuildId);
                            UpdateSubCategoryTargetCustomers.Add(item);
                        }
                    });

                    if (parellelResult.IsCompleted && UpdateSubCategoryTargetCustomers.Any())
                    {
                        if (UpdateSubCategoryTargetCustomers.Where(x => x.Id > 0).Count() > 0)
                        {
                            var tblTargetCustomers = UpdateSubCategoryTargetCustomers.Where(x => x.Id > 0).Select(x =>
                               new tblSubCategoryTargetCustomer
                               {
                                   CreatedBy = x.CreatedBy,
                                   CreatedDate = x.CreatedDate,
                                   CurrentMonthSales = x.CurrentMonthSales,
                                   CustomerId = x.CustomerId,
                                   GuildId = x.GuildId,
                                   Id = x.Id,
                                   IsActive = x.IsActive,
                                   IsClaimed = x.IsClaimed,
                                   IsCompleted = x.IsCompleted,
                                   IsDeleted = x.IsDeleted,
                                   IsTargetExpire = x.IsTargetExpire,
                                   ModifiedBy = x.ModifiedBy,
                                   ModifiedDate = x.ModifiedDate,
                                   NoOfLineItem = x.NoOfLineItem,
                                   PreviousMonthSales = x.PreviousMonthSales,
                                   RequiredNoOfLineItem = x.RequiredNoOfLineItem,
                                   SubCatTargetBrandId = x.SubCatTargetBrandId,
                                   SubCatTargetDetailId = x.SubCatTargetDetailId,
                                   Target = x.Target
                               }).ToList();
                            var tblTargetCustomerbrands = UpdateSubCategoryTargetCustomers.Where(x => x.Id > 0).SelectMany(x => x.TargetCustomerBrands).Select(x =>
                               new tblTargetCustomerBrand
                               {
                                   BrandId = x.BrandId,
                                   currentTarget = x.currentTarget,
                                   GuildId = x.GuildId,
                                   Id = x.Id,
                                   SubCategoryTargetCustomerId = x.SubCategoryTargetCustomerId,
                                   Target = x.Target
                               }).ToList();
                            var tblTargetCustomerItems = UpdateSubCategoryTargetCustomers.Where(x => x.Id > 0).SelectMany(x => x.TargetCustomerItems).Select(x =>
                              new tblTargetCustomerItem
                              {
                                  currentTarget = x.currentTarget,
                                  GuildId = x.GuildId,
                                  Id = x.Id,
                                  SellingSku = x.SellingSku,
                                  Target = x.Target,
                                  SubCategoryTargetCustomerId = x.SubCategoryTargetCustomerId
                              }).ToList();
                            var bulkTarget = new BulkOperations();
                            bulkTarget.Setup<tblSubCategoryTargetCustomer>(x => x.ForCollection(tblTargetCustomers))
                                .WithTable("SubCategoryTargetCustomers")
                                .WithBulkCopyBatchSize(4000)
                                 .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                                 .WithSqlCommandTimeout(720) // Default is 600 seconds
                                 .AddColumn(x => x.Target)
                                 .AddColumn(x => x.CurrentMonthSales)
                                 .AddColumn(x => x.ModifiedBy)
                                 .AddColumn(x => x.ModifiedDate)
                                 .AddColumn(x => x.RequiredNoOfLineItem)
                                 .AddColumn(x => x.NoOfLineItem)
                                 .AddColumn(x => x.IsCompleted)
                                 .AddColumn(x => x.GuildId)
                                .BulkUpdate()
                                .SetIdentityColumn(x => x.Id)
                                .MatchTargetOn(x => x.Id);
                            bulkTarget.CommitTransaction("AuthContext");

                            if (tblTargetCustomerbrands != null && tblTargetCustomerbrands.Any())
                            {
                                var bulkBrand = new BulkOperations();
                                bulkBrand.Setup<tblTargetCustomerBrand>(x => x.ForCollection(tblTargetCustomerbrands))
                                    .WithTable("TargetCustomerBrands")
                                    .WithBulkCopyBatchSize(4000)
                                    .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                                    .WithSqlCommandTimeout(720) // Default is 600 seconds
                                    .AddColumn(x => x.GuildId)
                                    .AddColumn(x => x.Target)
                                    .AddColumn(x => x.currentTarget)
                                    .BulkUpdate()
                                    .SetIdentityColumn(x => x.Id)
                                    .MatchTargetOn(x => x.Id);
                                bulkBrand.CommitTransaction("AuthContext");
                            }

                            if (tblTargetCustomerItems != null && tblTargetCustomerItems.Any())
                            {
                                var bulkItem = new BulkOperations();
                                bulkItem.Setup<tblTargetCustomerItem>(x => x.ForCollection(tblTargetCustomerItems))
                                    .WithTable("TargetCustomerItems")
                                    .WithBulkCopyBatchSize(4000)
                                    .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                                    .WithSqlCommandTimeout(720) // Default is 600 seconds
                                    .AddColumn(x => x.GuildId)
                                    .AddColumn(x => x.Target)
                                    .AddColumn(x => x.currentTarget)
                                    .BulkUpdate()
                                    .SetIdentityColumn(x => x.Id)
                                    .MatchTargetOn(x => x.Id);
                                bulkItem.CommitTransaction("AuthContext");
                            }
                        }
                        if (UpdateSubCategoryTargetCustomers.Where(x => x.Id == 0).Count() > 0)
                        {
                            var tblTargetCustomers = UpdateSubCategoryTargetCustomers.Where(x => x.Id == 0).Select(x =>
                               new tblSubCategoryTargetCustomer
                               {
                                   CreatedBy = x.CreatedBy,
                                   CreatedDate = x.CreatedDate,
                                   CurrentMonthSales = x.CurrentMonthSales,
                                   CustomerId = x.CustomerId,
                                   GuildId = x.GuildId,
                                   Id = x.Id,
                                   IsActive = x.IsActive,
                                   IsClaimed = x.IsClaimed,
                                   IsCompleted = x.IsCompleted,
                                   IsDeleted = x.IsDeleted,
                                   IsTargetExpire = x.IsTargetExpire,
                                   ModifiedBy = x.ModifiedBy,
                                   ModifiedDate = x.ModifiedDate,
                                   NoOfLineItem = x.NoOfLineItem,
                                   PreviousMonthSales = x.PreviousMonthSales,
                                   RequiredNoOfLineItem = x.RequiredNoOfLineItem,
                                   SubCatTargetBrandId = x.SubCatTargetBrandId,
                                   SubCatTargetDetailId = x.SubCatTargetDetailId,
                                   Target = x.Target
                               }).ToList();

                            var TargetCustomers = new BulkOperations();
                            TargetCustomers.Setup<tblSubCategoryTargetCustomer>(x => x.ForCollection(tblTargetCustomers))
                                .WithTable("SubCategoryTargetCustomers")
                                .WithBulkCopyBatchSize(4000)
                                .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                                .WithSqlCommandTimeout(720) // Default is 600 seconds
                                .AddAllColumns()
                                .BulkInsert();
                            TargetCustomers.CommitTransaction("AuthContext");

                            var SubCatTargetDetailIds = UpdateSubCategoryTargetCustomers.Where(x => x.Id == 0).Select(x => x.SubCatTargetDetailId).ToList();
                            List<tblTargetCustomerBrand> tblTargetCustomerbrands = new List<tblTargetCustomerBrand>();
                            List<tblTargetCustomerItem> tblTargetCustomerItems = new List<tblTargetCustomerItem>();
                            //using (var con = new AuthContext())
                            //{
                            var InsertedCustomerTargets = con.SubCategoryTargetCustomers.Where(x => customerids.Contains(x.CustomerId) && SubCatTargetDetailIds.Contains(x.SubCatTargetDetailId)).Select(x => new { x.Id, x.GuildId }).ToList();

                            tblTargetCustomerbrands = UpdateSubCategoryTargetCustomers.Where(x => x.Id == 0).SelectMany(x => x.TargetCustomerBrands).Select(x =>
                             new tblTargetCustomerBrand
                             {
                                 BrandId = x.BrandId,
                                 currentTarget = x.currentTarget,
                                 GuildId = x.GuildId,
                                 Id = x.Id,
                                 SubCategoryTargetCustomerId = InsertedCustomerTargets.FirstOrDefault(y => y.GuildId == x.GuildId) != null ? InsertedCustomerTargets.FirstOrDefault(y => y.GuildId == x.GuildId).Id : 0,
                                 Target = x.Target
                             }).ToList();

                            tblTargetCustomerItems = UpdateSubCategoryTargetCustomers.Where(x => x.Id == 0).SelectMany(x => x.TargetCustomerItems).Select(x =>
                           new tblTargetCustomerItem
                           {
                               currentTarget = x.currentTarget,
                               GuildId = x.GuildId,
                               Id = x.Id,
                               SellingSku = x.SellingSku,
                               Target = x.Target,
                               SubCategoryTargetCustomerId = InsertedCustomerTargets.FirstOrDefault(y => y.GuildId == x.GuildId).Id,
                           }).ToList();

                            //}

                            if (tblTargetCustomerbrands != null && tblTargetCustomerbrands.Any(x => x.SubCategoryTargetCustomerId > 0))
                            {
                                var bulkBrands = new BulkOperations();
                                bulkBrands.Setup<tblTargetCustomerBrand>(x => x.ForCollection(tblTargetCustomerbrands))
                                    .WithTable("TargetCustomerBrands")
                                    .WithBulkCopyBatchSize(4000)
                                    .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                                    .WithSqlCommandTimeout(720) // Default is 600 seconds
                                    .AddAllColumns()
                                    .BulkInsert();
                                bulkBrands.CommitTransaction("AuthContext");
                            }

                            if (tblTargetCustomerItems != null && tblTargetCustomerItems.Any())
                            {
                                var bulkItems = new BulkOperations();
                                bulkItems.Setup<tblTargetCustomerItem>(x => x.ForCollection(tblTargetCustomerItems))
                                    .WithTable("TargetCustomerItems")
                                    .WithBulkCopyBatchSize(4000)
                                    .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                                    .WithSqlCommandTimeout(720) // Default is 600 seconds
                                    .AddAllColumns()
                                    .BulkInsert();
                                bulkItems.CommitTransaction("AuthContext");
                            }
                        }

                    }
                }
            }
            result = true;
            return result;

        }


        [Route("GetCustomerCompanyTarget")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<CustomerTarget>> GetCustomerCompanyTarget(int warehouseId, int customerid)
        {
            List<CustomerTarget> targets = new List<CustomerTarget>();

            var target = new CustomerTarget();
            var subCategoryTargetCustomerDcs = new List<SubCategoryTargetCustomerDc>();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetCustomerCompanyTarget]";
                cmd.Parameters.Add(new SqlParameter("@CustomerId", customerid));
                cmd.Parameters.Add(new SqlParameter("@TargetDetailId", -1));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                subCategoryTargetCustomerDcs = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<SubCategoryTargetCustomerDc>(reader).ToList();
                reader.NextResult();
                List<GiftItemDc> GiftItemDcs = new List<GiftItemDc>();
                List<TargetCustomerBrandDc> TargetCustomerBrandDcs = new List<TargetCustomerBrandDc>();
                List<TargetCustomerItemDc> TargetCustomerItemDcs = new List<TargetCustomerItemDc>();

                if (reader.HasRows)
                {
                    GiftItemDcs = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<GiftItemDc>(reader).ToList();
                }
                reader.NextResult();
                if (reader.HasRows)
                {
                    TargetCustomerBrandDcs = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<TargetCustomerBrandDc>(reader).ToList();
                }

                reader.NextResult();
                if (reader.HasRows)
                {
                    TargetCustomerItemDcs = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<TargetCustomerItemDc>(reader).ToList();
                }

                foreach (var item in subCategoryTargetCustomerDcs)
                {
                    if (GiftItemDcs != null && GiftItemDcs.Any())
                    {
                        item.GiftItemDcs = GiftItemDcs.Where(x => x.id == item.id).ToList();
                    }
                    else
                    {
                        item.GiftItemDcs = new List<GiftItemDc>();
                    }
                    if (TargetCustomerBrandDcs != null && TargetCustomerBrandDcs.Any())
                    {
                        item.TargetCustomerBrandDcs = TargetCustomerBrandDcs.Where(x => x.id == item.id).ToList();
                    }
                    else
                    {
                        item.TargetCustomerBrandDcs = new List<TargetCustomerBrandDc>();
                    }
                    if (TargetCustomerItemDcs != null && TargetCustomerItemDcs.Any())
                    {
                        item.TargetCustomerItemDcs = TargetCustomerItemDcs.Where(x => x.id == item.id).ToList();
                    }
                    else
                        item.TargetCustomerItemDcs = new List<TargetCustomerItemDc>();
                }
            }


            foreach (var item in subCategoryTargetCustomerDcs)
            {
                target = new CustomerTarget();
                target.CompanyId = item.SubCatId;
                target.TargetMonth = item.StartDate.ToString("MMMM");
                target.StoreName = item.CompanyName;
                target.TargetDetailId = item.TargetDetailId;
                target.StoreUrl = item.CompanyLogoUrl;
                target.BrandNames = item.BrandNames;
                target.IsClaimed = item.IsClaimed;
                target.SKCode = item.Skcode;
                target.TargetAmount = Convert.ToDecimal(item.Target);
                target.TotalPurchaseAmount = Convert.ToDecimal(item.CurrentMonthSales);
                target.LeftDays = (item.EndDate - DateTime.Now).Days;
                target.TargetLineItem = item.RequiredNoOfLineItem;
                target.CurrentLineItem = item.NoOfLineItem;

                target.targetConditions = new List<targetCondition>();
                target.targetConditions.Add(new targetCondition
                {
                    ConditionText = "Shop worth Rs. " + Convert.ToInt32(target.TargetAmount).ToString(),
                    ConditionCompleteText = ConvertNumberToWord.minifyLong(Convert.ToInt32(target.TotalPurchaseAmount)) + "/" + ConvertNumberToWord.minifyLong(Convert.ToInt32(target.TargetAmount)),
                    Target = Convert.ToInt32(target.TargetAmount),
                    CurrentValue = Convert.ToInt32(target.TotalPurchaseAmount),
                    AchivePercent = target.TargetAmount > 0 ? (target.TotalPurchaseAmount / (target.TargetAmount / 100) > 100 ? 100 : target.TotalPurchaseAmount / (target.TargetAmount / 100)) : 100,
                    Message = (target.TargetAmount > target.TotalPurchaseAmount ? "Only Rs. " + (target.TargetAmount - target.TotalPurchaseAmount) + " away from the remaining target." : "")
                });

                if (target.TargetLineItem.HasValue && target.TargetLineItem.Value > 0)
                {
                    target.targetConditions.Add(new targetCondition
                    {
                        ConditionText = "Buy " + Convert.ToInt32(target.TargetLineItem).ToString() + " Unique line item",
                        ConditionCompleteText = Convert.ToInt32(target.CurrentLineItem.Value).ToString() + "/" + Convert.ToInt32(target.TargetLineItem).ToString(),
                        Target = target.TargetLineItem.Value,
                        CurrentValue = target.CurrentLineItem.Value,
                        AchivePercent = target.TargetLineItem.Value > 0 ? (target.CurrentLineItem.Value / (Convert.ToDecimal(target.TargetLineItem.Value) / 100) > 100 ? 100 : target.CurrentLineItem.Value / (Convert.ToDecimal(target.TargetLineItem.Value) / 100)) : 100,
                        Message = (target.TargetLineItem.Value > target.CurrentLineItem.Value ? "Buy " + (target.TargetLineItem.Value - target.CurrentLineItem.Value) + " unique line item more." : "")
                    });
                }

                if (item.GiftItemDcs != null && item.GiftItemDcs.Any())
                {
                    target.GiftImage = item.GiftItemDcs.FirstOrDefault().ItemLogo;
                    target.Value = item.GiftItemDcs.FirstOrDefault().Qty;
                    target.GiftItemName = item.GiftItemDcs.FirstOrDefault().ItemName;
                    target.Type = item.valueType;
                }
                else
                {
                    target.Type = item.valueType;
                    target.Value = item.WalletValue > 0 ? item.WalletValue / 10 : 0;
                }

                if (item.TargetCustomerBrandDcs != null && item.TargetCustomerBrandDcs.Any())
                {
                    foreach (var CustomerBrand in item.TargetCustomerBrandDcs)
                    {
                        target.targetConditions.Add(new targetCondition
                        {
                            ConditionText = "Shop " + CustomerBrand.BrandName + " worth Rs. " + Convert.ToInt32(CustomerBrand.Target).ToString(),
                            ConditionCompleteText = ConvertNumberToWord.minifyLong(Convert.ToInt32(CustomerBrand.currentTarget)) + "/" + ConvertNumberToWord.minifyLong(Convert.ToInt32(CustomerBrand.Target)),
                            Target = Convert.ToInt32(CustomerBrand.Target),
                            CurrentValue = Convert.ToInt32(CustomerBrand.currentTarget),
                            AchivePercent = CustomerBrand.Target > 0 ? (CustomerBrand.currentTarget / (CustomerBrand.Target / 100) > 100 ? 100 : CustomerBrand.currentTarget / (CustomerBrand.Target / 100)) : 100,
                            Message = (CustomerBrand.Target > CustomerBrand.currentTarget ? "Only Rs. " + (CustomerBrand.Target - CustomerBrand.currentTarget) + " away from the remaining target." : "")
                        });
                    }

                }

                if (item.TargetCustomerItemDcs != null && item.TargetCustomerItemDcs.Any())
                {
                    foreach (var CustomerBrand in item.TargetCustomerItemDcs)
                    {
                        target.targetConditions.Add(new targetCondition
                        {
                            ConditionText = "Shop " + CustomerBrand.ItemName + " worth Rs. " + Convert.ToInt32(CustomerBrand.Target).ToString(),
                            ConditionCompleteText = ConvertNumberToWord.minifyLong(Convert.ToInt32(CustomerBrand.currentTarget)) + "/" + ConvertNumberToWord.minifyLong(Convert.ToInt32(CustomerBrand.Target)),
                            Target = Convert.ToInt32(CustomerBrand.Target),
                            CurrentValue = Convert.ToInt32(CustomerBrand.currentTarget),
                            AchivePercent = CustomerBrand.Target > 0 ? (CustomerBrand.currentTarget / (CustomerBrand.Target / 100) > 100 ? 100 : CustomerBrand.currentTarget / (CustomerBrand.Target / 100)) : 100,
                            Message = (CustomerBrand.Target > CustomerBrand.currentTarget ? "Only Rs. " + (CustomerBrand.Target - CustomerBrand.currentTarget) + " away from the remaining target." : "")
                        });
                    }

                }


                var amtPer = target.TargetAmount > 0 ? target.TotalPurchaseAmount / (target.TargetAmount / 100) : 100;
                var lineitemPer = target.TargetLineItem.HasValue && target.TargetLineItem.Value > 0 ? (target.CurrentLineItem.HasValue ? target.CurrentLineItem.Value : 0) / (Convert.ToDecimal(target.TargetLineItem.Value) / 100) : 100;

                var totalcondition = 2 + target.targetConditions.Count();
                var completePer = 0;
                if (amtPer >= 100)
                {
                    completePer++;
                }
                if (lineitemPer >= 100)
                {
                    completePer++;
                }
                if (target.targetConditions.Any() && target.targetConditions.Any(x => x.AchivePercent >= 100))
                {
                    completePer += target.targetConditions.Count(x => x.AchivePercent >= 100);
                }

                target.AchivePercent = item.IsCompleted ? 100 : ((completePer / totalcondition) * 100);
                target.AchivePercent = Math.Round(target.AchivePercent, 0);
                targets.Add(target);
            }

            return targets;
        }

        [Route("ClaimCustomerCompanyTarget")]
        [HttpGet]
        public HttpResponseMessage ClaimCustomerCompanyTarget(int customerId, int targetDetailId)
        {
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetCustomerCompanyTarget]";
                cmd.Parameters.Add(new SqlParameter("@CustomerId", customerId));
                cmd.Parameters.Add(new SqlParameter("@TargetDetailId", targetDetailId));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                List<SubCategoryTargetCustomerDc> SubCategoryTargetCustomerDc = new List<SubCategoryTargetCustomerDc>();
                List<GiftItemDc> GiftItemDcs = new List<GiftItemDc>();
                List<TargetCustomerBrandDc> TargetCustomerBrandDcs = new List<TargetCustomerBrandDc>();
                List<TargetCustomerItemDc> TargetCustomerItemDcs = new List<TargetCustomerItemDc>();


                using (var reader = cmd.ExecuteReader())
                {
                    SubCategoryTargetCustomerDc = ((IObjectContextAdapter)context)
                   .ObjectContext
                   .Translate<SubCategoryTargetCustomerDc>(reader).ToList();

                    reader.NextResult();
                    GiftItemDcs = ((IObjectContextAdapter)context)
                                   .ObjectContext
                                   .Translate<GiftItemDc>(reader).ToList();

                    reader.NextResult();
                    TargetCustomerBrandDcs = ((IObjectContextAdapter)context)
                                               .ObjectContext
                                               .Translate<TargetCustomerBrandDc>(reader).ToList();

                    reader.NextResult();
                    TargetCustomerItemDcs = ((IObjectContextAdapter)context)
                                              .ObjectContext
                                              .Translate<TargetCustomerItemDc>(reader).ToList();
                }

                foreach (var item in SubCategoryTargetCustomerDc)
                {
                    if (GiftItemDcs != null && GiftItemDcs.Any())
                    {
                        item.GiftItemDcs = GiftItemDcs.Where(x => x.id == item.id).ToList();
                    }
                    if (TargetCustomerBrandDcs != null && TargetCustomerBrandDcs.Any())
                    {
                        item.TargetCustomerBrandDcs = TargetCustomerBrandDcs.Where(x => x.id == item.id).ToList();
                    }
                    if (TargetCustomerItemDcs != null && TargetCustomerItemDcs.Any())
                    {
                        item.TargetCustomerItemDcs = TargetCustomerItemDcs.Where(x => x.id == item.id).ToList();
                    }
                }

                if (SubCategoryTargetCustomerDc != null && SubCategoryTargetCustomerDc.Any())
                {
                    var subCategoryTargetCustomerDc = SubCategoryTargetCustomerDc.FirstOrDefault(); ;

                    if (subCategoryTargetCustomerDc.IsCompleted)
                    {
                        if (!subCategoryTargetCustomerDc.IsClaimed)
                        {
                            string message = "";
                            if (subCategoryTargetCustomerDc.valueType == "WalletPoint")
                            {
                                #region Reward Insertion in Wallet 

                                var cust = context.Customers.Where(x => x.CustomerId == subCategoryTargetCustomerDc.CustomerId).SingleOrDefault();

                                var IsWalletEdit = context.WalletDb.Where(x => x.CustomerId == subCategoryTargetCustomerDc.CustomerId).SingleOrDefault();
                                if (IsWalletEdit == null)
                                {
                                    IsWalletEdit = new Wallet();
                                    IsWalletEdit.CompanyId = cust.CompanyId ?? 0;
                                    IsWalletEdit.CustomerId = cust.CustomerId;
                                    IsWalletEdit.TransactionDate = indianTime;
                                    IsWalletEdit.TotalAmount = Convert.ToDouble(subCategoryTargetCustomerDc.WalletValue);
                                    IsWalletEdit.CreatedDate = indianTime;
                                    IsWalletEdit.UpdatedDate = indianTime;
                                    IsWalletEdit.Deleted = false;
                                    context.WalletDb.Add(IsWalletEdit);
                                    context.Commit();
                                }
                                CustomerWalletHistory customerwallethistory = new CustomerWalletHistory();

                                customerwallethistory.CustomerId = cust.CustomerId;
                                customerwallethistory.WarehouseId = cust.Warehouseid.Value;
                                customerwallethistory.Through = "Achieved " + subCategoryTargetCustomerDc.CompanyName + " - " + subCategoryTargetCustomerDc.CreatedDate.ToString("MMMyyyy") + " Target Reward";
                                customerwallethistory.CompanyId = cust.CompanyId ?? 0;
                                if (Convert.ToDouble(subCategoryTargetCustomerDc.WalletValue) >= 0)
                                {
                                    customerwallethistory.NewAddedWAmount = Convert.ToDouble(subCategoryTargetCustomerDc.WalletValue);
                                    customerwallethistory.TotalWalletAmount = IsWalletEdit.TotalAmount + Convert.ToDouble(subCategoryTargetCustomerDc.WalletValue);
                                }
                                customerwallethistory.UpdatedDate = indianTime;
                                customerwallethistory.TransactionDate = indianTime;
                                customerwallethistory.CreatedDate = indianTime;
                                context.CustomerWalletHistoryDb.Add(customerwallethistory);
                                IsWalletEdit.TotalAmount = IsWalletEdit.TotalAmount + Convert.ToDouble(subCategoryTargetCustomerDc.WalletValue);
                                IsWalletEdit.UpdatedDate = indianTime;
                                IsWalletEdit.TransactionDate = indianTime;
                                context.Entry(IsWalletEdit).State = EntityState.Modified;

                                #endregion

                                message = "Reward Points successfully Added in wallet";
                            }
                            else if (subCategoryTargetCustomerDc.valueType == "DreamItem")
                            {
                                #region Create Reward Ordered

                                Customer cust = new Customer();
                                Warehouse w = new Warehouse();
                                var sc = new DreamOrder();

                                cust = context.Customers.Where(x => x.CustomerId == customerId).SingleOrDefault();
                                w = context.Warehouses.Where(wr => wr.WarehouseId == cust.Warehouseid).FirstOrDefault();

                                if (cust != null)
                                {
                                    if (w != null)
                                    {
                                        sc.WarehouseId = w.WarehouseId;
                                        sc.WarehouseName = w.WarehouseName;
                                        sc.CityId = w.Cityid;
                                    }
                                    sc.ShopName = cust.ShopName;
                                    sc.Status = "Pending";
                                    sc.CustomerMobile = cust.Mobile;
                                    sc.ShippingAddress = cust.ShippingAddress;
                                    //sc.SalesPersonId = cust.ExecutiveId;
                                    sc.CompanyId = cust.CompanyId;
                                }
                                sc.CreatedDate = indianTime;
                                sc.UpdatedDate = indianTime;
                                sc.Deliverydate = indianTime.AddDays(2);
                                sc.Deleted = false;
                                sc.ReDispatchCount = 0;

                                foreach (var item in subCategoryTargetCustomerDc.GiftItemDcs)
                                {
                                    var dreamorder = new DreamItem();

                                    dreamorder.ShopName = cust.ShopName;
                                    dreamorder.ItemId = item.itemid;
                                    dreamorder.OrderQty = item.Qty;
                                    dreamorder.ShopName = cust.ShopName;
                                    dreamorder.Skcode = cust.Skcode;
                                    dreamorder.ItemName = item.ItemName;
                                    dreamorder.Discription = "";
                                    dreamorder.Status = "Pending";
                                    dreamorder.CreatedDate = indianTime;
                                    dreamorder.UpdatedDate = indianTime;
                                    dreamorder.Deleted = false;
                                    if (sc.DreamItemDetails == null)
                                    {
                                        sc.DreamItemDetails = new List<DreamItem>();
                                    }
                                    sc.DreamItemDetails.Add(dreamorder);
                                }
                                context.DreamOrderDb.Add(sc);


                                #endregion

                                message = "Reward order created successfully, will get you soon";
                            }

                            var customerTarget = context.SubCategoryTargetCustomers.FirstOrDefault(x => x.Id == subCategoryTargetCustomerDc.id);
                            customerTarget.IsClaimed = true;
                            customerTarget.ModifiedBy = customerId;
                            customerTarget.ModifiedDate = DateTime.Now;
                            context.Entry(customerTarget).State = EntityState.Modified;
                            context.Commit();
                            var response = new
                            {
                                Status = true,
                                Message = message
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, response);
                        }
                        else
                        {
                            var response = new
                            {
                                Status = false,
                                Message = "Already claimed processed"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, response);
                        }
                    }
                    else
                    {
                        var response = new
                        {
                            Status = false,
                            Message = "Target Not Completed!"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Target Not Found!"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
        }

        [Route("GetLevelData")]
        [HttpGet]
        public List<LevelDc> GetLevelData(int customerId, string SkCode)
        {
            List<LevelDc> levelDcs = new List<LevelDc>();
            var date = DateTime.Now;
            if (date.Day <= 10)
            {
                date = DateTime.Now.AddMonths(-1);
            }
            var lastDayOfMonth = DateTime.DaysInMonth(date.Year, date.Month);

            var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
            string DocumentName = "MonthlyTargetData_" + date.Month.ToString() + date.Year.ToString();
            var GetExportData = MonthlyCustomerTarget.Select(x => x.Skcode == SkCode, null, null, null, false, "", DocumentName).FirstOrDefault();


            levelDcs.Add(new LevelDc
            {
                Id = 1,
                BrandCount = 0,
                KKVolume = 0,
                LevelName = "Level 0",
                OrderCount = 0,
                Selected = false,
                Volume = 0
            });
            levelDcs.Add(new LevelDc
            {
                Id = 2,
                BrandCount = 0,
                KKVolume = 0,
                LevelName = "Level 1",
                OrderCount = 1,
                Selected = false,
                Volume = 0
            });
            levelDcs.Add(new LevelDc
            {
                Id = 3,
                BrandCount = 5,
                KKVolume = 0,
                LevelName = "Level 2",
                OrderCount = 3,
                Selected = false,
                Volume = 10000
            });
            levelDcs.Add(new LevelDc
            {
                Id = 4,
                BrandCount = 10,
                KKVolume = 2000,
                LevelName = "Level 3",
                OrderCount = 5,
                Selected = false,
                Volume = 20000
            });
            levelDcs.Add(new LevelDc
            {
                Id = 5,
                BrandCount = 20,
                KKVolume = 8000,
                LevelName = "Level 4",
                OrderCount = 8,
                Selected = false,
                Volume = 30000,
            });
            levelDcs.Add(new LevelDc
            {
                Id = 6,
                BrandCount = 40,
                KKVolume = 15000,
                LevelName = "Level 5",
                OrderCount = 12,
                Selected = false,
                Volume = 75000
            });


            if (GetExportData != null)
            {
                int level = 0;
                switch (GetExportData.Levels)
                {
                    case "Level 0":
                        level = 1;
                        break;
                    case "Level 1":
                        level = 2;
                        break;
                    case "Level 2":
                        level = 3;
                        break;
                    case "Level 3":
                        level = 4;
                        break;
                    case "Level 4":
                        level = 5;
                        break;
                    case "Level 5":
                        level = 6;
                        break;
                }

                foreach (var item in levelDcs)
                {
                    if (item.Id <= level)
                    {
                        item.Selected = true;
                    }
                }
            }
            return levelDcs;

        }

        [Route("CreateTargetOrder")]
        [HttpGet]
        public bool CreateTargetOrder()
        {
            using (var db = new AuthContext())
            {
                #region Create Reward Ordered
                //Customer cust = new Customer();
                List<Customer> custdb = new List<Customer>();
                List<Warehouse> w = new List<Warehouse>();
                List<DreamOrder> addlist = new List<DreamOrder>();

                string queary = " select Skcode,CAST(rItemId as int) as rItemId from GetTargetSkcodeList ";
                var SkcodeList = db.Database.SqlQuery<SkcodeTargetCustomerListDc>(queary).ToList();
                //var SkcodeList = SkcodeLists.Where(x => x.Skcode == "SK161621").ToList();
                var skcodes = SkcodeList.Select(x => x.Skcode).Distinct().ToList();

                custdb = db.Customers.Where(x => skcodes.Contains(x.Skcode) && x.Deleted == false).ToList();
                var warehouseIds = custdb.Select(x => x.Warehouseid).Distinct().ToList();
                w = db.Warehouses.Where(wr => warehouseIds.Contains(wr.WarehouseId)).ToList();
                custdb.ForEach(cust =>
                {
                    var sc = new DreamOrder();
                    var fileCust = SkcodeList.Where(x => x.Skcode.Trim() == cust.Skcode).FirstOrDefault();
                    var warehouse = w.Where(x => x.WarehouseId == cust.Warehouseid).FirstOrDefault();
                    if (fileCust != null)
                    {
                        if (warehouse != null)
                        {
                            sc.WarehouseId = warehouse.WarehouseId;
                            sc.WarehouseName = warehouse.WarehouseName;
                            sc.CityId = warehouse.Cityid;
                        }
                        sc.ShopName = cust.ShopName;
                        sc.Status = "Pending";
                        sc.CustomerMobile = cust.Mobile;
                        sc.ShippingAddress = cust.ShippingAddress;
                        //sc.SalesPersonId = cust.ExecutiveId;
                        sc.CompanyId = cust.CompanyId;
                        sc.CustomerId = cust.CustomerId;
                        sc.Skcode = cust.Skcode;
                        sc.CreatedDate = indianTime;
                        sc.UpdatedDate = indianTime;
                        sc.Deliverydate = indianTime.AddDays(2);
                        sc.Deleted = false;
                        sc.ReDispatchCount = 0;

                        var dreamorder = new DreamItem();
                        RewardItems it = db.RewardItemsDb.Where(x => x.rItemId == fileCust.rItemId).SingleOrDefault();
                        dreamorder.ShopName = cust.ShopName;
                        dreamorder.ItemId = it.rItemId;
                        dreamorder.OrderQty = 1;

                        dreamorder.ShopName = cust.ShopName;
                        dreamorder.Skcode = cust.Skcode;
                        dreamorder.ItemName = it.rItem;
                        dreamorder.Discription = it.Description;
                        dreamorder.Status = "Pending";
                        dreamorder.CreatedDate = indianTime;
                        dreamorder.UpdatedDate = indianTime;
                        dreamorder.Deleted = false;


                        try
                        {
                            if (sc.DreamItemDetails == null)
                            {
                                sc.DreamItemDetails = new List<DreamItem>();
                            }
                            sc.DreamItemDetails.Add(dreamorder);
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                        addlist.Add(sc);
                    }
                });
                db.DreamOrderDb.AddRange(addlist);
                db.Commit();
                #endregion
            }
            return true;
        }
        [Route("UpdateAllCustomerTarget")]
        [HttpGet]
        public bool UpdateAllCustomerTarget()
        {
            var date = DateTime.Now;
            var target = new CustomerTarget();
            var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
            string DocumentName = "MonthlyTargetData_" + date.Month.ToString() + date.Year.ToString();
            var Targets = MonthlyCustomerTarget.Select(x => x.WarehouseId > -1, null, null, null, false, "", DocumentName).ToList();
            using (var myContext = new AuthContext())
            {
                var querys = " select * from [dbo].[AllCustomerTarget] ";//ORDER BY Skcode OFFSET 3000 ROWS FETCH NEXT 1000 ROWS ONLY;
                var custdata = myContext.Database.SqlQuery<AllCustomerTargetDc>(querys).ToList();

                if (Targets != null && custdata != null && custdata.Any())
                {
                    var skcodes = custdata.Select(x => x.Skcode).Distinct().ToList();
                    var filterCustomer = Targets.Where(x => skcodes.Contains(x.Skcode)).ToList();
                    foreach (var item in filterCustomer)
                    {
                        var find = custdata.Where(x => x.Skcode == item.Skcode).FirstOrDefault();
                        if (find != null)
                        {
                            item.Target = Convert.ToDecimal(find.Target);
                            item.IsOffer = find.IsOffer;
                            item.OfferType = find.OfferType;
                            item.OfferValue = find.OfferValue;
                            item.OfferId = find.OfferId;
                            item.OfferDesc = find.OfferDesc;
                            item.MaxDiscount = find.MaxDiscount;
                            item.MOVMultiplier = find.MOVMultiplier;
                            MonthlyCustomerTarget.Replace(item.Id, item, DocumentName);
                        }
                    }
                }
            }
            return true;
        }


        [Route("GetDownloadeSampleFile")]
        [HttpGet]
        [AllowAnonymous]
        public DataTable GetDownloadeSampleFiles()
        {
            using (var myContext = new AuthContext())
            {
                //var data = myContext.Database.SqlQuery<dynamic>("EXEC sp_DownloadeSampleFile").FirstOrDefault();
                //return data;

                string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                DataTable DT = new DataTable();
                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = new SqlCommand("EXEC downloadSampleFile", connection))
                    //using (var command = new SqlCommand("EXEC sp_DownloadeSampleFileEnhancement", connection)) 

                    {

                        if (connection.State != ConnectionState.Open)
                            connection.Open();

                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(DT);
                        da.Dispose();
                        connection.Close();

                        return DT;
                    }
                }

            }
        }


        [Route("UpdateAllCustomerTargetExcel")]
        [AllowAnonymous]
        [HttpPost]
        public bool UpdateAllCustomerTargetExcel(int month, int year)
        {
            //var date = DateTime.Now;
            var target = new CustomerTarget();
            using (var myContext = new AuthContext())
            {
                //var querys = " select * from [dbo].[AllCustomerTarget] ";//ORDER BY Skcode OFFSET 3000 ROWS FETCH NEXT 1000 ROWS ONLY;
                // var custdata = myContext.Database.SqlQuery<AllCustomerTargetDc>(querys).ToList();

                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                if (httpPostedFile != null)
                {
                    var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
                    string DocumentName = "MonthlyTargetData_" + month.ToString() + year.ToString();
                    var Targets = MonthlyCustomerTarget.Select(x => x.WarehouseId > -1, null, null, null, false, "", DocumentName).ToList();
                    if (Targets == null)
                    {
                        return false;
                    }

                    string path = HttpContext.Current.Server.MapPath("~/Upload/MonthlyTargetData");
                    string a1, b;
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    a1 = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + httpPostedFile.FileName;
                    b = Path.Combine(HttpContext.Current.Server.MapPath("~/Upload/MonthlyTargetData"), a1);

                    httpPostedFile.SaveAs(b);
                    byte[] buffer = new byte[httpPostedFile.ContentLength];
                    using (BinaryReader br = new BinaryReader(File.OpenRead(b)))
                    {
                        br.Read(buffer, 0, buffer.Length);
                    }
                    XSSFWorkbook hssfwb;

                    List<AllCustomerTargetDc> uploaditemlist = new List<AllCustomerTargetDc>();

                    List<storetarget> storetargets = new List<storetarget>();
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        BinaryFormatter binForm = new BinaryFormatter();
                        memStream.Write(buffer, 0, buffer.Length);
                        memStream.Seek(0, SeekOrigin.Begin);
                        hssfwb = new XSSFWorkbook(memStream);
                        string sSheetName = hssfwb.GetSheetName(0);
                        ISheet sheet = hssfwb.GetSheet(sSheetName);
                        IRow rowData;
                        ICell cellData = null;
                        int? SkCellIndex = null;
                        int? TargetCellIndex = null;
                        int? IsOfferCellIndex = null;
                        int? OfferTypeCellIndex = null;
                        int? OfferValueCellIndex = null;
                        int? OfferIdCellIndex = null;
                        int? OfferDescCellIndex = null;
                        int? MaxDiscountCellIndex = null;
                        int? MOVMultiplierCellIndex = null;
                        int? TargetLineItemCellIndex = null;
                        int? LineItemMinAmountCellIndex = null;

                        int? storeSkCellIndex = null;
                        int? storeTargetCellIndex = null;
                        int? storeCellIndex = null;
                        int? storeLineItemCellIndex = null;

                        List<string> headerlst = new List<string>();
                        for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                        {
                            if (iRowIdx == 0)//checkingHeader
                            {
                                rowData = sheet.GetRow(iRowIdx);
                                if (rowData != null)
                                {
                                    foreach (var item in rowData.Cells)
                                    {
                                        headerlst.Add(item.ToString());
                                    }
                                    SkCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Skcode") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Skcode").ColumnIndex : (int?)null;
                                    if (!SkCellIndex.HasValue)
                                    {
                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                                        var strJSON = objJSSerializer.Serialize("Skcode does not exist..try again");
                                        return false;
                                    }

                                    TargetCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Target") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Target").ColumnIndex : (int?)null;


                                    IsOfferCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "IsOffer") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "IsOffer").ColumnIndex : (int?)null;

                                    OfferTypeCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "OfferType") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "OfferType").ColumnIndex : (int?)null;


                                    OfferValueCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "OfferValue") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "OfferValue").ColumnIndex : (int?)null;


                                    OfferIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "OfferId") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "OfferId").ColumnIndex : (int?)null;

                                    OfferDescCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "OfferDesc") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "OfferDesc").ColumnIndex : (int?)null;
                                    if (!OfferDescCellIndex.HasValue)
                                    {
                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                                        var strJSON = objJSSerializer.Serialize("OfferDesc does not exist..try again");
                                        return false;
                                    }


                                    MaxDiscountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "MaxDiscount") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "MaxDiscount").ColumnIndex : (int?)null;

                                    MOVMultiplierCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "MOVMultiplier") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "MOVMultiplier").ColumnIndex : (int?)null;
                                    TargetLineItemCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "TargetLineItem") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "TargetLineItem").ColumnIndex : (int?)null;
                                    LineItemMinAmountCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "LineItemMinAmount") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "LineItemMinAmount").ColumnIndex : (int?)null;
                                }
                            }
                            else
                            {
                                AllCustomerTargetDc additem = new AllCustomerTargetDc();
                                rowData = sheet.GetRow(iRowIdx);
                                if (rowData != null)
                                {
                                    if (rowData.GetCell(0) != null)
                                    {
                                        if (rowData.GetCell(0).ToString().Trim() != "")
                                        {
                                            cellData = rowData.GetCell(0);
                                            rowData = sheet.GetRow(iRowIdx);
                                            if (rowData != null && cellData != null)
                                            {
                                                string col = null;
                                                cellData = rowData.GetCell(SkCellIndex.Value);
                                                col = cellData == null ? "" : cellData.ToString();
                                                if (col != "")
                                                {
                                                    additem.Skcode = col;
                                                }

                                                string col1 = null;
                                                cellData = rowData.GetCell(1);
                                                col1 = cellData != null ? cellData.ToString() : "0";
                                                additem.Target = Convert.ToDouble(col1);

                                                string col2 = null;
                                                cellData = rowData.GetCell(2);
                                                col2 = cellData != null ? cellData.ToString() : "False";
                                                additem.IsOffer = Convert.ToBoolean(col2);

                                                string col3 = null;
                                                cellData = rowData.GetCell(3);
                                                col3 = cellData != null ? cellData.ToString() : "0";
                                                additem.OfferType = Convert.ToInt32(col3);

                                                cellData = rowData.GetCell(4);
                                                additem.OfferValue = cellData != null ? Convert.ToInt32(cellData.NumericCellValue) : 0;

                                                cellData = rowData.GetCell(5);
                                                additem.OfferId = cellData != null ? int.Parse(cellData.ToString()) : 0;

                                                cellData = rowData.GetCell(6);
                                                if (cellData != null)
                                                    additem.OfferDesc = cellData != null ? Convert.ToString(cellData) : "0";
                                                else
                                                {
                                                    return false;
                                                }
                                                cellData = rowData.GetCell(7);
                                                additem.MaxDiscount = cellData != null ? Convert.ToInt32(cellData.NumericCellValue) : 0;

                                                cellData = rowData.GetCell(8);
                                                additem.MOVMultiplier = cellData != null ? Convert.ToInt32(cellData.NumericCellValue) : 0;
                                                cellData = rowData.GetCell(9);
                                                additem.TargetLineItem = cellData != null ? Convert.ToInt32(cellData.NumericCellValue) : 0;
                                                cellData = rowData.GetCell(10);
                                                additem.LineItemMinAmount = cellData != null ? Convert.ToInt32(cellData.NumericCellValue) : 0;
                                                uploaditemlist.Add(additem);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        return false;
                                    }

                                }
                            }
                        }



                        string sSheetName2 = hssfwb.GetSheetName(1);
                        ISheet sheet1 = hssfwb.GetSheet(sSheetName2);
                        for (int iRowIdx = 0; iRowIdx <= sheet1.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                        {
                            if (iRowIdx == 0)//checkingHeader
                            {
                                rowData = sheet1.GetRow(iRowIdx);
                                if (rowData != null)
                                {
                                    storeSkCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Skcode") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Skcode").ColumnIndex : (int?)null;
                                    if (!storeSkCellIndex.HasValue)
                                    {
                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                                        var strJSON = objJSSerializer.Serialize("Skcode does not exist..try again");
                                        return false;
                                    }

                                    storeCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "StoreName") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "StoreName").ColumnIndex : (int?)null;
                                    if (!storeCellIndex.HasValue)
                                    {
                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                                        var strJSON = objJSSerializer.Serialize("StoreName does not exist..try again");
                                        return false;
                                    }

                                    storeTargetCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "TargetForAmount") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "TargetForAmount").ColumnIndex : (int?)null;

                                    storeLineItemCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "TargetForLineItem") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "TargetForLineItem").ColumnIndex : (int?)null;
                                }
                            }
                            else
                            {
                                storetarget additem = new storetarget();
                                rowData = sheet1.GetRow(iRowIdx);
                                if (rowData != null)
                                {
                                    if (rowData.GetCell(0) != null)
                                    {
                                        if (rowData.GetCell(0).ToString().Trim() != "")
                                        {
                                            cellData = rowData.GetCell(0);
                                            if (rowData != null && cellData != null)
                                            {
                                                string col = null;
                                                cellData = rowData.GetCell(storeSkCellIndex.Value);
                                                col = cellData == null ? "" : cellData.ToString();
                                                if (col != "")
                                                {
                                                    additem.skcode = col;
                                                }

                                                string col1 = null;
                                                cellData = rowData.GetCell(storeCellIndex.Value); //change here
                                                col1 = cellData != null ? cellData.ToString() : "";
                                                additem.storename = col1;

                                                col1 = null;
                                                cellData = rowData.GetCell(storeTargetCellIndex.Value);
                                                col1 = cellData != null ? cellData.ToString() : "0";
                                                additem.targetInAmount = Convert.ToDouble(col1);

                                                col1 = null;
                                                cellData = rowData.GetCell(storeLineItemCellIndex.Value);
                                                col1 = cellData != null ? cellData.ToString() : "0";
                                                additem.targetInLineItem = Convert.ToInt32(col1);

                                                storetargets.Add(additem);
                                            }
                                        }
                                    }

                                }
                            }
                        }



                    }
                    ParallelLoopResult parellelResult = new ParallelLoopResult();
                    bool result = false;
                    if (uploaditemlist != null && uploaditemlist.Any() && storetargets != null && storetargets.Any())
                    {
                        //uploaditemlist.ForEach(x =>
                        //{
                        parellelResult = Parallel.ForEach(uploaditemlist, (x) =>
                        {
                            if (storetargets.Any(y => y.skcode == x.Skcode))
                            {
                                x.storetargets = storetargets.Where(y => y.skcode == x.Skcode).ToList();
                            }
                        });
                        if (parellelResult.IsCompleted)
                        {
                            result = true;
                        }
                    }
                    else
                        result = true;

                    if (Targets != null && result && uploaditemlist != null && uploaditemlist.Any())
                    {
                        var stores = myContext.StoreDB.Where(x => x.IsActive).ToList();
                        var skcodes = uploaditemlist.Select(x => x.Skcode).Distinct().ToList();
                        var filterCustomer = Targets.Where(x => skcodes.Contains(x.Skcode)).ToList();
                        var mongoCustomer = filterCustomer.Select(x => x.Skcode).Distinct().ToList();
                        var notexistsCustomer = uploaditemlist.Where(x => !mongoCustomer.Contains(x.Skcode)).ToList();
                        //foreach (var item in filterCustomer)
                        //{
                        var parellelResult1 = Parallel.ForEach(filterCustomer, (item) =>
                        {
                            var find = uploaditemlist.Where(x => x.Skcode == item.Skcode).FirstOrDefault();
                            if (find != null)
                            {
                                item.Target = Convert.ToDecimal(find.Target);
                                item.IsOffer = Convert.ToInt32(find.OfferId) > 0 ? find.IsOffer : false;
                                item.OfferType = find.OfferType;
                                item.OfferValue = find.OfferValue;
                                item.OfferId = Convert.ToInt32(find.OfferId);
                                item.OfferDesc = find.OfferDesc;
                                item.MaxDiscount = find.MaxDiscount;
                                item.MOVMultiplier = find.MOVMultiplier;
                                item.TargetLineItem = find.TargetLineItem;
                                item.LineItemMinAmount = find.LineItemMinAmount;
                                if (find.storetargets != null && find.storetargets.Any())
                                {
                                    item.TargetOnStores = new List<TargetDivideOnStore>();
                                    foreach (var storetarget in find.storetargets)
                                    {
                                        item.TargetOnStores.Add(new TargetDivideOnStore
                                        {
                                            StoreName = stores.FirstOrDefault(x => x.Name.ToLower() == storetarget.storename.ToLower()).Name,
                                            CurrentLineItem = 0,
                                            CurrentVolume = 0,
                                            StoreId = stores.FirstOrDefault(x => x.Name.ToLower() == storetarget.storename.ToLower()).Id,
                                            Target = storetarget.targetInAmount,
                                            TargetLineItem = storetarget.targetInLineItem
                                        });
                                    }
                                }

                                MonthlyCustomerTarget.Replace(item.Id, item, DocumentName);
                            }
                        });

                        bool notexistsresult = false;
                        if (notexistsCustomer != null && notexistsCustomer.Any())
                        {
                            var AllskcodeList = notexistsCustomer.Select(x => x.Skcode).Distinct().ToList();
                            //var customers = myContext.Customers.Where(x => skcodeList.Contains(x.Skcode)).Select(x => new { x.Cityid, x.Warehouseid, x.Skcode }).ToList();
                            var skcodeList = new DataTable();
                            skcodeList.Columns.Add("stringValues");
                            if (AllskcodeList != null && AllskcodeList.Any())
                            {
                                foreach (var item in AllskcodeList)
                                {
                                    var dr = skcodeList.NewRow();
                                    dr["stringValues"] = item;
                                    skcodeList.Rows.Add(dr);
                                }
                            }
                            var Skcodeparam = new SqlParameter("Skcode", skcodeList);
                            Skcodeparam.SqlDbType = SqlDbType.Structured;
                            Skcodeparam.TypeName = "dbo.stringValues";

                            if (myContext.Database.Connection.State != ConnectionState.Open)
                                myContext.Database.Connection.Open();

                            myContext.Database.CommandTimeout = 400;
                            var cmd = myContext.Database.Connection.CreateCommand();
                            cmd.CommandText = "[dbo].GetCustomerDataBySkcode";
                            cmd.Parameters.Add(Skcodeparam);
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;

                            // Run the sproc
                            var reader = cmd.ExecuteReader();
                            List<AllSkcodeTargetCustomerListDC> customers = ((IObjectContextAdapter)myContext)
                            .ObjectContext
                            .Translate<AllSkcodeTargetCustomerListDC>(reader).ToList();

                            var warehouseIds = customers.Select(x => x.Warehouseid).Distinct().ToList();
                            var WarehouseList = myContext.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId) && x.active == true && x.Deleted == false).Select(x => new { x.WarehouseId, x.Cityid, x.CityName }).ToList();

                            //foreach (var item in notexistsCustomer)
                            //{
                            var parellelResult2 = Parallel.ForEach(notexistsCustomer, (item) =>
                            {
                                var filtercust = customers.Where(x => x.Skcode == item.Skcode).FirstOrDefault();
                                if (filtercust != null)
                                {
                                    MonthlyCustomerTarget addMongoDbHelper = new MonthlyCustomerTarget();
                                    addMongoDbHelper.Skcode = item.Skcode;
                                    addMongoDbHelper.WarehouseId = WarehouseList.Where(x => x.WarehouseId == filtercust.Warehouseid).Select(x => x.WarehouseId).FirstOrDefault();
                                    addMongoDbHelper.CityName = WarehouseList.Where(x => x.Cityid == filtercust.Cityid).Select(x => x.CityName).FirstOrDefault();
                                    addMongoDbHelper.Levels = "Level 0";
                                    addMongoDbHelper.Volume = 0;
                                    addMongoDbHelper.Bands = "Band 1";
                                    addMongoDbHelper.Source = "C#";
                                    addMongoDbHelper.CurrentVolume = 0;
                                    addMongoDbHelper.PendingVolume = 0;
                                    addMongoDbHelper.IsClaimed = false;
                                    addMongoDbHelper.CreatedDate = DateTime.Now;
                                    addMongoDbHelper.Target = Convert.ToDecimal(item.Target);
                                    addMongoDbHelper.IsOffer = Convert.ToInt32(item.OfferId) > 0 ? item.IsOffer : false;
                                    addMongoDbHelper.OfferType = item.OfferType;
                                    addMongoDbHelper.OfferValue = item.OfferValue;
                                    addMongoDbHelper.OfferId = Convert.ToInt32(item.OfferId);
                                    addMongoDbHelper.OfferDesc = item.OfferDesc;
                                    addMongoDbHelper.MaxDiscount = item.MaxDiscount;
                                    addMongoDbHelper.MOVMultiplier = item.MOVMultiplier;
                                    addMongoDbHelper.TargetLineItem = item.TargetLineItem;
                                    addMongoDbHelper.LineItemMinAmount = item.LineItemMinAmount;

                                    addMongoDbHelper.TargetOnStores = new List<TargetDivideOnStore>();
                                    if (item.storetargets != null && item.storetargets.Count > 0)
                                    {
                                        foreach (var storetarget in item.storetargets)
                                        {
                                            addMongoDbHelper.TargetOnStores.Add(new TargetDivideOnStore
                                            {
                                                StoreName = stores.FirstOrDefault(x => x.Name.ToLower() == storetarget.storename.ToLower()).Name,
                                                CurrentLineItem = 0,
                                                CurrentVolume = 0,
                                                StoreId = stores.FirstOrDefault(x => x.Name.ToLower() == storetarget.storename.ToLower()).Id,
                                                Target = storetarget.targetInAmount,
                                                TargetLineItem = storetarget.targetInLineItem
                                            });
                                        }
                                    }

                                    MonthlyCustomerTarget.Insert(addMongoDbHelper, DocumentName);
                                }
                            });
                            if (parellelResult2.IsCompleted)
                                notexistsresult = true;
                        }
                        else
                            notexistsresult = true;

                        if (parellelResult1.IsCompleted && notexistsresult)
                        {
                            var test = true;
                        }
                    }
                }
                return true;
            }
        }

        [Route("RemoveDuplicateForTarget")]
        [AllowAnonymous]
        [HttpGet]
        public bool RemoveDuplicateForTarget(int month, int year)
        {
            var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
            string DocumentName = "MonthlyTargetData_" + month.ToString() + year.ToString();
            var Targets = MonthlyCustomerTarget.Select(x => x.WarehouseId > -1, null, null, null, false, "", DocumentName).ToList();
            if (Targets == null)
            {
                return false;
            }
            var finalData = Targets.GroupBy(x => x.Skcode).Where(x => x.Count() > 1);
            var parellelResult = Parallel.ForEach(finalData, (x) =>
            {
                MonthlyCustomerTarget.Delete(x.FirstOrDefault().Id, DocumentName);
            });

            if (parellelResult.IsCompleted)
                return true;

            return true;
        }

        [Route("GetTargetItem")]
        [HttpGet]
        public async Task<DataContracts.External.SalesItemResponseDc> GetTargetItem(int companyId, int storeId, int customerId, int warehouseId, int peopleId, int skip, int take, string lang, string itemname)
        {
            var itemResponseDc = new DataContracts.External.SalesItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.SalesAppItemDataDC>() };
            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = new List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand>();

            List<int> CatIds = new List<int>();
            List<int> SubCats = new List<int>();
            List<int> SubSubCats = new List<int>();
            if (peopleId != 0)
            {
                SalesAppItemController salesAppItemConTroller = new SalesAppItemController();
                StoreCategorySubCategoryBrands = salesAppItemConTroller.GetCatSubCatwithStores(peopleId);
                if (storeId > 0)
                    StoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => x.StoreId == storeId).ToList();
                CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();
            }

            var today = DateTime.Today;
            if (today.Day <= 10 && companyId == 0)
            {
                today = today.AddMonths(-1);
            }
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var startDate = monthStart.ToString("yyyy-MM-dd");
            var enddate = monthEnd.ToString("yyyy-MM-dd");

            string query = $"SELECT itemnumber from skorderdata_{AppConstants.Environment} where whid={warehouseId} and custid={customerId}  and createddate>='{startDate}' and createddate <= '{enddate}'";
            if (!string.IsNullOrEmpty(itemname))
                query += $" and (lcase(itemname) like '%{itemname.ToLower()}%' or lcase(itemnumber) like '%{itemname.ToLower()}%') ";
            if (companyId > 0)
            {
                query += $" and compid={companyId}";
            }

            if (CatIds != null && CatIds.Any())
            {
                query += $" and catid in ({string.Join(",", CatIds)})";
            }

            if (SubCats != null && SubCats.Any())
            {
                query += $" and compid in ({string.Join(",", SubCats)})";
            }
            if (SubSubCats != null && SubSubCats.Any())
            {
                query += $" and brandid in ({string.Join(",", SubSubCats)})";
            }

            query += " group by itemnumber";


            ElasticSqlHelper<elasticItemvalue> elasticSqlHelper = new ElasticSqlHelper<elasticItemvalue>();

            var orderitems = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync(query)).ToList());
            var itemnumbers = string.Empty;
            if (orderitems != null && orderitems.Any())
            {
                itemnumbers = "'" + string.Join("','", orderitems.Select(x => x.itemnumber)) + "'";
            }
            string itemIndex = ConfigurationManager.AppSettings["ElasticSearchIndexName"];
            query = $"SELECT itemnumber from {itemIndex} where warehouseid={warehouseId} " +
                $" and active=true and deleted=false and  isdiscontinued=false and (isitemlimit=false or (isitemlimit=true and itemlimitqty>0 and itemlimitqty-minorderqty>0 )) and (itemapptype=0 or itemapptype=1)";

            if (!string.IsNullOrEmpty(itemnumbers))
                query += $" and itemnumber not in ({ itemnumbers })";

            if (!string.IsNullOrEmpty(itemname))
                query += $" and (lcase(itemname) like '%{itemname.ToLower()}%' or lcase(itemnumber) like '%{itemname.ToLower()}%' ) ";

            if (CatIds != null && CatIds.Any())
            {
                query += $" and categoryid in ({string.Join(",", CatIds)})";
            }

            if (SubCats != null && SubCats.Any())
            {
                query += $" and subcategoryid in ({string.Join(",", SubCats)})";
            }
            if (companyId != 0)
            {
                query += $" and subcategoryid ={companyId}";
            }
            if (SubSubCats != null && SubSubCats.Any())
            {
                query += $" and subsubcategoryid in ({string.Join(",", SubSubCats)})";
            }

            query += " group by itemnumber";

            ElasticSqlHelper<elasticItemvalue> elasticSqlHelper1 = new ElasticSqlHelper<elasticItemvalue>();

            var dbItemNumbers = AsyncContext.Run(async () => (await elasticSqlHelper1.GetListAsync(query)).ToList());

            itemResponseDc.TotalItem = dbItemNumbers.Count();
            var itemNumbers = dbItemNumbers.OrderBy(x => x.itemnumber).Skip(skip).Take(take).Select(x => x.itemnumber).ToList();

            List<DataContracts.External.MobileExecutiveDC.ItemDataDC> ItemData = new List<DataContracts.External.MobileExecutiveDC.ItemDataDC>();
            if (itemNumbers != null && itemNumbers.Any())
            {
                ElasticSalesAppClusterItemDataHelper elasticSalesAppClusterItem = new ElasticSalesAppClusterItemDataHelper();
                var data = AsyncContext.Run(() => elasticSalesAppClusterItem.GetSalesCatelogElasticData(warehouseId, StoreCategorySubCategoryBrands, itemNumbers, "", -1, -1, 0, 1000, "ASC", true,null));
                ItemData = data.ItemMasters;
            }
            if (ItemData != null && ItemData.Any())
            {
                ItemMasterManager itemMasterManager = new ItemMasterManager();
                var itemsIncentiveClassification = await itemMasterManager.GetItemIncentiveClassification(warehouseId, ItemData.Select(s => s.ItemMultiMRPId).Distinct().ToList());

                using (var context = new AuthContext())
                {
                    var offerids = ItemData.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                    var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();

                    foreach (var it in ItemData)
                    {
                        it.Classification = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.Classification;
                        it.BackgroundRgbColor = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.BackgroundRgbColor;

                        //Condition for offer end
                        if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                        {
                            if (it.OfferCategory == 2)
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;
                            }
                            else if (it.OfferCategory == 1)
                            {
                                it.IsOffer = false;
                                it.OfferCategory = 0;
                            }

                        }
                        else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;

                        }

                        if (it.OfferCategory == 1)
                        {
                            if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                                it.IsOffer = true;
                            else
                                it.IsOffer = false;
                        }

                        try
                        {
                            if (!it.IsOffer)
                            {
                                /// Dream Point Logic && Margin Point
                                int? MP, PP;
                                double xPoint = xPointValue * 10;
                                //Customer (0.2 * 10=1)
                                if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                {
                                    PP = 0;
                                }
                                else
                                {
                                    PP = it.promoPerItems;
                                }
                                if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                {
                                    MP = 0;
                                }
                                else
                                {
                                    double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                    MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                }
                                if (PP > 0 && MP > 0)
                                {
                                    int? PP_MP = PP + MP;
                                    it.dreamPoint = PP_MP;
                                }
                                else if (MP > 0)
                                {
                                    it.dreamPoint = MP;
                                }
                                else if (PP > 0)
                                {
                                    it.dreamPoint = PP;
                                }
                                else
                                {
                                    it.dreamPoint = 0;
                                }
                            }
                            else { it.dreamPoint = 0; }

                            // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                            if (it.price > it.UnitPrice)
                            {
                                it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                            }
                            else
                            {
                                it.marginPoint = 0;
                            }
                        }
                        catch { }

                        if (it.HindiName != null && !string.IsNullOrEmpty(lang) && lang == "hi")
                        {
                            if (it.IsSensitive == true && it.IsSensitiveMRP == true)
                            {
                                it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM;
                            }
                            else if (it.IsSensitive == true && it.IsSensitiveMRP == false)
                            {
                                it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name 
                            }

                            else if (it.IsSensitive == false && it.IsSensitiveMRP == false)
                            {
                                it.itemname = it.HindiName; //item display name
                            }
                            else if (it.IsSensitive == false && it.IsSensitiveMRP == true)
                            {
                                it.itemname = it.HindiName + " " + it.price + " MRP";//item display name 
                            }
                        }


                    }
                }
                RetailerAppManager retailerAppManager = new RetailerAppManager();
                List<ItemScheme> ItemSchemes = new List<ItemScheme>();
                var itemMultiMRPIds = ItemData.Select(x => x.ItemMultiMRPId).ToList();
                using (var context = new AuthContext())
                {
                    ItemSchemes = retailerAppManager.GetItemScheme(itemMultiMRPIds, warehouseId, context);
                }

                foreach (var item in ItemData)
                {
                    if (item.price > item.UnitPrice)
                    {
                        item.marginPoint = item.UnitPrice > 0 ? (((item.price - item.UnitPrice) * 100) / item.UnitPrice) : 0;//MP;  we replce marginpoint value by margin for app here 

                        if (ItemSchemes != null && ItemSchemes.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.PTR > 0))
                        {
                            var scheme = ItemSchemes.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId);
                            var ptrPercent = Math.Round((scheme.PTR - 1) * 100, 2);
                            var UPMRPMargin = item.marginPoint.Value;
                            if (UPMRPMargin - (ptrPercent + scheme.BaseScheme) > 0)
                                item.Scheme = ptrPercent + "% PTR + " + Math.Round(UPMRPMargin - ptrPercent, 2) + "% Extra";
                        }
                    }
                    else
                    {
                        item.marginPoint = 0;
                    }
                }

                itemResponseDc.ItemDataDCs = ItemData.GroupBy(x => new { x.ItemNumber, x.Itemtype }).Select(x => new DataContracts.External.SalesAppItemDataDC
                {
                    BaseCategoryId = x.FirstOrDefault().BaseCategoryId,
                    BillLimitQty = x.FirstOrDefault().BillLimitQty,
                    Categoryid = x.FirstOrDefault().Categoryid,
                    CompanyId = x.FirstOrDefault().CompanyId,
                    dreamPoint = x.FirstOrDefault().dreamPoint,
                    HindiName = x.FirstOrDefault().HindiName,
                    IsItemLimit = x.FirstOrDefault().IsItemLimit,
                    IsOffer = x.FirstOrDefault().IsOffer,
                    ItemId = x.FirstOrDefault().ItemId,
                    ItemlimitQty = x.FirstOrDefault().ItemlimitQty,
                    ItemMultiMRPId = x.FirstOrDefault().ItemMultiMRPId,
                    itemname = x.FirstOrDefault().itemname,
                    ItemNumber = x.FirstOrDefault().ItemNumber,
                    Itemtype = x.FirstOrDefault().Itemtype,
                    LastOrderDate = x.FirstOrDefault().LastOrderDate,
                    LastOrderDays = x.FirstOrDefault().LastOrderDays,
                    LastOrderQty = x.FirstOrDefault().LastOrderQty,
                    LogoUrl = x.FirstOrDefault().LogoUrl,
                    marginPoint = x.FirstOrDefault().marginPoint,
                    MinOrderQty = x.FirstOrDefault().MinOrderQty,
                    OfferCategory = x.FirstOrDefault().OfferCategory,
                    OfferFreeItemId = x.FirstOrDefault().OfferFreeItemId,
                    OfferFreeItemImage = x.FirstOrDefault().OfferFreeItemImage,
                    OfferFreeItemName = x.FirstOrDefault().OfferFreeItemName,
                    OfferFreeItemQuantity = x.FirstOrDefault().OfferFreeItemQuantity,
                    OfferId = x.FirstOrDefault().OfferId,
                    OfferMinimumQty = x.FirstOrDefault().OfferMinimumQty,
                    OfferType = x.FirstOrDefault().OfferType,
                    OfferWalletPoint = x.FirstOrDefault().OfferWalletPoint,
                    price = x.FirstOrDefault().price,
                    Sequence = x.FirstOrDefault().Sequence,
                    SubCategoryId = x.FirstOrDefault().SubCategoryId,
                    SubsubCategoryid = x.FirstOrDefault().SubsubCategoryid,
                    UnitPrice = x.FirstOrDefault().UnitPrice,
                    WarehouseId = x.FirstOrDefault().WarehouseId,
                    Active = x.FirstOrDefault().active,
                    Classification = x.FirstOrDefault().Classification,
                    BackgroundRgbColor = x.FirstOrDefault().BackgroundRgbColor,
                    moqList = x.Count() > 1 ? x.Select(y => new DataContracts.External.SalesAppItemDataDC
                    {
                        isChecked = (y.ItemMultiMRPId == x.FirstOrDefault().ItemMultiMRPId && y.MinOrderQty == x.FirstOrDefault().MinOrderQty),
                        BaseCategoryId = y.BaseCategoryId,
                        BillLimitQty = y.BillLimitQty,
                        Categoryid = y.Categoryid,
                        CompanyId = y.CompanyId,
                        dreamPoint = y.dreamPoint,
                        HindiName = y.HindiName,
                        IsItemLimit = y.IsItemLimit,
                        IsOffer = y.IsOffer,
                        ItemId = y.ItemId,
                        ItemlimitQty = y.ItemlimitQty,
                        ItemMultiMRPId = y.ItemMultiMRPId,
                        itemname = y.itemname,
                        ItemNumber = y.ItemNumber,
                        Itemtype = y.Itemtype,
                        LastOrderDate = y.LastOrderDate,
                        LastOrderDays = y.LastOrderDays,
                        LastOrderQty = y.LastOrderQty,
                        LogoUrl = y.LogoUrl,
                        marginPoint = y.marginPoint,
                        MinOrderQty = y.MinOrderQty,
                        OfferCategory = y.OfferCategory,
                        OfferFreeItemId = y.OfferFreeItemId,
                        OfferFreeItemImage = y.OfferFreeItemImage,
                        OfferFreeItemName = y.OfferFreeItemName,
                        OfferFreeItemQuantity = y.OfferFreeItemQuantity,
                        OfferId = y.OfferId,
                        OfferMinimumQty = y.OfferMinimumQty,
                        OfferType = y.OfferType,
                        OfferWalletPoint = y.OfferWalletPoint,
                        price = y.price,
                        Sequence = y.Sequence,
                        SubCategoryId = y.SubCategoryId,
                        SubsubCategoryid = y.SubsubCategoryid,
                        UnitPrice = y.UnitPrice,
                        WarehouseId = y.WarehouseId,
                        Active = y.active,
                        Classification = y.Classification,
                        BackgroundRgbColor = y.BackgroundRgbColor,
                    }).ToList() : new List<DataContracts.External.SalesAppItemDataDC>()
                }).OrderBy(x => x.Sequence).ToList();

            }


            return itemResponseDc;
        }

        [Route("GetAlreadyBoughtTargetItem")]
        [HttpGet]
        public async Task<DataContracts.External.SalesItemResponseDc> GetAlreadyBoughtTargetItem(int companyId, int storeId, int customerId, int warehouseId, int peopleId, int skip, int take, string lang, string itemname)
        {
            var itemResponseDc = new DataContracts.External.SalesItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.SalesAppItemDataDC>() };
            List<int> CatIds = new List<int>();
            List<int> SubCats = new List<int>();
            List<int> SubSubCats = new List<int>();
            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = new List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand>();
            if (peopleId != 0)
            {
                SalesAppItemController salesAppItemConTroller = new SalesAppItemController();
                StoreCategorySubCategoryBrands = salesAppItemConTroller.GetCatSubCatwithStores(peopleId);
                if (storeId > 0)
                    StoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => x.StoreId == storeId).ToList();
                CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();
            }

            
            var today = DateTime.Today;
            if (today.Day <= 10 && companyId == 0)
            {
                today = today.AddMonths(-1);
            }
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var startDate = monthStart.ToString("yyyy-MM-dd");
            var enddate = monthEnd.ToString("yyyy-MM-dd");

            string query = $"SELECT itemnumber from skorderdata_{AppConstants.Environment} where whid={warehouseId} and custid={customerId}  and createddate>='{startDate}' and createddate <= '{enddate}'";
            if (!string.IsNullOrEmpty(itemname))
                query += $"and (lcase(itemname) like '%{itemname.ToLower()}%' or lcase(itemnumber) like '%{itemname.ToLower()}%') ";

            if (companyId > 0)
                query += $" and compid={companyId}";

            if (CatIds != null && CatIds.Any())
            {
                query += $" and catid in ({string.Join(",", CatIds)})";
            }

            if (SubCats != null && SubCats.Any())
            {
                query += $" and compid in ({string.Join(",", SubCats)})";
            }
            if (SubSubCats != null && SubSubCats.Any())
            {
                query += $" and brandid in ({string.Join(",", SubSubCats)})";
            }

            query += " group by itemnumber";

            ElasticSqlHelper<elasticItemvalue> elasticSqlHelper = new ElasticSqlHelper<elasticItemvalue>();

            var orderitems = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync(query)).ToList());
            if (orderitems != null && orderitems.Any())
            {
                itemResponseDc.TotalItem = orderitems.Count();
                var orderitemnumbers = orderitems.OrderBy(x => x.itemnumber).Skip(skip).Take(take).Select(x => x.itemnumber).ToList();
                if (orderitemnumbers != null && orderitemnumbers.Any())
                {
                    List<DataContracts.External.MobileExecutiveDC.ItemDataDC> ItemData = new List<DataContracts.External.MobileExecutiveDC.ItemDataDC>();
                    ElasticSalesAppClusterItemDataHelper elasticSalesAppClusterItem = new ElasticSalesAppClusterItemDataHelper();
                    var data = AsyncContext.Run(() => elasticSalesAppClusterItem.GetCatelogAllElasticItemData(warehouseId, StoreCategorySubCategoryBrands, orderitemnumbers, "", -1, -1, 0, 1000, "ASC", true));
                    ItemData = data.ItemMasters;
                    if (ItemData != null && ItemData.Any())
                    {
                        ItemMasterManager itemMasterManager = new ItemMasterManager();
                        var itemsIncentiveClassification = await itemMasterManager.GetItemIncentiveClassification(warehouseId, ItemData.Select(s => s.ItemMultiMRPId).Distinct().ToList());


                        using (var context = new AuthContext())
                        {
                            var offerids = ItemData.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                            var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();

                            foreach (var it in ItemData)
                            {
                                it.Classification = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.Classification;
                                it.BackgroundRgbColor = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.BackgroundRgbColor;

                                //Condition for offer end
                                if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                                {
                                    if (it.OfferCategory == 2)
                                    {
                                        it.IsOffer = false;
                                        it.FlashDealSpecialPrice = 0;
                                        it.OfferCategory = 0;
                                    }
                                    else if (it.OfferCategory == 1)
                                    {
                                        it.IsOffer = false;
                                        it.OfferCategory = 0;
                                    }

                                }
                                else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;

                                }

                                if (it.OfferCategory == 1)
                                {
                                    if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                                        it.IsOffer = true;
                                    else
                                        it.IsOffer = false;
                                }

                                try
                                {
                                    if (!it.IsOffer)
                                    {
                                        /// Dream Point Logic && Margin Point
                                        int? MP, PP;
                                        double xPoint = xPointValue * 10;
                                        //Customer (0.2 * 10=1)
                                        if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                        {
                                            PP = 0;
                                        }
                                        else
                                        {
                                            PP = it.promoPerItems;
                                        }
                                        if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                        {
                                            MP = 0;
                                        }
                                        else
                                        {
                                            double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                            MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                        }
                                        if (PP > 0 && MP > 0)
                                        {
                                            int? PP_MP = PP + MP;
                                            it.dreamPoint = PP_MP;
                                        }
                                        else if (MP > 0)
                                        {
                                            it.dreamPoint = MP;
                                        }
                                        else if (PP > 0)
                                        {
                                            it.dreamPoint = PP;
                                        }
                                        else
                                        {
                                            it.dreamPoint = 0;
                                        }
                                    }
                                    else { it.dreamPoint = 0; }

                                    // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                                    if (it.price > it.UnitPrice)
                                    {
                                        it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                                    }
                                    else
                                    {
                                        it.marginPoint = 0;
                                    }
                                }
                                catch { }

                                if (it.HindiName != null && !string.IsNullOrEmpty(lang) && lang == "hi")
                                {
                                    if (it.IsSensitive == true && it.IsSensitiveMRP == true)
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM;
                                    }
                                    else if (it.IsSensitive == true && it.IsSensitiveMRP == false)
                                    {
                                        it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name 
                                    }

                                    else if (it.IsSensitive == false && it.IsSensitiveMRP == false)
                                    {
                                        it.itemname = it.HindiName; //item display name
                                    }
                                    else if (it.IsSensitive == false && it.IsSensitiveMRP == true)
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP";//item display name 
                                    }
                                }


                            }
                        }
                        RetailerAppManager retailerAppManager = new RetailerAppManager();
                        List<ItemScheme> ItemSchemes = new List<ItemScheme>();
                        var itemMultiMRPIds = ItemData.Select(x => x.ItemMultiMRPId).ToList();
                        using (var context = new AuthContext())
                        {
                            ItemSchemes = retailerAppManager.GetItemScheme(itemMultiMRPIds, warehouseId, context);
                        }

                        foreach (var item in ItemData)
                        {
                            if (item.price > item.UnitPrice)
                            {
                                item.marginPoint = item.UnitPrice > 0 ? (((item.price - item.UnitPrice) * 100) / item.UnitPrice) : 0;//MP;  we replce marginpoint value by margin for app here 

                                if (ItemSchemes != null && ItemSchemes.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.PTR > 0))
                                {
                                    var scheme = ItemSchemes.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId);
                                    var ptrPercent = Math.Round((scheme.PTR - 1) * 100, 2);
                                    var UPMRPMargin = item.marginPoint.Value;
                                    if (UPMRPMargin - (ptrPercent + scheme.BaseScheme) > 0)
                                        item.Scheme = ptrPercent + "% PTR + " + Math.Round(UPMRPMargin - ptrPercent, 2) + "% Extra";
                                }
                            }
                            else
                            {
                                item.marginPoint = 0;
                            }
                        }

                        itemResponseDc.ItemDataDCs = ItemData.GroupBy(x => new { x.ItemNumber, x.Itemtype }).Select(x => new DataContracts.External.SalesAppItemDataDC
                        {
                            BaseCategoryId = x.FirstOrDefault().BaseCategoryId,
                            BillLimitQty = x.FirstOrDefault().BillLimitQty,
                            Categoryid = x.FirstOrDefault().Categoryid,
                            CompanyId = x.FirstOrDefault().CompanyId,
                            dreamPoint = x.FirstOrDefault().dreamPoint,
                            HindiName = x.FirstOrDefault().HindiName,
                            IsItemLimit = x.FirstOrDefault().IsItemLimit,
                            IsOffer = x.FirstOrDefault().IsOffer,
                            ItemId = x.FirstOrDefault().ItemId,
                            ItemlimitQty = x.FirstOrDefault().ItemlimitQty,
                            ItemMultiMRPId = x.FirstOrDefault().ItemMultiMRPId,
                            itemname = x.FirstOrDefault().itemname,
                            ItemNumber = x.FirstOrDefault().ItemNumber,
                            Itemtype = x.FirstOrDefault().Itemtype,
                            LastOrderDate = x.FirstOrDefault().LastOrderDate,
                            LastOrderDays = x.FirstOrDefault().LastOrderDays,
                            LastOrderQty = x.FirstOrDefault().LastOrderQty,
                            LogoUrl = x.FirstOrDefault().LogoUrl,
                            marginPoint = x.FirstOrDefault().marginPoint,
                            MinOrderQty = x.FirstOrDefault().MinOrderQty,
                            OfferCategory = x.FirstOrDefault().OfferCategory,
                            OfferFreeItemId = x.FirstOrDefault().OfferFreeItemId,
                            OfferFreeItemImage = x.FirstOrDefault().OfferFreeItemImage,
                            OfferFreeItemName = x.FirstOrDefault().OfferFreeItemName,
                            OfferFreeItemQuantity = x.FirstOrDefault().OfferFreeItemQuantity,
                            OfferId = x.FirstOrDefault().OfferId,
                            OfferMinimumQty = x.FirstOrDefault().OfferMinimumQty,
                            OfferType = x.FirstOrDefault().OfferType,
                            OfferWalletPoint = x.FirstOrDefault().OfferWalletPoint,
                            price = x.FirstOrDefault().price,
                            Sequence = x.FirstOrDefault().Sequence,
                            SubCategoryId = x.FirstOrDefault().SubCategoryId,
                            SubsubCategoryid = x.FirstOrDefault().SubsubCategoryid,
                            UnitPrice = x.FirstOrDefault().UnitPrice,
                            WarehouseId = x.FirstOrDefault().WarehouseId,
                            Active = x.FirstOrDefault().active,
                            Classification = x.FirstOrDefault().Classification,
                            BackgroundRgbColor = x.FirstOrDefault().BackgroundRgbColor,
                            moqList = x.Count() > 1 ? x.Select(y => new DataContracts.External.SalesAppItemDataDC
                            {
                                isChecked = (y.ItemMultiMRPId == x.FirstOrDefault().ItemMultiMRPId && y.MinOrderQty == x.FirstOrDefault().MinOrderQty),
                                BaseCategoryId = y.BaseCategoryId,
                                BillLimitQty = y.BillLimitQty,
                                Categoryid = y.Categoryid,
                                CompanyId = y.CompanyId,
                                dreamPoint = y.dreamPoint,
                                HindiName = y.HindiName,
                                IsItemLimit = y.IsItemLimit,
                                IsOffer = y.IsOffer,
                                ItemId = y.ItemId,
                                ItemlimitQty = y.ItemlimitQty,
                                ItemMultiMRPId = y.ItemMultiMRPId,
                                itemname = y.itemname,
                                ItemNumber = y.ItemNumber,
                                Itemtype = y.Itemtype,
                                LastOrderDate = y.LastOrderDate,
                                LastOrderDays = y.LastOrderDays,
                                LastOrderQty = y.LastOrderQty,
                                LogoUrl = y.LogoUrl,
                                marginPoint = y.marginPoint,
                                MinOrderQty = y.MinOrderQty,
                                OfferCategory = y.OfferCategory,
                                OfferFreeItemId = y.OfferFreeItemId,
                                OfferFreeItemImage = y.OfferFreeItemImage,
                                OfferFreeItemName = y.OfferFreeItemName,
                                OfferFreeItemQuantity = y.OfferFreeItemQuantity,
                                OfferId = y.OfferId,
                                OfferMinimumQty = y.OfferMinimumQty,
                                OfferType = y.OfferType,
                                OfferWalletPoint = y.OfferWalletPoint,
                                price = y.price,
                                Sequence = y.Sequence,
                                SubCategoryId = y.SubCategoryId,
                                SubsubCategoryid = y.SubsubCategoryid,
                                UnitPrice = y.UnitPrice,
                                WarehouseId = y.WarehouseId,
                                Active = y.active,
                                Classification = y.Classification,
                                BackgroundRgbColor = y.BackgroundRgbColor,
                            }).ToList() : new List<DataContracts.External.SalesAppItemDataDC>()
                        }).OrderBy(x => x.Sequence).ToList();

                    }
                }
            }
            return itemResponseDc;
        }


        [Route("GetMoreInfoForSalesApp")]
        [HttpGet]
        public async Task<string> GetMoreInfoForSalesApp(string key, int peopleId)
        {
            string result = string.Empty;
            var today = DateTime.Today;
            if (today.Day <= 10)
            {
                today = today.AddMonths(-1);
            }

            MongoDbHelper<TargetAndIncentiveMoreInfo> mongoDbHelper = new MongoDbHelper<TargetAndIncentiveMoreInfo>();
            var data = (await mongoDbHelper.SelectAsync(x => x.Key == key)).FirstOrDefault();
            result = data.Template;
            if (key == "KPI" || key == "ItemCommission")
            {
                ItemMasterManager manager = new ItemMasterManager();
                var configs = await manager.GetSalesPersonKPIInfo(peopleId, key, today.Month, today.Year);
                if (key == "KPI")
                {
                    result = result.Replace("#KPI#", configs);
                }
                else if (key == "ItemCommission")
                {
                    result = result.Replace("#ItemCommission#", configs);
                }
            }

            return result;
        }

        [Route("CalculateValue")]
        [HttpGet]
        public string CalculateValue(DateTime fromdate)
        {
            string FileUrl = "";
            using (var con = new AuthContext())
            {

                var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
                string DocumentName = "MonthlyTargetData_" + fromdate.Month.ToString() + fromdate.Year.ToString();
                var Updateordervalue = MonthlyCustomerTarget.Select(x => x.WarehouseId > -1, null, null, null, false, "", DocumentName).ToList();

                var data = Updateordervalue.Where(x => x.TargetOnStores != null && x.TargetOnStores.Any(y => y.TargetLineItem <= y.CurrentLineItem && y.Target <= y.CurrentVolume)).ToList();
                List<TargetData> targetlist = new List<TargetData>();

                targetlist = data.SelectMany(x => x.TargetOnStores.Select(y => new TargetData
                {
                    CurrentLineItem = y.CurrentLineItem,
                    CurrentValume = y.CurrentVolume,
                    StoreName = y.StoreName,
                    TargetLineItem = y.TargetLineItem,
                    Target = y.Target,
                    Skcode = x.Skcode
                })).Where(x => x.Target <= x.CurrentValume && (!x.TargetLineItem.HasValue || x.TargetLineItem <= x.CurrentLineItem)).ToList(); ;


                var TargetDt = ClassToDataTable.CreateDataTable(targetlist);

                string ExcelSavePath = HttpContext.Current.Server.MapPath("~/Target/TargetExcel");
                if (!Directory.Exists(ExcelSavePath))
                    Directory.CreateDirectory(ExcelSavePath);

                var fileName = "Target" + DateTime.Now.ToString("ddMMyyyy") + ".xlsx";
                string filePath = ExcelSavePath + "\\" + fileName;

                ExcelGenerator.DataTable_To_Excel(TargetDt, "Target", filePath);

                FileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                              , HttpContext.Current.Request.Url.DnsSafeHost
                                                              , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                              , "/Target/TargetExcel/" + fileName);


            }
            return FileUrl;
        }


        [Route("GetCustomerTargetDashboard")]
        [HttpPost]
        public HttpResponseMessage GetCustomerTargetDashboardAsync(CustomerTargetDashboardFilter customerTargetDashboardFilter)
        {
            CustomerTargetDashboardDetail customerTargetDashboardDetail = new CustomerTargetDashboardDetail();
            int TotalCount = 0;
            var targetList = new List<CustomerTargetDashboard>();

            using (var con = new AuthContext())
            {
                var date = Convert.ToDateTime(customerTargetDashboardFilter.Year + "-" + customerTargetDashboardFilter.Month + "-" + "01");
                var isMonthComplete = false;
                //if (date.Day < 6)
                //{
                //    isMonthComplete = true;
                //    date = DateTime.Now.AddMonths(-1);
                //}
                var lastDayOfMonth = DateTime.DaysInMonth(date.Year, date.Month);

                var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
                string DocumentName = "MonthlyTargetData_" + date.Month.ToString() + date.Year.ToString();
                MonthlyCustomerTarget monthlyCustomerTarget = new MonthlyCustomerTarget();
                List<MonthlyCustomerTarget> monthlyCustomerTargetList = new List<MonthlyCustomerTarget>();
                var MonthlyCustomerTargetData = MonthlyCustomerTarget.Select(x => x.WarehouseId != 0, null, null, null, false, "", DocumentName).ToList();
                if (customerTargetDashboardFilter.SkCode != null && customerTargetDashboardFilter.SkCode.Any())
                {
                    foreach (var skcode in customerTargetDashboardFilter.SkCode)
                    {
                        monthlyCustomerTarget = MonthlyCustomerTargetData.Where(x => x.Skcode == skcode).FirstOrDefault();
                        if (monthlyCustomerTarget != null)
                            monthlyCustomerTargetList.Add(monthlyCustomerTarget);
                    }
                }
                else
                {
                    foreach (var WarehouseId in customerTargetDashboardFilter.WarehouseId)
                    {
                        List<MonthlyCustomerTarget> monthlyCustomerTargetData = MonthlyCustomerTargetData.Where(x => x.WarehouseId == WarehouseId).ToList();
                        if (monthlyCustomerTargetData != null && monthlyCustomerTargetData.Any())
                            monthlyCustomerTargetList.AddRange(monthlyCustomerTargetData);
                    }
                }

                if (monthlyCustomerTargetList != null && monthlyCustomerTargetList.Any())
                {
                    TotalCount = monthlyCustomerTargetList.Count();

                    int skiprows = customerTargetDashboardFilter.Skip * customerTargetDashboardFilter.Take;

                    var levelids = con.LevelMasterDB.Where(x => x.CreatedDate.Month == customerTargetDashboardFilter.Month && x.CreatedDate.Year == customerTargetDashboardFilter.Year).ToList();
                    var customerBands = con.CustomerBandsDB.Where(x => x.CreatedDate.Month == customerTargetDashboardFilter.Month && x.CreatedDate.Year == customerTargetDashboardFilter.Year).ToList();
                    var rewardItems = con.RewardItemsDb.Where(x => x.IsActive && !x.IsDeleted).ToList();
                    monthlyCustomerTargetList = monthlyCustomerTargetList.Skip(skiprows).Take(customerTargetDashboardFilter.Take).ToList();
                    foreach (var GetExportData in monthlyCustomerTargetList)
                    {
                        var levelid = levelids.Where(x => x.Name == GetExportData.Levels && x.WarehouseId == GetExportData.WarehouseId).Select(x => x.Id).FirstOrDefault();
                        var customerBand = customerBands.Where(x => x.LevelId == levelid && x.BandName == GetExportData.Bands).Select(x => new { x.Type, x.value, x.GiftId }).FirstOrDefault();
                        var giftDatail = customerBand != null && customerBand.Type == "Gift" && customerBand.GiftId.HasValue ? rewardItems.Where(x => x.rItemId == customerBand.GiftId).Select(x => new { reward = x.rName, rewardPoints = x.rPoint }).FirstOrDefault() : null;
                        var target = new CustomerTargetDashboard();
                        var totalpuramt = GetExportData.CurrentVolume;

                        target.SKCode = GetExportData.Skcode;
                        target.TargetMonth = date.ToString("MMMM");
                        target.TargetAmount = GetExportData.Target;

                        target.TargetLineItem = GetExportData.TargetLineItem.HasValue ? GetExportData.TargetLineItem.Value : 0;
                        target.CurrentLineItem = GetExportData.CurrentLineItem.HasValue ? GetExportData.CurrentLineItem.Value : 0;

                        target.MaxDiscount = GetExportData.MaxDiscount;
                        target.MOVMultiplier = GetExportData.MOVMultiplier;
                        target.IsClaimed = GetExportData.IsClaimed ? "Yes" : "No";
                        target.LeftDays = isMonthComplete ? 0 : (lastDayOfMonth - date.Day);
                        target.RewardType = customerBand != null && customerBand.Type != null ? customerBand.Type : string.Empty;
                        target.RewardValue = customerBand != null && customerBand.value != null ? customerBand.value : 0;
                        target.RewardDescription = (giftDatail != null && customerBand != null && customerBand.Type != null) && customerBand.Type == "Gift" ? giftDatail.reward : string.Empty;

                        target.TotalPurchaseAmount = Convert.ToDecimal(totalpuramt);
                        target.TotalPendingPurchaseAmount = Convert.ToDecimal(GetExportData.PendingVolume);
                        try
                        {
                            var amtPer = target.TotalPurchaseAmount / (target.TargetAmount / 100);
                            var lineitemPer = target.TargetLineItem.HasValue && target.TargetLineItem.Value > 0 ? (target.CurrentLineItem.HasValue ? target.CurrentLineItem.Value : 0) / (Convert.ToDecimal(target.TargetLineItem.Value) / 100) : 100;

                            target.AchivePercent = (amtPer + lineitemPer) / 2;
                            if (target.AchivePercent > 100)
                                target.AchivePercent = 100;

                            targetList.Add(target);
                        }
                        catch (Exception)
                        {
                            target.AchivePercent = 0;
                        }
                        customerTargetDashboardDetail.TotalCount = TotalCount;
                        customerTargetDashboardDetail.CustomerTargetDashboard = targetList;
                    }
                    var response = new
                    {
                        GetTargetData = customerTargetDashboardDetail,
                        Status = true,
                        Message = "Target Data"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var response = new
                    {
                        GetTargetData = customerTargetDashboardDetail,
                        Status = false,
                        Message = "Target Data Not Found."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
        }

        [Route("GetSkCodeListForTarget")]
        [HttpPost]
        [AllowAnonymous]
        public List<SkCodeListForDashboard> GetSkCodeListForTargetAsync(SkCodeListFilter skCodeListFilter)
        {
            using (AuthContext context = new AuthContext())
            {
                var dtCity = new DataTable();
                dtCity.Columns.Add("IntValue");
                foreach (var item in skCodeListFilter.CityId)
                {
                    var dr = dtCity.NewRow();
                    dr["IntValue"] = item;
                    dtCity.Rows.Add(dr);
                }
                var paramCityIds = new SqlParameter
                {
                    ParameterName = "CityId",
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.IntValues",
                    Value = dtCity
                };

                var dtWarehouseId = new DataTable();
                dtWarehouseId.Columns.Add("IntValue");
                foreach (var whid in skCodeListFilter.WarehouseId)
                {
                    var dr = dtWarehouseId.NewRow();
                    dr["IntValue"] = whid;
                    dtWarehouseId.Rows.Add(dr);
                }
                var paramWarehouseIds = new SqlParameter
                {
                    ParameterName = "WarehouseId",
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.IntValues",
                    Value = dtWarehouseId
                };

                List<SkCodeListForDashboard> skCodes = context.Database.SqlQuery<SkCodeListForDashboard>("exec [GetSkCodeListForTarget] @CityId, @WarehouseId", paramCityIds, paramWarehouseIds).ToList();
                return skCodes;
            }
        }


        [Route("ExportCustomerTargetMonthWise")]
        [HttpPost]
        public HttpResponseMessage ExportCustomerTargetMonthWise(int Month, int Year, List<int> WarehousIds)
        {
            var targetList = new List<ExportCustomerTarget>();

            using (var con = new AuthContext())
            {
                var date = Convert.ToDateTime(Year + "-" + Month + "-" + "01");
                var isMonthComplete = false;
                //if (date.Day < 6)
                //{
                //    isMonthComplete = true;
                //    date = DateTime.Now.AddMonths(-1);
                //}
                var lastDayOfMonth = DateTime.DaysInMonth(Year, Month);

                var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
                string DocumentName = "MonthlyTargetData_" + date.Month.ToString() + date.Year.ToString();
                MonthlyCustomerTarget monthlyCustomerTarget = new MonthlyCustomerTarget();
                List<MonthlyCustomerTarget> monthlyCustomerTargetList = new List<MonthlyCustomerTarget>();
                monthlyCustomerTargetList = MonthlyCustomerTarget.Select(x => WarehousIds.Contains(x.WarehouseId) && x.WarehouseId != 0, null, null, null, false, "", DocumentName).ToList();

                if (monthlyCustomerTargetList != null && monthlyCustomerTargetList.Any())
                {
                    var levelids = con.LevelMasterDB.Where(x => x.CreatedDate.Month == Month && x.CreatedDate.Year == Year).ToList();
                    var customerBands = con.CustomerBandsDB.Where(x => x.CreatedDate.Month == Month && x.CreatedDate.Year == Year).ToList();
                    var rewardItems = con.RewardItemsDb.Where(x => x.IsActive && !x.IsDeleted).ToList();
                    foreach (var GetExportData in monthlyCustomerTargetList)
                    {
                        var levelid = levelids.Where(x => x.Name == GetExportData.Levels && x.WarehouseId == GetExportData.WarehouseId).Select(x => x.Id).FirstOrDefault();
                        var customerBand = customerBands.Where(x => x.LevelId == levelid && x.BandName == GetExportData.Bands).Select(x => new { x.Type, x.value, x.GiftId }).FirstOrDefault();
                        var giftDatail = customerBand != null && customerBand.Type == "Gift" && customerBand.GiftId.HasValue ? rewardItems.Where(x => x.rItemId == customerBand.GiftId).Select(x => new { reward = x.rName, rewardPoints = x.rPoint }).FirstOrDefault() : null;
                        var target = new ExportCustomerTarget();
                        var totalpuramt = GetExportData.CurrentVolume;

                        target.SKCode = GetExportData.Skcode;
                        target.TargetMonth = date.ToString("MMMM");
                        target.TargetAmount = GetExportData.Target;

                        target.TargetLineItem = GetExportData.TargetLineItem.HasValue ? GetExportData.TargetLineItem.Value : 0;
                        target.CurrentLineItem = GetExportData.CurrentLineItem.HasValue ? GetExportData.CurrentLineItem.Value : 0;

                        target.MaxDiscount = GetExportData.MaxDiscount;
                        target.MOVMultiplier = GetExportData.MOVMultiplier;
                        target.IsClaimed = GetExportData.IsClaimed ? "Yes" : "No";
                        target.LeftDays = isMonthComplete ? 0 : (lastDayOfMonth - date.Day);
                        target.RewardType = customerBand != null && customerBand.Type != null ? customerBand.Type : string.Empty;
                        target.RewardValue = customerBand != null && customerBand.value != null ? customerBand.value : 0;
                        target.RewardDescription = (giftDatail != null && customerBand != null && customerBand.Type != null) && customerBand.Type == "Gift" ? giftDatail.reward : string.Empty;

                        target.TotalPurchaseAmount = Convert.ToDecimal(totalpuramt);
                        target.TotalPendingPurchaseAmount = Convert.ToDecimal(GetExportData.PendingVolume);
                        target.StoreNames = GetExportData.TargetOnStores != null ? string.Join(",", GetExportData.TargetOnStores.Select(x => x.StoreName)) : string.Empty;
                        try
                        {
                            var amtPer = target.TotalPurchaseAmount / (target.TargetAmount / 100);
                            var lineitemPer = target.TargetLineItem.HasValue && target.TargetLineItem.Value > 0 ? (target.CurrentLineItem.HasValue ? target.CurrentLineItem.Value : 0) / (Convert.ToDecimal(target.TargetLineItem.Value) / 100) : 100;

                            target.AchivePercent = (amtPer + lineitemPer) / 2;
                            if (target.AchivePercent > 100)
                                target.AchivePercent = 100;

                            targetList.Add(target);
                        }
                        catch (Exception)
                        {
                            target.AchivePercent = 0;
                        }

                    }
                    var response = new
                    {
                        GetTargetData = targetList,
                        Status = true,
                        Message = "Target Data"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var response = new
                    {
                        GetTargetData = new CustomerTarget(),
                        Status = false,
                        Message = "Target Data Not Found."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
        }

        [Route("GetTargetItemForSalesApp")]
        [HttpGet]
        public async Task<DataContracts.External.SalesItemResponseDc> GetTargetItemForSalesApp(int customerId, int warehouseId, int peopleId, int skip, int take, string lang)
        {
            skip = skip * 10;
            var SkCode = "";
            List<long> StoreIds = new List<long>();
            DataContracts.External.SalesItemResponseDc itemResponseDc = new DataContracts.External.SalesItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.SalesAppItemDataDC>() };

            using (var context = new AuthContext())
            {
                StoreIds = (from cse in context.ClusterStoreExecutives
                                        join s in context.StoreDB on cse.StoreId equals s.Id
                                        join c in context.Clusters on cse.ClusterId equals c.ClusterId
                                        where cse.IsActive == true && cse.IsDeleted == false && s.IsActive == true && s.IsDeleted == false
                                        && cse.ExecutiveId == peopleId && c.Active == true && c.Deleted == false
                                        select cse.StoreId
                                        ).Distinct().ToList();
                SkCode = context.Customers.Where(x => x.CustomerId == customerId && x.Active == true && x.Deleted == false).Select(x => x.Skcode).FirstOrDefault();
                
            }

            var today = DateTime.Today;
            if (today.Day <= 10)
            {
                today = today.AddMonths(-1);
            }
            var cartPredicate = PredicateBuilder.New<MonthlyCustomerTarget>(x => x.WarehouseId == warehouseId && x.Skcode == SkCode);
            var MonthlyCustomerTarget = new MongoDbHelper<MonthlyCustomerTarget>();
            string DocumentName = "MonthlyTargetData_" + today.Month.ToString() + today.Year.ToString();
            var TargetCustomers = MonthlyCustomerTarget.Select(cartPredicate, null, null, null, false, "", DocumentName).ToList();

            var StoreWiseTarget = TargetCustomers.Where(x => x.TargetOnStores != null).SelectMany(x => x.TargetOnStores).Where(y => StoreIds.Contains(y.StoreId)).ToList();

            if (StoreWiseTarget.Count > 0 && StoreWiseTarget.Any() && StoreIds.Count > 0 && StoreIds.Any())
            {
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = new List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand>();

                List<int> CatIds = new List<int>();
                List<int> SubCats = new List<int>();
                List<int> SubSubCats = new List<int>();
                if (peopleId != 0)
                {
                    SalesAppItemController salesAppItemConTroller = new SalesAppItemController();
                    StoreCategorySubCategoryBrands = salesAppItemConTroller.GetCatSubCatwithStores(peopleId);
                    if (StoreIds.Count > 0 && StoreIds.Any())
                        StoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => StoreIds.Contains(x.StoreId)).ToList();
                    CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                    SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                    SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();
                }

                var monthStart = new DateTime(today.Year, today.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var startDate = monthStart.ToString("yyyy-MM-dd");
                var enddate = monthEnd.ToString("yyyy-MM-dd");

                string query = $"SELECT itemnumber from skorderdata_{AppConstants.Environment} where whid={warehouseId} and custid={customerId}  and createddate>='{startDate}' and createddate <= '{enddate}'";

                if(StoreIds.Count > 0)
                {
                    query += $" and storeid in ({string.Join(",", StoreIds)})";
                }

                if (CatIds != null && CatIds.Any())
                {
                    query += $" and catid in ({string.Join(",", CatIds)})";
                }

                if (SubCats != null && SubCats.Any())
                {
                    query += $" and compid in ({string.Join(",", SubCats)})";
                }
                if (SubSubCats != null && SubSubCats.Any())
                {
                    query += $" and brandid in ({string.Join(",", SubSubCats)})";
                }

                query += " group by itemnumber";


                ElasticSqlHelper<elasticItemvalue> elasticSqlHelper = new ElasticSqlHelper<elasticItemvalue>();

                var orderitems = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync(query)).ToList());
                var itemnumbers = string.Empty;
                if (orderitems != null && orderitems.Any())
                {
                    itemnumbers = "'" + string.Join("','", orderitems.Select(x => x.itemnumber)) + "'";
                }
                string itemIndex = ConfigurationManager.AppSettings["ElasticSearchIndexName"];
                query = $"SELECT itemnumber from {itemIndex} where warehouseid={warehouseId} " +
                    $" and active=true and deleted=false and  isdiscontinued=false and (isitemlimit=false or (isitemlimit=true and itemlimitqty>0 and itemlimitqty-minorderqty>0 )) and (itemapptype=0 or itemapptype=1)";

                if (!string.IsNullOrEmpty(itemnumbers))
                    query += $" and itemnumber not in ({ itemnumbers })";

                if (CatIds != null && CatIds.Any())
                {
                    query += $" and categoryid in ({string.Join(",", CatIds)})";
                }

                if (SubCats != null && SubCats.Any())
                {
                    query += $" and subcategoryid in ({string.Join(",", SubCats)})";
                }
                if (SubSubCats != null && SubSubCats.Any())
                {
                    query += $" and subsubcategoryid in ({string.Join(",", SubSubCats)})";
                }

                query += " group by itemnumber";

                ElasticSqlHelper<elasticItemvalue> elasticSqlHelper1 = new ElasticSqlHelper<elasticItemvalue>();

                var dbItemNumbers = AsyncContext.Run(async () => (await elasticSqlHelper1.GetListAsync(query)).ToList());

                itemResponseDc.TotalItem = dbItemNumbers.Count();
                var itemNumbers = dbItemNumbers.OrderBy(x => x.itemnumber).Skip(skip).Take(take).Select(x => x.itemnumber).ToList();

                List<DataContracts.External.MobileExecutiveDC.ItemDataDC> ItemData = new List<DataContracts.External.MobileExecutiveDC.ItemDataDC>();
                if (itemNumbers != null && itemNumbers.Any())
                {
                    ElasticSalesAppClusterItemDataHelper elasticSalesAppClusterItem = new ElasticSalesAppClusterItemDataHelper();
                    var data = AsyncContext.Run(() => elasticSalesAppClusterItem.GetSalesCatelogElasticData(warehouseId, StoreCategorySubCategoryBrands, itemNumbers, "", -1, -1, 0, 1000, "ASC", true, null));
                    ItemData = data.ItemMasters;
                }
                if (ItemData != null && ItemData.Any())
                {
                    ItemMasterManager itemMasterManager = new ItemMasterManager();
                    var itemsIncentiveClassification = await itemMasterManager.GetItemIncentiveClassification(warehouseId, ItemData.Select(s => s.ItemMultiMRPId).Distinct().ToList());

                    using (var context = new AuthContext())
                    {
                        var offerids = ItemData.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                        var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();

                        foreach (var it in ItemData)
                        {
                            it.Classification = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.Classification;
                            it.BackgroundRgbColor = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.BackgroundRgbColor;

                            //Condition for offer end
                            if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                            {
                                if (it.OfferCategory == 2)
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;
                                }
                                else if (it.OfferCategory == 1)
                                {
                                    it.IsOffer = false;
                                    it.OfferCategory = 0;
                                }

                            }
                            else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;

                            }

                            if (it.OfferCategory == 1)
                            {
                                if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                                    it.IsOffer = true;
                                else
                                    it.IsOffer = false;
                            }

                            try
                            {
                                if (!it.IsOffer)
                                {
                                    /// Dream Point Logic && Margin Point
                                    int? MP, PP;
                                    double xPoint = xPointValue * 10;
                                    //Customer (0.2 * 10=1)
                                    if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                    {
                                        PP = 0;
                                    }
                                    else
                                    {
                                        PP = it.promoPerItems;
                                    }
                                    if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                    {
                                        MP = 0;
                                    }
                                    else
                                    {
                                        double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                        MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                    }
                                    if (PP > 0 && MP > 0)
                                    {
                                        int? PP_MP = PP + MP;
                                        it.dreamPoint = PP_MP;
                                    }
                                    else if (MP > 0)
                                    {
                                        it.dreamPoint = MP;
                                    }
                                    else if (PP > 0)
                                    {
                                        it.dreamPoint = PP;
                                    }
                                    else
                                    {
                                        it.dreamPoint = 0;
                                    }
                                }
                                else { it.dreamPoint = 0; }

                                // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                                if (it.price > it.UnitPrice)
                                {
                                    it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                                }
                                else
                                {
                                    it.marginPoint = 0;
                                }
                            }
                            catch { }

                            if (it.HindiName != null && !string.IsNullOrEmpty(lang) && lang == "hi")
                            {
                                if (it.IsSensitive == true && it.IsSensitiveMRP == true)
                                {
                                    it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM;
                                }
                                else if (it.IsSensitive == true && it.IsSensitiveMRP == false)
                                {
                                    it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name 
                                }

                                else if (it.IsSensitive == false && it.IsSensitiveMRP == false)
                                {
                                    it.itemname = it.HindiName; //item display name
                                }
                                else if (it.IsSensitive == false && it.IsSensitiveMRP == true)
                                {
                                    it.itemname = it.HindiName + " " + it.price + " MRP";//item display name 
                                }
                            }


                        }
                    }
                    RetailerAppManager retailerAppManager = new RetailerAppManager();
                    List<ItemScheme> ItemSchemes = new List<ItemScheme>();
                    var itemMultiMRPIds = ItemData.Select(x => x.ItemMultiMRPId).ToList();
                    using (var context = new AuthContext())
                    {
                        ItemSchemes = retailerAppManager.GetItemScheme(itemMultiMRPIds, warehouseId, context);
                    }

                    foreach (var item in ItemData)
                    {
                        if (item.price > item.UnitPrice)
                        {
                            item.marginPoint = item.UnitPrice > 0 ? (((item.price - item.UnitPrice) * 100) / item.UnitPrice) : 0;//MP;  we replce marginpoint value by margin for app here 

                            if (ItemSchemes != null && ItemSchemes.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.PTR > 0))
                            {
                                var scheme = ItemSchemes.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId);
                                var ptrPercent = Math.Round((scheme.PTR - 1) * 100, 2);
                                var UPMRPMargin = item.marginPoint.Value;
                                if (UPMRPMargin - (ptrPercent + scheme.BaseScheme) > 0)
                                    item.Scheme = ptrPercent + "% PTR + " + Math.Round(UPMRPMargin - ptrPercent, 2) + "% Extra";
                            }
                        }
                        else
                        {
                            item.marginPoint = 0;
                        }
                    }

                    itemResponseDc.ItemDataDCs = ItemData.GroupBy(x => new { x.ItemNumber, x.Itemtype }).Select(x => new DataContracts.External.SalesAppItemDataDC
                    {
                        BaseCategoryId = x.FirstOrDefault().BaseCategoryId,
                        BillLimitQty = x.FirstOrDefault().BillLimitQty,
                        Categoryid = x.FirstOrDefault().Categoryid,
                        CompanyId = x.FirstOrDefault().CompanyId,
                        dreamPoint = x.FirstOrDefault().dreamPoint,
                        HindiName = x.FirstOrDefault().HindiName,
                        IsItemLimit = x.FirstOrDefault().IsItemLimit,
                        IsOffer = x.FirstOrDefault().IsOffer,
                        ItemId = x.FirstOrDefault().ItemId,
                        ItemlimitQty = x.FirstOrDefault().ItemlimitQty,
                        ItemMultiMRPId = x.FirstOrDefault().ItemMultiMRPId,
                        itemname = x.FirstOrDefault().itemname,
                        ItemNumber = x.FirstOrDefault().ItemNumber,
                        Itemtype = x.FirstOrDefault().Itemtype,
                        LastOrderDate = x.FirstOrDefault().LastOrderDate,
                        LastOrderDays = x.FirstOrDefault().LastOrderDays,
                        LastOrderQty = x.FirstOrDefault().LastOrderQty,
                        LogoUrl = x.FirstOrDefault().LogoUrl,
                        marginPoint = x.FirstOrDefault().marginPoint,
                        MinOrderQty = x.FirstOrDefault().MinOrderQty,
                        OfferCategory = x.FirstOrDefault().OfferCategory,
                        OfferFreeItemId = x.FirstOrDefault().OfferFreeItemId,
                        OfferFreeItemImage = x.FirstOrDefault().OfferFreeItemImage,
                        OfferFreeItemName = x.FirstOrDefault().OfferFreeItemName,
                        OfferFreeItemQuantity = x.FirstOrDefault().OfferFreeItemQuantity,
                        OfferId = x.FirstOrDefault().OfferId,
                        OfferMinimumQty = x.FirstOrDefault().OfferMinimumQty,
                        OfferType = x.FirstOrDefault().OfferType,
                        OfferWalletPoint = x.FirstOrDefault().OfferWalletPoint,
                        price = x.FirstOrDefault().price,
                        Sequence = x.FirstOrDefault().Sequence,
                        SubCategoryId = x.FirstOrDefault().SubCategoryId,
                        SubsubCategoryid = x.FirstOrDefault().SubsubCategoryid,
                        UnitPrice = x.FirstOrDefault().UnitPrice,
                        WarehouseId = x.FirstOrDefault().WarehouseId,
                        Active = x.FirstOrDefault().active,
                        Classification = x.FirstOrDefault().Classification,
                        BackgroundRgbColor = x.FirstOrDefault().BackgroundRgbColor,
                        moqList = x.Count() > 1 ? x.Select(y => new DataContracts.External.SalesAppItemDataDC
                        {
                            isChecked = (y.ItemMultiMRPId == x.FirstOrDefault().ItemMultiMRPId && y.MinOrderQty == x.FirstOrDefault().MinOrderQty),
                            BaseCategoryId = y.BaseCategoryId,
                            BillLimitQty = y.BillLimitQty,
                            Categoryid = y.Categoryid,
                            CompanyId = y.CompanyId,
                            dreamPoint = y.dreamPoint,
                            HindiName = y.HindiName,
                            IsItemLimit = y.IsItemLimit,
                            IsOffer = y.IsOffer,
                            ItemId = y.ItemId,
                            ItemlimitQty = y.ItemlimitQty,
                            ItemMultiMRPId = y.ItemMultiMRPId,
                            itemname = y.itemname,
                            ItemNumber = y.ItemNumber,
                            Itemtype = y.Itemtype,
                            LastOrderDate = y.LastOrderDate,
                            LastOrderDays = y.LastOrderDays,
                            LastOrderQty = y.LastOrderQty,
                            LogoUrl = y.LogoUrl,
                            marginPoint = y.marginPoint,
                            MinOrderQty = y.MinOrderQty,
                            OfferCategory = y.OfferCategory,
                            OfferFreeItemId = y.OfferFreeItemId,
                            OfferFreeItemImage = y.OfferFreeItemImage,
                            OfferFreeItemName = y.OfferFreeItemName,
                            OfferFreeItemQuantity = y.OfferFreeItemQuantity,
                            OfferId = y.OfferId,
                            OfferMinimumQty = y.OfferMinimumQty,
                            OfferType = y.OfferType,
                            OfferWalletPoint = y.OfferWalletPoint,
                            price = y.price,
                            Sequence = y.Sequence,
                            SubCategoryId = y.SubCategoryId,
                            SubsubCategoryid = y.SubsubCategoryid,
                            UnitPrice = y.UnitPrice,
                            WarehouseId = y.WarehouseId,
                            Active = y.active,
                            Classification = y.Classification,
                            BackgroundRgbColor = y.BackgroundRgbColor,
                        }).ToList() : new List<DataContracts.External.SalesAppItemDataDC>()
                    }).OrderBy(x => x.Sequence).ToList();

                }
            }


            return itemResponseDc;
        }

    }
}

public class SkCodeListForDashboard
{
    public string skcode { get; set; }
    public string City { get; set; }
    public int Cityid { get; set; }
    public int Warehouseid { get; set; }
}
public class SkCodeListFilter
{
    public List<int> CityId { get; set; }
    public List<int> WarehouseId { get; set; }
}
public class TargetData
{
    public string Skcode { get; set; }
    public string StoreName { get; set; }
    public double Target { get; set; }
    public double CurrentValume { get; set; }
    public int? TargetLineItem { get; set; }
    public int? CurrentLineItem { get; set; }
}
public class TargetAndIncentiveMoreInfo
{
    [BsonId]
    public MongoDB.Bson.ObjectId Id { get; set; }
    public string Key { get; set; }
    public string Template { get; set; }
}

public class elasticItemvalue
{
    public string itemnumber { get; set; }
}
public class orderDataTarget
{
    public DateTime CreatedDate { get; set; }
    public int OrderId { get; set; }
    public string skcode { get; set; }
    public string Status { get; set; }
    public int CustomerId { get; set; }
    public double GrossAmount { get; set; }
}

public class OrderCustomerLineItemTarget
{
    public string skcode { get; set; }
    public int customerid { get; set; }
    public string itemnumber { get; set; }
    public double totalprice { get; set; }
    public int storeid { get; set; }
}

public class AllCustomerTargetDc
{
    public string Skcode { get; set; }
    public double Target { get; set; }
    public bool? IsOffer { get; set; }
    public int? OfferType { get; set; } //0-Percent 1=Values
    public int? OfferValue { get; set; }
    public int? OfferId { get; set; }
    public string OfferDesc { get; set; }
    public int? MaxDiscount { get; set; }
    public int? MOVMultiplier { get; set; }
    public int? TargetLineItem { get; set; }
    public double? LineItemMinAmount { get; set; }
    public List<storetarget> storetargets { get; set; }

}

public class storetarget
{
    public string skcode { get; set; }
    public string storename { get; set; }
    public double targetInAmount { get; set; }
    public int targetInLineItem { get; set; }
}
public class LevelDc
{
    public int Id { get; set; }
    public string LevelName { get; set; }
    public int Volume { get; set; }
    public int OrderCount { get; set; }
    public int BrandCount { get; set; }
    public int KKVolume { get; set; }
    public bool Selected { get; set; }
}
public class levelupdate
{
    public int LevelId { get; set; }
    public string LevelName { get; set; }
    public string LevelColor { get; set; }
}
public class mldata
{
    public string SkCode { get; set; }
    public int Cityid { get; set; }
    public string WarehouseName { get; set; }
    public int WarehouseId { get; set; }
    public string levels { get; set; }
    public double Volume { get; set; }
    public double Target { get; set; }
}
public class GetRewardDetails
{
    public int CustomerId { get; set; }
    public int WarehouseId { get; set; }
    public int CompanyId { get; set; }
    public string SKCode { get; set; }
    public string RewardType { get; set; }
    public int value { get; set; }
    public int GiftId { get; set; }
}
public class TA
{
    public int WarehouseId { get; set; }
    public string SkCode { get; set; }
    public string levels { get; set; }
    public string status { get; set; }
    public double allocation { get; set; }
}
public class CustomerTarget
{
    public string TargetMonth { get; set; }
    public int CompanyId { get; set; }
    public long StoreId { get; set; }
    public long TargetDetailId { get; set; }
    public string StoreName { get; set; }
    public string BrandNames { get; set; }
    public string StoreUrl { get; set; }
    public string SKCode { get; set; }
    public string GiftImage { get; set; }
    public string GiftItemName { get; set; }
    public string OfferDesc { get; set; }
    public string Type { get; set; }
    public string Level { get; set; }
    public decimal Value { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal TotalPurchaseAmount { get; set; }
    public decimal TotalPendingPurchaseAmount { get; set; }
    public int? TargetLineItem { get; set; }
    public int? CurrentLineItem { get; set; }
    public decimal AchivePercent { get; set; }
    public int LeftDays { get; set; }
    public bool IsClaimed { get; set; }
    public int OfferValue { get; set; }
    public int? OfferType { get; set; }
    public int? MaxDiscount { get; set; }
    public int? MOVMultiplier { get; set; }
    public List<targetCondition> targetConditions { get; set; }
}

public class CustomerTargetDashboardDetail
{
    public int TotalCount { get; set; }
    public List<CustomerTargetDashboard> CustomerTargetDashboard { get; set; }
}
public class CustomerTargetDashboard
{
    public string TargetMonth { get; set; }
    public string SKCode { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal TotalPurchaseAmount { get; set; }
    public decimal TotalPendingPurchaseAmount { get; set; }
    public int? TargetLineItem { get; set; }
    public int? CurrentLineItem { get; set; }
    public decimal AchivePercent { get; set; }
    public int LeftDays { get; set; }
    public string IsClaimed { get; set; }
    public int OfferValue { get; set; }
    public int? MaxDiscount { get; set; }
    public int? MOVMultiplier { get; set; }
    public string RewardType { get; set; }
    public int? RewardValue { get; set; }
    public string RewardDescription { get; set; }

}

public class ExportCustomerTarget
{
    public string TargetMonth { get; set; }
    public string SKCode { get; set; }
    public string StoreNames { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal TotalPurchaseAmount { get; set; }
    public decimal TotalPendingPurchaseAmount { get; set; }
    public int? TargetLineItem { get; set; }
    public int? CurrentLineItem { get; set; }
    public decimal AchivePercent { get; set; }
    public int LeftDays { get; set; }
    public string IsClaimed { get; set; }
    public int? MaxDiscount { get; set; }
    public int? MOVMultiplier { get; set; }
    public string RewardType { get; set; }
    public int? RewardValue { get; set; }
    public string RewardDescription { get; set; }

}

public class targetCondition
{
    public string ConditionText { get; set; }
    public string ConditionCompleteText { get; set; }
    public int Target { get; set; }
    public int CurrentValue { get; set; }

    public decimal AchivePercent { get; set; }
    public string Message { get; set; }
}
public class Dashboard
{
    public int Id { get; set; }
    public string Type { get; set; }
    public int Value { get; set; }
    public int WarehouseId { get; set; }
}
public class level
{
    public int Id { get; set; }
    public string LevelName { get; set; }
    public string Type { get; set; }
    public decimal Value { get; set; }
    public bool IsSelected { get; set; }
}
public class MonthlyTargetData
{
    public string LevelName { get; set; }
    public string Type { get; set; }
    public decimal Value { get; set; }
    public int Id { get; set; }
    public string BandName { get; set; }
    public decimal UpperLimit { get; set; }
    public decimal LowerLimit { get; set; }
    public DateTime CreatedDate { get; set; }
    //public List<Bands> LevelBands { get; set; }
}
public class GetLevelBandsDTO
{
    public string LevelName { get; set; }
    public string BandName { get; set; }
    public decimal BandLowerLimit { get; set; }
    public decimal BandUpperLimit { get; set; }
    public int ByValue { get; set; }
    public string Types { get; set; }
    public int NoOfCustomer { get; set; }
    public string Reward { get; set; }
    public DateTime Createdate { get; set; }
    public string GiftName { get; set; }
    public string GiftDescription { get; set; }

}
public class Bands
{
    public int Id { get; set; }
    public int LevelId { get; set; }
    public string BandName { get; set; }
    public decimal UpperLimit { get; set; }
    public decimal LowerLimit { get; set; }
    public string ImagePath { get; set; }
    public int? GiftId { get; set; }
    public string BandType { get; set; }
    public int Value { get; set; }
}
public class MLBudgetDTO
{
    public int BrandCount { get; set; }
    public int Cityid { get; set; }
    public bool IsActive { get; set; }
    public int OrderCount { get; set; }
    public int Salespersonordercount { get; set; }
    public int Selfordercount { get; set; }
    public string SkCode { get; set; }
    public double Volume { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; }
    public double kkVolumn { get; set; }
    public string levels { get; set; }
    public string status { get; set; }
    public string allocation { get; set; }
}

public class CustomerTargetDc
{
    public int CustomerId { get; set; }
    public string SkCode { get; set; }
    public string MobileNo { get; set; }
    public int WarehouseId { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal AchieveAmount { get; set; }
    public decimal RemainingAmount { get; set; }
}

public class BeatCustomerTargetDc
{
    public List<CustomerTargetDc> CustomerTargetDcs { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal AchieveAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public int TargetCount { get; set; }
    public int AchieveCount { get; set; }
    public int RemainingCount { get; set; }
}

public class BeatCustomerTargetDCs
{
    public BeatCustomerTargetDc beatCustomerTargets { get; set; }
    public List<StoreList> StoreList { get; set; }
}

public class CustomerOrderDetailDc
{

    public int Categoryid { get; set; }
    public int SubcategoryId { get; set; }
    public int SubsubcategoryId { get; set; }
    public DateTime CreatedDate { get; set; }
    public double TotalAmt { get; set; }
    public int qty { get; set; }
    public int ItemId { get; set; }
    public string itemNumber { get; set; }
    public int CustomerId { get; set; }
    public int CityId { get; set; }

}


public class SubCategoryTargetCustomerDc
{
    public long id { get; set; }
    public int CustomerId { get; set; }
    public string ShopName { get; set; }
    public string Skcode { get; set; }
    public decimal CurrentMonthSales { get; set; }
    public decimal Target { get; set; }
    public bool IsClaimed { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsTargetExpire { get; set; }
    public string valueType { get; set; }
    public int WalletValue { get; set; }
    public int SlabLowerLimit { get; set; }
    public int SlabUpperLimit { get; set; }
    public int SubCatId { get; set; }
    public int NoOfLineItem { get; set; }
    public int RequiredNoOfLineItem { get; set; }
    public string CompanyName { get; set; }
    public string CompanyLogoUrl { get; set; }
    public string BrandNames { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public long TargetDetailId { get; set; }
    public List<TargetCustomerBrandDc> TargetCustomerBrandDcs { get; set; }
    public List<TargetCustomerItemDc> TargetCustomerItemDcs { get; set; }
    public List<GiftItemDc> GiftItemDcs { get; set; }

}

public class GiftItemDc
{
    public long id { get; set; }
    public string ItemLogo { get; set; }
    public int itemid { get; set; }
    public string ItemName { get; set; }
    public int Qty { get; set; } //Dream Item         
}

public class TargetCustomerBrandDc
{
    public long id { get; set; }
    public string BrandName { get; set; }
    public decimal Target { get; set; }
    public decimal currentTarget { get; set; }
}

public class TargetCustomerItemDc
{
    public long id { get; set; }
    public string ItemName { get; set; }
    public decimal Target { get; set; }
    public decimal currentTarget { get; set; }
}


public class tblSubCategoryTargetCustomer
{
    [Key]
    public long Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public int CreatedBy { get; set; }
    public int? ModifiedBy { get; set; }
    public int CustomerId { get; set; }
    public decimal PreviousMonthSales { get; set; }
    public decimal CurrentMonthSales { get; set; }
    public decimal Target { get; set; }
    public bool IsClaimed { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsTargetExpire { get; set; }
    public int NoOfLineItem { get; set; }
    public int RequiredNoOfLineItem { get; set; }
    public long SubCatTargetBrandId { get; set; }
    public long SubCatTargetDetailId { get; set; }
    public string GuildId { get; set; }
}

public class tblTargetCustomerBrand
{
    [Key]
    public long Id { get; set; }
    public int BrandId { get; set; }
    public decimal Target { get; set; }
    public decimal currentTarget { get; set; }
    public long SubCategoryTargetCustomerId { get; set; }
    public string GuildId { get; set; }
}

public class tblTargetCustomerItem
{
    [Key]
    public long Id { get; set; }
    public string SellingSku { get; set; }
    public decimal Target { get; set; }
    public decimal currentTarget { get; set; }
    public long SubCategoryTargetCustomerId { get; set; }
    public string GuildId { get; set; }

}
public class SkcodeTargetCustomerListDc
{
    public string Skcode { get; set; }
    public int rItemId { get; set; }
}
public class GetCustomerTargetDC
{
    public int CustomerId { get; set; }
    public string Skcode { get; set; }
    public string fcmId { get; set; }
    public string ShopName { get; set; }
    public string Name { get; set; }
    public bool Active { get; set; }
    public bool Deleted { get; set; }
}

public class CustomerTargetDashboardFilter
{
    public int Month { get; set; }
    public int Year { get; set; }
    public List<int> CityId { get; set; }
    public List<int> WarehouseId { get; set; }
    public List<string> SkCode { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 50;
}
public class AllSkcodeTargetCustomerListDC
{
    public int CustomerId { get; set; }
    public string Skcode { get; set; }
    public int Cityid { get; set; }
    public int Warehouseid { get; set; }
}