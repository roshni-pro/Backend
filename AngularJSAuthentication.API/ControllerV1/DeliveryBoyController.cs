using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;


namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/DeliveryBoy")]
    public class DeliveryBoyController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Authorize]
        [Route("")]
        public IEnumerable<People> Get()
        {
            logger.Info("start Peoples: ");
            int compid = 0, userid = 0;
            int Warehouse_id = 0;
            string email = "";
            List<People> ass = new List<People>();
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    //int compid = 0, userid = 0;
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
                            Warehouse_id = int.Parse(claim.Value);
                        }
                        if (claim.Type == "email")
                        {
                            email = claim.Value;
                        }


                    }
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    if (Warehouse_id > 0)
                    {
                        ass = context.AllDBoyWid(CompanyId, Warehouse_id).ToList();
                        logger.Info("End  People: ");
                        return ass;
                    }
                    else
                    {
                        ass = context.AllDBoy(CompanyId).ToList();
                        logger.Info("End  People: ");
                        return ass;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in People " + ex.Message);
                    logger.Info("End  People: ");
                    return null;
                }
            }
        }

        [Route("WarehousebasedDeliveryBoyRole")]
        [HttpGet]
        public dynamic WarehousebasedDeliveryBoyRole(int WarehouseId)
        {
            using (var context = new AuthContext())
            {
                List<SqlParameter> paramList = new List<SqlParameter>();
                paramList.Add(new SqlParameter("@WarehouseId", WarehouseId));
                string sqlquery = "GetDeliveryBoyNotDamageAssignmentRole @WarehouseId";
                List<WhDeliveryRole> newdata = context.Database.SqlQuery<WhDeliveryRole>(sqlquery, paramList.ToArray()).ToList();
                 return newdata;
            }
        }


        [ResponseType(typeof(People))]
        [Route("")]
        [AcceptVerbs("POST")]
        public People add(People item)
        {
            logger.Info("start add People: ");
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    string Username = null;
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
                        if (claim.Type == "username")
                        {
                            Username = (claim.Value);
                        }
                    }

                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    item.CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    item.CreatedBy = Username;
                    var dboy = context.AddDboys(item);
                    if (dboy != null)
                    {
                        return dboy;
                    }
                    else { return null; }
                    logger.Info("End add People: ");
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in add People " + ex.Message);
                    logger.Info("End  addPeople: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(People))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public People Put(People item)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    string Username = null;
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
                        if (claim.Type == "username")
                        {
                            Username = (claim.Value);
                        }
                    }
                    item.CompanyId = compid;
                    item.UpdateBy = Username;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    return context.PutDboys(item);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }


        [ResponseType(typeof(People))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("start deletePeopley: ");
            using (var context = new AuthContext())
            {
                try
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
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
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.DeleteDboys(id, CompanyId);
                    logger.Info("End  delete People: ");
                }
                catch (Exception ex)
                {

                    logger.Error("Error in deletePeople" + ex.Message);

                }
            }

        }

        #region Get delivery boy list with there device info 
        /// <summary>
        /// Get Customer list with there device info  //by tejas 21-05-2019
        /// </summary>

        /// <returns></returns>
        //[Authorize]
        [Route("GetDboyDeviceInfo")]
        [HttpGet]
        public HttpResponseMessage GetDboyDeviceInfo(int Cityid)
        {
            logger.Info("start GetDboyDeviceInfo: ");
            using (var context = new AuthContext())
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

                    string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.Cityid=" + Cityid + " and r.Name='Delivery Boy' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                    List<People> TotalItem = context.Database.SqlQuery<People>(query).ToList();
                    //List<People> TotalItem = context.Peoples.Where(x => x.Cityid == Cityid && x.Department == "Delivery Boy").ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, TotalItem);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in TotalLineItem " + ex.Message);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }

        }

        #endregion
    }
}

public class WhDeliveryRole
{
    public string Name { get; set; }
    public int PeopleID { get; set; }
    public string DisplayName { get; set; }
    public string PeopleFirstName { get; set; }
    public string PeopleLastName { get; set; }
    public int WarehouseId { get; set; }
    public string Mobile { get; set; }
}







