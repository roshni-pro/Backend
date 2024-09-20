using AngularJSAuthentication.API.ControllerV7.VehicleMaster;
using AngularJSAuthentication.Model.FleetMaster;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Transactions;
using System.Web.Http;
using AngularJSAuthentication.Model;
using AgileObjects.AgileMapper;
using LinqKit;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using NPOI.SS.UserModel;
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using AngularJSAuthentication.API.App_Code.PackingMaterial;
using System.Security.Claims;
using AngularJSAuthentication.Model.DeliveryOptimization.FleetMaster;

namespace AngularJSAuthentication.API.Helper.VehicleMasterHelper
{
    public class FleetMasterHelper
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        #region Fleet Master
        public bool AddnewFleetMaster(FleetMasterDC AddFleetMasterdc, int userid)
        {
            bool result = false;

            if (AddFleetMasterdc != null && AddFleetMasterdc.fleetMasterDetails.Any())
            {
                using (var context = new AuthContext())
                {
                    List<FleetMasterDetail> Addlist = new List<FleetMasterDetail>();
                    FleetMaster fleetMaster = new FleetMaster();
                    var Obj = context.FleetMasterDB.Where(x => x.Id == AddFleetMasterdc.Id && x.IsDeleted == false).Include(x => x.fleetMasterDetails).FirstOrDefault();
                    if (Obj == null)
                    {
                        fleetMaster.FleetType = AddFleetMasterdc.FleetType;
                        fleetMaster.Channel = AddFleetMasterdc.Channel;
                        fleetMaster.OperatedBy = AddFleetMasterdc.OperatedBy;
                        fleetMaster.FreightDiscount = AddFleetMasterdc.FreightDiscount;
                        fleetMaster.TollAmt = AddFleetMasterdc.TollAmt;
                        fleetMaster.ContractStart = AddFleetMasterdc.ContractStart;
                        fleetMaster.ContractEnd = AddFleetMasterdc.ContractEnd;
                        fleetMaster.TransportName = AddFleetMasterdc.TransportName;
                        fleetMaster.TransportAgentName = AddFleetMasterdc.TransportAgentName;
                        fleetMaster.IsActive = true;
                        fleetMaster.IsDeleted = false;
                        fleetMaster.IsBlocked = false;
                        fleetMaster.CreatedDate = DateTime.Now;
                        fleetMaster.ModifiedDate = DateTime.Now;
                        fleetMaster.ModifiedBy = userid;
                        fleetMaster.CreatedBy = userid;
                        fleetMaster.CityId = AddFleetMasterdc.CityId;
                        fleetMaster.TransportAgentMobileNo = AddFleetMasterdc.TransportAgentMobileNo;
                        fleetMaster.AadharImagePath = AddFleetMasterdc.AadharImagePath;
                        fleetMaster.AadharNo = AddFleetMasterdc.AadharNo;
                        fleetMaster.PanImagePath = AddFleetMasterdc.PanImagePath;
                        fleetMaster.PanNo = AddFleetMasterdc.PanNo;
                        fleetMaster.Address = AddFleetMasterdc.Address;
                        fleetMaster.GSTIN = AddFleetMasterdc.GSTIN;
                        fleetMaster.TransporterStateId = AddFleetMasterdc.TransporterStateId;
                        fleetMaster.TransporterCityId = AddFleetMasterdc.TransporterCityId;
                        fleetMaster.fleetMasterDetails = new List<FleetMasterDetail>();
                        fleetMaster.IsMSME = AddFleetMasterdc.IsMSME;
                        fleetMaster.MSMECertificatePath = AddFleetMasterdc.MSMECertificatePath;
                        fleetMaster.AgreementPath = AddFleetMasterdc.AgreementPath;
                        foreach (var list in AddFleetMasterdc.fleetMasterDetails)
                        {
                            FleetMasterDetail obj = new FleetMasterDetail()
                            {
                                VehicleType = list.VehicleType,
                                NoOfVehicle = list.NoOfVehicle,
                                FixedCost = list.FixedCost,
                                ExtraKmCharge = list.ExtraKmCharge,
                                ExtraHrCharge = list.ExtraHrCharge,
                                WaitingCharge = list.WaitingCharge,
                                VehicleCapacity = list.VehicleCapacity,
                                Make = list.Make,
                                NonworkingDayAmt = list.NonworkingDayAmt,
                                DistanceContract = list.DistanceContract,
                                DaysContract = list.DaysContract,
                                HrContract = list.HrContract,
                                IsActive = true,
                                IsDeleted = false,
                                CreatedDate = DateTime.Now,
                                CreatedBy = userid
                            };
                            fleetMaster.fleetMasterDetails.Add(obj);
                        }
                        context.FleetMasterDB.Add(fleetMaster);
                    }
                    else
                    {
                        result = false;
                    }
                    if (context.Commit() > 0)
                    {
                        if (AddFleetMasterdc.fleetMasterAccountDetailDc != null && AddFleetMasterdc.fleetMasterAccountDetailDc.Any())
                        {
                            foreach (var item in AddFleetMasterdc.fleetMasterAccountDetailDc)
                            {
                                FleetMasterAccountDetail fleetMasterAccountDetail = new FleetMasterAccountDetail
                                {
                                    AccountNo = item.AccountNo,
                                    BankName = item.BankName,
                                    BranchName = item.BranchName,
                                    CreatedBy = userid,         
                                    CreatedDate = DateTime.Now,
                                    FleetMasterId = fleetMaster.Id,
                                    HolderName = item.HolderName,
                                    IFSCcode = item.IFSCcode,
                                    IsActive = true,
                                    IsDeleted = false,
                                    CancelledChequePath = item.CancelledChequePath,
                                };
                                context.fleetMasterAccountDetailDB.Add(fleetMasterAccountDetail);
                                context.Commit();
                            }
                        }
                        result = true;
                    }
                   
                }
            }
            return result;
        }

