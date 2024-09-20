using GenricEcommers.Models;
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
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/reward")]
    public class RewardController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
       
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //[Authorize]
        //[Route("")]
        //[AcceptVerbs("Get")]
        //public HttpResponseMessage Get()
        //{
        //    logger.Info("start WalletList: ");
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        int Warehouse_id = 0;

        //        foreach (Claim claim in identity.Claims)
        //        {
        //            if (claim.Type == "compid")
        //            {
        //                compid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "userid")
        //            {
        //                userid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "Warehouseid")
        //            {
        //                Warehouse_id = int.Parse(claim.Value);
        //            }
        //        }

        //        //if (Warehouse_id > 0)
        //        //{
        //        using (var context = new AuthContext())
        //        {
        //            var pointList = (from i in context.RewardPointDb
        //                             where i.Deleted == false
        //                             join k in context.Customers on i.CustomerId equals k.CustomerId
        //                             //where k.Warehouseid == Warehouse_id
        //                             join j in context.Customers on k.CustomerId equals j.CustomerId into ts
        //                             from j in ts.DefaultIfEmpty()
        //                             select new
        //                             {
        //                                 Id = i.Id,
        //                                 CustomerId = i.CustomerId,
        //                                 TotalPoint = i.TotalPoint,
        //                                 EarningPoint = i.EarningPoint,
        //                                 UsedPoint = i.UsedPoint,
        //                                 MilestonePoint = i.MilestonePoint,
        //                                 CreatedDate = i.CreatedDate,
        //                                 TransactionDate = i.TransactionDate,
        //                                 UpdatedDate = i.UpdatedDate,
        //                                 Skcode = j.Skcode,
        //                                 ShopName = j.ShopName
        //                             }).ToList();
        //            logger.Info("End  wallet: ");
        //            return Request.CreateResponse(HttpStatusCode.OK, pointList);
        //            //}
        //            // else {
        //            //var pointLists = (from i in context.RewardPointDb
        //            //                     where i.Deleted == false
        //            //                     let k = context.CustSupplierDb.Where(p2 => i.CustomerId == i.CustomerId).FirstOrDefault()
        //            //                     //join k in context.CustSupplierDb on i.CustomerId equals k.CustomerId where k.CompanyId == compid
        //            //                     join j in context.Customers on k.CustomerId equals j.CustomerId into ts
        //            //                     from j in ts.DefaultIfEmpty()
        //            //                     select new
        //            //                     {
        //            //                         Id = i.Id,
        //            //                         CustomerId = i.CustomerId,
        //            //                         TotalPoint = i.TotalPoint,
        //            //                         EarningPoint = i.EarningPoint,
        //            //                         UsedPoint = i.UsedPoint,
        //            //                         MilestonePoint = i.MilestonePoint,
        //            //                         CreatedDate = i.CreatedDate,
        //            //                         TransactionDate = i.TransactionDate,
        //            //                         UpdatedDate = i.UpdatedDate,
        //            //                         Skcode = j.Skcode,
        //            //                         ShopName = j.ShopName
        //            //                     }).ToList();
        //            //    logger.Info("End  wallet: ");
        //            //    return Request.CreateResponse(HttpStatusCode.OK, pointList);
        //            //}
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in WalletList " + ex.Message);
        //        logger.Info("End  WalletList: ");
        //        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        //    }
        //}

        [Route("GetRewardPointList")]
        [HttpPost]
        public TotalRecordRewardList GetRewardPointList(GetRewardDc getRewardDc )
        {
           // List<RewardPointDc> List = new List<RewardPointDc>();
            using (var db = new AuthContext())
            {
                TotalRecordRewardList List = new TotalRecordRewardList();

                if (db.Database.Connection.State != ConnectionState.Open)
                    db.Database.Connection.Open();

                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[dbo].[GetRewardPointList]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@skip", getRewardDc.Skip));
                cmd.Parameters.Add(new SqlParameter("@take", getRewardDc.Take));
                cmd.Parameters.Add(new SqlParameter("@Keyword", getRewardDc.Keyword));

                var reader = cmd.ExecuteReader();
                var data = ((IObjectContextAdapter)db).ObjectContext.Translate<RewardPointDc>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    List.TotalCount = Convert.ToInt32(reader["TotalCount"]);
                }
                List.rewardPointDcs = data;
                return List;
            }

        }

        //[Route("")]
        //[AcceptVerbs("Get")]
        //public HttpResponseMessage Get(int CustomerId)
        //{
        //    logger.Info("start single  GetcusomerWallets: ");
        //    RewardPoint Item = new RewardPoint();
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;

        //        foreach (Claim claim in identity.Claims)
        //        {
        //            if (claim.Type == "compid")
        //            {
        //                compid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "userid")
        //            {
        //                userid = int.Parse(claim.Value);
        //            }
        //        }

        //        var point =  context.GetRewardbyCustomerid(CustomerId,compid);
        //        return Request.CreateResponse(HttpStatusCode.OK, point);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in Get single GetcusomerWallets " + ex.Message);
        //        logger.Info("End  single GetcusomerWallets: ");
        //        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
        //    }
        //}
        [Route("")]
        [AcceptVerbs("Post")]
        public HttpResponseMessage post(RewardPoint point)
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

                //point.CompanyId = compid;
                using (var context = new AuthContext())
                {
                    if (point.CustomerId > 0)
                    {
                        var rpoint = context.RewardPointDb.Where(c => c.CustomerId == point.CustomerId).SingleOrDefault();
                        if (rpoint != null)
                        {
                            rpoint.CustomerId = rpoint.CustomerId;
                            if (rpoint.EarningPoint > 0)
                            {
                                rpoint.EarningPoint += rpoint.EarningPoint;
                                rpoint.UpdatedDate = indianTime;
                            }
                            if (rpoint.UsedPoint > 0)
                            {
                                rpoint.TotalPoint -= point.UsedPoint;
                                rpoint.UsedPoint += point.UsedPoint;
                                rpoint.TransactionDate = indianTime;
                            }
                            context.RewardPointDb.Attach(rpoint);
                            context.Entry(rpoint).State = EntityState.Modified;
                            context.Commit();
                        }
                        else
                        {
                            if (point.EarningPoint > 0) { }
                            else
                                point.EarningPoint = 0;
                            point.TotalPoint = 0;
                            point.UsedPoint = 0;
                            point.MilestonePoint = 0;
                            point.CreatedDate = indianTime;
                            point.UpdatedDate = indianTime;
                            point.Deleted = false;
                            context.RewardPointDb.Add(point);
                            context.Commit();
                            rpoint = point;
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, rpoint);
                    }
                    else
                        return Request.CreateResponse(HttpStatusCode.OK, "CustomerID null");
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Get single GetcusomerWallets " + ex.Message);
                logger.Info("End  single GetcusomerWallets: ");
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
            }
        }
    }
}
