using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/BrandBuyer")]
    public class BrandBuyerController : ApiController
    {
        [Route("GetDataTable")]
        [HttpGet]
        public DataTable GetDataTable()
        {
            string connString = ConfigurationManager.ConnectionStrings["AuthContext"].ToString();
            DataTable dataTable = new DataTable();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand("GetBrandBuyers", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(dataTable);
                conn.Close();
            }
            return dataTable;
        }



        [Route("UpdateBrandBuyer/wid/{warehouseId}/brandId/{brandId}/buyerId/{buyerId}/IsSetAllWarehouse/{IsSetAllWarehouse}")]
        [HttpPost]
        public DataTable UpdateBrandBuyer(int warehouseId, int brandId, int buyerId, bool IsSetAllWarehouse, List<int> warehouseIds)
        {
            using (var context = new AuthContext())
            {
                if (IsSetAllWarehouse)
                {
                    var warehouselist = context.Warehouses.Where(e => e.active == true && e.IsKPP == false && warehouseIds.Contains(e.WarehouseId)).Select(a => a.WarehouseId).ToList();
                    foreach (int wid in warehouselist)
                    {
                        var brandBuyer = context.BrandBuyerDB.FirstOrDefault(x => x.BrandId == brandId && x.WarehosueId == wid);
                        if (brandBuyer == null)
                        {
                            BrandBuyer BrandBuyerdata = new BrandBuyer();
                            BrandBuyerdata.WarehosueId = wid;
                            BrandBuyerdata.BrandId = brandId;
                            BrandBuyerdata.BuyerId = buyerId;
                            BrandBuyerdata.CreatedDate = DateTime.Now;
                            BrandBuyerdata.UpdateDate = DateTime.Now;
                            BrandBuyerdata.Deleted = false;
                            BrandBuyerdata.Active = true;
                            context.BrandBuyerDB.Add(BrandBuyerdata);
                            context.Commit();

                        };
                        if (brandBuyer != null)
                        {
                            brandBuyer.WarehosueId = wid;
                            brandBuyer.BrandId = brandId;
                            brandBuyer.BuyerId = buyerId;
                            brandBuyer.UpdateDate = DateTime.Now;
                            context.Entry(brandBuyer).State = EntityState.Modified;
                            context.Commit();
                        };
                    }
                }
                else
                {
                    var brandBuyer = context.BrandBuyerDB.FirstOrDefault(x => x.BrandId == brandId && x.WarehosueId == warehouseId);
                    if (brandBuyer == null)
                    {
                        BrandBuyer BrandBuyerdata = new BrandBuyer();
                        BrandBuyerdata.WarehosueId = warehouseId;
                        BrandBuyerdata.BrandId = brandId;
                        BrandBuyerdata.BuyerId = buyerId;
                        BrandBuyerdata.CreatedDate = DateTime.Now;
                        BrandBuyerdata.UpdateDate = DateTime.Now;
                        BrandBuyerdata.Deleted = false;
                        BrandBuyerdata.Active = true;
                        context.BrandBuyerDB.Add(BrandBuyerdata);
                        context.Commit();

                    };
                    if (brandBuyer != null)
                    {
                        brandBuyer.WarehosueId = warehouseId;
                        brandBuyer.BrandId = brandId;
                        brandBuyer.BuyerId = buyerId;
                        brandBuyer.UpdateDate = DateTime.Now;
                        context.Entry(brandBuyer).State = EntityState.Modified;
                        context.Commit();
                    };
                }
            }
            return null;
        }

        [Route("search")]
        [HttpPost]
        public DataTable search(SearchItems searchItem)
        {
            string connString = ConfigurationManager.ConnectionStrings["AuthContext"].ToString();
            DataTable dataTable = new DataTable();

            var sscat = string.Join(",", searchItem.subcategorylist);
            var warehouse = string.Join(",", searchItem.whouseList);

            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand("GetBrandBuyersSearch", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Warehouseids", SqlDbType.VarChar).Value = warehouse;
                cmd.Parameters.Add("@Brandids", SqlDbType.VarChar).Value = sscat;
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(dataTable);
                conn.Close();
            }
            return dataTable;
        }


        [Route("BrandBuyerDetail")]
        [HttpPost]
        public async Task<List<BrandBuyerDetailDc>> GetBrandBuyerDetail(BrandBuyerDetailSearchDc BrandBuyerDetailSearch)
        {
            List<BrandBuyerDetailDc> result = new List<BrandBuyerDetailDc>();
            if (BrandBuyerDetailSearch != null && BrandBuyerDetailSearch.WarehouseIds.Any() && BrandBuyerDetailSearch.BrandIds.Any())
            {

                using (AuthContext context = new AuthContext())
                {
                    var BrandIdList = new DataTable();
                    BrandIdList.Columns.Add("IntValue");
                    foreach (var item in BrandBuyerDetailSearch.BrandIds)
                    {
                        var dr = BrandIdList.NewRow();
                        dr["IntValue"] = item;
                        BrandIdList.Rows.Add(dr);
                    }
                    var BrandIds = new SqlParameter("BrandIds", BrandIdList);
                    BrandIds.SqlDbType = SqlDbType.Structured;
                    BrandIds.TypeName = "dbo.IntValues";

                    var WarehouseIdList = new DataTable();
                    WarehouseIdList.Columns.Add("IntValue");
                    foreach (var item in BrandBuyerDetailSearch.WarehouseIds)
                    {
                        var dr = WarehouseIdList.NewRow();
                        dr["IntValue"] = item;
                        WarehouseIdList.Rows.Add(dr);
                    }
                    var WarehouseIds = new SqlParameter("WarehouseIds", WarehouseIdList);
                    WarehouseIds.SqlDbType = SqlDbType.Structured;
                    WarehouseIds.TypeName = "dbo.IntValues";
                    result = await context.Database.SqlQuery<BrandBuyerDetailDc>("exec Buyer.GetBrandBuyerDetail @BrandIds, @WarehouseIds ", BrandIds, WarehouseIds).ToListAsync();
                }
            }
            return result;
        }

        [Route("GetBuyerBrandList")]
        [HttpPost]
        public  DataTable GetBuyerBrandList(warehouseidDc Wids)
        {
            string connString = ConfigurationManager.ConnectionStrings["AuthContext"].ToString();
            DataTable dataTable = new DataTable();
            using (var db = new AuthContext())
            {
                if (Wids.warehouseids != null)
                {
                    if (Wids.warehouseids.Count > 0)
                    {
                        string warehouse = string.Join(",", Wids.warehouseids);
                        using (SqlConnection conn = new SqlConnection(connString))
                        {
                            SqlCommand cmd = new SqlCommand("SpGetBuyerBrandList", conn);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@Warehouseids", SqlDbType.VarChar).Value = warehouse;
                            conn.Open();
                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            // this will query your database and return the result to your datatable
                            da.Fill(dataTable);
                            conn.Close();
                        }
                    }
                }
            }
            return dataTable;
        }

        [Route("GetBuyerWiseBrands")]
        [HttpPost]
        public async Task<APIResponse> GetBuyerBrandList(BuyerWarehouseIdDc Bids)
        {
            APIResponse res = new APIResponse();
            List<BuyerWiseBrandList> result = new List<BuyerWiseBrandList>();
            if (Bids!=null && Bids.warehouseids.Any() && Bids.buyerIds.Any())
            {
                using (AuthContext context = new AuthContext())
                {
                    var BuyerIdList = new DataTable();
                    BuyerIdList.Columns.Add("IntValue");
                    foreach (var item in Bids.buyerIds)
                    {
                        var dr = BuyerIdList.NewRow();
                        dr["IntValue"] = item;
                        BuyerIdList.Rows.Add(dr);
                    }
                    var BuyerIds = new SqlParameter("BuyerIds", BuyerIdList);
                    BuyerIds.SqlDbType = SqlDbType.Structured;
                    BuyerIds.TypeName = "dbo.IntValues";

                    var WarehouseIdList = new DataTable();
                    WarehouseIdList.Columns.Add("IntValue");
                    foreach (var item in Bids.warehouseids)
                    {
                        var dr = WarehouseIdList.NewRow();
                        dr["IntValue"] = item;
                        WarehouseIdList.Rows.Add(dr);
                    }
                    var WarehouseIds = new SqlParameter("WarehouseIds", WarehouseIdList);
                    WarehouseIds.SqlDbType = SqlDbType.Structured;
                    WarehouseIds.TypeName = "dbo.IntValues";
                    res.Data = await context.Database.SqlQuery<BuyerWiseBrandList>("exec NewDFRBandList  @WarehouseIds,@Buyerids ",  WarehouseIds, BuyerIds).ToListAsync();
                    res.Status = true;
                }
            }
            else
            {
                res.Status = false;
                res.Message = "parameters cant be empty";
            }
            return res;
        }
    }

    public class SearchItems
    {
        public List<int> subcategorylist { get; set; }
        public List<int> whouseList { get; set; }

    }
    public class warehouseidDc
    {
        public List<int> warehouseids { get; set; }
    }

    public class BuyerWarehouseIdDc
    {
        public List<int> warehouseids { get; set; }
        public List<int> buyerIds { get; set; }
    }
}