//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Data.Entity;
//using AngularJSAuthentication.Model;
//using AngularJSAuthentication.API;

//namespace pos.Model
//{
//    public class Transaction
//    {
//        public class SaleOrder
//        {

//            public List<TrnSaleOrder> Get()
//            {

//                var _data = _context.TrnSaleOrders.ToList();
//                return _data;
//            }
//            public TrnSaleOrder GetById(int Id)
//            {

//                var _data = _context.TrnSaleOrders.Where(x => x.SaleOrderId == Id).SingleOrDefault();
//                return _data;
//            }

//            public bool Post(ShoppingCart sc)
//            {
//                Data.CustomerData customerData = new Data.CustomerData();
//                Data.WarehouseData warehouseData = new Data.WarehouseData();
//                Data.ProductData productData = new Data.ProductData();
//                Transaction.Stock stockData = new Transaction.Stock();
//                Transaction.SaleOrder salesorderdata = new Transaction.SaleOrder();
//             //   Data.TaxGroupAssociation Taxdata = new Data.TaxGroupAssociation();



//                List<IDetail> cart = new List<IDetail>();
//                cart = sc.itemDetails.Where(a => a.qty != 0).Select(a => a).ToList<IDetail>();


//                Customer cust = customerData.GetByMobile(sc.Customerphonenum);
//                Warehouse warehouse = warehouseData.GetById(cust.Warehouseid);
//                TrnSaleOrder objsaleOrder = new TrnSaleOrder();
//                try
//                {
//                    TrnSaleOrder order = new TrnSaleOrder();
//                    List<TrnSaleOrder> ord = salesorderdata.Get();

//                    if (ord.Count > 0) { order = ord.Last(); }
//                    else { order.SaleOrderId = 0; }
//                    objsaleOrder.InvoiceNumber = order.SaleOrderId + 1;
//                    objsaleOrder.CustomerId = cust.CustomerId;

//                    //string[] words = sc.BillingAddress.Split(',');

//                    //objsaleOrder.AddressLine1 = words[0];
//                    //objsaleOrder.AddressLine2 = words[1];
//                    //objsaleOrder.AddressLine3 = words[2];
//                    //objsaleOrder.AddressLine4 = words[3];

//                    //objsaleOrder.ContactNumber1 = cust.ContactNumber1;
//                    //objsaleOrder.ContactNumber2 = cust.ContactNumber2;
//                    objsaleOrder.Date = indianTime;
//                    objsaleOrder.IsActive = true;
//                    objsaleOrder.IsDeleted = false;
//                    //objsaleOrder.LocalityId = cust.LocalityId;
//                    objsaleOrder.Status = "Pending";


//                    double ProductAmount = 0.0;
//                    int TotalProductQty = 0;
//                    double TotalProductAmount = 0;
//                    int TotalTaxPercentage = 0;
//                    List<TrnSaleOrderDetail> trnsalesOrderDetailList = new List<TrnSaleOrderDetail>();
//                    foreach (var tr in cart)
//                    {
//                        TrnStock stock = stockData.GetByWarehouse(tr.ItemId, cust.Warehouseid);
//                        if (stock != null)
//                        {
//                            ItemMaster product = productData.GetById(tr.ItemId);
//                         //   MstTaxGroupAssociation tx = Taxdata.GetByTaxGroupId(product.TaxGroupId);

//                            TrnSaleOrderDetail trnsalesOrderDetail = new TrnSaleOrderDetail();
//                            //trnsalesOrderDetail.SaleOrderId = objsaleOrder.SaleOrderId;
//                            //trnsalesOrderDetail.ProductId = stock.ProductId;
//                            trnsalesOrderDetail.Quantity = tr.qty;
//                            ProductAmount = stock.SalePrice * tr.qty;
//                            trnsalesOrderDetail.Amount = ProductAmount;
//                            trnsalesOrderDetail.IsActive = true;
//                            trnsalesOrderDetail.IsDeleted = false;
//                            trnsalesOrderDetail.ProductId = stock.ProductId;
//                            trnsalesOrderDetail.CreatedBy = "Admin";
//                            trnsalesOrderDetail.CreatedDate = indianTime;
//                            trnsalesOrderDetail.UpdatedBy = "Admin";
//                            trnsalesOrderDetail.UpdatedDate = indianTime;
//                            trnsalesOrderDetailList.Add(trnsalesOrderDetail);

