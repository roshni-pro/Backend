using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.DataContracts.External;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.Reporting
{
    [RoutePrefix("api/SpillOverOrders")]
    public class SpillOverOrdersController : ApiController
    {
        // GET: SpillOverOrders
        [Route("GetSpillOverOrdersList")]
        [HttpPost]
        public List<SpillOverOrderDC> GetSpillOverOrdersList(SpillOverOrderFilter spillOverOrderFilter)
        {
            using (AuthContext db = new AuthContext())
            {
                List<SpillOverOrderDC> SpillOverOrderDCList = new List<SpillOverOrderDC>();
                var manager = new SpillOverOrderManager();
                SpillOverOrderDCList = manager.GetSpillOverOrdersList(spillOverOrderFilter);
                return SpillOverOrderDCList;
            }
        }



        [Route("ExportSpillOver")]
        [HttpPost]
        public List<ExportDataNewDC> ExportSpillOverData(SpillOverOrderFilter spillOverOrderFilter)
        {
            using (AuthContext db = new AuthContext())
            {
                List<ExportDataNewDC> ExportData = new List<ExportDataNewDC>();


                DataTable dt = new DataTable();
                dt.Columns.Add("IntValue");

                foreach (var item in spillOverOrderFilter.WarehouseIds)
                {
                    var dr = dt.NewRow();
                    dr["IntValue"] = item;
                    dt.Rows.Add(dr);

                }
                var warehouseId = new SqlParameter("WarehouseIds", dt);
                warehouseId.SqlDbType = SqlDbType.Structured;
                warehouseId.TypeName = "dbo.IntValues";
                var startDate = new SqlParameter("SelectedStartDate", spillOverOrderFilter.SelectedStartDate);
                var endDate = new SqlParameter("SelectedEndDate", spillOverOrderFilter.SelectedEndDate);
                ExportData = db.Database.SqlQuery<ExportDataNewDC>("Exec ExportSpillOver @WarehouseIds,@SelectedStartDate,@SelectedEndDate", warehouseId, startDate,endDate).ToList();
                return ExportData;
            }
            
        }
    }

}

