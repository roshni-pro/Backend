//using AngularJSAuthentication.Model;
//using pos.Model;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Web.Http;
//using System.Web.Http.Description;

//namespace POSApi.Controllers
//{
//    [RoutePrefix("api/PurchaseDeliveryOrder")]
//    public class PurchaseDeliveryOrderController : ApiController
//    {

//        Transaction.PurchaseInvoice PurchaseInvoiceData = new Transaction.PurchaseInvoice();
//        Transaction.PurchaseInvoiceDetail PurchaseInvoiceDetailData = new Transaction.PurchaseInvoiceDetail();

//        Data.ProductData ProductData = new Data.ProductData();
//        Data.SupplierData SupplierData = new Data.SupplierData();
//        Data.Locality LocationData = new Data.Locality();
//        Data.WarehouseData WarehouseData = new Data.WarehouseData();
//        [Route("")]
//        public HttpResponseMessage Get()
//        {
//            try
//            {
//                List<TrnPurchaseInvoice> _data = PurchaseInvoiceData.Get();
//                List<Supplier> _Supplierdata = SupplierData.Get();

//                foreach (var i in _data)
//                {
//                    var supplier = _Supplierdata.Where(s => s.SupplierId == i.SupplierId).Select
//                        (s => new { s.Name, s.AddressLine1, s.AddressLine2, s.AddressLine3, s.AddressLine4, s.ContactNumber1, s.ContactNumber2 }).SingleOrDefault();
//                    i.SupplierName = supplier.Name;
//                    i.ContactNumber1 = supplier.ContactNumber1;
//                    i.ContactNumber1 = supplier.ContactNumber2;
//                    foreach (var j in i.PurchaseOrderDetails)
//                    {
//                        j.ItemName = ProductData.GetById(j.ItemID).Name.ToString();
//                        var prodata = ProductData.GetById(j.ProductId);
//                        j.ItemName = prodata.itemname;
//                        j.PurchasePrice = prodata.PurchasePrice;
//                        j.SalePrice = prodata.SalePrice;
//                    }
//                }

//                if (_data != null)
//                {
//                    return Request.CreateResponse(HttpStatusCode.OK, _data);
//                }
//                else {
//                    return Request.CreateResponse(HttpStatusCode.BadRequest);
//                }
//            }
//            catch (Exception ex)
//            {
//                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
//            }

//        }

//        Transaction.MoveStock movesStock = new Transaction.MoveStock();

//        [ResponseType(typeof(TrnMoveStock))]
//        [Route("")]
//        [AcceptVerbs("POST")]
//        public HttpResponseMessage add(TrnMoveStock stock)
//        {
//            try
//            {

//                if (movesStock.Post(stock))
//                {
//                    return Request.CreateResponse(HttpStatusCode.OK, stock);
//                }
//                else {
//                    return Request.CreateResponse(HttpStatusCode.BadRequest);
//                }
//                return null;
//            }
//            catch (Exception ex)
//            {
//                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
//            }
//        }


//    }
//}