        public bool UpdateFleetMaster(FleetMasterDC EditFleetMasterdc, int userid)
        {
            bool result = false;
            if (EditFleetMasterdc != null)
            {
                using (var context = new AuthContext())
                {
                    #region update Fleet Master
                    var EditFleetMaster = context.FleetMasterDB.Where(x => x.Id == EditFleetMasterdc.Id && x.IsDeleted == false).Include(x => x.fleetMasterDetails).FirstOrDefault();
                    if (EditFleetMaster != null)
                    {
                        EditFleetMaster.FleetType = EditFleetMasterdc.FleetType;
                        EditFleetMaster.Channel = EditFleetMasterdc.Channel;
                        EditFleetMaster.OperatedBy = EditFleetMasterdc.OperatedBy;
                        EditFleetMaster.FreightDiscount = EditFleetMasterdc.FreightDiscount;
                        EditFleetMaster.TollAmt = EditFleetMasterdc.TollAmt;
                        EditFleetMaster.ContractStart = EditFleetMasterdc.ContractStart;
                        EditFleetMaster.ContractEnd = EditFleetMasterdc.ContractEnd;
                        EditFleetMaster.TransportName = EditFleetMasterdc.TransportName;
                        EditFleetMaster.TransportAgentName = EditFleetMasterdc.TransportAgentName;
                        EditFleetMaster.IsActive = EditFleetMasterdc.IsActive;
                        EditFleetMaster.IsDeleted = false;
                        EditFleetMaster.ModifiedDate = DateTime.Now;
                        EditFleetMaster.ModifiedBy = userid;
                        EditFleetMaster.IsBlocked = EditFleetMasterdc.IsBlocked;
                        EditFleetMaster.CityId = EditFleetMasterdc.CityId;
                        EditFleetMaster.TransportAgentMobileNo = EditFleetMasterdc.TransportAgentMobileNo;
                        if (EditFleetMaster.IsAppprovedByAccount == false)
                        {
                            EditFleetMaster.AadharImagePath = EditFleetMasterdc.AadharImagePath;
                            EditFleetMaster.AadharNo = EditFleetMasterdc.AadharNo;
                            EditFleetMaster.PanImagePath = EditFleetMasterdc.PanImagePath;
                            EditFleetMaster.PanNo = EditFleetMasterdc.PanNo;
                            EditFleetMaster.Address = EditFleetMasterdc.Address;
                            EditFleetMaster.GSTIN = EditFleetMasterdc.GSTIN;
                            EditFleetMaster.TransporterCityId = EditFleetMasterdc.TransporterCityId;
                            EditFleetMaster.TransporterStateId = EditFleetMasterdc.TransporterStateId;
                            EditFleetMaster.IsMSME = EditFleetMasterdc.IsMSME;
                            EditFleetMaster.MSMECertificatePath = EditFleetMasterdc.MSMECertificatePath;
                            EditFleetMaster.AgreementPath = EditFleetMasterdc.AgreementPath;
                        }         
                        #endregion

                        #region Upadte fleet Master Details
                        if (EditFleetMasterdc.fleetMasterDetails != null && EditFleetMasterdc.fleetMasterDetails.Any())
                        {
                            List<int> newid = EditFleetMasterdc.fleetMasterDetails.Select(x => x.Id).Distinct().ToList();
                            foreach (var list in EditFleetMasterdc.fleetMasterDetails)
                            {
                                var editdetails = EditFleetMaster.fleetMasterDetails.Where(x => x.Id == list.Id).FirstOrDefault();
                                if (editdetails == null)
                                {
                                    FleetMasterDetail objs = new FleetMasterDetail()
                                    {
                                        VehicleType = list.VehicleType,
                                        NoOfVehicle = list.NoOfVehicle,
                                        FixedCost = list.FixedCost,
                                        ExtraKmCharge = list.ExtraKmCharge,
                                        ExtraHrCharge = list.ExtraHrCharge,
                                        WaitingCharge = list.WaitingCharge,
                                        VehicleCapacity = list.VehicleCapacity,
                                        Make = list.Make,
                                        NonworkingDayAmt = list.NonworkingDayAmt,
                                        DistanceContract = list.DistanceContract,
                                        DaysContract = list.DaysContract,
                                        HrContract = list.HrContract,
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreatedDate = DateTime.Now,
                                        CreatedBy = userid
                                    };
                                    EditFleetMaster.fleetMasterDetails.Add(objs);
                                }
                                else
                                {
                                    editdetails.VehicleType = list.VehicleType;
                                    editdetails.NoOfVehicle = list.NoOfVehicle;
                                    editdetails.FixedCost = list.FixedCost;
                                    editdetails.ExtraKmCharge = list.ExtraKmCharge;
                                    editdetails.ExtraHrCharge = list.ExtraHrCharge;
                                    editdetails.WaitingCharge = list.WaitingCharge;
                                    editdetails.VehicleCapacity = list.VehicleCapacity;
                                    editdetails.Make = list.Make;
                                    editdetails.NonworkingDayAmt = list.NonworkingDayAmt;
                                    editdetails.DistanceContract = list.DistanceContract;
                                    editdetails.DaysContract = list.DaysContract;
                                    editdetails.HrContract = list.HrContract;
                                    editdetails.IsActive = true;
                                    editdetails.IsDeleted = false;
                                    editdetails.ModifiedDate = DateTime.Now;
                                    editdetails.ModifiedBy = userid;
                                    context.Entry(editdetails).State = EntityState.Modified;
                                }
                            }
                            foreach (var obj in EditFleetMaster.fleetMasterDetails.Where(x => !newid.Contains(Convert.ToInt32(x.Id))).Select(z => z.Id).ToList())
                            {
                                var list = EditFleetMaster.fleetMasterDetails.Where(x => x.Id == obj).FirstOrDefault();
                                list.ModifiedDate = DateTime.Now;
                                list.ModifiedBy = userid;
                                list.IsActive = false;
                                list.IsDeleted = true;
                                context.Entry(list).State = EntityState.Modified;
                            }
                            context.Entry(EditFleetMaster).State = EntityState.Modified;
                        }
                        #endregion

                        #region Update FleetMaster AccountDeatil

                        if (EditFleetMasterdc.fleetMasterAccountDetailDc != null && EditFleetMasterdc.fleetMasterAccountDetailDc.Any() && EditFleetMaster.IsAppprovedByAccount==false)
                        {
                            var EditFleetAccountDetail = context.fleetMasterAccountDetailDB.Where(x => x.FleetMasterId == EditFleetMasterdc.Id && x.IsActive == true && x.IsDeleted == false).ToList();
                            foreach (var item in EditFleetMasterdc.fleetMasterAccountDetailDc)
                            {
                                if (item.Id > 0)
                                {
                                    var updateAccountdetail = EditFleetAccountDetail.Where(x => x.Id == item.Id).FirstOrDefault();
                                    if (updateAccountdetail != null)
                                    {
                                        updateAccountdetail.IFSCcode = item.IFSCcode;
                                        updateAccountdetail.HolderName = item.HolderName;
                                        updateAccountdetail.BankName = item.BankName;
                                        updateAccountdetail.BranchName = item.BranchName;
                                        updateAccountdetail.AccountNo = item.AccountNo;
                                        updateAccountdetail.CancelledChequePath = item.CancelledChequePath;
                                        context.Entry(updateAccountdetail).State = EntityState.Modified;
                                    }
                                }
                                else
                                {
                                    FleetMasterAccountDetail fleetMasterAccountDetail = new FleetMasterAccountDetail
                                    {
                                        AccountNo = item.AccountNo,
                                        BankName = item.BankName,
                                        BranchName = item.BranchName,
                                        CreatedBy = userid,
                                        CreatedDate = DateTime.Now,
                                        FleetMasterId = item.FleetMasterId,
                                        HolderName = item.HolderName,
                                        IFSCcode = item.IFSCcode,
                                        IsActive = true,
                                        IsDeleted = false,
                                        CancelledChequePath = item.CancelledChequePath
                                    };
                                    context.fleetMasterAccountDetailDB.Add(fleetMasterAccountDetail);
                                }
                            }
                        }
                        #endregion

                        if (context.Commit() > 0)
                        {
                            result = true;
                        }
                    }                  
                }

            }
            if (result)
            {
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }

        public FleetMasterPaggingDC getAllListFleetMaster(int skip, int take)
        {
            using (var db = new AuthContext())
            {
                int Skiplist = (skip - 1) * take;
                FleetMasterPaggingDC res = new FleetMasterPaggingDC();
                List<FleetMasterDC> list = new List<FleetMasterDC>();
                var data = db.FleetMasterDB.Where(x => x.IsDeleted == false).OrderByDescending(x => x.Id).Skip(Skiplist).Take(take).ToList();
                list = Mapper.Map(data).ToANew<List<FleetMasterDC>>();
                res.totalcount = db.FleetMasterDB.Count();
                var warehosuesids = list.Select(x => x.WarehouseId).Distinct().ToList();
                var cityids = list.Select(x => x.CityId).Distinct().ToList();
                var warehouses = db.Warehouses.Where(x => warehosuesids.Contains(x.WarehouseId)).ToList();
                var citylist = db.Cities.Where(x => cityids.Contains(x.Cityid)).ToList();
                list.ForEach(y =>
                {
                    y.CityName = citylist.Where(x => x.Cityid == y.CityId).Select(x => x.CityName).FirstOrDefault();
                    y.WarehouseName = warehouses.Where(x => x.WarehouseId == y.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                });
                res.FleetMasterList = list;
                return res;
            }
        }
        public AllDatas getAllListFleetMaster(DateTime? startDate, DateTime? endDate, string search, int statusValue, int Cityid, int skip, int take, int userid)
        {
            using (var db = new AuthContext())
            {
                var cid = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).Select(x => x.Cityid).FirstOrDefault();
                int Skiplist = (skip - 1) * take;
                if (search == "undefined")
                {
                    search = null;
                }
                var predicate = PredicateBuilder.True<FleetMaster>();
                AllDatas Data = new AllDatas();
                List<FleetMasterDC> list = new List<FleetMasterDC>();
                List<FleetMaster> data = new List<FleetMaster>();
                //if (cid > 0)
                //{
                //    predicate = predicate.And(x => x.CityId == cid && x.IsDeleted == false);
                //}
                if (Cityid > 0)
                {
                    predicate = predicate.And(x => x.CityId == Cityid && x.IsDeleted == false);
                }
                if (statusValue == 1)
                {
                    predicate = predicate.And(x => x.IsActive == true);
                }
                if (statusValue == 2)
                {
                    predicate = predicate.And(x => x.IsActive == false);
                }
                if (statusValue == 3)
                {
                    predicate = predicate.And(x => x.IsBlocked == true);
                }
                if (!string.IsNullOrEmpty(search))
                {
                    predicate = predicate.And(x => x.IsDeleted == false && x.TransportName.Contains(search) || x.TransportAgentName.Contains(search) || x.FleetType.Contains(search));

                }
                if (startDate != null && endDate != null)
                {

                    predicate = predicate.And(x => x.IsDeleted == false && x.CreatedDate >= startDate && x.CreatedDate <= endDate);
                }
                if ((Cityid == 0 || Cityid == null) && (search == null || search == "") && (statusValue == 0 || statusValue == null))
                {
                    data = db.FleetMasterDB.Where(x => x.IsDeleted == false && x.CityId == cid).OrderByDescending(x => x.Id).Skip(Skiplist).Take(take).ToList();
                }
                else
                {
                    data = db.FleetMasterDB.Where(predicate).OrderByDescending(x => x.Id).Skip(Skiplist).Take(take).ToList();
                }

                list = Mapper.Map(data).ToANew<List<FleetMasterDC>>();
                var cityids = list.Select(x => x.CityId).Distinct().ToList();
                var warehosuesids = list.Select(x => x.WarehouseId).Distinct().ToList();
                var citylist = db.Cities.Where(x => cityids.Contains(x.Cityid)).ToList();
                var warehouses = db.Warehouses.Where(x => warehosuesids.Contains(x.WarehouseId)).ToList();
                list.ForEach(y =>
                {
                    y.CityName = citylist.Where(x => x.Cityid == y.CityId).Select(x => x.CityName).FirstOrDefault();
                    y.WarehouseName = warehouses.Where(x => x.WarehouseId == y.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                });


                if ((Cityid == 0 || Cityid == null) && (search == null || search == "") && (statusValue == 0 || statusValue == null))
                {
                    Data.FleetMasterDCList = list.OrderByDescending(x => x.Id).ToList();
                    Data.totalcount = db.FleetMasterDB.Where(x => x.IsDeleted == false && x.CityId == cid).Count();
                }
                else
                {
                    Data.FleetMasterDCList = list.OrderByDescending(x => x.Id).ToList();
                    Data.totalcount = db.FleetMasterDB.Where(predicate).Count();
                }



                return Data;
            }

        }
        public FleetMasterDC FleetMasterbyId(long Id)
        {
            using (var db = new AuthContext())
            {
                FleetMasterDC list = new FleetMasterDC();
                var data = db.FleetMasterDB.Where(x => x.Id == Id && x.IsDeleted == false).Include(x => x.fleetMasterDetails).FirstOrDefault();
                var datalist = data.fleetMasterDetails.Where(x => x.IsDeleted == false).ToList();
                data.fleetMasterDetails = datalist;
                var AccountDetail = db.fleetMasterAccountDetailDB.Where(x => x.FleetMasterId == Id && x.IsDeleted == false).ToList();
                list= Mapper.Map(data).ToANew<FleetMasterDC>();
                if (AccountDetail != null && AccountDetail.Any())
                {
                    var res = Mapper.Map(AccountDetail).ToANew<List<FleetMasterAccountDetailDC>>();
                    list.fleetMasterAccountDetailDc = Mapper.Map(AccountDetail).ToANew<List<FleetMasterAccountDetailDC>>();
                }
               // list.fleetMasterDetails = Mapper.Map(data.fleetMasterDetails).ToANew<List<fleetMasterDetailDC>>();
                return list;
            }
        }
        #endregion
        #region Vehicle Master
        public bool AddnewVehicleMaster(VehicleMasterDC AddVehicleMaster, int userid)
        {
            bool result = false;
            if (AddVehicleMaster != null)
            {
                using (var context = new AuthContext())
                {
                    VehicleMaster addVehicle = new VehicleMaster()
                    {
                        FleetType = AddVehicleMaster.FleetType,
                        VehicleType = AddVehicleMaster.VehicleType,
                        VehicleNo = AddVehicleMaster.VehicleNo,
                        Model = AddVehicleMaster.Model,
                        RegistrationNo = AddVehicleMaster.RegistrationNo,
                        OwnerName = AddVehicleMaster.OwnerName,
                        ChasisNo = AddVehicleMaster.ChasisNo,
                        OwnershipType = AddVehicleMaster.OwnershipType,
                        InsuranceNo = AddVehicleMaster.InsuranceNo,
                        PUCNo = AddVehicleMaster.PUCNo,
                        EngineNo = AddVehicleMaster.EngineNo,
                        Make = AddVehicleMaster.Make,
                        VehicleWeight = AddVehicleMaster.VehicleWeight,
                        // IsBlocked = AddVehicleMaster.IsBlocked,
                        PUCValidTill = AddVehicleMaster.PUCValidTill,
                        PUCimage = AddVehicleMaster.PUCimage,
                        InsuranceImage = AddVehicleMaster.InsuranceImage,
                        RegistrationImage = AddVehicleMaster.RegistrationImage,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        CityId = AddVehicleMaster.CityId,
                        InsuranceValidity = AddVehicleMaster.InsuranceValidity,
                        MakerName = AddVehicleMaster.MakerName,
                        FleetDetailId = AddVehicleMaster.FleetId,
                        WarehouseId = AddVehicleMaster.WarehouseId,
                        RegistrationImageBack = AddVehicleMaster.RegistrationImageBack
                        //TripTypeEnum= AddVehicleMaster.TripTypeEnum
                    };
                    context.VehicleMasterDB.Add(addVehicle);
                    if (context.Commit() > 0)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }
        public bool EditVehicleMasters(VehicleMasterDC EditVehicleMaster, int userid)
        {
            bool result = false;
            if (EditVehicleMaster != null)
            {
                using (var context = new AuthContext())
                {
                    var EditVehicle = context.VehicleMasterDB.Where(x => x.Id == EditVehicleMaster.Id && x.IsDeleted == false).FirstOrDefault();

                    if(EditVehicleMaster != null && EditVehicle.FleetDetailId != EditVehicleMaster.FleetId)
                    {
                        EditVehicle.IsActive = false;
                        context.Entry(EditVehicle).State = EntityState.Modified;

                        VehicleMaster NewVehicle = new VehicleMaster 
                        {
                            FleetType = EditVehicleMaster.FleetType,
                            VehicleType = EditVehicleMaster.VehicleType,
                            VehicleNo = EditVehicleMaster.VehicleNo,
                            Model = EditVehicleMaster.Model,
                            RegistrationNo = EditVehicleMaster.RegistrationNo,
                            OwnerName = EditVehicleMaster.OwnerName,
                            ChasisNo = EditVehicleMaster.ChasisNo,
                            OwnershipType = EditVehicleMaster.OwnershipType,
                            InsuranceNo = EditVehicleMaster.InsuranceNo,
                            PUCNo = EditVehicleMaster.PUCNo,
                            EngineNo = EditVehicleMaster.EngineNo,
                            Make = EditVehicleMaster.Make,
                            VehicleWeight = EditVehicleMaster.VehicleWeight,
                            PUCValidTill = EditVehicleMaster.PUCValidTill,
                            PUCimage = EditVehicleMaster.PUCimage,
                            InsuranceImage = EditVehicleMaster.InsuranceImage,
                            RegistrationImage = EditVehicleMaster.RegistrationImage,
                            CreatedBy = userid,
                            CreatedDate = DateTime.Now,
                            IsDeleted = false,
                            CityId = EditVehicleMaster.CityId,
                            InsuranceValidity = EditVehicleMaster.InsuranceValidity,
                            MakerName = EditVehicleMaster.MakerName,
                            WarehouseId = EditVehicleMaster.WarehouseId,
                            FleetDetailId = EditVehicleMaster.FleetId,
                            RegistrationImageBack = EditVehicleMaster.RegistrationImageBack,
                            IsActive  = true,
                            IsAutoAddedVehicle = false,
                            IsBlocked = false
                        };
                        context.VehicleMasterDB.Add(NewVehicle);
                        if (context.Commit() > 0)
                        {
                            result = true;
                        }
                    }
                    else if (EditVehicleMaster != null)
                    {
                        EditVehicle.FleetType = EditVehicleMaster.FleetType;
                        EditVehicle.VehicleType = EditVehicleMaster.VehicleType;
                        EditVehicle.VehicleNo = EditVehicleMaster.VehicleNo;
                        EditVehicle.Model = EditVehicleMaster.Model;
                        EditVehicle.RegistrationNo = EditVehicleMaster.RegistrationNo;
                        EditVehicle.OwnerName = EditVehicleMaster.OwnerName;
                        EditVehicle.ChasisNo = EditVehicleMaster.ChasisNo;
                        EditVehicle.OwnershipType = EditVehicleMaster.OwnershipType;
                        EditVehicle.InsuranceNo = EditVehicleMaster.InsuranceNo;
                        EditVehicle.PUCNo = EditVehicleMaster.PUCNo;
                        EditVehicle.EngineNo = EditVehicleMaster.EngineNo;
                        EditVehicle.Make = EditVehicleMaster.Make;
                        EditVehicle.VehicleWeight = EditVehicleMaster.VehicleWeight;
                        // EditVehicle.IsBlocked = EditVehicleMaster.IsBlocked;
                        EditVehicle.PUCValidTill = EditVehicleMaster.PUCValidTill;
                        EditVehicle.PUCimage = EditVehicleMaster.PUCimage;
                        EditVehicle.InsuranceImage = EditVehicleMaster.InsuranceImage;
                        EditVehicle.RegistrationImage = EditVehicleMaster.RegistrationImage;
                        EditVehicle.ModifiedBy = userid;
                        EditVehicle.ModifiedDate = DateTime.Now;
                        EditVehicle.IsDeleted = false;
                        EditVehicle.CityId = EditVehicleMaster.CityId;
                        EditVehicle.InsuranceValidity = EditVehicleMaster.InsuranceValidity;
                        EditVehicle.MakerName = EditVehicleMaster.MakerName;
                        EditVehicle.WarehouseId = EditVehicleMaster.WarehouseId;
                        EditVehicle.FleetDetailId = EditVehicleMaster.FleetId;
                        EditVehicle.RegistrationImageBack = EditVehicleMaster.RegistrationImageBack;
                        //EditVehicle.TripTypeEnum = EditVehicleMaster.TripTypeEnum;
                    };
                    context.Entry(EditVehicle).State = EntityState.Modified;
                    if (context.Commit() > 0)
                    {
                        result = true;
                    }
                }
            }
            if (result)
            {
                result = true;
            }
            else
            {
                result = true;
            }
            return result;
        }
        public VehiclePaggingDC getAllListvehicleMaster(int skip, int take)
        {
            using (var db = new AuthContext())
            {
                FleetMaster fleetMasters = null;
                VehiclePaggingDC res = new VehiclePaggingDC();
                List<VehicleMasterDC> list = new List<VehicleMasterDC>();
                var data = db.VehicleMasterDB.Where(x => x.IsDeleted == false).OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
                list = Mapper.Map(data).ToANew<List<VehicleMasterDC>>();
                res.totalCount = db.VehicleMasterDB.Count();
                var fleetdetailIds = data.Select(x => x.FleetDetailId).Distinct().ToList();
                var fleetMasterDetails = db.FleetMasterDetailDB.Where(x => fleetdetailIds.Contains((int)x.Id)).ToList();
                var FleetMasterIds = fleetMasterDetails.Select(x => x.FleetMasterId).Distinct().ToList();
                var fleetMaster = db.FleetMasterDB.Where(x => FleetMasterIds.Contains(x.Id)).ToList();
                var warehosuesids = list.Select(x => x.WarehouseId).Distinct().ToList();
                var cityids = list.Select(x => x.CityId).Distinct().ToList();
                var warehouses = db.Warehouses.Where(x => warehosuesids.Contains(x.WarehouseId)).ToList();
                var citylist = db.Cities.Where(x => cityids.Contains(x.Cityid)).ToList();
                list.ForEach(y =>
                {
                    if (y.FleetDetailId > 0)
                    {
                        var fleetDetails = fleetMasterDetails.Where(x => x.Id == y.FleetDetailId).FirstOrDefault();
                        fleetMasters = fleetMaster.Where(x => x.Id == fleetDetails.FleetMasterId).FirstOrDefault();
                    }
                    y.TransportName = fleetMasters != null ? fleetMasters.TransportName : "NA";
                    y.CityName = citylist.Where(x => x.Cityid == y.CityId).Select(x => x.CityName).FirstOrDefault();
                    y.WarehouseName = warehouses.Where(x => x.WarehouseId == y.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                    //if (y.TripTypeEnum == 0)
                    //{
                    //    y.TripTypeName = "City";
                    //}
                    //else if (y.TripTypeEnum == 1)
                    //{
                    //    y.TripTypeName = "SKP";
                    //}
                    //else if (y.TripTypeEnum == 2)
                    //{
                    //    y.TripTypeName = "KPP";
                    //}
                    //else if (y.TripTypeEnum == 3)
                    //{
                    //    y.TripTypeName = "Damage_Expiry";
                    //}
                });
                res.VehicleMasterlist = list;
                return res;
            }
        }
        public AllData exportListvehicleMaster(DateTime? startDate, DateTime? endDate, string search, int statusValue, int WarehouseId, int Cityid, int skip, int take, int userid)
        {
            using (var db = new AuthContext())
            {

                int Skiplist = (skip - 1) * take;
                if (search == "undefined")
                {
                    search = null;
                }
                var wid = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).Select(x => x.WarehouseId).FirstOrDefault();
                var predicate = PredicateBuilder.True<VehicleMaster>();
                FleetMaster fleetMasters = null;
                AllData lists = new AllData();
                List<VehicleMasterExportDC> list = new List<VehicleMasterExportDC>();
                List<VehicleMaster> data = new List<VehicleMaster>();
                //if (wid > 0)
                //{
                //    predicate = predicate.And(x => x.WarehouseId == wid);
                //}
                if (Cityid > 0)
                {
                    predicate = predicate.And(x => x.CityId == Cityid && x.IsDeleted == false);
                }
                if (WarehouseId > 0)
                {
                    predicate = predicate.And(x => x.WarehouseId == WarehouseId && x.IsDeleted == false);
                }
                if (statusValue == 1)
                {
                    predicate = predicate.And(x => x.IsActive == true);
                }
                if (statusValue == 2)
                {
                    predicate = predicate.And(x => x.IsActive == false);
                }
                if (statusValue == 3)
                {
                    predicate = predicate.And(x => x.IsBlocked == true);
                }
                if (!string.IsNullOrEmpty(search))
                {
                    predicate = predicate.And(x => x.IsDeleted == false && search.Contains(x.OwnerName) || search.Contains(x.VehicleType) || search.Contains(x.VehicleNo));
                }
                else if (startDate != null && endDate != null)
                {
                    predicate = predicate.And(x => x.IsDeleted == false && x.CreatedDate >= startDate && x.CreatedDate <= endDate);

                }
                if ((Cityid == 0 || Cityid == null) && (WarehouseId == 0 || WarehouseId == null) && (search == null || search == "") && (statusValue == 0 || statusValue == null))
                {
                    data = db.VehicleMasterDB.Where(x => x.IsDeleted == false && x.WarehouseId == wid).OrderByDescending(x => x.Id).Skip(Skiplist).Take(take).ToList();
                }
                else
                {
                    data = db.VehicleMasterDB.Where(predicate).OrderByDescending(x => x.Id).Skip(Skiplist).Take(take).ToList();
                }

                list = Mapper.Map(data).ToANew<List<VehicleMasterExportDC>>();
                var fleetdetailIds = data.Select(x => x.FleetDetailId).Distinct().ToList();
                var fleetMasterDetails = db.FleetMasterDetailDB.Where(x => fleetdetailIds.Contains((int)x.Id)).ToList();
                var FleetMasterIds = fleetMasterDetails.Select(x => x.FleetMasterId).Distinct().ToList();
                var fleetMaster = db.FleetMasterDB.Where(x => FleetMasterIds.Contains(x.Id)).ToList();
                var cityids = list.Select(x => x.CityId).Distinct().ToList();
                var citylist = db.Cities.Where(x => cityids.Contains(x.Cityid)).ToList();
                var warehosuesids = list.Select(x => x.WarehouseId).Distinct().ToList();
                var warehouses = db.Warehouses.Where(x => warehosuesids.Contains(x.WarehouseId)).ToList();
                list.ForEach(y =>
                {
                    if (y.FleetDetailId > 0)
                    {
                        var fleetDetails = fleetMasterDetails.Where(x => x.Id == y.FleetDetailId).FirstOrDefault();
                        fleetMasters = fleetMaster.Where(x => x.Id == fleetDetails.FleetMasterId).FirstOrDefault();
                    }
                    y.TransportName = fleetMasters != null ? fleetMasters.TransportName : "NA";
                    y.CityName = citylist.Where(x => x.Cityid == y.CityId).Select(x => x.CityName).FirstOrDefault();
                    y.WarehouseName = warehouses.Where(x => x.WarehouseId == y.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                    //if (y.TripTypeEnum == 0)
                    //{
                    //    y.TripTypeName = "City";
                    //}
                    //else if (y.TripTypeEnum == 1)

                    //{
                    //    y.TripTypeName = "SKP";
                    //}
                    //else if (y.TripTypeEnum == 2)
                    //{
                    //    y.TripTypeName = "KPP";
                    //}
                    //else if (y.TripTypeEnum == 3)
                    //{
                    //    y.TripTypeName = "Damage_Expiry";
                    //}
                });


                if ((Cityid == 0 || Cityid == null) && (WarehouseId == 0 || WarehouseId == null) && (search == null || search == "") && (statusValue == 0 || statusValue == null))
                {
                    lists.totalCount = db.VehicleMasterDB.Where(x => x.IsDeleted == false && x.WarehouseId == wid).Count();
                    lists.VehicleMasterExportDCList = list;
                }
                else
                {
                    lists.totalCount = db.VehicleMasterDB.Where(predicate).Count();
                    lists.VehicleMasterExportDCList = list;
                }


                return lists;
            }
        }
        public VehicleMasterDC VehicleMasterId(long Id)
        {
            using (var db = new AuthContext())
            {
                VehicleMasterDC list = new VehicleMasterDC();
                var data = db.VehicleMasterDB.Where(x => x.Id == Id && x.IsDeleted == false).FirstOrDefault();
                list = Mapper.Map(data).ToANew<VehicleMasterDC>();
                var fleetMasterDetails = db.FleetMasterDetailDB.Where(x => x.Id == list.FleetDetailId).FirstOrDefault();
                if (fleetMasterDetails != null)
                {
                    var fleetMaster = db.FleetMasterDB.Where(x => x.Id == fleetMasterDetails.FleetMasterId).FirstOrDefault();
                    list.TransportName = fleetMaster != null ? fleetMaster.TransportName : "NA";
                    list.FleetMasterId = fleetMaster != null ? fleetMaster.Id : 0;
                }
                return list;
            }
        }
        #endregion

        #region report method
        public async Task<VehicleReportViewModel> GetVehicleReport(SearchVehicleReportDC search)
        {
            VehicleReportViewModel vm = null;

            using (var context = new AuthContext())
            {
                if (search != null && search.WarehouseId > 0 && search.Id > 0)
                {
                    search.ToDate = new DateTime(search.FromDate.Year, search.FromDate.Month, 1).AddMonths(1).AddDays(-1);
                    List<Object> parameters = new List<object>();
                    parameters.Add(new SqlParameter("@WarehouseId", search.WarehouseId));
                    parameters.Add(new SqlParameter("@VehicleId", search.Id));
                    parameters.Add(new SqlParameter("@startDate ", new DateTime(search.FromDate.Year, search.FromDate.Month, 1)));
                    parameters.Add(new SqlParameter("@endDate", search.ToDate));
                    string sqlquery = "exec GetVehicleReport @WarehouseId, @VehicleId , @startDate  , @endDate";
                    List<VehicleReportDC> result = await context.Database.SqlQuery<VehicleReportDC>(sqlquery, parameters.ToArray()).ToListAsync();



                    List<Object> parameter = new List<object>();
                    parameter.Add(new SqlParameter("@VehicleId", search.Id));
                    parameter.Add(new SqlParameter("@WarehouseId", search.WarehouseId));
                    string sqlquerys = "exec GetVehicleReportDetail @VehicleId , @WarehouseId";
                    var vehicleReportDetail = await context.Database.SqlQuery<VehicleReportDetailDC>(sqlquerys, parameter.ToArray()).FirstOrDefaultAsync();

                    if (vehicleReportDetail != null && result != null && result.Any())
                    {
                        vehicleReportDetail.ExtraDay = result.Count() - vehicleReportDetail.ContractDays;
                        if (vehicleReportDetail.ExtraDay < 0)
                        {
                            vehicleReportDetail.ExtraDay = 0;
                        }

                        vehicleReportDetail.Extrahours = (result.Sum(x => x.ExtraTimeInHour).HasValue ? result.Sum(x => x.ExtraTimeInHour).Value : 0) - vehicleReportDetail.ContractHours;
                        if (vehicleReportDetail.Extrahours < 0)
                        {
                            vehicleReportDetail.Extrahours = 0;
                        }

                        vehicleReportDetail.ExtraKM = (result.Sum(x => x.TotalKm).HasValue ? result.Sum(x => x.TotalKm).Value : 0) - vehicleReportDetail.ContractKMs;
                        if (vehicleReportDetail.ExtraKM < 0)
                        {
                            vehicleReportDetail.ExtraKM = 0;
                        }

                        vehicleReportDetail.RemainingKM = vehicleReportDetail.ContractKMs - (result.Sum(x => x.TotalKm).HasValue ? result.Sum(x => x.TotalKm).Value : 0);
                        if (vehicleReportDetail.RemainingKM < 0)
                        {
                            vehicleReportDetail.RemainingKM = 0;
                        }

                        vehicleReportDetail.TotalKM = (result.Sum(x => x.TotalKm).HasValue ? result.Sum(x => x.TotalKm).Value : 0.0);
                        vehicleReportDetail.ExtraKMCharged = vehicleReportDetail.ExtraKM * vehicleReportDetail.PerExtraKmCharge;
                        vehicleReportDetail.ExtraHrsCharge = vehicleReportDetail.Extrahours * vehicleReportDetail.PerExtraHrCharge;
                        vehicleReportDetail.ExtraCost = vehicleReportDetail.ExtraKMCharged + vehicleReportDetail.ExtraHrsCharge;
                        vehicleReportDetail.Month = search.FromDate.ToString("MMMM-yyyy");
                    }

                    vm = new VehicleReportViewModel
                    {
                        DayWiseTripList = result,
                        VehicleSummary = vehicleReportDetail
                    };

                }
                return vm;
            }
        }

        public bool ExportVehicleReport(VehicleReportViewModel vehicleReportData, string fileName)
        {
            if (vehicleReportData != null && vehicleReportData.DayWiseTripList != null && vehicleReportData.DayWiseTripList.Any())
            {
                var dt = ListToDatatable.ToDataTable<VehicleReportDC>(vehicleReportData.DayWiseTripList);
                TableToExcel(dt, fileName, vehicleReportData);
                return true;
            }
            else
            {
                return false;
            }

        }

        public void TableToExcel(DataTable dt, string file, VehicleReportViewModel vehicleReportData)
        {
            IWorkbook workbook;
            string fileExt = Path.GetExtension(file).ToLower();
            if (fileExt == ".xlsx") { workbook = new XSSFWorkbook(); } else if (fileExt == ".xls") { workbook = new HSSFWorkbook(); } else { workbook = null; }
            if (workbook == null) { return; }
            ISheet sheet = string.IsNullOrEmpty(dt.TableName) ? workbook.CreateSheet("Sheet1") : workbook.CreateSheet(dt.TableName);
            sheet.SetColumnWidth(0, 6000);
            sheet.SetColumnWidth(1, 6000);
            sheet.SetColumnWidth(2, 6000);
            sheet.SetColumnWidth(3, 6000);
            sheet.SetColumnWidth(4, 6000);
            sheet.SetColumnWidth(5, 6000);
            sheet.SetColumnWidth(6, 6000);
            sheet.SetColumnWidth(7, 6000);
            sheet.SetColumnWidth(8, 6000);
            sheet.SetColumnWidth(9, 6000);
            sheet.SetColumnWidth(10, 6000);

            // Create a new font and alter it
            IFont font = workbook.CreateFont();
            font.FontHeightInPoints = 16;
            font.FontName = "Courier New";
            font.Color = NPOI.SS.UserModel.IndexedColors.LightBlue.Index;
            // Fonts are set into a style so create a new one to use.
            ICellStyle headerStyle = workbook.CreateCellStyle();
            headerStyle.SetFont(font);

            //Header  



            int rowNUmber = 0;

            IRow rowHeader1 = sheet.CreateRow(rowNUmber++);
            ICell hdcell = rowHeader1.CreateCell(0);
            hdcell.SetCellValue(vehicleReportData.VehicleSummary.VehicleNo);
            hdcell.CellStyle = headerStyle;

            hdcell = rowHeader1.CreateCell(1);
            hdcell.SetCellValue(vehicleReportData.VehicleSummary.Location_StationCode);
            hdcell.CellStyle = headerStyle;

            hdcell = rowHeader1.CreateCell(2);
            hdcell.SetCellValue(vehicleReportData.VehicleSummary.ClientName);
            hdcell.CellStyle = headerStyle;

            hdcell = rowHeader1.CreateCell(3);
            hdcell.SetCellValue(vehicleReportData.VehicleSummary.Month);
            hdcell.CellStyle = headerStyle;

            hdcell = rowHeader1.CreateCell(4);
            hdcell.SetCellValue(vehicleReportData.VehicleSummary.Driver);
            hdcell.CellStyle = headerStyle;

            hdcell = rowHeader1.CreateCell(5);
            hdcell.SetCellValue(vehicleReportData.VehicleSummary.VehicleType);
            hdcell.CellStyle = headerStyle;



            IRow rowHeader2 = sheet.CreateRow(rowNUmber++);
            hdcell = rowHeader2.CreateCell(0);
            hdcell.SetCellValue("Vehicle No.");

            hdcell = rowHeader2.CreateCell(1);
            hdcell.SetCellValue("Location/Station Code");

            hdcell = rowHeader2.CreateCell(2);
            hdcell.SetCellValue("ClientName");

            hdcell = rowHeader2.CreateCell(3);
            hdcell.SetCellValue("Month");

            hdcell = rowHeader2.CreateCell(4);
            hdcell.SetCellValue("Driver");

            hdcell = rowHeader2.CreateCell(5);
            hdcell.SetCellValue("Vehicle Type");



            rowHeader2 = sheet.CreateRow(rowNUmber++);
            rowHeader2 = sheet.CreateRow(rowNUmber++);


            IRow rowHeader3 = sheet.CreateRow(rowNUmber++);
            hdcell = rowHeader3.CreateCell(0);
            hdcell.SetCellValue(vehicleReportData.VehicleSummary.ContractHours);
            hdcell.CellStyle = headerStyle;

            hdcell = rowHeader3.CreateCell(1);
            hdcell.SetCellValue(vehicleReportData.VehicleSummary.ContractDays);
            hdcell.CellStyle = headerStyle;

            hdcell = rowHeader3.CreateCell(2);
            hdcell.SetCellValue(vehicleReportData.VehicleSummary.ContractKMs);
            hdcell.CellStyle = headerStyle;

            hdcell = rowHeader3.CreateCell(3);
            hdcell.SetCellValue(vehicleReportData.VehicleSummary.ExtraDay);
            hdcell.CellStyle = headerStyle;

            hdcell = rowHeader3.CreateCell(4);
            hdcell.SetCellValue(vehicleReportData.VehicleSummary.ExtraKM);
            hdcell.CellStyle = headerStyle;

            hdcell = rowHeader3.CreateCell(5);
            hdcell.SetCellValue(vehicleReportData.VehicleSummary.Extrahours);
            hdcell.CellStyle = headerStyle;



            IRow rowHeader4 = sheet.CreateRow(rowNUmber++);
            hdcell = rowHeader4.CreateCell(0);
            hdcell.SetCellValue("Contract Hours");

            hdcell = rowHeader4.CreateCell(1);
            hdcell.SetCellValue("Contract Days");

            hdcell = rowHeader4.CreateCell(2);
            hdcell.SetCellValue("Contract Kms");

            hdcell = rowHeader4.CreateCell(3);
            hdcell.SetCellValue("Extra Days");

            hdcell = rowHeader4.CreateCell(4);
            hdcell.SetCellValue("Extra Kms");

            hdcell = rowHeader4.CreateCell(5);
            hdcell.SetCellValue("Extra Hours");



            rowHeader2 = sheet.CreateRow(rowNUmber++);
            rowHeader2 = sheet.CreateRow(rowNUmber++);







            IRow row = sheet.CreateRow(rowNUmber++);
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                ICell cell = row.CreateCell(i);
                cell.SetCellValue(dt.Columns[i].ColumnName);
            }


            // Data
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                IRow row1 = sheet.CreateRow(rowNUmber++);
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    ICell cell = row1.CreateCell(j);
                    cell.SetCellValue(dt.Rows[i][j].ToString());
                }
            }


            // Turn into a byte array
            MemoryStream stream = new MemoryStream();
            workbook.Write(stream);
            var buf = stream.ToArray();


            // Save as an Excel file
            using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                fs.Write(buf, 0, buf.Length);
                fs.Flush();
            }
        }


        #endregion

    }
}
