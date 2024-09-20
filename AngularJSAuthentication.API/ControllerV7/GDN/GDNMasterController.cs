using AngularJSAuthentication.API.Helper.GDN;
using AngularJSAuthentication.DataContracts.Transaction.PurchaseOrder;
using AngularJSAuthentication.Model.GDN;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using static AngularJSAuthentication.API.ControllerV7.SarthiAppController;

namespace AngularJSAuthentication.API.ControllerV7.GDN
{
    [RoutePrefix("api/GDNMaster")]
    public class GDNMasterController : ApiController
    {
        [Route("GetGDNMasters")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetGDNMasters(int POId, string GDNNumber, DateTime? startDate, DateTime? EndDate)
        {
            try
            {
                int totcount = 0;
                List<GDNDTO> GDNList = new List<GDNDTO>();
                using (var context = new AuthContext())
                {
                    if (GDNNumber == "undefined")
                    {
                        GDNNumber = "";
                    }

                    if (POId != 0 || !string.IsNullOrEmpty(GDNNumber))
                    {
                        GDNList = (from a in context.GoodsDescripancyNoteMasterDB
                                   where (a.IsDeleted == false && a.IsActive == true && a.IsGDNGenerate == true && (a.PurchaseOrderId == POId || a.GDNNumber.Contains(GDNNumber)))
                                   join b in context.DPurchaseOrderMaster on a.PurchaseOrderId equals b.PurchaseOrderId
                                   join p in context.Peoples on a.CreatedBy equals p.PeopleID
                                   select new GDNDTO
                                   {
                                       id = a.Id,
                                       PurchaseOrderId = a.PurchaseOrderId,
                                       SupplierName = b.SupplierName,
                                       GDNNumber = a.GDNNumber,
                                       Status = a.Status,
                                       CreatedBy = p.DisplayName,
                                       CreatedDate = a.CreatedDate,
                                       GrSerialNo = a.GrSerialNo,
                                       Comment = context.GDNConversations.Where(y => y.GDNId == a.Id && y.IsActive == true && y.PeopleType == "Supplier" && y.IsDeleted == false).FirstOrDefault().Commants
                                   }).ToList();
                    }
                    else
                    {

                        EndDate = EndDate.Value.AddDays(1);
                        GDNList = (from a in context.GoodsDescripancyNoteMasterDB
                                   where (a.IsDeleted == false && a.IsActive == true && a.IsGDNGenerate == true) && a.CreatedDate >= startDate && a.CreatedDate <= EndDate
                                   join b in context.DPurchaseOrderMaster on a.PurchaseOrderId equals b.PurchaseOrderId
                                   join p in context.Peoples on a.CreatedBy equals p.PeopleID
                                   select new GDNDTO
                                   {
                                       id = a.Id,
                                       PurchaseOrderId = a.PurchaseOrderId,
                                       SupplierName = b.SupplierName,
                                       GDNNumber = a.GDNNumber,
                                       Status = a.Status,
                                       CreatedBy = p.DisplayName,
                                       CreatedDate = a.CreatedDate,
                                       GrSerialNo = a.GrSerialNo,
                                       Comment = context.GDNConversations.Where(y => y.GDNId == a.Id && y.IsActive == true && y.PeopleType == "Supplier" && y.IsDeleted == false).FirstOrDefault().Commants
                                   }).OrderByDescending(x => x.CreatedDate).ToList();
                    }

                    totcount = context.GoodsDescripancyNoteMasterDB.Where(x => x.IsActive == true && x.IsDeleted == false && x.IsGDNGenerate == true).Count();
                }
                GDNMastersDTO obj = new GDNMastersDTO();
                obj.GoodsDescripancyNoteMasterDB = GDNList;
                obj.TotalCount = totcount;
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "");
            }
        }

