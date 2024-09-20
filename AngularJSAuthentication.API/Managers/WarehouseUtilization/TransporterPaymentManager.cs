using AngularJSAuthentication.DataContracts.WarehouseUtilization;
using AngularJSAuthentication.DataLayer.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Managers.WarehouseUtilization
{
    public class TransporterPaymentManager
    {
        public async Task<List<GetTranspoterPaymentDetailDc>> GetTranspoterPaymentDetailList(TransporterPaymentVm transporterPaymentVm)
        {
            using (var uom = new UnitOfWork())
            {
                var GetTranspoterPaymentDetail = await uom.TransporterPaymentRepository.GetTranspoterPaymentDetailList(transporterPaymentVm);
                return GetTranspoterPaymentDetail;
            }
        }

        public async Task<List<GetFleetListDc>> GetFleetListByWhId(int WarehouseId)
        {
            using (var uom = new UnitOfWork())
            {
                var GetFleetList = await uom.TransporterPaymentRepository.GetFleetListByWhId(WarehouseId);
                return GetFleetList;
            }
        }

        public async Task<bool> TransporterPaymentDetailInsert(long TransporterPaymentId, DateTime startDate, DateTime EndDate)
        {
            using (var uom = new UnitOfWork())
            {
                var InsertData = await uom.TransporterPaymentRepository.TransporterPaymentDetailInsert(TransporterPaymentId, startDate, EndDate);
                uom.Commit();
                return InsertData;
            }
        }

        public async Task<long> TransporterPaymentInsert(DateTime TodayDate, long FleetMasterId, int warehouseid)
        {
            using (var uom = new UnitOfWork())
            {
                var InsertData = await uom.TransporterPaymentRepository.TransporterPaymentInsert(TodayDate, FleetMasterId, warehouseid);
                uom.Commit();
                return InsertData;
            }
        }

        public async Task<List<TransporterPayVehicleAttadanceListDc>> GetTransporterPaymentVehicleAttadanceList(TransporterVehicleAttadanceDc transporterVehicleAttadanceDc)
        {
            using (var uom = new UnitOfWork())
            {
                var list = await uom.TransporterPaymentRepository.GetTransporterPaymentVehicleAttadanceList(transporterVehicleAttadanceDc);
                return list;
            }
        }

        public async Task<TransporterPayWithDetailDc> TransporterPayWithDetail(TransporterPaymentVm transporterPaymentVm)
        {
            using (var uom = new UnitOfWork())
            {
                var list = await uom.TransporterPaymentRepository.TransporterPayWithDetail(transporterPaymentVm);
                return list;
            }
        }
        public async Task<List<GetRegionalListDc>> GetRegionalList(int Warehouseid, DateTime ForDate)
        {
            using (var uom = new UnitOfWork())
            {
                var list = await uom.TransporterPaymentRepository.GetRegionalList(Warehouseid, ForDate);
                return list;
            }
        }

        public async Task<List<GetRegionalListDc>> GetRegionalListV2(RegionalAllWHInput input)
        {
            using (var uom = new UnitOfWork())
            {
                var list = await uom.TransporterPaymentRepository.GetRegionalListV2(input);
                return list;
            }
        }
        

        public async Task<List<GetRegionalListDc>> GetRegionalSummaryList(ReginalSummaryInput input)
        {
            using (var uom = new UnitOfWork())
            {
                var list = await uom.TransporterPaymentRepository.GetRegionalSummaryList(input);
                return list;
            }
        }



        public async Task<PaymentInvoiceDc> GenerateInvoice(int paymentid, DateTime startdate)
        {
            using (var uom = new UnitOfWork())
            {
                var data = await uom.TransporterPaymentRepository.GenerateInvoice(paymentid, startdate);
                return data;
            }
        }

        public async Task<List<TallyFileListDc>> ExportTallyFile(int warehouseid, DateTime Fordate)
        {
            using (var uom = new UnitOfWork())
            {
                var data = await uom.TransporterPaymentRepository.ExportTallyFile(warehouseid, Fordate);
                return data;
            }
        }

        public async Task<List<PaymentListDc>> ExportPaymentFile(int warehouseid, DateTime Fordate)
        {
            using (var uom = new UnitOfWork())
            {
                var data = await uom.TransporterPaymentRepository.ExportPaymentFile(warehouseid, Fordate);
                return data;
            }
        }

        public async Task<List<PaymentListDc>> ExportPaymentFile(RegionalAllWHInput input)
        {
            try
            {
                using (var uom = new UnitOfWork())
                {
                    var data = await uom.TransporterPaymentRepository.ExportPaymentFile(input);
                    return data;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<TransporterPaymentHistoryListDc>> GetTransporterPaymentHistoryList(int paymentId)
        {
            using (var uom = new UnitOfWork())
            {
                var data = await uom.TransporterPaymentRepository.GetTransporterPaymentHistoryList(paymentId);
                return data;
            }
        }

        public async Task<List<TransporterPaymentVehicleList>> GetTranspoterPaymentVehicleList(long TranspoterPaymentId)
        {
            using (var uom = new UnitOfWork())
            {
                var data = await uom.TransporterPaymentRepository.GetTranspoterPaymentVehicleList(TranspoterPaymentId);
                return data;
            }
        }

        public async Task<TransporterPayVehicleInfo> GetTransporterPayVehicleInfo(long VehicleMasterId)
        {
            using (var uom = new UnitOfWork())
            {
                var data = await uom.TransporterPaymentRepository.GetTransporterPayVehicleInfo(VehicleMasterId);
                return data;
            }
        }

        public async Task<List<GetRegionalListByAllWh>> GetRegionalListByAllWh(RegionalAllWHInput input)
        {
            using (var uom = new UnitOfWork())
            {
                var list = await uom.TransporterPaymentRepository.GetRegionalListByAllWh(input);
                return list;
            }
        }
    }
}