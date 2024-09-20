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
//    [RoutePrefix("api/SaleInvoice")]
//    public class SaleInvoiceController : ApiController
//    {



//        public class MYTrnSaleInvoiceDetail : TrnSaleInvoiceDetail
//        {

//            public string ProductName { get; set; }

//            public double ProductPrice { get; set; }
//        }

//        public class MYTrnSaleInvoice : TrnSaleInvoice
//        {
//            public string Country { get; set; }
//            public string State { get; set; }
//            public string City { get; set; }
//            public string WarehouseName { get; set; }

//            public Customer Customer = new Customer();
//            public List<TrnSaleInvoiceDetail> TrnSaleInvoiceDetails1 = new List<TrnSaleInvoiceDetail>();
//        }
//        Transaction.SaleInvoice SaleInvoiceData = new Transaction.SaleInvoice();
//        Data.Product ProductData = new Data.Product();
//        Data.Customer CustomerData = new Data.Customer();
//        Data.Locality LocationData = new Data.Locality();
//        Data.Warehouse WarehouseData = new Data.Warehouse();
//        [Route("")]
//        public HttpResponseMessage Get()
//        {
//            try
//            {
//                List<MYTrnSaleInvoice> trn = new List<>
//                List<TrnSaleInvoice> _data = SaleInvoiceData.Get();
//                List<MYTrnSaleInvoice> MyData = new List<MYTrnSaleInvoice>();
//                foreach (var orderdata in _data)
//                {
//                    MYTrnSaleInvoice mysaldata = new MYTrnSaleInvoice();
//                    MstLocality loc = new MstLocality();



//                    mysaldata.CustomerId = orderdata.CustomerId;
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
//                    mysaldata.SaleInvoiceId = orderdata.SaleInvoiceId;
//                    Customer cust = CustomerData.GetById(orderdata.CustomerId);
//                    mysaldata.Customer.Name = cust.Name;
//                    mysaldata.Customer.ContactNumber1 = cust.ContactNumber1;
//                    mysaldata.Customer.EmailId = cust.EmailId;
//                    mysaldata.Customer.AddressLine1 = cust.AddressLine1;
//                    mysaldata.Customer.AddressLine2 = cust.AddressLine2;
//                    mysaldata.Customer.AddressLine3 = cust.AddressLine3;
//                    mysaldata.Customer.AddressLine4 = cust.AddressLine4;

//                    foreach (var detail in orderdata.SaleInvoiceDetails)
//                    {
//                        MYTrnSaleInvoiceDetail dt = new MYTrnSaleInvoiceDetail();

//                        MstProduct mst = new MstProduct();
//                        mst = ProductData.GetById(detail.ProductId);
//                        dt.ProductName = mst.Name;
//                        dt.Amount = detail.Amount;
//                        dt.Quantity = detail.Quantity;
//                        dt.Remark = detail.Remark;
//                        dt.ProductPrice = mst.SalePrice;


//                        mysaldata.TrnSaleInvoiceDetails1.Add(dt);
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
//        [Route("")]
//        public HttpResponseMessage Get()
//        {
//            try
//            {
//                List<TrnSaleInvoice> _data = SaleInvoiceData.Get();
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
//        [ResponseType(typeof(TrnSaleInvoice))]
//        [Route("")]
//        [AcceptVerbs("POST")]
//        public HttpResponseMessage add(TrnSaleInvoice OrderData)
//        {
//            try
//            {
//                OrderData.Date = indianTime;
//                foreach (var inv in OrderData.SaleInvoiceDetails)
//                {
//                    inv.UpdatedDate = indianTime;
//                    inv.IsDeleted = false;
//                    inv.IsActive = true;
//                    inv.CreatedDate = indianTime;
//                }
//                if (SaleInvoiceData.Post(OrderData))
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

//        [ResponseType(typeof(TrnSaleInvoice))]
//        [Route("")]
//        [AcceptVerbs("PUT")]
//        public HttpResponseMessage Put(TrnSaleInvoice OrderData)
//        {

//            try
//            {
//                if (SaleInvoiceData.Put(OrderData))
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


//        [ResponseType(typeof(TrnSaleInvoice))]
//        [Route("")]
//        [AcceptVerbs("Delete")]
//        public HttpResponseMessage Remove(int id)
//        {
//            try
//            {
//                if (SaleInvoiceData.Delete(id))
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
