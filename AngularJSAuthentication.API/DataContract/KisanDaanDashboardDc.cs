using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.API.DataContract
{
    public class KisanDaanDashboardDc
    {
        public int? WarehouseId { get; set; }
        public string Skcode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }

    }
    public class CustomerKisanDaanDc
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public long OrderId { get; set; }
        public decimal KisanKiranaAmount { get; set; }
        public decimal KisanDanAmount { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string Shopimage { get; set; }
        public DateTime Date { get; set; }
    }


    public class kisandaanSearchDC
    {
        public int total_count { get; set; }
        public decimal Totaldan { get; set; }
        public List<CustomerdashboardDc> Customerdashboard { get; set; }

    }


    public class CustomerdashboardDc
    {
        public int WarehouseId { get; set; }
        public string Skcode { get; set; }
        public long OrderId { get; set; }
        public decimal KisanKiranaAmount { get; set; }
        public decimal KisanDanAmount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string strCreatedDate
        {
            get
            {
                return this.CreatedDate.ToString("yyyy-MM-dd");
            }
        }
        public string ShopName { get; set; }
        public decimal Total { get; set; }

    }


}