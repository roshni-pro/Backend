using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/VANTransaction")]
    public class VANTransactionController : ApiController
    {
        [Route("GetVANTransactionList")]
        [HttpPost]
        public List<VANTransactionDC> GetVantransationList(VanDC vanDC)
        {
            using (var myContext = new AuthContext())
            {
                List<VANTransactionDC> vANTransactionDCs = new List<VANTransactionDC>();
                CustomersManager customersManager = new CustomersManager();
                vANTransactionDCs = customersManager.GetVantransationList(vanDC);
                //vANTransactionDCs = myContext.Database.SqlQuery<VANTransactionDC>("GetVANCustomerList").ToList();
                //if (vANTransactionDCs != null && vANTransactionDCs.Any())
                //{
                //    return vANTransactionDCs;
                //}
                return vANTransactionDCs;
            }
        }


        [Route("GetSubVANTransactionList")]
        [HttpGet]
        public List<VANOrderListDc> GetVantransationList(long Id)
        {
            using (var myContext = new AuthContext())
            {
                var ParamId = new SqlParameter("@Id", Id);
                List<VANOrderListDc> vANOrderList = new List<VANOrderListDc>();
                vANOrderList = myContext.Database.SqlQuery<VANOrderListDc>("GetVANOrderList @Id", ParamId).ToList();
                if (vANOrderList != null && vANOrderList.Any())
                {
                    return vANOrderList;
                }
                return vANOrderList;
            }
        }

        [Route("GetRTGSpaymentReconcilationlist")]
        [HttpPost]
        public List<GetRTGSpaymentReconcilationlistDC> GetRTGSpaymentReconcilationlist(GetListRTGSDc getListRTGSDc)
        {
            using (var myContext = new AuthContext())
            {
                List<GetRTGSpaymentReconcilationlistDC> vANResponseList = new List<GetRTGSpaymentReconcilationlistDC>();
                int Skiplist = (getListRTGSDc.skip - 1) * getListRTGSDc.take;
                DataTable dt = new DataTable();
                dt.Columns.Add("IntValue");
                foreach (var item in getListRTGSDc.WarehouseIds)
                {
                    var dr = dt.NewRow();
                    dr["IntValue"] = item;
                    dt.Rows.Add(dr);
                }                
                var Keytype = new SqlParameter("@keytype", getListRTGSDc.keytype);
                var Warehouseids = new SqlParameter
                {
                    ParameterName = "WarehouseId",
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.IntValues",
                    Value = dt
                };
                var Skip = new SqlParameter("@Skip", Skiplist);
                var Take = new SqlParameter("@Take", getListRTGSDc.take);
                var startDate = new SqlParameter("@StartDate", getListRTGSDc.StartDate ?? (object)DBNull.Value);
                var endDate = new SqlParameter("@EndDate", getListRTGSDc.EndDate ?? (object)DBNull.Value);

                vANResponseList = myContext.Database.SqlQuery<GetRTGSpaymentReconcilationlistDC>("GetRTGSpaymentReconcilationlist @keytype,@WarehouseId,@Skip,@Take,@StartDate,@EndDate", Keytype, Warehouseids, Skip, Take, startDate,endDate).ToList();
                if (vANResponseList != null && vANResponseList.Any())
                {
                    return vANResponseList;
                }
                return vANResponseList;
            }
        }
    }
    public class GetListRTGSDc
    {
        public string keytype { get; set; }
        public List<int> WarehouseIds { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}










