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
//    [RoutePrefix("api/SaleOrder")]
//    public class SalesOrderController : ApiController
//    {
//        public class MYTrnSaleOrderDetail:TrnSaleOrderDetail
//        {

//        public string ProductName { get; set; }


//        }

//        public class MYTrnSaleOrder : TrnSaleOrder
//        {
//            public string Country { get; set; }
//            public string State { get; set; }
//            public string City { get; set; }
//            public string WarehouseName { get; set; }

//            public MstCustomer customer = new MstCustomer();
//            public List<TrnSaleOrderDetail> TrnSaleOrderDetails1 = new List<TrnSaleOrderDetail>();
//        }
//        Transaction.SaleOrder SaleOrderData = new Transaction.SaleOrder();
//        Data.Product ProductData = new Data.Product();
//        Data.Customer CustomerData = new Data.Customer();
//        Data.Locality LocationData = new Data.Locality();
//        Data.Warehouse WarehouseData = new Data.Warehouse();
//        [Route("")]
//        public HttpResponseMessage Get()
//        {
//            try
//            {
//                //List<MYTrnSaleOrder> trn=new List<>
//                List<TrnSaleOrder> _data = SaleOrderData.Get();
//                List<MYTrnSaleOrder> MyData = new List<MYTrnSaleOrder>();
//                foreach(var orderdata in _data)
//                {
//                    MYTrnSaleOrder mysaldata = new MYTrnSaleOrder();
//                    MstLocality loc = new MstLocality();
//                    MstWarehouse war = new MstWarehouse();
//                    war = WarehouseData.GetById(orderdata.WarehouseId);
//                    loc = LocationData.GetById(orderdata.LocalityId);

//                    mysaldata.AddressLine1 = orderdata.AddressLine1;
//                    mysaldata.AddressLine2 = orderdata.AddressLine2;
//                    mysaldata.AddressLine3 = orderdata.AddressLine3;
//                    mysaldata.AddressLine4 = orderdata.AddressLine4;
//                    mysaldata.DiscountAmount = orderdata.DiscountAmount;
//                    mysaldata.GrossAmount = orderdata.GrossAmount;
//                    mysaldata.InvoiceNumber = orderdata.InvoiceNumber;
//                    mysaldata.PinCode = orderdata.PinCode;
//                    mysaldata.TaxAmount = orderdata.TaxAmount;
//                    mysaldata.TotalAmount = orderdata.TotalAmount;
//                    mysaldata.WarehouseName = war.Name;
//                    mysaldata.CustomerId = orderdata.CustomerId;
//                    mysaldata.LocalityId = orderdata.LocalityId;
//                    mysaldata.Country = loc.Country;
//                    mysaldata.State = loc.State;
//                    mysaldata.City = loc.City;
//                    mysaldata.Status = orderdata.Status;
//                    mysaldata.ContactNumber1 = orderdata.ContactNumber1;
//                    mysaldata.ContactNumber2 = orderdata.ContactNumber2;
//                    mysaldata.SaleOrderId = orderdata.SaleOrderId;
//                    MstCustomer cust = CustomerData.GetById(orderdata.CustomerId);
//                    mysaldata.customer.Name = cust.Name;
//                    mysaldata.customer.ContactNumber1 = cust.ContactNumber1;
//                    mysaldata.customer.EmailId = cust.EmailId;
//                    mysaldata.customer.AddressLine1 = cust.AddressLine1;
//                    mysaldata.customer.AddressLine2 = cust.AddressLine2;
//                    mysaldata.customer.AddressLine3 = cust.AddressLine3;
//                    mysaldata.customer.AddressLine4 = cust.AddressLine4;
//                    mysaldata.WarehouseId = orderdata.WarehouseId;

//                    foreach (var detail in orderdata.TrnSaleOrderDetails)
//                    {
//                        MYTrnSaleOrderDetail dt = new MYTrnSaleOrderDetail();

//                        MstProduct mst = new MstProduct();
//                        mst = ProductData.GetById(detail.ProductId);
//                        dt.ProductName = mst.Name;
//                        dt.Amount = detail.Amount;
//                        dt.Quantity = detail.Quantity;
//                        dt.Remark = detail.Remark;
//                        dt.ProductId = detail.ProductId;

//                        mysaldata.TrnSaleOrderDetails1.Add(dt);
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


//        [ResponseType(typeof(TrnSaleOrder))]
//        [Route("")]
//        [AcceptVerbs("POST")]
//        public HttpResponseMessage add(ShoppingCart cart)
//        {
//            try
//            {
//                if (SaleOrderData.Post(cart))
//                {
//                   // Request.CreateResponse(HttpStatusCode.OK, OrderData);
//                    return null;
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

//        [ResponseType(typeof(TrnSaleOrder))]
//        [Route("")]
//        [AcceptVerbs("PUT")]
//        public HttpResponseMessage Put(TrnSaleOrder OrderData)
//        {

//            try
//            {
//                if (SaleOrderData.Put(OrderData))
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


//        [ResponseType(typeof(TrnSaleOrder))]
//        [Route("")]
//        [AcceptVerbs("Delete")]
//        public HttpResponseMessage Remove(int id)
//        {
//            try
//            {
//                if (SaleOrderData.Delete(id))
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
