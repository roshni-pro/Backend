using AgileObjects.AgileMapper;
using AngularJSAuthentication.Model.Rating;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using static AngularJSAuthentication.DataContracts.RatingMaster.RatingMasterDc;

namespace AngularJSAuthentication.API.Controllers.Rating
{
    [AllowAnonymous]
    [RoutePrefix("api/RatingMaster")]
    public class RatingMasterController : ApiController
    {
        [Route("AddRatingMaster")]
        [HttpPost]
        public ResRatingMasterDC AddRatingMaster(RatingMasterDC RatingMasterDc)
        {
            int userid = 0;
            ResRatingMasterDC res = new ResRatingMasterDC();
            using (var db = new AuthContext())
            {
                userid = GetUserId();
                var ratingMasters = db.RatingMasters.Where(x => x.AppType == RatingMasterDc.AppType && x.Rating == RatingMasterDc.Rating && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                if (ratingMasters == null)
                {
                    if (RatingMasterDc != null && RatingMasterDc.RatingDetails.Any())
                    {
                        List<RatingDetail> Addlist = new List<RatingDetail>();
                        RatingMaster add = new RatingMaster();
                        add.AppType = RatingMasterDc.AppType;
                        add.Rating = RatingMasterDc.Rating;
                        add.CreatedBy = userid;
                        add.CreatedDate = DateTime.Now;
                        add.IsActive = true;
                        add.IsDeleted = false;
                        add.RatingDetails = new List<RatingDetail>();
                        foreach (var list in RatingMasterDc.RatingDetails)
                        {
                            RatingDetail obj = new RatingDetail()
                            {
                                Detail = list.Detail,
                                CreatedBy = userid,
                                CreatedDate = DateTime.Now,
                                IsActive = true,
                                IsDeleted = false
                            };
                            add.RatingDetails.Add(obj);
                        }
                        db.RatingMasters.Add(add);
                        db.Commit();
                        res.msg = "Rating Master Add Successfully";
                        res.Result = true;
                    }
                    else
                    {
                        res.msg = "someting went worng!!";
                        res.Result = false;
                    }
                }
                else
                {
                    res.msg = "Already Added";
                    res.Result = false;
                }
            }
            return res;
        }
        [Route("UpdateRatingMaster")]
        [HttpPost]
        public ResRatingMasterDC UpdateRatingMaster(RatingMasterDC RatingMasterDc)
        {
            int userid = 0;
            ResRatingMasterDC res = new ResRatingMasterDC();
            List<RatingDetail> NewratingDetails = new List<RatingDetail>();
            using (var context = new AuthContext())
            {
                userid = GetUserId();
                if (RatingMasterDc != null && RatingMasterDc.RatingDetails.Any())
                {
                    var EditRatingMasters = context.RatingMasters.Where(x => x.Id == RatingMasterDc.Id && x.IsDeleted == false).Include(x => x.RatingDetails).FirstOrDefault();
                    if (EditRatingMasters != null)
                    {
                        EditRatingMasters.AppType = RatingMasterDc.AppType;
                        EditRatingMasters.Rating = RatingMasterDc.Rating;
                        EditRatingMasters.IsDeleted = false;
                        EditRatingMasters.ModifiedDate = DateTime.Now;
                        EditRatingMasters.ModifiedBy = userid;
                        List<long> newid = RatingMasterDc.RatingDetails.Select(x => x.Id).Distinct().ToList();
                        foreach (var list in RatingMasterDc.RatingDetails)
                        {
                            var editdetails = EditRatingMasters.RatingDetails.Where(x => x.Id == list.Id).FirstOrDefault();
                            if (editdetails == null)
                            {
                                RatingDetail objs = new RatingDetail()
                                {
                                    Detail = list.Detail,
                                    RatingMasterId = list.RatingMasterId,
                                    IsActive = true,
                                    IsDeleted = false,
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = userid
                                };
                                NewratingDetails.Add(objs);
                            }
                            else
                            {
                                editdetails.Detail = list.Detail;
                                editdetails.RatingMasterId = list.RatingMasterId;
                                editdetails.IsActive = true;
                                editdetails.IsDeleted = false;
                                editdetails.ModifiedDate = DateTime.Now;
                                editdetails.ModifiedBy = userid;
                                context.Entry(editdetails).State = EntityState.Modified;
                            }
                        }
                        foreach (var obj in EditRatingMasters.RatingDetails.Where(x => !newid.Contains(Convert.ToInt32(x.Id))).Select(z => z.Id).ToList())
                        {
                            var list = EditRatingMasters.RatingDetails.Where(x => x.Id == obj).FirstOrDefault();
                            list.ModifiedDate = DateTime.Now;
                            list.ModifiedBy = userid;
                            list.IsActive = false;
                            list.IsDeleted = true;
                            context.Entry(list).State = EntityState.Modified;
                        }
                        foreach (var item in NewratingDetails)
                        {
                            EditRatingMasters.RatingDetails.Add(item);
                        }
                        context.Entry(EditRatingMasters).State = EntityState.Modified;
                        if (context.Commit() > 0)
                        {
                            res.msg = "Rating Master Update Successfully!!";
                            res.Result = true;
                        }
                        else
                        {
                            res.msg = "Rating Master Not Updated!!";
                            res.Result = false;
                        }
                    }
                    else
                    {

                        res.msg = "someting went worng!!";
                        res.Result = false;
                    }
                }
                else
                {
                    res.msg = "Data not found!!";
                    res.Result = false;
                }
            }
            return res;
        }

        [Route("GetRatingMaster")]
        [HttpGet]
        public List<RatingMasterDC> GetRatingMaster(int AppType)
        {
            using (var db = new AuthContext())
            {
                List<RatingMasterDC> list = new List<RatingMasterDC>();
                var ratingMasters = db.RatingMasters.Where(x => x.AppType == AppType && x.IsActive == true && x.IsDeleted == false).OrderByDescending(x=>x.Rating).ToList();
                if (ratingMasters != null && ratingMasters.Any())
                {
                    list = Mapper.Map(ratingMasters).ToANew<List<RatingMasterDC>>();
                    list.ForEach(x =>
                    {
                        if (x.AppType == 1)
                        {
                            x.AppTypeName = "Sales Rating";
                        }
                        else if (x.AppType == 3)
                        {
                            x.AppTypeName = "Retailer Rating";
                        }
                        else if (x.AppType == 2)
                        {
                            x.AppTypeName = "Delivery Rating";
                        }
                    });
                }
                return list;
            }
        }
        [Route("GetRatingDetails")]
        [HttpGet]
        public RatingMasterDC GetRatingDetails(int Id)
        {
            using (var db = new AuthContext())
            {
                RatingMasterDC list = new RatingMasterDC();
                var ratingMasters = db.RatingMasters.Where(x => x.Id == Id).Include(x => x.RatingDetails).FirstOrDefault();
                ratingMasters.RatingDetails = ratingMasters.RatingDetails.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                if (ratingMasters != null)
                {
                    list = Mapper.Map(ratingMasters).ToANew<RatingMasterDC>();
                    if (list.AppType == 1)
                    {
                        list.AppTypeName = "Sales Rating";
                    }
                    else if (list.AppType == 3)
                    {
                        list.AppTypeName = "Retailer Rating";
                    }
                    else if (list.AppType == 2)
                    {
                        list.AppTypeName = "Delivery Rating";
                    }

                }
                return list;
            }
        }


        private int GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }
    }
}
