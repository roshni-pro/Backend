using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.Inbound
{
    [RoutePrefix("api/InboundPage")]
    public class InboundPageController : ApiController
    {
        [HttpPost]
        [Route("GetInboundDashboard")]
        public InboundMainDC InboundDashboard(DashBoardParamater DashBoardPara)
        {
            using (AuthContext authcontext = new AuthContext())
            {
                InboundMainDC inboundDashTB = new InboundMainDC();
                List<InboundDashboardTableDC> TableData = new List<InboundDashboardTableDC>();
                //List<InboundDashboardDC> statisticsData = new List<InboundDashboardDC>();
                if (authcontext.Database.Connection.State != ConnectionState.Open)
                    authcontext.Database.Connection.Open();
                var wIds = new DataTable();
                wIds.Columns.Add("IntValue");
                //if(DashBoardPara.WhId == null)
                //{
                //    WhId=[]
                //}
                //if (DashBoardPara.StoreId == null)
                //{
                //    WhId =[]
                //}
                foreach (var item in DashBoardPara.WhId)
                {
                    var dr = wIds.NewRow();
                    dr["IntValue"] = item;
                    wIds.Rows.Add(dr);
                }

                var storeids = new DataTable();
                storeids.Columns.Add("IntValue");
                foreach (var item in DashBoardPara.StoreId)
                {
                    var dr = storeids.NewRow();
                    dr["IntValue"] = item;
                    storeids.Rows.Add(dr);
                }
                var param = new SqlParameter
                {
                    ParameterName = "WhId",
                    Value = wIds
                };
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                var storeparam = new SqlParameter
                {
                    ParameterName = "StoreId",
                    Value = storeids
                };
                storeparam.SqlDbType = SqlDbType.Structured;
                storeparam.TypeName = "dbo.IntValues";
                var cmd = authcontext.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 800;
                cmd.CommandText = "[dbo].[GetInboundDashbordData]"; // new change for freight/weight/buyername/itemnumber
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);
                cmd.Parameters.Add(storeparam);
                cmd.Parameters.Add(new SqlParameter("@SearchDate", DashBoardPara.SearchDate));
                
                var reader = cmd.ExecuteReader();
                TableData = ((IObjectContextAdapter)authcontext).ObjectContext.Translate<InboundDashboardTableDC>(reader).ToList();

                reader.NextResult();

                inboundDashTB.inboundDashboardTableDC = TableData;
                if (reader.Read())
                {
                    inboundDashTB.TdStockIn = Convert.ToDouble(reader["TdStockIn"]);
                    inboundDashTB.MtdStockIn = Convert.ToDouble(reader["MtdStockIn"]);
                    inboundDashTB.TdStockOut = Convert.ToDouble(reader["TdStockOut"]);
                    inboundDashTB.MtdStockOut = Convert.ToDouble(reader["MtdStockOut"]);
                    inboundDashTB.TdDamage = Convert.ToDouble(reader["TdDamage"]);
                    inboundDashTB.MtdDamage = Convert.ToDouble(reader["MtdDamage"]);
                    inboundDashTB.TdExpiy = Convert.ToDouble(reader["TdExpiy"]);
                    inboundDashTB.MtdExpiy = Convert.ToDouble(reader["MtdExpiy"]);
                }
                return inboundDashTB;
            }
        }
        [HttpPost]
        [Route("stockAging")]
        public List<ResponseStockAgingDC> StockAgingData(ShlefLifeParameters ShlefLifeParam)
        {
            using (AuthContext context = new AuthContext())
            {
                List<ResponseStockAgingDC> inboundshelfLife = new List<ResponseStockAgingDC>();

                var wIds = new DataTable();
                wIds.Columns.Add("IntValue");
                foreach (var item in ShlefLifeParam.warehouseid)
                {
                    var dr = wIds.NewRow();
                    dr["IntValue"] = item;
                    wIds.Rows.Add(dr);
                }

                var storeids = new DataTable();
                storeids.Columns.Add("IntValue");
                foreach (var item in ShlefLifeParam.storeids)
                {
                    var dr = storeids.NewRow();
                    dr["IntValue"] = item;
                    storeids.Rows.Add(dr);
                }
                var param = new SqlParameter
                {
                    ParameterName = "warehouseid",
                    Value = wIds
                };
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                var storeparam = new SqlParameter
                {
                    ParameterName = "stores",
                    Value = storeids
                };
                storeparam.SqlDbType = SqlDbType.Structured;
                storeparam.TypeName = "dbo.IntValues";
                var SearchDate = new SqlParameter("@SearchDate", ShlefLifeParam.SearchDate);
                context.Database.CommandTimeout = 800;
                inboundshelfLife = context.Database.SqlQuery<ResponseStockAgingDC>("Exec GetInboundDashboard_shelfLifeData @warehouseid,@stores,@SearchDate", param, storeparam,SearchDate).ToList();
                return inboundshelfLife;
            }

        }

        [HttpGet]
        [AllowAnonymous]
        [Route("NEWInventoryHeads")]
        public List<InboundDashboardDC> NEWInventoryHeads(int? warehouseid)
        {
            using (AuthContext context = new AuthContext())
            {
                List<InboundDashboardDC> inboundDashboard = new List<InboundDashboardDC>();

                var currentdate = DateTime.Now;


                //var date = new SqlParameter("@date", dates ?? (object)DBNull.Value);


                var warehoueseid = new SqlParameter("@warehouseids", warehouseid ?? (object)DBNull.Value);


                inboundDashboard = context.Database.SqlQuery<InboundDashboardDC>("Exec InboundHeading @warehouseids,@date", warehoueseid).ToList();
                return inboundDashboard;
            }
        }

        [HttpPost]
        [Route("InboundDamage")]
        public List<InboundDamageDC> InboundDamage(DamageParameters DamagePara)
        {
            using (AuthContext context = new AuthContext())
            {
                List<InboundDamageDC> inboundDamage = new List<InboundDamageDC>();

                var wIds = new DataTable();
                wIds.Columns.Add("IntValue");
                foreach (var item in DamagePara.warehouseid)
                {
                    var dr = wIds.NewRow();
                    dr["IntValue"] = item;
                    wIds.Rows.Add(dr);
                }

                var storeids = new DataTable();
                storeids.Columns.Add("IntValue");
                foreach (var item in DamagePara.storeids)
                {
                    var dr = storeids.NewRow();
                    dr["IntValue"] = item;
                    storeids.Rows.Add(dr);
                }
                var param = new SqlParameter
                {
                    ParameterName = "warehouseid",
                    Value = wIds
                };
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                var storeparam = new SqlParameter
                {
                    ParameterName = "stores",
                    Value = storeids
                };
                var SearchDate = new SqlParameter("@SearchDate", DamagePara.SearchDate);
                storeparam.SqlDbType = SqlDbType.Structured;
                storeparam.TypeName = "dbo.IntValues";
                context.Database.CommandTimeout = 800;
                inboundDamage = context.Database.SqlQuery<InboundDamageDC>("Exec GetInboundDashbord_DamageData @warehouseid,@stores,@SearchDate", param, storeparam, SearchDate).ToList();
                return inboundDamage;
            }
        }


        [HttpPost]
        [Route("InboundNonSellable")]
        public List<InboundNonSellableDC> InboundNonSellable(NonSellableParameters nonSellablePara)
        {
            using (AuthContext context = new AuthContext())
            {
                List<InboundNonSellableDC> InboundNonSellable = new List<InboundNonSellableDC>();


                var wIds = new DataTable();
                wIds.Columns.Add("IntValue");
                foreach (var item in nonSellablePara.warehouseid)
                {
                    var dr = wIds.NewRow();
                    dr["IntValue"] = item;
                    wIds.Rows.Add(dr);
                }

                var storeids = new DataTable();
                storeids.Columns.Add("IntValue");
                foreach (var item in nonSellablePara.storeids)
                {
                    var dr = storeids.NewRow();
                    dr["IntValue"] = item;
                    storeids.Rows.Add(dr);
                }
                var param = new SqlParameter
                {
                    ParameterName = "warehouseid",
                    Value = wIds
                };
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                var storeparam = new SqlParameter
                {
                    ParameterName = "stores",
                    Value = storeids
                };
                var SearchDate = new SqlParameter("@SearchDate", nonSellablePara.SearchDate);
                storeparam.SqlDbType = SqlDbType.Structured;
                storeparam.TypeName = "dbo.IntValues";
                try
                {
                    context.Database.CommandTimeout = 800;
                    InboundNonSellable = context.Database.SqlQuery<InboundNonSellableDC>("Exec GetInboundDashbord_NonSellebleData @warehouseid,@stores,@SearchDate", param, storeparam, SearchDate).ToList();
                    return InboundNonSellable;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        [HttpPost]
        [Route("InboundClearance")]
        public List<InboundClearanceDC> InboundClearance(ClearanceParameters clearancePara)
        {
            using (AuthContext context = new AuthContext())
            {
                List<InboundClearanceDC> InboundClearance = new List<InboundClearanceDC>();


                var wIds = new DataTable();
                wIds.Columns.Add("IntValue");
                foreach (var item in clearancePara.warehouseid)
                {
                    var dr = wIds.NewRow();
                    dr["IntValue"] = item;
                    wIds.Rows.Add(dr);
                }

                var storeids = new DataTable();
                storeids.Columns.Add("IntValue");
                foreach (var item in clearancePara.storeids)
                {
                    var dr = storeids.NewRow();
                    dr["IntValue"] = item;
                    storeids.Rows.Add(dr);
                }
                var param = new SqlParameter
                {
                    ParameterName = "warehouseid",
                    Value = wIds
                };
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                var storeparam = new SqlParameter
                {
                    ParameterName = "stores",
                    Value = storeids
                };
                storeparam.SqlDbType = SqlDbType.Structured;
                storeparam.TypeName = "dbo.IntValues";
                var SearchDate = new SqlParameter("@SearchDate", clearancePara.SearchDate);
                try
                {
                    context.Database.CommandTimeout = 800;
                    InboundClearance = context.Database.SqlQuery<InboundClearanceDC>("Exec GetInboundDashbord_ClearanceData @warehouseid,@stores,@SearchDate", param, storeparam, SearchDate).ToList();
                    return InboundClearance;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        [HttpPost]
        [Route("InboundInventory")]
        public List<InboundNetInventoryDC> InboundClearance(InventoryParameters inventoryParam)
        {
            using (AuthContext context = new AuthContext())
            {
                List<InboundNetInventoryDC> InboundNetInventory = new List<InboundNetInventoryDC>();


                var wIds = new DataTable();
                wIds.Columns.Add("IntValue");
                foreach (var item in inventoryParam.warehouseid)
                {
                    var dr = wIds.NewRow();
                    dr["IntValue"] = item;
                    wIds.Rows.Add(dr);
                }

                var storeids = new DataTable();
                storeids.Columns.Add("IntValue");
                foreach (var item in inventoryParam.storeids)
                {
                    var dr = storeids.NewRow();
                    dr["IntValue"] = item;
                    storeids.Rows.Add(dr);
                }
                var param = new SqlParameter
                {
                    ParameterName = "warehouseid",
                    Value = wIds
                };
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                var storeparam = new SqlParameter
                {
                    ParameterName = "stores",
                    Value = storeids
                };
                storeparam.SqlDbType = SqlDbType.Structured;
                storeparam.TypeName = "dbo.IntValues";
                var SearchDate = new SqlParameter("@SearchDate", inventoryParam.SearchDate);
                try
                {
                    context.Database.CommandTimeout = 800;
                    InboundNetInventory = context.Database.SqlQuery<InboundNetInventoryDC>("Exec GetInboundDashbord_InventoryData @warehouseid,@stores,@SearchDate", param, storeparam, SearchDate).ToList();
                    return InboundNetInventory;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

    }
    public class DashBoardParamater
    {
        public List<int> WhId { get; set; }
        public List<int> StoreId { get; set; }
        public DateTime SearchDate { get; set; }

    }
    public class DamageParameters
    {
        public List<int> warehouseid { get; set; }
        public List<int> storeids { get; set; }
        public DateTime SearchDate { get; set; }

    }
    public class ShlefLifeParameters
    {
        public List<int> warehouseid { get; set; }
        public List<int> storeids { get; set; }
        public DateTime SearchDate { get; set; }

    }
    public class NonSellableParameters
    {
        public List<int> warehouseid { get; set; }
        public List<int> storeids { get; set; }

        public DateTime SearchDate { get; set; }
    }
    public class ClearanceParameters
    {
        public List<int> warehouseid { get; set; }
        public List<int> storeids { get; set; }
        public DateTime SearchDate { get; set; }
    }
    public class InventoryParameters
    {
        public List<int> warehouseid { get; set; }
        public List<int> storeids { get; set; }
        public DateTime SearchDate { get; set; }
    }
    public class InboundMainDC
    {
        public double TdStockIn { get; set; }
        public double MtdStockIn { get; set; }
        public double TdStockOut { get; set; }
        public double MtdStockOut { get; set; }
        public double TdDamage { get; set; }
        public double MtdDamage { get; set; }
        public double TdExpiy { get; set; }
        public double MtdExpiy { get; set; }
        public List<InboundDashboardTableDC> inboundDashboardTableDC { get; set; }



    }
    public class InboundDashboardDC
    {
        public double TdStockIn { get; set; }
        public double MtdStockIn { get; set; }
        public double TdStockOut { get; set; }
        public double MtdStockOut { get; set; }
        public double TdDamage { get; set; }
        public double MtdDamage { get; set; }
        public double TdExpiy { get; set; }
        public double MtdExpiy { get; set; }
    }
    public class InboundDashboardTableDC
    {
        public string KPIdesc { get; set; }
        public double YTD { get; set; }
        public double MTD { get; set; }
        public double Td { get; set; }
    }

    public class ResponseStockAgingDC
    {
        public string shelflife { get; set; }
        public int CountOfItem { get; set; }
        public double Amount { get; set; }
        public double PerOfStock { get; set; }

    }
    public class InboundNetInventoryDC
    {
        public string SubCategoryName { get; set; }
        public int CurrentInvCountOfItem { get; set; }
        public double CurrentInvAmount { get; set; }
        public double PerOfCurrentStock { get; set; }
        public double NetAmountDDPer { get; set; }
        public double NetAmount { get; set; }
        public int NetInvCountOfItem { get; set; }
    }
    public class InboundCurrentInventoryDC
    {
        public string cat { get; set; }
        public int countofitem { get; set; }
        public int amount { get; set; }
        public int perofStock { get; set; }
    }
    public class InboundDamageDC
    {
        public string brand { get; set; }
        public int? countofitems { get; set; }
        public double? Amount { get; set; }
        public double? perofDamage { get; set; }

    }
    public class InboundNonSellableDC
    {
        public string BrandName { get; set; }
        public int? CountOfItems { get; set; }
        public double? Amount { get; set; }
        public double? perofNonSellable { get; set; }
    }
    public class SearchInboundDC
    {
        public List<int> warehouseIdss { get; set; }
        public List<int> storeIds { get; set; }
        public string sectionName { get; set; }
    }
    public class InboundInventory
    {
        public string SubCategoryName { get; set; }
        public int CurrentInvCountOfItem { get; set; }
        public int NetInvCountOfItem { get; set; }
        public double CurentInvAmount { get; set; }
        public double CurentPercOfstock { get; set; }
        public double NetInventoryAmount { get; set; }
        public double NetPercOfStock { get; set; }


    }
    public class InboundClearanceDC
    {
        public string BrandName { get; set; }
        public int CountOfItem { get; set; }
        public double Amount { get; set; }
        public double perofClearance { get; set; }
    }

}