        [Route("GetGDNDetail")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetGDNDetails(long GDNId)
        {
            try
            {
                using (var context = new AuthContext())
                {
                    var param = new SqlParameter("@GDNId", GDNId);
                    List<GoodsDescripancyNoteDetailDC> data = context.Database.SqlQuery<GoodsDescripancyNoteDetailDC>("Exec SPGDNDetail @GDNId", param).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, data);
                }
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.BadRequest, "");
            }
        }



        [Route("GetGDNdetailsV7")]
        [HttpGet]
        [AllowAnonymous]
        public List<GDNDC> GetGDNdetailsV7(long GDNid)
        {
            var result = new List<GDNDC>();
            try
            {
                GDNHelper helper = new GDNHelper();
                result = helper.GetGDNdetailsv7(GDNid);
            }
            catch (Exception x)
            {
                result = null;
            }
            return result;
        }

        [Route("UpdateGDNDetail")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage UpdateGDNDetail(GoodsDescripancyNoteDetail obj)
        {
            try
            {
                GDNHelper helper = new GDNHelper();
                bool result = helper.BuyerAction(obj, GetUserId());
                if (result)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "1");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "0");
                }
            }
            catch (Exception n)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "0");
            }
        }
        [Route("GetGoodDescripancyNote")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GoodDescripancyNote(int PurchaseOrderId)
        {
            try
            {
                using (var context = new AuthContext())
                {
                    var param = new SqlParameter("@PurchaseOrderId", PurchaseOrderId);
                    List<GoodDescripancyNote> data = context.Database.SqlQuery<GoodDescripancyNote>("Exec SPGoodDescripancyNote @PurchaseOrderId", param).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, data);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "");
            }

        }


        [Route("SendToSupplire")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage SendToSupplire(int PurchaseOrderId, string WhName, int SupplierId, int GDNId)
        {
            bool flag = false;
            try
            {
                GDNHelper obj = new GDNHelper();

                bool IsSend = false;
                using (var context = new AuthContext())
                {
                    var supplier = context.Suppliers.Where(a => a.SupplierId == SupplierId).FirstOrDefault();

                    var grSerialno = context.GoodsDescripancyNoteMasterDB.Where(x => x.Id == GDNId).FirstOrDefault().GrSerialNo;

                    IsSend = GDNHelper.sendGDNtoSupplier(PurchaseOrderId, WhName, supplier.MobileNo, grSerialno, supplier.EmailId, context);

                    if (IsSend)
                    {
                        int Userid = GetUserId();
                        obj.InsertConversation(PurchaseOrderId, GDNId, Userid, context);
                    }
                    if (context.Commit() > 0)
                    { flag = true; }
                    return Request.CreateResponse(HttpStatusCode.OK, flag);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, flag);
            }

        }

        [Route("BuyerCancel")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage BuyerCancelGDN(long GDNid, string comment)
        {
            bool flag = false;
            try
            {
                GDNHelper obj = new GDNHelper();

                flag = obj.BuyerCancelGDN(GDNid, GetUserId(), comment);
                return Request.CreateResponse(HttpStatusCode.OK, flag);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, flag);
            }

        }


        private int GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }




    }
}
public class GDNMastersDTO
{
    public List<GDNDTO> GoodsDescripancyNoteMasterDB { get; set; }
    public int TotalCount { get; set; }
}
public class GoodDescripancyNote
{
    public int PurchaseOrderId { get; set; }
    public string suppliername { get; set; }
    public string WarehouseName { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public string InvoiceNumber { get; set; }
    public string SKUDescription { get; set; }
    public int ShortQty { get; set; }
    public int ReturnQty { get; set; }
    public double CostPrice { get; set; }
    public int GST { get; set; }
    public double Total { get; set; }
    public double Tax { get; set; }
}

public class GDNDTO
{
    public long id { get; set; }
    public string GDNNumber { get; set; }
    public long PurchaseOrderId { get; set; }
    public string SupplierName { get; set; }
    public string Status { get; set; } // Pending , Approved, Reject
    public string CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Comment { get; set; }
    public int GrSerialNo { get; set; }

} 