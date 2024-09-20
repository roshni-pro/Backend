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
//    [RoutePrefix("api/PurchaseOrder")]
//    public class PurchaseOrderController : ApiController
//    {



//        public class MYTrnPurchaseOrderDetail : TrnPurchaseOrderDetail
//        {

//            public string ProductName { get; set; }

//            public double ProductPrice { get; set; }
//        }

//        public class MYTrnPurchaseOrder : TrnPurchaseOrder
//        {
//            public string Country { get; set; }
//            public string State { get; set; }
//            public string City { get; set; }
//            public string WarehouseName { get; set; }

//            public Supplier supplier = new Supplier();
//            public List<TrnPurchaseOrderDetail> TrnPurchaseOrderDetails1 = new List<TrnPurchaseOrderDetail>();
//        }
//        Transaction.PurchaseOrder PurchaseOrderData = new Transaction.PurchaseOrder();
//        Data.Product ProductData = new Data.Product();
//        Data.Supplier SupplierData = new Data.Supplier();
//        Data.Locality LocationData = new Data.Locality();
//        Data.Warehouse WarehouseData = new Data.Warehouse();
//        [Route("")]
//        public HttpResponseMessage Get()
//        {
//            try
//            {
//                //List<MYTrnPurchaseOrder> trn=new List<>
//                List<TrnPurchaseOrder> _data = PurchaseOrderData.Get();
//                List<MYTrnPurchaseOrder> MyData = new List<MYTrnPurchaseOrder>();
//                foreach (var orderdata in _data)
//                {
//                    MYTrnPurchaseOrder mysaldata = new MYTrnPurchaseOrder();
//                    MstLocality loc = new MstLocality();
//                    MstWarehouse war = new MstWarehouse();
//                    war = WarehouseData.GetById(orderdata.WarehouseId);
//                    loc = LocationData.GetById(orderdata.LocalityId);

//                    mysaldata.AddressLine1 = orderdata.AddressLine1;
//                    mysaldata.AddressLine2 = orderdata.AddressLine2;
//                    mysaldata.AddressLine3 = orderdata.AddressLine3;
//                    mysaldata.AddressLine4 = orderdata.AddressLine4;
//                    mysaldata.SupplierId = orderdata.SupplierId;
//                    mysaldata.DiscountAmount = orderdata.DiscountAmount;
//                    mysaldata.GrossAmount = orderdata.GrossAmount;
//                    mysaldata.InvoiceNumber = orderdata.InvoiceNumber;
//                    mysaldata.PinCode = orderdata.PinCode;
//                    mysaldata.TaxAmount = orderdata.TaxAmount;
//                    mysaldata.TotalAmount = orderdata.TotalAmount;
//                    mysaldata.WarehouseName = war.Name;
//                    mysaldata.Country = loc.Country;
//                    mysaldata.State = loc.State;
//                    mysaldata.City = loc.City;
//                    mysaldata.Status = orderdata.Status;
//                    mysaldata.ContactNumber1 = orderdata.ContactNumber1;
//                    mysaldata.ContactNumber2 = orderdata.ContactNumber2;
//                    mysaldata.PurchaseOrderId = orderdata.PurchaseOrderId;
//                    Supplier cust = SupplierData.GetById(orderdata.SupplierId);
//                    mysaldata.Supplier.Name = cust.Name;
//                    mysaldata.Supplier.ContactNumber1 = cust.ContactNumber1;
//                    mysaldata.Supplier.EmailId = cust.EmailId;
//                    mysaldata.Supplier.AddressLine1 = cust.AddressLine1;
//                    mysaldata.Supplier.AddressLine2 = cust.AddressLine2;
//                    mysaldata.Supplier.AddressLine3 = cust.AddressLine3;
//                    mysaldata.Supplier.AddressLine4 = cust.AddressLine4;

//                    foreach (var detail in orderdata.PurchaseOrderDetails)
//                    {
//                        MYTrnPurchaseOrderDetail dt = new MYTrnPurchaseOrderDetail();

//                        MstProduct mst = new MstProduct();
//                        mst = ProductData.GetById(detail.ProductId);
//                        dt.ProductId = detail.ProductId;
//                        dt.ProductName = mst.Name;
//                        dt.Amount = detail.Amount;
//                        dt.Quantity = detail.Quantity;
//                        dt.Remark = detail.Remark;
//                        dt.ProductPrice = mst.SalePrice;

//                        mysaldata.TrnPurchaseOrderDetails1.Add(dt);
//                    }
//                    MyData.Add(mysaldata);


//                }
//                if (MyData != null)
//                {
//                    return Request.CreateResponse(HttpStatusCode.OK, MyData);
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
//        //[Route("")]
//        //public HttpResponseMessage Get()
//        //{
//        //    try
//        //    {
//        //        List<TrnPurchaseOrder> _data = PurchaseOrderData.Get();
//        //        if (_data != null)
//        //        {
//        //            return Request.CreateResponse(HttpStatusCode.OK, _data);
//        //        }
//        //        else {
//        //            return Request.CreateResponse(HttpStatusCode.BadRequest);
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
//        //    }
//        //}
//        [ResponseType(typeof(TrnPurchaseOrder))]
//        [Route("")]
//        [AcceptVerbs("POST")]
//        public HttpResponseMessage add(TrnPurchaseOrder OrderData)
//        {
//            try
//            {
//                if (PurchaseOrderData.Post(OrderData))
//                {
//                    return Request.CreateResponse(HttpStatusCode.OK, OrderData);
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

//        [ResponseType(typeof(TrnPurchaseOrder))]
//        [Route("")]
//        [AcceptVerbs("PUT")]
//        public HttpResponseMessage Put(TrnPurchaseOrder OrderData)
//        {

//            try
//            {
//                if (PurchaseOrderData.Put(OrderData))
//                {
//                    return Request.CreateResponse(HttpStatusCode.OK, OrderData);
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


//        [ResponseType(typeof(TrnPurchaseOrder))]
//        [Route("")]
//        [AcceptVerbs("Delete")]
//        public HttpResponseMessage Remove(int id)
//        {
//            try
//            {
//                if (PurchaseOrderData.Delete(id))
//                {
//                    return Request.CreateResponse(HttpStatusCode.OK, id);
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
//    }
//}
