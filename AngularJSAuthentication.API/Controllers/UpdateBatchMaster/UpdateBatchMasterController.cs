using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.UpdateBatchMaster
{
    [RoutePrefix("api/BatchMaster")]
    public class UpdateBatchMasterController : ApiController
    {
        [Route("SearchBatchMasterData")]
        [HttpGet]
        public List<ResponseBatchMasterData> SearchBatchMaster(string ItemNumber, string BatchCodeDate)
        {
            using (AuthContext context = new AuthContext())
            {
                List<ResponseBatchMasterData> RBMD = new List<ResponseBatchMasterData>();

                var itemNumber = new SqlParameter("@ItemNumber", ItemNumber);
                var batchCodeDate = new SqlParameter("@BatchCodeDate", BatchCodeDate);

                RBMD = context.Database.SqlQuery<ResponseBatchMasterData>("EXEC GetDataBatchMaster @ItemNumber,@BatchCodeDate", itemNumber, batchCodeDate).ToList();

                return RBMD;

            }
        }

        public class ResponseBatchMasterData
        {
            public int Id { get; set; }
            public string ItemNumber { get; set; }
            public string BatchCode { get; set; }

            public DateTime MFGDate { get; set; }

            public DateTime ExpiryDate { get; set; }

            public DateTime CreatedDate { get; set; }

        }
    }
}




//[HttpPut]
//[Route("EditValidityDate")]
//public bool EditGroupName(int GroupId, string ValidityDate)
//{

//    using (var context = new AuthContext())
//    {
//        //var Groupnames =context.SalesGroupDb.Where(x => (x.GroupName == GroupName)&& x.Id!= GroupId).FirstOrDefault();
//        //if (Groupnames!=null)
//        //{
//        //    return false;
//        //}
//        //else
//        //{
//        var Group = context.SalesGroupDb.Where(x => x.Id == GroupId).FirstOrDefault();
//        // var GroupIds = await context.SalesGroupDb.FirstOrDefaultAsync(x => x.Id == GroupId);
//        Group.ValidityDate = DateTime.Parse(ValidityDate);
//        Group.ModifiedDate = DateTime.Now;

//        context.Commit();
//        return true;
//    }

//}