//                            TotalProductQty = TotalProductQty + tr.qty;
//                            TotalProductAmount = TotalProductAmount + ProductAmount;
//                       //     TotalTaxPercentage = TotalTaxPercentage + Convert.ToInt32(tx.Percentage);

//                            //Update Stock 
//                            stock.Quantity = stock.Quantity - tr.qty;
//                            stock.UpdatedDate = indianTime;
//                            _context.TrnStock.Attach(stock);
//                            _context.Entry(stock).State = EntityState.Modified;

//                            int Stockid = _context.SaveChanges();
//                            //if(Stockid==0)
//                            //{
//                            //    return false;
//                            //}
//                        }
//                    }
//                    objsaleOrder.UpdatedDate = indianTime;
//                    objsaleOrder.TotalAmount = TotalProductAmount;
//                    objsaleOrder.TaxAmount = (TotalTaxPercentage * TotalProductAmount) / 100; //Add Tax for Total Amount 
//                    objsaleOrder.DiscountAmount = 0;
//                    objsaleOrder.GrossAmount = (objsaleOrder.TaxAmount + objsaleOrder.TotalAmount) - objsaleOrder.DiscountAmount;

//                    objsaleOrder.TrnSaleOrderDetails = trnsalesOrderDetailList;

//                    objsaleOrder.CreatedDate = indianTime;
//                    _context.TrnSaleOrders.Add(objsaleOrder);
//                    int i = _context.SaveChanges();
//                    if (i == 0)
//                    {
//                        return false;
//                    }
//                    else
//                    {

//                    }
//                    return true;


//                }
//                catch (Exception ex)
//                {

//                }

//                return false;
//            }

//            public bool Put(TrnSaleOrder _mc)
//            {
//                //  using (var _context = new AuthContext())
//                // {

//                _mc.UpdatedDate = indianTime;
//                _context.TrnSaleOrders.Attach(_mc);
//                _context.Entry(_mc).State = EntityState.Modified;

//                int i = _context.SaveChanges();
//                if (i == 1)
//                {
//                    return true;
//                }
//                return false;
//                // }
//            }
//            public bool Delete(int Id)
//            {

//                using (var _context = new AuthContext())
//                {
//                    var _data = _context.TrnSaleOrders.Where(x => x.SaleOrderId == Id).SingleOrDefault();

//                    _data.UpdatedDate = indianTime;
//                    _data.IsDeleted = true;
//                    _context.TrnSaleOrders.Attach(_data);
//                    _context.Entry(_data).State = EntityState.Modified;
//                    int i = _context.SaveChanges();
//                    if (i == 1)
//                    {
//                        return true;
//                    }
//                    return false;
//                }
//            }

//        }
//        public class SaleOrderDetail
//        {
//            public List<TrnSaleOrderDetail> Get()
//            {

//                var _data = _context.TrnSaleOrderDetails.ToList();
//                return _data;
//            }
//            public TrnSaleOrderDetail GetById(int Id)
//            {

//                var _data = _context.TrnSaleOrderDetails.Where(x => x.SaleOrderDetailId == Id).SingleOrDefault();
//                return _data;
//            }

//            public bool Post(TrnSaleOrderDetail _Data)
//            {

//                _Data.CreatedDate = indianTime;
//                _context.TrnSaleOrderDetails.Add(_Data);
//                int i = _context.SaveChanges();
//                if (i == 0)
//                {
//                    return false;
//                }
//                return true;
//            }
//            public bool Put(TrnSaleOrderDetail _mc)
//            {
//                //  using (var _context = new AuthContext())
//                // {

//                _mc.UpdatedDate = indianTime;
//                _context.TrnSaleOrderDetails.Attach(_mc);
//                _context.Entry(_mc).State = EntityState.Modified;

//                int i = _context.SaveChanges();
//                if (i == 1)
//                {
//                    return true;
//                }
//                return false;
//                // }
//            }
//            public bool Delete(int Id)
//            {

//                using (var _context = new AuthContext())
//                {
//                    var _data = _context.TrnSaleOrderDetails.Where(x => x.SaleOrderDetailId == Id).SingleOrDefault();

//                    _data.UpdatedDate = indianTime;
//                    _data.IsDeleted = true;
//                    _context.TrnSaleOrderDetails.Attach(_data);
//                    _context.Entry(_data).State = EntityState.Modified;
//                    int i = _context.SaveChanges();
//                    if (i == 1)
//                    {
//                        return true;
//                    }
//                    return false;
//                }
//            }

