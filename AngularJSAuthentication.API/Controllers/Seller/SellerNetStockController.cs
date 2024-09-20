using AgileObjects.AgileMapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.Seller
{
    [RoutePrefix("api/SellerNetStockController")]
    public class SellerNetStockController : ApiController
    {
        [Authorize]
        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetWarehouseSubcatId(int WarehouseId)
        {
            List<SellerNetStockDc> result = new List<SellerNetStockDc>();

            var SubCatId = Convert.ToInt32(Request.Headers.GetValues("SubCatId").First());

            if (SubCatId > 0 && WarehouseId > 0)
            {
                List<CurrentStockDTOM> ObjCurrentNetstock = new List<CurrentStockDTOM>();

                using (AuthContext context = new AuthContext())
                {
                    var param = new SqlParameter
                    {
                        ParameterName = "WarehouseId",
                        Value = WarehouseId
                    };
                    var param1 = new SqlParameter
                    {
                        ParameterName = "SubCatId",
                        Value = SubCatId
                    };

                    ObjCurrentNetstock = context.Database.SqlQuery<CurrentStockDTOM>("exec [SellerNetStock] @WarehouseId ,@SubCatId", param, param1).ToList();
                   result = Mapper.Map(ObjCurrentNetstock).ToANew<List<SellerNetStockDc>>();

                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

    }
    public class SellerNetStockDc
    {
        public string WarehouseName { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public int CurrentInventory { get; set; }
        public int CurrentDeliveryCanceledInventory { get; set; }
        public int FreestockNetInventory { get; set; }
        public double? CurrentNetStockAmount { get; set; }
        public double? Unitprice { get; set; }
        public double? PurchasePrice { get; set; }
        public double? MRP { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int IsActive { get; set; }
        public List<CurrentNetDeliveryCancelDTO> DeliveryCancelDetails { get; set; }
        public int OpenPOQTy { get; set; }
        public int AverageAging { get; set; }
        public double? AveragePurchasePrice { get; set; }
        public int CurrentNetInventory { get; set; }
        public int NetInventory { get; set; }
        public double? AgingAvgPurchasePrice { get; set; }
        public double? MarginPercent { get; set; }
        public int ItemlimitQty { get; set; }
        public int ItemLimitSaleQty { get; set; }
        public string ABCClassification { get; set; }


    }
}
