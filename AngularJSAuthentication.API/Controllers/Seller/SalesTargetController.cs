using AngularJSAuthentication.Model.Seller;
using LinqKit;
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

namespace AngularJSAuthentication.API.Controllers.Seller
{
    [RoutePrefix("api/SalesTarget")]
    public class SalesTargetController : ApiController
    {
        [Route("AddUpdateBaseQty")]
        [HttpPost]
        public ResSalesTargetDc AddUpdateBaseQty(SalesTargetPostDc obj)
        {
            ResSalesTargetDc res = new ResSalesTargetDc();
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (obj != null && userid > 0 && obj.ItemList[0].StoreId > 0)
                {
                    foreach (var item in obj.ItemList)
                    {
                        var SalesTarget = context.SalesTargets.FirstOrDefault(x => x.ItemMultiMrpId == item.ItemMultiMrpId && x.StoreId == item.StoreId && x.IsDeleted == false);
                        if (SalesTarget != null)
                        {
                            SalesTarget.IsDeleted = item.IsDeleted;
                            SalesTarget.BaseQty = item.BaseQty;
                            SalesTarget.ModifiedDate = DateTime.Now;
                            SalesTarget.ModifiedBy = userid;
                            context.Entry(SalesTarget).State = EntityState.Modified;
                            context.Commit();
                        }
                        else
                        {
                            var AddSalesTargetdata = new SalesTarget
                            {
                                ItemNumber = item.ItemNumber,
                                ItemMultiMrpId = item.ItemMultiMrpId,
                                StoreId = item.StoreId,
                                BaseQty = item.BaseQty,
                                IsActive = true,
                                IsDeleted = false,
                                CreatedBy = userid,
                                CreatedDate = DateTime.Now,
                                ModifiedDate = DateTime.Now,
                                ModifiedBy = userid,
                            };
                            context.SalesTargets.Add(AddSalesTargetdata);
                            //context.Commit();
                        }
                    }
                    context.Commit();
                    res.Result = true;
                    res.msg = "Successfully Added";
                }
                else
                {
                    res.Result = false;
                    res.msg = "Failed";
                }
            }
            return res;
        }

