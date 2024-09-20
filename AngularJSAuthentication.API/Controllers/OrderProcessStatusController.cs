using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/OrderStatusUpdate")]
    public class OrderProcessStatusController : ApiController
    {
       
        public static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
        [HttpPost]
        public object post(int OrderId, string status)
        {

            logger.Info("start OrderProcess: ");
            using (var db = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0, Warehouse_id = 0;
                // Access claims
                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                }
                try
                {
                    OrderMaster om = db.DbOrderMaster.Where(x => x.OrderId == OrderId).SingleOrDefault();
                    om.Status = status;
                    db.Commit();

                    OrderDispatchedMaster ODM = db.OrderDispatchedMasters.Where(x => x.OrderId == OrderId).SingleOrDefault();
                    ODM.Status = status;
                    db.Commit();


                    #region Order Master History for Status Post Order Canceled  

                    OrderDispatchedMaster ODM1 = db.OrderDispatchedMasters.Where(x => x.OrderId == OrderId).SingleOrDefault();
                    var UserName = db.Peoples.Where(x => x.PeopleID == userid).Select(a => a.DisplayName).SingleOrDefault();


                    try
                    {
                        OrderMasterHistories h1 = new OrderMasterHistories();
                        if (ODM1 != null)
                        {
                            h1.orderid = ODM1.OrderId;
                            h1.Status = ODM1.Status;
                            h1.Reasoncancel = null;
                            h1.Warehousename = ODM1.WarehouseName;
                            h1.username = UserName;
                            h1.userid = userid;
                            h1.CreatedDate = DateTime.Now;
                            db.OrderMasterHistoriesDB.Add(h1);
                            db.Commit();
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error loading  \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                    }
                    #endregion
                    List<OrderDetails> ODetails = db.DbOrderDetails.Where(x => x.OrderId == OrderId).ToList();
                    foreach (var st in ODetails)
                    {
                        st.Status = status;
                        db.Commit();
                    }

                    //List<OrderDispatchedDetails> O_Dis_Details = db.OrderDispatchedDetailss.Where(x => x.OrderId == OrderId).ToList();
                    //foreach (var st in O_Dis_Details)
                    //{
                    //    st.Status = status;
                    //    db.Commit();
                    //}
                    var data = db.DbOrderMaster.Where(s => s.OrderId == OrderId).SingleOrDefault();
                    return (data);

                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderProcess  " + ex.Message);
                    logger.Info("End  OrderProcess: ");
                    return null;
                }

            }
        }
    }
}
