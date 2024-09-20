using AngularJSAuthentication.API.Helpers;
using LinqKit;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/HisabKitab")]
    public class HisabKitabController : ApiController
    {
        [Route("PostCustomr")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<HisabKitabCustomer> PostCustomr(HisabKitabCustomer _hisabKitabCustomer)
        {
            MongoDbHelper<HisabKitabCustomer> mongoDbHelper = new MongoDbHelper<HisabKitabCustomer>();
            _hisabKitabCustomer.CreatedDate = DateTime.Now;
            _hisabKitabCustomer.ModifiedDate = DateTime.Now;
            await mongoDbHelper.InsertAsync(_hisabKitabCustomer);
            return _hisabKitabCustomer;
        }


        [Route("GetCustomer")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<HisabKitabCustomer>> GetCustomer(int _retailerId)
        {
            MongoDbHelper<HisabKitabCustomer> mongoDbHelper = new MongoDbHelper<HisabKitabCustomer>();

            var predicate = PredicateBuilder.New<HisabKitabCustomer>(x => x.RetailerId == _retailerId);

            var data = mongoDbHelper.Select(predicate).ToList();

            return data;
        }
    }

    public class HisabKitabCustomer
    {
        public ObjectId Id { get; set; }
        public int RetailerId { get; set; }
        [StringLength(200)]
        public string CustomerName { get; set; }
        [StringLength(20)]
        public string CustomerMobileNo { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }

    }
}
