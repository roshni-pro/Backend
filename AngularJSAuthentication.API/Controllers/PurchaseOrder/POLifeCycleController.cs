using AngularJSAuthentication.API.App_Code.PoLifeCycle;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.BusinessLayer.PoLifeCycle.BO;
using AngularJSAuthentication.DataContracts.ROC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.PurchaseOrder
{
    [RoutePrefix("api/POLifeCycle")]
    public class POLifeCycleController : ApiController
    {
        private PoDashBoardRepository PoDashBoardRepository;
        public POLifeCycleController()
        {
            this.PoDashBoardRepository = new PoDashBoardRepository(new AuthContext());
        }

        public POLifeCycleController(PoDashBoardRepository PoDashBoardRepository)
        {
            this.PoDashBoardRepository = PoDashBoardRepository;
        }

        [HttpPost]
        [Route("GetPoDashBoard")]
        public HttpResponseMessage GetPoDashBoard(PoDashBoardParam ObjPoDashBoardParam)
        {
            try
            {
                string WarehouseId = String.Join(",", ObjPoDashBoardParam.WarehouseId);
                string SupplierId = String.Join(",", ObjPoDashBoardParam.SupplierId);
                PoDashBoardDC poDashBoardDC = PoDashBoardRepository.GetDashBoard(WarehouseId, ObjPoDashBoardParam.StartDate, ObjPoDashBoardParam.EndDate, SupplierId, ObjPoDashBoardParam.BuyerId);
                return Request.CreateResponse(HttpStatusCode.OK, poDashBoardDC);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }

        }

        [HttpGet]
        [Route("GetSuppliers")]
        public HttpResponseMessage GetSuppliers()
        {
            try
            {

                var supplierlist = new List<Model.Supplier>();

                using (AuthContext context = new AuthContext())
                {
                    supplierlist = context.Suppliers.Where(x => x.Active == true && x.Deleted == false).ToList();
                }

                return Request.CreateResponse(HttpStatusCode.OK, supplierlist);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }

        }

        [HttpPost]
        [Route("GetPoTracker")]
        public HttpResponseMessage GetPoTracker(PoDashBoardParam ObjPoDashBoardParam)
        {
            try
            {
                string WarehouseId = String.Join(",", ObjPoDashBoardParam.WarehouseId);
                string SupplierId = String.Join(",", ObjPoDashBoardParam.SupplierId);
                List<POTrackerDC> POTrackerDC = PoDashBoardRepository.GetPoTrackerReport(WarehouseId, ObjPoDashBoardParam.StartDate, ObjPoDashBoardParam.EndDate, SupplierId, ObjPoDashBoardParam.BuyerId).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, POTrackerDC);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }

        }


        [HttpPost]
        [Route("GetItemTracker")]
        public HttpResponseMessage GetItemTracker(PoDashBoardParam ObjPoDashBoardParam)
        {
            try
            {
                string WarehouseId = String.Join(",", ObjPoDashBoardParam.WarehouseId);
                string SupplierId = String.Join(",", ObjPoDashBoardParam.SupplierId);
                List<ItemTrackerDC> ItemTrackerDC = PoDashBoardRepository.GetItemTrackerReport(WarehouseId, ObjPoDashBoardParam.StartDate, ObjPoDashBoardParam.EndDate, SupplierId, ObjPoDashBoardParam.BuyerId).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, ItemTrackerDC);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }

        }


        [HttpPost]
        [Route("GetPoDahboardExport")]
        public HttpResponseMessage GetPoDahboardExport(PoDashBoardParam ObjPoDashBoardParam)
        {
            try
            {
                string WarehouseId = String.Join(",", ObjPoDashBoardParam.WarehouseId);
                string SupplierId = String.Join(",", ObjPoDashBoardParam.SupplierId);
                string[] Status = ObjPoDashBoardParam.Status.Split(',');
                DataTable dt = new DataTable();
                dt.Columns.Add("stringValue");
                for (int i = 0; i < Status.Length; i++)
                {
                    DataRow dr = dt.NewRow();
                    dr["stringValue"] = Status[i];
                    dt.Rows.Add(dr);

                }
                //string Status = String.Join(",", ObjPoDashBoardParam.Status);
                List<POTrackerDC> POTrackerDC = PoDashBoardRepository.GetPoTrackerExport(WarehouseId, ObjPoDashBoardParam.StartDate, ObjPoDashBoardParam.EndDate, SupplierId, ObjPoDashBoardParam.BuyerId, dt).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, POTrackerDC);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }

        }

        [HttpPost]
        [Route("GetPoCreatedDahboardExport")]
        public HttpResponseMessage GetPoCreatedDahboardExport(PoDashBoardParam ObjPoDashBoardParam)
        {
            try
            {
                string WarehouseId = String.Join(",", ObjPoDashBoardParam.WarehouseId);
                string SupplierId = String.Join(",", ObjPoDashBoardParam.SupplierId);

                //string Status = String.Join(",", ObjPoDashBoardParam.Status);

                List<POTrackerDC> POTrackerDC = PoDashBoardRepository.GetCreatePoTrackerExport(WarehouseId, ObjPoDashBoardParam.StartDate, ObjPoDashBoardParam.EndDate, SupplierId, ObjPoDashBoardParam.BuyerId, ObjPoDashBoardParam.ProcedureName).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, POTrackerDC);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }

        }

        [HttpPost]
        [Route("GetDetailPoDashBoard")]
        public HttpResponseMessage GetDetailPoDashBoard(PoDashBoardParamm ObjPoDashBoardParam)
        {
            try
            {
                var identity = User.Identity as System.Security.Claims.ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                //if (ObjPoDashBoardParam.BuyerId == 0)
                //{
                //    ObjPoDashBoardParam.BuyerId = userid;
                //}
                string WarehouseId = String.Join(",", ObjPoDashBoardParam.WarehouseId);
                string SupplierId = String.Join(",", ObjPoDashBoardParam.SupplierId);
                DetailPoDashboardDC poDashBoardDC = PoDashBoardRepository.GetDetailDashBoard(WarehouseId, ObjPoDashBoardParam.StartDate, ObjPoDashBoardParam.EndDate, SupplierId, ObjPoDashBoardParam.BuyerId, ObjPoDashBoardParam.Poid,
                    ObjPoDashBoardParam.Skip, ObjPoDashBoardParam.Take);

                if (poDashBoardDC != null && poDashBoardDC.detailPoDashboards.Count > 0)
                {
                    TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                    List<ItemWarehouseDc> itemWarehouseDcs = new List<ItemWarehouseDc>();
                    var itemWarehouse = poDashBoardDC.detailPoDashboards.Select(x => new ItemWarehouseDc { WarehouseId = x.WarehouseId, ItemMultiMRPId = x.ItemMultiMRPId }).ToList();
                    var list = tripPlannerHelper.RocTagValueGet(itemWarehouse);
                    if (list != null)
                    {
                        foreach (var da in poDashBoardDC.detailPoDashboards)
                        {
                            da.Tag = list.Result.Where(x => x.ItemMultiMRPId == da.ItemMultiMRPId && x.WarehouseId == da.WarehouseId).Select(x => x.Tag).FirstOrDefault();
                        }
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, poDashBoardDC);

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }

        }

    }
}