        [Route("GetSalesTargetList")]
        [HttpPost]
        public ResSalesTargetDc SalesTargetList(SalesTargetFilterDc filter)
        {
            int SubCatId = Convert.ToInt32(Request.Headers.GetValues("SubCatId").First());

            filter.skip = (filter.skip - 1) * filter.take;
            ResSalesTargetDc res = new ResSalesTargetDc();
            List<SalesTargetDc> result = new List<SalesTargetDc>();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[Seller].[GetSalesTargetItem]";
                cmd.Parameters.Add(new SqlParameter("@SubCategoryId", SubCatId));
                cmd.Parameters.Add(new SqlParameter("@StoreId", filter.StoreId));
                cmd.Parameters.Add(new SqlParameter("@Skip", filter.skip));
                cmd.Parameters.Add(new SqlParameter("@Take", filter.take));
                cmd.Parameters.Add(new SqlParameter("@Searchkeyword", filter.Searchkeyword));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                var reader = cmd.ExecuteReader();
                result = ((IObjectContextAdapter)context).ObjectContext.Translate<SalesTargetDc>(reader).ToList();
                res.res = result;
                if (result != null && result.Any())
                {
                    res.totalcount = result.FirstOrDefault().totalRecord;
                }

            }
            return res;

        }

        // Store List
        [Route("GetStoreList")]
        [HttpGet]
        public List<StoreSalesTargetDc> GetStoreList()
        {
            List<StoreSalesTargetDc> result = new List<StoreSalesTargetDc>();
            using (var context = new AuthContext())
            {
                result = context.Database.SqlQuery<StoreSalesTargetDc>("exec Seller.GetStoreList").ToList();
            }
            return result;
        }

        //Mrp item list
        [Route("GetStoreMrpItemList")]
        [HttpGet]
        public List<MrpSalesTargetDc> GetStoreMrpItemList(long SubCatId)
        {
            int SubCatId2 = Convert.ToInt32(Request.Headers.GetValues("SubCatId").First());

            List<MrpSalesTargetDc> result = new List<MrpSalesTargetDc>();
            using (var context = new AuthContext())
            {
                List<SqlParameter> paramList = new List<SqlParameter>();
                paramList.Add(new SqlParameter("@SubCatId", SubCatId2));
                result = context.Database.SqlQuery<MrpSalesTargetDc>("exec Seller.GetStoreMrpItemList @SubCatId", paramList.ToArray()).ToList();
            }
            return result;
        }

        [Route("SalesTargetExport")]
        [HttpPost]
        public ResSalesTargetDc SalesTargetExport(SalesTargetFilterDc filter)
        {
            int SubCatId = Convert.ToInt32(Request.Headers.GetValues("SubCatId").First());
            ResSalesTargetDc res = new ResSalesTargetDc();
            List<SalesTargetExportDc> result = new List<SalesTargetExportDc>();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[Seller].[SalesTargetItemExport]";
                cmd.Parameters.Add(new SqlParameter("@SubCategoryId", SubCatId));
                cmd.Parameters.Add(new SqlParameter("@StoreId", filter.StoreId));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                var reader = cmd.ExecuteReader();
                result = ((IObjectContextAdapter)context).ObjectContext.Translate<SalesTargetExportDc>(reader).ToList();
                res.res = result;
            }
            return res;

        }

        [Route("EditBaseQty")]
        [HttpPost]
        public IHttpActionResult EditBaseQty(SalesTargetDc obj)
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var SalesTarget = context.SalesTargets.FirstOrDefault(x => x.Id == obj.Id && x.ItemMultiMrpId == obj.ItemMultiMrpId && x.StoreId == obj.StoreId && x.IsDeleted == false);
                if (SalesTarget != null)
                {
                    SalesTarget.IsDeleted = obj.IsDeleted;
                    SalesTarget.BaseQty = obj.BaseQty;
                    SalesTarget.ModifiedDate = DateTime.Now;
                    SalesTarget.ModifiedBy = userid;
                    context.Entry(SalesTarget).State = EntityState.Modified;
                    context.Commit();
                }
            }
            return Ok();
        }
        [Route("DeleteBaseQty")]
        [HttpPost]
        public IHttpActionResult DeleteBaseQty(SalesTargetDc obj)
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var SalesTarget = context.SalesTargets.FirstOrDefault(x => x.Id == obj.Id && x.ItemMultiMrpId == obj.ItemMultiMrpId && x.StoreId == obj.StoreId && x.IsDeleted == false);
                if (SalesTarget != null)
                {
                    SalesTarget.IsDeleted = true;
                    SalesTarget.BaseQty = obj.BaseQty;
                    SalesTarget.ModifiedDate = DateTime.Now;
                    SalesTarget.ModifiedBy = userid;
                    context.Entry(SalesTarget).State = EntityState.Modified;
                    context.Commit();
                }
            }
            return Ok();
        }
    }


    public class MrpSalesTargetDc
    {
        public string itemBaseName { get; set; }
        public string ItemNumber { get; set; }
        public int ItemMultiMrpId { get; set; }
        public double MRP { get; set; }
    }

    public class StoreSalesTargetDc
    {
        public long StoreId { get; set; }
        public string StoreName { get; set; }
    }

    public class ResSalesTargetDc
    {
        public int totalcount { get; set; }
        public dynamic res { get; set; }
        public bool Result { get; set; }
        public string msg { get; set; }
    }
    public class SalesTargetDc
    {
        public long Id { get; set; }
        public string itemBaseName { get; set; }
        public string ItemNumber { get; set; }
        public int ItemMultiMrpId { get; set; }
        public long StoreId { get; set; }
        public int BaseQty { get; set; }
        public bool IsDeleted { get; set; }
        public double MRP { get; set; }
        public int totalRecord { get; set; }

    }
    public class SalesTargetFilterDc
    {
        public int skip { get; set; }
        public int take { get; set; }
        public long StoreId { get; set; }
        public int SubCateId { get; set; }
        public string Searchkeyword { get; set; }

    }
    public class SalesTargetExportDc
    {
        // public long Id { get; set; }
       // public string ItemName { get; set; }
        public string itemBaseName { get; set; }
        public string storeName { get; set; }
        public string ItemNumber { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int BaseQty { get; set; }
        public double MRP { get; set; }
    }
    public class SalesTargetPostDc
    {
        public List<SalesTargetDc> ItemList { get; set; }
    }
}
