using AngularJSAuthentication.API.Controllers.Base;
using GenricEcommers.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/deliverycharge")]
    public class DeliverychargeController : BaseAuthController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        [AcceptVerbs("GET")]
        [Authorize]
        [Route("")]
        public IEnumerable<DeliveryCharge> Get()
        {
            logger.Info("start Category: ");
            List<DeliveryCharge> ass = new List<DeliveryCharge>();
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;


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
                    int CompanyId = compid;

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    if (Warehouse_id > 0)
                    {
                        ass = context.DeliveryChargeDb.Where(x => x.CompanyId == CompanyId && x.WarehouseId == Warehouse_id && x.isDeleted == false).ToList();
                    }
                    else
                    {
                        ass = context.DeliveryChargeDb.Where(x => x.CompanyId == CompanyId && x.isDeleted == false).ToList();

                    }
                    logger.Info("End  Category: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
        }
        [Route("")]
        public DeliveryCharge Get(int id)
        {
            logger.Info("start Category: ");
            DeliveryCharge ass = new DeliveryCharge();
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;


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
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = context.DeliveryChargeDb.Where(x => x.WarehouseId == id && x.CompanyId == CompanyId).FirstOrDefault();
                    logger.Info("End  Category: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
        }

        [AcceptVerbs("POST")]
        [Authorize]
        [Route("")]
        public DeliveryCharge add(DeliveryCharge delivery)
        {
            logger.Info("start deliverycharge");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 1, userid = 0;
                    int Warehouse_id = 0;


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
                    if(delivery.id >0)
                    {
                        delivery.ModifiedDate = DateTime.Now;
                        delivery.ModifiedBy = userid;
                    }
                    else
                    {
                        delivery.CreatedDate = DateTime.Now;
                        delivery.CreatedBy = userid;
                    }
                    delivery.CompanyId = compid;
                   
                    //delivery.WarehouseId = Warehouse_id;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    var del = context.AddUpdateDeliveryCharge(delivery);
                    return del;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in deliverycharge" + ex.Message);
                    logger.Info("End  deliverycharge: ");
                    return null;
                }
            }
        }


        [Route("")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetWarehou(int WarehouseId)//get all Issuances which are active for the delivery boy
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var DeliveryCharge = context.DeliveryChargeDb.Where(x => x.IsActive == true && x.WarehouseId == WarehouseId && x.isDeleted == false && !x.IsDistributor).ToList();
                    if (DeliveryCharge.Count > 0) { }
                    return Request.CreateResponse(HttpStatusCode.OK, DeliveryCharge);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        
        ///// <summary>
        ///// Delivery charge new API by anushka for sales app
        ///// </summary>
        ///// <param name="WarehouseId"></param>
        ///// <param name="PeopleId"></param>
        ///// <returns></returns>
        //[Route("V1")]
        //[HttpGet]
        //public async Task<DeliveryChageDC> GetWarehouse(int WarehouseId,int PeopleId)
        //{
        //    using (AuthContext context = new AuthContext())
        //    {
        //            var query = "Select delcharge.*,sum(agentcom.Amount) as CommissionAmt from DeliveryCharges delcharge inner join Customers cust on delcharge.WarehouseId = cust.Warehouseid inner join AgentCommissionforCities agentcom on agentcom.CustomerId = cust.CustomerId where delcharge.WarehouseId = "+ WarehouseId + " and agentcom.PeopleId = "+ PeopleId + " and delcharge.IsActive = 1 and delcharge.isDeleted = 0 group by delcharge.[id],delcharge.[CompanyId],delcharge.[min_Amount],delcharge.[max_Amount] ,delcharge.[del_Charge],delcharge.[WarehouseId],delcharge.[cluster_Id],delcharge.[warhouse_Name],delcharge.[cluster_Name],delcharge.[IsActive],delcharge.[isDeleted],delcharge.IsDistributor";
        //            DeliveryChageDC Commission =context.Database.SqlQuery<DeliveryChageDC>(query).FirstOrDefault();

        //        return Commission;
                         
        //    }
        //}

        #region get data by id  for deliverycharge 
        /// <summary>
        /// Created by Raj
        /// Created Date:13/08/2019
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("GetById")]
        public DeliveryCharge Getbyid(int id)
        {
            logger.Info("start Category: ");
            DeliveryCharge ass = new DeliveryCharge();
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0, Warehouse_id = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                        Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = context.DeliveryChargeDb.Where(x => x.id == id && x.isDeleted == false).FirstOrDefault();
                    logger.Info("End  Category: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
        }
        #endregion


        #region delete data by id  for deliverycharge 
        /// <summary>
        /// Created by Raj
        /// Created Date:14/08/2019
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("DeleteById")]
        [HttpDelete]
        public DeliveryCharge Deletebyid(int id)
        {
            logger.Info("start Category: ");
            DeliveryCharge ass = new DeliveryCharge();
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0, Warehouse_id = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                        Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = context.DeliveryChargeDb.Where(x => x.id == id && x.isDeleted == false).FirstOrDefault();
                    ass.isDeleted = true;
                    context.Entry(ass).State = EntityState.Modified;
                    context.Commit();
                    logger.Info("End  Category: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
        }
        #endregion

      
        
        
        public class DeliveryChageDC
        {
            public int id { get; set; }
            public int CompanyId { get; set; }
            public double? min_Amount { get; set; }
            public double? max_Amount { get; set; }
            public double? del_Charge { get; set; }
            public int? WarehouseId { get; set; }
            public int? cluster_Id { get; set; }
            public string warhouse_Name { get; set; }
            public string cluster_Name { get; set; }
            public bool IsActive { get; set; }
            public bool isDeleted { get; set; }
            public decimal CommissionAmt { get; set; }
            
        }

    }
}



