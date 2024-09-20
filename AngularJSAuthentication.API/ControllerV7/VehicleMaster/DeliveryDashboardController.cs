using AngularJSAuthentication.API.Managers.TripPlanner;
using AngularJSAuthentication.API.Managers.Zila;
using AngularJSAuthentication.BusinessLayer.Managers.TripPlanner;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.Transaction;
using AngularJSAuthentication.DataContracts.Transaction.TripPlanner;
using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.VehicleMaster
{
    [RoutePrefix("api/DeliveryDashboard")]
    public class DeliveryDashboardController : BaseApiController
    {
        [Route("GetTrips")]
        [AllowAnonymous]
        public List<PrimeNgDropDown<TripMasterForDropDown>> GetTripList(int warehouseId, int FilterType)
        {

            try
            {
                Warehouse wh = null;
                using (var context = new AuthContext())
                {
                    wh = context.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseId);
                }
                TripPlannerConfirmedMasterManager manager = new TripPlannerConfirmedMasterManager();
                List<TripMasterForDropDown> result = null;

                if (wh.StoreType == 0)
                {
                    result = manager.GetTripList(warehouseId, true, FilterType);
                }
                else
                {
                    ZilaTripManager zilaTripManager = new ZilaTripManager();
                    result = zilaTripManager.ZilaGetTripList(warehouseId, true, FilterType);
                }
                List<PrimeNgDropDown<TripMasterForDropDown>> list = null;
                if (result != null && result.Any())
                {
                    list = new List<PrimeNgDropDown<TripMasterForDropDown>>();

                    foreach (var item in result)
                    {
                        if (item.TripTypeEnum == (int)TripTypeEnum.City && !item.IsManual)
                        {
                            list.Add(new PrimeNgDropDown<TripMasterForDropDown>
                            {
                                label = (item.TripTypeEnum == 0 ? "City-" : "") + " (Trip: " + item.TripNumber.ToString() + ")" + " (" + item.TripPlannerMasterId + ")" + "(" + item.FreezedStatus + ")" + " (Dboy: " + (!string.IsNullOrEmpty(item.DboyName) ? item.DboyName : "Not Assigned") + ")",
                                value = item
                            });
                        }
                        else if (item.TripTypeEnum == (int)TripTypeEnum.SKP && !item.IsManual)
                        {

                            list.Add(new PrimeNgDropDown<TripMasterForDropDown>
                            {
                                label = (item.TripTypeEnum == 1 ? "SKP-" : "") + " (Trip: " + item.TripNumber.ToString() + ")" + " (" + item.TripPlannerMasterId + ")" + "(" + item.FreezedStatus + ")" + " (Dboy: " + (!string.IsNullOrEmpty(item.DboyName) ? item.DboyName : "Not Assigned") + ")",
                                value = item
                            });
                        }
                        else if (item.TripTypeEnum == (int)TripTypeEnum.KPP && !item.IsManual)
                        {

                            list.Add(new PrimeNgDropDown<TripMasterForDropDown>
                            {
                                label = (item.TripTypeEnum == 2 ? "KPP-" : "") + " (Trip: " + item.TripNumber.ToString() + ")" + " (" + item.TripPlannerMasterId + ")" + "(" + item.FreezedStatus + ")" + " (Dboy: " + (!string.IsNullOrEmpty(item.DboyName) ? item.DboyName : "Not Assigned") + ")",
                                value = item
                            });
                        }
                        else if (item.TripTypeEnum == (int)TripTypeEnum.Damage_Expiry && !item.IsManual)
                        {

                            list.Add(new PrimeNgDropDown<TripMasterForDropDown>
                            {
                                label = (item.TripTypeEnum == 3 ? "Damage_Expiry-" : "") + " (Trip: " + item.TripNumber.ToString() + ")" + " (" + item.TripPlannerMasterId + ")" + "(" + item.FreezedStatus + ")" + " (Dboy: " + (!string.IsNullOrEmpty(item.DboyName) ? item.DboyName : "Not Assigned") + ")",
                                value = item
                            });
                        }
                        else if (item.TripTypeEnum == (int)TripTypeEnum.NonSellable && !item.IsManual)
                        {

                            list.Add(new PrimeNgDropDown<TripMasterForDropDown>
                            {
                                label = (item.TripTypeEnum == 4 ? "NonSellable-" : "") + " (Trip: " + item.TripNumber.ToString() + ")" + " (" + item.TripPlannerMasterId + ")" + "(" + item.FreezedStatus + ")" + " (Dboy: " + (!string.IsNullOrEmpty(item.DboyName) ? item.DboyName : "Not Assigned") + ")",
                                value = item
                            });
                        }
                        else
                        {
                            if (item.TripTypeEnum == (int)TripTypeEnum.City && item.IsManual)
                            {
                                list.Add(new PrimeNgDropDown<TripMasterForDropDown>
                                {
                                    label = (item.TripTypeEnum == 0 ? "Manual-City-" : "") + " (Trip: " + item.TripNumber.ToString() + ")" + " (" + item.TripPlannerMasterId + ")" + "(" + item.FreezedStatus + ")" + " (Dboy: " + (!string.IsNullOrEmpty(item.DboyName) ? item.DboyName : "Not Assigned") + ")",
                                    value = item
                                });
                            }
                            else if (item.TripTypeEnum == (int)TripTypeEnum.SKP && item.IsManual)
                            {

                                list.Add(new PrimeNgDropDown<TripMasterForDropDown>
                                {
                                    label = (item.TripTypeEnum == 1 ? "Manual-SKP-" : "") + " (Trip: " + item.TripNumber.ToString() + ")" + " (" + item.TripPlannerMasterId + ")" + "(" + item.FreezedStatus + ")" + " (Dboy: " + (!string.IsNullOrEmpty(item.DboyName) ? item.DboyName : "Not Assigned") + ")",
                                    value = item
                                });
                            }
                            else if (item.TripTypeEnum == (int)TripTypeEnum.KPP && item.IsManual)
                            {

                                list.Add(new PrimeNgDropDown<TripMasterForDropDown>
                                {
                                    label = (item.TripTypeEnum == 2 ? "Manual-KPP-" : "") + " (Trip: " + item.TripNumber.ToString() + ")" + " (" + item.TripPlannerMasterId + ")" + "(" + item.FreezedStatus + ")" + " (Dboy: " + (!string.IsNullOrEmpty(item.DboyName) ? item.DboyName : "Not Assigned") + ")",
                                    value = item
                                });
                            }
                            else if (item.TripTypeEnum == (int)TripTypeEnum.Damage_Expiry && item.IsManual)
                            {

                                list.Add(new PrimeNgDropDown<TripMasterForDropDown>
                                {
                                    label = (item.TripTypeEnum == 3 ? "Manual-Damage_Expiry-" : "") + " (Trip: " + item.TripNumber.ToString() + ")" + " (" + item.TripPlannerMasterId + ")" + "(" + item.FreezedStatus + ")" + " (Dboy: " + (!string.IsNullOrEmpty(item.DboyName) ? item.DboyName : "Not Assigned") + ")",
                                    value = item
                                });
                            }
                            else if (item.TripTypeEnum == (int)TripTypeEnum.NonSellable && item.IsManual)
                            {

                                list.Add(new PrimeNgDropDown<TripMasterForDropDown>
                                {
                                    label = (item.TripTypeEnum == 4 ? "Manual-NonSellable-" : "") + " (Trip: " + item.TripNumber.ToString() + ")" + " (" + item.TripPlannerMasterId + ")" + "(" + item.FreezedStatus + ")" + " (Dboy: " + (!string.IsNullOrEmpty(item.DboyName) ? item.DboyName : "Not Assigned") + ")",
                                    value = item
                                });
                            }
                            else
                            {
                                list.Add(new PrimeNgDropDown<TripMasterForDropDown>
                                {
                                    label = (item.IsManual ? "Manual-" : "") + " (Trip: " + item.TripNumber.ToString() + ")" + " (" + item.TripPlannerMasterId + ")" + "(" + item.FreezedStatus + ")" + " (Dboy: " + (!string.IsNullOrEmpty(item.DboyName) ? item.DboyName : "Not Assigned") + ")",
                                    value = item
                                });
                            }
                        }
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Route("GetTripSummary")]
        public List<TripSummary> GetTripSummary(int tripMasterId)
        {
            using (var authContext = new AuthContext())
            {
                var tripPlannerMasterId = new SqlParameter("@TripPlannerMasterId", tripMasterId);
                var list = authContext.Database.SqlQuery<TripSummary>("Operation.TripPlanner_GetTripSummary @TripPlannerMasterId", tripPlannerMasterId).ToList();
                if (list != null && list.Any())
                {
                    list.ForEach(x =>
                    {
                        if (string.IsNullOrEmpty(x.CurrentStatus))
                        {
                            x.CurrentStatus = "Trip Not Freezed!";
                        }
                        else if (Int32.Parse(x.CurrentStatus) == (int)VehicleliveStatus.OnDuty)
                        {
                            x.CurrentStatus = "OnDuty";
                        }
                        else if (Int32.Parse(x.CurrentStatus) == (int)VehicleliveStatus.NotStarted)
                        {
                            x.CurrentStatus = "NotStarted";
                        }
                        else if (Int32.Parse(x.CurrentStatus) == (int)VehicleliveStatus.InTransit)
                        {
                            x.CurrentStatus = "InTransit";
                        }
                        else if (Int32.Parse(x.CurrentStatus) == (int)VehicleliveStatus.OnBreak)
                        {
                            x.CurrentStatus = "OnBreak";
                        }
                        else if (Int32.Parse(x.CurrentStatus) == (int)VehicleliveStatus.Delivering)
                        {
                            x.CurrentStatus = "Delivering";
                        }
                        else if (Int32.Parse(x.CurrentStatus) == (int)VehicleliveStatus.TripEnd)
                        {
                            x.CurrentStatus = "TripEnd";
                        }
                        else if (Int32.Parse(x.CurrentStatus) == (int)VehicleliveStatus.RejectTrip)
                        {
                            x.CurrentStatus = "RejectTrip";
                        }
                    });
                }
                return list;
            }
        }

        [HttpGet]
        [Route("GetClusterWiseInfo/{ClusterId}")]
        public List<TripDetailsDC> GetClusterTripInfo(int ClusterId)
        {
            List<TripDetailsDC> list = new List<TripDetailsDC>();
            using (var authContext = new AuthContext())
            {
                var clusterId = new SqlParameter("@ClusterId", ClusterId);
                list = authContext.Database.SqlQuery<TripDetailsDC>("GetClusterWiseInfo @ClusterId", clusterId).ToList();
                return list;
            }
        }

        [HttpGet]
        [Route("GetTouchPoints/{tripPlannerMasterId}")]
        public async Task<TripTouchPointInformation> GetTouchPoints(long tripPlannerMasterId)
        {
            TripTouchPointInformation resultList = new TripTouchPointInformation();
            TripPlannerManager tripPlannerManager = new TripPlannerManager();
            resultList = await tripPlannerManager.GetTouchPoints(tripPlannerMasterId);
            return resultList;
        }
        [Route("GetLiveTripList")]
        public List<PrimeNgDropDown<TripMasterForDropDown>> GetLiveTripList(int warehouseId, DateTime? tripTime = null)
        {
            try
            {
                TripPlannerConfirmedMasterManager manager = new TripPlannerConfirmedMasterManager();
                List<TripMasterForDropDown> result = null;
                if (tripTime == null)
                {
                    result = manager.GetLiveTripsList(warehouseId);
                }
                else if (tripTime.HasValue)
                {
                    result = manager.OldTripGet(warehouseId, tripTime.Value);
                }

                List<PrimeNgDropDown<TripMasterForDropDown>> list = null;
                if (result != null && result.Any())
                {
                    list = new List<PrimeNgDropDown<TripMasterForDropDown>>();

                    foreach (var item in result)
                    {
                        list.Add(new PrimeNgDropDown<TripMasterForDropDown>
                        {
                            label = (item.IsManual ? "Manual-" : "") + " (Trip: " + item.TripNumber.ToString() + ")" + " (" + item.TripPlannerMasterId + ")",
                            value = item
                        });
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        [Route("GetLiveTrackerTripHistory/{tripPlannerMasterId}")]
        [HttpGet]
        public List<TripPlannerVehicleHistorieDC> GetLiveTrackerTripHistory(long tripPlannerMasterId)
        {
            List<TripPlannerVehicleHistorieDC> list = new List<TripPlannerVehicleHistorieDC>();
            using (var context = new AuthContext())
            {
                var tripPlannerMasterIds = new SqlParameter("@TripPlannerMasterId", tripPlannerMasterId);
                list = context.Database.SqlQuery<TripPlannerVehicleHistorieDC>("Operation.TripPlanner_GetLiveTrackerTripHistory @TripPlannerMasterId", tripPlannerMasterIds).ToList();
                return list;
            }
        }

        [Route("GetTripOrderDetail/{tripPlannerConfirmDtailId}")]
        [HttpGet]
        public async Task<List<TripOrderDetail>> GetTripOrderDetail(long tripPlannerConfirmDtailId)
        {
            List<TripOrderDetail> list = new List<TripOrderDetail>();
            using (var context = new AuthContext())
            {
                var tripPlannerMasterIds = new SqlParameter("@TripPlanneConfirmedDetailId", tripPlannerConfirmDtailId);
                list = await context.Database.SqlQuery<TripOrderDetail>("Operation.TripPlanner_TripOrderDetail @TripPlanneConfirmedDetailId", tripPlannerMasterIds).ToListAsync();
                return list;
            }
        }

        [Route("GetCustomerList/warehouse/{warehouseId}")]
        [HttpGet]
        public async Task<List<CustomerLocationDc>> GetCustomerList(int warehouseId)
        {
            using (var context = new AuthContext())
            {
                var warehouseParam = new SqlParameter("@WarehouseId", warehouseId);
                var list = await context.Database.SqlQuery<CustomerLocationDc>("Operation.TripPlanner_GetCustomerWithLocationDiff @WarehouseId", warehouseParam).ToListAsync();
                return list;
            }
        }


        [Route("GetCustomerLocation/{customerId}")]
        [HttpGet]
        public async Task<dynamic> GetCustomerLocation(int customerId)
        {
            using (var context = new AuthContext())
            {
                var storedLocation = await context.Customers.Where(x => x.CustomerId == customerId).Select(x => new
                {
                    x.lat,
                    x.lg,
                    x.Warehouseid
                }).FirstOrDefaultAsync();


                var warehouseLocation = await context.Warehouses.Where(x => x.WarehouseId == storedLocation.Warehouseid).Select(x => new
                {
                    x.latitude,
                    x.longitude
                }).FirstOrDefaultAsync();

                var imageUploadedLocation = await context.CustomerUnloadLocationDb.Where(x => x.CustomerId == customerId && x.IsActive == true && x.IsDeleted == false).Select(x => new
                {
                    x.latitude,
                    x.Longitude,
                    x.CreatedDate,
                    x.ShopImageUrl,
                    x.Id
                }).FirstOrDefaultAsync();

                CustomerLocationDiffDc customerLocationDiffDc = new CustomerLocationDiffDc
                {
                    CustomerTableLatitude = storedLocation.lat,
                    CustomerTableLongitude = storedLocation.lg,
                    CustomerId = customerId,
                    UploadImageLatitude = imageUploadedLocation?.latitude,
                    UploadImageLongitude = imageUploadedLocation?.Longitude,
                    ImagePath = imageUploadedLocation?.ShopImageUrl,
                    CustomerUnloadLocationId = imageUploadedLocation?.Id,
                    WarehouseLatitude = warehouseLocation.latitude,
                    WarehouseLongitude = warehouseLocation.longitude
                };

                return customerLocationDiffDc;
            }
        }

        [Route("ChangeCustomerLocation/customer/{customerId}/customerUnloadLocation/{customerUnloadLocationId}")]
        [HttpGet]
        public async Task<bool> ChangeCustomerLocation(int customerId, long customerUnloadLocationId)
        {
            int userId = GetLoginUserId();
            if (customerId > 0 && customerUnloadLocationId > 0)
            {
                using (var context = new AuthContext())
                {
                    var displayName = context.Peoples.Where(x => x.PeopleID == userId).Select(y => y.DisplayName).FirstOrDefault();

                    var customer = await context.Customers.FirstOrDefaultAsync(x => x.CustomerId == customerId);

                    var customerUnloadLocation = await context.CustomerUnloadLocationDb.FirstOrDefaultAsync(x => x.Id == customerUnloadLocationId);

                    if (customerUnloadLocation == null)
                    {
                        return false;
                    }

                    customer.lat = customerUnloadLocation.latitude;
                    customer.lg = customerUnloadLocation.Longitude;
                    customer.UpdatedDate = DateTime.Now;
                    customer.LastModifiedBy = displayName;
                    context.Entry(customer).State = EntityState.Modified;

                    customerUnloadLocation.VerificationStatus = (int)customerUnloadLocationVerificationStatusEnum.Verified;
                    context.Entry(customerUnloadLocation).State = EntityState.Modified;

                    context.Commit();
                }
                return true;
            }
            else
            {
                return false;
            }

        }

        [Route("RemoveCustomerUnloadLocation/{customerUnloadLocationId}")]
        [HttpGet]
        public bool RemoveCustomerUnloadLocation(long customerUnloadLocationId)
        {
            int userId = GetLoginUserId();
            if (customerUnloadLocationId > 0)
            {
                using (var context = new AuthContext())
                {
                    var customerUnloadLocation = context.CustomerUnloadLocationDb.FirstOrDefault(x => x.Id == customerUnloadLocationId);
                    customerUnloadLocation.IsActive = false;
                    customerUnloadLocation.IsDeleted = true;
                    customerUnloadLocation.VerificationStatus = (int)customerUnloadLocationVerificationStatusEnum.Rejected;
                    customerUnloadLocation.ModifiedBy = userId;
                    customerUnloadLocation.ModifiedDate = DateTime.Now;
                    context.Commit();
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

    }


    public class TripSummary
    {
        public long? EstimatedTotalTimeInMin { get; set; }
        public long? EstimatedTraveledTimeInMin { get; set; }
        public long? EstimatedTimeLeftInMin { get; set; }
        public DateTime? TripStartTime { get; set; }
        public DateTime? TripEndTime { get; set; }
        public double? EstimatedTotalDistance { get; set; }
        public double? EstimatedDistanceLeft { get; set; }
        public double? EstimatedTraveledDistance { get; set; }
        public bool IsFreezed { get; set; }
        public bool IsTripGenerated { get; set; }
        public string CurrentStatus { get; set; }
        public DateTime? ReportingTime { get; set; }
        public int? ActualTotalTime { get; set; }
        public int? ActualTraveledTime { get; set; }
        public double? ActualTotalDistanceinKm { get; set; }
        public bool IsPickerGenerated { get; set; }
        public bool IsPickerRequired { get; set; }
        public long PickerMasterId { get; set; }
    }

    public class CustomerLocationDc
    {
        public string Customer { get; set; }
        public int CustomerId { get; set; }

    }

    public class CustomerLocationDiffDc
    {
        public int CustomerId { get; set; }
        public double CustomerTableLatitude { get; set; }
        public double CustomerTableLongitude { get; set; }
        public double? UploadImageLatitude { get; set; }
        public double? UploadImageLongitude { get; set; }
        public String ImagePath { get; set; }
        public long? CustomerUnloadLocationId { get; set; }
        public double WarehouseLatitude { get; set; }
        public double WarehouseLongitude { get; set; }

    }
}
