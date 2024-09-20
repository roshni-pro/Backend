using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.BusinessLayer.Managers.Transactions;
using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.Transaction.ClusterDashboard;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/ClusterDashboard")]
    public class ClusterDashboardController : BaseAuthController
    {
        [Route("MapData")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<List<ClusterMapData>> GetMapData(GetClusterMapDataParams param)
        {
            var manager = new ClusterDashboardManager();
            return await manager.GetMapData(param);
        }

        [Route("CustomerMapData")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<List<ClusterMapData>> GetCustomerMapData(GetClusterMapDataParams param)
        {
            var manager = new ClusterDashboardManager();
            return await manager.GetCustomerMapData(param);
        }



        [Route("FilteredMapData")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<FilteredMapData> GetFilteredMapData(GetClusterMapDataParams param)
        {
            var manager = new ClusterDashboardManager();
            return await manager.GetFilteredMapData(param);
        }
    }
}