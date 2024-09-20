using AngularJSAuthentication.DataContracts.NonRevenueOrderDc;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.NonRevenueOrders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/NonRevenue")]

    public class NonRevenueOrderController : ApiController
    {
        [Route("GetAllNonRevenueSettelmentOrders")]
        [HttpPost]
        public List<ReturnNonRevenueSettelmentOrders> GetAllNonRevenueSettelmentOrders(NonRevenueSettelmentOrders Items)
        {
            int userid = 0;
            var identity = User.Identity as ClaimsIdentity;

            List<string> roleNames = new List<string>();

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            var res = new List<ReturnNonRevenueSettelmentOrders>();
            using (var context = new AuthContext())
            {

                if (Items != null && userid > 0)
                {
                    var idlist = new DataTable();
                    idlist.Columns.Add("IntValue");
                    foreach (var item in Items.WarehouseIds)
                    {
                        var dr = idlist.NewRow();
                        dr["IntValue"] = item;
                        idlist.Rows.Add(dr);
                    }
                    var param = new SqlParameter("@WarehouseIds", idlist);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";

                    var CustType = new DataTable();
                    CustType.Columns.Add("stringValues");
                    foreach (var item in Items.CustomerType)
                    {
                        var dr = CustType.NewRow();
                        dr["stringValues"] = item;
                        CustType.Rows.Add(dr);
                    }

                    var param3 = new SqlParameter("@CustomerType", CustType);
                    param3.SqlDbType = SqlDbType.Structured;
                    param3.TypeName = "dbo.stringValues";

                    var Status = new DataTable();
                    Status.Columns.Add("stringValues");
                    foreach (var item in Items.Status)

                    {
                        var dr = Status.NewRow();
                        dr["stringValues"] = item;
                        Status.Rows.Add(dr);
                    }

                    var param4 = new SqlParameter("@Status", Status);
                    param4.SqlDbType = SqlDbType.Structured;
                    param4.TypeName = "dbo.stringValues";

                    var param5 = new SqlParameter("@Keyword", Items.Keyword ?? (object)DBNull.Value);
                    var param6 = new SqlParameter("@Skip", Items.Skip);
                    var param7 = new SqlParameter("@Take", Items.Take);
                    var param8 = new SqlParameter("@FromDate", Items.FromDate ?? (object)DBNull.Value);
                    var param9 = new SqlParameter("@ToDate", Items.ToDate ?? (object)DBNull.Value);
                    //Sp_NonRevenueDetails
                    //NonRevenueOrderSettelmentOrders
                    res = context.Database.SqlQuery<ReturnNonRevenueSettelmentOrders>("Exec Sp_NonRevenueDetails @WarehouseIds,@CustomerType,@Status,@Keyword,@Skip,@Take,@FromDate,@ToDate", param, param3, param4, param5, param6, param7, param8, param9).ToList();

                }

            }
            return res;

        }


        //    [Route("SettelmentsOrders")]
        //    [HttpPost]
        //    public async Task<ResponseMsg> SettelmentsOrders(int Orderid)
        //    {
        //        int userid = 0;
        //        var identity = User.Identity as ClaimsIdentity;

        //        List<string> roleNames = new List<string>();

        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
        //            roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

        //        var Result=new ResponseMsg();
        //        using (var context = new AuthContext())
        //        {
        //            if (roleNames.Any(x => x == "HQ IC Executive" || x == "IC executive"))
        //            {
        //                if (Orderid > 0 && userid > 0)
        //                {
        //                    var Order = context.DbOrderMaster.Where(x => x.OrderId == Orderid && x.Status == "Delivered").FirstOrDefault();
        //                    Order.Status = "sattled";
        //                    Order.UpdatedDate= DateTime.Now;
        //                    context.Entry(Order).State = EntityState.Modified;
        //                    if (context.Commit() > 0)
        //                    {
        //                        Result.Status = true;
        //                        Result.Message = "Approved Successfully";
        //                    }
        //                    else
        //                    {
        //                        Result.Status = false;
        //                        Result.Message = "Something Went Wrong";
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                Result.Status = false;
        //                Result.Message = "You are not Authorized";
        //            }
        //        }
        //        return Result;
        //    }



        [Route("CancelOrders")]
        [HttpGet]
        public async Task<ResponseMsg> CancelOrders(int Orderid)
        {
            int userid = 0;
            var identity = User.Identity as ClaimsIdentity;

            List<string> roleNames = new List<string>();

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            var Result = new ResponseMsg();
            using (var context = new AuthContext())
            {
                if (roleNames.Any(x => x == "HQ IC Executive" || x == "IC executive" || x == "HQ Master login"))
                {
                    if (Orderid > 0 && userid > 0)
                    {
                        var Order = context.DamageOrderMasterDB.FirstOrDefault(x => x.DamageOrderId == Orderid && x.Status == "Pending");
                        if (Order != null)
                        {
                            Order.Status = "Order Canceled";
                            Order.UpdatedDate = DateTime.Now;
                            Order.Updateby = userid;
                            context.Entry(Order).State = EntityState.Modified;

                            if (context.Commit() > 0)
                            {
                                Result.Status = true;
                                Result.Message = "Order Cancelled";
                            }
                            else
                            {
                                Result.Status = false;
                                Result.Message = "Something Went Wrong";
                            }
                        }
                        else
                        {
                            Result.Status = false;
                            Result.Message = "No data found";
                        }


                    }
                }
                else
                {
                    Result.Status = false;
                    Result.Message = "You are not Authorized";
                }
            }
            return Result;
        }
    }


}
