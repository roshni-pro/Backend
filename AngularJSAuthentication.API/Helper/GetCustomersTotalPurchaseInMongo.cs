using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.Mongo;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class GetCustomersTotalPurchaseInMongo
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public TCSResponse GetCustomersTotalPurchaseForTCS(int CustomerId,  string Pan, AuthContext context)
        {

            DateTime TodayDate = indianTime;
            DateTime FinancialStartDate = new DateTime(TodayDate.Year, 04, 01);
            DateTime FinancialEndDate = new DateTime(TodayDate.Year + 1, 03, 31);
            if (TodayDate < FinancialStartDate)
            {
                FinancialStartDate = new DateTime(TodayDate.Year - 1, 04, 01);
                FinancialEndDate = new DateTime(TodayDate.Year, 03, 31);
            }
            TCSResponse res = new TCSResponse();
            double PendingOrderAmount = 0;
           
            List<int> CustomersPanLists = !string.IsNullOrEmpty(Pan)? context.Customers.Where(x => x.PanNo == Pan).Select(x => x.CustomerId).ToList():new List<int> ();
            CustomersPanLists.Add(CustomerId);

            if(CustomersPanLists != null && CustomersPanLists.Count() > 0)
            {
                MongoDbHelper<AngularJSAuthentication.DataContracts.Transaction.Mongo.MongoOrderMaster> MongoHelper = new MongoDbHelper<AngularJSAuthentication.DataContracts.Transaction.Mongo.MongoOrderMaster>();
                var Data = MongoHelper.Select(x => CustomersPanLists.Contains(x.CustomerId) && x.TCSAmount > 0 && x.CreatedDate >= FinancialStartDate && x.CreatedDate <= FinancialEndDate, collectionName: "OrderMaster").FirstOrDefault();
                
                res.IsAlreadyTcsUsed = Data != null ? true : false;
            }
            if (CustomersPanLists != null && CustomersPanLists.Any())
            {
                string fy = (indianTime.Month >= 4 ? indianTime.Year + 1 : indianTime.Year).ToString();
                MongoDbHelper<TCSConfig> mConfigHelper = new MongoDbHelper<TCSConfig>();
                var tcsConfig = mConfigHelper.Select(x => x.FinancialYear == fy).FirstOrDefault();

                MongoDbHelper<TCSCustomer> mHelper = new MongoDbHelper<TCSCustomer>();
                var tcsCustomer = mHelper.Select(x => CustomersPanLists.Contains(x.CustomerId) && x.FinancialYear == fy).ToList();

                if (tcsConfig != null)
                {

                    MongoDbHelper<AngularJSAuthentication.DataContracts.Transaction.Mongo.MongoOrderMaster> Helper = new MongoDbHelper<AngularJSAuthentication.DataContracts.Transaction.Mongo.MongoOrderMaster>();
                    var orderPredicate = PredicateBuilder.New<AngularJSAuthentication.DataContracts.Transaction.Mongo.MongoOrderMaster>(x => CustomersPanLists.Contains(x.CustomerId) && x.Status == "Pending" && x.CreatedDate >= FinancialStartDate && x.CreatedDate <= FinancialEndDate);
                    var orderMasters = new List<AngularJSAuthentication.DataContracts.Transaction.Mongo.MongoOrderMaster>();
                    var totalGrossAmount = Helper.GetWithProjection(orderPredicate
                        ,x=>x.GrossAmount
                        , collectionName: "OrderMaster").Sum(x=>x);
                    PendingOrderAmount = totalGrossAmount;

                    res.TotalPurchase = tcsCustomer != null ? tcsCustomer.Sum(x => x.TotalPurchase) : 0;
                    res.TCSAmountLimit = tcsConfig.TCSAmountLimit;
                    res.NotGSTTCSPercent = tcsConfig.NotGSTTCSPercent;
                    res.GSTTCSPercent = tcsConfig.GSTTCSPercent;
                    res.PendingOrderAmount = PendingOrderAmount;
                    return res;
                }
            }
            return res;

        }
    }

    public class TCSResponse
    {
        public double PendingOrderAmount { get; set; }
        public double NotGSTTCSPercent { get; set; }
        public double GSTTCSPercent { get; set; }
        public double TCSAmountLimit { get; set; }
        public double TotalPurchase { get; set; }
        public bool IsAlreadyTcsUsed { get; set; }
    }
}