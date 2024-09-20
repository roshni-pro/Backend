using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.BusinessLayer.Managers.Transactions;
using AngularJSAuthentication.DataContracts.Shared;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/LatLng")]
    public class LatLngController : BaseAuthController
    {
        [Route("SortOrdersInAssignment")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<SortedAssignmentOrders> SortOrdersInAssignment(SortedAssignmentOrdersPostParams param)
        {
            SortedAssignmentOrders sortedOrders = new SortedAssignmentOrders();

            DeliveryAssignmentManager manager = new DeliveryAssignmentManager();
            sortedOrders.SortedOrders = await manager.GetAssignmentOrdersWithLatLng(param);

            return sortedOrders;
        }
    }
}