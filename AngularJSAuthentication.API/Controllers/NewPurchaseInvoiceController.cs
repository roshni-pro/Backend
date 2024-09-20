
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
//    [RoutePrefix("api/PurchaseInvoice")]
//    public class PurchaseInvoiceController : ApiController
//    {



//        public class MYTrnPurchaseInvoiceDetail : TrnPurchaseInvoiceDetail
//        {

//            public string ProductName { get; set; }

//            public double ProductPrice { get; set; }
//        }

//        public class MYTrnPurchaseInvoice : TrnPurchaseInvoice
//        {
//            public string Country { get; set; }
//            public string State { get; set; }
//            public string City { get; set; }
//            public string WarehouseName { get; set; }

//            public Supplier Supplier = new Supplier();
//            public List<TrnPurchaseInvoiceDetail> TrnPurchaseInvoiceDetails1 = new List<TrnPurchaseInvoiceDetail>();
//        }
//        Transaction.PurchaseInvoice PurchaseInvoiceData = new Transaction.PurchaseInvoice();
//        Data.ProductData ProductData = new Data.ProductData();
//        Data.SupplierData SupplierData = new Data.SupplierData();
//        Data.Locality LocationData = new Data.Locality();
//        Data.WarehouseData WarehouseData = new Data.WarehouseData();
//        [Route("")]
//        public HttpResponseMessage Get()
//        {
//            try
//            {
//                //List<MYTrnPurchaseInvoice> trn=new List<>
//                List<TrnPurchaseInvoice> _data = PurchaseInvoiceData.Get();
//                List<MYTrnPurchaseInvoice> MyData = new List<MYTrnPurchaseInvoice>();
//                foreach (var orderdata in _data)
//                {
//                    MYTrnPurchaseInvoice mysaldata = new MYTrnPurchaseInvoice();
//                    MstLocality loc = new MstLocality();



//                    mysaldata.SupplierId = orderdata.SupplierId;
//                    mysaldata.DiscountAmount = orderdata.DiscountAmount;
//                    mysaldata.GrossAmount = orderdata.GrossAmount;
//                    //mysaldata.InvoiceNumber = orderdata.InvoiceNumber;
//                    //mysaldata.PinCode = orderdata.PinCode;
//                    mysaldata.TaxAmount = orderdata.TaxAmount;
//                    mysaldata.TotalAmount = orderdata.TotalAmount;
//                    //mysaldata.WarehouseName = war.Name;
//                    mysaldata.Country = loc.Country;
//                    mysaldata.State = loc.State;
//                    mysaldata.City = loc.City;
//                    //mysaldata.Status = orderdata.Status;
//                    //mysaldata.ContactNumber1 = orderdata.ContactNumber1;
//                    //mysaldata.ContactNumber2 = orderdata.ContactNumber2;
//                    mysaldata.PurchaseInvoiceId = orderdata.PurchaseInvoiceId;
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
//                        MYTrnPurchaseInvoiceDetail dt = new MYTrnPurchaseInvoiceDetail();

//                        ItemMaster mst = new ItemMaster();
//                        mst = ProductData.GetById(detail.ProductId);
//                        dt.ProductName = mst.itemname;
//                        dt.Amount = detail.Amount;
//                        dt.Quantity = detail.Quantity;
//                        dt.Remark = detail.Remark;
//                        dt.ProductPrice = double.Parse( mst.SellingPrice.ToString());


//                        mysaldata.TrnPurchaseInvoiceDetails1.Add(dt);
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
//        //        List<TrnPurchaseInvoice> _data = PurchaseInvoiceData.Get();
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
//        [ResponseType(typeof(TrnPurchaseInvoice))]
//        [Route("")]
//        [AcceptVerbs("POST")]
//        public HttpResponseMessage add(TrnPurchaseInvoice OrderData)
//        {
//            try
//            {
//                OrderData.Date = indianTime;
//                foreach (var inv in OrderData.PurchaseOrderDetails)
//                {
//                    inv.UpdatedDate = indianTime;
//                    inv.IsDeleted = false;
//                    inv.IsActive = true;
//                    inv.CreatedDate = indianTime;
//                }
//                if (PurchaseInvoiceData.Post(OrderData))
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

//        [ResponseType(typeof(TrnPurchaseInvoice))]
//        [Route("")]
//        [AcceptVerbs("PUT")]
//        public HttpResponseMessage Put(TrnPurchaseInvoice OrderData)
//        {

//            try
//            {
//                if (PurchaseInvoiceData.Put(OrderData))
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


//        [ResponseType(typeof(TrnPurchaseInvoice))]
//        [Route("")]
//        [AcceptVerbs("Delete")]
//        public HttpResponseMessage Remove(int id)
//        {
//            try
//            {
//                if (PurchaseInvoiceData.Delete(id))
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
