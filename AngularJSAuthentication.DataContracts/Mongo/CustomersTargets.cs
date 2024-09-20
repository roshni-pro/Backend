using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class CustomersTargets
    {
        public class MonthlyCustomerTarget
        {
            public ObjectId Id { get; set; }
            public string Skcode { get; set; }
            public int WarehouseId { get; set; }
            public string CityName { get; set; }
            public string Levels { get; set; }
            public decimal Target { get; set; }
            public decimal Volume { get; set; }

            public int? TargetLineItem { get; set; }
            public int? CurrentLineItem { get; set; }
            public double? LineItemMinAmount { get; set; }
            public string Bands { get; set; }
            public DateTime CreatedDate { get; set; }
            public string Source { get; set; }
            public decimal CurrentVolume { get; set; }
            public decimal? PendingVolume { get; set; }
            public bool IsClaimed { get; set; }
            public bool? IsOffer { get; set; }
            public int? OfferType { get; set; } //0-Percent 1=Values
            public int? OfferValue { get; set; }
            public int? OfferId { get; set; }
            public string OfferDesc { get; set; }
            public int? MaxDiscount { get; set; }
            public int? MOVMultiplier { get; set; }
            public List<TargetDivideOnStore> TargetOnStores { get; set; }
            //public int Month { get; set; }
            //public int Year { get; set; }
            //public List<CustTargets> CustomerTargets { get; set; }
            //public List<CustTargets> Level0 { get; set; }
            //public List<CustTargets> Level1 { get; set; }
            //public List<CustTargets> Level2 { get; set; }
            //public List<CustTargets> Level3 { get; set; }
            //public List<CustTargets> Level4 { get; set; }
            //public List<CustTargets> Level5 { get; set; }
        }


        public class TargetDivideOnStore
        {
            public string StoreName { get; set; }
            public long StoreId { get; set; }
            public double Target { get; set; }
            public double CurrentVolume { get; set; }
            public int? TargetLineItem { get; set; }
            public int? CurrentLineItem { get; set; }
        }

       

        public class MonthlyCustomerTarget1
        {
            public ObjectId Id { get; set; }
            public int Month { get; set; }
            public int Year { get; set; }
            public List<CustTargets1> CustomerTargets { get; set; }
        }
        public class CustTargets1
        {
            public int Id { get; set; }
            public string Skcode { get; set; }
            public int WarehouseId { get; set; }
            public string CityName { get; set; }
            public string Levels { get; set; }
            public decimal Target { get; set; }
            public decimal Volume { get; set; }
            public string Bands { get; set; }
            public DateTime CreatedDate { get; set; }
            public string Source { get; set; }
            public decimal CurrentVolume { get; set; }
            public decimal? PendingVolume { get; set; }
            public bool IsClaimed { get; set; }
        }
    }

    public class CustStoreTargets
    {
        public string skcode { get; set; }
        public long StoreId { get; set; }
        public double Target { get; set; }
        public int? TargetLineItem { get; set; }
    }
}