//        }

//        public class PurchaseOrder
//        {
//            public List<TrnPurchaseOrder> Get()
//            {

//                var _data = _context.TrnPurchaseOrders.ToList();
//                return _data;
//            }
//            public TrnPurchaseOrder GetById(int Id)
//            {

//                var _data = _context.TrnPurchaseOrders.Where(x => x.PurchaseOrderId == Id).SingleOrDefault();
//                return _data;
//            }

//            public bool Post(TrnPurchaseOrder _Data)
//            {

//                _Data.CreatedDate = indianTime;
//                _context.TrnPurchaseOrders.Add(_Data);
//                int i = _context.SaveChanges();
//                if (i == 0)
//                {
//                    return false;
//                }
//                return true;
//            }
//            public bool Put(TrnPurchaseOrder _mc)
//            {
//                //  using (var _context = new AuthContext())
//                // {

//                _mc.UpdatedDate = indianTime;
//                _context.TrnPurchaseOrders.Attach(_mc);
//                _context.Entry(_mc).State = EntityState.Modified;

//                int i = _context.SaveChanges();
//                if (i == 1)
//                {
//                    return true;
//                }
//                return false;
//                // }
//            }
//            public bool Delete(int Id)
//            {

//                using (var _context = new AuthContext())
//                {
//                    var _data = _context.TrnPurchaseOrders.Where(x => x.PurchaseOrderId == Id).SingleOrDefault();

//                    _data.UpdatedDate = indianTime;
//                    _data.IsDeleted = true;
//                    _context.TrnPurchaseOrders.Attach(_data);
//                    _context.Entry(_data).State = EntityState.Modified;
//                    int i = _context.SaveChanges();
//                    if (i == 1)
//                    {
//                        return true;
//                    }
//                    return false;
//                }
//            }

//        }
//        public class PurchaseOrderDetail
//        {
//            public List<TrnPurchaseOrderDetail> Get()
//            {

//                var _data = _context.TrnPurchaseOrderDetails.ToList();
//                return _data;
//            }
//            public TrnPurchaseOrderDetail GetById(int Id)
//            {

//                var _data = _context.TrnPurchaseOrderDetails.Where(x => x.PurchaseOrderDetailId == Id).SingleOrDefault();
//                return _data;
//            }

//            public bool Post(TrnPurchaseOrderDetail _Data)
//            {

//                _Data.CreatedDate = indianTime;
//                _context.TrnPurchaseOrderDetails.Add(_Data);
//                int i = _context.SaveChanges();
//                if (i == 0)
//                {
//                    return false;
//                }
//                return true;
//            }
//            public bool Put(TrnPurchaseOrderDetail _mc)
//            {
//                //  using (var _context = new AuthContext())
//                // {

//                _mc.UpdatedDate = indianTime;
//                _context.TrnPurchaseOrderDetails.Attach(_mc);
//                _context.Entry(_mc).State = EntityState.Modified;

//                int i = _context.SaveChanges();
//                if (i == 1)
//                {
//                    return true;
//                }
//                return false;
//                // }
//            }
//            public bool Delete(int Id)
//            {

//                using (var _context = new AuthContext())
//                {
//                    var _data = _context.TrnPurchaseOrderDetails.Where(x => x.PurchaseOrderDetailId == Id).SingleOrDefault();

//                    _data.UpdatedDate = indianTime;
//                    _data.IsDeleted = true;
//                    _context.TrnPurchaseOrderDetails.Attach(_data);
//                    _context.Entry(_data).State = EntityState.Modified;
//                    int i = _context.SaveChanges();
//                    if (i == 1)
//                    {
//                        return true;
//                    }
//                    return false;
//                }
//            }

//        }
//        public class SaleInvoice
//        {
//            public List<TrnSaleInvoice> Get()
//            {

//                var _data = _context.TrnSaleInvoices.ToList();
//                return _data;
//            }
//            public TrnSaleInvoice GetById(int Id)
//            {

//                var _data = _context.TrnSaleInvoices.Where(x => x.SaleInvoiceId == Id).SingleOrDefault();
//                return _data;
//            }

//            public bool Post(TrnSaleInvoice _Data)
//            {

//                _Data.UpdatedDate = indianTime;
//                _Data.IsDeleted = false;
//                _Data.IsActive = true;
//                _Data.CreatedDate = indianTime;
//                _context.TrnSaleInvoices.Add(_Data);
//                int i = _context.SaveChanges();
//                if (i == 0)
//                {
//                    return false;
//                }
//                return true;
//            }
//            public bool Put(TrnSaleInvoice _mc)
//            {
//                //  using (var _context = new AuthContext())
//                // {

