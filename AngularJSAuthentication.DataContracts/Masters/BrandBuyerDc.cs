using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class BrandBuyerDc
    {

        public long Id { get; set; }
        public int WarehosueId { get; set; }
        public int BuyerId { get; set; }
        public int BrandId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }


    }

    public class BrandBuyerDetailDc
    {
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
    }

    public class BuyerWiseBrandList
    {
        public int SubsubCategoryid { get; set; }
        public string SubsubcategoryName { get; set; }
    }
    public class BrandBuyerDetailSearchDc
    {
        public List<int> BrandIds { get; set; }
        public List<int> WarehouseIds { get; set; }
    }
}

