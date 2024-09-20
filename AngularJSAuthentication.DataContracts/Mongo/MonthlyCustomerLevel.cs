using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class MonthlyCustomerLevel
    {
        public ObjectId Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public List<CustLevel> CustomerLevels { get; set; }
    }

    public class CustLevel
    {
        public int CustomerId { get; set; }
        public string SKCode { get; set; }
        public int LevelId { get; set; }
        public string LevelName { get; set; }
        public string ColourCode { get; set; }
    }
}