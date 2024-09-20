using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model.Seller;
using LinqKit;

namespace AngularJSAuthentication.API.ControllerV7.AppBanner
{
    [RoutePrefix("api/BrandStoreReq")]
    public class BrandStoreRequestController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("PostBrandStoreReq")]
        [HttpPost]
        public ResBrandStoreRequest AddBrandStoreReq(BrandStoreRequest obj)
        {
            ResBrandStoreRequest res = new ResBrandStoreRequest();
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (obj != null && obj.Id > 0 && userid > 0)
                {
                    var BrandStoreData = context.BrandStoreRequests.FirstOrDefault(x => x.Id == obj.Id && x.IsDeleted == false);
                    if (BrandStoreData != null && !BrandStoreData.IsApproved && BrandStoreData.Status != "Accept")
                    {
                        BrandStoreData.Comment = obj.Comment;
                        BrandStoreData.ModifiedBy = userid;
                        BrandStoreData.ModifiedDate = DateTime.Now;
                        BrandStoreData.ImageUrl = obj.ImageUrl;
                        BrandStoreData.StartDate = obj.StartDate;
                        BrandStoreData.EndDate = obj.EndDate;
                        if (BrandStoreData.Status == "Reject")
                        {
                            BrandStoreData.Status = "Pending";
                        }
                        else
                        {
                            BrandStoreData.Status = obj.Status;
                        }
                        context.Entry(BrandStoreData).State = EntityState.Modified;
                        context.Commit();
                        res.Result = true;

                        res.msg = "Successfully Saved";
                    }
                    else 
                    {
                        res.msg = "request cannot be changed, due to " + BrandStoreData.Status;

                    }

                }
                else if (obj != null && obj.WarehouseIds.Any())
                {
                    List<BrandStoreRequest> addBrandStoreRequests = new List<BrandStoreRequest>();

                    foreach (var wid in obj.WarehouseIds)
                    {
                        var BrandStoreData = new BrandStoreRequest
                        {
                            ImageUrl = obj.ImageUrl,
                            StartDate = obj.StartDate,
                            EndDate = obj.EndDate,
                            SubCatId = obj.SubCatId,
                            Comment = obj.Comment,
                            ApprovedDate = obj.ApprovedDate,
                            IsApproved = obj.IsApproved,
                            ApprovedBy = userid,
                            IsActive = false,
                            IsDeleted = false,
                            CreatedBy = userid,
                            CreatedDate = DateTime.Now,
                            WarehouseId = wid,
                            Status = "Pending"
                        };
                        addBrandStoreRequests.Add(BrandStoreData);
                    }
                    context.BrandStoreRequests.AddRange(addBrandStoreRequests);
                    context.Commit();
                    res.Result = true;

                    res.msg = "Successfully Added";
                }
                else
                {
                    res.msg = "Failed";
                    res.Result = false;
                }
            }
            return res;

        }

        [Route("GetBrandStoreRequestList")]
        [HttpPost]
        public ResBrandStoreRequest BrandStoreRequestList(BrandStoreFilterDc filter)
        {
            int Skiplist = (filter.skip - 1) * filter.take;
            ResBrandStoreRequest res = new ResBrandStoreRequest();

            List<BrandStoreRequest> BrandStoreReqList = new List<BrandStoreRequest>();
            List<BrandStoreRequestDc> List = new List<BrandStoreRequestDc>();
            using (var context = new AuthContext())
            {
                //var predicate = PredicateBuilder.True<BrandStoreRequest>();

                //predicate = predicate.And(x => x.IsDeleted == false);

                //if (filter.WarehouseIds.Count > 0)
                //{
                //    predicate.And(x => filter.WarehouseIds.Contains(x.WarehouseId));
                //}
                //if (filter.startDate != null && filter.endDate != null)
                //{
                //    predicate = predicate.And(x => x.CreatedDate >= filter.startDate && x.CreatedDate <= filter.endDate);
                //}
                var warehouseresult = string.Join(",", filter.WarehouseIds);
                string whereclause = "";

                if (filter.WarehouseIds.Count > 0)
                {
                    whereclause += " and b.WarehouseId in (" + warehouseresult + ")";
                }
                if (filter.startDate.HasValue && filter.endDate.HasValue)
                {
                    whereclause += " and (b.CreatedDate >= " + "'" + filter.startDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "'" + " And  b.CreatedDate <=" + "'" + filter.endDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "')";
                }
                if (filter.statusValue != null && filter.statusValue != "All")
                {
                    whereclause += " and b.status=" + "'" + filter.statusValue + "'";
                }
                string sqlquery = "select b.Id,b.ImageUrl,b.CategoryId,b.StartDate,b.EndDate,b.WarehouseId,b.Comment,b.ApprovedDate,b.IsApproved,b.ApprovedBy,b.CreatedDate,b.ModifiedDate,b.IsActive,b.IsDeleted,b.CreatedBy,b.ModifiedBy,b.SubCatId,b.Status,b.ObjectId,w.WarehouseName as Warehouse,s.SubcategoryName as SubCatName,c.CategoryName from BrandStoreRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join Categories as c on c.Categoryid=b.CategoryId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause + " Order by b.Id desc offset " + Skiplist + " rows fetch next " + filter.take + " rows only";
                string sqlcount = "select count(*) from BrandStoreRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join Categories as c on c.Categoryid=b.CategoryId join SubCategories as s on s.SubCategoryId = b.subcatId where b.IsDeleted=0 " + whereclause;

                int totalcount = context.Database.SqlQuery<int>(sqlcount).FirstOrDefault();
                List<BrandStoreRequestDc> newdata = context.Database.SqlQuery<BrandStoreRequestDc>(sqlquery).ToList();
                List = Mapper.Map(newdata).ToANew<List<BrandStoreRequestDc>>();

                res.totalcount = totalcount;
                res.BrandStoreRequestDcs = List;
                res.msg = "success";
                res.Result = true;
            }
            return res;
        }

        [Route("ExportBrandStoreRequest")]
        [HttpPost]
        public ResBrandStoreRequest ExportBrandStoreRequest(BrandStoreFilterDc filter)
        {

            ResBrandStoreRequest res = new ResBrandStoreRequest();

            List<BrandStoreRequest> BrandStoreReqList = new List<BrandStoreRequest>();
            List<BrandStoreRequestDc> List = new List<BrandStoreRequestDc>();
            using (var context = new AuthContext())
            {

                var warehouseresult = string.Join(",", filter.WarehouseIds);
                string whereclause = "";

                if (filter.WarehouseIds.Count > 0)
                {
                    whereclause += " and b.WarehouseId in (" + warehouseresult + ")";
                }
                if (filter.startDate.HasValue && filter.endDate.HasValue)
                {
                    whereclause += " and (b.CreatedDate >= " + "'" + filter.startDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "'" + " And  b.CreatedDate <=" + "'" + filter.endDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "')";
                }
                string sqlquery = "select p.DisplayName as CreatedbyName,Isnull(ps.DisplayName,'') as ModifiedByName, b.Id,b.ImageUrl,b.CategoryId,b.StartDate,b.EndDate,b.WarehouseId,b.Comment,b.ApprovedDate,b.IsApproved,b.ApprovedBy,b.CreatedDate,b.ModifiedDate,b.IsActive,b.IsDeleted,b.CreatedBy,b.ModifiedBy,b.SubCatId,b.Status,b.ObjectId,w.WarehouseName as Warehouse,s.SubcategoryName as SubCatName,c.CategoryName from BrandStoreRequests as b Join Warehouses as w on w.WarehouseId = b.WarehouseId join Categories as c on c.Categoryid=b.CategoryId join SubCategories as s on s.SubCategoryId = b.subcatId join People as p on p.PeopleID=b.CreatedBy left join People as ps on ps.PeopleID=b.ModifiedBy where b.IsDeleted=0 " + whereclause;

                List<BrandStoreRequestDc> newdata = context.Database.SqlQuery<BrandStoreRequestDc>(sqlquery).ToList();
                List = Mapper.Map(newdata).ToANew<List<BrandStoreRequestDc>>();

                res.totalcount = 0;
                res.BrandStoreRequestDcs = List;
                res.msg = "success";
                res.Result = true;
            }
            return res;
        }
        [Route("GetCategory")]
        [HttpGet]
        public List<CategoryDc> GetCategory(int SubCatId)
        {
            using (var context = new AuthContext())
            {
                List<CategoryDc> list = new List<CategoryDc>();
                list = context.SubCategorys.Where(y => y.SubCategoryId == SubCatId && y.Deleted == false).Select(x => new CategoryDc
                {
                    CategoryName = x.CategoryName,
                    Categoryid = x.Categoryid
                }).Distinct().ToList();

                return list;
            }
        }



        [Route("ApproveReq")]
        [HttpPost]
        public ResBrandStoreRequest ApproveReq(BrandStoreRequest obj)
        {
            ResBrandStoreRequest res = new ResBrandStoreRequest();
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                var BrandStoreData = context.BrandStoreRequests.FirstOrDefault(x => x.Id == obj.Id && x.IsDeleted == false);

                if (obj != null && obj.Id > 0 && userid > 0 && BrandStoreData != null && !BrandStoreData.IsApproved && obj.IsApproved == true && BrandStoreData.Status == "Accept")
                {


                    SellerRequestHelper helper = new SellerRequestHelper();
                    int objectid = helper.ApproveBrandStoreReq(BrandStoreData);
                    if (objectid > 0)
                    {
                        BrandStoreData.Status = "Approved";
                        BrandStoreData.ApprovedDate = DateTime.Now;
                        BrandStoreData.IsApproved = true;
                        BrandStoreData.ApprovedBy = userid;
                        BrandStoreData.ModifiedBy = userid;
                        BrandStoreData.ModifiedDate = DateTime.Now;
                        BrandStoreData.ObjectId = objectid;
                        context.Entry(BrandStoreData).State = EntityState.Modified;
                        context.Commit();
                        res.msg = "Request Approved Successfully";
                        res.Result = true;
                        return res;
                    }
                    res.msg = "Request not Approved, Something went wrong";
                    res.Result = false;
                }
                else if (BrandStoreData.IsApproved)
                {
                    res.msg = "Request already Approved";
                    res.Result = true;
                }
                else
                {
                    res.msg = "Failed";
                    res.Result = false;
                }
            }
            return res;

        }



        [Route("AcceptReq")]
        [HttpPost]
        public ResBrandStoreRequest AcceptReq(BrandStoreRequest obj)
        {
            ResBrandStoreRequest res = new ResBrandStoreRequest();
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                var BrandStoreData = context.BrandStoreRequests.FirstOrDefault(x => x.Id == obj.Id && x.IsDeleted == false);

                if (BrandStoreData != null && !BrandStoreData.IsApproved && BrandStoreData.Status != "Cancel")
                {
                    if (obj.Status == "Accept")
                    {
                        BrandStoreData.IsActive = true;
                    }
                    BrandStoreData.Status = obj.Status;
                    BrandStoreData.Comment = obj.Comment;

                    BrandStoreData.ModifiedBy = userid;
                    BrandStoreData.ModifiedDate = DateTime.Now;
                    context.Entry(BrandStoreData).State = EntityState.Modified;
                    context.Commit();
                    res.msg = "Request " + obj.Status + " Successfully";
                    res.Result = true;
                    return res;
                }
                else if (BrandStoreData != null)
                {
                    res.msg = "Request already " + BrandStoreData.Status;
                    res.Result = true;
                }
                else
                {
                    res.msg = "Failed";
                    res.Result = false;
                }
            }
            return res;

        }
    }
}

public class CategoryDc
{
    public int Categoryid { get; set; }
    public string CategoryName { get; set; }
}

public class BrandStoreFilterDc
{
    public int skip { get; set; }
    public int take { get; set; }
    public List<int> WarehouseIds { get; set; }
    public DateTime? startDate { get; set; }
    public DateTime? endDate { get; set; }
    public string statusValue { get; set; }
}