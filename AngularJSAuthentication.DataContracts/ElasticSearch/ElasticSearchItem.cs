using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.ElasticSearch
{
    public class ElasticSearchItem
    {
        public int id { get; set; }
        public int itemid { get { return id; } }
        public bool active { get; set; }
        public int basecategoryid { get; set; }
        [Keyword]
        public string basecategoryname { get; set; }
        public int categoryid { get; set; }
        [Keyword]
        public string categoryname { get; set; }
        public int subcategoryid { get; set; }
        [Keyword]
        public string subcategoryname { get; set; }
        public int subsubcategoryid { get; set; }
        [Keyword]
        public string subsubcategoryname { get; set; }
        public int itemlimitqty { get; set; }
        public bool isitemlimit { get; set; }
        [Keyword]
        public string itemnumber { get; set; }
        public int companyid { get; set; }
        public int warehouseid { get; set; }
        [Keyword]
        public string itemname { get; set; }
        public double price { get; set; }
        public double unitprice { get; set; }       
        public string logourl { get; set; }
        public int minorderqty { get; set; }
        public double totaltaxpercentage { get; set; }
        public double? marginpoint { get; set; }
        public int? promoperitems { get; set; }
        public int? dreampoint { get; set; }
        public bool isoffer { get; set; }
        public string unitofquantity { get; set; }
        public string uom { get; set; }
        [Keyword]
        public string itembasename { get; set; }
        public bool deleted { get; set; }
        public bool isflashdealused { get; set; }
        public int itemmultimrpid { get; set; }
        public double netpurchaseprice { get; set; }
        public int billlimitqty { get; set; }
        public bool issensitive { get; set; }
        public bool issensitivemrp { get; set; }
        [Keyword]
        public string hindiname { get; set; }
        public int? offercategory { get; set; }
        public DateTime? offerstarttime { get; set; }
        public DateTime? offerendtime { get; set; }
        public double? offerqtyavaiable { get; set; }
        public double? offerqtyconsumed { get; set; }
        public int? offerid { get; set; }
        public string offertype { get; set; }
        public double? offerwalletpoint { get; set; }
        public int? offerfreeitemid { get; set; }
        public int? freeitemid { get; set; }
        public double? offerpercentage { get; set; }
        public string offerfreeitemname { get; set; }
        public string offerfreeitemimage { get; set; }
        public int? offerfreeitemquantity { get; set; }
        public int? offerminimumqty { get; set; }
        public double? flashdealspecialprice { get; set; }
        public int? flashdealmaxqtypersoncantake { get; set; }
        public double? distributionprice { get; set; }
        public bool distributorshow { get; set; }
        public int itemapptype { get; set; }
        public double? mrp { get; set; }
        public bool isprimeitem { get; set; }
        public decimal primeprice { get; set; }
        public DateTime noprimeofferstarttime { get; set; }
        public DateTime currentstarttime { get; set; }
        public bool isflashdealstart { get; set; }
        [Keyword]
        public string scheme { get; set; }
        [Keyword]
        public string warehousename { get; set; }
        public int cityid { get; set; }
        [Keyword]
        public string cityname { get; set; }
        public double Score { get; set; }
        public double Rating { get; set; }

        public double Margin { get; set; }
        public double DreamPointCal { get; set; }
        public bool isdiscontinued { get; set; }
        [Keyword]
        public string incentiveClassification { get; set; }
        public double? TradePrice { get; set; }
        public double? DistributionMargin { get; set; }
        public double? TradeMargin { get; set; }
        public double? WholesalePrice { get; set; }//

    }


    public class ElasticItemResponse
    {
        public int itemid { get; set; }       
        public bool active { get; set; }
        public int basecategoryid { get; set; }
        [Keyword]
        public string basecategoryname { get; set; }
        public int categoryid { get; set; }
        [Keyword]
        public string categoryname { get; set; }
        public int subcategoryid { get; set; }
        [Keyword]
        public string subcategoryname { get; set; }
        public int subsubcategoryid { get; set; }
        [Keyword]
        public string subsubcategoryname { get; set; }
        public int itemlimitqty { get; set; }
        public bool isitemlimit { get; set; }
        [Keyword]
        public string itemnumber { get; set; }
        public int companyid { get; set; }
        public int warehouseid { get; set; }
        [Keyword]
        public string itemname { get; set; }
        public double price { get; set; }
        public double unitprice { get; set; }
        public string logourl { get; set; }
        public int minorderqty { get; set; }
        public double totaltaxpercentage { get; set; }
        public double? marginpoint { get; set; }
        public int? dreampoint { get; set; }
        public bool isoffer { get; set; }
        public string unitofquantity { get; set; }
        public string uom { get; set; }
        [Keyword]
        public string itembasename { get; set; }
        public bool deleted { get; set; }
        public bool isflashdealused { get; set; }
        public int itemmultimrpid { get; set; }
        public double netpurchaseprice { get; set; }
        public int billlimitqty { get; set; }
        public bool issensitive { get; set; }
        public bool issensitivemrp { get; set; }
        [Keyword]
        public string hindiname { get; set; }
        public int? offercategory { get; set; }
        public DateTime? offerstarttime { get; set; }
        public DateTime? offerendtime { get; set; }
        public double? offerqtyavaiable { get; set; }
        public double? offerqtyconsumed { get; set; }
        public int? offerid { get; set; }
        public string offertype { get; set; }
        public double? offerwalletpoint { get; set; }
        public int? offerfreeitemid { get; set; }
        public int? freeitemid { get; set; }
        public double? offerpercentage { get; set; }
        public string offerfreeitemname { get; set; }
        public string offerfreeitemimage { get; set; }
        public int? offerfreeitemquantity { get; set; }
        public int? offerminimumqty { get; set; }
        public double? flashdealspecialprice { get; set; }
        public int? flashdealmaxqtypersoncantake { get; set; }
        public double? distributionprice { get; set; }
        public bool distributorshow { get; set; }
        public int itemapptype { get; set; }

        public bool isflashdealstart { get; set; }

        [Keyword]
        public string warehousename { get; set; }
        public int cityid { get; set; }
        [Keyword]
        public string cityname { get; set; }
       
        public double Rating { get; set; }

        public double Margin { get; set; }
        public double DreamPointCal { get; set; }
       
    }

}
