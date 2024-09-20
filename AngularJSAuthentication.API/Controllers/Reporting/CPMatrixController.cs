using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.Transaction;
using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using AngularJSAuthentication.DataContracts.Transaction.Mongo.CpMatrix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.Reporting
{
    [RoutePrefix("api/CpMatrix")]
    public class CPMatrixController : BaseAuthController
    {
        [HttpGet]
        [Route("Calculate/{month}/{year}")]
        [AllowAnonymous]
        public async Task<bool> CalculateCpMatrixForMonth(int month, int year)
        {
            MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
            var startDate = new DateTime(year, month, 1);
            var endDate = new DateTime(year, month, DateTime.DaysInMonth(year, month)).Date.AddDays(1).AddSeconds(-1);

            var orders = mongoDbHelper.Select(x => x.CreatedDate >= startDate && x.CreatedDate < endDate
                                                   && (x.Status == "Ready to Dispatch" || x.Status == "Post Order Canceled"
                                                        || x.Status == "Delivery Redispatch"
                                                        || x.Status == "Account settled" || x.Status == "Delivered"
                                                        || x.Status == "Delivery Canceled"
                                                        || x.Status == "Issued"
                                                        || x.Status == "Partial settled"
                                                        || x.Status == "Shipped"
                                                        || x.Status == "sattled"
                                                        || x.Status == "Pending")
                                                    && !x.Deleted, collectionName: "OrderMaster").ToList();

            if (orders != null && orders.Any())
            {
                var orderDetails = orders.SelectMany(x => x.orderDetails);
                var groupedItems = orderDetails.GroupBy(x => new
                {
                    CustomerId = x.CustomerId,
                    ItemMultiMrpId = x.ItemMultiMRPId,
                    ItemName = x.itemname,
                    ItemNumber = x.itemNumber
                }).Select((x, index) => new CpMatrixGroupedItems
                {
                    Id = index,
                    CustomerId = x.Key.CustomerId,
                    ItemMultiMrpId = x.Key.ItemMultiMrpId,
                    ItemName = x.Key.ItemName,
                    ItemNumber = x.Key.ItemNumber,
                    TotalPrice = x.Sum(z => z.UnitPrice * z.qty)
                }).OrderByDescending(x => x.TotalPrice).ToList();

                var sale = groupedItems.Sum(x => x.TotalPrice) / 2;

                groupedItems = groupedItems.Where(x => groupedItems.Where(y => y.Id <= x.Id).Sum(y => y.TotalPrice) <= sale).ToList();


                var cpMatrix = new CustomerProductMatrix { Month = month, Year = year, ProductList = new List<ProductList>() };
                var itemMultiMrpIds = groupedItems.Select(z => z.ItemMultiMrpId + "_" + z.CustomerId);


                cpMatrix.ProductList = orderDetails.Where(x => itemMultiMrpIds.Contains(x.ItemMultiMRPId + "_" + x.CustomerId))
                .GroupBy(x => new
                {
                    x.ItemMultiMRPId,
                    x.itemname,
                    x.itemNumber
                })
                .Select(z => new ProductList
                {
                    ItemMultiMrpId = z.Key.ItemMultiMRPId,
                    ItemName = z.Key.itemname,
                    ItemNumber = z.Key.itemNumber,
                    OrderAmount = z.Sum(a => a.UnitPrice * a.qty),
                    Customers = z.GroupBy(a => new
                    {
                        a.CustomerId,
                    })
                    .Select(a => new CustomerDetail
                    {
                        CustomerId = a.Key.CustomerId,
                        CustomerName = orders.FirstOrDefault(w => w.CustomerId == a.Key.CustomerId).CustomerName,
                        SkCode = orders.FirstOrDefault(w => w.CustomerId == a.Key.CustomerId).Skcode,
                        ShopName = orders.FirstOrDefault(w => w.CustomerId == a.Key.CustomerId).ShopName,
                        WarehouseId = orders.FirstOrDefault(w => w.CustomerId == a.Key.CustomerId).WarehouseId,
                        WarehouseName = orders.FirstOrDefault(w => w.CustomerId == a.Key.CustomerId).WarehouseName,
                        OrderAmount = a.Sum(s => s.UnitPrice * s.qty)
                    }).OrderByDescending(a => a.OrderAmount).ToList()

                })
                .OrderByDescending(a => a.OrderAmount).ToList();


                MongoDbHelper<CustomerProductMatrix> cpMatrixMongoHelper = new MongoDbHelper<CustomerProductMatrix>();
                await cpMatrixMongoHelper.InsertAsync(cpMatrix);
            }


            return true;
        }

        [HttpGet]
        [Route("Get/{month}/{year}")]
        [AllowAnonymous]
        public async Task<List<CpMatrixProductDetailDc>> GetMonthCpMatrix(int month, int year)
        {
            List<CpMatrixProductDetailDc> returnList = new List<CpMatrixProductDetailDc>();
            MongoDbHelper<CustomerProductMatrix> cpMatrixMongoHelper = new MongoDbHelper<CustomerProductMatrix>();

            var cpMatrixList = cpMatrixMongoHelper.Select(x => x.Month == month && x.Year == year).SelectMany(z => z.ProductList)
                .OrderByDescending(x => x.OrderAmount).Skip(0).Take(100).ToList();

            cpMatrixList.ForEach(x =>
            {
                var customers = x.Customers.OrderByDescending(z => z.OrderAmount).Skip(0).Take(50).ToList();
                x.Customers = customers;
            });

            var customerIds = cpMatrixList.SelectMany(x => x.Customers.Select(z => z.CustomerId)).ToList();

            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endDate = DateTime.Now.Date.AddDays(1).Date;


            MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();

            var orders = mongoDbHelper.Select(x => x.CreatedDate >= startDate && x.CreatedDate < endDate
                                                   && (x.Status == "Ready to Dispatch" || x.Status == "Post Order Canceled"
                                                        || x.Status == "Delivery Redispatch"
                                                        || x.Status == "Account settled" || x.Status == "Delivered"
                                                        || x.Status == "Delivery Canceled"
                                                        || x.Status == "Issued"
                                                        || x.Status == "Partial settled"
                                                        || x.Status == "Shipped"
                                                        || x.Status == "sattled"
                                                        || x.Status == "Pending")
                                                    && !x.Deleted && customerIds.Contains(x.CustomerId), collectionName: "OrderMaster").ToList();



            var orderDetails = orders != null && orders.Any() ? orders.SelectMany(x => x.orderDetails.Select(z => z)).ToList() : null;
            var groupedItems = orderDetails != null && orderDetails.Any() ? orderDetails.GroupBy(x => new
            {
                x.CustomerId,
                ItemMultiMrpId = x.ItemMultiMRPId,
            })
            .Select(z => new CpMatrixGroupedItemsDc
            {
                CustomerId = z.Key.CustomerId,
                ItemMultiMrpId = z.Key.ItemMultiMrpId,
                Delta = z.Sum(a => a.UnitPrice * a.qty)
            })
            .ToList() : null;


            returnList = cpMatrixList.Select(x => new CpMatrixProductDetailDc
            {
                ItemMultiMrpId = x.ItemMultiMrpId,
                ItemName = x.ItemName,
                ItemNumber = x.ItemNumber,
                OrderAmount = x.OrderAmount,
                Customers = x.Customers.OrderByDescending(a => a.OrderAmount).Skip(0).Take(20).Select(z => new CustomerDetailDc
                {
                    CustomerId = z.CustomerId,
                    CustomerName = z.CustomerName,
                    OrderAmount = z.OrderAmount,
                    ShopName = z.ShopName,
                    SkCode = z.SkCode,
                    WarehouseId = z.WarehouseId,
                    WarehouseName = z.WarehouseName,
                    Delta = groupedItems != null && groupedItems.Any() ?
                                groupedItems.FirstOrDefault(a => a.CustomerId == z.CustomerId && a.ItemMultiMrpId == x.ItemMultiMrpId)?.Delta ?? 0
                                : 0
                }).ToList()
            }).ToList();




            return returnList;
        }

    }
}