using AngularJSAuthentication.API.NewHelper;
using AngularJSAuthentication.Model;
using GenricEcommers.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/ManualLedger")]
    public class ManualLedgerController : ApiController
    {
        [Route("GetLedger")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult GetLedger(int orderid)
        {
            LedgerCorrectorViewModel vm = new LedgerCorrectorViewModel();
            using (var authContext = new AuthContext())
            {
                vm.OrderDispatchedMasterInfo = authContext.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == orderid);

                if (vm.OrderDispatchedMasterInfo != null)
                {
                    vm.OrderDeliveryMasterInfo = authContext.OrderDeliveryMasterDB.FirstOrDefault(x => x.OrderId == orderid && x.DeliveryIssuanceId == vm.OrderDispatchedMasterInfo.DeliveryIssuanceIdOrderDeliveryMaster);
                }

                vm.FinalOrderDispatchedMasterInfo = authContext.FinalOrderDispatchedMasterDb.FirstOrDefault(x => x.OrderId == orderid);
                vm.PaymentResponseRetailerAppList = authContext.PaymentResponseRetailerAppDb.Where(x => x.OrderId == orderid).ToList();
                vm.LedgerUpdatedFields = new LedgerUpdatedFields();
            }
            return Ok(vm);
        }

        [HttpPost]
        [Route("UpdateLedger")]

        public IHttpActionResult UpdateAll(LedgerCorrectorViewModel vm)
        {

            CustomerLedgerHelper.UpdateLedger(vm);
            return Ok();
        }

        [HttpGet]
        [Route("UpdateLedgerdata")]
        [AllowAnonymous]
        public IHttpActionResult UpdateAlldata()
        {
            string sqlquery = "Select t1.OrderId FROM OrderDispatchedMasters t1 JOIN FinalOrderDispatchedMasters t2 ON t1.OrderId = t2.OrderId JOIN OrderDeliveryMasters t3 ON t1.OrderId = t3.OrderId and t3.Status = 'Delivered'"
    + "where t1.[Status] = t2.[Status] and(t1.CashAmount != t2.CashAmount or t1.ElectronicAmount != t2.ElectronicAmount or t1.CheckAmount != t2.CheckAmount) and(t2.CashAmount > 0 or t2.CheckAmount != 0 or t2.ElectronicAmount != 0) ";

            using (var authContext = new AuthContext())
            {
                List<orderidss> newData = authContext.Database.SqlQuery<orderidss>(sqlquery).ToList();
                if (newData != null && newData.Count > 0)
                {
                    foreach (var orderid in newData)
                    {
                        LedgerCorrectorViewModel vm = new LedgerCorrectorViewModel();
                        LedgerUpdatedFields data = new LedgerUpdatedFields();
                        vm.OrderDispatchedMasterInfo = authContext.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == orderid.OrderId);
                        vm.OrderDeliveryMasterInfo = authContext.OrderDeliveryMasterDB.FirstOrDefault(x => x.OrderId == orderid.OrderId);
                        vm.FinalOrderDispatchedMasterInfo = authContext.FinalOrderDispatchedMasterDb.FirstOrDefault(x => x.OrderId == orderid.OrderId);

                        vm.OrderDispatchedMasterInfo.CashAmount = vm.FinalOrderDispatchedMasterInfo.CashAmount;
                        vm.OrderDispatchedMasterInfo.CheckAmount = vm.FinalOrderDispatchedMasterInfo.CheckAmount;
                        vm.OrderDispatchedMasterInfo.ElectronicAmount = vm.FinalOrderDispatchedMasterInfo.ElectronicAmount;

                        vm.OrderDeliveryMasterInfo.CashAmount = vm.FinalOrderDispatchedMasterInfo.CashAmount;
                        vm.OrderDeliveryMasterInfo.CheckAmount = vm.FinalOrderDispatchedMasterInfo.CheckAmount;
                        vm.OrderDeliveryMasterInfo.ElectronicAmount = vm.FinalOrderDispatchedMasterInfo.ElectronicAmount;

                        vm.PaymentResponseRetailerAppList = authContext.PaymentResponseRetailerAppDb.Where(x => x.OrderId == orderid.OrderId).ToList();
                        data.Cash = true;
                        data.Cheque = true;
                        data.Electronic = true;
                        data.Mpos = true;
                        vm.LedgerUpdatedFields = data;

                        CustomerLedgerHelper.UpdateLedger(vm);

                    }

                }
            }

            return Ok();
        }
    }


    public class LedgerCorrectorViewModel
    {
        public OrderDispatchedMaster OrderDispatchedMasterInfo { get; set; }
        public OrderDeliveryMaster OrderDeliveryMasterInfo { get; set; }
        public FinalOrderDispatchedMaster FinalOrderDispatchedMasterInfo { get; set; }
        public List<PaymentResponseRetailerApp> PaymentResponseRetailerAppList { get; set; }
        public LedgerUpdatedFields LedgerUpdatedFields { get; set; }
    }

    public class LedgerUpdatedFields
    {
        public bool Cash { get; set; }
        public bool Cheque { get; set; }
        public bool Electronic { get; set; }
        public bool Mpos { get; set; }
    }

    public class orderidss
    {
        public int OrderId { get; set; }

    }
}
