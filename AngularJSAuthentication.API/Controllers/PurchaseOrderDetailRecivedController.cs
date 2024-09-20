

using AngularJSAuthentication.Model;
using LinqKit;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;
using AngularJSAuthentication.Model.PurchaseOrder;
using System.Data;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/PurchaseOrderDetailRecived")]
    public class PurchaseOrderDetailRecivedController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);


        [Route("")]
        public IEnumerable<PurchaseOrderDetail> Get(string recordtype)
        {
            if (recordtype == "details")
            {
                logger.Info("start PurchaseOrderDetail: ");
                List<PurchaseOrderDetail> ass = new List<PurchaseOrderDetail>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    using (var context = new AuthContext())
                    {
                        ass = context.AllPOdetails(compid).ToList();
                    }
                    logger.Info("End  order: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                    logger.Info("End  PurchaseOrderDetail: ");
                    return null;
                }
            }
            return null;
        }

        #region  GR in Current Stock
        /// <summary>
        /// GR in Current Stock
        /// Confirm GR
        /// </summary>
        /// <param name="pom"></param>
        /// <returns></returns>
        [ResponseType(typeof(PurchaseOrderMaster))]
        [Route("")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage add(PurchaseOrderMaster pom)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
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
                pom.CompanyId = compid;
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                var po = pom.purDetails;
                var count = 0;
                foreach (var a in po)
                {
                    if (count == 0)
                    {
                        if (a.QtyRecived5 != 0)
                            count = 5;
                        else
                        {
                            if (a.QtyRecived4 != 0)
                                count = 4;
                            else
                            {
                                if (a.QtyRecived3 != 0)
                                    count = 3;
                                else
                                {
                                    if (a.QtyRecived2 != 0)
                                        count = 2;
                                    else
                                    {
                                        if (a.QtyRecived1 != 0)
                                            count = 1;
                                    }
                                }
                            }
                        }
                    }
                    else if (count == 1)
                    {
                        if (a.QtyRecived5 != 0)
                            count = 5;
                        else
                        {
                            if (a.QtyRecived4 != 0)
                                count = 4;
                            else
                            {
                                if (a.QtyRecived3 != 0)
                                    count = 3;
                                else
                                {
                                    if (a.QtyRecived2 != 0)
                                        count = 2;
                                }
                            }
                        }
                    }
                    else if (count == 2)
                    {
                        if (a.QtyRecived5 != 0)
                            count = 5;
                        else
                        {
                            if (a.QtyRecived4 != 0)
                                count = 4;
                            else
                            {
                                if (a.QtyRecived3 != 0)
                                    count = 3;
                            }
                        }
                    }
                    else if (count == 3)
                    {
                        if (a.QtyRecived5 != 0)
                            count = 5;
                        else
                        {
                            if (a.QtyRecived4 != 0)
                                count = 4;
                        }
                    }
                    else if (count == 4)
                    {
                        if (a.QtyRecived5 != 0)
                            count = 5;
                    }
                }
                int pomID;
                using (var context = new AuthContext())
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {

                    PurchaseOrderMasterRecived m = new PurchaseOrderMasterRecived();
                    try
                    {
                        var flag = 0;
                        foreach (PurchaseOrderDetailRecived pc in po)
                        {
                            try
                            {
                                if (pc.isDeleted != true)
                                {
                                    var st = context.AddPurchaseOrderDetailsRecived(pc, count);
                                }
                            }
                            catch (Exception ee)
                            {
                                dbContextTransaction.Rollback();
                                logger.Error(ee.Message);
                                return Request.CreateResponse(HttpStatusCode.BadRequest, "got Excepion in Current inventory insertion.");
                            }
                            if ((pc.QtyRecived1 + pc.QtyRecived2 + pc.QtyRecived3 + pc.QtyRecived4 + pc.QtyRecived5) < pc.TotalQuantity)
                            {
                                flag = 1;
                            }
                        }
                        //for status on Purchase ordermaster...Received
                        pomID = po[0].PurchaseOrderId;
                        PurchaseOrderMaster pm = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == pomID && x.CompanyId == pom.CompanyId).FirstOrDefault();
                        List<PurchaseOrderDetailRecived> podrList = context.PurchaseOrderRecivedDetails.Where(x => x.PurchaseOrderId == pm.PurchaseOrderId && x.CompanyId == pm.CompanyId).ToList();
                        People p = context.Peoples.Where(q => q.PeopleID == userid && q.Active == true).SingleOrDefault();
                        var amount = 0.00;
                        foreach (var pord in podrList)
                        {
                            if (count == 1)
                            {

                                if (pord.dis1 != 0 && pord.dis1 != null)
                                    amount += ((pord.QtyRecived1 * pord.Price1 * 100) / (100 + pord.dis1)).GetValueOrDefault();
                                else
                                    amount += (pord.QtyRecived1 * pord.Price1).GetValueOrDefault();
                            }
                            else if (count == 2)
                            {
                                if (pord.dis2 != 0 && pord.dis2 != null)
                                    amount += ((pord.QtyRecived2 * pord.Price2 * 100) / (100 + pord.dis2)).GetValueOrDefault();
                                else
                                    amount += (pord.QtyRecived2 * pord.Price2).GetValueOrDefault();
                            }
                            else if (count == 3)
                            {
                                if (pord.dis3 != 0 && pord.dis3 != null)
                                    amount += ((pord.QtyRecived3 * pord.Price3 * 100) / (100 + pord.dis3)).GetValueOrDefault();
                                else
                                    amount += (pord.QtyRecived3 * pord.Price3).GetValueOrDefault();
                            }
                            else if (count == 4)
                            {
                                if (pord.dis4 != 0 && pord.dis4 != null)
                                    amount += ((pord.QtyRecived4 * pord.Price4 * 100) / (100 + pord.dis4)).GetValueOrDefault();
                                else
                                    amount += (pord.QtyRecived4 * pord.Price4).GetValueOrDefault();
                            }
                            else if (count == 5)
                            {
                                if (pord.dis5 != 0 && pord.dis5 != null)
                                    amount += ((pord.QtyRecived5 * pord.Price5 * 100) / (100 + pord.dis5)).GetValueOrDefault();
                                else
                                    amount += (pord.QtyRecived5 * pord.Price5).GetValueOrDefault();
                            }
                        }
                        if (count == 1)
                        {
                            if (pom.discount1 != null)
                            {
                                pm.discount1 = pom.discount1;
                                pm.Gr1_Amount = ((amount * 100) / (100 + pom.discount1)).GetValueOrDefault();
                                pm.TotalAmount = ((amount * 100) / (100 + pom.discount1)).GetValueOrDefault();
                            }
                            else
                            {
                                pm.TotalAmount += amount;
                                pm.Gr1_Amount = amount;
                            }
                            pm.Gr1Number = pm.PurchaseOrderId + "A";
                            pm.Gr1_Date = indianTime;
                            pm.Gr1PersonId = p.PeopleID;
                            pm.Gr1PersonName = p.DisplayName;
                            pm.VehicleNumber1 = pom.VehicleNumber;
                            pm.VehicleType1 = pom.VehicleType;
                            pm.Gr1Status = "Confirmed";
                            pm.GRcount = count;
                        }
                        else if (count == 2)
                        {
                            if (pom.discount2 != null)
                            {
                                pm.discount2 = pom.discount2;
                                pm.Gr2_Amount = ((amount * 100) / (100 + pom.discount2)).GetValueOrDefault();
                                pm.TotalAmount += ((amount * 100) / (100 + pom.discount2)).GetValueOrDefault();
                            }
                            else
                            {
                                pm.TotalAmount += amount;
                                pm.Gr2_Amount = amount;
                            }
                            pm.Gr2Number = pm.PurchaseOrderId + "B";
                            pm.Gr2_Date = indianTime;
                            pm.Gr2PersonId = p.PeopleID;
                            pm.Gr2PersonName = p.DisplayName;
                            pm.VehicleNumber2 = pom.VehicleNumber;
                            pm.VehicleType2 = pom.VehicleType;
                            pm.Gr2Status = "Confirmed";
                            pm.GRcount = count;
                        }
                        else if (count == 3)
                        {
                            if (pom.discount3 != null)
                            {
                                pm.discount3 = pom.discount3;
                                pm.Gr3_Amount = ((amount * 100) / (100 + pom.discount3)).GetValueOrDefault();
                                pm.TotalAmount += ((amount * 100) / (100 + pom.discount3)).GetValueOrDefault();
                            }
                            else
                            {
                                pm.TotalAmount += amount;
                                pm.Gr3_Amount = amount;
                            }
                            pm.Gr3Number = pm.PurchaseOrderId + "C";
                            pm.Gr3_Date = indianTime;
                            pm.Gr3PersonId = p.PeopleID;
                            pm.Gr3PersonName = p.DisplayName;
                            pm.VehicleNumber3 = pom.VehicleNumber;
                            pm.VehicleType3 = pom.VehicleType;
                            pm.Gr3Status = "Confirmed";
                            pm.GRcount = count;
                        }
                        else if (count == 4)
                        {
                            if (pom.discount4 != null)
                            {
                                pm.discount4 = pom.discount4;
                                pm.Gr4_Amount = ((amount * 100) / (100 + pom.discount4)).GetValueOrDefault();
                                pm.TotalAmount += ((amount * 100) / (100 + pom.discount4)).GetValueOrDefault();
                            }
                            else
                            {
                                pm.TotalAmount += amount;
                                pm.Gr4_Amount = amount;
                            }
                            pm.Gr4Number = pm.PurchaseOrderId + "D";
                            pm.Gr4_Date = indianTime;
                            pm.Gr4PersonId = p.PeopleID;
                            pm.Gr4PersonName = p.DisplayName;
                            pm.VehicleNumber4 = pom.VehicleNumber;
                            pm.VehicleType4 = pom.VehicleType;
                            pm.Gr4Status = "Confirmed";
                            pm.GRcount = count;
                        }
                        else if (count == 5)
                        {
                            if (pom.discount5 != null)
                            {
                                pm.discount5 = pom.discount5;
                                pm.Gr5_Amount = ((amount * 100) / (100 + pom.discount5)).GetValueOrDefault();
                                pm.TotalAmount += ((amount * 100) / (100 + pom.discount5)).GetValueOrDefault();
                            }
                            else
                            {
                                pm.TotalAmount += amount;
                                pm.Gr5_Amount = amount;
                            }
                            pm.Gr5Number = pm.PurchaseOrderId + "E";
                            pm.Gr5_Date = indianTime;
                            pm.Gr5PersonId = p.PeopleID;
                            pm.Gr5PersonName = p.DisplayName;
                            pm.VehicleNumber5 = pom.VehicleNumber;
                            pm.VehicleType5 = pom.VehicleType;
                            pm.Gr5Status = "Confirmed";
                            pm.GRcount = count;

                        }
                        if (flag == 1 && count != 5)
                        {
                            pm.Status = "Partial Received";
                            pm.progress = "70";
                            pm.Comment = pom.Comment;
                            //db.DPurchaseOrderMaster.Attach(pm);
                            context.Entry(pm).State = EntityState.Modified;
                            context.Commit();

                        }
                        else
                        {
                            pm.Status = "Received";
                            pm.Comment = pom.Comment;
                            pm.progress = "100";
                            //db.DPurchaseOrderMaster.Attach(pm);
                            context.Entry(pm).State = EntityState.Modified;
                            context.Commit();
                        }
                        dbContextTransaction.Commit();
                        return Request.CreateResponse(HttpStatusCode.OK, pom);
                    }
                    catch (Exception exe)
                    {
                        dbContextTransaction.Rollback();
                        Console.Write(exe.Message);
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "got Excepion");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in  " + ex.Message);
                logger.Info("End  : ");
                return Request.CreateResponse(HttpStatusCode.BadRequest, "got Excepion");
            }
        }
        #endregion

        #region GR in Temporary current stock
        /// <summary>
        /// Temporary current stock GR
        /// created on 12/07/2019
        /// </summary>
        /// <param name="pom"></param>
        /// <returns></returns>
        [ResponseType(typeof(PurchaseOrderMaster))]
        [Route("addtempGR")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage addTempGR(PurchaseOrderMaster pom)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int poid = pom.purDetails[0].PurchaseOrderId;
                bool IsValidate = false;
                var isLock = false;
                // Access claims              

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                IsValidate = pom.purDetails.Any(a => a.TotalQuantity < a.QtyRecived);

                //foreach (var a in pom.purDetails)
                //{
                //    if (a.TotalQuantity >= a.QtyRecived)
                //    {
                //        IsValidate = true;
                //    }
                //    else
                //    {
                //        IsValidate = false;
                //        break;
                //    }
                //}

                using (var context = new AuthContext())
                {
                    isLock = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == poid).Select(a => a.IsLock).SingleOrDefault();
                }

                if (!IsValidate && isLock)
                {
                    pom.CompanyId = compid;
                    var count = GetCount(pom.purDetails);
                    int pomID;

                    PurchaseOrderMasterRecived m = new PurchaseOrderMasterRecived();
                    using (var context = new AuthContext())
                    using (var dbContextTransaction = context.Database.BeginTransaction())
                    {
                        var isGRdoNe = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == poid && a.GRcount == count).SingleOrDefault();
                        if (isGRdoNe == null)
                        {
                            try
                            {
                                var flag = 0;
                                pomID = pom.purDetails[0].PurchaseOrderId;
                                PurchaseOrderMaster pm = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == pomID && x.CompanyId == pom.CompanyId).FirstOrDefault();

                                People p = context.Peoples.Where(q => q.PeopleID == userid && q.Active == true).SingleOrDefault();

                                foreach (PurchaseOrderDetailRecived pc in pom.purDetails)
                                {
                                    try
                                    {
                                        if (pc.isDeleted != true)
                                        {
                                            var st = context.AddPurchaseOrderDetailsRecivedInTempCS(pc, count);
                                        }
                                    }
                                    catch (Exception ee)
                                    {
                                        dbContextTransaction.Rollback();
                                        logger.Error(ee.Message);
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Got Excepion in Temporary current inventory insertion.");
                                    }
                                    if ((pc.QtyRecived1 + pc.QtyRecived2 + pc.QtyRecived3 + pc.QtyRecived4 + pc.QtyRecived5) < pc.TotalQuantity)
                                    {
                                        flag = 1;
                                    }
                                }
                                //for status on Purchase ordermaster...Received
                                List<PurchaseOrderDetailRecived> podrList = context.PurchaseOrderRecivedDetails.Where(x => x.PurchaseOrderId == pm.PurchaseOrderId && x.CompanyId == pm.CompanyId).ToList();
                                var amount = 0.00;
                                foreach (var pord in podrList)
                                {
                                    if (count == 1)
                                    {
                                        amount += (pord.QtyRecived1 * pord.Price1).GetValueOrDefault() + (pord.DamagQtyRecived1 * pord.Price1).GetValueOrDefault();
                                    }
                                    else if (count == 2)
                                    {
                                        amount += (pord.QtyRecived2 * pord.Price2).GetValueOrDefault() + (pord.DamagQtyRecived2 * pord.Price2).GetValueOrDefault();
                                    }
                                    else if (count == 3)
                                    {
                                        amount += (pord.QtyRecived3 * pord.Price3).GetValueOrDefault() + (pord.DamagQtyRecived3 * pord.Price3).GetValueOrDefault();
                                    }
                                    else if (count == 4)
                                    {
                                        amount += (pord.QtyRecived4 * pord.Price4).GetValueOrDefault() + (pord.DamagQtyRecived4 * pord.Price4).GetValueOrDefault();
                                    }
                                    else if (count == 5)
                                    {
                                        amount += (pord.QtyRecived5 * pord.Price5).GetValueOrDefault() + (pord.DamagQtyRecived5 * pord.Price5).GetValueOrDefault();
                                    }
                                }
                                if (count == 1)
                                {
                                    if (pom.discount1 != null)
                                    {
                                        pm.discount1 = pom.discount1;
                                        pm.Gr1_Amount = ((amount * 100) / (100 + pom.discount1)).GetValueOrDefault();
                                        pm.TotalAmount = ((amount * 100) / (100 + pom.discount1)).GetValueOrDefault();
                                    }
                                    else
                                    {
                                        pm.TotalAmount += amount;
                                        pm.Gr1_Amount = amount;
                                    }
                                    pm.Gr1Number = pm.PurchaseOrderId + "A";
                                    pm.Gr1_Date = indianTime;
                                    pm.Gr1PersonId = p.PeopleID;
                                    pm.Gr1PersonName = p.DisplayName;
                                    pm.VehicleNumber1 = pom.VehicleNumber;
                                    pm.VehicleType1 = pom.VehicleType;
                                    pm.Gr1Status = "Pending for Checker Side";
                                    pm.GRcount = count;
                                }
                                else if (count == 2)
                                {
                                    if (pom.discount2 != null)
                                    {
                                        pm.discount2 = pom.discount2;
                                        pm.Gr2_Amount = ((amount * 100) / (100 + pom.discount2)).GetValueOrDefault();
                                        pm.TotalAmount += ((amount * 100) / (100 + pom.discount2)).GetValueOrDefault();
                                    }
                                    else
                                    {
                                        pm.TotalAmount += amount;
                                        pm.Gr2_Amount = amount;
                                    }
                                    pm.Gr2Number = pm.PurchaseOrderId + "B";
                                    pm.Gr2_Date = indianTime;
                                    pm.Gr2PersonId = p.PeopleID;
                                    pm.Gr2PersonName = p.DisplayName;
                                    pm.VehicleNumber2 = pom.VehicleNumber;
                                    pm.VehicleType2 = pom.VehicleType;
                                    pm.Gr2Status = "Pending for Checker Side";
                                    pm.GRcount = count;
                                }
                                else if (count == 3)
                                {
                                    if (pom.discount3 != null)
                                    {
                                        pm.discount3 = pom.discount3;
                                        pm.Gr3_Amount = ((amount * 100) / (100 + pom.discount3)).GetValueOrDefault();
                                        pm.TotalAmount += ((amount * 100) / (100 + pom.discount3)).GetValueOrDefault();
                                    }
                                    else
                                    {
                                        pm.TotalAmount += amount;
                                        pm.Gr3_Amount = amount;
                                    }
                                    pm.Gr3Number = pm.PurchaseOrderId + "C";
                                    pm.Gr3_Date = indianTime;
                                    pm.Gr3PersonId = p.PeopleID;
                                    pm.Gr3PersonName = p.DisplayName;
                                    pm.VehicleNumber3 = pom.VehicleNumber;
                                    pm.VehicleType3 = pom.VehicleType;
                                    pm.Gr3Status = "Pending for Checker Side";
                                    pm.GRcount = count;
                                }
                                else if (count == 4)
                                {
                                    if (pom.discount4 != null)
                                    {
                                        pm.discount4 = pom.discount4;
                                        pm.Gr4_Amount = ((amount * 100) / (100 + pom.discount4)).GetValueOrDefault();
                                        pm.TotalAmount += ((amount * 100) / (100 + pom.discount4)).GetValueOrDefault();
                                    }
                                    else
                                    {
                                        pm.TotalAmount += amount;
                                        pm.Gr4_Amount = amount;
                                    }
                                    pm.Gr4Number = pm.PurchaseOrderId + "D";
                                    pm.Gr4_Date = indianTime;
                                    pm.Gr4PersonId = p.PeopleID;
                                    pm.Gr4PersonName = p.DisplayName;
                                    pm.VehicleNumber4 = pom.VehicleNumber;
                                    pm.VehicleType4 = pom.VehicleType;
                                    pm.Gr4Status = "Pending for Checker Side";
                                    pm.GRcount = count;
                                }
                                else if (count == 5)
                                {
                                    if (pom.discount5 != null)
                                    {
                                        pm.discount5 = pom.discount5;
                                        pm.Gr5_Amount = ((amount * 100) / (100 + pom.discount5)).GetValueOrDefault();
                                        pm.TotalAmount += ((amount * 100) / (100 + pom.discount5)).GetValueOrDefault();
                                    }
                                    else
                                    {
                                        pm.TotalAmount += amount;
                                        pm.Gr5_Amount = amount;
                                    }
                                    pm.Gr5Number = pm.PurchaseOrderId + "E";
                                    pm.Gr5_Date = indianTime;
                                    pm.Gr5PersonId = p.PeopleID;
                                    pm.Gr5PersonName = p.DisplayName;
                                    pm.VehicleNumber5 = pom.VehicleNumber;
                                    pm.VehicleType5 = pom.VehicleType;
                                    pm.Gr5Status = "Pending for Checker Side";
                                    pm.GRcount = count;
                                }
                                if (flag == 1 && count != 5)
                                {
                                    pm.Status = "UN Partial Received";
                                    pm.progress = "70";
                                    pm.Comment = pom.Comment;
                                    //db.DPurchaseOrderMaster.Attach(pm);
                                    context.Entry(pm).State = EntityState.Modified;
                                    context.Commit();
                                }
                                else
                                {
                                    pm.Status = "UN Received";
                                    pm.Comment = pom.Comment;
                                    pm.progress = "100";
                                    context.Entry(pm).State = EntityState.Modified;
                                    context.Commit();

                                }
                                dbContextTransaction.Commit();
                                return Request.CreateResponse(HttpStatusCode.OK, pom);
                            }
                            catch (Exception exe)
                            {
                                dbContextTransaction.Rollback();
                                Console.Write(exe.Message);
                                return Request.CreateResponse(HttpStatusCode.BadRequest, "got Excepion");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "GR: " + count + " Already done.");
                        }
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Receive Quantity should be Less then Total Quantity or PO is not lock.");
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in  " + ex.Message);
                logger.Info("End  : ");
                return Request.CreateResponse(HttpStatusCode.BadRequest, "got Excepion");
            }
        }
        #endregion

        #region GR close
        /// <summary>
        /// Close GR
        /// </summary>
        /// <param name="id"></param>
        /// <param name="po"></param>
        /// <returns></returns>
        //[Route("closePO")]
        //[AcceptVerbs("POST")]
        //public bool post(int id, List<PurchaseOrderDetailRecived> po)
        //{
        //    try
        //    {

        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        // Access claims

        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //            compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //        using (var context = new AuthContext())
        //        {

        //            List<PurchaseOrderDetailRecived> podrLists = context.PurchaseOrderRecivedDetails.Where(x => x.PurchaseOrderId == id && x.CompanyId == compid).ToList();

        //            if (podrLists.Count != 0)
        //            {
        //                PurchaseOrderMaster pm = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == id && x.CompanyId == compid).FirstOrDefault();
        //                pm.Status = "CN Received";
        //                pm.progress = "100";
        //                //db.DPurchaseOrderMaster.Attach(pm);
        //                context.Entry(pm).State = EntityState.Modified;
        //                context.Commit();
        //                List<PurchaseOrderDetailRecived> podrList = context.PurchaseOrderRecivedDetails.Where(x => x.PurchaseOrderId == pm.PurchaseOrderId && x.CompanyId == pm.CompanyId).ToList();
        //                if (podrList.Count != 0)
        //                {
        //                    foreach (var a in podrList)
        //                    {
        //                        a.CreationDate = indianTime;
        //                        a.Status = "CN Received";
        //                        //db.PurchaseOrderRecivedDetails.Attach(a);
        //                        context.Entry(a).State = EntityState.Modified;
        //                        context.Commit();
        //                    }
        //                }
        //                List<PurchaseOrderDetail> pod = context.DPurchaseOrderDeatil.Where(q => q.PurchaseOrderId == pm.PurchaseOrderId && q.CompanyId == pm.CompanyId).ToList();

        //                if (pod.Count != 0)
        //                {
        //                    foreach (var a in pod)
        //                    {
        //                        a.Status = "Close";
        //                        //db.DPurchaseOrderDeatil.Attach(a);
        //                        context.Entry(a).State = EntityState.Modified;
        //                        context.Commit();
        //                    }
        //                    #region PO History Received
        //                    POHistory Ph = new POHistory();
        //                    var UserName = context.Peoples.Where(y => y.PeopleID == userid).Select(c => c.DisplayName).SingleOrDefault();

        //                    Ph.PurchaseOrderId = pod[0].PurchaseOrderId;
        //                    Ph.Amount = pm.TotalAmount;
        //                    Ph.Status = "Received";
        //                    Ph.EditBy = UserName;
        //                    Ph.UpdatedDate = DateTime.Now;
        //                    context.POHistoryDB.Add(Ph);
        //                    context.Commit();
        //                    #endregion
        //                }
        //            }
        //            else
        //            {
        //                PurchaseOrderMaster pm = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == id && x.CompanyId == compid).FirstOrDefault();
        //                pm.Status = "Closed";
        //                pm.progress = "50";
        //                //db.DPurchaseOrderMaster.Attach(pm);
        //                context.Entry(pm).State = EntityState.Modified;
        //                context.Commit();
        //                List<PurchaseOrderDetail> pod = context.DPurchaseOrderDeatil.Where(q => q.PurchaseOrderId == pm.PurchaseOrderId && q.CompanyId == pm.CompanyId).ToList();

        //                if (pod.Count != 0)
        //                {
        //                    foreach (var a in pod)
        //                    {
        //                        a.Status = "Closed";
        //                        //db.DPurchaseOrderDeatil.Attach(a);
        //                        context.Entry(a).State = EntityState.Modified;
        //                        context.Commit();
        //                    }
        //                    #region PO History Received
        //                    POHistory Ph = new POHistory();
        //                    var UserName = context.Peoples.Where(y => y.PeopleID == userid).Select(c => c.DisplayName).SingleOrDefault();
        //                    Ph.PurchaseOrderId = pod[0].PurchaseOrderId;
        //                    Ph.Amount = pm.TotalAmount;
        //                    Ph.Status = "Closed";
        //                    Ph.EditBy = UserName;
        //                    Ph.UpdatedDate = DateTime.Now;
        //                    context.POHistoryDB.Add(Ph);
        //                    context.Commit();
        //                    #endregion
        //                }
        //            }
        //            return true;
        //        }
        //    }
        //    catch (Exception exe)
        //    {
        //        Console.Write(exe.Message);
        //        return false;
        //    }
        //}
        #endregion

        #region cancel PO
        /// <summary>
        /// Cancel po 
        /// created on 12/07/2019
        /// </summary>
        /// <param name="pom"></param>
        /// <returns></returns>
        [Route("cancelpo")]
        [AcceptVerbs("Post")]
        public HttpResponseMessage cancelPO(PurchaseOrderMaster pom)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                // Access claims
                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                }
                using (AuthContext db = new AuthContext())
                {
                    logger.Info("Start PurchaseOrderCancel");
                    // get people information tracking.
                    People p = db.Peoples.Where(q => q.PeopleID == userid && q.Active == true).SingleOrDefault();
                    // get purchase order master data for cancel po.
                    PurchaseOrderMaster pm = db.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == pom.PurchaseOrderId && (x.Status == "Self Approved" || x.Status == "Approved" || x.Status == "Send for Approval")).FirstOrDefault();
                    if (pm != null)
                    {
                        pm.Status = "Canceled";
                        pm.CanceledById = p.PeopleID;
                        pm.CanceledByName = p.DisplayName;
                        pm.CanceledDate = indianTime;
                        db.Commit();
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, "Success");
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in PurchaseOrderCancel: " + ex.Message);
                logger.Info("End PurchaseOrderCancel");
                return null;
            }
        }
        #endregion  

        [Authorize]
        [Route("")]
        public IEnumerable<PurchaseOrderDetail> Getallorderdetails(string id)
        {
            logger.Info("start : ");
            List<PurchaseOrderDetail> ass = new List<PurchaseOrderDetail>();
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int warehouseid = 0;
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
                    if (claim.Type == "Warehouseid")
                    {
                        warehouseid = int.Parse(claim.Value);
                    }
                }

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                int idd = Int32.Parse(id);
                using (var context = new AuthContext())
                {
                    ass = context.AllPOrderDetails(idd, compid, warehouseid).ToList();
                }
                logger.Info("End  : ");
                return ass;
            }
            catch (Exception ex)
            {
                logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                logger.Info("End  PurchaseOrderDetail: ");
                return null;
            }
        }

        [Authorize]
        [Route("")]
        public HttpResponseMessage GetallorderdetailRecived(string id, string a)
        {
            logger.Info("start : ");
            var _PurchaseOrderMaster = new PurchaseOrderMaster();
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
            int idd = Int32.Parse(id);
            using (var context = new AuthContext())
            {
                _PurchaseOrderMaster = context.DPurchaseOrderMaster.Where(c => c.PurchaseOrderId == idd).SingleOrDefault();
                _PurchaseOrderMaster.purDetails = context.PurchaseOrderRecivedDetails.Where(c => c.PurchaseOrderId == idd).ToList();
                var itemNumberList = _PurchaseOrderMaster.purDetails.Select(x => x.ItemNumber);
                var Centralitem = context.ItemMasterCentralDB.Where(c => itemNumberList.Contains(c.Number)).ToList();
                var MultiMrpList = context.ItemMultiMRPDB.Where(c => itemNumberList.Contains(c.ItemNumber)).ToList();
                if (_PurchaseOrderMaster.purDetails != null)
                {
                    foreach (var Ritem in _PurchaseOrderMaster.purDetails)
                    {
                        var item = Centralitem.Where(c => c.Number == Ritem.ItemNumber && c.CompanyId == compid).FirstOrDefault();
                        Ritem.TotalTaxPercentage = item.TotalTaxPercentage;
                        Ritem.CessTaxPercentage = item.TotalCessPercentage;
                        //Ritem.Barcode = item.Barcode;
                        Ritem.IsCommodity = false;

                        if (item.IsSensitive == true && item.IsSensitiveMRP == true)
                        {
                            Ritem.IsCommodity = false;
                        }
                        else if (item.IsSensitive == true && item.IsSensitiveMRP == false)
                        {
                            Ritem.IsCommodity = true;
                        }

                        Ritem.multiMrpIds = MultiMrpList.Where(e => e.ItemNumber == Ritem.ItemNumber).ToList();

                        if (Ritem.ItemMultiMRPId > 0 && Ritem.ItemMultiMRPId1 == 0)
                        {
                            Ritem.ItemMultiMRPId1 = Ritem.ItemMultiMRPId;
                        }
                        if (Ritem.ItemName != null && Ritem.ItemName1 == null)
                        {
                            Ritem.ItemName1 = Ritem.ItemName;
                        }

                    }
                }
                //ass = context.AllPOrderDetails1(idd, compid);

                //foreach (PurchaseOrderDetailRecived d in _PurchaseOrderMaster.purDetails)
                //{
                //    d.multiMrpIds = MultiMrpList.Where(e => e.ItemNumber == d.ItemNumber).ToList();
                //}
                _PurchaseOrderMaster.DueDays = context.Suppliers.Where(x => x.SupplierId == _PurchaseOrderMaster.SupplierId).Select(x => x.PaymentTerms).FirstOrDefault();
            }

            logger.Info("End  : ");
            return Request.CreateResponse(HttpStatusCode.OK, _PurchaseOrderMaster);

        }

        [Route("GetMRPByItemId")]
        public double GetMRPByItemId(int itemid)
        {
            logger.Info("start : ");
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouseid = 0;
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
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouseid = int.Parse(claim.Value);
                    }
                }

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                double MRP;
                using (var context = new AuthContext())
                {
                    var data = context.itemMasters.Where(a => a.ItemId == itemid).FirstOrDefault();
                    MRP = data.price;
                }

                logger.Info("End  : ");
                return MRP;

                //return Request.CreateResponse(HttpStatusCode.OK, ass); ;
            }
            catch (Exception ex)
            {
                logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                logger.Info("End  PurchaseOrderDetail: ");
                return 0;
            }
        }
        /// <summary>
        /// Get Unconfirmed GR detail
        /// </summary>
        /// <param name="GRID"></param>
        /// <returns></returns>
        [Route("GetUnconfirmGrDetail")]
        public HttpResponseMessage GetUnconfirmGrDetail(string GRID)
        {
            logger.Info("start : ");
            try
            {
                // Access claims
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                int CompanyId = compid;
                PurchaseOrderMaster pm = new PurchaseOrderMaster();
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                string POID = GRID.Remove(GRID.Length - 1, 1);
                int poid = Int32.Parse(POID);

                using (AuthContext db = new AuthContext())
                {
                    pm = db.DPurchaseOrderMaster.Where(c => c.PurchaseOrderId == poid && c.CompanyId == compid).SingleOrDefault();
                    pm.purDetails = db.PurchaseOrderRecivedDetails.Where(c => c.PurchaseOrderId == poid && c.CompanyId == compid).ToList();
                    if (pm.purDetails != null)
                    {
                        foreach (var a in pm.purDetails)
                        {
                            var item = db.itemMasters.Where(c => c.ItemId == a.ItemId && c.CompanyId == compid).SingleOrDefault();
                            if (item != null)
                            {
                                a.TotalTaxPercentage = item.TotalTaxPercentage;
                                a.CessTaxPercentage = item.TotalCessPercentage;
                            }
                        }
                    }
                }
                logger.Info("End  : ");
                return Request.CreateResponse(HttpStatusCode.OK, pm); ;
            }
            catch (Exception ex)
            {
                logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                logger.Info("End  PurchaseOrderDetail: ");
                return Request.CreateResponse(HttpStatusCode.BadRequest, "");
            }
        }
        /// <summary>
        /// Confirm GR 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        //[Route("PutUnconfirmGrDetail")]
        //[HttpPost]
        //public HttpResponseMessage PutUnconfirmGrDetail(PutUCGRDTO obj)
        //{
        //    logger.Info("start : ");
        //    using (AuthContext db = new AuthContext())
        //    using (var dbContextTransaction = db.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            // Access claims
        //            var identity = User.Identity as ClaimsIdentity;
        //            int compid = 0, userid = 0;
        //            int Warehouse_id = 0;

        //            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

        //            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
        //                Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
        //            int CompanyId = compid;

        //            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

        //            string sAllCharacter = obj.GrNumber;
        //            char GRType = sAllCharacter[sAllCharacter.Length - 1];
        //            PurchaseOrderMaster pm = new PurchaseOrderMaster();
        //            switch (GRType)
        //            {
        //                case 'A':
        //                    pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
        //                    pm.Gr1Status = "Confirmed";
        //                    foreach (PutListUCGRDTO k in obj.Detail)
        //                    {
        //                        PurchaseOrderDetailRecived detail = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderDetailRecivedId == k.PurchaseOrderDetailRecivedId).SingleOrDefault();
        //                        ItemMaster IM = db.itemMasters.Where(a => a.ItemId == k.ItemId).SingleOrDefault();
        //                        CurrentStock item = db.DbCurrentStock.Where(x => x.ItemNumber == IM.Number && x.WarehouseId == IM.WarehouseId && x.CompanyId == IM.CompanyId && x.ItemMultiMRPId == detail.ItemMultiMRPId).SingleOrDefault();
        //                        TemporaryCurrentStock Tcs = db.TemporaryCurrentStockDB.Where(x => x.ItemNumber == IM.Number && x.WarehouseId == IM.WarehouseId && x.CompanyId == IM.CompanyId && x.ItemMultiMRPId == detail.ItemMultiMRPId).SingleOrDefault();


        //                        if (k.QtyRecived != 0)
        //                        {
        //                            /// Deduct Stock from Temporary current stock
        //                            TemporaryCurrentStockHistory Tcsh = new TemporaryCurrentStockHistory();
        //                            if (Tcs != null)
        //                            {
        //                                Tcsh.StockId = Tcs.Id;
        //                                Tcsh.ItemNumber = Tcs.ItemNumber;
        //                                Tcsh.itemname = Tcs.itemname;
        //                                Tcsh.OdOrPoId = obj.PurchaseOrderId;

        //                                Tcsh.CurrentInventory = Tcs.CurrentInventory;
        //                                Tcsh.InventoryOut = Convert.ToInt32(k.QtyRecived);
        //                                Tcsh.TotalCurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived);

        //                                Tcsh.ExpInventoryOut = detail.ExpQtyRecived1 - k.ExpQtyRecived;
        //                                Tcsh.TotalExpCurrentInventory = Tcs.ExpCurrentInventory - (detail.ExpQtyRecived1 - k.ExpQtyRecived);

        //                                Tcsh.DamageInventoryOut = detail.DamagQtyRecived1 - Convert.ToInt32(k.DamagQtyRecived);
        //                                Tcsh.TotalDamageCurrentInventory = Tcs.DamageCurrentInventory - (detail.DamagQtyRecived1 - Convert.ToInt32(k.DamagQtyRecived));

        //                                Tcsh.WarehouseName = Tcs.WarehouseName;
        //                                Tcsh.Warehouseid = Tcs.WarehouseId;
        //                                Tcsh.CompanyId = Tcs.CompanyId;

        //                                Tcsh.CreationDate = indianTime;
        //                                Tcsh.userid = userid;
        //                                Tcsh.ItemMultiMRPId = Tcs.ItemMultiMRPId;
        //                                db.TemporaryCurrentStockHistoryDB.Add(Tcsh);
        //                            }
        //                            Tcs.CurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived);
        //                            db.UpdateTempCurrentStock(Tcs);
        //                            /// Add Stock in current stock from Temporary current stock
        //                            CurrentStockHistory Oss = new CurrentStockHistory();
        //                            if (item != null)
        //                            {
        //                                Oss.StockId = item.StockId;
        //                                Oss.ItemNumber = item.ItemNumber;
        //                                Oss.itemname = item.itemname;
        //                                Oss.OdOrPoId = obj.PurchaseOrderId;
        //                                Oss.CurrentInventory = item.CurrentInventory;
        //                                Oss.InventoryIn = Convert.ToInt32(k.QtyRecived);
        //                                Oss.TotalInventory = item.CurrentInventory + Convert.ToInt32(k.QtyRecived);
        //                                Oss.WarehouseName = item.WarehouseName;
        //                                Oss.Warehouseid = item.WarehouseId;
        //                                Oss.CompanyId = item.CompanyId;

        //                                Oss.CreationDate = indianTime;
        //                                Oss.ManualReason = "(+) Unconfirmed Stock GRN: " + sAllCharacter;
        //                                Oss.ItemMultiMRPId = item.ItemMultiMRPId;
        //                                db.CurrentStockHistoryDb.Add(Oss);
        //                            }
        //                            item.CurrentInventory = item.CurrentInventory + Convert.ToInt32(k.QtyRecived);
        //                            db.UpdateCurrentStock(item);
        //                        }
        //                        detail.QtyRecived1 = k.QtyRecived;
        //                        detail.DamagQtyRecived1 = k.DamagQtyRecived;
        //                        detail.ExpQtyRecived1 = k.ExpQtyRecived;
        //                        detail.PriceRecived = Convert.ToDouble(detail.Price1 * k.QtyRecived);
        //                        detail.GRDate1 = indianTime;
        //                        db.Entry(detail).State = EntityState.Modified;
        //                        db.Commit();
        //                    }
        //                    break;
        //                case 'B':
        //                    pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
        //                    pm.Gr2Status = "Confirmed";
        //                    foreach (PutListUCGRDTO k in obj.Detail)
        //                    {
        //                        PurchaseOrderDetailRecived detail = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderDetailRecivedId == k.PurchaseOrderDetailRecivedId).SingleOrDefault();
        //                        ItemMaster IM = db.itemMasters.Where(a => a.ItemId == k.ItemId).SingleOrDefault();
        //                        CurrentStock item = db.DbCurrentStock.Where(x => x.ItemNumber == IM.Number && x.WarehouseId == IM.WarehouseId && x.CompanyId == IM.CompanyId && x.ItemMultiMRPId == detail.ItemMultiMRPId).SingleOrDefault();
        //                        TemporaryCurrentStock Tcs = db.TemporaryCurrentStockDB.Where(x => x.ItemNumber == IM.Number && x.WarehouseId == IM.WarehouseId && x.CompanyId == IM.CompanyId && x.ItemMultiMRPId == detail.ItemMultiMRPId).SingleOrDefault();

        //                        if (k.QtyRecived != 0)
        //                        {
        //                            /// Deduct Stock from Temporary current stock
        //                            TemporaryCurrentStockHistory Tcsh = new TemporaryCurrentStockHistory();
        //                            if (Tcs != null)
        //                            {
        //                                Tcsh.StockId = Tcs.Id;
        //                                Tcsh.ItemNumber = Tcs.ItemNumber;
        //                                Tcsh.itemname = Tcs.itemname;
        //                                Tcsh.OdOrPoId = obj.PurchaseOrderId;

        //                                Tcsh.CurrentInventory = Tcs.CurrentInventory;
        //                                Tcsh.InventoryOut = Convert.ToInt32(k.QtyRecived);
        //                                Tcsh.TotalCurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived);

        //                                Tcsh.ExpInventoryOut = detail.ExpQtyRecived1 - k.ExpQtyRecived;
        //                                Tcsh.TotalExpCurrentInventory = Tcs.ExpCurrentInventory - (detail.ExpQtyRecived1 - k.ExpQtyRecived);

        //                                Tcsh.DamageInventoryOut = detail.DamagQtyRecived1 - Convert.ToInt32(k.DamagQtyRecived);
        //                                Tcsh.TotalDamageCurrentInventory = Tcs.DamageCurrentInventory - (detail.DamagQtyRecived1 - Convert.ToInt32(k.DamagQtyRecived));

        //                                Tcsh.WarehouseName = Tcs.WarehouseName;
        //                                Tcsh.Warehouseid = Tcs.WarehouseId;
        //                                Tcsh.CompanyId = Tcs.CompanyId;

        //                                Tcsh.CreationDate = indianTime;
        //                                Tcsh.userid = userid;
        //                                Tcsh.ItemMultiMRPId = Tcs.ItemMultiMRPId;
        //                                db.TemporaryCurrentStockHistoryDB.Add(Tcsh);
        //                            }
        //                            Tcs.CurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived);
        //                            db.UpdateTempCurrentStock(Tcs);
        //                            /// Add Stock in current stock from Temporary current stock
        //                            CurrentStockHistory Oss = new CurrentStockHistory();
        //                            if (item != null)
        //                            {
        //                                Oss.StockId = item.StockId;
        //                                Oss.ItemNumber = item.ItemNumber;
        //                                Oss.itemname = item.itemname;
        //                                Oss.OdOrPoId = obj.PurchaseOrderId;
        //                                Oss.CurrentInventory = item.CurrentInventory;
        //                                Oss.InventoryIn = Convert.ToInt32(k.QtyRecived);
        //                                Oss.TotalInventory = item.CurrentInventory + Convert.ToInt32(k.QtyRecived);
        //                                Oss.WarehouseName = item.WarehouseName;
        //                                Oss.Warehouseid = item.WarehouseId;
        //                                Oss.CompanyId = item.CompanyId;
        //                                Oss.CreationDate = indianTime;
        //                                Oss.ManualReason = "(+) Unconfirm Stock GRN: " + sAllCharacter;
        //                                Oss.ItemMultiMRPId = item.ItemMultiMRPId;
        //                                db.CurrentStockHistoryDb.Add(Oss);
        //                            }
        //                            item.CurrentInventory = item.CurrentInventory + Convert.ToInt32(k.QtyRecived);
        //                            db.UpdateCurrentStock(item);
        //                        }
        //                        detail.QtyRecived2 = k.QtyRecived;
        //                        detail.DamagQtyRecived2 = k.DamagQtyRecived;
        //                        detail.ExpQtyRecived2 = k.ExpQtyRecived;
        //                        detail.PriceRecived = Convert.ToDouble(detail.Price1 * detail.QtyRecived1) + Convert.ToDouble(detail.Price2 * k.QtyRecived);
        //                        detail.GRDate2 = indianTime;
        //                        db.Entry(detail).State = EntityState.Modified;
        //                        db.Commit();
        //                    }
        //                    break;
        //                case 'C':
        //                    pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
        //                    pm.Gr3Status = "Confirmed";
        //                    foreach (PutListUCGRDTO k in obj.Detail)
        //                    {
        //                        PurchaseOrderDetailRecived detail = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderDetailRecivedId == k.PurchaseOrderDetailRecivedId).SingleOrDefault();
        //                        ItemMaster IM = db.itemMasters.Where(a => a.ItemId == k.ItemId).SingleOrDefault();
        //                        CurrentStock item = db.DbCurrentStock.Where(x => x.ItemNumber == IM.Number && x.WarehouseId == IM.WarehouseId && x.CompanyId == IM.CompanyId && x.ItemMultiMRPId == detail.ItemMultiMRPId).SingleOrDefault();
        //                        TemporaryCurrentStock Tcs = db.TemporaryCurrentStockDB.Where(x => x.ItemNumber == IM.Number && x.WarehouseId == IM.WarehouseId && x.CompanyId == IM.CompanyId && x.ItemMultiMRPId == detail.ItemMultiMRPId).SingleOrDefault();

        //                        if (k.QtyRecived != 0)
        //                        {
        //                            /// Deduct Stock from Temporary current stock
        //                            TemporaryCurrentStockHistory Tcsh = new TemporaryCurrentStockHistory();
        //                            if (Tcs != null)
        //                            {
        //                                Tcsh.StockId = Tcs.Id;
        //                                Tcsh.ItemNumber = Tcs.ItemNumber;
        //                                Tcsh.itemname = Tcs.itemname;
        //                                Tcsh.OdOrPoId = obj.PurchaseOrderId;

        //                                Tcsh.CurrentInventory = Tcs.CurrentInventory;
        //                                Tcsh.InventoryOut = Convert.ToInt32(k.QtyRecived);
        //                                Tcsh.TotalCurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived);

        //                                Tcsh.ExpInventoryOut = detail.ExpQtyRecived1 - k.ExpQtyRecived;
        //                                Tcsh.TotalExpCurrentInventory = Tcs.ExpCurrentInventory - (detail.ExpQtyRecived1 - k.ExpQtyRecived);

        //                                Tcsh.DamageInventoryOut = detail.DamagQtyRecived1 - Convert.ToInt32(k.DamagQtyRecived);
        //                                Tcsh.TotalDamageCurrentInventory = Tcs.DamageCurrentInventory - (detail.DamagQtyRecived1 - Convert.ToInt32(k.DamagQtyRecived));

        //                                Tcsh.WarehouseName = Tcs.WarehouseName;
        //                                Tcsh.Warehouseid = Tcs.WarehouseId;
        //                                Tcsh.CompanyId = Tcs.CompanyId;

        //                                Tcsh.CreationDate = indianTime;
        //                                Tcsh.userid = userid;
        //                                Tcsh.ItemMultiMRPId = Tcs.ItemMultiMRPId;
        //                                db.TemporaryCurrentStockHistoryDB.Add(Tcsh);
        //                            }
        //                            Tcs.CurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived);
        //                            db.UpdateTempCurrentStock(Tcs);
        //                            /// Add Stock in current stock from Temporary current stock
        //                            CurrentStockHistory Oss = new CurrentStockHistory();
        //                            if (item != null)
        //                            {
        //                                Oss.StockId = item.StockId;
        //                                Oss.ItemNumber = item.ItemNumber;
        //                                Oss.itemname = item.itemname;
        //                                Oss.OdOrPoId = obj.PurchaseOrderId;
        //                                Oss.CurrentInventory = item.CurrentInventory;
        //                                Oss.InventoryIn = Convert.ToInt32(k.QtyRecived);
        //                                Oss.TotalInventory = item.CurrentInventory + Convert.ToInt32(k.QtyRecived);
        //                                Oss.WarehouseName = item.WarehouseName;
        //                                Oss.Warehouseid = item.WarehouseId;
        //                                Oss.CompanyId = item.CompanyId;

        //                                Oss.CreationDate = indianTime;
        //                                Oss.ManualReason = "(+) Unconfirm Stock GRN: " + sAllCharacter;
        //                                Oss.ItemMultiMRPId = item.ItemMultiMRPId;
        //                                db.CurrentStockHistoryDb.Add(Oss);
        //                            }
        //                            item.CurrentInventory = item.CurrentInventory + Convert.ToInt32(k.QtyRecived);
        //                            db.UpdateCurrentStock(item);
        //                        }
        //                        detail.QtyRecived3 = k.QtyRecived;
        //                        detail.DamagQtyRecived3 = k.DamagQtyRecived;
        //                        detail.ExpQtyRecived3 = k.ExpQtyRecived;
        //                        detail.GRDate3 = indianTime;
        //                        detail.PriceRecived = Convert.ToDouble(detail.Price1 * detail.QtyRecived1) + Convert.ToDouble(detail.Price2 * detail.QtyRecived2) + Convert.ToDouble(detail.Price3 * k.QtyRecived);
        //                        db.Entry(detail).State = EntityState.Modified;
        //                        db.Commit();
        //                    }
        //                    break;
        //                case 'D':
        //                    pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
        //                    pm.Gr4Status = "Confirmed";
        //                    foreach (PutListUCGRDTO k in obj.Detail)
        //                    {
        //                        PurchaseOrderDetailRecived detail = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderDetailRecivedId == k.PurchaseOrderDetailRecivedId).SingleOrDefault();
        //                        ItemMaster IM = db.itemMasters.Where(a => a.ItemId == k.ItemId).SingleOrDefault();
        //                        CurrentStock item = db.DbCurrentStock.Where(x => x.ItemNumber == IM.Number && x.WarehouseId == IM.WarehouseId && x.CompanyId == IM.CompanyId && x.ItemMultiMRPId == detail.ItemMultiMRPId).SingleOrDefault();
        //                        TemporaryCurrentStock Tcs = db.TemporaryCurrentStockDB.Where(x => x.ItemNumber == IM.Number && x.WarehouseId == IM.WarehouseId && x.CompanyId == IM.CompanyId && x.ItemMultiMRPId == detail.ItemMultiMRPId).SingleOrDefault();

        //                        if (k.QtyRecived != 0)
        //                        {
        //                            /// Deduct Stock from Temporary current stock
        //                            TemporaryCurrentStockHistory Tcsh = new TemporaryCurrentStockHistory();
        //                            if (Tcs != null)
        //                            {
        //                                Tcsh.StockId = Tcs.Id;
        //                                Tcsh.ItemNumber = Tcs.ItemNumber;
        //                                Tcsh.itemname = Tcs.itemname;
        //                                Tcsh.OdOrPoId = obj.PurchaseOrderId;

        //                                Tcsh.CurrentInventory = Tcs.CurrentInventory;
        //                                Tcsh.InventoryOut = Convert.ToInt32(k.QtyRecived);
        //                                Tcsh.TotalCurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived);

        //                                Tcsh.ExpInventoryOut = detail.ExpQtyRecived1 - k.ExpQtyRecived;
        //                                Tcsh.TotalExpCurrentInventory = Tcs.ExpCurrentInventory - (detail.ExpQtyRecived1 - k.ExpQtyRecived);

        //                                Tcsh.DamageInventoryOut = detail.DamagQtyRecived1 - Convert.ToInt32(k.DamagQtyRecived);
        //                                Tcsh.TotalDamageCurrentInventory = Tcs.DamageCurrentInventory - (detail.DamagQtyRecived1 - Convert.ToInt32(k.DamagQtyRecived));

        //                                Tcsh.WarehouseName = Tcs.WarehouseName;
        //                                Tcsh.Warehouseid = Tcs.WarehouseId;
        //                                Tcsh.CompanyId = Tcs.CompanyId;

        //                                Tcsh.CreationDate = indianTime;
        //                                Tcsh.userid = userid;
        //                                Tcsh.ItemMultiMRPId = Tcs.ItemMultiMRPId;
        //                                db.TemporaryCurrentStockHistoryDB.Add(Tcsh);
        //                            }
        //                            Tcs.CurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived);
        //                            db.UpdateTempCurrentStock(Tcs);
        //                            /// Add Stock in current stock from Temporary current stock
        //                            CurrentStockHistory Oss = new CurrentStockHistory();
        //                            if (item != null)
        //                            {
        //                                Oss.StockId = item.StockId;
        //                                Oss.ItemNumber = item.ItemNumber;
        //                                Oss.itemname = item.itemname;
        //                                Oss.OdOrPoId = obj.PurchaseOrderId;
        //                                Oss.CurrentInventory = item.CurrentInventory;
        //                                Oss.InventoryIn = Convert.ToInt32(k.QtyRecived);
        //                                Oss.TotalInventory = item.CurrentInventory + Convert.ToInt32(k.QtyRecived);
        //                                Oss.WarehouseName = item.WarehouseName;
        //                                Oss.Warehouseid = item.WarehouseId;
        //                                Oss.CompanyId = item.CompanyId;

        //                                Oss.CreationDate = indianTime;
        //                                Oss.ManualReason = "(+) Unconfirm Stock GRN: " + sAllCharacter;
        //                                Oss.ItemMultiMRPId = item.ItemMultiMRPId;
        //                                db.CurrentStockHistoryDb.Add(Oss);
        //                            }
        //                            item.CurrentInventory = item.CurrentInventory + Convert.ToInt32(k.QtyRecived);
        //                            db.UpdateCurrentStock(item);
        //                        }
        //                        detail.QtyRecived4 = k.QtyRecived;
        //                        detail.DamagQtyRecived4 = k.DamagQtyRecived;
        //                        detail.ExpQtyRecived4 = k.ExpQtyRecived;
        //                        detail.GRDate4 = indianTime;
        //                        detail.PriceRecived = Convert.ToDouble(detail.Price1 * detail.QtyRecived1) + Convert.ToDouble(detail.Price2 * detail.QtyRecived2) + Convert.ToDouble(detail.Price3 * detail.QtyRecived3) + Convert.ToDouble(detail.Price4 * k.QtyRecived);
        //                        db.Entry(detail).State = EntityState.Modified;

        //                        db.Commit();
        //                    }
        //                    break;
        //                case 'E':
        //                    pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
        //                    pm.Gr5Status = "Confirmed";
        //                    foreach (PutListUCGRDTO k in obj.Detail)
        //                    {
        //                        PurchaseOrderDetailRecived detail = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderDetailRecivedId == k.PurchaseOrderDetailRecivedId).SingleOrDefault();
        //                        ItemMaster IM = db.itemMasters.Where(a => a.ItemId == k.ItemId).SingleOrDefault();
        //                        CurrentStock item = db.DbCurrentStock.Where(x => x.ItemNumber == IM.Number && x.WarehouseId == IM.WarehouseId && x.CompanyId == IM.CompanyId && x.ItemMultiMRPId == detail.ItemMultiMRPId).SingleOrDefault();
        //                        TemporaryCurrentStock Tcs = db.TemporaryCurrentStockDB.Where(x => x.ItemNumber == IM.Number && x.WarehouseId == IM.WarehouseId && x.CompanyId == IM.CompanyId && x.ItemMultiMRPId == detail.ItemMultiMRPId).SingleOrDefault();

        //                        if (k.QtyRecived != 0)
        //                        {
        //                            /// Deduct Stock from Temporary current stock
        //                            TemporaryCurrentStockHistory Tcsh = new TemporaryCurrentStockHistory();
        //                            if (Tcs != null)
        //                            {
        //                                Tcsh.StockId = Tcs.Id;
        //                                Tcsh.ItemNumber = Tcs.ItemNumber;
        //                                Tcsh.itemname = Tcs.itemname;
        //                                Tcsh.OdOrPoId = obj.PurchaseOrderId;

        //                                Tcsh.CurrentInventory = Tcs.CurrentInventory;
        //                                Tcsh.InventoryOut = Convert.ToInt32(k.QtyRecived);
        //                                Tcsh.TotalCurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived);

        //                                Tcsh.ExpInventoryOut = detail.ExpQtyRecived1 - k.ExpQtyRecived;
        //                                Tcsh.TotalExpCurrentInventory = Tcs.ExpCurrentInventory - (detail.ExpQtyRecived1 - k.ExpQtyRecived);

        //                                Tcsh.DamageInventoryOut = detail.DamagQtyRecived1 - Convert.ToInt32(k.DamagQtyRecived);
        //                                Tcsh.TotalDamageCurrentInventory = Tcs.DamageCurrentInventory - (detail.DamagQtyRecived1 - Convert.ToInt32(k.DamagQtyRecived));

        //                                Tcsh.WarehouseName = Tcs.WarehouseName;
        //                                Tcsh.Warehouseid = Tcs.WarehouseId;
        //                                Tcsh.CompanyId = Tcs.CompanyId;

        //                                Tcsh.CreationDate = indianTime;
        //                                Tcsh.userid = userid;
        //                                Tcsh.ItemMultiMRPId = Tcs.ItemMultiMRPId;
        //                                db.TemporaryCurrentStockHistoryDB.Add(Tcsh);
        //                            }
        //                            Tcs.CurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived);
        //                            db.UpdateTempCurrentStock(Tcs);
        //                            /// Add Stock in current stock from Temporary current stock
        //                            CurrentStockHistory Oss = new CurrentStockHistory();
        //                            if (item != null)
        //                            {
        //                                Oss.StockId = item.StockId;
        //                                Oss.ItemNumber = item.ItemNumber;
        //                                Oss.itemname = item.itemname;
        //                                Oss.OdOrPoId = obj.PurchaseOrderId;
        //                                Oss.CurrentInventory = item.CurrentInventory;
        //                                Oss.InventoryIn = Convert.ToInt32(k.QtyRecived);
        //                                Oss.TotalInventory = item.CurrentInventory + Convert.ToInt32(k.QtyRecived);
        //                                Oss.WarehouseName = item.WarehouseName;
        //                                Oss.Warehouseid = item.WarehouseId;
        //                                Oss.CompanyId = item.CompanyId;

        //                                Oss.CreationDate = indianTime;
        //                                Oss.ManualReason = "(+) Unconfirm Stock GRN: " + sAllCharacter;
        //                                Oss.ItemMultiMRPId = item.ItemMultiMRPId;
        //                                db.CurrentStockHistoryDb.Add(Oss);
        //                            }
        //                            item.CurrentInventory = item.CurrentInventory + Convert.ToInt32(k.QtyRecived);
        //                            db.UpdateCurrentStock(item);
        //                        }
        //                        detail.QtyRecived5 = k.QtyRecived;
        //                        detail.DamagQtyRecived5 = k.DamagQtyRecived;
        //                        detail.ExpQtyRecived5 = k.ExpQtyRecived;
        //                        detail.GRDate5 = indianTime;
        //                        detail.PriceRecived = Convert.ToDouble(detail.Price1 * detail.QtyRecived1) + Convert.ToDouble(detail.Price2 * detail.QtyRecived2) + Convert.ToDouble(detail.Price3 * detail.QtyRecived3) + Convert.ToDouble(detail.Price4 * detail.QtyRecived4) + Convert.ToDouble(detail.Price5 * k.QtyRecived);
        //                        db.Commit();
        //                    }
        //                    break;
        //                default:
        //                    return null;
        //            }
        //            logger.Info("End  : ");
        //            dbContextTransaction.Commit();
        //            return Request.CreateResponse(HttpStatusCode.OK, pm); ;
        //        }
        //        catch (Exception ex)
        //        {
        //            dbContextTransaction.Rollback();
        //            logger.Error("Error in PurchaseOrderDetail " + ex.Message);
        //            logger.Info("End  PurchaseOrderDetail: ");
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, "");
        //        }
        //    }
        //}

        /// <summary>
        /// GetUnconfirmGrData
        /// </summary>
        /// <returns></returns>
        [Route("GetUnconfirmGrData")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetUnconfirmGrData(int wid)
        {
            logger.Info("start : ");
            try
            {
                // Access claims
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                int CompanyId = compid;
                List<PurchaseOrderMaster> pm = new List<PurchaseOrderMaster>();
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                var predicate = PredicateBuilder.True<PurchaseOrderMaster>();
                predicate = predicate.And(x => x.Gr1Status == "Pending for Checker Side" || x.Gr2Status == "Pending for Checker Side" || x.Gr3Status == "Pending for Checker Side" || x.Gr4Status == "Pending for Checker Side" || x.Gr5Status == "Pending for Checker Side");
                predicate = predicate.And(e => e.WarehouseId == wid);
                List<GetUCGRDTO> HeadData = new List<GetUCGRDTO>();
                using (AuthContext db = new AuthContext())
                {
                    pm = db.DPurchaseOrderMaster.Where(predicate).ToList();

                    foreach (PurchaseOrderMaster a in pm)
                    {
                        List<PurchaseOrderDetailRecived> _detail = db.PurchaseOrderRecivedDetails.Where(q => q.PurchaseOrderId == a.PurchaseOrderId).ToList();

                        if (a.Gr1_Amount != 0 && a.Gr1Status == "Pending for Checker Side")
                        {
                            List<GetListUCGRDTO> HeadDetail = new List<GetListUCGRDTO>();
                            foreach (PurchaseOrderDetailRecived b in _detail)
                            {
                                GetListUCGRDTO UCL = new GetListUCGRDTO()
                                {
                                    PurchaseOrderDetailRecivedId = b.PurchaseOrderDetailRecivedId,
                                    ItemId = b.ItemId,
                                    ItemName = b.ItemName1,
                                    Price = Convert.ToDouble(b.Price1),
                                    HSNCode = b.HSNCode,
                                    TotalQuantity = b.TotalQuantity,
                                    QtyRecived = Convert.ToInt32(b.QtyRecived1),
                                    DamagQtyRecived = b.DamagQtyRecived1,
                                    ExpQtyRecived = b.ExpQtyRecived1,
                                    BatchNo = b.BatchNo1,
                                    MFG = b.MFGDate1
                                };
                                HeadDetail.Add(UCL);
                            }

                            GetUCGRDTO UC = new GetUCGRDTO()
                            {
                                PurchaseOrderId = a.PurchaseOrderId,
                                GrNumber = a.Gr1Number,
                                GrPersonName = a.Gr1PersonName,
                                GrDate = Convert.ToDateTime(a.Gr1_Date),
                                GrAmount = a.Gr1_Amount,
                                VehicleType = a.VehicleType1,
                                VehicleNumber = a.VehicleNumber1,
                                Status = a.Gr1Status,
                                Detail = HeadDetail

                            };
                            HeadData.Add(UC);
                        }
                        if (a.Gr2_Amount != 0 && a.Gr2Status == "Pending for Checker Side")
                        {
                            List<GetListUCGRDTO> HeadDetail = new List<GetListUCGRDTO>();
                            foreach (PurchaseOrderDetailRecived b in _detail)
                            {
                                GetListUCGRDTO UCL = new GetListUCGRDTO()
                                {
                                    PurchaseOrderDetailRecivedId = b.PurchaseOrderDetailRecivedId,
                                    ItemId = b.ItemId,
                                    ItemName = b.ItemName2,
                                    Price = Convert.ToDouble(b.Price2),
                                    HSNCode = b.HSNCode,
                                    TotalQuantity = b.TotalQuantity,
                                    QtyRecived = Convert.ToInt32(b.QtyRecived2),
                                    DamagQtyRecived = b.DamagQtyRecived2,
                                    ExpQtyRecived = b.ExpQtyRecived2,
                                    BatchNo = b.BatchNo2,
                                    MFG = b.MFGDate2
                                };
                                HeadDetail.Add(UCL);
                            }

                            GetUCGRDTO UC = new GetUCGRDTO()
                            {
                                PurchaseOrderId = a.PurchaseOrderId,
                                GrNumber = a.Gr2Number,
                                GrPersonName = a.Gr2PersonName,
                                GrDate = Convert.ToDateTime(a.Gr2_Date),
                                GrAmount = a.Gr2_Amount,
                                VehicleType = a.VehicleType2,
                                VehicleNumber = a.VehicleNumber2,
                                Status = a.Gr2Status,
                                Detail = HeadDetail

                            };
                            HeadData.Add(UC);
                        }
                        if (a.Gr3_Amount != 0 && a.Gr3Status == "Pending for Checker Side")
                        {
                            List<GetListUCGRDTO> HeadDetail = new List<GetListUCGRDTO>();
                            foreach (PurchaseOrderDetailRecived b in _detail)
                            {
                                GetListUCGRDTO UCL = new GetListUCGRDTO()
                                {
                                    PurchaseOrderDetailRecivedId = b.PurchaseOrderDetailRecivedId,
                                    ItemId = b.ItemId,
                                    ItemName = b.ItemName3,
                                    Price = Convert.ToDouble(b.Price3),
                                    HSNCode = b.HSNCode,
                                    TotalQuantity = b.TotalQuantity,
                                    QtyRecived = Convert.ToInt32(b.QtyRecived3),
                                    DamagQtyRecived = b.DamagQtyRecived3,
                                    ExpQtyRecived = b.ExpQtyRecived3,
                                    BatchNo = b.BatchNo3,
                                    MFG = b.MFGDate3
                                };
                                HeadDetail.Add(UCL);
                            }

                            GetUCGRDTO UC = new GetUCGRDTO()
                            {
                                PurchaseOrderId = a.PurchaseOrderId,
                                GrNumber = a.Gr3Number,
                                GrPersonName = a.Gr3PersonName,
                                GrDate = Convert.ToDateTime(a.Gr3_Date),
                                GrAmount = a.Gr3_Amount,
                                VehicleType = a.VehicleType3,
                                VehicleNumber = a.VehicleNumber3,
                                Status = a.Gr3Status,
                                Detail = HeadDetail

                            };
                            HeadData.Add(UC);
                        }
                        if (a.Gr4_Amount != 0 && a.Gr4Status == "Pending for Checker Side")
                        {
                            List<GetListUCGRDTO> HeadDetail = new List<GetListUCGRDTO>();
                            foreach (PurchaseOrderDetailRecived b in _detail)
                            {
                                GetListUCGRDTO UCL = new GetListUCGRDTO()
                                {
                                    PurchaseOrderDetailRecivedId = b.PurchaseOrderDetailRecivedId,
                                    ItemId = b.ItemId,
                                    ItemName = b.ItemName4,
                                    Price = Convert.ToDouble(b.Price4),
                                    HSNCode = b.HSNCode,
                                    TotalQuantity = b.TotalQuantity,
                                    QtyRecived = Convert.ToInt32(b.QtyRecived4),
                                    DamagQtyRecived = b.DamagQtyRecived4,
                                    ExpQtyRecived = b.ExpQtyRecived4,
                                    BatchNo = b.BatchNo4,
                                    MFG = b.MFGDate4
                                };
                                HeadDetail.Add(UCL);
                            }

                            GetUCGRDTO UC = new GetUCGRDTO()
                            {
                                PurchaseOrderId = a.PurchaseOrderId,
                                GrNumber = a.Gr4Number,
                                GrPersonName = a.Gr4PersonName,
                                GrDate = Convert.ToDateTime(a.Gr4_Date),
                                GrAmount = a.Gr4_Amount,
                                VehicleType = a.VehicleType4,
                                VehicleNumber = a.VehicleNumber4,
                                Status = a.Gr4Status,
                                Detail = HeadDetail

                            };
                            HeadData.Add(UC);
                        }
                        if (a.Gr5_Amount != 0 && a.Gr5Status == "Pending for Checker Side")
                        {
                            List<GetListUCGRDTO> HeadDetail = new List<GetListUCGRDTO>();
                            foreach (PurchaseOrderDetailRecived b in _detail)
                            {
                                GetListUCGRDTO UCL = new GetListUCGRDTO()
                                {
                                    PurchaseOrderDetailRecivedId = b.PurchaseOrderDetailRecivedId,
                                    ItemId = b.ItemId,
                                    ItemName = b.ItemName5,
                                    Price = Convert.ToDouble(b.Price5),
                                    HSNCode = b.HSNCode,
                                    TotalQuantity = b.TotalQuantity,
                                    QtyRecived = Convert.ToInt32(b.QtyRecived5),
                                    DamagQtyRecived = b.DamagQtyRecived5,
                                    ExpQtyRecived = b.ExpQtyRecived5,
                                    BatchNo = b.BatchNo5,
                                    MFG = b.MFGDate5
                                };
                                HeadDetail.Add(UCL);
                            }

                            GetUCGRDTO UC = new GetUCGRDTO()
                            {
                                PurchaseOrderId = a.PurchaseOrderId,
                                GrNumber = a.Gr5Number,
                                GrPersonName = a.Gr5PersonName,
                                GrDate = Convert.ToDateTime(a.Gr5_Date),
                                GrAmount = a.Gr5_Amount,
                                VehicleType = a.VehicleType5,
                                VehicleNumber = a.VehicleNumber5,
                                Status = a.Gr5Status,
                                Detail = HeadDetail
                            };
                            HeadData.Add(UC);
                        }

                    }
                }
                logger.Info("End  : ");
                return Request.CreateResponse(HttpStatusCode.OK, HeadData.OrderByDescending(w => w.PurchaseOrderId)); ;
            }
            catch (Exception ex)
            {
                logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                logger.Info("End  PurchaseOrderDetail: ");
                return Request.CreateResponse(HttpStatusCode.BadRequest, "");
            }
        }

        //#region Approved Gr Details in PO
        ///// <summary>
        ///// Created By Raj
        ///// Created Date:30/08/2019
        ///// Approved GR 
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <returns></returns>
        //[Route("ApprovedGrDetail")]
        //[HttpPost]
        //public HttpResponseMessage ApprovedGrDetail(PutUCGRDTO obj)
        //{
        //    logger.Info("start : ");

        //    // Access claims
        //    var identity = User.Identity as ClaimsIdentity;
        //    int compid = 0, userid = 0;
        //    int Warehouse_id = 0;

        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
        //        Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
        //    int CompanyId = compid;

        //    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

        //    using (AuthContext db = new AuthContext())
        //    using (var dbContextTransaction = db.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            var people = db.Peoples.Where(x => x.PeopleID == userid && x.Deleted == false && x.Active).SingleOrDefault();
        //            bool IsRec = true;
        //            string sAllCharacter = obj.GrNumber;
        //            char GRType = sAllCharacter[sAllCharacter.Length - 1];
        //            PurchaseOrderMaster pm = new PurchaseOrderMaster();


        //            switch (GRType)
        //            {
        //                case 'A':

        //                    pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
        //                    List<PurchaseOrderDetailRecived> details = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).ToList();

        //                    if (pm.Gr1Status != "Approved")
        //                    {
        //                        pm.Gr1Status = "Approved";

        //                        foreach (var k in details)
        //                        {
        //                            TemporaryCurrentStock Tcs = db.TemporaryCurrentStockDB.Where(x => x.ItemNumber == k.ItemNumber && x.WarehouseId == k.WarehouseId && x.CompanyId == k.CompanyId && x.ItemMultiMRPId == k.ItemMultiMRPId1).SingleOrDefault();
        //                            if (k.QtyRecived != 0)
        //                            {
        //                                /// Deduct Stock from Temporary current stock
        //                                TemporaryCurrentStockHistory Tcsh = new TemporaryCurrentStockHistory();
        //                                if (Tcs != null)
        //                                {
        //                                    Tcsh.StockId = Tcs.Id;
        //                                    Tcsh.ItemNumber = Tcs.ItemNumber;
        //                                    Tcsh.itemname = Tcs.itemname;
        //                                    Tcsh.OdOrPoId = obj.PurchaseOrderId;

        //                                    Tcsh.CurrentInventory = Tcs.CurrentInventory;
        //                                    Tcsh.InventoryOut = Convert.ToInt32(k.QtyRecived1);
        //                                    Tcsh.TotalCurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived1);

        //                                    Tcsh.ExpInventoryOut = k.ExpQtyRecived1;
        //                                    Tcsh.TotalExpCurrentInventory = Tcs.ExpCurrentInventory;

        //                                    Tcsh.DamageInventoryOut = k.DamagQtyRecived1;
        //                                    Tcsh.TotalDamageCurrentInventory = Tcs.DamageCurrentInventory;

        //                                    Tcsh.WarehouseName = Tcs.WarehouseName;
        //                                    Tcsh.Warehouseid = Tcs.WarehouseId;
        //                                    Tcsh.CompanyId = Tcs.CompanyId;

        //                                    Tcsh.CreationDate = indianTime;
        //                                    Tcsh.userid = userid;
        //                                    Tcsh.ItemMultiMRPId = Tcs.ItemMultiMRPId;
        //                                    db.TemporaryCurrentStockHistoryDB.Add(Tcsh);
        //                                    Tcs.CurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived1);
        //                                    db.UpdateTempCurrentStock(Tcs);


        //                                    /// Add Stock in current stock from Temporary current stock
        //                                    CurrentStock item = db.DbCurrentStock.Where(x => x.ItemNumber == k.ItemNumber && x.WarehouseId == k.WarehouseId && x.CompanyId == k.CompanyId && x.ItemMultiMRPId == k.ItemMultiMRPId1 && !x.Deleted).SingleOrDefault();
        //                                    ItemMultiMRP MRPdetail = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == k.ItemMultiMRPId1).SingleOrDefault();
        //                                    if (item == null)
        //                                    {
        //                                        CurrentStock NewStock = new CurrentStock();
        //                                        NewStock.CompanyId = Tcs.CompanyId;
        //                                        NewStock.CreationDate = indianTime;
        //                                        NewStock.CurrentInventory = 0;
        //                                        NewStock.Deleted = false;
        //                                        NewStock.ItemMultiMRPId = k.ItemMultiMRPId1;
        //                                        NewStock.itemname = k.ItemName;
        //                                        NewStock.ItemNumber = k.ItemNumber;
        //                                        NewStock.itemBaseName = Tcs.itemBaseName;

        //                                        NewStock.UpdatedDate = indianTime;
        //                                        NewStock.WarehouseId = Tcs.WarehouseId;
        //                                        NewStock.WarehouseName = Tcs.WarehouseName;
        //                                        NewStock.MRP = MRPdetail.MRP;
        //                                        NewStock.UOM = MRPdetail.UOM;
        //                                        NewStock.userid = userid;
        //                                        db.DbCurrentStock.Add(NewStock);
        //                                        db.Commit();
        //                                    }
        //                                    // Recall
        //                                    item = db.DbCurrentStock.Where(x => x.ItemNumber == k.ItemNumber && x.WarehouseId == k.WarehouseId && x.CompanyId == k.CompanyId && x.ItemMultiMRPId == k.ItemMultiMRPId1 && !x.Deleted).SingleOrDefault();
        //                                    CurrentStockHistory Oss = new CurrentStockHistory();
        //                                    if (item != null)
        //                                    {
        //                                        Oss.StockId = item.StockId;
        //                                        Oss.ItemNumber = item.ItemNumber;
        //                                        Oss.itemname = item.itemname;
        //                                        Oss.OdOrPoId = obj.PurchaseOrderId;
        //                                        Oss.CurrentInventory = item.CurrentInventory;
        //                                        Oss.InventoryIn = Convert.ToInt32(k.QtyRecived1);
        //                                        Oss.TotalInventory = item.CurrentInventory + Convert.ToInt32(k.QtyRecived1);
        //                                        Oss.WarehouseName = item.WarehouseName;
        //                                        Oss.Warehouseid = item.WarehouseId;
        //                                        Oss.CompanyId = item.CompanyId;
        //                                        Oss.CreationDate = indianTime;
        //                                        Oss.ManualReason = "(+)Stock GRN: " + sAllCharacter;
        //                                        Oss.ItemMultiMRPId = item.ItemMultiMRPId;
        //                                        Oss.MRP = item.MRP;
        //                                        Oss.UOM = item.UOM;
        //                                        Oss.userid = people.PeopleID;
        //                                        Oss.UserName = people.DisplayName;
        //                                        db.CurrentStockHistoryDb.Add(Oss);
        //                                        item.CurrentInventory = item.CurrentInventory + Convert.ToInt32(k.QtyRecived1);
        //                                        db.UpdateCurrentStock(item);
        //                                    }

        //                                }
        //                                else
        //                                {
        //                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Temp Stock not exist.");
        //                                }
        //                            }
        //                            if (IsRec == true)
        //                            {
        //                                if (k.TotalQuantity > (k.QtyRecived1 + k.QtyRecived2 + k.QtyRecived3 + k.QtyRecived4 + k.QtyRecived5))
        //                                {
        //                                    IsRec = false;
        //                                }
        //                                else
        //                                {
        //                                    IsRec = true;
        //                                }
        //                            }
        //                            db.Commit();
        //                        }

        //                        #region ass Free item stock in Freestock table
        //                        List<FreeItem> FI = db.FreeItemDb.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId && a.GRNumber == obj.GrNumber).ToList();
        //                        var UserName = db.Peoples.Where(y => y.PeopleID == userid).Select(b => b.DisplayName).SingleOrDefault();

        //                        foreach (FreeItem f in FI)
        //                        {
        //                            FreeStock stok = db.FreeStockDB.Where(x => x.ItemNumber == f.itemNumber && x.WarehouseId == f.WarehouseId && x.ItemMultiMRPId == f.ItemMultiMRPId).FirstOrDefault();
        //                            ItemMultiMRP MRP = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == f.ItemMultiMRPId).SingleOrDefault();
        //                            if (stok != null)
        //                            {
        //                                FreeStockHistory Oss = new FreeStockHistory();
        //                                Oss.ManualReason = "Free Item";
        //                                Oss.FreeStockId = stok.FreeStockId;
        //                                Oss.ItemMultiMRPId = stok.ItemMultiMRPId;
        //                                Oss.ItemNumber = stok.ItemNumber;
        //                                Oss.itemname = stok.itemname;
        //                                Oss.OdOrPoId = obj.PurchaseOrderId;
        //                                Oss.CurrentInventory = f.TotalQuantity;
        //                                Oss.InventoryIn = f.TotalQuantity;
        //                                Oss.TotalInventory = Convert.ToInt32(stok.CurrentInventory + f.TotalQuantity);
        //                                Oss.WarehouseId = stok.WarehouseId;
        //                                Oss.CreationDate = DateTime.Now;
        //                                db.FreeStockHistoryDB.Add(Oss);
        //                                stok.CurrentInventory = stok.CurrentInventory + f.TotalQuantity;
        //                                if (stok.CurrentInventory < 0)
        //                                {
        //                                    stok.CurrentInventory = 0;
        //                                }
        //                                db.Entry(stok).State = EntityState.Modified;
        //                            }
        //                            else
        //                            {
        //                                FreeStock FSN = new FreeStock();
        //                                FSN.ItemNumber = f.itemNumber;
        //                                FSN.itemname = f.itemname;
        //                                FSN.ItemMultiMRPId = f.ItemMultiMRPId;
        //                                FSN.MRP = MRP.MRP;
        //                                FSN.WarehouseId = Convert.ToInt32(f.WarehouseId);
        //                                FSN.CurrentInventory = f.TotalQuantity;
        //                                FSN.CreatedBy = UserName;
        //                                FSN.CreationDate = indianTime;
        //                                FSN.Deleted = false;
        //                                FSN.UpdatedDate = indianTime;
        //                                db.FreeStockDB.Add(FSN);
        //                                db.Commit();
        //                                FreeStockHistory Oss = new FreeStockHistory();
        //                                Oss.ManualReason = "Free Item";
        //                                Oss.FreeStockId = FSN.FreeStockId;
        //                                Oss.ItemMultiMRPId = FSN.ItemMultiMRPId;
        //                                Oss.ItemNumber = FSN.ItemNumber;
        //                                Oss.itemname = FSN.itemname;
        //                                Oss.OdOrPoId = f.PurchaseOrderId;
        //                                Oss.CurrentInventory = f.TotalQuantity;
        //                                Oss.InventoryIn = f.TotalQuantity;
        //                                Oss.TotalInventory = Convert.ToInt32(FSN.CurrentInventory);
        //                                Oss.WarehouseId = FSN.WarehouseId;
        //                                Oss.CreationDate = DateTime.Now;
        //                                db.FreeStockHistoryDB.Add(Oss);
        //                            }
        //                        }

        //                        if (IsRec)
        //                        {
        //                            pm.progress = "100";
        //                            pm.Status = "CN Received";
        //                        }
        //                        else { pm.Status = "CN Partial Received"; }
        //                        db.Commit();
        //                        #endregion
        //                    }
        //                    else
        //                    {
        //                        return Request.CreateResponse(HttpStatusCode.BadRequest, "GR already Approved.");
        //                    }
        //                    break;
        //                case 'B':
        //                    pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
        //                    List<PurchaseOrderDetailRecived> detailsB = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).ToList();
        //                    if (pm.Gr2Status != "Approved")
        //                    {
        //                        pm.Gr2Status = "Approved";

        //                        foreach (var k in detailsB)
        //                        {

        //                            TemporaryCurrentStock Tcs = db.TemporaryCurrentStockDB.Where(x => x.ItemNumber == k.ItemNumber && x.WarehouseId == k.WarehouseId && x.CompanyId == k.CompanyId && x.ItemMultiMRPId == k.ItemMultiMRPId2).SingleOrDefault();
        //                            if (k.QtyRecived != 0)
        //                            {
        //                                /// Deduct Stock from Temporary current stock
        //                                TemporaryCurrentStockHistory Tcsh = new TemporaryCurrentStockHistory();
        //                                if (Tcs != null)
        //                                {
        //                                    Tcsh.StockId = Tcs.Id;
        //                                    Tcsh.ItemNumber = Tcs.ItemNumber;
        //                                    Tcsh.itemname = Tcs.itemname;
        //                                    Tcsh.OdOrPoId = obj.PurchaseOrderId;

        //                                    Tcsh.CurrentInventory = Tcs.CurrentInventory;
        //                                    Tcsh.InventoryOut = Convert.ToInt32(k.QtyRecived2);
        //                                    Tcsh.TotalCurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived2);

        //                                    Tcsh.ExpInventoryOut = k.ExpQtyRecived2;
        //                                    Tcsh.TotalExpCurrentInventory = Tcs.ExpCurrentInventory;

        //                                    Tcsh.DamageInventoryOut = k.DamagQtyRecived2;
        //                                    Tcsh.TotalDamageCurrentInventory = Tcs.DamageCurrentInventory;

        //                                    Tcsh.WarehouseName = Tcs.WarehouseName;
        //                                    Tcsh.Warehouseid = Tcs.WarehouseId;
        //                                    Tcsh.CompanyId = Tcs.CompanyId;

        //                                    Tcsh.CreationDate = indianTime;
        //                                    Tcsh.userid = userid;
        //                                    Tcsh.ItemMultiMRPId = Tcs.ItemMultiMRPId;
        //                                    db.TemporaryCurrentStockHistoryDB.Add(Tcsh);
        //                                    Tcs.CurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived2);
        //                                    db.UpdateTempCurrentStock(Tcs);

        //                                    /// Add Stock in current stock from Temporary current stock
        //                                    CurrentStock item = db.DbCurrentStock.Where(x => x.ItemNumber == k.ItemNumber && x.WarehouseId == k.WarehouseId && x.CompanyId == k.CompanyId && x.ItemMultiMRPId == k.ItemMultiMRPId2 && !x.Deleted).SingleOrDefault();
        //                                    ItemMultiMRP MRPdetail = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == k.ItemMultiMRPId2).SingleOrDefault();

        //                                    if (item == null)
        //                                    {
        //                                        CurrentStock NewStock = new CurrentStock();
        //                                        NewStock.CompanyId = Tcs.CompanyId;
        //                                        NewStock.CreationDate = indianTime;
        //                                        NewStock.CurrentInventory = 0;
        //                                        NewStock.Deleted = false;
        //                                        NewStock.ItemMultiMRPId = k.ItemMultiMRPId2;
        //                                        NewStock.itemname = k.ItemName;
        //                                        NewStock.ItemNumber = k.ItemNumber;
        //                                        NewStock.itemBaseName = Tcs.itemBaseName;
        //                                        // NewStock.ItemId = Tcs.ItemId;
        //                                        NewStock.UpdatedDate = indianTime;
        //                                        NewStock.WarehouseId = Tcs.WarehouseId;
        //                                        NewStock.WarehouseName = Tcs.WarehouseName;
        //                                        NewStock.MRP = MRPdetail.MRP;
        //                                        NewStock.UOM = MRPdetail.UOM;
        //                                        NewStock.userid = userid;
        //                                        db.DbCurrentStock.Add(NewStock);
        //                                        db.Commit();
        //                                    }
        //                                    // Recall
        //                                    item = db.DbCurrentStock.Where(x => x.ItemNumber == k.ItemNumber && x.WarehouseId == k.WarehouseId && x.CompanyId == k.CompanyId && x.ItemMultiMRPId == k.ItemMultiMRPId2 && !x.Deleted).SingleOrDefault();
        //                                    CurrentStockHistory Oss = new CurrentStockHistory();
        //                                    if (item != null)
        //                                    {
        //                                        Oss.StockId = item.StockId;
        //                                        Oss.ItemNumber = item.ItemNumber;
        //                                        Oss.itemname = item.itemname;
        //                                        Oss.OdOrPoId = obj.PurchaseOrderId;
        //                                        Oss.CurrentInventory = item.CurrentInventory;
        //                                        Oss.InventoryIn = Convert.ToInt32(k.QtyRecived2);
        //                                        Oss.TotalInventory = item.CurrentInventory + Convert.ToInt32(k.QtyRecived2);
        //                                        Oss.WarehouseName = item.WarehouseName;
        //                                        Oss.Warehouseid = item.WarehouseId;
        //                                        Oss.CompanyId = item.CompanyId;
        //                                        Oss.CreationDate = indianTime;
        //                                        Oss.ManualReason = "(+)Stock GRN: " + sAllCharacter;
        //                                        Oss.ItemMultiMRPId = item.ItemMultiMRPId;
        //                                        Oss.MRP = item.MRP;
        //                                        Oss.UOM = item.UOM;
        //                                        Oss.userid = people.PeopleID;
        //                                        Oss.UserName = people.DisplayName;
        //                                        db.CurrentStockHistoryDb.Add(Oss);
        //                                        item.CurrentInventory = item.CurrentInventory + Convert.ToInt32(k.QtyRecived2);
        //                                        db.UpdateCurrentStock(item);
        //                                    }

        //                                }
        //                                else
        //                                {
        //                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Temp Stock not exist.");
        //                                }
        //                            }
        //                            if (IsRec == true)
        //                            {
        //                                if (k.TotalQuantity > (k.QtyRecived1 + k.QtyRecived2 + k.QtyRecived3 + k.QtyRecived4 + k.QtyRecived5))
        //                                {
        //                                    IsRec = false;
        //                                }
        //                                else
        //                                {
        //                                    IsRec = true;
        //                                }
        //                            }
        //                            db.Commit();
        //                        }
        //                        #region ass Free item stock in Freestock table
        //                        List<FreeItem> FI = db.FreeItemDb.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId && a.GRNumber == obj.GrNumber).ToList();
        //                        var UserName = db.Peoples.Where(y => y.PeopleID == userid).Select(b => b.DisplayName).SingleOrDefault();
        //                        foreach (FreeItem f in FI)
        //                        {
        //                            FreeStock stok = db.FreeStockDB.Where(x => x.ItemNumber == f.itemNumber && x.WarehouseId == f.WarehouseId && x.ItemMultiMRPId == f.ItemMultiMRPId).FirstOrDefault();
        //                            ItemMultiMRP MRP = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == f.ItemMultiMRPId).SingleOrDefault();
        //                            if (stok != null)
        //                            {
        //                                FreeStockHistory Oss = new FreeStockHistory();
        //                                Oss.ManualReason = "Free Item";
        //                                Oss.FreeStockId = stok.FreeStockId;
        //                                Oss.ItemMultiMRPId = stok.ItemMultiMRPId;
        //                                Oss.ItemNumber = stok.ItemNumber;
        //                                Oss.itemname = stok.itemname;
        //                                Oss.OdOrPoId = obj.PurchaseOrderId;
        //                                Oss.CurrentInventory = f.TotalQuantity;
        //                                Oss.InventoryIn = f.TotalQuantity;
        //                                Oss.TotalInventory = Convert.ToInt32(stok.CurrentInventory + f.TotalQuantity);
        //                                Oss.WarehouseId = stok.WarehouseId;
        //                                Oss.CreationDate = DateTime.Now;
        //                                db.FreeStockHistoryDB.Add(Oss);
        //                                stok.CurrentInventory = stok.CurrentInventory + f.TotalQuantity;
        //                                if (stok.CurrentInventory < 0)
        //                                {
        //                                    stok.CurrentInventory = 0;
        //                                }
        //                                db.Entry(stok).State = EntityState.Modified;
        //                            }
        //                            else
        //                            {
        //                                FreeStock FSN = new FreeStock();
        //                                FSN.ItemNumber = f.itemNumber;
        //                                FSN.itemname = f.itemname;
        //                                FSN.ItemMultiMRPId = f.ItemMultiMRPId;
        //                                FSN.MRP = MRP.MRP;
        //                                FSN.WarehouseId = Convert.ToInt32(f.WarehouseId);
        //                                FSN.CurrentInventory = f.TotalQuantity;
        //                                FSN.CreatedBy = UserName;
        //                                FSN.CreationDate = indianTime;
        //                                FSN.Deleted = false;
        //                                FSN.UpdatedDate = indianTime;
        //                                db.FreeStockDB.Add(FSN);
        //                                db.Commit();
        //                                FreeStockHistory Oss = new FreeStockHistory();
        //                                Oss.ManualReason = "Free Item";
        //                                Oss.FreeStockId = FSN.FreeStockId;
        //                                Oss.ItemMultiMRPId = FSN.ItemMultiMRPId;
        //                                Oss.ItemNumber = FSN.ItemNumber;
        //                                Oss.itemname = FSN.itemname;
        //                                Oss.OdOrPoId = f.PurchaseOrderId;
        //                                Oss.CurrentInventory = f.TotalQuantity;
        //                                Oss.InventoryIn = f.TotalQuantity;
        //                                Oss.TotalInventory = Convert.ToInt32(FSN.CurrentInventory);
        //                                Oss.WarehouseId = FSN.WarehouseId;
        //                                Oss.CreationDate = DateTime.Now;
        //                                db.FreeStockHistoryDB.Add(Oss);
        //                            }
        //                        }
        //                        if (IsRec)
        //                        {
        //                            pm.progress = "100";
        //                            pm.Status = "CN Received";
        //                        }
        //                        else { pm.Status = "CN Partial Received"; }
        //                        db.Commit();
        //                        #endregion
        //                    }
        //                    else
        //                    {
        //                        return Request.CreateResponse(HttpStatusCode.BadRequest, "GR already Approved.");
        //                    }
        //                    break;
        //                case 'C':
        //                    pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
        //                    List<PurchaseOrderDetailRecived> detailsC = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).ToList();
        //                    if (pm.Gr3Status != "Approved")
        //                    {
        //                        pm.Gr3Status = "Approved";

        //                        foreach (var k in detailsC)
        //                        {
        //                            TemporaryCurrentStock Tcs = db.TemporaryCurrentStockDB.Where(x => x.ItemNumber == k.ItemNumber && x.WarehouseId == k.WarehouseId && x.CompanyId == k.CompanyId && x.ItemMultiMRPId == k.ItemMultiMRPId3).SingleOrDefault();

        //                            if (k.QtyRecived != 0)
        //                            {
        //                                /// Deduct Stock from Temporary current stock
        //                                TemporaryCurrentStockHistory Tcsh = new TemporaryCurrentStockHistory();
        //                                if (Tcs != null)
        //                                {
        //                                    Tcsh.StockId = Tcs.Id;
        //                                    Tcsh.ItemNumber = Tcs.ItemNumber;
        //                                    Tcsh.itemname = Tcs.itemname;
        //                                    Tcsh.OdOrPoId = obj.PurchaseOrderId;

        //                                    Tcsh.CurrentInventory = Tcs.CurrentInventory;
        //                                    Tcsh.InventoryOut = Convert.ToInt32(k.QtyRecived3);
        //                                    Tcsh.TotalCurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived3);

        //                                    Tcsh.ExpInventoryOut = k.ExpQtyRecived3;
        //                                    Tcsh.TotalExpCurrentInventory = Tcs.ExpCurrentInventory;

        //                                    Tcsh.DamageInventoryOut = k.DamagQtyRecived3;
        //                                    Tcsh.TotalDamageCurrentInventory = Tcs.DamageCurrentInventory;

        //                                    Tcsh.WarehouseName = Tcs.WarehouseName;
        //                                    Tcsh.Warehouseid = Tcs.WarehouseId;
        //                                    Tcsh.CompanyId = Tcs.CompanyId;

        //                                    Tcsh.CreationDate = indianTime;
        //                                    Tcsh.userid = userid;
        //                                    Tcsh.ItemMultiMRPId = Tcs.ItemMultiMRPId;
        //                                    db.TemporaryCurrentStockHistoryDB.Add(Tcsh);
        //                                    Tcs.CurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived3);
        //                                    db.UpdateTempCurrentStock(Tcs);

        //                                    /// Add Stock in current stock from Temporary current stock
        //                                    CurrentStock item = db.DbCurrentStock.Where(x => x.ItemNumber == k.ItemNumber && x.WarehouseId == k.WarehouseId && x.CompanyId == k.CompanyId && x.ItemMultiMRPId == k.ItemMultiMRPId3 && !x.Deleted).SingleOrDefault();
        //                                    ItemMultiMRP MRPdetail = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == k.ItemMultiMRPId3).SingleOrDefault();

        //                                    if (item == null)
        //                                    {
        //                                        CurrentStock NewStock = new CurrentStock();
        //                                        NewStock.CompanyId = Tcs.CompanyId;
        //                                        NewStock.CreationDate = indianTime;
        //                                        NewStock.CurrentInventory = 0;
        //                                        NewStock.Deleted = false;
        //                                        NewStock.ItemMultiMRPId = k.ItemMultiMRPId3;
        //                                        NewStock.itemname = k.ItemName;
        //                                        NewStock.ItemNumber = k.ItemNumber;
        //                                        NewStock.itemBaseName = Tcs.itemBaseName;
        //                                        //NewStock.ItemId = Tcs.ItemId;
        //                                        NewStock.UpdatedDate = indianTime;
        //                                        NewStock.WarehouseId = Tcs.WarehouseId;
        //                                        NewStock.WarehouseName = Tcs.WarehouseName;
        //                                        NewStock.MRP = MRPdetail.MRP;
        //                                        NewStock.UOM = MRPdetail.UOM;
        //                                        NewStock.userid = userid;
        //                                        db.DbCurrentStock.Add(NewStock);
        //                                        db.Commit();
        //                                    }
        //                                    // Recall
        //                                    item = db.DbCurrentStock.Where(x => x.ItemNumber == k.ItemNumber && x.WarehouseId == k.WarehouseId && x.CompanyId == k.CompanyId && x.ItemMultiMRPId == k.ItemMultiMRPId3 && !x.Deleted).SingleOrDefault();

        //                                    CurrentStockHistory Oss = new CurrentStockHistory();
        //                                    if (item != null)
        //                                    {
        //                                        Oss.StockId = item.StockId;
        //                                        Oss.ItemNumber = item.ItemNumber;
        //                                        Oss.itemname = item.itemname;
        //                                        Oss.OdOrPoId = obj.PurchaseOrderId;
        //                                        Oss.CurrentInventory = item.CurrentInventory;
        //                                        Oss.InventoryIn = Convert.ToInt32(k.QtyRecived3);
        //                                        Oss.TotalInventory = item.CurrentInventory + Convert.ToInt32(k.QtyRecived3);
        //                                        Oss.WarehouseName = item.WarehouseName;
        //                                        Oss.Warehouseid = item.WarehouseId;
        //                                        Oss.CompanyId = item.CompanyId;
        //                                        Oss.CreationDate = indianTime;
        //                                        Oss.ManualReason = "(+)Stock GRN: " + sAllCharacter;
        //                                        Oss.ItemMultiMRPId = item.ItemMultiMRPId;
        //                                        Oss.MRP = item.MRP;
        //                                        Oss.UOM = item.UOM;
        //                                        Oss.userid = people.PeopleID;
        //                                        Oss.UserName = people.DisplayName;
        //                                        db.CurrentStockHistoryDb.Add(Oss);
        //                                        item.CurrentInventory = item.CurrentInventory + Convert.ToInt32(k.QtyRecived3);
        //                                        db.UpdateCurrentStock(item);
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Temp Stock not exist.");
        //                                }
        //                            }
        //                            if (IsRec == true)
        //                            {
        //                                if (k.TotalQuantity > (k.QtyRecived1 + k.QtyRecived2 + k.QtyRecived3 + k.QtyRecived4 + k.QtyRecived5))
        //                                {
        //                                    IsRec = false;
        //                                }
        //                                else
        //                                {
        //                                    IsRec = true;
        //                                }
        //                            }
        //                            db.Commit();
        //                        }
        //                        #region ass Free item stock in Freestock table
        //                        List<FreeItem> FI = db.FreeItemDb.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId && a.GRNumber == obj.GrNumber).ToList();
        //                        var UserName = db.Peoples.Where(y => y.PeopleID == userid).Select(b => b.DisplayName).SingleOrDefault();
        //                        foreach (FreeItem f in FI)
        //                        {
        //                            FreeStock stok = db.FreeStockDB.Where(x => x.ItemNumber == f.itemNumber && x.WarehouseId == f.WarehouseId && x.ItemMultiMRPId == f.ItemMultiMRPId).FirstOrDefault();
        //                            ItemMultiMRP MRP = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == f.ItemMultiMRPId).SingleOrDefault();
        //                            if (stok != null)
        //                            {
        //                                FreeStockHistory Oss = new FreeStockHistory();
        //                                Oss.ManualReason = "Free Item";
        //                                Oss.FreeStockId = stok.FreeStockId;
        //                                Oss.ItemMultiMRPId = stok.ItemMultiMRPId;
        //                                Oss.ItemNumber = stok.ItemNumber;
        //                                Oss.itemname = stok.itemname;
        //                                Oss.OdOrPoId = obj.PurchaseOrderId;
        //                                Oss.CurrentInventory = f.TotalQuantity;
        //                                Oss.InventoryIn = f.TotalQuantity;
        //                                Oss.TotalInventory = Convert.ToInt32(stok.CurrentInventory + f.TotalQuantity);
        //                                Oss.WarehouseId = stok.WarehouseId;
        //                                Oss.CreationDate = DateTime.Now;
        //                                db.FreeStockHistoryDB.Add(Oss);
        //                                stok.CurrentInventory = stok.CurrentInventory + f.TotalQuantity;
        //                                if (stok.CurrentInventory < 0)
        //                                {
        //                                    stok.CurrentInventory = 0;
        //                                }
        //                                db.Entry(stok).State = EntityState.Modified;
        //                            }
        //                            else
        //                            {
        //                                FreeStock FSN = new FreeStock();
        //                                FSN.ItemNumber = f.itemNumber;
        //                                FSN.itemname = f.itemname;
        //                                FSN.ItemMultiMRPId = f.ItemMultiMRPId;
        //                                FSN.MRP = MRP.MRP;
        //                                FSN.WarehouseId = Convert.ToInt32(f.WarehouseId);
        //                                FSN.CurrentInventory = f.TotalQuantity;
        //                                FSN.CreatedBy = UserName;
        //                                FSN.CreationDate = indianTime;
        //                                FSN.Deleted = false;
        //                                FSN.UpdatedDate = indianTime;
        //                                db.FreeStockDB.Add(FSN);
        //                                db.Commit();
        //                                FreeStockHistory Oss = new FreeStockHistory();
        //                                Oss.ManualReason = "Free Item";
        //                                Oss.FreeStockId = FSN.FreeStockId;
        //                                Oss.ItemMultiMRPId = FSN.ItemMultiMRPId;
        //                                Oss.ItemNumber = FSN.ItemNumber;
        //                                Oss.itemname = FSN.itemname;
        //                                Oss.OdOrPoId = f.PurchaseOrderId;
        //                                Oss.CurrentInventory = f.TotalQuantity;
        //                                Oss.InventoryIn = f.TotalQuantity;
        //                                Oss.TotalInventory = Convert.ToInt32(FSN.CurrentInventory);
        //                                Oss.WarehouseId = FSN.WarehouseId;
        //                                Oss.CreationDate = DateTime.Now;
        //                                db.FreeStockHistoryDB.Add(Oss);
        //                            }
        //                        }
        //                        if (IsRec)
        //                        {
        //                            pm.progress = "100";
        //                            pm.Status = "CN Received";
        //                        }
        //                        else { pm.Status = "CN Partial Received"; }
        //                        db.Commit();
        //                        #endregion
        //                    }
        //                    else
        //                    {
        //                        return Request.CreateResponse(HttpStatusCode.BadRequest, "GR already Approved.");
        //                    }
        //                    break;
        //                case 'D':
        //                    pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
        //                    List<PurchaseOrderDetailRecived> detailsD = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).ToList();
        //                    if (pm.Gr4Status != "Approved")
        //                    {
        //                        pm.Gr4Status = "Approved";

        //                        foreach (var k in detailsD)
        //                        {
        //                            TemporaryCurrentStock Tcs = db.TemporaryCurrentStockDB.Where(x => x.ItemNumber == k.ItemNumber && x.WarehouseId == k.WarehouseId && x.CompanyId == k.CompanyId && x.ItemMultiMRPId == k.ItemMultiMRPId4).SingleOrDefault();

        //                            if (k.QtyRecived != 0)
        //                            {
        //                                /// Deduct Stock from Temporary current stock
        //                                TemporaryCurrentStockHistory Tcsh = new TemporaryCurrentStockHistory();
        //                                if (Tcs != null)
        //                                {
        //                                    Tcsh.StockId = Tcs.Id;
        //                                    Tcsh.ItemNumber = Tcs.ItemNumber;
        //                                    Tcsh.itemname = Tcs.itemname;
        //                                    Tcsh.OdOrPoId = obj.PurchaseOrderId;

        //                                    Tcsh.CurrentInventory = Tcs.CurrentInventory;
        //                                    Tcsh.InventoryOut = Convert.ToInt32(k.QtyRecived4);
        //                                    Tcsh.TotalCurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived4);

        //                                    Tcsh.ExpInventoryOut = k.ExpQtyRecived4;
        //                                    Tcsh.TotalExpCurrentInventory = Tcs.ExpCurrentInventory;

        //                                    Tcsh.DamageInventoryOut = k.DamagQtyRecived4;
        //                                    Tcsh.TotalDamageCurrentInventory = Tcs.DamageCurrentInventory;

        //                                    Tcsh.WarehouseName = Tcs.WarehouseName;
        //                                    Tcsh.Warehouseid = Tcs.WarehouseId;
        //                                    Tcsh.CompanyId = Tcs.CompanyId;

        //                                    Tcsh.CreationDate = indianTime;
        //                                    Tcsh.userid = userid;
        //                                    Tcsh.ItemMultiMRPId = Tcs.ItemMultiMRPId;
        //                                    db.TemporaryCurrentStockHistoryDB.Add(Tcsh);

        //                                    Tcs.CurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived4);
        //                                    db.UpdateTempCurrentStock(Tcs);

        //                                    /// Add Stock in current stock from Temporary current stock
        //                                    CurrentStock item = db.DbCurrentStock.Where(x => x.ItemNumber == k.ItemNumber && x.WarehouseId == k.WarehouseId && x.CompanyId == k.CompanyId && x.ItemMultiMRPId == k.ItemMultiMRPId4 && !x.Deleted).SingleOrDefault();
        //                                    ItemMultiMRP MRPdetail = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == k.ItemMultiMRPId4).SingleOrDefault();


        //                                    if (item == null)
        //                                    {
        //                                        CurrentStock NewStock = new CurrentStock();
        //                                        NewStock.CompanyId = Tcs.CompanyId;
        //                                        NewStock.CreationDate = indianTime;
        //                                        NewStock.CurrentInventory = 0;
        //                                        NewStock.Deleted = false;
        //                                        NewStock.ItemMultiMRPId = k.ItemMultiMRPId4;
        //                                        NewStock.itemname = k.ItemName;
        //                                        NewStock.ItemNumber = k.ItemNumber;
        //                                        NewStock.itemBaseName = Tcs.itemBaseName;
        //                                        //NewStock.ItemId = Tcs.ItemId;
        //                                        NewStock.UpdatedDate = indianTime;
        //                                        NewStock.WarehouseId = Tcs.WarehouseId;
        //                                        NewStock.WarehouseName = Tcs.WarehouseName;
        //                                        NewStock.MRP = MRPdetail.MRP;
        //                                        NewStock.UOM = MRPdetail.UOM;
        //                                        NewStock.userid = userid;
        //                                        db.DbCurrentStock.Add(NewStock);
        //                                        db.Commit();
        //                                    }
        //                                    // Recall
        //                                    item = db.DbCurrentStock.Where(x => x.ItemNumber == k.ItemNumber && x.WarehouseId == k.WarehouseId && x.CompanyId == k.CompanyId && x.ItemMultiMRPId == k.ItemMultiMRPId4 && !x.Deleted).SingleOrDefault();

        //                                    CurrentStockHistory Oss = new CurrentStockHistory();
        //                                    if (item != null)
        //                                    {
        //                                        Oss.StockId = item.StockId;
        //                                        Oss.ItemNumber = item.ItemNumber;
        //                                        Oss.itemname = item.itemname;
        //                                        Oss.OdOrPoId = obj.PurchaseOrderId;
        //                                        Oss.CurrentInventory = item.CurrentInventory;
        //                                        Oss.InventoryIn = Convert.ToInt32(k.QtyRecived4);
        //                                        Oss.TotalInventory = item.CurrentInventory + Convert.ToInt32(k.QtyRecived4);
        //                                        Oss.WarehouseName = item.WarehouseName;
        //                                        Oss.Warehouseid = item.WarehouseId;
        //                                        Oss.CompanyId = item.CompanyId;
        //                                        Oss.CreationDate = indianTime;
        //                                        Oss.ManualReason = "(+)Stock GRN: " + sAllCharacter;
        //                                        Oss.ItemMultiMRPId = item.ItemMultiMRPId;
        //                                        Oss.MRP = item.MRP;
        //                                        Oss.UOM = item.UOM;
        //                                        Oss.userid = people.PeopleID;
        //                                        Oss.UserName = people.DisplayName;
        //                                        db.CurrentStockHistoryDb.Add(Oss);
        //                                        item.CurrentInventory = item.CurrentInventory + Convert.ToInt32(k.QtyRecived4);
        //                                        db.UpdateCurrentStock(item);
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Temp Stock not exist.");
        //                                }
        //                            }
        //                            db.Commit();
        //                        }
        //                        #region ass Free item stock in Freestock table
        //                        List<FreeItem> FI = db.FreeItemDb.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId && a.GRNumber == obj.GrNumber).ToList();
        //                        var UserName = db.Peoples.Where(y => y.PeopleID == userid).Select(b => b.DisplayName).SingleOrDefault();
        //                        foreach (FreeItem f in FI)
        //                        {
        //                            FreeStock stok = db.FreeStockDB.Where(x => x.ItemNumber == f.itemNumber && x.WarehouseId == f.WarehouseId && x.ItemMultiMRPId == f.ItemMultiMRPId).FirstOrDefault();
        //                            ItemMultiMRP MRP = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == f.ItemMultiMRPId).SingleOrDefault();
        //                            if (stok != null)
        //                            {
        //                                FreeStockHistory Oss = new FreeStockHistory();
        //                                Oss.ManualReason = "Free Item";
        //                                Oss.FreeStockId = stok.FreeStockId;
        //                                Oss.ItemMultiMRPId = stok.ItemMultiMRPId;
        //                                Oss.ItemNumber = stok.ItemNumber;
        //                                Oss.itemname = stok.itemname;
        //                                Oss.OdOrPoId = obj.PurchaseOrderId;
        //                                Oss.CurrentInventory = f.TotalQuantity;
        //                                Oss.InventoryIn = f.TotalQuantity;
        //                                Oss.TotalInventory = Convert.ToInt32(stok.CurrentInventory + f.TotalQuantity);
        //                                Oss.WarehouseId = stok.WarehouseId;
        //                                Oss.CreationDate = DateTime.Now;
        //                                db.FreeStockHistoryDB.Add(Oss);
        //                                stok.CurrentInventory = stok.CurrentInventory + f.TotalQuantity;
        //                                if (stok.CurrentInventory < 0)
        //                                {
        //                                    stok.CurrentInventory = 0;
        //                                }
        //                                db.Entry(stok).State = EntityState.Modified;
        //                            }
        //                            else
        //                            {
        //                                FreeStock FSN = new FreeStock();
        //                                FSN.ItemNumber = f.itemNumber;
        //                                FSN.itemname = f.itemname;
        //                                FSN.ItemMultiMRPId = f.ItemMultiMRPId;
        //                                FSN.MRP = MRP.MRP;
        //                                FSN.WarehouseId = Convert.ToInt32(f.WarehouseId);
        //                                FSN.CurrentInventory = f.TotalQuantity;
        //                                FSN.CreatedBy = UserName;
        //                                FSN.CreationDate = indianTime;
        //                                FSN.Deleted = false;
        //                                FSN.UpdatedDate = indianTime;
        //                                db.FreeStockDB.Add(FSN);
        //                                db.Commit();
        //                                FreeStockHistory Oss = new FreeStockHistory();
        //                                Oss.ManualReason = "Free Item";
        //                                Oss.FreeStockId = FSN.FreeStockId;
        //                                Oss.ItemMultiMRPId = FSN.ItemMultiMRPId;
        //                                Oss.ItemNumber = FSN.ItemNumber;
        //                                Oss.itemname = FSN.itemname;
        //                                Oss.OdOrPoId = f.PurchaseOrderId;
        //                                Oss.CurrentInventory = f.TotalQuantity;
        //                                Oss.InventoryIn = f.TotalQuantity;
        //                                Oss.TotalInventory = Convert.ToInt32(FSN.CurrentInventory);
        //                                Oss.WarehouseId = FSN.WarehouseId;
        //                                Oss.CreationDate = DateTime.Now;
        //                                db.FreeStockHistoryDB.Add(Oss);
        //                            }
        //                        }
        //                        if (IsRec)
        //                        {
        //                            pm.progress = "100";
        //                            pm.Status = "CN Received";
        //                        }
        //                        else { pm.Status = "CN Partial Received"; }
        //                        db.Commit();
        //                        #endregion
        //                    }
        //                    else
        //                    {
        //                        return Request.CreateResponse(HttpStatusCode.BadRequest, "GR already Approved.");
        //                    }
        //                    break;
        //                case 'E':
        //                    pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
        //                    List<PurchaseOrderDetailRecived> detailsE = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).ToList();
        //                    if (pm.Gr5Status != "Approved")
        //                    {
        //                        pm.Gr5Status = "Approved";

        //                        foreach (var k in detailsE)
        //                        {
        //                            TemporaryCurrentStock Tcs = db.TemporaryCurrentStockDB.Where(x => x.ItemNumber == k.ItemNumber && x.WarehouseId == k.WarehouseId && x.CompanyId == k.CompanyId && x.ItemMultiMRPId == k.ItemMultiMRPId5).SingleOrDefault();

        //                            if (k.QtyRecived != 0)
        //                            {
        //                                /// Deduct Stock from Temporary current stock
        //                                TemporaryCurrentStockHistory Tcsh = new TemporaryCurrentStockHistory();
        //                                if (Tcs != null)
        //                                {
        //                                    Tcsh.StockId = Tcs.Id;
        //                                    Tcsh.ItemNumber = Tcs.ItemNumber;
        //                                    Tcsh.itemname = Tcs.itemname;
        //                                    Tcsh.OdOrPoId = obj.PurchaseOrderId;

        //                                    Tcsh.CurrentInventory = Tcs.CurrentInventory;
        //                                    Tcsh.InventoryOut = Convert.ToInt32(k.QtyRecived5);
        //                                    Tcsh.TotalCurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived5);

        //                                    Tcsh.ExpInventoryOut = k.ExpQtyRecived5;
        //                                    Tcsh.TotalExpCurrentInventory = Tcs.ExpCurrentInventory;

        //                                    Tcsh.DamageInventoryOut = k.DamagQtyRecived5;
        //                                    Tcsh.TotalDamageCurrentInventory = Tcs.DamageCurrentInventory;

        //                                    Tcsh.WarehouseName = Tcs.WarehouseName;
        //                                    Tcsh.Warehouseid = Tcs.WarehouseId;
        //                                    Tcsh.CompanyId = Tcs.CompanyId;

        //                                    Tcsh.CreationDate = indianTime;
        //                                    Tcsh.userid = userid;
        //                                    Tcsh.ItemMultiMRPId = Tcs.ItemMultiMRPId;
        //                                    db.TemporaryCurrentStockHistoryDB.Add(Tcsh);

        //                                    Tcs.CurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived5);
        //                                    db.UpdateTempCurrentStock(Tcs);

        //                                    /// Add Stock in current stock from Temporary current stock
        //                                    CurrentStock item = db.DbCurrentStock.Where(x => x.ItemNumber == k.ItemNumber && x.WarehouseId == k.WarehouseId && x.CompanyId == k.CompanyId && x.ItemMultiMRPId == k.ItemMultiMRPId5 && !x.Deleted).SingleOrDefault();
        //                                    ItemMultiMRP MRPdetail = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == k.ItemMultiMRPId5).SingleOrDefault();
        //                                    if (item == null)
        //                                    {
        //                                        CurrentStock NewStock = new CurrentStock();
        //                                        NewStock.CompanyId = Tcs.CompanyId;
        //                                        NewStock.CreationDate = indianTime;
        //                                        NewStock.CurrentInventory = 0;
        //                                        NewStock.Deleted = false;
        //                                        NewStock.ItemMultiMRPId = k.ItemMultiMRPId5;
        //                                        NewStock.itemname = k.ItemName;
        //                                        NewStock.ItemNumber = k.ItemNumber;
        //                                        NewStock.itemBaseName = Tcs.itemBaseName;
        //                                        //NewStock.ItemId = Tcs.ItemId;
        //                                        NewStock.UpdatedDate = indianTime;
        //                                        NewStock.WarehouseId = Tcs.WarehouseId;
        //                                        NewStock.WarehouseName = Tcs.WarehouseName;
        //                                        NewStock.MRP = MRPdetail.MRP;
        //                                        NewStock.UOM = MRPdetail.UOM;
        //                                        NewStock.userid = userid;
        //                                        db.DbCurrentStock.Add(NewStock);
        //                                        db.Commit();
        //                                    }
        //                                    // Recall
        //                                    item = db.DbCurrentStock.Where(x => x.ItemNumber == k.ItemNumber && x.WarehouseId == k.WarehouseId && x.CompanyId == k.CompanyId && x.ItemMultiMRPId == k.ItemMultiMRPId5 && !x.Deleted).SingleOrDefault();


        //                                    CurrentStockHistory Oss = new CurrentStockHistory();
        //                                    if (item != null)
        //                                    {
        //                                        Oss.StockId = item.StockId;
        //                                        Oss.ItemNumber = item.ItemNumber;
        //                                        Oss.itemname = item.itemname;
        //                                        Oss.OdOrPoId = obj.PurchaseOrderId;
        //                                        Oss.CurrentInventory = item.CurrentInventory;
        //                                        Oss.InventoryIn = Convert.ToInt32(k.QtyRecived5);
        //                                        Oss.TotalInventory = item.CurrentInventory + Convert.ToInt32(k.QtyRecived5);
        //                                        Oss.WarehouseName = item.WarehouseName;
        //                                        Oss.Warehouseid = item.WarehouseId;
        //                                        Oss.CompanyId = item.CompanyId;
        //                                        Oss.CreationDate = indianTime;
        //                                        Oss.ManualReason = "(+)Stock GRN: " + sAllCharacter;
        //                                        Oss.ItemMultiMRPId = item.ItemMultiMRPId;
        //                                        Oss.MRP = item.MRP;
        //                                        Oss.UOM = item.UOM;
        //                                        Oss.userid = people.PeopleID;
        //                                        Oss.UserName = people.DisplayName;
        //                                        db.CurrentStockHistoryDb.Add(Oss);
        //                                        item.CurrentInventory = item.CurrentInventory + Convert.ToInt32(k.QtyRecived5);
        //                                        db.UpdateCurrentStock(item);
        //                                    }

        //                                }
        //                                else
        //                                {
        //                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Temp Stock not exist.");
        //                                }
        //                            }
        //                            if (IsRec == true)
        //                            {
        //                                if (k.TotalQuantity > (k.QtyRecived1 + k.QtyRecived2 + k.QtyRecived3 + k.QtyRecived4 + k.QtyRecived5))
        //                                {
        //                                    IsRec = false;
        //                                }
        //                                else
        //                                {
        //                                    IsRec = true;
        //                                }
        //                            }
        //                            db.Commit();
        //                        }
        //                        #region ass Free item stock in Freestock table
        //                        List<FreeItem> FI = db.FreeItemDb.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId && a.GRNumber == obj.GrNumber).ToList();
        //                        var UserName = db.Peoples.Where(y => y.PeopleID == userid).Select(b => b.DisplayName).SingleOrDefault();
        //                        foreach (FreeItem f in FI)
        //                        {
        //                            FreeStock stok = db.FreeStockDB.Where(x => x.ItemNumber == f.itemNumber && x.WarehouseId == f.WarehouseId && x.ItemMultiMRPId == f.ItemMultiMRPId).FirstOrDefault();
        //                            ItemMultiMRP MRP = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == f.ItemMultiMRPId).SingleOrDefault();
        //                            if (stok != null)
        //                            {
        //                                FreeStockHistory Oss = new FreeStockHistory();
        //                                Oss.ManualReason = "Free Item";
        //                                Oss.FreeStockId = stok.FreeStockId;
        //                                Oss.ItemMultiMRPId = stok.ItemMultiMRPId;
        //                                Oss.ItemNumber = stok.ItemNumber;
        //                                Oss.itemname = stok.itemname;
        //                                Oss.OdOrPoId = obj.PurchaseOrderId;
        //                                Oss.CurrentInventory = f.TotalQuantity;
        //                                Oss.InventoryIn = f.TotalQuantity;
        //                                Oss.TotalInventory = Convert.ToInt32(stok.CurrentInventory + f.TotalQuantity);
        //                                Oss.WarehouseId = stok.WarehouseId;
        //                                Oss.CreationDate = DateTime.Now;
        //                                db.FreeStockHistoryDB.Add(Oss);
        //                                stok.CurrentInventory = stok.CurrentInventory + f.TotalQuantity;
        //                                if (stok.CurrentInventory < 0)
        //                                {
        //                                    stok.CurrentInventory = 0;
        //                                }
        //                                db.Entry(stok).State = EntityState.Modified;
        //                            }
        //                            else
        //                            {
        //                                FreeStock FSN = new FreeStock();
        //                                FSN.ItemNumber = f.itemNumber;
        //                                FSN.itemname = f.itemname;
        //                                FSN.ItemMultiMRPId = f.ItemMultiMRPId;
        //                                FSN.MRP = MRP.MRP;
        //                                FSN.WarehouseId = Convert.ToInt32(f.WarehouseId);
        //                                FSN.CurrentInventory = f.TotalQuantity;
        //                                FSN.CreatedBy = UserName;
        //                                FSN.CreationDate = indianTime;
        //                                FSN.Deleted = false;
        //                                FSN.UpdatedDate = indianTime;
        //                                db.FreeStockDB.Add(FSN);
        //                                db.Commit();
        //                                FreeStockHistory Oss = new FreeStockHistory();
        //                                Oss.ManualReason = "Free Item";
        //                                Oss.FreeStockId = FSN.FreeStockId;
        //                                Oss.ItemMultiMRPId = FSN.ItemMultiMRPId;
        //                                Oss.ItemNumber = FSN.ItemNumber;
        //                                Oss.itemname = FSN.itemname;
        //                                Oss.OdOrPoId = f.PurchaseOrderId;
        //                                Oss.CurrentInventory = f.TotalQuantity;
        //                                Oss.InventoryIn = f.TotalQuantity;
        //                                Oss.TotalInventory = Convert.ToInt32(FSN.CurrentInventory);
        //                                Oss.WarehouseId = FSN.WarehouseId;
        //                                Oss.CreationDate = DateTime.Now;
        //                                db.FreeStockHistoryDB.Add(Oss);
        //                            }
        //                        }
        //                        if (IsRec)
        //                        {
        //                            pm.progress = "100";
        //                            pm.Status = "CN Received";
        //                        }
        //                        else { pm.Status = "CN Partial Received"; }
        //                        db.Commit();
        //                        #endregion
        //                    }
        //                    else
        //                    {
        //                        return Request.CreateResponse(HttpStatusCode.BadRequest, "GR already Approved.");
        //                    }
        //                    break;
        //                default:
        //                    dbContextTransaction.Rollback();
        //                    return null;
        //            }

        //            pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
        //            List<PurchaseOrderDetailRecived> RD = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).ToList();
        //            var TQ = RD.Sum(e => e.TotalQuantity);
        //            var TRQ = RD.Sum(q => q.QtyRecived);
        //            if (TQ == TRQ)
        //            {
        //                pm.progress = "100";
        //                pm.Status = "CN Received";
        //                db.SaveChanges();
        //            }
        //            else
        //            {
        //                pm.Status = "CN Partial Received";
        //                pm.progress = "70";
        //                db.SaveChanges();
        //            }

        //            logger.Info("End  : ");
        //            dbContextTransaction.Commit();
        //            return Request.CreateResponse(HttpStatusCode.OK, pm);
        //        }
        //        catch (Exception ex)
        //        {
        //            dbContextTransaction.Rollback();
        //            logger.Error("Error in PurchaseOrderDetail " + ex.Message);
        //            logger.Info("End  PurchaseOrderDetail: ");
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, "");
        //        }
        //    }
        //}

        //#endregion

        #region Reject Gr Details in PO
        /// <summary>
        /// Created By Raj
        /// Created Date:30/08/2019
        /// Reject GR 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Route("RejectGrDetail")]
        [HttpPost]
        public HttpResponseMessage RejectGrDetail(PutUCGRDTO obj)
        {
            logger.Info("start : ");
            try
            {
                // Access claims
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                int CompanyId = compid;

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                string sAllCharacter = obj.GrNumber;
                char GRType = sAllCharacter[sAllCharacter.Length - 1];
                PurchaseOrderMaster pm = new PurchaseOrderMaster();
                switch (GRType)
                {
                    case 'A':
                        using (AuthContext db = new AuthContext())
                        {
                            pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
                            pm.Gr1Status = "RejectGR";
                            pm.progress = "50";
                            db.Commit();

                        }
                        break;
                    case 'B':
                        using (AuthContext db = new AuthContext())
                        {
                            pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
                            pm.Gr2Status = "RejectGR";
                            pm.progress = "50";
                            db.Commit();
                        }
                        break;
                    case 'C':
                        using (AuthContext db = new AuthContext())
                        {
                            pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
                            pm.Gr3Status = "RejectGR";
                            pm.progress = "50";
                            db.Commit();

                        }
                        break;
                    case 'D':
                        using (AuthContext db = new AuthContext())
                        {
                            pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
                            pm.Gr4Status = "RejectGR";
                            pm.progress = "50";
                            db.Commit();

                        }
                        break;
                    case 'E':
                        using (AuthContext db = new AuthContext())
                        {
                            pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
                            pm.Gr5Status = "RejectGR";
                            pm.progress = "50";
                            db.Commit();
                        }
                        break;
                    default:
                        return null;
                }
                logger.Info("End  : ");
                return Request.CreateResponse(HttpStatusCode.OK, pm); ;
            }
            catch (Exception ex)
            {
                logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                logger.Info("End  PurchaseOrderDetail: ");
                return Request.CreateResponse(HttpStatusCode.BadRequest, "");
            }
        }

        #endregion
        #region Get Reject GR Data 
        /// <summary>
        /// Created By Raj
        /// Created Date:30/08/2019
        /// GetRejectGrData
        /// </summary>
        /// <returns></returns>
        [Route("GetRejectGrData")]
        [HttpGet]
        public HttpResponseMessage GetRejectGrData(int wid)
        {
            logger.Info("start : ");
            try
            {
                // Access claims
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                int CompanyId = compid;
                List<PurchaseOrderMaster> pm = new List<PurchaseOrderMaster>();
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                var predicate = PredicateBuilder.True<PurchaseOrderMaster>();
                predicate = predicate.And(x => x.Gr1Status == "RejectGR" || x.Gr2Status == "RejectGR" || x.Gr3Status == "RejectGR" || x.Gr4Status == "RejectGR" || x.Gr5Status == "RejectGR");
                predicate = predicate.And(e => e.WarehouseId == wid);
                List<GetUCGRDTO> HeadData = new List<GetUCGRDTO>();
                using (AuthContext db = new AuthContext())
                {
                    pm = db.DPurchaseOrderMaster.Where(predicate).ToList();

                    foreach (PurchaseOrderMaster a in pm)
                    {
                        List<PurchaseOrderDetailRecived> _detail = db.PurchaseOrderRecivedDetails.Where(q => q.PurchaseOrderId == a.PurchaseOrderId).ToList();

                        if (a.Gr1_Amount != 0 && a.Gr1Status == "RejectGR")
                        {
                            List<GetListUCGRDTO> HeadDetail = new List<GetListUCGRDTO>();
                            foreach (PurchaseOrderDetailRecived b in _detail)
                            {
                                GetListUCGRDTO UCL = new GetListUCGRDTO()
                                {
                                    PurchaseOrderDetailRecivedId = b.PurchaseOrderDetailRecivedId,
                                    ItemId = b.ItemId,
                                    ItemName = b.ItemName1,
                                    Price = Convert.ToDouble(b.Price1),
                                    HSNCode = b.HSNCode,
                                    TotalQuantity = b.TotalQuantity,
                                    QtyRecived = Convert.ToInt32(b.QtyRecived1),
                                    DamagQtyRecived = b.DamagQtyRecived1,
                                    ExpQtyRecived = b.ExpQtyRecived1,
                                    BatchNo = b.BatchNo1,
                                    MFG = b.MFGDate1
                                };
                                HeadDetail.Add(UCL);
                            }

                            GetUCGRDTO UC = new GetUCGRDTO()
                            {
                                PurchaseOrderId = a.PurchaseOrderId,
                                GrNumber = a.Gr1Number,
                                GrPersonName = a.Gr1PersonName,
                                GrDate = Convert.ToDateTime(a.Gr1_Date),
                                GrAmount = a.Gr1_Amount,
                                VehicleType = a.VehicleType1,
                                VehicleNumber = a.VehicleNumber1,
                                Status = a.Gr1Status,
                                Detail = HeadDetail

                            };
                            HeadData.Add(UC);
                        }
                        if (a.Gr2_Amount != 0 && a.Gr2Status == "RejectGR")
                        {
                            List<GetListUCGRDTO> HeadDetail = new List<GetListUCGRDTO>();
                            foreach (PurchaseOrderDetailRecived b in _detail)
                            {
                                GetListUCGRDTO UCL = new GetListUCGRDTO()
                                {
                                    PurchaseOrderDetailRecivedId = b.PurchaseOrderDetailRecivedId,
                                    ItemId = b.ItemId,
                                    ItemName = b.ItemName2,
                                    Price = Convert.ToDouble(b.Price2),
                                    HSNCode = b.HSNCode,
                                    TotalQuantity = b.TotalQuantity,
                                    QtyRecived = Convert.ToInt32(b.QtyRecived2),
                                    DamagQtyRecived = b.DamagQtyRecived2,
                                    ExpQtyRecived = b.ExpQtyRecived2,
                                    BatchNo = b.BatchNo2,
                                    MFG = b.MFGDate2
                                };
                                HeadDetail.Add(UCL);
                            }

                            GetUCGRDTO UC = new GetUCGRDTO()
                            {
                                PurchaseOrderId = a.PurchaseOrderId,
                                GrNumber = a.Gr2Number,
                                GrPersonName = a.Gr2PersonName,
                                GrDate = Convert.ToDateTime(a.Gr2_Date),
                                GrAmount = a.Gr2_Amount,
                                VehicleType = a.VehicleType2,
                                VehicleNumber = a.VehicleNumber2,
                                Status = a.Gr2Status,
                                Detail = HeadDetail

                            };
                            HeadData.Add(UC);
                        }
                        if (a.Gr3_Amount != 0 && a.Gr3Status == "RejectGR")
                        {
                            List<GetListUCGRDTO> HeadDetail = new List<GetListUCGRDTO>();
                            foreach (PurchaseOrderDetailRecived b in _detail)
                            {
                                GetListUCGRDTO UCL = new GetListUCGRDTO()
                                {
                                    PurchaseOrderDetailRecivedId = b.PurchaseOrderDetailRecivedId,
                                    ItemId = b.ItemId,
                                    ItemName = b.ItemName3,
                                    Price = Convert.ToDouble(b.Price3),
                                    HSNCode = b.HSNCode,
                                    TotalQuantity = b.TotalQuantity,
                                    QtyRecived = Convert.ToInt32(b.QtyRecived3),
                                    DamagQtyRecived = b.DamagQtyRecived3,
                                    ExpQtyRecived = b.ExpQtyRecived3,
                                    BatchNo = b.BatchNo3,
                                    MFG = b.MFGDate3
                                };
                                HeadDetail.Add(UCL);
                            }

                            GetUCGRDTO UC = new GetUCGRDTO()
                            {
                                PurchaseOrderId = a.PurchaseOrderId,
                                GrNumber = a.Gr3Number,
                                GrPersonName = a.Gr3PersonName,
                                GrDate = Convert.ToDateTime(a.Gr3_Date),
                                GrAmount = a.Gr3_Amount,
                                VehicleType = a.VehicleType3,
                                VehicleNumber = a.VehicleNumber3,
                                Status = a.Gr3Status,
                                Detail = HeadDetail

                            };
                            HeadData.Add(UC);
                        }
                        if (a.Gr4_Amount != 0 && a.Gr4Status == "RejectGR")
                        {
                            List<GetListUCGRDTO> HeadDetail = new List<GetListUCGRDTO>();
                            foreach (PurchaseOrderDetailRecived b in _detail)
                            {
                                GetListUCGRDTO UCL = new GetListUCGRDTO()
                                {
                                    PurchaseOrderDetailRecivedId = b.PurchaseOrderDetailRecivedId,
                                    ItemId = b.ItemId,
                                    ItemName = b.ItemName4,
                                    Price = Convert.ToDouble(b.Price4),
                                    HSNCode = b.HSNCode,
                                    TotalQuantity = b.TotalQuantity,
                                    QtyRecived = Convert.ToInt32(b.QtyRecived4),
                                    DamagQtyRecived = b.DamagQtyRecived4,
                                    ExpQtyRecived = b.ExpQtyRecived4,
                                    BatchNo = b.BatchNo4,
                                    MFG = b.MFGDate4
                                };
                                HeadDetail.Add(UCL);
                            }

                            GetUCGRDTO UC = new GetUCGRDTO()
                            {
                                PurchaseOrderId = a.PurchaseOrderId,
                                GrNumber = a.Gr4Number,
                                GrPersonName = a.Gr4PersonName,
                                GrDate = Convert.ToDateTime(a.Gr4_Date),
                                GrAmount = a.Gr4_Amount,
                                VehicleType = a.VehicleType4,
                                VehicleNumber = a.VehicleNumber4,
                                Status = a.Gr4Status,
                                Detail = HeadDetail

                            };
                            HeadData.Add(UC);
                        }
                        if (a.Gr5_Amount != 0 && a.Gr5Status == "RejectGR")
                        {
                            List<GetListUCGRDTO> HeadDetail = new List<GetListUCGRDTO>();
                            foreach (PurchaseOrderDetailRecived b in _detail)
                            {
                                GetListUCGRDTO UCL = new GetListUCGRDTO()
                                {
                                    PurchaseOrderDetailRecivedId = b.PurchaseOrderDetailRecivedId,
                                    ItemId = b.ItemId,
                                    ItemName = b.ItemName5,
                                    Price = Convert.ToDouble(b.Price5),
                                    HSNCode = b.HSNCode,
                                    TotalQuantity = b.TotalQuantity,
                                    QtyRecived = Convert.ToInt32(b.QtyRecived5),
                                    DamagQtyRecived = b.DamagQtyRecived5,
                                    ExpQtyRecived = b.ExpQtyRecived5,
                                    BatchNo = b.BatchNo5,
                                    MFG = b.MFGDate5
                                };
                                HeadDetail.Add(UCL);
                            }

                            GetUCGRDTO UC = new GetUCGRDTO()
                            {
                                PurchaseOrderId = a.PurchaseOrderId,
                                GrNumber = a.Gr5Number,
                                GrPersonName = a.Gr5PersonName,
                                GrDate = Convert.ToDateTime(a.Gr5_Date),
                                GrAmount = a.Gr5_Amount,
                                VehicleType = a.VehicleType5,
                                VehicleNumber = a.VehicleNumber5,
                                Status = a.Gr5Status,
                                Detail = HeadDetail
                            };
                            HeadData.Add(UC);
                        }

                    }
                }
                logger.Info("End  : ");
                return Request.CreateResponse(HttpStatusCode.OK, HeadData.OrderByDescending(w => w.PurchaseOrderId)); ;
            }
            catch (Exception ex)
            {
                logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                logger.Info("End  PurchaseOrderDetail: ");
                return Request.CreateResponse(HttpStatusCode.BadRequest, "");
            }
        }
        #endregion
        #region approved to reject GR
        /// <summary>
        /// approved to reject GR
        /// Created Date:30/08/2019
        /// Creted By Raj
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Route("PutApprovedGrDetail")]
        [HttpPost]
        public HttpResponseMessage PutApprovedGrDetail(PutUCGRDTO obj)
        {
            logger.Info("start : ");
            try
            {
                // Access claims
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                int CompanyId = compid;

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                string sAllCharacter = obj.GrNumber;
                char GRType = sAllCharacter[sAllCharacter.Length - 1];
                PurchaseOrderMaster pm = new PurchaseOrderMaster();
                switch (GRType)
                {
                    case 'A':
                        using (AuthContext db = new AuthContext())
                        {
                            pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
                            pm.Gr1Status = "Pending for Checker Side";
                            pm.Gr1_Amount = 0;

                            foreach (PutListUCGRDTO k in obj.Detail)
                            {
                                PurchaseOrderDetailRecived detail = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderDetailRecivedId == k.PurchaseOrderDetailRecivedId).SingleOrDefault();

                                ItemMaster IM = db.itemMasters.Where(a => a.Number == detail.ItemNumber && a.ItemMultiMRPId == detail.ItemMultiMRPId1 && a.WarehouseId == pm.WarehouseId).FirstOrDefault();
                                TemporaryCurrentStock Tcs = db.TemporaryCurrentStockDB.Where(x => x.ItemNumber == IM.Number && x.WarehouseId == pm.WarehouseId && x.CompanyId == IM.CompanyId && x.ItemMultiMRPId == detail.ItemMultiMRPId).SingleOrDefault();
                                if (k.QtyRecived != 0)
                                {
                                    /// Deduct Stock from Temporary current stock
                                    TemporaryCurrentStockHistory Tcsh = new TemporaryCurrentStockHistory();
                                    if (Tcs != null)
                                    {
                                        Tcsh.StockId = Tcs.Id;
                                        Tcsh.ItemNumber = Tcs.ItemNumber;
                                        Tcsh.itemname = Tcs.itemname;
                                        Tcsh.OdOrPoId = obj.PurchaseOrderId;
                                        Tcsh.CurrentInventory = Tcs.CurrentInventory;
                                        Tcsh.InventoryOut = Convert.ToInt32(k.QtyRecived);
                                        Tcsh.TotalCurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived);
                                        Tcsh.ExpInventoryOut = detail.ExpQtyRecived1 - k.ExpQtyRecived;
                                        Tcsh.TotalExpCurrentInventory = Tcs.ExpCurrentInventory - (detail.ExpQtyRecived1 - k.ExpQtyRecived);

                                        Tcsh.DamageInventoryOut = detail.DamagQtyRecived1 - Convert.ToInt32(k.DamagQtyRecived);
                                        Tcsh.TotalDamageCurrentInventory = Tcs.DamageCurrentInventory - (detail.DamagQtyRecived1 - Convert.ToInt32(k.DamagQtyRecived));

                                        Tcsh.WarehouseName = Tcs.WarehouseName;
                                        Tcsh.Warehouseid = Tcs.WarehouseId;
                                        Tcsh.CompanyId = Tcs.CompanyId;

                                        Tcsh.CreationDate = indianTime;
                                        Tcsh.userid = userid;
                                        Tcsh.ItemMultiMRPId = Tcs.ItemMultiMRPId;
                                        db.TemporaryCurrentStockHistoryDB.Add(Tcsh);
                                    }
                                    Tcs.CurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived);
                                    db.UpdateTempCurrentStock(Tcs);
                                    /// Add Stock in current stock from Temporary current stock
                                }

                                detail.QtyRecived1 = k.QtyRecived;
                                detail.DamagQtyRecived1 = k.DamagQtyRecived;
                                detail.ExpQtyRecived1 = k.ExpQtyRecived;
                                detail.PriceRecived = Convert.ToDouble(detail.Price1 * k.QtyRecived);
                                detail.GRDate1 = indianTime;
                                pm.Gr1_Amount += detail.PriceRecived;
                                db.Entry(detail).State = EntityState.Modified;
                            }



                            db.Commit();
                        }
                        break;
                    case 'B':
                        using (AuthContext db = new AuthContext())
                        {
                            pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
                            pm.Gr2Status = "Pending for Checker Side";
                            pm.Gr2_Amount = 0;
                            foreach (PutListUCGRDTO k in obj.Detail)
                            {

                                PurchaseOrderDetailRecived detail = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderDetailRecivedId == k.PurchaseOrderDetailRecivedId).SingleOrDefault();

                                ItemMaster IM = db.itemMasters.Where(a => a.Number == detail.ItemNumber && a.ItemMultiMRPId == detail.ItemMultiMRPId2 && a.WarehouseId == pm.WarehouseId).FirstOrDefault();
                                TemporaryCurrentStock Tcs = db.TemporaryCurrentStockDB.Where(x => x.ItemNumber == IM.Number && x.WarehouseId == pm.WarehouseId && x.CompanyId == IM.CompanyId && x.ItemMultiMRPId == detail.ItemMultiMRPId).SingleOrDefault();
                                if (k.QtyRecived != 0)
                                {
                                    /// Deduct Stock from Temporary current stock
                                    TemporaryCurrentStockHistory Tcsh = new TemporaryCurrentStockHistory();
                                    if (Tcs != null)
                                    {
                                        Tcsh.StockId = Tcs.Id;
                                        Tcsh.ItemNumber = Tcs.ItemNumber;
                                        Tcsh.itemname = Tcs.itemname;
                                        Tcsh.OdOrPoId = obj.PurchaseOrderId;
                                        Tcsh.CurrentInventory = Tcs.CurrentInventory;
                                        Tcsh.InventoryOut = Convert.ToInt32(k.QtyRecived);
                                        Tcsh.TotalCurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived);
                                        Tcsh.ExpInventoryOut = detail.ExpQtyRecived1 - k.ExpQtyRecived;
                                        Tcsh.TotalExpCurrentInventory = Tcs.ExpCurrentInventory - (detail.ExpQtyRecived1 - k.ExpQtyRecived);

                                        Tcsh.DamageInventoryOut = detail.DamagQtyRecived1 - Convert.ToInt32(k.DamagQtyRecived);
                                        Tcsh.TotalDamageCurrentInventory = Tcs.DamageCurrentInventory - (detail.DamagQtyRecived1 - Convert.ToInt32(k.DamagQtyRecived));

                                        Tcsh.WarehouseName = Tcs.WarehouseName;
                                        Tcsh.Warehouseid = Tcs.WarehouseId;
                                        Tcsh.CompanyId = Tcs.CompanyId;

                                        Tcsh.CreationDate = indianTime;
                                        Tcsh.userid = userid;
                                        Tcsh.ItemMultiMRPId = Tcs.ItemMultiMRPId;
                                        db.TemporaryCurrentStockHistoryDB.Add(Tcsh);
                                    }
                                    Tcs.CurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived);
                                    db.UpdateTempCurrentStock(Tcs);
                                    /// Add Stock in current stock from Temporary current stock
                                }
                                detail.QtyRecived2 = k.QtyRecived;
                                detail.DamagQtyRecived2 = k.DamagQtyRecived;
                                detail.ExpQtyRecived2 = k.ExpQtyRecived;
                                detail.PriceRecived = Convert.ToDouble(detail.Price1 * detail.QtyRecived1) + Convert.ToDouble(detail.Price2 * k.QtyRecived);
                                detail.GRDate2 = indianTime;
                                pm.Gr2_Amount += detail.PriceRecived;
                                db.Entry(detail).State = EntityState.Modified;
                            }
                            db.Commit();
                        }
                        break;
                    case 'C':
                        using (AuthContext db = new AuthContext())
                        {
                            pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
                            pm.Gr3Status = "Pending for Checker Side";
                            pm.Gr3_Amount = 0;
                            foreach (PutListUCGRDTO k in obj.Detail)
                            {

                                PurchaseOrderDetailRecived detail = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderDetailRecivedId == k.PurchaseOrderDetailRecivedId).SingleOrDefault();

                                ItemMaster IM = db.itemMasters.Where(a => a.Number == detail.ItemNumber && a.ItemMultiMRPId == detail.ItemMultiMRPId3 && a.WarehouseId == pm.WarehouseId).FirstOrDefault();
                                TemporaryCurrentStock Tcs = db.TemporaryCurrentStockDB.Where(x => x.ItemNumber == IM.Number && x.WarehouseId == pm.WarehouseId && x.CompanyId == IM.CompanyId && x.ItemMultiMRPId == detail.ItemMultiMRPId).SingleOrDefault();
                                if (k.QtyRecived != 0)
                                {
                                    /// Deduct Stock from Temporary current stock
                                    TemporaryCurrentStockHistory Tcsh = new TemporaryCurrentStockHistory();
                                    if (Tcs != null)
                                    {
                                        Tcsh.StockId = Tcs.Id;
                                        Tcsh.ItemNumber = Tcs.ItemNumber;
                                        Tcsh.itemname = Tcs.itemname;
                                        Tcsh.OdOrPoId = obj.PurchaseOrderId;
                                        Tcsh.CurrentInventory = Tcs.CurrentInventory;
                                        Tcsh.InventoryOut = Convert.ToInt32(k.QtyRecived);
                                        Tcsh.TotalCurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived);
                                        Tcsh.ExpInventoryOut = detail.ExpQtyRecived1 - k.ExpQtyRecived;
                                        Tcsh.TotalExpCurrentInventory = Tcs.ExpCurrentInventory - (detail.ExpQtyRecived1 - k.ExpQtyRecived);

                                        Tcsh.DamageInventoryOut = detail.DamagQtyRecived1 - Convert.ToInt32(k.DamagQtyRecived);
                                        Tcsh.TotalDamageCurrentInventory = Tcs.DamageCurrentInventory - (detail.DamagQtyRecived1 - Convert.ToInt32(k.DamagQtyRecived));

                                        Tcsh.WarehouseName = Tcs.WarehouseName;
                                        Tcsh.Warehouseid = Tcs.WarehouseId;
                                        Tcsh.CompanyId = Tcs.CompanyId;

                                        Tcsh.CreationDate = indianTime;
                                        Tcsh.userid = userid;
                                        Tcsh.ItemMultiMRPId = Tcs.ItemMultiMRPId;
                                        db.TemporaryCurrentStockHistoryDB.Add(Tcsh);
                                    }
                                    Tcs.CurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived);
                                    db.UpdateTempCurrentStock(Tcs);

                                }
                                detail.QtyRecived3 = k.QtyRecived;
                                detail.DamagQtyRecived3 = k.DamagQtyRecived;
                                detail.ExpQtyRecived3 = k.ExpQtyRecived;
                                detail.GRDate3 = indianTime;
                                detail.PriceRecived = Convert.ToDouble(detail.Price1 * detail.QtyRecived1) + Convert.ToDouble(detail.Price2 * detail.QtyRecived2) + Convert.ToDouble(detail.Price3 * k.QtyRecived);
                                pm.Gr3_Amount += detail.PriceRecived;
                                db.Entry(detail).State = EntityState.Modified;
                            }
                            db.Commit();
                        }
                        break;
                    case 'D':
                        using (AuthContext db = new AuthContext())
                        {
                            pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
                            pm.Gr4Status = "Pending for Checker Side";
                            pm.Gr4_Amount = 0;
                            foreach (PutListUCGRDTO k in obj.Detail)
                            {

                                PurchaseOrderDetailRecived detail = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderDetailRecivedId == k.PurchaseOrderDetailRecivedId).SingleOrDefault();
                                ItemMaster IM = db.itemMasters.Where(a => a.Number == detail.ItemNumber && a.ItemMultiMRPId == detail.ItemMultiMRPId4 && a.WarehouseId == pm.WarehouseId).FirstOrDefault();
                                TemporaryCurrentStock Tcs = db.TemporaryCurrentStockDB.Where(x => x.ItemNumber == IM.Number && x.WarehouseId == pm.WarehouseId && x.CompanyId == IM.CompanyId && x.ItemMultiMRPId == detail.ItemMultiMRPId).SingleOrDefault();

                                if (k.QtyRecived != 0)
                                {
                                    /// Deduct Stock from Temporary current stock
                                    TemporaryCurrentStockHistory Tcsh = new TemporaryCurrentStockHistory();
                                    if (Tcs != null)
                                    {
                                        Tcsh.StockId = Tcs.Id;
                                        Tcsh.ItemNumber = Tcs.ItemNumber;
                                        Tcsh.itemname = Tcs.itemname;
                                        Tcsh.OdOrPoId = obj.PurchaseOrderId;

                                        Tcsh.CurrentInventory = Tcs.CurrentInventory;
                                        Tcsh.InventoryOut = Convert.ToInt32(k.QtyRecived);
                                        Tcsh.TotalCurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived);

                                        Tcsh.ExpInventoryOut = detail.ExpQtyRecived1 - k.ExpQtyRecived;
                                        Tcsh.TotalExpCurrentInventory = Tcs.ExpCurrentInventory - (detail.ExpQtyRecived1 - k.ExpQtyRecived);

                                        Tcsh.DamageInventoryOut = detail.DamagQtyRecived1 - Convert.ToInt32(k.DamagQtyRecived);
                                        Tcsh.TotalDamageCurrentInventory = Tcs.DamageCurrentInventory - (detail.DamagQtyRecived1 - Convert.ToInt32(k.DamagQtyRecived));

                                        Tcsh.WarehouseName = Tcs.WarehouseName;
                                        Tcsh.Warehouseid = Tcs.WarehouseId;
                                        Tcsh.CompanyId = Tcs.CompanyId;

                                        Tcsh.CreationDate = indianTime;
                                        Tcsh.userid = userid;
                                        Tcsh.ItemMultiMRPId = Tcs.ItemMultiMRPId;
                                        db.TemporaryCurrentStockHistoryDB.Add(Tcsh);
                                    }
                                    Tcs.CurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived);
                                    db.UpdateTempCurrentStock(Tcs);

                                }
                                detail.QtyRecived4 = k.QtyRecived;
                                detail.DamagQtyRecived4 = k.DamagQtyRecived;
                                detail.ExpQtyRecived4 = k.ExpQtyRecived;
                                detail.GRDate4 = indianTime;
                                detail.PriceRecived = Convert.ToDouble(detail.Price1 * detail.QtyRecived1) + Convert.ToDouble(detail.Price2 * detail.QtyRecived2) + Convert.ToDouble(detail.Price3 * detail.QtyRecived3) + Convert.ToDouble(detail.Price4 * k.QtyRecived);
                                pm.Gr4_Amount += detail.PriceRecived;
                                db.Entry(detail).State = EntityState.Modified;
                            }
                            db.Commit();
                        }
                        break;
                    case 'E':
                        using (AuthContext db = new AuthContext())
                        {
                            pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
                            pm.Gr5Status = "Pending for Checker Side";
                            pm.Gr5_Amount = 0;
                            foreach (PutListUCGRDTO k in obj.Detail)
                            {

                                PurchaseOrderDetailRecived detail = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderDetailRecivedId == k.PurchaseOrderDetailRecivedId).SingleOrDefault();

                                ItemMaster IM = db.itemMasters.Where(a => a.Number == detail.ItemNumber && a.ItemMultiMRPId == detail.ItemMultiMRPId5 && a.WarehouseId == pm.WarehouseId).FirstOrDefault();
                                TemporaryCurrentStock Tcs = db.TemporaryCurrentStockDB.Where(x => x.ItemNumber == IM.Number && x.WarehouseId == pm.WarehouseId && x.CompanyId == IM.CompanyId && x.ItemMultiMRPId == detail.ItemMultiMRPId).SingleOrDefault();
                                if (k.QtyRecived != 0)
                                {
                                    /// Deduct Stock from Temporary current stock
                                    TemporaryCurrentStockHistory Tcsh = new TemporaryCurrentStockHistory();
                                    if (Tcs != null)
                                    {
                                        Tcsh.StockId = Tcs.Id;
                                        Tcsh.ItemNumber = Tcs.ItemNumber;
                                        Tcsh.itemname = Tcs.itemname;
                                        Tcsh.OdOrPoId = obj.PurchaseOrderId;
                                        Tcsh.CurrentInventory = Tcs.CurrentInventory;
                                        Tcsh.InventoryOut = Convert.ToInt32(k.QtyRecived);
                                        Tcsh.TotalCurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived);
                                        Tcsh.ExpInventoryOut = detail.ExpQtyRecived1 - k.ExpQtyRecived;
                                        Tcsh.TotalExpCurrentInventory = Tcs.ExpCurrentInventory - (detail.ExpQtyRecived1 - k.ExpQtyRecived);

                                        Tcsh.DamageInventoryOut = detail.DamagQtyRecived1 - Convert.ToInt32(k.DamagQtyRecived);
                                        Tcsh.TotalDamageCurrentInventory = Tcs.DamageCurrentInventory - (detail.DamagQtyRecived1 - Convert.ToInt32(k.DamagQtyRecived));

                                        Tcsh.WarehouseName = Tcs.WarehouseName;
                                        Tcsh.Warehouseid = Tcs.WarehouseId;
                                        Tcsh.CompanyId = Tcs.CompanyId;

                                        Tcsh.CreationDate = indianTime;
                                        Tcsh.userid = userid;
                                        Tcsh.ItemMultiMRPId = Tcs.ItemMultiMRPId;
                                        db.TemporaryCurrentStockHistoryDB.Add(Tcsh);
                                    }
                                    Tcs.CurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.QtyRecived);
                                    db.UpdateTempCurrentStock(Tcs);

                                }
                                detail.QtyRecived5 = k.QtyRecived;
                                detail.DamagQtyRecived5 = k.DamagQtyRecived;
                                detail.ExpQtyRecived5 = k.ExpQtyRecived;
                                detail.GRDate5 = indianTime;
                                detail.PriceRecived = Convert.ToDouble(detail.Price1 * detail.QtyRecived1) + Convert.ToDouble(detail.Price2 * detail.QtyRecived2) + Convert.ToDouble(detail.Price3 * detail.QtyRecived3) + Convert.ToDouble(detail.Price4 * detail.QtyRecived4) + Convert.ToDouble(detail.Price5 * k.QtyRecived);
                                pm.Gr5_Amount += detail.PriceRecived;
                            }
                            db.Commit();
                        }
                        break;
                    default:
                        logger.Info("End  PurchaseOrderDetail: No data found in switch case.");
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "");
                }
                logger.Info("End  : ");
                return Request.CreateResponse(HttpStatusCode.OK, pm); ;
            }
            catch (Exception ex)
            {
                logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                logger.Info("End  PurchaseOrderDetail: ");
                return Request.CreateResponse(HttpStatusCode.BadRequest, "");
            }
        }
        #endregion


        ///// <summary>
        ///// Cancel GR
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <returns></returns>
        //[Route("CancelGrDetail")]
        //[HttpPost]
        //public HttpResponseMessage CancelGrDetailPO(PutUCGRDTO obj)
        //{
        //    logger.Info("start : ");
        //    try
        //    {
        //        // Access claims
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        int Warehouse_id = 0;

        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //            compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
        //            Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
        //        int CompanyId = compid;

        //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //        //var people = db.Peoples.Where(x => x.PeopleID == userid && x.Deleted == false && x.Active).SingleOrDefault();

        //        string sAllCharacter = obj.GrNumber;
        //        char GRType = sAllCharacter[sAllCharacter.Length - 1];
        //        PurchaseOrderMaster pm = new PurchaseOrderMaster();
        //        switch (GRType)
        //        {
        //            case 'A':
        //                using (AuthContext db = new AuthContext())
        //                {
        //                    var people = db.Peoples.Where(x => x.PeopleID == userid && x.Deleted == false && x.Active).SingleOrDefault();
        //                    pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();


        //                    foreach (PutListUCGRDTO k in obj.Detail)
        //                    {
        //                        PurchaseOrderDetailRecived detail = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderDetailRecivedId == k.PurchaseOrderDetailRecivedId).SingleOrDefault();
        //                        CurrentStock Cst = db.DbCurrentStock.Where(x => x.ItemNumber == detail.ItemNumber && x.WarehouseId == detail.WarehouseId && x.ItemMultiMRPId == detail.ItemMultiMRPId1).SingleOrDefault();

        //                        var IRM = db.IRMasterDB.Where(x => x.PurchaseOrderId == obj.PurchaseOrderId).ToList();

        //                        if (IRM.Count == 0)
        //                        {
        //                            if (Cst.CurrentInventory >= detail.QtyRecived1)
        //                            {
        //                                Cst.CurrentInventory = Cst.CurrentInventory - Convert.ToInt32(detail.QtyRecived1);
        //                                // db.Commit();

        //                                CurrentStock item = db.DbCurrentStock.Where(x => x.ItemNumber == detail.ItemNumber && x.WarehouseId == detail.WarehouseId && x.ItemMultiMRPId == detail.ItemMultiMRPId1 && !x.Deleted).SingleOrDefault();
        //                                CurrentStockHistory Oss = new CurrentStockHistory();
        //                                if (item != null)
        //                                {
        //                                    Oss.StockId = item.StockId;
        //                                    Oss.ItemNumber = item.ItemNumber;
        //                                    Oss.itemname = item.itemname;
        //                                    Oss.OdOrPoId = obj.PurchaseOrderId;
        //                                    Oss.CurrentInventory = item.CurrentInventory;
        //                                    Oss.PurchaseInventoryOut = Convert.ToInt32(detail.QtyRecived1);
        //                                    Oss.TotalInventory = item.CurrentInventory;
        //                                    Oss.WarehouseName = item.WarehouseName;
        //                                    Oss.Warehouseid = item.WarehouseId;
        //                                    Oss.CompanyId = item.CompanyId;
        //                                    Oss.CreationDate = indianTime;
        //                                    Oss.ManualReason = "(-)Stock GRN Canceled: " + sAllCharacter;
        //                                    Oss.ItemMultiMRPId = item.ItemMultiMRPId;
        //                                    Oss.MRP = item.MRP;
        //                                    Oss.UOM = item.UOM;
        //                                    Oss.userid = people.PeopleID;
        //                                    Oss.UserName = people.DisplayName;
        //                                    db.CurrentStockHistoryDb.Add(Oss);
        //                                    pm.Gr1Status = "Canceled GR";
        //                                    //  item.CurrentInventory = item.CurrentInventory - Convert.ToInt32(detail.QtyRecived1);
        //                                    // db.UpdateCurrentStock(item);
        //                                }

        //                            }
        //                            else
        //                            {
        //                                return Request.CreateResponse(HttpStatusCode.OK, "2");
        //                            }

        //                        }
        //                        else
        //                        {
        //                            return Request.CreateResponse(HttpStatusCode.OK, "1");
        //                        }

        //                        // #region ass Free item stock in Freestock table

        //                    }
        //                    //  db.Commit();

        //                    List<FreeItem> FI = db.FreeItemDb.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId && a.GRNumber == obj.GrNumber).ToList();
        //                    var UserName = db.Peoples.Where(y => y.PeopleID == userid).Select(b => b.DisplayName).SingleOrDefault();

        //                    foreach (FreeItem f in FI)
        //                    {
        //                        FreeStock stok = db.FreeStockDB.Where(x => x.ItemNumber == f.itemNumber && x.WarehouseId == f.WarehouseId && x.ItemMultiMRPId == f.ItemMultiMRPId).FirstOrDefault();
        //                        ItemMultiMRP MRP = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == f.ItemMultiMRPId).SingleOrDefault();
        //                        if (stok.CurrentInventory >= f.TotalQuantity)
        //                        {
        //                            FreeStockHistory Oss = new FreeStockHistory();
        //                            Oss.ManualReason = "Free Item";
        //                            Oss.FreeStockId = stok.FreeStockId;
        //                            Oss.ItemMultiMRPId = stok.ItemMultiMRPId;
        //                            Oss.ItemNumber = stok.ItemNumber;
        //                            Oss.itemname = stok.itemname;
        //                            Oss.OdOrPoId = obj.PurchaseOrderId;
        //                            Oss.CurrentInventory = f.TotalQuantity;
        //                            Oss.PurchaseInventoryOut = f.TotalQuantity;
        //                            Oss.TotalInventory = Convert.ToInt32(stok.CurrentInventory - f.TotalQuantity);
        //                            Oss.WarehouseId = stok.WarehouseId;
        //                            Oss.CreationDate = DateTime.Now;
        //                            db.FreeStockHistoryDB.Add(Oss);
        //                            stok.CurrentInventory = stok.CurrentInventory - f.TotalQuantity;
        //                            if (stok.CurrentInventory < 0)
        //                            {
        //                                stok.CurrentInventory = 0;
        //                            }
        //                            db.Entry(stok).State = EntityState.Modified;
        //                        }
        //                        else
        //                        {
        //                            return Request.CreateResponse(HttpStatusCode.OK, "3");
        //                        }
        //                    }

        //                    db.Commit();

        //                }

        //                break;
        //            case 'B':
        //                using (AuthContext db = new AuthContext())
        //                {
        //                    var people = db.Peoples.Where(x => x.PeopleID == userid && x.Deleted == false && x.Active).SingleOrDefault();
        //                    pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();

        //                    foreach (PutListUCGRDTO k in obj.Detail)
        //                    {

        //                        PurchaseOrderDetailRecived detail = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderDetailRecivedId == k.PurchaseOrderDetailRecivedId).SingleOrDefault();
        //                        CurrentStock Cst = db.DbCurrentStock.Where(x => x.ItemNumber == detail.ItemNumber && x.WarehouseId == detail.WarehouseId && x.ItemMultiMRPId == detail.ItemMultiMRPId2).SingleOrDefault();
        //                        var IRM = db.IRMasterDB.Where(x => x.PurchaseOrderId == obj.PurchaseOrderId).ToList();
        //                        if (IRM.Count == 0)
        //                        {
        //                            if (Cst.CurrentInventory >= detail.QtyRecived2)
        //                            {
        //                                Cst.CurrentInventory = Cst.CurrentInventory - Convert.ToInt32(detail.QtyRecived2);
        //                                // db.Commit();

        //                                CurrentStock item = db.DbCurrentStock.Where(x => x.ItemNumber == detail.ItemNumber && x.WarehouseId == detail.WarehouseId && x.ItemMultiMRPId == detail.ItemMultiMRPId2 && !x.Deleted).SingleOrDefault();
        //                                CurrentStockHistory Oss = new CurrentStockHistory();
        //                                if (item != null)
        //                                {
        //                                    Oss.StockId = item.StockId;
        //                                    Oss.ItemNumber = item.ItemNumber;
        //                                    Oss.itemname = item.itemname;
        //                                    Oss.OdOrPoId = obj.PurchaseOrderId;
        //                                    Oss.CurrentInventory = item.CurrentInventory;
        //                                    Oss.PurchaseInventoryOut = Convert.ToInt32(detail.QtyRecived2);
        //                                    Oss.TotalInventory = item.CurrentInventory;
        //                                    Oss.WarehouseName = item.WarehouseName;
        //                                    Oss.Warehouseid = item.WarehouseId;
        //                                    Oss.CompanyId = item.CompanyId;
        //                                    Oss.CreationDate = indianTime;
        //                                    Oss.ManualReason = "(-)Stock GRN Canceled: " + sAllCharacter;
        //                                    Oss.ItemMultiMRPId = item.ItemMultiMRPId;
        //                                    Oss.MRP = item.MRP;
        //                                    Oss.UOM = item.UOM;
        //                                    Oss.userid = people.PeopleID;
        //                                    Oss.UserName = people.DisplayName;
        //                                    db.CurrentStockHistoryDb.Add(Oss);
        //                                    pm.Gr2Status = "Canceled GR";
        //                                }
        //                            }
        //                            else
        //                            {
        //                                return Request.CreateResponse(HttpStatusCode.OK, "2");
        //                            }
        //                        }
        //                        else
        //                        {
        //                            return Request.CreateResponse(HttpStatusCode.OK, "1");
        //                        }
        //                    }

        //                    List<FreeItem> FI = db.FreeItemDb.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId && a.GRNumber == obj.GrNumber).ToList();
        //                    var UserName = db.Peoples.Where(y => y.PeopleID == userid).Select(b => b.DisplayName).SingleOrDefault();

        //                    foreach (FreeItem f in FI)
        //                    {
        //                        FreeStock stok = db.FreeStockDB.Where(x => x.ItemNumber == f.itemNumber && x.WarehouseId == f.WarehouseId && x.ItemMultiMRPId == f.ItemMultiMRPId).FirstOrDefault();
        //                        ItemMultiMRP MRP = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == f.ItemMultiMRPId).SingleOrDefault();
        //                        if (stok.CurrentInventory >= f.TotalQuantity)
        //                        {
        //                            FreeStockHistory Oss = new FreeStockHistory();
        //                            Oss.ManualReason = "Free Item";
        //                            Oss.FreeStockId = stok.FreeStockId;
        //                            Oss.ItemMultiMRPId = stok.ItemMultiMRPId;
        //                            Oss.ItemNumber = stok.ItemNumber;
        //                            Oss.itemname = stok.itemname;
        //                            Oss.OdOrPoId = obj.PurchaseOrderId;
        //                            Oss.CurrentInventory = f.TotalQuantity;
        //                            Oss.PurchaseInventoryOut = f.TotalQuantity;
        //                            Oss.TotalInventory = Convert.ToInt32(stok.CurrentInventory - f.TotalQuantity);
        //                            Oss.WarehouseId = stok.WarehouseId;
        //                            Oss.CreationDate = DateTime.Now;
        //                            db.FreeStockHistoryDB.Add(Oss);
        //                            stok.CurrentInventory = stok.CurrentInventory - f.TotalQuantity;
        //                            if (stok.CurrentInventory < 0)
        //                            {
        //                                stok.CurrentInventory = 0;
        //                            }
        //                            db.Entry(stok).State = EntityState.Modified;
        //                        }
        //                        else
        //                        {
        //                            return Request.CreateResponse(HttpStatusCode.OK, "3");
        //                        }
        //                    }

        //                    db.Commit();
        //                }
        //                break;
        //            case 'C':
        //                using (AuthContext db = new AuthContext())
        //                {
        //                    var people = db.Peoples.Where(x => x.PeopleID == userid && x.Deleted == false && x.Active).SingleOrDefault();

        //                    pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
        //                    foreach (PutListUCGRDTO k in obj.Detail)
        //                    {

        //                        PurchaseOrderDetailRecived detail = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderDetailRecivedId == k.PurchaseOrderDetailRecivedId).SingleOrDefault();
        //                        CurrentStock Cst = db.DbCurrentStock.Where(x => x.ItemNumber == detail.ItemNumber && x.WarehouseId == detail.WarehouseId && x.ItemMultiMRPId == detail.ItemMultiMRPId3).SingleOrDefault();

        //                        var IRM = db.IRMasterDB.Where(x => x.PurchaseOrderId == obj.PurchaseOrderId).ToList();
        //                        if (IRM.Count == 0)
        //                        {
        //                            if (Cst.CurrentInventory >= detail.QtyRecived3)
        //                            {
        //                                Cst.CurrentInventory = Cst.CurrentInventory - Convert.ToInt32(detail.QtyRecived3);
        //                                // db.Commit();

        //                                CurrentStock item = db.DbCurrentStock.Where(x => x.ItemNumber == detail.ItemNumber && x.WarehouseId == detail.WarehouseId && x.ItemMultiMRPId == detail.ItemMultiMRPId3 && !x.Deleted).SingleOrDefault();
        //                                CurrentStockHistory Oss = new CurrentStockHistory();
        //                                if (item != null)
        //                                {
        //                                    Oss.StockId = item.StockId;
        //                                    Oss.ItemNumber = item.ItemNumber;
        //                                    Oss.itemname = item.itemname;
        //                                    Oss.OdOrPoId = obj.PurchaseOrderId;
        //                                    Oss.CurrentInventory = item.CurrentInventory;
        //                                    Oss.PurchaseInventoryOut = Convert.ToInt32(detail.QtyRecived3);
        //                                    Oss.TotalInventory = item.CurrentInventory;
        //                                    Oss.WarehouseName = item.WarehouseName;
        //                                    Oss.Warehouseid = item.WarehouseId;
        //                                    Oss.CompanyId = item.CompanyId;
        //                                    Oss.CreationDate = indianTime;
        //                                    Oss.ManualReason = "(-)Stock GRN Canceled: " + sAllCharacter;
        //                                    Oss.ItemMultiMRPId = item.ItemMultiMRPId;
        //                                    Oss.MRP = item.MRP;
        //                                    Oss.UOM = item.UOM;
        //                                    Oss.userid = people.PeopleID;
        //                                    Oss.UserName = people.DisplayName;
        //                                    db.CurrentStockHistoryDb.Add(Oss);
        //                                    pm.Gr3Status = "Canceled GR";
        //                                }
        //                            }
        //                            else
        //                            {
        //                                return Request.CreateResponse(HttpStatusCode.OK, "2");
        //                            }
        //                        }
        //                        else
        //                        {
        //                            return Request.CreateResponse(HttpStatusCode.OK, "1");
        //                        }
        //                    }

        //                    List<FreeItem> FI = db.FreeItemDb.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId && a.GRNumber == obj.GrNumber).ToList();
        //                    var UserName = db.Peoples.Where(y => y.PeopleID == userid).Select(b => b.DisplayName).SingleOrDefault();

        //                    foreach (FreeItem f in FI)
        //                    {
        //                        FreeStock stok = db.FreeStockDB.Where(x => x.ItemNumber == f.itemNumber && x.WarehouseId == f.WarehouseId && x.ItemMultiMRPId == f.ItemMultiMRPId).FirstOrDefault();
        //                        ItemMultiMRP MRP = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == f.ItemMultiMRPId).SingleOrDefault();
        //                        if (stok.CurrentInventory >= f.TotalQuantity)
        //                        {
        //                            FreeStockHistory Oss = new FreeStockHistory();
        //                            Oss.ManualReason = "Free Item";
        //                            Oss.FreeStockId = stok.FreeStockId;
        //                            Oss.ItemMultiMRPId = stok.ItemMultiMRPId;
        //                            Oss.ItemNumber = stok.ItemNumber;
        //                            Oss.itemname = stok.itemname;
        //                            Oss.OdOrPoId = obj.PurchaseOrderId;
        //                            Oss.CurrentInventory = f.TotalQuantity;
        //                            Oss.PurchaseInventoryOut = f.TotalQuantity;
        //                            Oss.TotalInventory = Convert.ToInt32(stok.CurrentInventory - f.TotalQuantity);
        //                            Oss.WarehouseId = stok.WarehouseId;
        //                            Oss.CreationDate = DateTime.Now;
        //                            db.FreeStockHistoryDB.Add(Oss);
        //                            stok.CurrentInventory = stok.CurrentInventory - f.TotalQuantity;
        //                            if (stok.CurrentInventory < 0)
        //                            {
        //                                stok.CurrentInventory = 0;
        //                            }
        //                            db.Entry(stok).State = EntityState.Modified;
        //                        }
        //                        else
        //                        {
        //                            return Request.CreateResponse(HttpStatusCode.OK, "3");
        //                        }
        //                    }
        //                    db.Commit();
        //                }
        //                break;
        //            case 'D':
        //                using (AuthContext db = new AuthContext())
        //                {
        //                    var people = db.Peoples.Where(x => x.PeopleID == userid && x.Deleted == false && x.Active).SingleOrDefault();

        //                    pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
        //                    foreach (PutListUCGRDTO k in obj.Detail)
        //                    {

        //                        PurchaseOrderDetailRecived detail = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderDetailRecivedId == k.PurchaseOrderDetailRecivedId).SingleOrDefault();
        //                        CurrentStock Cst = db.DbCurrentStock.Where(x => x.ItemNumber == detail.ItemNumber && x.WarehouseId == detail.WarehouseId && x.ItemMultiMRPId == detail.ItemMultiMRPId4).SingleOrDefault();

        //                        var IRM = db.IRMasterDB.Where(x => x.PurchaseOrderId == obj.PurchaseOrderId).ToList();
        //                        if (IRM.Count == 0)
        //                        {
        //                            if (Cst.CurrentInventory >= detail.QtyRecived4)
        //                            {
        //                                Cst.CurrentInventory = Cst.CurrentInventory - Convert.ToInt32(detail.QtyRecived4);
        //                                //db.Commit();

        //                                CurrentStock item = db.DbCurrentStock.Where(x => x.ItemNumber == detail.ItemNumber && x.WarehouseId == detail.WarehouseId && x.ItemMultiMRPId == detail.ItemMultiMRPId4 && !x.Deleted).SingleOrDefault();
        //                                CurrentStockHistory Oss = new CurrentStockHistory();
        //                                if (item != null)
        //                                {
        //                                    Oss.StockId = item.StockId;
        //                                    Oss.ItemNumber = item.ItemNumber;
        //                                    Oss.itemname = item.itemname;
        //                                    Oss.OdOrPoId = obj.PurchaseOrderId;
        //                                    Oss.CurrentInventory = item.CurrentInventory;
        //                                    Oss.PurchaseInventoryOut = Convert.ToInt32(detail.QtyRecived4);
        //                                    Oss.TotalInventory = item.CurrentInventory;
        //                                    Oss.WarehouseName = item.WarehouseName;
        //                                    Oss.Warehouseid = item.WarehouseId;
        //                                    Oss.CompanyId = item.CompanyId;
        //                                    Oss.CreationDate = indianTime;
        //                                    Oss.ManualReason = "(-)Stock GRN Canceled: " + sAllCharacter;
        //                                    Oss.ItemMultiMRPId = item.ItemMultiMRPId;
        //                                    Oss.MRP = item.MRP;
        //                                    Oss.UOM = item.UOM;
        //                                    Oss.userid = people.PeopleID;
        //                                    Oss.UserName = people.DisplayName;
        //                                    db.CurrentStockHistoryDb.Add(Oss);
        //                                    pm.Gr4Status = "Canceled GR";
        //                                }
        //                            }
        //                            else
        //                            {
        //                                return Request.CreateResponse(HttpStatusCode.OK, "2");
        //                            }
        //                        }
        //                        else
        //                        {
        //                            return Request.CreateResponse(HttpStatusCode.OK, "1");
        //                        }
        //                    }

        //                    List<FreeItem> FI = db.FreeItemDb.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId && a.GRNumber == obj.GrNumber).ToList();
        //                    var UserName = db.Peoples.Where(y => y.PeopleID == userid).Select(b => b.DisplayName).SingleOrDefault();

        //                    foreach (FreeItem f in FI)
        //                    {
        //                        FreeStock stok = db.FreeStockDB.Where(x => x.ItemNumber == f.itemNumber && x.WarehouseId == f.WarehouseId && x.ItemMultiMRPId == f.ItemMultiMRPId).FirstOrDefault();
        //                        ItemMultiMRP MRP = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == f.ItemMultiMRPId).SingleOrDefault();
        //                        if (stok.CurrentInventory >= f.TotalQuantity)
        //                        {
        //                            FreeStockHistory Oss = new FreeStockHistory();
        //                            Oss.ManualReason = "Free Item";
        //                            Oss.FreeStockId = stok.FreeStockId;
        //                            Oss.ItemMultiMRPId = stok.ItemMultiMRPId;
        //                            Oss.ItemNumber = stok.ItemNumber;
        //                            Oss.itemname = stok.itemname;
        //                            Oss.OdOrPoId = obj.PurchaseOrderId;
        //                            Oss.CurrentInventory = f.TotalQuantity;
        //                            Oss.PurchaseInventoryOut = f.TotalQuantity;
        //                            Oss.TotalInventory = Convert.ToInt32(stok.CurrentInventory - f.TotalQuantity);
        //                            Oss.WarehouseId = stok.WarehouseId;
        //                            Oss.CreationDate = DateTime.Now;
        //                            db.FreeStockHistoryDB.Add(Oss);
        //                            stok.CurrentInventory = stok.CurrentInventory - f.TotalQuantity;
        //                            if (stok.CurrentInventory < 0)
        //                            {
        //                                stok.CurrentInventory = 0;
        //                            }
        //                            db.Entry(stok).State = EntityState.Modified;
        //                        }
        //                        else
        //                        {
        //                            return Request.CreateResponse(HttpStatusCode.OK, "3");
        //                        }
        //                    }
        //                    db.Commit();

        //                }
        //                break;
        //            case 'E':
        //                using (AuthContext db = new AuthContext())
        //                {
        //                    var people = db.Peoples.Where(x => x.PeopleID == userid && x.Deleted == false && x.Active).SingleOrDefault();

        //                    pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
        //                    // pm.Gr5Status = "Approved";
        //                    //pm.Gr5_Amount = 0;
        //                    foreach (PutListUCGRDTO k in obj.Detail)
        //                    {

        //                        PurchaseOrderDetailRecived detail = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderDetailRecivedId == k.PurchaseOrderDetailRecivedId).SingleOrDefault();
        //                        CurrentStock Cst = db.DbCurrentStock.Where(x => x.ItemNumber == detail.ItemNumber && x.WarehouseId == detail.WarehouseId && x.ItemMultiMRPId == detail.ItemMultiMRPId5).SingleOrDefault();
        //                        var IRM = db.IRMasterDB.Where(x => x.PurchaseOrderId == obj.PurchaseOrderId).ToList();
        //                        if (IRM.Count == 0)
        //                        {
        //                            if (Cst.CurrentInventory >= detail.QtyRecived5)
        //                            {
        //                                Cst.CurrentInventory = Cst.CurrentInventory - Convert.ToInt32(detail.QtyRecived5);
        //                                //db.Commit();

        //                                CurrentStock item = db.DbCurrentStock.Where(x => x.ItemNumber == detail.ItemNumber && x.WarehouseId == detail.WarehouseId && x.ItemMultiMRPId == detail.ItemMultiMRPId5 && !x.Deleted).SingleOrDefault();
        //                                CurrentStockHistory Oss = new CurrentStockHistory();
        //                                if (item != null)
        //                                {
        //                                    Oss.StockId = item.StockId;
        //                                    Oss.ItemNumber = item.ItemNumber;
        //                                    Oss.itemname = item.itemname;
        //                                    Oss.OdOrPoId = obj.PurchaseOrderId;
        //                                    Oss.CurrentInventory = item.CurrentInventory;
        //                                    Oss.PurchaseInventoryOut = Convert.ToInt32(detail.QtyRecived5);
        //                                    Oss.TotalInventory = item.CurrentInventory;
        //                                    Oss.WarehouseName = item.WarehouseName;
        //                                    Oss.Warehouseid = item.WarehouseId;
        //                                    Oss.CompanyId = item.CompanyId;
        //                                    Oss.CreationDate = indianTime;
        //                                    Oss.ManualReason = "(-)Stock GRN Canceled: " + sAllCharacter;
        //                                    Oss.ItemMultiMRPId = item.ItemMultiMRPId;
        //                                    Oss.MRP = item.MRP;
        //                                    Oss.UOM = item.UOM;
        //                                    Oss.userid = people.PeopleID;
        //                                    Oss.UserName = people.DisplayName;
        //                                    db.CurrentStockHistoryDb.Add(Oss);
        //                                    pm.Gr5Status = "Canceled GR";
        //                                }
        //                            }
        //                            else
        //                            {
        //                                return Request.CreateResponse(HttpStatusCode.OK, "2");
        //                            }
        //                        }
        //                        else
        //                        {
        //                            return Request.CreateResponse(HttpStatusCode.OK, "1");
        //                        }
        //                    }

        //                    List<FreeItem> FI = db.FreeItemDb.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId && a.GRNumber == obj.GrNumber).ToList();
        //                    var UserName = db.Peoples.Where(y => y.PeopleID == userid).Select(b => b.DisplayName).SingleOrDefault();

        //                    foreach (FreeItem f in FI)
        //                    {
        //                        FreeStock stok = db.FreeStockDB.Where(x => x.ItemNumber == f.itemNumber && x.WarehouseId == f.WarehouseId && x.ItemMultiMRPId == f.ItemMultiMRPId).FirstOrDefault();
        //                        ItemMultiMRP MRP = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == f.ItemMultiMRPId).SingleOrDefault();
        //                        if (stok.CurrentInventory >= f.TotalQuantity)
        //                        {
        //                            FreeStockHistory Oss = new FreeStockHistory();
        //                            Oss.ManualReason = "Free Item";
        //                            Oss.FreeStockId = stok.FreeStockId;
        //                            Oss.ItemMultiMRPId = stok.ItemMultiMRPId;
        //                            Oss.ItemNumber = stok.ItemNumber;
        //                            Oss.itemname = stok.itemname;
        //                            Oss.OdOrPoId = obj.PurchaseOrderId;
        //                            Oss.CurrentInventory = f.TotalQuantity;
        //                            Oss.PurchaseInventoryOut = f.TotalQuantity;
        //                            Oss.TotalInventory = Convert.ToInt32(stok.CurrentInventory - f.TotalQuantity);
        //                            Oss.WarehouseId = stok.WarehouseId;
        //                            Oss.CreationDate = DateTime.Now;
        //                            db.FreeStockHistoryDB.Add(Oss);
        //                            stok.CurrentInventory = stok.CurrentInventory - f.TotalQuantity;
        //                            if (stok.CurrentInventory < 0)
        //                            {
        //                                stok.CurrentInventory = 0;
        //                            }
        //                            db.Entry(stok).State = EntityState.Modified;
        //                        }
        //                        else
        //                        {
        //                            return Request.CreateResponse(HttpStatusCode.OK, "3");
        //                        }
        //                    }
        //                    db.Commit();

        //                }
        //                break;
        //            default:
        //                logger.Info("End  PurchaseOrderDetail: No data found in switch case.");
        //                return Request.CreateResponse(HttpStatusCode.BadRequest, "");
        //        }
        //        logger.Info("End  : ");
        //        return Request.CreateResponse(HttpStatusCode.OK, pm); ;
        //    }

        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in PurchaseOrderDetail " + ex.Message);
        //        logger.Info("End  PurchaseOrderDetail: ");
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, "");
        //    }
        //}


        #region
        /// <summary>
        /// Purchase Register Page 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>

        [Route("GetPurchaseReport")]
        [HttpGet]
        public dynamic GetPurchaseReport(DateTime from, DateTime to)
        {

            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var param1 = new SqlParameter("StartDate", from);
                    var param2 = new SqlParameter("EndDate", to);

                    var PurchseReport = db.Database.SqlQuery<PurchaseReportData>("exec SP_PurchaseReport @StartDate,@EndDate", param1, param2).OrderByDescending(t => t.PurchaseOrderId).ToList();

                    return Ok(PurchseReport);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }


        [Route("GetPurchaseRegistorData")]
        [HttpPost]
        [AllowAnonymous]
        public dynamic GetPurchaseRegistorData(PurchaseRegisterDC purchaseRegisterDC)
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var WareIdDtw = new DataTable();
                    WareIdDtw.Columns.Add("Intvalue");
                    foreach (var item in purchaseRegisterDC.warehouseid)
                    {
                        var dr = WareIdDtw.NewRow();
                        dr["IntValue"] = item;
                        WareIdDtw.Rows.Add(dr);
                    }

                    var Warehouseids = new SqlParameter
                    {
                        ParameterName = "warehouse",
                        SqlDbType = SqlDbType.Structured,
                        TypeName = "dbo.IntValues",
                        Value = WareIdDtw
                    };
                    var StartDate = new SqlParameter
                    {
                        ParameterName = "startDate",
                        Value = purchaseRegisterDC.from
                    };

                    var EndDate = new SqlParameter
                    {
                        ParameterName = "endDate",
                        Value = purchaseRegisterDC.to
                    };
                    db.Database.CommandTimeout = 1200;
                    var PurchaseRegistorData = db.Database.SqlQuery<PurchaseReportDataDc>("exec GetPurchaseRegistorData @startDate,@endDate,@warehouse", StartDate, EndDate, Warehouseids).OrderByDescending(t => t.PurchaseOrderId).ToList();

                    return Ok(PurchaseRegistorData);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion

        #region
        [Route("SearchPo")]
        [HttpGet]
        public dynamic SearchPo(int PoId)
        {
            logger.Info("start : ");
            try
            {
                // Access claims
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                int CompanyId = compid;
                List<PurchaseOrderMaster> pm = new List<PurchaseOrderMaster>();
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                var predicate = PredicateBuilder.True<PurchaseOrderMaster>();
                predicate = predicate.And(x => x.Gr1Status == "Approved" || x.Gr2Status == "Approved" || x.Gr3Status == "Approved" || x.Gr4Status == "Approved" || x.Gr5Status == "Approved");
                // predicate = predicate.And(e => e.PurchaseOrderId == PoId);
                List<GetUCGRDTO> HeadData = new List<GetUCGRDTO>();
                // string PurchaseOrderId= Convert.ToInt32(PoId);
                using (AuthContext db = new AuthContext())
                {
                    pm = db.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == PoId).ToList();

                    var IRM = db.IRMasterDB.Where(x => x.PurchaseOrderId == PoId).ToList();
                    if (IRM.Count == 0)
                    {
                        foreach (PurchaseOrderMaster a in pm)
                        {
                            List<PurchaseOrderDetailRecived> _detail = db.PurchaseOrderRecivedDetails.Where(q => q.PurchaseOrderId == a.PurchaseOrderId).ToList();

                            if (a.Gr1_Amount != 0 && a.Gr1Status == "Approved")
                            {
                                List<GetListUCGRDTO> HeadDetail = new List<GetListUCGRDTO>();
                                foreach (PurchaseOrderDetailRecived b in _detail)
                                {
                                    GetListUCGRDTO UCL = new GetListUCGRDTO()
                                    {
                                        PurchaseOrderDetailRecivedId = b.PurchaseOrderDetailRecivedId,
                                        ItemId = b.ItemId,
                                        ItemName = b.ItemName1,
                                        Price = Convert.ToDouble(b.Price1),
                                        HSNCode = b.HSNCode,
                                        TotalQuantity = b.TotalQuantity,
                                        QtyRecived = Convert.ToInt32(b.QtyRecived1),
                                        DamagQtyRecived = b.DamagQtyRecived1,
                                        ExpQtyRecived = b.ExpQtyRecived1,
                                        BatchNo = b.BatchNo1,
                                        MFG = b.MFGDate1
                                    };
                                    HeadDetail.Add(UCL);
                                }

                                GetUCGRDTO UC = new GetUCGRDTO()
                                {
                                    PurchaseOrderId = a.PurchaseOrderId,
                                    GrNumber = a.Gr1Number,
                                    GrPersonName = a.Gr1PersonName,
                                    GrDate = Convert.ToDateTime(a.Gr1_Date),
                                    GrAmount = a.Gr1_Amount,
                                    VehicleType = a.VehicleType1,
                                    VehicleNumber = a.VehicleNumber1,
                                    Status = a.Gr1Status,
                                    Detail = HeadDetail

                                };
                                HeadData.Add(UC);
                            }
                            if (a.Gr2_Amount != 0 && a.Gr2Status == "Approved")
                            {
                                List<GetListUCGRDTO> HeadDetail = new List<GetListUCGRDTO>();
                                foreach (PurchaseOrderDetailRecived b in _detail)
                                {
                                    GetListUCGRDTO UCL = new GetListUCGRDTO()
                                    {
                                        PurchaseOrderDetailRecivedId = b.PurchaseOrderDetailRecivedId,
                                        ItemId = b.ItemId,
                                        ItemName = b.ItemName2,
                                        Price = Convert.ToDouble(b.Price2),
                                        HSNCode = b.HSNCode,
                                        TotalQuantity = b.TotalQuantity,
                                        QtyRecived = Convert.ToInt32(b.QtyRecived2),
                                        DamagQtyRecived = b.DamagQtyRecived2,
                                        ExpQtyRecived = b.ExpQtyRecived2,
                                        BatchNo = b.BatchNo2,
                                        MFG = b.MFGDate2
                                    };
                                    HeadDetail.Add(UCL);
                                }

                                GetUCGRDTO UC = new GetUCGRDTO()
                                {
                                    PurchaseOrderId = a.PurchaseOrderId,
                                    GrNumber = a.Gr2Number,
                                    GrPersonName = a.Gr2PersonName,
                                    GrDate = Convert.ToDateTime(a.Gr2_Date),
                                    GrAmount = a.Gr2_Amount,
                                    VehicleType = a.VehicleType2,
                                    VehicleNumber = a.VehicleNumber2,
                                    Status = a.Gr2Status,
                                    Detail = HeadDetail

                                };
                                HeadData.Add(UC);
                            }
                            if (a.Gr3_Amount != 0 && a.Gr3Status == "Approved")
                            {
                                List<GetListUCGRDTO> HeadDetail = new List<GetListUCGRDTO>();
                                foreach (PurchaseOrderDetailRecived b in _detail)
                                {
                                    GetListUCGRDTO UCL = new GetListUCGRDTO()
                                    {
                                        PurchaseOrderDetailRecivedId = b.PurchaseOrderDetailRecivedId,
                                        ItemId = b.ItemId,
                                        ItemName = b.ItemName3,
                                        Price = Convert.ToDouble(b.Price3),
                                        HSNCode = b.HSNCode,
                                        TotalQuantity = b.TotalQuantity,
                                        QtyRecived = Convert.ToInt32(b.QtyRecived3),
                                        DamagQtyRecived = b.DamagQtyRecived3,
                                        ExpQtyRecived = b.ExpQtyRecived3,
                                        BatchNo = b.BatchNo3,
                                        MFG = b.MFGDate3
                                    };
                                    HeadDetail.Add(UCL);
                                }

                                GetUCGRDTO UC = new GetUCGRDTO()
                                {
                                    PurchaseOrderId = a.PurchaseOrderId,
                                    GrNumber = a.Gr3Number,
                                    GrPersonName = a.Gr3PersonName,
                                    GrDate = Convert.ToDateTime(a.Gr3_Date),
                                    GrAmount = a.Gr3_Amount,
                                    VehicleType = a.VehicleType3,
                                    VehicleNumber = a.VehicleNumber3,
                                    Status = a.Gr3Status,
                                    Detail = HeadDetail

                                };
                                HeadData.Add(UC);
                            }
                            if (a.Gr4_Amount != 0 && a.Gr4Status == "Approved")
                            {
                                List<GetListUCGRDTO> HeadDetail = new List<GetListUCGRDTO>();
                                foreach (PurchaseOrderDetailRecived b in _detail)
                                {
                                    GetListUCGRDTO UCL = new GetListUCGRDTO()
                                    {
                                        PurchaseOrderDetailRecivedId = b.PurchaseOrderDetailRecivedId,
                                        ItemId = b.ItemId,
                                        ItemName = b.ItemName4,
                                        Price = Convert.ToDouble(b.Price4),
                                        HSNCode = b.HSNCode,
                                        TotalQuantity = b.TotalQuantity,
                                        QtyRecived = Convert.ToInt32(b.QtyRecived4),
                                        DamagQtyRecived = b.DamagQtyRecived4,
                                        ExpQtyRecived = b.ExpQtyRecived4,
                                        BatchNo = b.BatchNo4,
                                        MFG = b.MFGDate4
                                    };
                                    HeadDetail.Add(UCL);
                                }

                                GetUCGRDTO UC = new GetUCGRDTO()
                                {
                                    PurchaseOrderId = a.PurchaseOrderId,
                                    GrNumber = a.Gr4Number,
                                    GrPersonName = a.Gr4PersonName,
                                    GrDate = Convert.ToDateTime(a.Gr4_Date),
                                    GrAmount = a.Gr4_Amount,
                                    VehicleType = a.VehicleType4,
                                    VehicleNumber = a.VehicleNumber4,
                                    Status = a.Gr4Status,
                                    Detail = HeadDetail

                                };
                                HeadData.Add(UC);
                            }
                            if (a.Gr5_Amount != 0 && a.Gr5Status == "Approved")
                            {
                                List<GetListUCGRDTO> HeadDetail = new List<GetListUCGRDTO>();
                                foreach (PurchaseOrderDetailRecived b in _detail)
                                {
                                    GetListUCGRDTO UCL = new GetListUCGRDTO()
                                    {
                                        PurchaseOrderDetailRecivedId = b.PurchaseOrderDetailRecivedId,
                                        ItemId = b.ItemId,
                                        ItemName = b.ItemName5,
                                        Price = Convert.ToDouble(b.Price5),
                                        HSNCode = b.HSNCode,
                                        TotalQuantity = b.TotalQuantity,
                                        QtyRecived = Convert.ToInt32(b.QtyRecived5),
                                        DamagQtyRecived = b.DamagQtyRecived5,
                                        ExpQtyRecived = b.ExpQtyRecived5,
                                        BatchNo = b.BatchNo5,
                                        MFG = b.MFGDate5
                                    };
                                    HeadDetail.Add(UCL);
                                }

                                GetUCGRDTO UC = new GetUCGRDTO()
                                {
                                    PurchaseOrderId = a.PurchaseOrderId,
                                    GrNumber = a.Gr5Number,
                                    GrPersonName = a.Gr5PersonName,
                                    GrDate = Convert.ToDateTime(a.Gr5_Date),
                                    GrAmount = a.Gr5_Amount,
                                    VehicleType = a.VehicleType5,
                                    VehicleNumber = a.VehicleNumber5,
                                    Status = a.Gr5Status,
                                    Detail = HeadDetail
                                };
                                HeadData.Add(UC);
                            }

                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "1");
                    }
                }
                logger.Info("End  : ");
                return Request.CreateResponse(HttpStatusCode.OK, HeadData.OrderByDescending(w => w.PurchaseOrderId)); ;
            }
            catch (Exception ex)
            {
                logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                logger.Info("End  PurchaseOrderDetail: ");
                return Request.CreateResponse(HttpStatusCode.BadRequest, "");
            }
        }

        #endregion

        public static int GetRandomNumber()
        {
            Random r = new Random();
            int genRand = r.Next(100, 50000);
            return genRand;
        }

        public int GetCount(List<PurchaseOrderDetailRecived> detail)
        {
            var count = 0;
            foreach (var a in detail)
            {
                if (count == 0)
                {
                    if (a.QtyRecived5 != 0 || a.DamagQtyRecived5 != 0 || a.ExpQtyRecived5 != 0)
                        count = 5;
                    else
                    {
                        if (a.QtyRecived4 != 0 || a.DamagQtyRecived4 != 0 || a.ExpQtyRecived4 != 0)
                            count = 4;
                        else
                        {
                            if (a.QtyRecived3 != 0 || a.DamagQtyRecived3 != 0 || a.ExpQtyRecived3 != 0)
                                count = 3;
                            else
                            {
                                if (a.QtyRecived2 != 0 || a.DamagQtyRecived2 != 0 || a.ExpQtyRecived2 != 0)
                                    count = 2;
                                else
                                {
                                    if (a.QtyRecived1 != 0 || a.DamagQtyRecived1 != 0 || a.ExpQtyRecived1 != 0)
                                        count = 1;
                                }
                            }
                        }
                    }
                }
                else if (count == 1)
                {
                    if (a.QtyRecived5 != 0 || a.DamagQtyRecived5 != 0 || a.ExpQtyRecived5 != 0)
                        count = 5;
                    else
                    {
                        if (a.QtyRecived4 != 0 || a.DamagQtyRecived4 != 0 || a.ExpQtyRecived4 != 0)
                            count = 4;
                        else
                        {
                            if (a.QtyRecived3 != 0 || a.DamagQtyRecived3 != 0 || a.ExpQtyRecived3 != 0)
                                count = 3;
                            else
                            {
                                if (a.QtyRecived2 != 0 || a.DamagQtyRecived2 != 0 || a.ExpQtyRecived2 != 0)
                                    count = 2;
                            }
                        }
                    }
                }
                else if (count == 2)
                {
                    if (a.QtyRecived5 != 0 || a.DamagQtyRecived5 != 0 || a.ExpQtyRecived5 != 0)
                        count = 5;
                    else
                    {
                        if (a.QtyRecived4 != 0 || a.DamagQtyRecived4 != 0 || a.ExpQtyRecived4 != 0)
                            count = 4;
                        else
                        {
                            if (a.QtyRecived3 != 0 || a.DamagQtyRecived3 != 0 || a.ExpQtyRecived3 != 0)
                                count = 3;
                        }
                    }
                }
                else if (count == 3)
                {
                    if (a.QtyRecived5 != 0 || a.DamagQtyRecived5 != 0 || a.ExpQtyRecived5 != 0)
                        count = 5;
                    else
                    {
                        if (a.QtyRecived4 != 0 || a.DamagQtyRecived4 != 0 || a.ExpQtyRecived4 != 0)
                            count = 4;
                    }
                }
                else if (count == 4)
                {
                    if (a.QtyRecived5 != 0 || a.DamagQtyRecived5 != 0 || a.ExpQtyRecived5 != 0)
                        count = 5;
                }
            }
            return count;
        }
    }

    public class PurchaseRegisterDC
    {
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public List<int> warehouseid { get; set; }
    }
    public class PutUCGRDTO
    {
        public int PurchaseOrderId { get; set; }
        public string GrNumber { get; set; }
        public List<PutListUCGRDTO> Detail { get; set; }
    }
    public class PutListUCGRDTO
    {
        public int PurchaseOrderDetailRecivedId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string HSNCode { get; set; }
        public double? TotalQuantity { get; set; }
        public int QtyRecived { get; set; }
        public int? DamagQtyRecived { get; set; }
        public int? ExpQtyRecived { get; set; }
        public double? Price { get; set; }
        public string BatchNo { get; set; }
        public DateTime? MFG { get; set; }

    }
    public class GetUCGRDTO
    {
        public int PurchaseOrderId { get; set; }
        public string GrNumber { get; set; }
        public string GrPersonName { get; set; }
        public DateTime GrDate { get; set; }
        public double GrAmount { get; set; }
        public string VehicleType { get; set; }
        public string VehicleNumber { get; set; }
        public string Status { get; set; }
        public List<GetListUCGRDTO> Detail { get; set; }
        public List<PoFreeItemMasterDC> FreeDetail { get; set; }
    }
    public class GetListUCGRDTO
    {
        public int PurchaseOrderDetailRecivedId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string HSNCode { get; set; }
        public double? TotalQuantity { get; set; }
        public int QtyRecived { get; set; }
        public int? DamagQtyRecived { get; set; }
        public int? ExpQtyRecived { get; set; }
        public double? Price { get; set; }
        public string BatchNo { get; set; }
        public DateTime? MFG { get; set; }

    }
}