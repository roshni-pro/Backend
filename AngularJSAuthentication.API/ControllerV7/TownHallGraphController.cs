using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.DataContracts.Transaction.Reports;
using LinqKit;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/TownHallGraph")]
    public class TownHallGraphController : ApiController
    {


        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("GetTownHallData")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<TownhallData> GetTownHallData()
        {
            TownHallManager manager = new TownHallManager();
            MongoDbHelper<TownhallData> mongoDbHelper = new MongoDbHelper<TownhallData>();

            //var month = DateTime.Now.Date.Month;
            //var year = DateTime.Now.Year;
            var month = DateTime.Now.AddMonths(-1).Month;
            var year = DateTime.Now.AddMonths(-1).Year;
            var predicate = PredicateBuilder.New<TownhallData>(x => x.Month == month && x.Year == year);
            var data = mongoDbHelper.Select(predicate).FirstOrDefault();

            if (data == null)
            {
                data = await manager.DownloadData();
                if (data != null)
                {
                    mongoDbHelper.Insert(data);
                }
            }
            return data;
        }

        [Route("SaveTownHallCommentsSection")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<TownHallCommentsSection> SaveKPPData(TownHallCommentsSection data)
        {
            TownHallManager manager = new TownHallManager();
            MongoDbHelper<TownHallCommentsSection> mongoDbHelper = new MongoDbHelper<TownHallCommentsSection>();

            var month = DateTime.Now.Date.Month - 1;
            var year = DateTime.Now.Year;
            var predicate = PredicateBuilder.New<TownHallCommentsSection>(x => x.Month == month && x.Year == year);
            TownHallCommentsSection townHallCommentsSection = mongoDbHelper.Select(predicate).FirstOrDefault();

            if (townHallCommentsSection != null)
            {
                data.Id = townHallCommentsSection.Id;
                await mongoDbHelper.ReplaceAsync(townHallCommentsSection.Id, data, "TownHallCommentsSection");
            }
            else
            {
                await mongoDbHelper.InsertAsync(data);
            }

            return data;
        }

        [Route("GetTownHallCommentsSection")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<TownHallCommentsSection> GetTownHallCommentsSection()
        {
            MongoDbHelper<TownHallCommentsSection> mongoDbHelper = new MongoDbHelper<TownHallCommentsSection>();
            var month = DateTime.Now.Date.Month - 1;
            var year = DateTime.Now.Year;
            var predicate = PredicateBuilder.New<TownHallCommentsSection>(x => x.Month == month && x.Year == year);
            TownHallCommentsSection townHallCommentsSection = mongoDbHelper.Select(predicate).FirstOrDefault();
            return townHallCommentsSection;
        }


    }

}