//                _mc.UpdatedDate = indianTime;
//                _context.TrnSaleInvoices.Attach(_mc);
//                _context.Entry(_mc).State = EntityState.Modified;

//                int i = _context.SaveChanges();
//                if (i == 1)
//                {
//                    return true;
//                }
//                return false;
//                // }
//            }
//            public bool Delete(int Id)
//            {

//                using (var _context = new AuthContext())
//                {
//                    var _data = _context.TrnSaleInvoices.Where(x => x.SaleInvoiceId == Id).SingleOrDefault();

//                    _data.UpdatedDate = indianTime;
//                    _data.IsDeleted = true;
//                    _context.TrnSaleInvoices.Attach(_data);
//                    _context.Entry(_data).State = EntityState.Modified;
//                    int i = _context.SaveChanges();
//                    if (i == 1)
//                    {
//                        return true;
//                    }
//                    return false;
//                }
//            }

//        }
//        public class SaleInvoiceDetail
//        {
//            public List<TrnSaleInvoiceDetail> Get()
//            {

//                var _data = _context.TrnSaleInvoiceDetails.ToList();
//                return _data;
//            }
//            public TrnSaleInvoiceDetail GetById(int Id)
//            {

//                var _data = _context.TrnSaleInvoiceDetails.Where(x => x.SaleInvoiceDetailId == Id).SingleOrDefault();
//                return _data;
//            }

//            public bool Post(TrnSaleInvoiceDetail _Data)
//            {

//                _Data.CreatedDate = indianTime;
//                _context.TrnSaleInvoiceDetails.Add(_Data);
//                int i = _context.SaveChanges();
//                if (i == 0)
//                {
//                    return false;
//                }
//                return true;
//            }
//            public bool Put(TrnSaleInvoiceDetail _mc)
//            {
//                //  using (var _context = new AuthContext())
//                // {

//                _mc.UpdatedDate = indianTime;
//                _context.TrnSaleInvoiceDetails.Attach(_mc);
//                _context.Entry(_mc).State = EntityState.Modified;

//                int i = _context.SaveChanges();
//                if (i == 1)
//                {
//                    return true;
//                }
//                return false;
//                // }
//            }
//            public bool Delete(int Id)
//            {

//                using (var _context = new AuthContext())
//                {
//                    var _data = _context.TrnSaleInvoiceDetails.Where(x => x.SaleInvoiceDetailId == Id).SingleOrDefault();

//                    _data.UpdatedDate = indianTime;
//                    _data.IsDeleted = true;
//                    _context.TrnSaleInvoiceDetails.Attach(_data);
//                    _context.Entry(_data).State = EntityState.Modified;
//                    int i = _context.SaveChanges();
//                    if (i == 1)
//                    {
//                        return true;
//                    }
//                    return false;
//                }
//            }

//        }

//        public class PurchaseInvoice
//        {
//            public List<TrnPurchaseInvoice> Get()
//            {

//                var _data = _context.TrnPurchaseInvoices.ToList();
//                return _data;
//            }
//            public TrnPurchaseInvoice GetById(int Id)
//            {

//                var _data = _context.TrnPurchaseInvoices.Where(x => x.PurchaseInvoiceId == Id).SingleOrDefault();
//                return _data;
//            }

//            public bool Post(TrnPurchaseInvoice _Data)
//            {

//                _Data.UpdatedDate = indianTime;
//                _Data.IsDeleted = false;
//                _Data.IsActive = true;
//                _Data.CreatedDate = indianTime;
//                _context.TrnPurchaseInvoices.Add(_Data);
//                int i = _context.SaveChanges();
//                if (i == 0)
//                {
//                    return false;
//                }
//                return true;
//            }
//            public bool Put(TrnPurchaseInvoice _mc)
//            {
//                //  using (var _context = new AuthContext())
//                // {

//                _mc.UpdatedDate = indianTime;
//                _context.TrnPurchaseInvoices.Attach(_mc);
//                _context.Entry(_mc).State = EntityState.Modified;

//                int i = _context.SaveChanges();
//                if (i == 1)
//                {
//                    return true;
//                }
//                return false;
//                // }
//            }
//            public bool Delete(int Id)
//            {

