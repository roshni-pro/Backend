using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.ForCast
{
    public class BuyersSummaryDC
    {
        
            public string Buyers { get; set; }

            public string SubCategory { get; set; }

            public double Value { get; set; }

            public int PeopleID { get; set; }

            public int SubCategoryId { get; set; }

           // public string Department { get; set; }

        //  public string Category { get; set; }
 
        // public double Value { get; set; }

    }
    public class PDCADC
    {
        public string Department { get; set; }

        public string Category { get; set; }

        public double Value { get; set; }

        public int BaseCategoryId { get; set; }

        public int Categoryid { get; set; }
        public double Percentage { get; set; }

    }

    public class SingleMapview
    {
        public List<BuyersSummaryDC> BuyersSummaryDC { get; set; }
        public List<PDCADC> PDCADC { get; set; }
    }

    public class SalesGroupMatrixElasticOrder
    {      
            public double dispatchqty { get; set; }
            public double price { get; set; }
            public int executiveid { get; set; }
            public int storeid { get; set; }
            public int clusterid { get; set; }        
    }
    public class SalesGroupMatrixDC
    {
        public List<SalesMatrixDC> SalesMatrixDCs { get; set; }
        public List<GroupMatrixDC> GroupMatrixDCs { get; set; }
        public int Level { get; set; }
    }
    public class SalesMatrixDC
    {

        public int TotalSales { get; set; }
        public int OrderCount { get; set; }
        public double AvgLineItem { get; set; }
        public int PendingOrder { get; set; }
        public string Skcode { get; set; }
        public string ShippingAddress { get; set; }
        public string Mobile { get; set; }
        public double lat { get; set; }

        public double lg { get; set; }

        public string ShopName { get; set; }


    }

    public class GroupMatrixDC
    {
        public int StoreId { get; set; }
        public string Name { get; set; }

        public int OrderId { get; set; }
        public int TotalSales { get; set; }


    }



}
