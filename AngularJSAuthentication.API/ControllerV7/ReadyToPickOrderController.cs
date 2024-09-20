using AngularJSAuthentication.API.Managers.TripPlanner;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.TripPlanner;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.TripPlanner;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/ReadyToPickOrder")]
    public class ReadyToPickOrderController : BaseApiController
    {
        [HttpGet]
        [Route("InsertReadyToPickHoldOrder")]
        public async Task<HoldOrderOutputDc> InsertReadyToPickHoldOrder(int OrderId, DateTime? Deliverydate)
        {
            DateTime date = DateTime.Now.Date;

            var identity = User.Identity as ClaimsIdentity;
            int userId = 0;
            string username = "";
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userId = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "username"))
                username = identity.Claims.FirstOrDefault(x => x.Type == "username").Value;

            ReadyToPickOrderManager readyToPickOrderManager = new ReadyToPickOrderManager();
            var data = await readyToPickOrderManager.InsertReadyToPickHoldOrder(OrderId, date, userId, username, Deliverydate);
            return data;

        }

        [HttpPost]
        [Route("GetPickOrderList")]
        public TotalPickOrderListDc GetPickOrderList(InputPickOrder inputPickOrder)
        {
            inputPickOrder.date = DateTime.Today;

            using (var db = new AuthContext())
            {
                TotalPickOrderListDc list = new TotalPickOrderListDc();

                if (db.Database.Connection.State != ConnectionState.Open)
                    db.Database.Connection.Open();

                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[dbo].[GetPickOrderList]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@WarehouseId", inputPickOrder.WarehouseId));
                cmd.Parameters.Add(new SqlParameter("@skip", inputPickOrder.skip));
                cmd.Parameters.Add(new SqlParameter("@take", inputPickOrder.take));
                cmd.Parameters.Add(new SqlParameter("@keyword", inputPickOrder.Keyword));
                cmd.Parameters.Add(new SqlParameter("@date", inputPickOrder.date.Value.Date));
                cmd.Parameters.Add(new SqlParameter("@userid", inputPickOrder.userId));

                var reader = cmd.ExecuteReader();
                var data = ((IObjectContextAdapter)db).ObjectContext.Translate<PickOrderListDc>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    list.TotalCount = Convert.ToInt32(reader["TotalCount"]);
                }
                list.PickOrderListDcs = data;
                return list;
            }
        }
    }
}