//                using (var _context = new AuthContext())
//                {
//                    var _data = _context.TrnPurchaseInvoices.Where(x => x.PurchaseInvoiceId == Id).SingleOrDefault();

//                    _data.UpdatedDate = indianTime;
//                    _data.IsDeleted = true;
//                    _context.TrnPurchaseInvoices.Attach(_data);
//                    _context.Entry(_data).State = EntityState.Modified;
//                    int i = _context.SaveChanges();
//                    if (i == 1)
//                    {
//                        return true;
//                    }
//                    return false;
//                }
//            }

//        }
//        public class PurchaseInvoiceDetail
//        {
//            public List<TrnPurchaseInvoiceDetail> Get()
//            {

//                var _data = _context.TrnPurchaseInvoiceDetails.ToList();
//                return _data;
//            }
//            public TrnPurchaseInvoiceDetail GetById(int Id)
//            {

//                var _data = _context.TrnPurchaseInvoiceDetails.Where(x => x.PurchaseInvoiceDetailId == Id).SingleOrDefault();
//                return _data;
//            }
//            public TrnPurchaseInvoiceDetail GetByInvioceId(int Id, int productid)
//            {

//                var _data = _context.TrnPurchaseInvoiceDetails.Where(x => x.PurchaseInvoiceId == Id && x.ProductId == productid).SingleOrDefault();
//                return _data;
//            }
//            public bool Post(TrnPurchaseInvoiceDetail _Data)
//            {

//                _Data.CreatedDate = indianTime;
//                _context.TrnPurchaseInvoiceDetails.Add(_Data);
//                int i = _context.SaveChanges();
//                if (i == 0)
//                {
//                    return false;
//                }
//                return true;
//            }
//            public bool Put(TrnPurchaseInvoiceDetail _mc)
//            {
//                //  using (var _context = new AuthContext())
//                // {

//                _mc.UpdatedDate = indianTime;
//                _context.TrnPurchaseInvoiceDetails.Attach(_mc);
//                _context.Entry(_mc).State = EntityState.Modified;

//                int i = _context.SaveChanges();
//                if (i == 1)
//                {
//                    return true;
//                }
//                return false;
//                // }
//            }
//            public bool Delete(int Id)
//            {

//                using (var _context = new AuthContext())
//                {
//                    var _data = _context.TrnPurchaseInvoiceDetails.Where(x => x.PurchaseInvoiceDetailId == Id).SingleOrDefault();

//                    _data.UpdatedDate = indianTime;
//                    _data.IsDeleted = true;
//                    _context.TrnPurchaseInvoiceDetails.Attach(_data);
//                    _context.Entry(_data).State = EntityState.Modified;
//                    int i = _context.SaveChanges();
//                    if (i == 1)
//                    {
//                        return true;
//                    }
//                    return false;
//                }
//            }

//        }

//        public class Stock
//        {
//            public List<TrnStock> Get()
//            {

//                var _data = _context.TrnStock.Where(x => x.IsDeleted == false).ToList();
//                return _data;
//            }
//            public TrnStock GetById(int Id)
//            {
//                //stock get by Product Id

//                var _data = _context.TrnStock.Where(x => x.ProductId == Id && x.IsDeleted == false).SingleOrDefault();
//                return _data;
//            }

//            public TrnStock GetByWarehouse(int Id, int? WarehouseId)
//            {
//                //stock get by Product Id

//                var _data = _context.TrnStock.Where(x => x.ProductId == Id && x.IsDeleted == false && x.WarehouseId == WarehouseId).SingleOrDefault();
//                return _data;
//            }

//            public bool Post(TrnStock _Data)
//            {

//                var _datastock = _context.TrnStock.Where(x => x.ProductId == _Data.ProductId).ToList();
//                if (_datastock == null)
//                {
//                    _Data.CreatedDate = indianTime;
//                    _Data.UpdatedDate = indianTime;
//                    _context.TrnStock.Add(_Data);
//                    int i = _context.SaveChanges();
//                    if (i == 0)
//                    {
//                        return false;
//                    }
//                }
//                return true;
//            }
//            public bool Put(TrnStock _mc)
//            {

//                _mc.UpdatedDate = indianTime;
//                _context.TrnStock.Attach(_mc);
//                _context.Entry(_mc).State = EntityState.Modified;

//                int i = _context.SaveChanges();
//                if (i == 1)
//                {
//                    return true;
//                }
//                return false;
//            }

