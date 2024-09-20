using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers.External.SalesManApp;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Masters.Store;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Store;
using SqlBulkTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.Store
{
    [RoutePrefix("api/ClusterStoreBeatMapping")]
    public class ClusterStoreBeatMappingController : ApiController
    {

        [Route("GetClusterListByWId/{WarehouseId}")]
        [HttpGet]
        public async Task<List<WarehouseClusterDc>> GetCustomersByClusterId(int WarehouseId)
        {
            var result = new List<WarehouseClusterDc>();
            if (WarehouseId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    string sqlquery = "exec GetClusterListByWId " + WarehouseId;
                    result = await context.Database.SqlQuery<WarehouseClusterDc>(sqlquery).ToListAsync();
                }
            }
            return result;
        }


        [Route("StoreClusterExecutive")]
        [HttpPost]
        public async Task<List<StoreClusterExecutiveDc>> GetStoreClusterExecutives(SearchStoreClusterDc Search)
        {
            List<StoreClusterExecutiveDc> result = new List<StoreClusterExecutiveDc>();
            if (Search != null && Search.ClusterIds.Any() && Search.StoreId > 0)
            {
                using (var context = new AuthContext())
                {
                    var clusterIdList = new DataTable();
                    clusterIdList.Columns.Add("IntValue");
                    foreach (var item in Search.ClusterIds)
                    {
                        var dr = clusterIdList.NewRow();
                        dr["IntValue"] = item;
                        clusterIdList.Rows.Add(dr);
                    }
                    var clIds = new SqlParameter("clusterIds", clusterIdList);
                    clIds.SqlDbType = SqlDbType.Structured;
                    clIds.TypeName = "dbo.IntValues";

                    var StoreId = new SqlParameter("StoreId", Search.StoreId);
                    result = await context.Database.SqlQuery<StoreClusterExecutiveDc>("exec GetStoreClusterExecutives @clusterIds , @StoreId", clIds, StoreId).ToListAsync();
                }
            }
            return result;
        }

    }
}
