using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.DataContracts.Masters.Seller;
using AngularJSAuthentication.Model.Seller;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.Seller
{
    [RoutePrefix("api/SellerLedger")]
    public class SellerLedgerController : ApiController
    {
        [HttpPost]
        [Route("Brand")]
        public async Task<List<SellerLedgerDc>> GetBrandLedger(BrandParamsDc param)
        {
            List<SellerLedgerDc> result = new List<SellerLedgerDc>();
            if (param != null && param.SubCatId > 0)
            {
                param.EndDate = param.EndDate.Date.AddDays(1).AddSeconds(-1);
                var manager = new ItemLedgerManager();
                result = await manager.GetBrandLedgerDataAsync(param.StartDate.Date, param.EndDate, param.CityIds, param.SubCatId);
            }
            return result;
        }

        [HttpPost]
        [Route("Export")]
        public async Task<List<BrandLedgerDetailDc>> GetExportBrandLedgerDetail(BrandParamsDc param)
        {
            List<BrandLedgerDetailDc> result = new List<BrandLedgerDetailDc>();
            if (param != null && param.SubCatId > 0)
            {
                param.EndDate = param.EndDate.Date.AddDays(1).AddSeconds(-1);
                var manager = new ItemLedgerManager();
                result = await manager.GetBrandLedgerDetail(param.StartDate.Date, param.EndDate, param.CityIds, param.SubCatId, param.WarehouseId);
            }
            return result;
        }

        //GetSellerMonthlyChargeMaster 
        [Route("Payment/{SubCatId}/{Month}/{Year}")]
        [HttpGet]
        public async Task<List<SellerMonthlyChargeMasterDc>> GetSellerMonthlyChargeMaster(int SubCatId, int Month, int Year)
        {
            var result = new List<SellerMonthlyChargeMasterDc>();
            using (var context = new AuthContext())
            {
                List<Object> parameters = new List<object>();
                string sqlquery = "exec Seller.GetSellerMonthlyChargeMaster";
                parameters.Add(new SqlParameter("@SubCatId", SubCatId));
                parameters.Add(new SqlParameter("@Month", Month));
                parameters.Add(new SqlParameter("@Year", Year));
                sqlquery = sqlquery + " @SubCatId" + ", @Month" + ", @Year";
                result = await context.Database.SqlQuery<SellerMonthlyChargeMasterDc>(sqlquery, parameters.ToArray()).ToListAsync();
            }
            return result;
        }


        //PaymentDetail Hub wise Detail 
        [Route("PaymentDetail/{SubCatId}/{Month}/{Year}/{ChargeType}")]
        [HttpGet]
        public async Task<List<SellerMonthlyChargeDetailsDc>> GetSellerMonthlyChargeDetail(int SubCatId, int Month, int Year, string ChargeType)
        {
            var result = new List<SellerMonthlyChargeDetailsDc>();
            using (var context = new AuthContext())
            {
                List<Object> parameters = new List<object>();
                string sqlquery = "exec Seller.GetSellerMonthlyChargeDetails";
                parameters.Add(new SqlParameter("@SubCatId", SubCatId));
                parameters.Add(new SqlParameter("@Month", Month));
                parameters.Add(new SqlParameter("@Year", Year));
                parameters.Add(new SqlParameter("@ChargeType", ChargeType));
                sqlquery = sqlquery + " @SubCatId" + ", @Month" + ", @Year" + ", @ChargeType";
                result = await context.Database.SqlQuery<SellerMonthlyChargeDetailsDc>(sqlquery, parameters.ToArray()).ToListAsync();
            }
            return result;
        }

        //Op Hub wise 
        [Route("ExportSellerOPLineItems/{SubCatId}/{Month}/{Year}")]
        [HttpGet]
        public async Task<List<ExportSellerLineItemsDc>> GetExportSellerLineItems(int SubCatId, int Month, int Year)
        {
            var result = new List<ExportSellerLineItemsDc>();
            using (var context = new AuthContext())
            {
                List<Object> parameters = new List<object>();
                string sqlquery = "exec Seller.ExportSellerOPLineItems";
                parameters.Add(new SqlParameter("@SubCatId", SubCatId));
                parameters.Add(new SqlParameter("@Month", Month));
                parameters.Add(new SqlParameter("@Year", Year));
                sqlquery = sqlquery + " @SubCatId" + ", @Month" + ", @Year";
                result = await context.Database.SqlQuery<ExportSellerLineItemsDc>(sqlquery, parameters.ToArray()).ToListAsync();
            }
            return result;
        }
        //Op Hub wise 
        [Route("ExportDelChargeLineItems/{SubCatId}/{Month}/{Year}")]
        [HttpGet]
        public async Task<List<ExportSellerLineItemsDc>> GetExportDelChargeLineItems(int SubCatId, int Month, int Year)
        {
            var result = new List<ExportSellerLineItemsDc>();
            using (var context = new AuthContext())
            {
                List<Object> parameters = new List<object>();
                string sqlquery = "exec Seller.ExportDelChargeLineItems";
                parameters.Add(new SqlParameter("@SubCatId", SubCatId));
                parameters.Add(new SqlParameter("@Month", Month));
                parameters.Add(new SqlParameter("@Year", Year));
                sqlquery = sqlquery + " @SubCatId" + ", @Month" + ", @Year";
                result = await context.Database.SqlQuery<ExportSellerLineItemsDc>(sqlquery, parameters.ToArray()).ToListAsync();
            }
            return result;
        }

        //Seller ActivetedLineItems  and Export
        //Op Hub wise 
        [Route("ActivetedLineItems/{SubCatId}/{Month}/{Year}")]
        [HttpGet]
        public async Task<List<SellerActivetedLineItemDc>> GetActivetedLineItems(int SubCatId, int Month, int Year)
        {
            var result = new List<SellerActivetedLineItemDc>();
            using (var context = new AuthContext())
            {
                List<Object> parameters = new List<object>();
                string sqlquery = "exec Seller.SellerActivetedLineItems";
                parameters.Add(new SqlParameter("@SubCatId", SubCatId));
                parameters.Add(new SqlParameter("@Month", Month));
                parameters.Add(new SqlParameter("@Year", Year));
                sqlquery = sqlquery + " @SubCatId" + ", @Month" + ", @Year";
                result = await context.Database.SqlQuery<SellerActivetedLineItemDc>(sqlquery, parameters.ToArray()).ToListAsync();
            }
            return result;
        }

        //Seller ActivetedLineItems  and Export
        //Op Hub wise 
        [Route("SellerClosingLineItems/{SubCatId}/{Month}/{Year}")]
        [HttpGet]
        public async Task<List<SellerClosingLineItemsDc>> GetSellerClosingLineItems(int SubCatId, int Month, int Year)
        {
            var result = new List<SellerClosingLineItemsDc>();
            using (var context = new AuthContext())
            {
                List<Object> parameters = new List<object>();
                string sqlquery = "exec Seller.SellerClosingLineItems";
                parameters.Add(new SqlParameter("@SubCatId", SubCatId));
                parameters.Add(new SqlParameter("@Month", Month));
                parameters.Add(new SqlParameter("@Year", Year));
                sqlquery = sqlquery + " @SubCatId" + ", @Month" + ", @Year";
                result = await context.Database.SqlQuery<SellerClosingLineItemsDc>(sqlquery, parameters.ToArray()).ToListAsync();
            }
            return result;
        }

    }

}
