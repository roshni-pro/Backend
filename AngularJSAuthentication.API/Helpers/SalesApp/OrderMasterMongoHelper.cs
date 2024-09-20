using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Helpers.SalesApp
{
    public class OrderMasterMongoHelper
    {
        public async Task<bool> GetHighesPOForReviewBasket(int CustomerId,int WarehouseId, string SkCode)
        {
            //MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
            //var enddate = DateTime.Now;
            //var startDate = enddate.AddMonths(-9);
            //var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.Status != "Inactive" && x.Status != "Dummy Order Cancelled" && !x.Deleted
            //                                    && x.CustomerId == CustomerId && x.WarehouseId == WarehouseId
            //                                    // && x.orderDetails.Any(y => itemmultiMrpIds.Contains(y.ItemMultiMRPId)
            //                                    // && x.CreatedDate >= startDate && x.CreatedDate <= enddate
            //                                    );
            //var ordercollection = mongoDbHelper.mongoDatabase.GetCollection<MongoOrderMaster>("MongoOrderMaster").AsQueryable();
            //var orderdetails = ordercollection.Where(orderPredicate)
            //                    .SelectMany(t => t.orderDetails, (t, a) => new
            //                    {
            //                        CreatedDate = t.CreatedDate,
            //                        ItemMultiMRPId = a.ItemMultiMRPId,
            //                        Qty = a.qty
            //                    }).Where(x => itemmultiMrpIds.Contains(x.ItemMultiMRPId))
            //                    .ToList();
            return false;
        }
    }
}