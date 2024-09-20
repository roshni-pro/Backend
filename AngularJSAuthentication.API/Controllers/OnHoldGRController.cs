using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/OnHoldGR")]
    public class OnHoldGRController : ApiController
    {
       
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();


        [ResponseType(typeof(OnHoldGR))]
        [Route("AddGR")]
        [AcceptVerbs("POST")]
        public List<OnHoldGR> add(List<OnHoldGR> item)
        {
            logger.Info("start Add OnHoldGR: ");
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    string D_Name = null;

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
                        if (claim.Type == "DisplayName")
                        {
                            D_Name = (claim.Value);
                        }
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    int WarehouseId = item[0].WarehouseId;
                    Warehouse name = db.Warehouses.Where(x => x.WarehouseId == WarehouseId).FirstOrDefault();
                    foreach (var data in item)
                    {
                        OnHoldGR gr = new OnHoldGR();
                        //gr.CompanyId = item.CompanyId;
                        gr.WarehouseId = data.WarehouseId;
                        gr.WarehouseName = name.WarehouseName;
                        gr.CityId = data.CityId;
                        gr.CityName = name.CityName;
                        gr.InvoiceNo = data.InvoiceNo;
                        gr.ItemName = data.ItemName;
                        gr.Price = data.Price;
                        gr.MRP = data.MRP;
                        gr.Qty = data.Qty;
                        gr.Discription = data.Discription;
                        gr.Image = data.Image;
                        gr.CreatedDate = indianTime;
                        gr.UpdatedDate = indianTime;
                        gr.CreatedBy = "Admin";
                        gr.UpdateBy = "Admin";
                        gr.CompanyId = compid;
                        db.OnHoldGRDB.Add(gr);
                        db.Commit();
                    }
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OnHoldGR " + ex.Message);
                    logger.Info("End  OnHoldGR: ");
                    return null;
                }
            }
        }
        /// <summary>
        /// /UplaodImage_On_cloudinary
        /// </summary>
        /// <returns></returns>
        /// 

        [Route("PostImage")]
        [AcceptVerbs("Post")]
        public OnHoldGR PostImage(OnHoldGR item)
        {
            logger.Info("start OnHoldGR: ");
            using (var db = new AuthContext())
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

                    if (item == null)
                    {
                        throw new ArgumentNullException("OnHoldGR");
                    }
                    if (compid == 0)
                    {
                        compid = 1;
                    }


                    OnHoldGR ass = db.OnHoldGRDB.Where(x => x.Deleted == false).FirstOrDefault();



                    db.Entry(ass).State = EntityState.Modified;
                    db.Commit();
                    return ass;

                }
                catch (Exception ex)
                {

                    return null;
                }
            }
        }


        [HttpGet]
        [Route("getAllGR")]
        public List<OnHoldGR> getAllGR()
        {
            logger.Info("start OnHoldGR: ");
            using (var db = new AuthContext())
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
                    List<OnHoldGRcity> retdata = new List<OnHoldGRcity>();
                    List<OnHoldGR> gr = db.OnHoldGRDB.Where(x => x.Deleted == false).ToList();


                    return gr;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add Case " + ex.Message);
                    logger.Info("End  Add Case: ");
                    return null;
                }
        }

        [HttpPost]
        [Route("UpdateGR")]
        public string UpdateGR(OnHoldGR onHoldGR)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    db.OnHoldGRDB.Attach(onHoldGR);
                    db.Entry(onHoldGR).State = EntityState.Modified;
                    db.Commit();
                    return "Success";
                }
                catch (Exception ee)
                {
                    return null;
                }

            }

        }
        public class OnHoldGRcity
        {

            public int OnHoldGRId { get; set; }
            public int CompanyId { get; set; }
            public int CityId { get; set; }
            public string CityName { get; set; }
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public string ItemName { get; set; }
            public string Image { get; set; }
            public string Discription { get; set; }
            public double Price { get; set; }
            public double Qty { get; set; }
            public string InvoiceNo { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime UpdatedDate { get; set; }
            public string CreatedBy { get; set; }
            public string UpdateBy { get; set; }
            public bool Deleted { get; set; }
            public double MRP { get; set; }

        }
    }
}
