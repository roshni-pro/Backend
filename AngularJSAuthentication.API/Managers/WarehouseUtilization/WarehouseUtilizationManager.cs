using AngularJSAuthentication.DataContracts.WarehouseUtilization;
using AngularJSAuthentication.DataLayer.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Managers.WarehouseUtilization
{
    public class WarehouseUtilizationManager
    {
        public async Task<UtilResponseDc> SaveOldDataDayWise(DateTime TodayDate)
        {
            using (var uom = new UnitOfWork())
            {
                var SaveOldDataDayWiseList = await uom.WarehouseUtilizationRepository.SaveOldDataDayWise(TodayDate);
                uom.Commit();
                return SaveOldDataDayWiseList;
            }
        }
        public async Task<UtilResponseDc> UpdateVehicleCount(UpdateVehicleCountDc updateVehicleCountDc)
        {
            using (var uom = new UnitOfWork())
            {
                var updateList = await uom.WarehouseUtilizationRepository.UpdateVehicleCount(updateVehicleCountDc);
                uom.Commit();
                return updateList;
            }
        }
        public async Task<List<GetUtilizationList>> GetWarehouseUtilizationList(WarehouseUtilVm warehouseUtilVm)
        {
            using (var uom = new UnitOfWork())
            {

                WarehouseUtilVm pastDataInput = new WarehouseUtilVm
                {
                    EndDate = warehouseUtilVm.EndDate,
                    StartDate = warehouseUtilVm.StartDate,
                    WarehouseId = warehouseUtilVm.WarehouseId
                };

                if (warehouseUtilVm.EndDate >= DateTime.Today)
                {
                    pastDataInput.EndDate = DateTime.Today.AddDays(-1);
                }

                var list = await uom.WarehouseUtilizationRepository.GetWarehouseUtilizationList(pastDataInput);


                if (warehouseUtilVm.EndDate >= DateTime.Today)
                {
                    pastDataInput.StartDate = pastDataInput.StartDate > DateTime.Today ? pastDataInput.StartDate : DateTime.Today;
                    pastDataInput.EndDate = warehouseUtilVm.EndDate;
                    var cumulatativeOrderCount = 0;
                    var list2 = await uom.WarehouseUtilizationRepository.GetWarehouseUtilizationFutureList(pastDataInput);
                    if (list2 != null && list2.Any())
                    {
                        cumulatativeOrderCount = list2.OrderBy(x => x.ETADate).First().CumulativePendingCount;
                        if (list == null)
                        {
                            list = new List<GetUtilizationList>();

                        }
                        list.AddRange(list2);
                        list = list.OrderBy(x => x.ETADate).ToList();
                    }

                    int i = 0;

                    //var cumulatativeETAOrderCount = list.First().CumulativePendingETACount;
                    //var cumulatativeETAChangedOrderCount = list.First().CumulativePendingChangeETACount;
                    foreach (var item in list)
                    {
                        if (item.ETADate >= DateTime.Today)
                        {

                            //if(list[i-1].CumulativePendingCount == 0 
                            //    && list[i - 1].DemandOrderCount < list[i - 1].PlannedThresholdOrderCount
                            //    && item.DemandOrderCount > item.PlannedThresholdOrderCount && item.CumulativePendingETACount > 0)
                            //{
                            //    var previousOrderCountCanAdjust = list[i - 1].PlannedThresholdOrderCount - list[i - 1].DemandOrderCount;

                            //    var futureDemandCount = item.DemandOrderCount - item.PlannedThresholdOrderCount;
                            //    if(futureDemandCount > item.CumulativePendingETACount)
                            //    {
                            //        var adjustcount = item.CumulativePendingETACount > previousOrderCountCanAdjust ? previousOrderCountCanAdjust : item.CumulativePendingETACount;
                            //        item.ThisOrNextDayPendingETACount = adjustcount;
                            //        item.DemandOrderCount -= adjustcount;

                            //        list[i - 1].ThisOrNextDayPendingETACount = -1 * adjustcount;
                            //        list[i - 1].DemandOrderCount += adjustcount;

                            //    }
                            //    else
                            //    {
                            //        var adjustcount = futureDemandCount > previousOrderCountCanAdjust ? previousOrderCountCanAdjust : futureDemandCount;
                            //        item.ThisOrNextDayPendingETACount =  adjustcount;
                            //        item.DemandOrderCount -= adjustcount;

                            //        list[i - 1].ThisOrNextDayPendingETACount = -1 * adjustcount;
                            //        list[i - 1].DemandOrderCount += adjustcount;
                            //    }



                            //}
                            var calc = (item.DemandOrderCount + cumulatativeOrderCount) / Convert.ToDouble(item.MaxVehicleOrderCount);
                            item.VehicleCountRequired = Convert.ToInt32(Math.Ceiling(calc));
                            item.CumulativePendingCount = cumulatativeOrderCount;
                            cumulatativeOrderCount = item.DemandOrderCount + cumulatativeOrderCount - item.PlannedThresholdOrderCount;
                            cumulatativeOrderCount = cumulatativeOrderCount > 0 ? cumulatativeOrderCount : 0;



                        }
                        i++;
                    }


                }

                return list.OrderBy(x => x.ETADate).ToList();
                //return GetUtilList;
            }
        }

        public async Task<List<GetUtilizationList>> GetWarehouseUtilizationFutureDataList(WarehouseUtilVm warehouseUtilVm)
        {
            using (var uom = new UnitOfWork())
            {
                var GetUtilList = await uom.WarehouseUtilizationRepository.GetWarehouseUtilizationFutureList(warehouseUtilVm);
                return GetUtilList;
            }
        }

        //public async Task<List<ExportUtilizationList>> ExportWarehouseUtilizationList(WarehouseUtilVm warehouseUtilVm)
        //{
        //    using (var uom = new UnitOfWork())
        //    {
        //        var ExportUtilList = await uom.WarehouseUtilizationRepository.ExportWarehouseUtilizationList(warehouseUtilVm);
        //        return ExportUtilList;
        //    }
        //}
        public async Task<List<GetUtilizationList>> GetWarehouseUtilizationListtt()
        {
            using (var uom = new UnitOfWork())
            {
                var list = await uom.WarehouseUtilizationRepository.GetWarehouseUtilizationListtttt();
                return list;
            }

        }

        public async Task<List<WarehouseUtilReportDc>> WarehouseUtilizationReport(DateTime startdate, DateTime enddate)
        {
            using (var uom = new UnitOfWork())
            {
                var list = await uom.WarehouseUtilizationRepository.WarehouseUtilizationReport(startdate, enddate);
                return list;
            }

        }
    }
}