//            public bool Delete(int Id)
//            {
//                using (var _context = new AuthContext())
//                {
//                    var _data = _context.TrnStock.Where(x => x.StockId == Id).SingleOrDefault();
//                    _data.UpdatedDate = indianTime;
//                    _data.IsDeleted = true;
//                    _context.TrnStock.Attach(_data);
//                    _context.Entry(_data).State = EntityState.Modified;
//                    int i = _context.SaveChanges();
//                    if (i == 1)
//                    {
//                        return true;
//                    }
//                    return false;
//                }

//            }

//        }

//        public class MoveStock
//        {

//            public bool Post(TrnMoveStock MoveStock)
//            {

//                Transaction.PurchaseInvoice pinvoiceData = new PurchaseInvoice();
//                Transaction.PurchaseInvoiceDetail pinvoiceDetail = new PurchaseInvoiceDetail();
//                //foreach (var _Data in MoveStock)
//                //{
//                var _datastock = _context.TrnStock.Where(x => x.ProductId == MoveStock.ProductId && x.WarehouseId == MoveStock.WarehouseId).SingleOrDefault();
//                if (_datastock == null)
//                {
//                    //Add new Stock

//                    TrnStock stk = new TrnStock();
//                    stk.CategoryId = MoveStock.CategoryId;
//                    stk.CreatedBy = "";
//                    stk.CreatedDate = indianTime;
//                    stk.IsActive = true;
//                    stk.IsDeleted = false;
//                    Data.ProductData productData = new Data.ProductData();
//                    ItemMaster pdata = productData.GetById(MoveStock.ProductId);
//                    if (pdata != null)
//                    {
//                        stk.ProductId = pdata.ItemId;
//                        stk.PurchasePrice = double.Parse(pdata.PurchasePrice.ToString());
//                        stk.Quantity = MoveStock.Quantity;
//                        stk.SalePrice = double.Parse(pdata.SellingPrice.ToString());
//                        //stk.BarCode = pdata.BarCode;
//                    }
//                    stk.UnitId = 1;
//                    stk.WarehouseId = MoveStock.WarehouseId;
//                    stk.UpdatedBy = "sumit";
//                    stk.UpdatedDate = indianTime;

//                    _context.TrnStock.Add(stk);
//                    int i = _context.SaveChanges();

//                    if (i == 0)
//                    {
//                        return false;
//                    }
//                    //deducted  Stock = PurchaseInvoice.Quantity- MoveItem Quantity

//                    var PurchaseInvoice = pinvoiceData.GetById(MoveStock.PurchaseInvoiceId);
//                    var PurchaseInvoiceDetail = pinvoiceDetail.GetByInvioceId(PurchaseInvoice.PurchaseInvoiceId, _datastock.ProductId);
//                    PurchaseInvoiceDetail.Quantity = PurchaseInvoiceDetail.Quantity - MoveStock.Quantity;
//                    _context.TrnPurchaseInvoiceDetails.Attach(PurchaseInvoiceDetail);
//                    _context.Entry(PurchaseInvoiceDetail).State = EntityState.Modified;
//                    int id = _context.SaveChanges();
//                    if (id == 0)
//                    {
//                        return false;
//                    }






//                    return true;
//                }
//                else
//                {
//                    _datastock.Quantity = MoveStock.Quantity;
//                    _datastock.UpdatedDate = indianTime;
//                    _datastock.CreatedBy = "admin";
//                    _context.TrnStock.Attach(_datastock);
//                    _context.Entry(_datastock).State = EntityState.Modified;

//                    int stockUppdateid = _context.SaveChanges();
//                    if (stockUppdateid == 1)
//                    {
//                        var PurchaseInvoice = pinvoiceData.GetById(MoveStock.PurchaseInvoiceId);
//                        var PurchaseInvoiceDetail = pinvoiceDetail.GetByInvioceId(PurchaseInvoice.PurchaseInvoiceId, _datastock.ProductId);
//                        PurchaseInvoiceDetail.Quantity = PurchaseInvoiceDetail.Quantity - MoveStock.Quantity;

//                        _context.TrnPurchaseInvoiceDetails.Attach(PurchaseInvoiceDetail);
//                        _context.Entry(PurchaseInvoiceDetail).State = EntityState.Modified;
//                        int id = _context.SaveChanges();
//                        if (id == 1)
//                        {
//                            return true;
//                        }

//                        return false;
//                    }

//                }

//                // }
//                return true;
//            }
//        }

//    }
//}
