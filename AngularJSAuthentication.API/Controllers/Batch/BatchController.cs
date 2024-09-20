using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.DataContracts.Masters.Batch;
using AngularJSAuthentication.Model.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;
using System.Security.Claims;
using System.Data.SqlClient;


namespace AngularJSAuthentication.API.Controllers.Batch
{
    [RoutePrefix("api/Batch")]
    public class BatchController : BaseAuthController
    {
        [Route("GetBatchListByNumber")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<BatchDc>> GetBatchListByNumber(string ItemNumber)
        {
            List<BatchDc> result = new List<BatchDc>();
            if (ItemNumber != null && ItemNumber.Length > 1)
            {
                using (AuthContext db = new AuthContext())
                {
                    result = db.BatchMasters.Where(x => x.ItemNumber == ItemNumber && x.IsActive==true && x.IsDeleted == false).Select(x => new BatchDc
                    {
                        BatchCode = x.BatchCode,
                        ItemNumber = x.ItemNumber,
                        BatchMasterId = x.Id,
                        MFGDate = x.MFGDate,
                        ExpiryDate = x.ExpiryDate,
                    }).ToList();
                }
            }
            return result;
        }

        [Route("GetBatchDetailsByNumberOrCode")]
        [HttpGet]
        [AllowAnonymous]
        public BatchDC GetBatchByNumberAndCode (string ItemNumber,int skip,int take)
        {
            using (AuthContext db = new AuthContext())
            {
                BatchDC res =new BatchDC();
                //var param = new SqlParameter("@ItemNumber", ItemNumber);
               // var param1 = new SqlParameter("@skip", skip);
                //var param2 = new SqlParameter("@take", take);
                res.BatchMaster = db.BatchMasters.Where(x=>(x.ItemNumber.Contains(ItemNumber)|| x.BatchCode.Contains(ItemNumber))&& x.IsActive==true && x.IsDeleted==false).OrderBy(y=>y.Id).Skip(skip).Take(take).ToList();
                //res.BatchMaster= db.Database.SqlQuery<BatchMaster>("exec SpGetBatchDetailByCodeOrNumber @ItemNumber,@skip,@take", param, param1, param2).ToList();
                //res = db.BatchMasters.Where(x => (x.ItemNumber == Keyword || x.BatchCode == Keyword) && x.IsActive == true && x.IsDeleted == false).ToList();
                res.TotalRecords = db.BatchMasters.Where(x=>(x.ItemNumber.Contains(ItemNumber) || x.BatchCode.Contains(ItemNumber)) && x.IsActive==true && x.IsDeleted == false).Count();
                return res;
            }
        }

        [Route("GetBatchApproverDetails")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetBatchApproverDetailsByStatus(string Status, string ItemNumber, int skip, int take)
        {
            using (var db = new AuthContext())
            {
                BatchMasterTempDC res = new BatchMasterTempDC();
                if (ItemNumber == null || ItemNumber.Length == 0 || ItemNumber == "")
                {
                    res.BatchMasterTemp = db.BatchMasterTemps.Where(x => x.Status == Status).OrderBy(y => y.Id).Skip(skip).Take(take).ToList();
                    res.TotalRecords = db.BatchMasterTemps.Where(x => x.Status == Status).Count();
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    res.BatchMasterTemp = db.BatchMasterTemps.Where(x => x.Status == Status && (x.ItemNumber.Contains(ItemNumber) || x.BatchCode.Contains(ItemNumber))).OrderBy(y => y.Id).Skip(skip).Take(take).ToList();
                    res.TotalRecords = db.BatchMasterTemps.Where(x => x.Status == Status && (x.ItemNumber.Contains(ItemNumber) || x.BatchCode.Contains(ItemNumber))).Count();
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }
        }


        [Route("EditBatchDetails")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage EditBatchDetails(int Id, DateTime ExpiryDate)
        {
            using (AuthContext db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                BatchMaster batchmaster = new BatchMaster();
                BatchMasterTemp batchmastertemp = new BatchMasterTemp();
                batchmaster = db.BatchMasters.Where(x => x.Id == Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                batchmastertemp = db.BatchMasterTemps.Where(x => x.BatchMasterDetailId == Id).FirstOrDefault();
                if (batchmaster != null)
                {
                    if (batchmastertemp != null)
                    {
                        batchmastertemp.NewExpiryDate = ExpiryDate;
                        batchmastertemp.Status = "Pending";
                        batchmastertemp.ModifiedBy = userid;
                        batchmastertemp.ModifiedDate = DateTime.Now;
                        db.Entry(batchmastertemp).State = EntityState.Modified;
                        db.Commit();
                        return Request.CreateResponse(HttpStatusCode.OK, "Expiry Date Requested");
                    }
                    else
                    {

                        BatchMasterTemp res = new BatchMasterTemp();
                        res.BatchMasterDetailId = Id;
                        res.ItemNumber = batchmaster.ItemNumber;
                        res.BatchCode = batchmaster.BatchCode;
                        res.OldExpiryDate = batchmaster.ExpiryDate;
                        res.NewExpiryDate = ExpiryDate;
                        res.CreatedDate = DateTime.Now;
                        res.CreatedBy = userid;
                        res.Status = "Pending";
                        db.BatchMasterTemps.Add(res);
                        db.Commit();
                        return Request.CreateResponse(HttpStatusCode.OK, "Expiry Date Requested");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Data Not Exists");
                }


            }
        }

        [Route("ApprovedRejectedBatchDetails")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage ApprovedRejectedBatchDetails(int Id, string Status)
        {
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                BatchMaster batchMaster = new BatchMaster();
                BatchMasterTemp batchMasterTemp = new BatchMasterTemp();
                batchMaster = db.BatchMasters.Where(x => x.Id == Id).FirstOrDefault();
                batchMasterTemp = db.BatchMasterTemps.Where(x => x.BatchMasterDetailId == Id).FirstOrDefault();
                if (Status == "Approved")
                {
                    batchMaster.ExpiryDate = batchMasterTemp.NewExpiryDate;
                    batchMaster.ModifiedBy = userid;
                    batchMaster.ModifiedDate = DateTime.Now;
                    batchMasterTemp.Status = Status;
                    batchMasterTemp.ModifiedBy = userid;
                    batchMasterTemp.ModifiedDate = DateTime.Now;
                    db.Entry(batchMaster).State = EntityState.Modified;
                    db.Entry(batchMasterTemp).State = EntityState.Modified;
                    db.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK, "Approved Successfully");
                }
                else
                {
                    batchMasterTemp.Status = Status;
                    batchMasterTemp.ModifiedBy= userid;
                    batchMasterTemp.ModifiedDate = DateTime.Now;
                    db.Entry(batchMasterTemp).State = EntityState.Modified;
                    db.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK, Status + " Successfully");
                }
            }
        }



    }
    public class BatchDC
    {
        public List<BatchMaster> BatchMaster { get; set; }
        public int TotalRecords { get; set; }


    }
    public class BatchMasterTempDC
    {
        public List<BatchMasterTemp> BatchMasterTemp { get; set; }
        public int TotalRecords { get; set; }


    }
}
