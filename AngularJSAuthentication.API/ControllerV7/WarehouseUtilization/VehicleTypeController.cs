using AngularJSAuthentication.API.Managers.WarehouseUtilization;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.WarehouseUtilization;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.WarehouseUtilization
{
    [RoutePrefix("api/VehicleType")]
    public class VehicleTypeController : BaseApiController
    {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [AllowAnonymous]
        [HttpGet]
        [Route("GetVehicleTypeList")]
        public async Task<List<VehicleTypeList>> GetVehicleTypeList(int WarehouseId)
        {
            VehicleTypeManager manager = new VehicleTypeManager();
            var list = await manager.GetVehicleTypeList(WarehouseId);
            return list;

        }

        [AllowAnonymous]
        [HttpPost]
        [Route("InsertVechicleAttandance")]
        public async Task<VehicleTypeResponse> InsertVechicleAttandance(InsertVehicleTypeDc insertVehicleTypeDc)
        {
            VehicleTypeManager manager = new VehicleTypeManager();
            var list = await manager.InsertVechicleAttandance(insertVehicleTypeDc);
            return list;

        }

        [AllowAnonymous]
        [HttpPost]
        [Route("UpdateVehicleType")]
        public async Task<VehicleTypeResponse> UpdateVehicleType(InsertVehicleTypeDc insertVehicleTypeDc)
        {
            VehicleTypeManager manager = new VehicleTypeManager();
            var list = await manager.UpdateVehicleType(insertVehicleTypeDc);
            return list;

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("DeleteVechicleTypeList")]
        public async Task<VehicleTypeResponse> DeleteVechicleTypeList(long Id)
        {
            VehicleTypeManager manager = new VehicleTypeManager();
            var list = await manager.DeleteVechicleTypeList(Id);
            return list;

        }
    }
}
