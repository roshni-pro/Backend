using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV1
{
    [RoutePrefix("api/TurnATime")]
    public class TurnATimeController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        [Route("")]
        [AcceptVerbs("Get")]
        public HttpResponseMessage Get(DateTime start, DateTime end, int WarehouseId)
        {
            logger.Info("start WalletList: ");
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;
                    List<OrderMaster> data = new List<OrderMaster>();
                    List<OrderMasterDTO> MainData = new List<OrderMasterDTO>();

                    List<int> CustomerId = new List<int>();

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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }

                    //data = context.DbOrderMaster.Where(x => x.CreatedDate >= start && x.CreatedDate <= end && x.WarehouseId == WarehouseId && (x.DeliveredDate!=null|| x.ReadytoDispatchedDate!=null)).ToList();
                    data = context.DbOrderMaster.Where(x => x.Deliverydate >= start && x.Deliverydate <= end && x.WarehouseId == WarehouseId && (x.DeliveredDate != null || x.ReadytoDispatchedDate != null) && (x.Status == "Delivered" || x.Status == "Ready to Dispatch" || x.Status == "Issued" || x.Status == "Shipped" || x.Status == "sattled" || x.Status == "Account settled" || x.Status == "Partial settled" || x.Status == "Partial receiving -Bounce")).ToList();
                    foreach (var aa in data)
                    {

                        List<OrderMasterHistories> orderdatedata = context.OrderMasterHistoriesDB.Where(x => x.orderid == aa.OrderId).ToList();

                        foreach (var itm in orderdatedata)
                        {
                            OrderMasterDTO ssd = new OrderMasterDTO();
                            ssd.Status = itm.Status;
                            ssd.CreatedDate = itm.CreatedDate;


                        }

                        double ReadytoDdiffhours = 0;
                        double ReadytoDelivereddiffhours = 0;
                        double Deliverydiffhours = 0;

                        try
                        {
                            TimeSpan Readydiffhours = aa.ReadytoDispatchedDate.Value.Subtract(aa.CreatedDate);
                            ReadytoDdiffhours = Readydiffhours.TotalHours;
                        }
                        catch (Exception ex) { }

                        try
                        {
                            TimeSpan ReadytoDeliveredhours = aa.DeliveredDate.Value.Subtract(aa.ReadytoDispatchedDate.Value);
                            ReadytoDelivereddiffhours = ReadytoDeliveredhours.TotalHours;


                        }
                        catch (Exception) { }


                        try
                        {
                            TimeSpan diff = aa.DeliveredDate.Value.Subtract(aa.CreatedDate);
                            Deliverydiffhours = diff.TotalHours;


                        }
                        catch (Exception) { }


                        if (ReadytoDdiffhours > 0 || ReadytoDelivereddiffhours > 0 || Deliverydiffhours > 0)
                        {

                            OrderMasterDTO od = new OrderMasterDTO();
                            od.OrderId = aa.OrderId;
                            od.Status = aa.Status;
                            od.CreatedDate = aa.CreatedDate;
                            od.ReadytoDdiffhours = ReadytoDdiffhours;
                            od.ReadytoDelivereddiffhours = ReadytoDelivereddiffhours;
                            od.Deliverydiffhours = Deliverydiffhours;
                            od.Skcode = aa.Skcode;
                            od.ShippingAddress = aa.ShippingAddress;
                            od.GrossAmount = aa.GrossAmount;
                           // od.CustomerName = aa.CustomerName;
                           // od.SalesPerson = aa.SalesPerson;
                            od.ReDispatchCount = aa.ReDispatchCount;
                            od.Deliverydate = aa.Deliverydate;
                            od.CreatedDate = aa.CreatedDate;
                            od.UpdatedDate = aa.UpdatedDate;
                            od.Delivered_Time = Convert.ToDateTime(aa.DeliveredDate);//Final Delivered Date required

                            if (orderdatedata != null && orderdatedata.Any(x => x.Status == "Ready to Dispatch"))
                            {
                                od.ReadyToDistpatch_Time = orderdatedata.FirstOrDefault(x => x.Status == "Ready to Dispatch").CreatedDate;
                            }
                            if (orderdatedata != null && orderdatedata.Any(x => x.Status == "Issued"))
                            {
                                od.Issued_Time = orderdatedata.FirstOrDefault(x => x.Status == "Issued").CreatedDate;
                            }
                            if (orderdatedata != null && orderdatedata.Any(x => x.Status == "Shipped"))
                            {
                                od.Shipped_Time = orderdatedata.FirstOrDefault(x => x.Status == "Shipped").CreatedDate;
                            }

                            if (orderdatedata != null && orderdatedata.Any(x => x.Status == "Delivery Redispatch"))
                            {
                                od.FirstRedispatch_Time = orderdatedata.FirstOrDefault(x => x.Status == "Delivery Redispatch").CreatedDate;
                                if (orderdatedata.Where(x => x.Status == "Delivery Redispatch").Count() >= 2)
                                {
                                    od.SecondRedispatch_Time = orderdatedata.Where(x => x.Status == "Delivery Redispatch").Skip(1).First().CreatedDate;
                                }

                                if (orderdatedata.Where(x => x.Status == "Delivery Redispatch").Count() >= 3)
                                {
                                    od.ThirdRedispatch_Time = orderdatedata.Where(x => x.Status == "Delivery Redispatch").Skip(2).First().CreatedDate;
                                }
                            }

                            MainData.Add(od);




                        }



                        logger.Info("End  wallet: ");

                    }
                    return Request.CreateResponse(HttpStatusCode.OK, MainData);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in WalletList " + ex.Message);
                    logger.Info("End  WalletList: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
    }
}



