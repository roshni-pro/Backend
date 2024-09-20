using AngularJSAuthentication.API.App_Code.PackingMaterial;
using AngularJSAuthentication.BusinessLayer.PackingMaterial.BO;
using AngularJSAuthentication.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.PackingMaterial
{
    [RoutePrefix("api/RawMaterial")]
    [Authorize]
    public class RawMaterailController : ApiController
    {
        private RawMaterialRepository _RawMaterialRepository;
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime CurrentDatetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        // private static DateTime CurrentDatetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

        public RawMaterailController()
        {

            this._RawMaterialRepository = new RawMaterialRepository(new AuthContext());

        }

        public RawMaterailController(RawMaterialRepository RawMaterialRepository)
        {
            this._RawMaterialRepository = RawMaterialRepository;
        }


        [Route("InsertRawMaterialMst")]
        [HttpPost]
        public HttpResponseMessage InsertRawMaterialMst(RawMaterailResponse ObjRawMaterailResponse)
        {
            try
            {
                int Userid = GetUserId();

                ObjRawMaterailResponse.RawMaterialMaster.CreatedBy = Userid;
                ObjRawMaterailResponse.RawMaterialMaster.BuyerId = Userid;
                ObjRawMaterailResponse.RawMaterialMaster.CreatedDate = CurrentDatetime;
                bool Result = _RawMaterialRepository.InsertRawMaterial(ObjRawMaterailResponse);
                return Request.CreateResponse(HttpStatusCode.OK, Result);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.InnerException.Message.ToString());
            }
        }


        [Route("GetItemMasterDtls")]
        [HttpGet]
        public HttpResponseMessage GetItemMasterDtls(string ItemNumber, int Warehouseid)
        {
            try
            {
                ItemMaster objItemMaster = _RawMaterialRepository.GetItemMaster(ItemNumber, Warehouseid);
                return Request.CreateResponse(HttpStatusCode.OK, objItemMaster);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.InnerException.Message.ToString());
            }
        }

        [Route("GettemMasterDetailsActype")]
        [HttpGet]
        public HttpResponseMessage GettemMasterDetailsActype(int WareHouseId, string ItemNumber)
        {
            try
            {
                ItemMasterDetailsActype objItemMasterDetailsActype = _RawMaterialRepository.GetItemMasterDetailsActype(2, WareHouseId, ItemNumber);
                return Request.CreateResponse(HttpStatusCode.OK, objItemMasterDetailsActype);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.InnerException.Message.ToString());
            }
        }

        [Route("GetInvoiceDeliveryChallanDetails")]
        [HttpGet]
        public HttpResponseMessage GetInvoiceDeliveryChallanDetails(int InvoiceChallanNo)
        {
            try
            {
                GetRawMaterialDetailsInvoiceResponse objGetRawMaterialDetailsInvoiceResponse = _RawMaterialRepository.RawMaterialInvoiceDtls(InvoiceChallanNo);
                return Request.CreateResponse(HttpStatusCode.OK, objGetRawMaterialDetailsInvoiceResponse);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.InnerException.Message.ToString());

            }
        }

        [Route("GetRawMasterAcBuyer")]
        [HttpGet]
        public HttpResponseMessage GetRawMasterAcBuyer(int Skip, int Take)
        {
            try
            {
                GetRawMaterialMstDetailsResponse ObjGetRawMaterialMstDetailsResponse = _RawMaterialRepository.GetRawMaterialMstDetailsAcBuyer(GetUserId(), Skip, Take);
                return Request.CreateResponse(HttpStatusCode.OK, ObjGetRawMaterialMstDetailsResponse);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.InnerException.Message.ToString());

            }
        }

        [Route("GetPackingMaterialReport")]
        [HttpGet]
        public HttpResponseMessage GetPackingMaterialReport(int WareHouseId, int Skip, int Take,int ?SupplierId)
        {
            if (string.IsNullOrEmpty(Convert.ToString(SupplierId)))
            {
                SupplierId = 0;
            }

            List<GetRawMaterialMstDetails> ObjGetRawMaterialMstDetails = _RawMaterialRepository.PackingMaterialReport(GetUserId(), Skip, Take, WareHouseId, SupplierId);
          

            GetRawMaterialMstDetailsResponse objGetRawMaterialMstDetailsResponse = new GetRawMaterialMstDetailsResponse()
            {
                GetRawMaterialMstDetails = ObjGetRawMaterialMstDetails,
                Count = ObjGetRawMaterialMstDetails.Count()

            };
            return Request.CreateResponse(HttpStatusCode.OK, objGetRawMaterialMstDetailsResponse);
        }

        [Route("GetInvoiceDtlsAcInvoice")]
        [HttpGet]
        public HttpResponseMessage GetInvoiceDtlsAcInvoice(int InvoiceChallanNo, int WareHouseId)
        {
            IEnumerable<RawMaterialDetailsDTO> ObjinvoiceDtls = _RawMaterialRepository.GetRawMaterialDetails(InvoiceChallanNo, GetUserId(), WareHouseId);
            return Request.CreateResponse(HttpStatusCode.OK, ObjinvoiceDtls);
        }
        private int GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }

        private int GetCompanyId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int CompId = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                CompId = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
            return CompId;
        }

    }
}

