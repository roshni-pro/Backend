using AngularJSAuthentication.DataContracts.WarehouseUtilization;
using AngularJSAuthentication.DataLayer.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Managers.WarehouseUtilization
{
    public class VehicleTypeManager
    {
        public async Task<List<VehicleTypeList>> GetVehicleTypeList(int WarehouseId)
        {
            using (var uom = new UnitOfWork())
            {
                var VehicleList = await uom.VehicleTypeRepository.GetVehicleTypeList(WarehouseId);              
                return VehicleList;
            }
        }

        public async Task<VehicleTypeResponse> InsertVechicleAttandance(InsertVehicleTypeDc insertVehicleTypeDc)
        {
            using (var uom = new UnitOfWork())
            {
                var List = await uom.VehicleTypeRepository.InsertVechicleAttandance(insertVehicleTypeDc);
                uom.Commit();
                return List;
            }
        }

        public async Task<VehicleTypeResponse> UpdateVehicleType(InsertVehicleTypeDc insertVehicleTypeDc)
        {
            using (var uom = new UnitOfWork())
            {
                var UpdateList = await uom.VehicleTypeRepository.UpdateVehicleType(insertVehicleTypeDc);
                uom.Commit();
                return UpdateList;
            }
        }

        public async Task<VehicleTypeResponse> DeleteVechicleTypeList(long Id)
        {
            using (var uom = new UnitOfWork())
            {
                var DeleteList = await uom.VehicleTypeRepository.DeleteVechicleTypeList(Id);
                uom.Commit();
                return DeleteList;
            }
        }
    }
}